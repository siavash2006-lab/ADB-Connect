using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ADB_Connect
{
    public static class AdbRunner
    {
        // همان امضای قدیمی تو
        public static (int exitCode, string stdout, string stderr) Run(string arguments, int timeoutMs = 15000)
        {
            var r = RunCaptureAsync(arguments, timeoutMs).GetAwaiter().GetResult();
            return (r.ExitCode, r.StdOut, r.StdErr);
        }

        // نسخه async برای اینکه Task.Run اضافی‌ها را حذف کنی (اختیاری ولی خوب)
        public static async Task<(int exitCode, string stdout, string stderr)> RunAsync(string arguments, int timeoutMs = 15000, CancellationToken ct = default)
        {
            var r = await RunCaptureAsync(arguments, timeoutMs, ct);
            return (r.ExitCode, r.StdOut, r.StdErr);
        }

        /// <summary>
        /// Pair a Wireless Debugging device without exposing the pairing code in
        /// command-line arguments, logs or the process list.
        /// </summary>
        public static async Task<(int exitCode, string stdout, string stderr)> PairAsync(
            string pairingEndpoint,
            string pairingCode,
            int timeoutMs = 30000,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pairingEndpoint))
                throw new ArgumentException("A pairing endpoint is required.", nameof(pairingEndpoint));

            if (string.IsNullOrWhiteSpace(pairingCode))
                throw new ArgumentException("A pairing code is required.", nameof(pairingCode));

            EnsureAdbExists();
            await _adbLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var psi = CreatePsi($"pair {pairingEndpoint}");
                psi.RedirectStandardInput = true;

                using var process = new Process { StartInfo = psi };
                process.Start();

                Task<string> outTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errTask = process.StandardError.ReadToEndAsync();

                // ADB prompts for the code on stdin. Do not include it in Arguments.
                await process.StandardInput.WriteLineAsync(pairingCode).ConfigureAwait(false);
                process.StandardInput.Close();

                using var timeoutCts = timeoutMs > 0 ? new CancellationTokenSource(timeoutMs) : null;
                using var linkedCts = timeoutCts != null
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
                    : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                try
                {
                    await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    try { if (!process.HasExited) process.Kill(true); } catch { }
                    string timedOutOutput = await SafeAwait(outTask).ConfigureAwait(false);
                    string timedOutError = await SafeAwait(errTask).ConfigureAwait(false);
                    bool timedOut = timeoutCts?.IsCancellationRequested == true &&
                                    !cancellationToken.IsCancellationRequested;

                    return (-1, timedOutOutput,
                        timedOut ? "Timeout: adb pairing did not finish." : timedOutError);
                }

                string stdout = await outTask.ConfigureAwait(false);
                string stderr = await errTask.ConfigureAwait(false);
                return (process.ExitCode, stdout, stderr);
            }
            finally
            {
                _adbLock.Release();
            }
        }

        /// <summary>
        /// Captures the selected Android display as a PNG without converting the
        /// binary stdout stream to text. The destination is replaced only after a
        /// complete, valid PNG has been received.
        /// </summary>
        public static async Task<(bool success, string error)> CaptureScreenshotAsync(
            string serial,
            string outputPath,
            int timeoutMs = 20000,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serial))
                throw new ArgumentException("A device serial is required.", nameof(serial));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("An output path is required.", nameof(outputPath));

            string fullOutputPath = Path.GetFullPath(outputPath);
            string? directory = Path.GetDirectoryName(fullOutputPath);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return (false, "The selected destination folder does not exist.");

            string temporaryPath = Path.Combine(
                directory,
                $".{Path.GetFileName(fullOutputPath)}.{Guid.NewGuid():N}.tmp");

            EnsureAdbExists();
            await _adbLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = AdbPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardErrorEncoding = Encoding.UTF8
                };
                psi.ArgumentList.Add("-s");
                psi.ArgumentList.Add(serial);
                psi.ArgumentList.Add("exec-out");
                psi.ArgumentList.Add("screencap");
                psi.ArgumentList.Add("-p");

                using var process = new Process { StartInfo = psi };
                process.Start();

                Task<string> errorTask = process.StandardError.ReadToEndAsync();
                using var timeoutCts = timeoutMs > 0 ? new CancellationTokenSource(timeoutMs) : null;
                using var linkedCts = timeoutCts != null
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
                    : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                try
                {
                    await using (var output = new FileStream(
                        temporaryPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        useAsync: true))
                    {
                        Task copyTask = process.StandardOutput.BaseStream.CopyToAsync(output, linkedCts.Token);
                        await Task.WhenAll(
                            process.WaitForExitAsync(linkedCts.Token),
                            copyTask).ConfigureAwait(false);
                        await output.FlushAsync(linkedCts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    try
                    {
                        if (!process.HasExited)
                            process.Kill(entireProcessTree: true);

                        await process.WaitForExitAsync().ConfigureAwait(false);
                    }
                    catch { }

                    await SafeAwait(errorTask).ConfigureAwait(false);

                    bool timedOut = timeoutCts?.IsCancellationRequested == true &&
                                    !cancellationToken.IsCancellationRequested;
                    return (false, timedOut
                        ? "Timeout: the screenshot was not received in time."
                        : "Screenshot capture was canceled.");
                }

                string stderr = await errorTask.ConfigureAwait(false);
                if (process.ExitCode != 0)
                {
                    return (false, string.IsNullOrWhiteSpace(stderr)
                        ? $"adb screencap failed with exit code {process.ExitCode}."
                        : stderr.Trim());
                }

                if (!await IsValidPngAsync(temporaryPath, cancellationToken).ConfigureAwait(false))
                    return (false, "The device did not return a valid PNG screenshot.");

                File.Move(temporaryPath, fullOutputPath, overwrite: true);
                return (true, string.Empty);
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
                catch { }

                _adbLock.Release();
            }
        }

        public static string AdbPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools", "adb.exe");

        // Global lock: prevents running multiple adb processes concurrently from this app
        private static readonly SemaphoreSlim _adbLock = new(1, 1);

        public readonly record struct AdbResult(int ExitCode, string StdOut, string StdErr, bool TimedOut);

        private static void EnsureAdbExists()
        {
            if (!File.Exists(AdbPath))
                throw new FileNotFoundException("adb.exe not found. Check if platform-tools is next to the program.", AdbPath);
        }

        private static ProcessStartInfo CreatePsi(string arguments)
        {
            return new ProcessStartInfo
            {
                FileName = AdbPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
        }

        /// <summary>
        /// Run adb and capture full stdout/stderr (best for short commands like getprop/pm/devices).
        /// </summary>
        public static async Task<AdbResult> RunCaptureAsync(
            string arguments,
            int timeoutMs = 15000,
            CancellationToken cancellationToken = default)
        {
            EnsureAdbExists();
            await _adbLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                using var p = new Process { StartInfo = CreatePsi(arguments) };
                p.Start();

                // Read concurrently to avoid deadlocks on big outputs
                Task<string> outTask = p.StandardOutput.ReadToEndAsync();
                Task<string> errTask = p.StandardError.ReadToEndAsync();

                using var timeoutCts = timeoutMs > 0 ? new CancellationTokenSource(timeoutMs) : null;
                using var linkedCts = timeoutCts != null
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
                    : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                try
                {
                    await p.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    try { if (!p.HasExited) p.Kill(true); } catch { }
                    var so = await SafeAwait(outTask).ConfigureAwait(false);
                    var se = await SafeAwait(errTask).ConfigureAwait(false);
                    bool timedOut = timeoutCts?.IsCancellationRequested == true && !cancellationToken.IsCancellationRequested;
                    return new AdbResult(-1, so, timedOut ? "Timeout: adb did not respond." : se, timedOut);
                }

                string stdout = await outTask.ConfigureAwait(false);
                string stderr = await errTask.ConfigureAwait(false);
                return new AdbResult(p.ExitCode, stdout, stderr, false);
            }
            finally
            {
                _adbLock.Release();
            }
        }

        /// <summary>
        /// Run adb and stream output line-by-line (best for bugreport/logcat/dumpsys).
        /// onLine is called for both stdout and stderr lines.
        /// </summary>
        public static async Task<int> RunStreamingAsync(
            string arguments,
            Action<string> onLine,
            int timeoutMs = 10 * 60 * 1000,
            CancellationToken cancellationToken = default)
        {
            if (onLine == null) throw new ArgumentNullException(nameof(onLine));

            EnsureAdbExists();
            await _adbLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                using var p = new Process
                {
                    StartInfo = CreatePsi(arguments),
                    EnableRaisingEvents = true
                };

                p.OutputDataReceived += (_, e) => { if (e.Data != null) onLine(e.Data); };
                p.ErrorDataReceived += (_, e) => { if (e.Data != null) onLine(e.Data); };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                using var timeoutCts = timeoutMs > 0 ? new CancellationTokenSource(timeoutMs) : null;
                using var linkedCts = timeoutCts != null
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
                    : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                try
                {
                    await p.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    bool timedOut = timeoutCts?.IsCancellationRequested == true && !cancellationToken.IsCancellationRequested;
                    try { if (!p.HasExited) p.Kill(true); } catch { }

                    onLine(timedOut
                        ? "Timeout: adb did not finish in time."
                        : "Canceled: operation was canceled by user/app.");

                    return -1;
                }

                return p.ExitCode;
            }
            finally
            {
                _adbLock.Release();
            }
        }

        private static async Task<string> SafeAwait(Task<string> t)
        {
            try { return await t.ConfigureAwait(false); }
            catch { return string.Empty; }
        }

        private static async Task<bool> IsValidPngAsync(
            string path,
            CancellationToken cancellationToken)
        {
            byte[] expectedSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            byte[] actualSignature = new byte[expectedSignature.Length];

            try
            {
                await using var input = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    4096,
                    useAsync: true);

                int totalRead = 0;
                while (totalRead < actualSignature.Length)
                {
                    int read = await input.ReadAsync(
                        actualSignature.AsMemory(totalRead),
                        cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                        return false;

                    totalRead += read;
                }

                return actualSignature.SequenceEqual(expectedSignature);
            }
            catch
            {
                return false;
            }
        }
    }
}

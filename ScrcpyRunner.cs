using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ADB_Connect
{
    /// <summary>
    /// Starts and supervises the bundled scrcpy process.
    /// scrcpy renders in its own native window; ADB Connect only manages its lifecycle.
    /// </summary>
    public sealed partial class ScrcpyRunner : IDisposable
    {
        private enum RuntimeKind
        {
            Primary,
            Compatibility
        }

        private sealed record LaunchOptions(
            string Serial,
            int? MaxSize,
            bool EnableAudio,
            (int Width, int Height)? ExpectedDisplaySize,
            bool EnableAutomaticCompatibility);

        private readonly object _sync = new();
        private Process? _process;
        private LaunchOptions? _launchOptions;
        private bool _fallbackRequested;
        private bool _switchingToCompatibility;
        private bool _manualStopRequested;
        private bool _disposed;

        public event Action<string>? StatusChanged;
        public event Action<string>? OutputReceived;

        public string ScrcpyPath =>
            Path.Combine(AppContext.BaseDirectory, "scrcpy", "scrcpy.exe");

        public string CompatibilityScrcpyPath =>
            Path.Combine(AppContext.BaseDirectory, "scrcpy", "compat", "scrcpy.exe");

        public bool IsAvailable => File.Exists(ScrcpyPath);
        public bool IsCompatibilityAvailable => File.Exists(CompatibilityScrcpyPath);

        public bool IsRunning
        {
            get
            {
                lock (_sync)
                {
                    return _process is { HasExited: false };
                }
            }
        }

        public void Start(
            string serial,
            int? maxSize,
            bool enableAudio,
            (int Width, int Height)? expectedDisplaySize,
            bool enableAutomaticCompatibility)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (string.IsNullOrWhiteSpace(serial))
                throw new ArgumentException("A device serial is required.", nameof(serial));

            if (!IsAvailable)
            {
                throw new FileNotFoundException(
                    "The primary scrcpy.exe was not found. Copy the complete official Windows release into the scrcpy folder next to ADB Connect.",
                    ScrcpyPath);
            }

            var options = new LaunchOptions(
                serial,
                maxSize,
                enableAudio,
                expectedDisplaySize,
                enableAutomaticCompatibility);

            lock (_sync)
            {
                if (_process is { HasExited: false })
                    throw new InvalidOperationException("scrcpy is already running.");

                _launchOptions = options;
                _fallbackRequested = false;
                _switchingToCompatibility = false;
                _manualStopRequested = false;
                StartProcessLocked(options, RuntimeKind.Primary);
            }

            StatusChanged?.Invoke("Running");
        }

        public async Task<(int Width, int Height)?> GetPrimaryDisplaySizeAsync(
            string serial,
            int timeoutMs = 10000, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (string.IsNullOrWhiteSpace(serial) || !IsAvailable)
                return null;

            lock (_sync)
            {
                if (_process is { HasExited: false })
                    throw new InvalidOperationException("Display size cannot be queried while scrcpy is running.");
            }

            var psi = new ProcessStartInfo
            {
                FileName = ScrcpyPath,
                WorkingDirectory = Path.GetDirectoryName(ScrcpyPath)!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            psi.Environment["ADB"] = AdbRunner.AdbPath;
            psi.ArgumentList.Add("--serial");
            psi.ArgumentList.Add(serial);
            psi.ArgumentList.Add("--list-displays");

            using var process = new Process { StartInfo = psi };
            if (!process.Start())
                return null;

            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
            try
            {
                await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
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

                try
                {
                    await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
                }
                catch { }

                cancellationToken.ThrowIfCancellationRequested();
                return null;
            }

            string output = $"{await stdoutTask.ConfigureAwait(false)}\n{await stderrTask.ConfigureAwait(false)}";
            Match match = PrimaryDisplaySizeRegex().Match(output);
            if (match.Success &&
                int.TryParse(match.Groups[1].Value, out int width) &&
                int.TryParse(match.Groups[2].Value, out int height) &&
                width > 0 && height > 0)
            {
                return (width, height);
            }

            return null;
        }

        /// <summary>
        /// Waits until the currently running scrcpy process creates its native window.
        /// The returned handle may change when automatic compatibility fallback restarts
        /// scrcpy, so callers should request it again if the old handle is destroyed.
        /// </summary>
        public async Task<IntPtr> WaitForMainWindowHandleAsync(
            int timeoutMs = 10000,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            while (timeoutMs <= 0 || stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Process? process;
                lock (_sync)
                {
                    process = _process;
                }

                if (process is not null)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Refresh();
                            IntPtr handle = process.MainWindowHandle;
                            if (handle != IntPtr.Zero)
                                return handle;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // The process may be replaced during automatic fallback.
                    }
                    catch (InvalidOperationException)
                    {
                        // The process may be replaced during automatic fallback.
                    }
                }

                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }

            return IntPtr.Zero;
        }

        public async Task StopAsync()
        {
            Process? process;
            lock (_sync)
            {
                _manualStopRequested = true;
                process = _process;
            }

            if (process is null)
            {
                StatusChanged?.Invoke("Stopped");
                return;
            }

            await StopProcessAsync(process).ConfigureAwait(false);
            StatusChanged?.Invoke("Stopped");
        }

        private void StartProcessLocked(LaunchOptions options, RuntimeKind runtime)
        {
            string executablePath = runtime == RuntimeKind.Primary
                ? ScrcpyPath
                : CompatibilityScrcpyPath;

            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException(
                    runtime == RuntimeKind.Primary
                        ? "The primary scrcpy runtime is missing."
                        : "The scrcpy compatibility runtime is missing. Copy a complete scrcpy 3.x Windows release into scrcpy\\compat.",
                    executablePath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = Path.GetDirectoryName(executablePath)!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            psi.Environment["ADB"] = AdbRunner.AdbPath;
            psi.ArgumentList.Add("--serial");
            psi.ArgumentList.Add(options.Serial);
            psi.ArgumentList.Add("--window-title");
            psi.ArgumentList.Add($"ADB Connect - {options.Serial}");

            if (options.MaxSize is > 0)
            {
                psi.ArgumentList.Add("--max-size");
                psi.ArgumentList.Add(options.MaxSize.Value.ToString());
            }

            if (!options.EnableAudio)
                psi.ArgumentList.Add("--no-audio");

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, e) => HandleOutput(process, runtime, e.Data);
            process.ErrorDataReceived += (_, e) => HandleOutput(process, runtime, e.Data);
            process.Exited += OnProcessExited;

            _process = process;
            try
            {
                if (!process.Start())
                    throw new InvalidOperationException("scrcpy could not be started.");

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch
            {
                _process = null;
                process.Dispose();
                throw;
            }
        }

        private void HandleOutput(Process process, RuntimeKind runtime, string? line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            OutputReceived?.Invoke(line);

            if (runtime != RuntimeKind.Primary || !TryParseTextureSize(line, out int width, out int height))
                return;

            LaunchOptions? options;
            bool shouldSwitch = false;
            bool compatibilityMissing = false;

            lock (_sync)
            {
                options = _launchOptions;
                if (!ReferenceEquals(_process, process) ||
                    _fallbackRequested ||
                    options is null ||
                    !options.EnableAutomaticCompatibility ||
                    options.ExpectedDisplaySize is null ||
                    !IsUnexpectedlyLow(options.ExpectedDisplaySize.Value, (width, height)))
                {
                    return;
                }

                _fallbackRequested = true;
                compatibilityMissing = !IsCompatibilityAvailable;
                if (!compatibilityMissing)
                {
                    _switchingToCompatibility = true;
                    shouldSwitch = true;
                }
            }

            var expected = options!.ExpectedDisplaySize!.Value;
            OutputReceived?.Invoke(
                $"Automatic resolution check: expected {expected.Width}x{expected.Height}, received {width}x{height}.");

            if (compatibilityMissing)
            {
                OutputReceived?.Invoke(
                    "A severely reduced stream was detected, but scrcpy\\compat\\scrcpy.exe is missing. " +
                    "Install the complete scrcpy 3.x compatibility runtime to enable automatic recovery.");
                StatusChanged?.Invoke("Low resolution - compatibility runtime missing");
                return;
            }

            if (shouldSwitch)
            {
                OutputReceived?.Invoke(
                    "A severely reduced stream was detected. Restarting with the compatibility runtime.");
                _ = Task.Run(() => SwitchToCompatibilityAsync(process));
            }
        }

        private async Task SwitchToCompatibilityAsync(Process primaryProcess)
        {
            StatusChanged?.Invoke("Switching to compatibility...");

            try
            {
                await StopProcessAsync(primaryProcess).ConfigureAwait(false);

                bool started = false;
                lock (_sync)
                {
                    if (!_disposed && !_manualStopRequested && _launchOptions is not null)
                    {
                        StartProcessLocked(_launchOptions, RuntimeKind.Compatibility);
                        started = true;
                    }

                    _switchingToCompatibility = false;
                }

                if (started)
                {
                    OutputReceived?.Invoke("Compatibility scrcpy started automatically.");
                    StatusChanged?.Invoke("Running (Compatibility)");
                }
                else
                {
                    StatusChanged?.Invoke("Stopped");
                }
            }
            catch (Exception ex)
            {
                lock (_sync)
                {
                    _switchingToCompatibility = false;
                }

                OutputReceived?.Invoke($"Could not start compatibility scrcpy: {ex.Message}");
                StatusChanged?.Invoke("Compatibility start failed");
            }
        }

        private static bool TryParseTextureSize(string line, out int width, out int height)
        {
            Match match = TextureSizeRegex().Match(line);
            if (match.Success &&
                int.TryParse(match.Groups[1].Value, out width) &&
                int.TryParse(match.Groups[2].Value, out height))
            {
                return true;
            }

            width = 0;
            height = 0;
            return false;
        }

        private static bool IsUnexpectedlyLow(
            (int Width, int Height) expected,
            (int Width, int Height) actual)
        {
            if (expected.Width <= 0 || expected.Height <= 0 || actual.Width <= 0 || actual.Height <= 0)
                return false;

            long expectedArea = (long)expected.Width * expected.Height;
            long actualArea = (long)actual.Width * actual.Height;

            // A normal encoder alignment adjustment only removes a few pixels. The v4 TV
            // regression produces roughly 4% of the requested area, so only treat a stream
            // below 20% as severe. This avoids binding the rule to any IP, model or resolution.
            return actualArea * 5 < expectedArea;
        }

        private async Task StopProcessAsync(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.CloseMainWindow();

                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    try
                    {
                        await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        if (!process.HasExited)
                            process.Kill(entireProcessTree: true);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // The process exited between the state check and the stop request.
            }
            finally
            {
                ClearProcess(process);
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            if (sender is not Process process)
                return;

            int? exitCode = null;
            try { exitCode = process.ExitCode; } catch { }

            bool wasCurrent = ClearProcess(process);
            bool suppressStatus;
            lock (_sync)
            {
                suppressStatus = _switchingToCompatibility;
            }

            if (wasCurrent && !suppressStatus)
            {
                StatusChanged?.Invoke(exitCode is null
                    ? "Stopped"
                    : $"Stopped (Exit code: {exitCode})");
            }
        }

        private bool ClearProcess(Process process)
        {
            bool shouldDispose = false;
            lock (_sync)
            {
                if (ReferenceEquals(_process, process))
                {
                    _process = null;
                    shouldDispose = true;
                }
            }

            if (!shouldDispose)
                return false;

            process.Exited -= OnProcessExited;
            process.Dispose();
            return true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Process? process;
            lock (_sync)
            {
                _manualStopRequested = true;
                process = _process;
                _process = null;
            }

            if (process is not null)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                catch { }
                finally
                {
                    process.Dispose();
                }
            }
        }

        [GeneratedRegex(@"Texture:\s*(\d+)x(\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex TextureSizeRegex();

        [GeneratedRegex(@"--display-id=0\s+\((\d+)x(\d+)\)", RegexOptions.IgnoreCase)]
        private static partial Regex PrimaryDisplaySizeRegex();
    }
}

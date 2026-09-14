using System.Diagnostics;
using System.Text;

namespace ADB_Connect;

internal static class ProcessExecutor
{
    // All callers retain ownership until both redirected streams have drained.
    public static async Task<AdbRunner.AdbResult> CaptureAsync(
        ProcessStartInfo startInfo, int timeoutMs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource();
        if (timeoutMs > 0) timeout.CancelAfter(timeoutMs);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
        try
        {
            await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
            return new(process.ExitCode, await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false), false);
        }
        catch (OperationCanceledException)
        {
            await KillAsync(process).ConfigureAwait(false);
            string output = await stdout.ConfigureAwait(false);
            string error = await stderr.ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return new(-1, output, "Timeout: adb did not finish.\n" + error, true);
        }
    }

    public static async Task KillAsync(Process process)
    {
        if (!process.HasExited) process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync().ConfigureAwait(false);
    }

    public static ProcessStartInfo Create(string executable, IEnumerable<string> arguments)
    {
        var info = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        foreach (string argument in arguments) info.ArgumentList.Add(argument);
        return info;
    }
}

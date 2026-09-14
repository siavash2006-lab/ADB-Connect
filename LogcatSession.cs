using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace ADB_Connect;

internal sealed class LogcatSession
{
    private readonly Process _process;
    private readonly ConcurrentQueue<string> _display = new();
    private int _queued;
    private long _dropped;
    private readonly Task _completion;
    private volatile bool _stopRequested;
    public string FilePath { get; }
    public string? Error { get; private set; }
    public bool IsRunning => !_completion.IsCompleted;
    public Task Completion => _completion;
    public long DroppedDisplayLines => Interlocked.Read(ref _dropped);

    public LogcatSession(ProcessStartInfo info, string filePath)
    {
        FilePath = filePath;
        // Open the archive before starting adb, so no unrecorded session can start.
        var file = new StreamWriter(new FileStream(filePath, FileMode.CreateNew,
            FileAccess.Write, FileShare.Read, 65536, FileOptions.Asynchronous), new UTF8Encoding(false));
        _process = new Process { StartInfo = info };
        try { _process.Start(); }
        catch { file.Dispose(); _process.Dispose(); throw; }
        _completion = RunAsync(file);
    }

    private async Task RunAsync(StreamWriter file)
    {
        // A single writer serializes stdout and stderr and flushes every line.
        using var writeLock = new SemaphoreSlim(1);
        async Task PumpAsync(StreamReader reader, bool isError)
        {
            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                await writeLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    await file.WriteLineAsync(isError ? "[adb stderr] " + line : line).ConfigureAwait(false);
                    await file.FlushAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Stop immediately on archive failure rather than silently losing evidence.
                    if (!_process.HasExited) _process.Kill(true);
                    throw;
                }
                finally { writeLock.Release(); }
                if (Interlocked.Increment(ref _queued) <= 4000)
                    _display.Enqueue(isError ? "[adb stderr] " + line : line);
                else
                {
                    Interlocked.Decrement(ref _queued);
                    Interlocked.Increment(ref _dropped);
                }
            }
        }
        try
        {
            await Task.WhenAll(PumpAsync(_process.StandardOutput, false),
                PumpAsync(_process.StandardError, true), _process.WaitForExitAsync()).ConfigureAwait(false);
            if (_process.ExitCode != 0 && !_stopRequested) Error = $"Logcat exited with code {_process.ExitCode}. See the raw log.";
        }
        catch (Exception ex) { Error = "Logcat recording failed: " + ex.Message; }
        finally
        {
            try { await file.DisposeAsync().ConfigureAwait(false); }
            catch (Exception ex) { Error = "Log file could not be finalized: " + ex.Message; }
            _process.Dispose();
        }
    }

    public bool TryRead(out string? line)
    {
        if (!_display.TryDequeue(out line)) return false;
        Interlocked.Decrement(ref _queued);
        return true;
    }

    public void ClearDisplay() { while (TryRead(out _)) { } }

    public async Task StopAsync()
    {
        _stopRequested = true;
        try { if (!_completion.IsCompleted && !_process.HasExited) _process.Kill(true); }
        catch (InvalidOperationException) { } // Natural exit raced with Stop.
        await _completion.ConfigureAwait(false);
    }
}

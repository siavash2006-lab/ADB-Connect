using ADB_Connect;
using System.Diagnostics;
using System.Reflection;

Console.OutputEncoding = new System.Text.UTF8Encoding(false);
Console.InputEncoding = new System.Text.UTF8Encoding(false);

if (args.Length > 0 && args[0] == "child")
{
    if (args[1] == "sleep")
    {
        Console.WriteLine(Environment.ProcessId);
        await Task.Delay(30000);
    }
    else if (args[1] == "echo") Console.WriteLine(args[2]);
    else
    {
        for (int i = 0; i < 15000; i++)
        {
            Console.WriteLine($"stdout-{i}");
            Console.Error.WriteLine($"stderr-{i}");
        }
        if (args[1] == "fail") Environment.ExitCode = 7;
    }
    return;
}

int checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAILED: " + name);
    checks++;
    Console.WriteLine("PASS: " + name);
}
ProcessStartInfo Child(string mode, params string[] extra) => ProcessExecutor.Create(
    Environment.ProcessPath!, new[] { "child", mode }.Concat(extra));

Check(!CommandRules.Succeeded("uninstall", 0, "Failure [DELETE_FAILED_INTERNAL_ERROR]", ""), "uninstall failure with exit zero");
Check(!CommandRules.Succeeded("install", 1, "Success", ""), "nonzero exit overrides Success");
Check(CommandRules.Succeeded("uninstall", 0, "Success\r\n", ""), "valid uninstall");
Check(CommandRules.Succeeded("stop", 0, "", ""), "silent force-stop success");
Check(!CommandRules.Succeeded("stop", 0, "", "java.lang.SecurityException"), "force-stop security failure");
Check(CommandRules.Succeeded("start", 0, "Events injected: 1", ""), "monkey launch success");
Check(!CommandRules.Succeeded("start", 0, "No activities found to run, monkey aborted.", ""), "missing launch activity");
Check(!CommandRules.IsPackageName("com.test; reboot") && CommandRules.IsPackageName("com.test_app"), "package shell metacharacters rejected");
Check(CommandRules.OnlineSerials("List of devices attached\nA\tdevice model:x\nB\toffline\nC\tunauthorized\nD\tdevicefoo\n").SequenceEqual(new[] { "A" }), "exact device-state parser");

var flood = await ProcessExecutor.CaptureAsync(Child("fail"), 15000, default);
Check(flood.ExitCode == 7 && flood.StdOut.Contains("stdout-14999") && flood.StdErr.Contains("stderr-14999"), "both large streams drained with original exit code");
var echo = await ProcessExecutor.CaptureAsync(Child("echo", "فایل با فاصله & literal"), 5000, default);
Check(echo.StdOut.Trim() == "فایل با فاصله & literal", "ArgumentList preserves unicode and spaces");
var timeout = await ProcessExecutor.CaptureAsync(Child("sleep"), 800, default);
Check(timeout.TimedOut && timeout.ExitCode == -1, "timeout classified");
bool dead;
try { using var p = Process.GetProcessById(int.Parse(timeout.StdOut.Trim())); dead = p.HasExited; }
catch (ArgumentException) { dead = true; }
Check(dead, "timed-out child terminated");
using (var cancellation = new CancellationTokenSource(800))
{
    bool canceled = false;
    try { await ProcessExecutor.CaptureAsync(Child("sleep"), 20000, cancellation.Token); }
    catch (OperationCanceledException) { canceled = true; }
    Check(canceled, "user cancellation distinct from timeout");
}
string path = Path.Combine(Path.GetTempPath(), $"adb-connect-test-{Guid.NewGuid():N}.txt");
try
{
    var log = new LogcatSession(Child("flood"), path);
    await log.Completion.WaitAsync(TimeSpan.FromSeconds(30));
    var lines = await File.ReadAllLinesAsync(path);
    Check(log.Error == null && lines.Length == 30000 && lines.Contains("stdout-14999") && lines.Contains("[adb stderr] stderr-14999"), "raw log survives display overflow with stderr");
    int displayed = 0; while (log.TryRead(out _)) displayed++;
    Check(displayed <= 4000 && log.DroppedDisplayLines == 26000, "display memory bounded independently of archive");
    await log.StopAsync();
    Check(!log.IsRunning, "stop safe after natural completion");
}
finally { File.Delete(path); }
string stopPath = Path.Combine(Path.GetTempPath(), $"adb-connect-stop-{Guid.NewGuid():N}.txt");
try
{
    var log = new LogcatSession(Child("sleep"), stopPath);
    await Task.Delay(300);
    await log.StopAsync();
    Check(log.Error == null && !log.IsRunning, "intentional Logcat stop finalizes without false error");
}
finally { File.Delete(stopPath); }
Console.WriteLine($"All {checks} regression checks passed. No Android devices were used.");

using System.Net;

namespace ADB_Connect;

public partial class Form1
{
    private CancellationTokenSource? _operationCts;
    private string? _operationSerial;
    private Task? _activeOperation;
    private bool _rebootRequested;
    private bool _closing;
    private bool _allowClose;
    private bool _canCancelOperation;
    private readonly Button btnCancelOperation = new()
    {
        Text = "Cancel operation", Size = new Size(120, 23),
        Location = new Point(140, 435), Enabled = false, Visible = false,
        UseVisualStyleBackColor = true
    };
    private CancellationToken OperationToken => _operationCts?.Token ?? CancellationToken.None;

    private void InitializeOperationControls()
    {
        panel4.Controls.Add(btnCancelOperation);
        btnCancelOperation.Click += (_, _) => _operationCts?.Cancel();
        var clearDevice = new Button
        {
            Text = "Clear Device...", Size = new Size(117, 23),
            Location = new Point(170, 221), UseVisualStyleBackColor = true,
            AccessibleDescription = "Clear the selected Android device's Logcat buffer after confirmation. Saved files are kept."
        };
        clearDevice.Click += async (_, _) => await RunUiOperationAsync(async () =>
        {
            if (!EnsureConnected()) return;
            if (MessageBox.Show(this, $"Clear the Android Logcat buffer on {_operationSerial}?\nSaved log files will be kept.",
                "Clear device buffer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var r = await RunDeviceAsync(new[] { "logcat", "-c" });
            if (r.exitCode != 0) throw new InvalidOperationException(r.stderr);
            AppendLog("Device log buffer cleared.", Color.Orange);
        });
        tabPage3.Controls.Add(clearDevice);
        btnClearLog.Text = "Clear View";
        Shown += (_, _) =>
        {
            if (!File.Exists(AdbRunner.AdbPath)) AppendLog("Missing platform-tools/adb.exe. Restore the bundled platform-tools folder.", Color.Red);
            if (!_scrcpyRunner.IsAvailable) AppendLog("TV Control unavailable: scrcpy runtime is missing.", Color.OrangeRed);
        };
    }

    private static IEnumerable<Control> AllControls(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in AllControls(child)) yield return descendant;
        }
    }

    private Task RunUiOperationAsync(Func<Task> operation)
    {
        if (_operationCts != null || _closing) return Task.CompletedTask;
        _activeOperation = ExecuteOperationAsync(operation);
        return _activeOperation;
    }

    private async Task ExecuteOperationAsync(Func<Task> operation)
    {
        using var cts = new CancellationTokenSource();
        _operationCts = cts;
        _operationSerial = _connectedSerial;
        _rebootRequested = false;
        UpdateUiByConnectionState();
        try { await operation(); }
        catch (OperationCanceledException) { AppendLog("Operation canceled. Completed device changes are not undone.", Color.Orange); }
        catch (Exception ex)
        {
            AppendLog(ex.Message, Color.Red);
            if (!_closing) MessageBox.Show(this, ex.Message, "Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Revalidate only the same selected device; never switch to another online device.
            if (!_closing && !_rebootRequested && _connectedSerial is { } serial)
            {
                try
                {
                    var r = await AdbRunner.RunArgumentsAsync(new[] { "-s", serial, "get-state" }, 3000, CancellationToken.None);
                    _isConnected = r.exitCode == 0 && r.stdout.Trim() == "device";
                }
                catch { _isConnected = false; }
            }
            _operationCts = null;
            _operationSerial = null;
            if (!_closing)
            {
                // Restore generic controls disabled by the common gate.
                foreach (var control in AllControls(this)) if (control is Button) control.Enabled = true;
                label11.Text = _isConnected ? $"Active: {_connectedSerial}" : $"Offline: {_connectedSerial ?? "no device"}";
                UpdateUiByConnectionState();
            }
        }
    }

    private async Task<(int exitCode, string stdout, string stderr)> RunDeviceAsync(IEnumerable<string> args, int timeoutMs = 20000)
    {
        string serial = _operationSerial ?? _connectedSerial ?? throw new InvalidOperationException("Select a device first.");
        var result = await AdbRunner.RunArgumentsAsync(new[] { "-s", serial }.Concat(args), timeoutMs, OperationToken);
        OperationToken.ThrowIfCancellationRequested();
        return result;
    }

    private async Task RunPackageOperationAsync(string operation)
    {
        if (!EnsureConnected()) return;
        var packages = checkedListBox1.CheckedItems.Cast<string>().ToArray();
        if (packages.Length == 0) throw new InvalidOperationException("Select at least one package.");
        if (packages.Any(p => !CommandRules.IsPackageName(p))) throw new InvalidOperationException("Invalid package name.");
        var results = new List<string>();
        foreach (string package in packages)
        {
            OperationToken.ThrowIfCancellationRequested();
            string[] args = operation switch
            {
                "uninstall" => new[] { "shell", "pm", "uninstall", "--user", "0", package },
                "start" => new[] { "shell", "monkey", "-p", package, "-c", "android.intent.category.LAUNCHER", "1" },
                "stop" => new[] { "shell", "am", "force-stop", package },
                _ => throw new ArgumentException("Unknown package operation.")
            };
            var r = await RunDeviceAsync(args);
            bool success = CommandRules.Succeeded(operation, r.exitCode, r.stdout, r.stderr);
            string detail = success ? "Succeeded" : $"Failed (exit {r.exitCode}): {(r.stdout + " " + r.stderr).Trim()}";
            results.Add($"{package}: {detail}");
            AppendLog($"{operation} — {package}: {detail}", success ? Color.Green : Color.Red);
        }
        if (operation == "uninstall") await packageListUpdate();
        MessageBox.Show(this, string.Join(Environment.NewLine, results), $"{operation} results — {_operationSerial}");
    }

    internal static string NormalizeEndpoint(string input)
    {
        if (IPAddress.TryParse(input, out var bare)) return bare.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? $"[{bare}]:5555" : $"{bare}:5555";
        if (!Uri.TryCreate("tcp://" + input, UriKind.Absolute, out var uri) || uri.Port < 1 || uri.Port > 65535 ||
            uri.AbsolutePath != "/" || uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0 ||
            !IPAddress.TryParse(uri.Host.Trim('[', ']'), out var address))
            throw new ArgumentException("Enter a valid IP address, optionally with a port (1–65535).");
        return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? $"[{address}]:{uri.Port}" : $"{address}:{uri.Port}";
    }

    private void WriteDisplayedLog(string text, Color color)
    {
        int limit = int.TryParse(txtLineLimitation.Text, out int value) ? Math.Clamp(value, 100, 10000) : 2000;
        LogViewWriter.Append(rtbLog, text, color, limit, MaxLogChars);
    }

    private async Task ObserveLogAsync(LogcatSession session)
    {
        await session.Completion;
        if (_closing || IsDisposed || _logSession != session) return;
        FlushLogToRtb();
        AppendLog(session.Error ?? "Logcat completed.", session.Error is null ? Color.Orange : Color.Red);
        if (session.DroppedDisplayLines > 0) AppendLog($"Display skipped {session.DroppedDisplayLines} lines. The raw file is independent of the display limit.", Color.Orange);
        UpdateUiByConnectionState();
    }

    private async Task StopLogAsync()
    {
        if (_logSession is { } session) await session.StopAsync();
        if (!_closing) { FlushLogToRtb(); UpdateUiByConnectionState(); }
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        if (_allowClose) { base.OnFormClosing(e); return; }
        e.Cancel = true;
        base.OnFormClosing(e);
        if (_closing) return;
        _closing = true;
        _operationCts?.Cancel();
        UpdateUiByConnectionState();
        try
        {
            if (_activeOperation != null) await _activeOperation;
            await StopLogAsync();
            await _scrcpyRunner.StopAsync();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        finally { _allowClose = true; Close(); }
    }
}

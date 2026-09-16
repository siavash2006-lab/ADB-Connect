using System.Text;

namespace ADB_Connect;

public partial class Form1
{
    private readonly Label _propertyHeading = new() { AutoEllipsis = true };
    private readonly TextBox _propertyValue = new()
    {
        Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
        AccessibleName = "Property result", WordWrap = true
    };
    private readonly Button _copyProperty = new() { Text = "Copy", Enabled = false };
    private readonly Button _saveProperty = new() { Text = "Save…", Enabled = false };
    private bool _hasPropertyResult;

    private void InitializePropertyResults()
    {
        _propertyHeading.SetBounds(8, 247, 570, 18);
        _propertyHeading.Text = "Select a property to view its value here.";
        _propertyValue.SetBounds(8, 269, 570, 76);
        _copyProperty.SetBounds(390, 210, 90, 28);
        _saveProperty.SetBounds(488, 210, 90, 28);
        tabPage1.Controls.AddRange(new Control[] { _propertyHeading, _propertyValue, _copyProperty, _saveProperty });
        _copyProperty.Click += (_, _) =>
        {
            try { if (_hasPropertyResult) Clipboard.SetText(_propertyValue.Text); }
            catch (System.Runtime.InteropServices.ExternalException) { _propertyHeading.Text = "Clipboard is busy. Try Copy again."; }
        };
        _saveProperty.Click += (_, _) =>
        {
            if (!_hasPropertyResult) return;
            using var dialog = new SaveFileDialog { Title = "Save property result", Filter = "Text file (*.txt)|*.txt", FileName = "android_properties.txt" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try { File.WriteAllText(dialog.FileName, _propertyValue.Text, Encoding.UTF8); _propertyHeading.Text = "Saved: " + dialog.FileName; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _propertyHeading.Text = "Save failed: " + ex.Message; }
        };
    }

    private async Task ReadPropertyAsync(Control? source, string command)
    {
        await RunUiOperationAsync(async () =>
        {
            string title = source?.Text ?? "Properties";
            string device = _operationSerial ?? _connectedSerial ?? "No device";
            _hasPropertyResult = false;
            _propertyValue.Clear();
            _propertyHeading.Text = $"Reading {title} — {device}…";
            UpdatePropertyActions(true);
            try
            {
                if (!_isConnected) { _propertyHeading.Text = "Connect a device before reading properties."; return; }
                var result = await RunAdbAsync(command, 30000);
                ShowPropertyResult(title, device, result.exitCode == 0 ? result.stdout : result.stderr + "\n" + result.stdout, result.exitCode != 0);
            }
            catch (OperationCanceledException) { _propertyHeading.Text = "Property read cancelled."; }
            catch (Exception ex) { ShowPropertyResult(title, device, ex.Message, true); }
        });
    }

    private void ShowPropertyResult(string title, string device, string value, bool failed)
    {
        _propertyHeading.Text = $"{title} — {device}" + (failed ? " • Failed" : "");
        _propertyValue.Text = string.IsNullOrWhiteSpace(value) ? (failed ? "The command failed without an error message." : "No value reported by the device.") : value.TrimEnd();
        _propertyValue.Select(0, 0);
        _hasPropertyResult = !failed && !string.IsNullOrWhiteSpace(value);
        UpdatePropertyActions(_operationCts != null);
    }

    private void UpdatePropertyActions(bool busy)
    {
        _copyProperty.Enabled = _saveProperty.Enabled = _hasPropertyResult && !busy;
    }
}

using System.Net;

namespace ADB_Connect;

public sealed class PairDeviceForm : Form
{
    private readonly TextBox _txtIp = new();
    private readonly TextBox _txtConnectPort = new() { MaxLength = 5 };
    private readonly TextBox _txtPairPort = new() { MaxLength = 5 };
    private readonly TextBox _txtPairCode = new() { MaxLength = 6, UseSystemPasswordChar = true };
    private readonly Panel _connection = new();
    private readonly Panel _pairing = new();
    private readonly Label _step = new();
    private readonly Label _hint = new();
    private readonly Label _error = new() { AutoEllipsis = true };
    private readonly Button _next = new() { Text = "Next", Tag = "primary" };
    private readonly Button _back = new() { Text = "Back" };
    private bool _secondStep;

    public string DeviceIp => _txtIp.Text.Trim();
    public int ConnectionPort => int.TryParse(_txtConnectPort.Text, out int port) ? port : 0;
    public int PairingPort => int.TryParse(_txtPairPort.Text, out int port) ? port : 0;
    public string PairingCode => _txtPairCode.Text.Trim();
    public string ConnectionEndpoint => Endpoint(ConnectionPort);
    public string PairingEndpoint => Endpoint(PairingPort);
    private string Endpoint(int port) => DeviceIp.Contains(':') ? $"[{DeviceIp}]:{port}" : $"{DeviceIp}:{port}";

    public PairDeviceForm(string? defaultIp = null)
    {
        Text = "Pair Device";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(430, 268);
        _step.SetBounds(16, 14, 398, 24);
        _step.Font = new Font(Font, FontStyle.Bold);
        _hint.SetBounds(16, 44, 398, 38);
        _connection.SetBounds(16, 88, 398, 78);
        _pairing.Bounds = _connection.Bounds;
        AddField(_connection, "IP address", _txtIp, 0);
        AddField(_connection, "Connection port", _txtConnectPort, 38);
        AddField(_pairing, "Pairing port", _txtPairPort, 0);
        AddField(_pairing, "Pairing code", _txtPairCode, 38);
        _txtIp.Text = defaultIp ?? "";
        foreach (var field in new[] { _txtConnectPort, _txtPairPort, _txtPairCode })
            field.KeyPress += (_, e) => e.Handled = !char.IsControl(e.KeyChar) && (e.KeyChar < '0' || e.KeyChar > '9');
        _error.SetBounds(16, 172, 398, 36);
        _back.SetBounds(16, 222, 90, 30);
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(210, 222, 90, 30) };
        _next.SetBounds(308, 222, 106, 30);
        _back.Click += (_, _) => ShowStep(false);
        _next.Click += (_, _) => Advance();
        Controls.AddRange(new Control[] { _step, _hint, _connection, _pairing, _error, _back, cancel, _next });
        AcceptButton = _next; CancelButton = cancel;
        AppTheme.Attach(this, () => _error.ForeColor = AppTheme.Dark ? Color.Salmon : Color.Firebrick);
        ShowStep(false);
    }

    private static void AddField(Panel panel, string name, TextBox input, int y)
    {
        panel.Controls.Add(new Label { Text = name, Bounds = new Rectangle(0, y + 4, 130, 24) });
        input.SetBounds(140, y, 258, 25);
        input.AccessibleName = name;
        panel.Controls.Add(input);
    }

    private void ShowStep(bool second)
    {
        _secondStep = second;
        _connection.Visible = !second; _pairing.Visible = second; _back.Visible = second;
        _step.Text = second ? "2 / 2   Pairing details" : "1 / 2   Device connection";
        _hint.Text = second ? $"Now open “Pair device with pairing code” on {DeviceIp} and enter its port and code."
            : "Enter the IP and connection port on the main Wireless Debugging screen.";
        _next.Text = second ? "Pair && Connect" : "Next";
        _error.Text = "";
        (second ? _txtPairPort : _txtIp).Focus();
    }

    private void Advance()
    {
        if (!IPAddress.TryParse(DeviceIp, out _)) { ShowStep(false); Fail("Enter a valid IP address.", _txtIp); return; }
        if (ConnectionPort is < 1 or > 65535) { ShowStep(false); Fail("Connection port must be between 1 and 65535.", _txtConnectPort); return; }
        if (!_secondStep) { ShowStep(true); return; }
        if (PairingPort is < 1 or > 65535) { Fail("Pairing port must be between 1 and 65535.", _txtPairPort); return; }
        if (PairingCode.Length != 6 || PairingCode.Any(c => c < '0' || c > '9')) { Fail("Enter the six-digit pairing code.", _txtPairCode); return; }
        DialogResult = DialogResult.OK;
        Close();
    }
    private void Fail(string message, Control field) { _error.Text = message; field.Focus(); }
}

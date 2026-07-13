using System.Net;

namespace ADB_Connect
{
    public sealed class PairDeviceForm : Form
    {
        private readonly TextBox _txtIp = new();
        private readonly TextBox _txtPairPort = new();
        private readonly TextBox _txtPairCode = new();
        private readonly TextBox _txtConnectPort = new();
        private readonly Button _btnPairAndConnect = new();
        private readonly Button _btnCancel = new();

        public string DeviceIp => _txtIp.Text.Trim();
        public int PairingPort => ParsePort(_txtPairPort.Text);
        public string PairingCode => _txtPairCode.Text.Trim();
        public int ConnectionPort => ParsePort(_txtConnectPort.Text);

        public string PairingEndpoint => BuildEndpoint(DeviceIp, PairingPort);
        public string ConnectionEndpoint => BuildEndpoint(DeviceIp, ConnectionPort);

        public PairDeviceForm(string? defaultIp = null)
        {
            Text = "Pair Wireless ADB Device";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(410, 295);

            var info = new Label
            {
                Location = new Point(15, 14),
                Size = new Size(380, 45),
                Text = "Enter the values shown by Wireless Debugging. Pairing Port and Connection Port are separate and may be different."
            };

            AddLabel("IP Address:", 70);
            _txtIp.Location = new Point(150, 67);
            _txtIp.Size = new Size(225, 23);
            _txtIp.Text = defaultIp ?? string.Empty;

            AddLabel("Pairing Port:", 105);
            ConfigureNumericTextBox(_txtPairPort, 102, 5);

            AddLabel("Pairing Code:", 140);
            _txtPairCode.Location = new Point(150, 137);
            _txtPairCode.Size = new Size(225, 23);
            _txtPairCode.MaxLength = 6;
            _txtPairCode.UseSystemPasswordChar = true;
            ConfigureNumericInput(_txtPairCode);

            AddLabel("Connection Port:", 175);
            ConfigureNumericTextBox(_txtConnectPort, 172, 5);

            var portNote = new Label
            {
                Location = new Point(150, 202),
                Size = new Size(225, 32),
                ForeColor = SystemColors.ControlDarkDark,
                Text = "Use the port on the main Wireless Debugging screen, not the pairing dialog."
            };

            _btnPairAndConnect.Location = new Point(150, 249);
            _btnPairAndConnect.Size = new Size(130, 30);
            _btnPairAndConnect.Text = "Pair && Connect";
            _btnPairAndConnect.Click += PairAndConnect_Click;

            _btnCancel.Location = new Point(290, 249);
            _btnCancel.Size = new Size(85, 30);
            _btnCancel.Text = "Cancel";
            _btnCancel.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                info,
                _txtIp,
                _txtPairPort,
                _txtPairCode,
                _txtConnectPort,
                portNote,
                _btnPairAndConnect,
                _btnCancel
            });

            AcceptButton = _btnPairAndConnect;
            CancelButton = _btnCancel;
        }

        private void AddLabel(string text, int y)
        {
            Controls.Add(new Label
            {
                AutoSize = true,
                Location = new Point(15, y),
                Text = text
            });
        }

        private static void ConfigureNumericTextBox(TextBox control, int y, int maxLength)
        {
            control.Location = new Point(150, y);
            control.Size = new Size(225, 23);
            control.MaxLength = maxLength;
            ConfigureNumericInput(control);
        }

        private static void ConfigureNumericInput(TextBox control)
        {
            control.KeyPress += (_, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };

            // KeyPress does not cover pasted text, so sanitize every change as well.
            control.TextChanged += (_, _) =>
            {
                string digitsOnly = new string(control.Text
                    .Where(char.IsDigit)
                    .Take(control.MaxLength)
                    .ToArray());

                if (control.Text == digitsOnly)
                    return;

                int caret = Math.Min(control.SelectionStart, digitsOnly.Length);
                control.Text = digitsOnly;
                control.SelectionStart = caret;
            };
        }

        private void PairAndConnect_Click(object? sender, EventArgs e)
        {
            if (!IPAddress.TryParse(DeviceIp, out _))
            {
                MessageBox.Show("Enter a valid IP address.", "Pair Device");
                _txtIp.Focus();
                return;
            }

            if (!IsValidPort(PairingPort) || !IsValidPort(ConnectionPort))
            {
                MessageBox.Show(
                    "Enter valid ports from 1 to 65535. The Pairing Port and Connection Port may be different.",
                    "Pair Device");
                if (!IsValidPort(PairingPort))
                    _txtPairPort.Focus();
                else
                    _txtConnectPort.Focus();
                return;
            }

            if (PairingCode.Length != 6 || PairingCode.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("Enter the 6-digit pairing code shown on the device.", "Pair Device");
                _txtPairCode.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static int ParsePort(string value)
        {
            return int.TryParse(value, out int port) ? port : 0;
        }

        private static bool IsValidPort(int port) => port is >= 1 and <= 65535;

        private static string BuildEndpoint(string ip, int port)
        {
            return ip.Contains(':')
                ? $"[{ip}]:{port}"
                : $"{ip}:{port}";
        }
    }
}

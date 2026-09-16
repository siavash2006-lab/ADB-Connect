using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ADB_Connect
{
    /// <summary>
    /// Hosts the native scrcpy window inside a WinForms panel and captures only
    /// the visible video area by using the Windows desktop framebuffer.
    /// </summary>
    public sealed class ScrcpyHostForm : Form
    {
        private const int GwlStyle = -16;
        private const long WsChild = 0x40000000L;
        private const long WsVisible = 0x10000000L;
        private const long WsCaption = 0x00C00000L;
        private const long WsThickFrame = 0x00040000L;
        private const long WsMinimizeBox = 0x00020000L;
        private const long WsMaximizeBox = 0x00010000L;
        private const long WsSysMenu = 0x00080000L;
        private const long WsPopup = 0x80000000L;
        private const uint SwpNoZOrder = 0x0004;
        private const uint SwpNoActivate = 0x0010;
        private const uint SwpFrameChanged = 0x0020;

        private readonly ScrcpyRunner _runner;
        private readonly string _serial;
        private readonly int? _maxSize;
        private readonly bool _enableAudio;
        private readonly (int Width, int Height)? _expectedDisplaySize;
        private readonly bool _enableAutomaticCompatibility;

        private readonly ToolStrip _toolbar = new();
        private readonly ToolStripButton _btnScreenshot = new("Screenshot");
        private readonly ToolStripButton _btnStop = new("Stop");
        private readonly ToolStripLabel _statusLabel = new("Starting...");
        private readonly Panel _videoPanel = new();
        private readonly System.Windows.Forms.Timer _windowTimer = new();

        private IntPtr _scrcpyWindow;
        private bool _embedding;
        private bool _stopInProgress;
        private bool _allowClose;

        public ScrcpyHostForm(
            ScrcpyRunner runner,
            string serial,
            int? maxSize,
            bool enableAudio,
            (int Width, int Height)? expectedDisplaySize,
            bool enableAutomaticCompatibility)
        {
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
            _serial = serial ?? throw new ArgumentNullException(nameof(serial));
            _maxSize = maxSize;
            _enableAudio = enableAudio;
            _expectedDisplaySize = expectedDisplaySize;
            _enableAutomaticCompatibility = enableAutomaticCompatibility;

            Text = $"ADB Connect - TV Control - {serial}";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(680, 440);
            ClientSize = new Size(1000, 620);
            ShowIcon = true;

            _toolbar.GripStyle = ToolStripGripStyle.Hidden;
            _toolbar.Dock = DockStyle.Top;
            _toolbar.ShowItemToolTips = false;
            _toolbar.Items.Add(_btnScreenshot);
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(_btnStop);
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(_statusLabel);

            _btnScreenshot.Enabled = false;
            _btnScreenshot.Click += BtnScreenshot_Click;

            _btnStop.Click += BtnStop_Click;

            _videoPanel.Dock = DockStyle.Fill;
            _videoPanel.BackColor = Color.Black;
            _videoPanel.Tag = "video";
            _videoPanel.Resize += (_, _) => ResizeEmbeddedWindow();

            Controls.Add(_videoPanel);
            Controls.Add(_toolbar);

            _windowTimer.Interval = 500;
            _windowTimer.Tick += WindowTimer_Tick;
            _runner.StatusChanged += Runner_StatusChanged;
            _toolbar.Padding = new Padding(8, 5, 8, 5);
            AppTheme.Attach(this);
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                _runner.Start(
                    _serial,
                    _maxSize,
                    _enableAudio,
                    _expectedDisplaySize,
                    _enableAutomaticCompatibility);

                _windowTimer.Start();
                await EmbedCurrentScrcpyWindowAsync();
            }
            catch (Exception ex)
            {
                AppDialog.Show(
                    this,
                    ex.Message,
                    "scrcpy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _allowClose = true;
                Close();
            }
        }

        private async void WindowTimer_Tick(object? sender, EventArgs e)
        {
            if (_scrcpyWindow == IntPtr.Zero || !IsWindow(_scrcpyWindow))
            {
                _scrcpyWindow = IntPtr.Zero;
                _btnScreenshot.Enabled = false;

                if (_runner.IsRunning)
                    await EmbedCurrentScrcpyWindowAsync();

                return;
            }

            ResizeEmbeddedWindow();
        }

        private async Task EmbedCurrentScrcpyWindowAsync()
        {
            if (_embedding || IsDisposed)
                return;

            _embedding = true;
            try
            {
                IntPtr window = await _runner.WaitForMainWindowHandleAsync(10000);
                if (window == IntPtr.Zero || IsDisposed || !_runner.IsRunning)
                {
                    _statusLabel.Text = "scrcpy window not found";
                    return;
                }

                IntPtr currentStyle = GetWindowLongPtr(window, GwlStyle);
                long style = IntPtr.Size == 8
                    ? currentStyle.ToInt64()
                    : unchecked((uint)currentStyle.ToInt32());
                style &= ~(WsCaption | WsThickFrame | WsMinimizeBox |
                           WsMaximizeBox | WsSysMenu | WsPopup);
                style |= WsChild | WsVisible;

                SetWindowLongPtr(window, GwlStyle, new IntPtr(style));
                SetParent(window, _videoPanel.Handle);
                if (GetParent(window) != _videoPanel.Handle)
                {
                    throw new InvalidOperationException(
                        $"Windows could not embed the scrcpy window (Win32 error {Marshal.GetLastWin32Error()}).");
                }

                SetWindowPos(
                    window,
                    IntPtr.Zero,
                    0,
                    0,
                    _videoPanel.ClientSize.Width,
                    _videoPanel.ClientSize.Height,
                    SwpNoZOrder | SwpNoActivate | SwpFrameChanged);

                _scrcpyWindow = window;
                _btnScreenshot.Enabled = true;
                _statusLabel.Text = "Ready";
            }
            finally
            {
                _embedding = false;
            }
        }

        private void ResizeEmbeddedWindow()
        {
            if (_scrcpyWindow == IntPtr.Zero || !IsWindow(_scrcpyWindow))
                return;

            int width = Math.Max(1, _videoPanel.ClientSize.Width);
            int height = Math.Max(1, _videoPanel.ClientSize.Height);
            MoveWindow(_scrcpyWindow, 0, 0, width, height, true);
        }

        private async void BtnScreenshot_Click(object? sender, EventArgs e)
        {
            if (_scrcpyWindow == IntPtr.Zero || !IsWindow(_scrcpyWindow))
            {
                AppDialog.Show(
                    this,
                    "The scrcpy window is not ready.",
                    "Screenshot",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _btnScreenshot.Enabled = false;
            _statusLabel.Text = "Capturing screenshot...";

            try
            {
                Activate();
                await Task.Delay(150);

                Rectangle captureArea = _videoPanel.RectangleToScreen(_videoPanel.ClientRectangle);
                if (captureArea.Width <= 0 || captureArea.Height <= 0)
                    throw new InvalidOperationException("The scrcpy display area has an invalid size.");

                using var bitmap = new Bitmap(
                    captureArea.Width,
                    captureArea.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        captureArea.Location,
                        Point.Empty,
                        captureArea.Size,
                        CopyPixelOperation.SourceCopy);
                }

                string picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (string.IsNullOrWhiteSpace(picturesFolder) || !Directory.Exists(picturesFolder))
                    picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                using var saveDialog = new SaveFileDialog
                {
                    Title = "Save Visible TV Control Screenshot",
                    Filter = "PNG image (*.png)|*.png",
                    DefaultExt = "png",
                    AddExtension = true,
                    OverwritePrompt = true,
                    InitialDirectory = picturesFolder,
                    FileName = $"ADB_Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                };

                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    _statusLabel.Text = "Screenshot save canceled";
                    return;
                }

                bitmap.Save(saveDialog.FileName, ImageFormat.Png);
                _statusLabel.Text = $"Saved: {Path.GetFileName(saveDialog.FileName)}";
                AppDialog.Show(
                    this,
                    $"Screenshot saved successfully.\n\n{saveDialog.FileName}",
                    "Screenshot",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Screenshot failed";
                AppDialog.Show(
                    this,
                    ex.Message,
                    "Screenshot",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _btnScreenshot.Enabled = _scrcpyWindow != IntPtr.Zero && IsWindow(_scrcpyWindow);
            }
        }

        private async void BtnStop_Click(object? sender, EventArgs e)
        {
            await StopAndCloseAsync();
        }

        private void Runner_StatusChanged(string status)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            BeginInvoke(new Action(async () =>
            {
                if (IsDisposed)
                    return;

                _statusLabel.Text = status;

                if (status.StartsWith("Running", StringComparison.OrdinalIgnoreCase))
                {
                    await EmbedCurrentScrcpyWindowAsync();
                }
                else if (status.StartsWith("Stopped", StringComparison.OrdinalIgnoreCase) &&
                         !_stopInProgress)
                {
                    _allowClose = true;
                    Close();
                }
            }));
        }

        private async Task StopAndCloseAsync()
        {
            if (_stopInProgress)
                return;

            _stopInProgress = true;
            _btnStop.Enabled = false;
            _btnScreenshot.Enabled = false;
            _statusLabel.Text = "Stopping...";

            try
            {
                await _runner.StopAsync();
            }
            finally
            {
                _allowClose = true;
                if (!IsDisposed)
                    Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose && _runner.IsRunning)
            {
                e.Cancel = true;
                _ = StopAndCloseAsync();
                return;
            }

            _windowTimer.Stop();
            _runner.StatusChanged -= Runner_StatusChanged;
            base.OnFormClosing(e);
        }

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int index)
        {
            return IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, index)
                : new IntPtr(GetWindowLong32(hWnd, index));
        }

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int index, IntPtr newValue)
        {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, index, newValue)
                : new IntPtr(SetWindowLong32(hWnd, index, newValue.ToInt32()));
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr childWindow, IntPtr newParentWindow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveWindow(
            IntPtr hWnd,
            int x,
            int y,
            int width,
            int height,
            [MarshalAs(UnmanagedType.Bool)] bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr insertAfter,
            int x,
            int y,
            int width,
            int height,
            uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
        private static extern int GetWindowLong32(IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
        private static extern int SetWindowLong32(IntPtr hWnd, int index, int newValue);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int index, IntPtr newValue);
    }
}

using System.Diagnostics;
using System.Text;

namespace ADB_Connect
{
    public partial class Form1 : Form
    {
        private bool _isConnected = false;
        private string? _connectedSerial = null;
        private string? _packages = "packages";
        private List<string> _allPackagesList = new List<string>();
        private readonly AdbProgressRunner _adbProgress = new AdbProgressRunner();
        private Process? _logcatProcess;
        private readonly System.Windows.Forms.Timer _logFlushTimer = new();
        private readonly System.Collections.Concurrent.ConcurrentQueue<string> _logQueue = new();
        private const int MaxLogChars = 300_000; // حدود 300KB - قابل تنظیم
        private readonly ScrcpyRunner _scrcpyRunner = new();
        private bool _updatingDeviceSelection;
        private ScrcpyHostForm? _scrcpyHostForm;

        private void UpdateUiByConnectionState()
        {
            btnReboot.Enabled = _isConnected;
            btnVersion.Enabled = _isConnected;
            btnInstallApk.Enabled = _isConnected;
            btnPackageList.Enabled = _isConnected;
            btnRecovery.Enabled = _isConnected;
            btnBootloader.Enabled = _isConnected;
            btnFastboot.Enabled = _isConnected;
            btnKernelTest.Enabled = _isConnected;
            btnVendorFingerprint.Enabled = _isConnected;
            btnVendorBuildDate.Enabled = _isConnected;
            btnDisplaySize.Enabled = _isConnected;
            button2.Enabled = _isConnected;
            btnBuildId.Enabled = _isConnected;
            btnSkdVersion.Enabled = _isConnected;
            btnSerialNo.Enabled = _isConnected;
            btnVendorName.Enabled = _isConnected;
            btnVendorModel.Enabled = _isConnected;
            btnVendorBrand.Enabled = _isConnected;
            btnManufacturer.Enabled = _isConnected;
            btnDevice.Enabled = _isConnected;
            btnBoardName.Enabled = _isConnected;
            btnCpuType.Enabled = _isConnected;
            btnOemKey.Enabled = _isConnected;
            button3.Enabled = _isConnected;
            btnValidation.Enabled = _isConnected;
            btnHardware.Enabled = _isConnected;
            btnClientIdBase.Enabled = _isConnected;
            btnBuildFingerprint.Enabled = _isConnected;
            btnVbmeta.Enabled = _isConnected;
            btnTimeZone.Enabled = _isConnected;
            btnArm.Enabled = _isConnected;
            btnGetEnforce.Enabled = _isConnected;
            btnGetProps.Enabled = _isConnected;
            btnStartApp.Enabled = _isConnected;
            btnStopApp.Enabled = _isConnected;
            ckbAllApp.Enabled = _isConnected;
            ckbSystemApps.Enabled = _isConnected;
            ckbUserApps.Enabled = _isConnected;
            btnStartLog.Enabled = _isConnected;
            btnStopLog.Enabled = _isConnected;
            btnUninstall.Enabled = _isConnected;
            btnBugreport.Enabled = _isConnected;

            bool scrcpyRunning = _scrcpyRunner.IsRunning;
            bool logcatRunning = _logcatProcess is { HasExited: false };
            bool scrcpyHostOpen = _scrcpyHostForm is { IsDisposed: false };
            comboBox1.Enabled = _isConnected && !scrcpyRunning && !scrcpyHostOpen && !logcatRunning;
            comboBox2.Enabled = _isConnected && !scrcpyRunning && !scrcpyHostOpen;
            checkBox1.Enabled = _isConnected && !scrcpyRunning && !scrcpyHostOpen;
            button1.Enabled = _isConnected && !scrcpyRunning && !scrcpyHostOpen;
            btnRefreshScrcpyDevices.Enabled = !scrcpyRunning && !scrcpyHostOpen;

        }

        private Task<(int exitCode, string stdout, string stderr)> RunAdbAsync(string args, int timeoutMs = 20000)
        {
            // Host-level commands must not be scoped to a selected device.
            string trimmedArgs = args.TrimStart();
            bool isHostCommand =
                trimmedArgs.Equals("devices", StringComparison.OrdinalIgnoreCase) ||
                trimmedArgs.StartsWith("devices ", StringComparison.OrdinalIgnoreCase) ||
                trimmedArgs.Equals("start-server", StringComparison.OrdinalIgnoreCase) ||
                trimmedArgs.Equals("kill-server", StringComparison.OrdinalIgnoreCase) ||
                trimmedArgs.Equals("disconnect", StringComparison.OrdinalIgnoreCase) ||
                trimmedArgs.StartsWith("disconnect ", StringComparison.OrdinalIgnoreCase) ||
                trimmedArgs.StartsWith("connect ", StringComparison.OrdinalIgnoreCase);

            // Add the globally selected serial to every device-targeted command.
            if (!string.IsNullOrWhiteSpace(_connectedSerial) &&
                !isHostCommand &&
                !args.Contains(" -s ") &&
                !args.StartsWith("-s "))
            {
                args = $"-s {_connectedSerial} {args}";
            }

            return AdbRunner.RunAsync(args, timeoutMs);
        }


        private async void packageListUpdate()
        {
            _isConnected = false;
            UpdateUiByConnectionState();
            checkedListBox1.Items.Clear();

            var result = await Task.Run(() =>
                RunAdbAsync($"shell pm list {_packages}", 20000)
            );

            if (result.exitCode != 0)
            {
                MessageBox.Show(result.stderr, "ADB Error");
                return;
            }

            // ذخیره پکیج‌ها در لیست مرجع
            _allPackagesList = result.stdout
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Replace("package:", "").Trim())
                .OrderBy(p => p)
                .ToList();

            // نمایش لیست (در این مرحله چون تکست‌باکس خالی است، همه را نشان می‌دهد)
            ApplyFilter();

            _isConnected = true;
            btnUninstall.Enabled = true;
            UpdateUiByConnectionState();
        }

        private void ApplyFilter()
        {
            string searchTerm = txbSearchPackages.Text.ToLower().Trim();

            checkedListBox1.BeginUpdate();
            checkedListBox1.Items.Clear();

            // فیلتر کردن لیست مرجع بر اساس متن جست‌وجو
            var filteredList = _allPackagesList
                .Where(pkg => pkg.ToLower().Contains(searchTerm))
                .ToList();

            foreach (var pkg in filteredList)
            {
                checkedListBox1.Items.Add(pkg, false);
            }
            checkedListBox1.EndUpdate();
        }

        private void AppendLog(string message, Color color)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => AppendLog(message, color)));
                return;
            }

            // ۱. آماده‌سازی برای اضافه کردن متن رنگی
            // ابتدا نشانگر را به انتهای متن فعلی می‌بریم
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;

            // ۲. تعیین رنگ انتخابی برای متنی که قرار است اضافه شود
            rtbLog.SelectionColor = color;

            // ۳. اضافه کردن متن با فرمت زمان (حالا با رنگ جدید اعمال می‌شود)
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");

            // ۴. بازگرداندن رنگ به حالت پیش‌فرض برای جلوگیری از تغییر رنگ ناخواسته در آینده
            rtbLog.SelectionColor = rtbLog.ForeColor;

            // ۵. مدیریت محدودیت تعداد خطوط (Memory Management)
            if (!int.TryParse(txtLineLimitation.Text, out int maxLines))
            {
                maxLines = 2000;
            }

            if (rtbLog.Lines.Length > maxLines)
            {
                // محاسبه تعداد خطوطی که باید حذف شوند (کمی بیشتر حذف می‌کنیم تا پرفورمنس بهتر شود)
                int linesToRemove = rtbLog.Lines.Length - maxLines + 50;
                int charIndex = rtbLog.GetFirstCharIndexFromLine(linesToRemove);

                if (charIndex > 0)
                {
                    // ذخیره وضعیت فعلی ReadOnly برای لحظه‌ای کوتاه جهت حذف متن
                    bool isReadOnly = rtbLog.ReadOnly;
                    rtbLog.ReadOnly = false;

                    rtbLog.Select(0, charIndex);
                    rtbLog.SelectedText = "";

                    rtbLog.ReadOnly = isReadOnly;
                }
            }

            // ۶. اسکرول خودکار به انتهای صفحه
            rtbLog.ScrollToCaret();
        }

        private async Task<string> GetPidByPackageNameAsync(string packageName)
        {
            var res = await RunAdbAsync($"-s {_connectedSerial} shell pidof -s {packageName}", 15000);
            return (res.exitCode == 0 ? res.stdout : "").Trim();
        }


        private bool EnsureConnected()
        {
            if (!_isConnected || string.IsNullOrWhiteSpace(_connectedSerial) ||
        _connectedSerial.Contains("\n") || _connectedSerial.Contains(" "))
            {
                MessageBox.Show("First, connect the device.");
                return false;
            }
            return true;
        }

        private void EnqueueLogLine(string line)
        {
            _logQueue.Enqueue(line);
        }

        private void FlushLogToRtb()
        {
            if (_logQueue.IsEmpty) return;

            // حتما روی UI Thread هستیم چون Timer ویندوز فرمز روی UI می‌زند
            var sb = new StringBuilder();
            int count = 0;

            while (count < 200 && _logQueue.TryDequeue(out var line)) // هر Tick تا 200 خط
            {
                sb.AppendLine(line);
                count++;
            }

            // جلوی پرش: Scroll lock موقت
            rtbLog.SuspendLayout();

            // محدودیت حجم برای جلوگیری از کندی و باگ‌های عجیب
            if (rtbLog.TextLength > MaxLogChars)
            {
                rtbLog.Clear();
            }

            rtbLog.AppendText(sb.ToString());
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();

            rtbLog.ResumeLayout();
        }


        public Form1()
        {
            InitializeComponent();
            System.Drawing.Icon? executableIcon =
                System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (executableIcon is not null)
                Icon = executableIcon;

            UpdateUiByConnectionState();
            // checkedListBox1.Visible = false;
            btnUninstall.Enabled = false;
            _logFlushTimer.Interval = 80; // 50 تا 150 خوبه
            _logFlushTimer.Tick += (_, __) => FlushLogToRtb();
            _logFlushTimer.Start();

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.Items.AddRange(new object[]
            {
                "Original",
                "1056 (TV Compatibility)",
                "1920",
                "1280",
                "1024"
            });
            comboBox2.SelectedIndex = 0;
            checkBox1.Checked = false;
            label11.Text = !_scrcpyRunner.IsAvailable
                ? "scrcpy folder not found"
                : _scrcpyRunner.IsCompatibilityAvailable
                    ? "Ready"
                    : "Ready - compatibility runtime missing";

            _scrcpyRunner.StatusChanged += ScrcpyRunner_StatusChanged;
            _scrcpyRunner.OutputReceived += ScrcpyRunner_OutputReceived;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
        }

        private void ScrcpyRunner_StatusChanged(string status)
        {
            if (IsDisposed || !IsHandleCreated) return;

            BeginInvoke(new Action(() =>
            {
                label11.Text = status;
                UpdateUiByConnectionState();
            }));
        }

        private void ScrcpyRunner_OutputReceived(string line)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action(() => AppendLog($"scrcpy: {line}", Color.MediumPurple)));
        }

        private async void TabControl1_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage4 && !_scrcpyRunner.IsRunning)
                await RefreshScrcpyDevicesAsync();
        }

        private static List<string> ParseOnlineDeviceSerials(string? adbDevicesOutput)
        {
            return (adbDevicesOutput ?? string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
                .Where(line => line.Contains("\tdevice", StringComparison.Ordinal))
                .Select(line => line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0])
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private void SetScrcpyDevices(IEnumerable<string> serials)
        {
            string? previous = comboBox1.SelectedItem as string;
            var items = serials.Distinct(StringComparer.Ordinal).ToList();

            _updatingDeviceSelection = true;
            try
            {
                comboBox1.BeginUpdate();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(items.Cast<object>().ToArray());
                comboBox1.EndUpdate();

                string? preferred = items.FirstOrDefault(x => x == _connectedSerial)
                    ?? items.FirstOrDefault(x => x == previous)
                    ?? items.FirstOrDefault();

                if (preferred is not null)
                {
                    comboBox1.SelectedItem = preferred;
                    _connectedSerial = preferred;
                    _isConnected = true;
                }
                else
                {
                    _connectedSerial = null;
                    _isConnected = false;
                }
            }
            finally
            {
                _updatingDeviceSelection = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingDeviceSelection) return;

            if (comboBox1.SelectedItem is string serial && !string.IsNullOrWhiteSpace(serial))
            {
                _connectedSerial = serial;
                _isConnected = true;
                label11.Text = $"Active: {serial}";
                AppendLog($"Active device changed to {serial}.", Color.DodgerBlue);
            }
            else
            {
                _connectedSerial = null;
                _isConnected = false;
            }

            UpdateUiByConnectionState();
        }

        private async Task RefreshScrcpyDevicesAsync()
        {
            btnRefreshScrcpyDevices.Enabled = false;
            label11.Text = "Checking devices...";

            try
            {
                // Do not use RunAdbAsync here; "adb devices" must not receive a -s argument.
                var result = await AdbRunner.RunAsync("devices", 20000);
                if (result.exitCode != 0)
                {
                    label11.Text = "ADB error";
                    AppendLog(result.stderr, Color.Red);
                    return;
                }

                var serials = ParseOnlineDeviceSerials(result.stdout);
                SetScrcpyDevices(serials);
                label11.Text = serials.Count == 0
                    ? "No online device"
                    : $"{serials.Count} device(s) ready";
            }
            catch (Exception ex)
            {
                label11.Text = "Device check failed";
                AppendLog(ex.Message, Color.Red);
            }
            finally
            {
                UpdateUiByConnectionState();
            }
        }

        private async void btnRefreshScrcpyDevices_Click(object sender, EventArgs e)
        {
            await RefreshScrcpyDevicesAsync();
        }

        private async void btnPairDevice_Click(object sender, EventArgs e)
        {
            string defaultIp = txtIp.Text.Trim();
            if (defaultIp.Count(c => c == ':') == 1)
                defaultIp = defaultIp.Split(':')[0];

            using var dialog = new PairDeviceForm(defaultIp);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            btnCheckDevice.Enabled = false;
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = false;

            try
            {
                AppendLog(
                    $"Pairing {dialog.DeviceIp} on port {dialog.PairingPort}; connection port is {dialog.ConnectionPort}.",
                    Color.DodgerBlue);

                // The pairing code is written through stdin by PairAsync and is never logged.
                var pairResult = await AdbRunner.PairAsync(
                    dialog.PairingEndpoint,
                    dialog.PairingCode,
                    timeoutMs: 30000);

                string pairOutput = $"{pairResult.stdout}\n{pairResult.stderr}".Trim();
                if (pairResult.exitCode != 0)
                {
                    AppendLog($"Pairing failed: {pairOutput}", Color.Red);
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(pairOutput) ? "Pairing failed." : pairOutput,
                        "Pair Device",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                AppendLog($"Successfully paired to {dialog.PairingEndpoint}.", Color.Green);

                // ConnectionEndpoint deliberately uses the independent Connection Port.
                var connectResult = await AdbRunner.RunAsync(
                    $"connect {dialog.ConnectionEndpoint}",
                    timeoutMs: 20000);

                string connectOutput = $"{connectResult.stdout}\n{connectResult.stderr}".Trim();
                bool connected = connectResult.exitCode == 0 &&
                    (connectOutput.Contains("connected to", StringComparison.OrdinalIgnoreCase) ||
                     connectOutput.Contains("already connected", StringComparison.OrdinalIgnoreCase));

                if (!connected)
                {
                    AppendLog($"Pair succeeded, but connection failed: {connectOutput}", Color.OrangeRed);
                    MessageBox.Show(
                        $"Pairing succeeded, but ADB could not connect to {dialog.ConnectionEndpoint}.\n\n{connectOutput}",
                        "Pair Device",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                await RefreshScrcpyDevicesAsync();

                if (comboBox1.Items.Contains(dialog.ConnectionEndpoint))
                    comboBox1.SelectedItem = dialog.ConnectionEndpoint;

                txtIp.Clear();
                AppendLog($"Connected to {dialog.ConnectionEndpoint}.", Color.Green);
                MessageBox.Show(
                    $"Pairing and connection completed successfully.\n\nPair port: {dialog.PairingPort}\nConnect port: {dialog.ConnectionPort}",
                    "Pair Device",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog($"Pairing error: {ex.Message}", Color.Red);
                MessageBox.Show(ex.Message, "Pair Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckDevice.Enabled = true;
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = true;
                UpdateUiByConnectionState();
            }
        }

        private static (int Width, int Height)? ParseEffectiveDisplaySize(string wmSizeOutput)
        {
            (int Width, int Height)? physicalSize = null;
            (int Width, int Height)? overrideSize = null;
            (int Width, int Height)? fallbackSize = null;

            foreach (string line in (wmSizeOutput ?? string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    line,
                    @"(?<width>\d+)\s*x\s*(?<height>\d+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (!match.Success ||
                    !int.TryParse(match.Groups["width"].Value, out int width) ||
                    !int.TryParse(match.Groups["height"].Value, out int height) ||
                    width <= 0 || height <= 0)
                {
                    continue;
                }

                var parsed = (Width: width, Height: height);
                fallbackSize = parsed;

                if (line.Contains("override", StringComparison.OrdinalIgnoreCase))
                    overrideSize = parsed;
                else if (line.Contains("physical", StringComparison.OrdinalIgnoreCase))
                    physicalSize = parsed;
            }

            // Android mirrors the override size when one is active; otherwise use physical.
            return overrideSize ?? physicalSize ?? fallbackSize;
        }

        private async void btnStartScrcpy_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;

            if (_scrcpyHostForm is { IsDisposed: false })
            {
                _scrcpyHostForm.Activate();
                return;
            }

            if (comboBox1.SelectedItem is not string serial || string.IsNullOrWhiteSpace(serial))
            {
                MessageBox.Show("Select an online device first.", "scrcpy");
                return;
            }

            int? maxSize = null;
            if (comboBox2.SelectedItem is string selectedSize &&
                !selectedSize.Equals("Original", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(selectedSize.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0], out int parsedSize))
            {
                maxSize = parsedSize;
            }

            button1.Enabled = false;
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            checkBox1.Enabled = false;

            try
            {
                (int Width, int Height)? expectedDisplaySize = null;
                bool enableAutomaticCompatibility = maxSize is null;

                if (enableAutomaticCompatibility)
                {
                    label11.Text = "Checking display size...";
                    expectedDisplaySize = await _scrcpyRunner.GetPrimaryDisplaySizeAsync(serial);

                    // Fall back to wm size if a vendor build does not support --list-displays.
                    if (expectedDisplaySize is null)
                    {
                        var sizeResult = await RunAdbAsync("shell wm size", 10000);
                        if (sizeResult.exitCode == 0)
                            expectedDisplaySize = ParseEffectiveDisplaySize(sizeResult.stdout);
                    }

                    if (expectedDisplaySize is { } expected)
                    {
                        AppendLog(
                            $"Expected display size for automatic scrcpy check: {expected.Width}x{expected.Height}.",
                            Color.MediumPurple);
                    }
                    else
                    {
                        AppendLog(
                            "ADB did not report a usable display size; scrcpy will start without automatic resolution fallback.",
                            Color.OrangeRed);
                    }
                }

                label11.Text = "Opening TV Control...";
                var hostForm = new ScrcpyHostForm(
                    _scrcpyRunner,
                    serial,
                    maxSize,
                    checkBox1.Checked,
                    expectedDisplaySize,
                    enableAutomaticCompatibility);
                hostForm.Icon = Icon;

                _scrcpyHostForm = hostForm;
                hostForm.FormClosed += (_, _) =>
                {
                    _scrcpyHostForm = null;
                    if (!IsDisposed && IsHandleCreated)
                    {
                        label11.Text = "Ready";
                        UpdateUiByConnectionState();
                    }
                };
                hostForm.Show(this);

                AppendLog($"Embedded TV Control opened for {serial}.", Color.MediumPurple);
                UpdateUiByConnectionState();
            }
            catch (Exception ex)
            {
                label11.Text = "Start failed";
                MessageBox.Show(ex.Message, "scrcpy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateUiByConnectionState();
            }
        }

        // Devices Button action
        private async void btnChekDevices(object sender, EventArgs e)
        {
            var result = await AdbRunner.RunAsync("devices", 20000);

            var lines = (result.stdout ?? "").Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var deviceLines = lines
            .Where(l => !l.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
            .ToList();

            if (deviceLines.Count == 0)
            {
                MessageBox.Show("No devices are connected.");
                SetScrcpyDevices(Array.Empty<string>());
                UpdateUiByConnectionState();
                return;
            }

            var online = deviceLines.Where(l => l.Contains("\tdevice")).ToList();
            var offline = deviceLines.Where(l => l.Contains("\toffline")).ToList();
            var unauthorized = deviceLines.Where(l => l.Contains("\tunauthorized")).ToList();

            var msg =
                $"Total: {deviceLines.Count}\n" +
                $"Online: {online.Count}\n" +
                $"Offline: {offline.Count}\n" +
                $"Unauthorized: {unauthorized.Count}\n\n" +
                string.Join("\n", deviceLines);

            MessageBox.Show(msg);

            // ✅ وضعیت اتصال را بر اساس آنلاین‌ها تعیین کن
            _isConnected = online.Count > 0;

            // ✅ فقط یک serial معتبر ذخیره کن (اولین دستگاه آنلاین)
            if (_isConnected)
            {
                var firstOnlineLine = online[0];
                _connectedSerial = firstOnlineLine
                    .Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

                SetScrcpyDevices(ParseOnlineDeviceSerials(result.stdout));
            }
            else
            {
                _connectedSerial = null;
                SetScrcpyDevices(Array.Empty<string>());
            }

            UpdateUiByConnectionState();
        }

        // Connect Button action
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;

            try
            {
                var ip = txtIp.Text.Trim();
                if (string.IsNullOrWhiteSpace(ip))
                {
                    MessageBox.Show("Enter the IP.");
                    return;
                }


                // If port not exist set the 5555 by default.
                if (!ip.Contains(":"))
                    ip += ":5555";

                await Task.Run(() => RunAdbAsync("start-server"));

                var result = await Task.Run(() => RunAdbAsync($"connect {ip}", 20000));
                var combined = (result.stdout + "\n" + result.stderr).ToLowerInvariant();

                _isConnected = combined.Contains("connected to") || combined.Contains("already connected");
                _connectedSerial = _isConnected ? ip : null;

                if (_isConnected)
                {
                    await RefreshScrcpyDevicesAsync();
                    txtIp.Clear();
                }

                UpdateUiByConnectionState();

                MessageBox.Show(_isConnected ? $"Connected: {_connectedSerial}" : $"Connect failed:\n{result.stdout}\n{result.stderr}");
            }
            catch (Exception ex)
            {
                await RefreshScrcpyDevicesAsync();
                MessageBox.Show(ex.Message);
            }

            finally
            {
                btnConnect.Enabled = true;
            }
        }

        // Disconnect Button action
        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                await _scrcpyRunner.StopAsync();
                var args = string.IsNullOrWhiteSpace(_connectedSerial) ? "disconnect" : $"disconnect {_connectedSerial}";
                var result = await Task.Run(() => RunAdbAsync(args, timeoutMs: 20000));

                if (result.exitCode == 0) MessageBox.Show(result.stdout);
                else MessageBox.Show(result.stderr);

                await RefreshScrcpyDevicesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        // Reboot Button action
        private async void btnReboot_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var r = await Task.Run(() => RunAdbAsync($"-s {_connectedSerial} reboot", 15000));

            if (r.exitCode == 0)
            {
                MessageBox.Show("The TV rebooted.");
                btnConnect.Enabled = true;
            }
            else
                MessageBox.Show(r.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();



        }

        // Install Button action
        private async void btnInstallApk_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;

            using var ofd = new OpenFileDialog
            {
                Title = "Select APK to install",
                Filter = "APK files (*.apk)|*.apk",
                CheckFileExists = true
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string apkPath = ofd.FileName;

            // --- شروع عملیات ---
            _isConnected = false;
            UpdateUiByConnectionState();
            btnInstallApk.Enabled = false;

            // نمایش گیف لودینگ
            checkedListBox1.Enabled = false;
            pbLoading.Visible = true;

            try
            {
                var args = $"-s {_connectedSerial} install -r \"{apkPath}\"";

                // اجرای دستور در پس‌زمینه (بدون فریز شدن فرم)
                var result = await Task.Run(() => RunAdbAsync(args, timeoutMs: 120000));

                var combined = (result.stdout + "\n" + result.stderr);
                bool ok = combined.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;

                MessageBox.Show(ok ? "Installation completed successfully ✅" : $"Installation failed ❌\n\n{combined}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // --- اتمام عملیات ---

                // مخفی کردن گیف لودینگ
                pbLoading.Visible = false;
                checkedListBox1.Enabled = true;

                _isConnected = true;
                btnInstallApk.Enabled = true;
                UpdateUiByConnectionState();
            }
        }


        // UnInstall Button action
        private async void btnUninstall_Click_1(object sender, EventArgs e)
        {
            var packages = checkedListBox1.CheckedItems
        .Cast<string>()
        .ToList();

            if (packages.Count == 0)
            {
                MessageBox.Show("Please select an item");
                return;
            }

            foreach (var pkg in packages)
            {
                var result = await Task.Run(() =>
                    RunAdbAsync($"shell pm uninstall --user 0 {pkg}", 20000)
                );


                if (!result.stdout.Contains("Success"))
                {
                    Debug.WriteLine($"Failed: {pkg} -> {result.stderr}");
                }
            }

            for (int i = checkedListBox1.Items.Count - 1; i >= 0; i--)
            {
                if (checkedListBox1.GetItemChecked(i))
                    checkedListBox1.Items.RemoveAt(i);
            }

            MessageBox.Show("Uninstall successfully");
        }

        // Start App Button action
        private async void btnStartApp_Click(object sender, EventArgs e)
        {
            var packages = checkedListBox1.CheckedItems
            .Cast<string>()
            .ToList();

            if (packages.Count == 0)
            {
                MessageBox.Show("Please select an item");
                return;
            }

            foreach (var pkg in packages)
            {
                var result = await Task.Run(() =>
                    RunAdbAsync($"shell monkey -p {pkg} -c android.intent.category.LAUNCHER 1", 20000)
                );
                AppendLog($"Starting app: {pkg}", Color.Cyan);


                if (!result.stdout.Contains("Success"))
                {
                    Debug.WriteLine($"Failed: {pkg} -> {result.stderr}");
                }
            }

            MessageBox.Show("The app started");
        }

        // Stop App Button action
        private async void btnStopApp_Click(object sender, EventArgs e)
        {
            var packages = checkedListBox1.CheckedItems
            .Cast<string>()
            .ToList();

            if (packages.Count == 0)
            {
                MessageBox.Show("Please select an item");
                return;
            }

            foreach (var pkg in packages)
            {
                var result = await Task.Run(() =>
                    RunAdbAsync($"shell am force-stop {pkg}", 20000)
                );
                AppendLog($"Force stopping app: {pkg}", Color.Orange);


                if (!result.stdout.Contains("Success"))
                {
                    Debug.WriteLine($"Failed: {pkg} -> {result.stderr}");
                }
            }

            MessageBox.Show("The app stoped");
        }


        // Package list Button action
        private async void btnPackages_Click(object sender, EventArgs e)
        {
            // checkedListBox1.Visible = true;\
            if (!EnsureConnected()) return;

            packageListUpdate();

        }

        // Version Button action
        private async void btnVersion_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.build.version.release", 20000));

            if (result.exitCode == 0)
                MessageBox.Show($"Android: {result.stdout}");
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Recovery Button action
        private async void btnRecovery_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var r = await Task.Run(() => RunAdbAsync($"-s {_connectedSerial} reboot recovery", 15000));

            if (r.exitCode == 0)
                MessageBox.Show("The TV went into recovery mode.");
            else
                MessageBox.Show(r.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Bootloader Button action
        private async void btnBootloader_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var r = await Task.Run(() => RunAdbAsync($"-s {_connectedSerial} reboot bootloader", 15000));

            if (r.exitCode == 0)
                MessageBox.Show("The TV went into recovery mode.");
            else
                MessageBox.Show(r.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Fastboot Button action
        private async void btnFastboot_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var r = await Task.Run(() => RunAdbAsync($"-s {_connectedSerial} reboot fastboot", 15000));

            if (r.exitCode == 0)
                MessageBox.Show("The TV went into recovery mode.");
            else
                MessageBox.Show(r.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Kernel Check Button action
        private async void btnKernelTest_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell uname -a", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Display Resolution Button action
        private async void btnDisplaySize_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop vendor.display-size", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Vendor Finger Print Check Button action
        private async void btnVendorFingerprint_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.vendor.build.fingerprint", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }


        // Build Date Check Button action
        private async void btnVendorBuildDate_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.vendor.build.date", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Software Version Check Button action
        private async void button2_Click_1(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.software.version_id", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Build ID Check Button action
        private async void btnBuildId_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.system_ext.build.id", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // SKD Version Check Button action
        private async void btnSkdVersion_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.system.build.version.sdk", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Serial Number Check Button action
        private async void btnSerialNo_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.serialno", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Vendor Name Check Button action
        private async void btnVendorName_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.product.vendor.name", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Vendor Model Check Button action
        private async void btnVendorModel_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.product.vendor.model", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Vendor Brand Check Button action
        private async void btnVendorBrand_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.product.vendor.brand", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Manufacturer Check Button action
        private async void btnManufacturer_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.product.system_ext.manufacturer", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Device Check Button action
        private async void btnDevice_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.product.system.device", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Board Name Check Button action
        private async void btnBoardName_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.product.board", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // CPU abi Check Button action
        private async void btnCpuType_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.product.cpu.abi", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // OEM Key Check Button action
        private async void btnOemKey_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.oem.key1", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // OpenGL Check Button action
        private async void button3_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.opengles.version", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Validation Check Button action
        private async void btnValidation_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.nrdp.validation", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Hardware Check Button action
        private async void btnHardware_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.hardware", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Client Base Check Button action
        private async void btnClientIdBase_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.com.google.clientidbase", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Build Fingerprint Check Button action
        private async void btnBuildFingerprint_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.build.fingerprint", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // VB Meta State Check Button action
        private async void btnVbmeta_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop ro.boot.vbmeta.device_state", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Time Zone Check Button action
        private async void btnTimeZone_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop persist.sys.timezone", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // ARM Check Button action
        private async void btnArm_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getprop dalvik.vm.isa.arm.variant", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Get Enforce Check Button action
        private async void btnGetEnforce_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            _isConnected = false;
            UpdateUiByConnectionState();

            var result = await Task.Run(() => RunAdbAsync("shell getenforce", 20000));

            if (result.exitCode == 0)
                MessageBox.Show(result.stdout);
            else MessageBox.Show(result.stderr);

            _isConnected = true;
            UpdateUiByConnectionState();
        }

        // Get Propertise Check Button action
        private async void btnGetProps_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
                return;

            using var sfd = new SaveFileDialog
            {
                Title = "Save Android Properties",
                Filter = "Text File (*.txt)|*.txt",
                FileName = "android_props.txt",
                OverwritePrompt = true
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var result = await Task.Run(() =>
                RunAdbAsync("shell getprop", 30000)
            );

            if (result.exitCode != 0 || string.IsNullOrWhiteSpace(result.stdout))
            {
                MessageBox.Show(
                    "Failed to read device properties.\n" + result.stderr,
                    "ADB Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            try
            {
                File.WriteAllText(sfd.FileName, result.stdout, Encoding.UTF8);
                MessageBox.Show(
                    "Properties saved successfully.",
                    "Done",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "File Write Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        //Checkbox for apps change action
        private void ckbSystemApps_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbSystemApps.Checked)
            {
                ckbAllApp.Checked = false;
                ckbUserApps.Checked = false;
            }

            _packages = "packages -s";
        }

        private void ckbAllApp_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbAllApp.Checked)
            {
                ckbSystemApps.Checked = false;
                ckbUserApps.Checked = false;
            }

            _packages = "packages";
        }

        private void ckbUserApps_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbUserApps.Checked)
            {
                ckbAllApp.Checked = false;
                ckbSystemApps.Checked = false;
            }

            _packages = "packages -3";
        }

        //Search for apps change action
        private void txbSearchPackages_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        //Start log button change action
        private async void btnStartLog_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;

            string package = txtPackageFilter.Text.Trim();
            string pid = "";

            // اگر نام پکیج وارد شده بود، PID آن را پیدا کن
            if (!string.IsNullOrEmpty(package))
            {
                pid = await GetPidByPackageNameAsync(package);

                if (string.IsNullOrEmpty(pid))
                {
                    MessageBox.Show("The desired program is not running or could not be found.");
                    return;
                }
            }

            rtbLog.Clear();
            btnStartLog.Enabled = false;
            btnStopLog.Enabled = true;

            // ساخت دستور ADB
            // --pid: فیلتر بر اساس پروسس خاص
            // -v time: نمایش زمان
            string arguments = $"-s {_connectedSerial} logcat -v time";
            if (!string.IsNullOrEmpty(pid)) arguments += $" --pid={pid}";

            var psi = new ProcessStartInfo
            {
                FileName = AdbRunner.AdbPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            _logcatProcess = new Process { StartInfo = psi };
            _logcatProcess.OutputDataReceived += (s, ev) =>
            {
                if (!string.IsNullOrEmpty(ev.Data))
                {
                    // --- فیلترینگ حین اجرا (بر اساس متن موجود در txtFilter) ---
                    string textFilter = txtFilter.Text.Trim();
                    if (!string.IsNullOrEmpty(textFilter) && !ev.Data.Contains(textFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        return; // اگر حاوی متن فیلتر نبود، نادیده بگیر
                    }

                    Color logColor = Color.White;
                    if (ev.Data.Contains(" E ")) logColor = Color.Red;   // Error
                    if (ev.Data.Contains(" W ")) logColor = Color.Yellow; // Warning

                    AppendLog(ev.Data, logColor);
                }
            };

            _logcatProcess.Start();
            _logcatProcess.BeginOutputReadLine();
        }

        //Stop log button change action
        private void btnStopLog_Click(object sender, EventArgs e)
        {
            if (_logcatProcess != null && !_logcatProcess.HasExited)
            {
                try
                {
                    _logcatProcess.Kill(true); // بستن پردازش و تمام زیرشاخه ها
                    AppendLog("Logcat stopped by user.", Color.Orange);
                }
                catch { /* ignore */ }
            }

            _logcatProcess = null;

            btnStartLog.Enabled = true;
            btnStopLog.Enabled = false;
        }

        //Export log button change action
        private void btnExportLog_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(rtbLog.Text))
            {
                MessageBox.Show("There is no log to save.");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Save Log File",
                Filter = "Text Files (*.txt)|*.txt",
                FileName = $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, rtbLog.Text);
                    MessageBox.Show("The file was saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}");
                }
            }
        }

        //Line log limitation change action
        private void txtLineLimitation_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        //Clear log button change action
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            // ۱. پاک کردن صفحه نمایش
            rtbLog.Clear();

            // ۲. پاک کردن بافر لاگ در خود دستگاه اندرویدی
            if (_isConnected)
            {
                Task.Run(() => RunAdbAsync($"-s {_connectedSerial} logcat -c"));
                AppendLog("Device log buffer cleared.", Color.Green);
            }
        }

        //Line log leave change action
        private void txtLineLimitation_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLineLimitation.Text))
            {
                txtLineLimitation.Text = "2000";
                txtLineLimitation.ForeColor = Color.Gray; // بازگشت به رنگ خاکستری
            }
        }

        //Line log Enter change action
        private void txtLineLimitation_Enter(object sender, EventArgs e)
        {
            if (txtLineLimitation.Text == "2000")
            {
                txtLineLimitation.Text = "";
                txtLineLimitation.ForeColor = Color.Black; // تغییر رنگ به مشکی برای تایپ کاربر
            }
        }

        // Bug Report Button click action.
        private async void btnBugreport_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;

            _isConnected = false;
            UpdateUiByConnectionState();


            void LogLine(string s)
            {
                if (rtbLog.InvokeRequired)
                {
                    rtbLog.BeginInvoke(new Action(() => LogLine(s)));
                    return;
                }
                rtbLog.AppendText(s + Environment.NewLine);
                rtbLog.ScrollToCaret();
            }

            try
            {
                rtbLog.Clear();
                LogLine("Starting bugreport...");

                // 1) خروجی را در Temp بساز
                var tempDir = Path.GetTempPath();
                var tempFile = Path.Combine(tempDir, $"bugreport_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

                LogLine("Generating file at: " + tempFile);
                LogLine("Please wait...");

                // نکته: برای بعضی دستگاه‌ها بهتر است --progress را هم اضافه کنید (اگر adb شما پشتیبانی کند)
                // var args = $"bugreport --progress \"{tempFile}\"";
                var args = $"-s \"{_connectedSerial}\" bugreport \"{tempFile}\"";

                int exitCode = await AdbRunner.RunStreamingAsync(args, LogLine, timeoutMs: 15 * 60 * 1000);

                LogLine($"adb exit code: {exitCode}");

                if (exitCode != 0)
                {
                    LogLine("Bugreport failed.");
                    return;
                }

                if (!File.Exists(tempFile))
                {
                    LogLine("Bugreport finished but file was not found.");
                    return;
                }

                LogLine("Bugreport created successfully.");

                // 2) از کاربر بپرس کجا ذخیره کند
                using var sfd = new SaveFileDialog
                {
                    Title = "Save bugreport",
                    Filter = "ZIP File (*.zip)|*.zip|All Files (*.*)|*.*",
                    FileName = Path.GetFileName(tempFile),
                    OverwritePrompt = true
                };

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    LogLine("Save canceled by user. File remains in temp: " + tempFile);
                    return;
                }

                File.Copy(tempFile, sfd.FileName, overwrite: true);
                LogLine("Saved to: " + sfd.FileName);

                // اختیاری: فایل temp را پاک کن
                try { File.Delete(tempFile); } catch { /* ignore */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isConnected = true;
                UpdateUiByConnectionState();
            }
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _scrcpyRunner.Dispose();
            base.OnFormClosed(e);
        }
    }
}

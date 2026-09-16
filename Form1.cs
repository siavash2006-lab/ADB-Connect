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
        private LogcatSession? _logSession;
        private string? _lastLogFile;
        private readonly System.Windows.Forms.Timer _logFlushTimer = new();
        private readonly System.Collections.Concurrent.ConcurrentQueue<string> _logQueue = new();
        private const int MaxLogChars = 300_000; // Display character limit (not a byte count). / سقف تعداد کاراکتر نمایش
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
            bool logcatRunning = _logSession is { IsRunning: true };
            bool scrcpyHostOpen = _scrcpyHostForm is { IsDisposed: false };
            comboBox1.Enabled = comboBox1.Items.Count > 0 && !scrcpyRunning && !scrcpyHostOpen && !logcatRunning;
            comboBox2.Enabled = _isConnected && !scrcpyRunning && !scrcpyHostOpen;
            checkBox1.Enabled = _isConnected && !scrcpyRunning && !scrcpyHostOpen;
            button1.Enabled = _isConnected && !scrcpyRunning && !scrcpyHostOpen;
            btnRefreshScrcpyDevices.Enabled = !scrcpyRunning && !scrcpyHostOpen;
            bool busy = _operationCts != null || _closing;
            btnStartLog.Enabled = _isConnected && !busy && !logcatRunning;
            btnStopLog.Enabled = logcatRunning && !_closing;
            btnConnect.Enabled = btnCheckDevice.Enabled = !busy && !logcatRunning && !scrcpyHostOpen;
            btnDisconnect.Enabled = _connectedSerial != null && !busy && !logcatRunning;
            txtIp.Enabled = btnConnect.Enabled;
            checkedListBox1.Enabled = !busy;
            btnCancelOperation.Enabled = _canCancelOperation && busy && !_closing;
            btnCancelOperation.Visible = _canCancelOperation && busy && !_closing;
            if (busy)
            {
                foreach (var control in AllControls(this))
                    if (control is Button && control != btnStopLog && control != btnCancelOperation && control != btnClearLog)
                        control.Enabled = false;
                comboBox1.Enabled = comboBox2.Enabled = checkBox1.Enabled = false;
                ckbAllApp.Enabled = ckbSystemApps.Enabled = ckbUserApps.Enabled = false;
            }
            btnExportLog.Enabled = !busy && _lastLogFile != null;
            UpdateConnectionBadge(busy);
            UpdatePropertyActions(busy);


        }

        private async Task<(int exitCode, string stdout, string stderr)> RunAdbAsync(string args, int timeoutMs = 20000)
        {
            string trimmed = args.TrimStart();
            bool host = trimmed == "devices" || trimmed.StartsWith("devices ") ||
                trimmed == "start-server" || trimmed == "kill-server" ||
                trimmed == "disconnect" || trimmed.StartsWith("disconnect ") || trimmed.StartsWith("connect ");
            if (!host && !trimmed.StartsWith("-s "))
            {
                string serial = _operationSerial ?? _connectedSerial ?? throw new InvalidOperationException("Select a device first.");
                if (serial.Any(char.IsWhiteSpace) || serial.Contains('"')) throw new InvalidOperationException("Invalid device serial.");
                args = $"-s \"{serial}\" {args}";
            }
            var result = await AdbRunner.RunAsync(args, timeoutMs, OperationToken);
            OperationToken.ThrowIfCancellationRequested();
            return result;
        }


        private async Task packageListUpdate()
        {
            
            UpdateUiByConnectionState();
            checkedListBox1.Items.Clear();
            _allPackagesList.Clear();

            var result = await RunDeviceAsync(CommandRules.PackageListArguments(_packages ?? "packages"));

            if (result.exitCode != 0)
            {
                throw new InvalidOperationException("Unable to refresh packages: " + result.stderr);
            }

            // Store the reference package list. / ذخیره پکیج‌ها در لیست مرجع
            _allPackagesList = CommandRules.InstalledPackages(result.stdout);

            // Apply the current search filter to the list. / اعمال فیلتر جست‌وجوی فعلی
            ApplyFilter();

            
            UpdateUiByConnectionState();
        }

        private void ApplyFilter()
        {
            string searchTerm = txbSearchPackages.Text.ToLower().Trim();

            checkedListBox1.BeginUpdate();
            checkedListBox1.Items.Clear();

            // Filter the reference list by search text. / فیلتر کردن لیست مرجع بر اساس متن جست‌وجو
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
            if (_closing || IsDisposed) return;
            if (InvokeRequired) { EnqueueLogLine(message); return; }
            WriteDisplayedLog($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}", color);
        }

        private async Task<string> GetPidByPackageNameAsync(string packageName)
        {
            if (!CommandRules.IsPackageName(packageName)) throw new ArgumentException("Enter a valid package name.");
            var r = await RunDeviceAsync(new[] { "shell", "pidof", "-s", packageName });
            string pid = r.stdout.Trim();
            return r.exitCode == 0 && pid.Length > 0 && pid.All(char.IsAsciiDigit) ? pid : "";
        }


        private bool EnsureConnected()
        {
            if (!_isConnected || string.IsNullOrWhiteSpace(_connectedSerial) ||
        _connectedSerial.Contains("\n") || _connectedSerial.Contains(" "))
            {
                AppDialog.Show("First, connect the device.");
                return false;
            }
            return true;
        }

        private void EnqueueLogLine(string line)
        {
            if (!_closing && _logQueue.Count < 1000) _logQueue.Enqueue(line);
        }

        private void FlushLogToRtb()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 100 && _logQueue.TryDequeue(out var message); i++) sb.AppendLine(message);
            string filter = txtFilter.Text.Trim(); // UI access stays on the UI thread.
            for (int i = 0; i < 500 && _logSession != null && _logSession.TryRead(out var line); i++)
                if (filter.Length == 0 || line!.Contains(filter, StringComparison.OrdinalIgnoreCase)) sb.AppendLine(line);
            if (sb.Length == 0) return;
            WriteDisplayedLog(sb.ToString(), rtbLog.ForeColor);
        }


        public Form1()
        {
            InitializeComponent();
            InitializeOperationControls();
            InitializeIndustrialTheme();
            System.Drawing.Icon? executableIcon =
                System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (executableIcon is not null)
                Icon = executableIcon;

            UpdateUiByConnectionState();
            // checkedListBox1.Visible = false;
            btnUninstall.Enabled = false;
            _logFlushTimer.Interval = 80; // Display update interval in milliseconds. / فاصله به‌روزرسانی نمایش به میلی‌ثانیه
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
            if (_closing || IsDisposed || !IsHandleCreated) return;

            BeginInvoke(new Action(() =>
            {
                label11.Text = status;
                UpdateUiByConnectionState();
            }));
        }

        private void ScrcpyRunner_OutputReceived(string line)
        {
            if (_closing || IsDisposed || !IsHandleCreated) return;
            EnqueueLogLine($"scrcpy: {line}");
        }

        private async void TabControl1_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (tabControl1.SelectedTab == tabPage4 && !_scrcpyRunner.IsRunning)
                await RefreshScrcpyDevicesAsync();
            });
        }

        private static List<string> ParseOnlineDeviceSerials(string? adbDevicesOutput)
        {
            return CommandRules.OnlineSerials(adbDevicesOutput ?? "");
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
                    ?? (_connectedSerial is null ? items.FirstOrDefault() : null);

                if (preferred is not null)
                {
                    comboBox1.SelectedItem = preferred;
                    _connectedSerial = preferred;
                    _isConnected = true;
                }
                else
                {
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
            if (_updatingDeviceSelection || _operationCts != null) return;

            if (comboBox1.SelectedItem is string serial && !string.IsNullOrWhiteSpace(serial))
            {
                if (_connectedSerial != serial) { _allPackagesList.Clear(); ApplyFilter(); }
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
                var result = await AdbRunner.RunAsync("devices", 20000, OperationToken);
                if (result.exitCode != 0)
                {
                    throw new InvalidOperationException("ADB device discovery failed: " + result.stderr);
                }

                var serials = ParseOnlineDeviceSerials(result.stdout);
                SetScrcpyDevices(serials);
                label11.Text = serials.Count == 0
                    ? "No online device"
                    : $"{serials.Count} device(s) ready";
            }
            catch (Exception ex)
            {
                _isConnected = false;
                OperationToken.ThrowIfCancellationRequested();
                label11.Text = "Device check failed";
                throw new InvalidOperationException("Device check failed: " + ex.Message, ex);
            }
            finally
            {
                UpdateUiByConnectionState();
            }
        }

        private async void btnRefreshScrcpyDevices_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            await RefreshScrcpyDevicesAsync();
            });
        }

        private async void btnPairDevice_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
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
                    timeoutMs: 30000, cancellationToken: OperationToken);

                string pairOutput = $"{pairResult.stdout}\n{pairResult.stderr}".Trim();
                if (pairResult.exitCode != 0)
                {
                    AppendLog($"Pairing failed: {pairOutput}", Color.Red);
                    AppDialog.Show(
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
                    timeoutMs: 20000, ct: OperationToken);

                string connectOutput = $"{connectResult.stdout}\n{connectResult.stderr}".Trim();
                bool connected = connectResult.exitCode == 0 &&
                    (connectOutput.Contains("connected to", StringComparison.OrdinalIgnoreCase) ||
                     connectOutput.Contains("already connected", StringComparison.OrdinalIgnoreCase));

                if (!connected)
                {
                    AppendLog($"Pair succeeded, but connection failed: {connectOutput}", Color.OrangeRed);
                    AppDialog.Show(
                        $"Pairing succeeded, but ADB could not connect to {dialog.ConnectionEndpoint}.\n\n{connectOutput}",
                        "Pair Device",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _connectedSerial = dialog.ConnectionEndpoint;
                _allPackagesList.Clear(); ApplyFilter();
                await RefreshScrcpyDevicesAsync();
                if (!_isConnected) throw new InvalidOperationException("Pairing succeeded, but the device is not online/authorized.");

                if (comboBox1.Items.Contains(dialog.ConnectionEndpoint))
                    comboBox1.SelectedItem = dialog.ConnectionEndpoint;

                txtIp.Clear();
                AppendLog($"Connected to {dialog.ConnectionEndpoint}.", Color.Green);
                AppDialog.Show(
                    $"Pairing and connection completed successfully.\n\nPair port: {dialog.PairingPort}\nConnect port: {dialog.ConnectionPort}",
                    "Pair Device",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                OperationToken.ThrowIfCancellationRequested();
                AppendLog($"Pairing error: {ex.Message}", Color.Red);
                AppDialog.Show(ex.Message, "Pair Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckDevice.Enabled = true;
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = true;
                UpdateUiByConnectionState();
            }
            });
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
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected()) return;

            if (_scrcpyHostForm is { IsDisposed: false })
            {
                _scrcpyHostForm.Activate();
                return;
            }

            if (comboBox1.SelectedItem is not string serial || string.IsNullOrWhiteSpace(serial))
            {
                AppDialog.Show("Select an online device first.", "scrcpy");
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
                    expectedDisplaySize = await _scrcpyRunner.GetPrimaryDisplaySizeAsync(serial, cancellationToken: OperationToken);

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
                OperationToken.ThrowIfCancellationRequested();
                label11.Text = "Start failed";
                AppDialog.Show(ex.Message, "scrcpy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateUiByConnectionState();
            }
            });
        }

        // Devices Button action
        private async void btnChekDevices(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            await RefreshScrcpyDevicesAsync();
            });
        }

        // Connect Button action
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            string endpoint = NormalizeEndpoint(txtIp.Text.Trim());
            var result = await RunAdbAsync($"connect {endpoint}");
            if (result.exitCode != 0 || !(result.stdout.Contains("connected to", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException((result.stdout + "\n" + result.stderr).Trim());
            _connectedSerial = endpoint;
            _allPackagesList.Clear(); ApplyFilter();
            await RefreshScrcpyDevicesAsync();
            if (!_isConnected) throw new InvalidOperationException("Connection requested, but the device is not online/authorized.");
            txtIp.Clear();
            AppendLog($"Connected: {endpoint}", Color.Green);
            });
        }

        // Disconnect Button action
        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            try
            {
                await _scrcpyRunner.StopAsync();
                var args = string.IsNullOrWhiteSpace(_connectedSerial) ? "disconnect" : $"disconnect {_connectedSerial}";
                var result = await RunAdbAsync(args, timeoutMs: 20000);

                if (result.exitCode == 0) AppDialog.Show(result.stdout);
                else AppDialog.Show(result.stderr);

                await RefreshScrcpyDevicesAsync();
            }
            catch (Exception ex)
            {
                OperationToken.ThrowIfCancellationRequested();
                AppDialog.Show(ex.Message);
            }
            });
        }


        // Reboot Button action
        private async void btnReboot_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected()) return;
            var result = await RunAdbAsync("reboot ".Trim());
            if (result.exitCode != 0) throw new InvalidOperationException(result.stderr);
            _isConnected = false;
            _rebootRequested = true;
            await StopLogAsync();
            AppendLog("Reboot  requested. Refresh devices after boot completes.", Color.Orange);
            });
        }

        // Install Button action
        private async void btnInstallApk_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected()) return;
            using var dialog = new OpenFileDialog { Title = "Select APK to install", Filter = "APK files (*.apk)|*.apk", CheckFileExists = true };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            pbLoading.Visible = true;
            try
            {
                var r = await RunDeviceAsync(new[] { "install", "-r", dialog.FileName }, 120000);
                if (!CommandRules.Succeeded("install", r.exitCode, r.stdout, r.stderr))
                    throw new InvalidOperationException($"Installation failed.\n{r.stdout}\n{r.stderr}");
                AppendLog("APK installed successfully.", Color.Green);
                await packageListUpdate();
            }
            finally { pbLoading.Visible = false; }
            });
        }


        // UnInstall Button action
        private async void btnUninstall_Click_1(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            await RunPackageOperationAsync("uninstall");
            });
        }

        // Start App Button action
        private async void btnStartApp_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            await RunPackageOperationAsync("start");
            });
        }

        // Stop App Button action
        private async void btnStopApp_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            await RunPackageOperationAsync("stop");
            });
        }


        // Package list Button action
        private async void btnPackages_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            // checkedListBox1.Visible = true;\
            if (!EnsureConnected()) return;

            await packageListUpdate();

            });
        }

        // Version Button action
        private async void btnVersion_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.build.version.release");
        }

        private async void btnRecovery_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected()) return;
            var result = await RunAdbAsync("reboot recovery".Trim());
            if (result.exitCode != 0) throw new InvalidOperationException(result.stderr);
            _isConnected = false;
            _rebootRequested = true;
            await StopLogAsync();
            AppendLog("Reboot recovery requested. Refresh devices after boot completes.", Color.Orange);
            });
        }

        // Bootloader Button action
        private async void btnBootloader_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected()) return;
            var result = await RunAdbAsync("reboot bootloader".Trim());
            if (result.exitCode != 0) throw new InvalidOperationException(result.stderr);
            _isConnected = false;
            _rebootRequested = true;
            await StopLogAsync();
            AppendLog("Reboot bootloader requested. Refresh devices after boot completes.", Color.Orange);
            });
        }

        // Fastboot Button action
        private async void btnFastboot_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected()) return;
            var result = await RunAdbAsync("reboot fastboot".Trim());
            if (result.exitCode != 0) throw new InvalidOperationException(result.stderr);
            _isConnected = false;
            _rebootRequested = true;
            await StopLogAsync();
            AppendLog("Reboot fastboot requested. Refresh devices after boot completes.", Color.Orange);
            });
        }

        // Kernel Check Button action
        private async void btnKernelTest_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell uname -a");
        }

        private async void btnDisplaySize_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop vendor.display-size");
        }

        private async void btnVendorFingerprint_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.vendor.build.fingerprint");
        }

        private async void btnVendorBuildDate_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.vendor.build.date");
        }

        private async void button2_Click_1(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.software.version_id");
        }

        private async void btnBuildId_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.system_ext.build.id");
        }

        private async void btnSkdVersion_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.system.build.version.sdk");
        }

        private async void btnSerialNo_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.serialno");
        }

        private async void btnVendorName_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.product.vendor.name");
        }

        private async void btnVendorModel_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.product.vendor.model");
        }

        private async void btnVendorBrand_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.product.vendor.brand");
        }

        private async void btnManufacturer_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.product.system_ext.manufacturer");
        }

        private async void btnDevice_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.product.system.device");
        }

        private async void btnBoardName_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.product.board");
        }

        private async void btnCpuType_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.product.cpu.abi");
        }

        private async void btnOemKey_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.oem.key1");
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.opengles.version");
        }

        private async void btnValidation_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.nrdp.validation");
        }

        private async void btnHardware_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.hardware");
        }

        private async void btnClientIdBase_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.com.google.clientidbase");
        }

        private async void btnBuildFingerprint_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.build.fingerprint");
        }

        private async void btnVbmeta_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop ro.boot.vbmeta.device_state");
        }

        private async void btnTimeZone_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop persist.sys.timezone");
        }

        private async void btnArm_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop dalvik.vm.isa.arm.variant");
        }

        private async void btnGetEnforce_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getenforce");
        }

        private async void btnGetProps_Click(object sender, EventArgs e)
        {
            await ReadPropertyAsync(sender as Control, "shell getprop");
        }

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
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected() || _logSession is { IsRunning: true }) return;
            string package = txtPackageFilter.Text.Trim();
            string pid = package.Length > 0 ? await GetPidByPackageNameAsync(package) : "";
            if (package.Length > 0 && pid.Length == 0) throw new InvalidOperationException("The requested package is not running.");
            string serial = _operationSerial!;
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ADB Connect", "Logs");
            Directory.CreateDirectory(folder);
            string safeSerial = string.Concat(serial.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            string path = Path.Combine(folder, $"Log_{safeSerial}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.txt");
            var args = new List<string> { "-s", serial, "logcat", "-v", "threadtime" };
            if (pid.Length > 0) args.Add($"--pid={pid}");
            var session = new LogcatSession(ProcessExecutor.Create(AdbRunner.AdbPath, args), path);
            _logSession = session;
            _lastLogFile = path;
            rtbLog.Clear();
            AppendLog($"Raw log: {path}", Color.Green);
            _ = ObserveLogAsync(session);
            });
        }

        //Stop log button change action
        private async void btnStopLog_Click(object sender, EventArgs e)
        {
            try { await StopLogAsync(); }
            catch (Exception ex) { AppendLog(ex.Message, Color.Red); }
        }

        //Export log button change action
        private async void btnExportLog_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (_lastLogFile is null || !File.Exists(_lastLogFile))
                throw new InvalidOperationException("No raw Logcat file is available. Start Log first.");
            using var dialog = new SaveFileDialog { Title = "Export raw log (independent of display filters)", Filter = "Text files (*.txt)|*.txt", FileName = Path.GetFileName(_lastLogFile) };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (Path.GetFullPath(dialog.FileName).Equals(Path.GetFullPath(_lastLogFile), StringComparison.OrdinalIgnoreCase)) return;
            // Copy a fixed-length snapshot; a live writer can keep appending safely.
            await using var source = new FileStream(_lastLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long remaining = source.Length;
            string temporary = dialog.FileName + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await using (var target = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                {
                    byte[] buffer = new byte[65536];
                    while (remaining > 0)
                    {
                        int count = await source.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)), OperationToken);
                        if (count == 0) throw new IOException("The log snapshot ended unexpectedly.");
                        await target.WriteAsync(buffer.AsMemory(0, count), OperationToken);
                        remaining -= count;
                    }
                }
                File.Move(temporary, dialog.FileName, true);
                AppendLog("Raw log snapshot exported.", Color.Green);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            });
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
            _logQueue.Clear();
            _logSession?.ClearDisplay();
            rtbLog.Clear();
        }

        //Line log leave change action
        private void txtLineLimitation_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLineLimitation.Text))
            {
                txtLineLimitation.Text = "2000";
                txtLineLimitation.ForeColor = Color.Gray; // Restore placeholder color. / بازگشت به رنگ خاکستری
            }
        }

        //Line log Enter change action
        private void txtLineLimitation_Enter(object sender, EventArgs e)
        {
            if (txtLineLimitation.Text == "2000")
            {
                txtLineLimitation.Text = "";
                txtLineLimitation.ForeColor = Color.Black; // Use the input color. / تغییر رنگ به مشکی برای تایپ کاربر
            }
        }

        // Bug Report Button click action.
        private async void btnBugreport_Click(object sender, EventArgs e)
        {
            await RunUiOperationAsync(async () =>
            {
            if (!EnsureConnected()) return;

            
            UpdateUiByConnectionState();


            void LogLine(string line) => EnqueueLogLine(line);

            try
            {
                rtbLog.Clear();
                LogLine("Starting bugreport...");

                // Generate the report in a temporary file. / خروجی را در Temp بساز
                var tempDir = Path.GetTempPath();
                var tempFile = Path.Combine(tempDir, $"bugreport_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.zip");

                LogLine("Generating file at: " + tempFile);
                LogLine("Please wait...");

                // Add --progress only if the ADB version supports it. / فقط در صورت پشتیبانی ADB
                // var args = $"bugreport --progress \"{tempFile}\"";
                var args = $"-s \"{_connectedSerial}\" bugreport \"{tempFile}\"";

                int exitCode;
                _canCancelOperation = true;
                UpdateUiByConnectionState();
                try
                {
                    exitCode = await AdbRunner.RunStreamingAsync(args, LogLine, timeoutMs: 15 * 60 * 1000, cancellationToken: OperationToken);
                }
                finally
                {
                    _canCancelOperation = false;
                    UpdateUiByConnectionState();
                }
                OperationToken.ThrowIfCancellationRequested();

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

                // Ask where to save the completed report. / از کاربر بپرس کجا ذخیره کند
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

                // Remove the temporary copy after saving. / فایل موقت را پس از ذخیره پاک کن
                try { File.Delete(tempFile); } catch { /* ignore */ }
            }
            catch (Exception ex)
            {
                OperationToken.ThrowIfCancellationRequested();
                AppDialog.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                
                UpdateUiByConnectionState();
            }
            });
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void linkSpadra_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://spadra.ir",
                    UseShellExecute = true
                });

                linkSpadra.LinkVisited = true;
            }
            catch (Exception ex)
            {
                AppDialog.Show(
                    $"Unable to open https://spadra.ir.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Open Website",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _logFlushTimer.Stop();
            _logFlushTimer.Dispose();
            _scrcpyRunner.Dispose();
            base.OnFormClosed(e);
        }
    }
}

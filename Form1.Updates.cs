using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;

namespace ADB_Connect;

public partial class Form1
{
    private readonly UpdateService _updateService = new();
    private readonly CancellationTokenSource _updateLifetime = new();
    private readonly UpdateIconButton _updateIcon = new();
    private ToolTip? _updateTip;
    private void SetUpdateStatus(string status) { _updateIcon.AccessibleName = status; _updateTip?.SetToolTip(_updateIcon, status); _updateIcon.HasUpdate = _availableUpdate != null; }
    private AvailableUpdate? _availableUpdate;
    private CancellationTokenSource? _downloadCancellation;
    private bool _checkingUpdate;
    private bool _handlingUpdate;
    private string UpdateArchitecture => Environment.Is64BitProcess ? "x64" : "x86";
    // Inno Setup's uninstaller stays alongside installed executables; portable packages omit it.
    private bool IsInstalledCopy => File.Exists(Path.Combine(AppContext.BaseDirectory, "unins000.exe")) &&
        File.Exists(Path.Combine(AppContext.BaseDirectory, "unins000.dat"));

    private void InitializeUpdates(Control header)
    {
        _updateIcon.SetBounds(528, 22, 36, 32);
        _updateTip = new ToolTip(components!);
        SetUpdateStatus("Check for updates");
        _updateIcon.AccessibleName = "Check for software updates";
        header.Controls.Add(_updateIcon);
        _updateIcon.BringToFront();
        _updateIcon.Click += async (_, _) =>
        {
            if (_downloadCancellation != null) { _downloadCancellation.Cancel(); return; }
            if (_handlingUpdate) return;
            if (_availableUpdate == null) await CheckForUpdatesAsync(true);
            else await OfferUpdateAsync();
        };
        var timer = new System.Windows.Forms.Timer(components!) { Interval = 15 * 60 * 1000 };
        timer.Tick += async (_, _) => await CheckForUpdatesAsync(false);
        Shown += async (_, _) => { timer.Start(); await CheckForUpdatesAsync(false); };
        Disposed += (_, _) => { _updateLifetime.Cancel(); _updateLifetime.Dispose(); _updateService.Dispose(); };
    }

    private async Task CheckForUpdatesAsync(bool manual)
    {
        if (_checkingUpdate || _handlingUpdate || _closing || _updateLifetime.IsCancellationRequested) return;
        _checkingUpdate = true;
        SetUpdateStatus("Checking for updates…");
        try
        {
            var current = typeof(Form1).Assembly.GetName().Version ?? new Version(1,6,7);
            var update = await _updateService.CheckAsync(current, UpdateArchitecture, IsInstalledCopy, _updateLifetime.Token);
            if (_closing || IsDisposed) return;
            _availableUpdate = update;
            SetUpdateStatus(update == null ? "Up to date · Check again" : $"Version {update.Version} available");
            if (manual && update != null) await OfferUpdateAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {
            if (!_closing && !IsDisposed)
            {
                SetUpdateStatus(_availableUpdate == null ? "Updates unavailable · Retry" : $"Version {_availableUpdate.Version} available");
                if (manual) AppDialog.Show(this, "Unable to check GitHub. Check your connection and try again.", "Updates");
            }
        }
        finally { _checkingUpdate = false; }
    }

    private bool UpdateCanProceed() => !_closing && _operationCts == null &&
        _logSession is not { IsRunning: true } && !_scrcpyRunner.IsRunning &&
        _scrcpyHostForm is not { IsDisposed: false };

    private async Task OfferUpdateAsync()
    {
        var update = _availableUpdate;
        if (update == null || _handlingUpdate) return;
        if (!UpdateCanProceed()) { AppDialog.Show(this, "Finish the current operation and stop logging or TV Control before updating.", "Updates"); return; }
        _handlingUpdate = true;
        try
        {
            if (update.Asset == null)
            {
                if (AppDialog.Show(this, "A newer version is available, but a verifiable download for this edition is missing. Open its GitHub release page?",
                    "Update available", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Process.Start(new ProcessStartInfo(update.PageUrl) { UseShellExecute = true });
                return;
            }
            string action = IsInstalledCopy ? "Download the installer? Installation will require a second confirmation." :
                "Download the portable update? Replacing this copy will require a second confirmation.";
            if (AppDialog.Show(this, $"ADB Connect {update.Version} is available.\n\n{action}",
                "Update available", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _downloadCancellation = CancellationTokenSource.CreateLinkedTokenSource(_updateLifetime.Token);
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ADB Connect", "Updates", update.Tag + "-" + UpdateArchitecture + "-" + Guid.NewGuid().ToString("N"));
            var progress = new Progress<int>(percent =>
            {
                if (!IsDisposed && !_closing && _downloadCancellation != null) SetUpdateStatus($"Downloading {percent}% · Cancel");
            });
            string file;
            try { file = await _updateService.DownloadAsync(update.Asset, directory, progress, _downloadCancellation.Token); }
            finally { _downloadCancellation.Dispose(); _downloadCancellation = null; }
            if (_closing || IsDisposed) return;
            SetUpdateStatus($"Version {update.Version} downloaded");
            if (AppDialog.Show(this,
                "The verified update is ready. Close ADB Connect, replace this version in its current folder, and reopen it?\n\n" +
                (IsInstalledCopy ? "Windows may ask for installation permission." : "Existing application files will be backed up before replacement."),
                "Install update", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (!UpdateCanProceed()) { AppDialog.Show(this, "Stop active operations before applying the update.", "Updates"); return; }
            Enabled = false;
            SetUpdateStatus("Preparing to restart and update…");
            await UpdateHandoff.StartAsync(file, !file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase), update.Asset.Sha256, directory);
            Close();
        }
        catch (OperationCanceledException) { if (!_closing && !IsDisposed) SetUpdateStatus("Download cancelled · Retry"); }
        catch (Exception ex)
        {
            if (!_closing && !IsDisposed)
            {
                SetUpdateStatus("Update failed · Retry");
                AppDialog.Show(this, ex.Message, "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally { _handlingUpdate = false; if (!IsDisposed && !_closing) Enabled = true; }
    }
}

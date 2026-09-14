# Initial laboratory upgrade assessment for ADB Connect

[فارسی](LAB-UPGRADE-PLAN-FA.md)

**Historical assessment, 2026-09-13, local version 1.6.4.** The version sequence below was superseded by RELEASE-ROADMAP-EN.md: fixes, Windows recording, file manager, multi-device capture, then sessions/tests. Findings describe the original code, not all remaining problems in 1.6.5.

## Scope and evidence

The project uses C#/WinForms, net10.0-windows and win-x64/win-x86 outputs. The visible shared conversation concerned EXE publishing, installers, retaining platform-tools and an x86 robocopy error. Uploaded images were represented only by placeholders and were not evidence for conclusions.

The main form, ADB execution, Pairing, scrcpy, screenshots, project settings and release scripts were reviewed. `dotnet build "ADB Connect.csproj" --no-restore -v:minimal` succeeded with no warnings/errors after an initial resource-output permission failure and an authorized retry. This was incremental Debug compilation, not clean release validation. At that point no real device had been exercised, no installer had been built/installed and no automated test project was found.

The local directory was not a Git repository. README identified a public repository, but source equivalence was not confirmed. The planning assumption was an Android/Android TV firmware/app/debugging lab; device counts, boards and priority tests were unknown.

## Existing features

Network connection, independent pairing/connection ports, ADB device discovery/selection, hardware/software properties, APK installation, package listing/start/stop/uninstall, filtered Logcat, Bugreport, getprop export, scrcpy video/audio control and window screenshots. Preserve scrcpy's fallback for severely reduced stream dimensions and retest on real models. The Fastboot button only requests `adb reboot fastboot`; it is not a flashing/management module.

## Original findings

| Priority | Code evidence | Laboratory impact | Proposed correction |
| --- | --- | --- | --- |
| P0 | Form1 uses `_isConnected` to disable controls while busy and restores true without device verification | False connection state after reboot, network loss or errors | Separate operation/connection state and verify readiness |
| P0 | Uninstall removes all selected entries and reports success even on failure; Start/Stop also report unconditional final messages | Invalid test outcomes | Per-app results and partial success; refresh inventory |
| P0 | Multi-app loops use the mutable global device selection | Target may change between steps | Capture a fixed serial per operation; never substitute another device |
| P0 | Logcat callback reads UI filters and synchronously invokes UI per line; declared queue is unused there | Cross-thread access and dense-log stalls | Snapshot filters, bound display queue and independently write to disk |
| P0 | Export saves only remaining RichTextBox content after old lines are removed | Lost long-test evidence | Continuous raw files with rotation; filter display or explicitly selected export |
| P0 | Form shutdown explicitly disposes only scrcpy, not Logcat | Orphan processes/callbacks | Cancel work and terminate/dispose owned processes |
| P1 | Global ADB lock includes long Bugreport operations; Logcat takes a separate path | Short commands and future multiple devices wait | Per-device mutation queues, independent streams, bounded concurrency |
| P1 | `packageListUpdate` is async void and does not restore state on errors | Difficult error handling and disabled UI | Task and try/finally with separate operation state |
| P1 | AdbProgressRunner ignores WaitForExit timeout result, lacks exit/cancel handling and is effectively unused | Duplicate incomplete execution path | Remove or consolidate |
| P1 | Screenshots use CopyFromScreen although a separate ADB screenshot helper exists | Depends on window size and coverage | Originally propose clearly labeled device/window capture with method/dimensions; later roadmap retains Windows capture |
| P1 | Installer discovery only checks Inno 6; iss lacks SignTool and has SignedUninstaller=no | Documentation and 'Signed' message do not prove signatures | Version discovery, aligned documentation, actual output verification |
| P1 | Form1 has roughly 1,800 lines mixing UI, device logic and operations | Changes risk regressions | Gradually extract models/services while retaining WinForms |

## Proposed product model

A session combines a device, firmware, application under test, scenario, results and evidence. Store an independent ID, optional operator, start/end time, notes and final state. Proposed pages: devices, overview, apps, logs/diagnostics, TV control, scenarios and reports. Keep device selection visible across pages; turn properties into a searchable/copyable/exportable comparison table and show routine information inline.

## Original stage 1 — Stabilize (proposed 1.7, superseded)

Establish the source baseline and Git tracking after reconciling the local/public source. Separate ConnectionState/OperationState and fix serials. Consolidate structured command arguments, timeout/cancel and exit/stdout/stderr/duration/status; shell arguments still need validation/quoting. Correct app outcomes, duplicate execution and partial success. Repair Logcat/full-file capture, process termination and separate view/device clearing. Check dependencies/versions at startup and use consistent ADB for scrcpy. Align installer/version/documentation and preserve publish subfolders.

Acceptance: manufactured failures never pass; mid-operation disconnection never changes target; one hour of dense logs retains full evidence without freezing; shutdown leaves no owned processes; test clean x64/x86 installation and upgrades with all dependencies.

## Original stage 2 — Daily laboratory tools (proposed 1.8, superseded)

Saved devices with laboratory name, model/board, stable identity, current endpoint, transport and last state; IP alone is not identity. Overview from supported properties: Android/SDK, fingerprints, CPU/ABI, RAM/storage/display, with Unsupported/Unavailable for missing data. Collect session properties, app versions, logs, Bugreport, images and notes together. Show versionName/versionCode, reinstall/batch results, clearly explained data clearing, and split APK support if actually needed. Add TV navigation/OK/Home/Back/Volume/text, captures/recording directly into a session. Export printable HTML, comparison CSV, primary JSON and evidence ZIP, including time, serial, firmware and tool versions.

Acceptance: generate and reopen a complete single-device session without manual file shuffling; confirm identity when reconnecting or changing IP.

## Original stage 3 — Repeatable tests (proposed 1.9, superseded)

Versioned scenario steps with preconditions, action, wait, success condition, timeout, failure policy, repetition and evidence. Automatically retry only operations safe to repeat. Initial scenarios: firmware smoke checks; reboot/reconnect/boot-completion loops; repeated app launch/stop with available crash/ANR/resource evidence; comparison before/after firmware upgrades without implicitly flashing firmware; and network tests to a specified destination that distinguish ADB-path loss from device failure.

Use Passed/Failed/Error/Skipped/Canceled; missing capability/access must not mean Pass. Support manual results. Acceptance: 20 reference cycles with per-cycle evidence, preserved partial results on cancellation, and no automatic repeat of uncertain mutations after a crash.

## Original stage 4 — Concurrency and specialist tests (proposed 2.0, superseded)

Run selected-device groups with bounded concurrency and separate files/results. Compare models, firmware, boot time, failures and available metrics. Separate shared commands and vendor-specific profiles. Define measurement and access before AV, HDMI/CEC, Bluetooth, Wi-Fi or long-play tests: some require a device app, vendor API or external equipment; ADB alone cannot verify physical AV quality. Treat flashing, full Fastboot, power control and measurement hardware as separate modules.

Acceptance: the same scenario on at least two devices; loss of one does not stop the other; commands/evidence never cross identities. Set final scale after learning actual device count.

## Architecture and validation

Retain WinForms and extract gradually. Domain: Device, DeviceCapabilities, LabSession, TestPlan, TestStep, TestResult, Artifact. Services: device, app, Logcat, diagnostics, screen, test runner and reports. Infrastructure: process/ADB/scrcpy and local storage. UI calls services; test conclusions do not live in event handlers.

SQLite was proposed for metadata, with large logs/videos on disk; choose library/version when implemented. Store data in a writable user location or selected laboratory folder, not Program Files.

Test parsers, state, outcome classification and scenario rules using real-model samples. Use controlled child processes for timeout, abrupt exit, large output, cancel and termination. Exercise USB/TCP/Pairing, two devices, network loss, reboot, simultaneous Logcat/scrcpy and fallback hardware. Check architecture/dependencies, SDK-free installation, preserved upgrade data and honest signature messages. Each stage delivers usable documented output with passed acceptance criteria.

## Inputs still needed at assessment time

Three recurring tasks, models/chipsets/Android versions, concurrent device count, transports, root/userdebug availability, desired report example and external equipment. The original provisional priority was stabilization followed by sessions/reports/raw logging; the newer roadmap explicitly replaces that order. Reliable scheduling requires these inputs and reference hardware tests.

Sources: [prior conversation](https://chatgpt.com/share/6aa67450-b05c-83eb-a360-7daf0c82b178), [ADB](https://developer.android.com/tools/adb), [Logcat](https://developer.android.com/tools/logcat), [scrcpy](https://github.com/Genymobile/scrcpy).

# ADB Connect 1.6.5 testing guide

[فارسی](TEST-1.6.5-FA.md)

Automated checks do not replace testing on real laboratory devices. Screenshots still capture the visible Windows video area. Recording and file management are not included in this release. Final 1.6.5 packages include both UI fixes; Cancel operation is visible only while a Bugreport is being generated.

## Running the application

Portable outputs are in `publish/v1.6.5/x64` and `publish/v1.6.5/x86`. Run `ADB Connect.exe` from its folder, keeping `platform-tools` and `scrcpy` beside it. Start with x64 on 64-bit Windows. The current packages are unsigned.

## Device checks

1. Connect using an IP address or Pairing; verify the selected device and Android version. Refresh lists ADB devices, including USB devices.
2. Install a disposable test APK, refresh Apps, then start and stop it. Each reported result must match the actual device state.
3. Uninstall only the test app. Uninstallation still targets user 0. The package list is read again afterward. Do not use system applications to manufacture failures; simulated failures are covered by automated tests.
4. Start Log, apply a Keyword and a display line limit, then Export Log. The exported raw file must be independent of the keyword and removal of old displayed lines. A Package filter limits ADB capture to that application's current PID; restart logging after the app restarts to select its new PID.
5. Clear View clears only the display. Clear Device... in the Log tab asks for confirmation before clearing Android's buffer; saved files remain intact.
6. Stop and restart logging. Each start must create a new file, and an intentional stop must not produce a false failure.
7. During Bugreport generation, use Cancel operation. Cancellation does not undo changes already made on the device. The button must disappear before result or save dialogs are displayed and must not appear for Properties or Uninstall.
8. Disconnect the test device and issue a command or Refresh. The application must not silently select another device as the operation's target. Reconnect the same device and Refresh.
9. When no unsaved work is at risk, test a normal Reboot. The message means the request was accepted, not that startup has completed. Refresh after boot. Recovery, Bootloader and Fastboot are not required for this initial check.
10. Test TV Control and Screenshot together with Logcat, then close the application while logging. The owned Logcat process must stop; the shared ADB server is intentionally not stopped with kill-server.

## Logs and issue reports

Raw files are stored under `%LOCALAPPDATA%\ADB Connect\Logs`; the path is displayed when logging begins. Old files are not automatically deleted, so allow enough disk space for long runs. Display lines may be skipped if the display cannot keep up; raw file storage is independent. If writing fails, recording stops and reports an error.

When reporting a problem, include application architecture, device model and Android version, connection type, reproduction steps, expected result, error text and a screenshot if useful. Review logs for personal information before sharing them.

## Developer checks

- `dotnet run --project tests/RegressionTests.csproj` covers package outcomes, Unicode arguments, large stdout/stderr, timeout, cancellation, process termination, raw logging independent of display limits, and natural/intentional termination. No Android device is used.
- `dotnet run --project tests/WinForms/LogViewTests.csproj` checks 60,000 displayed lines, trimming, read-only restoration, selection handling and bounded undo history.
- Build, Publish and installer compilation check packaging; clean Windows installation, native x86 execution and long hardware tests require separate validation.
- Suggested stability check: one hour of real Logcat while using the UI and Screenshot.

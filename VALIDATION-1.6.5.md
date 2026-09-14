# 1.6.5 validation record

Date: 2026-09-13. Environment: Windows build 26100, .NET SDK 10.0.301, Inno Setup 7.1.0.

## Completed

- Debug compilation succeeded with zero warnings/errors during implementation.
- 18 local regression assertions passed using a controlled child process, without issuing commands to Android devices. Coverage: package result classification, exact online-state parsing, shell metacharacter rejection in package names, Unicode arguments, concurrent large stdout/stderr, timeout/termination, cancellation, full raw logging despite display overflow, and natural/intentional Logcat termination.
- Raw logging test persisted all 30,000 generated stdout/stderr lines while display buffering remained limited to 4,000.
- Self-contained single-file application publication for win-x64 and win-x86 succeeded; required ADB/scrcpy/server/license files and executable version 1.6.5.0 were checked by Publish-Application.ps1.
- Published ADB reports Platform-Tools 36.0.0. Published x64 and x86 scrcpy binaries both successfully report version 4.0 on this Windows host.
- PowerShell release scripts parsed without syntax errors.
- Inno Setup successfully compiled unsigned local x64 and x86 installers. Signature status is reported as NotSigned, not claimed as signed.

## Still requires laboratory validation

- Real IP/USB/Pairing connections, APK operations, reboot and connection loss.
- Live WinForms usability/layout at the laboratory's DPI and display settings.
- Screenshot and automatic scrcpy compatibility fallback on actual TV models.
- One-hour real Logcat run, cancellation/shutdown while interacting with hardware.
- Clean-machine installation and upgrade; native 32-bit Windows execution.
- Trusted signing, if a public signed release is needed.

Use TEST-1.6.5-FA.md for the operator checklist. A successful build is not a claim that these hardware/installation checks have passed. No device was rebooted, cleared, installed to or uninstalled from during this implementation.

## Final source preparation, 2026-09-14

The final 1.6.5 source incorporates fix1 (compact UI, batched scrcpy messages and safe RichTextBox writes) and fix2 (Cancel operation only during actual Bugreport generation). The user reported that the UI/log fix worked. The separate WinForms test passed with 60,000 lines; the original 18 regression assertions also passed. This user feedback does not establish all hardware/installation checks listed above.

English document counterparts and bilingual comments were added for publication. Final packages are rebuilt from this source, without the preview informational-version suffixes.

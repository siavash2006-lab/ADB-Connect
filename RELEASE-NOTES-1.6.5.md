# ADB Connect 1.6.5

Maintenance release for Windows, including the UI and log-display fixes tested during the 1.6.5 preview.

## Changes

- Correct per-application install, uninstall, launch and stop results; refresh the package list after changes.
- Keep operations bound to the selected device and revalidate connection state.
- Save raw UTF-8 Logcat files independently of display filtering and line limits.
- Separate Clear View and confirmed clearing of the device log buffer.
- Improve shutdown, timeout and cancellation handling.
- Fix read-only log display updates and batch scrcpy messages; retain the compact window layout.
- Show Cancel operation only during Bugreport generation, never behind Properties or Uninstall result dialogs.
- Include English counterparts for Persian project documentation.

## Downloads

- `ADB-Connect-1.6.5-x64-Setup.exe`: installer for 64-bit Windows.
- `ADB-Connect-1.6.5-x86-Setup.exe`: x86 installer.
- `ADB-Connect-1.6.5-x64-Portable.zip` / `ADB-Connect-1.6.5-x86-Portable.zip`: extract the entire ZIP and run ADB Connect.exe; keep platform-tools and scrcpy beside it.
- `ADB-Connect-1.6.5-Source.zip`: application source, tests, installer definitions and bilingual documentation. Download third-party runtime dependencies separately to build/run from source.
- `ffmpeg-8.1.1.tar.xz` and `ffmpeg-7.1.1.tar.xz`: corresponding unmodified FFmpeg source archives referenced by the bundled runtimes.
- `SHA256SUMS.txt`: checksums for the attached files.

## Notes

These Windows packages are unsigned; Windows may display Unknown Publisher. Screenshots continue to capture the visible Windows video area. Screen recording, file management, multi-device capture and laboratory sessions are planned for later versions.

Automated checks cover process execution, raw logging and RichTextBox updates. The user reported improvement after the UI/log fix. Comprehensive hardware, native 32-bit Windows and clean installation/upgrade testing are not claimed; see the validation record and device test guide in the repository.

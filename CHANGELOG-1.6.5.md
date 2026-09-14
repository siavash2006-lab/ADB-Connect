# ADB Connect 1.6.5

- Report per-package install/uninstall/launch/stop results using exit codes and command-specific output; refresh package inventory after changes.
- Separate operation state from connection state, keep the selected serial stable during operations, and revalidate the selected device afterward.
- Add operation cancellation and coordinated application shutdown. Reboot success means the request was accepted, not that boot has completed.
- Archive Logcat stdout/stderr to unique UTF-8 files under LocalAppData. Keyword filtering and the bounded display do not truncate the archive. Export produces a snapshot of the raw file.
- Separate Clear View from confirmed clearing of the Android log buffer. Stop and finalize owned Logcat processes at shutdown.
- Use the bundled platform-tools ADB for scrcpy, including display discovery and compatibility fallback.
- Remove the unused progress runner. Add dependency-free regression checks.
- Discover Inno Setup 6/7, report actual signature status, and make signed installer/uninstaller builds explicit.
- Publish into version-specific folders to preserve previous outputs.
- Include the final compact UI, batched scrcpy log display and safe read-only RichTextBox updates from fix1.
- Show Cancel operation only during actual Bugreport generation (fix2), not behind result dialogs.
- Provide English counterparts to Persian documentation and bilingual source comments.

Windows-based Screenshot behavior is preserved. Screen recording, file management, concurrent device capture and lab sessions are scheduled for later versions.

Hardware validation and installation/upgrade testing are required before treating this as a validated laboratory release. Local builds are unsigned unless explicitly signed.

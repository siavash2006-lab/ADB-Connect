# ADB Connect 1.6.7

- Two-step wireless pairing: IP and connection port first, pairing port and code second.
- GitHub update checking at startup and every 15 minutes, with a notification badge beside the theme toggle.
- Verified downloads and confirmation before closing, updating in the same folder and restarting.
- Portable updates back up changed files and roll back applied changes on replacement failure; unrelated files are preserved. Installed copies use the installer with the existing destination.
- Compact plain-text messages, bordered tabs in both themes, corrected device-selector hover painting and themed scrollbars.

Downloads: choose x64 for 64-bit Windows or x86 for 32-bit Windows. Setup.exe is the installer; Portable.zip must be extracted completely before use. Source.zip contains application source and documentation; third-party runtime dependencies are obtained separately as described in README.md.

Windows packages are unsigned. Real-device pairing and elevated installer upgrades still require testing; see VALIDATION-1.6.7.md. Existing 1.6.6 users must install this first updater-enabled release manually.

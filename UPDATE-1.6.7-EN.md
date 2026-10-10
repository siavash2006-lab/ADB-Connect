# ADB Connect 1.6.7 update behavior

Local release packages are prepared; publication to GitHub is a separate step.

## Interface
- Pair Device uses two steps: IP/connection port, then pairing port/six-digit code.
- Short messages use compact labels, with scrolling and right-click Copy for long diagnostics.
- The update icon sits beside the theme toggle. An orange badge means a newer stable release is available. Hover for status; click to check/download or cancel an active download.
- Unselected tabs have visible borders in both themes.
- The device selector uses a custom closed face without native hover painting. Native dropdown/keyboard selection is retained.
- Native scrollbars follow the selected theme.

## Updating in place
The app checks GitHub at startup and every 15 minutes. Drafts/prereleases and equal or older numeric versions are ignored. Network failures only show a dialog for manual checks.

Downloads require confirmation and are verified against the GitHub asset size and SHA-256 digest. A second confirmation authorizes closing, replacing and reopening the application. The helper starts from the update workspace, signals readiness, then waits up to 60 seconds for the original process to exit. It rechecks the downloaded hash before applying anything.

- Portable: replace changed package files in the current application folder; skip identical files (including locked unchanged dependencies). Back up replaced files first. Preserve unrelated files. On a replacement failure, restore applied changes and remove newly added files; report any recovery failure and the backup location.
- Installed: run the downloaded Inno Setup installer with /SILENT, /NORESTART and the existing installation directory. Windows may request elevation. Restart only after a successful installer exit. Portable rollback does not apply to installer-managed changes.
- Successful updates reopen ADB Connect from the same folder. Theme settings remain under LocalAppData/ADB Connect.
- Backups and update-result.txt remain under LocalAppData/ADB Connect/Updates. Do not remove them until the new version has been checked.
- A portable destination must be writable. Changed files locked by another process may prevent replacement; unchanged files are left alone.

Assets must use ADB-Connect-VERSION-x64-Setup.exe or -x86-Setup.exe, or ADB-Connect-VERSION-x64-Portable.zip or -x86-Portable.zip. ZIP contents must have ADB Connect.exe at their root. Missing matching assets/digests offer the GitHub release page instead. Installed copies are identified by both unins000.exe and unins000.dat beside the application.

## Builds and validation
Use publish/v1.6.7/x64 or x86. Release packages are in release/v1.6.7. Temporary preview and update-test outputs were removed. See VALIDATION-1.6.7.md for completed checks and remaining validation.

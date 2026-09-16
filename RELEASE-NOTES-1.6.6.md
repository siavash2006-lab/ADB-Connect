# ADB Connect 1.6.6

[فارسی](https://github.com/siavash2006-lab/ADB-Connect/blob/v1.6.6/RELEASE-NOTES-1.6.6-FA.md)

Maintenance and interface update following 1.6.5.

## Changes
- Compact, aligned controls with muted tab and device-selection surfaces.
- One sun/moon button toggles light and dark themes. The initial theme follows Windows, and manual choices are saved.
- Consistent themes for pairing, TV controls, results, errors and confirmations.
- Property values, progress and errors appear inside the Properties tab. Copy and Save are available; Read All Properties no longer requires saving first.
- Package lists explicitly target Android user 0, matching uninstall. Successful uninstalls are verified against that user's installed packages.
- English and Persian release documentation.

## Downloads
- x64-Setup.exe / x86-Setup.exe: installers.
- x64-Portable.zip / x86-Portable.zip: extract the entire archive and run ADB Connect.exe.
- Source.zip: application source, tests and build definitions; third-party runtime binaries are obtained separately.
- ffmpeg-8.1.1.tar.xz / ffmpeg-7.1.1.tar.xz: corresponding source archives for bundled runtimes.
- SHA256SUMS.txt: file integrity checks.

## Notes
Windows executables are unsigned. Windows may display Unknown Publisher.
Removal is scoped to Android user 0; it does not remove other users' apps or preloaded APKs from system partitions.
Native Windows file pickers retain their system appearance. Installation and screenshot commands are unchanged.
The UI was reviewed during previews. Automated checks and rendering do not replace clean-install, native 32-bit Windows, multi-monitor DPI or physical-device validation.

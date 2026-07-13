# Third-Party Notices

ADB Connect interoperates with third-party software. Those components remain
the property of their respective copyright holders and are governed by their
own license terms.

This source repository does not contain Android SDK Platform-Tools binaries,
scrcpy binaries, .NET runtime binaries, compiled installers or other third-party
binary distributions.

## scrcpy

- Project: scrcpy
- Primary version used by ADB Connect 1.6.3: 4.0
- Compatibility version used by ADB Connect 1.6.3: 3.3.4
- Copyright holders: Genymobile, Romain Vimont and contributors
- Official source: <https://github.com/Genymobile/scrcpy>
- Version 4.0 release: <https://github.com/Genymobile/scrcpy/releases/tag/v4.0>
- Version 3.3.4 release: <https://github.com/Genymobile/scrcpy/releases/tag/v3.3.4>
- License: Apache License 2.0
- License text: [LICENSES/scrcpy-Apache-2.0.txt](LICENSES/scrcpy-Apache-2.0.txt)

ADB Connect launches scrcpy as an external process and hosts its native window
inside a Windows Forms control. The ADB Connect source repository does not
contain or redistribute scrcpy executables.

Official scrcpy Windows archives may contain additional third-party components,
including FFmpeg, SDL, libusb and related runtime libraries. Anyone who creates
or distributes a binary package containing scrcpy must preserve all license and
notice files supplied with the selected official scrcpy archive and comply with
the terms applicable to each included component.

## Android SDK Platform-Tools / Android Debug Bridge

- Component: Android Debug Bridge (`adb`)
- Provider: Google and the Android Open Source Project contributors
- Official download: <https://developer.android.com/tools/releases/platform-tools>
- ADB documentation: <https://developer.android.com/tools/adb>
- Android SDK terms: <https://developer.android.com/studio/terms>

ADB Connect invokes `adb` as an external command-line tool. Android SDK
Platform-Tools binaries are intentionally excluded from this public source
repository. Users must obtain them separately from the official source and are
responsible for accepting and complying with the applicable terms.

## Microsoft .NET

- Component: Microsoft .NET Runtime and libraries
- Target framework used by ADB Connect 1.6.3: .NET 10 for Windows
- Official source: <https://github.com/dotnet/runtime>
- License: MIT License
- License text: [LICENSES/dotnet-MIT.txt](LICENSES/dotnet-MIT.txt)
- Official third-party notices:
  <https://github.com/dotnet/runtime/blob/main/THIRD-PARTY-NOTICES.TXT>

This source repository targets .NET but does not contain a redistributed .NET
runtime. Anyone who publishes a self-contained binary should include the
license and third-party notice files corresponding to the exact .NET runtime
version used for that build.

## Project artwork and assets

The ADB Connect icon and project-specific graphical assets are provided as part
of the ADB Connect project under the project's Apache License 2.0, provided that
the contributor uploading them owns the assets or has the right to license
them. Third-party trademarks and logos remain the property of their respective
owners.

## No endorsement

ADB Connect is an independent project. References to Android, ADB, scrcpy, .NET,
Google, Genymobile and Microsoft are made solely to identify compatible tools
and technologies. No affiliation, sponsorship or endorsement is claimed.

# Third-Party Notices

ADB Connect interoperates with or may be distributed alongside third-party
software components. These components remain the property of their respective
copyright holders and are provided under their own license terms.

## scrcpy

- Project: scrcpy
- Primary version: 4.0
- Compatibility version: 3.2
- Developer: Genymobile / Romain Vimont and contributors
- Source: https://github.com/Genymobile/scrcpy
- Releases:
  - https://github.com/Genymobile/scrcpy/releases/tag/v4.0
  - https://github.com/Genymobile/scrcpy/releases/tag/v3.2
- License: Apache License 2.0
- Usage: ADB Connect launches scrcpy as an external process and embeds its
  native window inside the TV Control form.
- Modifications: No modifications are made to the official scrcpy binaries.
- License text: LICENSES/scrcpy-Apache-2.0.txt

The official Windows releases of scrcpy may contain additional third-party
components, including multimedia, USB and window-management libraries.
All license and notice files included in the official scrcpy distribution
must be preserved in redistributed packages.

## Android SDK Platform-Tools

- Component: Android Debug Bridge (ADB)
- Provider: Google / Android Open Source Project
- Official source:
  https://developer.android.com/tools/releases/platform-tools
- Usage: ADB Connect uses adb.exe to communicate with Android devices.
- Repository distribution: Android Platform-Tools binaries are not included
  in the public source repository.
- License terms:
  https://developer.android.com/studio/terms

Redistribution of Android SDK components must be reviewed separately before
they are included in a public installer.

## Microsoft .NET Runtime

- Component: Microsoft .NET Runtime
- Target version: .NET 10
- Source: https://github.com/dotnet/runtime
- License: MIT License
- Usage: ADB Connect may be published as a self-contained .NET application.
- License text: LICENSES/dotnet-MIT.txt
- Third-party notices: LICENSES/dotnet-ThirdPartyNotices.txt

## Project Assets

The ADB Connect application icon, images and other graphical assets must only
be distributed if they are original, properly licensed or used with the
permission of their respective owners.

## Disclaimer

Third-party components are distributed under their respective license terms.
ADB Connect does not claim ownership of these components.

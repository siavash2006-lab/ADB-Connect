# Third-Party Notices

ADB Connect interoperates with and may be distributed with third-party software.
Those components remain the property of their respective copyright holders and
are governed by their own terms.

The Git repository excludes third-party binaries. Optional all-in-one release
assets may include the binaries listed below together with these notices.

## scrcpy

- Primary version: 4.0
- Compatibility version: 3.3.4
- Copyright: Genymobile, Romain Vimont and contributors
- Source: <https://github.com/Genymobile/scrcpy>
- License: Apache License 2.0
- License text: [LICENSES/scrcpy-Apache-2.0.txt](LICENSES/scrcpy-Apache-2.0.txt)

ADB Connect launches scrcpy as an external process and hosts its native window
inside a Windows Forms control. Complete official Windows runtimes are packaged,
including the runtime libraries required by each version.

### Libraries included by the official scrcpy Windows runtimes

| Component | Primary runtime | Compatibility runtime | License document |
| --- | --- | --- | --- |
| FFmpeg | 8.1.1 | 7.1.1 | [LGPL 2.1](LICENSES/FFmpeg-LGPL-2.1.txt) |
| SDL | 3.4.8 | 2.32.8 | [SDL3](LICENSES/SDL3-zlib.txt), [SDL2](LICENSES/SDL2-zlib.txt) |
| libusb | 1.0.29 | 1.0.29 | [LGPL 2.1](LICENSES/libusb-LGPL-2.1.txt) |
| dav1d | 1.5.3 | 1.5.0 | [BSD 2-Clause](LICENSES/dav1d-BSD-2-Clause.txt) |
| zlib | bundled by the build | bundled by the build | [zlib License](LICENSES/zlib.txt) |

The official scrcpy build definitions compile shared FFmpeg libraries without
enabling GPL or nonfree options. The exact FFmpeg 8.1.1 and 7.1.1 source
archives are published beside every all-in-one binary release. Versioned source
URLs and hashes for all bundled components are recorded in
[BUNDLED_COMPONENT_SOURCES.md](LICENSES/BUNDLED_COMPONENT_SOURCES.md).

## Android SDK Platform-Tools / Android Debug Bridge

- Component: Android Debug Bridge (`adb`)
- Provider: Google and Android Open Source Project contributors
- Official download: <https://developer.android.com/tools/releases/platform-tools>
- Documentation: <https://developer.android.com/tools/adb>
- SDK terms: <https://developer.android.com/studio/terms>

ADB Connect invokes `adb` as an external command-line tool. Release packages
preserve the `NOTICE.txt` shipped with the exact Platform-Tools package. Reference
copies for versions used by the two scrcpy runtimes are included in `LICENSES`.

## Microsoft .NET

- Target framework: .NET 10 for Windows
- Source: <https://github.com/dotnet/runtime>
- License: MIT License
- License text: [LICENSES/dotnet-MIT.txt](LICENSES/dotnet-MIT.txt)

Release builds are self-contained. Before every publish, the project copies the
license and third-party notices from the installed .NET distribution into
`LICENSES`, so the packaged notices correspond to the runtime actually used.

## Inno Setup

Inno Setup 6 is used only to build the Windows installer. Its license and
required acknowledgement are preserved in
[LICENSES/Inno-Setup.txt](LICENSES/Inno-Setup.txt). The separate application
payload keeps its own license terms.

## Project name and artwork

Spadra is the personal project name used by the copyright owner, not a claim of
a registered company or separate legal entity. ADB Connect artwork owned by the
project owner is distributed under the repository's Apache License 2.0.

## No endorsement

ADB Connect is independent. References to Android, ADB, scrcpy, .NET, Google,
Genymobile and Microsoft identify compatible tools and technologies only. No
affiliation, sponsorship or endorsement is claimed.

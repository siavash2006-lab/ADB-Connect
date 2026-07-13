# ADB Connect

[![Platform](https://img.shields.io/badge/platform-Windows-blue)](https://www.microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Apache%202.0-green)](LICENSE)

ADB Connect is a Windows Forms application for connecting to, inspecting and
controlling Android and Android TV devices through Android Debug Bridge (ADB).
It also integrates scrcpy for interactive screen mirroring and remote control.

Repository: <https://github.com/siavash2006-lab/ADB-Connect>

## Current version

`1.6.3`

## Features

- Connect to Android devices over TCP/IP.
- Pair Wireless Debugging devices with separate pairing and connection ports.
- Discover online, offline and unauthorized ADB devices.
- Select one active device globally for all supported operations.
- Read Android, hardware, build, vendor and display properties.
- Install APK files and list, start, stop or uninstall packages.
- Capture and export Logcat output.
- Generate and save Android bug reports.
- Export the complete Android property list.
- Open an embedded TV Control window powered by scrcpy.
- Select Original or limited scrcpy resolutions, including the 1056 TV
  compatibility option.
- Detect a severely reduced scrcpy stream and switch to a compatibility runtime
  when available.
- Capture the visible TV Control area with the Windows screen-capture API and
  save it as PNG.

## Important repository scope

This public repository contains source code only. It does **not** distribute:

- compiled ADB Connect installers or release binaries;
- Android SDK Platform-Tools binaries;
- scrcpy binaries or their runtime libraries;
- code-signing certificates, logs, screenshots or device reports.

These dependencies must be obtained separately from their official sources.

## Requirements

- Windows 10 or Windows 11
- Visual Studio with the .NET desktop development workload
- .NET 10 SDK
- Android SDK Platform-Tools
- scrcpy 4.0 Windows runtime for primary operation
- scrcpy 3.2 Windows runtime for compatibility fallback
- Inno Setup only if you intend to create a local installer

## Dependency setup

### Android SDK Platform-Tools

Download Platform-Tools directly from Google:

<https://developer.android.com/tools/releases/platform-tools>

Copy the required official files into:

```text
platform-tools/
    adb.exe
    AdbWinApi.dll
    AdbWinUsbApi.dll
```

Android SDK Platform-Tools are not included in this repository. Use of the SDK
is subject to Google's applicable terms.

### scrcpy

Download scrcpy only from its official GitHub repository:

<https://github.com/Genymobile/scrcpy/releases>

The expected source layout is:

```text
scrcpy/
    win-x64/
        scrcpy.exe
        ...complete primary scrcpy 4.0 Windows runtime...
        compat/
            scrcpy.exe
            ...complete scrcpy 3.2 compatibility runtime...
    win-x86/
        scrcpy.exe
        ...complete primary scrcpy 4.0 Windows runtime...
        compat/
            scrcpy.exe
            ...complete scrcpy 3.2 compatibility runtime...
```

Copy the complete official runtime contents, not only `scrcpy.exe`. Keep all
license and notice files supplied with the official archives.

## Build

Open `ADB Connect.csproj` in Visual Studio and select the required runtime, or
use one of the included publishing scripts:

```text
Publish-x64.cmd
Publish-x86.cmd
```

Expected publish directories:

```text
publish/x64/
publish/x86/
```

The scripts verify that both the primary and compatibility scrcpy runtimes were
included in the publish output.

## Wireless Debugging

On devices that use Android Wireless Debugging, the pairing port and connection
port may be different. Enter the values exactly as displayed by the device. The
pairing code is sent to ADB through standard input and is not intentionally
written to application logs.

## Compatibility notes

Some Android TV video encoders may cause newer scrcpy versions to return a
stream that is much smaller than the physical display. When Original mode is
used, ADB Connect compares the expected display size with the received scrcpy
texture and can switch to the configured scrcpy 3.2 compatibility runtime when
a severe reduction is detected.

The fallback is based on the reported display and stream dimensions, not on a
specific IP address or television model.

Windows-based screenshots capture the visible TV Control area. Content protected
by Android or application security policies may appear black, while unprotected
menus and overlays may remain visible.

## Security and responsible use

Use ADB Connect only with devices that you own or are explicitly authorized to
manage. Enabling ADB or Wireless Debugging gives a connected computer extensive
access to the Android device.

Do not commit pairing codes, private IP inventories, bug reports, customer data,
code-signing certificates or other sensitive information to the repository.

## License

ADB Connect is licensed under the [Apache License 2.0](LICENSE).

Third-party components remain subject to their respective licenses. See
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) and the [LICENSES](LICENSES)
directory.

ADB Connect is an independent project and is not affiliated with, endorsed by,
or sponsored by Google, Genymobile or Microsoft.

## Issues and contributions

Bug reports and improvement proposals may be submitted through the repository's
Issues section. When reporting a device-specific problem, remove sensitive data
and include only the information required to reproduce the issue, such as the
device model, Android version, ADB state, scrcpy version and relevant sanitized
log lines.

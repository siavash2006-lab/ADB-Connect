# ADB Connect

[Documentation in English and Persian](DOCUMENTATION.md)

[![Platform](https://img.shields.io/badge/platform-Windows-blue)](https://www.microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Apache%202.0-green)](LICENSE)

ADB Connect is a Windows Forms application for connecting to, inspecting and
controlling Android and Android TV devices through Android Debug Bridge (ADB).
It integrates scrcpy for interactive screen mirroring and remote control.

- Repository: <https://github.com/siavash2006-lab/ADB-Connect>
- Project website: <https://spadra.ir>
- Current version: **1.6.6**

Spadra is the personal project name used by the owner of ADB Connect. It is not
presented as a registered company or separate legal entity.

Release notes: [English](RELEASE-NOTES-1.6.6.md) | [فارسی](RELEASE-NOTES-1.6.6-FA.md).

## Features

- Compact interface with a single sun/moon theme toggle. The initial theme follows Windows; manual choices are saved.
- Connect to Android devices over TCP/IP.
- Pair Wireless Debugging devices with separate pairing and connection ports.
- Discover online, offline and unauthorized ADB devices.
- Select one active device globally for all supported operations.
- Read properties inline, with Copy and optional Save actions.
- Install APK files and list, start, stop or uninstall packages.
- Capture and export Logcat output.
- Generate and save Android bug reports.
- Export the complete Android property list.
- Open a TV Control window powered by scrcpy.
- Select Original or limited scrcpy resolutions, including the 1056 TV
  compatibility option.
- Detect a severely reduced scrcpy stream and switch to scrcpy 3.3.4 when
  available.
- Capture the visible TV Control area with the Windows screen-capture API and
  save it as PNG.
- Open the Spadra website by clicking **Spadra** in the main form footer.

## Repository and release scope

The Git repository contains the ADB Connect source, release scripts and license
documentation. Third-party binaries and credentials are excluded from Git. The
source package includes the generic Inno Setup definitions under `Installer/`;
only generated `Installer/Output/` files are excluded from Git.

GitHub Releases may provide all-in-one x64 and x86 installers and portable ZIP
files. Those packages include the required ADB, scrcpy and self-contained .NET
runtime files plus the applicable notices. The corresponding FFmpeg source
archives are published beside the binary assets.

## Requirements

- Windows 10 or Windows 11
- Visual Studio with the .NET desktop development workload
- .NET 10 SDK
- Android SDK Platform-Tools
- scrcpy 4.0 Windows runtime for primary operation
- scrcpy 3.3.4 Windows runtime for compatibility fallback
- Inno Setup 6 or 7 for installers
- Windows SDK and a trusted Authenticode certificate for public releases

## Dependency layout

Download dependencies only from their official sources. Do not commit these
binary directories to Git.

```text
platform-tools/
    adb.exe
    AdbWinApi.dll
    AdbWinUsbApi.dll
    NOTICE.txt

scrcpy/
    win-x64/
        ...complete scrcpy 4.0 Windows runtime...
        compat/
            ...complete scrcpy 3.3.4 Windows runtime...
    win-x86/
        ...complete scrcpy 4.0 Windows runtime...
        compat/
            ...complete scrcpy 3.3.4 Windows runtime...
```

- Platform-Tools: <https://developer.android.com/tools/releases/platform-tools>
- scrcpy: <https://github.com/Genymobile/scrcpy/releases>

Copy the complete scrcpy runtime contents, not only `scrcpy.exe`. The publish
scripts intentionally include only the required Platform-Tools files and verify
that `NOTICE.txt` is present.

## Build and publish

Open `ADB Connect.csproj` in Visual Studio for development. For release output,
run both scripts from a Developer Command Prompt:

```text
Publish-x64.cmd
Publish-x86.cmd
```

The scripts create self-contained outputs under `publish/v1.6.6/x64` and `publish/v1.6.6/x86`,
copy the license files for the installed .NET version and fail if a required
runtime or notice file is missing.

## Signing and all-in-one release

See [SIGNING.md](SIGNING.md) before producing public binaries. Spadra remains the
personal project name in application metadata; the trusted Publisher displayed
by Windows is the legal personal name contained in the code-signing certificate.

After publishing:

1. Sign both application executables with `Sign-Published-Binaries.ps1`.
2. Configure the Inno Setup sign tool named `PersonalCodeSign`.
3. Build signed installers using `Build-Installers.ps1 -RequireSigned -SignCommand 'YOUR_SIGNING_COMMAND'` (see SIGNING.md).
4. Run `Create-Release-Packages.ps1`.
5. Verify the generated file with `Verify-SHA256SUMS.ps1`.

The final script creates x64/x86 portable ZIPs, copies the signed installers,
downloads and verifies the corresponding FFmpeg sources, and generates
`SHA256SUMS.txt` under `release/v1.6.6`.

It also creates `ADB-Connect-1.6.6-Source.zip` and a clean `Source/` tree for
browser-based repository uploads. For the current unsigned packages, use
`Create-Release-Packages.ps1 -AllowUnsigned`. See [browser upload instructions](GITHUB-UPLOAD-EN.md)
before uploading source files and release assets.

## Wireless Debugging

The pairing port and connection port may be different. Enter both values exactly
as displayed by the device. The pairing code is sent to ADB through standard
input and is not intentionally written to application logs.

## Compatibility notes

Some Android TV video encoders may cause newer scrcpy versions to return a stream
that is much smaller than the physical display. In Original mode, ADB Connect
compares the expected display size with the received texture and can switch to
scrcpy 3.3.4 when a severe reduction is detected. The decision is based on the
reported dimensions, not on an IP address or device model.

Windows-based screenshots capture the visible TV Control area. Content protected
by Android or application security policies may appear black, while unprotected
menus and overlays may remain visible.

## Security and responsible use

Use ADB Connect only with devices that you own or are authorized to manage. Do
not commit pairing codes, device reports, private IP inventories, logs,
code-signing certificates or certificate passwords.

## License

ADB Connect is licensed under the [Apache License 2.0](LICENSE). Third-party
components remain subject to their own licenses; see
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) and [LICENSES](LICENSES).

ADB Connect is independent and is not affiliated with, endorsed by or sponsored
by Google, Genymobile or Microsoft.

## Validation

See [1.6.6 validation](VALIDATION-1.6.6.md), [release notes](RELEASE-NOTES-1.6.6.md) and the historical [1.6.5 changelog](CHANGELOG-1.6.5.md) and [Persian test guide](TEST-1.6.5-FA.md). Run `dotnet run --project tests/RegressionTests.csproj` for local regression checks without Android devices. Raw Logcat files are saved under `%LOCALAPPDATA%\ADB Connect\Logs`; Keyword filtering affects display only. Package filtering limits capture to the PID selected at Start. Clear View does not erase the device buffer. Local `Build-Installers.ps1` builds are unsigned unless `-RequireSigned` and `-SignCommand` are supplied.

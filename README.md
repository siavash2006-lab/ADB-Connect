# ADB Connect

ADB Connect is a Windows WinForms application for managing Android TV
devices through ADB and controlling them using scrcpy.

## Current Features

- ADB connection over TCP/IP
- Wireless ADB pairing
- Global device selection
- APK installation and package management
- Device information
- Logcat and bugreport
- Embedded scrcpy TV Control
- Automatic scrcpy compatibility fallback
- Windows-based screenshot capture

## Requirements

- Windows 10 or Windows 11
- Visual Studio
- .NET 10 SDK
- Inno Setup
- Android SDK Platform Tools
- scrcpy x86 and x64 runtimes

## Build

Use the following scripts:

- `Publish-x64.cmd`
- `Publish-x86.cmd`

## Runtime Dependencies

Binary runtime files are not stored in this repository.

Required directories:

- `platform-tools`
- `scrcpy/win-x64`
- `scrcpy/win-x86`

See `scrcpy/README.md` for setup instructions.

## Version

Current version: 1.6.3

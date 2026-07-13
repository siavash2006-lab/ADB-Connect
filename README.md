# ADB Connect

Windows WinForms application for managing Android TV devices through ADB
and controlling them using scrcpy.

## Requirements

- Windows 10/11
- Visual Studio
- .NET 10 SDK
- Inno Setup
- Android SDK Platform Tools
- scrcpy x64 and x86 runtimes

## Build

Use:

- Publish-x64.cmd
- Publish-x86.cmd

## Runtime dependencies

The following binaries are not stored in the Git repository:

- platform-tools
- scrcpy/win-x64
- scrcpy/win-x86

See scrcpy/README.md for installation instructions.
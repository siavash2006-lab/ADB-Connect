# Android Platform-Tools files

Download the official Windows Platform-Tools package from:

<https://developer.android.com/tools/releases/platform-tools>

For the current ADB Connect project, place the following version-matched files
in this directory:

```text
adb.exe
AdbWinApi.dll
AdbWinUsbApi.dll
NOTICE.txt
```

ADB Connect 1.6.4 was prepared against Platform-Tools 36.0.0. If the binaries
are upgraded, replace `NOTICE.txt` with the notice from the same official package.
Do not commit the binaries or notice package to Git.

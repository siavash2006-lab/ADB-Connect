# scrcpy runtime files

Download complete official Windows runtimes from:

<https://github.com/Genymobile/scrcpy/releases>

Expected layout:

```text
scrcpy/
    win-x64/              scrcpy 4.0 x64 runtime
        compat/           scrcpy 3.3.4 x64 runtime
    win-x86/              scrcpy 4.0 x86 runtime
        compat/           scrcpy 3.3.4 x86 runtime
```

Copy every file from each official archive, not only `scrcpy.exe`. Do not commit
runtime binaries to Git. Licenses, source references and FFmpeg source-release
requirements are documented in `THIRD_PARTY_NOTICES.md` and `LICENSES/`.

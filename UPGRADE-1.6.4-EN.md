# Upgrading to ADB Connect 1.6.4 (historical guide)

[فارسی](UPGRADE-1.6.4-FA.md)

This is the English counterpart of the original 1.6.4 guide. It documents that release's paths and workflow, not current 1.6.5 behavior. For current builds use README.md, SIGNING.md and TEST-1.6.5-EN.md. In particular, current installer signing is explicit and outputs use versioned folders.

Version 1.6.4 links the Spadra footer to https://spadra.ir, updates the version and extends all-in-one packaging, signing and SHA-256 generation. Spadra is a personal project name, not a registered legal entity. A trusted Windows Publisher is the legal identity in the signing certificate.

## 1. Preparation

Back up the project and close Visual Studio, ADB Connect and scrcpy. The original guide suggested the following if Clean Solution encountered locked ADB files; these affect all matching processes and the shared server, so they are historical troubleshooting instructions, not part of the current app's shutdown behavior:

```bat
adb kill-server
taskkill /F /IM adb.exe
taskkill /F /IM scrcpy.exe
```

## 2. Files replaced by the 1.6.4 package

```text
ADB Connect.csproj
Form1.cs
Form1.Designer.cs
Publish-x64.cmd
Publish-x86.cmd
README.md
SCRCPY_INTEGRATION.md
THIRD_PARTY_NOTICES.md
NOTICE
LICENSES/README.md
LICENSES/dotnet-MIT.txt
LICENSES/scrcpy-Apache-2.0.txt
```

## 3. Files added

```text
.gitignore
Prepare-DotNet-Notices.ps1
Sign-Published-Binaries.ps1
Build-Installers.ps1
Create-Release-Packages.ps1
Create-SHA256SUMS.ps1
Verify-SHA256SUMS.ps1
Download-Release-Sources.ps1
SIGNING.md
UPGRADE-1.6.4-FA.md
Installer/
LICENSES/Android-Platform-Tools-36.0.0-NOTICE.txt
LICENSES/Android-Platform-Tools-37.0.0-NOTICE.txt
LICENSES/BUNDLED_COMPONENT_SOURCES.md
LICENSES/FFmpeg-LGPL-2.1.txt
LICENSES/Inno-Setup.txt
LICENSES/SDL2-zlib.txt
LICENSES/SDL3-zlib.txt
LICENSES/dav1d-BSD-2-Clause.txt
LICENSES/libusb-LGPL-2.1.txt
LICENSES/zlib.txt
platform-tools/NOTICE.txt
platform-tools/README.md
scrcpy/README.md
```

The original instructions removed `Properties/PublishProfiles/win-x64.pubxml.user` and `win-x86.pubxml.user`; Visual Studio can recreate them. Do not remove/replace dependency executables as part of that upgrade. Keep NOTICE.txt beside adb.exe, primary scrcpy 4.0 and compatibility scrcpy 3.3.4. Publish generates .NET third-party notices from the installed runtime; do not add them manually.

## 4. Visual Studio verification

Open the project, Clean Solution, then Rebuild Solution. Run it and click Spadra; the browser should open https://spadra.ir and the footer should show Version: 1.6.4.

## 5. Architecture outputs

Run `Publish-x64.cmd` and `Publish-x86.cmd`. In the original release, outputs were self-contained under `publish/x64` and `publish/x86`; the scripts checked ADB, both scrcpy runtimes and licenses.

## 6. Unknown Publisher and signing

A publicly trusted personal Authenticode certificate is needed to replace Unknown Publisher with a verified identity. Spadra metadata alone is insufficient unless recognized by the certificate authority. Find the certificate thumbprint:

```powershell
Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert |
  Select-Object Subject, Thumbprint, NotAfter
```

Sign the application:

```powershell
.\Sign-Published-Binaries.ps1 `
  -CertificateThumbprint 'THUMBPRINT' `
  -TimestampUrl 'TIMESTAMP_URL_PROVIDED_BY_YOUR_CA'
```

The old guide then configured PersonalCodeSign in Inno Setup and ran Build-Installers.ps1. Current 1.6.5 instead requires explicit `-RequireSigned -SignCommand` for signed installers; see SIGNING.md.

## 7. All-in-one assets and hashes

After signing the applications and installers, the original workflow ran Create-Release-Packages.ps1 and Verify-SHA256SUMS.ps1. Expected 1.6.4 assets were x64/x86 Setup.exe and Portable.zip, ffmpeg-8.1.1.tar.xz, ffmpeg-7.1.1.tar.xz and SHA256SUMS.txt under release/v1.6.4. Upload all seven to a draft release tagged v1.6.4, verify signatures/hashes, then publish.

## 8. Source upload

Replace the files in section 2 and add section 3 through GitHub's website. The historical guide kept Installer private via .gitignore. For the current complete source package, only the generic .iss definitions are included; generated installer output remains excluded. Never upload certificates/passwords, publish, release, bin, obj or user-specific settings into the source repository.

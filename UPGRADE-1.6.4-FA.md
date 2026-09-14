# راهنمای ارتقا به ADB Connect 1.6.4

[English](UPGRADE-1.6.4-EN.md)

این راهنما مربوط به نسخهٔ قدیمی 1.6.4 است؛ برای مسیرها و امضای نسخهٔ فعلی README.md و SIGNING.md را بخوانید.

این نسخه کلیک روی عبارت **Spadra** در پایین فرم اصلی را به آدرس
`https://spadra.ir` متصل می‌کند، شماره نسخه را به 1.6.4 ارتقا می‌دهد و فرآیند
انتشار All-in-one، امضای دیجیتال و تولید SHA-256 را تکمیل می‌کند.

Spadra در مشخصات برنامه به‌عنوان نام پروژه شخصی استفاده شده است و به‌معنای شرکت
ثبت‌شده یا شخصیت حقوقی مستقل نیست. در امضای معتبر Windows، Publisher برابر نام
قانونی دارنده گواهی شخصی Code Signing خواهد بود.

## 1. پیش‌نیاز

1. از پوشه فعلی پروژه یک نسخه پشتیبان تهیه کنید.
2. Visual Studio، ADB Connect و scrcpy را ببندید.
3. اگر `Clean Solution` با خطای قفل بودن ADB مواجه شد، در Command Prompt اجرا کنید:

```bat
adb kill-server
taskkill /F /IM adb.exe
taskkill /F /IM scrcpy.exe
```

## 2. فایل‌های قابل جایگزینی

فایل‌های زیر را از بسته نسخه 1.6.4 روی فایل‌های هم‌نام پروژه خود Overwrite کنید:

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

## 3. فایل‌ها و پوشه‌های جدید

موارد زیر را به ریشه پروژه اضافه کنید:

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

دو فایل محلی زیر را حذف کنید؛ Visual Studio در صورت نیاز دوباره آن‌ها را می‌سازد:

```text
Properties/PublishProfiles/win-x64.pubxml.user
Properties/PublishProfiles/win-x86.pubxml.user
```

فایل‌های اجرایی پوشه‌های `platform-tools` و `scrcpy` خود را حذف یا جایگزین نکنید.
نسخه حاضر `platform-tools/NOTICE.txt` مربوط به Platform-Tools 36.0.0 را اضافه
می‌کند. فقط مطمئن شوید:

- فایل `platform-tools/NOTICE.txt` کنار `adb.exe` وجود دارد؛
- نسخه اصلی scrcpy برابر 4.0 است؛
- نسخه سازگاری scrcpy برابر 3.3.4 است.

فایل `LICENSES/dotnet-THIRD-PARTY-NOTICES.txt` را دستی اضافه نکنید؛ اسکریپت Publish
آن را مستقیماً از نسخه .NET نصب‌شده روی سیستم شما ایجاد می‌کند.

## 4. کنترل در Visual Studio

1. پروژه را باز کنید و `Clean Solution` و سپس `Rebuild Solution` را اجرا کنید.
2. برنامه را اجرا کنید و پایین فرم روی **Spadra** کلیک کنید.
3. باید مرورگر پیش‌فرض، آدرس `https://spadra.ir` را باز کند.
4. در پایین فرم باید `Version: 1.6.4` نمایش داده شود.

## 5. ساخت خروجی‌های x64 و x86

در ریشه پروژه اجرا کنید:

```bat
Publish-x64.cmd
Publish-x86.cmd
```

اسکریپت‌ها خروجی‌های Self-contained را در `publish/x64` و `publish/x86` می‌سازند
و وجود ADB، هر دو نسخه scrcpy و مجوزهای لازم را کنترل می‌کنند.

## 6. رفع Unknown Publisher

برای حذف واقعی عبارت `Unknown Publisher` باید یک گواهی معتبر عمومی Authenticode
به‌نام شخص خود تهیه کنید. نام Spadra به‌تنهایی قابل تبدیل به Publisher مورد اعتماد
Windows نیست، مگر آنکه مرجع صدور گواهی آن را به‌عنوان هویت قانونی تأیید کند.

پس از نصب گواهی، Thumbprint آن را پیدا کنید:

```powershell
Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert |
  Select-Object Subject, Thumbprint, NotAfter
```

فایل‌های برنامه را امضا کنید:

```powershell
.\Sign-Published-Binaries.ps1 `
  -CertificateThumbprint 'THUMBPRINT' `
  -TimestampUrl 'TIMESTAMP_URL_PROVIDED_BY_YOUR_CA'
```

سپس طبق `SIGNING.md` ابزار `PersonalCodeSign` را در Inno Setup تعریف و اجرا کنید:

```powershell
.\Build-Installers.ps1
```

## 7. آماده‌سازی All-in-one و SHA256SUMS

پس از امضای فایل‌های اجرایی و Setupها اجرا کنید:

```powershell
.\Create-Release-Packages.ps1
.\Verify-SHA256SUMS.ps1
```

پوشه `release/v1.6.4` شامل این موارد خواهد بود:

```text
ADB-Connect-1.6.4-x64-Setup.exe
ADB-Connect-1.6.4-x86-Setup.exe
ADB-Connect-1.6.4-x64-Portable.zip
ADB-Connect-1.6.4-x86-Portable.zip
ffmpeg-8.1.1.tar.xz
ffmpeg-7.1.1.tar.xz
SHA256SUMS.txt
```

تمام هفت فایل را در Release جدید GitHub با Tag برابر `v1.6.4` بارگذاری کنید.
ابتدا Release را به‌صورت Draft بسازید، امضاها و SHA-256 را کنترل کنید و سپس آن را
منتشر نمایید.

## 8. به‌روزرسانی سورس در GitHub

در وب‌سایت GitHub فایل‌های بخش 2 را جایگزین و فایل‌های جدید بخش 3 را اضافه کنید.
اگر می‌خواهید تعریف‌های Installer خصوصی بمانند، پوشه `Installer/` را در GitHub
بارگذاری نکنید؛ این پوشه در `.gitignore` نیز قرار گرفته است. فایل‌های گواهی، رمز،
`publish/`، `release/`، `bin/` و `obj/` را هرگز بارگذاری نکنید.

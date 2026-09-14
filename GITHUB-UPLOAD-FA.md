# بارگذاری نسخهٔ 1.6.5 از طریق مرورگر GitHub

[English](GITHUB-UPLOAD-EN.md)

مخزن: https://github.com/siavash2006-lab/ADB-Connect

## ۱. سورس برنامه

وارد مخزن شو و **Add file > Upload files** را انتخاب کن. **محتویات** پوشهٔ `release/v1.6.5/Source` را بکش داخل صفحه؛ خود پوشهٔ بیرونی Source یا فایل Source.zip را داخل درخت کد نگذار. ساختار زیرپوشه‌ها حفظ شود. پوشهٔ آماده‌شده شامل فایل‌های شخصی، لاگ، گواهی، `.user`، bin/obj، خروجی نصب‌کننده یا فایل‌های اجرایی وابستگی‌ها نیست. محدودیت آپلود مرورگر ۱۰۰ فایل در هر بار و ۲۵ MiB برای هر فایل است؛ در صورت نیاز چند نوبت آپلود کن. [راهنمای GitHub](https://docs.github.com/en/repositories/working-with-files/managing-files/adding-a-file-to-a-repository)

متن Commit: `Prepare ADB Connect 1.6.5 with fixes and bilingual documentation`. برای بازبینی با Pull Request می‌توانی یک شاخه مثل `release-1.6.5` بسازی؛ همهٔ نوبت‌های آپلود روی همان شاخه باشند. فایل قدیمی **AdbProgressRunner.cs** را از منوی همان فایل در شاخه حذف کن؛ آپلود فایل‌های جدید، فایل قدیمی را حذف نمی‌کند. فایل‌های مستقل موجود مثل docs/images را نگه دار. پیش از ساخت Tag، تغییرات را بررسی و با main ادغام کن. اگر حفاظت شاخه فعال است، مسیر Pull Request مخزن را دنبال کن.

در main وجود نسخهٔ 1.6.5 در پروژه، Form1.Operations.cs، LogViewWriter.cs، tests/WinForms، DOCUMENTATION.md، جفت مستندات EN/FA و Installer/*.iss را بررسی کن. Setupها و Portable.zip نباید در درخت کد باشند. آرشیوهای Source code خودکار GitHub از Commit متصل به Tag ساخته می‌شوند؛ بنابراین Tag را بعد از به‌روزرسانی سورس بساز.

## ۲. فایل‌های Release

به **Releases > Draft a new release** برو. Tag جدید **v1.6.5** را روی main به‌روز بساز و عنوان را **ADB Connect 1.6.5** بگذار. متن RELEASE-NOTES-1.6.5.md را در توضیحات کپی کن. اگر این Tag از قبل وجود داشت، ابتدا مقصد و Release آن را بررسی کن؛ از Tag متصل به سورس قدیمی استفاده نکن. هشت فایل زیر را از ریشهٔ `release/v1.6.5` پیوست کن. اول Draft ذخیره کن، سپس بررسی و منتشر کن. [راهنمای Release](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository)

```text
ADB-Connect-1.6.5-x64-Setup.exe
ADB-Connect-1.6.5-x86-Setup.exe
ADB-Connect-1.6.5-x64-Portable.zip
ADB-Connect-1.6.5-x86-Portable.zip
ADB-Connect-1.6.5-Source.zip
ffmpeg-8.1.1.tar.xz
ffmpeg-7.1.1.tar.xz
SHA256SUMS.txt
```

Source.zip یک نسخهٔ آماده از سورس محلی است و جای آپلود فایل‌های واقعی در درخت کد را نمی‌گیرد. بسته‌های ویندوز بدون امضا هستند و این موضوع در متن Release نوشته شده است. قبل از انتشار، کامل‌شدن همهٔ پیوست‌ها و وجود اصلاحات fix1/fix2 در بسته نهایی را بررسی کن. بعد از انتشار، یک فایل را دانلود و SHA-256 آن را با SHA256SUMS.txt مقایسه کن. بسته‌های آزمایشی قدیمی پوشه‌های fix1/fix2 را برای این Release استفاده نکن.

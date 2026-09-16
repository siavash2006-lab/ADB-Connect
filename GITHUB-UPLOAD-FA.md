# انتشار ADB Connect 1.6.6 در مرورگر

[English](GITHUB-UPLOAD-EN.md)

۱. در مخزن https://github.com/siavash2006-lab/ADB-Connect گزینهٔ Add file > Upload files را بزن. محتویات داخل release/v1.6.6/Source را با حفظ زیرپوشه‌ها بارگذاری کن؛ خود پوشهٔ بیرونی Source و فایل‌های اجرایی یا ZIP را وارد درخت کد نکن.

۲. شاخهٔ release-1.6.6 بساز و پیام Commit را بگذار:
Release 1.6.6: themes, inline properties and package fixes
فایل .gitignore موجود را حفظ کن؛ اگر مرورگر فایل مخفی را نپذیرفت، محتوای محلی را در ویرایشگر GitHub کپی کن. حذف دوبارهٔ AdbProgressRunner.cs لازم نیست.

۳. Pull Request را بررسی و در main ادغام کن. وجود AppTheme.cs، AppDialog.cs، ThemeControls.cs، Form1.Theme.cs، Form1.Properties.cs و نسخهٔ 1.6.6 پروژه را بررسی کن.

۴. Release جدید با Tag برابر v1.6.6 روی main به‌روز و عنوان ADB Connect 1.6.6 بساز. متن RELEASE-NOTES-1.6.6.md را در توضیحات کپی کن.

۵. این هشت فایل را از release/v1.6.6 پیوست کن؛ پیش‌نویس را ذخیره و بررسی کن، سپس به‌عنوان Latest منتشر کن. نسخهٔ 1.6.5 را تغییر نده.

ADB-Connect-1.6.6-x64-Setup.exe
ADB-Connect-1.6.6-x86-Setup.exe
ADB-Connect-1.6.6-x64-Portable.zip
ADB-Connect-1.6.6-x86-Portable.zip
ADB-Connect-1.6.6-Source.zip
ffmpeg-8.1.1.tar.xz
ffmpeg-7.1.1.tar.xz
SHA256SUMS.txt

بسته‌ها بدون امضا هستند و این موضوع در توضیحات ذکر شده است. پس از انتشار هش فایل‌ها را با SHA256SUMS.txt مقایسه کن. سورس خودکار GitHub مربوط به Commit تگ است و Source.zip یک نسخهٔ اضافی از سورس آماده‌شده است.

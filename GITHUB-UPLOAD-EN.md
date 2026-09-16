# Uploading ADB Connect 1.6.6

[فارسی](GITHUB-UPLOAD-FA.md)

1. Open https://github.com/siavash2006-lab/ADB-Connect and choose Add file > Upload files. Upload the contents of release/v1.6.6/Source, keeping subfolders. Do not upload the enclosing Source folder, executable packages or ZIP files into the code tree.
2. Create branch release-1.6.6 and commit with: Release 1.6.6: themes, inline properties and package fixes. Preserve the existing .gitignore; if the browser refuses that hidden file, edit it directly and paste the local contents. No additional deletion of AdbProgressRunner.cs is needed.
3. Review the pull request and merge into main. Confirm AppTheme.cs, AppDialog.cs, ThemeControls.cs, Form1.Theme.cs, Form1.Properties.cs and project version 1.6.6 are present.
4. Create a release with new tag v1.6.6 on updated main, title ADB Connect 1.6.6, and paste RELEASE-NOTES-1.6.6.md.
5. Attach the eight files below from release/v1.6.6. Save draft, review the attachments and publish as Latest. Keep v1.6.5 unchanged.

ADB-Connect-1.6.6-x64-Setup.exe
ADB-Connect-1.6.6-x86-Setup.exe
ADB-Connect-1.6.6-x64-Portable.zip
ADB-Connect-1.6.6-x86-Portable.zip
ADB-Connect-1.6.6-Source.zip
ffmpeg-8.1.1.tar.xz
ffmpeg-7.1.1.tar.xz
SHA256SUMS.txt

Packages are unsigned; the release notes state this. After publication, verify asset hashes against SHA256SUMS.txt. GitHub's automatic source archives follow the tagged commit; Source.zip is an additional snapshot.

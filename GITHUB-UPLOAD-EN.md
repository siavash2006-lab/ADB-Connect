# Uploading 1.6.5 through GitHub's browser interface

[فارسی](GITHUB-UPLOAD-FA.md)

Repository: https://github.com/siavash2006-lab/ADB-Connect

## 1. Source

Open the repository, choose **Add file > Upload files**, and drag the **contents** of `release/v1.6.5/Source` onto the page. Do not drag the enclosing Source folder or upload Source.zip into the code tree. Keep nested paths intact. The prepared tree excludes runtime binaries, logs, certificates, `.user` files, bin/obj and installer outputs. GitHub browser uploads allow 100 files at a time and 25 MiB per file; split into batches if necessary. [GitHub file-upload documentation](https://docs.github.com/en/repositories/working-with-files/managing-files/adding-a-file-to-a-repository)

Use commit message `Prepare ADB Connect 1.6.5 with fixes and bilingual documentation`. Choose a new branch (for example `release-1.6.5`) if reviewing through a pull request. Upload all batches to the same branch. Delete the obsolete **AdbProgressRunner.cs** from that branch using the file's menu; uploads do not remove old files. Preserve unrelated existing assets such as docs/images. Review/merge into main before creating the release tag. If branch protection changes the available buttons, follow the repository's PR flow.

Check main: version 1.6.5 in the project, Form1.Operations.cs, LogViewWriter.cs, tests/WinForms, DOCUMENTATION.md, EN/FA document pairs, and Installer/*.iss. The source tree must not contain Setup executables or portable ZIPs. GitHub-generated source archives will reflect the tagged commit, so the tag must be created after the source update.

## 2. Release files

Open **Releases > Draft a new release**, create tag **v1.6.5** targeting the updated main, and title it **ADB Connect 1.6.5**. Paste RELEASE-NOTES-1.6.5.md into the description. If v1.6.5 already exists, inspect its target and release before proceeding; do not reuse a tag pointing to old source. Attach the eight files directly under `release/v1.6.5` listed below. Save a draft first, then review and publish. [GitHub release documentation](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository)

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

The source ZIP is an additional convenient local snapshot; it does not replace the code-tree upload. The Windows packages are unsigned, as stated in the notes. Before publishing, confirm all attachments completed and the final packages contain fix1/fix2. After publication, download an asset and compare its SHA-256 against SHA256SUMS.txt. Do not use the older preview packages from v1.6.5-fix1/fix2.

# Code signing ADB Connect releases

The `Company` and `AppPublisher` metadata use **Spadra** as the personal project
name. This metadata does not create a Windows-trusted publisher identity.

For public releases, obtain a publicly trusted Authenticode code-signing
certificate as an individual. Windows will display the legal personal name in
that certificate as the verified Publisher. Do not use a certificate issued to
an unregistered company name and never commit certificate files or passwords to
the repository.

## Sign the published applications

After running both publish scripts, open PowerShell in the project directory:

```powershell
.\Sign-Published-Binaries.ps1 `
  -CertificateThumbprint 'YOUR_CERTIFICATE_THUMBPRINT' `
  -TimestampUrl 'YOUR_CERTIFICATE_AUTHORITY_TIMESTAMP_URL'
```

The script signs and verifies the x64 and x86 application executables with
SHA-256. The Windows SDK must be installed so `signtool.exe` is available.

## Configure Inno Setup

Open **Tools > Configure Sign Tools** in Inno Setup and add a tool named
`PersonalCodeSign`. Use the exact signing command supplied by the certificate
authority. A typical command is:

```text
"C:\Path\To\signtool.exe" sign /sha1 YOUR_CERTIFICATE_THUMBPRINT /fd SHA256 /td SHA256 /tr YOUR_TIMESTAMP_URL /d "ADB Connect" $f
```

For local testing, `Build-Installers.ps1` creates unsigned installers and reports
their actual signature status. For a signed release, supply the signing command
explicitly; this also enables signing the uninstaller:

```powershell
.\Build-Installers.ps1 -RequireSigned -SignCommand '"C:\Path\To\signtool.exe" sign /sha1 YOUR_CERTIFICATE_THUMBPRINT /fd SHA256 /td SHA256 /tr YOUR_TIMESTAMP_URL /d "ADB Connect" $f'
```

Verify every generated setup file before publishing:

```powershell
Get-AuthenticodeSignature '.\Installer\Output\ADB-Connect-1.6.5-x64-Setup.exe'
Get-AuthenticodeSignature '.\Installer\Output\ADB-Connect-1.6.5-x86-Setup.exe'
```

The status must be `Valid`. Code signing identifies the publisher and protects
file integrity, but SmartScreen reputation is separate and may still take time
to develop for a new certificate and newly downloaded files.

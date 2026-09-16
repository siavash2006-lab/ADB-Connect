[CmdletBinding()]
param([ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '1.6.6')
$ErrorActionPreference = 'Stop'
$release = Join-Path $PSScriptRoot "release\v$Version"
$stage = Join-Path $release 'Source'
if (Test-Path -LiteralPath $stage) { throw "Source staging already exists: $stage. Review it before rebuilding; this script never deletes it automatically." }
New-Item -ItemType Directory -Path $stage -Force | Out-Null
$files = @(Get-ChildItem -LiteralPath $PSScriptRoot -File -Force | Where-Object {
    $_.Extension -in @('.cs','.resx','.csproj','.slnx','.ps1','.cmd','.md') -or $_.Name -in @('.gitignore','LICENSE','NOTICE')
})
foreach ($folder in @('Properties','img','LICENSES','Installer','tests')) {
    $files += Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot $folder) -Recurse -File | Where-Object {
        $relative = $_.FullName.Substring($PSScriptRoot.Length + 1)
        $relative -notmatch '(?i)(^|[\\/])(bin|obj|Output|PublishProfiles)([\\/]|$)' -and
        $_.Extension -in @('.cs','.resx','.csproj','.ico','.gif','.png','.md','.txt','.iss') -and
        $_.Name -ne 'dotnet-THIRD-PARTY-NOTICES.txt'
    }
}
foreach ($relative in @('platform-tools\README.md','platform-tools\NOTICE.txt','scrcpy\README.md')) {
    $files += Get-Item -LiteralPath (Join-Path $PSScriptRoot $relative)
}
foreach ($file in $files) {
    $relative = $file.FullName.Substring($PSScriptRoot.Length + 1)
    $destination = Join-Path $stage $relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $destination
}
$manifest = foreach ($file in ($files | Sort-Object FullName)) {
    $relative = $file.FullName.Substring($PSScriptRoot.Length + 1).Replace('\','/')
    "{0} *{1}" -f (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $relative
}
[IO.File]::WriteAllLines((Join-Path $stage 'SOURCE-MANIFEST.sha256'), $manifest, [Text.Encoding]::UTF8)
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = Join-Path $release "ADB-Connect-$Version-Source.zip"
if (Test-Path -LiteralPath $zip) { throw "Source archive already exists: $zip" }
[IO.Compression.ZipFile]::CreateFromDirectory($stage, $zip)
Write-Host "Source files staged: $($files.Count + 1). ZIP: $zip"

[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version = '1.6.6',

    [switch]$AllowUnsigned
)

$ErrorActionPreference = 'Stop'
$releasePath = Join-Path $PSScriptRoot "release\v$Version"
$installerPath = Join-Path $PSScriptRoot 'Installer\Output'

$publishTargets = @{
    x64 = (Join-Path $PSScriptRoot "publish\v$Version\x64")
    x86 = (Join-Path $PSScriptRoot "publish\v$Version\x86")
}

foreach ($architecture in $publishTargets.Keys) {
    $publishPath = $publishTargets[$architecture]
    $appPath = Join-Path $publishPath 'ADB Connect.exe'
    if (-not (Test-Path -LiteralPath $appPath)) {
        throw "Published application not found: $appPath"
    }

    $signature = Get-AuthenticodeSignature -LiteralPath $appPath
    if (-not $AllowUnsigned -and $signature.Status -ne 'Valid') {
        throw "The published $architecture application is not signed with a valid certificate."
    }
}

New-Item -ItemType Directory -Path $releasePath -Force | Out-Null
# Preserve unrelated files and previous evidence; replace only named release assets.

foreach ($architecture in @('x64', 'x86')) {
    $zipPath = Join-Path $releasePath "ADB-Connect-$Version-$architecture-Portable.zip"
    Compress-Archive -Path (Join-Path $publishTargets[$architecture] '*') -DestinationPath $zipPath -CompressionLevel Optimal -Force

    $installer = Join-Path $installerPath "ADB-Connect-$Version-$architecture-Setup.exe"
    if (-not (Test-Path -LiteralPath $installer)) {
        throw "Installer not found: $installer"
    }

    $installerSignature = Get-AuthenticodeSignature -LiteralPath $installer
    if (-not $AllowUnsigned -and $installerSignature.Status -ne 'Valid') {
        throw "The $architecture installer is not signed with a valid certificate."
    }

    Copy-Item -LiteralPath $installer -Destination $releasePath -Force
}

& (Join-Path $PSScriptRoot 'Download-Release-Sources.ps1') -ReleaseDirectory $releasePath
if (-not (Test-Path -LiteralPath (Join-Path $releasePath "ADB-Connect-$Version-Source.zip"))) {
    & (Join-Path $PSScriptRoot 'Create-Source-Package.ps1') -Version $Version
}
& (Join-Path $PSScriptRoot 'Create-SHA256SUMS.ps1') -ReleaseDirectory $releasePath
if ($AllowUnsigned) {
    Write-Warning 'The release contains unsigned executables. Windows will display Unknown Publisher.'
}
Write-Host "Release packages are ready in: $releasePath"

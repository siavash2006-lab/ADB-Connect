[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$CertificateThumbprint,

    [Parameter(Mandatory = $true)]
    [string]$TimestampUrl,

    [string]$SignToolPath
)

$ErrorActionPreference = 'Stop'
$thumbprint = $CertificateThumbprint -replace '\s', ''

if (-not $SignToolPath) {
    $windowsKitsBin = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
    $SignToolPath = Get-ChildItem -Path $windowsKitsBin -Filter signtool.exe -File -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\signtool\.exe$' } |
        Sort-Object FullName -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}

if (-not $SignToolPath -or -not (Test-Path -LiteralPath $SignToolPath)) {
    throw 'signtool.exe was not found. Install the Windows SDK or pass -SignToolPath.'
}

$version = ([xml](Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ADB Connect.csproj') -Raw)).Project.PropertyGroup.Version | Select-Object -First 1
$targets = @(
    (Join-Path $PSScriptRoot "publish\v$version\x64\ADB Connect.exe"),
    (Join-Path $PSScriptRoot "publish\v$version\x86\ADB Connect.exe")
)

foreach ($target in $targets) {
    if (-not (Test-Path -LiteralPath $target)) {
        throw "Published executable not found: $target"
    }

    & $SignToolPath sign /sha1 $thumbprint /fd SHA256 /td SHA256 /tr $TimestampUrl /d 'ADB Connect' $target
    if ($LASTEXITCODE -ne 0) {
        throw "Signing failed: $target"
    }

    & $SignToolPath verify /pa /all /v $target
    if ($LASTEXITCODE -ne 0) {
        throw "Signature verification failed: $target"
    }
}

Write-Host 'Published x64 and x86 executables were signed and verified.'

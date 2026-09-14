[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$dotnetCommand = Get-Command dotnet.exe -ErrorAction Stop
$dotnetRoot = Split-Path -Parent $dotnetCommand.Source
$licenseDirectory = Join-Path $PSScriptRoot 'LICENSES'

$licenseSource = @(
    (Join-Path $dotnetRoot 'LICENSE.txt'),
    (Join-Path $dotnetRoot 'LICENSE.TXT')
) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1

$noticesSource = @(
    (Join-Path $dotnetRoot 'ThirdPartyNotices.txt'),
    (Join-Path $dotnetRoot 'THIRD-PARTY-NOTICES.TXT')
) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1

if (-not $licenseSource) {
    throw "The .NET license file was not found under '$dotnetRoot'."
}

if (-not $noticesSource) {
    throw "The .NET third-party notices file was not found under '$dotnetRoot'."
}

New-Item -ItemType Directory -Path $licenseDirectory -Force | Out-Null
Copy-Item -LiteralPath $licenseSource -Destination (Join-Path $licenseDirectory 'dotnet-MIT.txt') -Force
Copy-Item -LiteralPath $noticesSource -Destination (Join-Path $licenseDirectory 'dotnet-THIRD-PARTY-NOTICES.txt') -Force

Write-Host "Copied .NET license files from: $dotnetRoot"

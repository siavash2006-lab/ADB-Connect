[CmdletBinding()]
param(
    [string]$ChecksumFile
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ChecksumFile)) {
    $ChecksumFile = Join-Path $PSScriptRoot 'release\v1.6.5\SHA256SUMS.txt'
}

$checksumPath = [System.IO.Path]::GetFullPath($ChecksumFile)
$releasePath = Split-Path -Parent $checksumPath

if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
    throw "Checksum file not found: $checksumPath"
}

$failed = $false
$results = foreach ($line in Get-Content -LiteralPath $checksumPath) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    if ($line -notmatch '^([0-9a-fA-F]{64}) \*(.+)$') {
        throw "Invalid checksum line: $line"
    }

    $expected = $matches[1].ToLowerInvariant()
    $name = $matches[2]
    $path = Join-Path $releasePath $name
    $actual = if (Test-Path -LiteralPath $path -PathType Leaf) {
        (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    else {
        'missing'
    }

    $valid = $actual -eq $expected
    if (-not $valid) {
        $failed = $true
    }

    [pscustomobject]@{
        File = $name
        Valid = $valid
        Actual = $actual
    }
}

$results | Format-Table -AutoSize
if ($failed) {
    exit 1
}

Write-Host 'All SHA-256 checksums are valid.'

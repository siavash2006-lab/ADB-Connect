[CmdletBinding()]
param(
    [string]$ReleaseDirectory = (Join-Path $PSScriptRoot 'release\v1.6.6')
)

$ErrorActionPreference = 'Stop'
$releasePath = [System.IO.Path]::GetFullPath($ReleaseDirectory)

if (-not (Test-Path -LiteralPath $releasePath -PathType Container)) {
    throw "Release directory not found: $releasePath"
}

$files = Get-ChildItem -LiteralPath $releasePath -File |
    Where-Object { $_.Name -ne 'SHA256SUMS.txt' } |
    Sort-Object Name

if (-not $files) {
    throw "No release files were found in: $releasePath"
}

$lines = foreach ($file in $files) {
    $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash *$($file.Name)"
}

$outputPath = Join-Path $releasePath 'SHA256SUMS.txt'
[System.IO.File]::WriteAllLines($outputPath, $lines, [System.Text.Encoding]::ASCII)
Write-Host "Created: $outputPath"

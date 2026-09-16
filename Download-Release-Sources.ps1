[CmdletBinding()]
param(
    [string]$ReleaseDirectory = (Join-Path $PSScriptRoot 'release\v1.6.6')
)

$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path $ReleaseDirectory -Force | Out-Null

$sources = @(
    @{
        Name = 'ffmpeg-8.1.1.tar.xz'
        Url = 'https://ffmpeg.org/releases/ffmpeg-8.1.1.tar.xz'
        Sha256 = 'b6863adde98898f42602017462871b5f6333e65aec803fdd7a6308639c52edf3'
    },
    @{
        Name = 'ffmpeg-7.1.1.tar.xz'
        Url = 'https://ffmpeg.org/releases/ffmpeg-7.1.1.tar.xz'
        Sha256 = '733984395e0dbbe5c046abda2dc49a5544e7e0e1e2366bba849222ae9e3a03b1'
    }
)

foreach ($source in $sources) {
    $destination = Join-Path $ReleaseDirectory $source.Name
    if ((Test-Path -LiteralPath $destination) -and
        (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash.ToLowerInvariant() -eq $source.Sha256) {
        Write-Host "Already verified: $($source.Name)"
        continue
    }
    $cached = Join-Path $PSScriptRoot "release\v1.6.4\$($source.Name)"
    if ((Test-Path -LiteralPath $cached) -and
        (Get-FileHash -LiteralPath $cached -Algorithm SHA256).Hash.ToLowerInvariant() -eq $source.Sha256) {
        Copy-Item -LiteralPath $cached -Destination $destination -Force
        Write-Host "Copied verified source archive: $($source.Name)"
        continue
    }
    Invoke-WebRequest -Uri $source.Url -OutFile $destination -UseBasicParsing

    $actualHash = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -ne $source.Sha256) {
        Remove-Item -LiteralPath $destination -Force
        throw "SHA-256 verification failed for $($source.Name)."
    }

    Write-Host "Downloaded and verified: $($source.Name)"
}

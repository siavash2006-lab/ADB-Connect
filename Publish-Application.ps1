[CmdletBinding()]
param([Parameter(Mandatory=$true)][ValidateSet('x64','x86')][string]$Architecture)
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'ADB Connect.csproj'
$version = ([xml](Get-Content -LiteralPath $project -Raw)).Project.PropertyGroup.Version | Select-Object -First 1
$output = Join-Path $PSScriptRoot "publish\v$version\$Architecture"
& (Join-Path $PSScriptRoot 'Prepare-DotNet-Notices.ps1')
& dotnet publish $project -c Release -r "win-$Architecture" --self-contained true -p:PublishSingleFile=true -o $output
if ($LASTEXITCODE -ne 0) { throw "Publish failed: $Architecture" }
foreach ($relative in @('ADB Connect.exe','platform-tools\adb.exe','platform-tools\AdbWinApi.dll','platform-tools\AdbWinUsbApi.dll','platform-tools\NOTICE.txt','scrcpy\scrcpy.exe','scrcpy\scrcpy-server','scrcpy\compat\scrcpy.exe','scrcpy\compat\scrcpy-server','LICENSE','THIRD_PARTY_NOTICES.md','LICENSES\dotnet-THIRD-PARTY-NOTICES.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $output $relative) -PathType Leaf)) { throw "Missing published dependency: $relative" }
}
$actual = (Get-Item -LiteralPath (Join-Path $output 'ADB Connect.exe')).VersionInfo.FileVersion
if ($actual -ne "$version.0") { throw "Unexpected executable version: $actual" }
Write-Host "Published $version $Architecture to $output. Previous version folders were preserved."

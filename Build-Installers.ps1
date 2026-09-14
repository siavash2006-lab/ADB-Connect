[CmdletBinding()]
param(
    [string]$InnoCompilerPath,
    [switch]$RequireSigned,
    [string]$SignCommand
)

$ErrorActionPreference = 'Stop'

if (-not $InnoCompilerPath) {
    $candidates = @(
        (Join-Path $env:ProgramFiles 'Inno Setup 7\ISCC.exe'),
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 7\ISCC.exe'),
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
    )
    $InnoCompilerPath = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if (-not $InnoCompilerPath -or -not (Test-Path -LiteralPath $InnoCompilerPath)) {
    throw 'ISCC.exe was not found. Install Inno Setup 6/7 or pass -InnoCompilerPath.'
}

$scripts = @(
    (Join-Path $PSScriptRoot 'Installer\ADB-Connect-x64.iss'),
    (Join-Path $PSScriptRoot 'Installer\ADB-Connect-x86.iss')
)

if ($RequireSigned -and -not $SignCommand) {
    throw 'Pass -SignCommand for the PersonalCodeSign tool when using -RequireSigned.'
}

foreach ($script in $scripts) {
    $compilerArgs = @()
    if ($RequireSigned) { $compilerArgs += '/DSignedBuild'; $compilerArgs += "/SPersonalCodeSign=$SignCommand" }
    $compilerArgs += $script
    & $InnoCompilerPath @compilerArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Installer compilation failed: $script"
    }
}

$version = ([xml](Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ADB Connect.csproj') -Raw)).Project.PropertyGroup.Version | Select-Object -First 1
foreach ($architecture in @('x64', 'x86')) {
    $output = Join-Path $PSScriptRoot "Installer\Output\ADB-Connect-$version-$architecture-Setup.exe"
    $signature = Get-AuthenticodeSignature -LiteralPath $output
    if ($RequireSigned -and $signature.Status -ne 'Valid') { throw "Invalid installer signature: $output" }
    Write-Host "$architecture installer: $output (signature: $($signature.Status))"
}
if (-not $RequireSigned) { Write-Warning 'Local unsigned build. Use -RequireSigned with -SignCommand for signed release installers.' }

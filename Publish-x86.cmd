@echo off
setlocal
pushd "%~dp0"

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Prepare-DotNet-Notices.ps1"
if errorlevel 1 goto :fail

if exist "publish\x86" rmdir /s /q "publish\x86"
dotnet publish "ADB Connect.csproj" -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -o "publish\x86"
if errorlevel 1 goto :fail

call :require "publish\x86\ADB Connect.exe" "ADB Connect executable"
if errorlevel 1 goto :fail
call :require "publish\x86\platform-tools\adb.exe" "ADB executable"
if errorlevel 1 goto :fail
call :require "publish\x86\platform-tools\NOTICE.txt" "Android Platform-Tools notice"
if errorlevel 1 goto :fail
call :require "publish\x86\scrcpy\scrcpy.exe" "primary scrcpy runtime"
if errorlevel 1 goto :fail
call :require "publish\x86\scrcpy\compat\scrcpy.exe" "scrcpy 3.3.4 compatibility runtime"
if errorlevel 1 goto :fail
call :require "publish\x86\LICENSE" "ADB Connect license"
if errorlevel 1 goto :fail
call :require "publish\x86\THIRD_PARTY_NOTICES.md" "third-party notices"
if errorlevel 1 goto :fail
call :require "publish\x86\LICENSES\dotnet-THIRD-PARTY-NOTICES.txt" ".NET third-party notices"
if errorlevel 1 goto :fail

echo x86 publish completed successfully.
popd
exit /b 0

:require
if exist "%~1" exit /b 0
echo ERROR: Missing %~2: %~1
exit /b 1

:fail
set "EXIT_CODE=%errorlevel%"
if "%EXIT_CODE%"=="0" set "EXIT_CODE=1"
popd
exit /b %EXIT_CODE%

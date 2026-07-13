@echo off
setlocal
dotnet publish "ADB Connect.csproj" -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -o "publish\x86"
if errorlevel 1 exit /b %errorlevel%
if not exist "publish\x86\scrcpy\scrcpy.exe" (
  echo ERROR: x86 primary scrcpy runtime was not included. See scrcpy\README.md.
  exit /b 1
)
if not exist "publish\x86\scrcpy\compat\scrcpy.exe" (
  echo ERROR: x86 scrcpy 3.x compatibility runtime was not included. See scrcpy\README.md.
  exit /b 1
)
echo x86 publish completed successfully.

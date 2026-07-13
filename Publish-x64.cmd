@echo off
setlocal
dotnet publish "ADB Connect.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "publish\x64"
if errorlevel 1 exit /b %errorlevel%
if not exist "publish\x64\scrcpy\scrcpy.exe" (
  echo ERROR: x64 primary scrcpy runtime was not included. See scrcpy\README.md.
  exit /b 1
)
if not exist "publish\x64\scrcpy\compat\scrcpy.exe" (
  echo ERROR: x64 scrcpy 3.x compatibility runtime was not included. See scrcpy\README.md.
  exit /b 1
)
echo x64 publish completed successfully.

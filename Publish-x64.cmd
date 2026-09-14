@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Publish-Application.ps1" -Architecture x64
exit /b %errorlevel%

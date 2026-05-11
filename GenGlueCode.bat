@echo off
setlocal
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -Command "[System.Management.Automation.PSLanguageMode]::FullLanguage; & '%~dp0BuildScript\build.ps1' -CodeGenOnly %*"
exit /b %ERRORLEVEL%
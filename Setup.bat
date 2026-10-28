@echo off
rem Titan Windows Setup: deps check + restore, build codegen tools, glue generation (configure).
rem Full build: BuildEngine.bat. This script only prepares the repo so VS can build directly.
rem Extra args are forwarded to BuildScript\build.ps1 (e.g. -Configuration Release -SkipRestore).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0BuildScript\build.ps1" -CodeGenOnly %*
exit /b %ERRORLEVEL%

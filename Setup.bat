@echo off
rem Titan Windows Setup：依赖校验+restore、代码生成工具构建、胶水生成（configure 语义）
rem 全量构建用 BuildEngine.bat；本脚本只把仓库准备到「可直接打开 VS 构建」的状态。
rem 额外参数原样透传给 BuildScript\build.ps1（如 -Configuration Release -SkipRestore）。
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0BuildScript\build.ps1" -CodeGenOnly %*
exit /b %ERRORLEVEL%

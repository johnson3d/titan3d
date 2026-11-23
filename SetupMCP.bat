@echo off
rem Titan MCP setup: verifies that the committed .mcp.json can run on this machine.
rem   SetupMCP.bat                        verify only, change nothing
rem   SetupMCP.bat -InstallPythonPackages  also pip install Plugins\AIGC\MCPServer\Python\requirements.txt
rem   SetupMCP.bat -WriteUserOverride      also merge absolute-path entries into %USERPROFILE%\.qoder\mcp.json
rem Restart Qoder IDE afterwards: the project .mcp.json is only read at IDE start.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0BuildScript\mcp\Setup-MCPServers.ps1" %*
exit /b %ERRORLEVEL%

rem Keep build temp inside the repo. Security software (e.g. Tencent PC Manager) blocks
rem freshly built unsigned exes from creating directories under the system %TEMP%, which makes
rem the in-process MSBuild hosted by GameBuilder.exe fail with
rem "Access to the path ...\MSBuildTemp*.xxx is denied".
set "TITAN_BUILD_TEMP=%~dp0binaries\temp"
if not exist "%TITAN_BUILD_TEMP%" mkdir "%TITAN_BUILD_TEMP%"
set "TEMP=%TITAN_BUILD_TEMP%"
set "TMP=%TITAN_BUILD_TEMP%"

cd %~dp0\binaries
%~dp0\binaries\net10.0\GameBuilder.exe %~dp0\ %~dp0\Module\GameProject\GameProject.csproj %~dp0\content net10.0 %~dp0\content\config\engineconfig_windows_alonegame.jscfg
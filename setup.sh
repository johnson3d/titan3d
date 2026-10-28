#!/usr/bin/env bash
# Titan 统一 Setup 入口（UE 风格，Setup.bat / BuildEngine.bat 的跨平台对齐物）：
#   macOS  -> Core.Mac/setup.sh（依赖校验 -> codegen -> configure -> build）
#   Windows -> BuildScript/build.ps1 -CodeGenOnly（依赖校验+restore -> 工具 -> 胶水生成）
#             （在 Git Bash / MSYS2 中运行；cmd 用户请直接用 Setup.bat）
# Windows 开关:
#   --check-only   只跑 BuildScript/check/deps.ps1 依赖校验（不 restore 不生成）
#   其余参数       原样透传给 build.ps1（如 -Configuration Release -SkipRestore）
set -euo pipefail
cd "$(dirname "$0")"

UNAME="$(uname -s)"
case "$UNAME" in
  Darwin)
    exec Core.Mac/setup.sh "$@"
    ;;
  MINGW*|MSYS*|CYGWIN*|*_NT-*)
    POWERSHELL=powershell
    command -v powershell >/dev/null 2>&1 || POWERSHELL="/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"
    if [ "${1:-}" = "--check-only" ]; then
      exec "$POWERSHELL" -NoProfile -ExecutionPolicy Bypass -File BuildScript/check/deps.ps1
    fi
    echo "[TitanSetup] Windows configure: deps 校验+restore -> 代码生成工具 -> 胶水生成（全量构建请用 BuildEngine.bat）"
    exec "$POWERSHELL" -NoProfile -ExecutionPolicy Bypass -File BuildScript/build.ps1 -CodeGenOnly "$@"
    ;;
  *)
    echo "[TitanSetup] 当前平台 $UNAME 不支持本脚本。"
    exit 1
    ;;
esac

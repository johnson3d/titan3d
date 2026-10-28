#!/usr/bin/env bash
# macOS 冒烟：TitanCMD + RHI_VirtualDevice 执行 ExeCmd=MakeSln（无窗口、无 GPU）
# 用法（仓库根）: Core.Mac/smoke_titancmd.sh [超时秒数，默认 300]
# 前置: 按 Core.Mac/VALIDATION.md 完成环境门禁、codegen 校验和原生/托管构建；
#       构建命令以该文档“C：托管构建与冒烟”为准。
#       （PreInitEngine 要求 DataCopyer 插件，缺失时直接失败）
set -u
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
TIMEOUT="${1:-300}"
LOG="$ROOT/cache/mac_titancmd_smoke.log"
CFG="$ROOT/content/config/engineconfig_mac_cmd.jscfg"

command -v perl >/dev/null 2>&1 || { echo "[TitanSmoke] FAIL perl not found"; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo "[TitanSmoke] FAIL dotnet not found"; exit 1; }
case "$TIMEOUT" in
  ''|0|0*|*[!0-9]*) echo "[TitanSmoke] FAIL invalid timeout: $TIMEOUT"; exit 1 ;;
esac
if ! [ "$TIMEOUT" -le 86400 ] 2>/dev/null; then
  echo "[TitanSmoke] FAIL invalid timeout: $TIMEOUT"
  exit 1
fi

# Program.cs 在 engine.jscfg 缺失时直接写文件，目录不存在会抛异常
mkdir -p "$ROOT/cache/config"

for f in "$ROOT/binaries/net8.0/TitanCMD.dll" "$ROOT/binaries/debug/Core.Mac.dylib" "$CFG" \
         "$ROOT/binaries/Plugins/DataCopyer.plugin" "$ROOT/binaries/Plugins/DataCopyer/DataCopyer.All.dll"; do
  if [ ! -f "$f" ]; then
    echo "[TitanSmoke] FAIL missing $f"
    exit 1
  fi
done

cd "$ROOT/binaries"
# macOS 没有 coreutils timeout，用 perl alarm；超时退出码 142（128+SIGALRM）
perl -e 'alarm shift; exec @ARGV' "$TIMEOUT" \
  dotnet net8.0/TitanCMD.dll ExeCmd=MakeSln "CookCfg=$CFG" NativeDLL=debug >"$LOG" 2>&1
RC=$?
tail -n 40 "$LOG"

if [ "$RC" -eq 142 ]; then
  echo "[TitanSmoke] FAIL timeout ${TIMEOUT}s (log: $LOG)"
  exit 1
fi
if [ "$RC" -ne 0 ]; then
  echo "[TitanSmoke] FAIL exit=$RC (log: $LOG)"
  exit 1
fi
if ! grep -q "Hello Titan" "$LOG"; then
  echo "[TitanSmoke] FAIL no 'Hello Titan' in log (log: $LOG)"
  exit 1
fi
echo "[TitanSmoke] PASS (log: $LOG)"
exit 0

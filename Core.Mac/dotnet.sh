#!/usr/bin/env bash
# VS Code 任务 / 手动命令的 dotnet 入口：与 ./setup.sh 共用同一套 SDK 解析规则
#   - 优先使用 PATH 上满足 global.json 主版本 SDK + osx-arm64 RID 的 dotnet；
#   - 否则使用仓库内 3rd/tools/dotnet-sdk（setup 会自动安装）；
#   - 都没有时自动安装，并只在本次进程注入 DOTNET_ROOT/PATH。
# 用法与 dotnet 完全一致：Core.Mac/dotnet.sh build Module/Engine.Mac/Engine.Mac.csproj ...
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=Core.Mac/ensure_dotnet.sh
. "$ROOT/Core.Mac/ensure_dotnet.sh"

# 解析出的 SDK 信息（“使用系统/仓库内 SDK”）走 stderr，避免污染 build 输出
titan_ensure_dotnet >&2
dotnet_exe="$(command -v dotnet)"

# 会触发 NuGet 还原的常规命令走进度包装器：下载期间输出包名/大小/速度，不再假死
case "${1:-}" in
  build|restore|publish|run|test|pack|msbuild)
    exec "$ROOT/BuildScript/common/progress-run.sh" "dotnet $1" 10 "$dotnet_exe" "$@"
    ;;
esac
exec "$dotnet_exe" "$@"

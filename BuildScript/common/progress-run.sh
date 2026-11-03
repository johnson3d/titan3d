#!/usr/bin/env bash
# 长任务进度包装器（给 dotnet/NuGet 这类“下载时完全静默”的命令用）：
#   - 若是 restore/build/publish 等命令，会扫描 NuGet 的 http-cache 里正在下载的
#     `nupkg_<id>.<version>.dat-new` 文件，按包输出已下载大小与整体增量；
#   - 没有下载活动时退回每 N 秒一行的耗时心跳；
#   - 子命令 stdout/stderr 与退出码原样透传。
#
# 用法: progress-run.sh <标签> <心跳秒数> <命令> [参数...]
# 例:   progress-run.sh "dotnet build" 10 dotnet build Foo.csproj
set -u

if [ "$#" -lt 3 ]; then
  echo "用法: $0 <标签> <心跳秒数> <命令> [参数...]" >&2
  exit 2
fi

label="$1"
interval="$2"
shift 2
case "$interval" in
  ''|*[!0-9]*) interval=10 ;;
esac
[ "$interval" -gt 0 ] || interval=10

# 默认与 dotnet/NuGet 的缓存位置一致；可用标准环境变量覆盖
NUGET_HTTP_CACHE="${NUGET_HTTP_CACHE_PATH:-${XDG_DATA_HOME:-$HOME/.local/share}/NuGet/http-cache}"

# 只有 dotnet restore/build/... 才扫 NuGet 缓存，避免执行工具 dll 时误报
_scan_nuget=0
for _arg in "$@"; do
  case "$_arg" in
    restore|build|publish|run|test|pack|msbuild) _scan_nuget=1 ;;
  esac
done

file_size() {
  stat -f%z "$1" 2>/dev/null || stat -c%s "$1" 2>/dev/null || echo 0
}

human_mb() {
  awk -v b="$1" 'BEGIN { printf "%.1f", b / 1048576 }'
}

start=$SECONDS
prev_active_bytes=0
"$@" &
pid=$!
rc=0
while kill -0 "$pid" 2>/dev/null; do
  sleep "$interval"
  kill -0 "$pid" 2>/dev/null || break
  elapsed=$((SECONDS - start))

  if [ "$_scan_nuget" -eq 1 ] && [ -d "$NUGET_HTTP_CACHE" ]; then
    active_bytes=0
    active_count=0
    while IFS= read -r cache_file; do
      [ -n "$cache_file" ] || continue
      base="${cache_file##*/}"
      pkg="${base#nupkg_}"
      pkg="${pkg%.dat-new}"
      size="$(file_size "$cache_file")"
      active_count=$((active_count + 1))
      active_bytes=$((active_bytes + size))
      echo "[NuGetProgress] 下载 ${pkg}: $(human_mb "$size") MB（已运行 ${elapsed}s）" >&2
    done < <(find "$NUGET_HTTP_CACHE" -name '*.dat-new' -mmin -1 2>/dev/null)
    if [ "$active_count" -gt 0 ]; then
      delta=$((active_bytes - prev_active_bytes))
      [ "$delta" -lt 0 ] && delta=0
      avg=0
      [ "$interval" -gt 0 ] && avg=$((delta / interval))
      echo "[NuGetProgress] 正在下载 ${active_count} 个包，累计 $(human_mb "$active_bytes") MB；最近 ${interval}s +$(human_mb "$delta") MB（约 $(human_mb "$avg") MB/s）" >&2
      prev_active_bytes=$active_bytes
    else
      prev_active_bytes=0
      echo "[TitanProgress] ${label}… 已运行 ${elapsed}s（当前无 NuGet 下载，正在还原索引/编译）" >&2
    fi
  else
    echo "[TitanProgress] ${label}… 已运行 ${elapsed}s（无输出属正常，正在执行）" >&2
  fi
done
wait "$pid" || rc=$?
exit "$rc"

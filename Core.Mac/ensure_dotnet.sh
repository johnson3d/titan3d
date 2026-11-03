#!/usr/bin/env bash
# Titan 的 .NET SDK 引导器（macOS）；版本与主版本门禁一律取自 global.json。
#
# 优先复用 PATH 上满足「global.json 主版本 SDK + 本机 RID」的 dotnet；缺失或不合格时，用官方
# dotnet-install.sh 把 global.json 指定的版本安装到仓库内 3rd/tools/dotnet-sdk
# （无需 sudo、不改 shell 配置），并且只在当前进程内注入 DOTNET_ROOT/PATH。
#
# 被 Core.Mac/setup.sh source 时露出 titan_ensure_dotnet；也可独立运行：
#   Core.Mac/ensure_dotnet.sh --check   只校验，不下载（setup.sh --check-only 用）
#   Core.Mac/ensure_dotnet.sh           缺失时安装
#   Core.Mac/ensure_dotnet.sh --force   忽略已有安装，重新安装仓库内 SDK
#
# 环境变量：TITAN_DOTNET_DIR 可覆盖安装目录（默认 <repo>/3rd/tools/dotnet-sdk）。
# 版本/RID 门禁与 Core.Mac/VALIDATION.md「环境准备」一致：global.json 主版本 SDK + osx-arm64。
#
# 下载前必须经用户 y/N 确认（默认 N，拒绝则只给手动安装指引）；
# 建议：尽量在本机安装官方 SDK，仓库内下载是退路。
# 自动化场景可设 TITAN_DOTNET_AUTO_INSTALL=1 跳过提问；无 tty 且未设该变量时直接失败。

TITAN_DOTNET_REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TITAN_DOTNET_DIR="${TITAN_DOTNET_DIR:-$TITAN_DOTNET_REPO_ROOT/3rd/tools/dotnet-sdk}"
TITAN_DOTNET_INSTALL_SCRIPT="$TITAN_DOTNET_REPO_ROOT/cache/dotnet-install.sh"
TITAN_DOTNET_INSTALL_URL="https://dot.net/v1/dotnet-install.sh"

# global.json 锁定的 SDK 版本（如 10.0.100）；静默失败——
# 错误由 titan_ensure_dotnet 入口在父进程一次性打印，避免 $( ) 子 shell 内重复输出
titan_dotnet_version() {
  local version=""
  if [ -f "$TITAN_DOTNET_REPO_ROOT/global.json" ]; then
    version="$(sed -n 's/^[[:space:]]*"version"[[:space:]]*:[[:space:]]*"\([0-9][0-9.]*\)".*$/\1/p' \
      "$TITAN_DOTNET_REPO_ROOT/global.json" | head -n 1)"
  fi
  [ -n "$version" ] || return 1
  printf '%s\n' "$version"
}

# global.json 锁定的 SDK 主版本（如 10.0.100 → 10）；供门禁与文案使用，不写死数字
titan_dotnet_major() {
  local version
  version="$(titan_dotnet_version)" || return 1
  printf '%s\n' "${version%%.*}"
}

# dotnet-install.sh 的 --architecture 参数
titan_dotnet_arch() {
  case "$(uname -m)" in
    arm64) echo "arm64" ;;
    *) echo "x64" ;;
  esac
}

# dotnet --info 里的宿主 RID 门禁；Core.Mac 首版要求 osx-arm64
titan_dotnet_rid() {
  case "$(uname -m)" in
    arm64) echo "osx-arm64" ;;
    *) echo "osx-x64" ;;
  esac
}

# 可用 = 能列出 global.json 主版本的 SDK，且宿主 RID 与当前机器一致（排除 Rosetta 下的 x64 SDK）
titan_dotnet_usable() {
  local exe="$1" rid sdks info
  [ -n "$exe" ] || return 1
  if [ ! -x "$exe" ] && ! command -v "$exe" >/dev/null 2>&1; then
    return 1
  fi
  # 用 here-string 而非管道：setup.sh 的 pipefail 下不能因 grep -q 提前退出而误判
  sdks="$("$exe" --list-sdks 2>/dev/null || true)"
  local major
  major="$(titan_dotnet_major)" || return 1
  grep -q "^${major}\." <<<"$sdks" || return 1
  rid="$(titan_dotnet_rid)"
  info="$("$exe" --info 2>/dev/null || true)"
  grep -Eq "^[[:space:]]*RID:[[:space:]]+${rid}([[:space:]]|$)" <<<"$info" || return 1
  return 0
}

# 优先系统 dotnet，其次仓库内安装；返回可执行文件绝对路径
titan_dotnet_find() {
  local exe candidate
  exe="$(command -v dotnet 2>/dev/null || true)"
  if [ -n "$exe" ] && titan_dotnet_usable "$exe"; then
    printf '%s\n' "$exe"
    return 0
  fi
  candidate="$TITAN_DOTNET_DIR/dotnet"
  if [ -x "$candidate" ] && titan_dotnet_usable "$candidate"; then
    printf '%s\n' "$candidate"
    return 0
  fi
  return 1
}

# 仅当使用仓库内 SDK 时注入环境变量；重复调用是幂等的
titan_dotnet_use_local() {
  export DOTNET_ROOT="$TITAN_DOTNET_DIR"
  case ":${PATH}:" in
    *":$TITAN_DOTNET_DIR:"*) ;;
    *) export PATH="$TITAN_DOTNET_DIR:$PATH" ;;
  esac
}

titan_dotnet_install() {
  local version arch
  version="$(titan_dotnet_version)" || return 1
  arch="$(titan_dotnet_arch)"

  mkdir -p "$TITAN_DOTNET_DIR" "$(dirname "$TITAN_DOTNET_INSTALL_SCRIPT")"
  if [ ! -x "$TITAN_DOTNET_INSTALL_SCRIPT" ]; then
    echo "[TitanDotnet] 下载官方安装脚本: $TITAN_DOTNET_INSTALL_URL"
    if ! curl -fsSL --retry 3 --connect-timeout 20 \
      -o "$TITAN_DOTNET_INSTALL_SCRIPT" "$TITAN_DOTNET_INSTALL_URL"; then
      echo "[TitanDotnet][错误] 下载 dotnet-install.sh 失败（需要网络）" >&2
      return 1
    fi
    chmod +x "$TITAN_DOTNET_INSTALL_SCRIPT"
  fi

  echo "[TitanDotnet] 安装 .NET SDK ${version}（${arch}）到 ${TITAN_DOTNET_DIR}（无需 sudo，首次约 200MB）"
  if ! bash "$TITAN_DOTNET_INSTALL_SCRIPT" \
    --version "$version" \
    --architecture "$arch" \
    --install-dir "$TITAN_DOTNET_DIR" \
    --no-path; then
    echo "[TitanDotnet][错误] dotnet-install.sh 执行失败" >&2
    return 1
  fi

  titan_dotnet_use_local
  if ! titan_dotnet_usable "$TITAN_DOTNET_DIR/dotnet"; then
    echo "[TitanDotnet][错误] 安装后校验失败: $TITAN_DOTNET_DIR/dotnet" >&2
    return 1
  fi

  echo "[TitanDotnet] 已安装: $("$TITAN_DOTNET_DIR/dotnet" --version)"
  echo "[TitanDotnet] 手动执行 dotnet 前先: export DOTNET_ROOT=\"$TITAN_DOTNET_DIR\" && export PATH=\"$TITAN_DOTNET_DIR:\$PATH\""
  return 0
}

# 下载前的 y/N 确认（默认 N）：优先建议本机安装官方 SDK，仓库内下载是退路。
# 自动化可设 TITAN_DOTNET_AUTO_INSTALL=1 跳过提问；无 tty 且未设变量时拒绝下载。
titan_dotnet_ask_install() {
  if [ "${TITAN_DOTNET_AUTO_INSTALL:-}" = "1" ]; then
    echo "[TitanDotnet] TITAN_DOTNET_AUTO_INSTALL=1，跳过确认直接安装"
    return 0
  fi
  if ! [ -t 0 ]; then
    echo "[TitanDotnet][缺失] 未找到可用的 .NET $(titan_dotnet_major) SDK，且当前为非交互环境，不会自动下载" >&2
    echo "[TitanDotnet] 建议在本机安装官方 .NET $(titan_dotnet_major) SDK（global.json 锁定 $(titan_dotnet_version)）:" >&2
    echo "[TitanDotnet]   brew install --cask dotnet-sdk  或  https://dotnet.microsoft.com/download/dotnet/$(titan_dotnet_major).0" >&2
    echo "[TitanDotnet] 确需下载到仓库内时: 设置 TITAN_DOTNET_AUTO_INSTALL=1 后重跑" >&2
    return 1
  fi
  echo "[TitanDotnet] 未找到可用的 .NET $(titan_dotnet_major) SDK（版本 $(titan_dotnet_version)，RID $(titan_dotnet_rid)）"
  echo "[TitanDotnet] 建议尽量在本机安装官方 .NET $(titan_dotnet_major) SDK（全局可用、无需每次注入 DOTNET_ROOT）:"
  echo "[TitanDotnet]   brew install --cask dotnet-sdk  或  https://dotnet.microsoft.com/download/dotnet/$(titan_dotnet_major).0"
  echo "[TitanDotnet] 若暂不安装，可由本脚本下载官方 SDK 到仓库内 ${TITAN_DOTNET_DIR}（约 200MB，仅本仓库可用）"
  local answer
  read -r -p "[TitanDotnet] 是否允许 setup 下载到仓库内? [y/N] " answer
  case "$answer" in
    [yY]|[yY][eE][sS]) return 0 ;;
    *)
      echo "[TitanDotnet] 已取消下载。请先在本机安装 .NET $(titan_dotnet_major) SDK 后重跑 ./setup.sh" >&2
      return 1
      ;;
  esac
}

# 用法：titan_ensure_dotnet [--check|--force]
# 成功时保证当前进程的 PATH 上 dotnet 可用（系统优先，其次仓库内安装）。
titan_ensure_dotnet() {
  local check_only=0 force=0 exe
  # 入口一次性校验 global.json（在父进程直接调用，非 $( ) 子 shell），失败即止
  if ! titan_dotnet_version >/dev/null; then
    echo "[TitanDotnet][错误] 无法从 global.json 读取 SDK 版本（文件缺失或 version 字段非法）" >&2
    return 1
  fi
  while [ $# -gt 0 ]; do
    case "$1" in
      --check) check_only=1; shift ;;
      --force) force=1; shift ;;
      -h|--help)
        echo "用法: Core.Mac/ensure_dotnet.sh [--check|--force]"
        echo "  --check  只校验 SDK（版本取自 global.json，不下载）"
        echo "  --force  忽略已有安装，重新安装仓库内 SDK（3rd/tools/dotnet-sdk）"
        return 0
        ;;
      *)
        echo "[TitanDotnet][错误] 未知参数: $1" >&2
        return 2
        ;;
    esac
  done

  if [ "$force" -eq 0 ]; then
    if exe="$(titan_dotnet_find)"; then
      case "$exe" in
        "$TITAN_DOTNET_DIR"/*)
          titan_dotnet_use_local
          echo "[TitanDotnet] 使用仓库内 SDK: $exe"
          ;;
        *)
          echo "[TitanDotnet] 使用系统 dotnet: $exe"
          ;;
      esac
      return 0
    fi
  fi

  if [ "$check_only" -eq 1 ]; then
    echo "[TitanDotnet][缺失] 未找到可用的 .NET $(titan_dotnet_major) SDK" >&2
    echo "[TitanDotnet] 要求: SDK $(titan_dotnet_major).x、RID $(titan_dotnet_rid)，版本取自 global.json（$(titan_dotnet_version)）" >&2
    echo "[TitanDotnet] 不带 --check 重跑会询问是否下载到 ${TITAN_DOTNET_DIR}；也可手动安装 https://dotnet.microsoft.com/download/dotnet/$(titan_dotnet_major).0" >&2
    return 1
  fi

  titan_dotnet_ask_install && titan_dotnet_install
}

# 直接运行时执行 CLI；被 setup.sh source 时只提供上面的函数
if [ "${BASH_SOURCE[0]}" = "$0" ]; then
  set -euo pipefail
  titan_ensure_dotnet "$@"
fi

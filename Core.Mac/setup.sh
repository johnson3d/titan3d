#!/usr/bin/env bash
# Titan Mac 一键 Setup（UE 风格统一入口）：
#   依赖校验（工具链 + vendored 原生库）→ codegen → configure → build
# 全新 clone 后在仓库根执行:  ./setup.sh
# 常用开关:
#   --config Debug|Release   目标配置（默认 Debug）
#   --skip-codegen           跳过胶水生成（stamp 缺失时仍会强制执行）
#   --check-only             只做依赖校验，不生成不构建
#   --skip-build             校验+生成+configure，但不执行 build
# 中间产物统一落 intermediate/cpp/Core.Mac/<Config>（与 CMake 工程守卫一致）。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

CFG="Debug"
SKIP_CODEGEN=0
CHECK_ONLY=0
SKIP_BUILD=0
while [ $# -gt 0 ]; do
  case "$1" in
    --config) CFG="${2:?missing value}"; shift 2 ;;
    --skip-codegen) SKIP_CODEGEN=1; shift ;;
    --check-only) CHECK_ONLY=1; shift ;;
    --skip-build) SKIP_BUILD=1; shift ;;
    *) echo "[TitanSetup] 未知参数: $1"; exit 2 ;;
  esac
done
case "$CFG" in
  Debug|Release) ;;
  *) echo "[TitanSetup] --config 只支持 Debug|Release，得到: $CFG"; exit 2 ;;
esac

errors=0
fail() { echo "[TitanSetup][缺失] $1"; errors=$((errors+1)); }
have() { command -v "$1" >/dev/null 2>&1; }

# 静态 libc++ 源码构建（install-mac/lib/libc++.a 缺失时由 setup 自动补齐）。
# 引擎必须用源码 libc++.a 替换系统 libc++（跨堆错配，见 Core.Mac/CMakeLists.txt titan-libcxx 段）；
# 构建参数与 libcxx 22.1.8 standalone 的已验证配置一致，源码保持 sparse + blobless。
LLVM_TAG="llvmorg-22.1.8"
LLVM_DIR="$ROOT/3rd/native/llvm-project"
LIBCXX_PREFIX="$LLVM_DIR/install-mac"
build_libcxx() {
  local bdir="$LLVM_DIR/build-mac/cxx"
  if [ ! -d "$LLVM_DIR/.git" ]; then
    echo "[TitanSetup] 克隆 llvm-project ${LLVM_TAG}（sparse + blobless）"
    git clone --depth 1 --branch "$LLVM_TAG" --filter=blob:none --sparse \
      https://github.com/llvm/llvm-project.git "$LLVM_DIR" || return 1
  elif [ "$(git -C "$LLVM_DIR" describe --tags 2>/dev/null)" != "$LLVM_TAG" ]; then
    echo "[TitanSetup][错误] ${LLVM_DIR} 不是 ${LLVM_TAG}，请手动处理后重跑"
    return 1
  fi
  # libc++ 22.x 的 shared 头与 LLVM-libc 共享源码在 libc/ 下，必须一并 checkout
  git -C "$LLVM_DIR" sparse-checkout set \
    cmake libc/hdr libc/include libc/shared libc/src/__support \
    libcxx libcxxabi libunwind llvm/cmake llvm/include llvm/runtimes runtimes third-party || return 1
  ln -sfn ../../../libc/shared "$LLVM_DIR/libcxx/src/include/shared"

  # 两处构建补丁（幂等）：
  # 1) shared 头按 ../libc 解析；2) refstring 的 rep 块用 malloc/free——系统 libc++abi 的
  #    dtor 用 free 释放，走 ::operator new 会落进引擎 VFX 堆，被系统 free 时 abort
  python3 - "$LLVM_DIR" <<'PY' || return 1
import pathlib
import sys

root = pathlib.Path(sys.argv[1])

p = root / "libcxx/src/CMakeLists.txt"
s = p.read_text()
if "../libc}" not in s:
    for tgt in ("cxx_shared", "cxx_static"):
        old = f"target_include_directories({tgt} PRIVATE ${{CMAKE_CURRENT_SOURCE_DIR}})"
        s = s.replace(old, old[:-1] + " ${CMAKE_CURRENT_SOURCE_DIR}/../libc)")
    p.write_text(s)

p = root / "libcxx/src/include/refstring.h"
s = p.read_text()
if "_LIBCPP_REFSTRING_ALLOC" not in s:
    s = s.replace("#include <cstddef>\n", "#include <cstddef>\n#include <cstdlib>\n", 1)
    macros = (
        "\n// TitanEngine (Mac)：本 dylib 静态链接 libc++.a，而 runtime_error 的析构由系统\n"
        "// libc++abi.dylib 提供，其 dtor 用 free() 释放 rep 块；这里必须用 malloc() 分配，\n"
        "// 否则会落进引擎 VFX 堆并被系统 free 释放（malloc abort）。\n"
        "#define _LIBCPP_REFSTRING_ALLOC(s) std::malloc(s)\n"
        "#define _LIBCPP_REFSTRING_FREE(p) std::free(p)\n"
    )
    s = s.replace("#include <stdexcept>\n", "#include <stdexcept>\n" + macros, 1)
    s = s.replace("::operator new(sizeof(*rep) + len + 1)",
                  "_LIBCPP_REFSTRING_ALLOC(sizeof(*rep) + len + 1)")
    s = s.replace("static_cast<_Rep_base*>(_LIBCPP_REFSTRING_ALLOC(sizeof(*rep) + len + 1));",
                  "static_cast<_Rep_base*>(_LIBCPP_REFSTRING_ALLOC(sizeof(*rep) + len + 1));\n"
                  "  if (rep == nullptr)\n"
                  "    abort();")
    s = s.replace("::operator delete(old_rep)", "_LIBCPP_REFSTRING_FREE(old_rep)")
    s = s.replace("::operator delete(rep)", "_LIBCPP_REFSTRING_FREE(rep)")
    p.write_text(s)
PY

  echo "[TitanSetup] 构建 libc++.a（${LLVM_TAG}，Release/arm64）"
  cmake -S "$LLVM_DIR/libcxx" -B "$bdir" -G Ninja \
    -DCMAKE_BUILD_TYPE=Release \
    -DCMAKE_INSTALL_PREFIX="$LIBCXX_PREFIX" \
    -DCMAKE_OSX_ARCHITECTURES=arm64 \
    -DLIBCXX_CXX_ABI=system-libcxxabi \
    -DLIBCXX_CXX_ABI_INCLUDE_PATHS="$LLVM_DIR/libcxxabi/include" \
    -DLIBCXX_ENABLE_NEW_DELETE_DEFINITIONS=OFF \
    -DLIBCXX_ENABLE_SHARED=OFF \
    -DLIBCXX_ENABLE_STATIC=ON \
    -DLIBCXX_INSTALL_MODULES=OFF || return 1
  cmake --build "$bdir" || return 1
  cmake --install "$bdir" || return 1
  [ -f "$LIBCXX_PREFIX/lib/libc++.a" ]
}

echo "[TitanSetup] === 1/4 环境校验（仓库根: ${ROOT}）==="

# --- 工具链 ---
if [ "$(uname -s)" != "Darwin" ]; then
  fail "本脚本只用于 macOS；Windows 请用 Setup.bat / BuildEngine.bat"
fi
if ! xcode-select -p >/dev/null 2>&1; then
  fail "未安装 Xcode Command Line Tools: 运行 xcode-select --install"
fi
if [ "$(uname -m)" != "arm64" ]; then
  echo "[TitanSetup][警告] 当前非 arm64 宿主（Core.Mac 首版只支持 arm64）"
fi
if ! have cmake; then
  fail "缺少 cmake (>=3.24): brew install cmake"
elif [ "$(printf '%s\n' "$(cmake --version | head -1 | awk '{print $3}')" 3.24 | sort -V | head -1)" != "3.24" ]; then
  fail "cmake 版本过低（需 >=3.24）: $(cmake --version | head -1)"
fi
if ! have ninja; then
  fail "缺少 ninja: brew install ninja"
fi
if ! have dotnet; then
  fail "缺少 dotnet SDK: global.json 要求 8.0.x（https://dotnet.microsoft.com/download/dotnet/8.0）"
elif ! dotnet --list-sdks 2>/dev/null | grep -q '^8\.'; then
  fail "未找到 .NET 8 SDK（global.json 锁定 8.0.x）: dotnet --list-sdks 输出无 8.x"
fi

# --- vendored 原生依赖（与 Core.Mac/CMakeLists.txt 的要求一一对应）---
if [ ! -f "$LIBCXX_PREFIX/lib/libc++.a" ]; then
  if [ "$CHECK_ONLY" -eq 1 ]; then
    fail "缺少静态 libc++: ${LIBCXX_PREFIX}/lib/libc++.a（正常 setup 会自动克隆并构建）"
  elif [ "$errors" -gt 0 ]; then
    fail "缺少静态 libc++，且工具链校验未通过，无法自动构建: ${LIBCXX_PREFIX}/lib/libc++.a"
  elif ! have git; then
    fail "缺少 git，无法克隆 llvm-project 构建静态 libc++"
  else
    echo "[TitanSetup] 未找到静态 libc++，自动从 GitHub 构建（首次约 10 分钟）"
    if ! build_libcxx; then
      fail "静态 libc++ 构建失败，见上方 cmake 输出"
    fi
  fi
fi

# 条件依赖按 TITAN_MAC_MODULES 实际包含的模块判定（与 CMake 的 IN_LIST 判断同源）
SOURCES_GEN="$ROOT/Core.Mac/generated/TitanSources.cmake"
if [ -f "$SOURCES_GEN" ]; then
  # metis 与 CMake 语义一致（EXISTS 可选链接）：缺失只警告，链接期缺符号再补
  if [ ! -f "$ROOT/3rd/native/metis/5.1.0/libmetis/Mac/$CFG/libmetis.a" ]; then
    echo "[TitanSetup][警告] metis Mac 静态库缺失（可选链接）: 3rd/native/metis/5.1.0/libmetis/Mac/$CFG/libmetis.a"
  fi
  if grep -q 'NativeCode/Bricks/PhysicsCore/NvPhysX5/NvPhysX5.vcxitems' "$SOURCES_GEN" \
     && [ ! -d "$ROOT/3rd/native/PhysX5/bin/mac.arm64.clang.mt" ]; then
    fail "缺少 PhysX5 Mac 静态库目录: 3rd/native/PhysX5/bin/mac.arm64.clang.mt（见 3rd/native/PhysX5/Mac/README.md）"
  fi
  if grep -q 'NativeCode/NextRHI/Vulkan/Vulkan.vcxitems' "$SOURCES_GEN"; then
    if [ ! -f "$ROOT/3rd/native/Vulkan/Mac/libvulkan.1.dylib" ]; then
      fail "缺少 Vulkan loader: 3rd/native/Vulkan/Mac/libvulkan.1.dylib（见 3rd/native/Vulkan/Mac/README.md）"
    fi
    if [ ! -f "$ROOT/3rd/native/Vulkan/Mac/libMoltenVK.dylib" ]; then
      fail "缺少 MoltenVK: 3rd/native/Vulkan/Mac/libMoltenVK.dylib"
    fi
    if [ ! -f "$ROOT/3rd/native/Vulkan/Mac/icd.d/MoltenVK_icd.json" ]; then
      fail "缺少 ICD 清单: 3rd/native/Vulkan/Mac/icd.d/MoltenVK_icd.json"
    fi
  fi
else
  echo "[TitanSetup][警告] 缺少 Core.Mac/generated/TitanSources.cmake，跳过条件依赖检查（首次 codegen 会生成）"
fi

if [ "$errors" -gt 0 ]; then
  echo "[TitanSetup] 依赖校验失败，共 $errors 项缺失。补齐后重跑 ./setup.sh"
  exit 1
fi
echo "[TitanSetup] 依赖校验通过"

if [ "$CHECK_ONLY" -eq 1 ]; then
  echo "[TitanSetup] --check-only: 到此为止"
  exit 0
fi

echo "[TitanSetup] === 2/4 胶水生成（${CFG}）==="
STAMP="$ROOT/codegen/Mac/NativeBinder/.codegen.stamp"
if [ "$SKIP_CODEGEN" -eq 1 ] && [ -f "$STAMP" ]; then
  echo "[TitanSetup] --skip-codegen 且 stamp 存在，跳过"
else
  mkdir -p cache
  cmake -DTITAN_CONFIGURATION="$CFG" -P Core.Mac/GenerateCodegen.cmake 2>&1 | tee "cache/mac_setup_codegen_$CFG.log"
fi

BUILD_DIR="$ROOT/intermediate/cpp/Core.Mac/$CFG"
echo "[TitanSetup] === 3/4 configure（${BUILD_DIR}）==="
if [ ! -f "$BUILD_DIR/build.ninja" ]; then
  echo "[TitanSetup] 构建目录不存在或未 configure，自动执行 configure"
  cmake -S Core.Mac -B "$BUILD_DIR" -G Ninja \
    -DCMAKE_BUILD_TYPE="$CFG" \
    -DCMAKE_OSX_ARCHITECTURES=arm64 \
    -DTITAN_MAC_ALLOW_UNDEFINED=OFF \
    -DCMAKE_EXPORT_COMPILE_COMMANDS=ON
fi

if [ "$SKIP_BUILD" -eq 1 ]; then
  echo "[TitanSetup] --skip-build: 到此为止"
  exit 0
fi

echo "[TitanSetup] === 4/4 build（${CFG}）==="
cmake --build "$BUILD_DIR"
echo "[TitanSetup] 完成: binaries/$(echo "$CFG" | tr '[:upper:]' '[:lower:]')/Core.Mac.dylib"

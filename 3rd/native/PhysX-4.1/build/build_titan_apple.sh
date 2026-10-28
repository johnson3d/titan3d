#!/usr/bin/env bash
# Build PhysX 4.1 static libs for Apple arm64 (macOS + iOS). Run on a Mac with Xcode (command line tools) + CMake >= 3.14.
# PhysX 4.1's own mac preset is x86_64/SSE2 only, so macOS arm64 reuses the "ios" platform cmake files
# (PxPreprocessor.h treats __APPLE__ && __arm64__ as PX_IOS with NEON) but compiles against the macOS SDK.
#
# Usage:   bash build_titan_apple.sh [all|mac|ios] [config ...]
# Example: bash build_titan_apple.sh ios release
# Output:  physx/bin/mac.arm_64/<config>/  and  physx/bin/ios.arm_64/<config>/
# Env:     MAC_DEPLOYMENT_TARGET (default 11.0), IOS_DEPLOYMENT_TARGET (default 12.0)
set -euo pipefail

PLATFORMS="${1:-all}"
if [ $# -gt 0 ]; then shift; fi
CONFIGS=("$@")
if [ ${#CONFIGS[@]} -eq 0 ]; then CONFIGS=(debug checked profile release); fi

case "$PLATFORMS" in
    all) PLATFORMS=(mac ios) ;;
    mac|ios) PLATFORMS=("$PLATFORMS") ;;
    *) echo "Unknown platform '$PLATFORMS' (expected all|mac|ios)" >&2; exit 1 ;;
esac

command -v cmake >/dev/null || { echo "cmake not found (brew install cmake)" >&2; exit 1; }
command -v xcrun >/dev/null || { echo "xcrun not found (install Xcode)" >&2; exit 1; }

PHYSX_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
EXTERNALS="$(cd "$PHYSX_ROOT/../externals" && pwd)"
export PHYSX_ROOT_DIR="$PHYSX_ROOT"
export PM_CMakeModules_PATH="$EXTERNALS/cmakemodules"
export PM_PxShared_PATH="$(cd "$PHYSX_ROOT/../pxshared" && pwd)"
export PM_PATHS="$EXTERNALS/cmakemodules;$EXTERNALS/targa"

if command -v ninja >/dev/null; then GENERATOR="Ninja"; else GENERATOR="Unix Makefiles"; fi
JOBS="$(sysctl -n hw.ncpu)"

for platform in "${PLATFORMS[@]}"; do
    if [ "$platform" = "mac" ]; then
        BIN_NAME="mac.arm_64"
        PLATFORM_ARGS=(
            -DCMAKE_OSX_SYSROOT=macosx
            -DCMAKE_OSX_DEPLOYMENT_TARGET="${MAC_DEPLOYMENT_TARGET:-11.0}"
        )
    else
        BIN_NAME="ios.arm_64"
        PLATFORM_ARGS=(
            -DCMAKE_SYSTEM_NAME=iOS
            -DCMAKE_OSX_SYSROOT=iphoneos
            -DCMAKE_OSX_DEPLOYMENT_TARGET="${IOS_DEPLOYMENT_TARGET:-12.0}"
        )
    fi

    for cfg in "${CONFIGS[@]}"; do
        echo "==== Building $platform arm64 $cfg ===="
        BUILD_DIR="$PHYSX_ROOT/compiler/apple-$platform-$cfg"
        rm -rf "$BUILD_DIR"

        cmake -S "$PHYSX_ROOT/compiler/public" -B "$BUILD_DIR" \
            -G "$GENERATOR" \
            --no-warn-unused-cli \
            -DCMAKE_POLICY_VERSION_MINIMUM=3.5 \
            -DCMAKE_PROJECT_INCLUDE="$PHYSX_ROOT/buildtools/TitanRelaxWarnings.cmake" \
            "${PLATFORM_ARGS[@]}" \
            -DCMAKE_OSX_ARCHITECTURES=arm64 \
            -DTARGET_BUILD_PLATFORM=ios -DPX_OUTPUT_ARCH=arm \
            -DNV_FORCE_64BIT_SUFFIX=TRUE \
            -DPX_OUTPUT_PLATFORM_BIN_NAME="$BIN_NAME" \
            -DCMAKE_BUILD_TYPE="$cfg" \
            -DCMAKE_PREFIX_PATH="$PM_PATHS" \
            -DPHYSX_ROOT_DIR="$PHYSX_ROOT" \
            -DPX_OUTPUT_LIB_DIR="$PHYSX_ROOT" \
            -DPX_OUTPUT_BIN_DIR="$PHYSX_ROOT" \
            -DPX_BUILDSNIPPETS=FALSE -DPX_BUILDPUBLICSAMPLES=FALSE \
            -DPX_GENERATE_STATIC_LIBRARIES=TRUE \
            -DCMAKE_INSTALL_PREFIX="$PHYSX_ROOT/install/$platform-arm64/PhysX"

        cmake --build "$BUILD_DIR" --parallel "$JOBS"
    done

    # include/PxConfig.h is regenerated per platform; keep the matching copy next to the binaries.
    cp "$PHYSX_ROOT/include/PxConfig.h" "$PHYSX_ROOT/bin/$BIN_NAME/PxConfig.h"
    echo "Done: $PHYSX_ROOT/bin/$BIN_NAME"
done

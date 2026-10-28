# PhysX 5.6.1 macOS arm64 静态库

`bin/mac.arm64.clang.mt/{debug,release}/libPhysX*_static_64.a` 由 PhysX 5.6.1 源码构建（对齐 `3rd/native/PhysX5` 官方 SDK 版本，`PX_PHYSICS_VERSION 5.6.1`）。

## 重建步骤

1. 检出源码（官方 tag 已被 Omniverse 仓库移除，用等价提交）：

   ```sh
   git clone --filter=blob:none https://github.com/NVIDIA-Omniverse/PhysX.git
   cd PhysX && git checkout 5ca9f47   # "Update PhysX to 5.6.1 at b07ddd12"
   ```

2. 应用本目录的 `physx-5.6.1-mac-arm64.patch`（含 `source/compiler/cmake/mac/` 平台移植 + 4 处 Apple arm64 修复：PxSIMDHelpers unix 分支、FdUnixFPU arm64 FP 异常、FdTempAllocator 整型二义、SnSerialUtils mac64 平台标记）。

3. 构建（Debug 与 Release 各一次，改 `-DCMAKE_BUILD_TYPE` 与输出目录）：

   ```sh
   cmake -S physx/compiler/public -B build-mac64-debug -G Ninja \
     -DTARGET_BUILD_PLATFORM=mac -DPX_OUTPUT_ARCH=arm64 -DLIBPATH_SUFFIX=arm64 \
     -DPHYSX_ROOT_DIR=$PWD/physx \
     -DPX_OUTPUT_LIB_DIR=$PWD/physx -DPX_OUTPUT_BIN_DIR=$PWD/physx \
     -DPX_GENERATE_STATIC_LIBRARIES=ON \
     -DCMAKE_BUILD_TYPE=debug -DCMAKE_OSX_ARCHITECTURES=arm64
   cmake --build build-mac64-debug
   ```

4. 产物在 `physx/bin/mac.x86_64/<config>/`（命名残留 x86，内容为 arm64），拷贝到本目录 `bin/mac.arm64.clang.mt/<config>/`。

注意：构建关闭了 OmniPVD（`PX_SUPPORT_OMNI_PVD=0`，mac 补丁内置），GPU 物理不参与。

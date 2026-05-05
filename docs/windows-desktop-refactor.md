# Windows Desktop Refactor / Windows 桌面重构说明

Base branch: `origin/ImGuiEditor_johnson` at `5bc5a5143`

Target branch: `windows-desktop-refactor`

## 中文说明

### 目标和边界

本轮重构从最新 `ImGuiEditor_johnson` 重新开始，不再尝试合并旧 refactor 历史。目标是保留现有编辑器、工具链、资源生产、RPC、AutoSync、Macross、Serializer、NativeBinder、插件等能力，同时把默认开发路径收敛为 Windows desktop。

本轮不删除 Android/iOS 源码，只把移动端从新的默认 Windows 构建入口和 Windows solution 中移出。C# 层也不整体删除：C# 继续作为编辑器、工具、内容生产和托管运行时层；Windows runtime 核心继续以 C++/NativeBinder/PInvoke bridge 为主。唯一被移除的是历史语言转换链路 `Cs2Cpp`。

### 构建入口

新增统一入口：

- `eng/build.ps1`
- `eng/deps.ps1`
- `eng/common.ps1`
- `eng/dependencies.json`
- `eng/NativePackages.props`
- `Engine.Windows.sln`

`eng/build.ps1` 负责显式编排 Windows 桌面构建阶段：

1. 查找 VS 2022 MSBuild、VS dev environment、Windows SDK、dotnet、NuGet。
2. 把 VS 自带 LLVM/libclang 加入 `PATH`，供 `ClangSharp`/`CppWeavingTools` 使用。
3. 可选执行 `eng/deps.ps1 -Restore`，统一检查 NuGet 和 native SDK。
4. 构建 generator tools：`CppWeavingTools`、`CSharpCodeTools`。
5. 显式执行 NativeBinder、RPC、AutoSync、Macross codegen。
6. 构建 `Core.Window` x64 `Debug`/`Release`。
7. 构建 `Engine.Window`、`MainEditor`、Windows 插件和必要工具。

`Engine.Windows.sln` 只保留 Windows desktop 主路径，主配置收敛为 `Debug|x64` 和 `Release|x64`。Android/iOS 项目没有加入这个 solution。

历史脚本 `CallTHT.bat`、`CS_Engine.bat`、`CS_Rpc_AutoSync.bat`、`CS_Rpc_Engine.bat` 现在只是薄 wrapper，真实顺序由 `eng/build.ps1` 控制，避免隐藏 pre-build 造成首次构建缺生成文件、二次构建才成功的问题。

### 依赖和版本管理

托管 NuGet 版本已集中到 `Directory.Packages.props`，并启用 Central Package Management。项目文件中的散落 `PackageReference Version=...` 已移除。

新增 `global.json`，锁定 .NET SDK 起点：

- SDK: `8.0.300`
- Roll forward: `latestFeature`

关键托管包版本：

- `Microsoft.CodeAnalysis.*`: `4.10.0`
- `ClangSharp`: `20.1.2.1`
- `libclang.runtime.win-x64`: `20.1.2`
- `ppy.SDL3-CS`: `2025.703.0`
- `WinPixEventRuntime`: `1.0.220124001`

`eng/dependencies.json` 记录并检查 native SDK/包，包括：

- Visual Studio 2022 Build Tools / MSBuild
- Windows SDK
- WinPixEventRuntime
- PhysX 4.1.2
- PhysX 5.6.1 Windows vc143 mt libs
- Autodesk FBX SDK 2019.2
- Vulkan vendored headers/libs and loader
- Embree 4.x
- NVIDIA Nsight Aftermath
- ShaderConductor
- Python 3.10 runtime
- HPSocket
- RenderDoc
- pthread-win32
- METIS
- Crunch
- CGAL/Boost
- SPIR-V toolchain

当前机器上的依赖检查仍发现部分 native 包缺失。完整构建前需要补齐：

- `3rd/native/PhysX5/bin/win.x86_64.vc143.mt/debug/*.lib`
- `3rd/native/PhysX5/bin/win.x86_64.vc143.mt/release/*.lib`
- `3rd/native/embree/embree4/include/embree4/rtcore.h`
- `3rd/native/embree/embree4/lib/embree4.lib`
- `3rd/native/embree/embree4/lib/tbb12.lib`
- `3rd/native/CGAL/auxiliary/gmp/lib/libgmp-10.lib`
- `3rd/native/SpirvTools/lib/win64`
- `3rd/native/glslang/lib`

### Cs2Cpp 移除和 runtime bridge

本轮删除 `Cs2Cpp` 语言转换路径：

- 移除 `mode=Cs2Cpp`。
- 移除 `codegen/Cs2Cpp` shared project / vcxitems。
- 移除活跃 `UCs2CppAttribute` 测试入口。
- 移除 native 对 `.cs2cpp.h` 的依赖。
- `Rpc_Engine.txt` 不再声明 `Cs2Cpp_Target`。

NativeBinder 继续保留。C++ 头上的 `TR_CLASS` / `TR_FUNCTION` 仍生成 C# P/Invoke 包装，这是 ImGui、RHI、物理、资源系统等能力的基础。

原先依赖 Cs2Cpp 的 `TtNativeCoreProvider` 被改为明确 bridge：

- C# 初始化时注册 callback table。
- C++ 侧通过稳定函数表调用托管对象能力。
- 保留 GCHandle、pin、属性 get/set、list get/add/clear/remove、数组 pin 等运行时能力。
- `Engine` 启动/退出时显式初始化和清理该 bridge。

### ImGui 状态

这次重做以 `ImGuiEditor_johnson` 最新提交为基线，保留该分支上的 ImGui 1.92.x/docking 改动和本地绑定层。重构没有回滚 ImGuiEditor 的更新，也没有把旧 refactor 分支的 ImGui 状态强行合入。

后续继续升级 ImGui 时，本地侵入点应继续收敛到：

- `imconfig.h`
- `imgui_binding.*`
- C# `ImGuiAPI.cs`
- SDL3 window/docking bridge
- draw data renderer
- file dialog / ColorTextEditor 集成层

### 当前验证结果

已执行并验证：

- `rg` 扫描确认主代码路径无 `Cs2Cpp` / `UCs2Cpp` / `.cs2cpp.h` 活跃引用。
- `rg` 扫描确认 `.csproj` 中无散落 `PackageReference Version=...`。
- `Engine.Windows.sln` 中没有 Android/iOS 项目。
- `eng/build.ps1 -Configuration Debug -SkipDependencyCheck -CodeGenOnly` 成功完成 generator 构建和 NativeBinder/RPC/AutoSync/Macross codegen。

仍受当前机器 native 依赖缺失影响：

- `eng/deps.ps1 -Restore` 能恢复 NuGet，但会报告上述 14 个 native 缺失项。
- `eng/build.ps1 -Configuration Debug -SkipDependencyCheck -SkipNative -SkipCodeGen -SkipPlugins` 已推进到 `Engine.Window`，随后因为当前 worktree 中 NativeBinder 生成源码不完整而失败；缺项包含 Embree、PhysX 和 ImGui 相关生成文件。为了避免把“缺 native SDK 机器上生成出的不完整 projectitems”提交进去，projectitems 保留完整列表，仅将旧 `UCs2CppBase` 条目替换为新的 `TtManagedObjectBridge` 条目。
- 因完整 native 构建未完成，`MainEditor.exe` 的最新产物交互烟测尚未能在本轮分支上完成。

推荐下一步：

1. 补齐 PhysX5、Embree4、CGAL gmp、SPIR-V/glslang native 包。
2. 运行 `eng/deps.ps1 -Restore`，确认依赖检查通过。
3. 运行 `eng/build.ps1 -Configuration Debug`。
4. 启动 `binaries/debug/MainEditor.exe`，验证 DX12、dock/viewport、菜单、content browser、file dialog、文本编辑器、资源保存、Macross 打开/编译、插件加载。

## English Notes

### Goals and Scope

This refactor was replayed cleanly from the latest `ImGuiEditor_johnson` branch instead of merging the older refactor history. The goal is to preserve the existing editor, tools, asset production, RPC, AutoSync, Macross, Serializer, NativeBinder, plugin, and Windows runtime capabilities while making Windows desktop the default supported development path.

Android/iOS source code is preserved, but mobile projects are excluded from the new Windows solution and default build entry. The C# layer is preserved as the editor/tool/content-production and managed runtime layer. The Windows runtime core remains C++ plus NativeBinder/PInvoke bridge. The removed piece is the historical `Cs2Cpp` language-conversion pipeline.

### Build Orchestration

New entry points:

- `eng/build.ps1`
- `eng/deps.ps1`
- `eng/common.ps1`
- `eng/dependencies.json`
- `eng/NativePackages.props`
- `Engine.Windows.sln`

`eng/build.ps1` explicitly orchestrates the Windows desktop build:

1. Locate VS 2022 MSBuild, VS developer environment, Windows SDK, dotnet, and NuGet.
2. Add VS LLVM/libclang to `PATH` for `ClangSharp`/`CppWeavingTools`.
3. Optionally run `eng/deps.ps1 -Restore` for NuGet/native dependency checks.
4. Build generator tools: `CppWeavingTools` and `CSharpCodeTools`.
5. Run NativeBinder, RPC, AutoSync, and Macross code generation.
6. Build `Core.Window` x64 `Debug`/`Release`.
7. Build `Engine.Window`, `MainEditor`, Windows plugins, and supporting tools.

`Engine.Windows.sln` contains the Windows desktop path only and is limited to `Debug|x64` and `Release|x64`. Android/iOS projects are intentionally excluded.

Legacy scripts are now thin wrappers around the unified build entry. The real order lives in `eng/build.ps1`, which removes hidden pre-build sequencing.

### Dependency Management

Managed NuGet versions are centralized in `Directory.Packages.props` with Central Package Management enabled. Scattered `PackageReference Version=...` attributes were removed from project files.

`global.json` pins the SDK floor to .NET SDK `8.0.300` with `latestFeature` roll-forward.

`eng/dependencies.json` records the required Windows native and managed dependency surface, including VS 2022, Windows SDK, WinPix, PhysX, FBX SDK, Vulkan, Embree, Aftermath, ShaderConductor, Python, HPSocket, RenderDoc, pthread, METIS, Crunch, CGAL/Boost, and SPIR-V tooling.

The current machine is still missing several native dependencies: PhysX5 vc143 mt libraries, Embree 4 headers/libs, CGAL GMP lib, and SPIR-V/glslang library directories. These must be restored before a full native build and editor smoke test can pass.

### Cs2Cpp Removal and Runtime Bridge

The `Cs2Cpp` path was removed:

- No active `mode=Cs2Cpp`.
- No `codegen/Cs2Cpp` imports.
- No active `UCs2CppAttribute` test path.
- No native `.cs2cpp.h` dependency.
- `Rpc_Engine.txt` no longer declares `Cs2Cpp_Target`.

NativeBinder remains the supported interop mechanism. `TR_CLASS` / `TR_FUNCTION` C++ declarations still generate C# P/Invoke wrappers for ImGui, RHI, physics, resources, and other native systems.

`TtNativeCoreProvider` is now an explicit runtime bridge. C# registers a callback table at startup; C++ stores and calls stable function pointers for GCHandle, pinning, property access, list access, and array pinning. The engine initializes and releases this bridge explicitly.

### Verification

Verified so far:

- Main code paths scan clean for `Cs2Cpp`, `UCs2Cpp`, and `.cs2cpp.h`.
- `.csproj` files no longer contain scattered `PackageReference Version=...` entries.
- `Engine.Windows.sln` excludes Android/iOS projects.
- `eng/build.ps1 -Configuration Debug -SkipDependencyCheck -CodeGenOnly` completes generator build and NativeBinder/RPC/AutoSync/Macross codegen.

Blocked by local native dependency gaps:

- `eng/deps.ps1 -Restore` restores NuGet packages but reports the missing native packages listed above.
- Managed build reaches `Engine.Window` and then fails because NativeBinder generated sources are incomplete in the current worktree; missing generated files include Embree, PhysX, and ImGui entries. To avoid committing a degraded projectitems list produced on a machine with missing native SDKs, the projectitems keep the complete generated list and only replace the old `UCs2CppBase` entry with the new `TtManagedObjectBridge` entry.
- Latest-branch `MainEditor.exe` interactive smoke test is pending until the native dependency set is restored and a full Debug build completes.

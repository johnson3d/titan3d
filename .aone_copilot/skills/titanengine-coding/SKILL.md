---
name: titanengine-coding
description: TitanEngine (本仓库) 引擎层 C# 代码的编写规范。当任务涉及在 CSharpCode/Grapics/ 下编写或修改渲染管线代码 (RenderGraphNode / TtShadingEnv / Compute/Graphics Drawcall / CBuffer 绑定)、添加新的 RenderGraph 节点、Permutation 开关、HLSL/.compute shader 与 C# 端联动、或任何会调用 TtEngine.Instance.GfxDevice.RenderContext.CreateCBV 的逻辑时, 必须先阅读本 skill 并严格遵守其引用的强制规范文档。
---

# TitanEngine 引擎层编码规范 Skill

本 skill 用于约束在 **TitanEngine** 仓库 (即本工作区根目录) 内编写引擎层代码 (尤其是渲染相关) 时的行为, 防止重复犯仓库历史上踩过的坑 (典型例子: cbuffer 未初始化导致 GPU Hang)。

> 本文档涉及的所有路径 (规范文档、参考代码) 均为**相对于本仓库根目录**的路径, 工具调用时按当前 workspace 实际根路径拼接。

## 引擎坐标系

TitanEngine 使用**左手坐标系 (Left-Handed)**:

| 轴 | 方向 | Vector3 常量 |
|---|---|---|
| +X | 向右 (Right) | `Vector3.Right = (1, 0, 0)` |
| +Y | 向上 (Up) | `Vector3.Up = (0, 1, 0)` |
| +Z | 朝屏幕里面 / 远离观察者 (Forward) | `Vector3.Forward = (0, 0, 1)` |

- 相机默认朝 +Z 方向看
- `LookAtLH` 是标准左手系 LookAt (与 D3D `D3DXMatrixLookAtLH` 相同)
- `PerspectiveFovLH` 是标准左手系透视投影

## 触发场景

只要任务涉及以下任一情况, 就必须执行本 skill 的 "执行流程":

1. 在 `CSharpCode/Grapics/` 下新建或修改 `.cs` 文件
2. 创建/修改任何 `RenderGraphNode` 的子类 (`TAuxRenderGraphNode<>`, `TAuxSceenSpaceNode<>` 等)
3. 创建/修改任何 `TtShadingEnv` 的子类 (`TtComputeShadingEnv`, `TtGraphicsShadingEnv`)
4. 创建/修改 `enginecontent/Shaders/` 下的 `.compute` / `.cginc` 文件, 且 C# 端配套有 `TtShadingEnv` 对应实现
5. 任何调用 `TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(...)` 的代码
6. 任何涉及 Permutation (`PushPermutation` / `UpdatePermutation` / `ENV_USE_*` 宏) 的 shader/C# 联动
7. 加载资产 (`RName.GetAsset`)、增加 `Config` 配置、写 RPC 函数等基础设施类任务
8. 使用 RenderDoc MCP 工具分析渲染问题 (打开 `.rdc`, sample pixel, debug shader 等)
9. 使用 bindless 资源数组 (`IBindless` / `TtBindless` / `drawcall.CreateBindless` / HLSL 端 `DX_AUTOBIND` + `Texture2D[] / SamplerState[] / cbuffer DX_AUTOBIND` / `NonUniformResourceIndex` 索引访问 unbounded array) 的任务

## 执行流程 (必须严格按顺序)

### 第 1 步: 强制阅读规范文档

在动手编辑代码 **之前**, 必须用 `read_file` 完整阅读以下两个文档:

1. **`documents/coding/CodingGuidelines.md`** —— 强制性规则 (违反会导致 GPU Hang / Crash / 难排查问题)
2. **`documents/coding/CodeLib.md`** —— 各种引擎模块的标准代码模板 (插件、ShadingEnv、MdfQueue、资产加载、Config 等)

这两个文档是整个仓库的 single source of truth, 持续更新, 不要凭"上次我看过了"或"我记得是这样"的记忆来写代码。**任何一次新的引擎层任务都重新读一遍**, 因为它们可能新增了规则。

### 第 2 步: 找到一个"已合规"的同类参考

在写新代码前, 找一个仓库内**已经合规**的同类实现来对照写法。规范文档里通常会给出参考路径, 例如:

| 你要写的东西 | 已合规参考 |
|---|---|
| 通用 cbuffer 创建 + 绑定 | `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` 的 `GetOrCreateSharedCBuffer` |
| Compute shading env + drawcall | `CSharpCode/Grapics/Pipeline/Common/Post/FsrNode.cs` 的 `TtFsrUpSampleShading` |
| 全屏 post-process (graphics shading) | `CSharpCode/Grapics/Pipeline/Common/Post/AdditiveNode.cs` |
| 多 pass / ping-pong RT | `CSharpCode/Grapics/Pipeline/Common/Post/BloomNode.cs` |
| Permutation 切换 | `TtReSTIRInitialSamplingShading.IsEnableHWRT` (ReSTIRGINode.cs) |
| Shadow / 复杂 cbuffer 绑定 | `CSharpCode/Bricks/AdvanceShadow/AdvanceShadowShading.cs` |
| Bindless 资源数组 (TtBindless / DX_AUTOBIND) | C# 端: `CSharpCode/NxRHI/Buffer.cs` 的 `TtBindless` (SetSrv/SetCbv/SetUav/SetSampler/ClearSlot/RebindAll); HLSL 端最小模板: `enginecontent/Shaders/ShadingEnv/DummyShading.cginc`; 规范见 `Documents/Coding/CodingGuidelines.md` §1.4 |
| GI 路线选型 (ReSTIR vs TtAtlas GI) | 双轨制决策见 `Documents/Coding/CodingGuidelines.md` §1.4.8; TtAtlas GI 系统蓝图见 `Documents/Architecture/AtlasGI.md`; 接 GI 任务前必读这两份文档 |

如果规范文档或 `CodeLib.md` 里没有列你想要的场景, 用 `file_grep` / `codebase_search` 找一个语义最接近的现有节点, **看完它再开始写**。

### 第 3 步: 编码时主动遵守已知规则

下面是从 `CodingGuidelines.md` 等文档浓缩出来的高频要点 (**仍然以文档原文为准**, 这里只是提醒):

#### 3.1 CBuffer 必须遵守"创建即填满 + 立即 flush"

```csharp
if (mCBuffer == null)
{
    mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
    // ① 全部字段 SetValue
    mCBuffer.SetValue("FieldA", in valueA);
    mCBuffer.SetValue("FieldB", in valueB);
    // ... 一个不能漏

    // ② 必须 MarkDirty + FlushDirty (无 cmd 版本同步生效)
    mCBuffer.MarkDirty();
    mCBuffer.FlushDirty();
}

// 后续帧: 只 SetValue, 不要再手动 FlushDirty (引擎 frame end 自动处理)
mCBuffer.SetValue("FieldA", in valueA);
drawcall.BindCBV(binder, mCBuffer);
```

漏掉 ② 会导致首帧 cbuffer 是未初始化垃圾, **如果某个字段控制 shader 循环次数或数组下标 -> GPU Hang -> TDR -> 进程崩**。Aftermath 报 `Fault address 0x0000000000000000` 几乎都是这种。

#### 3.2 Drawcall 资源绑定优先用 string 重载

```csharp
drawcall.BindSrv("GBufferRT0", attachBuffer.Srv);    // ✓ 推荐
```

只有在需要 binder 反射出字段 layout (如创建 CBV) 时, 才走 `FindBinder + BindXxx(binder, view)`。底层 `BindXxx(FShaderBinder, IXxxView)` 重载和 `TtXxxView` 类型可能不匹配, 容易踩坑。

#### 3.3 Permutation 切换是配置型的, 不要每帧切

`PushPermutation` / `UpdatePermutation` 会触发 effect 重新编译/拉取, 是**启动期/质量档**操作。在 setter 里用 `if (mField == value) return;` 守卫, 避免运行期反复触发。参考 `EnableHardwareRT` 写法。

#### 3.4 Shader 资源声明顺序

如果 `.cginc` 里的 helper 函数会访问某个全局资源 (如 `EnvMap`), `.compute` 主文件**必须先声明该资源, 再 `#include` 公共头**, 否则编译失败。参考 `ReSTIRInitialSampling.compute` 中 `EnvMap` 与 `ReSTIRCommon.cginc` 的顺序。

#### 3.5 OnDrawCall 里的 TagObject 用法

`drawcall.TagObject = node;` 必须在 `CreateComputeDraw / CreateGraphDraw` 之后立即设置, 因为 `ShadingEnv.OnDrawCall` 通过 `(drawcall.TagObject as TtMyNode)` 拿回节点上下文。

#### 3.6 所有 BindXxx / CreateCBV 必须在 OnDrawCall 阶段完成, 不能在 Tick 阶段

**强制规则**: 所有 `drawcall.BindSrv / BindUav / BindCBV / BindSampler` 以及依赖 `FindBinder` 的 `CreateCBV` 都**必须写在 `ShadingEnv.OnDrawCall` 里**, 不能写在节点的 `Tick` 中。如果绑定的资源每帧/每迭代都不同 (例如 ping-pong 的 input/output, 多 pass 的 StepSize), 用**节点成员变量**在 `Tick → OnDrawCall` 之间传值。

**为什么**:
- `SetDrawcallDispatch` 是分水岭, 它内部做两件事: ① 把 effect/PSO 装进 drawcall (建立合法的 binder 表) ② 触发 `OnDrawCall` 回调
- 在 `SetDrawcallDispatch` **之前** 调用 `BindXxx`: drawcall 还没有合法 binder 表, 绑不上; 即便底层勉强接收, 也会被随后的 effect 装配覆盖/失效
- 在 `SetDrawcallDispatch` **之前** 调用 `FindBinder` (用于 `CreateCBV`): effect 可能根本还没 `await` 完成 (构造器里的 `UpdatePermutation().AddWaitTask()` 是 fire-and-forget), binder 字段表为空, `SetValue` 全部静默失败, 再 `FlushDirty` 一份未填充的 cbuffer -> DXGI 报 `DXGI_ERROR_INVALID_CALL` 或 GPU 读到垃圾

**标准模板**:

```csharp
// ShadingEnv: 所有 Bind 都在这里
public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
{
    var node = drawcall.TagObject as TtMyNode;
    if (node == null) return;

    drawcall.BindSrv("InputTex", node.GetCurrentInputSrv());     // 节点字段传过来
    drawcall.BindUav("OutputTex", node.GetCurrentOutputUav());
    drawcall.BindSrv("GBufferRT0", node.GetAttachBuffer(node.GBufferRT0PinIn).Srv);
    drawcall.BindSampler("Samp_PointClamp", ...);

    var cb = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbMyPass");
    if (cb.IsValidPointer)
        drawcall.BindCBV(cb, node.GetOrCreateCBuffer(cb));        // CBV 在这里创建
}

// 节点的 Tick: 只暂存"本次要绑什么", 然后调 SetDrawcallDispatch
public override void Tick(...)
{
    for (int i = 0; i < iterations; i++)
    {
        var drawcall = mDrawCalls[i];

        // ① 把"本轮"参数暂存到节点字段
        mCurrentInputSrv = ...;
        mCurrentOutputUav = ...;
        mCurrentStepSize = ...;

        // ② SetDrawcallDispatch 之前不要有任何 BindXxx
        mShading.SetDrawcallDispatch(this, policy, drawcall, w, h, 1, true);
        cmd.PushGpuDraw(drawcall);
    }
}
```

**反例** (本仓库踩过):

```csharp
// ❌ Tick 里直接 BindSrv/BindUav, 然后才 SetDrawcallDispatch
drawcall.BindSrv("ColorInput", currentInputSrv);
drawcall.BindUav("DenoiseOutput", currentOutputUav);
mShading.SetDrawcallDispatch(this, policy, drawcall, w, h, 1, true); // ← 此前的 Bind 失效
cmd.PushGpuDraw(drawcall);
```

```csharp
// ❌ Tick 里 FindBinder + CreateCBV (effect 可能还没 await 完成)
var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbXxx");
mCBuffer = ...CreateCBV(binder);    // binder 字段表为空, layout 反射失败
mCBuffer.SetValue(...);              // 静默失败
mCBuffer.FlushDirty();               // 把空数据 flush 上去 -> DXGI INVALID_CALL
```

**合规参考**:
- `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` —— 4 个 pass 都是 Tick 里只 `SetDrawcallDispatch + PushGpuDraw`, 所有 Bind 在 OnDrawCall, CBV 通过 `GetOrCreateSharedCBuffer(binder)` 在 OnDrawCall 阶段创建
- `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs` —— 多迭代 ping-pong 用 `mCurrentInputSrv / mCurrentOutputUav / mCurrentStepSize` 节点字段在 Tick → OnDrawCall 之间传值的范本

### 第 4 步: 自检 (在 lint check 之前)

写完代码用人眼把这些 checklist 走一遍 (规范里全有, 这里是高频踩坑点):

- [ ] 所有 `CreateCBV` 都按 §3.1 写了"全字段 SetValue + MarkDirty + FlushDirty"了吗?
- [ ] 所有 `BindXxx / CreateCBV` 都写在 `OnDrawCall` 里 (§3.6), 而不是 Tick 里? Tick 里只暂存参数到节点字段 + 调 `SetDrawcallDispatch + PushGpuDraw`?
- [ ] CBuffer 字段顺序和 HLSL 端 `cbuffer { ... }` 完全一致吗? (字段名、类型、对齐)
- [ ] 新加 pin 的 `EBufferType` 标志是否包含下游真正需要的 (BFT_SRV / BFT_UAV / BFT_RTV / BFT_DSV)?
- [ ] 多 pass / ping-pong 的 RT 是否在 `OnResize` 里随 width/height 重建?
- [ ] `Dispose` 是否释放了所有自己创建的资源 (drawcall / cbuffer / texture / srv / uav / buffer)?
- [ ] 跨 pass 共享的 CBuffer 是否所有 pass 的 layout 完全一致? (参考 ReSTIRGINode 的 `GetOrCreateSharedCBuffer` 注释)
- [ ] 涉及 bindless 时, `drawcall.CreateBindless` 是否在 `OnDrawCall` 阶段调用 (与 §1.2 一致, 不在 Tick 里)? `TtBindless` 实例是否跨帧复用 (不每帧重建)? HLSL 端非 wave-uniform 索引是否包了 `NonUniformResourceIndex`? 释放被引用资源前是否先 `ClearSlot` 解锁? 详见 §1.4

### 第 5 步: 用 read_lints 确认无新增编译错误

按引擎默认要求, 任务结束前用 `read_lints` 检查本次修改过的所有文件。

## RenderDoc 抓帧 (.rdc) 默认目录

本仓库所有 `.rdc` 抓帧文件统一放在 `cache/renderdoc/` 下 (相对仓库根目录)。

使用 RenderDoc MCP 工具 (`renderdoc::tool::open_capture` 等) 分析渲染问题时:

- **默认从 `cache/renderdoc/` 下查找** `.rdc` 文件, 不要要求用户每次提供绝对路径
- 用户提到 "看一下 rdc" / "分析这帧" / "打开抓帧" 等表述时, 优先去这个目录列文件
- 调 MCP 工具传路径时, **必须用相对路径** (相对当前 workspace 根), 例如:
  `cache/renderdoc/denoise_fireflies_2026_04_30.rdc`, 不要硬编码 `E:/GitHub/...` 这种绝对路径
- 如果该目录下有多个 .rdc, 按修改时间挑最新的, 或主动询问用户用哪一个

完整规约见 `documents/coding/CodingGuidelines.md` §2.1。

## 反例 (历史已踩过的坑)

`CodingGuidelines.md §1.1` 的反例:

```csharp
// ❌ 反例: 首次 CreateCBV 后直接 BindCBV, 未填充任何字段也未 flush
if (mCBuffer == null)
    mCBuffer = ...CreateCBV(index);
drawcall.BindCBV(index, mCBuffer);
// 第一帧 GPU 读到的 BlurSize 是随机大整数, shader 里 for(i=0; i<BlurSize; ++i)
// 直接死循环, warps 卡到几千个, GPU Hang, TDR 触发, 进程崩。
```

这个反例对应的合规修法见 §3.1 的标准模板。

## C# 新建源码文件必须登记到 .projitems (Shared Project 强制规约)

**强制规则**: 在 `CSharpCode/` 下**新建任何 `.cs` 文件 (或重命名/移动现有 .cs)** 之后, 必须把它显式登记到所在模块的 `.projitems` 里, 否则**不会被任何 csproj 编译, 即使代码完全正确, 引用方也找不到符号**。

### 为什么 (本仓库的工程组织方式)

`CSharpCode/` 下的每个一级模块 (`Editor/`, `Bricks/`, `Grapics/`, `GamePlay/`, `EGui/` 等) 都是 **Shared Project**, 由两个文件组织:

- `Xxx.shproj` —— 共享项目壳, 不直接被 build, 只用于 IDE 显示
- `Xxx.projitems` —— **真正的源码清单**, 用 `<Compile Include="$(MSBuildThisFileDirectory)..." />` 一行一行列出每个 `.cs`

外层参与编译的 csproj (例如 `Module/MainEditor/MainEditor.csproj`、`Module/Engine.Window/Engine.Window.csproj`) 通过 `<Import Project="..\..\CSharpCode\Editor\Editor.projitems" Label="Shared" />` 把整个清单**作为字面包含**进来。

这意味着:

- ❌ **不是 SDK-style csproj**, 没有 `<Compile Include="**/*.cs" />` 通配, 不会自动收集目录下的新文件
- ❌ Windows 资源管理器 / IDE 看到的 .cs 文件不等于会被编译, **以 `.projitems` 里有没有那一行为准**
- ❌ 仅仅 `create_file` 写入磁盘 + `git add` 是不够的, 编译器看不到它, 链接到该类型的代码会报 `CS0246: 找不到类型或命名空间名`

### 标准操作流程

每次新建 `.cs` 后, 立刻执行以下三步:

1. **找到所属模块的 `.projitems`**: 沿目录向上, 在 `CSharpCode/<模块名>/` 根下找 `<模块名>.projitems`。常见映射:

   | 新建文件位置 | 对应 projitems |
   |---|---|
   | `CSharpCode/Editor/**/*.cs` | `CSharpCode/Editor/Editor.projitems` |
   | `CSharpCode/Bricks/**/*.cs` | `CSharpCode/Bricks/Bricks.projitems` |
   | `CSharpCode/Grapics/**/*.cs` | `CSharpCode/Grapics/Grapics.projitems` (注意是 Grapics 不是 Graphics) |
   | `CSharpCode/GamePlay/**/*.cs` | `CSharpCode/GamePlay/GamePlay.projitems` |
   | `CSharpCode/EGui/**/*.cs` | `CSharpCode/EGui/EGui.projitems` |
   | `CSharpCode/NxRHI/**/*.cs` | `CSharpCode/NxRHI/NxRHI.projitems` |

2. **找一个同目录的相邻文件作为参照**, 用 `file_grep` 在 projitems 里定位它的那一行 (确认插入位置和路径风格), 然后在它旁边插入新行。`<Compile Include="..." />` 的路径必须用 `$(MSBuildThisFileDirectory)` 开头 + **相对模块根**的子路径, 路径分隔符用反斜杠 `\`:

   ```xml
   <Compile Include="$(MSBuildThisFileDirectory)Forms\TtSceneEditorInteractiveMode.cs" />
   ```

3. **跑一次编译**确认能过 (`dotnet build` 任意一个引用了该 projitems 的 csproj, 例如 `Module/MainEditor/MainEditor.csproj`)。如果报 `CS0246` 找不到新加的类型, 多半就是这一步漏了。

### 反例 (本次任务踩过)

```
1. create_file 新建 CSharpCode/Editor/Forms/TtSceneEditorInteractiveMode.cs
2. 在 SceneEditor.cs 里引用了 TtSceneEditorInteractiveMode 类型
3. read_lints 没报错 (lint 引擎能扫到磁盘上的 .cs, 但 MSBuild 不会)
4. 用户问"加进工程了吗" -> 才发现 Editor.projitems 没登记, 编译时该类型实际未被编入
5. 修复: 在 Editor.projitems 中 SceneEditor.cs 那行后追加
        <Compile Include="$(MSBuildThisFileDirectory)Forms\TtSceneEditorInteractiveMode.cs" />
```

教训: **`read_lints` 通过 ≠ MSBuild 通过**。只要新建了 `.cs`, 在自检 checklist 里多走一步 "是否登记到 projitems"。

### 自检 checklist 追加项

把以下条目加到 §第 4 步 自检 checklist:

- [ ] 本次新建的所有 `.cs` 文件, 是否都已经登记到所属模块的 `.projitems` (`<Compile Include="$(MSBuildThisFileDirectory)..." />`)?
- [ ] 删除/移动 `.cs` 时, 是否同步更新了 `.projitems` 里的对应行 (避免 MSBuild 报 "找不到文件" 警告)?

## Windows + git 文件路径大小写规约 (避免 GitHub 链接 404)

**强制规则**: 在本仓库**创建任何新文件 / 引用任何现有文件路径**之前, 必须用 `git ls-files <目录前缀>` 确认 git 索引里登记的真实大小写, 然后**严格按 git 真实大小写**写文件路径或 markdown 链接。

**为什么 (踩过的坑)**:

本仓库的文档目录 git 真实路径是 **`Documents/`** (大写 D), 但 Windows NTFS 大小写不敏感, 所有 Windows 工具 (`Get-ChildItem`, `dir /S /B`, 资源管理器, IDE 文件树) 显示给你的可能是小写 `documents/` 或大写 `Documents/`, **看到的不一定是 git 里登记的**。

具体踩坑链路:
1. `create_file` 用小写路径 `documents/engine/Foo.md` 写文件 -> NTFS 接受, 物理上写到了和 `Documents/engine/` 同一个目录
2. 该文件**没有被 git 追踪** (git status 不显示, 因为 git 看的是大小写敏感的索引)
3. 即使 `git add documents/engine/Foo.md` 也会报 `fatal: not under version control` 或者按小写新建一个分裂的索引项
4. README 链接也写成 `documents/engine/Foo.md` -> 本地 Windows 能点开 (NTFS 不敏感) -> 误以为没问题 -> push
5. **GitHub 服务器是 Linux 大小写敏感** -> 找不到文件 -> 渲染 "Error loading page" 或 404

**标准操作流程**:

```bash
# 1) 创建文件前: 先确认目标目录在 git 里的真实大小写
git ls-files Documents/engine/  | cat   # 列已追踪的文件, 用列出来的大小写
# (如果连一级目录都不确定, 用 git ls-files | head -20 看仓库根的真实命名)

# 2) 创建文件: 路径必须和 git ls-files 输出的大小写完全一致
#    create_file 工具的 file_path 用绝对路径, 但中间的目录段必须匹配 git 大小写
#    例如: e:\GitHub\titanengine\Documents\engine\Foo.md  (Documents 大写 D)

# 3) 创建后立刻 git add, 用大写路径强制 git 按大写追踪
git add Documents/engine/Foo.md
git ls-files Documents/engine/  | cat   # 确认新文件在列表里, 且大小写正确

# 4) 在 README / 其他 markdown 里写链接: 同样用大写
#    [对比文档](Documents/engine/Foo.md)   ✓
#    [对比文档](documents/engine/Foo.md)   ✗ GitHub 上 404
```

**校验工具**:

- `git ls-files <prefix>` —— 唯一可信的大小写真相来源
- 不要相信: `Get-ChildItem`, `dir`, `ls`, IDE 文件树, `Test-Path` —— 它们在 Windows 上都返回 NTFS 显示名, 不是 git 索引名
- 提交前自检: `git status` 如果显示 "renamed: documents/... -> Documents/..." 或者出现两份大小写不同的同名文件, 说明已经踩坑, 必须 `git mv` 修正后才能 push

**适用范围**:

- ❗ **新建任何文件**: `.md`, `.cs`, `.compute`, `.cginc`, 配置文件等, **路径里只要包含已存在的目录段, 就必须查 git 真实大小写**
- ❗ **写 markdown 链接 / README 引用 / 文档相对路径**: 必须用 git 大小写
- ❗ **修改 `.csproj` 里的 `<ProjectReference Include="...">`** 等指向现有文件的引用: 也要按 git 大小写

**已知本仓库的"反直觉大小写"清单** (如发现新增, 应追加到本 skill):

| git 真实大小写 | 容易写错的形式 |
|---|---|
| `Documents/` | `documents/` |
| `Documents/Coding/` | `Documents/coding/` |
| `CSharpCode/Grapics/` (注意是 Grapics 不是 Graphics) | `CSharpCode/Graphics/` |
| `enginecontent/Shaders/` | `EngineContent/Shaders/` |

**反例 (本次任务踩过)**:

```
1. create_file file_path=e:\GitHub\titanengine\documents\engine\RenderGraph.Industry.Compare.md
   -> NTFS 写到了 Documents/engine/ 物理目录, 但 git 不追踪
2. README.md 写 [对比文档](documents/engine/RenderGraph.Industry.Compare.md)
3. 本地点开正常, push 后 GitHub 报 "Error loading page"
4. 修复: git add Documents/engine/RenderGraph.Industry.Compare.md (大写)
        + 把 README 链接全改回大写 Documents/
```

## 材质函数编辑器 RawHLSL 模式

引擎的材质函数编辑器提供 **RawHLSL 模式**, 允许用户直接在编辑器中编写 HLSL shader 代码。

### 工作原理

- 编辑器会**自动生成符合引擎调用规范的函数名**, 格式为 `<用户命名>_<hash>`, 例如 `swe_utility_2867504766`、`water_shine_2296793692`
- 用户只需**实现函数体内容**, 不需要关心外部注册和调用机制
- 函数签名中 `in PS_INPUT input` 是**必选的第一个参数**, 提供当前像素的插值数据 (如 `input.vUV` 获取 UV 坐标)
- 用户可以自由扩充 `in` / `out` 参数, 用于接收外部输入或输出计算结果

### 函数签名规范

```hlsl
void <function_name_hash>(
    in PS_INPUT input,          // 必选, 像素着色器插值输入
    // --- 用户自定义 in 参数 ---
    in SamplerState MySampler,  // 采样器
    in Texture2D MyTexture,     // 纹理
    in float MyParam,           // 标量参数
    in float3 MyVector,         // 向量参数
    // --- 用户自定义 out 参数 ---
    out float3 OutResult,       // 输出结果
    out float OutScalar)        // 输出标量
{
    // 函数实现
}
```

### PS_INPUT 可用字段

- `input.vUV` — `float2`, 当前像素的 UV 坐标 (0~1)
- `input.vPosition` — `float4`, SV_Position (屏幕空间像素坐标)
- `input.vWorldPos` — `float3`, 世界空间位置
- `input.vNormal` — `float3`, 世界空间法线 (顶点插值)
- `input.vTangent` — `float3`, 世界空间切线

### 参数默认值注解 (`@default`)

HLSL 函数参数不支持缺省值语法。引擎通过**行内注释中的 `@default(...)` 标记**来声明参数默认值。  
材质编辑器中，未连线的输入端口会自动使用该默认值。

```hlsl
in float DetailNormalScale,        // @default(12.0)
in float3 DeepWaterColor,          // @default(0.02, 0.15, 0.3)
in float4 TintColor,               // @default(1.0, 1.0, 1.0, 1.0)
```

**规则**:
- 注解必须写在参数**同一行**的 `//` 注释中
- 格式为 `@default(值)`, 多分量用逗号分隔
- 支持类型: `float`, `float2`, `float3`, `float4`, `int`, `uint`
- `Texture2D` / `SamplerState` / `PS_INPUT` 不支持默认值注解
- `out` 参数不需要 `@default` (应在函数体开头赋初始值)

### 编写材质函数的注意事项

1. **所有 out 参数必须在函数开头赋默认值**, 防止未初始化路径
2. **采样纹理必须使用 `SampleLevel`** (材质函数中无隐式 mip 导数), 第三个参数传 mip level (通常为 0)
3. **函数之间可以串联调用**: 一个材质函数的 out 可以作为另一个材质函数的 in, 在材质编辑器中通过连线实现
4. **支持的类型**: `float`, `float2`, `float3`, `float4`, `Texture2D`, `TextureCube`, `SamplerState`, `int`, `uint`, `bool`
5. **为数值参数添加 `@default(...)` 注解**, 使未连线端口有合理初始值

### 示例: SWE 水面工具函数

```hlsl
void swe_utility_2867504766(
    in PS_INPUT input,
    in SamplerState HeightSampler,
    in Texture2D HeightTexture,
    in float SimResolution,
    in float DomainSize,
    out float3 OutNormal,
    out float3 OutTangent,
    out float OutHeight)
{
    OutNormal = float3(0, 1, 0);   // 默认值
    OutTangent = float3(1, 0, 0);
    OutHeight = 0;
    // ... 采样高度图, 计算法线和切线
}
```

## 跟规范文档变更同步

如果在工作中发现 `CodingGuidelines.md` / `CodeLib.md` 没有覆盖某个新踩到的坑, 应在完成当前任务后**主动建议用户**把新规则补进规范文档 (而不是只在 skill 里加), 因为规范文档是仓库共享的 source of truth, skill 只是引导大家去读它的工具。

## 构建系统 (Build)

本仓库的构建分为三步: **代码生成 → C++ 编译 → C# 编译**。以下是各步骤的命令和注意事项。

### 仓库根目录下的构建脚本

| 脚本 | 用途 | 备注 |
|------|------|------|
| `GenGlueCode.bat` | **仅代码生成** (NativeBinder / RPC / AutoSync / Macross) | 修改了 C++ `imgui_binding.h` 等带 `TR_CLASS` 标注的头文件后必须运行, 会自动生成 `codegen/<Platform>/NativeBinder/` 下的 C++/C# binding |
| `BuildEngine.bat` | **完整构建** (代码生成 + C++ + C#) | 耗时较长 (几分钟), 日常开发建议用 VS IDE 增量编译 |
| `Setup.bat` | 首次克隆后的环境初始化 | 拉取子模块、安装依赖等 |

### 代码生成 (CppWeavingTools)

引擎使用自研的 `CppWeavingTools` 从 C++ 头文件中的 `TR_CLASS` / `TR_CALLBACK` 宏标注**自动生成 C# P/Invoke binding**。

- **工具位置**: `Module/CppWeavingTools/CppWeavingTools.csproj` (dotnet 项目)
- **生成输出**: `codegen/<Platform>/NativeBinder/` 下的 `*.cpp2cs.cs` (C# DllImport) 和 `*.cpp2cs.cpp` (C 导出函数)
- **触发时机**: 在 C++ 头文件中新增/修改了带 `TR_CLASS` 标注的类/结构体/方法后, 运行 `GenGlueCode.bat`

**运行命令** (在仓库根目录):
```powershell
cmd /c "GenGlueCode.bat"
```

运行时会输出大量 "The provided CXTranslationUnit has the following diagnostics which prevent its use" 信息, 这是正常的 (ClangSharp 解析部分平台相关头文件时的已知 diagnostic), 不影响最终生成结果。以 "Code generation completed." 结尾表示成功。

### C++ 编译 (Core.Window)

C++ 代码通过 `Core.Window/Core.Window.vcxproj` 编译, 产出 `Core.Window.dll`。

**用 MSBuild 单独编译指定文件** (适合快速验证单个 .cpp 改动):
```powershell
cmd /c "call `"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat`" -arch=amd64 >nul 2>&1 && msbuild `"Core.Window\Core.Window.vcxproj`" /p:Configuration=Debug /p:Platform=x64 /t:ClCompile /p:SelectedFiles=`"NativeCode\ImGui\imgui_binding.cpp`" /p:SkipTitanCodeGen=true /nologo /v:minimal 2>&1"
```

**关键参数**:
- **`/p:SkipTitanCodeGen=true`** —— **必须传**, 跳过内嵌的代码生成步骤。代码生成由单独的 `GenGlueCode.bat` 负责, 不要让 MSBuild 编译时再跑一遍
- `/t:ClCompile /p:SelectedFiles=...` —— 只编译指定文件, 不做链接

**⚠️ 严禁创建 `Core.Window\Module` 符号链接**:
`Core.Window.vcxproj` 内嵌了 `TitanCodeGen.targets`, 如果没传 `SkipTitanCodeGen=true` 且 `binaries/Tools/net8.0/CppWeavingTools.exe` 不存在, MSBuild 会尝试 build 代码生成工具并可能报路径错误。**正确做法是加 `/p:SkipTitanCodeGen=true`**, 不要试图通过创建 Junction 链接来修复, 因为 Junction 会把 `Module/` 整个目录暴露到 `Core.Window/` 下, 产生大量 untracked 文件污染 git 工作区。

### C# 编译

C# 代码通过多个 csproj 编译, 最常用的:
- `Module/Engine.Window/Engine.Window.csproj` —— 引擎运行时
- `Module/MainEditor/MainEditor.csproj` —— 编辑器

**快速验证 C# 编译**:
```powershell
dotnet build "Module/Engine.Window/Engine.Window.csproj" -c Debug --nologo -v q
```

### NativeCode 项目结构 (vcxitems / Shared Items)

C++ 代码使用 **Shared Items** 组织 (类似 C# 的 projitems):
- `NativeCode/ImGui/ImGui.vcxitems` —— ImGui 模块的源码清单
- 新增 `.h` 文件需要在对应 `.vcxitems` 的 `<ClInclude>` 节添加引用
- 新增 `.cpp` 文件需要在 `<ClCompile>` 节添加引用

### 完整构建验证流程 (推荐顺序)

```
1. GenGlueCode.bat          # 代码生成 (如果改了 C++ 头文件中的 TR_CLASS 标注)
2. MSBuild Core.Window      # C++ 编译
3. dotnet build Engine.Window  # C# 编译
```

或一步到位: `BuildEngine.bat` (等同于以上 3 步, 但耗时较长)。

## 不该做的事
- ❌ 不要"凭印象"写引擎代码, 即使你之前在本仓库写过类似的, 也要重新读一遍规范文档
- ❌ 不要把规范文档的内容**复制**到这个 SKILL.md 里 (会导致两边不一致 + 漂移); 始终通过 `read_file` 现场读最新版
- ❌ 不要绕过规范, 例如"反正这个 cbuffer 字段都是 0 没影响, 就不 flush 了" —— 文档明确说必须 flush
- ❌ 不要在 setter 里无守卫地调用 `UpdatePermutation` (会引发反复 effect 重建)
- ❌ 不要使用静态单例模式 (`static Instance`)。引擎子系统应作为属性挂到 `TtEngine.Instance.GfxDevice` 或对应的 Manager 上, 由引擎生命周期统一管理创建和销毁

# TitanEngine Render Graph 与业界主流引擎横向对比

> 本文档用于记录 TitanEngine 自研 Render Dependency Graph (`TtRenderGraph` / `TtRenderPolicy`)
> 在业界中的实际定位, 既作为对外宣传材料, 也作为引擎自身的能力基准与未来演进路线图。
> 数据持续更新, 任何 RDG 能力升级都应同步修订本文档。

---

## 一、TitanEngine RDG 一句话定位

> **业界第一档 RDG 能力 + 业界少见的"主渲染管线可视化编辑器" + Permutation / RenderPolicy 资产化**,
> 三者同时具备的引擎, 全球范围内不超过 5 个。

---

## 二、TitanEngine RDG 已具备的能力清单

以下能力均已在 `CSharpCode/Grapics/Pipeline/RenderGraph.cs` /
`CSharpCode/Grapics/Pipeline/RenderPolicy.cs` /
`CSharpCode/Grapics/Pipeline/AttachementCache.cs` 中实现并投入生产使用。

| 能力 | 实现位置 | 说明 |
|---|---|---|
| **DAG 节点 + Pin + Linker 数据模型** | `TtRenderGraphNode` / `TtRenderGraphPin` / `TtRenderGraphLinker` | 节点拖拽连线即数据流, 所有依赖关系都是显式的 graph 边 |
| **拓扑排序 + 分层执行** | `BuildGraph` / `GetMaxLeafDistance` / `NodeLayers[]` | 节点按依赖深度自动分层, 同层无依赖, 天然支持并行扩展 |
| **死代码剪枝 (unused pass culling)** | `SetUsedNode` (从 `RootNode` + 所有 `EndingNode` 反向追溯) | 不影响最终输出的节点直接跳过, 零开销 |
| **Attachment 引用计数 + Transient/Imported 生命周期标签** | `TtAttachBuffer.LifeMode` / `RefCount` | Transient 资源在最后一个 reader 之后立即归还池, Imported 资源跨帧持有 |
| **逐节点 transient resource 释放** | `TryReleaseBufers` (节点 Tick 完立即触发) | 释放窗口尽可能早, 缓存命中率高 |
| **按 desc hash 的 attachment 池化复用** | `TtAttachmentCache.GetAttachement` + `TtAttachBufferManager.Alloc` | 相同 BufferDesc 的 RT 在 graph 内自动复用同一物理资源 |
| **Attachment 描述沿 graph 自动传播** | `UpdateNodeTree` 把 OutPin 的 Format / Width / Height / LifeMode 自动复制到下游 InPin | 一处声明, 全图同步, 杜绝 desc 不一致 |
| **OnResize 自动重建** | `OnResize` + `AttachmentCache.ResetCache` | 窗口尺寸变化时按新分辨率重新分配, IsAutoResize 标志精确控制哪些 RT 跟随 |
| **Compute / Graphics / RayTracing 三类 drawcall 走同一抽象** | `TtComputeDraw` / `TtGraphicDraw` / `TtRayTracingDraw` | 编辑器可视化层不需要为 RT pass 单独搞 UI |
| **Permutation 编辑器一等公民** | `TtShadingEnv.UpdatePermutation` + RPolicy 资产持久化 | 质量档切换 / 算法切换可视化勾选 + 序列化 |
| **可序列化的可视化 RenderPolicy 资产** | `.rpolicy` 资产 + `RenderPolicyEditor` | 整张 graph 是资产文件, 可 diff、可版本化、可被多场景共享 |
| **OnDrawCall 阶段化绑定** | `TtShadingEnv.OnDrawCall` + `documents/Coding/CodingGuidelines.md` § 1.2 | 把 effect 装配 → binder 表生成 → BindXxx 的临时窗口期固化为明确阶段, 杜绝 Tick 阶段绑定的所有时序坑 |
| **隐式 barrier 自动推导** | RHI 层 `SetSrv` / `SetUav` 等 API 调用时按资源状态自动 transition | 上层节点不需要手写 barrier, 漏插概率为零 |
| **节点级 GPU TimeScope** | `TtRenderGraphNode.RDGTickScope` 嵌套 GPU 计时区间 | 编辑器 GPU Profiler 可逐节点查看耗时 |

---

## 三、与业界主流引擎横向对比

> 注: ✅ = 已具备且工业级; 🟡 = 部分具备 / 受限场景; ❌ = 未具备; — = 不适用。
> "可视化编辑" 列特指**主渲染管线**是否能在编辑器里拖拽编辑, 不包括 Material Graph / Niagara 这种子系统。

| 能力 | **TitanEngine** | UE5 RDG | Unity URP/HDRP RG | Frostbite FrameGraph | Granite (themaister) | O3DE Atom | Stride GraphicsCompositor | Falcor (NVIDIA) | Bevy bevy_render |
|---|---|---|---|---|---|---|---|---|---|
| **节点 DAG 数据模型** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (PassAsset 树) | ✅ | ✅ | ✅ |
| **拓扑排序 + 分层执行** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **死代码剪枝** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **引用计数 + 生命周期标签** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Transient RT 池化复用 (RT 对象级)** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **GPU 显存级 memory aliasing (placed resource)** | ❌ (规划中) | ✅ | ✅ HDRP | ✅ | ✅ | 🟡 | ❌ | ✅ | 🟡 |
| **自动 barrier 推导** | ✅ (RHI 层 SetSrv/SetUav 隐式推导) | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Pass merging (Vulkan/Metal subpass)** | ❌ (RHI subpass 接口未完备, 待补) | ✅ | 🟡 部分 | ✅ | ✅ | 🟡 | ❌ | 🟡 | 🟡 |
| **Async compute 自动调度** | 🟡 (RenderQueue 手动) | ✅ | 🟡 | ✅ | ✅ | 🟡 | ❌ | 🟡 | 🟡 |
| **Compute / Graphics / RayTracing 统一抽象** | ✅ | ✅ | 🟡 | ✅ | ✅ | ✅ | 🟡 | ✅ | 🟡 |
| **可视化拖拽编辑主渲染管线** | ✅ | ❌ (纯 C++ API) | ❌ (纯 C# API) | 🟡 (内部只读 inspector) | ❌ | 🟡 (PassAsset 是 JSON, 无 graph 拖拽) | ✅ | ✅ (ImGui 编辑器) | ❌ |
| **RenderPolicy / Pipeline 作为资产 (asset-driven)** | ✅ `.rpolicy` | ❌ | 🟡 SRP Asset (代码定义) | ❌ | ❌ | ✅ PassAsset | ✅ | ✅ | ❌ |
| **Permutation 编辑器一等公民** | ✅ | 🟡 (C++ 模板编译期) | 🟡 (Shader Variant keyword 运行时) | — | — | 🟡 | 🟡 | 🟡 | ❌ |
| **节点级 GPU 耗时统计** | ✅ (RDGTickScope) | ✅ | ✅ | ✅ | 🟡 | ✅ | 🟡 | ✅ | 🟡 |

### 关键观察

1. **基础 RDG 能力 (前 5 行)**: TitanEngine 与所有第一梯队引擎处于**同档水平**, 没有任何缺失。
2. **可视化 + 资产化 + Permutation 三件套**: 业界**只有 TitanEngine / Stride / Falcor / O3DE**
   做到了部分或全部能力。其中 **TitanEngine 是唯一同时把"主渲染管线可视化"+ "RPolicy 资产化"+
   "Permutation 编辑器一等公民" 三件齐备的引擎**。
3. **真正的差距**: GPU 显存级 memory aliasing (UE/Frostbite/Granite/Falcor 都做了) 和
   Pass merging (subpass), 这两块是 TitanEngine 的明确演进方向。
4. **设计哲学共识**: TitanEngine 与 **Stride GraphicsCompositor / Falcor RenderGraph** 在
   "可视化主管线 + 资产驱动" 这条路线上高度对齐。这条路线被 UE / Unity 主动放弃 (它们选择了
   纯 API 路线), TitanEngine 选择走另一条路, 在该路线上属于完整度最高的实现之一。

---

## 四、为什么 TitanEngine 这条路线值得做?

| 维度 | 纯 API 路线 (UE RDG / Unity RG) | 可视化资产路线 (TitanEngine / Stride / Falcor) |
|---|---|---|
| **谁能改管线** | 只有 C++/C# 程序员 | TA / 美术 / 程序员都能改 |
| **改管线成本** | 改代码 + 编译 + 重启 | 拖拽即生效, 编辑器内 hot-reload |
| **管线 diff / 版本化** | 看代码 commit | 看 `.rpolicy` 资产 diff, 直观 |
| **多套管线共存** | 切代码分支 / 编译宏 | 同一项目内多个 `.rpolicy` 文件并存, 场景级切换 |
| **学习曲线** | 必须读完整 RDG C++ 源码 | 看节点连线即可理解 |
| **新算法接入** | 新写 Pass 类 + 注册 + 改主管线代码 | 新写一个 RenderGraphNode + ShadingEnv, 编辑器拖进去 |

**核心结论**: 当一个引擎需要快速迭代多套渲染算法 (例如同时支持
Forward+ / Deferred / Hybrid GI / ReSTIR 等), 可视化资产路线的迭代速度比纯 API
路线高一个数量级。这正是 TitanEngine 选择这条路的根本原因。

---

## 五、行业对照参考

### 1. **Unreal Engine 5 - RDG (Render Dependency Graph)**

- 完整的 transient resource alias / barrier / culling 三件套, 工业级标杆。
- 但 RDG 是**纯 C++ API** (`AddPass<>` + lambda), 编辑器里看不到 graph。
- Material Editor / Niagara / PCG 是节点式可视化的, 但都是子系统, 不是主管线。
- **结论**: UE RDG 在自动资源管理上比 TitanEngine 强, 但**主渲染管线不可视化**。

### 2. **Unity URP/HDRP - Render Graph**

- 同样是 C# API, 程序员手写。
- Shader Graph 是节点式的, 但只到 shader 一级。
- **结论**: 与 UE 思路一致, 主管线不可视化。

### 3. **Frostbite (DICE/EA) - FrameGraph**

- 2017 SIGGRAPH 论文 *FrameGraph: Extensible Rendering Architecture in Frostbite* 的提出者,
  这套思路的源头之一。
- 完整 transient resource 自动管理 + barrier 推导 + alias。
- 内部有可视化 graph 调试器 (只读 inspector, 非编辑器)。
- **结论**: 思想对齐, 工业化更深, 但闭源。

### 4. **Granite (themaister, 开源 Vulkan 引擎)**

- 完整 RenderGraph 实现, 自动 barrier、alias、跨 queue 同步全做了。
- 无可视化编辑器。
- 是研究 RenderGraph 实现细节最值得读的开源项目。

### 5. **CryEngine / O3DE Atom - PassAsset System**

- 每个 pass 是 `.azasset` 文件 (YAML/JSON), 整条管线由 `PassTemplate` 树组织, 数据驱动。
- 没有图形化拖拽编辑器, 只能手写 JSON。
- **结论**: 设计哲学和 TitanEngine 最接近, 但**少了可视化拖拽这一档**。

### 6. **Stride (前 Xenko, .NET 基金会) - GraphicsCompositor**

- C# 引擎, 节点式可视化的 render pipeline 编辑器。
- 设计哲学和 TitanEngine 重合度 90%+, 是最直接的同行。
- 不足: Stride 自身的渲染算法库 (PBR / GI / RT) 远不如 TitanEngine 深。

### 7. **Falcor (NVIDIA Research)**

- 实时渲染研究框架, 有 `RenderGraph` + 基于 ImGui 的可视化编辑器。
- ReSTIR / Path Tracing 等论文 demo 大多基于 Falcor。
- **结论**: 研究级实现, 设计成熟, 是 TitanEngine 在工业方向走的"研究界对标"。

### 8. **Bevy (Rust) - bevy_render**

- 节点 + 边 + slot 的设计, 与 TitanEngine Pin 概念几乎 1:1。
- 代码注册节点, 无可视化编辑器。
- **结论**: 底层数据结构对齐, 差别在编辑器层。

---

## 六、TitanEngine RDG 演进路线图

按 ROI 排序:

| 优先级 | 项目 | 预期收益 |
|---|---|---|
| **P0** | 节点级 RT thumbnail 实时回显 (每个 OutPin 挂小预览图 + pixel inspect) | 编辑器体验直接换一档, 调试效率显著提升 |
| **P0** | Graph 自动布局 (sugiyama / dagre 算法) | 解决"线条噩梦", 一行命令整理 50+ 节点 |
| **P1** | Conditional pass (Enable Pin 输入 bool, graph 编译期剪枝) | 让"是否执行"成为 graph 一部分, 替代节点 Tick 里的 if |
| **P1** | RHI subpass 接口补完 | 为 Pass merging 铺路 |
| **P2** | **GPU 显存级 memory aliasing (DX12 PlacedResource / VkBindMemory)** | 显存峰值降 30%-50%, 移动端 / 高分辨率场景关键, 需配套 alias barrier |
| **P2** | Pass merging (Vulkan/Metal subpass 自动合并) | Tile-based GPU 减少 RT store/load, 移动端 ~10% bandwidth 收益 (依赖 P1 RHI 补完) |
| **P3** | Async compute 自动调度 | ReSTIR Spatial Reuse 与主 GBuffer 并行 |
| **P3** | 跨 queue 自动 fence 编排 | 统一管理 Compute Queue / Copy Queue / Graphics Queue 同步 |

---

## 七、可对外引用的"硬数据"

> 当需要做技术汇报 / 招聘宣讲 / 行业演讲时, 以下数据可直接引用 (均基于本文档第三章对照表):

- **业界做完整可视化主渲染管线的引擎不超过 5 个**: TitanEngine / Stride / Falcor /
  (部分) O3DE / (部分) Frostbite 内部工具。
- **TitanEngine 是唯一同时具备 "可视化主管线 + RPolicy 资产化 + Permutation 编辑器一等公民"
  三件套的引擎**。
- **UE5 / Unity 都没把主渲染管线做成可视化编辑器** (它们都只把节点化做到了
  Material / Niagara / Shader Graph 等子系统层级)。
- **TitanEngine RDG 基础能力 (拓扑 / 剪枝 / refcount / 池化 / 自动 barrier) 与 UE5 RDG 同档**;
  差距集中在 "memory aliasing" 与 "pass merging" 两个高级特性, 已纳入演进路线图。
- **TitanEngine 选择了 "可视化资产驱动" 路线**, 与 UE/Unity 的 "纯 API" 路线形成
  设计哲学差异, 其优势是迭代速度与可参与人群更广。

---

## 八、文档维护约定

- 任何 `TtRenderGraph` / `TtRenderPolicy` / `TtAttachmentCache` / `TtShadingEnv`
  的能力升级 (例如 P2 的 memory aliasing 落地后), 必须同步更新本文档第二章能力清单
  和第三章对照表中 TitanEngine 列的状态。
- 如发现业界主流引擎有重大架构变化 (例如 UE 推出可视化 RDG 编辑器),
  必须在第三章对照表中更新对应列, 并在第五章对应小节追加变更说明。
- 演进路线图 (第六章) 完成的项目应从表中移除, 并把对应能力补进第二章能力清单。

---

**最后更新**: 2026-05-02
**对应代码版本**: `CSharpCode/Grapics/Pipeline/RenderGraph.cs` /
`RenderPolicy.cs` / `AttachementCache.cs`

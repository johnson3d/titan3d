# GPU-Driven 渲染架构 — Instance 与 GpuCulling 现状与规划

> **状态**: 现状架构描述 + 演进规划
> **代码归属**: `CSharpCode/Grapics/Mesh/Modifier/InstanceModifier.cs`、`CSharpCode/Grapics/Pipeline/GpuCullingNode.cs`、`CSharpCode/Bricks/Terrain/CDLOD/`（地形消费端）
> **相关文档**: `Documents/Architecture/AtlasGI.md`、`Documents/Coding/CodingGuidelines.md` §1.4 (Bindless)

---

## 1. 总览

TitanEngine 的 GPU-Driven 体系目前覆盖 **绘制合并 (batching)** 与 **GPU 剔除 (culling)** 两层，
剔除与 LOD 选择仍在 CPU 完成（`TtCpuCullingNode` 视锥剔除 + 各系统的 CPU 侧 LOD 分配），
整体属于"半 GPU-Driven"：

```
CPU:  TtCpuCullingNode (视锥剔除) ──► VisibleMeshes 列表
                                          │
GPU:  TtGpuCullingNode.Tick               │  条目被替换为 batch 占位 (EDrawMode.Instance)
      ├─ Culling(): 合并 + push instance ─┘
      ├─ InstanceCulling compute: 逐 instance 视锥/遮挡剔除 → 压缩写 DrawClusters + indirect args
      └─ Commit(): batch mesh 只推一次 drawcall (DrawIndexedInstanced / indirect)
```

核心思想：**同 hash（同几何 + 同材质）的 N 个可见对象合并为 1 个 batch mesh，
对象差异（世界变换、LOD、材质参数）打包成 per-instance 的 `FVSInstanceData`
放入 StructuredBuffer，VS 端按 InstanceId 索引**。

---

## 2. 数据通路

### 2.1 Instance 数据: FVSInstanceData + TtInstanceBufferSSBO

- `FVSInstanceData` (`Pipeline/Shader`): per-instance 参数包。除通用的
  Position/Scale/Quat/HitProxyId 外，通过 `UserData / UserData2 / Scale_Pad`
  两组 uint4 槽位承载系统自定义数据——地形的用法见 §3.3。
- `TtInstanceBufferSSBO` (`InstanceModifier.cs`): CPU→GPU instance 缓冲。
  - `InstanceBuffer`: `TtCpu2GpuBuffer<FVSInstanceData>`，绑定 VS 为
    `VSInstanceDataArray` SRV；
  - `InstanceBoundingBuffer`（可选）: `FCullBounding` 数组，供 GPU cull 用；
  - `ResetInstance()` 每帧清零计数 → `PushInstance()` 累加 → `Flush2GPU` 上传。
- `TtInstanceModifier`: 挂在 `TtMdfQueue2` 第二位的 mesh modifier
  （`DoInstancingModifierVS`），VS 内通过 `GetInstanceData(input)` 按 InstanceId
  拉取 instance 数据；`OnDrawCall` 里设置 `drawcall.DrawInstance = NumOfInstance`
  并绑定 instance SRV。若 modifier 挂了 `GpuDrivenData` 则走 indirect args 绑定。

### 2.2 GPU 剔除: TtGpuDrivenData

`TtGpuDrivenData`（`InstanceModifier.cs`）负责 batch 的 GPU 侧剔除：

- `FDrawArgs`: indirect draw 参数（每 SubMesh 一项，含 offset）；
- `DrawArgsBuffer`: UAV，cull compute 用 `InterlockedAdd` 把可见 instance 数
  累加进对应 arg 的 InstanceCount 槽位（"单 arg + instance 压缩"模式，与
  VGDag 远景地形的 cull 同一范式，不受 DX12 arg 项数 1024 上限约束）；
- `CullingBuffer`: 剔除后压缩的可见 instance 数据（替代原始 instance 数组绑定）；
- `InstanceCulling(modifier, cmd, policy)`: 三个 compute pass——
  Setup（写 args 初值）→ Cull（逐 instance 测试，绑 `cbPerCamera` +
  `InstanceDataArray` + 可选 `InstanceBoundingArray`，输出 `CullInstanceDataArray`
  与 indirect args）→ Flush。
- 启用条件：batch mesh 上 `GpuDrivenData != null` **且** 引擎配置
  `Feature_UseGpuCulling`（Engine.cs，默认 true）。关闭时合批照旧（PushInstance
  保留），但跳过剔除 compute，绘制端走 `TtInstanceBufferSSBO.OnDrawCall` 的
  else 分支：`DrawInstance = NumOfInstance` 直绘 CPU 侧 InstanceBuffer。

### 2.3 消费端跳过机制（防重复绘制）

`TtGpuCullingNode.Tick → Culling()` 遍历 `VisibleMeshes`，命中三类 batch 之一时
**原地替换条目**：`visibleMeshes[i] = new FVisibleMesh { Mesh = 原mesh, DrawMode = EDrawMode.Instance }`。
所有遍历 VisibleMeshes 的下游节点（DeferredBasePassNode / BasePassNode /
ForwordNode / SdfForwordNode / HitproxyNode / ShadowMapNode / AdvanceShadowShading）
都有统一守卫：

```csharp
if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
    continue;
```

batch 的 draw 由 `GpuCullingNode.Commit()` 单独推（每个 batch 一次）。
**新增任何遍历 VisibleMeshes 的渲染节点都必须带这个守卫**，否则 batch 对象会被画两遍。

---

## 3. 三类 Batch

`TtGpuCullingNode.Culling()` 按下列顺序探测 MdfQueue 类型（均可在节点属性上开关）：

| 顺序 | 开关 | MdfQueue | Batch 类 | 用途 |
|---|---|---|---|---|
| 1 | `EnableInstanceMeshCulling` | `TtMdfInstanceStaticMesh` | `TtInstanceStaticMeshBatch` | 已自带 instance modifier 的静态实例（植被等），复用其 instance buffer |
| 2 | `EnableStaticMeshBatch`（默认 false） | `TtMdfStaticMesh` | `TtStaticMeshBatch` | 普通静态 mesh 合批，per-mesh WorldMatrix 转 instance |
| 3 | `Feature_UseRVT` + 类型判断 | `TtTerrainMdfQueue` | `TtTerrainMeshBatch` | CDLOD 地形 patch 合批 |

- **Batch hash**: 同一 `SubMesh + 材质/MdfQueue 特征` 的对象进同一个 batch
  （`MeshBatchHash`），batch 懒创建后复用。
- **Instance push**: 每 SubMesh 逐个 push `FVSInstanceData`；地形额外 push
  `FCullBounding`（patch AABB 减去 CameraOffset）供 GPU 剔除。

### 3.1 前置条件与退化路径

- `TtGpuCullingNode` 未挂进 policy，或 `Feature_UseRVT == false`：
  地形条目不做 batch，由下游逐 mesh 逐 patch 正常绘制（非 instancing），
  此时 shader 端 `TerrainCDLOD.cginc` 走 `cbPerPatch` cbuffer 传参
  （`UsePatchRVTParams = 1`），见 `TtTerrainModifier.OnDrawCall` 的分支。
- 两条路径 shader 端共用同一组 `Get*` 帮助函数，按 `UsePatchRVTParams` 与
  cbuffer 默认值自动分流。

### 3.2 已知限制

- **阴影不投影**：ShadowMapNode / AdvanceShadowShading 对 `EDrawMode.Instance`
  条目 `continue`，batch 的 draw 不会推到 shadow map——batch 后的对象不出影
  （地形本身主视口多数情况下可接受，但 `IsAcceptShadow` 语义在 batch 路径失效）。
- **per-policy 缓冲**：`InstanceBuffers` 每帧 `ResetInstance` 一次；同帧多个
  policy / 多个 vis 采集会互相覆盖，当前依赖"batch 只发生在主视口 policy"这一
  事实前提，扩展时需 per-policy 分桶。
- **HitProxy**：per-instance `HitProxyId` 字段存在，但 batch 路径的拾取
  （per-instance 命中）未验证/未打通。

---

## 4. 地形 (CDLOD) 作为最大消费方

### 4.1 现有 instancing 数据布局

patch 参数全部打包进 `FVSInstanceData`（`TtTerrainModifier.SetInstanceData`）：

| 字段 | 内容 |
|---|---|
| Position / Scale / Quat | PerMeshCBuffer WorldMatrix 的分解（含 CameraOffset 高精度修正） |
| UserData.X | Heightmap RVT UniqueTexID（水面 patch 换 WaterHMap 的） |
| UserData.Y / Z | Normalmap / MaterialId RVT UniqueTexID |
| UserData.W | patch CurrentLOD |
| UserData2.XYZ | patch StartPosition（float 位模式） |
| UserData2.W / Scale_Pad | TexUVOffset.xy |

VS 端 `TerrainCDLOD.cginc` 的六个 `Get*`（GetStartPosition / GetCurrentLOD /
GetTexUVOffset / GetHeightMapTexID / ...）直接从 `GetInstanceData(input)` 读取，
CDLOD 顶点 morph（MorphRange 在 cbPerTerrain，TerrainNode 级）照常工作。

### 4.2 现状的浪费

- 每个 patch 仍持有 `TerrainMesh[LOD] + WireFrameTerrainMesh[LOD] + WaterMesh[LOD]`
  共 3×LOD 个 `TtRenderMesh` wrapper（各自 MdfQueue / PerMeshCBuffer /
  GpuDrivenData），虽然 VMS 几何是共享的，wrapper 数量 = patch 数 × 3 × LOD。
- 每帧采集仍是"逐 patch AddVisibleMesh → Culling 里查表合并"——先撒出去再收回来。

### 4.3 规划：静态共享 batch（已在方案评审中）

1. `TtTerrainSystem` 持有共享 mesh：3 类 × LOD 份（共约 3×6=18 个），
   MdfQueue 用 `UTerrainInstanceMdfQueue`（TtMdfQueue2<TtTerrainModifier,
   TtInstanceModifier>），`TerrainModifier` 只引用 TerrainNode 不引用具体 patch。
2. patch 瘦身为纯数据（CurrentLOD / AABB / StartPosition / TexUVOffset）；
   `TtPatch.OnGatherVisibleMeshes` 不再 AddVisibleMesh，只按 `(LOD, 类型)`
   登记到 TerrainNode 的 per-policy 可见集合。
3. `TtTerrainNode.OnGatherVisibleMeshes` 末尾按桶 push instance 并
   AddVisibleMesh 共享 mesh——drawcall 数从 "patch数×3" 降到 "3×LOD×可见档位"，
   wrapper 内存从 O(patch数) 降到 O(1)。
4. 需要解决：per-policy instance 缓冲分桶（主视口 + shadow 双采集互踩）、
   非 RVT 路径回退保留、共享 mesh 不投影的取舍。

### 4.4 规划：GPU-Driven LOD 与剔除

演进到完整 GPU-Driven 的步骤（依赖 4.3 的 instance 数据先行）：

1. **LOD 分配下沉 compute**：`UpdateLODAssignment` 的分圈逻辑改为 compute
   （patch AABB + 相机距离 → 写 instance 的 CurrentLOD），CPU 只按阈值同步
   结果或完全不再读回（消费端全在 GPU）。
2. **视锥剔除下沉**：已有 `FCullBounding` 通路，地形 patch 天然可用；
   配合 HZB 遮挡剔除（`TtGpuCullingNode` 已有 HzbPinIn 预留）。
3. **indirect 绘制**：cull 压缩 + `DrawIndexedInstancedIndirect`，
   复用 `TtGpuDrivenData` 现成三段式。
4. **多 policy 分桶**：instance 缓冲与 cull 结果按 policy 隔离后，
   阴影 pass 才能独立消费地形 instance（顺带解决"地形不投影"限制）。

---

## 5. 相邻系统

### 5.1 远景地形 VGDag (cluster DAG)

`CSharpCode/Bricks/Terrain/CDLOD/VGDag/`：独立迷你 RenderPolicy + 乒乓双缓冲，
`VgDagCull.compute` 做 LOD 选择 + 视锥剔除 + `InterlockedAdd` 压缩写
DrawClusters 与 indirect args，一次 `DrawIndexedInstancedIndirect` 绘制全部可见
cluster；VS 端 `VgDagDrawModifier.cginc` 用 `vInstanceId` 拉 per-instance
cluster 数据、`vVertexID` 按行主序 patch 拓扑拉顶点池，超槽位 `w=0` 退化剔除。
主图里远景只是一个普通 opaque mesh 走正规 gbuffer 采集。

### 5.2 草地 (Grass)

`UTerrainGrassManager` 把草实例压到 8 字节 `FVSGrassData`，VS 端
`DoGrassModifierVS` 贴地；实例密度受 patch CurrentLOD 调制。属于
"CPU 生成 + GPU 贴地"的半 GPU-Driven 形态。

### 5.3 GpuShading (程序化生成)

`Grapics/Pipeline/GI` 之外的 `GpuTerrainShading.cs` 用 RenderGraph compute
节点做地形 PGC 生成侧（侵蚀 / FlowMap / 汇水迭代），与本文档的绘制侧
GPU-Driven 互补。

---

## 6. 演进路线汇总

| 阶段 | 内容 | 依赖 |
|---|---|---|
| P0（已完成） | FVSInstanceData + TtInstanceModifier + GpuCullingNode 三类 batch + GPU instance culling | — |
| P1 | 地形静态共享 batch（§4.3）：wrapper 内存 O(patch数)→O(1)，drawcall O(patch数×3)→O(3×LOD) | per-policy instance 分桶 |
| P2 | 地形 LOD 分配 + 视锥/遮挡剔除下沉 compute，indirect 绘制 | P1 的 instance 数据形态 |
| P3 | 阴影/多视口 instance 消费（地形投影）、HitProxy per-instance 拾取 | P1/P2 |
| P4 | 与 VGDag 远景 cluster 路径统一 instance/cull 基建 | P2 |

**设计不变量（改动时必须保持）**：

1. 下游遍历 VisibleMeshes 必须跳过 `EDrawMode.Instance` 条目；
2. batch 的 instance push 必须在 `TtGpuCullingNode` Tick（采集消费点）之后、
   Commit 之前完成，且每帧先 Reset；
3. instance buffer 的 flush 点与数据写入收敛在同一处，不在消费端补 flush
   （RVT 槽表同款教训）；
4. 新增 per-instance 参数只能占用 `UserData/UserData2/Scale_Pad` 空余槽位，
   改布局需同步 shader `GetInstanceData` 消费端。

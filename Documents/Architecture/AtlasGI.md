# TtAtlas GI — TitanEngine 全功能全局光照系统设计蓝图

> **状态**: 设计阶段 (L0.5 调研已完成, 第 2/3/4 节已基于 SIGGRAPH 2022
> Lumen course 与 Skorobogatova 2022 论文落地; 等待 L1+ 阶段编码实施)
> **决策依据**: `Documents/Coding/CodingGuidelines.md` §1.4.8.2 双轨制路线决策
> **路线定位**: PC + 高端主机主力 GI 路线, 完整保留 MaterialGraph + 多 ShadingModel 表达力

---

## 0. 命名由来 (为什么叫 Atlas)

- **神话学**: Atlas 是希腊神话**泰坦十二神之一**, 与引擎名 Titan 同神话体系,
  扛着天空 — 隐喻"扛起整个场景的全局光照".
- **图形学双关**: `texture atlas` / `card atlas` / `virtual texture atlas`
  是图形学日常术语. TtAtlas GI 的核心数据结构正是 **Mesh Card Atlas**, 名字
  和算法本质同构.
- **避免山寨感**: 不沿用 UE 的 "Lumen" 命名, 但保留对 Lumen 算法体系的尊重 —
  本系统在算法上**深度参考** Lumen (SIGGRAPH 2022 course), 在工程上**适配
  TitanEngine** 的 RHI / RenderGraphNode / MaterialGraph 体系.

---

## 1. 系统定位与边界

### 1.1 适用场景

- **目标硬件**: PC (RTX 20 系起步) + 高端主机 (PS5 / Xbox Series X) +
  桌面级 Mac (M2 Pro/Max 起步).
- **不适用**: 移动端 / Switch 等显存受限平台 -> 走 ReSTIR 轨道 A
  (见 CodingGuidelines.md §1.4.8.2).

### 1.2 核心能力与子系统映射

| 核心能力 | 实现路径 (对应章节 / Node) |
|---|---|
| 间接光漫反射 (indirect diffuse) | §2.5 Screen Probe Gather + §2.6 Final Gather → §3.3 #6/#7/#8/#9 |
| 间接光镜面反射 (indirect specular) | §2.7 Reflection 通路 → §3.3 #10 (单独 ray + 单独 denoise) |
| 自发光物体的间接照明 (emissive bounce) | §2.3 Card Capture 写 Emissive atlas → §2.5 probe 在 hit 时把 emissive 当 radiance 累加 |
| 天光 / 环境光遮蔽 (sky occlusion + AO) | §2.0 hybrid pipeline 的 Skylight 终端后端 + §2.5 probe ray miss 采 sky |
| 多 ShadingModel 支持 (DefaultLit / Cloth / Hair / Eye / SSS) | §4.3 方案 A: ShadingModel ID 路由 + lighting compute 分支 |
| 动态光源对 GI 的实时贡献 | §3.3 #3 SurfaceCacheDirectLighting 每帧 1024² texel 重算 + §2.2 LRU 老化机制 |
| 半透明物体接收 GI | §2.6 *Domain 划分* 中的 "Transparency / Volumetric Fog" froxel 通路 |

每项能力都不允许"以后再设计", 全部在第 2/3 节有对应实现节点; 实施排期
见 §5。

### 1.3 与现有系统的关系

| 现有系统 | TtAtlas GI 的关系 |
|---|---|
| `CSharpCode/Grapics/Pipeline/GI/ReSTIR/` (ReSTIR GI) | 并行存在, 互不替代. 渲染管线根据平台 / 配置二选一启用 |
| `MaterialGraph` (`CSharpCode/Bricks/CodeBuilder/ShaderNode/`) | **强依赖**. Atlas 烘焙 pass 需要 MaterialGraph 生成新的 codegen 路径 `DO_CARD_CAPTURE_IMPL` |
| `TtBindless` (CodingGuidelines.md §1.4) | **强依赖**. Card Atlas 切片 / per-instance card 索引都通过 bindless 暴露给 RT hit shader |
| `TtRenderPolicy` / `RenderGraphNode` | TtAtlas GI 的所有子系统都作为独立 Node 集成进 policy, 复用现有 pin / attachment / cbuffer 机制 |
| `Documents/Coding/CodingGuidelines.md` §1.1-§1.4 | 所有 C# 代码必须严格遵守 (CBuffer 创建 / Bind 时机 / Bindless 规约) |

---

## 2. 核心算法概览

> 本节基于 Wright/Narkowicz/Kelly. *Lumen — Real-time Global
> Illumination in Unreal Engine 5* (SIGGRAPH 2022 Advances course)
> 与 Skorobogatova. *Real-Time Global Illumination in Unreal Engine 5*
> (Brno MU 硕士论文, 2022). 引用以 SIGGRAPH 2022 PDF 页码为准, 形如
> `[S2022 P##]`; 硕士论文标记为 `[Sko2022 §x.y]`.

### 2.0 整体管线 (Hybrid Ray Tracing Pipeline)

Lumen 的核心抽象: **每条采样光线按"从近到远精度递减"的顺序串接多个
tracing 后端**, 任一后端解出 hit 就提前结束 `[S2022 P9, P22, P46]`.
TtAtlas GI 沿用同一拓扑:

```
Screen Trace  ->  Mesh SDF Trace (短距, 0~2m)  ->  Global SDF / HW RT (远距)  ->  Skylight
                                  ↓                          ↓
                          Surface Cache 采样            Voxel Lighting 采样
```

每个后端的统一接口: `{ origin, dir, tMax }` -> `{ hitT, hitRadiance, bHit }`.
某后端 `bHit=true` 即 lane 跳过下游 `[S2022 P29]`. *Compaction* 必须用
threadgroup-local prefix sum 而非 atomic, 否则 ray 顺序被打散导致后续
trace 失去 wave 一致性 (论文测得 prefix-sum 比 atomic 快 21%).

引擎实现: 每个后端是一个 RenderGraphNode (见 §3); 紧凑由独立 compute
pass 写一份 indirect dispatch args buffer, 下一段 trace 节点的 drawcall
通过 `BindIndirectDispatchArgsBuffer(...)` 接管 (与本仓库
`CSharpCode/Bricks/Particle/Effector.cs` 中 GPU 粒子的写法一致, 这是
NxRHI 当前唯一已落地的 indirect dispatch 入口).

### 2.1 Mesh Card 系统

**定义**. Mesh Card 是 mesh 表面的**oriented bounding 矩形投影区域**,
每张 card 携带一个正交相机 (位置 / 朝向 / OBB), 用于把覆盖区域
rasterize 到 2D atlas `[S2022 P58, P60]`.

**Card 生成算法** (asset import 时离线, `[S2022 P59~P63]`):

1. **Surfel sampling**: 在 mesh 表面按面积重要性撒 surfel, 每个
   surfel 记 `{ pos, normal, occlusion }` (occlusion 由 mesh 内部投少量
   ray 求得, 用于打分).
2. **Surfel 聚类初始化**: 随机选未分配 surfel 当 seed, 按
   `weight = f(distToBounds, clusterAspect, surfelOcclusion, visibility)`
   迭代加入候选, 直到覆盖率 / 长宽比稳定; centroid 收敛后接受.
3. **全局 cluster 优化**: 用 cluster centroid 当新 seed, 并行 re-grow
   全部 cluster; 删除过小 cluster, 在空隙插新 cluster.
4. **挑选**: 按 coverage 排序, 取前 N (默认 32, 复杂 mesh 64) 拟合 OBB.

Card 生成是离线工序 (asset cook 阶段), 结果作为 mesh 资产附加字段持久化.

**Card 数据格式**:

```cpp
// GPU 端 structured buffer, per-mesh 一段
struct TtMeshCard {
    float3 origin;          // OBB 中心 (mesh 局部空间)
    float3 extentX;         // OBB 半轴 X
    float3 extentY;         // OBB 半轴 Y
    float  depth;           // OBB 沿投影方向的厚度
    uint   axisIndex;       // 0..5 = ±X / ±Y / ±Z (sample 时 6 选 3)
    uint   atlasPageId;     // 指向 Surface Cache page table
    uint16 resX, resY;      // capture 分辨率 (8~512)
};
```

**Card Merging** `[S2022 P71]`. 大量小 instance (远建筑 / 散落石头) 单
独分配 card 不划算, runtime 把"同 tag / 同 BVH 簇"的 instance 合并为一
个 6-card cubemap-like capture, 一次 draw 烘焙整组. 该路径**强依赖**
Nanite 类的 GPU-driven raster (本仓库当前没有, 见 §7 风险).

### 2.2 Surface Cache (Virtual Card Atlas)

**物理布局** `[S2022 P67, P69]`:

- 单张 4K×4K **physical atlas** 容纳 128×128 的 *physical page* (1024
  页). Card >= 128: 拆多页 (每页带 0.5 texel border); Card < 128: 在某
  页内用 2D allocator sub-allocate.
- 每通道一张 atlas, runtime BC 压缩:

  | 通道 | 格式 | atlas 大小 | 备注 |
  |---|---|---|---|
  | Albedo | RGB8 BC7 | 16 MB | (R,G,B,_) |
  | Opacity | R8 BC4 | 8 MB | any-hit shading 用 |
  | Depth | R16 (未压缩) | 32 MB | sample 时投影 / 有效性判定; Depth=MAX = invalid |
  | Normal | RG8 BC5 | 16 MB | 半球 octahedral |
  | Emissive | RGB Float16 BC6H | 16 MB | HDR |
  | DirectLighting | RGBA Float16 | 32 MB | runtime 累积, 不压缩 |
  | IndirectLighting | RGBA Float16 | 32 MB | n+2 bounce 反馈, 不压缩 |
  | FinalLighting | RGBA Float16 | 16 MB | `Direct + Indirect * Albedo`, 给 ray hit 直接读 |

  **总预算约 168 MB**, 与 §1.2 "高端 PC + 主机"硬件目标一致.

**Page Table 格式** `[S2022 P67]`:

```
4 bits | 4 bits | 12 bits | 12 bits
ScaleX  ScaleY    BiasX    BiasY      (Scale = log2, Bias = mult of 8)
```

每 mesh 持有一个小 *card grid* (per-mesh 3D 网格, 每 cell 索引该位置
覆盖的 6 张 card), sample 流程:
**MeshIndex → CardGrid → {cell, hitPos} → 6 cards → 按 normal 选 3**
`[S2022 P70]`.

**Virtual Surface Cache 双层策略** `[S2022 P64, P65]`:

- **Always-resident low-res page**: 相机周围所有可见 mesh 都分配低分
  辨率 (32×32 / 64×64), 给 GI 用 (低频, 大量小 card 全覆盖).
- **On-demand high-res page**: 按 GPU feedback 动态 alloc / evict, 给
  reflection / 高频材质用. LRU heap 管理, 显存满 evict 最老.

**GPU Feedback** `[S2022 P66, P74]`:

- 每条 ray hit 后, 在 surface cache sample 时**随机选一个 sample**, 把
  `{ pageId, requestedMip, frameId }` 写到 feedback buffer.
- GPU 端 hash table 合并 (避免重复请求), compact 后 download 到 CPU.
- CPU 排序后决定下一帧 map / unmap 哪些 page.

**Lookup fallback** `[S2022 P67]`. 高分页缺失时 page table 自动回落到
always-resident 低分页, shader 端**单次查找即可**, 不需递归搜 fallback.

### 2.3 Card Capture Pass

**职责**. 把 mesh + material 烘到 §2.2 的 Albedo / Opacity / Depth /
Normal / Emissive 五张 atlas, 即 Lumen 的"GBuffer 替身".

**烘焙时机** `[S2022 P68]`:

- runtime 增量更新, **每帧固定预算 512×512 texel**.
- 优先级 = `f(distToCamera, GPU feedback recency)`.
- 同时每帧轮询少量 oldest page 重烘, 支持动态材质 (闪烁灯 / 流体).

**Capture 流程**:

```
对每张待更新 card:
  1) ortho camera <- TtMeshCard.{origin, extentXY, depth, axisIndex}
  2) 渲染单 mesh 到 4 RT (Albedo / Normal / Emissive / Depth)
     PS 调用 MaterialGraph 生成的 DO_CARD_CAPTURE_IMPL (§4)
  3) Compute pass: 4 RT 做 BC 压缩 + copy 到 atlas page
  4) PageTable[pageId] = { atlasScale, atlasBias }   // mark valid
```

**与 raster GBuffer 的差异** `[S2022 P69]` (必守):

- **关闭 alpha masking**: 保留 alpha 完整, 让 any-hit shading 自己采
  Opacity atlas 判定 (避免 capture 时 discard 留洞).
- **Specular / SSS 近似**: capture 不存 specular / SSS 参数, 把这些能
  量损失折算到 albedo (`finalAlbedo = albedo * (1 - specEnergy)`), 让
  surface cache lighting 的 diffuse 计算近似守恒.
- **Invalid texel 标记**: depth = MAX (R16 的 0xFFFF), sample 时权重
  清零 (避免黑边 / 飞点).

**Nanite-like 加速** `[S2022 P68]`. UE 用 Nanite 把所有 card raster 合
成单个 vis-buffer draw + 每材质一个 dispatch. 本仓库目前没有 Nanite,
**首版 L2 用普通 raster**: 一次烘 N 张走 N 次 draw, 每帧预算从 512
降到 128, 见 §7 风险.

### 2.4 Voxel Lighting

**为什么需要它**. Global SDF (远场 trace) 不携带 mesh instance ID, hit
后**无法直接采 Surface Cache** (Surface Cache 需要 `MeshIndex → CardGrid`).
所以必须额外做"全场景 merge 后的体素辐射缓存", 给远场 trace 用
`[S2022 P80]`.

**结构**:

- **4 级 clipmap, 每级 64×64×64 voxel**, 围相机 (近密远疏; 不需要
  Global SDF 的 256³, 因只存方向化辐射不存几何).
- 每 voxel 存 **6 个轴向方向的 radiance** (RGBA Float16 ×6 = 48 byte/voxel
  ≈ 12 MB / 级, 全管线 ≈ 48 MB).
- alpha 通道存"该方向有效权重", 补偿没被任何 card 覆盖的方向.

**采样接口**:

```hlsl
// VoxelLighting.cginc
float3 SampleVoxelLighting(float3 worldPos, float3 normal, out float weightSum)
{
    int clipmap = SelectClipmap(worldPos);
    float3 absN = abs(normal);
    int3   axes = SortAxesByMagnitude(absN);   // |Nx|,|Ny|,|Nz| 取前 3
    float3 radiance = 0;
    [unroll] for (int i = 0; i < 3; ++i)
        radiance += absN[axes[i]] *
                    SampleDirectionalRadiance(clipmap, worldPos,
                                              axes[i], normal[axes[i]] >= 0);
    return radiance;
}
```

**更新策略** `[S2022 P81]` (*Voxel Lighting Visibility Buffer*):

- 跟踪场景 mesh 移动, GPU 端构造 *modified bricks* 列表 (每 brick 4³
  voxel).
- 对每 modified brick 投 **每 voxel 6 条 ray** 做"merge 哪个 mesh"的
  atomic min, 写 *visibility buffer* (24 bit MeshIndex | 8 bit hitT).
- visibility buffer **缓存几何而非光照** —— 光照逐帧变, 但"哪个 mesh
  在哪个 voxel" 帧间稳定.
- 取光时, 按 visibility buffer 反查 surface cache (`Direct + Indirect *
  Albedo`) 写入 voxel atlas.

### 2.5 Screen Probe Gather

> Lumen 的 *Final Gather (Diffuse)* 核心数据结构 = **Screen Space
> Radiance Cache (SSRC)**, 由 Screen Space Probe (SSP) 组成. 本节描述
> SSP 的分布 / trace / spatial reuse, 像素级插值放 §2.6.

**Probe 屏幕空间分布**:

- 默认每 **16×16 像素** 1 个 probe (1080p ≈ 8000 个).
- *Adaptive placement*: GBuffer 法线 / 深度变化剧烈区域加密 (4×4
  hierarchical refinement, 类似 hi-Z), 预算超时回退稀疏档.
- Probe 的 *plane equation* (`{normal, depth}`) 同时存下, 后续 final
  gather 用它做 plane weighting / visibility 防漏光.

**Probe trace 内容**:

- 每 probe 投 64 条 ray (8×8 octahedral), tMax = 50m.
- ray 串接 §2.0 的 hybrid pipeline: Screen → MeshSDF → GlobalSDF → Skylight.
- hit 后采 Surface Cache 的 `FinalLighting` atlas (已乘 albedo, 直接
  当 diffuse radiance 累加).

**Probe 编码格式**:

- 每 probe 输出 **8×8 octahedral irradiance + 8×8 hitT** (RGBA Float16).
- 配一份 **球谐 SH3 (9 系数 RGB)**, 用于 final gather 的低频快通道
  (高粗糙度 specular / 低频 diffuse 都直接读 SH).

**Spatial Reuse / Importance Sampling** `[S2022 P77~P78]`:

- **2D bilateral spatial filter**: 16×16 邻域内, 用 plane weight +
  visibility weight 加权平均 4 个 closest probe 的 octahedral.
- **Product Importance Sampling**: 用上一帧 octahedral 推下一帧 ray 方
  向 (pdf ∝ 上一帧 radiance × cosθ × BRDF). 这是 Lumen 区别于纯
  ReSTIR 的关键, 让 64 ray/probe 的 effective sample count ≈ 上百 ray.

**时间 reuse**:

- probe octahedral 4 帧 EMA (`[S2022 P78]`, "*4 frame temporal
  accumulation, num accumulated per texel*").
- probe 位置每帧 jitter (×4 stratification), ray 方向每帧 jitter.

### 2.6 Final Gather (Diffuse, 屏幕空间)

**任务**. 把 §2.5 的 sparse SSP 插值到屏幕每像素的 indirect diffuse.

**插值算法** `[S2022 P78]`:

```hlsl
// 每像素 PS:
float3 GatherIndirectDiffuse(int2 px, float3 normal, float depth)
{
    float3 sumRad = 0;
    float  sumW   = 1e-6;
    [unroll] for (int i = 0; i < 4; ++i) {
        ProbeIndex p = NearestProbes[i];
        float wPlane    = PlaneWeight(p.plane, normal, depth);     // 法线/深度差
        float wVisible  = VisibilityWeight(p.octahedral_hitT, GBufferDepth);
        float wBilinear = BilinearWeight(px, p.screenPos);
        float w = wPlane * wVisible * wBilinear;
        sumRad += w * SampleProbeOctahedral(p, normal);
        sumW   += w;
    }
    return sumRad / sumW;
}
```

**Domain 划分** `[S2022 P14]`:

- **Opaque**: 屏幕空间 2.5D, 即上面这套.
- **Transparency / Volumetric Fog**: camera-aligned **3D froxel grid**,
  每 froxel 一个轻量 probe (类似 Frostbite froxel volume).
- **Surface Cache 自身的 indirect**: 在 atlas texture 空间做一遍 final
  gather (§2.2 `IndirectLighting` atlas), 用于 n+2 bounce 反馈.

**与现有 GBuffer 耦合**. TitanEngine 实际 GBuffer 布局 (见
`enginecontent/Shaders/ShadingEnv/Deferred/DeferredCommon.cginc` 的
`GBufferData::EncodeGBuffer/DecodeGBuffer`):

| RT | .r | .g | .b | .a |
|---|---|---|---|---|
| GBufferRT0 | MtlColorRaw.r | .g | .b | CustomData.r |
| GBufferRT1 | EncodeNormal(WorldNormal).x | .y | .z | ObjectFlags_2Bit / 3 |
| GBufferRT2 | Metallicity | Specular | Roughness | AO |
| GBufferRT3 | EncodeMotionVector(MV).x | .y | RenderFlags_10Bit / 1023 | CustomData.g |
| DepthStencil | scene depth | (stencil 暂未使用) | | |

**注意**:

- 当前 GBuffer **没有专门的 ShadingModelId 通道**, Emissive 也未独立
  存储 (`GBufferData::Emissive` 字段在源码里被注释掉). §4.3 方案 A 落
  地时需要把 ShadingModelId 复用到 RT1.a (与 ObjectFlags_2Bit 共占,
  RT1.a 的 bit 分配从 "2 bit ObjectFlags" 扩到 "2 bit ObjectFlags + 6
  bit ShadingModelId"), 同时把 `mEmissive` 写到一张新 RT4 或挪到
  CustomData 之一. 这部分**改造任务挂在 L1 阶段**, TtAtlas GI 先按"扩
  展后的 GBuffer 布局"设计, L1 完成后再对齐.

`TtAtlasFinalGatherNode` 需 SRV: GBufferRT1 (法线), DepthStencil (深度),
GBufferRT2 (rough/metal —— 高粗糙度 ≥ 0.6 时走 §2.7 高粗糙度回退).
输出: 一张 RGBA Float16 `IndirectDiffuse` RT.

### 2.7 Reflection 通路

**为什么不能复用 SSP**. SSP 的 octahedral 8×8 + 16 像素插值, 高频细
节完全糊掉. Reflection 是高频信号 (尤其低粗糙度), 必须**单独 trace
ray + 单独 denoise** `[S2022 P11, P15]`.

**双模式** `[S2022 P83, P99]`:

| 模式 | Albedo | DirectLighting | IndirectLighting | 备注 |
|---|---|---|---|---|
| **Surface Cache** (默认) | atlas | atlas | atlas | 一次 ray 直接读 FinalLighting atlas, 性能最优 |
| **Hit Lighting** (高质量) | dynamic | dynamic (re-trace shadow) | atlas | 适用于动态对象 / 没有 surface cache 的 mesh |

Hit Lighting 流程 `[S2022 P100]`:

```
Screen tracing → Trace + Surface Cache (lightweight payload) →
    Sort by Material ID → Retrace + Hit Lighting (full PBR + shadow ray) →
    Skylight
```

排序 (Sorted-Deferred) 让同 material 的 ray 集中执行, 大幅降低 shader
divergence.

**Lightweight Payload** `[S2022 P96]` (20 byte, 用于 Surface Cache 模式):

```cpp
struct SurfaceCachePayload {
    uint32 hitT;           // 32 bit float
    uint32 flags;          // 1b translucent | 1b two-sided | 30b material
    uint32 packedNormal;   // octahedral
    uint32 primitiveId;
    uint32 instanceId;
};
```

**Reflection Denoising** `[S2022 P15]`:

- ray 方向按 GGX 重要性采样 (PDF ∝ D(h)·G2 / 4|n·v|).
- spatial reuse: 8×8 tile 内按 BRDF lobe + hitT 一致性聚类邻居 ray.
- temporal reuse: 上一帧 reproject + neighborhood clamp (类似 SVGF).
- **复用 diffuse ray**: 高粗糙度 (>= 0.6) 直接读 `IndirectDiffuse` RT,
  不另投 ray.

**与 SSR 混合** `[S2022 P22~P28]`. Screen trace 永远跑在最前面, 命中即
返回; 否则降级到 Surface Cache. Hand-off **回退到最近未遮挡位置** (避
免漏光).

### 2.8 Denoise + TAA 整合

**输出格式约定**:

- `IndirectDiffuse`  : RGBA Float16, alpha = trace hitT (denoise 用)
- `IndirectSpecular` : RGBA Float16, alpha = pdf (能量补偿用)
- 两者**严格分离**, 直到 `TtAtlasCompositeNode` 才合到 lighting buffer.
  合成: `directDiffuse + indirectDiffuse * BaseColor` +
        `directSpec + indirectSpec * F`.

**Denoise pipeline 复用** (与 ReSTIR 轨道共享):

- `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs::TtDenoiseNode`
  (已确认存在, `TAuxRenderGraphNode<TtDenoiseNode>`) —— spatial filter
  模板 (ping-pong, multi-iteration, step size 1/2/4/8 渐进).
- 计划新增 `TtAtlasTemporalDenoiseNode`: SVGF 风格, 用 history hitT +
  history normal 做 reproject + neighborhood clamp. 实现位置:
  `CSharpCode/Grapics/Pipeline/GI/Atlas/TtAtlasTemporalDenoiseNode.cs`.

**TAA 集成**. TtAtlas GI 输出送进引擎现有 TAA **之前**, 必须先做一遍
**diffuse 自身的 temporal accumulation** (4 frame EMA), 否则 TAA 的速
度向量 reproject 在 indirect 信号上会 ghosting `[S2022 P78]`.

输出顺序:

```
IndirectDiffuse  ─┐
IndirectSpecular ─┼─> Composite ──> Lighting Buffer ──> 引擎 TAA ──> Tonemap
DirectLighting   ─┘
```

---

## 3. 子系统拆分到 RenderGraphNode

按 TitanEngine 的 RenderGraphNode 体系, TtAtlas GI 拆分为 11 个独立
Node, 每个对应 §2 中的一个算法阶段. 所有 Node 都继承自
`TAuxRenderGraphNode<TtSelfNode>` (这一基类已在仓库中验证存在,
`CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs::TtReSTIRGINode`
即采用此模式; `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs::
TtDenoiseNode` 同样如此). Card Capture 是 graphics raster pass, 内部
持有 `TtGraphicsShadingEnv` (而不是另起新基类), 通过节点字段把每张
card 的 ortho VP 矩阵和目标 atlas page id 传到 `OnDrawCall`. 全部 Node
严守 `Documents/Coding/CodingGuidelines.md` §1.1-§1.4 (CBuffer / Bind
时机 / Bindless 规约).

### 3.1 命名空间与目录约定

```
CSharpCode/Grapics/Pipeline/GI/Atlas/        // C# 节点
    TtAtlasMeshCardManager.cs                 // 非 Node, 全局单例: card 元数据 / atlas 分配
    TtAtlasSurfaceCacheManager.cs             // 非 Node, atlas 物理资源 + page table
    TtAtlasVoxelLightingManager.cs            // 非 Node, voxel clipmap 资源
    TtAtlasCardCaptureNode.cs                 // §3.3 #1
    TtAtlasSurfaceCacheUpdateNode.cs          // §3.3 #2
    TtAtlasSurfaceCacheDirectLightingNode.cs  // §3.3 #3
    TtAtlasSurfaceCacheIndirectLightingNode.cs// §3.3 #4
    TtAtlasVoxelLightingUpdateNode.cs         // §3.3 #5
    TtAtlasScreenProbeAllocNode.cs            // §3.3 #6
    TtAtlasScreenProbeTraceNode.cs            // §3.3 #7
    TtAtlasScreenProbeFilterNode.cs           // §3.3 #8
    TtAtlasFinalGatherNode.cs                 // §3.3 #9
    TtAtlasReflectionNode.cs                  // §3.3 #10
    TtAtlasCompositeNode.cs                   // §3.3 #11

enginecontent/Shaders/GI/Atlas/               // HLSL (与同级 ReSTIR/ TrayTracing/ SH/ 平级)
    AtlasCommon.cginc                         // 通用结构 + sampler 声明
    MeshCard.cginc                            // TtMeshCard / CardGrid 解码
    SurfaceCache.cginc                        // page table 查询 + 6→3 card 采样
    VoxelLighting.cginc                       // §2.4 SampleVoxelLighting
    HybridTrace.cginc                         // §2.0 串接 Screen→MeshSDF→GlobalSDF→Sky
    AtlasCardCapture.shadingenv.var           // PS shading env, 调 DO_CARD_CAPTURE_IMPL
    SurfaceCacheUpdate.compute                // §3.3 #2
    SurfaceCacheDirectLighting.compute        // §3.3 #3
    SurfaceCacheIndirectGather.compute        // §3.3 #4
    VoxelLightingUpdate.compute               // §3.3 #5
    ScreenProbeAlloc.compute                  // §3.3 #6
    ScreenProbeTrace.compute                  // §3.3 #7
    ScreenProbeFilter.compute                 // §3.3 #8 (spatial + temporal)
    AtlasFinalGather.compute                  // §3.3 #9
    AtlasReflection.compute                   // §3.3 #10
    AtlasComposite.compute                    // §3.3 #11
```

**Shader 文件登记**: 经核查, `enginecontent/Shaders/GI/ReSTIR/*.compute`
**没有**登记到 `enginecontent/Shaders/CoreShader.vcxitems`, 说明本仓库
HLSL 文件不走 vcxitems 注册 (与 C# 端 `.projitems` 不同), 而是由引擎
runtime 按目录扫描加载 (具体加载入口见 `TtShadingEnv` 的 shader 资源
解析逻辑). 所以新增 .compute / .cginc / .shadingenv.var 不需要登记到
任何 xml, **直接放进对应目录即可**.

`Atlas` 子目录里所有 cginc 都遵守 `SKILL.md` §3.4 的资源声明顺序: 主
.compute 文件**先声明**全局资源 (Texture/RWTexture/Sampler/cbuffer),
**再 #include** 公共 cginc.

### 3.2 全局共享资源 (跨 Node)

由 4 个 Manager 类持有 (引擎启动时创建, 跟随 RenderPolicy 生命周期):

| Manager | 持有资源 | 提供给哪些 Node |
|---|---|---|
| `TtAtlasMeshCardManager` | `MeshCardSb` (StructuredBuffer<TtMeshCard>), `MeshCardGridTex` (3D Texture per mesh, bindless) | CardCapture / SurfaceCacheUpdate / Voxel / ProbeTrace / Reflection |
| `TtAtlasSurfaceCacheManager` | `PageTableTex` (R32_UINT 2D), 8 张 atlas (Albedo / Opacity / Depth / Normal / Emissive / DirectLighting / IndirectLighting / FinalLighting), `FeedbackBuffer` (RWStructuredBuffer), `RequestHashTable` | 所有 trace 类节点 + 所有 lighting 类节点 |
| `TtAtlasVoxelLightingManager` | 4 级 clipmap 体积纹理 (RGBA Float16 ×6 方向), `VoxelVisibilityBuffer` (R32_UINT) | VoxelLightingUpdate / ProbeTrace / Reflection |
| `TtAtlasGlobalSDFManager` | 4 级 256³ Global SDF clipmap (R8 narrow band, ±4 voxel/byte), `MeshSDFBindless` (TtBindless 绑定每个 mesh 的 mip0 体素 brick) | ProbeTrace / Reflection / VoxelLightingUpdate |

`TtAtlasGlobalSDFManager` **是 TtAtlas GI 自带的子系统**, 不依赖
ReSTIR 轨道 (经核查, `CSharpCode/Grapics/Pipeline/GI/ReSTIR/` 当前没有
Global SDF 实现, ReSTIR 走的是 HW RT BVH + screen trace, 没有 SW 远场
fallback). Global SDF 的实现细节参考 `[S2022 P40~P43]`, 在 §3.3 #5
节点之外另起一个 `TtAtlasGlobalSDFUpdateNode` (#5b), 出于篇幅原因本
文档把它合并到 #5 一起讨论, 但实施时是独立 Node.

所有 atlas / page table / cbuffer **跨节点共享**, 通过 Manager 单例
访问, 不在节点 `OnResize` 里重建 (改用 `Manager.OnPolicyResize`).

### 3.3 Node 拆分清单

每个 Node 标注: 频率 / 输入 pin / 输出 pin / 主 cbuffer / 主 shader.

#### #1 `TtAtlasCardCaptureNode` —— Card 烘焙

- **频率**: 编辑期一次性 + runtime 增量 (每帧 ≤ 16 张 card, 见 §2.3 预算)
- **输入 pin**: `MeshCardRequestSrv` (来自 SurfaceCacheManager 的 GPU
  feedback hash 表)
- **输出 pin**: `AlbedoAtlasUav`, `OpacityAtlasUav`, `DepthAtlasUav`,
  `NormalAtlasUav`, `EmissiveAtlasUav` (5 张 atlas, BFT_UAV | BFT_SRV)
- **类型**: graphics + compute 混合. 每张 card 走一遍 ortho raster
  PS (4 RT MRT), 完成后用 compute pass 做 BC 压缩 + copy 到 atlas page
- **shading env**: `AtlasCardCapture.shadingenv.var`, 调 MaterialGraph
  生成的 `DO_CARD_CAPTURE_IMPL` (§4)
- **cbuffer** (HLSL 字段顺序按 std140-like 16 字节对齐, 见
  CodingGuidelines.md §1.1):

  ```hlsl
  cbuffer cbCardCapture {
      float4x4 OrthoVP;          // 64 bytes, 4 个 vec4
      float4   CardOrigin;       // .xyz = origin, .w = depth (合并填充)
      float4   CardExtentX;      // .xyz = extent X, .w = atlas page id (uint asfloat)
      float4   CardExtentY;      // .xyz = extent Y, .w = card axis (uint asfloat)
      uint4    Misc;             // .x = material id, .yzw = reserved
  };
  // C# 端对应 struct 必须用 [StructLayout(Pack=16)] 严格对齐
  ```

#### #2 `TtAtlasSurfaceCacheUpdateNode` —— Atlas 维护

- **频率**: 每帧
- **职责**: ① 处理 GPU feedback hash 表 (合并 + compact + download
  到 CPU); ② CPU 端 alloc / evict pages 后, 把新 page 的 PageTable
  entry 写回 GPU; ③ 标记 evicted page 为 invalid (Depth=MAX)
- **输入 pin**: `FeedbackBufferSrv`
- **输出 pin**: `PageTableUav` (R32_UINT, BFT_UAV | BFT_SRV)
- **shader**: `SurfaceCacheUpdate.compute`
- **cbuffer**:

  ```hlsl
  cbuffer cbSurfaceCache {
      uint4 AtlasParams;     // .x = AtlasResolution, .y = PhysicalPageSize,
                             // .z = CurrentFrameIndex, .w = MaxRequestPerFrame
  };
  ```

  (本节点不持有 card 元数据, card 元数据在 Manager)

#### #3 `TtAtlasSurfaceCacheDirectLightingNode` —— atlas 直接光照

- **频率**: 每帧, 预算 1024×1024 texel `[S2022 P74]`
- **职责**: 选 1024×1024 个待更新 page tile (8×8 z-order), 每 tile 选 8
  盏灯, 1 bit/灯 shadow mask, sample shadow map → trace offscreen shadow
  → apply lights, 写 `DirectLightingAtlas`
- **输入 pin**: `PageTableSrv`, `AlbedoAtlasSrv`, `NormalAtlasSrv`,
  `DepthAtlasSrv`, `ShadowMapSrv` (来自现有 ShadowMapNode), `LightListSb`
  (引擎光源 SB)
- **输出 pin**: `DirectLightingAtlasUav`
- **shader**: `SurfaceCacheDirectLighting.compute`
- **cbuffer**:

  ```hlsl
  cbuffer cbDirectLighting {
      uint4  TileParams;      // .x = TileCount, .y = MaxLightsPerTile,
                              // .z = LightCount, .w = reserved
      float4 TraceParams;     // .x = SurfaceBias, .y = ShadowRayMaxT,
                              // .z/.w = reserved
  };
  ```

#### #4 `TtAtlasSurfaceCacheIndirectLightingNode` —— atlas 间接光照 (n+2 bounce)

- **频率**: 每帧, 预算 512×512 texel (1/4 of direct, `[S2022 P76]`)
- **职责**: 在 atlas 空间做 final gather, 4×4 tile 一个半球 probe, 每
  probe 投 ≤8 ray, hit 时**采上一帧** `FinalLighting` atlas (即 n+1
  bounce 的结果, 这是多 bounce 的反馈机制)
- **输入 pin**: `PageTableSrv`, `AlbedoAtlasSrv`, `NormalAtlasSrv`,
  `DepthAtlasSrv`, `DirectLightingAtlasSrv`, `PrevIndirectLightingAtlasSrv`
  (history, ping-pong), `MeshCardSb`, `MeshSDFBindless` (用于 trace)
- **输出 pin**: `IndirectLightingAtlasUav`, `FinalLightingAtlasUav`
  (= Direct + Indirect × Albedo)
- **shader**: `SurfaceCacheIndirectGather.compute`
- **cbuffer**:

  ```hlsl
  cbuffer cbIndirectLighting {
      uint4  GatherParams;    // .x = ProbesPerPage, .y = RayCountPerProbe,
                              // .z = TemporalFrameIndex, .w = MaxAccumFrames (固定 4)
  };
  ```

#### #5 `TtAtlasVoxelLightingUpdateNode` —— Voxel 体素辐射缓存

- **频率**: 每帧 (modified bricks 增量, `[S2022 P81]`)
- **职责**: 跟踪 mesh 移动 → 构造 modified bricks list → 对每 brick 投
  6 ray/voxel 写 `VoxelVisibilityBuffer` → 按 visibility 反查 surface
  cache (`FinalLighting`) 写 voxel atlas
- **输入 pin**: `MeshCardSb`, `MeshSDFBindless`, `FinalLightingAtlasSrv`,
  `PageTableSrv`
- **输出 pin**: `VoxelClipmapUav[4]` (4 级, 每级 6 方向 RGBA Float16),
  `VoxelVisibilityBufferUav`
- **shader**: `VoxelLightingUpdate.compute` (含 3 个 entry point:
  `CSBuildModifiedBricks`, `CSVoxelizeVisibility`, `CSGatherRadiance`)
- **cbuffer** (HLSL `float3 array[N]` 实际占 N×16 字节, 用 float4 显式
  表达避免 C# struct 对齐踩坑):

  ```hlsl
  cbuffer cbVoxelClipmap {
      float4 ClipmapCenter[4];     // .xyz = center, .w = voxel size
      uint4  UpdateParams;         // .x = ModifiedBrickCount, .yzw = reserved
  };
  ```

#### #6 `TtAtlasScreenProbeAllocNode` —— 屏幕 probe 分布

- **频率**: 每帧
- **职责**: ① 16×16 base placement; ② adaptive refinement (4×4
  hierarchical); ③ 写 `ProbeListSb` + `ProbeIndirectArgsBuf` 给后续
  trace 用 ExecuteIndirect
- **输入 pin**: `GBufferRT1Srv` (Normal), `DepthSrv`
- **输出 pin**: `ProbeListSbUav` (StructuredBuffer<{px, py, plane}>),
  `ProbeIndirectArgsUav` (uint4 dispatch args)
- **shader**: `ScreenProbeAlloc.compute`
- **cbuffer**:

  ```hlsl
  cbuffer cbProbeAlloc {
      uint4  ScreenParams;     // .xy = ScreenSize, .z = BaseProbeStride,
                               // .w = MaxProbeCount
      float4 RefineParams;     // .x = RefineNormalThreshold (cosθ),
                               // .y = RefineDepthThreshold (相对值),
                               // .z/.w = reserved
  };
  ```

#### #7 `TtAtlasScreenProbeTraceNode` —— probe 投 ray

- **频率**: 每帧, ExecuteIndirect (probe count 不定)
- **职责**: 每 probe 投 64 ray (8×8 octahedral), 串接
  Screen→MeshSDF→GlobalSDF→Sky (HybridTrace.cginc), hit 后采
  `FinalLightingAtlas`, 输出 octahedral irradiance + hitT
- **输入 pin**: `ProbeListSbSrv`, `ProbeIndirectArgsSrv`, GBuffer (Normal /
  Depth), `MeshSDFBindless` (TtAtlasMeshCardManager 提供),
  `GlobalSDFClipmapSrv[4]` (TtAtlasGlobalSDFManager 提供, **本系统自带,
  非 ReSTIR 复用**), `FinalLightingAtlasSrv`, `VoxelClipmapSrv[4]`,
  `PrevProbeOctahedralSrv` (用于 product importance sampling)
- **输出 pin**: `ProbeOctahedralUav` (RGBA Float16, 8×8 atlas),
  `ProbeHitTUav` (R16Float, 8×8 atlas)
- **shader**: `ScreenProbeTrace.compute` (Permutation 切 HW RayQuery 1.1
  inline RT 或 SW HybridTrace)
- **Permutation**: `ENV_USE_HARDWARE_RT` (1=HW RayQuery, 0=SW
  HybridTrace), 与现有 `ReSTIRInitialSampling.compute` 的 permutation
  风格一致
- **cbuffer**:

  ```hlsl
  cbuffer cbProbeTrace {
      uint4  TraceParams;      // .x = RaysPerProbe, .y = TemporalFrameIndex,
                               // .z = EnableProductIS, .w = reserved
      float4 TraceFloatParams; // .x = TraceMaxT, .yzw = reserved
  };
  ```

#### #8 `TtAtlasScreenProbeFilterNode` —— probe 时空 reuse

- **频率**: 每帧, multi-iteration (默认 3 轮 spatial + 1 轮 temporal)
- **职责**: ① spatial bilateral filter (16×16 邻域, plane + visibility
  weight); ② temporal EMA (4 帧, history reproject); ③ 同时输出 SH3
  低频版本给 specular 高粗糙度回退用
- **输入 pin**: `ProbeOctahedralSrv`, `ProbeHitTSrv`, `ProbeListSbSrv`,
  GBuffer (Normal / Depth), `PrevFilteredOctahedralSrv` (history)
- **输出 pin**: `FilteredOctahedralUav`, `ProbeSH3Uav` (RGBA Float16 ×9)
- **shader**: `ScreenProbeFilter.compute` (含 2 个 entry: `CSSpatial`,
  `CSTemporal`); ping-pong 参考 `DenoiseNode.cs` 的 `mCurrentInputSrv /
  mCurrentOutputUav` 暂存模式 (`SKILL.md` §3.6)
- **cbuffer**:

  ```hlsl
  cbuffer cbProbeFilter {
      uint4  IterParams;       // .x = StepSize, .y = Iteration,
                               // .z = MaxAccumFrames, .w = reserved
      float4 SigmaParams;      // .x = PlaneSigma, .y = VisibilitySigma,
                               // .zw = reserved
  };
  ```

#### #9 `TtAtlasFinalGatherNode` —— 像素级 indirect diffuse

- **频率**: 每帧
- **职责**: 每像素找 4 个最近 probe, 按 §2.6 公式插值 → indirect diffuse
- **输入 pin**: `FilteredOctahedralSrv`, `ProbeSH3Srv`, `ProbeListSbSrv`,
  GBuffer (Normal / Depth)
- **输出 pin**: `IndirectDiffuseUav` (RGBA Float16, alpha = avgHitT)
- **shader**: `AtlasFinalGather.compute`
- **cbuffer**:

  ```hlsl
  cbuffer cbFinalGather {
      uint4  ScreenParams;     // .xy = ScreenSize, .z = BaseProbeStride,
                               // .w = reserved
      float4 SigmaParams;      // .x = ProbePlaneSigma, .yzw = reserved
  };
  ```

#### #10 `TtAtlasReflectionNode` —— 单独 reflection 通路

- **频率**: 每帧
- **职责**: 每像素按 GGX 重要性采样 1 ray (高粗糙度 ≥ 0.6 时直接读
  IndirectDiffuse 跳过 trace), 串接 Screen→MeshSDF→GlobalSDF→Sky, hit
  采 `FinalLightingAtlas`, 输出未 denoise 的 specular radiance
- **输入 pin**: GBuffer (Normal / Roughness / Metallic), `IndirectDiffuseSrv`
  (高粗糙回退), `MeshSDFBindless`, `FinalLightingAtlasSrv`,
  `VoxelClipmapSrv[4]`
- **输出 pin**: `IndirectSpecularUav` (RGBA Float16, alpha = pdf),
  `SpecularHitTUav` (R16Float, 给后续 denoise)
- **shader**: `AtlasReflection.compute`
- **Permutation**: `ENV_USE_HARDWARE_RT` (同 #7), `ENV_REFLECTION_MODE`
  (0=SurfaceCache, 1=HitLighting)
- **cbuffer**:

  ```hlsl
  cbuffer cbReflection {
      float4 ReflParams;       // .x = RoughnessFallbackThreshold (默认 0.6),
                               // .y/.z/.w = reserved
      uint4  ReflFlags;        // .x = MaxTranslucentSkipCount,
                               // .y = EnableSortedDeferred,
                               // .z/.w = reserved
  };
  ```

#### #11 `TtAtlasCompositeNode` —— 合成到 lighting buffer

- **频率**: 每帧, 在引擎 TAA 之前
- **职责**: `Final = directDiffuse + indirectDiffuse * BaseColor +
  directSpec + indirectSpec * F`, 写回引擎主 lighting RT
- **输入 pin**: `IndirectDiffuseSrv`, `IndirectSpecularSrv`, GBuffer (全部),
  `DirectLightingRtSrv` (引擎现有直接光照输出)
- **输出 pin**: `SceneColorRtUav` (引擎主 lighting RT, BFT_UAV | BFT_SRV;
  Composite 走 compute, 不需要 BFT_RTV; 如果走 fragment 则改 BFT_RTV)
- **shader**: `AtlasComposite.compute`
- **cbuffer**:

  ```hlsl
  cbuffer cbComposite {
      float4 Intensity;        // .x = IndirectDiffuseIntensity,
                               // .y = IndirectSpecularIntensity,
                               // .z/.w = reserved
      uint4  Flags;            // .x = EnableEnergyCompensation,
                               // .yzw = reserved
  };
  ```

### 3.4 Node 在 RenderPolicy 中的串接顺序

```
[已有] GBufferNode  →  ShadowMapNode  →  DirectLightingNode
                                                ↓
[新增] AtlasSurfaceCacheUpdateNode    (#2, 每帧 atlas 维护)
       AtlasCardCaptureNode           (#1, 每帧 ≤16 张, ExecuteIndirect)
       AtlasSurfaceCacheDirectLighting(#3, 每帧 1024² texel)
       AtlasSurfaceCacheIndirectLight (#4, 每帧 512² texel, 用上帧 FinalLighting)
       AtlasVoxelLightingUpdateNode   (#5, 每帧 modified bricks)
       ───────────────────────────── (Surface Cache + Voxel 准备好)
       AtlasScreenProbeAllocNode      (#6)
       AtlasScreenProbeTraceNode      (#7, 投 ray)
       AtlasScreenProbeFilterNode     (#8, ping-pong 多轮)
       AtlasFinalGatherNode           (#9, indirect diffuse)
       AtlasReflectionNode            (#10, indirect specular)
       AtlasCompositeNode             (#11, 合到 SceneColor)
                                                ↓
[已有] TAA  →  Tonemap  →  PostProcess  →  UI  →  Present
```

**关键依赖**: #4 读上一帧 `FinalLightingAtlas` → 这是一帧延迟 (符合
Lumen "n+2 bounce 反馈" 设计, `[S2022 P76]`); #5 voxel update 读当帧
`FinalLightingAtlas` (#4 写完之后).

### 3.5 强制规约 (写 Node 时必看)

每个 Node 落地都必须满足 (与 ReSTIRGINode.cs 对齐):

1. CBuffer 创建走 `OnDrawCall` 阶段 + 全字段 SetValue + MarkDirty +
   FlushDirty (`SKILL.md` §3.1, §3.6)
2. 所有 `BindSrv / BindUav / BindCBV / BindSampler` 都在
   `ShadingEnv.OnDrawCall`, 不在 Tick (`SKILL.md` §3.6)
3. ping-pong / 多 pass 用节点字段 `mCurrentInputSrv / mCurrentOutputUav /
   mCurrentStepSize` 在 Tick→OnDrawCall 之间传值, 不直接 Bind 在 Tick
4. atlas / page table / voxel 等跨节点共享资源走 Manager, 不在节点
   `OnResize` 里重建
5. bindless (MeshSDF / MeshCard / Texture array) 走 §1.4 规约, HLSL 端
   非 wave-uniform 索引必须 `NonUniformResourceIndex(...)`

---

## 4. MaterialGraph 改造点

### 4.1 当前 MaterialGraph codegen 现状摸底

(基于对仓库实际代码的扫描, 见 `CSharpCode/Bricks/CodeBuilder/ShaderNode/
MaterialOutput.cs` L210~L302 与 `CSharpCode/Grapics/Pipeline/Shader/
ShaderPredefineType.cs` L362~L412)

- **入口函数**: `MaterialOutput.BuildStatements` 把 graph 编译成两个
  C-style 函数:

  ```hlsl
  void DO_PS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl);
  void DO_VS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl);
  ```

  PS 版本生成所有非 `VertexOffset*` 的 graph 输出, VS 版本只生成
  `VertexOffset*` (顶点位移).

- **`MTL_OUTPUT` 字段** (定义在 `ShaderPredefineType.cs`, **共 17 个
  字段**, 不是设计文档之前写的 "9 字段"):

  | 字段 | 类型 | 含义 | Card Capture 是否需要 |
  |---|---|---|---|
  | mAlbedo | float3 | base color | ✓ Albedo atlas |
  | mNormal | float3 | tangent space normal | ✓ Normal atlas |
  | mMetallic | float | metallic | ✗ 折算到 Albedo (§2.3) |
  | mRough | float | roughness (= 1 - smoothness) | ✗ 折算到 Albedo |
  | mAbsSpecular | float | non-metal specular intensity | ✗ 折算到 Albedo |
  | mTransmit | float | translucency | △ Opacity atlas |
  | mEmissive | float3 | HDR emissive | ✓ Emissive atlas |
  | mFuzz | float | cloth fuzz | ✗ (Cloth 扩展, §4.3) |
  | mIridescence | float | iridescence | ✗ |
  | mDistortion | float | refraction | ✗ |
  | mAlpha | float | opacity | ✓ Opacity atlas |
  | mAlphaTest | float | alpha test threshold | ✗ Card Capture 关闭 alpha test |
  | mVertexOffset | float3 | VS 位移 | ✗ Card Capture 用 mesh local space |
  | mSubAlbedo | float3 | SSS sub-color | △ 高端档可烘第 6 张 atlas |
  | mAO | float | ambient occlusion | ✓ 写入 Albedo atlas 的 alpha |
  | mMask | float | misc mask | ✗ |
  | mShadowColor / mDeepShadow / mMoodColor | ... | art-only | ✗ |

- **graph 求值机制**: 通过 `FieldPins` 遍历, 对有 linker 的输入引脚做
  表达式构造, 形如 `mtl.mAlbedo = <pin expression>`. graph 节点的具体
  统计、UniformVars、采样器都是在这一步收集的.

### 4.2 codegen 增加 `DO_CARD_CAPTURE_IMPL` 路径

新增第 3 个 codegen 出口, 与 PS / VS 函数**共享同一份 graph 表达式构
造**, 只换函数签名和写回字段集合:

```hlsl
// 新增, 由 MaterialOutput.BuildStatements 在 PSFunction / VSFunction 之外
// 再生成一份, 用于 Card Capture pass 的 PS
void DO_CARD_CAPTURE_IMPL(in CARD_CAPTURE_INPUT input, inout MTL_OUTPUT mtl)
{
    // 只把 §4.1 表里 "Card Capture 需要" 的字段从 graph 求值并写回
    mtl.mAlbedo   = <expression>;
    mtl.mNormal   = <expression>;
    mtl.mEmissive = <expression>;
    mtl.mAlpha    = <expression>;
    mtl.mAO       = <expression>;
    // (其余字段保持 MTL_OUTPUT 的默认值, 不参与 graph 求值)

    // §2.3 specular/metallic 折算到 albedo:
    float specEnergy = lerp(mtl.mAbsSpecular, 0.5, mtl.mMetallic);
    mtl.mAlbedo *= (1.0 - specEnergy);
}
```

**与 `DO_PS_MATERIAL_IMPL` 的差异**:

| 维度 | PS 版本 | Card Capture 版本 |
|---|---|---|
| 输入类型 | `PS_INPUT` | `CARD_CAPTURE_INPUT` (新结构, §4.4) |
| 输出字段 | 全部 17 字段 | 只 5 字段 (Albedo / Normal / Emissive / Alpha / AO) |
| `ddx/ddy` | 可用 | **不可用** (ortho raster 没合理的屏幕导数) |
| Mip 选取 | 自动 | 必须 fixed mip 或 ray cone (见 §4.4) |
| Alpha test | `clip(mAlpha - mAlphaTest)` | **禁用** (§2.3 必守) |
| `mVertexOffset` | 走 VS 函数 | 完全跳过 (Card Capture 用 mesh local space, 不做 displacement, 否则 Card 与原 mesh 几何不一致) |
| `WorldPosition / WorldNormal` | 由 raster 插值 | 来自 ortho 投影矩阵 (§4.4) |

**实现位置**: 在 `MaterialOutput.cs` L210 附近的 `BuildStatements` 方法
里, 在 `PSFunction` / `VSFunction` 之后再加一个 `CardCaptureFunction`
(`TtMethodDeclaration`) 走相同的 graph 遍历逻辑, 但 `FieldPins` 过滤改
为白名单 5 字段, 同时跳过 `mVertexOffset`.

**Permutation 配合**: 在 Card Capture 的 ShadingEnv
(`AtlasCardCapture.shadingenv.var`) 里 `PushPermutation("ENV_CARD_CAPTURE", 1)`,
shader 端用 `#if ENV_CARD_CAPTURE` 把所有 graph 内部的纹理采样从
`tex.Sample(samp, uv)` 改写为 `tex.SampleLevel(samp, uv, 0)` (mip 0
固定). 经核查, 本仓库 `MaterialOutput.cs` / `Material.cs` 当前**没有**
统一的 `SAMPLE_TEX2D` 之类的采样宏, 美术 graph 的纹理节点直接调
`Texture2D::Sample`. 所以改造方案是: 在 `MaterialOutput.cs` 的 codegen
里, 当走 `CardCaptureFunction` 分支时, 把所有
`<TextureSampleNode>.BuildStatements` 生成的 `tex.Sample(samp, uv)` 表
达式替换为 `tex.SampleLevel(samp, uv, 0)`. 这是一处 codegen 表达式重
写, 放在 §4.5 的 checklist 里.

### 4.3 ShadingModel-aware 的 Atlas 通道扩展

**核心问题**: Lumen 在 Surface Cache 里**只存 Default Lit 一个 BRDF 模
型**, 把 Cloth / Hair / Eye 这些非默认 ShadingModel 的能量也强行折算
到 Default Lit 的 Albedo `[S2022 P69]`. 这是论文里明确的简化, 但本仓
库的 §1.2 设计目标包含 "多 ShadingModel 支持", 所以必须做扩展.

**方案选择** (3 选 1):

#### 方案 A (推荐): 固定通道 + ShadingModel ID 路由

- `MTL_OUTPUT` 不动, atlas 数量不变 (仍是 §2.2 那 8 张).
- Capture 时把 **`mtl.mShadingModelId` (新增字段)** 写到 `Normal` atlas
  的某个空闲 bit (Normal 用 RG8 octahedral 后 alpha 通道空着, 8 bit
  正好 256 个 ShadingModel).
- Lighting 时按 ShadingModel ID 走 `switch`, 在 SurfaceCacheLighting
  的 compute 里分支求 BRDF (DefaultLit / Cloth / Hair / Eye 各一个
  分支). atlas 字段保持复用 (mAlbedo 仍代表 base color, 不同 model
  解读不同).

**优点**: atlas 数量 / 显存不变, 只改 lighting compute. 最贴近 Lumen.
**缺点**: 表达力受限 (Cloth fuzz 没专属通道, 只能从 mRough / mFuzz
共用一个槽近似).

#### 方案 B: per-ShadingModel 子 atlas

- 主 atlas 还是那 8 张; 给 Cloth / Hair / Eye 各加一张 1Kx1K 的小 atlas
  (CRGBA Float16 BC6H, 8 MB/张), 存额外参数.
- Card Capture pass 按 ShadingModel ID 决定写哪张子 atlas.
- Lighting 时按 ShadingModel ID 决定读哪张子 atlas.

**优点**: 表达力完整. **缺点**: 显存涨 24~40 MB; capture / lighting
都要按 model 多采一次纹理.

#### 方案 C: bindless atlas array

- 主 atlas 数量按需配置 (DefaultLit 8 张, +Cloth 1 张, +Hair 2 张...),
  通过 `TtBindless` (§1.4) 把所有 atlas 暴露为 `Texture2D[]`.
- ShadingModel 决定 bindless index 范围.

**优点**: 完全可扩展, 后续加 BRDF 不用改管线. **缺点**: bindless 索
引非 wave-uniform 时必须 `NonUniformResourceIndex(...)`, lighting
pass 的 wave 一致性会被破坏, 性能不如方案 A.

**决策**: **L2~L7 阶段先走方案 A** (与 Lumen 论文一致, 最小风险);
方案 B/C 留作 L8 之后的可选增强 (§5 实施阶段没排期).

**`MTL_OUTPUT` 新增字段** (方案 A 落地需要):

```csharp
// ShaderPredefineType.cs L362 附近, MTL_OUTPUT 末尾追加
[TtShaderDefine(ShaderName = "mShadingModelId")]
public uint mShadingModelId;          // 0=DefaultLit 1=Cloth 2=Hair 3=Eye 4=SSS ...

[TtShaderDefine(ShaderName = "mShadingModelParam0")]
public float mShadingModelParam0;     // model-specific param 1 (e.g. Cloth fuzz)

[TtShaderDefine(ShaderName = "mShadingModelParam1")]
public float mShadingModelParam1;     // model-specific param 2 (e.g. Cloth sheenTint)
```

`mShadingModelId` 同时被 raster GBuffer (写到 GBufferRT1.a) 和 Card
Capture (写到 Normal atlas alpha) 使用. `Param0/1` 跟 `mShadingModelId`
同 atlas (Normal atlas 的 alpha 是 8 bit ShadingModel; 再单独占 Emissive
atlas 的 alpha 8 bit 给 Param0; Param1 走 Albedo atlas alpha).

### 4.4 `CARD_CAPTURE_INPUT` 结构定义

新结构, 替代 PS_INPUT 用于 Card Capture pass. 字段集合是 PS_INPUT 的
**真子集** (Card Capture 没 viewport, 没 lightmap UV, 没 instance
data 等), 同时多两个 ortho 相关字段:

```csharp
// 建议加在 CSharpCode/Grapics/Pipeline/Shader/ShaderPredefineType.cs
// PS_INPUT 定义之后
[TtShaderDefine(ShaderName = "CARD_CAPTURE_INPUT", Order = 1)]
public struct CARD_CAPTURE_INPUT
{
    [TtShaderDefine(ShaderName = "vWorldPos")]
    public Vector3 vWorldPos;          // 来自 ortho VS, mesh local→ortho clip 投影前的 world pos

    [TtShaderDefine(ShaderName = "vWorldNormal")]
    public Vector3 vWorldNormal;       // mesh vertex normal 经 ortho 矩阵后

    [TtShaderDefine(ShaderName = "vUV0")]
    public Vector2 vUV0;               // 主 UV, 必有

    [TtShaderDefine(ShaderName = "vUV1")]
    public Vector2 vUV1;               // 次 UV (lightmap UV 复用), 可选

    [TtShaderDefine(ShaderName = "vCardLocalUV")]
    public Vector2 vCardLocalUV;       // 当前像素在 card 上的归一化坐标 (0~1)

    [TtShaderDefine(ShaderName = "vCardDepth")]
    public float vCardDepth;           // ortho 投影深度, 用于写 Depth atlas

    [TtShaderDefine(ShaderName = "vCardAxis")]
    public uint vCardAxis;             // 0..5 = ±X/±Y/±Z, 用于 6 轴归类
};
```

**Sampler 行为**. Card Capture PS 是一个 ortho 投影 raster, 其 ddx/ddy
是合法的 (硬件 PS 内 2×2 quad 派生导数仍能算), 但因为 ortho 相机的像
素物理尺寸跟正常透视相机差异极大, 直接走 `Sample` 会导致 mip 选择不
合理 (经常掉到最高 mip). 论文 `[S2022 P68]` 的做法是**强制 mip 0**
(配合 Nanite continuous LOD 的 simplified mesh, 已经是合适的几何分辨
率, 不需要再做纹理 mip).

落地方案: 走 §4.2 中描述的 codegen 表达式重写 —— 所有
`<TextureSampleNode>` 在 `CardCaptureFunction` 分支里生成
`tex.SampleLevel(samp, uv, 0)` 而不是 `tex.Sample(samp, uv)`. 不引入
新宏, 不依赖运行时 permutation 分支 (`#if ENV_CARD_CAPTURE` 仅用于
PS 输出 RT 数量与 alpha test 跳过, 不用在 sampler 路径上).

### 4.5 改造影响面与改动 checklist

| 文件 | 改动 |
|---|---|
| `CSharpCode/Bricks/CodeBuilder/ShaderNode/MaterialOutput.cs` | 新增 `CardCaptureFunction` 字段 + `BuildStatements` 里第 3 段 codegen: ① 字段白名单 = `{mAlbedo, mNormal, mEmissive, mAlpha, mAO, mShadingModelId, mShadingModelParam0, mShadingModelParam1}`; ② 跳过 `mVertexOffset`; ③ 末尾追加 `specEnergy = lerp(mAbsSpecular, 0.5, mMetallic); mAlbedo *= (1 - specEnergy);` |
| `CSharpCode/Bricks/CodeBuilder/ShaderNode/TextureSampleNode.cs` (或对应纹理采样节点类) | `BuildStatements` 增加分支判断当前 codegen 上下文 (PS / VS / CardCapture); CardCapture 走 `tex.SampleLevel(samp, uv, 0)`, 其余维持 `tex.Sample(samp, uv)` |
| `CSharpCode/Grapics/Pipeline/Shader/ShaderPredefineType.cs` | `MTL_OUTPUT` 末尾追加 `mShadingModelId / mShadingModelParam0 / mShadingModelParam1`; 新增 `CARD_CAPTURE_INPUT` 结构 |
| `enginecontent/Shaders/GI/Atlas/AtlasCardCapture.shadingenv.var` | 新建 ShadingEnv, `PushPermutation("ENV_CARD_CAPTURE", 1)`, PS 调用 `DO_CARD_CAPTURE_IMPL`, MRT 输出 4 RT (Albedo / Normal / Emissive / Depth, alpha 各自携带 ShadingModelId / Param0 / Param1) |
| `CSharpCode/Grapics/Pipeline/GI/Atlas/TtAtlasCardCaptureNode.cs` | §3.3 #1 节点实现, OnDrawCall 阶段 Bind 5 张 atlas UAV + cbCardCapture |

**C# `.projitems` 登记** (skill §"C# 新建源码文件必须登记到 .projitems"):

- `CSharpCode/Grapics/Grapics.projitems` 追加 **15 个**新 .cs:
  - 4 个 Manager: `TtAtlasMeshCardManager.cs`,
    `TtAtlasSurfaceCacheManager.cs`, `TtAtlasVoxelLightingManager.cs`,
    `TtAtlasGlobalSDFManager.cs`
  - 11 个 Node: 见 §3.1 列表
  - 加上后续 §2.8 `TtAtlasTemporalDenoiseNode.cs` (实施时若决定独立成
    Node 则补登记)

**HLSL 文件登记**: 经核查, 本仓库 `enginecontent/Shaders/GI/ReSTIR/`
下的 `.compute` / `.cginc` 文件**不**登记到 `enginecontent/Shaders/
CoreShader.vcxitems` (`file_grep` 在 vcxitems 里搜 `ReSTIRInitialSampling.compute`
等无任何匹配). 说明引擎 runtime 通过目录扫描 + `RName` 资源系统按需加
载 HLSL, 不走 vcxitems 编译列表. 因此 `enginecontent/Shaders/GI/Atlas/`
下的新 shader **直接放入对应目录即可, 无需修改任何 xml**. 但务必满足:

- 文件名 / 路径大小写与 git 真实大小写一致 (skill §"Windows + git 文
  件路径大小写规约"), 通过 `git ls-files enginecontent/Shaders/GI/`
  确认现有 `ReSTIR/` `SH/` `TrayTracing/` 三个子目录的真实大小写后,
  再以同样风格创建 `Atlas/`.
- 目录名 `Atlas` 用首字母大写 (与 `ReSTIR/` `SH/` 一致).

---

## 5. 实施阶段计划

| 阶段 | 周期 (人周) | 内容 | 前置依赖 |
|---|---|---|---|
| L0 | 已完成 | Bindless 基础设施 (TtBindless wrapper + CodingGuidelines.md §1.4 规范) | 无 |
| L0.5 | 已完成 | SIGGRAPH 2022 Lumen course + Skorobogatova 2022 论文调研, 填充本文档第 2/3/4 节 (本任务交付) | L0 |
| L1 | 1-2 | ① 场景级 `TtSceneMaterialTable` / `TtSceneMeshTable` (CodingGuidelines.md §1.4.8.3 Phase B); ② GBuffer 扩展 ShadingModelId 通道 (RT1.a 复用 + Emissive 落地, 见 §2.6); ③ 4 个 Manager 类骨架 (`TtAtlasMeshCardManager` / `TtAtlasSurfaceCacheManager` / `TtAtlasVoxelLightingManager` / `TtAtlasGlobalSDFManager`) | L0 |
| L2 | 3-4 | Mesh Card 生成 (offline asset cook) + Card Capture pass + MaterialGraph codegen 改造 (§4 全部条目) | L0.5, L1 |
| L3 | 2-3 | Surface Cache atlas 管理 (page table / LRU evict / GPU feedback / page hash 合并 / virtual lookup fallback) | L2 |
| L3.5 | 1-2 | Surface Cache Direct + Indirect Lighting (§3.3 #3 + #4, n+2 bounce 反馈机制) | L3 |
| L4 | 2-3 | ① TtAtlasGlobalSDFManager (4 级 256³ clipmap, mesh SDF brick streaming, modified bricks 更新); ② TtAtlasVoxelLightingUpdate (§3.3 #5, visibility buffer + radiance gather) | L3.5 |
| L5 | 2-3 | Screen Probe (§3.3 #6 alloc + #7 trace + #8 filter, 含 product importance sampling) | L3.5, L4 |
| L6 | 1-2 | Final Gather (§3.3 #9, probe → per-pixel indirect diffuse) | L5 |
| L7 | 2 | Reflection 单独通路 (§3.3 #10, Surface Cache 模式优先, Hit Lighting 模式作为 Permutation 选项) | L3.5, L4 |
| L8 | 1-2 | Denoise (TtAtlasTemporalDenoiseNode, SVGF) + Composite (§3.3 #11) + 与引擎 TAA 串联 | L6, L7 |
| L9 | 1 | 性能调优 (compaction / wave 一致性 / GPU 计时 / shading divergence), 风险关闭 | L8 |
| **总计** | **16-23 人周** (一人全职) | | |

每个阶段独立可交付, 阶段之间可以分别 review / debug, 避免大爆炸式重构.
周期范围下界对应"已经熟悉本仓库 RHI / RenderGraph 的资深工程师", 上
界对应"需要边做边学引擎机制的中级工程师".

---

## 6. 与 ReSTIR 轨道的协作策略

- **共享基础设施**: `TtSceneMaterialTable` / `TtSceneMeshTable` (规划在
  CodingGuidelines.md §1.4.8.3 Phase B, **L1 阶段才会实现**, 当前两条
  轨道都还没用上). L1 完成后两条轨道复用同一份, 不重复实现.
- **运行时切换**: `TtRenderPolicy` 增加配置项 `EGiPath`
  (`Disabled` / `ReSTIR` / `TtAtlas`), **二选一启用**, 不允许两条同时
  跑. `Disabled` 模式下场景只有直接光 + 天光环境光, 不参与 GI (本仓库
  当前**没有 lightmap 系统**, 不存在 fallback 到 baked lighting 的选
  项, 之前文档里提到的"fallback 到 lightmap"是误写, 已修正).
- **Global SDF 不共享**: 经核查, ReSTIR 轨道当前未实现 SW Global SDF
  (走 HW RT BVH + screen trace), TtAtlas GI 自带的 `TtAtlasGlobalSDFManager`
  独立实现. 如果将来 ReSTIR 也需要 SW 远场 trace, 可以反向复用 TtAtlas
  的 Global SDF.
- **Atlas 反哺 ReSTIR (潜在扩展, 当前不在计划)**: 如果将来想让 ReSTIR
  HW RT 分支在桌面 PC 上质量接近 TtAtlas GI, 可让 ReSTIR 的 closest hit
  shader 查询 TtAtlas Surface Cache 的 `FinalLighting` atlas (而不是自
  己重新求 PBR 4 字段, 即 `[S2022 P96]` 的 *Surface-Cache Payload* 同
  款思路). 这是 L9 之后的可选优化, 当前不在计划内.

---

## 7. 风险与开放问题 (基于 Lumen 论文经验细化)

| # | 风险 | 触发条件 | 影响 | 缓解方案 |
|---|---|---|---|---|
| R1 | **Card 自动生成对复杂 mesh 失败** | 非流形 mesh / 高曲率表面 / 镂空结构 (栅栏 / 树叶 / 链条) | Card 覆盖率 < 80%, atlas 出现大片"未覆盖区域", lighting 漏光或丢失 | ① surfel sampling 时增加非流形检测, 失败 mesh fallback 到强制 6 面 cubemap-like 投影 (`[S2022 P71]` Card Merging 同款); ② 美术 DCC 端提供"手动 card 标注"工具补救; ③ 未覆盖 texel 的 sample 走 §2.4 voxel lighting 兜底 |
| R2 | **Atlas 显存预算在大世界场景下超限** | 开放世界 ≥ 1M instance, 同时 reflection 触发大量 high-res page 请求 | 168 MB 预算被穿透, on-demand pages 频繁 evict, surface cache 命中率 < 50%, GI 闪烁 | ① 严格执行 §2.2 LRU heap, 显存满时按"最近未用时间 × 距离"评分 evict; ② 引入 §2.1 *Card Merging* (`[S2022 P71]`) 把远处小 instance 合并; ③ Config 选项允许调整 atlas 大小 (4Kx4K → 8Kx4K), 分辨率提到 168 → 336 MB |
| R3 | **动态 mesh (skeletal / morph target) 的 atlas 更新延迟** | 角色 mesh 高频形变 (动画 / 物理) | atlas 是按 rest pose 烘的, 形变后 SDF / card 投影错位, GI 拖影 | ① 动态 mesh **不参与 surface cache**, 走 ReSTIR 同款的 dynamic object special path (§6 协作里的"Hit Lighting 反哺"机制); ② skeletal mesh 改用 per-bone OBB 近似 + 多张 card 关联 (各 bone 独立) |
| R4 | **MaterialGraph codegen 三路径维护成本** | graph 改一次, 现在要同时生成 PS / VS / CardCapture 三个 IMPL | 美术改一个材质, 引擎要重新编译 3 份 shader, 编辑器响应变慢 | ① codegen 走"共享 graph 表达式 + 不同 entry 函数"模式 (§4.2 实现位置已设计), 表达式构造只跑 1 次; ② 编辑器内 incremental compile, 只重编受影响的 IMPL 函数 |
| R5 | **Voxel grid 世界空间分辨率不足导致漏光** | 64³ × 4 clipmap 在 200m 半径下, 每 voxel 边长 ≈ 80cm | 远场 GI 把整面墙当成"一束光", 室内可能被户外光穿墙照亮 | ① clipmap 分辨率提到 96³ (显存涨 ≈ 1.5x); ② 在 §2.4 *Visibility Buffer* (`[S2022 P81]`) 里加 thin wall 检测, voxel 内有薄面时走子体素遮挡 |
| R6 | **GBuffer ShadingModelId 通道扩展打破现有渲染管线** | RT1.a 当前由 ObjectFlags_2Bit / 3 占用, 改成 "2 bit ObjectFlags + 6 bit ShadingModelId" 后, 所有 lighting shader 必须重写解码 | 现有 `DeferredCommon.cginc::DecodeGBuffer` 与所有 deferred lighting pass 失效 | L1 阶段集中改造 (见 §5), 改造范围限定在 `enginecontent/Shaders/ShadingEnv/Deferred/`, 对 forward pass 无影响; PR 必须包含完整 deferred lighting 回归 test |
| R7 | **缺少 Nanite, Card Capture 单帧预算可能跑不完** | 论文用 Nanite 的"单 vis-buffer draw + 每材质一个 dispatch" (`[S2022 P68]`), 本仓库没有 | 首版 §2.3 把每帧预算从 512x512 降到 128x128 (对应 ~16 张 64² card), 烘焙速度 4 倍变慢, 大场景可能滞后明显 | ① 利用引擎现有的 indirect draw 机制 (`Effector.cs::BindIndirectDispatchArgsBuffer` 同款) 把 N 张 card 合并到一次 indirect draw; ② 长期方案是接入 mesh shader / GPU-driven raster (与本仓库其他高优需求合并立项) |
| R8 | **NxRHI 当前的 indirect dispatch API 受限** | `BindIndirectDispatchArgsBuffer` 仅在 Particle (`Effector.cs`) 用过, 其它路径未验证 | §2.0 hybrid pipeline 的 *compaction* + ExecuteIndirect 模式可能踩到未发现的 RHI bug | L4 / L5 阶段实现前**必须**先做 RHI 接口 spike (单写 1 个 compute → 写 args → 第 2 个 compute indirect dispatch 的最小 demo), 排除 RHI 问题再开始 GI 接入 |
| R9 | **HW RayQuery 在 Mac (Metal) 缺失, 需要 SW fallback** | §1.1 设计目标包含 Mac M2 Pro/Max, 但 Metal 没有 DXR-1.1 inline RT | `ENV_USE_HARDWARE_RT=1` 在 Mac 上无法编译 | §3.3 #7 / #10 的 Permutation 必须保证 `ENV_USE_HARDWARE_RT=0` 路径完整可用, Mac 平台默认走 SW HybridTrace, 性能档位文档化 |
| R10 | **Surface Cache 多 ShadingModel 表达力不足 (方案 A 折中)** | §4.3 方案 A 复用 atlas 通道, Cloth 的 fuzz / sheen / Hair 的 tangent / scatter 等都挤压在 mAlbedo / mShadingModelParam0/1 里 | 复杂材质 (Cloth + 多层 sheen) 在 GI 下色调偏差 10~20% | ① 验收阶段对每种 ShadingModel 做"GI ON 与 path-traced 参考图"对比, 偏差 > 阈值时升级为方案 B (per-ShadingModel 子 atlas); ② Param0/1 插槽不够时直接扩到 Param0~3 (再占 RT3.a 等 GBuffer 空闲 8bit) |

---

## 8. 参考资料

### 8.1 主参考 (本设计直接依赖)

- **`[S2022]`** Wright, D., Narkowicz, K., Kelly, P. *Lumen — Real-time
  Global Illumination in Unreal Engine 5*. SIGGRAPH 2022 *Advances in
  Real-Time Rendering in Games* course, 199 pages. 本地副本:
  `cache/research/SIGGRAPH2022-Advances-Lumen-Wright%20et%20al.pdf`.
  在线: `https://advances.realtimerendering.com/s2022/index.html`.
  本文档 §2 / §3 中所有 `[S2022 P##]` 形式的引用均指此 PDF 的具体页码.

- **`[Sko2022]`** Skorobogatova, A. *Real-Time Global Illumination in
  Unreal Engine 5*. Master's Thesis, Faculty of Informatics, Masaryk
  University, Brno, Fall 2022, 66 pages. 本地副本:
  `cache/research/real-time_GI_in_UE5.pdf`. 在线 (MUNI Thesis Archive):
  `https://is.muni.cz/th/u3a45/`. 主要用于 §4.1 / §5.1 中关于
  Acceleration Structure / Material Sampling / Surface Cache Atlas 的
  解释性补充.

### 8.2 参考实现的前置 / 衍生工作

- Wright, D. *Dynamic Occlusion with Signed Distance Fields*. SIGGRAPH
  2015 *Advances* course. (Mesh SDF tracing 的鼻祖, `[S2022 P6]` 引用)
- Aaltonen, S. *GPU-based clay simulation and ray-tracing tech in
  Claybook*. GDC 2018. (Mip 加速的 SDF raymarch + Eikonal propagation,
  `[S2022 P36, P42]` 引用)
- Wright, D. *Radiance Caching for Real-Time Global Illumination*.
  SIGGRAPH 2021 *Advances* course. 在线:
  `https://advances.realtimerendering.com/s2021/index.html`. (SSRC /
  WSRC 的前身, Lumen Final Gather 的算法基础)
- Karis, B. et al. *A Deep Dive into Nanite Virtualized Geometry*.
  SIGGRAPH 2021 *Advances* course. (`[S2022 P68]` 引用, Card Capture
  10x 加速的关键)
- Aalto, T. *Sorted-Deferred Ray Tracing*. ReBoot Develop 2018; Kelly,
  P. et al. *Ray Tracing in Fortnite*, in Marrs/Shirley (eds.) *Ray
  Tracing Gems II*, 2021. (`[S2022 P99~P100]` 引用, Hit-Lighting
  pipeline 的算法基础)
- Tabellion, E., Lamorlette, A. *An Approximate Global Illumination
  System for Computer Generated Films*. SIGGRAPH 2004. (`[S2022 P112]`
  引用, ray tracing 多 LOD 自相交的解决思路)

### 8.3 工程对照参考 (用于本仓库实施)

- **本仓库已合规节点**:
  `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs::TtReSTIRGINode`
  —— RenderGraphNode + multi-pass + 共享 cbuffer 的范本.
- **本仓库 Denoise 模板**:
  `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs::TtDenoiseNode`
  —— ping-pong 多迭代 + Tick 暂存 + OnDrawCall 绑定的范本.
- **本仓库 indirect dispatch 现有用法**:
  `CSharpCode/Bricks/Particle/Effector.cs` 中的
  `mParticleUpdateDrawcall.BindIndirectDispatchArgsBuffer(...)` ——
  §2.0 hybrid pipeline compaction 的现成 RHI 入口.
- **本仓库 GBuffer 编解码**:
  `enginecontent/Shaders/ShadingEnv/Deferred/DeferredCommon.cginc::
  GBufferData` —— §2.6 GBuffer 耦合的实际数据结构, L1 阶段
  ShadingModelId 改造的目标文件.
- **本仓库强制规范**:
  - `Documents/Coding/CodingGuidelines.md` §1.1 (CBuffer 创建后必须
    SetValue + FlushDirty)
  - `Documents/Coding/CodingGuidelines.md` §1.2 (BindXxx / CreateCBV
    必须在 OnDrawCall 阶段)
  - `Documents/Coding/CodingGuidelines.md` §1.4 (TtBindless 规约)
  - `Documents/Coding/CodingGuidelines.md` §1.4.8.2 (双轨制路线决策)
  - `Documents/Coding/CodingGuidelines.md` §1.4.8.3 (场景级
    `TtSceneMaterialTable / TtSceneMeshTable` Phase B)

### 8.4 不在本设计采用范围内的对比参考

以下方案在调研期评估过, 因路线 / 显存 / 工程量等原因未采纳:

- **NVIDIA RTXDI / RTXGI SDK**. RTXDI 走的是 ReSTIR DI 思路, 与本仓库
  ReSTIR 轨道 (轨道 A) 同源, 不适合 TtAtlas GI 这条 Surface Cache 路
  线. RTXGI (DDGI) 是 probe based, 但精度不如 Lumen Surface Cache,
  且没有反射通路.
- **AMD GI-1.0** (Boisse, 2022, GPUOpen). 双层 radiance cache 的快速
  方案, 是 Surface Cache 路线的轻量替代. 算法上更简但 atlas 表达力比
  Lumen 弱, 不能完整满足 §1.2 的"多 ShadingModel" 与 "高质量
  reflection" 需求, 仅作思路参考.
- **Lumen 开源复现** (社区如 *Lumen-Global-Illumination-In-Unity*,
  *Hybrid-Rendering* 等). 实现度参差, 多数只复刻 Surface Cache 或
  Voxel Lighting 单点, 无完整 hybrid pipeline. 仅作 shader 实现细节
  对照, 不直接借用代码.

---

## 9. 文档维护约定

- 本文档随 TtAtlas GI 实施推进**持续迭代**, 每完成一个 L 阶段, 对应小节
  从"待填充"升级为"已实施", 并记录实际选型与最初设计的偏差.
- 本文档不收录 C# 代码强制规则 (那些在 `CodingGuidelines.md`), 只收录
  TtAtlas GI 系统层面的设计决策 / 子系统拆分 / 算法选型.
- 任何对本文档的重大修改 (子系统拆分调整 / 算法选型变更), 必须在 PR
  描述里说明决策依据.

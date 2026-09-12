# TitanEngine 功能路线图

本文档记录引擎当前的功能缺口与推进计划。引擎在渲染中后段（RenderGraph、ReSTIR GI、虚拟阴影、GpuDriven）已处于第一档水平，因此本路线图聚焦于"把一款完整游戏做完并交付"所需的外围能力。

## 如何使用本文档

- 每个条目带复选框，完成后勾选并在条目末尾补一行 `完成于:` 说明实际落地方案（如与计划不符，直接改写计划正文，保持文档与代码一致）。
- 优先级含义：

| 级别 | 含义 |
| --- | --- |
| P0 | 阻塞游戏交付，缺失则任何品类都做不完整 |
| P1 | 影响品质上限与项目规模，中大型项目必需 |
| P2 | 影响表现力与制作效率，可按品类需求排期 |
| P3 | 平台扩展与工程生态，长期投入 |

- 条目结构统一为：现状 / 目标 / 关键改动点 / 验收标准。"关键改动点"给出的是已确认的代码位置，不是猜测。

## 能力基线（已具备，避免重复投入）

以下能力已有可用实现，规划时不要当作缺口：

- **GPU Skinning**：VS 端 4 骨骼加权蒙皮，见 `enginecontent/Shaders/modifier/SkinModifier.cginc` 的 `DoSkinModifierVS`；骨骼变换以"位置 + 四元数"形式经 `cbSkinMesh` 上传（`AbsBonePos[360]` / `AbsBoneQuat[360]`，见 `enginecontent/Shaders/CBuffer/VarBase_PerSkinMesh.cginc`）；C# 侧由 `TtSkinModifier` 声明 Modifier 与顶点流需求，`TtMdfSkinMesh` 负责绑定 CBV。
- 骨骼动画播放、BlendSpace 1D/2D、动画状态机、动画 Notify（含时间轴编辑 UI）。
- **RootMotion 与 AnimMontage**：已落地，详见 P0-4 条目（含实际方案与遗留缺口），规划动作类玩法时不要再当作缺口。
- **Morph Target / BlendShape**：已落地，详见 P0-1。稀疏 delta 存 `.vms`，`TtMorphModifier` + `TtMdfSkinMorphMesh` / `TtMdfMorphMesh`，编辑器有权重滑条。**但权重尚未接动画曲线，且无切线 delta。**
- **通用时间轴控件** `EGui.Controls.TtTimelineControl`（`CSharpCode/ImGui/Controls/TimelineControl.cs`）：标尺 / 播放头 / 多轨条与打点的选中·拖动·拉伸·右键，Montage 与 Notify 编辑器在用，Sequencer 可以它为基底（但**没有**滚轮缩放 / 滚动 / 轨道树 / 多选 / 曲线绘制，需按 P2-12 阶段 3 扩能）。
- Kawaii 次级物理（chain / cloth / rod，XPBD 求解）。
- 两骨 IK（`TwoBoneIK`）。
- Actor / Component 体系、CharacterMovement、BehaviorTree、Recast 导航（含 NavCrowd 群体避障）。
- RPC 框架 + TCP 传输 + 属性级同步（`TtRpcPropertyAttribute`）。
- 视锥剔除、GPU 剔除、HZB、TAA、FSR、Bloom、SSAO、自动曝光、ToneMapping、ColorGrading LUT、SunShaft、高度雾。
- **延迟贴花（Decal）与屏幕空间反射（SSR）**：已落地，详见 §7 / §8。贴花是 graphics pass 直写
  GBuffer + **材质驱动**（RL_Decal 材质域，对应 UE 的 MaterialDomain=Deferred Decal，
  `TtDecalPassNode` + `GamePlay.Scene.TtDecalNode`），SSR 是 `TtSSRNode` 两 pass compute，两者都已接进
  `enginecontent/graphics/deferred_simple.rpolicy`（引擎默认 policy）。
- ReSTIR GI、VXGI、PRT 探针、虚拟阴影贴图、CSM、AdvShadow、DXR 接入。
- 节点式材质编辑器、RenderGraph 可视化编辑、粒子图编辑器、SDF 字体、Prefab 编辑器、宏图视觉脚本、Python 运行时。
- 资产管线：FBX 导入、纹理 Cook（ASTC/ETC/BC）、网格简化、TitanCMD 批处理、资产引用图（RefGraph）。
- 诊断：CPU / GPU / 内存 Profiler、内嵌 RenderDoc、NVIDIA Aftermath、GpuDump。
- 地形：CDLOD、双精度坐标、地形级 LevelStreaming、地形草。**注意这里只有渲染与物理，没有编辑**——高度与材质唯一来源是 PGC 程序化图，编辑器里无法用笔刷雕刻，见 P2-19。

---

## P0 阻塞游戏交付

### 1. Morph Target / BlendShape 系统

稀疏 morph 资产 + VS 端 delta 叠加已落地，编辑器可拖权重预览。

- **实际方案**
  - **稀疏存储**：`FMorphDelta`（`VertexIndex` + `DeltaPosition` + `DeltaNormal`）为最小单元，`TtMorphTarget`（一个形变目标 = 名字 + delta 数组）聚合成 `TtMorphTargetSet`，随 `TtMeshPrimitives` 存进 `.vms` 的 XND attribute。只存被影响的顶点，不做全量顶点副本。
  - **独立 modifier + 双 Modifier 队列**（与原计划一致）：`TtMorphModifier` 排在 skin 之前，`TtMdfSkinMorphMesh : TtMdfQueue2<TtMorphModifier, TtSkinModifier>`（蒙皮角色）与 `TtMdfMorphMesh : TtMdfQueue1<TtMorphModifier>`（纯 morph 静态网格）两条路并存。顺序由 `TtMdfQueueBase.BuildMdfFunctions` 按 `Modifiers` 列表发射保证。
  - **与原计划的偏离①——GPU 端不做权重累加**：原计划是「delta StructuredBuffer + 活跃权重小 CBuffer」，由 VS 遍历活跃 morph 累加。实际改为**CPU 端累加成逐顶点稠密表**再整表上传（`TtCpu2GpuBuffer<FMorphDelta>`，`StructuredBuffer<FMorphDelta> MorphDeltas`）。取舍：VS 端退化成一次无分支索引 `MorphDeltas[vert.vVertexID]`，与 morph 数量完全无关（原方案 VS 里要循环所有活跃 morph）；代价是权重变化时有 CPU 累加开销，用 `TouchedVertices` 记录上次写过的顶点做增量清零，使每次重算是 O(受影响顶点数) 而非 O(顶点总数)。
  - **与原计划的偏离②——导入走纯 C#**：没有改 `FBXMeshImporter.cpp`，而是用 Assimp 的 `MeshAnimationAttachments`（`aiMesh::mAnimMeshes`）在托管层解析，无需 native 改动与重编。
  - **双写 vert 与 vsOut**：`DoMorphModifierVS` 必须同时写 `vert`（供后续 `DoSkinModifierVS` 读到已形变顶点）与 `vsOut`（纯 morph 路径下没有后续 modifier 覆写 vsOut）。原因是各 ShadingEnv 的 `VS_Main` 里 `Default_VSInput2PSInput` 在 `MdfQueueDoModifiers` **之前**就跑过了，vsOut 里已是未形变副本。
  - **MdfQueue 选型链**（本轮顺带建立的通用机制，见 `CodingGuidelines.md` §7.7）：优先级为「调用方显式指定 > `.ums` 上的 `MdfQueueType` > 按资产内容自动兜底」。导入期即把四路之一写进 `.ums`；`TtRenderMesh.Initialize` 按 `PartialSkeleton` × `MorphTargets` 四路兜底；场景节点的 `MdfQueueType` 默认值由 `TtMdfStaticMesh` 改为 `null`（表示「本节点不指定」）。**改默认值连带修了一个既有 bug**：`TtMeshNode.HasSkin` 原先比对 NodeData 里的类型字符串，默认值变 null 后会误判无蒙皮 → 拿不到 `PerSkinMeshCBuffer`、`IsNoTick` 恒为 true → 骨骼动画整体失效；已改为以实际创建出的 `RenderMesh.MdfQueue` 为准。
  - **per-SubMesh 状态**：一个 MdfQueue 服务整个 `TtMaterialMesh`，而每个 SubMesh 的 `TtMeshPrimitives` 有各自独立的 `vVertexID` 空间，因此稠密表按 `atom.SubMesh.MeshIndex` 分开持有，不能混用。morph 顶点数与 mesh 顶点数不符时（重导入而 morph 未跟上）丢弃 morph 数据并告警，宁可不形变也不画错。
- **代码位置**
  - `CSharpCode/Grapics/Mesh/MorphTarget.cs`：`FMorphDelta` / `TtMorphTarget` / `TtMorphTargetSet`。
  - `CSharpCode/Grapics/Mesh/Modifier/MorphModifier.cs`：`TtMorphModifier`（权重接口 + 累加 + `OnDrawCall` 绑 SRV）。
  - `CSharpCode/Grapics/Mesh/MdfMorphMesh.cs`：`TtMdfMorphMesh` / `TtMdfSkinMorphMesh`。
  - `enginecontent/Shaders/Modifier/MorphModifier.cginc`：`DoMorphModifierVS`。
  - `CSharpCode/Bricks/AssetImpExp/PartialMeshPrimitives.cs`：blendshape 解析 + 导入期写 `.ums` 的 `MdfQueueType`。
  - `CSharpCode/Grapics/Mesh/Mesh.cs`（`TtRenderMesh.Initialize` 四路兜底 + 错配 Info 日志）、`GamePlay/Scene/MeshNode.cs` / `PBRTestNode.cs` / `PrimitiveMeshNode.cs`（默认值 null）。
  - `CSharpCode/Editor/Forms/MeshPrimitiveEditor.cs`：`MorphTargets` 面板（逐 morph 滑条 + Reset All）。面板显隐由**资产是否带 morph** 决定而非 modifier 是否存在，队列选错时显示原因与修复指引。
- **验收标准**
  - 一个带 blendshape 的 FBX 头部模型可导入并在编辑器中拖动权重实时插值。（**未验收 —— 缺素材**）
  - morph 与骨骼动画同时生效时无顶点撕裂、法线正确。（未验收）
  - 多个 morph 叠加（如口型 + 眉毛）结果符合 DCC 中的预期。（未验收）
- **遗留缺口**
  - **切线 delta 未做**：`FMorphDelta` 只有位置与法线两项，没有 `DeltaTangent`。因为蒙皮本身也还没蒙皮切线（见「已知技术债」首条），两者是同一条通路，应一并补齐。
  - **权重驱动只有编辑器滑条**：原计划的「动画曲线 / PoseAsset / 表情控制器驱动」全部未做。运行时目前只能由代码调 `TtMorphModifier.SetMorphWeight`。接动画曲线是做表情与口型（LipSync）的前提。
  - **已有资产的 `.ums` 里 `MdfQueueType` 仍为空**：导入期写入只对新导入生效，存量资产走自动兜底（行为正确但未显式授权），需重新导入或批量刷 `.ums`。

- [x] 已完成（资产 + 导入 + 运行时 + Shader + 编辑器预览）
- 完成于: 稀疏 `FMorphDelta` 存 `.vms`，导入走 Assimp `MeshAnimationAttachments`（未改 native），运行时 CPU 累加成逐顶点稠密表经 `StructuredBuffer` 上传、VS 端一次无分支索引叠加，`TtMdfQueue2<TtMorphModifier, TtSkinModifier>` 保证 morph 先于 skin。顺带建立了 MdfQueue 三级选型链（`CodingGuidelines.md` §7.7）并修掉 `TtMeshNode.HasSkin` 的类型字符串误判。Engine.Window / MainEditor 编译 0 error 0 warning；**切线 delta 与动画曲线驱动未做，且尚未实机验收（缺带 blendshape 的 FBX）**。

### 2. 音频系统

全引擎唯一"绝对零实现"的一级子系统。`TtAudio` / `AudioSystem` / `SoundWave` / `XAudio2` 等标识符命中数为 0，`BuildScript/dependencies.json` 未声明任何音频库。

- **现状**
  - `CSharpCode/Bricks/Animation/Notify/SoundAnimNotify.cs` 已经定义了 `Sound` 资源字段，但 `Trigger()` 内只有一行注释 `//Sound trigger` —— 上游调用点已就绪，下游完全不存在。
- **目标**
  - 后端接入（建议先做一个后端即可，不追求多后端抽象）。
  - 音频资产类型 + WAV / OGG 解码，支持流式播放长音频。
  - 混音总线层级（Master / BGM / SFX / UI / Voice），支持音量与静音控制。
  - 3D 空间化：距离衰减、方向、可选多普勒；监听者跟随相机。
  - 独立音频线程 + 无锁队列，避免阻塞主线程。
  - 播放生命周期事件回调。
- **关键改动点**
  - `NativeCode/Bricks/` 下新增音频模块（参照现有 Brick 的 `.vcxitems` 组织方式），`CSharpCode/Bricks/` 下新增对应托管层。
  - `BuildScript/dependencies.json` 增加依赖组声明。
  - 点亮 `SoundAnimNotify.Trigger()`。
- **选型建议**
  - 优先考虑轻量自持方案（如 miniaudio 单头文件），可控且无授权负担；商业授权可接受时再评估 FMOD / Wwise。避免一开始就抽象出多后端接口层，先跑通再抽象。
- **验收标准**
  - 动画 Notify 能触发音效播放。
  - 3D 音源随角色移动有正确的方位与衰减。
  - BGM 流式播放不占用大内存、切换无爆音。

- [ ] 未开始

### 3. 存档系统

`SaveGame` / `TtSaveSlot` / `PlayerSave` 命中数为 0。底层序列化设施完整，缺的是存档语义层。

- **现状**
  - 已有 `ISerializer` / Xnd / Json 序列化能力，可直接复用。
- **目标**
  - 存档槽位管理：多槽位、自动存档、槽位元信息（时间戳、缩略图、进度摘要）。
  - 对象图保存与恢复，支持场景状态、玩家状态、任务进度。
  - 存档版本号与迁移机制（老存档在字段增删后仍可读）。
  - 完整性校验，可选加密。
- **关键改动点**
  - `CSharpCode/Bricks/` 下新增存档模块；复用 `CSharpCode/Base/IO/` 现有序列化器。
  - 与宏图打通，暴露"保存 / 读取存档"节点。
- **验收标准**
  - 退出重进后玩家位置、背包、任务进度一致。
  - 人为增删一个存档字段后，旧存档仍能正常读取。

- [ ] 未开始

### 4. 动画 Gameplay 层：RootMotion 与 Montage

动作类游戏无法绕开的两项，运行时与编辑器均已落地。

- **实际方案**
  - **RootMotion 不走 Tick 阶段的权重传播**，而是作为 Pose 的附带数据在命令图中流动（等价于 UE5 `AnimRootMotionProvider` 的 attribute 方案）：`IRuntimePose.RootMotion` 随 `CopyPose` / `BlendPoses` 自动传递，并按与 Pose 完全相同的权重混合。这样做的原因是本引擎 BlendTree 的 Tick 不是严格递归树 —— `TtBlendTree_CrossfadePose.Tick` 只推进自己的混合权重、不 tick 子节点，子树由状态机的 StateAttachment 分散驱动，因此 Tick 阶段拿不到可靠的最终权重。
  - Clip 资产新增 `EnableRootMotion` / `RootMotionRootLock`(RefPose·AnimFirstFrame·Zero) / `ForceRootLock`；曲线采样、位移提取与根锁定收敛到 `TtClipPoseSampler` 一处，BlendTree Clip 节点、简单播放器、Montage 段落三条路径共用同一实现。
  - **Montage 为独立资产** `.animmontage`：Slot 轨道 + Section 跳转链（`NextSectionName`，自指即循环）+ 通知 + 进出混合与 `BlendOutTriggerTime`。`TtAnimMontageInstance` 按 Section 与段落边界子步进（保证跨段求位移与通知派发都不失真），`TtAnimMontageHost` 处理同 Slot 抢占淡出，`TtBlendTree_Slot` 按实例权重把 Montage Pose 叠在基础 Pose 之上。
  - 位移消费在 `TtCharacterMovement.UpdatePlacement`：root motion 位移与速度位移合并后统一交给 `TtPhyControllerNodeBase.TryMove`，碰撞阻挡对两者一致生效；`ERootMotionMode`(Ignore / FromEverything / FromMontagesOnly) 由 Movement 节点配置并每帧同步给动画节点。
  - 编辑器：新增 Montage 编辑器（时间轴管时间、PropertyGrid 管明细、预览直接驱动运行时实例，带 RootMotion 预览开关）；AnimationClip 编辑器补上 Notify 时间轴与 RootMotion 预览。
- **代码位置**
  - `CSharpCode/Bricks/Animation/RootMotion/`：类型与数学工具、根骨骼曲线提取器、`IRootMotionSource`、单测。
  - `CSharpCode/Bricks/Animation/Montage/`：`TtClipPoseSampler`、`TtAnimMontageInstance`、`TtAnimMontageHost`、单测。
  - `CSharpCode/Bricks/Animation/Asset/AnimMontage.cs`、`BlendTree/Node/BlendTree_Slot.cs`、`Player/AnimStateMachinePlayer.cs`（`Montage_Play/Stop/JumpToSection/SetNextSection`）。
  - `CSharpCode/Editor/Forms/AnimMontageEditor.cs`、`CSharpCode/ImGui/Controls/TimelineControl.cs`。
  - `CSharpCode/GamePlay/Movemnet/Movement.cs` / `CharacterMovement.cs`。
- **改动时必读**
  - 本引擎的乘法约定与 UE 相反（`q1 * q2` 表示先转 q1 再转 q2；`FTransform.Multiply(out, A, B)` 中 A 是局部、B 是父），且 `FTransform.GetRelativeTransform` 是从 UE 逐行搬来的、四元数序与位置序混用了两套约定，**不可用来求增量**。`TtRootMotionUtil.CalcDelta / ApplyDelta / Combine` 是唯一正确入口，改动后必须让 `UTest_RootMotion` 全绿。
- **验收标准**
  - 一段带位移的攻击动画播放后，角色最终落点与动画一致，且不穿墙。（待手工验收）
  - Montage 能在行走状态机之上叠加上半身挥手动作，混出后平滑回到底层状态。（待手工验收）
- **遗留缺口**
  - RootMotion 的网络一致性（同步与预测回滚）未做，随 P1-10 一并推进。
  - 未做：SyncGroup / Marker 时间同步、Inertialization 惯性混合、BlendProfile 逐骨骼权重、动画压缩（P2-16）、Montage Slot 的 Macross 图节点描述。

- [x] 已完成（运行时 + 编辑器 + 单测）
- 完成于: RootMotion 采用「随 Pose 流动」通路 + `TtClipPoseSampler` 统一采样，位移经 `TryMove` 走碰撞；Montage 为 `.animmontage` 资产 + `TtBlendTree_Slot` 叠加。变换约定已用独立数值验证（25 组增量往返、125 组分步复合、旋转系纯平移、循环回绕）全绿；两组 `[TtTest]` 单测已写但需在引擎内开 `Config.DoUnitTest` 才会执行，尚未实跑。

### 5. Gameplay 中间层：Tag / Timer / 属性技能系统

宏图视觉脚本缺少可站立的地基。以下标识符命中数均为 0：`GameplayTag`、`TtTagContainer`、`TtTimerManager`、`AbilitySystem`、`GameplayAttribute`。

- **目标**
  - **GameplayTag**：层级化标签（如 `State.Buff.Stun`），支持标签容器、查询、匹配；作为技能、AI、UI 的统一条件语言。
  - **Timer 管理器**：统一的延时 / 循环回调，与协程体系协同，支持暂停与序列化（配合存档）。
  - **属性与技能系统**：属性集（生命、法力、攻击力等）、属性修饰器（加算 / 乘算 / 覆盖）与叠加规则、Buff / DeBuff 生命周期、技能冷却与消耗、伤害管线。
- **关键改动点**
  - `CSharpCode/GamePlay/` 下新增模块，并为宏图导出节点（这一步不可省，否则"0 代码做游戏"的卖点无法落到实处）。
- **优先级说明**
  - 三者中 Tag 应最先做，因为技能系统与 AI 感知都会以它为基础；属性技能系统体量最大，可拆为多个迭代。
- **验收标准**
  - 用宏图不写 C# 代码即可配出"命中造成伤害、施加 3 秒减速 Buff、Buff 期间免疫二次减速"的完整链路。

- [ ] 未开始

---

## P1 品质上限与项目规模

### 6. 静态光照路径：Lightmap 烘焙与反射探针

- **现状**：`Lightmap` 相关命中全部是顶点流 `VST_LightMap` 与 FBX 的 UV 通道，**没有任何烘焙器**；`ReflectionProbe` / `ReflectionCapture` 命中数为 0。GI 只有实时一条路。
- **目标**：Lightmap 烘焙管线（UV 展开、光照烘焙、Lightmap 图集）；反射探针捕获与运行时混合。已有 Embree 依赖可用于离线光线求交。
- **意义**：这是移动端与低端机唯一的退路。目前 Mobile 管线存在，但没有静态光照可回落。
- [ ] 未开始

### 7. Decal 贴花系统

延迟贴花已落地：投影盒 + DBuffer + 在 DirLighting 里应用。

- **实际方案**
  - **不直接改写 GBuffer，而是走 DBuffer**（等价于 UE 的 DBufferA/B/C）：贴花先投影进三张
    `R8G8B8A8_UNORM` 中间缓冲，再由 `DeferredDirLighting.cginc` 在 `DecodeGBuffer` 之后覆盖到
    GBuffer 的语义字段上。这么做有两个硬原因，不是实现妥协：
    - GBuffer 的法线是**八面体编码**（`rt1.rg`，见 `DeferredCommon.cginc`），对编码值做混合在跨八面体时
      是错的；DBuffer 存线性编码法线（`*0.5+0.5`），混合完全正确。
    - GBuffer 的 RT1/RT3 是 `R10G10B10A2_UNORM`，该格式的 typed UAV load 在 D3D11 与部分 Vulkan
      实现上不保证可用；RGBA8 到处都能当 storage image。
  - **逐贴花一次 compute dispatch**，只覆盖该贴花投影盒的**屏幕空间外接矩形**（8 个角投屏求
    min/max，有角点在相机后面则退化为全屏）。选这个粒度是因为每个贴花有自己的贴图，一次
    dispatch 内按贴花下标索引任意纹理需要 bindless（仅 DX12，且依赖 `CodingGuidelines §1.4.8`
    Phase B 的场景级 bindless 表，尚未落地）。顺序执行同时天然保证了 `SortOrder` 的混合顺序。
  - **三通道独立权重**：颜色 / 法线 / (Roughness·Metallic·Specular) 各有自己的混合权重与覆盖率
    （DBuffer 的 alpha 就是覆盖率，lighting 端 `lerp(sceneValue, dbuffer.rgb, dbuffer.a)`）。
  - **剔除与淡出**：投影盒剔除（局域 `[-0.5,0.5]^3` 之外丢弃）、法线夹角淡出（避免侧壁拉伸）、
    盒体边缘淡出、距离淡出（CPU 端算系数）。
  - Hair ShadingMode 不接受贴花法线（它的 `rt1` 存的是切线，混进去会破坏各向异性高光的切线基）。
- **代码位置**
  - `CSharpCode/GamePlay/Scene/DecalNode.cs`：`TtDecalNode` + `TtDecalNodeData`（投影盒由
    `Placement.Scale` 决定边长，投影方向 = 局域 +Z，UV 取局域 XY；贴图异步加载）。
  - `CSharpCode/Grapics/Pipeline/Deferred/DecalPassNode.cs`：`TtDecalPassNode` + 两个 ShadingEnv
    （清空 pass / 投影 pass），逐贴花 drawcall 与 cbuffer 数组（遵守 §1.3：同帧多 dispatch 不共享 CBV）。
  - `enginecontent/Shaders/Compute/ScreenSpace/DecalProject.compute`：`CS_DecalClear` / `CS_DecalProject`。
  - `enginecontent/Shaders/ShadingEnv/Deferred/DecalCommon.cginc`：`ApplyDBufferToGBuffer`。
  - `DeferredDirLightingNode.cs` / `DeferredDirLighting.cginc`：`ENV_ENABLE_DECAL` permutation +
    `DBufferA/B/C` 输入 pin（三张必须全连，只连部分会打警告并禁用）。
  - `TtRenderPolicy.EnableDecal` 总开关；已接入 `enginecontent/graphics/deferred_simple.rpolicy`。
- **验收标准**
  - 场景里放一个 Decal 节点、给一张带 alpha 的颜色贴图，贴花贴合地面/墙面且不溢出投影盒。（待手工验收）
  - 贴花法线贴图能扰动受影响像素的光照，法线为 (0,0,1) 时与无贴花完全一致。（待手工验收）
  - 多个贴花重叠时按 `SortOrder` 叠加；关 `policy.EnableDecal` 或关节点 `Enable` 都能干净地回到无贴花状态。（待手工验收）
- **遗留缺口**
  - **前向贴花未做**（半透明物体上的贴花）。需要材质侧支持，不在 DBuffer 通路内。
  - 贴花只支持固定的三张贴图（color / normal / RMS），**没有接材质图**。要做程序化贴花需要给
    MaterialGraph 增加一条 decal 输出路径。
  - 逐贴花 dispatch 的扩展性上限：默认 `MaxDecalsPerFrame = 64`。合并成单次 dispatch 需要
    `CodingGuidelines §1.4.9` 里规划的 bindless 场景表。
  - SSAO / ReSTIR / SSR 读的是**未叠加贴花**的 GBuffer 法线与粗糙度（DBuffer 只在 DirLighting 里应用）。
    与 UE 的同类近似一致，视觉上可接受；要修需要把 DBuffer 应用推广到这些 pass。
  - 相邻贴花 dispatch 之间是对同一份 DBuffer 的读-改-写，依赖引擎的资源自动转换；重叠区域的
    混合顺序在极端情况下未做过 RenderDoc 实证。

- [x] 已完成（延迟贴花运行时 + 场景节点 + 默认 policy 接线）
- 完成于: 采用 DBuffer 通路（三张 RGBA8）而非直改 GBuffer，逐贴花一次 compute dispatch 覆盖投影盒的
  屏幕外接矩形，`ENV_ENABLE_DECAL` 在 `DeferredDirLighting` 的 `DecodeGBuffer` 之后应用。C# 两个工程
  （Engine.Window / MainEditor）编译通过，`deferred_simple.rpolicy` 已接线并通过 XML + 图结构校验；
  **尚未实机跑图验收**。

#### 7.1 进行中的重构：去掉 DBuffer，改走 graphics pass + 逐 RT 写掩码

上面的 DBuffer 版能跑，但对**低带宽 / 低显存（移动端 deferred）性价比太低**：1080p 下 3 张全屏
 RGBA8 ≈ 25 MB，加上每帧一次全屏 clear + 全屏写 + lighting 里 3 次采样的带宽。已拍定重构。

**关键认识（重构的依据）**

- DBuffer 存在的本质是两个需求的交点：**「oct 编码法线」×「想要软边」**。去掉任何一个就不需要它。
- 而且两个堵点都**只在 rt1 上**：rt0 / rt2 是 RGBA8，硬件混合精确正确，typed UAV load 也到处可用
  （DX12 additional list 里有，Vulkan 是 mandatory storage image）。
- **法线也不需要硬件混合**：把淡出因子烤进切线空间的法线值本身
  （`ts = normalize(lerp(float3(0,0,1), tsRaw, alpha))`）与世界空间的精确混合**数学上等价**
  （TBN 是线性正交变换，且 `(0,0,1)` 正好映射到基准法线）。于是 rt1 上可以直接**关掉硬件混合**，
  软过渡仍在，oct 问题也被抹平（混合发生在编码之前）。
- RHI 通路**已确认完整，不需要改 RHI**：`FBlendDesc.IndependentBlendEnable` +
  `RenderTarget[0..3]`（逐 RT 的 `BlendEnable` / `RenderTargetWriteMask`）→
  `PipelineManager.GetPipelineState` → `drawcall.BindPipeline`（`NxRHI/Drawcall.cs:73`）。
  “在 OnDrawCall 里覆盖 PSO”的既有先例：`Shadow/ShadowMapNode.cs:263` + `:39`，
  `Bricks/AdvanceShadow/AdvanceShadowShading.cs:391` + `:44`。
- 每个贴花一个 box mesh 实例 → 自己的 atom → 自己的 drawcall，`OnDrawCall` 里用
  `atom.RenderMesh.HostNode as TtDecalNode` 回溯取参数。因此逐贴花 cbuffer 天然避开了
  `CodingGuidelines §1.3`（同帧多 drawcall 不能共享 CBV）。
- 对照过 UE（`D:\Engine\TechEngine`）：UE 同时有 DBuffer 和直写 GBuffer 两条贴花路（`DBuffer` /
  `SceneColorAndGBuffer` / `SceneColorAndGBufferNoNormal` 等 5 种 `EDecalRenderTargetMode`），
  后者就是靠 `DecalRenderingCommon.cpp:325` 的逐 RT 写掩码。UE 的 GBuffer 法线是**线性编码**
  （`DeferredShadingCommon.ush:137` `EncodeNormal(N) = N*0.5+0.5`，oct 那行被注释掉了），所以它能直接
  硬件混合；UE 的 `PF_A2B10G10R10` 在 Lumen / Tonemap 中间纹理上，**GBuffer 一个都没有**，而且那些
  UAV 用法全是 write-only。**UE 从来不用 compute 去读-改-写 GBuffer。**

**目标形态**

逐贴花一个 graphics drawcall，光栅化投影盒，按 `EDecalMode` 绑不同的 RT 与 PSO：

| `EDecalMode` | 绑的 RT | rt 混合 | 额外显存 |
| --- | --- | --- | --- |
| `ColorAndMaterial`（缺省） | 2-RT：`(rt0, rt2)` | 两张都开 SrcAlpha/InvSrcAlpha，写掩码 = RGB | **0** |
| `WithNormal` | 3-RT：`(rt0, rt1, rt2)` | rt0/rt2 开；rt1 **关**，淡出烤进切线空间 | **0** |

写掩码一律只开 `RGB`，保住 `rt0.a`（SpecOcclusion / SSSProfile / ShiftOffset）、`rt1.a`（Mask）、
`rt2.a`（AO）；rt3（motion vector / RenderFlags）完全不绑。

**已拍定的默认假设**

1. 投影盒**只画背面**（`CullMode = Front`）+ **完全不做深度测试**，纯靠盒体数学 + `discard` 定覆盖。
   遗留：**相机进入盒子内部时背面会被近裁面裁掉，贴花消失**（UE 对此有正/背面切换逻辑）。
   贴花盒通常很扁，暂时接受。
2. 复用现有 **4-RT GBuffer**（`TtDeferredBasePassNode`），不弄精简 GBuffer 变体。
   遗留：将来移动端若用精简 GBuffer（RT 数量或通道分配不同），贴花的 RT 绑定与写掩码要跟着变，
   可能需要再加一档 `EDecalMode`。

**贴花法线图的作者约定（必须遵守）**

贴花法线图的**边缘必须是平的** `(128,128,255)`。这样边界处 `ts = (0,0,1)`，解码出来就是基准
法线本身，写进去等于没写，**不会出现硬接缝**。边缘不平的法线图无论怎么混合都会露。

**重构进度（已全部完成）**

- [x] A. 场景侧：`EDecalMode` 枚举 + `TtDecalNodeData.Mode`（缺省 `ColorAndMaterial`）+
      `TtDecalNode.ProjectionMesh`（单位盒 mesh，**不进可见 mesh 列表**，否则 BasePass 会把它
      当不透明物体画成实心盒子）+ 逐贴花 `cbDecal`。
- [x] B. `TtDecalPassNode` 已改成 graphics：两套 render pass（2-RT / 3-RT，`LoadActionLoad`）+
      两套 PSO（`IndependentBlendEnable` + 逐 RT `BlendEnable` / `RenderTargetWriteMask = 0x07`），
      pin 改成 `CreateInputOutput(BFT_RTV | BFT_SRV)`。PSO 在 `ShadingEnv.OnDrawCall` 里用
      `drawcall.BindPipeline` 覆盖。
- [x] C. `enginecontent/Shaders/ShadingEnv/Decal/DecalPS.cginc`，permutation `ENV_DECAL_WRITE_NORMAL`。
      两个实现上的简化：世界→盒局域直接用 `WorldMatrixInverse`，贴花的 T/B/ProjDir 直接从
      `WorldMatrix` 取，两者都不需要额外传 cbuffer。
- [x] D. DirLighting 已回退：`ENV_ENABLE_DECAL`、`DBufferA/B/C` pin、`DecalCommon.cginc`、
      `ApplyDBufferToGBuffer`、`DecalPassNode` 属性与 `IsDecalConnected` 全部删除；
      旧的 `Compute/ScreenSpace/DecalProject.compute` 已删。
- [x] E. `deferred_simple.rpolicy` 已重接：
      `BasePass.MRT0/1/2/3 → Decal.MRT0/1/2/3`，`Decal.MRT0/1/2 → DirLighting.MRT0/1/2`。
      利用 InputOutput 的别名语义（`UpdateNodeTree` 会把上游的 `AttachmentName` 拷给 InputOutput pin），
      `Decal.MRT0` 与 `BasePass.MRT0` 指向**同一张物理纹理** —— 重接只是为了确立执行顺序，
      所以其他读 BasePass MRT 的节点（ReSTIR / SSAO / SSR / SSSBlur 等）无需改动。

**重构后的收益（早期 DBuffer 版的遗留缺口已消失）**

- 额外显存 25 MB → **0**；每帧全屏 clear + 全屏写 + lighting 三次采样的带宽 → **0**。
- 逐贴花 dispatch 上限（`MaxDecalsPerFrame`）不再需要，已删。
- 贴花直接进 GBuffer，因此 **SSAO / ReSTIR / SSR 现在看得到贴花**。
- 节点 `Enable=false` 时是干净的**直通**（InputOutput pin 本身就是上游那张图），不会像 DBuffer
  方案那样留未初始化的池化缓存。

**验证状态**

C# 两个工程（Engine.Window / MainEditor）编译 0 error；`deferred_simple.rpolicy` 通过 XML 解析 +
图结构反向 dump 校验（55 节点 / 112 连线，无 DBuffer 残留）。
**HLSL 未经引擎编译器验证（shader 只在运行时编译），且尚未实机跑图验收。**
首次实机验收时建议重点看：① 盒体内外的覆盖边界是否正确；② `RenderTargetWriteMask = 0x07` 是否
真的保住了 rt0.a / rt2.a（如果 AO 或 SpecOcclusion 被误写，会表现为贴花区域变暗）；
③ `WithNormal` 模式下几何法线的朝向（`ddx/ddy` + 朝相机翻转）是否正确。

#### 7.2 材质驱动重构：贴花视觉由材质定义（对齐 UE 的 MaterialDomain=Deferred Decal）

§7.1 落地后贴花节点上还挂着三张散装贴图（Color/Normal/Material Texture）+ 颜色 / RMS 散参数。
本轮把贴花视觉全部迁到**一张 RL_Decal 材质**上，节点只保留投影几何、混合权重与淡出参数 ——
对应 UE 的 MaterialDomain=Deferred Decal + DecalBlendMode。当初没这么做是历史原因：最初是
compute 实现（DBuffer 时代），材质图入口 `DO_PS_MATERIAL(input, mtl)` 依赖 PS_INPUT 插值量，
compute 里拿不到；改成 graphics pass 后路径已打开，这轮补上。

**改动清单**

- `RenderLayer.cs`：新增 `ERenderLayer.RL_Decal`（枚举尾部、`RL_Num` 前 —— sbyte 顺序敏感，
  `TtLayerDrawBuffers.PassBuffers[]` 按它分配）。它是贴花材质的域标记：不参与任何
  RenderLayer 分桶 pass（贴花由 `TtDecalPassNode` 自己收集渲染），同时兼防错 —— 加载时校验
  非 RL_Decal 材质则告警拒渲染。
- `Material.cs` / `MaterialInstance.cs`：`EDecalMode`（`ColorAndMaterial` 缺省 / `WithNormal`）
  + `DecalMode` 属性（Rtti.Meta 序列化，材质编辑器 PropertyGrid 直接可改）。
  `UpdateShaderCode()` 对 RL_Decal 材质注入 `#define ENV_DECAL_WRITE_NORMAL`；材质没有法线手段
  （NormalNone）时自动降级为 0，避免写出全零法线。
- `DecalNode.cs`：`TtDecalNodeData` 删 9 个字段（三张贴图 + ColorTint/Opacity/Roughness/
  Metallic/Specular/Mode），加 `DecalMaterial`（RName）。加载状态机 `EnsureMaterial()`
  （非 RL_Decal 材质告警拒渲染；加载期间 RName 被换则丢弃结果下一帧重载）。
  `cbDecal` 缩为 8 字段（三组权重 + 全局强度 + 三个淡出参数）。
  `ProjectionMesh` 直接用材质实例（未就绪返回 null 不渲染）。
- `DecalPassNode.cs`：`TtDecalWithNormalShading` 子类删除 —— **不用 ShadingEnv permutation**：
  effect hash 含 `material.AssetName` 且 `MaterialHash` 变化自动 Refresh，材质注入的
  `ENV_DECAL_WRITE_NORMAL` 天然把两种 DecalMode 分裂成两个 effect，单个 ShadingEnv 服务全部贴花。
- `DecalPS.cginc`：删三张贴图声明与 `DECAL_FLAG_*`；`#include "Material"` 接材质图。
  PS 里把 `PS_INPUT` 的插值量覆盖成"被投影表面"语境（`Set_vUV` = 投影 UV、`Set_vWorldPos` =
  表面坐标、`Set_vNormal/Set_vTangent` = 贴花切线基），之后走标准三行
  `Default_PSInput2Material + DO_PS_MATERIAL` —— 法线图解包（CalcNormalMap 的 TBN）、世界坐标
  节点、过程纹理等一切材质图手段直接可用。B 基由 CalcNormalMap 用 `cross(T, N)` 现算
  （`vTangent.w = 0` 走正号分支），方向恰为贴花的 V 方向。
- 贴花贴图与 `cbPerMaterial` 不再手动绑定：由 `Mesh.TtAtom.BuildDrawCall` 的材质绑定机制
  （`Material.UsedSrView` / `UsedSamplerStates`）自动挂上。

**统一 3-RT（取代 §7.1 的 2-RT / 3-RT 双 render pass）**

两种 DecalMode 共用同一个 3-RT render pass（rt0/rt1/rt2 → slot 0/1/2），仅 PSO 不同：

| DecalMode | slot1 (rt1 法线) | slot0 / slot2 |
| --- | --- | --- |
| ColorAndMaterial（缺省） | 写掩码 0（完全不写） | SrcAlpha/InvSrcAlpha |
| WithNormal | 开写但关混合，淡出烤进法线值 | SrcAlpha/InvSrcAlpha |

- 统一后 rt1 恒为 RTV 不能再当 SRV 读 → 角度淡出的基准法线统一改为**几何法线**（`ddx/ddy`
  重建 + 朝相机翻转）。对角度淡出反而更合适：只反映表面朝向，不被 BasePass 已写进 rt1 的
  着色法线扰动影响。
- 法线淡出从切线空间改到世界空间：材质图直接输出世界法线（`mtl.GetWorldNormal(input)`），
  `lerp(baseNormal, decalNormalWS, w)`，w→0 时写回基准法线等于没写（与 §7.1 的切线空间版本
  数学等价，但更直接）。NormalNone 的零向量有防御（退化成 baseNormal，防 `normalize(0)` 出 NaN）。
- 输出通道语义与 DeferredBasePassPS 对齐：`rt0.rgb = mAlbedo + mEmissive`、
  `rt2.rgb = (mMetallic, mAbsSpecular, mRough)`（Deferred 端 mRough 无 1-x 换算）、
  `alpha = mtl.mAlpha(材质图 Opacity) × GlobalIntensity × 角度/边缘/距离淡出`。
- 法线模式由逐 atom 的材质决定（`OnDrawCall` 里 `atom.Material.DecalMode` 选 PSO），因此
  同一场景可混用两种模式的贴花。

**验收指引与遗留**

- `.material` 是二进制 Xnd，**默认贴花材质需在编辑器里创建**（RenderLayer 选 RL_Decal，
  NormalMode 按需选 NormalMap），`DecalMaterial` 为空时贴花不渲染。
- 旧节点数据里的散装贴图字段已删，旧场景的 Decal 节点需要重新指认 DecalMaterial。
- §7.1 的法线图"边缘必须平 (128,128,255)"约定在材质图路径下依然成立。
- 前向贴花（半透明物体上的贴花）仍未做。
- 构建流程备忘：引擎属性集变化后需先重编 `Module/CSharpCodeTools` 再跑
  `mode=DataCopyer`，否则旧 `Copyer.gen.cs` 引用已删属性会让 DataCopyer.All 工程编译失败，
  且它是 Engine.Window 的 AfterTargets 生成链，失败会卡住整体 build。

**验证状态**：C# 三个工程（CSharpCodeTools / Engine.Window / MainEditor）+ DataCopyer.All
全部编译 0 error；HLSL 未经引擎编译器验证（shader 只在运行时编译），尚未实机跑图验收。

#### 7.3 编辑器拾取：billboard 图标承载 HitProxy

贴花在编辑器里点不中。原因有两层：

1. **没有可拾取的实体**。HitProxy pass（`TtHitproxyNode`）是遍历 `VisibleMeshes` 里
   `IsDrawHitproxy == true` 的 mesh 来画的，而贴花的投影盒 (`ProjectionMesh`) 刻意不进可见
   mesh 列表（否则 BasePass 会把它当不透明物体画成实心盒），只有线框盒 (`DebugMesh`) 在
   `LightDebug` filter 下可见，1px 宽的边鼠标几乎不可能点中。
2. **hitproxy id 根本没设上**。`OnHitProxyChanged` 挂在懒创建的 `mDebugMesh` 上，而 id 分配
   （`HitproxyType` setter → `MapProxy` → 回调 `OnHitProxyChanged`）发生在节点入世界时，那时
   `mDebugMesh` 还是 null，直接早退，之后也没有补设的时机。

照灯光 / Volume 的既有惯例修：新增专用的 `mHitproxyMesh` —— 0.25m 的 `MakeRect2D`，材质复用
`material/utility/volume.uminst`（贴花与 Volume 同属"盒体范围型"辅助对象，需要专属图标时只换
RName），在 `OnPostInitNode` 里 `await InitHitproxyMesh()` 建好 mesh **之后**才设
`HitproxyType = Root`，保证 setter 的同步回调能拿到 mesh 写入 id。

- 只在 `UtilityEditor` filter 下进可见列表，游戏内与关闭该 filter 时不干扰拾取。
- 图标 billboard 朝向相机（`GetYawFaceToCamera`）且**不跟随节点 Scale** —— 贴花常被压成薄片
  （Scale.z 很小），跟随缩放会让图标一起被压扁到点不中。
- `GetHitProxyDrawMesh`（选中描边用，非 hitproxy pass）也改指向图标 mesh；线框盒保持
  `IsDrawHitproxy = false`。

**验证状态**：Engine.Window / MainEditor 编译 0 error，尚未实机点选验收。

### 8. 屏幕空间反射 SSR

两 pass compute SSR 已落地，复用了 Contact Shadow 那套 HZB 步进设施。

- **实际方案**
  - **Pass 1 Trace**：每像素投一条反射线（`reflect(-V, N)`），在屏幕空间步进求交，命中后采样
    SceneColor。输出 `rgb = 命中点颜色, a = 置信度`。
    - HZB 路径直接复用 `enginecontent/Shaders/Inc/HzbRayCast.cginc` 的 `HzbRayCastFromWorldRay`
      （与 Contact Shadow 同一份实现，`roughness` 参数控制每步的 mip 抬升速度）；HZB pin 悬空时
      permutation 自动退回线性等步长 fallback。
    - 粗糙表面按 `roughness²` 扰动反射方向（每像素一条线），噪声交给 Resolve pass 的空间复用消化。
      这不是严格的 GGX 重要性采样，但成本恒定，是半分辨率 SSR 的常见做法。
    - 拒绝条件：天空、`roughness > MaxRoughness`、背面命中、命中点是天空、屏幕外。
  - **Pass 2 Resolve**：按深度/法线/置信度加权的邻域滤波（半径随粗糙度增大），再乘上解析型
    环境 BRDF（`EnvBRDFMobile` 同构式，`F0 = lerp(Specular, Albedo, Metallic)` 与
    `DeferredDirLighting.cginc` 的 `OptSpecShading` 一致），输出**可以直接相加**的能量。
  - 因此合成只需一个普通 `TtAdditiveNode`，不需要专门的 SSR 合成节点；默认半分辨率，由 Additive
    的 LinearClamp 采样自动升采样。
  - **与其他反射通路的关系（原条目要求明确这一点）**：SSR 是**非光追设备上的主力反射**，覆盖屏幕内
    可见的清晰反射；ReSTIR GI 负责的是间接漫反射，两者不冲突可同时开；屏幕外 / 被遮挡的反射 SSR
    取不到（置信度为 0 就是 0，**不做任何猜测补全**），由 IBL（`gEnvMap`）兜底，需要完整反射时走 DXR。
- **代码位置**
  - `CSharpCode/Grapics/Pipeline/Common/Post/SSRNode.cs`：`TtSSRNode` + trace / resolve 两个 ShadingEnv。
  - `enginecontent/Shaders/Compute/ScreenSpace/SSR.compute`（`CS_SSRTrace`，`ENV_SSR_USE_HZB` permutation）。
  - `enginecontent/Shaders/Compute/ScreenSpace/SSRResolve.compute`（`CS_SSRResolve`）。
  - `TtRenderPolicy.EnableSSR` 总开关；已接入 `deferred_simple.rpolicy`（新增一个 `SSRAdditiveNode`
    插在 GI 合成 Additive 与 `ForwordNode.Color` 之间）。
- **验收标准**
  - 光滑地面/金属面上能看到屏幕内物体的反射，随相机移动稳定跟随。（待手工验收）
  - 反射在屏幕边缘平滑淡出而不是硬切。（待手工验收）
  - `MaxRoughness` 附近无硬边（Resolve 里在最后 25% 区间做线性淡出）。（待手工验收）
- **遗留缺口**
  - **没有时域累积**。Resolve 只做空间复用，粗糙面在运动时仍可能有可见噪声。已有的 `TtDenoiseNode`
    可以插在 `SSRNode.Reflection` 与 Additive 之间做时域降噪（它需要 Color/Normal/Depth/MotionVector
    四个输入，都是现成的），但**默认 policy 里没有接**，需要按项目噪声情况自行接。
  - SceneColor 取的是 `DirLighting.Result`，因此反射里不含前向渲染物体与半透明。
  - 读的是未叠加贴花的 GBuffer（见 P1-7 的同款遗留）。

- [x] 已完成（trace + resolve 两 pass + 默认 policy 接线）
- 完成于: 复用 `HzbRayCast.cginc` 做 HZB 加速步进（无 HZB 时自动退线性 fallback），Resolve pass 做
  粗糙度感知的空间复用并施加解析环境 BRDF，输出经一个新增的 `SSRAdditiveNode` 合成。C# 两个工程编译
  通过，policy 接线通过校验；**尚未实机跑图验收**，时域降噪未接。

### 9. 通用场景流送与 HLOD

- **现状**：`LevelStreaming` 只存在于 `CSharpCode/Bricks/Terrain/CDLOD/LevelStreaming.cs`，是地形专用的；`HLOD` / `Impostor` 的命中全部是 `CurrentPatchLOD`、`MorphLODs` 之类的子串误报。
- **目标**：通用对象级 / 分区级流送（地形之上的物件也能流送）；HLOD 合并代理与 Impostor 远景替代。
- **意义**："双精度坐标 + 无限世界"目前只对地形成立，地形上的物件没有流送方案，这是宣传与实现的实际落差之一。
- [ ] 未开始

### 10. 网络：兴趣管理、状态同步、预测回滚

- **现状**：RPC + TCP + 属性同步已可用；`Relevancy` / 兴趣管理、状态快照同步、客户端预测与回滚**仅存在于设计文档**，无运行时实现。
- **目标**
  - 兴趣管理（按距离 / 分区裁剪同步范围）——没有它，"大型 MMO"在带宽上不成立，应最先做。
  - 状态快照同步与差异压缩。
  - 客户端预测与回滚——动作类联机的前提，也是 RootMotion（P0-4 已完成本地部分）能否用于联机的前提：位移由动画驱动后，服务端验证与客户端重放必须能复现相同的位移序列。
- **同步动作**：在实现到位之前，建议调整 `README.md` 中"自带超大型服务器集群构架，大型 MMO 项目可用"的措辞，避免宣称超出实现。
- [ ] 未开始

### 11. 动画进阶：重定向、FootIK、Ragdoll

- **现状**：`Retarget` 命中数 0；IK 只有 `TwoBoneIK`；`Ragdoll` 仅在 `TtPhysicsAsset.cs` 第 236 行的注释里被提到。
- **目标**
  - 动画重定向：不同骨架间复用动画（否则每个角色都要重做一套动画，是内容成本的大头）。
  - FootIK / 全身 IK：基于已有 `TwoBoneIK` 扩展，解决踩地与斜坡贴合。
  - Ragdoll：`TtPhysicsAsset` 已有 BoneBody 与约束概念，补齐动画到物理的接管与混合。
- [ ] 未开始

---

## P2 表现力与制作效率

### 12. Sequencer 过场序列系统

对标 UE 的 `LevelSequence` + Sequencer 编辑器：一份可保存的 `.sequence` 资产 + 多轨道时间轴编辑器 + 运行时播放器。

- **执行计划**：[design/Sequencer.Plan.md](design/Sequencer.Plan.md) —— 四层数据模型（`Binding → Track → Section → Channel`）、`Int64` tick 双帧率时基、十条硬约束、UE 对标九条结论、可复用设施清单、四阶段任务表与逐阶段验收标准全在那份文档里维护。**本条只维护状态，细节不要往这里搬。**
- **现状**：阶段 1 代码已全部落地（brick `CSharpCode/Bricks/Sequencer` + `Editor/Forms/SequenceEditor.cs`，`Engine.Window` 与 `MainEditor` 全量重编 0 错误），**未经引擎实跑验收**。前置清理两件已做完：① 从未被调用过的反射式属性 setter 注册表（`TtPropertySetterModule` 一整套）已删除，`Animatable.cs` 309 → 160 行；② 废弃前身 `TtSceneAnimationPlayer` 连同 `Animation.projitems` 的引用行已删除。
- **阶段状态**（每阶段自身可验收，判据见计划文档 §5；阶段 1 的实施偏离见 §8）
  - [x] **阶段 1｜骨架 + Transform 轨**：资产四层结构、`TtSequencerModule` 驱动、绑定解析（父 Guid + 相对路径）、播放头 scrub、**原值快照与恢复**、Undo/Redo —— 代码完成，**待按计划文档 §5 的 4 条验收标准实跑（含 `UTest_Sequencer` 的 6 组断言）**
  - [ ] **阶段 2｜镜头 + 动画 + 事件**：相机切换轨、骨骼动画轨、事件轨；顺带修好 `TtGamePlayCamera` 的 tick 与 `NodeId`
  - [ ] **阶段 3｜编辑体验**：时间轴扩能（缩放/滚动/多选）、曲线与切线编辑、通用属性轨 —— 其中 3.4 通用属性轨的**运行时与资产层已提前完成**（值适配器体系 + `RName` 资产轨 + 反射访问器，见计划文档 §9），**编辑器 UI 未做，界面上还用不起来**
  - [ ] **阶段 4｜输出**：PIE 与游戏内播放两条路、PNG 图像序列导出
  - [ ] **音轨**：随音频系统（P0-2）落地后补，不阻塞前四阶段
- **依赖**：音频系统（P0-2）应先落地，否则序列器缺音轨；镜头语言的表现力依赖 P2-13。
- [~] 进行中（阶段 1 代码完成待实跑验收，阶段 2-4 未开始）

### 13. 电影级后处理：DOF 与 MotionBlur

- **现状**：`DepthOfField` / `MotionBlur` 命中数均为 0。
- **目标**：景深（近景 / 远景散焦、光圈形状）；运动模糊（复用已有的 velocity 输出）。
- **依赖**：与 Sequencer 一起做收益最大，否则没有镜头语言的使用场景。
- [ ] 未开始

### 14. 天空与大气：大气散射、体积云、体积雾

- **现状**：`SkyAtmosphere` / `VolumetricCloud` 命中数为 0，现有只有指数高度雾与 SunShaft。
- **目标**：物理大气散射模型、体积云、真正的体积光照散射雾（区别于当前的屏幕空间高度雾）。
- **意义**：对主打无限世界的引擎，天空与大气是门面级表现。
- [ ] 未开始

### 15. 本地化系统

- **现状**：`LocalizationManager.cs` 只实现了中英文字符分类与分词，没有字符串表、没有运行时语言切换；`NativeCode/Bricks/Localization` 只有项目文件无实现。
- **目标**：字符串表资产（支持 CSV / JSON 导入）、运行时语言切换、多语言字体与回退链、日期与数字格式化。代码注释中已提到计划引入 ICU 与 harfbuzz，可一并评估。
- [ ] 未开始

### 16. 动画压缩

- **现状**：无压缩实现，动画数据全量存储。
- **目标**：关键帧抽取 + 曲线拟合 + 量化，带误差阈值控制；影响包体与内存。
- [ ] 未开始

### 17. AI 感知（Perception）

- **现状**：`Perception` 命中数为 0。BehaviorTree 与 Recast 导航已具备，缺的是"AI 如何获知世界"这一层。
- **目标**：视线检测（含视锥与遮挡）、听觉事件、目标记忆与遗忘、感知信息接入行为树。
- **依赖**：GameplayTag（P0-5）落地后做，阵营与目标筛选可直接用标签表达。
- [ ] 未开始

### 18. 顶点动画纹理 VAT（Vertex Animation Texture）

把离线模拟结果（布料、软体、破碎、流体、群集）烘成纹理，运行时在 VS 里重放。

- **先正一个常见认知偏差**
  - **UE 引擎本身没有内建的 VAT 子系统**。人们所说的「UE 的 VAT」是一套**约定**：Houdini 的 SideFX Labs「Vertex Animation Textures」ROP 导出位置/法线纹理，再在 UE 侧用一组材质函数通过 World Position Offset 重放。引擎只提供了 WPO 这个通用能力。
  - UE 真正**内建**的同类能力是 Alembic 导入（`AbcImporter`）：要么导成 GeometryCache（逐帧顶点流，运行时流式回放），要么做 PCA 分解后导成 SkeletalMesh + 一组 MorphTarget 基 + 逐帧权重曲线。
  - 因此本条目的定位是：**不是「补齐 UE 有而我们没的子系统」，而是用极低成本把已成为行业事实标准的 Houdini VAT 工作流接进来**。价值不在技术难度，而在打通一条现成的美术管线。
- **与已落地的 Morph Target（P0-1）是什么关系**
  - 两者在引擎里走**两条不同的顶点位移通路**：morph 走 `IMeshModifier` + MdfQueue（C# 硬编码，逐 SubMesh 绑资源），VAT 走**材质图的 VertexOffset 引脚**（美术可视化接线）。适用场景互补，不是替代关系：
  - morph 适合**少量有语义的形变基**（张嘴、眨眼）—— 可任意加权组合，数据稀疏，权重需要被动画或逻辑精确驱动。
  - VAT 适合**高帧数、无语义的模拟序列**（300 帧布料飘动）—— 用 morph 表达就是 300 个全顶点目标，稀疏存储完全失效，而纹理重放只需一次采样。
  - 成本上 VAT 比 morph **更便宜**：无需稀疏索引、无权重累加、CPU 每帧只更新一个时间标量（morph 还要重算稠密表并上传）。
- **现状：引擎侧基础设施已 100% 就位，不需要任何框架改动**
  - `VAT` / `VertexAnim` / `VertexAnimationTexture` 全仓命中数为 0，资产与运行时都不存在。
  - 但**「材质图驱动顶点位移」这条通路是现成的，而且已有生产用例**：
    - `MTL_OUTPUT` 有 `mVertexOffset` 字段（`CSharpCode/Grapics/Pipeline/Shader/ShaderPredefineType.cs`，带 `TtShaderDefine(ShaderName = "mVertexOffset")`），`TtMaterialOutput` 靠反射这些 attribute 自动生成 `VertexOffset` 输入引脚。
    - `MaterialOutput.cs` 已实现**阶段分流**：`PSFunction` 显式 `continue` 跳过 VertexOffset，`VSFunction` 只处理 VertexOffset，生成 `DO_VS_MATERIAL_IMPL`。
    - 各 ShadingEnv 的 `VS_Main` 里有 `output.vPosition.xyz += mtl.mVertexOffset;`（如 `DeferredOpaque.cginc`）—— 这就是 UE World Position Offset 的等价物，只不过叠加发生在 `mul(WorldMatrix)` 之前，是物体空间偏移。
    - **已有先例**：`CSharpCode/Bricks/FX/Water/SWEWaterNode.cs` 的 GPU 浅水方程水面，就是「平整平面网格 + 材质里通过 `mVertexOffset` 采样高度图做顶点位移」。VAT 与它是同一个模式，只是采样坐标从 `(x, z)` 换成 `(vertexID, frame)`。
  - 材质函数（可复用子图）机制也已存在：`TtMaterialFunction` / `TtMaterialFunctionGraph` / `TtCallMaterialFunctionNode` + `MaterialFunctionEditor.cs`，是 UE Material Function 的等价物。
  - 顺带记录：`enginecontent/Shaders/ShadingEnv/Mobile/heightmap.cginc` 的 `DoTerrainModifierVS` 证明了 modifier 路线同样可行（VS 里按 `vert.vVertexID` 采样纹理）。**但本条目不走这条路**，理由见「关键改动点」。
- **目标（阶段1：只做恒定拓扑）**
  - Houdini Labs 的 VAT 分三种模式：① soft body / rigid（顶点数恒定）② fluid（逐帧拓扑变化，需预分配最大顶点数 + 逐帧有效顶点数）③ sprite/particle。**先只做①**：它覆盖布料、软体、风吹植被、预烘群集动画等绝大多数需求，且完全不需要变长缓冲管理。
  - 纹理布局：宽 = 顶点数，高 = 帧数；UV = `((vertexID + 0.5) / width, (frame + 0.5) / height)`。一张位置纹理（必需）+ 一张法线纹理（可选，无则用相邻帧或让光照退化）。
  - 时间驱动：逐实例的归一化播放进度（可循环 / 一次性 / 反播 / 随机相位偏移），帧间可选线性插值（两次采样 + lerp）。
- **必须写进实现的三条约束（VAT 的经典坑，违反任一条网格直接炸开）**
  1. **顶点方向必须 point 采样**。相邻两列是两个毫无关系的顶点，线性插值会把它们混在一起。帧方向想插值就手动取两行再 `lerp`，不要依赖采样器。
  2. **纹理绝不能压缩、绝不能生 mip**。BC / ASTC 是块压缩，会把相邻顶点的位置值互相污染；mip 降采样同理。这是引擎侧**真正需要新增**的东西：给纹理导入加一个「数据纹理」通道（`CompressFormat` 不压缩 + `MipLevel = 1` + 不走 sRGB）。
  3. **精度要么给够要么带 bounds**。位置用 `R16G16B16A16_FLOAT` 以上；若为省带宽用 RGBA8，必须随资产存 bounds min/max 在 VS 里反归一化（Houdini 导出时会给这两个值）。法线可用 RGBA8 配 `*2-1`。
- **实现路线选定：材质节点，而不是 MdfQueue modifier**
  - modifier 路线（`TtVATModifier` + `VATModifier.cginc` + `TtMdfVATMesh`）技术上可行，但要额外背上 MdfQueue 三级选型链、`.ums` 写入、per-SubMesh 资源绑定，且结果**不可与其他材质效果组合**（位移逻辑硬编码在 C# 里）。
  - 材质节点路线复用已验证的 `mVertexOffset` 通路：普通静态网格 + 一个材质即可播 VAT，位移可与风力扰动、噪声、缩放等任意材质表达式自由叠加，且美术可自行调参。**成本更低、能力更强，故选此路线。**
- **关键改动点（除一处纹理导入外，全部是「加内容」，没有一处改框架）**
  - `CSharpCode/Bricks/FX/VAT/VATShader.cs`（新增）：一个 `[TtMaterialShader]` 标注的 `partial class`，内含 `SampleVAT` 静态方法。范式照 `CSharpCode/Bricks/Font/FontSDF.cs` 的 `TtFontHLSLMethod` 或 `CSharpCode/Bricks/FX/Hair/HairShader.cs` 的 `TtHairShader`：
    ```csharp
    [Rtti.Meta("")]
    [TtMaterialShader(Name = "SampleVAT", Include = "@Engine/Shaders/Bricks/FX/VAT.cginc")]
    [ContextMenu("SampleVAT", "FX\\VAT\\SampleVAT", TtMaterialGraph.MaterialEditorKeyword)]
    public static Vector3 SampleVAT(...) { return Vector3.Zero; }
    ```
    C# 方法体只是空壳（仅用于反射出引脚签名），真实 HLSL 在 `Include` 指向的 cginc 里。新 `.cs` 须登记进对应 `.projitems`（§8）。
  - `enginecontent/Shaders/Bricks/FX/VAT.cginc`（新增）：`SampleVAT` 的 HLSL 实现。**必须把双帧 lerp 与 bounds 反归一化封装在这里**，不要让美术用节点搭 —— 这两处最容易搭错，且搭错就是整个网格炸开。
  - `CSharpCode/NxRHI/Texture.cs`：`FPictureDesc` 的 `CompressFormat` / `MipLevel` 是「数据纹理」通道的落脚点（当前 `MipLevel` 由 `CalcMipLevel` 自动算满级，需可被覆盖）。**这是本条目唯一真正需要改的既有引擎代码。**
  - 一个开箱即用的材质函数资产：把「`SampleVAT` → `VertexOffset`」连同时间参数搭好存成材质函数，美术直接引用，无需理解内部接线。
  - **不需要**的东西：`TtVATModifier`、`TtMdfVATMesh`、MdfQueue 选型链、`.ums` 改动，全部省掉。
  - 资产描述：一个轻量 `.vat`（帧数 / fps / 顶点数 / bounds min-max / 是否带法线）。VAT 纹理本身由 DCC 侧产出，**引擎不做烘焙器**—— 这是本条目成本低的关键前提。
- **法线怎么办（必须在设计阶段定下来，否则做到一半会卡住）**
  - `mVertexOffset` 只有位置一项。法线属于着色量，应走材质图的 Normal 引脚，在 PS 阶段采样。
  - 但 PS 阶段**拿不到 `vVertexID`**（它是 `VS_MODIFIER` 的字段，不在 `PS_INPUT` 里）。
  - 解法正是 Houdini VAT 的标准做法：**导出时把顶点在纹理中的列坐标烘进一个 UV 通道**。这样 VS 与 PS 取到的是同一个采样坐标，位置与法线各自在自己的阶段采样。这同时绕开了「材质图里取不到顶点索引」这个限制 —— 也是材质节点路线能成立的关键。
- **优先级说明**
  - 不进 P0：缺了不会让任何品类做不完整，且强依赖外部 DCC 工具链（没 Houdini 就产不出素材）。
  - 但在 P2 里**性价比最高**：一个静态方法 + 一个 cginc 函数 + 一个纹理导入选项，就能把布料 / 破碎 / 流体的**视觉结果**搬进引擎，比真做这些模拟系统便宜两个数量级。
- **验收标准**
  - 一个 Houdini 导出的布料或破碎 VAT（位置 + 法线纹理）在引擎里循环播放，形变与 Houdini 预览一致。
  - 同一个 VAT 资产多实例各自相位偏移，不同步、不互相影响。
  - 关键反例回归：故意把 VAT 纹理设为压缩或开 mip，应能看到网格炸开 —— 确认「数据纹理」通道真的生效了而不是碰巧。

- [ ] 未开始

### 19. 场景编辑器地形编辑（TtInteractiveMode 扩展）

在场景编辑器里用笔刷直接雕刻地形高度与刷材质，而不是只能回去改 PGC 图。

- **现状**：地形的渲染与物理完整，但**编辑是零**。`brush` / `sculpt` / `splat` / `paint` / `weightmap` 在 `CSharpCode` 与 `NativeCode` 均无有效命中（已按附录要求逐扩展名重扫），不存在 TerrainEditor；高度与材质的唯一来源是 PGC 程序化图（`UTerrainLevelData.BuildLevelDataFromPGC`），想改地形只能改图重生成。`Patch.cs` 里的 `TtPatchLayers` / `TtLayerManager` 分层权重骨架全库无引用，是死代码。
- **目标**：阶段1 高度雕刻（Raise / Lower / Smooth / Flatten）→ 阶段2 材质 ID 绘制 → 阶段3 权重层与植被绘制。路线已定：走 `TtInteractiveMode` 扩展新增一个并发 mode，不新建编辑器框架、不改基类。
- **三条最致命的约束**
  - 地表不进 HitProxy、也无 Terrain 专用 `LineCheck`，落笔点必须自己写 heightfield ray march。
  - 编辑结果绝不能写进 `cache/terrain/*.trlvl` —— 它由 `TerrainGenHash` 门禁，PGC 图一改就整块重建、手工编辑被静默吞掉；必须另立稀疏 delta 覆盖层。
  - DX12 / VK 的 `Texture::UpdateGpuData` 都把 `FootPrint.X/Y/Z` 硬编码为 0，**GPU 局部区域上传不可用**，只能整层重传 + 按 level 节流。
- **验收入口**：拖拽能实时抬高地形且抬手后角色能站上去；Ctrl+Z 逐笔回退（视觉与物理同步）；**改 PGC 图重新生成后手工 delta 仍叠加生效**。

- [ ] 未开始

---

## P3 平台扩展与工程生态

### 20. CI 与自动化回归

- **现状**：仓库内唯一的 CI 配置文件全部来自第三方库（eigen、glm），引擎自身**没有任何 CI**。
- **目标**
  - 构建 CI：拉起 `BuildScript/build.ps1` 全流程（含代码生成），保证 C++ / C# 混编与 codegen 链路不被静默破坏。
  - 渲染截图回归：对固定场景与固定 RenderPolicy 出图比对，容差可配。
  - 性能基线：帧时间与 Drawcall 数的基线看护。
- **意义**：这是纯工程投入但保护的是引擎最贵的资产。目前每次 RenderGraph 或 ReSTIR 改动都在裸奔。
- [ ] 未开始

### 21. 测试体系落地

- **现状**：`CSharpCode/Base/UTest/UnitTester.cs` 提供了反射式 `[TtTest]` 运行器，全仓库 16 处标注，但只有 3 个测试类真正调用 `TAssert` 做断言（`ISerializer.cs` 与新增的 `UTest_RootMotion` / `UTest_AnimMontage`），其余多为空壳、已注释或带 `IgnorTest` 开关，仍是"有壳无肉"。
- **已知障碍**：运行器只在引擎启动流程内、且 `Config.DoUnitTest` 为真时执行（`Base/Engine.cs`），因此单测无法脱离 GPU 设备与完整初始化跑，无法直接当 CI 门禁。需要一个无渲染的 headless 入口（或把纯算法模块拆到不依赖引擎实例的测试工程）。
- **目标**：先为高风险模块补测试（数学库、序列化、RenderGraph 拓扑剪枝与资源生命周期、动画采样与混合），再纳入 CI 门禁。
- [ ] 未开始

### 22. 移动端与桌面端平台落地

- **现状**：`Core.Android` / `Core.iOS` 是 vcxproj 壳工程，`dependencies.json` 中所有原生依赖只有 `win.x86_64` 变体；无 Mac / Linux / 主机支持。生产可用平台实际只有 Windows。
- **目标**：Android 完整跑通（含 Vulkan 移动路径与原生依赖的 ARM 变体）→ iOS → 视需求评估 Mac / Linux。
- **依赖**：静态光照路径（P1-6）应先具备，否则移动端没有光照回落方案。
- [ ] 未开始

### 23. 文档与示例工程

- **现状**：`Documents/tutorials/` 下多个文件（helloworld.md、character.md 等）为 0 字节；`content/survivor`、`content/project_t` 不完整。
- **目标**：一个从零到可玩的完整示例工程 + 配套教程；核心 API 参考。
- **说明**：这一项的实际优先级取决于是否要吸引外部用户。若近期只服务内部项目，可延后；若要做开源生态，应提到 P1。
- [ ] 未开始

---

## 已知技术债

体量小但影响正确性，建议随手就近修掉，不必单独立项。

- [ ] **SkinModifier 与 Morph 都缺切线通路**：`TtSkinModifier.GetNeedStreams()` 只申请 Position / Normal / SkinIndex / SkinWeight，`DoSkinModifierVS` 也只变换 Position 与 Normal。带法线贴图的蒙皮角色其切线空间不随骨骼旋转，光照会出错。morph（P0-1）已落地但同样没有 `DeltaTangent`，两者是同一条通路，应一并补齐。**注意这会改变现有蒙皮角色的光照结果，需做回归检查。**
- [ ] **`DO_VS_MATERIAL` 与 `MdfQueueDoModifiers` 的执行顺序在各 ShadingEnv 里不一致**：共 13 处调用点，其中 **先材质后 modifier** 的有 `DeferredOpaque` / `DeferredTranslucent` / `MobileOpaque` / `MobileTranslucent` / `ForwordTranslucent` / `SSM`（shadow）/ `pick_setup` / `DummyShading`；**先 modifier 后材质** 的有 `ForwordOpaque` / `HitProxy` / `MultiViewID/BasePass` / `DrawViewportShading` / `ScreenSpaceUI`。由于 `DO_VS_MATERIAL(output, mtl)` 的第一个形参是 `in PS_INPUT`（只读），材质的 VertexOffset 表达式一旦**读了 `output` 的位置或法线**（例如「沿法线膨胀」、按局部位置算噪声），就会在不同 pass 里读到不同的值：先材质时是未形变的原始顶点，先 modifier 时是已蒙皮/已 morph 的顶点。最值得注意的两个组合：`DeferredOpaque`（先材质）vs `HitProxy`（先 modifier）→ 点选位置与渲染位置对不上；`ForwordOpaque`（先 modifier）vs `SSM` shadow（先材质）→ 前向管线下阴影与本体错位。**对不读 `output` 的表达式（包括 P2-18 VAT，只需 UV + 时间）无影响**，所以目前没有暴露为 bug；但应统一为一种顺序（建议先 modifier 后材质，让材质能看到真实的形变后顶点），或至少在文档里明确声明「VertexOffset 表达式不得依赖顶点位置/法线」。
- [ ] **`SoundAnimNotify.Trigger()` 为空实现**：随音频系统（P0-2）点亮。
- [ ] **Montage Slot 缺 Macross 图节点**：`TtBlendTree_Slot` 目前只能由 `TtAnimStateMachinePlayer` 在顶层自动挂（SlotName 由 `MontageSlotName` 配），在 BlendTree 图里不可见、也无法插到图的任意位置。需补 `BlendTreeNodeClassDescription` + `GraphElement` 两个描述类（参照 `BlendTree_KawaiiPhysicsClassDescription`）。
- [x] **RootMotion 可能延迟一帧被消费**（已接入 `GetTickOrder`，待运行时验收）：根因比「按 Children 顺序」更坏 —— `World.TickLogic` 是用 `Root.ParallelIterateChildren` **并行**收集节点、靠 `lock (list)` 往 `TickNodes` / `ParallelTickNodes` 里塞（`GamePlay/World.cs:565-593`），所以**连 `TickNodes` 那轮同步 `foreach`（:609）的顺序也是逐帧不稳定的**，`ParallelTickNodes` 更是直接 `ParallelFor`（:621）。真正抢顺序的是 `TtSkeletonAnimPlayNode`（`IRootMotionSource`）与 `TtMovement`：两者都是 `TtLightWeightNodeBase`、都没有 `ParallelTick` 标记，**同在 `TickNodes` 里**，若 Movement 这帧排在前面，消费到的就是上一帧的位移。`TtRootMotionAccumulator` 已用帧号检测并警告一次，但未从机制上解决。机制与接入已完成：`TtNode.GetTickOrder()`（`GamePlay/Scene/Node.cs`）+ `World.TickLogic` 对同步组 `TickNodes` 的升序排序；`TtSkeletonAnimPlayNode` 与 `TtAnimStateMachinePlayNode` 返回 `ETickOrder.Animation`（-1000），`TtMovement` 返回 `ETickOrder.Movement`（1000，`TtCharacterMovement` 继承），两边均已证实在同步组。`TtRootMotionAccumulator` 的帧号检测保留作哨兵，警告文案已改为指向 `GetTickOrder`。**验收：跑一个带 RootMotion 的角色（`RootMotionMode != Ignore`），确认日志里不再出现「RootMotion被延迟一帧消费」警告。**
      同源的跨组问题（`TtMeshNode` 在并行组，却要消费同步组动画节点写入的 `RuntimePose`）已由「同步组先于并行组」的执行顺序反转一并解决，见下条。
- [x] **蒙皮 pose 比动画求值晚一帧（已由「同步组先于并行组」解决，待运行时验收）**：`TtMeshNode` 在构造时 `SetStyle(ENodeStyles.ParallelTick)`（`GamePlay/Scene/MeshNode.cs:111`）→ 落在**并行组**；而写 pose 的 `TtSkeletonAnimPlayNode` / `TtAnimStateMachinePlayNode` 没有该标记 → 在**同步组**。**原先** `World.TickLogic` 里并行组整体跑完才轮到同步组，所以 `TtMeshNode.OnTickLogic`（`MeshNode.cs:563`）在 :573 把 `RuntimePose` 转成 MeshSpace、再于 :576-596 填 `PerSkinMeshCBuffer` 的 `AbsBonePos` / `AbsBoneQuat` 并 `FlushWrite` 时，**读到的永远是上一帧动画写入的内容**。关键细节：`BindingTo` 里 `meshNode.RuntimePose = mAnimatedPose` 两边持的是**同一个对象引用**，动画节点每帧 `CopyPose(ref mAnimatedPose, Player.OutPose)` 是原地改内容，所以不会报错、只是数据晚一帧。（两个组是前后串行的两个阶段，**不是** 数据竞争。）
      修复前的影响：单看表现是“整体动画晚一帧”，视觉上几乎不可察；但跟 RootMotion 叠起来就是“位移用本帧、姿态用上一帧”的恒定错配，理论上会产生轻微滑步。
      修法：把 `World.TickLogic` 的两组执行顺序反转为**先同步组（`GamePlay/World.cs:602-617`，含 `GetTickOrder` 升序排序）、再并行组（:619-627）**，不需要动 `TtMeshNode` 本身、也不损失并行度。依据是 `ENodeStyles.ParallelTick` 的语义就是「我不关心顺序」，而不关心顺序的节点不可能是被依赖方（没人能保证在它之后跑），只能是下游消费方 —— 它天然该排在有序的同步组之后；反转后节点层成为一条可推理的单调时间轴：同步组按 `GetTickOrder` 升序，并行组等价于 `order = +∞` 且组内无序。影响面已核查：并行组里 override 了 `OnTickLogic` 的只有 `TtMeshNode`（`TtPrimitiveMeshNode` 未 override），而 `ENodeStyles` 带 `[Rtti.Meta("")]` 会序列化进场景/prefab、`IsParallelTick` 又是 PropertyGrid 里可勾的可写属性，所以并行组成员不限于代码里那两处 —— 但按上述语义它们都只能是消费方，反转对它们同样是变好。
      残留风险与哨兵：反转后“并行组的产出被同步组消费”变成了会晚一帧的方向，而这种依赖无法静态穷举（`ENodeStyles` 会序列化进资产）。为此在并行收集阶段加了 `TtNode.CheckTickOrderIgnored()`（`GamePlay/Scene/Node.cs`，由 `World.TickLogic` 对并行组节点调用）：一个节点若**同时**带 `ParallelTick` 和非缺省 `GetTickOrder()`，就是自相矛盾的声明（后者在并行组里被静默忽略），会打一次 `ELogTag.Warning` 日志 + `Debug.Assert`，每节点只报一次（靠节点上的非序列化标记去重，首帧之后开销为一次布尔判断）。已核实现有三处 `GetTickOrder` override 均无 `ParallelTick`，因此当前代码不会误触发。**注意这个哨兵只能盖住「声明了顺序却又并行」这一类；若一个并行节点从未 override `GetTickOrder`、却有同步节点暗自依赖它的产出，仍无法检出 —— 彻底解法要等节点层有显式依赖声明。**
      **验收：跑带蒙皮动画的角色，确认姿态与位移不再有一帧错配；并对比 `ScopeTick_SyncTick` / `ScopeTick_ParallelTick` 的耗时，确认并行度没被这次调整意外压缩。**
- [ ] **骨骼上限 360 硬编码**：`cbSkinMesh` 中 `AbsBonePos[360]` / `AbsBoneQuat[360]` 固定占用较大 CBuffer 空间。若后续要做大规模人群或更精细骨架，考虑改为 StructuredBuffer 或按骨架规模分档 Permutation。同时这也是 Morph 数据不应再挤进该 CBuffer 的原因。
- [ ] **`TtTrack.AddKeyframeBack` 不排序，但 `Evaluate` 靠 `BinarySearch`**：`Bricks/Animation/Base/Track.cs` 里 `AddKeyframeBack` 自带注释 `//should check time and sort by time` 却直接 `Add`，而 `Evaluate` 用 `KeyFramesList.BinarySearch` 定区间 —— 一旦调用方不按时间递增插入，求值结果就是错的且不报错。应要么在插入时保序，要么改名为 `AddKeyframeUnsafe` 并补一个显式 `Sort`。Sequencer（P2-12）会大量做任意位置打点，这条必须先修。同文件的 `EvaluateClamp` 是 `return default;` 空壳，`FKeyframe.InSlope` / `OutSlope` 与 `FTrackCache.Coeff0..3` 定义了但求值完全不用。
- [ ] **`FCurveValue` 是个危险的 explicit union**：`Bricks/Animation/Base/Curve.cs` 中 `[StructLayout(LayoutKind.Explicit, Size = 3)]`，`Nullable<float> FloatValue` 与 `FNullableVector3 Vector3Value` 都在 `FieldOffset(0)`。`Size = 3` 与实际载荷（约 24 字节）自相矛盾，两个字段内存重叠，两个构造函数只是靠写入顺序碰对。目前使用面窄所以未暴露，但任何新增的曲线类型都会踩上。应拆成正常结构体（tag + 分开的字段）或直接改成逐通道 float。另，`TtQuaternionCurve.Evaluate` 用 `Quaternion.Lerp`（非 Slerp）且输出转成 Euler，大角度旋转会失真。
- [ ] **`TtGamePlayCamera` 的 `OnTickLogic` 被整段注释**：`GamePlay/Camera/Camera.cs:128-133`。后果是**移动相机节点不会改变视图**，节点的 `Placement` 与它持有的 `TtCamera` 完全脱钩；而且 `mCamera` 自己从不创建，只是在 `GameBase.cs:317` / `:387` 被赋为 `RenderPolicy.DefaultCamera`（借用）。同时它继承 `TtLightWeightNodeBase` → `NodeId` 永远是 `Guid.Empty`，无法被持久引用。随 Sequencer（P2-12）一并修。
- [ ] **README 网络能力措辞**：与 P1-10 同步修正。

---

## 附录：结论核实方式

本文档的"命中数为 0"结论来自对 `CSharpCode` / `NativeCode` / `enginecontent` 的全量标识符扫描，并对每个命中逐行确认语义，已排除以下误报：

- `Lightmap` 的命中实为顶点流类型 `VST_LightMap` 与 FBX UV 通道，非烘焙系统。
- `HLOD` / `Impostor` 的命中实为 `CurrentPatchLOD`、`MorphLODs`、`FlushLODBuffers` 等子串。
- `Decal` 的命中实为 `EditorDecorator` 与 imgui 内部标识符。（此结论已过期：P1-7 落地后贴花系统真实存在，
  入口是 `TtDecalPassNode` / `GamePlay.Scene.TtDecalNode`）
- `Ragdoll` 的命中实为 `TtPhysicsAsset.cs` 中的中文注释。

扫描存在固有局限：若某能力使用了未被预期的命名，可能被误判为缺失。发现与实际不符的条目时，请直接修正本文档正文。

需要注意的一个已知教训：GPU Skinning 曾被误判为缺失，原因是扫描时漏掉了 `.cginc` 扩展名，且骨骼变换以 `AbsBonePos` / `AbsBoneQuat`（位置 + 四元数）形式表达，而非常见的 `BoneMatrix` 命名。

这个坑在规划 VAT（P2-18）时**连续复现了三次**，而且暴露出原本的归因是错的：

1. 全仓搜 `vVertexID` 返回 0 命中，限定 `--glob *.cginc` 后找到 10 多处。
2. 全仓搜 `DO_VS_MATERIAL` 返回 0 命中，限定 `--glob *.cs` 后在 `Material.cs` / `MaterialOutput.cs` 里找到 15 处。
3. 全仓搜 `mVertexOffset` 返回 0 命中，限定 `--glob *.cs` 后在 `ShaderPredefineType.cs` 里找到字段定义。

第 2 、 3 条的目标文件都是普通 `.cs`，所以**真正的原因不是「`.cginc` 这个扩展名特殊」，而是「不指定 glob 的全仓搜索本身就会大面积漏报」**。如果停在第一步，会得出两个严重错误的结论：「顶点索引不可用」以及「材质图没有顶点阶段通路，VAT 必须写 modifier」—— 而后者会直接把一个零框架改动的需求做成一整套 modifier + MdfQueue 选型链。

**结论：对本仓做能力扫描时，任何一次「命中数为 0」都不能当结论，必须至少再用 `*.cs` 与 `*.cginc` / `*.compute` 各显式重扫一遍。**

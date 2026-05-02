# 编码守则

本文档收录 TitanEngine 项目在长期开发实践中沉淀下来的强制性编码守则。
凡是被列入本文档的规则，均代表 **违反后会导致严重 Bug、Crash、性能问题或难以排查的问题**，
所有提交者必须遵守。文档随项目迭代持续扩充。

---

## 1. 渲染相关

### 1.1 CBuffer 在 CreateCBV 之后必须立即 SetValue 全部字段并 FlushDirty 一次

**适用场景**：所有调用 `TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(...)` 创建 cbuffer 的位置。

**强制规则**：

1. CBuffer 在 `CreateCBV` **当次**、`BindCBV` 之前，**必须** 通过 `SetValue` 把所有字段填充完整。
2. 紧随其后**必须** 调用一次 `MarkDirty()` + `FlushDirty()`，把首帧的初始数据立即写入 GPU 内存，
   避免 GPU 在第一帧读到未初始化的垃圾数据。
3. **后续帧** 只需正常 `SetValue` 即可，**不需要** 每次 drawcall 都手动 `FlushDirty`。
   引擎会在每帧 frame end 阶段根据 dirty 状态自动 flush 所有 cbuffer 到 GPU。

**标准模板**：

```csharp
if (mCBuffer == null)
{
    mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
    // ① 首次创建时：把所有字段都 SetValue 一遍，避免 GPU 读到未初始化数据
    mCBuffer.SetValue("FieldA", in valueA);
    mCBuffer.SetValue("FieldB", in valueB);
    // ... 全部字段

    // ② 首次创建时：强制 flush 一次，确保第一帧 GPU 就能读到正确的初始值
    //    （无 cmd 版本会立即 map + memcpy + unmap，同步生效）
    mCBuffer.MarkDirty();
    mCBuffer.FlushDirty();
}

// ③ 后续帧的数据更新：直接 SetValue 即可，引擎会在 frame end 自动 flush
mCBuffer.SetValue("FieldA", in valueA);

// ④ BindCBV
drawcall.BindCBV(index, mCBuffer);
```

**为什么首次创建必须显式 flush**：

- 引擎自动 flush 的时机在每帧 frame end，**对当前帧已经提交的 drawcall 来说太晚**。
- 如果 `CreateCBV` 后直接 `BindCBV`，GPU 在执行本帧的 draw 时，cbuffer 中是**未初始化的内存**，
  字段值随机不可预测。
- 一旦 cbuffer 中包含 **控制 shader 循环次数、数组下标、内存偏移** 的字段（例如 `BlurSize`、`NodeCount` 等），
  GPU 读到 0 或随机大整数都会直接导致 **着色器死循环** 或 **越界访问空指针**，进而 GPU Hang。
- `Fault address: 0x0000000000000000` 这种 Aftermath 报错，几乎都对应 cbuffer 数据未到位的场景。

**反例**（曾经导致 GPU Hang）：

```csharp
// ❌ 反例：首次 CreateCBV 后直接 BindCBV，未填充任何字段也未 flush
if (mCBuffer == null)
    mCBuffer = ...CreateCBV(index);
drawcall.BindCBV(index, mCBuffer);
// 第一帧 GPU 读到的 BlurSize 是随机大整数，shader 里的循环 for(i=0; i<BlurSize; ++i)
// 直接爆炸，warps 卡到几千个，最终 GPU Hang，TDR 触发，进程崩溃。
```

**已有合规实现的参考位置**：

- `CSharpCode/Grapics/Pipeline/Common/Post/GauseNode.cs` — `TtGaussNode.OnDrawCall` / `TtGaussAdditiveShading.OnDrawCall`
- `CSharpCode/Bricks/AdvanceShadow/AdvanceShadowShading.cs` — `cbAdvanceShadow` 绑定逻辑
- `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` — `GetOrCreateSharedCBuffer`

---

### 1.2 所有 BindXxx / CreateCBV 必须在 OnDrawCall 阶段完成，不能在 Tick 阶段

**适用场景**：所有自定义 `RenderGraphNode` + `TtShadingEnv` 的资源绑定，包括 `BindSrv` / `BindUav` /
`BindCBV` / `BindSampler`，以及依赖 `FindBinder` 的 `CreateCBV`。

**强制规则**：

1. 节点的 `Tick` 中**不允许**调用任何 `drawcall.BindXxx`，也**不允许**调用 `FindBinder` /
   `CreateCBV`。
2. 所有资源绑定与 CBV 的创建/获取都必须写在 `ShadingEnv.OnDrawCall(drawcall, policy)` 内部。
3. 如果绑定的资源**每帧 / 每迭代 / 每 pass 都不同**（典型如 ping-pong 的 input/output、
   多 pass 的 StepSize），用**节点成员变量**在 `Tick → OnDrawCall` 之间传值。
4. `Tick` 中调用 `mShading.SetDrawcallDispatch(...)` **必须在**任何与该 drawcall
   相关的状态/参数设置**之后**，并且**之前**不能有 `BindXxx`。

**为什么**：

- `SetDrawcallDispatch` 是分水岭，它内部做两件事：
  ① 把 effect / PSO 装进 drawcall（建立合法的 binder 表）；
  ② 触发 `OnDrawCall(drawcall, policy)` 回调。
- 在 `SetDrawcallDispatch` **之前** 调 `BindXxx`：drawcall 还没有合法 binder 表，绑不上；
  即便底层勉强接收，也会被随后 effect 装配过程覆盖 / 失效。
- 在 `SetDrawcallDispatch` **之前** 调 `FindBinder` 用于 `CreateCBV`：
  effect 可能还没 `await` 完成（构造器里的 `UpdatePermutation().AddWaitTask()` 是
  fire-and-forget），binder 字段表为空，`SetValue` 全部静默失败，再 `FlushDirty`
  一份未填充的 cbuffer 上去 → DXGI 报 `DXGI_ERROR_INVALID_CALL` 或 GPU 读到垃圾。

**标准模板**：

```csharp
// ===== ShadingEnv：所有 Bind 都在这里 =====
public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
{
    var node = drawcall.TagObject as TtMyNode;
    if (node == null) return;

    // 节点字段传过来的"每轮变化"资源
    drawcall.BindSrv("InputTex",  node.GetCurrentInputSrv());
    drawcall.BindUav("OutputTex", node.GetCurrentOutputUav());

    // 静态资源
    drawcall.BindSrv("GBufferRT0", node.GetAttachBuffer(node.GBufferRT0PinIn).Srv);
    drawcall.BindSampler("Samp_PointClamp",
        TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

    // CBV 在这里创建（此时 effect 已就绪，binder 反射有效）
    var cb = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbMyPass");
    if (cb.IsValidPointer)
        drawcall.BindCBV(cb, node.GetOrCreateCBuffer(cb));
}

// ===== 节点 Tick：只暂存"本次要绑什么"，然后 SetDrawcallDispatch + PushGpuDraw =====
public override void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
{
    for (int i = 0; i < iterations; i++)
    {
        var drawcall = mDrawCalls[i];

        // ① 把"本轮"参数暂存到节点字段（OnDrawCall 会读取）
        mCurrentInputSrv  = ...;
        mCurrentOutputUav = ...;
        mCurrentStepSize  = ...;

        // ② SetDrawcallDispatch 之前不要有任何 BindXxx
        mShading.SetDrawcallDispatch(this, policy, drawcall, w, h, 1, true);
        cmd.PushGpuDraw(drawcall);
    }
}
```

**反例**（本仓库踩过）：

```csharp
// ❌ 反例 1：Tick 里直接 BindSrv / BindUav，然后才 SetDrawcallDispatch
drawcall.BindSrv("ColorInput", currentInputSrv);
drawcall.BindUav("DenoiseOutput", currentOutputUav);
mShading.SetDrawcallDispatch(this, policy, drawcall, w, h, 1, true); // ← 此前的 Bind 失效
cmd.PushGpuDraw(drawcall);
```

```csharp
// ❌ 反例 2：Tick 里 FindBinder + CreateCBV（effect 可能还没 await 完成）
var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbXxx");
mCBuffer = ...CreateCBV(binder);    // binder 字段表为空，layout 反射失败
mCBuffer.SetValue(...);              // 静默失败
mCBuffer.FlushDirty();               // 把空数据 flush 上去 → DXGI INVALID_CALL
```

**已有合规实现的参考位置**：

- `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` — 4 个 pass 的 Tick 都是
  "暂存参数 + `SetDrawcallDispatch` + `PushGpuDraw`"；所有 Bind 在各 ShadingEnv 的
  `OnDrawCall` 里完成；CBV 通过 `GetOrCreateSharedCBuffer(binder)` 在 OnDrawCall
  阶段创建并跨 pass 共享。
- `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs` — 多迭代 ping-pong 的范本：
  `mCurrentInputSrv` / `mCurrentOutputUav` / `mCurrentStepSize` 三个节点字段在
  Tick → OnDrawCall 之间传值。

---

### 1.3 同帧多个 drawcall 写入不同数据时, 不能共享同一份 TtCbView

**适用场景**：在节点 `Tick` / `OnDrawCall` 中, 同一帧内提交多个 drawcall, 且这些
drawcall 期望读到 **不同的 cbuffer 数据**（典型如多 pass 迭代每轮 StepSize / mip
层级 / 偏移量不同, 或 N 个粒子 emitter 各自的参数不同）。

**强制规则**：

1. 一份 `TtCbView` 在同一帧内, **只能服务于一组期望读到相同数据的 drawcall**。
2. 如果同帧多个 drawcall 期望读 **不同的数据**, 必须为每个 drawcall 准备 **独立的**
   `TtCbView`, 通常用 `TtCbView[N]` 数组按下标索引。
3. 不能"循环里反复 SetValue 同一个 cbuffer, 然后 push 多个 drawcall"——这样写得到的结果
   是 **GPU 看到的全是最后一次 SetValue 的值**, 前面所有 drawcall 的参数都被覆盖。

**为什么**：

- `TtCbView.SetValue` 写的是 CPU 端 shadow copy, 不会立即上传 GPU; `MarkDirty` 标记
  脏后, 引擎在每帧 frame end 阶段统一 flush 所有 dirty cbuffer (合并多次 SetValue
  以减少 PCIe 传输)。
- 同帧内 4 个 drawcall 共享同一份 cbuffer + 4 次 SetValue, 最终只有 **最后一次的内容**
  会被 flush 到 GPU。所有 4 个 drawcall 在 GPU 执行时读到的都是同样的数据, 前 3 次
  SetValue 形同虚设。
- 这与 §1.1 是同一根本问题（cbuffer flush 是延迟的）的两个不同表现：
  §1.1 是"首帧 cbuffer 是垃圾", §1.3 是"同帧多 drawcall 共享 cbuffer 导致前面的写入丢失"。
- D3D12 / Vulkan 没有"自动 versioning cbuffer per drawcall"机制, **必须由应用层**
  显式准备 N 份独立的 cbuffer 资源。

**标准模板**：

```csharp
// 节点字段: 每个迭代 (drawcall) 独立一份
TtCbView[] mDenoiseCBuffers;       // 长度 = 迭代次数

// Initialize: 数组分配
mDenoiseCBuffers = new TtCbView[maxIter];

// Dispose: 逐个释放
for (int i = 0; i < mDenoiseCBuffers.Length; i++)
    CoreSDK.DisposeObject(ref mDenoiseCBuffers[i]);

// OnDrawCall: 按当前迭代下标取 (或首次创建) 对应的 cbuffer
public TtCbView GetOrCreateCBufferForIteration(FShaderBinder binder, int iteration)
{
    ref TtCbView slot = ref mDenoiseCBuffers[iteration];
    if (slot == null)
    {
        slot = ...CreateCBV(binder);
        // §1.1: 全字段 SetValue + MarkDirty + FlushDirty
        slot.SetValue(...);
        slot.MarkDirty();
        slot.FlushDirty();
        return slot;
    }
    slot.SetValue("StepSize", mCurrentStepSize);   // 本轮独有的参数
    slot.SetValue(...);                              // 其他通用参数
    return slot;
}
```

**反例**（本仓库踩过, RenderDoc 实证 4 个 dispatch 全部读到 StepSize=8）：

```csharp
// ❌ 反例: 4 个迭代共享同一份 DenoiseCBuffer
TtCbView DenoiseCBuffer;
public TtCbView GetOrCreateCBuffer(FShaderBinder binder)
{
    if (DenoiseCBuffer == null) { DenoiseCBuffer = ...CreateCBV(binder); /*…*/ }
    DenoiseCBuffer.SetValue("StepSize", mCurrentStepSize);   // 4 次互相覆盖
    return DenoiseCBuffer;
}
// for (int i = 0; i < 4; i++) { mCurrentStepSize = 1<<i; SetDrawcallDispatch(...); PushGpuDraw(...); }
// 结果: GPU 看到的 4 个 dispatch 全部 StepSize = 8 (最后一次的值)
```

**已有合规实现的参考位置**：

- `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs` — `mDenoiseCBuffers[N]`
  数组, 每个迭代独立一份, `GetOrCreateCBufferForIteration(binder, iteration)` 按下标取
- `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` — **正面对照**: 4 个 pass
  共享一份 `mSharedCBuffer` 是**安全的**, 因为 4 个 pass 期望读到的 cbuffer 数据
  **完全相同** (cbReSTIR 没有"per-pass 独有字段")。共享的判断标准是"数据是否相同",
  不是"pass 数量"。

---

### 1.4 Bindless 资源数组的使用规范 (TtBindless / DX_AUTOBIND)

**适用场景**: 所有调用 `drawcall.CreateBindless(string name)` 创建 `TtBindless` 实例,
或在 HLSL 端声明 `Texture2D<float4> Foo[] DX_AUTOBIND;` /
`SamplerState Bar[] DX_AUTOBIND;` / `cbuffer cbBaz DX_AUTOBIND` 这种 unbounded
descriptor array (descriptor heap 直接索引) 的场景。

**前置说明 (Bindless 是什么 / 为什么要用)**:

- 传统绑定: 一个 drawcall 最多绑十几个 SRV/UAV/CBV slot, 全部由 root signature
  / descriptor table 静态描述. 适合 "管线确定后 shader 固定知道要采几张图".
- Bindless: 把整个 descriptor heap 暴露成 unbounded array, shader 端用任意整数
  索引访问. 适合 "shader 在运行时才知道要访问哪个资源" 的场景, 例如:
  - HW RT 命中三角形后按 InstanceID / MaterialID 取该材质的所有纹理
  - GPU-driven rendering 里每个 cluster 自己决定用哪张纹理
  - 大场景同帧渲染上千个不同材质, 不可能为每个 drawcall 单独绑
- 本仓库 RHI 已经具备完整 Bindless 基础设施 (`NxBuffer.h: IBindless`,
  `DX12Buffer.h: DX12Bindless`, `Buffer.cs: TtBindless`), HLSL 端通过
  `DX_AUTOBIND` 标记自动注册到 effect binder 表. 当前主要用例是 ReSTIR GI
  HW RT 命中点的二级 lighting 重建 (规划中, 尚未落地), 以及未来的 GPU-driven
  渲染 / RT 反射.

**强制规则**:

#### 1.4.1 创建时机与 §1.2 一致 (OnDrawCall 阶段)

`drawcall.CreateBindless(name)` 内部需要 effect 已经 binder-resolve 完毕
(否则 `name` 找不到对应的 unbounded array 声明, 返回 null), 这与 §1.2
中 `FindBinder` / `CreateCBV` 的限制完全相同:

- **必须**: 在 `ShadingEnv.OnDrawCall(drawcall, policy)` 里调用
  `drawcall.CreateBindless(name)`.
- **禁止**: 在节点的 `Tick` / `Initialize` / `BeforeTick` 中调用
  (effect 未必 await 完成, name 解析会失败, 返回 null 或 invalid handle).

#### 1.4.2 TtBindless 实例可跨帧/跨 drawcall 复用, 不要每帧重建

底层 `IBindless` 维护了 `FBindResource` fingerprint 表, `SetResource(idx, res)`
只在 fingerprint 真变了的时候才会调 `OnBind` 写 descriptor heap. 这意味着:

- **推荐**: 把 `TtBindless` 存为节点 / scene-level 单例的成员字段, 跨帧复用,
  每帧只对变化的 slot 调 `SetSrv/SetCbv/SetUav/SetSampler`.
- **禁止**: 每帧都 `CreateBindless` + 全量 `SetXxx`, 这会让 descriptor heap
  分配器持续增长直到 OOM.

#### 1.4.3 资源 lifetime 反向锁: 引用着的 underlying resource 不能提前 Dispose

`TtBindless.SetSrv(idx, srv)` 内部只把 `srv.mCoreObject.NativeSuper` 的指针
存到 descriptor heap. 它**不会** AddRef 该资源. 一旦上层把这个 srv 对应的
texture / buffer Dispose 掉:

- CPU 端 `TtBindless.mResources[idx].Resource` 仍指向已释放对象 (野指针)
- GPU 端 descriptor heap entry 仍指向已回收的 GPU 内存
- 下一次该 drawcall 提交时 GPU 读到 garbage / unmap 区域 -> page fault / TDR

**规则**:

- 持有 `TtBindless` 的代码必须负责管理被引用 resource 的 lifetime.
- 释放某个 resource 之前, **必须**先调用 `TtBindless.ClearSlot(idx)` 或
  把对应 slot SetXxx 成另一个有效资源, 把 slot 显式断开.
- 节点 `Dispose` 阶段: 先 `TtBindless.RebindAll()` 后再 `Dispose()` 实例本身,
  避免引擎 frame end 还引用着已断指针.

#### 1.4.4 NonUniformResourceIndex 强制规则 (HLSL 端)

如果 bindless 的索引值在同一个 wave 内**不同 lane 取值不同** (典型场景:
ray hit InstanceID, GPU-driven cluster ID, 屏幕像素逐个的 MaterialID),
HLSL 端访问时**必须**用 `NonUniformResourceIndex` 包起来:

```hlsl
// ✓ 正确: 索引来自不同 lane (每像素 ray hit 不同三角形)
uint matId = HitInstanceMaterialIDs[hitInstance];
float4 c = BindlessTextures[NonUniformResourceIndex(matId)].Sample(samp, uv);

// ✗ 错误: 不包 NonUniformResourceIndex, GPU 行为未定义
//   - 部分 GPU 会读到错的 wave-broadcast 值
//   - DXC validator 在 -enable-16bit-types / SM6.6 下可能直接拒绝编译
float4 c = BindlessTextures[matId].Sample(samp, uv);
```

**例外**: 如果索引在编译期是常量 (`BindlessTextures[0]`) 或在 wave 内
所有 lane 都相同 (例如来自 cbuffer 的全局参数), 不需要包.

#### 1.4.5 HLSL 端声明模板

```hlsl
// 声明: 必须用 [] (unbounded) + DX_AUTOBIND
// 必须用 #if RHI_TYPE == RHI_DX12 守卫, 因为其他 RHI 后端可能未实现 bindless
#if RHI_TYPE == RHI_DX12
Texture2D<float4>          BindlessAlbedo[]  DX_AUTOBIND;  // SRV 数组
RWTexture2D<float4>        BindlessUavRT[]   DX_AUTOBIND;  // UAV 数组
SamplerState               BindlessSamps[]   DX_AUTOBIND;  // Sampler 数组
StructuredBuffer<MyVertex> BindlessVB[]      DX_AUTOBIND;  // 结构化 buffer 数组
#endif

// 访问: 不同 lane 不同索引时必须 NonUniformResourceIndex
float4 SampleMaterial(uint matId, float2 uv)
{
#if RHI_TYPE == RHI_DX12
    return BindlessAlbedo[NonUniformResourceIndex(matId)]
        .Sample(BindlessSamps[NonUniformResourceIndex(matId)], uv);
#else
    return float4(1, 0, 1, 1); // 其他 RHI 走 fallback
#endif
}
```

#### 1.4.6 ResourceCount 与 MaxBindless 上限

- `TtBindless.ResourceCount` 是当前实例**已分配**的 slot 数, 由 effect 编译时
  探测到的最大索引决定 (例如 effect 里出现过 `BindlessAlbedo[15]` 就会把
  ResourceCount 至少分到 16). 上层 `SetSrv(idx, ...)` 必须保证
  `idx < ResourceCount`, 否则返回 false 静默失败.
- `TtBindless.MaxBindless` (= `IBindless::MaxBindless` = 4096) 是单个 bindless
  表的全局硬上限. 超过此值需要拆分多个 TtBindless 实例.
- 如果场景规模需要超过 4096 个材质, 解决方案是按"材质类型"拆表 (例如
  AlbedoTable / NormalTable 各自 4096 上限, 而不是合并到一个 4096 大表).

#### 1.4.7 IsCopyNull 默认 false, 不要随便改

`TtBindless.IsCopyNull` 控制 `RebindAll` 时是否对 null slot 显式绑 NullHeap.

- **默认 false**: null slot 不参与重绑, 保留上一次的 descriptor (可能是已释放
  资源的悬挂指针, 但因为 §1.4.3 的 lifetime 反向锁, 上层应该确保不会进入这种
  状态).
- 改成 true 的代价: 每次 RebindAll 都会触发 4096 次 NullHeap 重绑 syscall,
  CPU 端 ~毫秒级开销.
- **唯一应该改成 true 的场景**: 你需要"明确清空 slot, 让 shader 索引到时拿到
  null 数据" 的安全保证, 而不是依赖 ClearSlot 的 per-slot 显式清空.

**为什么 (深层原因)**:

`OnDrawCall` 阶段的限制 (§1.4.1) 与 §1.2 是同一根本问题: effect 是异步加载的,
binder 表只在 `SetDrawcallDispatch` 触发 effect 装配后才有效. CreateBindless
依赖 binder 表反射出 unbounded array 的 root parameter 索引, 提前调用必然
拿到 invalid handle.

`IsCopyNull` 规则的存在是因为 D3D12 的 descriptor heap 本身没有 "已释放"
状态: 一个 entry 一旦写入就一直被 GPU 当成有效, 直到被覆盖成 NullHeap 或
另一个有效 descriptor. 所以"释放 resource 不等于 GPU 端 slot 失效",
必须靠应用层显式管理.

NonUniformResourceIndex 规则则是 HLSL/HW 的硬约束: GPU SIMD 执行模型默认假设
descriptor 索引是 wave-uniform 的, 标记 NonUniformResourceIndex 才会切换到
"逐 lane 各自走 descriptor heap" 的执行路径. 不标会拿错 descriptor (而且
在不同 IHV 上行为不一致, 有的不报错只是结果错, 极难排查).

**反例**:

```csharp
// ❌ 反例 1: 在节点 Tick 里 CreateBindless (违反 §1.4.1)
public override void Tick(...)
{
    // effect 未必 await 完成, name 解析失败, 返回 null
    mMaterialTable = drawcall.CreateBindless("BindlessAlbedo");
    mMaterialTable.SetSrv(0, mAlbedoSrv);  // null reference exception
    mShading.SetDrawcallDispatch(...);
}
```

```csharp
// ❌ 反例 2: 每帧重建 TtBindless (违反 §1.4.2)
public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
{
    var table = drawcall.CreateBindless("BindlessAlbedo");
    for (int i = 0; i < scene.Materials.Count; i++)
        table.SetSrv((uint)i, scene.Materials[i].AlbedoSrv);
    // 每帧创建一个新表 -> descriptor heap 持续增长 -> OOM
}
```

```csharp
// ❌ 反例 3: 不解锁 slot 直接 Dispose 资源 (违反 §1.4.3)
mMaterialTable.SetSrv(0, oldAlbedoSrv);
oldAlbedoSrv.Dispose();  // GPU descriptor 现在指向已释放内存
                         // 下一次 drawcall 提交 -> page fault / TDR
// 正确做法: mMaterialTable.ClearSlot(0); 然后再 oldAlbedoSrv.Dispose();
```

```hlsl
// ❌ 反例 4: 不同 lane 不同索引但漏写 NonUniformResourceIndex (违反 §1.4.4)
uint matId = HitInstanceMaterialIDs[hitInstance];
float4 c = BindlessTextures[matId].Sample(samp, uv);
// 在 NVIDIA / AMD / Intel GPU 上行为不一致, 极难排查
```

```hlsl
// ❌ 反例 5: 用固定数组大小代替 unbounded array (违反 §1.4.5)
Texture2D<float4> BindlessAlbedo[64] DX_AUTOBIND; // 限制了 ResourceCount=64
                                                   // effect 编译期就锁死, 失去 bindless
                                                   // 的全部意义
```

**已有合规实现的参考位置**:

- `enginecontent/Shaders/ShadingEnv/DummyShading.cginc` — HLSL 端 `BindlessTextures[]
  DX_AUTOBIND` + `RHI_TYPE == RHI_DX12` 守卫 + 字面量索引访问的最小模板.
- `enginecontent/Shaders/Bricks/AdvanceShadow/AdvanceShadow.cginc` — `cbuffer cbXxx
  DX_AUTOBIND` (DX_AUTOBIND 不仅可以标 unbounded array, 也可以标 cbuffer 自动注册).
- `CSharpCode/NxRHI/Buffer.cs: TtBindless` — C# 端完整 wrapper (SetSrv/SetCbv/SetUav/
  SetSampler/ClearSlot/RebindAll/ResourceCount/MaxBindless).
- `CSharpCode/NxRHI/Drawcall.cs: TtComputeDraw / TtGraphicDraw / TtRayTracingDraw`
  — 三种 drawcall 都提供 `CreateBindless(string name)`.

#### 1.4.8 Bindless 与 ReSTIR Hybrid GI 的前置依赖关系 (重点上下文)

**这是本仓库当前阶段把 Bindless 列为引擎重点升级方向的根本原因. 任何接手
ReSTIR / Hybrid GI / RT 反射 / RT AO 类工作的同学必须先理解这个上下文,
否则会重复设计出"绕开 bindless 的临时方案", 浪费迭代成本.**

**问题背景 (当前 ReSTIR GI HW RT 路径的根本缺陷)**:

`enginecontent/Shaders/GI/ReSTIR/ReSTIRInitialSampling.compute` 的 HW RT
分支在拿到 ray hit 之后, **没有真正在 hit 点重做 lighting**, 而是把 hit 的
世界坐标投影回当前帧屏幕空间, 采样 `PrevColor` (上一帧已经包含直接光 + 阴影
的 final color) 作为该 hit 点的 outgoing radiance 估计. 这种"屏幕反投影
取色"的方案有 4 个无法回避的失败模式:

1. **Hit 点投影出屏**: 摄像机左侧的墙反射应该来自摄像机右侧的物体, hit 点
   投影到屏幕外, 取不到 PrevColor (当前 §1.4.8 上游 commit 的修法是返回
   sky, 但这等于丢了反射).
2. **Hit 点被前景遮挡**: hit 点的屏幕投影位置被另一个更近的物体挡住,
   PrevColor 取到的是遮挡物的颜色, 不是 hit 点真实的颜色 (典型表现: 嘴里的
   红色 emissive 被前牙挡住, ReSTIR 取到牙的白色, 跨帧抖动产生闪烁).
3. **首帧 / 切镜头 / disocclusion 区域**: PrevColor 还没有该位置的有效数据,
   只能 fallback 到 sky 或黑色, ReSTIR reservoir 收敛极慢.
4. **本质上是 1-bounce screen-space lighting**: 取的是 "上一帧屏幕空间已有
   光照", 而不是 "在 hit 点重新做一次 direct lighting". 任何屏幕外的间接光
   贡献都丢了.

**正确的解法 (业界标准)**: 在 hit 点直接重建 lighting:

```
hit -> 拿到 InstanceID + PrimitiveID + Barycentric
    -> 查 InstanceData[InstanceID] 拿到该 instance 的 MaterialID +
       VertexBuffer/IndexBuffer 索引
    -> 查 IndexBuffer + VertexBuffer 重建 hit 点的 normal / uv / tangent
    -> 用 MaterialID 索引 BindlessAlbedo[matId] / BindlessNormal[matId] /
       BindlessRoughness[matId] / BindlessEmissive[matId] 采样材质
    -> 在 hit 点做 direct lighting (sun shadow ray + analytical light loop)
    -> 这个真实的 hit 点 radiance 喂给 ReSTIR reservoir
```

**为什么这个方案离开 Bindless 就做不了**:

- 一个场景可能有几百到几千个不同材质 (每个材质 4-8 张纹理). 传统绑定方式
  一个 drawcall 最多绑十几张 SRV, 完全装不下整场材质表.
- ray hit 在 wave 内不同 lane 命中不同 instance, 每个 lane 需要采样不同
  material 的纹理. 这是 §1.4.4 NonUniformResourceIndex 的标准用例 — 没有
  bindless + NonUniformResourceIndex, GPU 端根本无法表达 "按 ray hit 结果
  动态选择材质" 的逻辑.
- 替代方案 (例如 "把所有材质纹理打包成 texture array") 受 array slice 数量
  / 分辨率必须一致 / 格式必须一致 等硬约束, 实际工程上不可行.

**所以 Bindless 不是某个 nice-to-have 的优化**, 而是 ReSTIR Hybrid GI 走到
"真·hit 点 lighting" 这一步的唯一前置基础设施. 引擎升级的优先级是:

1. **Phase A** (本次已交付): C# 端 `TtBindless` wrapper + 本规范 §1.4
   完整建立, 让任何后续工作有合规可依.
2. **Phase B** (待做, ReSTIR HW RT 改造前必须完成):
   - 场景级 `TtSceneMaterialTable` — 单例的 `TtBindless` 实例池 (按
     SBT_SRV / SBT_Sampler 拆表), 维护 MaterialID -> bindless slot 的
     映射, 跨帧复用.
   - `TtSceneMeshTable` — 同上但管 vertex/index buffer 的 SRV (供
     ray hit 重建几何属性), 可能合并到 MaterialTable 也可能拆开.
   - 材质资产加载/卸载 hook 同步维护 bindless 表 (§1.4.3 的 lifetime
     反向锁约束).
3. **Phase C** (Phase B 完成后):
   - 改造 `ReSTIRInitialSampling.compute` 的 HW RT 分支, 用 hit 点真实
     lighting 替代 PrevColor 屏幕反投影.
   - 同时受益的: RT 反射 (`reflection ray hit -> shading`)、RT AO
     (其实只需要 visibility, 不必 bindless, 但顺便走通)、未来的
     path tracing reference renderer.

**接手任何上述工作的同学**:

- 必须先把 §1.4 全文读完 (§1.4.1 ~ §1.4.8).
- Phase B 的两个 Table 类设计时, 直接复用 `TtBindless`, 不要自己造轮子.
- 如果发现现有 RHI / asset pipeline 在 Phase B 落地时有缺口
  (例如材质卸载时没有 hook 通知 bindless table 解锁 slot), 必须在
  CodingGuidelines.md 增补新规则, 而不是在自己的代码里临时绕过.

#### 1.4.9 其他已识别的未来 Bindless 用例 (优先级低于 §1.4.8)

- **GPU-driven rendering**: 每个 cluster 在 culling compute 阶段决定自己的
  材质, GBuffer pass 用 `NonUniformResourceIndex(matId)` 取该 cluster 的
  所有纹理. 与 §1.4.8 共享同一份场景级 material bindless table.
- **Virtual Texture / Mega-texture page table**: page indirection 表用
  bindless `Texture2D<uint2>` 数组组织, shader 端按 mipID 索引拿到 page
  uv. 当前 `enginecontent/Shaders/Bricks/VirtualTexture/RealtimeVT.cginc`
  的注释已经预留了这个用例.
- **Decal / Light atlas**: decal projector 数量动态变化, 用 bindless 数组
  按 decal ID 索引贴图, 配合 cluster culling.

---

## 2. 调试相关

### 2.1 RenderDoc 抓帧 (.rdc) 默认存放目录

**约定**：本仓库所有 RenderDoc 抓帧文件统一放在 `cache/renderdoc/` 目录下（相对仓库根）。

**适用场景**：

- 需要分析渲染问题（萤火虫、闪烁、色块、performance spike 等）时，开发者抓的 `.rdc`
  必须存到 `cache/renderdoc/`，**不要散落到桌面、Downloads 或临时目录**。
- AI 助手 / 协作者打开 RDC 进行分析时，**默认从 `cache/renderdoc/` 下查找**，
  无需用户每次提供绝对路径。

**为什么统一目录**：

- 便于团队成员/AI 工具之间快速复现问题（约定俗成的查找位置）
- `cache/` 目录已被 `.gitignore` 排除，不会污染 git 历史，但本机自动归档
- AI 工具调用 RenderDoc MCP 时可以直接用相对路径 `cache/renderdoc/xxx.rdc`，
  避免硬编码绝对路径，**任何机器、任何 clone 路径**都能复用同一套分析流程

**存放规范**：

- 文件命名建议带场景/特征关键字，例如：
  - `denoise_fireflies_2026_04_30.rdc`
  - `restir_first_frame_hang.rdc`
  - `gbuffer_split.rdc`
- 太大的 RDC（> 500MB）分析完应及时清理，避免本地 cache 膨胀

---

### 2.2 async Thread.Async.TtTask 严禁 fire-and-forget，必须 AddWaitTask

**适用场景**：所有返回 `Thread.Async.TtTask` / `Thread.Async.TtTask<T>` 的异步方法的调用。

**强制规则**：

1. 任何 `async Thread.Async.TtTask` / `async Thread.Async.TtTask<T>` 方法的返回值，
   要么 **`await`**，要么 **`.AddWaitTask([可选 callback])`**，**绝对不允许** 用 `_ = XxxAsync()`、
   `XxxAsync();` 直接丢弃返回值。
2. 这条规则对 "fire-and-forget 后台任务"（例如 worker loop、setter 触发的 Permutation 重建）
   **同样适用**，必须用 `AddWaitTask` 兜底，不能用 `_ = ...` 丢弃。
3. 需要在任务完成时做善后（拿结果、记录日志、释放资源等）时，把回调传给 `AddWaitTask`：
   `XxxAsync().AddWaitTask(task => { ... })`。

**为什么必须 AddWaitTask**：

- `TtTask` / `TtTask<T>` 内部的 `TtTaskData` / `TtTaskData<T>` 是从 **对象池** (`TtObjectPool`)
  分配的，需要通过 **`Dispose`** 显式归还到池里复用。
- `await` 的路径上，编译生成的 `TtFiberAwaiter.GetResult` 会自动 `Dispose`（参考 Task.cs 中
  `TtFiberAwaiter<T>.GetResult`）。
- `AddWaitTask` 的路径上，`TtTaskCollector.Tick` 在任务完成后会自动 `Dispose`（参考 Task.cs 中
  `TtTaskCollector.Tick` 的 `IsCompletedDispose` 分支）。
- **`_ = XxxAsync()` 没有任何一方负责 Dispose**，`TtTaskData` 无法归还对象池，导致：
  - 池里的对象只进不出，长期运行后池退化成"每次都新分配"，**TtTask 对象池失效**；
  - GC 压力上升、堆碎片增加；
  - 异常 (`TrySetException`) 发生时也没人观察，**异常被吞掉无任何日志**，问题极难排查。

**标准模板**：

```csharp
// ✓ 推荐：调用方就在异步上下文里，直接 await
await DoSomethingAsync();

// ✓ 推荐：fire-and-forget 后台任务（不能 await 的同步上下文），用 AddWaitTask 兜底
DoSomethingAsync().AddWaitTask();

// ✓ 推荐：需要在任务完成时做收尾（日志、释放、状态机推进等）
RunWorkerAsync().AddWaitTask((task) =>
{
    // task 类型是 ITask, 如果需要拿结果可以 cast 回 TtTask<T> 再读 DirectResult
    if (task.Exception != null)
        Profiler.Log.WriteException(task.Exception);
});

// ✓ Permutation 切换的标准写法（仓库里大量使用）
set { EnableHWRT.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
```

**反例**（会导致 TtTask 对象池失效 + 异常被吞）：

```csharp
// ❌ 反例 1：直接弃用返回值
_ = WorkerLoop();          // TtTaskData 永远不归还池, 池失效
WorkerLoop();              // 同上, 而且编译器不会警告 (因为是 struct)

// ❌ 反例 2：在 lambda 里 fire-and-forget 一个 async TtTask
RunOn((state) =>
{
    _ = BackgroundLoopAsync();   // 同样违规
    return true;
});
```

**合规修法**：把上面所有 `_ = XxxAsync()` 全部改成 `XxxAsync().AddWaitTask()`，
如果需要观察异常或做收尾就再加一个 callback。

**同步等待 API（必须在同步上下文里拿异步结果时使用）**：

某些场景调用方就是同步函数（构造器、IDisposable.Dispose、ImGui 绘制回调、native 回调等），
不能 `await`，但又必须**当场拿到异步结果**。这种情况下不要走 `AddWaitTask` 异步回调，
而是用 `TtTask` / `TtTask<T>` 自带的同步等待 API，**它们内部都会自动 Dispose 并归还对象池**：

| 调用对象 | API | 行为 | 何时用 |
|---|---|---|---|
| `TtTask` (无返回值) | `WaitCompleted()` | 阻塞当前 fiber 直到任务完成；**不 Dispose**，需手动调 `Dispose()` 或紧接着用 `WaitCompletedAndDispose()` | 想阻塞但还要继续读 `Exception` 等字段时 |
| `TtTask` | `WaitCompletedAndDispose()` | 阻塞 + 自动归还对象池 | **常用**：纯粹要等它跑完，不关心结果 |
| `TtTask<T>` | `GetResultUntilCompleted()` | 阻塞 + 拿结果 + **自动归还对象池** | **常用**：同步上下文里要拿 `T` 结果 |
| `TtTask<T>` | `WaitCompleted()` | 仅阻塞，**不归还对象池** | 极少用，需要后续手动 `Dispose` 或读 `DirectResult` 后 `Dispose` |
| `TtTask<T>` | `DirectResult` | 直接读结果字段，**不阻塞、不 Dispose**；任务未完成会抛 `InvalidOperationException` | 仅当你已经通过别的方式确保 `IsCompleted == true`（例如 `await` 后 / `WaitCompleted` 后）|
| `TtTask<T>` | `GetResultAndRelease()` | 等价于 `GetAwaiter().GetResult()`；**要求任务已完成**，会自动 Dispose | 在 `await` 取代品场景使用 |

**`WaitCompleted` 的实现机制**：底层走 `TtContextThread.CurrentContext.WaitTask`，
即把当前 fiber 让出去给同 context 的其他任务跑，直到目标任务完成才回来——
**不会真的卡死操作系统线程**，所以即使在主线程上调用也是安全的（只要不在 GPU 渲染线程的关键路径上 spin）。

**标准模板**：

```csharp
// ✓ 同步上下文里要等异步任务且不关心结果（例如 IDisposable.Dispose 收尾）
public void Dispose()
{
    if (mPendingFlush != null)
    {
        FlushAsync().WaitCompletedAndDispose();   // 阻塞 + 自动 Dispose
    }
}

// ✓ 同步上下文里要等异步任务并拿结果
public Texture LoadSync(RName name)
{
    return LoadAsync(name).GetResultUntilCompleted();   // 阻塞 + 拿 T + 自动 Dispose
}

// ✓ 已经 await 过, 想再次复用结果（极少用）
var task = ComputeAsync();
await task;
var result = task.DirectResult;        // 直接读, 不再 Dispose（await 已经 Dispose 过了, 此处其实会 NRE, 真要复用要在 await 前先 task = ...; 用 GetResultAndRelease 之类）
// 一般直接 var result = await ComputeAsync(); 即可, 不要这种花哨写法
```

**反例**：

```csharp
// ❌ 用 .Result / .Wait() 之类 .NET 标准 Task API
var v = LoadAsync().Result;             // TtTask<T> 上没有 Result 属性, 会编译失败; 即便有也违反对象池约定

// ❌ 同步上下文里 .GetAwaiter().GetResult() 之前没确认 IsCompleted
var v = LoadAsync().GetResultAndRelease();   // 任务还没完成会直接抛 InvalidOperationException
                                              // 如果就是想阻塞等, 用 GetResultUntilCompleted

// ❌ WaitCompleted 后忘记 Dispose
LoadAsync().WaitCompleted();             // 阻塞完成了, 但 TtTaskData 没归还对象池, 等同 fire-and-forget
                                         // 应改用 WaitCompletedAndDispose / GetResultUntilCompleted
```

**总结**：

- 异步上下文（方法签名是 `async TtTask` / `async TtTask<T>`）→ **`await`**
- 同步上下文 + fire-and-forget → **`.AddWaitTask([cb])`**
- 同步上下文 + 必须当场拿结果 / 必须当场跑完 → **`.GetResultUntilCompleted()` / `.WaitCompletedAndDispose()`**

**已有合规实现的参考位置**：

- `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` — `EnableHWRT` / `EnableSkyCube` setter 中的
  `UpdatePermutation().AddWaitTask()`
- `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs` — 同上模式
- `CSharpCode/Editor/Snapshot.cs` — `TtSnapshotGenQueue.EnsureWorker` 中后台 `WorkerLoop().AddWaitTask(...)`
- `CSharpCode/Base/Thread/Async/Task.cs` — `WaitCompleted` / `WaitCompletedAndDispose` /
  `GetResultUntilCompleted` / `GetResultAndRelease` / `DirectResult` 的源码定义

---

**跨上下文桥接：把 TtTask 的"完成"事件转成 await 点（用 TtSemaphore）**

很多调度器场景的形态是：

- **生产者**：在某个回调 / 同步循环里"播种"一批异步任务（`AddWaitTask` 提交），不能 `await`。
- **消费者**：在 `async` 上下文里需要等"我提交的那一项跑完"，必须能被异步唤醒。

这种"同步生产 + 异步等待"的桥梁，**不要**用 `while (!done) await Yield();` 这种 polling
（每帧轮询一次 done 标志，既费 fiber 调度也不及时），而是用 `Thread.TtSemaphore.Await()`：

- `TtSemaphore.CreateSemaphore(1)`：创建一个 count=1 的 semaphore（count 减到 ≤0 时唤醒）。
- `await semaphore.Await()`：异步等待 count ≤ 0，**只 yield 一次 fiber**，释放即被唤醒，
  零轮询开销。底层走 `EventPoster.AwaitSemaphore`，与引擎 fiber 调度一体。
- `semaphore.Release()`：count -= 1，达到 0 时把等待方重新调度回来。
- `semaphore.FreeSemaphore()`：等完后回收资源。

**桥接公式**：把 semaphore 的 `Release` 放到异步任务的 `AddWaitTask` 回调里，
让两个原本互相隔离的世界通过 semaphore 串起来。

**标准模板**（来自 `CSharpCode/Editor/Snapshot.cs: TtSnapshotGenQueue`）：

```csharp
class FPendingItem
{
    public IO.IAssetMeta Meta;
    public bool Result;
    public Thread.TtSemaphore FinishedSemaphore;
}

// ─── 异步消费方：可以 await 等"我这一项跑完" ───
public async Thread.Async.TtTask<bool> EnqueueAutoGen(IO.IAssetMeta meta)
{
    var item = new FPendingItem
    {
        Meta = meta,
        FinishedSemaphore = Thread.TtSemaphore.CreateSemaphore(1),  // count=1, Release 一次即唤醒
    };
    lock (mLock) { mPendingItems.Enqueue(item); }
    EnsureWorker();

    await item.FinishedSemaphore.Await();   // 只 yield 一次 fiber, 不轮询
    item.FinishedSemaphore.FreeSemaphore(); // 唤醒后立刻回收
    return item.Result;
}

// ─── 同步生产方：worker 是普通同步循环, 不是 async TtTask ───
void WorkerLoop()
{
    while (true)
    {
        FPendingItem item;
        lock (mLock)
        {
            if (mPendingItems.Count == 0) { mWorkerRunning = false; return; }
            item = mPendingItems.Dequeue();
        }

        // 关键: 不要 await, 用 AddWaitTask 把完成事件桥到 semaphore
        item.Meta.AutoGenSnapshot().AddWaitTask((task) =>
        {
            var snapshotTask = (Thread.Async.TtTask<bool>)task;
            item.Result = snapshotTask.DirectResult;     // 在 callback 里读 DirectResult, 此时 IsCompleted 必为 true
            item.FinishedSemaphore.Release();            // 唤醒等待方
        });
    }
}
```

**为什么 worker 用同步函数而不是 `async TtTask`**：

- 如果 worker 是 `async TtTask` 且循环里 `await item.Meta.AutoGenSnapshot()`，
  那么"前一项 await 完才会处理后一项"——但这是 await 串行，**调度行为正确**。
- 然而如果 worker 进一步想"批量提交、各自串行/并行控制"，写成同步循环 + `AddWaitTask` 提交
  会更直观：worker 只负责"出队 + 提交"，**真正的串行控制由队列的"一次只 dequeue 一个"
  自然保证**，每个 item 的完成回调只负责通知它自己的等待方，互不干扰。
- 同步 worker 还有一个好处：**不产生 `TtTask` 对象**，无需走 `AddWaitTask` 的对象池开销。

**回调里读结果的注意事项**：

- `AddWaitTask((task) => { ... })` 的 `task` 参数类型是 `ITask`（`TtTaskCollector.FOnTaskFinished`
  签名），需要 cast 回 `TtTask<T>` 才能拿结果：`(TtTask<T>)task`。
- cast 后**用 `DirectResult` 读结果，不要用 `GetResultAndRelease`**：因为 `TtTaskCollector.Tick`
  在回调返回后会自动 `Dispose`，回调里再 `GetResultAndRelease`（内部也调 Dispose）会**双重释放**
  `TtTaskData`，把对象池搞乱。`DirectResult` 只读字段不释放，是回调里的正确选择。
- 异常处理：如果异步任务可能抛异常，回调里要先判 `task.Exception`，再决定读 `DirectResult`
  还是走异常分支（直接读 `DirectResult` 在 Failed 状态会重抛异常）。

**反例**：

```csharp
// ❌ 反例 1: 用 polling 代替 semaphore
public async TtTask<bool> EnqueueAsync(...)
{
    var item = new FPendingItem { ... };
    Enqueue(item);
    while (!item.IsDone)
    {
        await TtEngine.Instance.EventPoster.Post((s) => true, EAsyncTarget.AsyncEditor);
        // 每帧都要 yield + 重新调度, 唤醒延迟最差到 1 帧, 而且 fiber 调度负载高
    }
    return item.Result;
}
```

```csharp
// ❌ 反例 2: 在 AddWaitTask 回调里调 GetResultAndRelease
item.Meta.AutoGenSnapshot().AddWaitTask((task) =>
{
    item.Result = ((TtTask<bool>)task).GetResultAndRelease();  // 双重 Dispose
    item.FinishedSemaphore.Release();
});
// TtTaskCollector.Tick 在回调返回后还会 Dispose 一次, TtTaskData 错误归还两次
// 池里出现重复对象, 后续 QueryObjectSync 拿到同一对象多次, 数据互相覆盖
```

```csharp
// ❌ 反例 3: 忘记 FreeSemaphore
await item.FinishedSemaphore.Await();
return item.Result;  // FinishedSemaphore 没回收, PostEvent / Waiter 引用残留
```

**总结**：

- 异步上下文（方法签名是 `async TtTask`）→ **`await`**
- 同步上下文 + fire-and-forget → **`.AddWaitTask([cb])`**
- 同步上下文 + 必须当场拿结果 / 跑完 → **`.GetResultUntilCompleted()` / `.WaitCompletedAndDispose()`**
- 同步生产 + 异步消费（调度器 / 排队器）→ **`TtSemaphore` + `AddWaitTask` 回调里 `Release`**

**已有合规实现的参考位置**：

- `CSharpCode/Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` — `EnableHWRT` / `EnableSkyCube` setter 中的
  `UpdatePermutation().AddWaitTask()`
- `CSharpCode/Grapics/Pipeline/Common/Post/DenoiseNode.cs` — 同上模式
- `CSharpCode/Editor/Snapshot.cs` — `TtSnapshotGenQueue` 的"同步 worker + AddWaitTask 桥接 TtSemaphore"
  的完整模板（重点参考 `EnqueueAutoGen` + `WorkerLoop`）
- `CSharpCode/Base/Thread/Async/Task.cs` — `WaitCompleted` / `WaitCompletedAndDispose` /
  `GetResultUntilCompleted` / `GetResultAndRelease` / `DirectResult` 的源码定义
- `CSharpCode/Base/Thread/EventPoster.cs` — `TtSemaphore` (`CreateSemaphore` / `Await` /
  `Release` / `FreeSemaphore`) 的源码定义

---
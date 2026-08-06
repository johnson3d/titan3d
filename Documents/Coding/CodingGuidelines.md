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

#### 1.4.8 Bindless 与 GI 双轨制路线决策 (重点上下文)

**这是本仓库当前阶段把 Bindless 列为引擎重点升级方向的根本原因. 任何接手
ReSTIR / TtAtlas GI / RT 反射 / RT AO 类工作的同学必须先理解这个上下文,
否则会重复设计出"绕开 bindless 的临时方案", 浪费迭代成本.**

##### 1.4.8.1 问题背景: 当前 ReSTIR GI HW RT 路径的根本缺陷

`enginecontent/Shaders/GI/ReSTIR/ReSTIRInitialSampling.compute` 的 HW RT
分支在拿到 ray hit 之后, **没有真正在 hit 点重做 lighting**, 而是把 hit 的
世界坐标投影回当前帧屏幕空间, 采样 `PrevColor` (上一帧已经包含直接光 + 阴影
的 final color) 作为该 hit 点的 outgoing radiance 估计. 这种"屏幕反投影
取色"的方案有 4 个无法回避的失败模式:

1. **Hit 点投影出屏**: 摄像机左侧的墙反射应该来自摄像机右侧的物体, hit 点
   投影到屏幕外, 取不到 PrevColor (当前修法是返回 sky, 但这等于丢了反射).
2. **Hit 点被前景遮挡**: hit 点的屏幕投影位置被另一个更近的物体挡住,
   PrevColor 取到的是遮挡物的颜色, 不是 hit 点真实的颜色 (典型表现: 嘴里的
   红色 emissive 被前牙挡住, ReSTIR 取到牙的白色, 跨帧抖动产生闪烁).
3. **首帧 / 切镜头 / disocclusion 区域**: PrevColor 还没有该位置的有效数据,
   只能 fallback 到 sky 或黑色, ReSTIR reservoir 收敛极慢.
4. **本质上是 1-bounce screen-space lighting**: 取的是 "上一帧屏幕空间已有
   光照", 而不是 "在 hit 点重新做一次 direct lighting". 任何屏幕外的间接光
   贡献都丢了.

业界 (NVIDIA RTXDI / UE5 Lumen) 共同的解法是**不再依赖 PrevColor 反投影,
而是在 hit 点本地直接获取 radiance**. 但具体怎么"在 hit 点本地获取
radiance", 两家走了完全不同的路, 各有适用场景. TitanEngine 经过评估后
选择**双轨制**: 两条路线**共享同一份 bindless 基础设施**, 但服务于不同
硬件档位和不同画质目标.

##### 1.4.8.2 双轨制路线决策 (2026-05 拍板)

**轨道 A: ReSTIR Hybrid GI — 移动 / 低端 fallback 路线**

- **定位**: 高端移动 GPU (Adreno 740+ / Mali-G715+ / Apple A17 Pro 起步) +
  低端 PC GPU 的 GI 兜底方案.
- **核心思路 (NVIDIA RTXDI 风格)**: 投 BRDF ray, hit 点直接用**简化 PBR 4
  字段**(diffuseAlbedo / specularF0 / roughness / emissive) 重建 lighting,
  结果存进 ReSTIR GI Reservoir, 走时空 reuse + spatial filter 降噪.
- **硬约束 (这是 RTXDI 设计就锁定的, 不是实现妥协)**:
  - hit 点的材质模型只支持标准 metallic-roughness PBR 4 字段
  - 多 ShadingModel (cloth / hair / eye / SSS / clearcoat) 的 lobe 在
    secondary hit 上**会被退化为标准 PBR**, 用户在 GI 里看不到这些 lobe
    的特殊响应
  - 这是该路线的本质限制, 不是 bug, 不要试图"在 ReSTIR 路径里支持
    cloth/hair", 要支持就走轨道 B
- **不需要离线烘焙**, 安装包零增长, 适合移动端 "开机即用".
- **运行时显存占用固定** (主要是 reservoir buffer = 屏幕大小 × 2),
  对移动端友好.

**轨道 B: TtAtlas GI — PC / 高端主机主力路线**

- **定位**: PC + 高端主机的全功能 GI, **完整保留 MaterialGraph 表达力 +
  支持任意数量的自定义 ShadingModel**.
- **核心思路 (UE5 Lumen 风格)**: 离线/编辑期把每个 mesh 的表面投影到
  Card Atlas (texture atlas), 在编辑期或低频更新时跑完整的 MaterialGraph
  把材质烘焙进 atlas. RT hit 后只需要查 atlas 拿 lighting 结果, **不需要
  在 hit shader 里重新跑 MaterialGraph**.
- **优势**:
  - 完整的 MaterialGraph 表达力 (用户在 graph 里写多复杂都行,
    cost 在烘焙期付掉, 不在 RT runtime 付)
  - 支持任意数量的 ShadingModel (cloth / hair / eye / SSS / 未来的
    任何新 lobe), 因为 atlas 里存的就是各 lobe 已经求过的最终响应
  - 没有 PSO 爆炸问题 (只有一份 hit shader, 永远是 "查 atlas")
- **代价**:
  - Card 生成需要离线/编辑期 pass (类似 Lightmap UV unwrap)
  - Atlas 显存占用大 (几百 MB 量级), 移动端撑不住, 这是为什么轨道 A
    必须独立存在
  - 动态/形变 mesh 的 atlas 更新有延迟 (Lumen 的已知限制)
- **完整设计文档**: 见 `Documents/Architecture/AtlasGI.md` (TtAtlas GI
  系统蓝图, 子系统拆分, 与现有 RenderGraphNode / MaterialGraph 的接入点,
  实施分阶段计划). **接手 Atlas GI 工作前必须先读这份文档**, 本规范
  §1.4.8 只负责说明它和 Bindless 的依赖关系.

**两条路线为什么共享 bindless 基础设施**:

- 轨道 A 需要 bindless 来按 hit instance 索引材质纹理 (重建 PBR 4 字段)
- 轨道 B 需要 bindless 来按 mesh card ID 索引 Atlas 切片 + 索引
  per-instance 数据
- 两边都需要 §1.4.8.3 描述的 `TtSceneMaterialTable` / `TtSceneMeshTable`
  这套设施, 写一份给两边用

##### 1.4.8.3 共享基础设施: 场景级 Bindless 表

**Phase A** (已交付): C# 端 `TtBindless` wrapper + 本规范 §1.4 完整建立,
让任何后续工作有合规可依.

**Phase B** (轨道 A 和轨道 B 都依赖, 必须先做):

- 场景级 `TtSceneMaterialTable` — 单例的 `TtBindless` 实例池 (按
  SBT_SRV / SBT_Sampler 拆表), 维护 MaterialID -> bindless slot 的
  映射, 跨帧复用.
- `TtSceneMeshTable` — 同上但管 vertex/index buffer 的 SRV (供
  ray hit 重建几何属性), 可能合并到 MaterialTable 也可能拆开.
- 材质资产加载/卸载 hook 同步维护 bindless 表 (§1.4.3 的 lifetime
  反向锁约束).
- **设计建议**: 表的接口要做成 ShadingModel-aware (按 ShadingModel
  分表), 这样轨道 B 之后引入 cloth/hair/eye 时不影响轨道 A 的 PBR 表.

**Phase B 完成后, 两条轨道并行推进**:

- **轨道 A (ReSTIR HW RT 改造)**:
  - 改造 `ReSTIRInitialSampling.compute` 的 HW RT 分支, 用 hit 点真实
    lighting 替代 PrevColor 屏幕反投影.
  - hit 点的材质求值固定走 PBR 4 字段简化模型 (即使 MaterialGraph 配置了
    cloth/hair, 也按 base PBR 退化求值).
  - 同时受益: RT 反射、RT AO (其实只需要 visibility, 不必 bindless, 但
    顺便走通)、移动端 path tracing 参考实现.

- **轨道 B (TtAtlas GI)**:
  - 严格按 `Documents/Architecture/AtlasGI.md` 的子系统拆分推进.
  - MaterialGraph codegen 改造: 在生成 `DO_PS_MATERIAL_IMPL` 之外, 增加
    `DO_CARD_CAPTURE_IMPL` 路径, 用于 Card Atlas 烘焙.
  - 共享 Phase B 的 bindless 表, 按 Card ID 索引 Atlas 切片.

##### 1.4.8.4 接手任何 GI 工作的同学必读

- 必须先把 §1.4 全文读完 (§1.4.1 ~ §1.4.9).
- 必须先理解 §1.4.8.2 的双轨制定位, **不要把 ReSTIR 当主力 GI 改造,
  也不要在 ReSTIR 路径里硬塞多 ShadingModel 支持** —— 那是轨道 B 的活.
- 接 TtAtlas GI 工作前, 额外必读 `Documents/Architecture/AtlasGI.md`.
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

### 1.5 世界坐标传入渲染前必须转换为局域坐标 (ToLocalPosition)

**适用场景**: 任何从 `Placement.AbsTransform.Position` 取出的世界坐标, 在传入 GPU (写入 cbuffer / structured buffer / 任何 shader 可见的数据) 之前.

**强制规则**:

所有从 `Placement` 获取的 `DVector3` 位置, 在传入渲染管线之前, **必须** 通过 `pos.ToLocalPosition(world.CameraOffset)` 转换为局域坐标. 引擎使用相机偏移的局域坐标系 (camera-relative rendering) 来避免大世界场景下 float32 精度不足的问题.

**正确写法**:

```csharp
var pos = node.Placement.AbsTransform.Position;
var localPos = pos.ToLocalPosition(world.CameraOffset);
light.PositionAndRadius = new Vector4(localPos.ToSingleVector3(), radius);
```

**错误写法** (直接用 AbsTransform.Position 转 float3):

```csharp
// ❌ 错误！大世界下 float32 精度丢失, 灯光/物体位置偏移
var pos = node.Placement.AbsTransform.Position;
light.PositionAndRadius = new Vector4(pos.ToSingleVector3(), radius);
```

**Why**: 引擎的世界坐标使用 `DVector3` (double 精度), GPU 只支持 float32. 如果不做相机偏移, 当相机远离原点时 (例如坐标 > 10000), float32 的精度不足会导致灯光、阴影、物体位置出现明显的抖动和偏移.

---

### 1.6 GPU Buffer 创建/上传/回读规范

**适用场景**: 任何 compute / graphics drawcall 需要的 structured buffer / raw buffer
(传 SRV / UAV / CBV) 的创建、上传 CPU 数据、回读 GPU 数据.

**强制规则**:

1. **不要手写 `FBufferDesc + CreateBuffer + CreateUAV + CreateSRV` 这一长串**, 引擎已经
   提供了两个高级封装, 优先使用; 自己手写很容易漏掉 `StructureStride` / `isRaw` /
   `Usage` / `CpuAccess` 之一, 导致 GPU 报 `DXGI_ERROR_INVALID_CALL` 或 binder 类型
   不匹配.
2. **CPU 数据需要持续累积/部分修改后上传** (典型: 每帧增删的对象列表, GPU-driven
   场景的 instance buffer): 用 `Graphics.Pipeline.TtCpu2GpuBuffer<T>`.
3. **GPU 端持有, 偶尔重建** (典型: 一次性烘焙好的 BVH/SDF/voxel grid, 无需保留
   CPU 镜像): 用 `Graphics.Pipeline.TtGpuBuffer<T>`.
4. **GPU→CPU 回读**: 用 `TtBuffer.FetchGpuData(uint subRes, IBlobObject blob)`,
   不要自己写 staging buffer + CopyResource + Map/Unmap.

#### 1.5.1 TtCpu2GpuBuffer<T> —— CPU 累积型

`Graphics/Pipeline/GpuBuffer.cs:1-160` 完整实现. 核心特性:

- 内部维护 `Support.TtNativeArray<T> DataArray` 作为 CPU 镜像, 公开
  `PushData / UpdateData / SetSize / Clear` 接口
- 第一次 `Flush2GPU` 时按容量创建 `GpuBuffer + Uav + Srv + Cbv` (按
  `Initialize(BufferTypes)` 传入的 `EBufferType` 决定要创建哪几个 view)
- **容量自动 1.5x 增长**: 当 `DataArray.Count >= GpuCapacity` 时整体 dispose 重建,
  否则走 `UpdateGpuData` 增量更新, 不重建 buffer
- **Usage 自动选择**: 含 `BFT_UAV` 时用 `USAGE_DEFAULT`; 否则用
  `USAGE_DYNAMIC + CAS_WRITE` (CPU 频繁更新的优化路径)
- **isRaw 自动判定**: `T` 是 `int/uint/float` 时按 raw buffer 创建, 其他按
  structured buffer

**标准模板**:

```csharp
TtCpu2GpuBuffer<MyStruct> mBuffer = new TtCpu2GpuBuffer<MyStruct>();
mBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);              // ① 一次性
mBuffer.SetSize(initialCount);                              // ② 预分配

// 累积/更新数据 (任何线程, 不触发 GPU 操作)
for (int i = 0; i < n; i++) mBuffer.UpdateData(i, in items[i]);

// 在合适的 cmd list 上 flush (典型: Tick 开头, 渲染前)
mBuffer.Flush2GPU(cmd);                                     // ③ 增量更新或重建

// drawcall 用 mBuffer.Srv / Uav / Cbv 绑定 (在 OnDrawCall 里, §1.2)
drawcall.BindSrv("MyBuffer", mBuffer.Srv);
```

**反例**:

```csharp
// ❌ 自己 new TtBuffer + new SRV/UAV, 容量管理/usage 选择/isRaw 判定全部要自己写
var bfDesc = new FBufferDesc();
bfDesc.SetDefault(false, EBufferType.BFT_SRV);
bfDesc.Size = ...; bfDesc.StructureStride = ...; bfDesc.InitData = ...;
var buf = rc.CreateBuffer(in bfDesc);
var srv = rc.CreateSRV(buf, in srvDesc);
// ↑ 一旦数据 size 变了又得整体重写, 用 TtCpu2GpuBuffer<T> 一行 SetSize 搞定
```

**实战参考**: `Bricks/AdvanceShadow/QTileTree.cs:101-218` —— `FAdvShadowNodeData`
配合 `TtCpu2GpuBuffer<T>` 的端到端用法 (`Initialize → SetSize → UpdateData →
Flush2GPU`).

#### 1.5.2 TtGpuBuffer<T> —— GPU 常驻型

`Graphics/Pipeline/GpuBuffer.cs:206+` 完整实现. 核心特性:

- 没有 CPU 镜像, 只有 `GpuResource + Uav + Srv + Cbv + Rtv + Dsv`
- `SetSize(count, pInitData, bufferType)` 一把全部创建; `pInitData` 传 null 表示
  GPU-only (典型: ping-pong 的 reservoir buffer)
- 没有 `Flush2GPU` —— 重新 `SetSize` 等于整体重建
- 同样支持 `SetTexture2D` 一行创建 2D 纹理 (含 UAV/SRV)

**适用场景**: 一次烘焙好就长期持有的 GPU 数据 (BVH 节点表 / SDF 体素 / voxel
grid), 或 ping-pong 的 GPU-only reservoir.

**反例**:

```csharp
// ❌ 不要拿 TtGpuBuffer<T> 当 "每帧追加" buffer 用 —— 它没有 Append/Resize 概念,
//    每次 SetSize 都整体重建. 这种场景换 TtCpu2GpuBuffer<T>.
gpuBuf.SetSize(newCount, ...);  // 每帧都做这个 = 每帧整体重建 GPU 资源
```

**实战参考**: `Grapics/Pipeline/GI/ReSTIR/ReSTIRGINode.cs` —— `mReservoirBuffers`
是 GPU-only ping-pong, 用 `TtGpuBuffer<FReSTIRPackedReservoir>` + `SetSize(count,
IntPtr.Zero.ToPointer(), ...)` 创建; `Bricks/Collision/BVH/TtGpuBvh.cs` ——
BVH 节点表一次烘焙长期持有.

#### 1.5.3 TtBuffer.FetchGpuData —— GPU→CPU 回读

`NxRHI/Buffer.cs:150` (`TtBuffer`), `:241` (`TtTexture`) 完整实现. 签名:

```csharp
public bool FetchGpuData(uint index, EngineNS.IBlobObject blob);
```

**强制规则**:

1. **不要自己创建 staging buffer + CopyResource + Map/Unmap**. `FetchGpuData`
   内部自动判断目标 buffer 的 `GpuAccess` 状态, 必要时 copy 到 cpu-readable
   staging buffer 并 flush queue, 调用方拿到的 `IBlobObject` 已是 cpu 可读.
2. **同步阻塞**: 调用返回时 GPU 写入已完成, 数据已经在 `blob.DataPointer` 里.
   不需要 fence wait.
3. 调用方负责用 `using (var blob = new Support.TtBlobObject())` 管理 blob 生命
   周期, 用 `blob.Size` / `blob.DataPointer` 读出原始字节, 自己 cast 成目标
   struct 数组.
4. **代价不低**: 内部可能涉及 staging copy + queue flush, **不要在主渲染循环里
   每帧调用**. 用于 debug / 离线对照 / 烘焙 readback.
5. **⚠️ blob 头部有 8 字节 pitch 头, 必须跳过才是真数据**. `IBuffer::FetchGpuData`
   (native, NxRHI/Buffer.cpp) 实现里固定先 PushData 两个 `UINT` 再 push buffer
   raw bytes:

   ```cpp
   // native 实现, 字面照搬
   blob->PushData(&subRes.RowPitch,   sizeof(UINT));   // [0..4)
   blob->PushData(&subRes.DepthPitch, sizeof(UINT));   // [4..8)
   blob->PushData(subRes.pData, this->Desc.Size);      // [8..)  ← 真正的 buffer 数据
   ```

   所以 `blob.Size == 8 + Desc.Size`, **真数据从 offset 8 开始**, C# 端读取必须:

   ```csharp
   const uint kBufferReadbackHeader = sizeof(uint) * 2;  // 8
   var pSrc = (byte*)blob.DataPointer + kBufferReadbackHeader;
   ```

   反例: 直接 `MemoryCopy(blob.DataPointer, pDst, count*sizeof(T), ...)` —— 前
   8 字节 pitch 当 `T[0]` 的前 8 字节读, 整个数组错位 8B, 最后一个元素缺尾巴
   8B 没读到。本仓库 `TtGpuBvh.ReadbackHits` 踩过这个坑 (FGpuHit 16B → 错位
   半个 hit, 表现是"hit 列表整体平移、最后一个 hit 全是垃圾值")。

   **`ITexture::FetchGpuData` 头部布局相同** (也是 `RowPitch + DepthPitch + raw
   bytes`), 但 texture 路径**必须用 `RowPitch` 做按行 stride 走指针**, 不能假设
   `RowPitch == width * sizeof(pixel)` —— GPU 端为 cache line 对齐 (常见 256B)
   通常会把 RowPitch padding 到比 `width * sizeof(pixel)` 大。下面是
   `Bricks/Procedure/Node/Algorithm/GpuErosionNode.cs:46-65` 的真实代码 (R32_FLOAT
   单通道纹理 → CPU `Output` buffer):

   ```csharp
   readTexture.FetchGpuData(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject,
                            0, blob.mCoreObject);
   using (var reader = IO.TtMemReader.CreateInstance(
                           (byte*)blob.DataPointer, blob.Size))
   {
       uint rowPitch, depthPitch;
       reader.Read(out rowPitch);                            // [0..4)
       reader.Read(out depthPitch);                          // [4..8)
       // 注意: 原版 GpuErosionNode.cs:53 在这两行之后又写了一行
       //   rowPitch = (uint)(sizeof(float) * Output.Width);
       // 把 native 返回的 rowPitch 直接覆盖掉。该行没有注释说明意图, 不清楚是
       // 历史遗留、特定 staging texture 的优化, 还是潜在 bug。**新代码请直接信任
       // native 返回的 rowPitch (它代表真实 stride, 含 GPU cache line padding),
       // 不要照抄这一行覆盖**。
       var pImage = (byte*)blob.DataPointer + reader.GetPosition();   // [8..)
       for (int y = 0; y < Output.Height; y++)
       {
           for (int x = 0; x < Output.Width; x++)
           {
               Output.SetFloat1(x, y, 0, ((float*)pImage)[x]);   // 一行内连续读
           }
           pImage += rowPitch;                               // ← 关键: 跨行用 rowPitch
       }
   }
   ```

   buffer 路径就简单 —— 用户原贴的 native 实现:
   `blob->PushData(subRes.pData, this->Desc.Size);` 写的是 buffer 的逻辑大小
   `Desc.Size`, **不是** `RowPitch * something`, 所以 buffer 没有"按行跨指针"的
   概念, 直接 `(byte*)blob.DataPointer + 8` 开始连续读 `Desc.Size` 字节即可。
   头部那 8 字节的 `RowPitch` / `DepthPitch` 对 buffer 没意义 (native 端塞进去
   是为了让 buffer / texture 两条路径的 blob 头格式保持一致, 方便复用同一套
   `IO.TtMemReader` 解析逻辑)。

**标准模板**:

```csharp
using (var blob = new Support.TtBlobObject())
{
    bool ok = mBuffer.GpuBuffer.FetchGpuData(0, blob.mCoreObject);
    if (!ok) return;

    // 跳过 IBuffer::FetchGpuData 写在 blob 头部的 8 字节 (RowPitch + DepthPitch)
    const uint kBufferReadbackHeader = sizeof(uint) * 2;

    uint expected = (uint)sizeof(MyStruct) * elementCount;
    if (blob.Size < kBufferReadbackHeader + expected) { /* 报错 */ return; }

    var hits = new MyStruct[elementCount];
    fixed (MyStruct* pDst = hits)
    {
        var pSrc = (byte*)blob.DataPointer + kBufferReadbackHeader;
        System.Buffer.MemoryCopy(pSrc, pDst, expected, expected);
    }
    // 用 hits ...
}
```

> **Texture readback 标准模板**: 直接复用上面规则 5 内嵌的 `GpuErosionNode.cs`
> 完整代码 (用 `IO.TtMemReader` 解析 8B 头 + 用返回的 `rowPitch` 走行 stride)。
> 这里不重复贴, 因为 buffer / texture 两条路径只在"如何使用读出来的字节"这一步
> 不同, blob 头部 + skip 8B 的部分完全相同。

**实战参考**:
- `Bricks/GpuDriven/Cluster.cs:509` —— Buffer 路径: 烘焙后 readback triangles
- `Bricks/Procedure/Node/GpuNode/Height2FlowMap.cs:47` —— Texture 路径的同款用法
- `Bricks/Procedure/Node/Algorithm/GpuErosionNode.cs:46-65` —— Texture 路径完整范例
  (含 `IO.TtMemReader` 解析 8B 头 + 按 `rowPitch` 走 stride 的真实代码)
- `Bricks/Collision/BVH/TtGpuBvh.cs` —— `ReadbackHits` 把 BVH dispatch 结果拉回
  CPU 与 `TtDynamicBVH.RayCast` 对照 (debug-only)

##### 1.5.3.1 AsyncFetchGpuData —— 非阻塞回读 (推荐用于非 debug 场景)

`NxRHI/Buffer.cs` 给 `TtBuffer` / `TtTexture` 都提供了对称的异步版本:

```csharp
public async Thread.Async.TtTask<bool> AsyncFetchGpuData(uint subRes, IBlobObject blob);
```

**何时用异步版**:

- **生产路径上的 readback** (例如 GPU pick / culling 结果反馈 / 烘焙流水线)
  必须用 async, 否则会阻塞调用线程 (UI 线程 / 编辑器主循环) 等几毫秒~几十毫秒,
  造成卡顿。
- **debug 一次性回读** (例如 BVH GpuRayCast 按钮对照 CPU 结果) 可以继续用同步版,
  反正调用方就是要等结果, await 链反而把代码搞复杂。

**强制规则**:

1. **绝对不能写"`RenderQueue.QueueCmd` 里同步 `FetchGpuData` + `Release`"**。`FetchGpuData`
   内部含 `fence.Wait`, 把它丢进 `QueueCmd` 等于让渲染线程同步等 GPU, 会**吞一帧渲染**。
   本仓库历史踩过这个坑, 引擎层封装的标准做法是 fence wait 必须在引擎 TPools 工作线程跑
   (见 §1.6 关于 `EventPoster.RunOn` 与 `EAsyncTarget.TPools` 的说明)。
2. **路径**: 调用线程 `CreateReadable` (record copy 到 staging) →
   `EventPoster.RunOn(callback, EAsyncTarget.TPools)` 投递到 TPools 工作线程:
   `FTransientCmd` 提交 cpDraw + `IncreaseSignal(fence)` + `fence.Wait(1)` (阻塞
   该 TPools 工作线程而非渲染/调用线程) + 从 staging buffer `FetchGpuData` 到 blob +
   `semaphore.Release()` → 调用线程 `await sem.Await()` 非阻塞挂起后唤醒。
3. **必须用 `EAsyncTarget.TPools`**, 不能用 `AsyncIO`/`Logic`/`Render` 这些**单专线**目标。
   单专线上的 fence wait 会阻塞该专线后排队的其他任务; TPools 是真正的多线程池,
   任务之间无依赖、可并行 (见 §1.6 选型表)。
4. **资源生命周期**: cpDraw / fence 是 C# `AuxPtrType`, 用 `Dispose`; `readable` 是
   native `IBuffer`, 用 `Release()` 不是 `Dispose`。`semaphore.FreeSemaphore()` 也
   要在 await 之后调一次。引擎封装的 `AsyncFetchGpuData` 已经把这些处理好, 调用方
   只管 `using` 自己的 blob。
5. **异常安全**: `EventPoster.RunOn` 回调里**任何异常**都必须走 `finally { sem.Release(); }`,
   否则 `await` 端永远挂起 (引擎封装已实现)。

**标准模板** (调用方视角, 参考 `Bricks/Collision/BVH/TtGpuBvh.cs::ReadbackHitsAsync`):

```csharp
public async Thread.Async.TtTask<(bool ok, MyStruct[] data)> ReadbackAsync()
{
    using (var blob = new Support.TtBlobObject())
    {
        bool ok = await mBuffer.GpuBuffer.AsyncFetchGpuData(0, blob.mCoreObject);
        if (!ok) return (false, null);

        unsafe
        {
            // 同同步版: 跳过 IBuffer::FetchGpuData 写在 blob 头部的 8 字节 (RowPitch + DepthPitch)
            const uint kBufferReadbackHeader = sizeof(uint) * 2;

            uint expected = (uint)sizeof(MyStruct) * elementCount;
            if (blob.Size < kBufferReadbackHeader + expected) return (false, null);

            var data = new MyStruct[elementCount];
            fixed (MyStruct* pDst = data)
            {
                var pSrc = (byte*)blob.DataPointer + kBufferReadbackHeader;
                System.Buffer.MemoryCopy(pSrc, pDst, expected, expected);
            }
            return (true, data);
        }
    }
}
```

> **Texture async readback**: 框架完全相同 —— 把上面 buffer 模板里的
> `mBuffer.GpuBuffer.AsyncFetchGpuData(...)` 换成 `mTexture.GpuTexture.AsyncFetchGpuData(...)`,
> 然后**读出来的字节按 §1.5.3 规则 5 的 texture 路径处理**(用 `IO.TtMemReader`
> 解析 8B 头 + 按返回的 rowPitch 走行 stride, 不要假设 RowPitch == width *
> sizeof(pixel))。这里不再重复贴 texture 完整代码, 见规则 5 的 `GpuErosionNode.cs`
> 范例。

**实战参考**:
- `Bricks/Collision/BVH/TtGpuBvh.cs::ReadbackHitsAsync` —— 完整 async readback 范例
- `Editor/Snapshot.cs:284 Texture2MemImage` —— 异步版底层路径的"原型", 异步封装即把
  这套 fence + transient cmd 模式从渲染线程挪到 ThreadPool 的产物
- `Editor/Snapshot.cs:175 EnqueueAutoGen` —— `TtSemaphore.Await` 在 async 链路上的范例

#### 1.5.4 选型决策表

| 场景 | 推荐封装 | 理由 |
|---|---|---|
| 每帧 CPU 累积 → 上传 GPU (instance/灯光列表/物体属性表) | `TtCpu2GpuBuffer<T>` | DataArray 镜像 + 增量 UpdateGpuData + 自动扩容 |
| 一次烘焙长期持有 (BVH/SDF/voxel) | `TtGpuBuffer<T>` | GPU-only, 无 CPU 镜像浪费 |
| GPU-only ping-pong (reservoir/history) | `TtGpuBuffer<T>` 配 `pInitData=null` | 同上 |
| 临时 vertex/index buffer (debug 线条/aabb) | `TtTransientBuffer` | 跨帧不持有, 帧末释放 |
| GPU→CPU 回读 (debug/烘焙) | `TtBuffer.FetchGpuData` | 自动 staging+flush |
| Constant buffer (per-pass 参数) | `rc.CreateCBV(binder)` + 见 §1.1 | 走 CBV 专用路径, 不用上面四个 |

### 1.6 异步任务投递 —— 必须使用 EventPoster, 禁止直接用 .NET ThreadPool

**适用场景**: 任何需要把工作丢到非调用线程上跑的场景 —— 资产加载、IO、烘焙、物理仿真、
GPU readback 的 fence wait、并行 culling、长耗时计算等。

**强制规则**:

1. **所有异步投递必须走 `TtEngine.Instance.EventPoster`**, 不要直接用
   `System.Threading.ThreadPool.QueueUserWorkItem` / `Task.Run` / 自建 `Thread`。
   引擎线程模型由 `TtContextThreadManager` 统一管理 (`CSharpCode/Base/Thread/Async/
   ContextThreadManager.cs`), 每个 `EAsyncTarget` 都有专属上下文 + stack size 配置 +
   profiler hook + frame end 回流, 绕开它会丢失这些设施而且容易死锁。
2. **必须显式选 `EAsyncTarget`**, 默认值 `AsyncIO` 只适合 IO 类任务, 不要图省事全用
   默认。选错 target (例如把 fence wait 放 `AsyncIO`) 会阻塞专线上排队的真正 IO 任务。
3. **`TPools` 上跑的任务之间不能有依赖**, 因为线程池有多条线程并行取任务, 顺序不保证。
   有依赖关系的串行链应该用 `Logic` / `AsyncIO` 单专线。
4. **`RunOn` 是 fire-and-forget**, 不返回 awaitable; 需要等结果用 `Post` (返回
   `FTaskAwaiter<T>` 可 `await`) 或配 `TtSemaphore.Await()` (见 §1.5.3.1 范例)。
5. **传参用 `userArgs` + `static lambda`**, 不要用 closure。closure 会捕获外层变量
   分配额外对象; `static lambda + userArgs` 让回调零分配。

#### 1.6.1 核心 API 速查

| API | 签名 / 用途 | 何时用 |
|---|---|---|
| `RunOn<T>(evt, target=AsyncIO, userArgs=null, completedEvent=null)` | fire-and-forget, 不返回 awaitable | 投递不需要 await 结果的任务; 配 `TtSemaphore.Await()` 实现 await 语义 |
| `Post<T>(evt, target=AsyncIO)` → `FTaskAwaiter<T>` | 可 `await`, 完成后**自动调度回调用线程** | 需要拿返回值且后续代码要回到原线程 |
| `PostTask(target, TtAsyncTaskState<bool>)` | 投递已构造好的 task 实例 | 复杂场景, 自己控制 task lifecycle |
| `AwaitSemaphore(smp)` | 内部由 `TtSemaphore.Await()` 调用 | 不要直接调, 用 `await sem.Await()` |

#### 1.6.2 EAsyncTarget 选型表

定义见 `Base/Thread/Async/ContextThreadManager.cs:29`。每个 target 都有独立 stack size
配置 (`TtThreadConfig`)。

### 1.7 GPU 命令提交必须走 RenderQueue, 禁用 `RenderContext.GpuQueue` 直接 submit

**适用场景**: 所有需要把 `TtCommandList` / `FRenderCmd` 交给 GPU 执行的位置 ——
RenderGraph 节点 Tick、editor cmd 按钮、async readback 投递、热重载 / Tools / 烘焙
脚本里的 GPU 操作等等。

**强制规则**:

1. **`TtCommandList` 级别提交必须走** `TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmd, name, qType)`
   或 `policy.CommitCommandList(cmd, name, qType)`。
2. **`FRenderCmd` 单条 cmd 提交必须走** `TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd(cmd, name, tag, qType, type, bImm)`
   或 `policy.QueueCmd(cmd, name, tag, qType, type, bImm)`。其中 `type` (`ERCmdType`,
   默认 `Cmd`) 与 `bImm` (默认 `false`) 是可选参, 绝大多数场景保持默认即可; 仅在
   "逻辑线程帧末哨兵 (`FrameEnd`)" 或 "启动期 / shutdown flush 等需要立即同步执行
   (`bImm: true`)" 的少数场景才显式传入, 详见 `CodeLib.md §12.2 / §12.4`。
3. **绝对禁止** 直接调用 `TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(...)`
   / `RenderContext.GpuQueue.QueueCmdlist(...)` 等任何**底层 `IGpuQueue`** 上的提交 API。
   `RenderContext.GpuQueue` 是 native 层 `IGpuQueue` 的直接暴露, 仅供引擎内部 (CmdQueue
   / RenderQueue) 调用, 业务代码不该直接接触。
4. `IncreaseSignal` / `WaitToSignal` 等 fence 操作仍然在 `RenderContext.GpuQueue` 上, 这些
   不是 cmdlist 提交, 不在禁用范围 (例如 `NxRHI/Buffer.cs` 的 `AsyncFetchGpuData` 仍然
   显式调 `rc.GpuQueue.IncreaseSignal(fence)`, 这是合规的)。

**为什么必须遵守**:

直接调 `GpuQueue.ExecuteCommandList` 看起来"更直接、少一层", 实际绕开了引擎的三套关键设施:

- **CmdQueue 归并**: 引擎的 `RenderQueue` 内部维护 transient cmdlist 池, 把同一 tick
  里所有 `QueueCmdlist` 进来的 cmdlist 合到一条 (或少数几条) cmdlist 后再 submit, 大幅
  降低 native submit 调用频率 (尤其在 D3D12 / Vulkan 这种 submit 开销显著的 RHI 上)。
  绕开后每条 cmd 走自己的 submit, 同等帧 native API 调用数能涨 5-10x。
- **Profiler / RenderDoc 标签链**: `name` / `tag` 参数走 `RenderQueue` 才会被引擎的
  cpu profiler 接 + 注入到 RenderDoc 抓帧的 event marker。直接走 `GpuQueue` 时这两个
  参数即使传了也只是被 native 层忽略, RenderDoc 抓帧后 event 树里完全找不到归属。
- **policy 时序保证**: `RenderQueue` 的 flush 由引擎在合适的时机 (帧末 / RenderGraph
  完成回调) 触发, 保证和 RenderGraph 主流的提交顺序一致。直接走 `GpuQueue` 是同步立即
  submit, 顺序和 RenderGraph 任意交错 —— 表现是 "有时正常、有时画面缺一块" 这类难复现
  bug, 抓帧才能看出来 cmd 顺序乱。

**正例**:

```csharp
// cmdlist 级 (例如 compute dispatch)
mShading.SetDrawcallDispatch(this, null, mDrawcall, gx, 1, 1, true);
mCmdList.PushGpuDraw(mDrawcall);
mCmdList.FlushDraws();
TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(
    mCmdList, "GpuBvh.RayCast", EQueueType.QU_Compute);   // ✓ 走 RenderQueue

// 单条 cmd 级 (例如 readback copy), type/bImm 使用默认值即可
TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd(
    cpDraw, "MyReadback.Copy", null, EQueueType.QU_Default);  // ✓ 走 RenderQueue

// 少数场景: 逻辑线程帧末哨兵, 让渲染线程 TickRender 消费到 FrameEnd 后 break
TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd(
    static (queue, ref info) => { }, "#TickLogicEnd#", null,
    EQueueType.QU_Default, ERCmdType.FrameEnd);               // ✓ 仅用于帧末哨兵

// 少数场景: 启动期 / fence signal 之后需要立即同步执行, 不能等下一帧
TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd(
    cmd, "GpuFetch.FenceSignal", null,
    EQueueType.QU_Compute, ERCmdType.Cmd, bImm: true);        // ✓ 当前线程立即执行

// 在 RenderGraph 节点里更优先用 policy 路径
policy.CommitCommandList(mCmdList, "MyNode.Dispatch");  // ✓ 走 policy (CmdQueue 归并)

// fence signal/wait 仍在 GpuQueue, 这是合规的 (不是 cmdlist 提交)
rc.GpuQueue.IncreaseSignal(fence);                        // ✓ fence 操作不在禁用范围
fence.Wait(1);
```

**反例 (本仓库已知违规, 摘自 `Bricks/Procedure/Node/GpuShading/GpuFetch.cs:51-52`)**:

```csharp
// ❌ 绕开 RenderQueue 直接 submit, 失去 CmdQueue 归并 + Profiler hook + 时序保证
//    mCmdList / mFinishFence 是 GpuFetch 节点的成员字段, 此处片段保留原文件名引用便于追溯
TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(
    mCmdList, NxRHI.EQueueType.QU_Compute);
TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(
    mFinishFence, NxRHI.EQueueType.QU_Compute);
```

`IncreaseSignal` 本身合规 (fence 操作允许走 GpuQueue), 真正违规的是上面那条
`ExecuteCommandList` —— 应改写为 `TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(
mCmdList, "GpuFetch.Submit", NxRHI.EQueueType.QU_Compute);` 让引擎做归并和时序管理。
新写代码不许跟随这个反例。

**自检清单**:

- [ ] 本次新写的代码里有没有出现 `RenderContext.GpuQueue.ExecuteCommandList` /
      `RenderContext.GpuQueue.QueueCmdlist` 字样? 有的话改成
      `RenderQueue.QueueCmdlist` 或 `policy.CommitCommandList`。
- [ ] 在 RenderGraph 节点上下文里, 有没有可以拿到 `policy` 的位置? 有的话优先用
      `policy.QueueCmd` / `policy.CommitCommandList` 让 CmdQueue 帮你做归并。
- [ ] grep 全仓 `RenderContext\.GpuQueue\.(Execute|Queue)` 看有没有新增违规点 (注意
      正则要包含 `Execute` —— 真实方法名是 `ExecuteCommandList` 全词, 不是
      `ExecuteCmdlist`; 写成 `Cmdlist?` 会漏掉这个最常被误用的 API)。本任务后
      剩余应该只有 `GpuFetch.cs:51` 一处历史残留 (调用 `ExecuteCommandList`)。

**配套 API: `RenderQueue.QueueFence` —— 合规的"在 cmdlist 提交点插 fence"方式**

如果业务确实需要在 `RenderQueue.QueueCmdlist` 提交点之后插入一个**显式 fence
屏障** (例如同步 readback 想确保 GPU 已完成、跨队列同步等), **正确做法是调
`TtEngine.Instance.GfxDevice.RenderQueue.QueueFence(...)`**, 它走 RenderQueue
路径、返回一个可 `Wait` 的 fence 句柄, 时序和归并都被引擎正确管理。

```csharp
// dispatch 走 RenderQueue
TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(
    mCmdList, "GpuBvh.RayCast", EQueueType.QU_Compute);

// 在同一 RenderQueue 末尾插 fence (走合规路径, 不要手写 rc.GpuQueue.IncreaseSignal)
var fence = TtEngine.Instance.GfxDevice.RenderQueue.QueueFence(
    null, "GpuBvh.RayCast", EQueueType.QU_Compute, true);
fence.Wait(1);   // 同步阻塞调用线程; 异步场景请放到 EventPoster.RunOn(TPools) 里 wait

// 现在可以安全 readback (其实 FetchGpuData 自身也带 flush+wait, 这里 fence 只是
// 让"GPU 已完成"这个屏障在代码里显式可见, 便于调试和阅读)
mHitBuffer.GpuBuffer.FetchGpuData(0, blob.mCoreObject);
```

**反例**: 别再写 `rc.GpuQueue.IncreaseSignal(fence) + fence.Wait(1)` —— `IncreaseSignal`
本身合规, 但跟 `RenderQueue.QueueCmdlist` 配对时**它和 cmdlist 不在同一个 submit
点**, fence signal 时机比 cmdlist 实际跑完更早或更晚都有可能, wait 出来的语义
是含糊的。`QueueFence` 是引擎专门为这个场景提供的"绑在 RenderQueue 提交序列上
的 fence"。

**实战参考**: `Bricks/Collision/BVH/TtBVHDebugNode.cs::FGpuRayCast` 在 dispatch
之后用 `RenderQueue.QueueFence + fence.Wait(1)` 同步等 GPU 完成, 然后调
`mGpuBvh.ReadbackHits` 与 CPU 端 BVH 结果做断言对照 (debug-only 场景)。

---

## 3. 命名规范

### 3.1 C# 端 struct 必须 `F` 前缀, enum 必须 `E` 前缀

**适用场景**: 所有在 `CSharpCode/` 下新建的 `struct` / `enum`, 尤其是用作 GPU buffer
元素 / cbuffer 布局 / shader 端类型对照的 POD 结构。

**强制规则**:

1. **`struct` 类型名必须以 `F` 开头** (取自 "Field-only struct" 的 F, 也对应引擎
   现有所有 native struct 的命名风格)。
2. **`enum` 类型名必须以 `E` 开头**。
3. **`class` 类型名仍用 `Tt` 开头** (TitanEngine 类前缀, 现状不变, 例如
   `TtBuffer` / `TtRenderPolicy`)。
4. **C# 端名字与 `[TtShaderDefine(ShaderName = "...")]` 中的 `ShaderName` 必须
   完全一致**, 不允许出现 "C# 叫 `GpuBvhNode`, ShaderName 写 `FGpuBvhNode`" 这种
   两边对不上的情况。让 grep / 跳转能一次找全所有引用。

**为什么必须遵守**:

- **可读性**: 在阅读代码时一眼能看出是值类型还是引用类型, 避免误用
  `default(T)` / `null` 检查 / 装箱拆箱等差异化语义。
- **风格一致性**: 引擎里 native 端 (`NxBuffer.h` 等) + 已有的托管 struct
  (`FShaderBinder` / `FFenceDesc` / `FMeshlet` / `FAdvShadowNodeData` /
  `FSubResourceFootPrint` / `FBuffer_SRV` / `FTextureDesc` / `FSubResourceFootPrint`)
  全部是 `F` 前缀, 新写代码不跟随会显得格格不入, 长期累积导致代码风格分裂。
- **HLSL 联动**: shader 端历史代码也都是 `FXxx` (`FCameraData` / `FMeshlet` 等);
  C# 端 ShaderName 不带前缀会让 HLSL 端引用突兀。

**正例**:

```csharp
// struct: F 前缀, [TtShaderDefine] 与之一致
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGpuBvhNode")]
public struct FGpuBvhNode { ... }

[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FAdvShadowLayerData")]
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct FAdvShadowLayerData { ... }

// enum: E 前缀
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EParticleFlags")]
public enum EParticleFlags : uint { ... }
```

**反例 (本仓库踩过)**:

```csharp
// ❌ 没有 F 前缀, 看代码以为是 class, 实际是 32 byte 的 POD struct
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "GpuBvhNode")]
public struct GpuBvhNode { ... }

// ❌ ShaderName 和 C# 端不一致, grep "FGpuHit" 找不到 C# 定义, grep
// "GpuHit" 又会一并匹到 HLSL 端的 RWStructuredBuffer<FGpuHit> Hits
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGpuHit")]
public struct GpuHit { ... }
```

**自检清单**:

- [ ] 新建 / 改名 struct 时, 名字以 `F` 开头?
- [ ] 新建 / 改名 enum 时, 名字以 `E` 开头?
- [ ] 标 `[TtShaderDefine]` 时, `ShaderName` 与 C# 端**完全一致** (不只是大小写
      一致, 是字符级一致)?
- [ ] 改名后, `Get-ChildItem -Recurse | Select-String -Pattern "\b旧名\b"` 在
      整个仓库 (含 `enginecontent/Shaders/`) 内已无残留?

### 3.2 `[TtShaderDefine]` struct 内的字段必须以 `m` 开头

**适用场景**: 所有标了 `[EngineNS.Editor.ShaderCompiler.TtShaderDefine]` 的 C# `struct`
里**没有字段级 attribute** 的 `public` field。

**强制规则**:

1. **字段名必须以小写 `m` 开头** (例如 `mBoxMin` / `mHitProxyId` / `mShadowMatrix`)。
   引擎在 `CSharpCode/Editor/ShaderCompiler/ShaderCode.cs:384` 处:
   ```csharp
   System.Diagnostics.Debug.Assert(i.Name[0] == 'm');
   codeBuilder.AddLine($"{typeStr} {i.Name.Substring(1)};", ref sourceCode);
   ```
   会断言并**剥掉首字母 `m`** 作为 HLSL 端字段名 (`mBoxMin` → HLSL `BoxMin`)。
   不带 `m` 前缀会触发断言, Debug 构建直接挂; Release 构建虽然不挂, 但 HLSL
   端字段名会变成把首字母也吃掉的形态 (`BoxMin` → `oxMin`), 静默错乱。
2. **HLSL 端字段名仍写"剥 m 后的裸名"** (`BoxMin` / `Origin` / `HitT`), **不要**
   写 `mBoxMin`。这是引擎自动生成的形式, 也是 HLSL 端历史代码的命名风格。
3. **例外: 字段自带 `[TtShaderDefine(ShaderName = "...")]` 时, C# 字段名可任意**。
   引擎走 attribute 分支 (`ShaderCode.cs:323-376`) 用 `attr.ShaderName` 而不
   是字段名, 此时 `m` 前缀检查不生效 —— 适合需要 "C# 端起 property 风格名字、
   HLSL 端用另一个名字" 的场景。范例: `FMeshlet.VertexOffset` /
   `TriangleOffset` (`Bricks/GpuDriven/Cluster.cs:282`)。

**为什么必须遵守**:

- **避免静默错乱**: 不带 `m` 前缀, Release 构建下 `i.Name.Substring(1)` 会把
  字段名第一个字符也削掉 (`Origin` → `rigin`), HLSL 端拿到错位的字段名, 反射
  布局对不上, GPU 读到垃圾数据 —— 而且不会有任何编译/运行时报错, 极难排查。
- **风格统一**: 引擎现有 `[TtShaderDefine]` 标注的 struct 全部用 `m` 前缀
  (`FAdvShadowNodeData.mShadowMatrix` / `mChildIndex00` / `mNodeType` /
  `mZNear`, `FAdvShadowLayerData.mLayerStartAndSide` / `mLayerGridSize` 等),
  新写代码不跟随会显得格格不入。
- **C# vs HLSL 命名风格一致**: C# 端 `m` 前缀符合"private/instance field"风格,
  HLSL 端剥 `m` 后是裸名符合 HLSL 字段命名风格, 两边各自看着都自然。

**正例**:

```csharp
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGpuBvhNode")]
public struct FGpuBvhNode
{
    public Vector3 mBoxMin;          // -> HLSL: BoxMin
    public uint    mChild1;          // -> HLSL: Child1
    public Vector3 mBoxMax;          // -> HLSL: BoxMax
    public uint    mChild2OrPayload; // -> HLSL: Child2OrPayload
}

// 例外: 字段级 attribute 让字段名可任意 (走 ShaderName 路径, 不走 m-strip)
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FMeshlet")]
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct FMeshlet
{
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "VertexOffset")]
    public uint VertexOffset;     // 没 m 前缀 OK, 因为有字段级 ShaderName
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "TriangleOffset")]
    public uint TriangleOffset;
}
```

**反例 (本仓库踩过)**:

```csharp
// ❌ 没字段级 attribute 又没 m 前缀: Debug 构建在 ShaderCode.cs:384 直接断言挂掉,
//    Release 构建会把首字母吃掉变成 oxMin / hild1 / oxMax, 静默错乱
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGpuBvhNode")]
public struct FGpuBvhNode
{
    public Vector3 BoxMin;          // ❌
    public uint    Child1;          // ❌
    public Vector3 BoxMax;          // ❌
    public uint    Child2OrPayload; // ❌
}
```

**自检清单**:

- [ ] 新写的 `[TtShaderDefine]` struct 里, **每一个**没字段级 attribute 的
      `public` field 都以小写 `m` 开头?
- [ ] HLSL 端写的 `bvh.BoxMin` / `ray.Origin` 这种引用是否用的是**剥 m 后的裸
      名**? (HLSL 端**不要写** `mBoxMin`)
- [ ] 把 C# 字段从 `BoxMin` 改名为 `mBoxMin` 之后, 仓库里所有 C# 端调用
      (`node.BoxMin = ...`, `rays[i].Origin = ...`) 都同步改了? 用
      `Get-ChildItem -Recurse -Filter *.cs | Select-String -Pattern "\.旧名\b"`
      验证残留。

### 3.3 任何 shader (`.compute` / `.cginc`) 必须 `#include "Inc/GlobalDefine.cginc"`

**适用场景**: 所有新写的 `.compute` / `.cginc`, **即使**该 shader 不使用任何
内置 cbuffer (`CameraData` / `LightData` / `Time` 等) 或全局函数。

**强制规则**:

1. **`.compute` 入口文件第一行 `#include "<相对路径>/Inc/GlobalDefine.cginc"`**,
   相对路径根据文件深度调整:
   - `enginecontent/Shaders/Bricks/Foo/Bar.compute` → `../../Inc/GlobalDefine.cginc`
   - `enginecontent/Shaders/Bricks/Foo/Sub/Bar.compute` → `../../../Inc/GlobalDefine.cginc`
2. **`.cginc` 内部如果引用了 `[TtShaderDefine]` 自动生成的 struct (作为变量类
   型 / `StructuredBuffer<T>` 模板参数 / 函数参数), 同样必须在第一行
   `#include`**, 不能依赖调用方 include。
3. `.compute` 入口和它 include 的所有 `.cginc` **两端都 include**, 重复
   include 由 `#ifndef _GLOBAL_DEFINE_H_` 守卫去重 (GlobalDefine.cginc 自身有
   头文件守卫)。**不要为了"省一行"只在某一端写**。

**为什么必须遵守**:

- `[TtShaderDefine]` struct (`FGpuBvhNode` / `FGpuRay` / `FAdvShadowNodeData`
  等) **不在任何 .cginc / .compute 里手写**, 而是引擎反射 C# 端定义后, 在
  shader 编译期把它们注入到 `ENGINE_PREPROCESSORTS_INC` 块里。这个块的入口
  就是 `Inc/GlobalDefine.cginc`。
- 不 include GlobalDefine 等于这些 struct 在 shader 编译单元里**完全不存在**,
  报错形式极具迷惑性: 第一处 `error X3000: syntax error: unexpected token
  'FXxx'` (因为 HLSL 解析器把它当作未声明 identifier), 然后**整个函数体的
  形参全部失去声明**, 在远处报一连串 `error X3004: undeclared identifier
  'outFoo'` (实际是函数签名整体绑定失败的下游错误)。看着像 cginc 内部错乱,
  实际是 include 缺失。
- `.compute` 入口和 `.cginc` 都 include 是因为引擎的反射 / 预处理 pass 偶尔会
  把 `.compute` 单独拎出来跑, 此时只在 `.cginc` 里 include 拿不到注入。

**正例**:

```hlsl
// GpuBvhTraversal.compute 第一行
#include "../../../Inc/GlobalDefine.cginc"
#include "GpuBvhCommon.cginc"

StructuredBuffer<FGpuBvhNode> BvhNodes;     // FGpuBvhNode 通过 GlobalDefine 注入
```

```hlsl
// GpuBvhCommon.cginc 顶部 (即使被 .compute include 时已经间接拿到, 也要写)
#ifndef _GPU_BVH_COMMON_H_
#define _GPU_BVH_COMMON_H_

#include "../../../Inc/GlobalDefine.cginc"

void TraverseBvhClosest(StructuredBuffer<FGpuBvhNode> nodes, ...) { ... }
```

**反例 (本仓库踩过)**:

```hlsl
// ❌ GpuBvhCommon.cginc 顶部直接进入 helper 定义, 没 include GlobalDefine
#ifndef _GPU_BVH_COMMON_H_
#define _GPU_BVH_COMMON_H_

void TraverseBvhClosest(StructuredBuffer<FGpuBvhNode> nodes, ...)  // ← X3000: unexpected token 'FGpuBvhNode'
{
    outHitT = ray.MaxT;                                            // ← X3004: undeclared identifier 'outHitT' (下游错位错误)
    ...
}
```

**自检清单**:

- [ ] 新写的每个 `.compute` 文件, 第一行是不是 `#include
      "<相对路径>/Inc/GlobalDefine.cginc"`?
- [ ] 新写的每个 `.cginc` 文件, 头文件守卫 `#ifndef ... #define` 之后第一行是
      不是 `#include "<相对路径>/Inc/GlobalDefine.cginc"`? (即使该 cginc 当前
      只用引擎内置 cbuffer, 也建议 include —— 0 成本, 防未来加 `[TtShaderDefine]`
      引用时漏 include)
- [ ] 相对路径回退层数对不对? `Bricks/X/Y/Z.compute` → 三级回退
      `../../../Inc/GlobalDefine.cginc`。算错时 HLSL 编译器会报 include 路径
      解析失败, 检查报错信息里的相对路径段是否对应到 `enginecontent/Shaders/Inc/`
      就能定位。



| target | 性质 | 典型用途 | 反例 |
|---|---|---|---|
| `AsyncIO` (默认) | 单专线 | 资产加载、磁盘读写、网络 IO | 长 CPU 计算 (会阻塞 IO 队列) |
| `Logic` | 单专线 | 游戏逻辑相关串行任务、物理 task 投递目标 | 高耗时 GPU readback (会阻塞 logic 队列) |
| `Physics` | 单专线 | PhysX 仿真 (`PhyScene.cs:379`) | 与物理无关的任务 |
| `Render` | 单专线 | RHI/渲染线程上需要的副作用 | 阻塞调用 (会吞渲染帧) |
| `Main` | 主线程 | 必须在主线程做的 UI 操作 | 长耗时任务 |
| `AsyncEditor` | 单专线 | 编辑器后台任务 | 运行时任务 |
| `TPools` | **多线程池** (并行) | fence wait、并行 culling、独立小任务批 | 有依赖关系的任务 (顺序不保证) |

#### 1.6.3 标准模板

**模板 A: fire-and-forget (RunOn)** —— 适合不关心结果或自己用 `completedEvent` 等。
参考 `Bricks/PhysicsCore/PhyScene.cs:379`:

```csharp
TtEngine.Instance.EventPoster.RunOn(static (state) =>
{
    var scene = state.UserArguments.Obj0 as TtPhySceneMember;
    scene.TickPxScene(scene.TickLogic_ellapse);
    return true;
}, Thread.Async.EAsyncTarget.Physics, this /*userArgs*/, PxSceneTickEndEvent /*completedEvent*/);
```

**模板 B: 拿返回值 (Post + await)** —— 完成后自动回到调用线程。
参考 `Bricks/PhysicsCore/PhyMaterial.cs:227`:

```csharp
var result = await TtEngine.Instance.EventPoster.Post((state) =>
{
    return DoHeavyAsyncWork();           // 在 AsyncIO 线程跑
}, Thread.Async.EAsyncTarget.AsyncIO);
// 这里已经回到原调用线程, 可以安全地访问主线程资源
```

**模板 C: RunOn + TtSemaphore.Await (低层异步原语)** —— 适合需要在 fire-and-forget
回调里做完特定动作后唤醒调用线程的场景, 例如 GPU readback (见 §1.5.3.1)。
参考 `NxRHI/Buffer.cs::TtBuffer.AsyncFetchGpuData`:

```csharp
var sem = Thread.TtSemaphore.CreateSemaphore(1);
bool result = false;
TtEngine.Instance.EventPoster.RunOn((state) =>
{
    try
    {
        ...同步阻塞工作 (fence.Wait / 长 CPU 计算 / blocking IO)...
        result = true;
        return true;
    }
    catch (Exception e) { Profiler.Log.WriteLineSingle(e.ToString()); return false; }
    finally { sem.Release(); }    // ← 任何路径都必须 Release, 否则 await 端永远挂起
}, Thread.Async.EAsyncTarget.TPools);

await sem.Await();
sem.FreeSemaphore();
```

#### 1.6.4 反例

```csharp
// ❌ 反例 1: 直接用 .NET ThreadPool, 绕过 EventPoster
System.Threading.ThreadPool.QueueUserWorkItem(_ =>
{
    fence.Wait(1);
    // ... 没有 stack size 配置, 没有 profiler hook, 异常无人接管
});

// ❌ 反例 2: 把同步阻塞任务放到 AsyncIO 单专线上, 卡住 IO 队列
TtEngine.Instance.EventPoster.RunOn((state) =>
{
    fence.Wait(1);                       // 几十毫秒同步阻塞 → 后续 IO 全卡住
    return true;
}, Thread.Async.EAsyncTarget.AsyncIO);   // 应该用 TPools

// ❌ 反例 3: TPools 上跑有依赖关系的任务
for (int i = 0; i < N; i++)
{
    TtEngine.Instance.EventPoster.RunOn((state) =>
    {
        DoStep(i);                       // step i 依赖 step i-1 完成, 但 TPools 并行跑, 顺序乱
        return true;
    }, Thread.Async.EAsyncTarget.TPools);
}
// 应该用 Logic / AsyncIO 单专线保序

// ❌ 反例 4: 用 closure 捕获外层变量
var heavyObj = ComputeBigThing();
TtEngine.Instance.EventPoster.RunOn((state) =>     // ← 隐式分配 closure 持有 heavyObj 引用
{
    heavyObj.Do();
    return true;
});
// 应该改成: RunOn(static (state) => { (state.UserArguments.Obj0 as MyType).Do(); return true; }, target, heavyObj);
```

#### 1.6.5 实战参考

- `Bricks/PhysicsCore/PhyScene.cs:379` —— `RunOn(static, Physics, this, completedEvent)`
  四参完整范例
- `Bricks/PhysicsCore/PhyScene.cs:221+` —— `PostTask(EAsyncTarget.Logic, task)` 多处
- `Bricks/PhysicsCore/PhyMaterial.cs:227` —— `await EventPoster.Post(...)` 拿返回值
- `Bricks/Procedure/PgcNodeBase.cs:397/415/432` —— PGC 节点烘焙 RunOn
- `NxRHI/Buffer.cs::TtBuffer/TtTexture::AsyncFetchGpuData` —— `RunOn(TPools)` + `TtSemaphore`
  组合实现非阻塞 GPU readback

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

### 1.7 向 GPU 传递 Matrix 时必须 Matrix.Transpose

**适用场景**：所有通过 `SetValue`（非 `SetMatrix`）、`UpdateData`、`memcpy`、`StructuredBuffer` 上传、
或直接写 native 内存等方式向 GPU 传递 `Matrix` / `float4x4` 数据的代码。

**强制规则**：

1. 引擎的 C# `Matrix` 在内存中是 **行主序 (row-major)** 布局。
2. HLSL 的 `float4x4` 默认按 **列主序 (column-major)** 解释内存。
3. 因此 C# 端的矩阵在写入 GPU 内存前，**必须调用 `Matrix.Transpose()`** 进行转置，
   使得 HLSL 端 `mul(float4 v, float4x4 m)` 能得到正确的变换结果。

**唯一例外**：`CBuffer.SetMatrix(name, ref matrix, bool transpose = true)` 方法的缺省参数
`transpose = true` 会在内部自动完成转置，调用方无需手动 Transpose。
但只要不是走 `SetMatrix` 接口（包括 `SetValue`），就必须手动处理。

**标准模板**：

```csharp
// ✓ 通过 SetValue 传矩阵 — 必须手动 Transpose
var vp = camera.GetViewProjection();
var vpT = Matrix.Transpose(vp);
mCBuffer.SetValue("ViewProjMtx", in vpT);

// ✓ 通过 StructuredBuffer / UpdateData 传矩阵数组 — 逐个 Transpose
mPageDataArray[i].mViewProj = Matrix.Transpose(pageCamera.GetViewProjection());
buffer.UpdateData(0, ptr, totalBytes);

// ✓ 通过 SetMatrix 传矩阵 — 内部自动 Transpose，无需手动处理
mCBuffer.SetMatrix("WorldMtx", ref worldMatrix);  // transpose 缺省 = true
```

**反例**（曾经导致阴影投影被挤压/变形）：

```csharp
// ❌ 通过 StructuredBuffer 传 VP 矩阵但未 Transpose
mPageDataArray[i].mViewProj = pageCamera.GetViewProjection();  // HLSL 端 mul 结果错误
buffer.UpdateData(0, ptr, totalBytes);

// ❌ 通过 SetValue 传矩阵但未 Transpose
mCBuffer.SetValue("LightVP", in lightVP);  // GPU 读到的是转置后的矩阵, mul 结果错误
```

**为什么 `SetMatrix` 不需要手动 Transpose**：

`SetMatrix` 内部实现会检查 `transpose` 参数（缺省 `true`），在写入 cbuffer 内存前自动对矩阵做转置。
这是一个便利封装。但 `SetValue` 是通用的 memcpy，不会对数据做任何变换，
所以矩阵必须在调用前由开发者自行 Transpose。

**已有合规实现的参考位置**：

- `CSharpCode/Bricks/AdvanceShadow/AdvanceShadowShading.cs` — `mClipmapPageDataArray[i].mViewProj = Matrix.Transpose(...)`
- `CSharpCode/Grapics/Pipeline/CCamera.cs` — `SetMatrix` 用法

---

## 4. 编辑器 Undo/Redo 规范 (统一撤销重做架构)

本章约束所有资产编辑器 (`IAssetEditor` 实现) 的撤销重做行为。引擎已经建立统一的
命令式 Undo/Redo 基础设施, **所有新编辑器必须接入, 所有可撤销操作必须走命令记录**。
旧的 `UAction / UActionRecorder / IActionRecordable` 机制已于统一架构落地时整体删除,
**严禁复活**。

### 4.0 架构总览 (先读这节再看细则)

基础设施位于 `CSharpCode/Editor/Infrastructure/Undo/` (4 个文件, 新增文件需登记
`Editor.projitems`):

| 类 | 职责 |
|---|---|
| `TtEditorCommand` | 命令基类: `Name` / `Do()` / `Undo()` / `TryMerge()` (连续修改合并) / `Seal()` (封口后不再合并) |
| `TtPropertyChangeCommand` | 属性修改命令: 宿主 + 成员名 + 新旧值, 反射写回; 由 PropertyGrid 写入漏斗自动创建 |
| `TtDelegateCommand` | do/undo 委托包装, 编辑器专有操作 (增删节点/控件等) 的首选载体 |
| `TtTransactionCommand` | 复合命令: `BeginTransaction..EndTransaction` 期间收集的子命令合为一条历史记录 |
| `TtEditorHistory` | 每编辑器独立历史栈: `ExecuteCommand` / `PushCommand` / `Undo` / `Redo` / `JumpTo` / `SetSavePoint` / `IsApplying`; `MaxHistorySteps` 实例属性 (缺省 64) |
| `TtEditorHistoryPanel` | 操作栈历史 dock 面板, 点击条目多步跳转 |
| `EditorUndoUtils` | 工具栏 Undo/Redo 按钮 + `HandleUndoShortcut` (Ctrl+Z / Ctrl+Y / Ctrl+Shift+Z) |

三条自动记录通道 (业务代码通常只需"挂接", 不需要手写命令):

1. **PropertyGrid 拦截**: `TtPropertyGrid.HistoryHost` 挂上历史栈后, 所有属性修改
   自动记录 (PGRenderer 的 `SetValueWithHistory` 写入漏斗)。
2. **NodeGraph 命令化**: `TtNodeGraph.HistoryHost` 挂上后, 增删节点/连线/移动/粘贴
   自动记录 (含级联断线的嵌套事务)。
3. **编辑器专有命令**: 场景节点增删 (`TtSceneEditor.DeleteNodeWithHistory` /
   `PushNodeCreateCommand`)、gizmo 拖动 (`TtAxis.HistoryHost`)、UI 控件树增删等,
   用 `TtDelegateCommand` 手工记录。

### 4.1 控制门: 是否 new 出历史栈, 无任何全局配置

**适用场景**: 所有实现 `Editor.IAssetEditor` 的编辑器类。

**强制规则**:

1. `IAssetEditor` 通过 **默认接口成员** 提供控制门, 缺省全关:
   ```csharp
   Infrastructure.TtEditorHistory EditorHistory { get => null; }
   bool EnableUndoRedo { get => false; }
   ```
2. 编辑器开门 = 在类上声明**同签名公开成员** (隐式实现, **没有 override 关键字**):
   ```csharp
   public bool EnableUndoRedo => EditorHistory != null;
   public Infrastructure.TtEditorHistory EditorHistory => mEditorHistory;
   Infrastructure.TtEditorHistory mEditorHistory = new Infrastructure.TtEditorHistory();
   Infrastructure.TtEditorHistoryPanel mHistoryPanel = new Infrastructure.TtEditorHistoryPanel();
   ```
3. **回退方式 = 把 `mEditorHistory` 字段改为 null** (所有 `Clear()` 调用必须写成
   `mEditorHistory?.Clear()` 防空)。**不允许**引入任何全局开关 / jscfg 配置项来控制
   Undo 启用与否 —— 这是评审时被明确否决过的方案。
4. 默认接口成员**只能经接口类型访问**: 管理器/工具代码取门状态必须通过
   `IAssetEditor` 引用; 编辑器自身代码用 `this.EnableUndoRedo` 时必须在类上声明了
   该成员, 否则用 `mEditorHistory != null` 判断。
5. 历史栈容量按需调整用实例属性: `mEditorHistory.MaxHistorySteps = 128;`

### 4.2 编辑器标准接线 6 步 (新编辑器照抄)

```csharp
// ① 字段 region: 见 §4.1 第 2 条

// ② OpenEditor: 资产 PG 挂 HistoryHost (先 Clear 防重开残留);
//    编辑器自身设置类 PG (EditorPropGrid 等非资产数据) 不挂
mEditorHistory?.Clear();
AssetPropGrid.HistoryHost = mEditorHistory;
// 图编辑器另挂: graph.HistoryHost = mEditorHistory;  (在 SetGraph 之后)

// ③ DrawToolBar: 替换 Undo/Redo 按钮 + 快捷键 (必须在编辑器主窗口 Begin/End 作用域内)
Infrastructure.EditorUndoUtils.DrawUndoRedoButtons(mEditorHistory);
Infrastructure.EditorUndoUtils.HandleUndoShortcut(mEditorHistory);

// ④ OnDraw 尾部画历史面板 + ResetDockspace 登记 dock 窗口
if (mEditorHistory != null)
    mHistoryPanel.OnDraw(in mDockKeyClass, "History", mEditorHistory);
// ResetDockspace 内: DockBuilderDockWindow(GetDockWindowName("History", mDockKeyClass), 某分区Id);

// ⑤ Save 成功后设保存点 (联动 IsDirtyFromHistory 脏标记)
mEditorHistory?.SetSavePoint();

// ⑥ OnCloseEditor / Dispose 清理
AssetPropGrid.HistoryHost = null;
mEditorHistory?.Clear();
```

**已有合规实现的参考位置** (按复杂度递增):

- 属性型最小接线: `CSharpCode/Bricks/PhysicsCore/PhyMaterialEditor.cs`
- 属性型 + 多 PG: `CSharpCode/Editor/Forms/MaterialInstanceEditor.cs` (首个试点)
- 图编辑器: `CSharpCode/Bricks/CodeBuilder/ShaderNode/MaterialEditor.cs`
- 多方法图共用编辑器级栈: `CSharpCode/Bricks/CodeBuilder/MacrossNode/MacrossEditor.cs`
  (`OpenMethodGraph` 时逐图挂 `method.HistoryHost`, 图切换**不** Clear 历史)
- 场景编辑器全家桶: `CSharpCode/Editor/Forms/SceneEditor.cs` (PG 拦截 + gizmo 事务 +
  节点增删命令 + WorldOutliner.HistoryHost)
- UI 编辑器: `CSharpCode/Bricks/UI/Editor/TtUIEditor.cs` (控件树增删记录 index 供
  undo 插回原位)

### 4.3 PropertyGrid 拦截通道的边界

**强制规则**:

1. `TtPropertyGrid.HistoryHost` 为 null 或 `history.IsApplying == true` 时, 写入路径
   与旧行为**完全一致** —— 不挂就是不记录, 不会有额外开销。
2. **只给"资产数据"的 PG 挂 HistoryHost**。编辑器自身设置 (预览转速、相机参数、
   EditorPropGrid) 不属于资产修改, 不挂, 避免污染历史栈。
3. 嵌套 struct 属性修改由 PGRenderer 级联写回机制保证**只在最外层引用类型宿主记录
   一次**, 业务代码不需要也不应该额外处理。
4. 拖滑条产生的连续修改靠 `TryMerge` + "无激活控件时封口" 自动合并为一条记录,
   不要在业务代码里手工去抖。
5. 列表/字典元素的增删 (ListEditor/DictionaryEditor 路径) **当前未接入拦截**,
   如需支持请在 PGRenderer 漏斗处扩展, 并同步更新本节, 不要在编辑器里散写特例。

### 4.4 NodeGraph / 编辑器专有命令的编写规范

**强制规则**:

1. **修改图结构必须走 `TtNodeGraph` 的公开方法** (`AddNode` / `RemoveNode` /
   `AddLink` / `RemoveLink` / `DeleteSelectedNodes` / `Paste`), 它们已内置命令记录。
   **禁止**直接操作 `graph.Nodes` / `graph.Linkers` 集合 —— 绕过后该操作不可撤销,
   且 undo 其它命令时可能因图状态不一致而崩溃。
2. **命令回放防重入统一靠 `IsApplying`**: 命令的 Do/Undo 闭包里调用图/场景的公开
   方法是安全的 —— 回放期间 `history.IsApplying == true`, 埋点会自动走无记录路径。
   自己新增埋点时必须遵守同一模式:
   ```csharp
   var history = HistoryHost;
   if (history != null && history.IsApplying == false)
   {
       history.PushCommand(new TtDelegateCommand("Xxx", () => DoIt(), () => UndoIt()));
   }
   ```
3. **复合操作必须包事务**: 多选删除、粘贴、克隆、gizmo 整次拖动等一个用户动作产生
   多条子命令的场景, 用 `BeginTransaction(name) .. EndTransaction()` 包成历史面板里
   的一条记录。`BeginTransaction` 支持嵌套计数 (删多选节点时外层事务包含每个
   RemoveNode 自身的级联断线事务), 只有最外层 End 才入栈。
4. **`ExecuteCommand` vs `PushCommand`**: 操作尚未执行 → `ExecuteCommand(cmd)`
   (帮你调 Do 再入栈); 操作已经在外部执行完毕 → `PushCommand(cmd)` (只入栈,
   **不要**再手动调一次 Do, 否则操作执行两遍)。
5. **Do/Undo 必须严格对称**, 且闭包捕获的对象引用生命周期由命令持有者保证:
   - 删除类命令的 Undo 要恢复**全部**副作用 (例: 场景节点删除的 undo 除了恢复
     `Parent` 还必须从 `Scene.PendingDeleteNodeFiles` 移除待删文件, 否则 undo 后
     Save 会把磁盘上的 .node 文件误删 —— 用 `TtSceneEditor.DeleteNodeWithHistory`
     而不要自己写);
   - 插入类命令要记录 index, undo/redo 插回原位 (参考 TtUIEditor 控件树命令)。
6. **拖动类操作在"结束"时刻封一条命令**, 不要拖动过程中每帧记录:
   - 图节点移动: `LeftPress` 记旧位置 → `LeftRelease` 封 `Move` 命令 (已内置);
   - gizmo: `TtAxis.HistoryHost` 挂上后 `EndTransAxis` 自动把整次拖动封为一条
     Transform 命令, 无位移不产生记录。

**反例**:

```csharp
// ❌ 反例 1: 绕过公开方法直接改集合 (不可撤销 + 状态不一致)
graph.Nodes.Remove(node);              // 应该: graph.RemoveNode(node)
parentGraph.Linkers.RemoveAt(i);       // 应该: graph.RemoveLink(...)

// ❌ 反例 2: PushCommand 之后又手动执行一遍 (操作跑两次)
var cmd = new TtDelegateCommand("Add", () => list.Add(x), () => list.Remove(x));
history.PushCommand(cmd);
list.Add(x);                           // ← Push 前已 Add 过才对; 要么改用 ExecuteCommand

// ❌ 反例 3: 多选删除不包事务 (历史面板出现 N 条零散记录, undo 要按 N 次)
foreach (var n in selected) TtSceneEditor.DeleteNodeWithHistory(history, n);
// 应该: history?.BeginTransaction("Delete N Nodes"); foreach(...); history?.EndTransaction();

// ❌ 反例 4: 命令闭包里再判断/记录历史 (回放时重复入栈)
history.PushCommand(new TtDelegateCommand("X",
    () => { DoIt(); history.PushCommand(...); },   // ← Do 回放时 IsApplying=true,
    () => UndoIt()));                              //   PushCommand 会被忽略, 但这种写法
                                                   //   说明你没理解防重入模型, 禁止
```

### 4.5 禁止事项与文本编辑器例外

1. **严禁复活旧机制**: `UAction` / `UActionRecorder` / `IActionRecordable` /
   `UMaterialInstanceEditorRecorder` 已整体删除 (原 `CSharpCode/GamePlay/Action/`),
   不要以任何形式重新引入"在属性 setter 里手工埋 OnChanged"的模式 —— 统一走
   PropertyGrid 拦截或显式命令。
2. **DesignMacross 的 `TtCommandHistory` 是适配器**
   (`CSharpCode/Bricks/DesignMacross/CommandHistory.cs`), 内部委托编辑器级
   `TtEditorHistory`。新代码可以继续用它的 `CreateAndExtuteCommand` API, 但**不要**
   再造第二套独立命令栈 —— 一个编辑器只有一个历史栈。
3. **纯文本编辑器 (TtCodeEditor) 例外**: ShaderAsset / ShadingEnv 等纯 HLSL 文本
   编辑的撤销由 TtCodeEditor **原生托管** (含 Ctrl+Z), 按钮直接转发
   `mShaderEditor.mCoreObject.Undo()/Redo()`, **不进** `TtEditorHistory`
   (文本粒度命令成本过高, 收益低)。混合型编辑器 (如 MaterialEditor 的 TextEditor
   面板) 同理: 图/属性走统一栈, 文本面板走原生。
4. **只读查看器不放 Undo/Redo 按钮** (参考 MetaViewEditor 的清理), 不要为了
   工具栏对齐摆两个死按钮。

**自检清单** (新写/修改编辑器时过一遍):

- [ ] 编辑器类上有 `EnableUndoRedo` / `EditorHistory` / `mEditorHistory` /
      `mHistoryPanel` 四件套? 回退路径 (`mEditorHistory` 置 null) 下所有调用都
      `?.` 防空?
- [ ] 资产 PG 挂了 `HistoryHost`, 编辑器设置 PG **没**挂?
- [ ] 图编辑器在 `SetGraph` 之后挂了 `graph.HistoryHost`?
- [ ] 所有结构性修改走公开方法或显式命令, 没有裸改集合?
- [ ] 一个用户动作 = 历史面板一条记录 (复合操作包了事务)?
- [ ] Save 后调了 `SetSavePoint()`? 关闭编辑器时 `HistoryHost = null` + `Clear()`?
- [ ] 实机验证: 改属性/拖滑条/结构操作 → Ctrl+Z / Ctrl+Y 对称还原, 历史面板跳转
      正常, undo 后 Save 无副作用 (场景编辑器重点验证 .node 文件不被误删)?

---

## 5. 跨编辑器共享状态 (路径收藏夹)

本章约束"在 A 编辑器里选中一个东西, 到 B 编辑器里引用它"这类需求。引擎已有统一机制
(`CSharpCode/Editor/Infrastructure/Favorites/`: `TtEditorFavoritePaths` +
`TtPGFavoritePathAttribute`), **接入步骤与代码模板见 `CodeLib.md` §13**, 本节只列强制约束。

### 5.1 共享入口唯一, 不得再造全局状态

**强制规则**:

1. 跨编辑器传递的"选中 / 收藏 / 最近使用"路径数据**只能放 `TtEditorFavoritePaths`**。
   不要在各编辑器里再写 static 字段 / 单例 / 往 `UIProxyManager` 塞业务数据
   (`UIProxyManager` 只放 UI 代理对象, 例如 SearchBar)。
2. **加入收藏夹的入口只能是 `TtPGFavoritePathAttribute` 在 Details 面板上的按钮**。
   不得在资源树 / 视口 / 右键菜单里散落第二个"拷贝路径"入口 —— 这是评审时明确
   定下的交互约束, 保证用户只需记住一个地方。
3. 不得把机制改成"按类型强绑定"。底层键是 **channel 字符串**, 类型只是通过
   `RegisterTypeChannel` 推导默认 channel 的便捷手段; 同一类型必须能分出多个用途通道。

### 5.2 路径字符串里禁止编码 index / 运行时 id

**适用场景**: 所有往收藏夹写入的路径, 以及消费端的解析代码。

**强制规则**:

1. 路径只能由"重开工程也稳定的名字"组成, 统一格式 `资产名:条目名`,
   且必须在 `TtEditorFavoritePaths` 里**成对**提供 `MakeXxxPath` / `TryParseXxxPath`,
   不得让业务代码自行拼接或拆分字符串。
2. **绝不把骨骼 index / 节点 id / 运行时句柄编进路径**。骨骼 index 会随 FBX 重导入变化,
   存了就会出现"选的是 A 骨骼, 重导入后指到 B 骨骼"这种难排查问题。
3. 消费端必须**每次按名字到当前数据里重新解析**出 index / 引用, 解析失败时跳过
   该条目而不是写入一个非法值 (参考 `AnimUtil.cs` 的 `TryResolveFavoritePath`)。
4. 用这个格式前必须确认**同一容器内条目名唯一** (骨骼成立: `TtSkinSkeleton.HashDic`
   以 NameHash 为键)。不唯一的域要改用层级路径, 并同步更新本节与 `CodeLib.md` §13.3。

### 5.3 生命周期与持久化

**强制规则**:

1. 收藏夹是**会话级内存**, 关掉编辑器即清空。业务代码**不得假设条目跨重启仍存在**,
   也不得把它当作资产数据的存储位置 (资产必须自己存完整引用)。
2. 将来要持久化必须走 `[IO.TtConfig]` + `TtConfigManager` (参考 `CodeLib.md` §10),
   **不得**在收藏夹里自己读写文件或往 `DynConfigData` 塞结构化数据。
3. 每个 channel 有容量上限 (`Capacity`, 缺省 32), 不要把它当无限长的历史记录用。

### 5.4 只读属性上必须关掉 Pick

**强制规则**: 生产端的路径属性是 getter-only 时, attribute 必须写 `AllowPick = false`。
PropertyGrid 对自定义编辑器返回 `true` 的行为是"去 SetValue", 只读属性上返回 `true`
会直接报错。同理, 自己写新的 `TtPGCustomValueEditorAttribute` 时, `info.Readonly == true`
的分支必须保证不会返回 `true`。

**自检清单** (新接一个收藏夹通道时过一遍):

- [ ] channel 常量集中定在 `TtEditorFavoritePaths` 里, 没有在业务代码里裸写字符串?
- [ ] `MakeXxxPath` / `TryParseXxxPath` 成对提供, 且路径里没有 index / 运行时 id?
- [ ] 生产端 `AllowPick = false`, 消费端 `AllowAdd = false` (各只开一面)?
- [ ] 消费端解析失败时是"跳过该条目", 不会写出非法值?
- [ ] 新增文件登记到 `.projitems`?
- [ ] 实机验证: A 编辑器 `[+]` → B 编辑器下拉能看到并选中, 选完写回的值正确?

---

## 6. 引擎单位与坐标系约定 (米制 + Y-up)

引擎的长度单位是 **米**, 坐标系是 **Y-up**。这是全局硬约定, 所有几何 / 物理 /
光照数据都建立在它上面。违反的表现**不是崩溃也不是报错, 而是行为诡异但看着"能跑"**
(见 §6.2 的真实案例), 编译器不会帮你查 —— 只能靠本章的规则和 review 拦住。

### 6.0 事实锚点 (改任何带单位的常量前先看这里)

| 锚点 | 值 | 位置 |
|---|---|---|
| 骨骼 mesh space | 人形角色脚 Y≈0.06, 头顶 Y≈1.47 (人高 ~1.5) | `content/tutorials/animation/graychan.skt` |
| PhysX 主场景重力 | `new Vector3(0, -9.8f, 0)` | `CSharpCode/Bricks/PhysicsCore/PhyScene.cs` |
| Kawaii 物理重力 | `v3dxVector3(0, -9.8f, 0)` | `NativeCode/Bricks/Animation/KawaiiPhysics/KawaiiPhySettings.h` |
| 世界坐标 | `DVector3` (double), 进渲染前转本地坐标 | 见 §1.5 |
| 碰撞体轴向 / 上方向 | `Vector3.UnitY` | — |
| 导入期换算钩子 | `TtAssetImportOption_Mesh.UnitScale` | `CSharpCode/Bricks/AssetImpExp/AssetImporter.cs` 的 `MakeBoneDesc` |

### 6.1 强制规则

1. 新写的任何带长度 / 速度 / 加速度单位的常量, 一律用米 (m, m/s, m/s²)。物理与
   光学公式直接用 SI 数值 (g=9.8, 声速 343, 光度学 lux=lm/m², 体积雾 extinction per
   meter), **不要引入单位换算因子** —— 那是永久性的维护税。
2. **从外部代码移植时必须逐个字面量换算**。重点是 UE 系插件 (UE 的 1 单位 = 1cm,
   所有默认值都是厘米): 除以 100。换算后必须在类型头部或字段注释里写明
   "原始单位 = cm, 已换算", 供下一个人对账 (参考 `KawaiiPhySettings.h` 顶部的
   UNIT CONVENTION 块)。
3. 单位是约定而非类型, 编译器不查。**带单位的常量禁止裸写在算法里**, 必须是命名
   常量 / 可配置字段, 并在注释里注明单位。典型容易漏的一批: 重力、碰撞半径、
   contact/rest offset、sleep 阈值、snap 网格、LOD 距离、near/far、shadow bias、
   decal offset、传送阈值、风力。
4. 外部资产的单位元数据必须在**导入期**归一到米: glTF 规范强制米制; USD 看
   `metersPerUnit`; FBX 看 `UnitScaleFactor` (Maya/Max 默认厘米)。不要把 DCC 的单位
   带进运行时数据, 更不要在运行时到处补系数。
5. Y-up: 重力沿 -Y, 骨骼 mesh space +Y 朝上, 碰撞体轴向用 `Vector3.UnitY`。移植
   Z-up 来源 (部分 UE / Blender 数据、Assimp 的 Z-up 场景) 的向量常量时, **轴和量级要
   一起换**, 只改一个比两个都不改更难查。

### 6.2 为何必须当强制规则: 真实案例

Kawaii 物理马尾"完全垂下、纹丝不动": 重力保留了上游 UE 插件的厘米值 -980, 在米制
骨架上等于 100 倍过强。dt=1/60 时单帧自由落体位移约 0.27m, 超过一整节马尾骨长 (~0.1m),
长度约束每帧把链投影回正下方, 动画位移被 10:1 淹没。同批厘米遗留:
`Radius 3→0.03`、`TeleportSpeedThreshold 300→3`、`ClothThickness 1→0.01`。
**全程不报错、不崩溃、日志干净**, 只能靠算量级发现。

### 6.3 精度: 单位不是精度问题, 世界尺寸才是

1. float32 的**相对**精度恒定 (24 位有效位, 半个 ULP ≈ 6e-8), 整个世界乘 100 精度
   一位不差 —— 所以"米还是厘米"**对 float 精度没有影响**。真正决定精度的是
   *世界最大坐标 / 需要分辨的最小细节* 之比:
   - 保证 1mm 误差: 约 ±16 km (米制与厘米制完全相同)
   - 地球半径 6371 km 处 float32 ULP ≈ 0.76 m → 行星级世界**必须** double 世界坐标 +
     相机相对渲染, 与单位无关 (见 §1.5)。
2. **fp16 是唯一米制 / 厘米制不等价的地方**: half 最大值 65504、10 位尾数。厘米制下
   655 m 就溢出 fp16; 米制下 65 km 才溢出。shader 里用 half 存位置 / 世界偏移 /
   运动矢量时, 米制的安全余量大两个数量级 —— 这也是本引擎选米的实际收益之一。
3. 反过来, 需要"1 单位 = 1 个整数刻度"的定点 / 量化管线 (int16 顶点压缩、网络坐标
   同步、voxel / navmesh 体素、lightmap texels-per-unit) 在米制下要**显式**引入分数
   刻度 (如 1/1024 m)。禁止为了省事把某个子系统偷偷改回厘米 —— **混用单位比统一用
   哪个都糟**。

### 6.4 已有资产的迁移

改默认值**不会**修正已经 authored 进资产的旧数值 (序列化数据里存的是具体数字, 不是
"跟随默认值"的引用)。换算单位后必须:

1. 列出受影响字段, 在编辑器面板里重新填写 (如 dmc_grayanim 链上的 `Radius=3` → `0.03`);
2. GenCode 类资产重新 GenCode 一次, 让生成代码带上新值;
3. 无法自动判定旧值时不要猜, 去看当时的原始单位 (UE = cm) 再除 100。

**自检清单** (引入或移植带单位的数据时过一遍):

- [ ] 新常量是米 / m/s / m/s², 注释里写了单位?
- [ ] 从 UE 或 DCC 搬来的默认值逐个除过 100 了? 注明了原始单位?
- [ ] Z-up 来源的向量常量把轴换成 Y-up 了?
- [ ] shader 里有 half 存位置 / 偏移吗? 量级在 fp16 安全范围内?
- [ ] 需要大世界精度的路径走了 `DVector3` + `ToLocalPosition` (§1.5)?
- [ ] 已 authored 的旧资产数值列出来并重新填过了?

---
using System;
using System.Runtime.InteropServices;
using EngineNS.NxRHI;

// =============================================================================
// TtGpuBvh
//
// Owns the GPU-side mirror of a TtDynamicBVH<T>:
//   - One StructuredBuffer<FGpuBvhNode> (BFS-flattened, root at index 0).
//   - Matching SRV for shader-side traversal kernels.
//   - Leaf payload is NOT shipped here on purpose; the caller hands a callback
//     to BuildFromCpuBvh and writes its own payload buffer in lock-step with
//     the dense leafIndex we generate.
//
// Lifecycle in the current debug workflow:
//   1. User clicks [Cmd] UploadToGpu on TtBVHDebugNode.
//   2. We allocate / re-allocate buffer + SRV via rc.CreateBuffer (with
//      InitData), the same pattern Bricks/Particle/Emitter.cs uses to seed
//      structured buffers from CPU memory in one shot.
//   3. Subsequent compute kernels (e.g. GpuBvhTraversal.compute) bind
//      NodeSrv as the BVH input.
//   4. On dispose / next upload, we tear down the GPU resources first, then
//      re-create.  Refit / partial update is intentionally out of scope for
//      the debug research stage — every topology change re-uploads in full.
//
// Compliance notes:
//   - This class doesn't create any CBV / drawcall, so §1.1 (CBV flush) and
//     §1.2 (BindXxx in OnDrawCall) don't apply here directly.  They apply to
//     TtGpuBvhRayCastShading which consumes this buffer.
//   - Resource lifetime: the underlying TtCpu2GpuBuffer<FGpuBvhNode> (and
//     its Srv) is owned by this instance and released in Dispose.  Anyone
//     holding a long-lived TtBindless slot pointing at NodeSrv must
//     ClearSlot() before this is disposed (see CodingGuidelines.md §1.4.3).
// =============================================================================

namespace EngineNS.Bricks.Collision.BVH
{
    /// <summary>
    /// 32-byte AoS layout uploaded to the GPU.  Cache-line friendly (half a
    /// 64B line), one fetch covers the entire node.
    ///
    /// HLSL counterpart is **auto-generated** by the engine's shader compiler
    /// (see <c>CodeLib.md §11</c>): the <c>[TtShaderDefine]</c> attribute makes
    /// the engine reflect this struct and emit a matching HLSL
    /// <c>struct FGpuBvhNode { ... }</c> into shader code at compile time. Do
    /// **not** hand-write the same struct in any .cginc / .compute — that
    /// would cause a redefinition error.
    ///
    /// Conventions:
    ///   - Internal node: Child1 / Child2OrPayload are both indices into the
    ///                    packed node buffer (root is always 0).
    ///   - Leaf node:     Child1 == <see cref="TtDynamicBVH{T}.LeafChildSentinel"/>
    ///                    (0xFFFFFFFF), Child2OrPayload is a dense leafIndex
    ///                    the caller uses to index its own payload buffer.
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGpuBvhNode")]
    public struct FGpuBvhNode
    {
        // Field names must start with 'm' — the shader code generator strips
        // the leading 'm' to produce the HLSL field name (mBoxMin -> BoxMin),
        // and ShaderCode.cs:384 asserts on it. See CodingGuidelines.md §3.2.
        public Vector3 mBoxMin;          //  0..12  -> HLSL: BoxMin
        public uint    mChild1;          // 12..16  -> HLSL: Child1
        public Vector3 mBoxMax;          // 16..28  -> HLSL: BoxMax
        public uint    mChild2OrPayload; // 28..32  -> HLSL: Child2OrPayload
    }

    /// <summary>
    /// 16-byte hit record written by GpuBvhTraversal.compute. HLSL counterpart
    /// is auto-generated from this C# definition via <c>[TtShaderDefine]</c>;
    /// do not hand-write a matching struct on the shader side.
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGpuHit")]
    public struct FGpuHit
    {
        // 'm' prefix mandatory (shader generator strips it). See §3.2.
        public uint  mHitProxyId;  // -> HitProxyId; dense leafIndex of the closest hit, or TtGpuBvh.NullPayload on miss
        public float mHitT;        // -> HitT;       ray-AABB enter t for the closest hit (== ray.MaxT on miss)
        public uint  mHitNodeId;   // -> HitNodeId;  GPU node index of the closest hit (debug-only)
        public uint  mPad;         // -> Pad
    }

    /// <summary>
    /// 32-byte ray record consumed by GpuBvhTraversal.compute. HLSL counterpart
    /// is auto-generated from this C# definition via <c>[TtShaderDefine]</c>;
    /// do not hand-write a matching struct on the shader side.
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGpuRay")]
    public struct FGpuRay
    {
        // 'm' prefix mandatory (shader generator strips it). See §3.2.
        public Vector3 mOrigin;  //  0..12  -> HLSL: Origin
        public float   mMaxT;    // 12..16  -> HLSL: MaxT
        public Vector3 mDir;     // 16..28  -> HLSL: Dir
        public uint    mRayId;   // 28..32  -> HLSL: RayId
    }

    /// <summary>
    /// GPU mirror of a flattened dynamic BVH. Decoupled from the original
    /// payload type T so the same uploader works for any TtDynamicBVH&lt;T&gt;.
    /// </summary>
    public class TtGpuBvh : IDisposable
    {
        /// <summary>
        /// Sentinel value <c>FGpuHit.mHitProxyId</c> takes when a ray misses
        /// the BVH entirely. Mirrors <c>GPU_BVH_NULL_PAYLOAD</c> in
        /// <c>GpuBvhCommon.cginc</c>. Same numeric value as
        /// <see cref="TtDynamicBVH{T}.LeafChildSentinel"/> (both are
        /// <c>0xFFFFFFFF</c>) but the two are kept as separate constants
        /// because their semantics are different: the leaf sentinel marks a
        /// node-table slot as "this child is a leaf, not an internal node",
        /// while NullPayload marks a hit slot as "this ray missed entirely".
        /// </summary>
        public const uint NullPayload = 0xFFFFFFFFu;

        // ------------------------------------------------------------------
        // Live GPU state. Re-allocated on every successful BuildFromCpuBvh.
        //
        // mNodeBuffer (CPU→GPU upload type) uses TtCpu2GpuBuffer<FGpuBvhNode>:
        // BuildFromCpuBvh does Initialize(BFT_SRV) + SetSize + UpdateData per
        // node + Flush2GPU(null). Same pattern as mRayBuffer below — we get
        // SRV creation, 1.5x grow-on-Flush, and CPU mirror for free, instead
        // of hand-rolling CreateBuffer + CreateSRV + InitData. See
        // CodingGuidelines.md §1.5.1.
        // ------------------------------------------------------------------
        Graphics.Pipeline.TtCpu2GpuBuffer<FGpuBvhNode> mNodeBuffer;
        public TtSrView NodeSrv { get { return mNodeBuffer != null ? mNodeBuffer.Srv : null; } }

        /// <summary>Number of <see cref="FGpuBvhNode"/> entries currently uploaded.</summary>
        public int NodeCount { get; private set; }

        /// <summary>Number of leaves currently uploaded (== caller's payload table size).</summary>
        public int LeafCount { get; private set; }

        /// <summary>
        /// True iff the GPU node buffer + SRV are valid and contain at least
        /// one node. Use this from the consumer side before binding to a
        /// drawcall.
        /// </summary>
        public bool IsReady
        {
            get { return mNodeBuffer != null && mNodeBuffer.Srv != null && NodeCount > 0; }
        }

        // ------------------------------------------------------------------
        // Ray-cast dispatch state.
        //
        // mRayBuffer (CPU→GPU upload type) uses TtCpu2GpuBuffer<FGpuRay>: it
        // keeps a CPU mirror (DataArray) in sync with the GPU side and grows
        // capacity 1.5x automatically; we just Clear+SetSize+UpdateData each
        // dispatch then Flush2GPU. See CodingGuidelines.md §1.5.1.
        //
        // mHitBuffer (GPU-write / CPU-readback) uses TtGpuBuffer<FGpuHit>:
        // there is no CPU upload, so a CPU mirror would just be wasted memory;
        // we ship a GPU-only buffer with both UAV (for the compute shader to
        // write) and SRV (kept for symmetry / debug viz). FetchGpuData reads
        // it back to CPU when the user asks. See CodingGuidelines.md §1.5.2.
        //
        // mDrawcall + mCmdList live for the whole node lifetime; we just
        // re-issue them on each dispatch. mShading is awaited once in
        // InitializeShadingAsync so that by the time RayCastDispatch fires,
        // the effect is fully resolved (binder table valid, see §1.2).
        //
        // mTraversalCBuffer is created on first OnDrawCall via the standard
        // §1.1 pattern (full SetValue + MarkDirty + FlushDirty). Per-dispatch
        // values flow through the public CurrentRayCount field — same
        // Tick→OnDrawCall passing pattern as DenoiseNode's mCurrentStepSize.
        // ------------------------------------------------------------------
        Graphics.Pipeline.TtCpu2GpuBuffer<FGpuRay> mRayBuffer;
        Graphics.Pipeline.TtGpuBuffer<FGpuHit>     mHitBuffer;
        public TtSrView RaySrv { get { return mRayBuffer != null ? mRayBuffer.Srv : null; } }
        public TtUaView HitUav { get { return mHitBuffer != null ? mHitBuffer.Uav : null; } }

        TtCbView           mTraversalCBuffer;
        TtComputeDraw      mDrawcall;
        TtCommandList      mCmdList;
        TtGpuBvhRayCastShading mShading;

        /// <summary>Per-dispatch parameter read by OnDrawCall. Populated by RayCastDispatch.</summary>
        public uint CurrentRayCount { get; private set; }

        public void Dispose()
        {
            ReleaseDispatchResources();
            ReleaseGpuResources();
        }

        /// <summary>
        /// Drop the current GPU buffer + SRV without rebuilding. After this
        /// call <see cref="IsReady"/> returns false. Dispatch resources
        /// (drawcall / cbuffer / shading) are kept so the next BuildFromCpuBvh
        /// + RayCastDispatch can reuse them.
        /// </summary>
        public void Clear()
        {
            ReleaseGpuResources();
        }

        void ReleaseGpuResources()
        {
            if (mNodeBuffer != null) { mNodeBuffer.Dispose(); mNodeBuffer = null; }
            NodeCount = 0;
            LeafCount = 0;
        }

        void ReleaseDispatchResources()
        {
            // Order: cbuffer -> drawcall -> cmdlist -> ray/hit buffers ->
            // shading. We drop the high-level holders first so anything still
            // referencing them sees null before the underlying GPU resource
            // is freed.
            CoreSDK.DisposeObject(ref mTraversalCBuffer);
            CoreSDK.DisposeObject(ref mDrawcall);
            CoreSDK.DisposeObject(ref mCmdList);
            if (mRayBuffer != null) { mRayBuffer.Dispose(); mRayBuffer = null; }
            if (mHitBuffer != null) { mHitBuffer.Dispose(); mHitBuffer = null; }
            mShading = null;
            CurrentRayCount = 0;
        }

        /// <summary>
        /// Flatten <paramref name="cpuBvh"/> into a packed GPU buffer and
        /// upload it. Replaces any existing GPU state.
        ///
        /// The optional <paramref name="onLeafFlattened"/> callback is invoked
        /// once per leaf in flatten order; the leafIndex argument is the dense
        /// slot the caller should use when populating its own payload buffer.
        ///
        /// Returns false if the tree is empty (nothing to upload).
        /// </summary>
        public bool BuildFromCpuBvh<T>(
            TtDynamicBVH<T> cpuBvh,
            Action<int /*proxyId*/, T /*payload*/, int /*leafIndex*/> onLeafFlattened = null)
        {
            if (cpuBvh == null) throw new ArgumentNullException(nameof(cpuBvh));

            ReleaseGpuResources();

            int n = cpuBvh.FlattenedNodeCount;
            if (n <= 0) return false;

            // 1) Flatten on the CPU into split arrays (matches the
            //    TtDynamicBVH.FlattenToGpu contract).
            var boxMins = new Vector3[n];
            var boxMaxs = new Vector3[n];
            var child1  = new uint[n];
            var child2  = new uint[n];
            int written = cpuBvh.FlattenToGpu(boxMins, boxMaxs, child1, child2, onLeafFlattened);

            // 2) Push the packed nodes through TtCpu2GpuBuffer. The helper
            //    creates the underlying structured buffer + SRV on first
            //    Flush2GPU, grows 1.5x as needed, and keeps a CPU mirror in
            //    DataArray (cheap here because BVH rebuilds are rare). Same
            //    pattern as mRayBuffer below — see CodingGuidelines.md §1.5.1.
            //
            //    AoS pack happens inline (BoxMin / Child1 / BoxMax /
            //    Child2OrPayload) so we never materialize a separate
            //    FGpuBvhNode[] managed array.
            mNodeBuffer = new Graphics.Pipeline.TtCpu2GpuBuffer<FGpuBvhNode>();
            mNodeBuffer.Initialize(EBufferType.BFT_SRV);
            mNodeBuffer.SetSize(written);
            for (int i = 0; i < written; i++)
            {
                FGpuBvhNode node;
                node.mBoxMin          = boxMins[i];
                node.mChild1          = child1[i];
                node.mBoxMax          = boxMaxs[i];
                node.mChild2OrPayload = child2[i];
                mNodeBuffer.UpdateData(i, in node);
            }
            mNodeBuffer.Flush2GPU((NxRHI.TtCommandList)null);

            if (mNodeBuffer.Srv == null)
            {
                Profiler.Log.WriteLineSingle("[TtGpuBvh] TtCpu2GpuBuffer.Flush2GPU failed for " + written + " nodes.");
                ReleaseGpuResources();
                return false;
            }

            NodeCount = written;
            LeafCount = cpuBvh.FlattenedLeafCount;
            return true;
        }

        // ==================================================================
        // Ray-cast dispatch
        // ==================================================================

        /// <summary>
        /// Awaitable initializer for the compute shading env + drawcall +
        /// command list. Must complete before <see cref="RayCastDispatch"/>
        /// is called, otherwise the effect's binder table will be empty when
        /// OnDrawCall tries to FindBinder (see CodingGuidelines.md §1.2).
        /// Idempotent: subsequent calls are no-ops.
        /// </summary>
        public async Thread.Async.TtTask InitializeShadingAsync()
        {
            if (mShading != null) return;

            mShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtGpuBvhRayCastShading>();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mDrawcall = rc.CreateComputeDraw();
            // §3.5: TagObject must be set immediately after CreateComputeDraw
            // so OnDrawCall can recover the TtGpuBvh instance.
            mDrawcall.TagObject = this;
            mCmdList = rc.CreateCommandList();
        }

        /// <summary>
        /// Get or create the cbGpuBvhTraversal CBV. Called from OnDrawCall.
        /// Follows §1.1: on first creation, all fields are SetValue + MarkDirty
        /// + FlushDirty so the very first dispatch sees valid data, not GPU
        /// garbage. Subsequent calls only re-set the values; the engine
        /// auto-flushes at frame end.
        /// </summary>
        public TtCbView GetOrCreateTraversalCBuffer(FShaderBinder binder)
        {
            if (mTraversalCBuffer == null)
            {
                mTraversalCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                // §1.1 first-frame protection: every cbuffer field must be
                // SetValue then MarkDirty+FlushDirty. RootIndex stays 0
                // forever (the BFS flatten always puts root at slot 0); the
                // other three follow the dispatch.
                uint nodeCount = (uint)NodeCount;
                uint rootIndex = 0u;
                uint pad = 0u;
                uint rayCount = CurrentRayCount;
                mTraversalCBuffer.SetValue("NodeCount", in nodeCount);
                mTraversalCBuffer.SetValue("RayCount",  in rayCount);
                mTraversalCBuffer.SetValue("RootIndex", in rootIndex);
                mTraversalCBuffer.SetValue("Pad0",      in pad);
                mTraversalCBuffer.MarkDirty();
                mTraversalCBuffer.FlushDirty();
                return mTraversalCBuffer;
            }

            // Subsequent dispatches: just refresh the values. RayCount /
            // NodeCount can change between dispatches.
            uint nodeCount2 = (uint)NodeCount;
            uint rootIndex2 = 0u;
            uint rayCount2 = CurrentRayCount;
            mTraversalCBuffer.SetValue("NodeCount", in nodeCount2);
            mTraversalCBuffer.SetValue("RayCount",  in rayCount2);
            mTraversalCBuffer.SetValue("RootIndex", in rootIndex2);
            return mTraversalCBuffer;
        }

        /// <summary>
        /// Upload <paramref name="rays"/> and dispatch the GPU traversal
        /// kernel. Hits land in <see cref="HitUav"/>'s underlying buffer;
        /// call <see cref="ReadbackHits"/> after this to pull them back to
        /// the CPU.
        ///
        /// Returns false if the BVH isn't uploaded or shading isn't ready.
        /// </summary>
        public unsafe bool RayCastDispatch(FGpuRay[] rays)
        {
            if (!IsReady)
            {
                Profiler.Log.WriteLineSingle("[TtGpuBvh] RayCastDispatch skipped: BVH not uploaded.");
                return false;
            }
            if (mShading == null || mDrawcall == null || mCmdList == null)
            {
                Profiler.Log.WriteLineSingle("[TtGpuBvh] RayCastDispatch skipped: shading not initialized (await InitializeShadingAsync first).");
                return false;
            }
            if (rays == null || rays.Length == 0)
                return false;

            uint rayCount = (uint)rays.Length;
            CurrentRayCount = rayCount;

            // (Re)allocate hit buffer (GPU-only) if size changed. TtGpuBuffer
            // re-creates buffer + UAV + SRV in one shot when SetSize is called.
            if (mHitBuffer == null || mHitBuffer.NumElement != rayCount)
            {
                if (mHitBuffer != null) { mHitBuffer.Dispose(); mHitBuffer = null; }
                mHitBuffer = new Graphics.Pipeline.TtGpuBuffer<FGpuHit>();
                mHitBuffer.SetSize(rayCount, IntPtr.Zero.ToPointer(), EBufferType.BFT_UAV | EBufferType.BFT_SRV);
            }

            // Upload rays via TtCpu2GpuBuffer: Clear + SetSize sets the CPU
            // mirror length; UpdateData fills it; Flush2GPU(null) flushes on
            // a transient cmd. The wrapper (re)creates the GPU buffer + SRV
            // automatically when capacity grows past 1.5x. See
            // CodingGuidelines.md §1.5.1.
            if (mRayBuffer == null)
            {
                mRayBuffer = new Graphics.Pipeline.TtCpu2GpuBuffer<FGpuRay>();
                mRayBuffer.Initialize(EBufferType.BFT_SRV);
            }
            mRayBuffer.Clear();
            mRayBuffer.SetSize((int)rayCount);
            for (int i = 0; i < rays.Length; i++)
                mRayBuffer.UpdateData(i, in rays[i]);
            mRayBuffer.Flush2GPU((NxRHI.TtCommandList)null);

            // Dispatch: round up to numthreads(64,1,1).
            uint groupX = (rayCount + 63u) / 64u;

            using (new TtCmdListScope(mCmdList, "GpuBvh.RayCast"))
            {
                // §1.2: Tick path only does SetDrawcallDispatch + PushGpuDraw.
                // OnDrawCall (in TtGpuBvhRayCastShading) does all BindXxx and
                // creates the CBV via GetOrCreateTraversalCBuffer above.
                mShading.SetDrawcallDispatch(this, null, mDrawcall, groupX, 1, 1, true);
                mCmdList.PushGpuDraw(mDrawcall);
                mCmdList.FlushDraws();
            }

            // Submission goes through RenderQueue.QueueCmdlist (the engine-owned
            // queue), NOT through RenderContext.GpuQueue.ExecuteCommandList — the
            // latter bypasses the engine's CmdQueue grouping / Profiler hooks /
            // policy-bound flush logic. See CodingGuidelines.md §1.7 + CodeLib.md §12.
            TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(mCmdList, "GpuBvh.RayCast", EQueueType.QU_Compute, true);
            return true;
        }

        /// <summary>
        /// Pull the latest dispatched hit results back to the CPU.
        /// Synchronous: the underlying <c>FetchGpuData</c> blocks until the
        /// GPU has finished writing the buffer, so it's safe to call
        /// immediately after <see cref="RayCastDispatch"/>. The same usage
        /// pattern is in <c>Bricks/Procedure/Node/GpuNode/Height2FlowMap.cs:47</c>
        /// and <c>Bricks/GpuDriven/Cluster.cs:509</c>.
        ///
        /// Returns false (and leaves <paramref name="hits"/> = null) when the
        /// dispatch state isn't valid yet, the buffer hasn't been allocated,
        /// or the readback blob is too small for the requested element count.
        /// </summary>
        public unsafe bool ReadbackHits(out FGpuHit[] hits)
        {
            hits = null;
            if (mHitBuffer == null || mHitBuffer.GpuBuffer == null || CurrentRayCount == 0)
                return false;

            uint count = CurrentRayCount;
            using (var blob = new Support.TtBlobObject())
            {
                mHitBuffer.GpuBuffer.FetchGpuData(0, blob.mCoreObject);

                // IBuffer::FetchGpuData (native, NxRHI/Buffer.cpp) PushData 顺序:
                //   [0..4)  RowPitch   (UINT)
                //   [4..8)  DepthPitch (UINT)
                //   [8..)   Buffer raw bytes (Desc.Size)
                // 真数据从 offset 8 开始, blob 总大小 = 8 + Desc.Size。
                // 详见 CodingGuidelines.md §1.5.3 "FetchGpuData blob 布局"。
                const uint kBufferReadbackHeader = sizeof(uint) * 2;

                uint expectedBytes = (uint)sizeof(FGpuHit) * count;
                if (blob.Size < kBufferReadbackHeader + expectedBytes)
                {
                    Profiler.Log.WriteLineSingle(
                        $"[TtGpuBvh] ReadbackHits size mismatch: blob={blob.Size}, expected>={kBufferReadbackHeader + expectedBytes}.");
                    return false;
                }

                hits = new FGpuHit[count];
                fixed (FGpuHit* pDst = hits)
                {
                    var pSrc = (byte*)blob.DataPointer + kBufferReadbackHeader;
                    System.Buffer.MemoryCopy(pSrc, pDst,
                                             expectedBytes, expectedBytes);
                }
            }
            return true;
        }

        /// <summary>
        /// 异步版 <see cref="ReadbackHits"/>: 复用 <c>TtBuffer.AsyncFetchGpuData</c>,
        /// 把 GPU readback 投递到渲染线程, 调用方通过 <c>TtSemaphore.Await</c> 非阻塞挂起,
        /// 不卡当前线程。同步 <see cref="ReadbackHits"/> 仍然保留, debug 命令使用同步版以避免
        /// 把 await 链拉进 PG cmd 回调。
        ///
        /// 适合放进 ray-cast 流水线的后置阶段 (例如非 debug 场景下需要回读 hit 结果做 CPU 决策时),
        /// 或测量场景的批量 readback (一次发上千条 ray, await 一次, 不阻塞主线程)。
        ///
        /// 范式参考 <c>Editor/Snapshot.cs:175 EnqueueAutoGen</c>。
        /// </summary>
        /// <returns>(ok, hits) — ok 为 false 时 hits 为 null。</returns>
        public async Thread.Async.TtTask<(bool ok, FGpuHit[] hits)> ReadbackHitsAsync()
        {
            if (mHitBuffer == null || mHitBuffer.GpuBuffer == null || CurrentRayCount == 0)
                return (false, null);

            uint count = CurrentRayCount;
            using (var blob = new Support.TtBlobObject())
            {
                bool ok = await mHitBuffer.GpuBuffer.AsyncFetchGpuData(0, blob.mCoreObject);
                if (!ok)
                    return (false, null);

                unsafe
                {
                    // 同 ReadbackHits: 跳过 IBuffer::FetchGpuData 写在 blob 头部的
                    // 8 字节 (RowPitch + DepthPitch). 详见 CodingGuidelines.md §1.5.3。
                    const uint kBufferReadbackHeader = sizeof(uint) * 2;

                    uint expectedBytes = (uint)sizeof(FGpuHit) * count;
                    if (blob.Size < kBufferReadbackHeader + expectedBytes)
                    {
                        Profiler.Log.WriteLineSingle(
                            $"[TtGpuBvh] ReadbackHitsAsync size mismatch: blob={blob.Size}, expected>={kBufferReadbackHeader + expectedBytes}.");
                        return (false, null);
                    }

                    var hits = new FGpuHit[count];
                    fixed (FGpuHit* pDst = hits)
                    {
                        var pSrc = (byte*)blob.DataPointer + kBufferReadbackHeader;
                        System.Buffer.MemoryCopy(pSrc, pDst,
                                                 expectedBytes, expectedBytes);
                    }
                    return (true, hits);
                }
            }
        }
    }
}

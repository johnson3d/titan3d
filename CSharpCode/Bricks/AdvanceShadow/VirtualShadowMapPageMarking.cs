using System;
using System.Threading;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;

namespace EngineNS.Bricks.AdvanceShadow
{
    /// <summary>
    /// Compute shader that reads the GBuffer depth and marks which QTree pages
    /// are needed for shadow rendering this frame. Analogous to UE5 VSM MarkPages pass.
    /// </summary>
    public class TtVSMPageMarkingShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtVSMPageMarkingShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/AdvanceShadow/VSMPageMarking.compute", RName.ERNameType.Engine);
            MainName = "CS_MarkPages";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtVSMPageMarkingNode;
            if (node == null)
                return;

            // Bind depth buffer from GBuffer pass
            drawcall.BindSrv("DepthBuffer", node.GetCurrentDepthSrv());
            drawcall.BindSampler("Samp_DepthBuffer", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            // Bind output: page request flags UAV
            drawcall.BindUav("PageRequestFlags", node.GetPageRequestFlagsUav());

            // Bind QTree layer info
            drawcall.BindSrv("LayerInfoBuffer", node.GetLayerInfoSrv());

            // Bind cbuffer
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVSMPageMarking");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateMarkingCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// RenderGraph node that dispatches the page marking compute shader.
    /// Reads GBuffer depth, outputs a per-page request flag buffer that drives
    /// on-demand page allocation in TtAdvanceShadowMapNode.
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("VSMPageMarking", "Shadow\\VSMPageMarking", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtVSMPageMarkingNode : TAuxRenderGraphNode<TtVSMPageMarkingNode>
    {
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth", EBufferType.BFT_SRV);
        public TtRenderGraphPin AdvShadowPinIn = TtRenderGraphPin.CreateInput("AdvShadow", EBufferType.BFT_NONE);
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput("Result", false, EPixelFormat.PXF_UNKNOWN, EBufferType.BFT_NONE);

        private TtVSMPageMarkingShading mMarkingShading;
        private TtComputeDraw mDrawcall;
        private TtCbView mMarkingCBuffer;

        // Page request flags buffer (one uint per virtual page, written by CS)
        private TtBuffer mPageRequestFlagsBuffer;
        private TtUaView mPageRequestFlagsUav;
        private TtSrView mPageRequestFlagsSrv;

        // CPU-side readback of page request flags for driving PagePool allocation
        private uint[] mPageRequestFlagsCpu;

        // --- Phase 2: Async GPU Readback ---
        // Double-buffered readback: we read the PREVIOUS frame's results while the current
        // frame's compute shader writes new flags. This avoids GPU stalls.
        private uint[] mReadbackResultPrev;  // Previous frame's readback (ready to use)
        private volatile bool mReadbackInFlight;  // Is a readback currently pending?

        // Layer info buffer (QTree layers metadata for the shader)
        private TtCpu2GpuBuffer<FAdvShadowLayerData> mLayerInfoBuffer;

        // Cached references resolved each frame from connected pins
        private TtSrView mCurrentDepthSrv;
        private TtAdvanceShadowMapNode mAdvShadowNode;

        public TtVSMPageMarkingNode()
        {
            Name = "VSMPageMarking";
        }

        public override void InitNodePins()
        {
            AddInput(DepthPinIn);
            AddInput(AdvShadowPinIn);
            AdvShadowPinIn.LinkType = "AdvShadow";
            AddOutput(ResultPinOut);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            mMarkingShading = await TtShadingEnv.CreateShadingEnv<TtVSMPageMarkingShading>();
            mDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mPageRequestFlagsBuffer);
            mPageRequestFlagsUav?.Dispose();
            mPageRequestFlagsUav = null;
            mPageRequestFlagsSrv?.Dispose();
            mPageRequestFlagsSrv = null;
            mLayerInfoBuffer?.Dispose();
            mLayerInfoBuffer = null;
            CoreSDK.DisposeObject(ref mMarkingCBuffer);
            base.Dispose();
        }

        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            // Cache policy for use in OnDrawCall (GetOrCreateMarkingCBuffer)
            mCachedPolicy = policy;

            // Resolve connected AdvShadow node
            mAdvShadowNode = FindConnectedAdvShadowNode(policy);
            if (mAdvShadowNode == null || mAdvShadowNode.mShadowQTree == null || mAdvShadowNode.PagePool == null)
                return;

            // Resolve depth SRV from input pin
            var depthAttach = GetAttachBuffer(DepthPinIn);
            if (depthAttach == null)
                return;
            mCurrentDepthSrv = depthAttach.Srv;

            // Ensure GPU resources are created
            EnsureResources(mAdvShadowNode);

            // Clear page request flags to zero before dispatch
            ClearPageRequestFlags();

            // Dispatch the marking compute shader
            var camera = policy.DefaultCamera;
            uint screenWidth = (uint)camera.Width;
            uint screenHeight = (uint)camera.Height;

            mMarkingShading.SetDrawcallDispatch(this, policy, mDrawcall, screenWidth, screenHeight, 1, true);

            var cmdlist = TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmdlist, "VSMPageMarking"))
            {
                cmdlist.PushGpuDraw(mDrawcall);
                cmdlist.FlushDraws();
            }
            policy.CommitCommandList(cmdlist, "VSMPageMarking");

            // Phase 2: GPU-driven page allocation via async readback.
            // Use previous frame's readback result to drive allocation (1-frame latency, no stall).
            // Kick off a new readback for the current frame's flags after dispatch completes.
            ProcessReadbackResults(mAdvShadowNode);
            KickAsyncReadback();
        }

        #region Resource Access (called by ShadingEnv.OnDrawCall)

        public TtSrView GetCurrentDepthSrv() => mCurrentDepthSrv;

        public TtUaView GetPageRequestFlagsUav() => mPageRequestFlagsUav;

        public TtSrView GetLayerInfoSrv() => mLayerInfoBuffer?.Srv;

        public TtCbView GetOrCreateMarkingCBuffer(FShaderBinder binder)
        {
            if (mMarkingCBuffer == null && mAdvShadowNode?.mShadowQTree != null)
            {
                var qTree = mAdvShadowNode.mShadowQTree;

                mMarkingCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);

                // §1.1: Fill all fields immediately after CreateCBV
                mMarkingCBuffer.SetValue("InvViewProjection", Matrix.Identity);
                mMarkingCBuffer.SetValue("ShadowProjection", DMatrixToMatrix(in qTree.ProjectShadowMatrix));
                mMarkingCBuffer.SetValue("BoxMin", qTree.Root.AABB.Minimum.AsSingleVector());
                mMarkingCBuffer.SetValue("BoxMax", qTree.Root.AABB.Maximum.AsSingleVector());
                mMarkingCBuffer.SetValue("ScreenSize", new Vector2(1920, 1080));
                mMarkingCBuffer.SetValue("MaxShadowDistance", qTree.MaxShadowDistance);
                mMarkingCBuffer.SetValue("MaxDeepLevel", qTree.MaxDeepLevel);
                mMarkingCBuffer.SetValue("CameraPosition", Vector3.Zero);
                mMarkingCBuffer.SetValue("NodeCount", qTree.QNodes.Length);
                mMarkingCBuffer.MarkDirty();
                mMarkingCBuffer.FlushDirty();
            }

            // Update per-frame dynamic values
            if (mMarkingCBuffer != null && mAdvShadowNode?.mShadowQTree != null)
            {
                var camera = mCachedPolicy?.DefaultCamera;
                if (camera != null)
                {
                    var viewProj = camera.GetViewProjection();
                    var invViewProj = Matrix.Invert(in viewProj);
                    mMarkingCBuffer.SetValue("InvViewProjection", invViewProj);
                    mMarkingCBuffer.SetValue("CameraPosition", camera.GetLocalPosition());
                    mMarkingCBuffer.SetValue("ScreenSize", new Vector2(camera.Width, camera.Height));
                }

                var qTree = mAdvShadowNode.mShadowQTree;
                mMarkingCBuffer.SetValue("ShadowProjection", DMatrixToMatrix(in qTree.ProjectShadowMatrix));
            }

            return mMarkingCBuffer;
        }

        // Cached policy reference for OnDrawCall access
        private TtRenderPolicy mCachedPolicy;


        #endregion

        #region Private Implementation

        /// <summary>
        /// Convert DMatrix (double) to Matrix (float) by truncating precision.
        /// </summary>
        private static Matrix DMatrixToMatrix(in DMatrix dm)
        {
            Matrix result;
            result.M11 = (float)dm.M11; result.M12 = (float)dm.M12; result.M13 = (float)dm.M13; result.M14 = (float)dm.M14;
            result.M21 = (float)dm.M21; result.M22 = (float)dm.M22; result.M23 = (float)dm.M23; result.M24 = (float)dm.M24;
            result.M31 = (float)dm.M31; result.M32 = (float)dm.M32; result.M33 = (float)dm.M33; result.M34 = (float)dm.M34;
            result.M41 = (float)dm.M41; result.M42 = (float)dm.M42; result.M43 = (float)dm.M43; result.M44 = (float)dm.M44;
            return result;
        }

        private TtAdvanceShadowMapNode FindConnectedAdvShadowNode(TtRenderPolicy policy)
        {
            // The AdvShadow input pin uses LinkType = "AdvShadow" which matches
            // TtAdvanceShadowMapNode.SelfNodePinOut. The connection itself is enough
            // to identify the node. Use FindFirstNode as the reliable lookup method.
            return policy.FindFirstNode<TtAdvanceShadowMapNode>();
        }

        private unsafe void EnsureResources(TtAdvanceShadowMapNode advNode)
        {
            var qTree = advNode.mShadowQTree;
            int nodeCount = qTree.QNodes.Length;

            // Create page request flags buffer (RWStructuredBuffer<uint>)
            if (mPageRequestFlagsBuffer == null)
            {
                var bufDesc = new FBufferDesc();
                bufDesc.SetDefault(false, EBufferType.BFT_UAV | EBufferType.BFT_SRV);
                bufDesc.Size = (uint)(nodeCount * sizeof(uint));
                bufDesc.StructureStride = (uint)sizeof(uint);
                mPageRequestFlagsBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bufDesc);

                var uavDesc = new FUavDesc();
                uavDesc.SetBuffer(false);
                uavDesc.Format = EPixelFormat.PXF_UNKNOWN;
                uavDesc.Buffer.NumElements = (uint)nodeCount;
                uavDesc.Buffer.StructureByteStride = (uint)sizeof(uint);
                mPageRequestFlagsUav = TtEngine.Instance.GfxDevice.RenderContext.CreateUAV(mPageRequestFlagsBuffer, in uavDesc);

                var srvDesc = new FSrvDesc();
                srvDesc.SetBuffer(false);
                srvDesc.Format = EPixelFormat.PXF_UNKNOWN;
                srvDesc.Buffer.NumElements = (uint)nodeCount;
                srvDesc.Buffer.StructureByteStride = (uint)sizeof(uint);
                mPageRequestFlagsSrv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(mPageRequestFlagsBuffer, in srvDesc);

                // CPU-side mirror for readback-driven allocation
                mPageRequestFlagsCpu = new uint[nodeCount];
            }

            // Create layer info buffer
            if (mLayerInfoBuffer == null)
            {
                mLayerInfoBuffer = new TtCpu2GpuBuffer<FAdvShadowLayerData>();
                mLayerInfoBuffer.Initialize(EBufferType.BFT_SRV);
                mLayerInfoBuffer.SetSize(qTree.QTreeBuilder.Layers.Length);

                var layerDataArray = GetLayerDataArray(qTree);
                fixed (FAdvShadowLayerData* ptr = &layerDataArray[0])
                {
                    mLayerInfoBuffer.UpdateData(0, ptr, qTree.QTreeBuilder.Layers.Length * sizeof(FAdvShadowLayerData));
                }
            }
        }

        private FAdvShadowLayerData[] GetLayerDataArray(TtQTree qTree)
        {
            var layers = qTree.QTreeBuilder.Layers;
            var result = new FAdvShadowLayerData[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                result[i].mLayerStartAndSide = new Vector2i(layers[i].LayerStartIndex, layers[i].Side);
                result[i].mLayerGridSize = layers[i].GridSize.AsSingleVector();
            }
            return result;
        }

        /// <summary>
        /// Clear page request flags to zero before each frame's dispatch.
        /// Uses a transient command list to upload zeroed data to the DEFAULT-usage UAV buffer.
        /// </summary>
        private unsafe void ClearPageRequestFlags()
        {
            if (mPageRequestFlagsCpu == null || mPageRequestFlagsBuffer == null)
                return;

            // Zero the CPU-side flags array
            Array.Clear(mPageRequestFlagsCpu, 0, mPageRequestFlagsCpu.Length);

            // Upload zeroed flags to the GPU buffer.
            // Use the no-cmd overload which internally handles staging for DEFAULT buffers.
            fixed (uint* ptr = &mPageRequestFlagsCpu[0])
            {
                mPageRequestFlagsBuffer.UpdateGpuData(0, ptr,
                    (uint)(mPageRequestFlagsCpu.Length * sizeof(uint)));
            }
        }

        /// <summary>
        /// Process the previous frame's readback results to drive PagePool allocation.
        /// Falls back to QTree's dirty list if readback data is not yet available.
        /// </summary>
        private void ProcessReadbackResults(TtAdvanceShadowMapNode advNode)
        {
            var pagePool = advNode.PagePool;
            if (pagePool == null)
                return;

            if (mReadbackResultPrev != null)
            {
                // GPU-driven path: allocate pages that the screen-space marking flagged
                for (int i = 0; i < mReadbackResultPrev.Length; i++)
                {
                    uint flags = mReadbackResultPrev[i];
                    if (flags == 0)
                        continue;

                    if ((flags & (uint)EVSMPageRequestFlags.Requested) != 0)
                    {
                        pagePool.AllocatePage(i);
                    }
                    if ((flags & (uint)EVSMPageRequestFlags.DynamicDirty) != 0)
                    {
                        pagePool.InvalidatePage(i);
                        pagePool.AllocatePage(i);
                    }
                }
            }
            else
            {
                // Fallback: no readback available yet (first frames), use QTree dirty list
                foreach (var qNode in advNode.mShadowQTree.UpdateShadowMapNodes)
                {
                    if (qNode.NodeIndex >= 0)
                        pagePool.AllocatePage(qNode.NodeIndex);
                }
            }
        }

        /// <summary>
        /// Kick an async GPU readback of the page request flags buffer.
        /// The result will be consumed NEXT frame via mReadbackResultPrev.
        /// Uses TtBuffer.AsyncFetchGpuData (FTransientCmd + fence wait on background thread).
        /// </summary>
        private unsafe void KickAsyncReadback()
        {
            if (mPageRequestFlagsBuffer == null || mReadbackInFlight)
                return;

            mReadbackInFlight = true;
            int expectedCount = mPageRequestFlagsCpu.Length;

            // Launch the async readback coroutine
            DoReadbackAsync(expectedCount).AddWaitTask((task) =>
            {
                // Callback fires on main thread after task completes (next Tick cycle).
                // DirectResult is safe to read here per §2.2.
                mReadbackInFlight = false;
            });
        }

        /// <summary>
        /// Async coroutine that performs GPU readback and copies results into mReadbackResultPrev.
        /// </summary>
        private async Thread.Async.TtTask<bool> DoReadbackAsync(int expectedCount)
        {
            using (var blob = new Support.TtBlobObject())
            {
                bool ok = await mPageRequestFlagsBuffer.AsyncFetchGpuData(0, blob.mCoreObject);
                if (!ok)
                    return false;

                unsafe
                {
                    // Per CodingGuidelines §1.5.3 rule 5: blob header is 8 bytes (RowPitch + DepthPitch)
                    const uint kHeader = sizeof(uint) * 2;
                    uint expectedBytes = (uint)(expectedCount * sizeof(uint));

                    if (blob.Size < kHeader + expectedBytes)
                        return false;

                    if (mReadbackResultPrev == null || mReadbackResultPrev.Length != expectedCount)
                        mReadbackResultPrev = new uint[expectedCount];

                    fixed (uint* dst = &mReadbackResultPrev[0])
                    {
                        byte* src = (byte*)blob.DataPointer + kHeader;
                        System.Buffer.MemoryCopy(src, dst, expectedBytes, expectedBytes);
                    }
                }
                return true;
            }
        }

        #endregion
    }
}

using System;
using System.Numerics;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;

namespace EngineNS.Bricks.GpuDriven
{
    /// <summary>
    /// Resolve modes for VisBuffer → Color conversion
    /// </summary>
    public enum EVisBufferResolveMode : uint
    {
        ClusterID = 0,
        MipLevel = 1,
        TriangleID = 2,
        InstanceID = 3,
        NormalLit = 4,
        UVChecker = 5,
        TangentVis = 6,
    }

    #region Shading Environment

    public class TtVisBufferResolveShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtVisBufferResolveShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkVisResolve.compute", RName.ERNameType.Engine);
            MainName = "CS_ResolveVisBuffer";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkVisResolveNode;
            if (node == null) return;

            // Bind VisBuffer64 input (from the VisBuffer node)
            if (node.VisBufferNode != null)
            {
                if (node.VisBufferNode.VisBuffer64.Srv != null)
                    drawcall.BindSrv("VisBuffer64", node.VisBufferNode.VisBuffer64.Srv);

                // Bind vertex/index/cluster data for attribute reconstruction
                if (node.VisBufferNode.VisVertices.Srv != null)
                    drawcall.BindSrv("VisVertexBuffer", node.VisBufferNode.VisVertices.Srv);
                if (node.VisBufferNode.VisIndices.Srv != null)
                    drawcall.BindSrv("VisIndexBuffer", node.VisBufferNode.VisIndices.Srv);
                if (node.VisBufferNode.ClusterVisBuffer.Srv != null)
                    drawcall.BindSrv("ClusterVisBuffer", node.VisBufferNode.ClusterVisBuffer.Srv);
            }

            // Bind output textures
            var colorUav = node.OverrideColorUav ?? node.GetAttachBuffer(node.ColorRTPinOut)?.Uav;
            if (colorUav != null)
                drawcall.BindUav("ResolveColorRT", colorUav);

            var depthUav = node.OverrideDepthUav ?? node.GetAttachBuffer(node.DepthRTPinOut)?.Uav;
            if (depthUav != null)
                drawcall.BindUav("ResolveDepthRT", depthUav);

            // Bind cbuffer
            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbResolveParams");
            if (binder.IsValidPointer)
            {
                if (node.CBResolveParams == null)
                {
                    node.CBResolveParams = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBResolveParams.SetValue("ResolveParams", in node.mResolveParams);
                    node.CBResolveParams.MarkDirty();
                    node.CBResolveParams.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBResolveParams);
            }
        }
    }

    #endregion

    /// <summary>
    /// RenderGraph node that resolves the 64-bit VisBuffer into ColorRT and DepthRT.
    /// Phase 1: Simple ClusterID hash coloring.
    /// Future: barycentric coordinate interpolation + material-based shading.
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("QuarkVisResolve", "Quark\\QuarkVisResolve", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtQuarkVisResolveNode : TAuxRenderGraphNode<TtQuarkVisResolveNode>
    {
        // Input pin: connects to VisBufferNode.VisBufferPinOut for graph execution ordering
        public TtRenderGraphPin VisBufferPinIn = TtRenderGraphPin.CreateInput("VisBuffer", EBufferType.BFT_UAV | EBufferType.BFT_SRV);

        public TtRenderGraphPin ColorRTPinOut = TtRenderGraphPin.CreateOutput("ColorRT", true,
            EPixelFormat.PXF_R8G8B8A8_UNORM, EBufferType.BFT_UAV | EBufferType.BFT_SRV);

        public TtRenderGraphPin DepthRTPinOut = TtRenderGraphPin.CreateOutput("DepthRT", true,
            EPixelFormat.PXF_R32_FLOAT, EBufferType.BFT_UAV | EBufferType.BFT_SRV);

        /// <summary>
        /// Override UAVs for standalone (editor debug) mode.
        /// </summary>
        public TtUaView OverrideColorUav;
        public TtUaView OverrideDepthUav;

        /// <summary>
        /// Reference to the VisBuffer node that produces the 64-bit buffer.
        /// Must be set before Tick.
        /// </summary>
        public TtQuarkVisBufferNode VisBufferNode;

        #region Parameters

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FResolveParams
        {
            public Vector2 RTSize;
            public uint ResolveMode;
            public uint ClusterCount;
            public uint VertexStride;   // 8 or 12 (in floats)
            public uint HasTangents;    // 0 or 1
            public uint Padding0;
            public uint Padding1;
        }

        public FResolveParams mResolveParams;
        public TtCbView CBResolveParams;

        private EVisBufferResolveMode mResolveMode = EVisBufferResolveMode.ClusterID;
        public EVisBufferResolveMode ResolveMode
        {
            get => mResolveMode;
            set
            {
                if (mResolveMode == value) return;
                mResolveMode = value;
                mResolveParams.ResolveMode = (uint)value;
            }
        }

        #endregion

        #region Shading

        private TtVisBufferResolveShading mResolveShading;
        private TtComputeDraw mResolveDrawcall;

        #endregion

        private uint mWidth = 0;
        private uint mHeight = 0;

        public TtQuarkVisResolveNode()
        {
            Name = "QuarkVisResolveNode";
            mResolveParams.ResolveMode = (uint)EVisBufferResolveMode.ClusterID;
        }

        public override void InitNodePins()
        {
            AddInput(VisBufferPinIn);
            AddOutput(ColorRTPinOut);
            AddOutput(DepthRTPinOut);
            base.InitNodePins();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);
            mWidth = (uint)x;
            mHeight = (uint)y;
            mResolveParams.RTSize = new Vector2(x, y);
        }

        /// <summary>
        /// Set size for standalone mode (no render graph resize).
        /// </summary>
        public void SetResolveSize(uint width, uint height)
        {
            if (width == mWidth && height == mHeight)
                return;
            mWidth = width;
            mHeight = height;
            mResolveParams.RTSize = new Vector2(width, height);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            await InitializeStandalone();

            // Auto-link VisBufferNode from graph connection if not already set
            if (VisBufferNode == null && policy != null)
            {
                var linker = VisBufferPinIn.FindInLinker();
                if (linker != null && linker.OutPin.HostNode is TtQuarkVisBufferNode vbNode)
                {
                    VisBufferNode = vbNode;
                }
            }
        }

        /// <summary>
        /// Initialize shading env and drawcall (standalone mode).
        /// </summary>
        public async Thread.Async.TtTask InitializeStandalone()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            CoreSDK.DisposeObject(ref mResolveDrawcall);
            mResolveDrawcall = rc.CreateComputeDraw();
            mResolveShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVisBufferResolveShading>();
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mResolveDrawcall);
            CoreSDK.DisposeObject(ref CBResolveParams);
            base.Dispose();
        }

        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            TickInternal(policy);
        }

        /// <summary>
        /// Standalone tick for editor debug usage.
        /// </summary>
        public unsafe void TickStandalone()
        {
            TickInternal(null);
        }

        private unsafe void TickInternal(TtRenderPolicy policy)
        {
            if (mWidth == 0 || mHeight == 0)
                return;
            if (VisBufferNode == null || VisBufferNode.VisBuffer64.Srv == null)
                return;

            // Sync ClusterCount and stride info from VisBuffer node
            if (VisBufferNode != null)
            {
                mResolveParams.ClusterCount = VisBufferNode.mVisBufferParams.ClusterCount;
                mResolveParams.VertexStride = VisBufferNode.VertexStride;
                mResolveParams.HasTangents = VisBufferNode.HasTangents ? 1u : 0u;
            }

            // Update cbuffer
            if (CBResolveParams != null)
            {
                CBResolveParams.SetValue("ResolveParams", in mResolveParams);
            }

            var cmd = TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmd, "QuarkVisResolve"))
            {
                mResolveShading.SetDrawcallDispatch(this, policy, mResolveDrawcall,
                    mWidth, mHeight, 1, true);
                cmd.PushGpuDraw(mResolveDrawcall);
                cmd.FlushDraws();
            }

            if (policy != null)
                policy.CommitCommandList(cmd, "QuarkVisResolve");
            else
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmd, "QuarkVisResolve");
        }
    }
}

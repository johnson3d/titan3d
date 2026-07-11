using System;
using System.Collections.Generic;
using System.Numerics;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;

namespace EngineNS.Bricks.GpuDriven
{
    /// <summary>
    /// Visualization modes for the Quark DAG debug view
    /// </summary>
    public enum EQuarkVisMode : uint
    {
        ClusterID = 0,
        MipLevel = 1,
        LODError = 2,
    }

    /// <summary>
    /// Extended cluster data for visualization (matches HLSL FClusterVisData)
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FClusterVisData
    {
        public Vector3 BoundMin;
        public int IndexStart;
        public Vector3 BoundMax;
        public int IndexEnd;
        public Matrix WVPMatrix;
        public uint ClusterID;
        public uint MipLevel;
        public float LODError;
        public float MaxLODError;
        public uint MaterialID;
        public uint InstanceID;
        public uint VertexStart;
        public uint Padding;
    }

    /// <summary>
    /// BVH box data for wireframe visualization (matches HLSL FBVHBoxData)
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FBVHBoxData
    {
        public Vector3 Center;
        public float LODError;
        public Vector3 Extent;
        public uint Level;
    }

    #region Shading Environments

    public class TtQuarkVisClearShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtQuarkVisClearShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkDAGVisualize.compute", RName.ERNameType.Engine);
            MainName = "CS_ClearVisTexture";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkDAGVisualizeNode;
            if (node == null) return;

            var uav = node.OverrideOutputUav ?? node.GetAttachBuffer(node.VisRTPinOut)?.Uav;
            if (uav == null) return;
            drawcall.BindUav("VisOutputTexture", uav);

            var depthUav = node.OverrideDepthUav ?? node.GetAttachBuffer(node.VisDepthPinOut)?.Uav;
            if (depthUav != null)
                drawcall.BindUav("VisDepthTexture", depthUav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVisParams");
            if (binder.IsValidPointer)
            {
                if (node.CBVisParams == null)
                {
                    node.CBVisParams = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBVisParams.SetValue("VisParams", in node.mVisParams);
                    node.CBVisParams.MarkDirty();
                    node.CBVisParams.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBVisParams);
            }
        }
    }

    public class TtQuarkVisDAGShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);

        public TtQuarkVisDAGShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkDAGVisualize.compute", RName.ERNameType.Engine);
            MainName = "CS_VisualizeDAG";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkDAGVisualizeNode;
            if (node == null) return;

            drawcall.BindSrv("ClusterVisBuffer", node.ClusterVisBuffer.Srv);
            drawcall.BindSrv("VisVertexBuffer", node.VisVertices.Srv);
            drawcall.BindSrv("VisIndexBuffer", node.VisIndices.Srv);
            var uav = node.OverrideOutputUav ?? node.GetAttachBuffer(node.VisRTPinOut)?.Uav;
            if (uav == null) return;
            drawcall.BindUav("VisOutputTexture", uav);

            var depthUav = node.OverrideDepthUav ?? node.GetAttachBuffer(node.VisDepthPinOut)?.Uav;
            if (depthUav != null)
                drawcall.BindUav("VisDepthTexture", depthUav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVisParams");
            if (binder.IsValidPointer)
            {
                if (node.CBVisParams == null)
                {
                    node.CBVisParams = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBVisParams.SetValue("VisParams", in node.mVisParams);
                    node.CBVisParams.MarkDirty();
                    node.CBVisParams.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBVisParams);
            }
        }
    }

    public class TtQuarkVisBVHShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);

        public TtQuarkVisBVHShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkDAGVisualize.compute", RName.ERNameType.Engine);
            MainName = "CS_DrawBVHBoxes";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkDAGVisualizeNode;
            if (node == null) return;

            drawcall.BindSrv("BVHBoxBuffer", node.BVHBoxBuffer.Srv);
            var uav = node.OverrideOutputUav ?? node.GetAttachBuffer(node.VisRTPinOut)?.Uav;
            if (uav == null) return;
            drawcall.BindUav("VisOutputTexture", uav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbBVHParams");
            if (binder.IsValidPointer)
            {
                if (node.CBBVHParams == null)
                {
                    node.CBBVHParams = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBBVHParams.SetValue("BVHRTSize", in node.mBVHRTSize);
                    node.CBBVHParams.SetValue("BVHBoxCount", in node.mBVHBoxCount);
                    node.CBBVHParams.SetValue("BVHFilterLevel", in node.mBVHFilterLevel);
                    node.CBBVHParams.SetValue("BVHViewProj", in node.mBVHViewProj);
                    node.CBBVHParams.MarkDirty();
                    node.CBBVHParams.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBBVHParams);
            }
        }
    }

    #endregion

    /// <summary>
    /// RenderGraph node for Quark DAG visualization.
    /// Supports: Cluster coloring by ID/MipLevel/LODError, BVH wireframe, MipLevel filtering.
    /// Can also run standalone (without RenderGraph) by setting OverrideOutputUav.
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("QuarkVisualize", "Quark\\QuarkVisualize", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtQuarkDAGVisualizeNode : TAuxRenderGraphNode<TtQuarkDAGVisualizeNode>
    {
        public TtRenderGraphPin VisRTPinOut = TtRenderGraphPin.CreateOutput("VisRT", true,
            EPixelFormat.PXF_R8G8B8A8_UNORM, EBufferType.BFT_UAV | EBufferType.BFT_SRV);

        // Depth buffer (R32_UINT UAV) for atomic depth test in software rasterizer
        public TtRenderGraphPin VisDepthPinOut = TtRenderGraphPin.CreateOutput("VisDepth", true,
            EPixelFormat.PXF_R32_UINT, EBufferType.BFT_UAV);

        /// <summary>
        /// When set, shading envs bind this UAV instead of the RenderGraph-managed buffer.
        /// Used for standalone (editor debug) mode.
        /// </summary>
        public NxRHI.TtUaView OverrideOutputUav;
        public NxRHI.TtUaView OverrideDepthUav;

        #region Visualization Parameters

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FVisParams
        {
            public Vector2 RTSize;
            public uint VisMode;
            public uint FilterMipLevel;
            public uint ClusterCount;
            public uint ShowAllMips;
            public Vector2 padding;
        }

        public FVisParams mVisParams;
        public TtCbView CBVisParams;

        public Vector2 mBVHRTSize;
        public uint mBVHBoxCount;
        public uint mBVHFilterLevel;
        public Matrix mBVHViewProj;
        public TtCbView CBBVHParams;

        private EQuarkVisMode mVisMode = EQuarkVisMode.MipLevel;
        public EQuarkVisMode VisMode
        {
            get => mVisMode;
            set { mVisMode = value; mVisParams.VisMode = (uint)value; }
        }

        private uint mFilterMipLevel = 0;
        public uint FilterMipLevel
        {
            get => mFilterMipLevel;
            set { mFilterMipLevel = value; mVisParams.FilterMipLevel = value; }
        }

        private bool mShowAllMips = true;
        public bool ShowAllMips
        {
            get => mShowAllMips;
            set { mShowAllMips = value; mVisParams.ShowAllMips = value ? 1u : 0u; }
        }

        private bool mShowBVH = false;
        public bool ShowBVH
        {
            get => mShowBVH;
            set => mShowBVH = value;
        }

        private uint mBVHDisplayLevel = 0;
        public uint BVHDisplayLevel
        {
            get => mBVHDisplayLevel;
            set { mBVHDisplayLevel = value; mBVHFilterLevel = value; }
        }

        public uint MaxMipLevel { get; private set; } = 0;

        #endregion

        #region GPU Buffers

        public TtCpu2GpuBuffer<float> VisVertices = new TtCpu2GpuBuffer<float>();
        public TtCpu2GpuBuffer<uint> VisIndices = new TtCpu2GpuBuffer<uint>();
        public TtCpu2GpuBuffer<FClusterVisData> ClusterVisBuffer = new TtCpu2GpuBuffer<FClusterVisData>();
        public TtCpu2GpuBuffer<FBVHBoxData> BVHBoxBuffer = new TtCpu2GpuBuffer<FBVHBoxData>();

        #endregion

        #region Shading

        private TtQuarkVisClearShading mClearShading;
        private TtComputeDraw mClearDrawcall;

        private TtQuarkVisDAGShading mDAGShading;
        private TtComputeDraw mDAGDrawcall;

        private TtQuarkVisBVHShading mBVHShading;
        private TtComputeDraw mBVHDrawcall;

        #endregion

        private bool mDataDirty = false;
        private float mMaxLODError = 1.0f;

        public TtQuarkDAGVisualizeNode()
        {
            Name = "QuarkDAGVisualizeNode";
            mVisParams.ShowAllMips = 1;
            mVisParams.VisMode = (uint)EQuarkVisMode.MipLevel;
        }

        public override void InitNodePins()
        {
            AddOutput(VisRTPinOut);
            AddOutput(VisDepthPinOut);
            base.InitNodePins();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);
            mVisParams.RTSize = new Vector2(x, y);
            mBVHRTSize = new Vector2(x, y);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            await InitializeStandalone();
        }

        /// <summary>
        /// Initialize shading envs and buffers without a render policy (standalone mode).
        /// </summary>
        public async Thread.Async.TtTask InitializeStandalone()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            // Clear pass
            CoreSDK.DisposeObject(ref mClearDrawcall);
            mClearDrawcall = rc.CreateComputeDraw();
            mClearShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtQuarkVisClearShading>();

            // DAG visualization pass
            CoreSDK.DisposeObject(ref mDAGDrawcall);
            mDAGDrawcall = rc.CreateComputeDraw();
            mDAGShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtQuarkVisDAGShading>();

            // BVH wireframe pass
            CoreSDK.DisposeObject(ref mBVHDrawcall);
            mBVHDrawcall = rc.CreateComputeDraw();
            mBVHShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtQuarkVisBVHShading>();

            // Initialize buffers
            VisVertices.Initialize(EBufferType.BFT_SRV);
            VisIndices.Initialize(EBufferType.BFT_SRV);
            ClusterVisBuffer.Initialize(EBufferType.BFT_SRV);
            BVHBoxBuffer.Initialize(EBufferType.BFT_SRV);
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mClearDrawcall);
            CoreSDK.DisposeObject(ref mDAGDrawcall);
            CoreSDK.DisposeObject(ref mBVHDrawcall);
            CoreSDK.DisposeObject(ref CBVisParams);
            CoreSDK.DisposeObject(ref CBBVHParams);
            base.Dispose();
        }

        /// <summary>
        /// Upload DAG cluster data for visualization.
        /// Call this after BuildQuarkDAG completes to populate the visualization buffers.
        /// </summary>
        public unsafe void UploadDAGData(Graphics.Mesh.TtMeshPrimitives mesh, TtCamera camera)
        {
            if (mesh == null || !mesh.mCoreObject.IsValidPointer)
                return;

            var coreObj = mesh.mCoreObject;
            uint clusterCount = coreObj.GetClusterCount();
            if (clusterCount == 0)
                return;

            uint dagMipLevels = coreObj.GetDAGMipLevels();
            MaxMipLevel = dagMipLevels > 0 ? dagMipLevels - 1 : 0;

            // Find max LOD error for normalization
            mMaxLODError = 0.001f;
            for (uint i = 0; i < clusterCount; i++)
            {
                float err = coreObj.GetClusterLODError((int)i);
                if (err > mMaxLODError)
                    mMaxLODError = err;
            }

            // Upload vertex buffer (Position + Normal + UV = 8 floats per vertex)
            uint vbCount = coreObj.GetClustersVBCount();
            if (vbCount > 0)
            {
                int floatCount = (int)(vbCount * coreObj.GetClustersVBStride());
                VisVertices.SetSize(floatCount);
                var vbPtr = coreObj.GetClustersVB();
                // Direct copy: C++ stores full vertex data (stride=8 or 12)
                VisVertices.UpdateData(0, vbPtr, floatCount * sizeof(float));
            }

            // Upload index buffer
            uint ibCount = coreObj.GetClustersIBCount();
            if (ibCount > 0)
            {
                VisIndices.SetSize((int)ibCount);
                var ibPtr = coreObj.GetClustersIB();
                VisIndices.UpdateData(0, ibPtr, (int)ibCount * sizeof(uint));
            }

            // Upload cluster visualization data
            // Use ViewProjection (no viewport transform) - shader will map NDC to RTSize
            // Matrix is row-major in C# (M11,M12...) matching StructuredBuffer row-major layout
            // mul(float4(pos,1), M) in HLSL computes pos*M correctly without transpose
            var viewProjMatrix = camera.GetViewProjection();
            var clusterArray = new FClusterVisData[clusterCount];
            for (uint i = 0; i < clusterCount; i++)
            {
                var cluster = coreObj.GetCluster((int)i);
                clusterArray[i].BoundMin = new Vector3(cluster.Bounds.Minimum.X, cluster.Bounds.Minimum.Y, cluster.Bounds.Minimum.Z);
                clusterArray[i].BoundMax = new Vector3(cluster.Bounds.Maximum.X, cluster.Bounds.Maximum.Y, cluster.Bounds.Maximum.Z);
                clusterArray[i].IndexStart = cluster.IndexStart;
                clusterArray[i].IndexEnd = cluster.IndexStart + cluster.IndexCount;
                clusterArray[i].WVPMatrix = viewProjMatrix;
                clusterArray[i].ClusterID = i;
                clusterArray[i].MipLevel = (uint)coreObj.GetClusterMipLevel((int)i);
                clusterArray[i].LODError = coreObj.GetClusterLODError((int)i);
                clusterArray[i].MaxLODError = mMaxLODError;
                clusterArray[i].MaterialID = (uint)cluster.PrimaryMaterialID;
                clusterArray[i].InstanceID = 0;
                clusterArray[i].VertexStart = (uint)cluster.VertexStart;
                clusterArray[i].Padding = 0;
            }

            int clusterBufSize = (int)clusterCount;
            ClusterVisBuffer.SetSize(clusterBufSize);
            fixed (FClusterVisData* pData = &clusterArray[0])
            {
                ClusterVisBuffer.UpdateData(0, pData, (int)clusterCount * sizeof(FClusterVisData));
            }

            mVisParams.ClusterCount = clusterCount;
            mDataDirty = true;
        }

        /// <summary>
        /// Lightweight per-frame camera update. Only rewrites WVPMatrix in ClusterVisBuffer.
        /// Call this every frame when the viewport camera moves.
        /// </summary>
        public unsafe void UpdateCamera(TtCamera camera)
        {
            if (camera == null || mVisParams.ClusterCount == 0)
                return;

            var viewProjMatrix = camera.GetViewProjection();
            uint clusterCount = mVisParams.ClusterCount;

            // Update WVPMatrix for each cluster entry in-place
            // FClusterVisData layout: BoundMin(12) + IndexStart(4) + BoundMax(12) + IndexEnd(4) + WVPMatrix(64) + tail(16) = 112 bytes
            // WVPMatrix offset within struct = 32 bytes
            int structSize = sizeof(FClusterVisData);
            for (uint i = 0; i < clusterCount; i++)
            {
                int byteOffset = (int)i * structSize + 32; // 32 = offset of WVPMatrix
                ClusterVisBuffer.UpdateData(byteOffset, &viewProjMatrix, sizeof(Matrix));
            }

            // Also update BVH view proj (same camera, but for cbuffer - needs transpose for HLSL column-major cbuffer convention)
            var bvhVP = viewProjMatrix;
            bvhVP.Transpose();
            mBVHViewProj = bvhVP;

            mDataDirty = true;
        }

        /// <summary>
        /// Upload BVH box data for wireframe visualization.
        /// </summary>
        public unsafe void UploadBVHData(List<FBVHBoxData> boxes)
        {
            if (boxes == null || boxes.Count == 0)
                return;

            int bufSize = boxes.Count;
            BVHBoxBuffer.SetSize(bufSize);
            var data = new FBVHBoxData[boxes.Count];
            for (int i = 0; i < boxes.Count; i++)
            {
                data[i] = boxes[i];
            }
            fixed (FBVHBoxData* pData = &data[0])
            {
                BVHBoxBuffer.UpdateData(0, pData, boxes.Count * sizeof(FBVHBoxData));
            }
            mBVHBoxCount = (uint)boxes.Count;
            mDataDirty = true;
        }

        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            TickInternal(policy);
        }

        /// <summary>
        /// Standalone tick for editor debug usage. Call this each frame when not embedded in a RenderGraph.
        /// </summary>
        public unsafe void TickStandalone()
        {
            TickInternal(null);
        }

        private unsafe void TickInternal(TtRenderPolicy policy)
        {
            if (mVisParams.ClusterCount == 0)
                return;

            // Flush GPU buffers if data is dirty
            if (mDataDirty)
            {
                using (var tsCmd = new FTransientCmd(EQueueType.QU_Default, "QuarkVis.FlushBuffers"))
                {
                    VisVertices.Flush2GPU(tsCmd.CmdList);
                    VisIndices.Flush2GPU(tsCmd.CmdList);
                    ClusterVisBuffer.Flush2GPU(tsCmd.CmdList);
                    if (mBVHBoxCount > 0)
                        BVHBoxBuffer.Flush2GPU(tsCmd.CmdList);
                }
                mDataDirty = false;
            }

            // Update cbuffer values
            if (CBVisParams != null)
            {
                CBVisParams.SetValue("VisParams", in mVisParams);
            }

            if (CBBVHParams != null && mShowBVH)
            {
                CBBVHParams.SetValue("BVHRTSize", in mBVHRTSize);
                CBBVHParams.SetValue("BVHBoxCount", in mBVHBoxCount);
                CBBVHParams.SetValue("BVHFilterLevel", in mBVHFilterLevel);
                CBBVHParams.SetValue("BVHViewProj", in mBVHViewProj);
            }

            var cmd = TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmd, "QuarkDAGVisualize"))
            {
                // Pass 1: Clear texture
                mClearShading.SetDrawcallDispatch(this, policy, mClearDrawcall,
                    (uint)mVisParams.RTSize.X, (uint)mVisParams.RTSize.Y, 1, true);
                cmd.PushGpuDraw(mClearDrawcall);

                // Pass 2: Rasterize clusters with debug coloring
                mDAGShading.SetDrawcallDispatch(this, policy, mDAGDrawcall,
                    mVisParams.ClusterCount, 1, 1, true);
                cmd.PushGpuDraw(mDAGDrawcall);

                // Pass 3: BVH wireframe (optional)
                if (mShowBVH && mBVHBoxCount > 0)
                {
                    mBVHShading.SetDrawcallDispatch(this, policy, mBVHDrawcall,
                        mBVHBoxCount, 1, 1, true);
                    cmd.PushGpuDraw(mBVHDrawcall);
                }

                cmd.FlushDraws();
            }

            if (policy != null)
                policy.CommitCommandList(cmd, "QuarkDAGVisualize");
            else
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmd, "QuarkDAGVisualize");
        }
    }
}

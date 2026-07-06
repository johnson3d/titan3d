using System;
using System.Collections.Generic;
using System.Numerics;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;

namespace EngineNS.Bricks.GpuDriven
{
    #region Shading Environments

    public class TtVisBufferClearShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtVisBufferClearShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkVisBuffer.compute", RName.ERNameType.Engine);
            MainName = "CS_ClearVisBuffer";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkVisBufferNode;
            if (node == null) return;

            drawcall.BindUav("VisBuffer64", node.VisBuffer64.Uav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVisBuffer");
            if (binder.IsValidPointer)
            {
                if (node.CBVisBuffer == null)
                {
                    node.CBVisBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBVisBuffer.SetValue("VisBufferParams", in node.mVisBufferParams);
                    node.CBVisBuffer.MarkDirty();
                    node.CBVisBuffer.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBVisBuffer);
            }
        }
    }

    public class TtVisBufferRasterizeShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(128, 1, 1);

        public TtVisBufferRasterizeShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkVisBuffer.compute", RName.ERNameType.Engine);
            MainName = "CS_RasterizeVisBuffer";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkVisBufferNode;
            if (node == null) return;

            drawcall.BindSrv("ClusterVisBuffer", node.ClusterVisBuffer.Srv);
            drawcall.BindSrv("VisVertexBuffer", node.VisVertices.Srv);
            drawcall.BindSrv("VisIndexBuffer", node.VisIndices.Srv);
            drawcall.BindUav("VisBuffer64", node.VisBuffer64.Uav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVisBuffer");
            if (binder.IsValidPointer)
            {
                if (node.CBVisBuffer == null)
                {
                    node.CBVisBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBVisBuffer.SetValue("VisBufferParams", in node.mVisBufferParams);
                    node.CBVisBuffer.MarkDirty();
                    node.CBVisBuffer.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBVisBuffer);
            }
        }
    }

    #endregion

    /// <summary>
    /// RenderGraph node for 64-bit Visibility Buffer generation.
    /// Pass 1: Clear VisBuffer64 to (0, 0xFFFFFFFF)
    /// Pass 2: Software rasterize clusters, atomic depth test + ID write
    /// Output: VisBuffer64 (RWByteAddressBuffer, 8 bytes/pixel = [PackedID_lo32 | Depth_hi32])
    /// The resolve node (TtQuarkVisResolveNode) reads VisBuffer64 to produce ColorRT + DepthRT.
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("QuarkVisBuffer", "Quark\\QuarkVisBuffer", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtQuarkVisBufferNode : TAuxRenderGraphNode<TtQuarkVisBufferNode>
    {
        // Output pin for rpolicy graph dependency (Resolve node connects its input here)
        public TtRenderGraphPin VisBufferPinOut = TtRenderGraphPin.CreateOutput("VisBuffer", true,
            EPixelFormat.PXF_R8_UINT, EBufferType.BFT_UAV | EBufferType.BFT_SRV);
        #region Visualization Parameters

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FVisBufferParams
        {
            public Vector2 RTSize;
            public uint ClusterCount;
            public uint VertexStride;  // 8 or 12 (in floats)
            public uint CullMode;     // 0=None(double-sided), 1=CCW front(default), 2=CW front
            public uint Padding0;
            public uint Padding1;
            public uint Padding2;
        }

        public FVisBufferParams mVisBufferParams;
        public TtCbView CBVisBuffer;

        #endregion

        #region GPU Buffers

        /// <summary>
        /// 64-bit VisBuffer: RWByteAddressBuffer, size = Width * Height * 8 bytes.
        /// Layout per pixel: [lo32 = PackedID, hi32 = QuantizedDepth]
        /// PackedID = InstanceID:8 | ClusterID:16 | TriangleID:8
        /// </summary>
        public TtGpuBuffer<uint> VisBuffer64 = new TtGpuBuffer<uint>();

        public TtCpu2GpuBuffer<float> VisVertices = new TtCpu2GpuBuffer<float>();
        public TtCpu2GpuBuffer<uint> VisIndices = new TtCpu2GpuBuffer<uint>();
        public TtCpu2GpuBuffer<FClusterVisData> ClusterVisBuffer = new TtCpu2GpuBuffer<FClusterVisData>();

        #endregion

        #region Shading

        private TtVisBufferClearShading mClearShading;
        private TtComputeDraw mClearDrawcall;

        private TtVisBufferRasterizeShading mRasterizeShading;
        private TtComputeDraw mRasterizeDrawcall;

        #endregion

        private bool mDataDirty = false;
        private uint mWidth = 0;
        private uint mHeight = 0;

        /// <summary>
        /// Per-mesh vertex stride (8 = no tangent, 12 = with tangent). Updated on UploadDAGData.
        /// </summary>
        public uint VertexStride { get; private set; } = 8;

        /// <summary>
        /// Whether the uploaded mesh has explicit tangent data.
        /// </summary>
        public bool HasTangents { get; private set; } = false;

        public TtQuarkVisBufferNode()
        {
            Name = "QuarkVisBufferNode";
            mVisBufferParams.CullMode = 1; // CCW front (default)
        }

        public override void InitNodePins()
        {
            AddOutput(VisBufferPinOut);
            base.InitNodePins();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);
            ResizeVisBuffer((uint)x, (uint)y);
        }

        /// <summary>
        /// Resize the VisBuffer64 to match viewport dimensions.
        /// </summary>
        public unsafe void ResizeVisBuffer(uint width, uint height)
        {
            if (width == mWidth && height == mHeight)
                return;
            if (width == 0 || height == 0)
                return;

            mWidth = width;
            mHeight = height;
            mVisBufferParams.RTSize = new Vector2(width, height);

            // VisBuffer64 = width * height * 2 uints (8 bytes per pixel)
            uint pixelCount = width * height;
            VisBuffer64.SetSize(pixelCount * 2, null, EBufferType.BFT_UAV | EBufferType.BFT_SRV);
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
            mClearShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVisBufferClearShading>();

            // Rasterize pass
            CoreSDK.DisposeObject(ref mRasterizeDrawcall);
            mRasterizeDrawcall = rc.CreateComputeDraw();
            mRasterizeShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVisBufferRasterizeShading>();

            // Initialize CPU→GPU buffers
            VisVertices.Initialize(EBufferType.BFT_SRV);
            VisIndices.Initialize(EBufferType.BFT_SRV);
            ClusterVisBuffer.Initialize(EBufferType.BFT_SRV);
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mClearDrawcall);
            CoreSDK.DisposeObject(ref mRasterizeDrawcall);
            CoreSDK.DisposeObject(ref CBVisBuffer);
            VisBuffer64?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Upload DAG cluster data for VisBuffer rasterization.
        /// Call after BuildQuarkDAG to populate vertex/index/cluster buffers.
        /// </summary>
        public unsafe void UploadDAGData(Graphics.Mesh.TtMeshPrimitives mesh, TtCamera camera)
        {
            if (mesh == null || !mesh.mCoreObject.IsValidPointer)
                return;

            var coreObj = mesh.mCoreObject;
            uint clusterCount = coreObj.GetClusterCount();
            if (clusterCount == 0)
                return;

            // Upload vertex buffer (dynamic stride: 8 or 12 floats per vertex)
            uint vbCount = coreObj.GetClustersVBCount();
            uint stride = coreObj.GetClustersVBStride();
            VertexStride = stride;
            HasTangents = coreObj.GetClustersHasTangents();
            if (vbCount > 0)
            {
                int floatCount = (int)(vbCount * stride);
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

            // Upload cluster data
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
                clusterArray[i].MaxLODError = 1.0f;
                clusterArray[i].MaterialID = (uint)cluster.PrimaryMaterialID;
                clusterArray[i].InstanceID = 0;
                clusterArray[i].VertexStart = (uint)cluster.VertexStart;
                clusterArray[i].Padding = 0;
            }

            ClusterVisBuffer.SetSize((int)clusterCount);
            fixed (FClusterVisData* pData = &clusterArray[0])
            {
                ClusterVisBuffer.UpdateData(0, pData, (int)clusterCount * sizeof(FClusterVisData));
            }

            mVisBufferParams.ClusterCount = clusterCount;
            mVisBufferParams.VertexStride = stride;
            mDataDirty = true;
        }

        /// <summary>
        /// Per-frame camera update. Rewrites WVPMatrix in ClusterVisBuffer.
        /// </summary>
        public unsafe void UpdateCamera(TtCamera camera)
        {
            if (camera == null || mVisBufferParams.ClusterCount == 0)
                return;

            var viewProjMatrix = camera.GetViewProjection();
            uint clusterCount = mVisBufferParams.ClusterCount;

            int structSize = sizeof(FClusterVisData);
            for (uint i = 0; i < clusterCount; i++)
            {
                int byteOffset = (int)i * structSize + 32; // 32 = offset of WVPMatrix in FClusterVisData
                ClusterVisBuffer.UpdateData(byteOffset, &viewProjMatrix, sizeof(Matrix));
            }

            mDataDirty = true;
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
            if (mVisBufferParams.ClusterCount == 0 || mWidth == 0 || mHeight == 0)
                return;

            if (VisBuffer64.Uav == null)
                return;

            // Flush GPU buffers if data is dirty
            if (mDataDirty)
            {
                using (var tsCmd = new FTransientCmd(EQueueType.QU_Default, "QuarkVisBuffer.FlushBuffers"))
                {
                    VisVertices.Flush2GPU(tsCmd.CmdList);
                    VisIndices.Flush2GPU(tsCmd.CmdList);
                    ClusterVisBuffer.Flush2GPU(tsCmd.CmdList);
                }
                mDataDirty = false;
            }

            // Update cbuffer values
            if (CBVisBuffer != null)
            {
                CBVisBuffer.SetValue("VisBufferParams", in mVisBufferParams);
            }

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new TtCmdListScope(cmd, "QuarkVisBuffer"))
            {
                // Pass 1: Clear VisBuffer64
                mClearShading.SetDrawcallDispatch(this, policy, mClearDrawcall,
                    mWidth, mHeight, 1, true);
                cmd.PushGpuDraw(mClearDrawcall);

                // Pass 2: Rasterize clusters into VisBuffer64
                // Each group = one cluster (128 threads per group handle triangles)
                // Pass group count directly (bRoundupXYZ=false) since ClusterCount IS the group count
                mRasterizeShading.SetDrawcallDispatch(this, policy, mRasterizeDrawcall,
                    mVisBufferParams.ClusterCount, 1, 1, false);
                cmd.PushGpuDraw(mRasterizeDrawcall);

                cmd.FlushDraws();
            }

            if (policy != null)
                policy.CommitCommandList(cmd, "QuarkVisBuffer");
            else
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmd, "QuarkVisBuffer");
        }
    }
}

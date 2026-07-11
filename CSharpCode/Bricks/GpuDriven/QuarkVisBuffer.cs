using System;
using System.Collections.Generic;
using System.Numerics;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;

namespace EngineNS.Bricks.GpuDriven
{
    /// <summary>
    /// GPU-friendly flattened group data for LOD cut selection (matches FClusterGroupGPU in QuarkLODSelect.compute).
    /// Size = 48 bytes = 12 ints.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FClusterGroupGPU
    {
        public Vector3 LODBoundsCenter;
        public float LODBoundsRadius;
        public float ParentLODError;
        public int MipLevel;
        public int ChildrenStart;
        public int ChildrenCount;
        public int ParentsStart;
        public int ParentsCount;
        public int Padding0;
        public int Padding1;
    }

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

            // Bind SelectedClusters for LOD-driven indirect mode
            if (node.LODSelectionEnabled && node.SelectedClusters.Srv != null)
            {
                drawcall.BindSrv("SelectedClusters", node.SelectedClusters.Srv);
            }

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

    #region LOD Selection Shading Environments

    public class TtLODSelectShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);

        public TtLODSelectShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkLODSelect.compute", RName.ERNameType.Engine);
            MainName = "CS_LODSelect";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkVisBufferNode;
            if (node == null) return;

            drawcall.BindSrv("GroupBuffer", node.GroupBuffer.Srv);
            drawcall.BindSrv("GroupChildrenBuffer", node.GroupChildrenBuffer.Srv);
            drawcall.BindSrv("GroupParentsBuffer", node.GroupParentsBuffer.Srv);
            drawcall.BindSrv("ClusterGroupMap", node.ClusterGroupMapBuffer.Srv);

            // Ping-pong: bind current/next based on pass parity
            bool evenPass = (node.mCurrentPassIndex % 2) == 0;
            drawcall.BindUav("WorkQueueCurrent", evenPass ? node.WorkQueueA.Uav : node.WorkQueueB.Uav);
            drawcall.BindUav("WorkQueueNext", evenPass ? node.WorkQueueB.Uav : node.WorkQueueA.Uav);
            drawcall.BindUav("SelectedClusters", node.SelectedClusters.Uav);
            drawcall.BindUav("LODIndirectArgs", node.LODIndirectArgs.Uav);
            drawcall.BindUav("GroupVisited", node.GroupVisited.Uav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbLODSelect");
            if (binder.IsValidPointer)
            {
                if (node.CBLODSelect == null)
                {
                    node.CBLODSelect = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBLODSelect.SetValue("LODSelectParams", in node.mLODSelectParams);
                    node.CBLODSelect.MarkDirty();
                    node.CBLODSelect.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBLODSelect);
            }
        }
    }

    public class TtLODPrepareArgsShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(1, 1, 1);

        public TtLODPrepareArgsShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkLODSelect.compute", RName.ERNameType.Engine);
            MainName = "CS_PrepareIndirectArgs";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkVisBufferNode;
            if (node == null) return;

            // WorkQueueNext = the queue that was just written to by CS_LODSelect
            bool evenPass = (node.mCurrentPassIndex % 2) == 0;
            drawcall.BindUav("WorkQueueNext", evenPass ? node.WorkQueueB.Uav : node.WorkQueueA.Uav);
            drawcall.BindUav("LODIndirectArgs", node.LODIndirectArgs.Uav);
        }
    }

    public class TtLODPrepareRasterArgsShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(1, 1, 1);

        public TtLODPrepareRasterArgsShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkLODSelect.compute", RName.ERNameType.Engine);
            MainName = "CS_PrepareRasterIndirectArgs";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkVisBufferNode;
            if (node == null) return;

            drawcall.BindUav("SelectedClusters", node.SelectedClusters.Uav);
            drawcall.BindUav("LODIndirectArgs", node.LODIndirectArgs.Uav);
        }
    }

    public class TtLODClearVisitedShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);

        public TtLODClearVisitedShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/Quark/QuarkLODSelect.compute", RName.ERNameType.Engine);
            MainName = "CS_ClearGroupVisited";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtQuarkVisBufferNode;
            if (node == null) return;

            drawcall.BindUav("GroupVisited", node.GroupVisited.Uav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbLODSelect");
            if (binder.IsValidPointer)
            {
                if (node.CBLODSelect == null)
                {
                    node.CBLODSelect = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBLODSelect.SetValue("LODSelectParams", in node.mLODSelectParams);
                    node.CBLODSelect.MarkDirty();
                    node.CBLODSelect.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBLODSelect);
            }
        }
    }

    #endregion

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
            public uint UseLODSelection; // 0=direct dispatch, 1=indirect via SelectedClusters
            public uint Padding0;
            public uint Padding1;
        }

        public FVisBufferParams mVisBufferParams;
        public TtCbView CBVisBuffer;

        /// <summary>
        /// LOD selection cbuffer params (matches FLODSelectParams in QuarkLODSelect.compute).
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FLODSelectParams
        {
            public Matrix ViewProj;
            public Vector2 ScreenSize;
            public float LODScale;
            public uint MaxGroupCount;
        }

        public FLODSelectParams mLODSelectParams;
        public TtCbView CBLODSelect;

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

        // LOD Selection buffers
        public TtCpu2GpuBuffer<FClusterGroupGPU> GroupBuffer = new TtCpu2GpuBuffer<FClusterGroupGPU>();
        public TtCpu2GpuBuffer<uint> GroupChildrenBuffer = new TtCpu2GpuBuffer<uint>();
        public TtCpu2GpuBuffer<uint> GroupParentsBuffer = new TtCpu2GpuBuffer<uint>();
        public TtCpu2GpuBuffer<uint> ClusterGroupMapBuffer = new TtCpu2GpuBuffer<uint>();
        public TtGpuBuffer<uint> WorkQueueA = new TtGpuBuffer<uint>();
        public TtGpuBuffer<uint> WorkQueueB = new TtGpuBuffer<uint>();
        public TtGpuBuffer<uint> SelectedClusters = new TtGpuBuffer<uint>();
        public TtGpuBuffer<uint> LODIndirectArgs = new TtGpuBuffer<uint>();
        public TtGpuBuffer<uint> GroupVisited = new TtGpuBuffer<uint>();

        #endregion

        #region Shading

        private TtVisBufferClearShading mClearShading;
        private TtComputeDraw mClearDrawcall;

        private TtVisBufferRasterizeShading mRasterizeShading;
        private TtComputeDraw mRasterizeDrawcall;

        // LOD Selection shading
        private const int MAX_LOD_PASSES = 12;
        private TtLODSelectShading mLODSelectShading;
        private TtComputeDraw[] mLODSelectDrawcalls = new TtComputeDraw[MAX_LOD_PASSES];
        private TtLODPrepareArgsShading mLODPrepareArgsShading;
        private TtComputeDraw[] mLODPrepareArgsDrawcalls = new TtComputeDraw[MAX_LOD_PASSES];
        private TtLODClearVisitedShading mLODClearVisitedShading;
        private TtComputeDraw mLODClearVisitedDrawcall;
        private TtLODPrepareRasterArgsShading mLODPrepareRasterArgsShading;
        private TtComputeDraw mLODPrepareRasterArgsDrawcall;

        #endregion

        private bool mDataDirty = false;
        private bool mLODDataDirty = false;
        private uint mWidth = 0;
        private uint mHeight = 0;

        // LOD Selection state
        public bool LODSelectionEnabled { get; set; } = false;
        public float LODScale { get; set; } = 1.0f;
        internal int mCurrentPassIndex = 0;
        private uint mGroupCount = 0;
        private uint mRootGroupCount = 0;
        private uint mMipLevels = 0;
        private uint[] mRootGroupIndices;  // CPU copy for initializing WorkQueueA each frame

        /// <summary>
        /// Per-mesh vertex stride (8 = no tangent, 12 = with tangent). Updated on BuildMergedBuffers.
        /// </summary>
        public uint VertexStride { get; private set; } = 8;

        /// <summary>
        /// Whether the uploaded mesh has explicit tangent data.
        /// </summary>
        public bool HasTangents { get; private set; } = false;

        #region Multi-Mesh Instance Management

        /// <summary>
        /// Per-instance data for merged multi-mesh rendering.
        /// </summary>
        public struct MeshInstanceData
        {
            public Graphics.Mesh.TtMeshPrimitives Mesh;
            public Matrix WorldMatrix;
            // Upload-time computed global offsets
            public uint ClusterOffset;   // global cluster start index
            public uint VertexOffset;    // global vertex start index (in vertex count units)
            public uint IndexOffset;     // global IB start position (in uint units)
            public uint GroupOffset;     // global DAG group start index
            public uint ChildrenOffset;  // GroupChildrenBuffer start
            public uint ParentsOffset;   // GroupParentsBuffer start
            public uint ClusterCount;    // cluster count for this mesh
            public uint GroupCount;      // DAG group count for this mesh
            public uint RootGroupCount;
            public uint[] LocalRootGroups; // mesh-local root group indices (before remap)
        }

        private List<MeshInstanceData> mMeshInstances = new List<MeshInstanceData>();
        private bool mMergedBuffersDirty = false;

        #endregion

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

            // LOD Selection passes
            mLODSelectShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtLODSelectShading>();
            mLODPrepareArgsShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtLODPrepareArgsShading>();
            mLODClearVisitedShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtLODClearVisitedShading>();
            mLODPrepareRasterArgsShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtLODPrepareRasterArgsShading>();

            CoreSDK.DisposeObject(ref mLODClearVisitedDrawcall);
            mLODClearVisitedDrawcall = rc.CreateComputeDraw();
            CoreSDK.DisposeObject(ref mLODPrepareRasterArgsDrawcall);
            mLODPrepareRasterArgsDrawcall = rc.CreateComputeDraw();

            for (int i = 0; i < MAX_LOD_PASSES; i++)
            {
                CoreSDK.DisposeObject(ref mLODSelectDrawcalls[i]);
                mLODSelectDrawcalls[i] = rc.CreateComputeDraw();
                CoreSDK.DisposeObject(ref mLODPrepareArgsDrawcalls[i]);
                mLODPrepareArgsDrawcalls[i] = rc.CreateComputeDraw();
            }

            // Initialize CPU→GPU buffers
            VisVertices.Initialize(EBufferType.BFT_SRV);
            VisIndices.Initialize(EBufferType.BFT_SRV);
            ClusterVisBuffer.Initialize(EBufferType.BFT_SRV);

            // LOD Selection CPU→GPU buffers
            GroupBuffer.Initialize(EBufferType.BFT_SRV);
            GroupChildrenBuffer.Initialize(EBufferType.BFT_SRV);
            GroupParentsBuffer.Initialize(EBufferType.BFT_SRV);
            ClusterGroupMapBuffer.Initialize(EBufferType.BFT_SRV);
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mClearDrawcall);
            CoreSDK.DisposeObject(ref mRasterizeDrawcall);
            CoreSDK.DisposeObject(ref CBVisBuffer);
            CoreSDK.DisposeObject(ref CBLODSelect);
            VisBuffer64?.Dispose();

            // LOD Selection cleanup
            CoreSDK.DisposeObject(ref mLODClearVisitedDrawcall);
            CoreSDK.DisposeObject(ref mLODPrepareRasterArgsDrawcall);
            for (int i = 0; i < MAX_LOD_PASSES; i++)
            {
                CoreSDK.DisposeObject(ref mLODSelectDrawcalls[i]);
                CoreSDK.DisposeObject(ref mLODPrepareArgsDrawcalls[i]);
            }
            WorkQueueA?.Dispose();
            WorkQueueB?.Dispose();
            SelectedClusters?.Dispose();
            LODIndirectArgs?.Dispose();
            GroupVisited?.Dispose();

            base.Dispose();
        }

        /// <summary>
        /// Upload DAG cluster data for VisBuffer rasterization (backward-compatible single-mesh shortcut).
        /// Internally calls ClearInstances + AddMeshInstance + BuildMergedBuffers.
        /// </summary>
        public unsafe void UploadDAGData(Graphics.Mesh.TtMeshPrimitives mesh, TtCamera camera)
        {
            ClearInstances();
            AddMeshInstance(mesh, Matrix.Identity);
            BuildMergedBuffers(camera);
        }

        /// <summary>
        /// Upload DAG group hierarchy data for GPU LOD cut selection (backward-compatible single-mesh shortcut).
        /// BuildMergedBuffers already handles DAG upload internally.
        /// If BuildMergedBuffers was already called, this is a no-op.
        /// </summary>
        public unsafe void UploadDAGGroupData(Graphics.Mesh.TtMeshPrimitives mesh)
        {
            // BuildMergedBuffers already exports DAG data.
            // Re-trigger if needed (e.g., LOD was not yet enabled at BuildMergedBuffers time)
            if (mGroupCount == 0 && mMeshInstances.Count > 0)
            {
                LODSelectionEnabled = true;
                BuildMergedDAGBuffers();
            }
        }

        /// <summary>
        /// Per-frame camera update. Rewrites WVPMatrix per-instance in ClusterVisBuffer.
        /// </summary>
        public unsafe void UpdateCamera(TtCamera camera)
        {
            if (camera == null || mVisBufferParams.ClusterCount == 0)
                return;

            var viewProjMatrix = camera.GetViewProjection();
            int structSize = sizeof(FClusterVisData);

            if (mMeshInstances.Count > 0)
            {
                // Multi-mesh path: per-instance WVP = WorldMatrix * ViewProj
                foreach (var inst in mMeshInstances)
                {
                    Matrix wvp = inst.WorldMatrix * viewProjMatrix;
                    for (uint i = 0; i < inst.ClusterCount; i++)
                    {
                        uint globalIdx = inst.ClusterOffset + i;
                        int byteOffset = (int)globalIdx * structSize + 32; // 32 = offset of WVPMatrix
                        ClusterVisBuffer.UpdateData(byteOffset, &wvp, sizeof(Matrix));
                    }
                }
            }
            else
            {
                // Fallback: single shared VP (legacy)
                uint clusterCount = mVisBufferParams.ClusterCount;
                for (uint i = 0; i < clusterCount; i++)
                {
                    int byteOffset = (int)i * structSize + 32;
                    ClusterVisBuffer.UpdateData(byteOffset, &viewProjMatrix, sizeof(Matrix));
                }
            }

            // Update LOD selection params with current view
            if (LODSelectionEnabled)
            {
                mLODSelectParams.ViewProj = viewProjMatrix;
                mLODSelectParams.ScreenSize = new Vector2(mWidth, mHeight);
                mLODSelectParams.LODScale = LODScale;
                mLODSelectParams.MaxGroupCount = mGroupCount;

                if (CBLODSelect != null)
                {
                    CBLODSelect.SetValue("LODSelectParams", in mLODSelectParams);
                }
            }

            mDataDirty = true;
        }

        #region Multi-Mesh Public API

        /// <summary>
        /// Clear all mesh instances. Call before re-adding meshes.
        /// </summary>
        public void ClearInstances()
        {
            mMeshInstances.Clear();
            mMergedBuffersDirty = true;
        }

        /// <summary>
        /// Add a mesh instance with its own world transform.
        /// Call BuildMergedBuffers() after all instances are added.
        /// </summary>
        public void AddMeshInstance(Graphics.Mesh.TtMeshPrimitives mesh, in Matrix worldMatrix)
        {
            if (mesh == null || !mesh.mCoreObject.IsValidPointer)
                return;
            if (mesh.mCoreObject.GetClusterCount() == 0)
                return;

            var inst = new MeshInstanceData();
            inst.Mesh = mesh;
            inst.WorldMatrix = worldMatrix;
            mMeshInstances.Add(inst);
            mMergedBuffersDirty = true;
        }

        /// <summary>
        /// Update the world transform of an existing instance. Marks camera dirty for per-frame WVP update.
        /// </summary>
        public void UpdateInstanceTransform(int instanceIndex, in Matrix worldMatrix)
        {
            if (instanceIndex < 0 || instanceIndex >= mMeshInstances.Count)
                return;
            var inst = mMeshInstances[instanceIndex];
            inst.WorldMatrix = worldMatrix;
            mMeshInstances[instanceIndex] = inst;
            mDataDirty = true; // trigger WVP rewrite on next UpdateCamera
        }

        /// <summary>
        /// Build merged GPU buffers from all added mesh instances.
        /// Merges VB/IB/ClusterVisBuffer and optionally DAG hierarchy data.
        /// </summary>
        public unsafe void BuildMergedBuffers(TtCamera camera)
        {
            if (mMeshInstances.Count == 0 || camera == null)
                return;

            var viewProjMatrix = camera.GetViewProjection();

            // === Pass 1: Scan global sizes ===
            uint totalVertices = 0, totalIndices = 0, totalClusters = 0;
            uint maxStride = 0;
            for (int n = 0; n < mMeshInstances.Count; n++)
            {
                var coreObj = mMeshInstances[n].Mesh.mCoreObject;
                uint meshStride = coreObj.GetClustersVBStride();
                if (meshStride > maxStride) maxStride = meshStride;
                totalVertices += coreObj.GetClustersVBCount();
                totalIndices += coreObj.GetClustersIBCount();
                totalClusters += coreObj.GetClusterCount();
            }

            if (totalClusters == 0 || maxStride == 0)
                return;

            VertexStride = maxStride;
            HasTangents = (maxStride >= 12);

            // === Pass 2: Allocate and fill merged arrays ===
            var globalVB = new float[totalVertices * maxStride];
            var globalIB = new uint[totalIndices];
            var globalClusters = new FClusterVisData[totalClusters];

            uint vOffset = 0, iOffset = 0, cOffset = 0;
            for (int n = 0; n < mMeshInstances.Count; n++)
            {
                var inst = mMeshInstances[n];
                var coreObj = inst.Mesh.mCoreObject;
                uint meshVBCount = coreObj.GetClustersVBCount();
                uint meshIBCount = coreObj.GetClustersIBCount();
                uint meshClusterCount = coreObj.GetClusterCount();
                uint meshStride = coreObj.GetClustersVBStride();

                inst.VertexOffset = vOffset;
                inst.IndexOffset = iOffset;
                inst.ClusterOffset = cOffset;
                inst.ClusterCount = meshClusterCount;

                // Copy VB (with padding if stride differs)
                float* vbPtr = coreObj.GetClustersVB();
                CopyVBWithPad(vbPtr, meshStride, maxStride, meshVBCount, globalVB, vOffset * maxStride);

                // Copy IB (rebase index values by vertex offset)
                uint* ibPtr = coreObj.GetClustersIB();
                RebaseAndCopyIB(ibPtr, meshIBCount, vOffset, globalIB, iOffset);

                // Fill cluster data
                Matrix wvp = inst.WorldMatrix * viewProjMatrix;
                for (uint c = 0; c < meshClusterCount; c++)
                {
                    var cluster = coreObj.GetCluster((int)c);
                    uint gi = cOffset + c; // global cluster index
                    globalClusters[gi].BoundMin = new Vector3(cluster.Bounds.Minimum.X, cluster.Bounds.Minimum.Y, cluster.Bounds.Minimum.Z);
                    globalClusters[gi].BoundMax = new Vector3(cluster.Bounds.Maximum.X, cluster.Bounds.Maximum.Y, cluster.Bounds.Maximum.Z);
                    globalClusters[gi].IndexStart = (int)(cluster.IndexStart + iOffset);
                    globalClusters[gi].IndexEnd = (int)(cluster.IndexStart + cluster.IndexCount + iOffset);
                    globalClusters[gi].WVPMatrix = wvp;
                    globalClusters[gi].ClusterID = gi;
                    globalClusters[gi].MipLevel = (uint)coreObj.GetClusterMipLevel((int)c);
                    globalClusters[gi].LODError = coreObj.GetClusterLODError((int)c);
                    globalClusters[gi].MaxLODError = 1.0f;
                    globalClusters[gi].MaterialID = (uint)cluster.PrimaryMaterialID;
                    globalClusters[gi].InstanceID = (uint)n;
                    globalClusters[gi].VertexStart = (uint)(cluster.VertexStart + vOffset);
                    globalClusters[gi].Padding = 0;
                }

                vOffset += meshVBCount;
                iOffset += meshIBCount;
                cOffset += meshClusterCount;

                mMeshInstances[n] = inst;
            }

            // === Upload to CPU→GPU buffers ===
            int totalFloats = (int)(totalVertices * maxStride);
            VisVertices.SetSize(totalFloats);
            fixed (float* pVB = &globalVB[0])
            {
                VisVertices.UpdateData(0, pVB, totalFloats * sizeof(float));
            }

            VisIndices.SetSize((int)totalIndices);
            fixed (uint* pIB = &globalIB[0])
            {
                VisIndices.UpdateData(0, pIB, (int)totalIndices * sizeof(uint));
            }

            ClusterVisBuffer.SetSize((int)totalClusters);
            fixed (FClusterVisData* pClusters = &globalClusters[0])
            {
                ClusterVisBuffer.UpdateData(0, pClusters, (int)totalClusters * sizeof(FClusterVisData));
            }

            mVisBufferParams.ClusterCount = totalClusters;
            mVisBufferParams.VertexStride = maxStride;
            mDataDirty = true;
            mMergedBuffersDirty = false;

            // === Build DAG if any mesh has DAG data ===
            BuildMergedDAGBuffers();
        }

        /// <summary>
        /// Internal: merge DAG hierarchy data from all instances for LOD selection.
        /// </summary>
        private unsafe void BuildMergedDAGBuffers()
        {
            // Check if any mesh has DAG data
            bool hasDAG = false;
            for (int n = 0; n < mMeshInstances.Count; n++)
            {
                if (mMeshInstances[n].Mesh.mCoreObject.GetDAGGroupCount() > 0)
                {
                    hasDAG = true;
                    break;
                }
            }
            if (!hasDAG)
                return;

            // === Pass 1: Scan DAG sizes ===
            uint totalGroups = 0, totalChildren = 0, totalParents = 0;
            uint totalClusters = mVisBufferParams.ClusterCount;
            uint totalRootGroups = 0;
            uint maxMipLevels = 0;

            for (int n = 0; n < mMeshInstances.Count; n++)
            {
                var inst = mMeshInstances[n];
                var coreObj = inst.Mesh.mCoreObject;
                uint groupCount = coreObj.GetDAGGroupCount();
                if (groupCount == 0)
                    continue;

                uint childrenTotal = 0, parentsTotal = 0, clusterCount = 0, rootGroupCount = 0;
                coreObj.GetDAGExportSizes(&groupCount, &childrenTotal, &parentsTotal, &clusterCount, &rootGroupCount);

                inst.GroupOffset = totalGroups;
                inst.ChildrenOffset = totalChildren;
                inst.ParentsOffset = totalParents;
                inst.GroupCount = groupCount;
                inst.RootGroupCount = rootGroupCount;

                totalGroups += groupCount;
                totalChildren += childrenTotal;
                totalParents += parentsTotal;
                totalRootGroups += rootGroupCount;

                uint mipLevels = coreObj.GetDAGMipLevels();
                if (mipLevels > maxMipLevels) maxMipLevels = mipLevels;

                mMeshInstances[n] = inst;
            }

            if (totalGroups == 0)
                return;

            // === Pass 2: Export and remap per-mesh ===
            var globalGroups = new FClusterGroupGPU[totalGroups];
            var globalChildren = new uint[totalChildren > 0 ? totalChildren : 1];
            var globalParents = new uint[totalParents > 0 ? totalParents : 1];
            var globalClusterGroupMap = new uint[totalClusters > 0 ? totalClusters : 1];
            // Fill clusterGroupMap with 0xFFFFFFFF (invalid)
            for (int i = 0; i < globalClusterGroupMap.Length; i++)
                globalClusterGroupMap[i] = 0xFFFFFFFF;

            var allRootGroups = new List<uint>();

            for (int n = 0; n < mMeshInstances.Count; n++)
            {
                var inst = mMeshInstances[n];
                var coreObj = inst.Mesh.mCoreObject;
                uint groupCount = inst.GroupCount;
                if (groupCount == 0)
                    continue;

                // Query sizes again for allocation
                uint childrenTotal = 0, parentsTotal = 0, clusterCount = 0, rootGroupCount = 0;
                uint gc = groupCount;
                coreObj.GetDAGExportSizes(&gc, &childrenTotal, &parentsTotal, &clusterCount, &rootGroupCount);

                var localGroups = new FClusterGroupGPU[groupCount];
                var localChildren = new uint[childrenTotal > 0 ? childrenTotal : 1];
                var localParents = new uint[parentsTotal > 0 ? parentsTotal : 1];
                var localClusterGroupMap = new uint[clusterCount > 0 ? clusterCount : 1];
                var localRoots = new uint[rootGroupCount > 0 ? rootGroupCount : 1];

                uint outChildrenTotal = 0, outParentsTotal = 0, outRootGroupCount = 0;

                fixed (FClusterGroupGPU* pGroups = &localGroups[0])
                fixed (uint* pChildren = &localChildren[0])
                fixed (uint* pParents = &localParents[0])
                fixed (uint* pClusterGroupMap = &localClusterGroupMap[0])
                fixed (uint* pRootGroups = &localRoots[0])
                {
                    coreObj.ExportDAGGroupsForGPU(
                        pGroups, groupCount,
                        pChildren, childrenTotal,
                        pParents, parentsTotal,
                        pClusterGroupMap, clusterCount,
                        pRootGroups, rootGroupCount,
                        &outChildrenTotal, &outParentsTotal, &outRootGroupCount);
                }

                // Remap groups: adjust indices and transform bounds to world space
                float maxScale = GetMaxScaleFactor(in inst.WorldMatrix);
                for (uint g = 0; g < groupCount; g++)
                {
                    localGroups[g].ChildrenStart += (int)inst.ChildrenOffset;
                    localGroups[g].ParentsStart += (int)inst.ParentsOffset;
                    // Transform LODBoundsCenter to world space
                    localGroups[g].LODBoundsCenter = Vector3.TransformCoordinate(localGroups[g].LODBoundsCenter, inst.WorldMatrix);
                    localGroups[g].LODBoundsRadius *= maxScale;
                }
                Array.Copy(localGroups, 0, globalGroups, (int)inst.GroupOffset, (int)groupCount);

                // Remap children buffer: cluster indices += ClusterOffset
                for (uint i = 0; i < outChildrenTotal; i++)
                    localChildren[i] += inst.ClusterOffset;
                Array.Copy(localChildren, 0, globalChildren, (int)inst.ChildrenOffset, (int)outChildrenTotal);

                // Remap parents buffer: cluster indices += ClusterOffset
                for (uint i = 0; i < outParentsTotal; i++)
                    localParents[i] += inst.ClusterOffset;
                Array.Copy(localParents, 0, globalParents, (int)inst.ParentsOffset, (int)outParentsTotal);

                // Remap clusterGroupMap: group indices += GroupOffset
                for (uint i = 0; i < clusterCount; i++)
                {
                    uint val = localClusterGroupMap[i];
                    if (val != 0xFFFFFFFF)
                        val += inst.GroupOffset;
                    globalClusterGroupMap[inst.ClusterOffset + i] = val;
                }

                // Root groups: += GroupOffset
                inst.LocalRootGroups = new uint[outRootGroupCount];
                for (uint i = 0; i < outRootGroupCount; i++)
                {
                    uint remapped = localRoots[i] + inst.GroupOffset;
                    allRootGroups.Add(remapped);
                    inst.LocalRootGroups[i] = remapped;
                }

                mMeshInstances[n] = inst;
            }

            // === Build merged RootGroupIndices ===
            mRootGroupIndices = new uint[totalRootGroups + 1];
            mRootGroupIndices[0] = totalRootGroups;
            for (int i = 0; i < allRootGroups.Count; i++)
                mRootGroupIndices[i + 1] = allRootGroups[i];

            // === Upload to CPU→GPU buffers ===
            GroupBuffer.SetSize((int)totalGroups);
            fixed (FClusterGroupGPU* pGroups = &globalGroups[0])
            {
                GroupBuffer.UpdateData(0, pGroups, (int)totalGroups * sizeof(FClusterGroupGPU));
            }

            if (totalChildren > 0)
            {
                GroupChildrenBuffer.SetSize((int)totalChildren);
                fixed (uint* pChildren = &globalChildren[0])
                {
                    GroupChildrenBuffer.UpdateData(0, pChildren, (int)totalChildren * sizeof(uint));
                }
            }

            if (totalParents > 0)
            {
                GroupParentsBuffer.SetSize((int)totalParents);
                fixed (uint* pParents = &globalParents[0])
                {
                    GroupParentsBuffer.UpdateData(0, pParents, (int)totalParents * sizeof(uint));
                }
            }

            if (totalClusters > 0)
            {
                ClusterGroupMapBuffer.SetSize((int)totalClusters);
                fixed (uint* pMap = &globalClusterGroupMap[0])
                {
                    ClusterGroupMapBuffer.UpdateData(0, pMap, (int)totalClusters * sizeof(uint));
                }
            }

            // Store state
            mGroupCount = totalGroups;
            mRootGroupCount = totalRootGroups;
            mMipLevels = maxMipLevels;

            // Allocate GPU-only LOD buffers
            uint maxQueueSize = totalGroups + 1;
            uint maxSelectedSize = totalClusters + 1;
            WorkQueueA.SetSize(maxQueueSize, null, EBufferType.BFT_UAV | EBufferType.BFT_SRV);
            WorkQueueB.SetSize(maxQueueSize, null, EBufferType.BFT_UAV | EBufferType.BFT_SRV);
            SelectedClusters.SetSize(maxSelectedSize, null, EBufferType.BFT_UAV | EBufferType.BFT_SRV);
            GroupVisited.SetSize(totalGroups, null, EBufferType.BFT_UAV | EBufferType.BFT_SRV);

            uint indirectArgSize = 3;
            LODIndirectArgs.SetSize(indirectArgSize, null, EBufferType.BFT_UAV | EBufferType.BFT_SRV | EBufferType.BFT_IndirectArgs);

            // Initialize LOD selection params
            mLODSelectParams.LODScale = LODScale;
            mLODSelectParams.MaxGroupCount = totalGroups;

            LODSelectionEnabled = true;
            mLODDataDirty = true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Copy vertex buffer with stride padding (zero-fills extra floats per vertex).
        /// </summary>
        private static unsafe void CopyVBWithPad(float* src, uint srcStride, uint dstStride, uint vertexCount, float[] dst, uint dstFloatOffset)
        {
            fixed (float* pDst = &dst[dstFloatOffset])
            {
                if (srcStride == dstStride)
                {
                    Buffer.MemoryCopy(src, pDst, vertexCount * dstStride * 4, vertexCount * srcStride * 4);
                }
                else
                {
                    for (uint v = 0; v < vertexCount; v++)
                    {
                        Buffer.MemoryCopy(src + v * srcStride, pDst + v * dstStride, srcStride * 4, srcStride * 4);
                        for (uint p = srcStride; p < dstStride; p++)
                            pDst[v * dstStride + p] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Copy index buffer with vertex offset rebasing.
        /// </summary>
        private static unsafe void RebaseAndCopyIB(uint* src, uint count, uint vertexOffset, uint[] dst, uint dstOffset)
        {
            fixed (uint* pDst = &dst[dstOffset])
            {
                for (uint i = 0; i < count; i++)
                    pDst[i] = src[i] + vertexOffset;
            }
        }

        /// <summary>
        /// Get the maximum axis scale factor from a world matrix (for LOD bounds radius scaling).
        /// </summary>
        private static float GetMaxScaleFactor(in Matrix m)
        {
            float sx = new Vector3(m.M11, m.M12, m.M13).Length();
            float sy = new Vector3(m.M21, m.M22, m.M23).Length();
            float sz = new Vector3(m.M31, m.M32, m.M33).Length();
            return System.Math.Max(sx, System.Math.Max(sy, sz));
        }

        #endregion

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

            if (mLODDataDirty)
            {
                using (var tsCmd = new FTransientCmd(EQueueType.QU_Default, "QuarkVisBuffer.FlushLODBuffers"))
                {
                    GroupBuffer.Flush2GPU(tsCmd.CmdList);
                    GroupChildrenBuffer.Flush2GPU(tsCmd.CmdList);
                    GroupParentsBuffer.Flush2GPU(tsCmd.CmdList);
                    ClusterGroupMapBuffer.Flush2GPU(tsCmd.CmdList);
                }
                mLODDataDirty = false;
            }

            // Update cbuffer values
            if (CBVisBuffer != null)
            {
                CBVisBuffer.SetValue("VisBufferParams", in mVisBufferParams);
            }

            var cmd = TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmd, "QuarkVisBuffer"))
            {
                // Pass 1: Clear VisBuffer64
                mClearShading.SetDrawcallDispatch(this, policy, mClearDrawcall,
                    mWidth, mHeight, 1, true);
                cmd.PushGpuDraw(mClearDrawcall);

                // === LOD Cut Selection (multi-pass) ===
                if (LODSelectionEnabled && mGroupCount > 0 && mRootGroupCount > 0 &&
                    WorkQueueA.Uav != null && SelectedClusters.Uav != null)
                {
                    mVisBufferParams.UseLODSelection = 1;

                    // Per-frame buffer initialization: record copies directly on cmd
                    // (these go on the native cmd BEFORE any PushGpuDraw dispatches are flushed)
                    fixed (uint* pRootData = &mRootGroupIndices[0])
                    {
                        WorkQueueA.GpuBuffer.UpdateGpuData(cmd.mCoreObject,
                            0, pRootData,
                            (uint)(mRootGroupIndices.Length * sizeof(uint)));
                    }
                    // Clear counters using WriteBufferImmediate (no staging buffer needed)
                    var bfWriter = new NxRHI.FBufferWriter();
                    bfWriter.Value = 0;
                    bfWriter.Buffer = SelectedClusters.GpuBuffer.mCoreObject;
                    bfWriter.Offset = 0;
                    cmd.WriteBufferUINT32(1, &bfWriter);
                    bfWriter.Buffer = WorkQueueB.GpuBuffer.mCoreObject;
                    cmd.WriteBufferUINT32(1, &bfWriter);

                    // Clear GroupVisited flags
                    mLODClearVisitedShading.SetDrawcallDispatch(this, policy, mLODClearVisitedDrawcall,
                        mGroupCount, 1, 1, true);
                    cmd.PushGpuDraw(mLODClearVisitedDrawcall);

                    // Multi-pass LOD selection
                    // Use FlushDraws() between passes to ensure correct ordering:
                    // copies/writes recorded immediately vs dispatches recorded at flush time.
                    int numPasses = (int)System.Math.Min(mMipLevels, (uint)MAX_LOD_PASSES);
                    for (int pass = 0; pass < numPasses; pass++)
                    {
                        mCurrentPassIndex = pass;

                        // Dispatch CS_LODSelect
                        if (pass == 0)
                        {
                            uint dispatchX = (mRootGroupCount + 63) / 64;
                            mLODSelectShading.SetDrawcallDispatch(this, policy, mLODSelectDrawcalls[pass],
                                dispatchX, 1, 1, false);
                        }
                        else
                        {
                            mLODSelectShading.SetDrawcallIndirectDispatch(this, policy, mLODSelectDrawcalls[pass],
                                LODIndirectArgs.GpuBuffer);
                        }
                        cmd.PushGpuDraw(mLODSelectDrawcalls[pass]);

                        // Dispatch CS_PrepareIndirectArgs (for next pass)
                        if (pass < numPasses - 1)
                        {
                            mLODPrepareArgsShading.SetDrawcallDispatch(this, policy, mLODPrepareArgsDrawcalls[pass],
                                1, 1, 1, false);
                            cmd.PushGpuDraw(mLODPrepareArgsDrawcalls[pass]);
                        }

                        // Flush dispatches so far, then clear the "next-next" queue counter
                        // so it's ready for the next iteration's expand output.
                        if (pass < numPasses - 1)
                        {
                            cmd.FlushDraws();
                            // After this pass: ping-pong swap happens implicitly.
                            // The next pass's "next" = this pass's "current" queue.
                            bool evenPass = (pass % 2) == 0;
                            bfWriter.Buffer = (evenPass ? WorkQueueA : WorkQueueB).GpuBuffer.mCoreObject;
                            bfWriter.Offset = 0;
                            bfWriter.Value = 0;
                            cmd.WriteBufferUINT32(1, &bfWriter);
                        }
                    }

                    // Prepare indirect args for rasterize pass
                    mLODPrepareRasterArgsShading.SetDrawcallDispatch(this, policy, mLODPrepareRasterArgsDrawcall,
                        1, 1, 1, false);
                    cmd.PushGpuDraw(mLODPrepareRasterArgsDrawcall);

                    // Rasterize with indirect dispatch via SelectedClusters
                    mRasterizeShading.SetDrawcallIndirectDispatch(this, policy, mRasterizeDrawcall,
                        LODIndirectArgs.GpuBuffer);
                    cmd.PushGpuDraw(mRasterizeDrawcall);
                }
                else
                {
                    // Legacy path: direct dispatch all clusters
                    mVisBufferParams.UseLODSelection = 0;
                    mRasterizeShading.SetDrawcallDispatch(this, policy, mRasterizeDrawcall,
                        mVisBufferParams.ClusterCount, 1, 1, false);
                    cmd.PushGpuDraw(mRasterizeDrawcall);
                }

                cmd.FlushDraws();
            }

            if (policy != null)
                policy.CommitCommandList(cmd, "QuarkVisBuffer");
            else
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmd, "QuarkVisBuffer");
        }
    }
}

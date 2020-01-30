using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.GpuDriven.GpuScene
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public unsafe struct CB_MeshBatch
    {
        public fixed float CameraPlanes[6 * 4];

        public Vector3 CameraDirection;
        public uint MeshBatchVertexStride;

        public Vector3 FrustumMinPoint;
        public uint ClusterNumber;

        public Vector3 FrustumMaxPoint;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
    public struct GpuMeshInstanceData
    {
        public Matrix Matrix;
        public Matrix InvMatrix;
        public UInt32_4 VTMaterialId;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
    public struct GpuDrawArgs
    {
        public uint IndexCountPerInstance;
        public uint InstanceCount;
        public uint StartIndexLocation;
        public int BaseVertexLocation;
        public uint StartInstanceLocation;
        //public uint Pad0;
    }

    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    public partial class SceneDataManager
    {
        List<Cluster.GpuSceneVertex> mAllVertices = new List<Cluster.GpuSceneVertex>();
        public List<Cluster.GpuSceneVertex> AllVertices
        {
            get { return mAllVertices; }
        }

        List<uint> mAllIndices = new List<uint>();
        public List<uint> AllIndices
        {
            get { return mAllIndices; }
        }

        List<Graphics.CGfxMaterialInstance> mGpuSceneMaterials = new List<Graphics.CGfxMaterialInstance>();
        List<GpuMeshInstanceData> mGpuInstanceDatas = new List<GpuMeshInstanceData>();
        public List<GpuMeshInstanceData> GpuInstanceDatas
        {
            get { return mGpuInstanceDatas; }
        }

        List<Cluster.GpuCluster> mGpuClusters = new List<Cluster.GpuCluster>(); 
        public List<Cluster.GpuCluster> GpuClusters
        {
            get { return mGpuClusters; }
        }

        Dictionary<RName, KeyValuePair<uint,Cluster.ClusteredMesh>> mClusteredMeshs = new Dictionary<RName, KeyValuePair<uint,Cluster.ClusteredMesh>>();

        public int GetOrAddMaterialId(Graphics.CGfxMaterialInstance mtl)
        {
            for (int i = 0; i < mGpuSceneMaterials.Count; i++)
            {
                if (mGpuSceneMaterials[i] == mtl)
                    return i;
            }

            mGpuSceneMaterials.Add(mtl);
            return mGpuSceneMaterials.Count - 1;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void AddMeshInstance(GamePlay.Actor.GActor actor,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.MeshCluster)]
            RName clusterName)
        {
            var mesh = actor.GetComponentMesh();
            if(mesh!=null)
                AddMeshInstance(CEngine.Instance.RenderContext, mesh, clusterName, actor.Placement);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void BindDrawGpuScene(GamePlay.GGameInstance game)
        {
            var rPolicy = game.RenderPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_GameMobile;
            rPolicy.mForwardBasePass.BeforeBuildRenderPass = (InCamera, InView, InRc, InCmd, InDPLimitter, InGraphicsDebug) =>
            {
                this.DrawScene(InRc, InCmd, InCamera, InView);
            };
        }
        public void AddMeshInstance(CRenderContext rc, Graphics.Mesh.CGfxMesh mesh, RName clusterName, GamePlay.Component.GPlacementComponent placement)
        {
            var worldMatrix = placement.WorldMatrix;
            uint instStart = (uint)mGpuInstanceDatas.Count;
            for (int i = 0; i < mesh.MtlMeshArray.Length; i++)
            {
                var data = new GpuMeshInstanceData();
                data.VTMaterialId.x = 1;//(uint)GetOrAddMaterialId(mesh.MtlMeshArray[i].MtlInst);
                data.Matrix = worldMatrix;
                Matrix.Invert(ref data.Matrix, out data.InvMatrix);
                data.Matrix.Transpose();
                data.InvMatrix.Transpose();

                mGpuInstanceDatas.Add(data);

                //var module = CEngine.Instance.GetCurrentModule();
                //module.World.AddActor();
            }

            uint indexOffset;
            Cluster.ClusteredMesh cluster = GetClusteredMesh(rc, clusterName, out indexOffset);
            for (int i = 0; i < cluster.ClusterDatas.Count; i++)
            {
                Cluster.GpuCluster gpuCluster = cluster.ClusterDatas[i];
                gpuCluster.InstanceId += instStart;
                gpuCluster.StartFaceIndex += indexOffset / 3;

                BoundingBox tmpBox = new BoundingBox(gpuCluster.BoundCenter - gpuCluster.BoundExtent,gpuCluster.BoundCenter + gpuCluster.BoundExtent);
                tmpBox = BoundingBox.Transform(ref tmpBox, ref worldMatrix);

                gpuCluster.BoundCenter = tmpBox.GetCenter();
                gpuCluster.BoundExtent = tmpBox.GetSize() * 0.5f;

                mGpuClusters.Add(gpuCluster);
            }
        }
        private Cluster.ClusteredMesh GetClusteredMesh(CRenderContext rc, RName clusterName, out uint indexOffset)
        {
            KeyValuePair<uint, Cluster.ClusteredMesh> result;
            if (mClusteredMeshs.TryGetValue(clusterName, out result))
            {
                indexOffset = result.Key;
                return result.Value;
            }
            else
            {
                uint vertexOffset = (uint)mAllVertices.Count;
                Cluster.ClusteredMesh clusterMesh = CEngine.Instance.ClusteredMeshManager.GetResource(rc, clusterName, true);
                mAllVertices.AddRange(clusterMesh.MeshVertices);

                indexOffset = (uint)mAllIndices.Count;
                for (int i = 0; i < clusterMesh.IndexBuffer.Count; i++)
                {
                    uint index = vertexOffset + clusterMesh.IndexBuffer[i];
                    mAllIndices.Add(index);
                }

                result = new KeyValuePair<uint, Cluster.ClusteredMesh>(indexOffset, clusterMesh);
                mClusteredMeshs.Add(clusterName, result);

                return result.Value;
            }
        }

        private CShaderDesc mCS_ClearBatchArgsDesc;
        private CComputeShader mCS_ClearBatchArgs;

        private CShaderDesc mCS_MeshBatchDesc;
        private CComputeShader mCS_MeshBatch;

        CUnorderedAccessView uavMeshInstanceArray;
        CGpuBuffer bufferMeshInstanceArray;

        CUnorderedAccessView uavClusterArray;
        CGpuBuffer bufferClusterArray;

        CUnorderedAccessView uavStaticSceneAllFaces;
        CGpuBuffer bufferStaticSceneAllFaces;

        CUnorderedAccessView uavStaticSceneDrawFaces;
        CGpuBuffer bufferStaticSceneDrawFaces;

        CUnorderedAccessView uavIndirectDrawArgs;
        CGpuBuffer bufferIndirectDrawArgs;

        //Draw VS Buffer
        CConstantBuffer mCBMeshBatch;
        CIndexBuffer mDrawIndexBuffer;
        CGpuBuffer bufferAllVertex;
        CShaderResourceView mAllVertexSRV;
        CShaderResourceView mMeshInstanceSRV;

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public unsafe void UpdateGpuBuffer()
        {
            mNeedUpdateGpuBuffer = true;
        }
        bool mNeedUpdateGpuBuffer = false;
        public unsafe void UpdateGpuBuffer(CRenderContext rc, EngineNS.CCommandList cmd, Graphics.CGfxCamera Camera)
        {
            if (UseVTF)
            {
                UpdateGpuBufferVTF(rc, cmd, Camera);
            }
            else
            {
                var bfDesc = new CGpuBufferDesc();
                //mAllVertexSRV
                {
                    bfDesc.SetMode(false, false);
                    bfDesc.ByteWidth = (uint)(mAllVertices.Count * sizeof(EngineNS.Bricks.GpuDriven.Cluster.GpuSceneVertex));
                    bfDesc.StructureByteStride = (uint)sizeof(EngineNS.Bricks.GpuDriven.Cluster.GpuSceneVertex);
                    var copyArray = mAllVertices.ToArray();
                    fixed (EngineNS.Bricks.GpuDriven.Cluster.GpuSceneVertex* p = &copyArray[0])
                    {
                        bufferAllVertex = rc.CreateGpuBuffer(bfDesc, (IntPtr)p);
                    }

                    var srvDesc = new ISRVDesc();
                    srvDesc.ToDefault();
                    srvDesc.ViewDimension = EResourceDimension.RESOURCE_DIMENSION_BUFFER;
                    srvDesc.Buffer.ElementOffset = 0;
                    srvDesc.Buffer.NumElements = (uint)mAllVertices.Count;
                    mAllVertexSRV = rc.CreateShaderResourceViewFromBuffer(bufferAllVertex, srvDesc);
                }
                //uavMeshInstanceArray
                {
                    bfDesc.SetMode(false, true);
                    bfDesc.ByteWidth = (uint)(GpuInstanceDatas.Count * sizeof(EngineNS.Bricks.GpuDriven.GpuScene.GpuMeshInstanceData));
                    bfDesc.StructureByteStride = (uint)sizeof(EngineNS.Bricks.GpuDriven.GpuScene.GpuMeshInstanceData);
                    var copyArray = GpuInstanceDatas.ToArray();
                    fixed (EngineNS.Bricks.GpuDriven.GpuScene.GpuMeshInstanceData* p = &copyArray[0])
                    {
                        bufferMeshInstanceArray = rc.CreateGpuBuffer(bfDesc, (IntPtr)p);
                        //bufferMeshInstanceArray.UpdateBufferData(cmd, (IntPtr)p, bfDesc.ByteWidth);
                    }

                    var uavDesc = new CUnorderedAccessViewDesc();
                    uavDesc.ToDefault();
                    uavDesc.Buffer.NumElements = (uint)GpuInstanceDatas.Count;
                    uavMeshInstanceArray = rc.CreateUnorderedAccessView(bufferMeshInstanceArray, uavDesc);

                    var srvDesc = new ISRVDesc();
                    srvDesc.ToDefault();
                    srvDesc.ViewDimension = EResourceDimension.RESOURCE_DIMENSION_BUFFER;
                    srvDesc.Buffer.ElementOffset = 0;
                    srvDesc.Buffer.NumElements = (uint)GpuInstanceDatas.Count;
                    mMeshInstanceSRV = rc.CreateShaderResourceViewFromBuffer(bufferMeshInstanceArray, srvDesc);
                }
            }
            if (UseComputeShader)
            {
                var bfDesc = new CGpuBufferDesc();
                if (uavMeshInstanceArray == null)
                {
                    bfDesc.SetMode(false, true);
                    bfDesc.ByteWidth = (uint)(GpuInstanceDatas.Count * sizeof(EngineNS.Bricks.GpuDriven.GpuScene.GpuMeshInstanceData));
                    bfDesc.StructureByteStride = (uint)sizeof(EngineNS.Bricks.GpuDriven.GpuScene.GpuMeshInstanceData);
                    var copyArray = GpuInstanceDatas.ToArray();
                    fixed (EngineNS.Bricks.GpuDriven.GpuScene.GpuMeshInstanceData* p = &copyArray[0])
                    {
                        bufferMeshInstanceArray = rc.CreateGpuBuffer(bfDesc, (IntPtr)p);
                        //bufferMeshInstanceArray.UpdateBufferData(cmd, (IntPtr)p, bfDesc.ByteWidth);
                    }
                    
                    var uavDesc = new CUnorderedAccessViewDesc();
                    uavDesc.ToDefault();
                    uavDesc.Buffer.NumElements = (uint)GpuInstanceDatas.Count;
                    uavMeshInstanceArray = rc.CreateUnorderedAccessView(bufferMeshInstanceArray, uavDesc);

                    var srvDesc = new ISRVDesc();
                    srvDesc.ToDefault();
                    srvDesc.ViewDimension = EResourceDimension.RESOURCE_DIMENSION_BUFFER;
                    srvDesc.Buffer.ElementOffset = 0;
                    srvDesc.Buffer.NumElements = (uint)GpuInstanceDatas.Count;
                    mMeshInstanceSRV = rc.CreateShaderResourceViewFromBuffer(bufferMeshInstanceArray, srvDesc);
                }
                
                //uavClusterArray
                {
                    bfDesc.SetMode(false, true);
                    bfDesc.ByteWidth = (uint)(GpuClusters.Count * sizeof(EngineNS.Bricks.GpuDriven.Cluster.GpuCluster));
                    bfDesc.StructureByteStride = (uint)sizeof(EngineNS.Bricks.GpuDriven.Cluster.GpuCluster);
                    var copyArray = GpuClusters.ToArray();
                    fixed (EngineNS.Bricks.GpuDriven.Cluster.GpuCluster* p = &copyArray[0])
                    {
                        bufferClusterArray = rc.CreateGpuBuffer(bfDesc, (IntPtr)p);
                        //bufferClusterArray.UpdateBufferData(cmd, (IntPtr)p, bfDesc.ByteWidth);
                    }
                    var uavDesc = new CUnorderedAccessViewDesc();
                    uavDesc.ToDefault();
                    uavDesc.Buffer.NumElements = (uint)GpuClusters.Count;
                    uavClusterArray = rc.CreateUnorderedAccessView(bufferClusterArray, uavDesc);
                }
                
                //uavStaticSceneAllFaces
                {
                    bfDesc.SetMode(false, true);
                    bfDesc.ByteWidth = (uint)(AllIndices.Count * sizeof(uint));
                    bfDesc.StructureByteStride = (uint)sizeof(uint);
                    var copyArray = AllIndices.ToArray();
                    fixed (uint* p = &copyArray[0])
                    {
                        bufferStaticSceneAllFaces = rc.CreateGpuBuffer(bfDesc, (IntPtr)p);
                        //bufferStaticSceneAllFaces.UpdateBufferData(cmd, (IntPtr)p, bfDesc.ByteWidth);
                    }

                    var uavDesc = new CUnorderedAccessViewDesc();
                    uavDesc.ToDefault();
                    uavDesc.Buffer.NumElements = (uint)(AllIndices.Count);
                    uavStaticSceneAllFaces = rc.CreateUnorderedAccessView(bufferStaticSceneAllFaces, uavDesc);
                }
                
                //uavStaticSceneDrawFaces
                {
                    bfDesc.SetMode(false, true);

                    int MaxInstanceNumber = 20;
                    bfDesc.ByteWidth = (uint)(AllIndices.Count * MaxInstanceNumber * sizeof(uint));
                    bfDesc.StructureByteStride = (uint)sizeof(uint);
                    bfDesc.MiscFlags = (UInt32)EResourceMiscFlag.BUFFER_ALLOW_RAW_VIEWS;
                    bfDesc.BindFlags |= (UInt32)EBindFlag.INDEX_BUFFER;
                    bfDesc.CPUAccessFlags = 0;
                    bufferStaticSceneDrawFaces = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);

                    var uavDesc = new CUnorderedAccessViewDesc();
                    uavDesc.ToDefault();
                    uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                    uavDesc.Buffer.NumElements = (uint)(AllIndices.Count * MaxInstanceNumber);
                    uavDesc.Buffer.Flags = (UInt32)EUAVBufferFlag.UAV_FLAG_RAW;
                    uavStaticSceneDrawFaces = rc.CreateUnorderedAccessView(bufferStaticSceneDrawFaces, uavDesc);

                    var ibDesc = new CIndexBufferDesc();
                    ibDesc.CPUAccess = 0;
                    ibDesc.InitData = IntPtr.Zero;
                    ibDesc.ByteWidth = bfDesc.ByteWidth;
                    ibDesc.Type = EIndexBufferType.IBT_Int32;

                    mDrawIndexBuffer = rc.CreateIndexBufferFromBuffer(ibDesc, bufferStaticSceneDrawFaces);
                }
                
                //uavIndirectDrawArgs
                {
                    bfDesc.SetMode(false, true);

                    bfDesc.ByteWidth = 20;//(uint)(1 * sizeof(EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs));
                    bfDesc.StructureByteStride = 4;//(uint)sizeof(EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs);
                    bfDesc.MiscFlags = (UInt32)(EResourceMiscFlag.DRAWINDIRECT_ARGS | EResourceMiscFlag.BUFFER_ALLOW_RAW_VIEWS);
                    bfDesc.CPUAccessFlags = 0;
                    bufferIndirectDrawArgs = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
                    var uavDesc = new CUnorderedAccessViewDesc();
                    uavDesc.ToDefault();
                    uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;

                    uavDesc.Buffer.NumElements = (uint)(5);
                    uavDesc.Buffer.Flags = (UInt32)EUAVBufferFlag.UAV_FLAG_RAW;
                    uavIndirectDrawArgs = rc.CreateUnorderedAccessView(bufferIndirectDrawArgs, uavDesc);

                    var drawAgrs = new EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs();
                    drawAgrs.InstanceCount = 1;
                    drawAgrs.StartInstanceLocation = 0;
                    drawAgrs.IndexCountPerInstance = 0;
                    bufferIndirectDrawArgs.UpdateBufferData(cmd, (IntPtr)(&drawAgrs), bfDesc.ByteWidth);
                }

                ComputeDispatch(rc, cmd, Camera);
            }
            else
            {
                CIndexBufferDesc ibDesc = new CIndexBufferDesc(EIndexBufferType.IBT_Int32);
                ibDesc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                ibDesc.ByteWidth = (uint)(mAllIndices.Count * sizeof(UInt32) * 20);
                mCpuDrawIndexBuffer = rc.CreateIndexBuffer(ibDesc);
            }
            

            //CEngine.Instance.EventPoster.RunOn(() =>
            //{
            //    var blobDrawArgs = new EngineNS.Support.CBlobObject();
            //    bufferIndirectDrawArgs.GetBufferData(rc, blobDrawArgs);
            //    EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs* pArg = (EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs*)blobDrawArgs.Data.ToPointer();
            //    if (pArg != null)
            //    {
            //        mDrawArgs = *pArg;
            //    }
            //    return null;
            //}, Thread.Async.EAsyncTarget.Main);
        }

        private void UpdateCBMeshbatch(EngineNS.CCommandList cmd, Graphics.CGfxCamera Camera)
        {
            var varIdx = mCBMeshBatch.FindVar("ClusterNumber");
            mCBMeshBatch.SetValue(varIdx, GpuClusters.Count, 0);
            varIdx = mCBMeshBatch.FindVar("MeshBatchVertexStride");
            mCBMeshBatch.SetValue(varIdx, AllVertices.Count, 0);

            if (Camera != null)
            {
                unsafe
                {
                    varIdx = mCBMeshBatch.FindVar("GpuDrivenCameraPlanes");
                    var pPlanes = stackalloc Plane[6];
                    Camera.CullingFrustum.GetPlanes(pPlanes);
                    for (uint i = 0; i < 6; i++)
                    {
                        mCBMeshBatch.SetValue(varIdx, pPlanes[i], i);
                    }

                    varIdx = mCBMeshBatch.FindVar("GpuDrivenCameraPosition");
                    mCBMeshBatch.SetValue(varIdx, Camera.CullingFrustum.TipPos, 0);

                    BoundingBox box = new BoundingBox();
                    Camera.CullingFrustum.GetBoundBox(ref box);
                    varIdx = mCBMeshBatch.FindVar("GpuDrivenFrustumMinPoint");
                    mCBMeshBatch.SetValue(varIdx, box.Minimum, 0);
                    varIdx = mCBMeshBatch.FindVar("GpuDrivenFrustumMaxPoint");
                    mCBMeshBatch.SetValue(varIdx, box.Maximum, 0);

                    varIdx = mCBMeshBatch.FindVar("EnableGpuCulling");
                    mCBMeshBatch.SetValue(varIdx, (uint)(1), 0);
                }
            }
            else
            {
                varIdx = mCBMeshBatch.FindVar("EnableGpuCulling");
                mCBMeshBatch.SetValue(varIdx, (uint)(0), 0);
            }
        }
        public void ComputeDispatch(CRenderContext rc, EngineNS.CCommandList cmd, Graphics.CGfxCamera Camera)
        {
            if (mCS_ClearBatchArgs == null)
                return;

            UInt32[] pUAVInitialCounts = new UInt32[1] { 1, };

            {
                cmd.SetComputeShader(mCS_ClearBatchArgs);
                //if (mCS_ClearBatchArgsDesc.GetCBufferDesc("IndirectDrawArgs", ref cbDesc))
                //    cmd.CSSetUnorderedAccessView(cbDesc.CSBindPoint, uavIndirectDrawArgs, pUAVInitialCounts);
                cmd.CSSetUnorderedAccessView(5, uavIndirectDrawArgs, pUAVInitialCounts);
                cmd.CSDispatch(1, 1, 1);
                cmd.CSSetUnorderedAccessView(5, null, pUAVInitialCounts);
            }

            cmd.SetComputeShader(mCS_MeshBatch);
            //if (mCS_MeshBatchDesc.GetCBufferDesc("MeshInstanceArray", ref cbDesc))
            //    cmd.CSSetUnorderedAccessView(cbDesc.CSBindPoint, uavMeshInstanceArray, pUAVInitialCounts);
            //if (mCS_MeshBatchDesc.GetCBufferDesc("ClusterArray", ref cbDesc))
            //    cmd.CSSetUnorderedAccessView(cbDesc.CSBindPoint, uavClusterArray, pUAVInitialCounts);
            //if (mCS_MeshBatchDesc.GetCBufferDesc("StaticSceneAllFaces", ref cbDesc))
            //    cmd.CSSetUnorderedAccessView(cbDesc.CSBindPoint, uavStaticSceneAllFaces, pUAVInitialCounts);
            //if (mCS_MeshBatchDesc.GetCBufferDesc("StaticSceneDrawFaces", ref cbDesc))
            //    cmd.CSSetUnorderedAccessView(cbDesc.CSBindPoint, uavStaticSceneDrawFaces, pUAVInitialCounts);
            //if (mCS_MeshBatchDesc.GetCBufferDesc("IndirectDrawArgs", ref cbDesc))
            //    cmd.CSSetUnorderedAccessView(cbDesc.CSBindPoint, uavIndirectDrawArgs, pUAVInitialCounts);

            cmd.CSSetUnorderedAccessView(1, uavMeshInstanceArray, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(2, uavClusterArray, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(3, uavStaticSceneAllFaces, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(4, uavStaticSceneDrawFaces, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(5, uavIndirectDrawArgs, pUAVInitialCounts);

            UpdateCBMeshbatch(cmd, Camera);
            mCBMeshBatch.FlushContent(cmd);

            var cbIndex = mCS_MeshBatchDesc.FindCBufferDesc("cbMeshBatch");
            var tmpDesc = new EngineNS.CConstantBufferDesc();
            if (mCS_MeshBatchDesc.GetCBufferDesc(cbIndex, ref tmpDesc))
            {
                if (tmpDesc.Type == EngineNS.ECBufferRhiType.SIT_CBUFFER)
                {
                    cmd.CSSetConstantBuffer(tmpDesc.CSBindPoint, mCBMeshBatch);
                }
            }

            cmd.CSDispatch((uint)((GpuClusters.Count + 63) / 64), 1, 1);

            cmd.CSSetUnorderedAccessView(1, null, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(2, null, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(3, null, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(4, null, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(5, null, pUAVInitialCounts);

            cmd.SetComputeShader(null);
            //unsafe
            //{
            //    CEngine.Instance.EventPoster.RunOn(() =>
            //    {
            //        var blobDrawArgs = new EngineNS.Support.CBlobObject();
            //        bufferIndirectDrawArgs.GetBufferData(rc, blobDrawArgs);
            //        EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs* pArg = (EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs*)blobDrawArgs.Data.ToPointer();
            //        if (pArg != null)
            //        {
            //            mDrawArgs = *pArg;
            //        }
            //        return null;
            //    }, Thread.Async.EAsyncTarget.Main);
            //}
        }

        bool UseVTF = false;
        bool UseComputeShader = true;
        
        EngineNS.Graphics.CGfxShadingEnv mMergeInstanceSE;
        CPass mPass;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task InitPass(CRenderContext rc, Graphics.CGfxMaterialInstance MtlInst = null, bool isSM3 = false)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;

            if(CRenderContext.ShaderModel>=4)
            {
                UseComputeShader = true;
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"GpuDriven: SM = {CRenderContext.ShaderModel},Use ComputeShader");
            }
            else
            {
                UseComputeShader = false;
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"GpuDriven: SM = {CRenderContext.ShaderModel},CPU Culling");
            }

            if(rc.ContextCaps.MaxVertexShaderStorageBlocks == 0)
            {
                UseVTF = true;
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"GpuDriven: No SSBO in VertexShader,Use VTF");
            }
            else
            {
                UseVTF = false;
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"GpuDriven: Use SSBO in VertexShader");
            }
            if (mMergeInstanceSE == null)
            {
                mMergeInstanceSE = mMergeInstanceSE = EngineNS.CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMergeInstanceSE>();
                if (UseVTF)
                    mMergeInstanceSE.SetMacroDefineValue("ENV_USEVTF", "1");
                else
                    mMergeInstanceSE.SetMacroDefineValue("ENV_USEVTF", "0");
            }
            if (MtlInst == null)
            {
                MtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/defaultmaterial.instmtl"));
            }
            if (mPass == null)
            {
                var pass = rc.CreatePass();

                var meshpri = CEngine.Instance.MeshPrimitivesManager.CreateMeshPrimitives(rc, 1);
                EngineNS.Graphics.Mesh.CGfxMeshCooker.MakeRect3D(rc, meshpri);
                //var meshpri = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, CEngineDesc.ScreenAlignedTriangleName, true);
                var ViewportMesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshpri);
                ViewportMesh.SetMaterialInstance(rc, 0, MtlInst,
                    CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);

                var affectLights = new List<GamePlay.SceneGraph.GSceneGraph.AffectLight>();
                ViewportMesh.mMeshVars.SetPointLights(affectLights);

                await pass.InitPassForViewportView(rc, mMergeInstanceSE, MtlInst, ViewportMesh);

                mPass = pass;

                var vsDesc = mPass.Effect.ShaderProgram.VertexShader.Desc;

                var tbInfo = new CTextureBindInfo();
                if (vsDesc.GetSRVDesc(vsDesc.FindSRVDesc("AllVertexArray"), ref tbInfo))
                {

                }
                if (vsDesc.GetSRVDesc(vsDesc.FindSRVDesc("MeshInstanceArray"), ref tbInfo))
                {

                }
            }

            //const string CSVersion = "cs_5_0";
            var macros = new CShaderDefinitions();
            var shaderFile = RName.GetRName("Shaders/Compute/GpuDriven/Cluster.compute", RName.enRNameType.Engine);

            mCS_MeshBatchDesc = rc.CreateShaderDesc(shaderFile, "CSMain_MeshBatch", EShaderType.EST_ComputeShader, macros, CIPlatform.Instance.PlatformType);
            mCS_MeshBatch = rc.CreateComputeShader(mCS_MeshBatchDesc);

            mCS_ClearBatchArgsDesc = rc.CreateShaderDesc(shaderFile, "CSMain_ClearBatchArgs", EShaderType.EST_ComputeShader, macros, CIPlatform.Instance.PlatformType);
            mCS_ClearBatchArgs = rc.CreateComputeShader(mCS_ClearBatchArgsDesc);

            var cbIndex = mCS_MeshBatchDesc.FindCBufferDesc("cbMeshBatch");
            if (cbIndex != uint.MaxValue)
                mCBMeshBatch = rc.CreateConstantBuffer(mCS_MeshBatchDesc, cbIndex);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool EnableDraw
        {
            get;
            set;
        } = true;
        public void DrawScene(CRenderContext rc, CCommandList cmd, Graphics.CGfxCamera Camera, Graphics.View.CGfxSceneView view)
        {
            if (EnableDraw == false)
                return;
            if (mNeedUpdateGpuBuffer)
            {
                mNeedUpdateGpuBuffer = false;
                UpdateGpuBuffer(rc, cmd, Camera);
            }

            var program = mPass.Effect.ShaderProgram;
            if (UseComputeShader == false)
            {
                UpdateIndexBufferCPU(cmd, Camera);
            }
            else
            {
                ComputeDispatch(rc, cmd, Camera);
            }

            if(UseVTF)
            {
                CTextureBindInfo tbInfo = new CTextureBindInfo();
                if (program.FindTextureBindInfo(null, "gVertexTexture", ref tbInfo))
                {
                    mPass.ShaderResources.VSBindTexture(tbInfo.VSBindPoint, mVertexTextureView);
                }
                if (program.FindTextureBindInfo(null, "gInstanceDataTexture", ref tbInfo))
                {
                    mPass.ShaderResources.VSBindTexture(tbInfo.VSBindPoint, mInstanceDataTextureView);
                }
                var spInfo = new CSamplerBindInfo();
                if (program.FindSamplerBindInfo(null, "Samp_gVertexTexture", ref spInfo))
                {
                    mPass.ShaderSamplerBinder.VSBindSampler(spInfo.VSBindPoint, mSamplerState);
                }
                if (program.FindSamplerBindInfo(null, "Samp_gInstanceDataTexture", ref spInfo))
                {
                    mPass.ShaderSamplerBinder.VSBindSampler(spInfo.VSBindPoint, mSamplerState);
                }
            }
            else
            {
                //CTextureBindInfo tbInfo = new CTextureBindInfo();
                //if(program.FindTextureBindInfo(null, "AllVertexArray", ref tbInfo))
                //{
                //    mPass.ShaderResources.VSBindTexture(tbInfo.VSBindPoint, mAllVertexSRV);
                //}
                //if (program.FindTextureBindInfo(null, "MeshInstanceArray", ref tbInfo))
                //{
                //    mPass.ShaderResources.VSBindTexture(tbInfo.VSBindPoint, mMeshInstanceSRV);
                //}
                mPass.ShaderResources.VSBindTexture(14, mAllVertexSRV);
                mPass.ShaderResources.VSBindTexture(13, mMeshInstanceSRV);
            }

            CConstantBufferDesc cbInfo = new CConstantBufferDesc();
            if (mCBMeshBatch!=null && program.GetCBufferDesc(program.FindCBuffer("cbMeshBatch"), ref cbInfo))
            {
                mPass.BindCBufferVS(cbInfo.VSBindPoint, mCBMeshBatch);
            }
            mPass.ViewPort = Camera.SceneView.Viewport;

            mPass.BindCBuffer(mPass.Effect.ShaderProgram, mPass.Effect.CacheData.CBID_Camera, Camera.CBuffer);
            if (view != null)
                mPass.BindCBuffer(mPass.Effect.ShaderProgram, mPass.Effect.CacheData.CBID_View, view.SceneViewCB);

            //mPass.ViewPort = view.Viewport;

            if (UseComputeShader==false)
            {
                mPass.AttachIndexBuffer = mCpuDrawIndexBuffer;

                var dpDesc = new CDrawPrimitiveDesc();
                dpDesc.SetDefault();
                dpDesc.NumPrimitives = mDrawArgs.IndexCountPerInstance / 3;
                mPass.GeometryMesh.SetAtom(0, 0, ref dpDesc);
            }
            else
            {
                mPass.AttachIndexBuffer = mDrawIndexBuffer;
                mPass.SetIndirectDraw(bufferIndirectDrawArgs, 0);
            }

            cmd.PushPass(mPass);
        }
    }
}

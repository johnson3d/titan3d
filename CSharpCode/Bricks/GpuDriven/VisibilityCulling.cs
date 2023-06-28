using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Common;
using System.Threading.Tasks;
using EngineNS.Graphics.Pipeline;
using EngineNS.GamePlay;
using NPOI.HSSF.Record.AutoFilter;

namespace EngineNS.Bricks.GpuDriven
{
    //nanite: https://zhuanlan.zhihu.com/p/382687738

    //public class TtGpuSceneCullInstanceShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    //{
    //    public override Vector3ui DispatchArg
    //    {
    //        get => new Vector3ui(64, 1, 1);
    //    }
    //    public TtGpuSceneCullInstanceShading()
    //    {
    //        CodeName = RName.GetRName("Shaders/Occlusion/GpuSceneCullInstance.cginc", RName.ERNameType.Engine);
    //        MainName = "GpuSceneCullInstance";

    //        this.UpdatePermutation();
    //    }
    //    protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
    //    {
    //        base.EnvShadingDefines(in id, defines);
    //    }
    //    public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
    //    {
    //        var node = drawcall.TagObject as TtCullInstanceNode;

    //        drawcall.BindUav("InstanceSceneData", node.CullInstancesBuffer.DataUAV);
    //        drawcall.BindUav("NumberVisibilityGpuActorBuffer", node.NumberVisibilityGpuActorBuffer.DataUAV);
    //        drawcall.BindUav("VisibilityGpuActorsBuffer", node.VisibilityGpuActorsBuffer.DataUAV);
    //        drawcall.BindSrv("NumberGpuActors", node.NumberGpuActorsBuffer.DataSRV);
    //        drawcall.BindCBuffer("cbPerPatchHZBCullData", null);

    //        var index = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerPatchHZBCullData");
    //        if (index.IsValidPointer)
    //        {
    //            if (node.HZBCullInstanceCBuffer == null)
    //            {
    //                node.HZBCullInstanceCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
    //            }
    //            drawcall.BindCBuffer(index, node.HZBCullInstanceCBuffer);
    //        }
    //    }
    //}

    //public class TtGpuSceneCullClusterSetupShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    //{
    //    public override Vector3ui DispatchArg
    //    {
    //        get => new Vector3ui(1, 1, 1);
    //    }
    //    public TtGpuSceneCullClusterSetupShading()
    //    {
    //        CodeName = RName.GetRName("Shaders/Occlusion/SetupDrawClusterArgs.cginc", RName.ERNameType.Engine);
    //        MainName = "SetupDrawClusterArgsCS";

    //        this.UpdatePermutation();
    //    }
    //    protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
    //    {
    //        base.EnvShadingDefines(in id, defines);
    //    }
    //    public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
    //    {
    //        var node = drawcall.TagObject as TtCullInstanceNode;

    //        drawcall.BindSrv("NumnerVisibleClusterMeshData", node.NumnerVisibleClusterMeshData.DataSRV);
    //        drawcall.BindUav("DrawClusterIndirectArgs", node.SetupDrawClusterIndirectArgs.DataUAV);
    //    }
    //}

    public class TtGpuSceneCullClusterShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(64, 1, 1);
        }
        public TtGpuSceneCullClusterShading()
        {
            CodeName = RName.GetRName("Shaders/Occlusion/GpuSceneCullCluster.cginc", RName.ERNameType.Engine);
            MainName = "GpuSceneCullCluster";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtCullInstanceNode;

            //drawcall.BindSrv("NumberVisibilityGpuActorBuffer", node.NumberVisibilityGpuActorBuffer.DataSRV);
            //drawcall.BindUav("VisibleClusterMeshData", node.VisibleClusterMeshData.DataUAV);
            //drawcall.BindUav("NumnerVisibleClusterMeshData", node.NumnerVisibleClusterMeshData.DataUAV);

            if (node == null)
            {
                node = policy.FindFirstNode<TtCullInstanceNode>();
            }
            drawcall.BindUav("VisibleTriangles", node.VisibleTriangles.DataUAV);
        }
    }

    public class TtCullInstanceNode : URenderGraphNode
    {
        //public struct FActorInstance
        //{
        //    public Matrix WorldMatrix;
        //}
        //public struct TtCullInstanceData
        //{
        //    public Vector3 BoundCenter;
        //    public uint ChildrenStart;
        //    public Vector3 BoundExtent;
        //    public uint ChildrenEnd;
        //    public Matrix WorldMatrix;
        //    public uint GpuSceneIndex;
        //    public int ClusterCount;
        //}
        //public struct TtNumberVisibilityGpuActorBuffer
        //{
        //    public int InstanceCount;
        //    public int ClusterCount;
        //}
        //public struct TtVisibilityGpuActorData
        //{
        //    public int GpuSceneIndex;
        //    public int InstanceCount;
        //    public int ClusterCount;
        //}
        //public struct TtCullClusterData
        //{
        //    public Vector3 BoundCenter;
        //    public Vector3 BoundExtent;
        //    public Matrix WorldMatrix;
        //    public uint ClusterID;
        //}
        //public struct TtVisibleClusterMeshData
        //{
        //    public int GpuSceneIndex;
        //    public uint ClusterId;
        //}
        //public struct TtNumnerVisibleClusterMeshData
        //{
        //    public uint ClusterCount;
        //}

        //public TtCpu2GpuBuffer<TtCullInstanceData> CullInstancesBuffer = new TtCpu2GpuBuffer<TtCullInstanceData>();
        //public TtCpu2GpuBuffer<TtCullClusterData> CullClustersBuffer = new TtCpu2GpuBuffer<TtCullClusterData>();
        //public TtGpuBuffer<TtNumberVisibilityGpuActorBuffer> NumberVisibilityGpuActorBuffer = new TtGpuBuffer<TtNumberVisibilityGpuActorBuffer>();
        //public TtGpuBuffer<TtVisibilityGpuActorData> VisibilityGpuActorsBuffer = new TtGpuBuffer<TtVisibilityGpuActorData>();
        //public TtGpuBuffer<uint> NumberGpuActorsBuffer = new TtGpuBuffer<uint>();

        //public TtGpuBuffer<uint> SetupDrawClusterIndirectArgs = new TtGpuBuffer<uint>();

        //public TtGpuBuffer<TtVisibleClusterMeshData> VisibleClusterMeshData = new TtGpuBuffer<TtVisibleClusterMeshData>();
        //public TtGpuBuffer<TtNumnerVisibleClusterMeshData> NumnerVisibleClusterMeshData = new TtGpuBuffer<TtNumnerVisibleClusterMeshData>();
        //public NxRHI.UCbView HZBCullInstanceCBuffer;
        //public TtGpuSceneCullInstanceShading GpuSceneCullInstanceShading;
        //private NxRHI.UComputeDraw GpuSceneCullInstanceDrawcall;

        //public TtGpuSceneCullClusterSetupShading GpuSceneCullClusterSetupShading;
        //private NxRHI.UComputeDraw GpuSceneCullClusterSetupDrawcall;

        public struct Triangle
        {
            public Vector3 pos0;
            public Vector3 pos1;
            public Vector3 pos2;
        }

        //public URenderGraphPin HzbPinIn = URenderGraphPin.CreateInput("Hzb");
        public URenderGraphPin VisibleClutersPinOut = URenderGraphPin.CreateOutput("VisibleClusters", false, EPixelFormat.PXF_UNKNOWN);

        public TtGpuSceneCullClusterShading GpuSceneCullClusterShading;
        private NxRHI.UComputeDraw TtGpuSceneCullClusterDrawcall;

        public TtGpuBuffer<Triangle> VisibleTriangles = new TtGpuBuffer<Triangle>();
        
        public TtCullInstanceNode()
        {
            Name = "GpuCullInstanceNode";
        }
        public override void InitNodePins()
        {
            AddOutput(VisibleClutersPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            
            base.InitNodePins();
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            CoreSDK.DisposeObject(ref TtGpuSceneCullClusterDrawcall);
            TtGpuSceneCullClusterDrawcall = rc.CreateComputeDraw();
            GpuSceneCullClusterShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuSceneCullClusterShading>();

            unsafe
            {
                // TODO: max < 128 triangles per cluster
                VisibleTriangles.SetSize(128, null, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            }            
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            base.BeforeTickLogic(policy);

            var visibleClusters = ImportAttachment(VisibleClutersPinOut);
            visibleClusters.Srv = VisibleTriangles.DataSRV;
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            BuildInstances(world, policy.DefaultCamera.VisParameter);

            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();

            GpuSceneCullClusterShading.SetDrawcallIndirectDispatch(policy, TtGpuSceneCullClusterDrawcall, null);
            cmd.PushGpuDraw(TtGpuSceneCullClusterDrawcall);

            cmd.FlushDraws();
            cmd.EndCommand();
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }

        //public List<GamePlay.Scene.TtGpuSceneNode> GpuSceneActors = new List<GamePlay.Scene.TtGpuSceneNode>();
        //public TtCpu2GpuBuffer<FActorInstance> GpuInstances = new TtCpu2GpuBuffer<FActorInstance>();

        public void BuildInstances(GamePlay.UWorld world, GamePlay.UWorld.UVisParameter rp)
        {
            //foreach(var i in rp.VisibleNodes)
            //{
            //    var meshNode = i as GamePlay.Scene.UMeshNode;
            //    if (meshNode == null)
            //        continue;

            //    var cluster = meshNode.Mesh.MaterialMesh.Mesh.ClusteredMesh;
            //}
            //foreach (var i in GpuSceneActors)
            //{
            //    i.GpuSceneIndex = -1;
            //}
            //GpuSceneActors.Clear();
            //world.Root.DFS_VisitNodeTree(static (GamePlay.Scene.UNode node, object arg) =>
            //{
            //    var actor = node as GamePlay.Scene.TtGpuSceneNode;
            //    if (actor == null)
            //        return false;
            //    var This = arg as UGpuSceneNode;
            //    This.GpuSceneActors.Add(actor);

            //    return false;
            //}, this);

            //CullInstancesBuffer.SetSize(GpuSceneActors.Count);
            //uint CalculateClusterID = 0;
            //foreach (var i in GpuSceneActors)
            //{
            //    FActorInstance data;
            //    data.WorldMatrix = i.Placement.AbsTransform.ToMatrixWithScale(in world.mCameraOffset);
            //    i.GpuSceneIndex = GpuInstances.PushData(data);

            //    TtCullInstanceData CullInstanceData;
            //    CullInstanceData.BoundCenter = i.BoundVolume.LocalAABB.GetCenter();
            //    CullInstanceData.BoundExtent = i.BoundVolume.LocalAABB.GetSize();

            //    CullInstanceData.WorldMatrix = data.WorldMatrix;
            //    CullInstanceData.GpuSceneIndex = (uint)i.GpuSceneIndex;
            //    CullInstanceData.ClusterCount = i.ClusteredMeshs.Count;


            //    uint StartClusterID = CalculateClusterID;
            //    //foreach (var ClusteredMesh in i.ClusteredMeshs)
            //    for (int j = 0; j < i.ClusteredMeshs.Count; j++)
            //    {
            //        var ClusteredMesh = i.ClusteredMeshs[j];
            //        TtCullClusterData CullClusterData;
            //        CullClusterData.BoundCenter = (ClusteredMesh.AABB.Minimum + ClusteredMesh.AABB.Maximum) * 0.5f;
            //        CullClusterData.BoundExtent = ClusteredMesh.AABB.Maximum - ClusteredMesh.AABB.Minimum;
            //        CullClusterData.WorldMatrix = data.WorldMatrix;

            //        CalculateClusterID = (uint)CullClustersBuffer.DataArray.Count;
            //        CullClusterData.ClusterID = CalculateClusterID;
            //        CullClustersBuffer.PushData(CullClusterData);
            //    }

            //    CullInstanceData.ChildrenStart = StartClusterID;
            //    CullInstanceData.ChildrenEnd = CalculateClusterID;

            //    CullInstancesBuffer.PushData(CullInstanceData);
            //}
            ////ActorInstances.SetSize(GpuSceneActors.Count);
        }
    }

    public class TtCullClusterShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(64, 1, 1);
        }
        public TtCullClusterShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/ClusterCulling.cginc", RName.ERNameType.Engine);
            MainName = "CS_ClusterCullingMain";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtCullClusterNode;

            drawcall.BindSrv("ClusterBuffer", node.Clusters.DataSRV);
            drawcall.BindSrv("SrcClusterBuffer", node.Clusters.DataSRV);
            drawcall.BindUav("VisClusterBuffer", node.Clusters.DataUAV);
        }
    }
    public class TtCullClusterNode : URenderGraphNode
    {
        public URenderGraphPin HzbPinIn = URenderGraphPin.CreateInput("Hzb");
        public URenderGraphPin VerticesPinIn = URenderGraphPin.CreateInputOutput("Vertices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin IndicesPinIn = URenderGraphPin.CreateInputOutput("Indices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin ClustersPinIn = URenderGraphPin.CreateInputOutput("Clusters", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin VisibleClutersPinOut = URenderGraphPin.CreateOutput("VisibleClusters", false, EPixelFormat.PXF_UNKNOWN);

        public TtGpuSceneCullClusterShading GpuSceneCullClusterShading;
        private NxRHI.UComputeDraw TtGpuSceneCullClusterDrawcall;

        
        public TtCpu2GpuBuffer<float> Vertices = new TtCpu2GpuBuffer<float>();
        public TtCpu2GpuBuffer<uint> Indices = new TtCpu2GpuBuffer<uint>();
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FClusterData
        {
            public Vector3 BoundCenter;
            public int FaceStart;
            public Vector3 BoundExtent;
            public int FaceEnd;
            public Matrix WorldMatrix;
        }
        public TtCpu2GpuBuffer<FClusterData> Clusters = new TtCpu2GpuBuffer<FClusterData>();
        public TtCpu2GpuBuffer<int> SrcClusters = new TtCpu2GpuBuffer<int>();
        public TtCpu2GpuBuffer<int> VisClusters = new TtCpu2GpuBuffer<int>();

        public TtCullClusterNode()
        {
            Name = "CullClusterNode";
        }
        public override void InitNodePins()
        {
            AddInput(HzbPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(VerticesPinIn, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(IndicesPinIn, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(ClustersPinIn, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddOutput(VisibleClutersPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

            HzbPinIn.IsAllowInputNull = true;
            VerticesPinIn.IsAllowInputNull = true;
            IndicesPinIn.IsAllowInputNull = true;
            ClustersPinIn.IsAllowInputNull = true;

            base.InitNodePins();
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            CoreSDK.DisposeObject(ref TtGpuSceneCullClusterDrawcall);
            TtGpuSceneCullClusterDrawcall = rc.CreateComputeDraw();
            GpuSceneCullClusterShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuSceneCullClusterShading>();

            if (true)
            {
                InitBuffers();
            }

            unsafe
            {
                VisClusters.Initialize(true);
                VisClusters.SetSize(sizeof(int) * 100 + sizeof(int));
                var visClst = stackalloc int[100 + 1];
                visClst[0] = 0;
                VisClusters.UpdateData(0, visClst, sizeof(int) * 100 + sizeof(int));
                VisClusters.Flush2GPU();
            }
        }
        private unsafe void InitBuffers()
        {
            Vertices.Initialize(false);
            Vertices.SetSize(sizeof(FQuarkVertex) * 3 / sizeof(float));
            var verts = stackalloc FQuarkVertex[3];
            verts[0].Position = new Vector3(50, 50, 0);
            verts[1].Position = new Vector3(150, 50, 0);
            verts[2].Position = new Vector3(50, 150, 0);
            Vertices.UpdateData(0, verts, sizeof(FQuarkVertex) * 3);
            Vertices.Flush2GPU();

            Indices.Initialize(false);
            Indices.SetSize(sizeof(uint) * 3);
            var idx = stackalloc uint[3];
            idx[0] = 0;
            idx[1] = 1;
            idx[2] = 2;
            Indices.UpdateData(0, idx, sizeof(uint) * 3);
            Indices.Flush2GPU();

            Clusters.Initialize(false);
            Clusters.SetSize(sizeof(FClusterData) * 1);
            var clst = stackalloc FClusterData[1];
            clst[0].WorldMatrix = Matrix.Identity;
            clst[0].BoundCenter = Vector3.Zero;
            clst[0].BoundExtent = Vector3.One;
            clst[0].FaceStart = 0;
            clst[0].FaceEnd = 0;
            Clusters.UpdateData(0, clst, sizeof(FClusterData) * 1);
            Clusters.Flush2GPU();

            SrcClusters.Initialize(false);
            SrcClusters.SetSize(sizeof(int) * 1 + sizeof(int));
            var srcClst = stackalloc int[1 + 1];
            srcClst[0] = 1;
            srcClst[0 + 1] = 0;
            SrcClusters.UpdateData(0, srcClst, sizeof(int) * 1 + sizeof(int));
            SrcClusters.Flush2GPU();
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            base.BeforeTickLogic(policy);

            var attachment = ImportAttachment(VisibleClutersPinOut);
            attachment.Uav = VisClusters.DataUAV;

            if (VerticesPinIn.FindInLinker() == null)
            {
                attachment = ImportAttachment(VerticesPinIn);
                attachment.Srv = Vertices.DataSRV;
            }
            if (IndicesPinIn.FindInLinker() == null)
            {
                attachment = ImportAttachment(IndicesPinIn);
                attachment.Srv = Indices.DataSRV;
            }
            if (ClustersPinIn.FindInLinker() == null)
            {
                attachment = ImportAttachment(ClustersPinIn);
                attachment.Srv = Clusters.DataSRV;
            }
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            BuildInstances(world, policy.DefaultCamera.VisParameter);

            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();

            // TODO: dispatch x/y/z
            GpuSceneCullClusterShading.SetDrawcallDispatch(policy, TtGpuSceneCullClusterDrawcall, 1, 1, 1, true);

            cmd.PushGpuDraw(TtGpuSceneCullClusterDrawcall);

            cmd.FlushDraws();
            cmd.EndCommand();
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }

        //public List<GamePlay.Scene.TtGpuSceneNode> GpuSceneActors = new List<GamePlay.Scene.TtGpuSceneNode>();
        //public TtCpu2GpuBuffer<FActorInstance> GpuInstances = new TtCpu2GpuBuffer<FActorInstance>();

        public void BuildInstances(GamePlay.UWorld world, GamePlay.UWorld.UVisParameter rp)
        {
            foreach (var i in rp.VisibleNodes)
            {
                var meshNode = i as GamePlay.Scene.UMeshNode;
                if (meshNode == null)
                    continue;

                var cluster = meshNode.Mesh.MaterialMesh.Mesh.ClusteredMesh;
            }
            //foreach (var i in GpuSceneActors)
            //{
            //    i.GpuSceneIndex = -1;
            //}
            //GpuSceneActors.Clear();
            //world.Root.DFS_VisitNodeTree(static (GamePlay.Scene.UNode node, object arg) =>
            //{
            //    var actor = node as GamePlay.Scene.TtGpuSceneNode;
            //    if (actor == null)
            //        return false;
            //    var This = arg as UGpuSceneNode;
            //    This.GpuSceneActors.Add(actor);

            //    return false;
            //}, this);

            //CullInstancesBuffer.SetSize(GpuSceneActors.Count);
            //uint CalculateClusterID = 0;
            //foreach (var i in GpuSceneActors)
            //{
            //    FActorInstance data;
            //    data.WorldMatrix = i.Placement.AbsTransform.ToMatrixWithScale(in world.mCameraOffset);
            //    i.GpuSceneIndex = GpuInstances.PushData(data);

            //    TtCullInstanceData CullInstanceData;
            //    CullInstanceData.BoundCenter = i.BoundVolume.LocalAABB.GetCenter();
            //    CullInstanceData.BoundExtent = i.BoundVolume.LocalAABB.GetSize();

            //    CullInstanceData.WorldMatrix = data.WorldMatrix;
            //    CullInstanceData.GpuSceneIndex = (uint)i.GpuSceneIndex;
            //    CullInstanceData.ClusterCount = i.ClusteredMeshs.Count;


            //    uint StartClusterID = CalculateClusterID;
            //    //foreach (var ClusteredMesh in i.ClusteredMeshs)
            //    for (int j = 0; j < i.ClusteredMeshs.Count; j++)
            //    {
            //        var ClusteredMesh = i.ClusteredMeshs[j];
            //        TtCullClusterData CullClusterData;
            //        CullClusterData.BoundCenter = (ClusteredMesh.AABB.Minimum + ClusteredMesh.AABB.Maximum) * 0.5f;
            //        CullClusterData.BoundExtent = ClusteredMesh.AABB.Maximum - ClusteredMesh.AABB.Minimum;
            //        CullClusterData.WorldMatrix = data.WorldMatrix;

            //        CalculateClusterID = (uint)CullClustersBuffer.DataArray.Count;
            //        CullClusterData.ClusterID = CalculateClusterID;
            //        CullClustersBuffer.PushData(CullClusterData);
            //    }

            //    CullInstanceData.ChildrenStart = StartClusterID;
            //    CullInstanceData.ChildrenEnd = CalculateClusterID;

            //    CullInstancesBuffer.PushData(CullInstanceData);
            //}
            ////ActorInstances.SetSize(GpuSceneActors.Count);
        }
    }
}


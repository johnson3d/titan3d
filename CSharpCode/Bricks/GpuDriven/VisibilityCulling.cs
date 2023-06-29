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
            drawcall.BindSrv("SrcClusterBuffer", node.SrcClusters.DataSRV);
            drawcall.BindUav("VisClusterBuffer", node.VisClusters.DataUAV);
        }
    }
    public class TtCullClusterNode : URenderGraphNode
    {
        public URenderGraphPin HzbPinIn = URenderGraphPin.CreateInput("Hzb");
        public URenderGraphPin VerticesPinIn = URenderGraphPin.CreateInputOutput("Vertices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin IndicesPinIn = URenderGraphPin.CreateInputOutput("Indices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin ClustersPinIn = URenderGraphPin.CreateInputOutput("Clusters", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin VisibleClutersPinOut = URenderGraphPin.CreateOutput("VisibleClusters", false, EPixelFormat.PXF_UNKNOWN);
        
        public TtCullClusterShading CullClusterShading;
        private NxRHI.UComputeDraw CullClusterShadingDrawcall;

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

            CoreSDK.DisposeObject(ref CullClusterShadingDrawcall);
            CullClusterShadingDrawcall = rc.CreateComputeDraw();
            CullClusterShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtCullClusterShading>();

            if (true)
            {
                InitBuffers();
            }

            unsafe
            {
                VisClusters.Initialize(true);
                VisClusters.SetSize(100 + 1);
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
            Indices.SetSize(3);
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
            SrcClusters.SetSize(1 + 1);
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
            attachment.Srv = VisClusters.DataSRV;

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
        public unsafe override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            BuildInstances(world, policy.DefaultCamera.VisParameter);

            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();
            
            NxRHI.FBufferWriter bfWriter = new NxRHI.FBufferWriter();
            bfWriter.Buffer = VisClusters.GpuBuffer.mCoreObject;
            bfWriter.Offset = 0;
            bfWriter.Value = 0;
            cmd.WriteBufferUINT32(1, &bfWriter);
            // TODO: dispatch x/y/z
            CullClusterShading.SetDrawcallDispatch(this, policy, CullClusterShadingDrawcall, 1, 1, 1, true);
            cmd.PushGpuDraw(CullClusterShadingDrawcall);

            cmd.BeginEvent(Name);
            cmd.FlushDraws();
            cmd.EndEvent();

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


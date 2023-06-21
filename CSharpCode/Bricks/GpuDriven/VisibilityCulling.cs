using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Common;
using System.Threading.Tasks;
using EngineNS.Graphics.Pipeline;
using EngineNS.GamePlay;

namespace EngineNS.Bricks.GpuDriven
{
    //nanite: https://zhuanlan.zhihu.com/p/382687738

    public class TtGpuSceneCullInstanceShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(64, 1, 1);
        }
        public TtGpuSceneCullInstanceShading()
        {
            CodeName = RName.GetRName("Shaders/Occlusion/GpuSceneCullInstance.cginc", RName.ERNameType.Engine);
            MainName = "GpuSceneCullInstance";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtGpuCullNode;

            drawcall.BindUav("InstanceSceneData", node.CullInstancesBuffer.DataUAV);
            drawcall.BindUav("NumberVisibilityGpuActorBuffer", node.NumberVisibilityGpuActorBuffer.DataUAV);
            drawcall.BindUav("VisibilityGpuActorsBuffer", node.VisibilityGpuActorsBuffer.DataUAV);
            drawcall.BindSrv("NumberGpuActors", node.NumberGpuActorsBuffer.DataSRV);
            drawcall.BindCBuffer("cbPerPatchHZBCullData", null);

            var index = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerPatchHZBCullData");
            if (index.IsValidPointer)
            {
                if (node.HZBCullInstanceCBuffer == null)
                {
                    node.HZBCullInstanceCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, node.HZBCullInstanceCBuffer);
            }
        }
    }

    public class TtGpuSceneCullClusterSetupShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(1, 1, 1);
        }
        public TtGpuSceneCullClusterSetupShading()
        {
            CodeName = RName.GetRName("Shaders/Occlusion/SetupDrawClusterArgs.cginc", RName.ERNameType.Engine);
            MainName = "SetupDrawClusterArgsCS";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtGpuCullNode;

            drawcall.BindSrv("NumnerVisibleClusterMeshData", node.NumnerVisibleClusterMeshData.DataSRV);
            drawcall.BindUav("DrawClusterIndirectArgs", node.SetupDrawClusterIndirectArgs.DataUAV);
        }
    }

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
            var node = drawcall.TagObject as TtGpuCullNode;

            drawcall.BindSrv("NumberVisibilityGpuActorBuffer", node.NumberVisibilityGpuActorBuffer.DataSRV);
            drawcall.BindUav("VisibleClusterMeshData", node.VisibleClusterMeshData.DataUAV);
            drawcall.BindUav("NumnerVisibleClusterMeshData", node.NumnerVisibleClusterMeshData.DataUAV);
        }
    }

    public class TtGpuCullNode : URenderGraphNode
    {
        public URenderGraphPin HzbPinIn = URenderGraphPin.CreateInput("Hzb");

        public URenderGraphPin VisActorsPinOut = URenderGraphPin.CreateOutput("VisActors", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin VisClutersPinOut = URenderGraphPin.CreateOutput("VisClusters", false, EPixelFormat.PXF_UNKNOWN);
        public struct FActorInstance
        {
            public Matrix WorldMatrix;
        }
        public struct TtCullInstanceData
        {
            public Vector3 BoundCenter;
            public uint ChildrenStart;
            public Vector3 BoundExtent;
            public uint ChildrenEnd;
            public Matrix WorldMatrix;
            public uint GpuSceneIndex;
            public int ClusterCount;
        }
        public struct TtNumberVisibilityGpuActorBuffer
        {
            public int InstanceCount;
            public int ClusterCount;
        }
        public struct TtVisibilityGpuActorData
        {
            public int GpuSceneIndex;
            public int InstanceCount;
            public int ClusterCount;
        }
        public struct TtCullClusterData
        {
            public Vector3 BoundCenter;
            public Vector3 BoundExtent;
            public Matrix WorldMatrix;
            public uint ClusterID;
        }
        public struct TtVisibleClusterMeshData
        {
            public int GpuSceneIndex;
            public uint ClusterId;
        }
        public struct TtNumnerVisibleClusterMeshData
        {
            public uint ClusterCount;
        }
        public TtCpu2GpuBuffer<TtCullInstanceData> CullInstancesBuffer = new TtCpu2GpuBuffer<TtCullInstanceData>();
        public TtCpu2GpuBuffer<TtCullClusterData> CullClustersBuffer = new TtCpu2GpuBuffer<TtCullClusterData>();
        public TtGpuBuffer<TtNumberVisibilityGpuActorBuffer> NumberVisibilityGpuActorBuffer = new TtGpuBuffer<TtNumberVisibilityGpuActorBuffer>();
        public TtGpuBuffer<TtVisibilityGpuActorData> VisibilityGpuActorsBuffer = new TtGpuBuffer<TtVisibilityGpuActorData>();
        public TtGpuBuffer<uint> NumberGpuActorsBuffer = new TtGpuBuffer<uint>();

        public TtGpuBuffer<uint> SetupDrawClusterIndirectArgs = new TtGpuBuffer<uint>();

        public TtGpuBuffer<TtVisibleClusterMeshData> VisibleClusterMeshData = new TtGpuBuffer<TtVisibleClusterMeshData>();
        public TtGpuBuffer<TtNumnerVisibleClusterMeshData> NumnerVisibleClusterMeshData = new TtGpuBuffer<TtNumnerVisibleClusterMeshData>();
        public NxRHI.UCbView HZBCullInstanceCBuffer;

        public TtGpuSceneCullInstanceShading GpuSceneCullInstanceShading;
        private NxRHI.UComputeDraw GpuSceneCullInstanceDrawcall;

        public TtGpuSceneCullClusterSetupShading GpuSceneCullClusterSetupShading;
        private NxRHI.UComputeDraw GpuSceneCullClusterSetupDrawcall;

        public TtGpuSceneCullClusterShading GpuSceneCullClusterShading;
        private NxRHI.UComputeDraw TtGpuSceneCullClusterDrawcall;

        public TtGpuCullNode()
        {
            Name = "GpuCullInstanceNode";
        }
        public override void InitNodePins()
        {
            AddOutput(VisActorsPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            
            base.InitNodePins();
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            CoreSDK.DisposeObject(ref GpuSceneCullInstanceDrawcall);
            GpuSceneCullInstanceDrawcall = rc.CreateComputeDraw();
            GpuSceneCullInstanceShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuSceneCullInstanceShading>();

            CoreSDK.DisposeObject(ref GpuSceneCullClusterSetupDrawcall);
            GpuSceneCullClusterSetupDrawcall = rc.CreateComputeDraw();
            GpuSceneCullClusterSetupShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuSceneCullClusterSetupShading>();

            CoreSDK.DisposeObject(ref TtGpuSceneCullClusterDrawcall);
            TtGpuSceneCullClusterDrawcall = rc.CreateComputeDraw();
            GpuSceneCullClusterShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuSceneCullClusterShading>();
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            base.BeforeTickLogic(policy);

            var visActors = ImportAttachment(VisActorsPinOut);
            visActors.Uav = VisibilityGpuActorsBuffer.DataUAV;
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();

            GpuSceneCullInstanceShading.SetDrawcallDispatch(policy, GpuSceneCullInstanceDrawcall, (uint)GpuSceneActors.Count,
                            1, 1, true);
            cmd.PushGpuDraw(GpuSceneCullInstanceDrawcall);

            GpuSceneCullClusterSetupShading.SetDrawcallDispatch(policy, GpuSceneCullClusterSetupDrawcall, 1, 1, 1, true);
            cmd.PushGpuDraw(GpuSceneCullClusterSetupDrawcall);

            GpuSceneCullClusterShading.SetDrawcallIndirectDispatch(policy, TtGpuSceneCullClusterDrawcall, null);
            cmd.PushGpuDraw(TtGpuSceneCullClusterDrawcall);

            cmd.FlushDraws();
            cmd.EndCommand();
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }
        public List<GamePlay.Scene.TtGpuSceneNode> GpuSceneActors = new List<GamePlay.Scene.TtGpuSceneNode>();
        public TtCpu2GpuBuffer<FActorInstance> GpuInstances = new TtCpu2GpuBuffer<FActorInstance>();
        public void BuildInstances(GamePlay.UWorld world, GamePlay.UWorld.UVisParameter rp)
        {
            foreach(var i in rp.VisibleNodes)
            {
                var meshNode = i as GamePlay.Scene.UMeshNode;
                if (meshNode != null)
                    continue;

                var cluster = meshNode.Mesh.MaterialMesh.Mesh.ClusteredMesh;
            }

            foreach (var i in GpuSceneActors)
            {
                i.GpuSceneIndex = -1;
            }
            GpuSceneActors.Clear();
            world.Root.DFS_VisitNodeTree(static (GamePlay.Scene.UNode node, object arg) =>
            {
                var actor = node as GamePlay.Scene.TtGpuSceneNode;
                if (actor == null)
                    return false;
                var This = arg as UGpuSceneNode;
                This.GpuSceneActors.Add(actor);

                return false;
            }, this);

            CullInstancesBuffer.SetSize(GpuSceneActors.Count);
            uint CalculateClusterID = 0;
            foreach (var i in GpuSceneActors)
            {
                FActorInstance data;
                data.WorldMatrix = i.Placement.AbsTransform.ToMatrixWithScale(in world.mCameraOffset);
                i.GpuSceneIndex = GpuInstances.PushData(data);

                TtCullInstanceData CullInstanceData;
                CullInstanceData.BoundCenter = i.BoundVolume.LocalAABB.GetCenter();
                CullInstanceData.BoundExtent = i.BoundVolume.LocalAABB.GetSize();

                CullInstanceData.WorldMatrix = data.WorldMatrix;
                CullInstanceData.GpuSceneIndex = (uint)i.GpuSceneIndex;
                CullInstanceData.ClusterCount = i.ClusteredMeshs.Count;


                uint StartClusterID = CalculateClusterID;
                //foreach (var ClusteredMesh in i.ClusteredMeshs)
                for (int j = 0; j < i.ClusteredMeshs.Count; j++)
                {
                    var ClusteredMesh = i.ClusteredMeshs[j];
                    TtCullClusterData CullClusterData;
                    CullClusterData.BoundCenter = (ClusteredMesh.AABB.Minimum + ClusteredMesh.AABB.Maximum) * 0.5f;
                    CullClusterData.BoundExtent = ClusteredMesh.AABB.Maximum - ClusteredMesh.AABB.Minimum;
                    CullClusterData.WorldMatrix = data.WorldMatrix;

                    CalculateClusterID = (uint)CullClustersBuffer.DataArray.Count;
                    CullClusterData.ClusterID = CalculateClusterID;
                    CullClustersBuffer.PushData(CullClusterData);
                }

                CullInstanceData.ChildrenStart = StartClusterID;
                CullInstanceData.ChildrenEnd = CalculateClusterID;

                CullInstancesBuffer.PushData(CullInstanceData);
            }
            //ActorInstances.SetSize(GpuSceneActors.Count);
        }
    }
}


using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class TtGpuSceneNode
    {
        public TtRenderGraphPin InstancePinOut = TtRenderGraphPin.CreateOutput("Instances", false, EPixelFormat.PXF_UNKNOWN);
        public List<GamePlay.Scene.TtGpuSceneNode> GpuSceneActors = new List<GamePlay.Scene.TtGpuSceneNode>();
        public struct FActorInstance
        {
            public Matrix WorldMatrix;
        }
        public TtCpu2GpuBuffer<FActorInstance> GpuInstances = new TtCpu2GpuBuffer<FActorInstance>();

        public struct HZBCullData
        {
            public Matrix PrevTranslatedWorldToClip;
            public Matrix PrevPreViewTranslation;
            public Matrix WorldToClip;
            public Vector4i HZBTestViewRect;
        }

        public class UCBufferHZBCullData : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrevTranslatedWorldToClip;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrevPreViewTranslation;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldToClip;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4i))]
            public NxRHI.FShaderVarDesc HZBTestViewRect;
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

        public struct TtCullClusterData
        {
            public Vector3 BoundCenter;
            public Vector3 BoundExtent;
            public Matrix WorldMatrix;
            public uint ClusterID;
        }

        public struct TtVisibilityGpuActorData
        {
            public int GpuSceneIndex;
            public int InstanceCount;
            public int ClusterCount;
        }

        public struct TtNeedCullClusterMeshData
        {
            public int GpuSceneIndex;
            public uint ClusterId;
        }

        public struct TtNumberVisibilityGpuActorBuffer
        {
            public int InstanceCount;
            public int ClusterCount;
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

        //cs: cull Object to cluster PVS
        public TtGpuBuffer<TtCullInstanceData> ClusterPVSBuffer = new TtGpuBuffer<TtCullInstanceData>();
        //cs: cull PVS cluster to visibility buffer

        public TtGpuBuffer<TtVisibilityGpuActorData> VisibilityGpuActorsBuffer = new TtGpuBuffer<TtVisibilityGpuActorData>();
        
        public TtGpuBuffer<TtNeedCullClusterMeshData> NeedCullClusterMesBuffer = new TtGpuBuffer<TtNeedCullClusterMeshData>();

        public TtGpuBuffer<TtNumberVisibilityGpuActorBuffer> NumberVisibilityGpuActorBuffer = new TtGpuBuffer<TtNumberVisibilityGpuActorBuffer>();

        public TtGpuBuffer<TtVisibleClusterMeshData> VisibleClusterMeshData = new TtGpuBuffer<TtVisibleClusterMeshData>();
        public TtGpuBuffer<TtNumnerVisibleClusterMeshData> NumnerVisibleClusterMeshData = new TtGpuBuffer<TtNumnerVisibleClusterMeshData>();

        public TtGpuBuffer<uint> NumberGpuActorsBuffer = new TtGpuBuffer<uint>();

        public TtGpuBuffer<uint> VisibilityClusterBuffer = new TtGpuBuffer<uint>();

        public TtGpuBuffer<uint> CullClusterIndirectArgs = new TtGpuBuffer<uint>();

        public TtGpuBuffer<uint> SetupDrawClusterIndirectArgs = new TtGpuBuffer<uint>();


        public List<TtClusteDrawArgs> ClusteDrawArgsList = new List<TtClusteDrawArgs>();

        public class CullGpuIndexsShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                //get => new Vector3ui((MathHelper.Max(1, MathHelper.DivideAndRoundUp((uint)GpuSceneActors.Count, 64u))), 1, 1);
                get => new Vector3ui(64, 1, 1);
            }
            public CullGpuIndexsShading()
            {
                CodeName = RName.GetRName("Shaders/Occlusion/GpuSceneCullInstance.cginc", RName.ERNameType.Engine);
                MainName = "GpuSceneCullInstance";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtGpuSceneNode;

                node.cbPerHZBCullData_CullInstance = CurrentEffect.FindBinder("cbPerPatchHZBCullData");

                node.HZBCullInstanceData.UpdateFieldVar(CurrentEffect.mComputeShader, "cbPerPatchHZBCullData");
                node.HZBCullInstanceCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(node.HZBCullInstanceData.Binder.mCoreObject);

                //HZBCullInstanceCBuffer.SetValue

                drawcall.BindUav("InstanceSceneData", node.CullInstancesBuffer.Uav);
                drawcall.BindUav("NumberVisibilityGpuActorBuffer", node.NumberVisibilityGpuActorBuffer.Uav);
                drawcall.BindUav("VisibilityGpuActorsBuffer", node.VisibilityGpuActorsBuffer.Uav);
                drawcall.BindSrv("NumberGpuActors", node.NumberGpuActorsBuffer.Srv);
            }
        }
        public CullGpuIndexsShading Cull_CullGpuIndexs;

        public class SetupCullClusterArgsShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                //get => new Vector3ui((MathHelper.Max(1, MathHelper.DivideAndRoundUp((uint)GpuSceneActors.Count, 64u))), 1, 1);
                get => new Vector3ui(1, 1, 1);
            }
            public SetupCullClusterArgsShading()
            {
                CodeName = RName.GetRName("Shaders/Occlusion/GpuSceneSetupCullCluster.cginc", RName.ERNameType.Engine);
                MainName = "SetupCullClusterArgsCS";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtGpuSceneNode;

                drawcall.BindUav("CullClusterIndirectArgs", node.CullClusterIndirectArgs.Uav);
                drawcall.BindSrv("NumberVisibilityGpuActorBuffer", node.NumberVisibilityGpuActorBuffer.Srv);
            }
        }
        public SetupCullClusterArgsShading Cull_SetupCullClusterArgs;

        public class CullClusterShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                //get => new Vector3ui((MathHelper.Max(1, MathHelper.DivideAndRoundUp((uint)GpuSceneActors.Count, 64u))), 1, 1);
                get => new Vector3ui(1, 1, 1);
            }
            public CullClusterShading()
            {
                CodeName = RName.GetRName("Shaders/Occlusion/GpuSceneCullCluster.cginc", RName.ERNameType.Engine);
                MainName = "GpuSceneCullCluster";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtGpuSceneNode;

                drawcall.BindSrv("NumberVisibilityGpuActorBuffer", node.NumberVisibilityGpuActorBuffer.Srv);
                drawcall.BindUav("VisibleClusterMeshData", node.VisibleClusterMeshData.Uav);
                drawcall.BindUav("NumnerVisibleClusterMeshData", node.NumnerVisibleClusterMeshData.Uav);
            }
        }
        public CullClusterShading Cull_CullCluster;

        public class SetupDrawClusterArgsBufferShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                //get => new Vector3ui((MathHelper.Max(1, MathHelper.DivideAndRoundUp((uint)GpuSceneActors.Count, 64u))), 1, 1);
                get => new Vector3ui(1, 1, 1);
            }
            public SetupDrawClusterArgsBufferShading()
            {
                CodeName = RName.GetRName("Shaders/Occlusion/SetupDrawClusterArgs.cginc", RName.ERNameType.Engine);
                MainName = "SetupDrawClusterArgsCS";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtGpuSceneNode;

                drawcall.BindSrv("NumnerVisibleClusterMeshData", node.NumnerVisibleClusterMeshData.Srv);
                drawcall.BindUav("DrawClusterIndirectArgs", node.SetupDrawClusterIndirectArgs.Uav);
            }
        }
        public SetupDrawClusterArgsBufferShading Cull_SetupDrawClusterArgsBuffer;

        public NxRHI.TtComputeDraw Cull_CullGpuIndexsDrawcall;
        public NxRHI.TtComputeDraw Cull_SetupCullClusterArgsDrawcall;

        public NxRHI.TtComputeDraw Cull_CullClusterDrawcall;
        public NxRHI.TtComputeDraw Cull_SetupDrawClusterArgsDrawcall;

        public NxRHI.TtShaderBinder cbPerHZBCullData_CullInstance;
        public NxRHI.TtShaderBinder cbPerHZBCullData_CullCluster;

        public UCBufferHZBCullData HZBCullInstanceData = new UCBufferHZBCullData();
        public UCBufferHZBCullData HZBCullClusterData = new UCBufferHZBCullData();

        public NxRHI.TtCbView HZBCullInstanceCBuffer;
        public NxRHI.TtCbView HZBCullClusterCBuffer;

        //public UDrawBuffers BasePass = new UDrawBuffers();

        public int NumThreadsPerGroup = 64;

        public async Thread.Async.TtTask<bool> Initialize_Instance(TtRenderPolicy policy, string debugName)
        {
            GpuInstances.Initialize(EBufferType.BFT_SRV);
            CullInstancesBuffer.Initialize(EBufferType.BFT_SRV | EBufferType.BFT_UAV);
            CullClustersBuffer.Initialize(EBufferType.BFT_SRV | EBufferType.BFT_UAV);


            //After BuildInstances function..
            var defines = new NxRHI.TtShaderDefinitions();
            
            Cull_CullGpuIndexs = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<CullGpuIndexsShading>();

            Cull_SetupCullClusterArgs = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<SetupCullClusterArgsShading>();

            Cull_CullCluster = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<CullClusterShading>();

            Cull_SetupDrawClusterArgsBuffer = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<SetupDrawClusterArgsBufferShading>();

            //cbPerHZBCullData_CullCluster = Cull_CullCluster.FindBinder("cbPerPatchHZBCullData");
            //HZBCullClusterCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(HZBCullClusterData.Binder.mCoreObject);

            //drawcall.BindCBuffer(effectBinder.cbPerPatch.mCoreObject, pat.PatchCBuffer);

            return true;
        }
        public void BuildInstances(GamePlay.TtWorld world)
        {
            foreach (var i in GpuSceneActors)
            {
                i.GpuSceneIndex = -1;
            }
            GpuSceneActors.Clear();
            world.Root.DFS_VisitNodeTree(static (GamePlay.Scene.TtNode node, object arg) =>
            {
                var actor = node as GamePlay.Scene.TtGpuSceneNode;
                if (actor == null)
                    return false;
                var This = arg as TtGpuSceneNode;
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
                for (int j = 0; j < i.ClusteredMeshs.Count; j ++)
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

        private void Dispose_Instance()
        {
            GpuInstances?.Dispose();
            GpuInstances = null;

            CullInstancesBuffer?.Dispose();
            CullInstancesBuffer = null;

        }
        private unsafe void FrameBuild_Instance()
        {
            InstancePinOut.Attachement.Height = (uint)GpuInstances.DataArray.Count;
            InstancePinOut.Attachement.Width = (uint)sizeof(FActorInstance);
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(InstancePinOut);
            
            attachement.GpuResource = GpuInstances.GpuBuffer;
            attachement.Srv = GpuInstances.Srv;
            attachement.Uav = GpuInstances.Uav;

            // TODO CullInstancesBuffer
        }
        private void TickLogic_Instance(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, UCommandList cmd)
        {
            //GpuInstances.Clear(); Fixd


            foreach (var i in GpuSceneActors)
            {
                FActorInstance data;
                data.WorldMatrix = i.Placement.AbsTransform.ToMatrixWithScale(in world.mCameraOffset);
                GpuInstances.UpdateData(i.GpuSceneIndex, in data);

                TtCullInstanceData CullInstanceData;
                CullInstanceData.BoundCenter = i.BoundVolume.LocalAABB.GetCenter();
                CullInstanceData.BoundExtent = i.BoundVolume.LocalAABB.GetSize();
                CullInstanceData.ChildrenStart = 0; //TODO
                CullInstanceData.ChildrenEnd = 0; //TODO
                CullInstanceData.WorldMatrix = data.WorldMatrix;
                CullInstanceData.GpuSceneIndex = (uint)i.GpuSceneIndex;
                CullInstanceData.ClusterCount = i.ClusteredMeshs.Count;

                CullInstancesBuffer.UpdateData(i.GpuSceneIndex, in CullInstanceData);
            }


            GpuInstances.Flush2GPU(cmd.mCoreObject);

            CullInstancesBuffer.Flush2GPU(cmd.mCoreObject);
        }

        private unsafe void Cull(NxRHI.TtGpuDevice rcj, Graphics.Pipeline.TtRenderPolicy policy)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();

            {
                VisibilityGpuActorsBuffer.SetSize((uint)CullInstancesBuffer.DataArray.Count, null, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
                NeedCullClusterMesBuffer.SetSize((uint)CullClustersBuffer.DataArray.Count, null, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                uint NumberVisibilityGpuIndex = 0;
                NumberVisibilityGpuActorBuffer.SetSize((uint)1, &NumberVisibilityGpuIndex, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                uint NumberGpuActors = (uint)CullInstancesBuffer.DataArray.Count;
                NumberGpuActorsBuffer.SetSize(1u, &NumberGpuActors, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                if (Cull_CullGpuIndexsDrawcall == null)
                {
                    Cull_CullGpuIndexsDrawcall = rc.CreateComputeDraw();
                }

                Cull_CullGpuIndexs.SetDrawcallDispatch(this, policy, Cull_CullGpuIndexsDrawcall, (uint)GpuSceneActors.Count, 1, 1, true);
                
                //Cull_CullGpuIndexsDrawcall.Commit(cmd);
                cmd.PushGpuDraw(Cull_CullGpuIndexsDrawcall);
            }

            {
                uint InitCullClusterIndirectArgs = 0u;
                CullClusterIndirectArgs.SetSize(3u, &InitCullClusterIndirectArgs, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                if (Cull_SetupCullClusterArgsDrawcall == null)
                {
                    Cull_SetupCullClusterArgsDrawcall = rc.CreateComputeDraw();
                }

                Cull_SetupCullClusterArgs.SetDrawcallDispatch(this, policy, Cull_SetupCullClusterArgsDrawcall, 1, 1, 1, true);

                //Cull_SetupCullClusterArgsDrawcall.Commit(cmd);
                cmd.PushGpuDraw(Cull_SetupCullClusterArgsDrawcall);

            }

            {
                VisibleClusterMeshData.SetSize((uint)CullClustersBuffer.DataArray.Count, null, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
                TtNumnerVisibleClusterMeshData InitNumnerVisibleClusterMeshData;
                InitNumnerVisibleClusterMeshData.ClusterCount = 0;
                NumnerVisibleClusterMeshData.SetSize(1u, &InitNumnerVisibleClusterMeshData, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                //uint InitSetupDrawClusterIndirectArgs = 0;
                if (Cull_CullClusterDrawcall == null)
                {
                    Cull_CullClusterDrawcall = rc.CreateComputeDraw();
                }

                Cull_CullCluster.SetDrawcallIndirectDispatch(this, policy, Cull_CullClusterDrawcall, CullClusterIndirectArgs.GpuBuffer);

                //Cull_CullClusterDrawcall.Commit(cmd);
                cmd.PushGpuDraw(Cull_CullClusterDrawcall);
            }

            {
                uint InitSetupDrawClusterIndirectArgs = 0u;
                SetupDrawClusterIndirectArgs.SetSize(3u, &InitSetupDrawClusterIndirectArgs, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV | EBufferType.BFT_IndirectArgs);

                if (Cull_SetupDrawClusterArgsDrawcall == null)
                {
                    Cull_SetupDrawClusterArgsDrawcall = rc.CreateComputeDraw();
                }

                Cull_SetupDrawClusterArgsBuffer.SetDrawcallDispatch(this, policy, Cull_SetupDrawClusterArgsDrawcall, 1, 1, 1, true);
                
                //Cull_SetupDrawClusterArgsDrawcall.Commit(cmd);
                cmd.PushGpuDraw(Cull_SetupDrawClusterArgsDrawcall);
            }
            
            //awzklyq!            
            cmd.FlushDraws();
            cmd.EndCommand();

            policy.CommitCommandList(cmd);
        }
    }
}

using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class UGpuSceneNode
    {
        public Common.URenderGraphPin InstancePinOut = Common.URenderGraphPin.CreateOutput("Instances", false, EPixelFormat.PXF_UNKNOWN);
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

        public class UCBufferHZBCullData : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrevTranslatedWorldToClip;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrevPreViewTranslation;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldToClip;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4i))]
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

        public NxRHI.UComputeEffect Cull_CullGpuIndexs;
        public NxRHI.UComputeEffect Cull_SetupCullClusterArgs;

        public NxRHI.UComputeEffect Cull_CullCluster;
        public NxRHI.UComputeEffect Cull_SetupDrawClusterArgsBuffer;

        public NxRHI.UComputeDraw Cull_CullGpuIndexsDrawcall;
        public NxRHI.UComputeDraw Cull_SetupCullClusterArgsDrawcall;

        public NxRHI.UComputeDraw Cull_CullClusterDrawcall;
        public NxRHI.UComputeDraw Cull_SetupDrawClusterArgsDrawcall;

        public NxRHI.UShaderBinder cbPerHZBCullData_CullInstance;
        public NxRHI.UShaderBinder cbPerHZBCullData_CullCluster;

        public UCBufferHZBCullData HZBCullInstanceData = new UCBufferHZBCullData();
        public UCBufferHZBCullData HZBCullClusterData = new UCBufferHZBCullData();

        public NxRHI.UCbView HZBCullInstanceCBuffer;
        public NxRHI.UCbView HZBCullClusterCBuffer;

        //public UDrawBuffers BasePass = new UDrawBuffers();

        public int NumThreadsPerGroup = 64;

        public async Thread.Async.TtTask<bool> Initialize_Instance(URenderPolicy policy, string debugName)
        {
            GpuInstances.Initialize(EBufferType.BFT_SRV);
            CullInstancesBuffer.Initialize(EBufferType.BFT_SRV | EBufferType.BFT_UAV);
            CullClustersBuffer.Initialize(EBufferType.BFT_SRV | EBufferType.BFT_UAV);


            //After BuildInstances function..
            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{(MathHelper.Max(1, MathHelper.DivideAndRoundUp((uint)GpuSceneActors.Count, 64u)))}");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");

            Cull_CullGpuIndexs = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Occlusion/GpuSceneCullInstance.cginc", RName.ERNameType.Engine),
                "GpuSceneCullInstance", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            cbPerHZBCullData_CullInstance = Cull_CullGpuIndexs.FindBinder("cbPerPatchHZBCullData");

            defines.mCoreObject.AddDefine("DispatchX", $"1");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            //defines.mCoreObject.AddDefine("NumThreadsPerGroup", $"{NumThreadsPerGroup}"); 
            Cull_SetupCullClusterArgs = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Occlusion/GpuSceneSetupCullCluster.cginc", RName.ERNameType.Engine),
                "SetupCullClusterArgsCS", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"1");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            //defines.mCoreObject.AddDefine("NumThreadsPerGroup", $"{NumThreadsPerGroup}"); 
            Cull_CullCluster = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Occlusion/GpuSceneCullCluster.cginc", RName.ERNameType.Engine),
                "GpuSceneCullCluster", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"1");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            Cull_SetupDrawClusterArgsBuffer = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Occlusion/SetupDrawClusterArgs.cginc", RName.ERNameType.Engine),
                "SetupDrawClusterArgsCS", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            //cbPerHZBCullData_CullCluster = Cull_CullCluster.FindBinder("cbPerPatchHZBCullData");
            //HZBCullClusterCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(HZBCullClusterData.Binder.mCoreObject);

            //drawcall.BindCBuffer(effectBinder.cbPerPatch.mCoreObject, pat.PatchCBuffer);

            return true;
        }
        public void BuildInstances(GamePlay.UWorld world)
        {
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
            
            attachement.Buffer = GpuInstances.GpuBuffer;
            attachement.Srv = GpuInstances.DataSRV;
            attachement.Uav = GpuInstances.DataUAV;

            // TODO CullInstancesBuffer
        }
        private void TickLogic_Instance(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, UCommandList cmd)
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

        private unsafe void Cull(NxRHI.UGpuDevice rc)
        {
            var cmd = BasePass.DrawCmdList;
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
                    Cull_CullGpuIndexsDrawcall.SetComputeEffect(Cull_CullGpuIndexs);
                }

                HZBCullInstanceData.UpdateFieldVar(Cull_CullGpuIndexs.mComputeShader, "cbPerPatchHZBCullData");
                HZBCullInstanceCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(HZBCullInstanceData.Binder.mCoreObject);

                //HZBCullInstanceCBuffer.SetValue

                Cull_CullGpuIndexsDrawcall.BindUav("InstanceSceneData", CullInstancesBuffer.DataUAV);
                Cull_CullGpuIndexsDrawcall.BindUav("NumberVisibilityGpuActorBuffer", NumberVisibilityGpuActorBuffer.DataUAV);
                Cull_CullGpuIndexsDrawcall.BindUav("VisibilityGpuActorsBuffer", VisibilityGpuActorsBuffer.DataUAV);
                Cull_CullGpuIndexsDrawcall.BindSrv("NumberGpuActors", NumberGpuActorsBuffer.DataSRV);

                //Cull_CullGpuIndexsDrawcall.Commit(cmd);
                cmd.PushGpuDraw(Cull_CullGpuIndexsDrawcall);
            }

            {
                uint InitCullClusterIndirectArgs = 0u;
                CullClusterIndirectArgs.SetSize(3u, &InitCullClusterIndirectArgs, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                if (Cull_SetupCullClusterArgsDrawcall == null)
                {
                    Cull_SetupCullClusterArgsDrawcall = rc.CreateComputeDraw();
                    Cull_SetupCullClusterArgsDrawcall.SetComputeEffect(Cull_SetupCullClusterArgs);
                }

                Cull_SetupCullClusterArgsDrawcall.BindUav("CullClusterIndirectArgs", CullClusterIndirectArgs.DataUAV);
                Cull_SetupCullClusterArgsDrawcall.BindSrv("NumberVisibilityGpuActorBuffer", NumberVisibilityGpuActorBuffer.DataSRV);

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
                    Cull_CullClusterDrawcall.SetComputeEffect(Cull_CullCluster);
                    Cull_CullClusterDrawcall.BindIndirectDispatchArgsBuffer(CullClusterIndirectArgs.GpuBuffer);
                }

                Cull_CullClusterDrawcall.BindSrv("NumberVisibilityGpuActorBuffer", NumberVisibilityGpuActorBuffer.DataSRV);
                Cull_CullClusterDrawcall.BindUav("VisibleClusterMeshData", VisibleClusterMeshData.DataUAV);
                Cull_CullClusterDrawcall.BindUav("NumnerVisibleClusterMeshData", NumnerVisibleClusterMeshData.DataUAV);

                //Cull_CullClusterDrawcall.Commit(cmd);
                cmd.PushGpuDraw(Cull_CullClusterDrawcall);
            }

            {
                uint InitSetupDrawClusterIndirectArgs = 0u;
                SetupDrawClusterIndirectArgs.SetSize(3u, &InitSetupDrawClusterIndirectArgs, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV | EBufferType.BFT_IndirectArgs);

                if (Cull_SetupDrawClusterArgsDrawcall == null)
                {
                    Cull_SetupDrawClusterArgsDrawcall = rc.CreateComputeDraw();
                    Cull_SetupDrawClusterArgsDrawcall.SetComputeEffect(Cull_SetupDrawClusterArgsBuffer);
                }

                Cull_SetupDrawClusterArgsDrawcall.BindSrv("NumnerVisibleClusterMeshData", NumnerVisibleClusterMeshData.DataSRV);
                Cull_SetupDrawClusterArgsDrawcall.BindUav("DrawClusterIndirectArgs", SetupDrawClusterIndirectArgs.DataUAV);

                //Cull_SetupDrawClusterArgsDrawcall.Commit(cmd);
                cmd.PushGpuDraw(Cull_SetupDrawClusterArgsDrawcall);
            }
            //Cull_CullGpuIndexsDrawcall.

            //defines.mCoreObject.AddDefine("BufferHeadSize", $"{UGpuParticleResources.BufferHeadSize * 4}");

            //awzklyq!
            cmd.FlushDraws();
        }
    }
}

using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Common;
using System.Threading.Tasks;
using EngineNS.Graphics.Pipeline;
using EngineNS.GamePlay;
using NPOI.HSSF.Record.AutoFilter;
using System.Diagnostics;
using NPOI.SS.Formula.Functions;

namespace EngineNS.Bricks.GpuDriven
{
    //nanite: https://zhuanlan.zhihu.com/p/382687738

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
        public URenderGraphPin SrcClustersPin = URenderGraphPin.CreateInput("SrcClusters");

        public URenderGraphPin VerticesPinOut = URenderGraphPin.CreateInputOutput("Vertices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin IndicesPinOut = URenderGraphPin.CreateInputOutput("Indices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin ClustersPinOut = URenderGraphPin.CreateInputOutput("Clusters", false, EPixelFormat.PXF_UNKNOWN);
        
        public URenderGraphPin VisibleClutersPinOut = URenderGraphPin.CreateOutput("VisibleClusters", false, EPixelFormat.PXF_UNKNOWN);
        
        public TtCullClusterShading CullClusterShading;
        private NxRHI.UComputeDraw CullClusterShadingDrawcall;

        public TtCpu2GpuBuffer<float> Vertices = new TtCpu2GpuBuffer<float>();
        public TtCpu2GpuBuffer<uint> Indices = new TtCpu2GpuBuffer<uint>();
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FClusterData
        {
            public Vector3 BoundCenter;
            public int IndexStart;
            public Vector3 BoundExtent;
            public int IndexEnd;
            public Matrix WVPMatrix;
        }
        public TtCpu2GpuBuffer<FClusterData> Clusters = new TtCpu2GpuBuffer<FClusterData>();
        public TtCpu2GpuBuffer<uint> SrcClusters = new TtCpu2GpuBuffer<uint>();
        public TtCpu2GpuBuffer<uint> VisClusters = new TtCpu2GpuBuffer<uint>();

        public TtCullClusterNode()
        {
            Name = "CullClusterNode";
        }
        public override void InitNodePins()
        {
            AddInput(HzbPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(SrcClustersPin, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

            AddInputOutput(VerticesPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(IndicesPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(ClustersPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            
            AddOutput(VisibleClutersPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

            HzbPinIn.IsAllowInputNull = true;
            VerticesPinOut.IsAllowInputNull = true;
            IndicesPinOut.IsAllowInputNull = true;
            ClustersPinOut.IsAllowInputNull = true;
            SrcClustersPin.IsAllowInputNull = true;

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

            unsafe
            {
                VisClusters.Initialize(NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
                VisClusters.SetSize(100 + 1);
                var visClst = stackalloc int[100 + 1];
                visClst[0] = 0;
                VisClusters.UpdateData(0, visClst, sizeof(int) * 100 + sizeof(int));
                VisClusters.Flush2GPU();
            }
        }
        private unsafe void UpdateBuffers(Vector3[] vb, uint[] ib, List<FClusterData> clusters, EngineNS.Graphics.Pipeline.UCamera camera)
        {
            // TODO: update once?
            if (vb.Length >0)
            {
                Vertices.Initialize(NxRHI.EBufferType.BFT_SRV);
                Vertices.SetSize(sizeof(FQuarkVertex) * vb.Length / sizeof(float));
                FQuarkVertex[] quarkVB = new FQuarkVertex[vb.Length];
                for (int i = 0; i < vb.Length; i++)
                {
                    quarkVB[i].Position = vb[i];
                    // TODO:
                    quarkVB[i].Normal = Vector3.Zero;
                    quarkVB[i].UV = Vector2.Zero;
                }
                fixed (FQuarkVertex* p = &quarkVB[0])
                {
                    Vertices.UpdateData(0, p, sizeof(FQuarkVertex) * vb.Length);
                }
                Vertices.Flush2GPU();
            }
            
            if (ib.Length > 0)
            {
                Indices.Initialize(NxRHI.EBufferType.BFT_SRV);
                Indices.SetSize(ib.Length);
                fixed (uint* p = &ib[0])
                {
                    Indices.UpdateData(0, p, sizeof(uint) * ib.Length);
                }
                Indices.Flush2GPU();
            }
            
            if (clusters.Count > 0)
            {
                Clusters.Initialize(NxRHI.EBufferType.BFT_SRV);
                Clusters.SetSize(sizeof(FClusterData) * clusters.Count);
                var clst = stackalloc FClusterData[clusters.Count];

                //Matrix worldMatrix = Matrix.Identity;
                //worldMatrix.M41 = modelPos.X;
                //worldMatrix.M42 = modelPos.Y;
                //worldMatrix.M43 = modelPos.Z;

                for (int i = 0; i < clusters.Count; i++)
                {
                    clst[i].WVPMatrix = camera.GetToViewPortMatrix();
                    clst[i].WVPMatrix.Transpose();

                    clst[i].IndexStart = clusters[i].IndexStart;
                    clst[i].IndexEnd = clusters[i].IndexEnd;
                    // TODO:
                    clst[i].BoundCenter = Vector3.Zero;
                    clst[i].BoundExtent = Vector3.One;
                }

                Clusters.UpdateData(0, clst, sizeof(FClusterData) * clusters.Count);
                Clusters.Flush2GPU();
            }

            {
                SrcClusters.Initialize(NxRHI.EBufferType.BFT_SRV);
                SrcClusters.SetSize(sizeof(int) * 1 + sizeof(int));
                var src = stackalloc int[2];
                src[0] = clusters.Count > 0 ? 1 : 0;
                src[1] = 0;
                SrcClusters.UpdateData(0, src, sizeof(int) * 1 + sizeof(int));
                SrcClusters.Flush2GPU();
            }

            {
                var attachment = ImportAttachment(VerticesPinOut);
                attachment.Srv = Vertices.DataSRV;
            }
            {
                var attachment = ImportAttachment(IndicesPinOut);
                attachment.Srv = Indices.DataSRV;
            }
            {
                var attachment = ImportAttachment(ClustersPinOut);
                attachment.Srv = Clusters.DataSRV;
            }

            {
                var attachment = ImportAttachment(SrcClustersPin);
                attachment.Srv = SrcClusters.DataSRV;
            }
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            base.BeforeTickLogic(policy);

            {
                var attachment = ImportAttachment(VisibleClutersPinOut);
                attachment.Uav = VisClusters.DataUAV;
                attachment.Srv = VisClusters.DataSRV;
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

        public void BuildInstances(GamePlay.UWorld world, GamePlay.UWorld.UVisParameter rp)
        {
            List<FClusterData> clusters = new List<FClusterData>();
            List<Vector3> position = new List<Vector3>();
            List<uint> ib = new List<uint>();

            foreach (var i in rp.VisibleNodes)
            {
                var meshNode = i as GamePlay.Scene.UMeshNode;
                if (meshNode == null)
                    continue;

                var clusterMesh = meshNode.Mesh.MaterialMesh.Mesh.ClusteredMesh;
                if (clusterMesh == null)
                    continue;

                for (int clusterId = 0; clusterId < clusterMesh.Clusters.Count; clusterId++)
                {
                    var cluster = new FClusterData();
                    // NOTE: 
                    cluster.IndexStart = clusterMesh.Clusters[clusterId].IndexStart;
                    cluster.IndexEnd = clusterMesh.Clusters[clusterId].IndexCount;
                    clusters.Add(cluster);                    
                }
                if (clusterMesh.Vertices != null)
                    position.AddRange(new List<Vector3>(clusterMesh.Vertices));
                if (clusterMesh.Indices != null)
                    ib.AddRange(new List<uint>(clusterMesh.Indices));
            }
            // debug
//             var view2ScreenMat = rp.CullCamera.GetToViewPortMatrix();
//             for (int i = 0; i < position.Count; i++)
//             {
//                 var sreenPos = Vector3.TransformCoordinate(position[i], view2ScreenMat);
//                 Debug.WriteLine(sreenPos.ToString());
//             }

            // debug
            //position.Clear();
            //ib.Clear();
            //clusters.Clear();
            UpdateBuffers(position.ToArray(), ib.ToArray(), clusters, rp.CullCamera);
        }
    }
}


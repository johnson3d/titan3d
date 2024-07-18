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

            drawcall.BindSrv("ClusterBuffer", node.Clusters.Srv);
            drawcall.BindSrv("SrcClusterBuffer", node.SrcClusters.Srv);
            drawcall.BindUav("VisClusterBuffer", node.VisClusters.Uav);
            
            var index = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbCameraFrustum");
            if (index.IsValidPointer)
            {
                if (node.CBCameraFrustum == null)
                {
                    node.CBCameraFrustum = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, node.CBCameraFrustum);
            }

            index = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "HZBTexture");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.HzbPinIn);
                drawcall.BindSrv(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_Sampler, "Samp_HZBTexture");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

        }
    }
    public class TtCullClusterNode : TtRenderGraphNode
    {
        public TtRenderGraphPin HzbPinIn = TtRenderGraphPin.CreateInput("Hzb");
        public TtRenderGraphPin SrcClustersPin = TtRenderGraphPin.CreateInput("SrcClusters");

        public TtRenderGraphPin VerticesPinOut = TtRenderGraphPin.CreateInputOutput("Vertices", false, EPixelFormat.PXF_UNKNOWN);
        public TtRenderGraphPin IndicesPinOut = TtRenderGraphPin.CreateInputOutput("Indices", false, EPixelFormat.PXF_UNKNOWN);
        public TtRenderGraphPin ClustersPinOut = TtRenderGraphPin.CreateInputOutput("Clusters", false, EPixelFormat.PXF_UNKNOWN);
        
        public TtRenderGraphPin VisibleClutersPinOut = TtRenderGraphPin.CreateOutput("VisibleClusters", false, EPixelFormat.PXF_UNKNOWN);
        
        public TtCullClusterShading CullClusterShading;
        private NxRHI.UComputeDraw CullClusterShadingDrawcall;

        public TtCpu2GpuBuffer<float> Vertices = new TtCpu2GpuBuffer<float>();
        public TtCpu2GpuBuffer<uint> Indices = new TtCpu2GpuBuffer<uint>();
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FClusterData
        {
            public Vector3 BoundMin;
            public int IndexStart;
            public Vector3 BoundMax;
            public int IndexEnd;
            public Matrix WVPMatrix;
        }
        public TtCpu2GpuBuffer<FClusterData> Clusters = new TtCpu2GpuBuffer<FClusterData>();
        public TtCpu2GpuBuffer<uint> SrcClusters = new TtCpu2GpuBuffer<uint>();
        public TtCpu2GpuBuffer<uint> VisClusters = new TtCpu2GpuBuffer<uint>();

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public unsafe struct FFrustumCullingParams
        {
            public FFrustumCullingParams()
            {
                FrustumMinPoint = Vector3.Zero;
                FrustumMaxPoint = Vector3.One;
            }
            public fixed float CameraPlanes[24];
            public Vector3 FrustumMinPoint;
            public Vector3 FrustumMaxPoint;
        };
        public FFrustumCullingParams mFrustumCullingData = new FFrustumCullingParams();
        public NxRHI.UCbView CBCameraFrustum;

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

            Vertices.Initialize(NxRHI.EBufferType.BFT_SRV);
            Indices.Initialize(NxRHI.EBufferType.BFT_SRV);
            Clusters.Initialize(NxRHI.EBufferType.BFT_SRV);
            SrcClusters.Initialize(NxRHI.EBufferType.BFT_SRV);
            unsafe
            {
                VisClusters.Initialize(NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
                VisClusters.SetSize(100 + 1);
                var visClst = stackalloc int[100 + 1];
                visClst[0] = 0;
                VisClusters.UpdateData(0, visClst, sizeof(int) * 100 + sizeof(int));
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Default, "VisClusters.Flush"))
                {
                    VisClusters.Flush2GPU(tsCmd.CmdList);
                }
            }
        }
        private unsafe void UpdateBuffers(NxRHI.ICommandList cmd, Vector3[] vb, uint[] ib, List<FClusterData> clusters, EngineNS.Graphics.Pipeline.UCamera camera)
        {
            // TODO: update once?
            if (vb.Length >0)
            {
                //Vertices.Initialize(NxRHI.EBufferType.BFT_SRV);
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
                Vertices.Flush2GPU(cmd);
            }
            
            if (ib.Length > 0)
            {
                //Indices.Initialize(NxRHI.EBufferType.BFT_SRV);
                Indices.SetSize(ib.Length);
                fixed (uint* p = &ib[0])
                {
                    Indices.UpdateData(0, p, sizeof(uint) * ib.Length);
                }
                Indices.Flush2GPU(cmd);
            }
            
            if (clusters.Count > 0)
            {
                //Clusters.Initialize(NxRHI.EBufferType.BFT_SRV);
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
                    clst[i].BoundMin = clusters[i].BoundMin;
                    clst[i].BoundMax = clusters[i].BoundMax;
                }

                Clusters.UpdateData(0, clst, sizeof(FClusterData) * clusters.Count);
                Clusters.Flush2GPU(cmd);
            }

            {
                //SrcClusters.Initialize(NxRHI.EBufferType.BFT_SRV);
                SrcClusters.SetSize(sizeof(int) * 1 + sizeof(int));
                var src = stackalloc int[2];
                src[0] = clusters.Count > 0 ? 1 : 0;
                src[1] = 0;
                SrcClusters.UpdateData(0, src, sizeof(int) * 1 + sizeof(int));
                SrcClusters.Flush2GPU(cmd);
            }

            {
                var attachment = ImportAttachment(VerticesPinOut);
                attachment.Srv = Vertices.Srv;
            }
            {
                var attachment = ImportAttachment(IndicesPinOut);
                attachment.Srv = Indices.Srv;
            }
            {
                var attachment = ImportAttachment(ClustersPinOut);
                attachment.Srv = Clusters.Srv;
            }

            {
                var attachment = ImportAttachment(SrcClustersPin);
                attachment.Srv = SrcClusters.Srv;
            }
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            base.BeforeTickLogic(policy);

            {
                var attachment = ImportAttachment(VisibleClutersPinOut);
                attachment.Uav = VisClusters.Uav;
                attachment.Srv = VisClusters.Srv;
            }

        }
        public unsafe override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            if (CBCameraFrustum != null)
            {
                CBCameraFrustum.SetValue("FrustumInfo", in mFrustumCullingData);
            }

            var cmd = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmd))
            {
                PrepareCullClusterInfos(cmd.mCoreObject, world, policy.DefaultCamera.VisParameter);

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
            }
            
            policy.CommitCommandList(cmd);
        }

        public Vector4 TransformViewProj(in Vector3 coord, in Matrix transform)
        {
            Vector4 vector;

            vector.X = (((coord.X * transform.M11) + (coord.Y * transform.M21)) + (coord.Z * transform.M31)) + transform.M41;
            vector.Y = (((coord.X * transform.M12) + (coord.Y * transform.M22)) + (coord.Z * transform.M32)) + transform.M42;
            vector.Z = (((coord.X * transform.M13) + (coord.Y * transform.M23)) + (coord.Z * transform.M33)) + transform.M43;
            vector.W = ((((coord.X * transform.M14) + (coord.Y * transform.M24)) + (coord.Z * transform.M34)) + transform.M44);

            return vector;
        }
        public unsafe void UpdateCameraInfo(EngineNS.Graphics.Pipeline.UCamera camera)
        {
            if (camera == null)
                return;

            var frustum = camera.GetFrustum();
            for (int i = 0; i < 6; i++)
            {
                ref var plane = ref *frustum.GetPlane((ENUM_FRUSTUM_PLANE)i);
                unsafe
                {
                    mFrustumCullingData.CameraPlanes[i * 4 + 0] = plane.A;
                    mFrustumCullingData.CameraPlanes[i * 4 + 1] = plane.B;
                    mFrustumCullingData.CameraPlanes[i * 4 + 2] = plane.C;
                    mFrustumCullingData.CameraPlanes[i * 4 + 3] = plane.D;
                }
            }
            BoundingBox frustumAABB = new BoundingBox();
            frustum.GetAABB(ref frustumAABB);

            mFrustumCullingData.FrustumMinPoint = frustumAABB.Minimum;
            mFrustumCullingData.FrustumMaxPoint = frustumAABB.Maximum;
        }
        public void PrepareCullClusterInfos(NxRHI.ICommandList cmd, GamePlay.UWorld world, GamePlay.UWorld.UVisParameter rp)
        {
            List<FClusterData> clusters = new List<FClusterData>();
            List<Vector3> position = new List<Vector3>();
            List<uint> ib = new List<uint>();

            foreach (var i in rp.VisibleNodes)
            {
                var meshNode = i as GamePlay.Scene.UMeshNode;
                if (meshNode == null || meshNode.Mesh == null)
                    continue;

                foreach (var j in meshNode.Mesh.MaterialMesh.SubMeshes)
                {
                    if (j.Mesh == null)
                        continue;
                    var clusterMesh = j.Mesh.ClusteredMesh;
                    if (clusterMesh == null)
                        continue;

                    for (int clusterId = 0; clusterId < clusterMesh.Clusters.Count; clusterId++)
                    {
                        var clusterData = new FClusterData();
                        // NOTE: 
                        var cluster = clusterMesh.Clusters[clusterId];
                        clusterData.IndexStart = cluster.IndexStart;
                        clusterData.IndexEnd = cluster.IndexCount;
                        clusterData.BoundMin = cluster.AABB.Minimum;
                        clusterData.BoundMax = cluster.AABB.Maximum;
                        clusters.Add(clusterData);
                    }
                    if (clusterMesh.Vertices != null)
                        position.AddRange(new List<Vector3>(clusterMesh.Vertices));
                    if (clusterMesh.Indices != null)
                        ib.AddRange(new List<uint>(clusterMesh.Indices));
                }
            }
            // debug
            //var view2ScreenMat = rp.CullCamera.GetToViewPortMatrix();
            //for (int i = 0; i < position.Count; i++)
            //{
            //    var sreenPos = Vector3.TransformCoordinate(position[i], view2ScreenMat);
            //    Debug.WriteLine(sreenPos.ToString());
            //}

            //var viewProjMat = rp.CullCamera.GetViewProjection();
            //for (int i = 0; i < position.Count; i++)
            //{
            //    var sreenPos = TransformViewProj(position[i], viewProjMat);
            //    Debug.WriteLine(sreenPos.ToString());
            //}
            //Debug.WriteLine("===============================");

            //var view2ScreenMat = rp.CullCamera.GetViewProjection();
            //for (int i = 0; i < position.Count; i++)
            //{
            //    var sreenPos = Vector3.TransformCoordinate(position[i], view2ScreenMat);
            //    Debug.WriteLine(sreenPos.ToString());
            //}
            //
            // debug
            //position.Clear();
            //ib.Clear();
            //clusters.Clear();

            UpdateBuffers(cmd, position.ToArray(), ib.ToArray(), clusters, rp.CullCamera);
            UpdateCameraInfo(rp.CullCamera);
        }
    }
}


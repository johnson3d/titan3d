using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtStaticMeshBatch
    {
        public Mesh.UMeshPrimitives Mesh;
        public List<UMaterial> Materials;
        public Mesh.UMdfInstanceStaticMesh MdfQueue;
        public void Initialize(Mesh.UMaterialMesh.TtSubMaterialedMesh mesh)
        {
            MdfQueue = new Mesh.UMdfInstanceStaticMesh();
            Mesh = mesh.Mesh;
        }
        public static unsafe Hash64 MeshBatchHash(Mesh.UMaterialMesh.TtSubMaterialedMesh mesh)
        {
            var data = stackalloc int[1 + mesh.Materials.Count];
            data[0] = mesh.Mesh.GetHashCode();
            for (int i = 0; i < mesh.Materials.Count; i++)
            {
                data[1 + i] = mesh.Materials[i].GetHashCode();
            }
            return Hash64.FromData((byte*)data, sizeof(int) * (1 + mesh.Materials.Count));
        }
    }
    public class TtCullingNode : TtRenderGraphNode
    {
        public TtRenderGraphPin HzbPinInOut = TtRenderGraphPin.CreateInputOutput("Hzb", false, EPixelFormat.PXF_UNKNOWN);
        public TtCullingNode()
        {
            Name = "GpuCulling";
        }
        public override void InitNodePins()
        {
            AddInputOutput(HzbPinInOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        }
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmd))
            {
                Culling(policy, cmd);

                cmd.FlushDraws();
            }
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd, "GpuCulling");
        }
        private void Culling(URenderPolicy policy, NxRHI.UCommandList cmd)
        {
            StaticMeshBatches.Clear();
            foreach (var i in policy.VisibleMeshes)
            {
                if (EnableGpuCulling)
                {
                    var mdfQueue = i.MdfQueue as Mesh.UMdfInstanceStaticMesh;
                    if (mdfQueue != null)
                    {
                        var modifier = mdfQueue.InstanceModifier;
                        if (modifier.IsGpuCulling)
                            modifier.InstanceBuffers.InstanceCulling(modifier, cmd.mCoreObject, policy, modifier.DrawArgsBuffer.Uav, 0);
                        continue;
                    }
                }
                if (EnableStaticMeshBatch)
                {
                    var mdfQueue = i.MdfQueue as Mesh.UMdfStaticMesh;
                    if (mdfQueue != null)
                    {
                        PushStaticMeshBatch(i);
                        continue;
                    }
                }
                if (EnableTerrainMeshBatch)
                {
                    var mdfQueue = i.MdfQueue as Bricks.Terrain.CDLOD.UTerrainMdfQueue;
                    if (mdfQueue != null)
                    {
                        PushStaticMeshBatch(i);
                        continue;
                    }
                }
            }
        }
        [Rtti.Meta]
        public bool EnableGpuCulling { get; set; } = true;
        [Rtti.Meta]
        public bool EnableStaticMeshBatch { get; set; } = false;
        [Rtti.Meta]
        public bool EnableTerrainMeshBatch { get; set; } = false;
        public Dictionary<Hash64, TtStaticMeshBatch> StaticMeshBatches = new Dictionary<Hash64, TtStaticMeshBatch>();
        private void PushStaticMeshBatch(Mesh.TtMesh mesh)
        {
            foreach (var i in mesh.MaterialMesh.SubMeshes)
            {
                var hash = TtStaticMeshBatch.MeshBatchHash(i);
                TtStaticMeshBatch batch;
                if (StaticMeshBatches.TryGetValue(hash, out batch) == false)
                {
                    batch = new TtStaticMeshBatch();
                    batch.Initialize(i);
                    StaticMeshBatches.Add(hash, batch);
                }
                var cb = mesh.PerMeshCBuffer;
                var matrix = cb.GetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);
                var instance = new Graphics.Mesh.Modifier.FVSInstanceData();
                instance.SetMatrix(matrix);
                instance.HitProxyId = cb.GetValue<uint>(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.HitProxyId);
                batch.MdfQueue.InstanceModifier.PushInstance(in instance);
            }
        }
    }
}

namespace EngineNS.Graphics.Mesh
{
    public partial class UMeshPrimitives
    {
    }
}
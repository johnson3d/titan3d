using EngineNS.Graphics.Pipeline.Shader;
using Microsoft.Toolkit.HighPerformance;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtMeshBatchBase
    {
        public Mesh.UMeshPrimitives Mesh;
        public List<UMaterial> Materials;
        public Graphics.Mesh.UMaterialMesh MaterialMesh;
        public Graphics.Mesh.TtMesh RenderMesh;
        public virtual Graphics.Mesh.Modifier.TtInstanceModifier GetInstanceModifier()
        {
            return null;
        }
        public T InitRenderMesh<T>(Mesh.UMaterialMesh.TtSubMaterialedMesh mesh) where T : Pipeline.Shader.TtMdfQueueBase
        {
            Mesh = mesh.Mesh;
            Materials = mesh.Materials;
            MaterialMesh = new Mesh.UMaterialMesh();
            MaterialMesh.Initialize(new List<Mesh.UMeshPrimitives>() { Mesh }, new List<List<Pipeline.Shader.UMaterial>> { Materials });

            RenderMesh = new Graphics.Mesh.TtMesh();
            RenderMesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<T>.TypeDesc );
            return RenderMesh.MdfQueue as T;
        }
    }
    public class TtStaticMeshBatch : TtMeshBatchBase
    {
        public Mesh.UMdfInstanceStaticMesh MdfQueue;
        public void Initialize(Mesh.UMaterialMesh.TtSubMaterialedMesh mesh)
        {
            MdfQueue = InitRenderMesh<Mesh.UMdfInstanceStaticMesh>(mesh);
        }
        public override Graphics.Mesh.Modifier.TtInstanceModifier GetInstanceModifier()
        {
            return MdfQueue.InstanceModifier;
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
    public class TtTerrainMeshBatch : TtMeshBatchBase, IPooledObject
    {
        public bool IsAlloc { get; set; }
        public Bricks.Terrain.CDLOD.UTerrainInstanceMdfQueue MdfQueue;
        public void Initialize(Mesh.UMaterialMesh.TtSubMaterialedMesh mesh)
        {
            MdfQueue = InitRenderMesh<Bricks.Terrain.CDLOD.UTerrainInstanceMdfQueue>(mesh);
            MdfQueue.InstanceModifier.SetCapacity(512, true);
        }
        public override Graphics.Mesh.Modifier.TtInstanceModifier GetInstanceModifier()
        {
            return MdfQueue.InstanceModifier;
        }

        public static unsafe Hash64 MeshBatchHash(Mesh.UMaterialMesh.TtSubMaterialedMesh mesh, Bricks.Terrain.CDLOD.UTerrainMdfQueue mdfQueue)
        {
            var data = stackalloc int[1 + mesh.Materials.Count + 1];
            data[0] = mesh.Mesh.GetHashCode();
            data[1] = mdfQueue.Patch.TerrainNode.GetHashCode();
            for (int i = 0; i < mesh.Materials.Count; i++)
            {
                data[2 + i] = mesh.Materials[i].GetHashCode();
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
        public Shader.UGraphicsShadingEnv mOpaqueShading;
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Graphics.Pipeline.Deferred.UDeferredOpaque>();
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
        public override Shader.UGraphicsShadingEnv GetPassShading(Graphics.Pipeline.URenderPolicy.EShadingType type, Mesh.TtMesh.TtAtom atom)
        {
            return mOpaqueShading;
        }
        public void Commit(URenderPolicy policy, NxRHI.UCommandList cmd, UGraphicsBuffers GBuffers)
        {
            foreach (var i in TerrainMeshBatches.Values)
            {
                foreach (var j in i.RenderMesh.SubMeshes)
                {
                    foreach (var k in j.Atoms)
                    {
                        var layer = k.Material.RenderLayer;
                        if (layer == ERenderLayer.RL_Opaque)
                        {
                            var drawcall = k.GetDrawCall(cmd.mCoreObject, GBuffers, policy, URenderPolicy.EShadingType.BasePass, this);
                            if (drawcall != null)
                            {
                                drawcall.BindGBuffer(policy.DefaultCamera, GBuffers);
                                cmd.PushGpuDraw(drawcall);
                            }
                        }   
                    }
                }
            }
        }
        private  unsafe void Culling(URenderPolicy policy, NxRHI.UCommandList cmd)
        {
            bool UseRVT = UEngine.Instance.Config.Feature_UseRVT;
            ResetStaticMeshBatch();
            ResetTerrainMeshBatch();
            using (new NxRHI.TtGpuEventScope(cmd, VNameString.FromString("InstanceCulling")))
            {
                for (int i = 0; i < policy.VisibleMeshes.Count; i++)
                {
                    if (EnableGpuCulling)
                    {
                        var mdfQueue = policy.VisibleMeshes[i].Mesh.MdfQueue as Mesh.UMdfInstanceStaticMesh;
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
                        var mdfQueue = policy.VisibleMeshes[i].Mesh.MdfQueue as Mesh.UMdfStaticMesh;
                        if (mdfQueue != null)
                        {
                            policy.VisibleMeshes[i] = PushStaticMeshBatch(policy.VisibleMeshes[i].Mesh);
                            continue;
                        }
                    }
                    if (UseRVT)
                    {
                        var mdfQueue = policy.VisibleMeshes[i].Mesh.MdfQueue as Bricks.Terrain.CDLOD.UTerrainMdfQueue;
                        if (mdfQueue != null)
                        {
                            policy.VisibleMeshes[i] = PushTerrainMeshBatch(policy.VisibleMeshes[i].Mesh, mdfQueue);
                            continue;
                        }
                    }
                }
            }
            if (EnableGpuCulling)
            {
                using(new NxRHI.TtGpuEventScope(cmd, VNameString.FromString("StaticMeshBatchCulling")))
                {
                    foreach (var i in StaticMeshBatches)
                    {
                        var modifier = i.Value.GetInstanceModifier();
                        modifier.InstanceBuffers.InstanceCulling(modifier, cmd.mCoreObject, policy, modifier.DrawArgsBuffer.Uav, 0);
                    }
                }
                using (new NxRHI.TtGpuEventScope(cmd, VNameString.FromString("TerrainMeshBatchCulling")))
                {
                    foreach (var i in TerrainMeshBatches)
                    {
                        var modifier = i.Value.GetInstanceModifier();
                        modifier.InstanceBuffers.InstanceCulling(modifier, cmd.mCoreObject, policy, modifier.DrawArgsBuffer.Uav, 0);
                    }
                }
            }
        }
        [Rtti.Meta]
        public bool EnableGpuCulling { get; set; } = true;
        [Rtti.Meta]
        public bool EnableStaticMeshBatch { get; set; } = false;
        public Dictionary<Hash64, TtStaticMeshBatch> StaticMeshBatches = new Dictionary<Hash64, TtStaticMeshBatch>();
        private void ResetStaticMeshBatch()
        {
            foreach(var i in StaticMeshBatches)
            {
                i.Value.GetInstanceModifier().InstanceBuffers.ResetInstance();
            }
        }
        private FVisibleMesh PushStaticMeshBatch(Mesh.TtMesh mesh)
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
                batch.GetInstanceModifier().PushInstance(in instance, new Graphics.Mesh.Modifier.FCullBounding());
            }
            return new FVisibleMesh(){Mesh = mesh, DrawMode = FVisibleMesh.EDrawMode.Instance };
        }
        public Dictionary<Hash64, TtTerrainMeshBatch> TerrainMeshBatches = new Dictionary<Hash64, TtTerrainMeshBatch>();
        private void ResetTerrainMeshBatch()
        {
            foreach (var i in TerrainMeshBatches)
            {
                i.Value.GetInstanceModifier().InstanceBuffers.ResetInstance();
            }
        }
        private FVisibleMesh PushTerrainMeshBatch(Mesh.TtMesh mesh, Bricks.Terrain.CDLOD.UTerrainMdfQueue mdfQueue)
        {
            foreach (var i in mesh.MaterialMesh.SubMeshes)
            {
                var hash = TtTerrainMeshBatch.MeshBatchHash(i, mdfQueue);
                TtTerrainMeshBatch batch;
                if (TerrainMeshBatches.TryGetValue(hash, out batch) == false)
                {
                    batch = new TtTerrainMeshBatch();
                    batch.Initialize(i);
                    batch.MdfQueue.TerrainModifier.Patch = mdfQueue.Patch;
                    batch.MdfQueue.TerrainModifier.TerrainNode = mdfQueue.Patch.TerrainNode;
                    batch.RenderMesh.SetWorldTransform(in mdfQueue.Patch.TerrainNode.Placement.TransformRef, mdfQueue.Patch.TerrainNode.GetWorld(), true);
                    TerrainMeshBatches.Add(hash, batch);
                }

                var world = mdfQueue.TerrainModifier.TerrainNode.GetWorld();
                mdfQueue.TerrainModifier.ActiveRVTs();
                var instance = new Graphics.Mesh.Modifier.FVSInstanceData();
                Bricks.Terrain.CDLOD.TtTerrainModifier.SetInstanceData(mesh, mdfQueue, ref instance);
                DBoundingBox aabb;
                aabb.Minimum = mdfQueue.Patch.AABB.Minimum - world.CameraOffset;
                aabb.Maximum = mdfQueue.Patch.AABB.Maximum - world.CameraOffset;
                var cullBounding = new Graphics.Mesh.Modifier.FCullBounding(aabb);
                batch.GetInstanceModifier().PushInstance(in instance, in cullBounding);
            }
            return new FVisibleMesh() { Mesh = mesh, DrawMode = FVisibleMesh.EDrawMode.Instance };
        }
    }
}

namespace EngineNS.Graphics.Mesh
{
    public partial class UMeshPrimitives
    {
    }
}
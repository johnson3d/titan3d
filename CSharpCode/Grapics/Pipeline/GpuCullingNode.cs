using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtMeshBatchBase
    {
        public Mesh.TtMeshPrimitives Mesh;
        public List<TtMaterial> Materials;
        public Graphics.Mesh.TtMaterialMesh MaterialMesh;
        public Graphics.Mesh.TtRenderMesh RenderMesh;
        public Graphics.Mesh.Modifier.TtGpuDrivenData GpuDrivenData = new Graphics.Mesh.Modifier.TtGpuDrivenData();
        public virtual Graphics.Mesh.Modifier.TtInstanceModifier GetInstanceModifier()
        {
            return null;
        }
        public T InitRenderMesh<T>(Mesh.TtMaterialMesh.TtSubMaterialedMesh mesh, TtMaterial.ERenderFlags meshRenderFlags) where T : Pipeline.Shader.TtMdfQueueBase
        {
            Mesh = mesh.Mesh;
            Materials = mesh.Materials;
            MaterialMesh = new Mesh.TtMaterialMesh();
            MaterialMesh.Initialize(new List<Mesh.TtMeshPrimitives>() { Mesh }, new List<List<Pipeline.Shader.TtMaterial>> { Materials });

            // flags 属于 batch，在初始化期间、首次创建 cbPerMesh 之前确定。
            RenderMesh = new Graphics.Mesh.TtRenderMesh() { MeshRenderFlags = meshRenderFlags };
            RenderMesh.Initialize(MaterialMesh, Rtti.TtTypeDescGetter<T>.TypeDesc );
            return RenderMesh.MdfQueue as T;
        }
    }
    public class TtInstanceStaticMeshBatch
    {
        public bool IsDraw = false;
        public Graphics.Mesh.TtRenderMesh RenderMesh;
        public Mesh.TtMdfInstanceStaticMesh MdfQueue;
        public Graphics.Mesh.Modifier.TtGpuDrivenData GpuDrivenData = new Graphics.Mesh.Modifier.TtGpuDrivenData();
        public void Initialize(Graphics.Pipeline.TtGpuCullingNode node, Mesh.TtMaterialMesh mesh, Mesh.TtMdfInstanceStaticMesh mdfQueue, TtMaterial.ERenderFlags meshRenderFlags)
        {
            // 在共享源 mesh 的 CBV 之前缓存 flags，构建后不再逐实例写入。
            RenderMesh = new Graphics.Mesh.TtRenderMesh() { MeshRenderFlags = meshRenderFlags };
            RenderMesh.Initialize(mesh, Rtti.TtTypeDescGetter<Mesh.TtMdfInstanceStaticMesh>.TypeDesc);

            MdfQueue = RenderMesh.MdfQueue as Mesh.TtMdfInstanceStaticMesh;
            GpuDrivenData.SetupGpuData(node, mdfQueue.InstanceModifier);
            MdfQueue.InstanceModifier.GpuDrivenData = GpuDrivenData;
            IsDraw = true;
        }
        public static unsafe Hash64 MeshBatchHash(Mesh.TtRenderMesh mesh)
        {
            var data = stackalloc int[1];
            data[0] = mesh.GetHashCode();
            return Hash64.FromData((byte*)data, sizeof(int));
        }
    }
    public class TtStaticMeshBatch : TtMeshBatchBase
    {
        public Mesh.TtMdfInstanceStaticMesh MdfQueue;
        public void Initialize(Graphics.Pipeline.TtGpuCullingNode node, Mesh.TtMaterialMesh.TtSubMaterialedMesh mesh, TtMaterial.ERenderFlags meshRenderFlags)
        {
            MdfQueue = InitRenderMesh<Mesh.TtMdfInstanceStaticMesh>(mesh, meshRenderFlags);
            MdfQueue.InstanceModifier.SetCapacity(512, true);
            GpuDrivenData.SetupGpuData(node, MdfQueue.InstanceModifier);
            MdfQueue.InstanceModifier.GpuDrivenData = GpuDrivenData;
        }
        public override Graphics.Mesh.Modifier.TtInstanceModifier GetInstanceModifier()
        {
            return MdfQueue.InstanceModifier;
        }
        public static unsafe Hash64 MeshBatchHash(Mesh.TtMaterialMesh.TtSubMaterialedMesh mesh)
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
    public class TtTerrainMeshBatch : TtMeshBatchBase
    {
        public bool IsAlloc { get; set; }
        public Bricks.Terrain.CDLOD.TtTerrainInstanceMdfQueue MdfQueue;
        // 本 batch 合并的是哪一级 LOD 的地形网格 (与 patch.TerrainMesh[LodLevel] 对应,
        // 同一 batch 内所有实例几何同 LOD, per-patch 的运行时 LOD 走 instance 数据)
        public int LodLevel;
        public void Initialize(Graphics.Pipeline.TtGpuCullingNode node, Mesh.TtMaterialMesh.TtSubMaterialedMesh mesh, int lodLevel, Bricks.Terrain.CDLOD.TtTerrainNode terrainNode)
        {
            LodLevel = lodLevel;
            MdfQueue = InitRenderMesh<Bricks.Terrain.CDLOD.TtTerrainInstanceMdfQueue>(mesh, terrainNode.MeshRenderFlags);
            MdfQueue.TerrainModifier.TerrainNode = terrainNode;
            MdfQueue.InstanceModifier.SetCapacity(512, true);
            GpuDrivenData.SetupGpuData(node, MdfQueue.InstanceModifier);
            MdfQueue.InstanceModifier.GpuDrivenData = GpuDrivenData;
        }
        public void UpdateRenderFlags()
        {
            // 节点是唯一来源；每个 batch 在剔除阶段检查一次，属性 setter 只上传变化值。
            RenderMesh.MeshRenderFlags = MdfQueue.TerrainModifier.TerrainNode.MeshRenderFlags;
        }
        public override Graphics.Mesh.Modifier.TtInstanceModifier GetInstanceModifier()
        {
            return MdfQueue.InstanceModifier;
        }

        public static unsafe Hash64 MeshBatchHash(Mesh.TtMaterialMesh.TtSubMaterialedMesh mesh, Bricks.Terrain.CDLOD.TtTerrainMdfQueue mdfQueue)
        {
            var size = 2 + mesh.Materials.Count;
            var data = stackalloc int[size];
            int index = 0;
            data[index++] = mesh.Mesh.GetHashCode();
            data[index++] = mdfQueue.Patch.TerrainNode.GetHashCode();
            for (int i = 0; i < mesh.Materials.Count; i++)
            {
                data[index++] = mesh.Materials[i].GetHashCode();//mesh.Materials[i].AssetName.GetHashCode();// 
            }
            return Hash64.FromData((byte*)data, sizeof(int) * size);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("GpuCulling", "Culling\\GpuCulling", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtGpuCullingNode : TAuxRenderGraphNode<TtGpuCullingNode>
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles", NxRHI.EBufferType.BFT_NONE);
        public TtRenderGraphPin HzbPinIn = TtRenderGraphPin.CreateInput("Hzb", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        public TtRenderGraphPin GpuCullOut = TtRenderGraphPin.CreateOutput("GpuCull", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_NONE);

        public Mesh.Modifier.TtGpuCullSetupShading GpuCullSetupShading;
        public Mesh.Modifier.TtGpuCullFlushShading GpuCullFlushShading;
        public Mesh.Modifier.TtGpuCullShading GpuCullShading;
        
        public TtGpuCullingNode()
        {
            Name = "GpuCulling";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn);
            AddInput(HzbPinIn);

            HzbPinIn.IsAllowInputNull = true;
            GpuCullOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(GpuCullOut);
        }
        public Shader.TtGraphicsShadingEnv mOpaqueShading;
        public TtCpuCullingNode CpuCullNode = null;
        public async override Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            mOpaqueShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Graphics.Pipeline.Deferred.TtDeferredOpaque>();
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            
            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
            }
            System.Diagnostics.Debug.Assert(CpuCullNode != null);

            GpuCullSetupShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Mesh.Modifier.TtGpuCullSetupShading>();
            GpuCullFlushShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Mesh.Modifier.TtGpuCullFlushShading>();
            GpuCullShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Mesh.Modifier.TtGpuCullShading>();
        }
        public override unsafe void Tick(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var cmd = NxRHI.TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "GpuCulling"))
            {
                Culling(policy, cmd);

                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "GpuCulling");
        }
        public override Shader.TtGraphicsShadingEnv GetPassShading(Mesh.TtRenderMesh.TtAtom atom)
        {
            return mOpaqueShading;
        }
        
        private  unsafe void Culling(TtRenderPolicy policy, NxRHI.TtCommandList cmd)
        {
            bool UseRVT = TtEngine.Instance.Config.Feature_UseRVT;
            ResetInstanceMesh();
            ResetStaticMeshBatch();
            ResetTerrainMeshBatch();
            using (new NxRHI.TtGpuEventScope(cmd, TtNameTable.InstanceCulling))
            {
                var visibleMeshes = CpuCullNode.VisParameter.VisibleMeshes;
                //todo:ParrallelFor
                for (int i = 0; i < visibleMeshes.Count; i++)
                {
                    if (EnableInstanceMeshCulling)
                    {
                        var mdfQueue = visibleMeshes[i].Mesh.MdfQueue as Mesh.TtMdfInstanceStaticMesh;
                        if (mdfQueue != null)
                        {
                            visibleMeshes[i] = PushInstanceMesh(visibleMeshes[i].Mesh, mdfQueue);
                            continue;
                        }
                    }
                    if (EnableStaticMeshBatch)
                    {
                        var mdfQueue = visibleMeshes[i].Mesh.MdfQueue as Mesh.TtMdfStaticMesh;
                        if (mdfQueue != null)
                        {
                            visibleMeshes[i] = PushStaticMeshBatch(visibleMeshes[i].Mesh);
                            continue;
                        }
                    }
                    if (EnableTerrainBatch && UseRVT)
                    {
                        var mdfQueue = visibleMeshes[i].Mesh.MdfQueue as Bricks.Terrain.CDLOD.TtTerrainMdfQueue;
                        if (mdfQueue != null)
                        {
                            visibleMeshes[i] = PushTerrainMeshBatch(visibleMeshes[i].Mesh, mdfQueue);
                            continue;
                        }
                    }
                }
            }
            if (EnableInstanceMeshCulling)
            {
                using (new NxRHI.TtGpuEventScope(cmd, TtNameTable.InstanceMeshCulling))
                {
                    foreach (var i in InstanceMeshes)
                    {
                        if (i.Value.IsDraw == false)
                            continue;
                        var modifier = i.Value.MdfQueue.InstanceModifier;
                        i.Value.GpuDrivenData.InstanceCulling(modifier, cmd.mCoreObject, policy);
                    }
                }
            }
            if (EnableStaticMeshBatch)
            {
                using (new NxRHI.TtGpuEventScope(cmd, TtNameTable.StaticMeshBatchCulling))
                {
                    foreach (var i in StaticMeshBatches)
                    {
                        var modifier = i.Value.GetInstanceModifier();
                        i.Value.GpuDrivenData.InstanceCulling(modifier, cmd.mCoreObject, policy);
                    }
                }
            }
            if (EnableTerrainBatch && UseRVT)
            {
                using (new NxRHI.TtGpuEventScope(cmd, TtNameTable.TerrainMeshBatchCulling))
                {
                    foreach (var i in TerrainMeshBatches)
                    {
                        i.Value.UpdateRenderFlags();
                        var modifier = i.Value.GetInstanceModifier();
                        i.Value.GpuDrivenData.InstanceCulling(modifier, cmd.mCoreObject, policy);
                    }
                }
            }   
        }
        public void Commit(TtRenderPolicy policy, NxRHI.TtCommandList cmd, NxRHI.TtCmdRecorder cmdRecorder, TtGraphicsBuffers GBuffers)
        {
            var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
            foreach (var i in TerrainMeshBatches.Values)
            {
                foreach (var j in i.RenderMesh.SubMeshes)
                {
                    foreach (var k in j.Atoms)
                    {
                        var layer = k.Material.RenderLayer;
                        if (layer == ERenderLayer.RL_Opaque)
                        {
                            var drawcall = k.GetDrawCall(cmd.mCoreObject, GBuffers, policy, this);
                            if (drawcall != null)
                            {
                                drawcall.BindGBuffer(camera, GBuffers);
                                cmdRecorder.PushGpuDraw(drawcall);
                            }
                        }
                    }
                }
            }

            foreach (var i in InstanceMeshes.Values)
            {
                if (i.IsDraw == false)
                    continue;
                foreach (var j in i.RenderMesh.SubMeshes)
                {
                    foreach (var k in j.Atoms)
                    {
                        var layer = k.Material.RenderLayer;
                        if (layer == ERenderLayer.RL_Opaque)
                        {
                            var drawcall = k.GetDrawCall(cmd.mCoreObject, GBuffers, policy, this);
                            if (drawcall != null)
                            {
                                drawcall.BindGBuffer(camera, GBuffers);
                                cmdRecorder.PushGpuDraw(drawcall);
                            }
                        }
                    }
                }
            }
        }
        [Rtti.Meta("")]
        public bool EnableInstanceMeshCulling { get; set; } = true;
        [Rtti.Meta("")]
        public bool EnableStaticMeshBatch { get; set; } = false;
        [Rtti.Meta("")]
        public bool EnableTerrainBatch { get; set; } = true;
        #region InstanceMeshCulling
        // flags 在 batch 构建时固定，运行时切换会选取对应的实例渲染 batch。
        public Dictionary<(Hash64 MeshHash, TtMaterial.ERenderFlags MeshRenderFlags), TtInstanceStaticMeshBatch> InstanceMeshes = new Dictionary<(Hash64, TtMaterial.ERenderFlags), TtInstanceStaticMeshBatch>();
        private void ResetInstanceMesh()
        {
            foreach (var i in InstanceMeshes.Values)
            {
                i.IsDraw = false;
            }
        }
        private FVisibleMesh PushInstanceMesh(Mesh.TtRenderMesh mesh, Mesh.TtMdfInstanceStaticMesh mdfQueue)
        {
            var key = (TtInstanceStaticMeshBatch.MeshBatchHash(mesh), mesh.MeshRenderFlags);
            TtInstanceStaticMeshBatch batch;
            if (InstanceMeshes.TryGetValue(key, out batch) == false)
            {
                batch = new TtInstanceStaticMeshBatch();
                batch.Initialize(this, mesh.MaterialMesh, mdfQueue, key.Item2);
                InstanceMeshes.Add(key, batch);
            }
            batch.IsDraw = true;
            //var tm = mesh.PerMeshCBuffer.GetMatrix(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);
            //batch.RenderMesh.DirectSetWorldMatrix(in tm);
            batch.RenderMesh.UnsafeSetPerMeshCBuffer(mesh.PerMeshCBuffer);
            batch.MdfQueue.InstanceModifier.InstanceBuffers.InstanceBuffer = mdfQueue.InstanceModifier.InstanceBuffers.InstanceBuffer;
            batch.MdfQueue.InstanceModifier.InstanceBuffers.InstanceBoundingBuffer = mdfQueue.InstanceModifier.InstanceBuffers.InstanceBoundingBuffer;
            return new FVisibleMesh() { Mesh = mesh, DrawMode = FVisibleMesh.EDrawMode.Instance };
        }
        #endregion
        #region StaticMeshBatch
        // flags 是精确键的一部分，不能让不同 flags 的实例共享同一份 batch CBV。
        public Dictionary<(Hash64 MeshHash, TtMaterial.ERenderFlags MeshRenderFlags), TtStaticMeshBatch> StaticMeshBatches = new Dictionary<(Hash64, TtMaterial.ERenderFlags), TtStaticMeshBatch>();
        private void ResetStaticMeshBatch()
        {
            foreach(var i in StaticMeshBatches)
            {
                i.Value.GetInstanceModifier().InstanceBuffers.ResetInstance();
            }
        }
        private FVisibleMesh PushStaticMeshBatch(Mesh.TtRenderMesh mesh)
        {
            foreach (var i in mesh.MaterialMesh.SubMeshes)
            {
                var key = (TtStaticMeshBatch.MeshBatchHash(i), mesh.MeshRenderFlags);
                TtStaticMeshBatch batch;
                if (StaticMeshBatches.TryGetValue(key, out batch) == false)
                {
                    batch = new TtStaticMeshBatch();
                    batch.Initialize(this, i, key.Item2);
                    StaticMeshBatches.Add(key, batch);
                }
                var cb = mesh.PerMeshCBuffer;
                var matrix = cb.GetMatrix(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrix);
                var instance = new Graphics.Pipeline.Shader.FVSInstanceData();
                instance.SetMatrix(matrix);
                instance.HitProxyId = cb.GetValue<uint>(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.HitProxyId);
                batch.GetInstanceModifier().PushInstance(in instance, new Graphics.Mesh.Modifier.FCullBounding());
            }
            return new FVisibleMesh(){Mesh = mesh, DrawMode = FVisibleMesh.EDrawMode.Instance };
        }
        #endregion
        #region TerrainMeshBatch
        // 地形 flags 由 TerrainNode 统一维护，不按 patch 的 flags 拆分 batch。
        public Dictionary<Hash64, TtTerrainMeshBatch> TerrainMeshBatches = new Dictionary<Hash64, TtTerrainMeshBatch>();
        private void ResetTerrainMeshBatch()
        {
            foreach (var i in TerrainMeshBatches)
            {
                i.Value.GetInstanceModifier().InstanceBuffers.ResetInstance();
            }
        }
        private FVisibleMesh PushTerrainMeshBatch(Mesh.TtRenderMesh mesh, Bricks.Terrain.CDLOD.TtTerrainMdfQueue mdfQueue)
        {
            foreach (var i in mesh.MaterialMesh.SubMeshes)
            {
                var hash = TtTerrainMeshBatch.MeshBatchHash(i, mdfQueue);
                TtTerrainMeshBatch batch;
                if (TerrainMeshBatches.TryGetValue(hash, out batch) == false)
                {
                    batch = new TtTerrainMeshBatch();
                    batch.Initialize(this, i, mdfQueue.LodLevel, mdfQueue.Patch.TerrainNode);
                    //batch.MdfQueue.TerrainModifier.Patch = mdfQueue.Patch;
                    TerrainMeshBatches.Add(hash, batch);
                    //batch.RenderMesh.SetWorldTransform(in mdfQueue.Patch.TerrainNode.Placement.TransformRef, mdfQueue.Patch.TerrainNode.GetWorld(), true);
                    //batch.RenderMesh.UnsafeSetPerMeshCBuffer(mesh.PerMeshCBuffer);
                }
                else
                {
                    //batch.Initialize(this, i, mdfQueue.LodLevel);
                    //batch.MdfQueue.TerrainModifier.TerrainNode = mdfQueue.Patch.TerrainNode;
                }
                // 世界矩阵每次 cull 都要重刷: SetWorldTransform 会把 world.CameraOffset
                // 烘焙进 WorldMatrix, 而 batch 按"几何+材质"哈希缓存、与基准无关,
                // 只在创建时写一次的话, 跨 level 换基准后矩阵停在旧偏移上,
                // 表现为 EnableTerrainBatch 开启时整个地形持续错位一个 cell 差。
                // 每帧重刷成本可忽略 (一次减法+矩阵组装), 与 patch cbuffer/instance
                // 数据/cull AABB 同源同帧。
                batch.RenderMesh.SetWorldTransform(in mdfQueue.Patch.TerrainNode.Placement.TransformRef, mdfQueue.Patch.TerrainNode.GetWorld(), true);
                
                var world = mdfQueue.TerrainModifier.TerrainNode.GetWorld();
                mdfQueue.TerrainModifier.ActiveRVTs_OnRenderThread();
                var instance = new Graphics.Pipeline.Shader.FVSInstanceData();
                Bricks.Terrain.CDLOD.TtTerrainModifier.SetInstanceData(mesh, mdfQueue, ref instance);
                DBoundingBox aabb;
                aabb.Minimum = mdfQueue.Patch.AABB.Minimum - world.CameraOffset;
                aabb.Maximum = mdfQueue.Patch.AABB.Maximum - world.CameraOffset;
                var cullBounding = new Graphics.Mesh.Modifier.FCullBounding(aabb);
                batch.GetInstanceModifier().PushInstance(in instance, in cullBounding);
            }
            return new FVisibleMesh() { Mesh = mesh, DrawMode = FVisibleMesh.EDrawMode.Instance };
        }
        #endregion
    }
}

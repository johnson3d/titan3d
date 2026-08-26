using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class TtPatchLayers
    {
        public class TtPatchLayer
        {
            public string Name { get; set; }
            public float[,] WeightData = null;
        }
        public List<TtPatchLayer> Layers = new List<TtPatchLayer>();
        internal int CurrentLayer = -1;
        public void SetLayerData(TtLayerManager mgr, int x, int z, float data)
        {
            if (CurrentLayer < 0 || Layers[CurrentLayer].Name != mgr.CurrentLayerName)
            {
                CurrentLayer = GetLayer(mgr.CurrentLayerName);
                if (CurrentLayer < 0)
                {
                    CurrentLayer = AddLayer(mgr.CurrentLayerName);
                }
            }
            Layers[CurrentLayer].WeightData[z, x] = data;
        }
        public float GetLayerData(TtLayerManager mgr, int x, int z)
        {
            if (CurrentLayer < 0 || Layers[CurrentLayer].Name != mgr.CurrentLayerName)
            {
                CurrentLayer = GetLayer(mgr.CurrentLayerName);
            }
            if (CurrentLayer < 0)
                return float.NaN;
            return Layers[CurrentLayer].WeightData[z, x];
        }
        public int GetLayer(string name)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].Name == name)
                    return i;
            }
            return -1;
        }
        public int AddLayer(string name)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].Name == name)
                    return i;
            }
            Layers.Add(new TtPatchLayer());
            return Layers.Count - 1;
        }
    }

    public class TtLayerManager : IO.BaseSerializer
    {
        public string CurrentLayerName { get; set; } = null;
        [Rtti.Meta("")]
        public List<string> LayerNames { get; set; } = new List<string>();
    }

    public class TtPatch : IDisposable
    {
        public int IndexX;
        public int IndexZ;
        public int XInLevel;
        public int ZInLevel;
        public DBoundingBox AABB;

        public TtPatchLayers Layers = new TtPatchLayers();

        public UTerrainLevelData Level;
        public TtTerrainNode TerrainNode
        {
            get => Level.Level.Node;
        }
        public Graphics.Mesh.TtRenderMesh[] TerrainMesh;
        public Graphics.Mesh.TtRenderMesh[] WaterMesh;
        public Graphics.Mesh.TtRenderMesh[] WireFrameTerrainMesh;
        //public Graphics.Pipeline.Shader.UMaterialInstance Material;
        //public Graphics.Pipeline.Shader.UMaterialInstance WaterMaterial;
        public NxRHI.TtCbView PatchCBuffer;
        public void SureCBuffer(NxRHI.IGraphicsEffect shaderProg, ref NxRHI.TtCbView cbuffer)
        {
            var coreBinder = Graphics.Pipeline.TtCoreShaderBinder.TtPerTerrainPatchCBufferVarIndexer.Instance;
            if (cbuffer == null)
            {
                coreBinder.UpdateFieldVar(shaderProg, "cbPerPatch");
                cbuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.Binder.mCoreObject);

                // Defensive init: fill safe defaults right after creation so the cbuffer is never
                // read as uninitialized garbage by the GPU (real values get written by the caller
                // before BindCBV in TerrainMdfQueue, but we guard against any reordering issue).
                cbuffer.SetValue(coreBinder.StartPosition, in StartPosition);
                cbuffer.SetValue(coreBinder.CurrentLOD, mCurrentLOD);
                cbuffer.SetValue(coreBinder.TexUVOffset, in TexUVOffset);
                // 0 = "参数不在本 cbuffer 里", 是两条路径里更保守的默认值:
                // 它让 shader 去读 instance data 而不是相信一份还没填好的 patch 参数。
                cbuffer.SetValue(coreBinder.UsePatchRVTParams, (uint)0);
                cbuffer.SetValue(coreBinder.HeightMapTexID, (uint)0);
                cbuffer.SetValue(coreBinder.NormalMapTexID, (uint)0);
                cbuffer.SetValue(coreBinder.MaterialIdTexID, (uint)0);
                cbuffer.MarkDirty();
                cbuffer.FlushDirty();
            }
            if (TerrainNode.TerrainCBuffer == null)
            {
                coreBinder.UpdateFieldVar(shaderProg, "cbPerTerrain");
                TerrainNode.TerrainCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.Binder.mCoreObject);

                // Defensive init: trigger TerrainNode's own cbuffer fill logic right away so that
                // the very first frame after creation never binds an uninitialized TerrainCBuffer.
                // Without this, if SureCBuffer happens to run after TerrainNode.UpdateCBuffer() in
                // the same frame, the cbuffer would be bound with garbage data for one frame.
                TerrainNode.SureTerrainCBufferInitialized();
            }
        }

        public UTerrainGrassManager GrassManager;

        // 把本 patch 用到的四张图标记为本帧活跃, 供 TtVirtualTextureBase.TickSync 分配 slot 并上传到 atlas。
        // ActiveTexIDs 每帧会被 ProcessChanged 清空, 所以只要 patch 还可见就必须每帧重新调一次。
        // 幂等: ActiveRVT 内部会对 UniqueTexID 去重, 同一 level 的多个 patch 重复调用无副作用。
        public void ActiveRVTs()
        {
            var terrain = TerrainNode.Terrain;
            terrain.HeightmapRVT.ActiveRVT(Level.HeightMapSRV);
            terrain.HeightmapRVT.ActiveRVT(Level.WaterHMapSRV);
            terrain.NormalmapRVT.ActiveRVT(Level.NormalMapSRV);
            terrain.MaterialIdRVT.ActiveRVT(Level.MaterialIdMapSRV);
        }

        public Vector3 StartPosition = new Vector3(0);
        public Vector2 TexUVOffset;        
        int mCurrentLOD;
        public int CurrentLOD
        {
            get => mCurrentLOD;
            set
            {
                mCurrentLOD = value;
            }
        }
        ~TtPatch()
        {
            Dispose();
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref PatchCBuffer);
            if (TerrainMesh != null)
            {
                foreach(var i in TerrainMesh)
                {
                    i.Dispose();
                }
                TerrainMesh = null;
            }
            if (WaterMesh != null)
            {
                foreach (var i in WaterMesh)
                {
                    i.Dispose();
                }
                WaterMesh = null;
            }
            if (WireFrameTerrainMesh != null)
            {
                foreach (var i in WireFrameTerrainMesh)
                {
                    i.Dispose();
                }
                WireFrameTerrainMesh = null;
            }
            CoreSDK.DisposeObject(ref GrassManager);
        }
        public void Initialize(UTerrainLevelData level, int x, int z, Bricks.Procedure.TtBufferComponent HeightMap)
        {
            Level = level;

            // 原本这里是 `if (x == 16 || z == 16) return;`, 硬编码了 PatchSide 的默认值。
            // PatchSide == 16 时索引范围是 0..15, 所以它从来没真正生效过; 而一旦 PatchSide
            // 被调大, 它反而会让 x/z == 16 的 patch 静默地半初始化 (XInLevel/IndexX/AABB 全留 0),
            // 表现为地形中间少一行一列并且剔除盒错位。换成真正的边界检查。
            var patchSide = level.Level.PatchSide;
            System.Diagnostics.Debug.Assert(x >= 0 && x < patchSide && z >= 0 && z < patchSide);
            if (x < 0 || x >= patchSide || z < 0 || z >= patchSide)
            {
                return;
            }
            XInLevel = x;
            ZInLevel = z;
            var terrain = level.GetTerrainNode().Terrain;
            IndexX = x + level.Level.LevelX * level.GetTerrainNode().PatchSide;
            IndexZ = z + level.Level.LevelZ * level.GetTerrainNode().PatchSide;

            //Material = Graphics.Pipeline.Shader.UMaterialInstance.CreateMaterialInstance(terrain.Material);
            //WaterMaterial = Graphics.Pipeline.Shader.UMaterialInstance.CreateMaterialInstance(terrain.WaterMaterial);
            //var srv = Material.FindSRV("Diffuse");
            //if (srv != null)
            //{
            //    //srv.Value = RName.GetRName("");
            //}

            var mdfType = Rtti.TtTypeDesc.TypeOf(typeof(UTerrainMdfQueue));
            var tMaterials = new Graphics.Pipeline.Shader.TtMaterial[1];
            tMaterials[0] = terrain.Material;

            var tWireFrameMaterials = new Graphics.Pipeline.Shader.TtMaterial[1];
            tWireFrameMaterials[0] = (terrain as TtTerrainSystem).WireFrameMaterial;

            var twMaterials = new Graphics.Pipeline.Shader.TtMaterial[1];
            twMaterials[0] = terrain.WaterMaterial;

            TerrainMesh = new Graphics.Mesh.TtRenderMesh[terrain.GridMipLevels.Length];
            WaterMesh = new Graphics.Mesh.TtRenderMesh[terrain.GridMipLevels.Length];
            WireFrameTerrainMesh = new Graphics.Mesh.TtRenderMesh[terrain.GridMipLevels.Length];
            
            for (int i = 0; i < terrain.GridMipLevels.Length; i++)
            {
                TerrainMesh[i] = new Graphics.Mesh.TtRenderMesh();
                TerrainMesh[i].Initialize(terrain.GridMipLevels[i], tMaterials, mdfType);
                var trMdfQueue = TerrainMesh[i].MdfQueue as UTerrainMdfQueue;
                trMdfQueue.TerrainModifier.TerrainNode = this.TerrainNode;
                trMdfQueue.TerrainModifier.Patch = this;
                trMdfQueue.TerrainModifier.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);

                WireFrameTerrainMesh[i] = new Graphics.Mesh.TtRenderMesh();
                WireFrameTerrainMesh[i].Initialize(terrain.GridMipLevels[i], tWireFrameMaterials, mdfType);
                trMdfQueue = WireFrameTerrainMesh[i].MdfQueue as UTerrainMdfQueue;
                trMdfQueue.TerrainModifier.TerrainNode = this.TerrainNode;
                trMdfQueue.TerrainModifier.Patch = this;
                trMdfQueue.TerrainModifier.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);

                WaterMesh[i] = new Graphics.Mesh.TtRenderMesh();
                WaterMesh[i].Initialize(terrain.GridMipLevels[i], twMaterials, mdfType);
                trMdfQueue = WaterMesh[i].MdfQueue as UTerrainMdfQueue;
                trMdfQueue.TerrainModifier.TerrainNode = this.TerrainNode;
                trMdfQueue.TerrainModifier.Patch = this;
                trMdfQueue.TerrainModifier.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);
                trMdfQueue.TerrainModifier.IsWater = true;

                //trMdfQueue.StartPosition.X = IndexX * terrain.PatchSize;
                //trMdfQueue.StartPosition.Z = IndexZ * terrain.PatchSize;

                //trMdfQueue.StartPosition += node.StartPosition;
            }

            var PatchSize = level.GetTerrainNode().PatchSize;

            AABB.Minimum.X = IndexX * PatchSize;
            AABB.Minimum.Z = IndexZ * PatchSize;
            AABB.Minimum.Y = double.MaxValue;

            AABB.Maximum.X = (IndexX + 1) * PatchSize;
            AABB.Maximum.Z = (IndexZ + 1) * PatchSize;
            AABB.Maximum.Y = double.MinValue;

            UpdateAABB(HeightMap, null);
            
            AABB.Minimum += level.GetTerrainNode().Placement.AbsTransform.mPosition;
            AABB.Maximum += level.GetTerrainNode().Placement.AbsTransform.mPosition;

            var terrainNode = this.Level.Level.Node;
            OnAbsTransformChanged(terrainNode, terrainNode.GetWorld());
            UpdateCameraOffset(terrainNode.GetWorld());
            SetAcceptShadow(level.GetTerrainNode().IsAcceptShadow);

            GrassManager = new UTerrainGrassManager(this);
        }
        public void UpdateAABB(Bricks.Procedure.TtBufferComponent HeightMap, Bricks.Procedure.TtBufferComponent WaterHMap)
        {
            int TexSizePerPatch = Level.GetTerrainNode().TexSizePerPatch;
            for (int i = 0; i < TexSizePerPatch; i++)
            {
                for (int j = 0; j < TexSizePerPatch; j++)
                {
                    float alt = HeightMap.GetPixel<float>(XInLevel * TexSizePerPatch + j, ZInLevel * TexSizePerPatch + i);
                    AABB.Maximum.Y = MathHelper.Max(alt, AABB.Maximum.Y);
                    AABB.Minimum.Y = MathHelper.Min(alt, AABB.Minimum.Y);

                    if (WaterHMap != null)
                    {
                        alt = WaterHMap.GetPixel<float>(XInLevel * TexSizePerPatch + j, ZInLevel * TexSizePerPatch + i);
                        AABB.Maximum.Y = MathHelper.Max(alt, AABB.Maximum.Y);
                        AABB.Minimum.Y = MathHelper.Min(alt, AABB.Minimum.Y);
                    }
                }
            }
        }
        public void SetAcceptShadow(bool value)
        {
            for (int i = 0; i < TerrainMesh.Length; i++)
            {
                var mMesh = TerrainMesh[i];
                if (mMesh == null)
                    return;

                //var saved = mMesh.MdfQueue.MdfDatas;
                //Rtti.TtTypeDesc mdfQueueType;
                //if (value)
                //{
                //    mdfQueueType = mMesh.MdfQueue.MdfPermutations.ReplacePermutation<Graphics.Pipeline.Shader.UMdf_NoShadow, Graphics.Pipeline.Shader.UMdf_Shadow>();
                //}
                //else
                //{
                //    mdfQueueType = mMesh.MdfQueue.MdfPermutations.ReplacePermutation<Graphics.Pipeline.Shader.UMdf_Shadow, Graphics.Pipeline.Shader.UMdf_NoShadow>();
                //}
                //mMesh.SetMdfQueueType(mdfQueueType);
                //mMesh.MdfQueue.MdfDatas = saved;

                //int ObjectFlags_2Bit = 0;
                //if (value)
                //    ObjectFlags_2Bit |= 1;
                //else
                //    ObjectFlags_2Bit &= (~1);
                //mMesh.PerMeshCBuffer.SetValue(NxRHI.UBuffer.mPerMeshIndexer.ObjectFLags_2Bit, in ObjectFlags_2Bit);
                mMesh.IsAcceptShadow = value;
            }
        }
        public void Tick(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            if (TerrainMesh == null)
                return;

            var node = Level.GetTerrainNode();
            var patchSize = node.PatchSize;
            
            DVector3 CameraOffset = node.Placement.AbsTransform.mPosition - world.CameraOffset;

            StartPosition.X = (float)(((double)(IndexX * patchSize)) + CameraOffset.X);
            StartPosition.Z = (float)(((double)(IndexZ * patchSize)) + CameraOffset.Z);
            StartPosition.Y = (float)(Level.HeightMapMinHeight + CameraOffset.Y);
        }        
        public void OnAbsTransformChanged(TtTerrainNode node, GamePlay.TtWorld world)
        {
            ref var transform = ref node.Placement.AbsTransform;
            foreach (var i in TerrainMesh)
            {
                i.SetWorldTransform(in transform, world, false);
            }
            foreach (var i in WireFrameTerrainMesh)
            {
                i.SetWorldTransform(in transform, world, false);
            }
            foreach (var i in WaterMesh)
            {
                i.SetWorldTransform(in transform, world, false);
            }
        }
        public void UpdateCameraOffset(GamePlay.TtWorld world)
        {
            foreach (var i in TerrainMesh)
            {
                if (i==null)
                    continue;
                i.UpdateCameraOffset(world);
            }
            foreach (var i in WireFrameTerrainMesh)
            {
                if (i==null)
                    continue;
                i.UpdateCameraOffset(world);
            }
            foreach (var i in WaterMesh)
            {
                if (i==null)
                    continue;
                i.UpdateCameraOffset(world);
            }
        }
        public void OnGatherVisibleMeshes(GamePlay.TtWorld.TtVisParameter rp)
        {
            if (CurrentLOD >= TerrainMesh.Length)
                return;

            switch (Level.GetTerrainNode().Terrain.ShowMode)
            {
                case TtTerrainSystem.EShowMode.Normal:
                    rp.AddVisibleMesh(TerrainMesh[CurrentLOD]);
                    break;
                case TtTerrainSystem.EShowMode.WireFrame:
                    rp.AddVisibleMesh(WireFrameTerrainMesh[CurrentLOD]);
                    break;
                case TtTerrainSystem.EShowMode.Both:
                    {
                        rp.AddVisibleMesh(TerrainMesh[CurrentLOD]);
                        rp.AddVisibleMesh(WireFrameTerrainMesh[CurrentLOD]);
                    }
                    break;
            }

            if (Level.GetTerrainNode().Terrain.IsShowWater)
                rp.AddVisibleMesh(WaterMesh[CurrentLOD]);

            GrassManager?.OnGatherVisibleMeshes(rp);
        }
    }
}

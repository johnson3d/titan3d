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

        public TtTerrainLevelData Level;
        public TtTerrainNode TerrainNode
        {
            get => Level.Level.Node;
        }
        public Graphics.Mesh.TtRenderMesh[] TerrainMesh;
        public Graphics.Mesh.TtRenderMesh[] WaterMesh;
        public Graphics.Mesh.TtRenderMesh[] WireFrameTerrainMesh;
        // TtTerrainNode.ShowLODGrid 调试用: patch 边界线 (白) 与 level 边界线 (品红) 的
        // LineList 网格, 在首次可见且开关打开时懒构建, 高度从 Level.SourceHeightMap 采样
        public Graphics.Mesh.TtRenderMesh DebugBorderMesh;
        public Graphics.Mesh.TtRenderMesh DebugLevelBorderMesh;
        float mDebugBuiltOffset = float.NaN;
        int mDebugBuiltHeightVersion = -1;
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
                cbuffer.SetValue(coreBinder.CurrentLOD, mCurrentLOD >= TerrainMesh.Length ? TerrainMesh.Length - 1 : mCurrentLOD);
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
            DisposeDebugBorderMeshes();
            CoreSDK.DisposeObject(ref GrassManager);
        }
        public void Initialize(TtTerrainLevelData level, int x, int z, Bricks.Procedure.TtBufferComponent HeightMap)
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

            var mdfType = Rtti.TtTypeDesc.TypeOf(typeof(TtTerrainMdfQueue));
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
                var trMdfQueue = TerrainMesh[i].MdfQueue as TtTerrainMdfQueue;
                trMdfQueue.TerrainModifier.TerrainNode = this.TerrainNode;
                trMdfQueue.TerrainModifier.Patch = this;
                trMdfQueue.TerrainModifier.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);

                WireFrameTerrainMesh[i] = new Graphics.Mesh.TtRenderMesh();
                WireFrameTerrainMesh[i].Initialize(terrain.GridMipLevels[i], tWireFrameMaterials, mdfType);
                trMdfQueue = WireFrameTerrainMesh[i].MdfQueue as TtTerrainMdfQueue;
                trMdfQueue.TerrainModifier.TerrainNode = this.TerrainNode;
                trMdfQueue.TerrainModifier.Patch = this;
                trMdfQueue.TerrainModifier.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);

                WaterMesh[i] = new Graphics.Mesh.TtRenderMesh();
                WaterMesh[i].Initialize(terrain.GridMipLevels[i], twMaterials, mdfType);
                trMdfQueue = WaterMesh[i].MdfQueue as TtTerrainMdfQueue;
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

        void DisposeDebugBorderMeshes()
        {
            DebugBorderMesh?.Dispose();
            DebugBorderMesh = null;
            DebugLevelBorderMesh?.Dispose();
            DebugLevelBorderMesh = null;
            mDebugBuiltOffset = float.NaN;
            mDebugBuiltHeightVersion = -1;
        }

        /// <summary>
        /// TtTerrainNode.ShowLODGrid 打开时构建/刷新 patch 与 level 的边界调试线。
        /// 顶点直接烘焙节点局部坐标 + LODGridHeightOffset 抬升, 高度从 Level.SourceHeightMap
        /// 采样; 抬升值变化或笔刷编辑了高度 (Level.HeightMapEditVersion 变化) 时自动重建,
        /// 保证线条始终贴着当前地形。重建只发生在调试线可见时, 且每 patch 只有 32 段线, 开销可忽略。
        /// patch 边界用白色 (NavMeshDebugMaterial), level 外框用品红 (NavMeshDebugWireMaterial)。
        /// </summary>
        public unsafe void EnsureDebugBorderMeshes()
        {
            var node = TerrainNode;
            float offset = node.LODGridHeightOffset;
            if (DebugBorderMesh != null && mDebugBuiltOffset == offset &&
                mDebugBuiltHeightVersion == Level.HeightMapEditVersion)
                return;
            var hMap = Level.SourceHeightMap;
            if (hMap == null || hMap.SuperPixels == null)
                return;

            int texPerPatch = node.TexSizePerPatch;
            int patchSide = node.PatchSide;
            const int Steps = 8;
            int texStep = texPerPatch / Steps;
            float segLen = node.PatchSize / Steps;
            int u0 = XInLevel * texPerPatch;
            int v0 = ZInLevel * texPerPatch;
            float x0 = IndexX * node.PatchSize;
            float z0 = IndexZ * node.PatchSize;

            float SampleAlt(int tu, int tv)
            {
                tu = Math.Clamp(tu, 0, hMap.Width - 1);
                tv = Math.Clamp(tv, 0, hMap.Height - 1);
                return hMap.GetPixel<float>(tu, tv);
            }

            var patchLines = new List<(Vector3 A, Vector3 B)>();
            var levelLines = new List<(Vector3 A, Vector3 B)>();
            void EmitEdge(List<(Vector3 A, Vector3 B)> lines, Vector3 start, Vector3 dir,
                int tuStart, int tvStart, int tuDir, int tvDir)
            {
                var prev = new Vector3(start.X, SampleAlt(tuStart, tvStart) + offset, start.Z);
                for (int i = 1; i <= Steps; i++)
                {
                    var cur = new Vector3(start.X + dir.X * i * segLen,
                        SampleAlt(tuStart + tuDir * i, tvStart + tvDir * i) + offset,
                        start.Z + dir.Z * i * segLen);
                    lines.Add((prev, cur));
                    prev = cur;
                }
            }

            // 落在 level 外框上的边归入 levelLines (品红), 其余是 patch 间边界 (白)
            bool onSouth = ZInLevel == 0;
            bool onNorth = ZInLevel == patchSide - 1;
            bool onWest = XInLevel == 0;
            bool onEast = XInLevel == patchSide - 1;
            EmitEdge(onSouth ? levelLines : patchLines, new Vector3(x0, 0, z0), new Vector3(1, 0, 0), u0, v0, texStep, 0);
            EmitEdge(onNorth ? levelLines : patchLines, new Vector3(x0, 0, z0 + node.PatchSize), new Vector3(1, 0, 0), u0, v0 + texPerPatch, texStep, 0);
            EmitEdge(onWest ? levelLines : patchLines, new Vector3(x0, 0, z0), new Vector3(0, 0, 1), u0, v0, 0, texStep);
            EmitEdge(onEast ? levelLines : patchLines, new Vector3(x0 + node.PatchSize, 0, z0), new Vector3(0, 0, 1), u0 + texPerPatch, v0, 0, texStep);

            Graphics.Mesh.TtRenderMesh BuildLineMesh(List<(Vector3 A, Vector3 B)> lines,
                Graphics.Pipeline.Shader.TtMaterial mat, string name)
            {
                if (lines.Count == 0)
                    return null;
                var mesh = new Graphics.Mesh.TtMeshDataProvider();
                mesh.AssetName = RName.GetRName("@" + name, RName.ERNameType.Transient);
                var builder = mesh.mCoreObject;
                uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_Color));
                builder.Init(streams, false, 1);
                var aabb = new BoundingBox(x0, 0, z0, x0 + node.PatchSize, offset + 1, z0 + node.PatchSize);
                builder.SetAABB(ref aabb);
                var dpDesc = new NxRHI.FMeshAtomDesc();
                dpDesc.SetDefault();
                dpDesc.PrimitiveType = NxRHI.EPrimitiveType.EPT_LineList;
                dpDesc.NumPrimitives = (uint)lines.Count;
                for (int i = 0; i < lines.Count; i++)
                {
                    var v = lines[i];
                    builder.AddVertex(in v.A, in Vector3.UnitY, in Vector2.One, 0xFFFFFFFF);
                    builder.AddVertex(in v.B, in Vector3.UnitY, in Vector2.One, 0xFFFFFFFF);
                    builder.AddLine((ushort)(i * 2), (ushort)(i * 2 + 1));
                }
                builder.PushAtomLOD(0, &dpDesc);
                return mesh.ToDrawMesh(mat);
            }

            DisposeDebugBorderMeshes();
            var mats = TtEngine.Instance.GfxDevice.MaterialManager;
            DebugBorderMesh = BuildLineMesh(patchLines, mats.NavMeshDebugMaterial, "TerrainPatchBorder");
            DebugLevelBorderMesh = BuildLineMesh(levelLines, mats.NavMeshDebugWireMaterial, "TerrainLevelBorder");

            // 新建的网格必须立刻吃上节点变换 (含 world.CameraOffset), 否则会带着单位矩阵
            // 渲染, 与地形错开一段相机偏移量, 改 LODGridHeightOffset 触发重建时看起来
            // 就像"修改没生效"。OnAbsTransformChanged 只在节点移动时才刷, 救不了这里。
            var world = node.GetWorld();
            ref var nodeTransform = ref node.Placement.AbsTransform;
            if (DebugBorderMesh != null)
                DebugBorderMesh.SetWorldTransform(in nodeTransform, world, false);
            if (DebugLevelBorderMesh != null)
                DebugLevelBorderMesh.SetWorldTransform(in nodeTransform, world, false);

            mDebugBuiltOffset = offset;
            mDebugBuiltHeightVersion = Level.HeightMapEditVersion;
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
            if (DebugBorderMesh != null)
                DebugBorderMesh.SetWorldTransform(in transform, world, false);
            if (DebugLevelBorderMesh != null)
                DebugLevelBorderMesh.SetWorldTransform(in transform, world, false);
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
            if (DebugBorderMesh != null)
                DebugBorderMesh.UpdateCameraOffset(world);
            if (DebugLevelBorderMesh != null)
                DebugLevelBorderMesh.UpdateCameraOffset(world);
            foreach (var i in WaterMesh)
            {
                if (i==null)
                    continue;
                i.UpdateCameraOffset(world);
            }
        }
        public void OnGatherVisibleMeshes(GamePlay.TtWorld.TtVisParameter rp)
        {
            // CurrentLOD 未分配 (int.MaxValue 哨兵残留) 或越界时钳到最粗糙网格,
            // 而不是跳过绘制: 最远圈之外的 patch 不会被任何 LOD 圈命中, 但仍需可见
            int lod = CurrentLOD;
            if (lod >= TerrainMesh.Length)
                lod = TerrainMesh.Length - 1;

            switch (Level.GetTerrainNode().Terrain.ShowMode)
            {
                case TtTerrainSystem.EShowMode.Normal:
                    rp.AddVisibleMesh(TerrainMesh[lod]);
                    break;
                case TtTerrainSystem.EShowMode.WireFrame:
                    rp.AddVisibleMesh(WireFrameTerrainMesh[lod]);
                    break;
                case TtTerrainSystem.EShowMode.Both:
                    {
                        rp.AddVisibleMesh(TerrainMesh[lod]);
                        rp.AddVisibleMesh(WireFrameTerrainMesh[lod]);
                    }
                    break;
            }

            if (Level.GetTerrainNode().Terrain.IsShowWater)
                rp.AddVisibleMesh(WaterMesh[lod]);

            if (Level.GetTerrainNode().ShowLODGrid)
                EnsureDebugBorderMeshes();
            if (DebugBorderMesh != null)
                rp.AddVisibleMesh(DebugBorderMesh);
            if (DebugLevelBorderMesh != null)
                rp.AddVisibleMesh(DebugLevelBorderMesh);

            GrassManager?.OnGatherVisibleMeshes(rp);
        }
    }
}

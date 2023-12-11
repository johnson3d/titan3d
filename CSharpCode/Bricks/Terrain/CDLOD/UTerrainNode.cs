using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    [Bricks.CodeBuilder.ContextMenu("TerrainNode", "TerrainNode", GamePlay.Scene.UNode.EditorKeyword)]
    [GamePlay.Scene.UNode(NodeDataType = typeof(UTerrainData), DefaultNamePrefix = "Terrain")]
    public class UTerrainNode : GamePlay.Scene.USceneActorNode
    {
        public override void Dispose()
        {
            LevelStreaming.Dispose();

            ActiveLevels = null;
            if (Levels != null)
            {
                foreach (var i in Levels)
                {
                    i.LevelData?.Dispose();
                }
                Levels = null;
            }

            TerrainMaterialIdManager?.Cleanup();
            TerrainMaterialIdManager = null;
        }
        public class UTerrainData : GamePlay.Scene.UNodeData
        {
            public UTerrainData()
            {
                LODRangeFloat.Add(100.0f);
                LODRangeFloat.Add(200.0f);
                LODRangeFloat.Add(400.0f);
                LODRangeFloat.Add(600.0f);
                LODRangeFloat.Add(950.0f);
            }
            [Rtti.Meta]
            public int MipLevels { get; set; } = 6;
            [Rtti.Meta]
            public int NumOfLevelX { get; set; } = 100;
            [Rtti.Meta]
            public int NumOfLevelZ { get; set; } = 100;
            [Rtti.Meta]
            public int PatchSide { get; set; } = 16;
            [Rtti.Meta]
            public float PatchSize { get; set; } = 64.0f;
            [Rtti.Meta]
            public int ActiveLevel { get; set; } = 1;
            [Rtti.Meta]
            public List<float> LODRangeFloat { get; set; } = new List<float>();
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Procedure.UPgcAsset.AssetExt)]
            public RName PgcName { get; set; }
            
            public int LevelSideX = 1024;
            public int LevelSideZ = 1024;
        }
        
        public UTerrainSystem Terrain { get; } = new UTerrainSystem();
        public int NumOfLevelX;
        public int NumOfLevelZ;
        public int ActiveLevel;
        public float LevelSize;
        public UTerrainLevel[,] Levels;
        public UTerrainLevel[,] ActiveLevels;
        public float PatchSize
        {
            get;
            set;
        } = 64.0f;
        public int PatchSide
        {
            get;
            set;
        } = 16;
        public int TexSizePerPatch
        {
            get;
            set;
        } = 64;
        public float GridSize = 1.0f;
        public float GridTileSize = 1.0f * 1024.0f / 1023.0f;
        public float TexUVScale = 64.0f / 1024.0f;
        public float MaterialIdUVStep = 1.0f / 1024.0f;//TerrainMaterialIdManager
        public float DiffuseUVStep = 1.0f / 1024.0f;// (1.0f / 1024.0f) * 16.0f;//TerrainMaterialIdManager
        public override bool IsAcceptShadow
        {
            get
            {
                return base.IsAcceptShadow;
            }
            set
            {
                base.IsAcceptShadow = value;
                foreach (var i in ActiveLevels)
                {
                    if (i == null)
                        continue;
                    i.LevelData?.SetAcceptShadow(value);
                }
            }
        }
        public Vector2[] MorphRange;
        public DVector3 EyeCenter;
        public Vector3 EyeLocalCenter;

        Hash160 mTerrainGenHash;
        public Hash160 TerrainGenHash
        {
            get
            {
                return mTerrainGenHash;
            }
        }
        Bricks.Procedure.UPgcAsset mTerrainGen;
        public Bricks.Procedure.UPgcAsset TerrainGen
        {
            get
            {
                return mTerrainGen;
            }
        }

        public ULevelStreaming LevelStreaming = new ULevelStreaming();

        public NxRHI.UCbView TerrainCBuffer;

        public VirtualTexture.UVirtualTextureArray RVTextureArray;
        public UTerrainMaterialIdManager TerrainMaterialIdManager { get; set; }
        
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, GamePlay.Scene.UNodeData data, GamePlay.Scene.EBoundVolumeType bvType, Type placementType)
        {
            if (data as UTerrainData == null)
            {
                data = new UTerrainData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            if (await Terrain.Initialize(GetNodeData<UTerrainData>().MipLevels) == false)
                return false;

            var trData = data as UTerrainData;
            PatchSide = trData.PatchSide;
            PatchSize = trData.PatchSize;
            LevelSize = PatchSize * PatchSide;
            NumOfLevelX = trData.NumOfLevelX;
            NumOfLevelZ = trData.NumOfLevelZ;
            ActiveLevel = trData.ActiveLevel;
            GridSize = PatchSize / TexSizePerPatch;
            Levels = new UTerrainLevel[NumOfLevelZ, NumOfLevelX];
            for (int i = 0; i < NumOfLevelZ; i++)
            {
                for (int j = 0; j < NumOfLevelX; j++)
                {
                    Levels[i, j] = new UTerrainLevel();
                    Levels[i, j].CreateLevel(j, i, trData.PatchSide, this);
                }
            }

            int NumOfActiveLevel = 1 + trData.ActiveLevel * 2;
            ActiveLevels = new UTerrainLevel[NumOfActiveLevel, NumOfActiveLevel];

            SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);

            MorphRange = new Vector2[(data as UTerrainData).LODRangeFloat.Count];

            IsAcceptShadow = true;

            //RVTextureArray = new EngineNS.Bricks.VirtualTexture.UVirtualTextureArray();
            //RVTextureArray.CreateRVT(64, 64, 1, EPixelFormat.PXF_R8G8B8A8_UNORM, 256);
            //var cmd = UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList();
            //RVTextureArray.PushTexture2D(cmd, RName.GetRName("utest/texture/xsl.srv"));

            if(trData.PgcName==null)
                trData.PgcName = RName.GetRName("UTest/terraingen.pgc");

            mTerrainGen = Procedure.UPgcAsset.LoadAsset(trData.PgcName);// RName.GetRName("UTest/terraingen.pgc"));            
            {
                var pgcText = IO.TtFileManager.ReadAllText(trData.PgcName.Address);
                var refAssets = mTerrainGen.GetAMeta().RefAssetRNames;
                refAssets.Sort();
                foreach (var i in refAssets)
                {
                    pgcText += i.ToString();//todo:Asset Hash
                }
                mTerrainGenHash = Hash160.CreateHash160(pgcText);
            }
            var hmNode = mTerrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.UMaterialIdMapNode;
            if (hmNode != null)
            {
                TerrainMaterialIdManager = hmNode.MaterialIdManager;
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Default, "TerrainBuildSRV"))
                {
                    TerrainMaterialIdManager.BuildSRV(tsCmd.CmdList);
                }
                await hmNode.SureMaterialResources();
            }

            float bvSX = 0;
            float bvSZ = 0;
            float bvEX = PatchSize * PatchSide * NumOfLevelX;
            float bvEZ = PatchSize * PatchSide * NumOfLevelZ;
            this.BoundVolume.mLocalAABB.Minimum.SetValue(bvSX, -0.5f, bvSZ);
            this.BoundVolume.mLocalAABB.Maximum.SetValue(bvEX, 0.5f, bvEZ);

            return true;
        }

        public struct LODLayer
        {
            public float MorphStart;
            public float MorphRcqRange;
            public float MorphEndDivRange;
            public float MorphEnd;
            
            public int Dimension;
            public float HalfDim;
            public float TwoRcpDim;
            public float LODPad0;
        }
        private void UpdateCBuffer()
        {
            if (TerrainCBuffer == null)
                return;
            var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
            TerrainCBuffer.SetValue(coreBinder.CBPerTerrain.GridSize, GridSize);

            TerrainCBuffer.SetValue(coreBinder.CBPerTerrain.PatchSize, PatchSize);

            TerrainCBuffer.SetValue(coreBinder.CBPerTerrain.TexUVScale, TexUVScale);

            TerrainCBuffer.SetValue(coreBinder.CBPerTerrain.MaterialIdUVStep, MaterialIdUVStep);

            TerrainCBuffer.SetValue(coreBinder.CBPerTerrain.DiffuseUVStep, DiffuseUVStep);

            for (int i = 0; i < MorphRange.Length; i++)
            {
                LODLayer tmp = new LODLayer();
                tmp.MorphStart = MorphRange[i].X;
                tmp.MorphEnd = MorphRange[i].Y;
                tmp.MorphRcqRange = 1.0f / (tmp.MorphEnd - tmp.MorphStart);
                tmp.MorphEndDivRange = tmp.MorphEnd / (tmp.MorphEnd - tmp.MorphStart);

                tmp.Dimension = (int)Math.Pow(2, Terrain.MipLevels - i - 1);
                tmp.HalfDim = 0.5f * (float)tmp.Dimension;
                tmp.TwoRcpDim = 2.0f / (float)tmp.Dimension;

                TerrainCBuffer.SetValue(coreBinder.CBPerTerrain.MorphLODs, i, in tmp);
            }
        }
        protected override void OnParentSceneChanged(GamePlay.Scene.UScene prev, GamePlay.Scene.UScene cur)
        {
            if (ActiveLevels == null)
                return;
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.OnParentSceneChanged(prev, cur);
            }
        }

        #region deprecated
        public void UpdateRangeLOD(List<float> radius, DVector3 eyePos)
        {
            double morphStart = 0;

            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.SetLODLevel(radius.Count - 1);
            }
            
            var sphere = new DBoundingSphere(eyePos, 0);
            for (int i = 0; i < radius.Count - 1; i++)
            {
                sphere.Radius = radius[i];
                this.MorphRange[i].Y = (float)sphere.Radius;
                this.MorphRange[i].X = (float)MathHelper.Lerp(morphStart, sphere.Radius, 0.8f);

                morphStart = SphereCover(in sphere, i, radius.Count - 1);
                morphStart = Math.Sqrt(morphStart);
            }
            this.MorphRange[radius.Count - 1].Y = radius[radius.Count - 1];
            this.MorphRange[radius.Count - 1].X = (float)MathHelper.Lerp(morphStart, radius[radius.Count - 1], 0.8f);

            for (int i = 0; i < radius.Count; i++)
            {
                if (this.MorphRange[i].X > this.MorphRange[i].Y)
                {
                    this.MorphRange[i].Y = this.MorphRange[i].X + 1;
                }
            }

            UpdateCBuffer();

            //if (DebugPrintLOD)
            //{
            //    System.Diagnostics.Debug.Write($"===========================\n");
            //    for (int i = 0; i < NumOfSide; i++)
            //    {
            //        for (int j = 0; j < NumOfSide; j++)
            //        {
            //            System.Diagnostics.Debug.Write($"{TiledDesc[i, j].LODLevel} ");
            //        }
            //        System.Diagnostics.Debug.Write($"\n");
            //    }
            //    System.Diagnostics.Debug.Write($"===========================\n");
            //}
        }
        private unsafe double SphereCover(in DBoundingSphere sphere, int level, int maxLevel)
        {
            double maxDist = 0;
            foreach (var i in ActiveLevels)
            {
                if (i == null || i.LevelData == null)
                    continue;
                var d = i.LevelData.SphereCover(in sphere, level, maxLevel);
                if (d >= maxDist)
                {
                    maxDist = d;
                }
            }
            return maxDist;
        }
        #endregion

        public void UpdateRangeLOD(List<float> radius, DVector3 eyePos, List<TtPatch> patches)
        {
            double morphStart = 0;

            foreach (var i in patches)
            {
                i.CurrentLOD = radius.Count - 1;
            }

            var sphere = new DBoundingSphere(eyePos, 0);
            for (int i = 0; i < radius.Count - 1; i++)
            {
                sphere.Radius = radius[i];
                this.MorphRange[i].Y = (float)sphere.Radius;
                this.MorphRange[i].X = (float)MathHelper.Lerp(morphStart, sphere.Radius, 0.8f);

                morphStart = SphereCover(in sphere, i, radius.Count - 1, patches);
                morphStart = Math.Sqrt(morphStart);
            }
            this.MorphRange[radius.Count - 1].Y = radius[radius.Count - 1];
            this.MorphRange[radius.Count - 1].X = (float)MathHelper.Lerp(morphStart, radius[radius.Count - 1], 0.8f);

            for (int i = 0; i < radius.Count; i++)
            {
                if (this.MorphRange[i].X > this.MorphRange[i].Y)
                {
                    this.MorphRange[i].Y = this.MorphRange[i].X + 1;
                }
            }

            UpdateCBuffer();
        }
        private unsafe double SphereCover(in DBoundingSphere sphere, int level, int maxLevel, List<TtPatch> patches)
        {
            double maxDist = 0;
            DVector3* boxCorner = stackalloc DVector3[8];
            foreach (var i in patches)
            {
                if (i.CurrentLOD == maxLevel)
                {
                    if (DBoundingBox.Intersects(in i.AABB, sphere))
                    {
                        i.CurrentLOD = level;
                        i.AABB.GetCornersUnsafe(boxCorner);
                        for (int k = 0; k < 8; k++)
                        {
                            double d = DVector3.DistanceSquared(in boxCorner[k], in sphere.Center);
                            if (d >= maxDist)
                            {
                                maxDist = d;
                            }
                        }
                    }
                }
            }
            return maxDist;
        }
        //bool DebugPrintLOD = false;
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UTerrainNode), nameof(TickLogic));
        public override bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                EyeCenter = policy.DefaultCamera.mCoreObject.GetPosition();
                EyeLocalCenter = policy.DefaultCamera.mCoreObject.GetLocalPosition();

                if (SetActiveCenter(in EyeCenter))
                {
                    world.CameraOffset = EyeCenter;
                    policy.DefaultCamera.mCoreObject.SetMatrixStartPosition(in EyeCenter);
                }

                //UpdateRangeLOD((NodeData as UTerrainData).LODRangeFloat, EyeCenter - this.Location);

                foreach (var i in ActiveLevels)
                {
                    if (i == null)
                        continue;
                    i.LevelData?.Tick(world, policy);
                }

                LevelStreaming.Tick(UEngine.Instance.ElapsedSecond);
                return base.OnTickLogic(world, policy);
            }   
        }
        protected override void OnAbsTransformChanged()
        {
            if (ActiveLevels != null)
            {
                var world = GetWorld();
                foreach (var i in ActiveLevels)
                {
                    if (i == null)
                        continue;
                    i.LevelData?.OnAbsTransformChanged(this, world);
                }
            }
        }
        private uint CameralOffsetSerialId = 0;
        public override void GetHitProxyDrawMesh(List<TtMesh> meshes)
        {
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.PlantManager.GetHitProxyDrawMesh(meshes);
            }
        }
        public override void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            if (rp.World.CameralOffsetSerialId != CameralOffsetSerialId)
            {
                CameralOffsetSerialId = rp.World.CameralOffsetSerialId;
                foreach (var i in ActiveLevels)
                {
                    if (i == null)
                        continue;
                    i.LevelData?.UpdateCameraOffset(rp.World);
                }
            }
            //todo: QTree culling?
            //foreach (var i in ActiveLevels)
            //{
            //    if (i == null)
            //        continue;
            //    i.LevelData?.OnGatherVisibleMeshes(rp);
            //}

            FrustumCull(rp, VisiblePatches);
            if (rp.CullType == 0)
                UpdateRangeLOD((NodeData as UTerrainData).LODRangeFloat, EyeCenter - this.Location, VisiblePatches);

            foreach (var i in VisiblePatches)
            {
                i.OnGatherVisibleMeshes(rp);
            }

            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.PlantManager.OnGatherVisibleMeshes(rp);
            }

            if (rp.VisibleNodes != null)
            {
                rp.VisibleNodes.Add(this);
            }
        }
        public List<TtPatch> VisiblePatches = new List<TtPatch>();
        public void FrustumCull(GamePlay.UWorld.UVisParameter rp, List<TtPatch> patches)
        {
            patches.Clear();
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.FrustumCull(rp, patches);
            }
        }

        #region LevelManager        
        public Vector2i GetLevelIndex(in DVector3 pos)
        {
            var nsPos = pos - this.Placement.AbsTransform.mPosition;
            Vector2i result;
            result.X = (int)(nsPos.X / LevelSize);
            result.Y = (int)(nsPos.Z / LevelSize);
            return result;
        }
        public UTerrainLevel GetLevel(in DVector3 pos)
        {
            var idxLevel = GetLevelIndex(in pos);
            if (idxLevel.X < 0 || idxLevel.Y < 0 || idxLevel.X >= NumOfLevelX || idxLevel.Y >= NumOfLevelZ)
            {
                return null;
            }
            return Levels[idxLevel.Y, idxLevel.X];
        }
        Vector2i CurrentActiveCenterLevel = new Vector2i(-1, -1);
        public bool SetActiveCenter(in DVector3 pos)
        {
            var idxLevel = GetLevelIndex(in pos);
            if (idxLevel.X < 0 || idxLevel.Y < 0 || idxLevel.X >= NumOfLevelX || idxLevel.Y >= NumOfLevelZ)
            {
                return false;
            }

            if (idxLevel == CurrentActiveCenterLevel)
            {
                return false;
            }
            CurrentActiveCenterLevel = idxLevel;

            var curLevel = Levels[idxLevel.Y, idxLevel.X];
            curLevel.LoadLevelData(LevelStreaming, true);

            int xMin = idxLevel.X - ActiveLevel;
            int xMax = idxLevel.X + ActiveLevel;

            int zMin = idxLevel.Y - ActiveLevel;
            int zMax = idxLevel.Y + ActiveLevel;

            int ActiveNum = 1 + 2 * ActiveLevel;
            for (int i = 0; i < ActiveNum; i++)
            {
                int z = i + zMin;
                if (z < 0 || z >= NumOfLevelZ)
                {
                    continue;
                }
                for (int j = 0; j < ActiveNum; j++)
                {
                    var old = ActiveLevels[i, j];
                    if (old != null)
                    {
                        if (old.LevelX > xMax || old.LevelX < xMin ||
                           old.LevelZ > zMax || old.LevelZ < zMin)
                        {
                            old.ReleaseLevel(LevelStreaming);
                        }
                        ActiveLevels[i, j] = null;
                    }
                    int x = j + xMin;
                    if (x < 0 || x >= NumOfLevelX)
                    { 
                        continue;
                    }
                    else
                    {
                        curLevel = Levels[z, x];
                        curLevel.LoadLevelData(LevelStreaming, false);
                        ActiveLevels[i, j] = curLevel;
                    }
                }
            }

            return true;
        }

        public double GetAltitude(double x, double z, bool forceLoad = true)
        {
            var xGrid = (int)((x - Placement.AbsTransform.Position.X) / LevelSize);
            var zGrid = (int)((z - Placement.AbsTransform.Position.Z) / LevelSize);
            if (xGrid < 0 || xGrid >= NumOfLevelX || zGrid < 0 || zGrid >= NumOfLevelZ)
                return float.MinValue;

            var level = Levels[zGrid, xGrid].LevelData;
            if (level == null)
            {
                if (forceLoad == false)
                    return float.MinValue;

                Levels[zGrid, xGrid].LoadLevelData(LevelStreaming, true);
            }
            var xInLevel = (float)(x % LevelSize);
            var zInLevel = (float)(z % LevelSize);

            return (double)level.GetAltitude(xInLevel, zInLevel, GridSize) + Placement.AbsTransform.Position.Y;
        }
        public float GetLocalAltitude(float x, float z)
        {
            var xGrid = (int)(x / LevelSize);
            var zGrid = (int)(z / LevelSize);
            if (xGrid < 0 || xGrid >= NumOfLevelX || zGrid < 0 || zGrid >= NumOfLevelZ)
                return float.MinValue;

            var level = Levels[zGrid, xGrid].LevelData;
            if (level == null)
            {
                return float.MinValue;
            }
            var xInLevel = (float)(x % LevelSize);
            var zInLevel = (float)(z % LevelSize);

            return level.GetAltitude(xInLevel, zInLevel, GridSize);
        }
        #endregion
    }
}

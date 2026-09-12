using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    [Bricks.CodeBuilder.ContextMenu("TerrainNode", "Graphics\\TerrainNode", GamePlay.Scene.TtNode.EditorKeyword)]
    [GamePlay.Scene.TtNode(NodeDataType = typeof(TtTerrainData), DefaultNamePrefix = "Terrain")]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Bricks.Terrain.CDLOD.UTerrainNode@EngineCore" })]
    public partial class TtTerrainNode : GamePlay.Scene.TtVisual
    {
        public TtTerrainNode() 
        { 
            this.IsCastShadow = true;
            this.IsAcceptShadow = true;
        }
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
        [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Bricks.Terrain.CDLOD.UTerrainNode.UTerrainData@EngineCore" })]
        public class TtTerrainData : GamePlay.Scene.TtNodeData
        {
            public TtTerrainData()
            {
                LODRangeFloat.Add(100.0f);
                LODRangeFloat.Add(200.0f);
                LODRangeFloat.Add(400.0f);
                LODRangeFloat.Add(600.0f);
                LODRangeFloat.Add(950.0f);
                //MaterialName = RName.GetRName("utest/material/terrainidmap.material");
                MaterialName = RName.GetRName("material/terrainidmap.material", RName.ERNameType.Engine);
            }
            [Category("Option")]
            [Rtti.Meta("")]
            public int MipLevels { get; set; } = 6;
            [Category("Option")]
            [Rtti.Meta("")]
            public int NumOfLevelX { get; set; } = 100;
            [Category("Option")]
            [Rtti.Meta("")]
            public int NumOfLevelZ { get; set; } = 100;
            [Category("Option")]
            [Rtti.Meta("")]
            public int PatchSide { get; set; } = 16;
            [Category("Option")]
            [Rtti.Meta("")]
            public float PatchSize { get; set; } = 64.0f;
            [Category("Option")]
            [Rtti.Meta("")]
            public int ActiveLevel { get; set; } = 1;
            [Category("Option")]
            [Rtti.Meta("")]
            public List<float> LODRangeFloat { get; set; } = new List<float>();
            [Category("Option")]
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = Procedure.UPgcAsset.AssetExt)]
            public RName PgcName { get; set; }
            [Category("Option")]
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = TtMaterial.AssetExt)]
            public RName MaterialName { get; set; }
            /// <summary>
            /// 地形材质贴图的场景级覆盖, 按下标与 PGC 图表 MatIdMapping 节点的 MaterialIdArray 对齐。
            /// 长度由 TtTerrainNode.EnsureMaterialTextures 跟随维护, 手动增删无意义。
            /// </summary>
            [Category("Option")]
            [Rtti.Meta("")]
            public List<TtTerrainMaterialTextureOverride> MaterialTextureOverrides { get; set; } = new List<TtTerrainMaterialTextureOverride>();

            public int LevelSideX = 1024;
            public int LevelSideZ = 1024;
        }
        
        public TtTerrainSystem Terrain { get; } = new TtTerrainSystem();
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
                if (ActiveLevels != null)
                {
                    foreach (var i in ActiveLevels)
                    {
                        if (i == null)
                            continue;
                        i.LevelData?.SetAcceptShadow(value);
                    }
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

        public TtLevelStreaming LevelStreaming = new TtLevelStreaming();

        public NxRHI.TtCbView TerrainCBuffer;

        public VirtualTexture.TtVirtualTextureArray RVTextureArray;
        /// <summary>
        /// 指向 PGC 图表 MatIdMapping 节点里的 manager 实例 (UPgcAsset.LoadAsset 不做实例缓存,
        /// 所以每个地形节点持有自己那份副本)。在 Details 里改它只影响本节点的内存副本, 不落盘;
        /// 要持久化贴图改动用 MaterialTextureOverrides。
        /// </summary>
        [Category("TerrainMaterial")]
        public UTerrainMaterialIdManager TerrainMaterialIdManager { get; set; }
        /// <summary>
        /// 地形材质贴图的场景级覆盖, 与 TerrainMaterialIdManager.MaterialIdArray 按下标对齐,
        /// 某项留空即沿用 PGC 图表原值。改这里会存进 .scene, 不会动 .pgc。
        /// 注意: 材质的"数量"以及 TransitionRange / Plants 是 PGC 生成 ID 图的输入 (并计入 level 缓存 hash),
        /// 只能在 PGC 编辑器里改。
        /// </summary>
        [Category("TerrainMaterial")]
        public List<TtTerrainMaterialTextureOverride> MaterialTextureOverrides
        {
            get => TerrainData?.MaterialTextureOverrides;
            set
            {
                var nd = TerrainData;
                if (nd == null)
                    return;
                nd.MaterialTextureOverrides = value ?? new List<TtTerrainMaterialTextureOverride>();
                // 不在这里重建: 下一帧 EnsureMaterialTextures 比对签名后会自动重建
            }
        }
        RName[] mBuiltTexDiffuse;
        RName[] mBuiltTexNormal;
        static bool SameRName(RName a, RName b)
        {
            return (a == null) ? (b == null) : a.Equals(b);
        }
        /// <summary>
        /// 比对"最终生效的贴图组合", 变了就重建 diffuse / normal 的 Texture2DArray。
        /// 之所以用签名比对而不是写在 setter 里: 用户在 Details 里改的是 MaterialTextureOverrides[i]
        /// 或 MaterialIdArray[i] 的子对象属性, 宿主不是本节点, PropertyGrid 不会调到节点上任何 setter
        /// (undo/redo 反射回写同理)。比对成本是材质数量次引用比较, 真正的重建 (读盘 + 建贴图数组) 只在变化时发生。
        /// </summary>
        /// <param name="immediateUpload">
        /// true = 贴图上传立即在当前线程执行 (启动期用, 保证首帧就有贴图);
        /// false = 进全局 RenderQueue, 由渲染线程下一 tick 消费。
        /// </param>
        public void EnsureMaterialTextures(bool immediateUpload = false)
        {
            var mgr = TerrainMaterialIdManager;
            if (mgr == null)
                return;
            int count = mgr.MaterialIdArray.Count;
            if (count == 0)
                return;

            // 覆盖列表长度跟随 MaterialIdArray: 材质数量只能在 PGC 图表里改, 这里只补齐 / 截断槽位
            var overrides = MaterialTextureOverrides;
            if (overrides != null)
            {
                while (overrides.Count < count)
                    overrides.Add(new TtTerrainMaterialTextureOverride());
                if (overrides.Count > count)
                    overrides.RemoveRange(count, overrides.Count - count);
                mgr.TextureOverrides = overrides;
            }

            if (mBuiltTexDiffuse != null && mBuiltTexDiffuse.Length == count)
            {
                bool same = true;
                for (int i = 0; i < count; i++)
                {
                    if (SameRName(mBuiltTexDiffuse[i], mgr.GetEffectiveTexDiffuse(i)) == false ||
                        SameRName(mBuiltTexNormal[i], mgr.GetEffectiveTexNormal(i)) == false)
                    {
                        same = false;
                        break;
                    }
                }
                if (same)
                    return;
            }

            // BuildSRV 内部会先 Cleanup 掉旧的贴图数组, 再把每个 slice 上传。这是启动期 / 编辑期改材质
            // 才会走到的路径, 运行期列表不变则一帧都不会重建。
            // 上传命令按 CodingGuidelines §1.7 走 RenderQueue (同模块参考: TtTerrainSystem.TickSync 的 RVT 上传)。
            var cmdlist = NxRHI.TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "TerrainBuildSRV"))
            {
                mgr.BuildSRV(cmdlist.mCoreObject);
                cmdlist.FlushDraws();
            }
            TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmdlist, "TerrainMaterial.BuildSRV",
                NxRHI.EQueueType.QU_Default, immediateUpload);

            mBuiltTexDiffuse = new RName[count];
            mBuiltTexNormal = new RName[count];
            for (int i = 0; i < count; i++)
            {
                mBuiltTexDiffuse[i] = mgr.GetEffectiveTexDiffuse(i);
                mBuiltTexNormal[i] = mgr.GetEffectiveTexNormal(i);
            }
        }
        public override void AddAssetReferences(IO.IAssetMeta ameta)
        {
            base.AddAssetReferences(ameta);
            var nd = TerrainData;
            if (nd == null)
                return;
            // PGC 图表本体: 图表内部引用的材质贴图 / 植被 mesh 由 UPgcAsset.UpdateAMetaReferences 收集, 经此传递
            if (nd.PgcName != null)
                ameta.AddReferenceAsset(nd.PgcName);
            if (nd.MaterialName != null)
                ameta.AddReferenceAsset(nd.MaterialName);
            // 场景级贴图覆盖不在任何资产的引用链里, 必须显式收集, 否则 cook 后覆盖贴图会丢
            if (nd.MaterialTextureOverrides != null)
            {
                foreach (var i in nd.MaterialTextureOverrides)
                {
                    if (i.TexDiffuse != null)
                        ameta.AddReferenceAsset(i.TexDiffuse);
                    if (i.TexNormal != null)
                        ameta.AddReferenceAsset(i.TexNormal);
                }
            }
        }
        public string TerrainName
        {
            get
            {
                return GetNodeData<TtTerrainData>()?.PgcName?.Name;
            }
        }
        public TtTerrainData TerrainData { get => GetNodeData<TtTerrainData>(); }

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, GamePlay.Scene.TtNodeData data, GamePlay.Scene.EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtTerrainData == null)
            {
                data = new TtTerrainData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            if (await Terrain.Initialize(TerrainData.MaterialName, TerrainData.MipLevels) == false)
                return false;

            var trData = data as TtTerrainData;
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

            SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);

            MorphRange = new Vector2[(data as TtTerrainData).LODRangeFloat.Count];

            IsAcceptShadow = true;

            //RVTextureArray = new EngineNS.Bricks.VirtualTexture.UVirtualTextureArray();
            //RVTextureArray.CreateRVT(64, 64, 1, EPixelFormat.PXF_R8G8B8A8_UNORM, 256);
            //var cmd = TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList();
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
                // 补齐覆盖槽位 + 首次 BuildSRV (含场景级贴图覆盖), 启动期立即上传
                EnsureMaterialTextures(true);
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
        // Public wrapper for defensive initialization from outside (e.g. Patch.SureCBuffer).
        // Ensures TerrainCBuffer is never bound with uninitialized data on the first frame
        // after creation, which would otherwise be possible if SureCBuffer runs after
        // UpdateCBuffer() in the same frame.
        public void SureTerrainCBufferInitialized()
        {
            UpdateCBuffer();
        }
        private void UpdateCBuffer()
        {
            if (TerrainCBuffer == null)
                return;
            var coreBinder = Graphics.Pipeline.TtCoreShaderBinder.TtPerTerrainCBufferVarIndexer.Instance;
            TerrainCBuffer.SetValue(coreBinder.EyeCenter, EyeLocalCenter);
            TerrainCBuffer.SetValue(coreBinder.GridSize, GridSize);

            TerrainCBuffer.SetValue(coreBinder.PatchSize, PatchSize);
            TerrainCBuffer.SetValue(coreBinder.TexUVScale, TexUVScale);
            TerrainCBuffer.SetValue(coreBinder.MaterialIdUVStep, MaterialIdUVStep);
            TerrainCBuffer.SetValue(coreBinder.DiffuseUVStep, DiffuseUVStep);

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

                TerrainCBuffer.SetValue(coreBinder.MorphLODs, i, in tmp);
            }
        }
        protected override void OnParentSceneChanged(GamePlay.Scene.TtScene prev, GamePlay.Scene.TtScene cur)
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
                        i.AABB.UnsafeGetCorners(boxCorner);
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
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtTerrainNode), nameof(TickLogic));
                return mScopeTick;
            }
        }
        
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                // Details 里改了材质贴图的话在这里被发现并重建贴图数组, 没变化时仅做引用比较
                EnsureMaterialTextures();

                EyeCenter = args.Policy.DefaultCamera.mCoreObject.GetPosition();
                EyeLocalCenter = args.Policy.DefaultCamera.mCoreObject.GetLocalPosition();

                if (SetActiveCenter(in EyeCenter))
                {
                    args.World.CameraOffset = EyeCenter;
                    args.Policy.DefaultCamera.mCoreObject.SetMatrixStartPosition(in EyeCenter);
                }

                //UpdateRangeLOD((NodeData as UTerrainData).LODRangeFloat, EyeCenter - this.Location);

                foreach (var i in ActiveLevels)
                {
                    if (i == null)
                        continue;
                    i.LevelData?.Tick(args.World, args.Policy);
                }

                LevelStreaming.Tick(TtEngine.Instance.ElapsedSecond);
                
                if (TtEngine.Instance.Config.Feature_UseRVT)
                {
                    TtEngine.Instance.TickableManager.AddTickSync(static (arg) =>
                    {
                        var ts = arg as TtTerrainSystem;
                        ts.TickSync();
                    }, this.Terrain);
                }

                return base.OnTickLogic(args);
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
        public override void GetHitProxyDrawMesh(List<TtRenderMesh> meshes)
        {
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.PlantManager.GetHitProxyDrawMesh(meshes);
            }
        }
        protected override void OnCameralOffsetChanged(TtWorld world)
        {
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.UpdateCameraOffset(world);
            }
        }
        public override void OnGatherVisibleMeshes(GamePlay.TtWorld.TtVisParameter rp)
        {
            UpdateCameralOffset(rp.World);
            //todo: QTree culling?
            //foreach (var i in ActiveLevels)
            //{
            //    if (i == null)
            //        continue;
            //    i.LevelData?.OnGatherVisibleMeshes(rp);
            //}

            FrustumCull(rp, VisiblePatches);
            if (rp.CullType == 0)
                UpdateRangeLOD((NodeData as TtTerrainData).LODRangeFloat, EyeCenter - this.Location, VisiblePatches);

            foreach (var i in VisiblePatches)
            {
                i.OnGatherVisibleMeshes(rp);
            }

            // RVT 上传不能依赖某个具体渲染节点存在。原来唯一的活跃点在
            // TtGpuCullingNode.PushTerrainMeshBatch 里, 于是主 rpolicy 一旦换成不含
            // TtGpuCullingNode 的 (例如 deferred_simple), ActiveTexIDs 永远是空的,
            // TickSync 在 ProcessChanged() == false 处直接短路, atlas 从未写入 ——
            // 表现为地形整块渲染成平板, 而 CPU 侧高度数据是完全正常的。
            if (TtEngine.Instance.Config.Feature_UseRVT)
            {
                foreach (var i in VisiblePatches)
                {
                    i.ActiveRVTs();
                }
            }

            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.PlantManager.OnGatherVisibleMeshes(rp);
            }

            rp.AddVisibleNode(this);
        }
        public List<TtPatch> VisiblePatches = new List<TtPatch>();
        public void FrustumCull(GamePlay.TtWorld.TtVisParameter rp, List<TtPatch> patches)
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

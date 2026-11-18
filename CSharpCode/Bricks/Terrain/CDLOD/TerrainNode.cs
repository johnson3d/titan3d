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
        /// <summary>Dispose 与异步重建交叉保护: 置位后 BuildTerrainCore 不再发布新地形</summary>
        volatile bool mDisposed = false;
        public TtTerrainNode() 
        { 
            this.IsCastShadow = true;
            this.IsAcceptShadow = true;
        }
        public override void Dispose()
        {
            mDisposed = true;

            // 先取消所有在飞的 DAG bake (bake 单个可达秒级, 不取消会占死线程池线程拖慢退出);
            // CancelBakeTask 幂等, 但用 IsBakeRunning 显式表达"只在在飞时才取消"的意图
            if (Levels != null)
            {
                foreach (var i in Levels)
                {
                    if (i?.IsBakeRunning == true)
                        i.CancelBakeTask();
                }
            }

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

            Terrain?.Dispose();
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
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public int MipLevels { get; set; } = 6;
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public int NumOfLevelX { get; set; } = 100;
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public int NumOfLevelZ { get; set; } = 100;
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public int PatchSide { get; set; } = 16;
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public float PatchSize { get; set; } = 64.0f;
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public int ActiveLevel { get; set; } = 1;
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public List<float> LODRangeFloat { get; set; } = new List<float>();
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = Procedure.TtPgcAsset.AssetExt)]
            public RName PgcName { get; set; }
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = TtMaterial.AssetExt)]
            public RName MaterialName { get; set; }
            /// <summary>
            /// 地形材质贴图的场景级覆盖, 按下标与 PGC 图表 MatIdMapping 节点的 MaterialIdArray 对齐。
            /// 长度由 TtTerrainNode.EnsureMaterialTextures 跟随维护, 手动增删无意义。
            /// </summary>
            [System.ComponentModel.Category("Option")]
            [Rtti.Meta("")]
            public List<TtTerrainMaterialTextureOverride> MaterialTextureOverrides { get; set; } = new List<TtTerrainMaterialTextureOverride>();

            public int LevelSideX = 1024;
            public int LevelSideZ = 1024;
            /// <summary>
            /// 远景 cluster DAG 叶子网格的降采样步长 (texel): 行列取 {0, s, 2s, …} ∪ {Size-1}。
            /// 1 = 全分辨率叶子; 越大烘焙越快, 叶子误差由 M2 精确度量并单调传到祖先, 不会被低估。
            /// </summary>
            [Rtti.Meta("")]
            [System.ComponentModel.Category("VgDag")]
            public int VgDagStep = 2;
        }
        
        /// <summary>
        /// 地形系统 (材质/RVT/各层 Grid)。Details 里改结构参数 (PgcName/尺寸/Patch*/MipLevels/
        /// MaterialName/ActiveLevel) 后会在主循环自动重建并替换此实例 (见 CheckTerrainRebuild)。
        /// </summary>
        public TtTerrainSystem Terrain
        {
            get;
            private set;
        } = new TtTerrainSystem();
        public float LevelSize;
        public TtTerrainLevel[,] Levels;
        public TtTerrainLevel[,] ActiveLevels;
        // PatchSide / PatchSize / NumOfLevelX / NumOfLevelZ / ActiveLevel / MipLevels 的运行时副本
        // 已删除: 现在统一走下方 #region 编辑器属性 的同名包装属性直接读写 TtTerrainData,
        // 避免双份数据源不同步 (外部引用如 Level.Node.PatchSide 对属性透明)。
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
        // 所有 patch/LOD batch 共用节点的对象 flags；由节点状态派生，不重复存储。
        // ShadingMode 等材质 flags 仍由共享材质提供。
        [Browsable(false)]
        public TtMaterial.ERenderFlags MeshRenderFlags
        {
            get => IsAcceptShadow ? TtMaterial.ERenderFlags.AcceptShadow : TtMaterial.ERenderFlags.None;
        }
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

        // CurrentLOD 预计算节流: EyeCenter 移动超过 PatchSize*0.5 才全量重算一次
        // (所有 Level 的 patch CurrentLOD + MorphRange), 静止帧零重算成本;
        // 重建地形/活动窗口变化会置 mLodDirty 强制刷新。
        bool mLodDirty = true;
        DVector3 mLodLastEye;
        double mLodUpdateDistSq = 0;

        /// <summary>
        /// 离线 cook (CookTerrainAsync) 期间置位: 让 level 加载钩子里的 BakeVgDag
        /// 绕过运行期自动 cook 开关 (TtEditorConfig.AutoCookTerrainVgDag, 默认关)。
        /// </summary>
        public bool ForceBakeVgDag;

        Hash160 mTerrainGenHash;
        public Hash160 TerrainGenHash
        {
            get
            {
                return mTerrainGenHash;
            }
        }
        Bricks.Procedure.TtPgcAsset mTerrainGen;
        public Bricks.Procedure.TtPgcAsset TerrainGen
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
        [System.ComponentModel.Category("TerrainMaterial")]
        public UTerrainMaterialIdManager TerrainMaterialIdManager { get; set; }
        /// <summary>
        /// 地形材质贴图的场景级覆盖, 与 TerrainMaterialIdManager.MaterialIdArray 按下标对齐,
        /// 某项留空即沿用 PGC 图表原值。改这里会存进 .scene, 不会动 .pgc。
        /// 注意: 材质的"数量"以及 TransitionRange / Plants 是 PGC 生成 ID 图的输入 (并计入 level 缓存 hash),
        /// 只能在 PGC 编辑器里改。
        /// </summary>
        [System.ComponentModel.Category("TerrainMaterial")]
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
        [Browsable(false)]
        public TtTerrainData TerrainData { get => GetNodeData<TtTerrainData>(); }

        #region 编辑器属性 (转发到 TtTerrainData, 规范见 CodingGuidelines §9)
        // 刷新策略见 CheckTerrainRebuild 注释: 结构级参数由每帧签名比对驱动重建, 这里统一纯转发。
        // getter/setter 都要判空: PropertyGrid 反射读 getter 不挑时机, NodeData 未就绪时不能抛。

        [Category("Option")]
        public int MipLevels { get => TerrainData?.MipLevels ?? 6; set { var nd = TerrainData; if (nd == null) return; nd.MipLevels = value; } }
        [Category("Option")]
        public int NumOfLevelX { get => TerrainData?.NumOfLevelX ?? 100; set { var nd = TerrainData; if (nd == null) return; nd.NumOfLevelX = value; } }
        [Category("Option")]
        public int NumOfLevelZ { get => TerrainData?.NumOfLevelZ ?? 100; set { var nd = TerrainData; if (nd == null) return; nd.NumOfLevelZ = value; } }
        [Category("Option")]
        public int PatchSide { get => TerrainData?.PatchSide ?? 16; set { var nd = TerrainData; if (nd == null) return; nd.PatchSide = value; } }
        [Category("Option")]
        public float PatchSize { get => TerrainData?.PatchSize ?? 64.0f; set { var nd = TerrainData; if (nd == null) return; nd.PatchSize = value; } }
        [Category("Option")]
        public int ActiveLevel { get => TerrainData?.ActiveLevel ?? 1; set { var nd = TerrainData; if (nd == null) return; nd.ActiveLevel = value; } }
        [Category("Option")]
        public List<float> LODRangeFloat { get => TerrainData?.LODRangeFloat; set { var nd = TerrainData; if (nd == null) return; nd.LODRangeFloat = value; } }
        [Category("Option")]
        [RName.PGRName(FilterExts = Procedure.TtPgcAsset.AssetExt)]
        public RName PgcName { get => TerrainData?.PgcName; set { var nd = TerrainData; if (nd == null) return; nd.PgcName = value; } }
        [Category("Option")]
        [RName.PGRName(FilterExts = TtMaterial.AssetExt)]
        public RName MaterialName { get => TerrainData?.MaterialName; set { var nd = TerrainData; if (nd == null) return; nd.MaterialName = value; } }

        /// <summary>
        /// 远景 DAG 叶子降采样步长 (texel)。该值参与 DAG hash, 下次加载/离线 cook 自动判定是否重烘。
        /// </summary>
        [Category("VgDag")]
        public int VgDagStep { get => TerrainData?.VgDagStep ?? 2; set { var nd = TerrainData; if (nd == null) return; nd.VgDagStep = value; } }

        /// <summary>
        /// 地形显示模式: Normal=实体 / WireFrame=线框 / Both=实体叠线框。
        /// 纯转发到 TtTerrainSystem.ShowMode, 由 TtPatch.OnGatherVisibleMeshes_OnRenderThread 每帧消费, 改动立即生效。
        /// </summary>
        [Category("Debug")]
        public TtTerrainSystem.EShowMode ShowMode
        {
            get => Terrain?.ShowMode ?? TtTerrainSystem.EShowMode.Normal;
            set { if (Terrain != null) Terrain.ShowMode = value; }
        }

        /// <summary>
        /// LOD 调试开关: 打开后每个可见 patch 在地表上方 LODGridHeightOffset 处画
        /// 边界 LineList 调试线 (patch 边界白色 / level 外框品红, 懒构建见
        /// TtPatch.EnsureDebugBorderMeshes), 用于观察 level/patch 分割。纯运行时开关。
        /// </summary>
        [Category("Debug")]
        public bool ShowLODGrid { get; set; } = false;
        /// <summary>边界调试线相对地表的抬升高度, 改动会触发调试线重建。</summary>
        [Category("Debug")]
        public float LODGridHeightOffset { get; set; } = 2.0f;

        /// <summary>
        /// 打开 level 网格视图 (只读): 展示所有 level 的活动/加载/bake 状态,
        /// 左键 cell 或右键菜单 Goto 把场景视口相机拉过去。
        /// 承载位属性, 值不使用。
        /// </summary>
        [Category("Debug")]
        [EGui.Controls.PropertyGrid.TtPGPopupButtonEditor(
            ButtonText = "Levels", DrawMethodName = nameof(DrawLevelGridPopup),
            PopupTitle = "Terrain Levels###TerrainLevelGridPopup",
            Tooltip = "打开 Level 网格视图: 左键 cell 跳转, 右键 cell 弹出菜单")]
        public int LevelGridAction { get; set; }

        void DrawLevelGridPopup()
        {
            TtTerrainLevelGridEditor.Draw(this);
        }
        #endregion

        /// <summary>
        /// 当前已构建地形的参数签名 (TickLogic 每帧比对, 变了就重建)。
        /// </summary>
        string mBuiltTerrainSignature;
        /// <summary>重建防重入: 上一轮重建没完成时跳过本轮。</summary>
        bool mRebuilding = false;

        string GetTerrainSignature()
        {
            var nd = TerrainData;
            if (nd == null)
                return null;
            return $"{nd.PatchSide}|{nd.PatchSize}|{nd.NumOfLevelX}|{nd.NumOfLevelZ}|{nd.ActiveLevel}|{nd.MipLevels}|{nd.LODRangeFloat?.Count}|{nd.PgcName?.Address}|{nd.MaterialName?.Address}";
        }

        /// <summary>
        /// Details 里改了结构级参数 (PgcName/尺寸/Patch*/MipLevels/MaterialName/ActiveLevel) 时,
        /// 在主循环发现并重建地形。之所以用签名比对而不是写在 setter 里: 与 EnsureMaterialTextures
        /// 同一原因 —— PG 编辑子对象属性/undo 回写的宿主不是本节点, 不会调到节点上任何 setter。
        /// 重建是异步的 (Pgc 加载/材质加载), 期间旧地形继续渲染, 构建完成后原子替换引用。
        /// </summary>
        void CheckTerrainRebuild()
        {
            var sig = GetTerrainSignature();
            if (sig == null || sig == mBuiltTerrainSignature)
                return;
            if (mRebuilding)
                return;
            mBuiltTerrainSignature = sig; // 先记新签名防每帧重触发
            _ = RebuildTerrainAsync();
        }

        /// <summary>当前已构建地形使用的 PgcName.Address (用于检测换 PGC 时清缓存目录)</summary>
        string mBuiltPgcAddress;
        readonly object mCacheWriteLock = new object();
        internal object PgcBuildLock { get; } = new object();
        int mCacheGeneration;
        internal int CacheGeneration => System.Threading.Volatile.Read(ref mCacheGeneration);
        /// <summary>缓存目录以 NodeId 命名看不出归属, 首次落盘时在目录里放一份说明; 清目录后重写</summary>
        const string CacheInfoFileName = "_source.txt";
        bool mCacheInfoWritten;

        /// <summary>
        /// 本节点全部缓存产物的目录: cache/terrain/{NodeId}/, level 的 .trlvl / .trvgdag 直接平铺其下。
        /// NodeId 创建即有、全局唯一, 一个节点同一时刻只用一个 PGC, 所以不需要再按场景名/pgc 名分层;
        /// 缓存是否可用由文件内的 runtime hash 判定 (含 PGC 内容), 目录只负责隔离节点。
        /// 注意: 整目录复制场景会连 NodeId 一起复制, 两个场景将共用同一目录 —— 该问题应在复制场景时
        /// 重新生成 NodeId 解决, 不在缓存路径上防。
        /// </summary>
        public string GetCacheDir()
        {
            var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "terrain/";
            return IO.TtFileManager.CombinePath(root, NodeId.ToString());
        }

        void DeleteCacheDir()
        {
            lock (mCacheWriteLock)
            {
                // 与最终落盘使用同一把锁: 清理前失效旧代次, 旧任务即便晚完成也不能再发布缓存。
                System.Threading.Interlocked.Increment(ref mCacheGeneration);
                mCacheInfoWritten = false;
                if (Levels != null)
                    foreach (var level in Levels)
                        level?.CancelBakeTask();
                try
                {
                    var dir = GetCacheDir();
                    if (IO.TtFileManager.DirectoryExists(dir))
                    {
                        IO.TtFileManager.DeleteDirectory(dir);
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Info,
                            $"TerrainNode pgc changed, removed stale cache dir: {dir}");
                    }
                }
                catch (Exception ex)
                {
                    // 其他进程正读取时可能无法删除; 旧文件仍受内容 hash 门禁, 旧代次也已失效。
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning,
                        $"TerrainNode remove stale pgc cache dir failed: {ex.Message}");
                }
            }
        }

        /// <summary>独立临时文件写完再发布, 防编辑器与 worker 同时写出半截 XND; 旧代次禁止覆盖新缓存。</summary>
        internal bool TryWriteCache(int generation, string file, Action<string> writer, Thread.Async.TtAsyncTaskToken cancelToken = null)
        {
            lock (mCacheWriteLock)
            {
                if (mDisposed || generation != CacheGeneration || cancelToken?.TaskState == Thread.Async.EAsyncTaskState.Canceled)
                    return false;
                // XND 约定小写路径, 临时文件与最终文件须保持同一约定 (含大小写敏感平台)。
                file = file.ToLower();
                var temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    writer(temporary);
                    if (!System.IO.File.Exists(temporary) || new System.IO.FileInfo(temporary).Length == 0)
                        throw new System.IO.IOException($"Terrain cache write failed: {file}");
                    if (cancelToken?.TaskState == Thread.Async.EAsyncTaskState.Canceled)
                        return false;
                    // SaveXnd 只刷新临时路径; 发布时也必须刷新正式路径的 F2M 映射。
                    IO.TtRes2Memory.OnBeforeWriteFile(file);
                    try
                    {
                        System.IO.File.Move(temporary, file, true);
                    }
                    finally
                    {
                        IO.TtRes2Memory.OnAfterWriteFile(file);
                    }
                    EnsureCacheInfo();
                    return true;
                }
                finally
                {
                    if (System.IO.File.Exists(temporary))
                        System.IO.File.Delete(temporary);
                }
            }
        }

        /// <summary>
        /// 写缓存目录说明 (所属场景/节点/PGC/level 规模), 只给人看, 加载不读它。
        /// 调用方须持 mCacheWriteLock; 写失败只记日志, 不影响缓存本身。
        /// </summary>
        void EnsureCacheInfo()
        {
            if (mCacheInfoWritten)
                return;
            try
            {
                var scene = ParentScene?.AssetName;
                var sceneText = scene != null ? $"{scene.Name} ({scene.RNameType})" : IO.TtCookContext.Current?.Asset ?? "(unknown)";
                var nd = TerrainData;
                var pgc = nd?.PgcName;
                var text = "TitanEngine terrain cache: .trlvl / .trvgdag in this directory belong to the terrain node below.\n"
                    + $"scene: {sceneText}\n"
                    + $"node: {NodeName}\n"
                    + $"nodeId: {NodeId}\n"
                    + $"pgc: {(pgc != null ? $"{pgc.Name} ({pgc.RNameType})" : "(none)")}\n"
                    + $"levels: {nd?.NumOfLevelX} x {nd?.NumOfLevelZ}\n"
                    + $"written by: {System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name} pid {Environment.ProcessId} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                System.IO.File.WriteAllText(IO.TtFileManager.CombinePath(GetCacheDir(), CacheInfoFileName), text);
                mCacheInfoWritten = true;
            }
            catch (Exception ex)
            {
                // 编辑器与 worker 可能同时写; 下次落盘再试
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning,
                    $"TerrainNode write cache info failed: {ex.Message}");
            }
        }

        async System.Threading.Tasks.Task RebuildTerrainAsync()
        {
            if (mRebuilding)
                return;
            mRebuilding = true;
            try
            {
                // 旧资源引用先存下, 构建期间旧地形继续渲染 (TickLogic/OnDraw 用旧引用),
                // 全部构建成功后才在同步块内原子替换发布 (见 BuildTerrainCore 末尾)
                var oldTerrain = Terrain;
                var oldLevels = Levels;
                var oldStreaming = LevelStreaming;
                var oldMgr = TerrainMaterialIdManager;

                // 换了 PGC: 本节点缓存目录整体作废。新旧缓存同目录, 必须在新地形构建 (及其流式加载写缓存)
                // 之前清 —— 构建完再删会把新生成的 .trlvl 一起删掉; 清理同时失效旧代次, 阻止旧任务回写。
                var oldPgcAddress = mBuiltPgcAddress;
                var curPgcAddress = TerrainData?.PgcName?.Address;
                if (oldPgcAddress != null && oldPgcAddress != curPgcAddress)
                    DeleteCacheDir();

                // 强制 EnsureMaterialTextures 在新 manager 上重建贴图数组 (签名可能相同而提前 return)
                mBuiltTexDiffuse = null;
                mBuiltTexNormal = null;

                if (await BuildTerrainCore() == false)
                {
                    // 失败时保留旧地形继续渲染, 签名不回滚也不重试 (避免每帧重建风暴),
                    // 用户再次修改参数后才会重新尝试
                    Profiler.Log.WriteLine<Profiler.TtLogCategory>(Profiler.ELogTag.Error,
                        "TerrainNode rebuild failed, keep old terrain.");
                    return;
                }

                // 新地形已发布, 旧资源延迟到这里再清 (旧 streaming 已 IsStreaming=false,
                // 其在飞任务完成后会自行释放新建的 LevelData, 见 LevelStreaming.CreateLevelData)
                oldStreaming.Dispose();
                if (oldLevels != null)
                {
                    foreach (var i in oldLevels)
                        i.LevelData?.Dispose();
                }
                oldTerrain?.Dispose();
                oldMgr?.Cleanup();
            }
            catch (Exception ex)
            {
                // 注意: TtTask<bool> 的 await 会把内部异常重新抛出, 这里是兑底
                Profiler.Log.WriteLine<Profiler.TtLogCategory>(Profiler.ELogTag.Error,
                    $"TerrainNode rebuild failed: {ex.ToString()}");
            }
            finally
            {
                mRebuilding = false;
            }
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, GamePlay.Scene.TtNodeData data, GamePlay.Scene.EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtTerrainData == null)
            {
                data = new TtTerrainData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            if (await BuildTerrainCore() == false)
                return false;

            mBuiltTerrainSignature = GetTerrainSignature();

            SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);

            IsAcceptShadow = true;

            return true;
        }

        /// <summary>
        /// 节点级 cook (TtNode.CookAsset 重写): 地形节点是场景里唯一有 cook 产物的节点 ——
        /// PGC 高度场 → .trlvl 缓存 → 各 level 的 VGDag bake 落盘 .trvgdag。
        /// </summary>
        public override async Thread.Async.TtTask<bool> CookAsset()
        {
            return await CookTerrainAsync();
        }

        /// <summary>
        /// 离线 cook 入口 (仅 TitanCMD 的 Cook/CookServer): 对完整 level 表走两轮,
        /// 第 1 轮逐块生成/校验 .trlvl, 第 2 轮再逐块校验/补烘 .trvgdag (每块后台实际结束后验证产物)。
        /// DAG 再慢或失败, 第 1 轮落盘的 .trlvl 也已可供运行期流式加载。
        /// 不依赖相机活动窗口或“已有任务”计数。已烘过的 level (缓存 hash 命中)
        /// 不会重复烘, 所以重复执行 cook 是幂等的。
        /// 各阶段与等待期间都会写进度日志 (每 10 秒一条心跳), 避免 cook 时无法判断是否卡死。
        /// 重活在 AsyncIO / TPools 上跑, 主线程只等待; 超时与卡死由 CookServer 按进度上报判定,
        /// 这里只响应 context 的协作式取消。
        /// 返回 false 表示构建失败、产物校验失败或已取消。
        /// </summary>
        /// <remarks>
        /// cook 期间 ForceBakeVgDag = true, 让 level 加载钩子里的 BakeVgDag 绕过
        /// 运行期自动 cook 开关 (editor.jscfg AutoCookTerrainVgDag, 默认关);
        /// finally 还原, 防止 cook 完成后运行期 bake 一直被强制打开。
        /// </remarks>
        public async Thread.Async.TtTask<bool> CookTerrainAsync()
        {
            // 编辑器只能提交给 worker; 即使误调节点 API, 也绝不在主编辑器全量加载地形。
            var entryName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
            var context = IO.TtCookContext.Current;
            if (!string.Equals(entryName, "TitanCMD", StringComparison.OrdinalIgnoreCase) || context == null)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error,
                    "Terrain cook requires TitanCMD Cook/CookServer and a CookDriver context.");
                return false;
            }
            if (ForceBakeVgDag || mRebuilding || mDisposed || TerrainData == null)
                return false;
            ForceBakeVgDag = true;
            try
            {
                return await CookTerrainCoreAsync(context);
            }
            catch (OperationCanceledException)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Warning,
                    $"TerrainNode cook cancelled: node={NodeId}, stage={context.LastStage}");
                return false;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error,
                    $"TerrainNode cook failed: node={NodeId}, {ex}");
                return false;
            }
            finally
            {
                // 取消/失败后等后台真正退出再结束 job, 不用 CancelTask 提前回池。
                if (Levels != null)
                {
                    foreach (var level in Levels)
                        level?.CancelBakeTask();
                    while (HasRunningBake())
                        await WaitCookTick();
                }
                ForceBakeVgDag = false;
            }
        }

        bool HasRunningBake()
        {
            if (Levels != null)
                foreach (var level in Levels)
                    if (level?.IsBakeRunning == true)
                        return true;
            return false;
        }

        // 每拍一个引擎信号量, Timer 只唤醒; 续体回到引擎上下文, 不 await CLR Task.Delay。
        static async Thread.Async.TtTask<bool> WaitCookTick()
        {
            var semaphore = Thread.TtSemaphore.CreateSemaphore(1);
            using (var timer = new System.Threading.Timer(_ => semaphore.Release(), null, 100, System.Threading.Timeout.Infinite))
                await semaphore.Await();
            return true;
        }

        async Thread.Async.TtTask<bool> CookTerrainCoreAsync(IO.TtCookContext context)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (Levels == null && !await BuildTerrainCore())
                return false;
            if (Levels == null || Levels.Length == 0)
                return false;
            // 全缓存命中时不会有新的落盘, 这里先给已有目录补上归属说明
            lock (mCacheWriteLock)
            {
                if (IO.TtFileManager.DirectoryExists(GetCacheDir()))
                    EnsureCacheInfo();
            }

            // 全量遍历只在 TitanCMD 进程执行。一次仅持有一个 level 的临时数据, 不把全图 GPU 资源常驻。
            // 两轮: 先让全图 .trlvl 就绪 (保证跑图流式加载), 再逐块烘 DAG; 进度按 2N 个产物步骤计, 非耗时估计。
            int total = Levels.Length;
            long lastBeatMs = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                bool vgDagPass = pass == 1;
                string passName = vgDagPass ? ".trvgdag" : ".trlvl";
                int completed = 0;
                foreach (var level in Levels)
                {
                    if (level == null || context.IsCancellationRequested || mDisposed)
                        return false;
                    while (level.IsBakeRunning)
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        await WaitCookTick();
                    }
                    context.ReportStage($"terrain 第 {pass + 1}/2 轮 {passName} level {completed + 1}/{total}: X{level.LevelX}_Y{level.LevelZ} | 开始");
                    context.ReportProgress((float)(pass * total + completed) / (2 * total));
                    using (var data = new TtTerrainLevelData())
                    {
                        await data.PrepareCookDataAsync(level, completed, total, vgDagPass);
                        while (level.IsBakeRunning)
                        {
                            if (context.IsCancellationRequested || mDisposed)
                            {
                                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Warning,
                                    $"TerrainNode cook cancelled: node={NodeId}, level={level.LevelX},{level.LevelZ}");
                                level.CancelBakeTask();
                                return false;
                            }
                            if (watch.ElapsedMilliseconds - lastBeatMs >= 10000)
                            {
                                lastBeatMs = watch.ElapsedMilliseconds;
                                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info,
                                    $"TerrainNode cook: dag completed={completed}/{total}, baking={level.LevelX},{level.LevelZ}, elapsed={lastBeatMs / 1000.0:F1}s");
                            }
                            await WaitCookTick();
                        }
                        data.ReportCookStage(vgDagPass ? "校验已落盘的 .trlvl / .trvgdag 及各自 hash" : "校验已落盘的 .trlvl 及 hash");
                        // 没有任务不等于成功: 本轮产物必须存在且 hash 对上, 否则 job 明确失败。
                        if (!data.ValidateCookArtifacts())
                        {
                            Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error,
                                $"TerrainNode cook: missing/invalid {passName}, node={NodeId}, level={level.LevelX},{level.LevelZ}");
                            return false;
                        }
                    }
                    completed++;
                    context.ReportStage($"terrain 第 {pass + 1}/2 轮 {passName} 已完成 {completed}/{total}: X{level.LevelX}_Y{level.LevelZ} 已验证");
                    context.ReportProgress((float)(pass * total + completed) / (2 * total));
                    // 即使全是缓存命中也让出主循环, 给 worker 处理 cancel/心跳的机会。
                    await WaitCookTick();
                }
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info,
                    $"TerrainNode cook pass {pass + 1}/2 ({passName}) done: node={NodeId}, verified={completed}/{total}, elapsed={watch.ElapsedMilliseconds} ms");
            }
            if (context.IsCancellationRequested)
                return false;
            Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info,
                $"TerrainNode cook done: node={NodeId}, levels={total}, elapsed={watch.ElapsedMilliseconds} ms");
            return true;
        }

        /// <summary>
        /// 地形构建主体, InitializeNode 与 CheckTerrainRebuild 共用。
        /// 全程只写局部变量, 成功后在末尾同步块内原子发布全部字段; 失败时不碰任何成员字段,
        /// 旧地形继续渲染。返回 false 表示构建失败 (原因已写日志)。
        /// </summary>
        async Thread.Async.TtTask<bool> BuildTerrainCore()
        {
            var trData = TerrainData;
            if (trData == null)
                return false;

            try
            {
                var terrain = new TtTerrainSystem();
                if (await terrain.Initialize(trData.MaterialName, trData.MipLevels) == false)
                {
                    terrain.Dispose();
                    return false;
                }

                var gridSize = trData.PatchSize / TexSizePerPatch;
                var levelSize = trData.PatchSize * trData.PatchSide;
                var newLevels = new TtTerrainLevel[trData.NumOfLevelZ, trData.NumOfLevelX];
                for (int i = 0; i < trData.NumOfLevelZ; i++)
                {
                    for (int j = 0; j < trData.NumOfLevelX; j++)
                    {
                        newLevels[i, j] = new TtTerrainLevel();
                        newLevels[i, j].CreateLevel(j, i, trData.PatchSide, this);
                    }
                }

                int numOfActiveLevel = 1 + trData.ActiveLevel * 2;
                var newActiveLevels = new TtTerrainLevel[numOfActiveLevel, numOfActiveLevel];

                var newMorphRange = new Vector2[trData.LODRangeFloat.Count];

                if (trData.PgcName == null)
                    trData.PgcName = RName.GetRName("template/emptyterrain.pgc", RName.ERNameType.Engine);

                var terrainGen = Procedure.TtPgcAsset.LoadAsset(trData.PgcName);
                {
                    var pgcText = IO.TtFileManager.ReadAllText(trData.PgcName.Address);
                    var refAssets = terrainGen.GetAMeta().RefAssetRNames;
                    refAssets.Sort();
                    foreach (var i in refAssets)
                    {
                        pgcText += i.ToString();//todo:Asset Hash
                    }
                    mTerrainGenHash = Hash160.CreateHash160(pgcText);
                }
                var hmNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.TtMaterialIdMapNode;
                UTerrainMaterialIdManager newMgr = null;
                if (hmNode != null)
                {
                    newMgr = hmNode.MaterialIdManager;
                    // 先发布 manager 才能确保 EnsureMaterialTextures 用新的 (覆盖槽位补齐 + 首次 BuildSRV)
                    TerrainMaterialIdManager = newMgr;
                    EnsureMaterialTextures(true);
                    await hmNode.SureMaterialResources();
                }

                if (mDisposed)
                {
                    // 构建期间节点被销毁: 丢弃新建资源, 不发布
                    terrain.Dispose();
                    newMgr?.Cleanup();
                    return false;
                }

                float bvSX = 0;
                float bvSZ = 0;
                float bvEX = trData.PatchSize * trData.PatchSide * trData.NumOfLevelX;
                float bvEZ = trData.PatchSize * trData.PatchSide * trData.NumOfLevelZ;

                // —— 原子发布: 同一同步块内替换全套字段, 外部要么见旧全套要么见新全套 ——
                Terrain = terrain;
                Levels = newLevels;
                ActiveLevels = newActiveLevels;
                MorphRange = newMorphRange;
                mLodDirty = true;   // 全新 patch 集, 强制重算 CurrentLOD
                mTerrainGen = terrainGen;
                if (newMgr != null)
                    TerrainMaterialIdManager = newMgr;
                GridSize = gridSize;
                LevelSize = levelSize;
                mBuiltPgcAddress = trData.PgcName?.Address;
                this.BoundVolume.mLocalAABB.Minimum.SetValue(bvSX, -0.5f, bvSZ);
                this.BoundVolume.mLocalAABB.Maximum.SetValue(bvEX, 0.5f, bvEZ);
                // 重置活动中心, 让下一帧 SetActiveCenter 用新数组重新填充 ActiveLevels
                // (否则 CurrentActiveCenterLevel 还停在旧 cell, ActiveLevels 全 null 地形消失)
                CurrentActiveCenterLevel = new Vector2i(-1, -1);

                return true;
            }
            catch (Exception ex)
            {
                // 非 TtTask<bool> 的 await 链异常在此捕获, 保证不外渗 (Rebuild 侧另有兑底日志)
                Profiler.Log.WriteLine<Profiler.TtLogCategory>(Profiler.ELogTag.Error,
                    $"TerrainNode build terrain failed: {ex.ToString()}");
                return false;
            }
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

        // LOD 分圈分配: 遍历所有 ActiveLevels 的全部 patch, 由近及远逐圈降级 CurrentLOD;
        // 成本高 (圈数×补丁数 AABB 测试), 由 OnTickLogic 按 EyeCenter 移动阈值节流调用。
        // 不写 MorphRange: 分界环必须逐帧跟随当前相机 (见 UpdateMorphRange)
        public void UpdateLODAssignment(List<float> radius, DVector3 eyePos)
        {
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.SetLODLevel(int.MaxValue);   // 未分配哨兵, 由近及远逐圈占位
            }

            var sphere = new DBoundingSphere(eyePos, 0);
            for (int i = 0; i < radius.Count - 1; i++)
            {
                sphere.Radius = radius[i];

                // 逐 patch 降级判定 (原 SphereCover 顺带做的事):
                // 尚未分配且与本圈球相交的 patch 降为当前圈 LOD
                foreach (var lv in ActiveLevels)
                {
                    if (lv == null || lv.LevelData == null)
                        continue;
                    lv.LevelData.AssignLODLevel(in sphere, i);
                }
            }
        }

        // MorphRange + cbuffer: 必须每帧跟随当前 EyeCenter, 不能随 CurrentLOD 一起节流 ——
        // 分界环定义在以 eye 为中心的世界空间, 节流会让 morph 环钉在旧位置、
        // 越阈值瞬间跳变。成本只是圈数个标量 + 一次 cbuffer 上传。
        // MorphRange.X 直接取上一圈半径, 不再用 SphereCover 逐 patch 统计实际覆盖距离
        public void UpdateMorphRange(List<float> radius, DVector3 eyePos)
        {
            double morphStart = 0;
            var sphere = new DBoundingSphere(eyePos, 0);
            for (int i = 0; i < radius.Count - 1; i++)
            {
                sphere.Radius = radius[i];
                this.MorphRange[i].Y = (float)sphere.Radius;
                this.MorphRange[i].X = (float)MathHelper.Lerp(morphStart, sphere.Radius, 0.8f);

                morphStart = sphere.Radius;
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
            // worker 的离线 cook 独占 PGC 图; 不并行运行相机驱动的流式加载/重建。
            if (ForceBakeVgDag)
                return base.OnTickLogic(args);
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                // Details 里改了地形结构参数 (PgcName/尺寸/Patch*/MipLevels/MaterialName/ActiveLevel)
                // 时在这里被发现并异步重建地形, 没变化时仅做字符串比较
                CheckTerrainRebuild();

                // Details 里改了材质贴图的话在这里被发现并重建贴图数组, 没变化时仅做引用比较
                EnsureMaterialTextures();

                EyeCenter = args.Policy.DefaultCamera.GetPosition();
                EyeLocalCenter = args.Policy.DefaultCamera.GetLocalPosition();

                if (SetActiveCenter(in EyeCenter))
                {
                    args.World.SetCameraOffset(in EyeCenter, args.Policy.DefaultCamera);
                    UpdateAfterCameraOffset();
                }

                // MorphRange/cbuffer 每帧跟随当前 EyeCenter (分界环不能滞后, 否则越阈值跳变);
                // CurrentLOD 分配按阈值节流, 只在移动超阈值(或置脏)时重算
                var lodRadius = (NodeData as TtTerrainData).LODRangeFloat;
                UpdateMorphRange(lodRadius, EyeCenter - this.Location);
                if (mLodDirty || DVector3.DistanceSquared(in EyeCenter, in mLodLastEye) > mLodUpdateDistSq)
                {
                    mLodDirty = false;
                    mLodLastEye = EyeCenter;
                    mLodUpdateDistSq = (double)PatchSize * 0.1f * (PatchSize * 0.1f);
                    UpdateLODAssignment(lodRadius, EyeCenter - this.Location);
                }

                LevelStreaming.Tick(TtEngine.Instance.ElapsedSecond);
            }   
            return base.OnTickLogic(args);
        }
        // 渲染空间的 patch 原点基准 = 节点世界位置 - world.CameraOffset。
        // Patch.StartPosition 只在该基准变化时刷新, 平时保持只读供渲染线程读。
        internal DVector3 PatchRenderOffset = DVector3.Zero;

        void UpdateAfterCameraOffset()
        {
            var world = GetWorld();
            if (world == null)
                return;
            PatchRenderOffset = this.Placement.AbsTransform.mPosition - world.CameraOffset;
            if (ActiveLevels == null)
                return;
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.UpdateAfterCameraOffset(this, in PatchRenderOffset);
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
            // 节点被移动时渲染空间基准同步变化 (StartPosition 消费方是 shader, 必须跟手)
            UpdateAfterCameraOffset();
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
            UpdateAfterCameraOffset();
        }
        public override void OnGatherVisibleMeshes_OnRenderThread(GamePlay.TtWorld.TtVisParameter rp)
        {
            Thread.TtContextThread.CheckOnRenderThread();
            // 渲染帧开头拍 LOD 快照: 之后整帧内选网格/打包 instance 读到的都是同一份,
            // 逻辑线程的重算 (可含哨兵中态) 对渲染侧不可见; 原子读+钳制保证单值合法
            if (ActiveLevels != null)
            {
                foreach (var lv in ActiveLevels)
                {
                    if (lv == null)
                        continue;
                    lv.LevelData?.PublishRenderLOD_OnRenderThread();
                }
            }
            UpdateCameralOffset(rp.World);
            //todo: QTree culling?
            //foreach (var i in ActiveLevels)
            //{
            //    if (i == null)
            //        continue;
            //    i.LevelData?.OnGatherVisibleMeshes_OnRenderThread(rp);
            //}

            FrustumCull(rp, VisiblePatches);
            // CurrentLOD/MorphRange 已在 OnTickLogic 按阈值节流预计算, 这里只消费

            foreach (var i in VisiblePatches)
            {
                i.OnGatherVisibleMeshes_OnRenderThread(rp);
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
                    i.ActiveRVTs_OnRenderThread();
                }
                Terrain.UpdateRVT_OnRenderThread();
            }

            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.LevelData?.PlantManager.OnGatherVisibleMeshes_OnRenderThread(rp);
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
        public TtTerrainLevel GetLevel(in DVector3 pos)
        {
            if (Levels == null)
                return null;
            var idxLevel = GetLevelIndex(in pos);
            // 边界用数组自身维度而非 NodeData 实时值: 参数改了但重建还没完成时, 两者会不一致
            if (idxLevel.X < 0 || idxLevel.Y < 0 || idxLevel.X >= Levels.GetLength(1) || idxLevel.Y >= Levels.GetLength(0))
            {
                return null;
            }
            return Levels[idxLevel.Y, idxLevel.X];
        }
        Vector2i CurrentActiveCenterLevel = new Vector2i(-1, -1);
        /// <summary>当前活动窗口中心 level 索引 (SetActiveCenter 最近一次生效的位置), 编辑器 level 网格视图用红框标注</summary>
        public Vector2i ActiveCenterLevelIndex => CurrentActiveCenterLevel;
        public bool SetActiveCenter(in DVector3 pos)
        {
            if (Levels == null || ActiveLevels == null)
                return false;
            var idxLevel = GetLevelIndex(in pos);
            if (idxLevel.X < 0 || idxLevel.Y < 0 || idxLevel.X >= Levels.GetLength(1) || idxLevel.Y >= Levels.GetLength(0))
            {
                return false;
            }

            if (idxLevel == CurrentActiveCenterLevel)
            {
                return false;
            }
            CurrentActiveCenterLevel = idxLevel;
            mLodDirty = true;   // 活动窗口重建, patch 集变化, 强制重算 CurrentLOD
            Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Info, "TerrainLevelGrid",
                $"SetActiveCenter: ({CurrentActiveCenterLevel.X},{CurrentActiveCenterLevel.Y}) pos=({pos.X:0.#},{pos.Z:0.#})");

            var curLevel = Levels[idxLevel.Y, idxLevel.X];
            curLevel.LoadLevelData(LevelStreaming, true);

            // 活动半径从 ActiveLevels 数组维度反推, 而不是读 ActiveLevel 实时值:
            // 参数改了但新数组还没发布时, 两者不一致会越界
            int activeRadius = (ActiveLevels.GetLength(0) - 1) / 2;
            int xMin = idxLevel.X - activeRadius;
            int xMax = idxLevel.X + activeRadius;

            int zMin = idxLevel.Y - activeRadius;
            int zMax = idxLevel.Y + activeRadius;

            int ActiveNum = ActiveLevels.GetLength(0);
            for (int i = 0; i < ActiveNum; i++)
            {
                int z = i + zMin;
                if (z < 0 || z >= Levels.GetLength(0))
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
                    if (x < 0 || x >= Levels.GetLength(1))
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

        #region LevelData 状态
        /// <summary>
        /// 某个 level 当前的 LevelData 生命周期状态:
        /// Loaded(激活) / Loading(加载中) / PendingDestroy(等待销毁) / Destroying(正在销毁) / Unloaded(已彻底销毁)。
        /// 读的是 level 上的状态字段, 由 TtLevelStreaming 在各转移点维护。
        /// </summary>
        public ELevelDataState GetLevelDataState(TtTerrainLevel level)
        {
            if (level == null)
                return ELevelDataState.Unloaded;
            return level.DataState;
        }
        public ELevelDataState GetLevelDataState(int levelX, int levelZ)
        {
            if (Levels == null)
                return ELevelDataState.Unloaded;
            if (levelX < 0 || levelX >= Levels.GetLength(1) || levelZ < 0 || levelZ >= Levels.GetLength(0))
                return ELevelDataState.Unloaded;
            return Levels[levelZ, levelX].DataState;
        }

        /// <summary>
        /// 确保指定 level 的 LevelData 就绪 (goto / 编辑器直接打开目标 level 时用)。
        /// 按 DataState 分别处理, 而不是无条件 force 重建:
        ///   Loaded         → 直接返回, 不碰
        ///   PendingDestroy → 从延迟卸载队列捞回来直接复用 (LevelData 仍然有效, 省一次完整重建)
        ///   Destroying     → 阻塞等 Dispose 结束, 然后按已销毁重新加载
        ///   Loading        → 阻塞等在飞加载落地 (bForce 语义)
        ///   Unloaded       → 直接重新加载
        /// 返回 true 表示返回时 level.LevelData != null。
        /// 注意: Loading/Unloaded 路径的 bForce 会在当前线程同步构建整块 LevelData。
        /// </summary>
        public bool EnsureLevelDataReady(TtTerrainLevel level)
        {
            if (level == null || LevelStreaming == null)
                return false;

            // 状态会被流式 Tick 迁移, 这里按最新状态重判几轮:
            // 等待销毁 → 被 Tick 摘走并开始销毁 → 等到销毁结束变已销毁 → 重新加载
            for (int retry = 0; retry < 3; retry++)
            {
                switch (level.DataState)
                {
                    case ELevelDataState.Loaded:
                        return true;

                    case ELevelDataState.PendingDestroy:
                        // 从延迟卸载队列捞回来直接复用, 不重建
                        if (LevelStreaming.ReviveUnloadingLevel(level))
                            return true;
                        // 没能捞回来说明 Tick 刚把它从队列里摘走, 重新读状态
                        continue;

                    case ELevelDataState.Destroying:
                        LevelStreaming.WaitDestroyComplete(level);
                        if (level.DataState == ELevelDataState.Destroying)
                        {
                            // 超时: Dispose 还没结束, 此时去重排加载会拿到一个即将被释放的 LevelData
                            Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "TerrainLevelGrid",
                                $"EnsureLevelDataReady({level.LevelX},{level.LevelZ}): 等待销毁超时");
                            return false;
                        }
                        continue;

                    case ELevelDataState.Loading:
                        // PushStreamingLevel 的 bForce 分支会在锁外等待在飞任务落地
                        level.LoadLevelData(LevelStreaming, true);
                        return level.LevelData != null;

                    case ELevelDataState.Unloaded:
                    default:
                        // 已彻底销毁: 直接重新加载
                        level.LoadLevelData(LevelStreaming, true);
                        return level.LevelData != null;
                }
            }

            return level.LevelData != null;
        }
        #endregion

        public double GetAltitude(double x, double z, bool forceLoad = true)
        {
            if (Levels == null)
                return float.MinValue;
            var xGrid = (int)((x - Placement.AbsTransform.Position.X) / LevelSize);
            var zGrid = (int)((z - Placement.AbsTransform.Position.Z) / LevelSize);
            if (xGrid < 0 || xGrid >= Levels.GetLength(1) || zGrid < 0 || zGrid >= Levels.GetLength(0))
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
            if (Levels == null)
                return float.MinValue;
            var xGrid = (int)(x / LevelSize);
            var zGrid = (int)(z / LevelSize);
            if (xGrid < 0 || xGrid >= Levels.GetLength(1) || zGrid < 0 || zGrid >= Levels.GetLength(0))
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

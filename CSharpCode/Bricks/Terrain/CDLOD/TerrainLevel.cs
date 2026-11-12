using EngineNS.Bricks.Terrain.CDLOD.VGDag;
using EngineNS.Bricks.VirtualTexture;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// 一个 level 的 LevelData 生命周期状态。
    /// 由 TtLevelStreaming 在加载/卸载的每个转移点维护, 供 goto / 编辑器网格视图等
    /// 外部调用方判定"这个 level 现在能不能直接用, 还是需要等一等/重新加载"。
    /// </summary>
    public enum ELevelDataState
    {
        /// <summary>已彻底销毁: LevelData == null, 也不在加载队列里</summary>
        Unloaded = 0,
        /// <summary>加载中: 已入 StreamingLevels, 异步任务在飞, LevelData 尚为 null</summary>
        Loading = 1,
        /// <summary>激活: LevelData 可用</summary>
        Loaded = 2,
        /// <summary>等待销毁: 在 UnloadingLevels 队列里倒计时, LevelData 仍然有效</summary>
        PendingDestroy = 3,
        /// <summary>正在销毁: Dispose 进行中, 结束后变 Unloaded</summary>
        Destroying = 4,
    }

    public class TtTerrainLevel
    {
        public TtTerrainNode Node;
        public int LevelX = 0;
        public int LevelZ = 0;
        public DVector3 StartPosition;

        /// <summary>
        /// LevelData 生命周期状态。用 int + Volatile 而不是普通字段:
        /// 状态会被主线程(流式 Tick / 编辑)与 AsyncIO 线程(异步加载完成回写)共同读写。
        /// </summary>
        int mDataState = (int)ELevelDataState.Unloaded;
        /// <summary>当前 LevelData 状态 (激活 / 加载中 / 等待销毁 / 正在销毁 / 已彻底销毁)</summary>
        public ELevelDataState DataState
        {
            get { return (ELevelDataState)System.Threading.Volatile.Read(ref mDataState); }
        }
        /// <summary>状态转移统一由 TtLevelStreaming 维护, 其它地方不要直接改</summary>
        internal void SetDataState(ELevelDataState state)
        {
            System.Threading.Volatile.Write(ref mDataState, (int)state);
        }
        public int PatchSide
        {
            get
            {
                return Node.PatchSide;
            }
        }
        public int TexSizePerPatch
        {
            get
            {
                return Node.TexSizePerPatch;
            }
        }
        public TtTerrainLevelData LevelData
        {
            get;
            internal set;
        }
        public void ReleaseLevel(TtLevelStreaming streaming)
        {
            streaming.PushUnloadLevel(this);
        }
        public void CreateLevel(int levelX, int levelZ, int patchSide, TtTerrainNode node)
        {
            Node = node;
            LevelX = levelX;
            LevelZ = levelZ;

            var patchSize = node.PatchSize;
            StartPosition.X = ((double)patchSide * patchSize) * LevelX;
            StartPosition.Z = ((double)patchSide * patchSize) * LevelZ;
        }
        public void LoadLevelData(TtLevelStreaming streaming, bool bForce)
        {
            streaming.PushStreamingLevel(this, bForce);
        }

        public UTerrainMaterialIdManager GetMaterialIdManager()
        {
            return Node?.TerrainMaterialIdManager;
        }

        /// <summary>当前在飞的 bake 任务与取消 token (CancelBakeTask 用); 无在飞任务时为 default/null。
        /// 放在 level (而非 LevelData) 上: LevelData 可能被流式卸载, 取消状态不能跟着丢</summary>
        Thread.Async.TtTask<TtVgDagAsset> mBakeTask;
        Thread.Async.TtAsyncTaskToken mBakeCancelToken;

        /// <summary>是否有 DAG bake 正在后台执行 (含已被请求取消但后台还没退出的)</summary>
        public bool IsBakeRunning => mBakeTask.TaskState == Thread.Async.ETaskStatus.Pending;

        /// <summary>
        /// 请求取消在飞的 DAG bake (关地图/卸载 level 时调):
        /// 把取消 token 置 Canceled, 后台线程在下一个检查点提前退出, 并走正常完成路径返回 null
        /// (AddWaitTask 回调里 DirectResult 为 null, 不会挂接资产, task 随后被 collector 正常 Dispose)。
        /// 幂等, 无在飞任务时安全空操作。
        /// 注意: 故意不调 task.CancelTask() —— Cancelling 态会让 collector 立即 Dispose task,
        /// 而后台 bake 可能还在跑, 数秒后 TrySetResult 会写到已回池的 TtTaskData 上污染复用者;
        /// token 方式让后台先正常完成、再走 Dispose, 无此竞态。
        /// </summary>
        public void CancelBakeTask()
        {
            if (mBakeCancelToken != null)
                mBakeCancelToken.TaskState = Thread.Async.EAsyncTaskState.Canceled;
        }

        /// <summary>bake 发起方 (UTerrainLevelData.BakeVgDag) 在任务创建后登记任务与 token</summary>
        internal void SetBakeTask(Thread.Async.TtTask<TtVgDagAsset> task, Thread.Async.TtAsyncTaskToken token)
        {
            mBakeTask = task;
            mBakeCancelToken = token;
        }
    }

    public partial class TtTerrainLevelData : IDisposable
    {
        public TtTerrainLevel Level;
        public TtPatch[,] TiledPatch;
        public NxRHI.TtTexture HeightMap;
        public NxRHI.TtSrView HeightMapSRV;
        public NxRHI.TtTexture WaterHMap;
        public NxRHI.TtSrView WaterHMapSRV;
        public NxRHI.TtTexture NormalMap;
        public NxRHI.TtSrView NormalMapSRV;
        /// <summary>
        /// 材质 ID 图的纹理对象。与 HeightMap / NormalMap 一致: 只持引用,
        /// 不在 Dispose 里单独释放, 靠 SRV 的引用计数。材质笔刷的局部上传需要它。
        /// </summary>
        public NxRHI.TtTexture MaterialIdMap;
        public NxRHI.TtSrView MaterialIdMapSRV;
        public float HeightMapMinHeight;
        public float HeightMapMaxHeight;
        
        public int HeightfieldWidth;
        public int HeightfieldHeight;
        public PhyHeightFieldSample[] PxHeightfieldSamples;
        public float PxHeightfieldScale;
        public Bricks.PhysicsCore.TtPhyHeightfield PhyHeightfield { get; protected set; }
        public Bricks.PhysicsCore.TtPhyActor PhyActor { get; protected set; }
        public float RemainUnloadTime = 10.0f;
        public UTerainPlantManager PlantManager = new UTerainPlantManager();
        public Bricks.VirtualTexture.TtRVT GetHeightmapRVT()
        {
            return Level.Node.Terrain.HeightmapRVT.RegRVT(HeightMapSRV);
        }
        public Bricks.VirtualTexture.TtRVT GetWaterHeightmapRVT()
        {
            return Level.Node.Terrain.HeightmapRVT.RegRVT(WaterHMapSRV);
        }
        public Bricks.VirtualTexture.TtRVT GetNormalmapRVT()
        {
            return Level.Node.Terrain.NormalmapRVT.RegRVT(NormalMapSRV);
        }
        public Bricks.VirtualTexture.TtRVT GetMaterialIdRVT()
        {
            return Level.Node.Terrain.MaterialIdRVT.RegRVT(MaterialIdMapSRV);
        }
        ~TtTerrainLevelData()
        {
            Dispose();
        }
        public void Dispose()
        {
            PhyActor?.AddToScene(null);
            PhyActor = null;

            if (TiledPatch != null)
            {
                foreach (var i in TiledPatch)
                {
                    i.Dispose();
                }
                TiledPatch = null;
            }

            CoreSDK.DisposeObject(ref PlantManager);

            CoreSDK.DisposeObject(ref HeightMapSRV);
            CoreSDK.DisposeObject(ref NormalMapSRV);
            CoreSDK.DisposeObject(ref MaterialIdMapSRV);

            // SourceHeightMap 是 CreateFromBuffer 里 Clone 出来的, 生命周期归本对象,
            // 必须释放, 否则每次 level streaming 卸载都会漏一份 4MB 的高度缓冲。
            CoreSDK.DisposeObject(ref SourceHeightMap);

            // 材质 ID 的 CPU 副本是纯托管 byte[], 置 null 就行; 同时让 IsMaterialIdEditable
            // 变 false, 避免已卸载 level 上的 undo 命令去碰一个已释放的纹理。
            SourceMaterialIdMap = null;
            mBaseMaterialIdMap = null;
        }
        public TtTerrainNode GetTerrainNode()
        {
            return Level.Node;
        }
        public async System.Threading.Tasks.Task CreateLevelData(TtTerrainLevel level, bool bForce)
        {
            Level = level;
            
            if (bForce)
            {
                BuildLevelDataFromPGC(Level.Node.GetNodeData<TtTerrainNode.TtTerrainData>());
            }
            else
            {
                await TtEngine.Instance.EventPoster.Post((state) =>
                {
                    BuildLevelDataFromPGC(Level.Node.GetNodeData<TtTerrainNode.TtTerrainData>());
                    return true;
                }, Thread.Async.EAsyncTarget.AsyncIO);
            }

            OnParentSceneChanged(null, Level.Node.ParentScene);
        }
        //bool bTryLoad = false;
        protected bool LoadLevelFromCache(string file, in Hash160 testHash)
        {
            //if (bTryLoad == false)
            //    return false;
            var patchSide = Level.PatchSide;
            var terrainGen = Level.Node.TerrainGen;
            var IdMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.TtMaterialIdMapNode;
            if (IdMapNode!=null)
                IdMapNode.InitProcedure(terrainGen.AssetGraph);

            var hMap = Procedure.TtBufferComponent.CreateInstance(Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
            var norMap = Procedure.TtBufferComponent.CreateInstance(Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<Vector3, Procedure.FFloat3Operator>>(1, 1, 1));
            var idMap = Procedure.TtBufferComponent.CreateInstance(Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
            Procedure.TtBufferComponent waterMap = null;
            Procedure.TtBufferComponent transform = null;
            Procedure.TtBufferComponent plants = null;

            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return false;

                var attr = xnd.RootNode.TryGetAttribute("Desc");
                if (attr.IsValidPointer == false)
                    return false;

                Hash160 hash;
                using (var ar = attr.GetReader(null))
                {
                    ar.Read(out hash);
                }
                if (hash != testHash)
                    return false;

                var node = xnd.RootNode.TryGetChildNode("HeightMap");
                if (node.IsValidPointer == false)
                    return false;
                hMap.LoadXnd(node, Hash160.Emtpy);

                node = xnd.RootNode.TryGetChildNode("NormalMap");
                if (node.IsValidPointer == false)
                    return false;
                norMap.LoadXnd(node, Hash160.Emtpy);

                node = xnd.RootNode.TryGetChildNode("MatIdMap");
                if (node.IsValidPointer == false)
                    return false;
                idMap.LoadXnd(node, Hash160.Emtpy);

                node = xnd.RootNode.TryGetChildNode("WaterMap");
                if (node.IsValidPointer)
                {
                    waterMap = Procedure.TtBufferComponent.CreateInstance(Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
                    waterMap.LoadXnd(node, Hash160.Emtpy);
                }

                node = xnd.RootNode.TryGetChildNode("PlantTransform");
                if (node.IsValidPointer)
                {
                    transform = Procedure.TtBufferComponent.CreateInstance(Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<FTransform, Procedure.FTransformOperator>>(1, 1, 1));
                    transform.LoadXnd(node, Hash160.Emtpy);
                }

                node = xnd.RootNode.TryGetChildNode("PlantInfo");
                if (node.IsValidPointer)
                {
                    plants = Procedure.TtBufferComponent.CreateInstance(Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<Vector2i, Procedure.FInt2Operator>>(1, 1, 1));
                    plants.LoadXnd(node, Hash160.Emtpy);
                }
                

                // 先尝试加载 .trvgdag (与 .trlvl 同目录同 hash): 已就绪则下方 bake 钩子
                // 会因 CachedVgDag != null 跳过, 避免重复烘
                var vgFile = file.Substring(0, file.Length - ".trlvl".Length) + ".trvgdag";
                CachedVgDag = LoadVgDagFile(vgFile, testHash);
                
                var hashLocal = testHash;
                CreateFromBuffer(hMap, norMap, waterMap, idMap, transform, plants, (finalHMap) =>
                {
                    // 缓存命中路径同样触发补烘: 旧缓存 (目录变更/首版无 DAG) 的 .trlvl 命中时
                    // 没有经过 BuildLevelDataFromPGC, 必须在这里给 DAG 一次机会;
                    // .trvgdag 已存在且 hash 匹配时上面已加载, BakeVgDag 内部会跳过。                    
                    BakeVgDag(vgFile, hashLocal, finalHMap);
                });

                node = xnd.RootNode.TryGetChildNode("GrassInfo");
                if (node.IsValidPointer)
                {
                    var nodeCount = node.GetNumOfNode();
                    for(uint i=0; i<nodeCount; i++)
                    {
                        var subNode = node.GetNode(i);
                        if (subNode.IsValidPointer == false)
                            continue;
                        var gAtt = subNode.TryGetAttribute("GrassData");
                        var grassData = new UTerrainGrass();
                        if (!gAtt.IsValidPointer)
                            continue;

                        using (var gReader = gAtt.GetReader(grassData))
                        {
                            IO.ISerializer serializer;
                            gReader.Read(out serializer, null);
                            grassData = (UTerrainGrass)serializer;
                        }

                        var gBuffer = Procedure.TtBufferComponent.CreateInstance(Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
                        gBuffer.LoadXnd(subNode, Hash160.Emtpy);

                        UpdateGrass(grassData, gBuffer);
                    }
                }
            }

            return true;
        }

        private void CreateFromBuffer(Procedure.TtBufferComponent hMap, Procedure.TtBufferComponent norMap, Procedure.TtBufferComponent waterMap,
            Procedure.TtBufferComponent idMap, Procedure.TtBufferComponent transform, Procedure.TtBufferComponent plants,
            Action<Procedure.TtBufferComponent> onFinalHeightfield = null)
        {
            // 先把手绘的高度覆盖层叠回去, 再建立任何派生数据 —— SourceHeightMap / GPU
            // 高度图 / 法线 / patch AABB / physx heightfield 全部基于叠加后的数据。
            // 放在 IsTerrainEditSupported 判断之外, 非编辑器态也生效 —— 游戏里看到的就是
            // 编辑后的地形。
            //
            // 返回的是副本: BuildLevelDataFromPGC 传进来的 hMap 就是 PGC 的 result buffer,
            // 它稍后会被 SaveLevelToCache 写进 .trlvl 基底缓存, 原地叠加会把 delta 烤进基底,
            // 下次加载再叠一次 → 高度翻倍。
            var overlaidHMap = ApplyHeightOverlayIfAny(hMap, norMap);
            if (overlaidHMap != null)
            {
                hMap = overlaidHMap;
            }

            // 地形编辑需要 CPU 侧的 float 高度源数据 (从 PhyHeightfieldSamples 的 short 反推会累积量化误差)。
            // 三条加载路径 (LoadLevelFromCache / BuildLevelDataFromPGC / BuildEmptyLevelData) 都汇聚到这里。
            //
            // 必须 Clone 而不能直接存引用: BuildLevelDataFromPGC 末尾会调
            // terrainGen.AssetGraph.BufferCache.ResetCache(), 它对缓存里每个 buffer 调 Dispose()
            // 把底层 SuperPixels 释放掉, 但 UBufferComponent 对象本身还活着 —— 直接存引用
            // 会拿到一个 SuperPixels == null 的空壳, 下次 GetPixel 就是 NullReferenceException。
            //
            // 只 Clone 高度图 (1024²×float ≈ 4MB/level)。法线是纯派生数据, 编辑时从高度现算并
            // 直接上传, 不再落 CPU 缓存 (省 12MB/level, 也避开了法线缓存与高度不同步)。
            // 仅编辑器态保留, 避免运行时内存回归。
            if (IsTerrainEditSupported)
            {
                SourceHeightMap = hMap.Clone();
            }

            UpdateHeightMap(hMap);

            UpdateNormalMap(norMap);

            UpdateWaterMap(waterMap);

            // 材质 ID 覆盖层同样要在建纹理之前叠回去。和高度一样必须拿副本:
            // 传进来的 idMap 就是 PGC 的 result buffer, 随后会被 SaveLevelToCache 写进 .trlvl,
            // 原地叠加会把手绘材质烤进基底。
            var overlaidIdMap = ApplyMaterialIdOverlayIfAny(idMap);
            if (overlaidIdMap != null)
            {
                idMap = overlaidIdMap;
            }

            UpdateMaterialIdMap(idMap);

            UpdatePlants(transform, plants);

            UpdateAABB(hMap, waterMap);

            // 远景 VGDag bake 等需要"叠加 overlay 后的最终高度场"的消费者挂在这里:
            // 此时编辑态/运行态都已拿到同一份最终数据, 且叠加副本尚未 Dispose。
            onFinalHeightfield?.Invoke(hMap);

            // 叠加副本的生命周期只到这里: 上面需要它的地方要么已经上传到 GPU,
            // 要么已经 Clone 成了 SourceHeightMap。
            CoreSDK.DisposeObject(ref overlaidHMap);
            CoreSDK.DisposeObject(ref overlaidIdMap);
        }

        public void UpdateAABB(Procedure.TtBufferComponent hMap, Procedure.TtBufferComponent waterMap)
        {
            if (TiledPatch == null)
                return;
            var patchSide = Level.PatchSide;
            for (int i = 0; i < patchSide; i++)
            {
                for (int j = 0; j < patchSide; j++)
                {
                    TiledPatch[i, j].UpdateAABB(hMap, waterMap);
                }
            }
        }
        public void UpdateHeightMap(Procedure.TtBufferComponent hMap)
        {
            var patchSide = Level.PatchSide;
            TiledPatch = new TtPatch[patchSide, patchSide];
            for (int i = 0; i < patchSide; i++)
            {
                for (int j = 0; j < patchSide; j++)
                {
                    if (TiledPatch[i, j] == null)
                        TiledPatch[i, j] = new TtPatch();
                    TiledPatch[i, j].Initialize(this, j, i, hMap);
                    // 新建 patch 不再依赖每帧 Tick 补 StartPosition, 创建时立即用当前渲染空间基准刷一次
                    TiledPatch[i, j].UpdateAfterCameraOffset(Level.Node, in Level.Node.PatchRenderOffset);
                }
            }

            hMap.GetRangeUnsafe<float, Procedure.FFloatOperator>(out HeightMapMinHeight, out HeightMapMaxHeight);
            // 编辑会话期间高度纹理的编码基准必须冻结, 否则一旦雕刻改变 min/max,
            // 整层已上传的 texel 编码会同时失效。详见 TerrainEdit.cs 的 BeginEditSession。
            ApplyFrozenHeightRangeIfEditing();
            float HeightfieldMidHeight = (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
            PxHeightfieldScale = 0.1f;//0.1f精度为分米
            HeightfieldWidth = hMap.Width;
            HeightfieldHeight = hMap.Height;
            PxHeightfieldSamples = new PhyHeightFieldSample[hMap.Width * hMap.Height];
            for (int i = 0; i < HeightfieldHeight; i++)
            {
                for (int j = 0; j < HeightfieldWidth; j++)
                {
                    float height = hMap.GetPixel<float>(i, j);
                    float localHeight = height - HeightfieldMidHeight;
                    PxHeightfieldSamples[i * HeightfieldWidth + j].height = (short)(localHeight / PxHeightfieldScale);//(short)((height * (float)short.MaxValue) / maxHeight);
                    PxHeightfieldSamples[i * HeightfieldWidth + j].materialIndex0 = 0;
                    PxHeightfieldSamples[i * HeightfieldWidth + j].materialIndex1 = 0;
                }
            }

            InitPhysics(Level);

            // AssetName 必须在 SRV 首次进入 RegRVT 之前就位: RegRVT 会把注册时的名字
            // 冻结进 TtRVT.TextureName, 空名会让所有 level 命中同一条按名复用分支、
            // 共用同一个 UniqueTexID 和 atlas 层, 后加载者把先加载者的高度顶掉
            HeightMapSRV = hMap.CreateAsHeightMapTexture2D(out HeightMap, HeightMapMinHeight, HeightMapMaxHeight, EPixelFormat.PXF_R16_FLOAT);
            HeightMapSRV.SetDebugName("HeightMapSRV");
            HeightMapSRV.AssetName = RName.GetRName($"@Height_{this.GetTerrainNode().TerrainName}_{Level.LevelX}_{Level.LevelZ}@", RName.ERNameType.Transient);
            var rvt = HeightMapSRV?.Rvt;
            if (rvt != null)
            {
                rvt.UpdateTexture(HeightMapSRV);
            }
        }

        public void UpdateWaterMap(Procedure.TtBufferComponent waterMap)
        {
            if (waterMap == null)
                return;
            // 同上: WaterHMapSRV 之前从未设过 AssetName, 是空名错误复用的重灾区
            WaterHMapSRV = waterMap.CreateAsHeightMapTexture2D(out WaterHMap, HeightMapMinHeight, HeightMapMaxHeight, EPixelFormat.PXF_R16_FLOAT);
            WaterHMapSRV.SetDebugName("WaterHMapSRV");
            WaterHMapSRV.AssetName = RName.GetRName($"@Water_{this.GetTerrainNode().TerrainName}_{Level.LevelX}_{Level.LevelZ}@", RName.ERNameType.Transient);
            var rvt = WaterHMapSRV?.Rvt;
            if (rvt != null)
            {
                rvt.UpdateTexture(WaterHMapSRV);
            }
        }
        
        public void UpdateNormalMap(Procedure.TtBufferComponent norMap)
        {
            var norImage = new Bricks.Procedure.TtImage2D();
            norImage.Initialize(norMap.Width, norMap.Height,
                norMap as Procedure.TtSuperBuffer<Vector3, Procedure.FFloat3Operator>,
                null, 0);

            NormalMapSRV = norImage.CreateRGBA8Texture2DAsNormal(out NormalMap); //terrainGen.mResultNormalImage.CreateRGBA8Texture2DAsNormal();            
            NormalMapSRV.SetDebugName("NormalMapSRV");
            NormalMapSRV.AssetName = RName.GetRName($"@Normal_{this.GetTerrainNode().TerrainName}_{Level.LevelX}_{Level.LevelZ}@", RName.ERNameType.Transient);
            var rvt = NormalMapSRV?.Rvt;
            if (rvt != null)
            {
                rvt.UpdateTexture(NormalMapSRV);
            }
        }
        public void UpdateMaterialIdMap(Procedure.TtBufferComponent idMap)
        {
            if (idMap == null)
                return;

            var idMapImage = new Bricks.Procedure.TtImage2D();
            idMapImage.Initialize(idMap.Width, idMap.Height,
                idMap as Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>,
                null,
                null,
                null);

            MaterialIdMapSRV = idMapImage.CreateRGBA8Texture2D(out MaterialIdMap, false);
            MaterialIdMapSRV.SetDebugName("MaterialIdMapSRV");
            MaterialIdMapSRV.AssetName = RName.GetRName($"@MtlID_{this.GetTerrainNode().TerrainName}_{Level.LevelX}_{Level.LevelZ}@", RName.ERNameType.Transient);
            var rvt = MaterialIdMapSRV?.Rvt;
            if (rvt != null)
            {
                rvt.UpdateTexture(MaterialIdMapSRV);
            }

            // 材质笔刷需要 CPU 侧的 byte ID 副本 (1MB/level)。只在编辑器态建,
            // 且必须建在 idMap 已经叠过覆盖层之后 —— 它是“当前生效的 ID”而不是基底。
            if (IsTerrainEditSupported)
            {
                CaptureSourceMaterialIdMap(idMap);
            }
        }
        public void UpdatePlants(Procedure.TtBufferComponent transform, Procedure.TtBufferComponent plants)
        {
            var terrainGen = Level.Node.TerrainGen;
            var IdMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.TtMaterialIdMapNode;
            if (transform != null && plants != null)
            {
                for (int i = 0; i < plants.Width; i++)
                {
                    ref var t = ref transform.GetPixel<FTransform>(i, 0, 0);
                    ref var p = ref plants.GetPixel<Vector2i>(i, 0, 0);

                    var material = IdMapNode.MaterialIdManager.MaterialIdArray[p.X];
                    var plt = material.Plants[p.Y];
                    PlantManager.AddPlant(this.Level.Node, in Level.StartPosition, plt, in t, plants.Width);
                }
            }
        }
        //////////////////////////////////////////////////
        //static bool GAdded = false;
        //////////////////////////////////////////////////
        public void UpdateGrass(Procedure.Node.TtGrassNode node)
        {
            for(int i=0; i<node.Inputs.Count; i++)
            {
                var grassData = node.GrassDefines[i].GrassData;
                var buffer = node.GetResultBuffer(i);
                UpdateGrass(grassData, buffer);
            }
        }
        public void UpdateGrass(UTerrainGrass grassData, Procedure.TtBufferComponent buffer)
        {
            if (buffer == null)
                return;
            var patchSide = Level.PatchSide;
            float weightMin, weightMax;
            buffer.GetRangeUnsafe<float, Procedure.FFloatOperator>(out weightMin, out weightMax);
            for (int patchIdxY = 0; patchIdxY < patchSide; patchIdxY++)
            {
                for (int patchIdxX = 0; patchIdxX < patchSide; patchIdxX++)
                {
                    if (TiledPatch[patchIdxY, patchIdxX] == null)
                        continue;
                    var patchOffset = new DVector3(
                        patchIdxX * GetTerrainNode().PatchSize + Level.StartPosition.X,
                        Level.StartPosition.Y,
                        patchIdxY * GetTerrainNode().PatchSize + Level.StartPosition.Z);
                    TiledPatch[patchIdxY, patchIdxX].GrassManager.AddGrass(patchOffset, grassData, buffer, weightMin, weightMax);
                }
            }
        }
        //public unsafe void UpdateGrass(Procedure.UBufferConponent grass)
        //{
        //    if (grass == null || grass.Width <= 0)
        //        return;

        //    var terrainGen = Level.Node.TerrainGen;
        //    var idMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.TtMaterialIdMapNode;
        //    var grassAdr = grass.GetSuperPixelAddress(0, 0, 0);
        //    float minScale = float.MaxValue;
        //    float maxScale = float.MinValue;
        //    //////////////////////////////////////////////////
        //    //DVector3 maxPos = new DVector3(double.MinValue, double.MinValue, double.MinValue);
        //    //DVector3 minPos = new DVector3(double.MaxValue, double.MaxValue, double.MaxValue);
        //    //////////////////////////////////////////////////
        //    for (int i=0; i<grass.Width; i++)
        //    {
        //        FGrassTransformData* tData = (FGrassTransformData*)(grassAdr + i * sizeof(FGrassTransformData));
        //        var scale = tData->Transform.Scale.Y;
        //        if(scale < minScale)
        //            minScale = scale;
        //        if(scale > maxScale)
        //            maxScale = scale;
        //        //////////////////////////////////////////////////
        //        //if (maxPos.X < tData->Transform.Position.X)
        //        //    maxPos.X = tData->Transform.Position.X;
        //        //if(maxPos.Y < tData->Transform.Position.Y)
        //        //    maxPos.Y = tData->Transform.Position.Y;
        //        //if(maxPos.Z < tData->Transform.Position.Z)
        //        //    maxPos.Z = tData->Transform.Position.Z;
        //        //if(minPos.X > tData->Transform.Position.X)
        //        //    minPos.X = tData->Transform.Position.X;
        //        //if(minPos.Y > tData->Transform.Position.Y)
        //        //    minPos.Y = tData->Transform.Position.Y;
        //        //if(minPos.Z > tData->Transform.Position.Z)
        //        //    minPos.Z = tData->Transform.Position.Z;
        //        //////////////////////////////////////////////////
        //    }
        //    for(int i=0; i<grass.Width; i++)
        //    {
        //        //////////////////////////////////////////////////
        //        //if (i > 1 || GAdded)
        //        //{
        //        //    GAdded = true;
        //        //    break;
        //        //}
        //        //////////////////////////////////////////////////
        //        ref var p = ref grass.GetPixel<FGrassTransformData>(i, 0, 0);
        //        var levelSize = Level.PatchSide * Level.Node.PatchSize;
        //        if((p.Transform.Position.X < Level.StartPosition.X) || (p.Transform.Position.X > Level.StartPosition.X + levelSize) ||
        //           (p.Transform.Position.Z < Level.StartPosition.Z) || (p.Transform.Position.Z > Level.StartPosition.Z + levelSize))
        //                continue;

        //        var material = idMapNode.MaterialIdManager.MaterialIdArray[(int)p.MaterialIdx];
        //        var grs = material.Grasses[(int)p.GrassIdx];
        //        grs.MinScale = minScale;
        //        grs.MaxScale = maxScale;
        //        // to patch
        //        var patchIdxX = (int)((p.Transform.Position.X - Level.StartPosition.X) / GetTerrainNode().PatchSize);
        //        var patchIdxY = (int)((p.Transform.Position.Z - Level.StartPosition.Z) / GetTerrainNode().PatchSize);

        //        //////////////////////////////////////////////////
        //        //if (patchIdxX != 0 || patchIdxY != 1)
        //        //    continue;
        //        //patchIdxX = 1;
        //        //patchIdxY = 1;
        //        //p.Transform.Position = new DVector3(patchIdxX * Level.Node.PatchSize, 0, patchIdxY * Level.Node.PatchSize) + Level.StartPosition;
        //        //p.Transform.Scale = new Vector3(minScale);
        //        //p.Transform.Quat = Quaternion.Identity;
        //        //if (i == 1)
        //        //{
        //        //    p.Transform.Position += new DVector3(40, 0, 0);
        //        //    p.Transform.Scale = new Vector3(maxScale);
        //        //    p.Transform.Quat = Quaternion.RotationAxis(Vector3.Up, (float)(System.Math.PI * 0.25));
        //        //}
        //        //////////////////////////////////////////////////

        //        if (Level.PatchSide <= patchIdxX || Level.PatchSide <= patchIdxY)
        //            continue;
        //        var patch = TiledPatch[patchIdxY, patchIdxX];
        //        var patchOffset = new DVector3(
        //            patchIdxX * GetTerrainNode().PatchSize + Level.StartPosition.X,
        //            Level.StartPosition.Y,
        //            patchIdxY * GetTerrainNode().PatchSize + Level.StartPosition.Z);
        //        patch.GrassManager.AddGrass(patchOffset, grs, p.Transform, grass.Width);
        //    }
        //}
        /// <summary>本 level bake 出的远景 cluster DAG (未加载/未 bake 时为 null)。
        /// 与 .trlvl 同 hash 缓存; 将来由远景绘制节点消费 (TODO M3: 接入 TtVgDagFarTerrain)。</summary>
        public TtVgDagAsset CachedVgDag;

        /// <summary>
        /// level 缓存目录: cache/terrain/场景AssetName/TerrainNode/&lt;NodeId&gt;/&lt;pgcName&gt;/
        /// 同一个 pgc 被不同场景/不同 TerrainNode 使用时各自独立, 不会互相覆盖。
        /// 场景未保存 (AssetName 为空) 时回退到旧的 pgc-only 路径, 仅编译期兼容,
        /// 此时保存的缓存会在场景保存后失效 (hash 相同但目录不同, 最多浪费一次生成)。
        /// </summary>
        protected string GetLevelCacheDir(RName terrainName)
        {
            var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "terrain/";
            var scene = Level.Node.ParentScene;
            if (scene?.AssetName == null)
                return IO.TtFileManager.CombinePath(root, terrainName.Name);
            var dir = IO.TtFileManager.CombinePath(root, scene.AssetName.Name);
            dir = IO.TtFileManager.CombinePath(dir, "TerrainNode");
            dir = IO.TtFileManager.CombinePath(dir, Level.Node.NodeId.ToString());
            return IO.TtFileManager.CombinePath(dir, terrainName.Name);
        }

        /// <summary>
        /// 烘焙远景 DAG (异步 TtTask): 在 CreateFromBuffer 钩子线程同步拷出"叠加 overlay 后最终高度场"
        /// (随后该副本会被 Dispose), 真正的 QEM bake + .trvgdag 落盘在 TPools 线程池执行,
        /// 完成后经 AddWaitTask 回调 (引擎主循环) 把产物挂到 CachedVgDag。
        /// 地形打开不依赖 DAG 就绪: 消费端以 CachedVgDag == null 作为"远景尚未启动"的条件。
        /// </summary>
        protected void BakeVgDag(string vgFile, in Hash160 hash, Procedure.TtBufferComponent finalHMap)
        {
            if (CachedVgDag != null)
                return; // 已就绪 (缓存命中或重复触发), 不重复烘
            var nodeData = Level.Node.TerrainData;
            if (nodeData == null)
                return;
            // 运行期自动 cook DAG 开关 (editor.jscfg AutoCookTerrainVgDag, 默认关):
            // 单 level bake 可达秒级且占死线程池线程, 全图加载会拖垮运行期。
            // 离线 cook (TitanCMD Cook 命令 / 节点 CookAsset) 经 ForceBakeVgDag 绕过本开关
            if (Level.Node.ForceBakeVgDag == false)
            {
                var editorCfg = TtEngine.Instance.ConfigManager.GetConfig<EngineNS.Editor.TtEditorConfig>();
                if (editorCfg == null || editorCfg.AutoCookTerrainVgDag == false)
                    return;
            }
            int size = (int)finalHMap.Width;
            if ((int)finalHMap.Height != size)
                return; // 非方形高度场暂不支持 DAG bake
            int leafQuads = nodeData.VgDagLeafQuads;
            if (leafQuads < 1 || (leafQuads & (leafQuads - 1)) != 0)
                return; // 配置了非法的 VgDagLeafQuads (必须是 2 的幂), 跳过 bake 避免 bake 内 throw 打断 level 生成

            // 钩子线程同步拷贝: finalHMap 离开 CreateFromBuffer 后会被 Dispose,
            // 后台任务只能依赖这份副本。
            var hf = new float[size * size];
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                    hf[z * size + x] = finalHMap.GetPixel<float>(x, z);
            }

            // token 在这里创建并登记到 Level (取消状态跟 level 走, LevelData 卸载也不丢)
            var cancelToken = new Thread.Async.TtAsyncTaskToken();
            var bakeTaskLocal = BakeVgDagAsync(vgFile, hash, hf, size, leafQuads, nodeData.VgDagMaxErrorPerLevel, cancelToken);
            Level.SetBakeTask(bakeTaskLocal, cancelToken);
            // bake 完成后的挂接在 collector 回调统一处理 (引擎主循环线程):
            // level 可能在 bake 期间被流式卸载 (本对象被 task/闭包持有不会回收),
            // 赋值本身无害; 下次加载时从独立缓存文件读到。
            // 取消时 DirectResult 为 null (Cancelling 态), 天然跳过赋值。
            // 闭包捕获局部副本而非 Level 上的字段: 取消/重建会把字段覆盖,
            // 回调若读字段会在取消场景下 NRE; struct 副本持有同一 TtTaskData 引用, 安全。
            bakeTaskLocal.AddWaitTask((tsk) =>
            {
                if (bakeTaskLocal.DirectResult != null)
                    CachedVgDag = bakeTaskLocal.DirectResult;
            });
        }

        private async Thread.Async.TtTask<TtVgDagAsset> BakeVgDagAsync(string vgFile, Hash160 hash, float[] hf, int size, int leafQuads, float maxError, Thread.Async.TtAsyncTaskToken cancelToken)
        {
            // bake + 落盘同在 TPools 线程池完成 (不阻塞 level 生成/主线程);
            // token 关联到后台任务: CancelBakeTask 置 Canceled 后, bake 在下一个检查点返回 null,
            // 此时跳过落盘直接结束 (TrySetResult 对已 Cancelling 的 task 返回 false, 不会复活状态)。
            var asset = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                // preStep=2: 整层网格先结构化降采样 (面数 ÷4), 远景 LOD 无需全分辨率 QEM;
                // 引入的偏差 ≤ 1 texel 高度, 相对 maxError (默认 1600) 可忽略
                var result = TtVgClusterBaker.BakeFromLevel(hf, size, leafQuads, maxError, state, 2);
                if (result == null)
                    return null; // 被取消: 不落盘
                SaveVgDagFile(vgFile, hash, result);
                return result;
            }, Thread.Async.EAsyncTarget.TPools).WithToken(cancelToken); // QEM bake 单个可能耗时秒级, 不能占 AsyncIO 专用线程, 抛线程池并行执行 (bake 任务相互无依赖)
            return asset;
        }

        /// <summary>
        /// 大块字节写入 xnd attribute。不用 AuxWriter 的 byte[] 重载: 它用 UInt16 记长度 (≤64KB),
        /// DAG 资产远超, 必须 int 长度 + WritePtr 成对读写 (与 ReadBytesFromAttribute 配对)。
        /// </summary>
        protected static unsafe void WriteBytesToAttribute(XndAttribute attr, byte[] bytes)
        {
            using (var ar = attr.GetWriter((ulong)(sizeof(int) + bytes.Length)))
            {
                int len = bytes.Length;
                fixed (byte* p = bytes)
                {
                    ar.WritePtr(&len, sizeof(int));
                    ar.WritePtr(p, len);
                }
            }
        }

        protected static unsafe byte[] ReadBytesFromAttribute(XndAttribute attr)
        {
            using (var ar = attr.GetReader(null))
            {
                int len = 0;
                ar.ReadPtr(&len, sizeof(int));
                var bytes = new byte[len];
                if (len > 0)
                {
                    fixed (byte* p = bytes)
                    {
                        ar.ReadPtr(p, len);
                    }
                }
                return bytes;
            }
        }

        protected unsafe void SaveVgDagFile(string file, in Hash160 hash, TtVgDagAsset asset)
        {
            using (var xnd = new IO.TtXndHolder("VgDag", 0, 0))
            {
                using (var attr = xnd.NewAttribute("Desc", 0, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    using (var ar = attr.GetWriter(32))
                    {
                        ar.Write(hash);
                    }
                }
                using (var attr = xnd.NewAttribute("VgDagData", 0, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    WriteBytesToAttribute(attr, asset.ToBytes());
                }
                xnd.SaveXnd(file);
            }
        }

        /// <summary>DAG 未生成/hash 不匹配时返回 null —— 远景渲染以此判定未就绪。</summary>
        protected TtVgDagAsset LoadVgDagFile(string file, in Hash160 testHash)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;
                var attr = xnd.RootNode.TryGetAttribute("Desc");
                if (attr.IsValidPointer == false)
                    return null;
                Hash160 hash;
                using (var ar = attr.GetReader(null))
                {
                    ar.Read(out hash);
                }
                if (hash != testHash)
                    return null;
                attr = xnd.RootNode.TryGetAttribute("VgDagData");
                if (attr.IsValidPointer == false)
                    return null;
                return TtVgDagAsset.FromBytes(ReadBytesFromAttribute(attr));
            }
        }

        protected unsafe void SaveLevelToCache(string file, in Hash160 hash)
        {
            var terrainGen = Level.Node.TerrainGen;
            var hMap = terrainGen.AssetGraph.Root.GetResultBuffer("Height");
            var norMap = terrainGen.AssetGraph.Root.GetResultBuffer("Normal");
            var idMap = terrainGen.AssetGraph.Root.GetResultBuffer("MatId");
            var waterMap = terrainGen.AssetGraph.Root.GetResultBuffer("Water");
            var transform = terrainGen.AssetGraph.Root.GetResultBuffer("Transform");
            var plants = terrainGen.AssetGraph.Root.GetResultBuffer("Plants");

            var grassPin = terrainGen.AssetGraph.Root.FindPinIn("Grass");
            Procedure.Node.TtGrassNode linkedGrassNode = null;
            if (grassPin != null)
            {
                linkedGrassNode = terrainGen.AssetGraph.Root.GetInputNode(terrainGen.AssetGraph.Root.ParentGraph as Procedure.TtPgcGraph, grassPin) as Procedure.Node.TtGrassNode;
            }

            using (var xnd = new IO.TtXndHolder("TrLevel", 0, 0))
            {
                using (var attr = xnd.NewAttribute("Desc", 0, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    using (var ar = attr.GetWriter(24))
                    {
                        ar.Write(hash);
                    }
                }

                if (hMap != null)
                {
                    using (var node = xnd.NewNode("HeightMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        hMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (norMap != null)
                {
                    using (var node = xnd.NewNode("NormalMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        norMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (idMap != null)
                {
                    using (var node = xnd.NewNode("MatIdMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        idMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (waterMap != null)
                {
                    using (var node = xnd.NewNode("WaterMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        waterMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (transform != null)
                {
                    using (var node = xnd.NewNode("PlantTransform", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        transform.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (plants != null)
                {
                    using (var node = xnd.NewNode("PlantInfo", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        plants.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if(linkedGrassNode != null)
                {
                    using (var node = xnd.NewNode("GrassInfo", 0, 0))
                    {
                        for (int i = 0; i < linkedGrassNode.Inputs.Count; i++)
                        {
                            using (var subNode = xnd.NewNode("data", 0, 0))
                            {
                                var grassData = linkedGrassNode.GrassDefines[i].GrassData;
                                using (var gAtt = xnd.NewAttribute("GrassData", 0, 0))
                                {
                                    using (var gWriter = gAtt.GetWriter(0))
                                    {
                                        IO.SerializerHelper.Write(gWriter, grassData);
                                    }
                                    subNode.AddAttribute(gAtt);
                                }

                                var buffer = linkedGrassNode.GetResultBuffer(i);
                                buffer.SaveXnd(xnd, subNode, in Hash160.Emtpy);

                                node.AddNode(subNode);
                            }
                        }
                        xnd.RootNode.AddNode(node);
                    }
                }

                xnd.SaveXnd(file);
            }   
        }
        static Hash160 BuildPgcRuntimeHash(Hash160 baseHash, Procedure.TtPgcGraph graph,
            Procedure.TtPgcExecutionContext context)
        {
            var signature = new StringBuilder(baseHash.ToString());
            signature.Append('|').Append(BitConverter.DoubleToInt64Bits(context.WorldOrigin.X));
            signature.Append('|').Append(BitConverter.DoubleToInt64Bits(context.WorldOrigin.Z));
            signature.Append('|').Append(BitConverter.SingleToInt32Bits(context.GridCellSize.X));
            signature.Append('|').Append(BitConverter.SingleToInt32Bits(context.GridCellSize.Y));

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (!(graph.Nodes[i] is Procedure.Node.TtSplineRegionSamplerNode sampler))
                    continue;
                signature.Append('|').Append((int)sampler.QueryScope);
                signature.Append('|').Append(sampler.SplineName);
                signature.Append('|').Append(BitConverter.SingleToInt32Bits(sampler.InteriorSampleSpacing));
                signature.Append('|').Append(BitConverter.SingleToInt32Bits(sampler.BorderSampleSpacing));
                signature.Append('|').Append(sampler.Seed);
                signature.Append('|').Append(BitConverter.SingleToInt32Bits(sampler.Jitter));
                signature.Append('|').Append(sampler.ProjectOntoSplineSurface ? 1 : 0);
                signature.Append('|').Append(BitConverter.SingleToInt32Bits(sampler.BoundaryEpsilon));
                if (sampler.DensityFalloff != null)
                {
                    for (int pointIndex = 0; pointIndex < sampler.DensityFalloff.Count; pointIndex++)
                    {
                        var point = sampler.DensityFalloff[pointIndex];
                        signature.Append('|').Append(BitConverter.SingleToInt32Bits(point.Position.X));
                        signature.Append('|').Append(BitConverter.SingleToInt32Bits(point.Position.Y));
                        signature.Append('|').Append(BitConverter.SingleToInt32Bits(point.ControlPoint.X));
                        signature.Append('|').Append(BitConverter.SingleToInt32Bits(point.ControlPoint.Y));
                    }
                }

                var splineQuery = new Procedure.FSplineQuery(sampler.QueryScope, sampler.SplineName);
                if (context.TryGetSplineRegion(in splineQuery, sampler.BorderSampleSpacing,
                    out var region, out var error))
                {
                    signature.Append('|').Append(region.SourceRevision);
                    for (int pointIndex = 0; pointIndex < region.BoundaryPoints.Count; pointIndex++)
                    {
                        var point = region.BoundaryPoints[pointIndex];
                        signature.Append('|').Append(BitConverter.SingleToInt32Bits(point.X));
                        signature.Append('|').Append(BitConverter.SingleToInt32Bits(point.Y));
                        signature.Append('|').Append(BitConverter.SingleToInt32Bits(point.Z));
                    }
                }
                else
                {
                    signature.Append('|').Append(error);
                }
            }
            return Hash160.CreateHash160(signature.ToString());
        }

        protected void BuildLevelDataFromPGC(TtTerrainNode.TtTerrainData nodeData)
        {
            RName terrainName = nodeData.PgcName;
            if (terrainName == null)
            {
                BuildEmptyLevelData(nodeData.LevelSideX, nodeData.LevelSideZ);
                return;
            }
            var patchSide = Level.PatchSide;
            var terrainGen = Level.Node.TerrainGen;
            var worldOrigin = Level.StartPosition;
            var pgcContext = new Procedure.TtPgcExecutionContext(Level.Node.GetWorld(),
                in worldOrigin, Level.Node.GridSize);
            var runtimeHash = BuildPgcRuntimeHash(Level.Node.TerrainGenHash,
                terrainGen.AssetGraph, pgcContext);

            var dir = GetLevelCacheDir(terrainName);
            IO.TtFileManager.SureDirectory(dir);
            var lvlFile = $"{dir}/X{Level.LevelX}_Y{Level.LevelZ}.trlvl";
            var vgFile = $"{dir}/X{Level.LevelX}_Y{Level.LevelZ}.trvgdag";
            if (terrainName != null)
            {
                if (LoadLevelFromCache(lvlFile, runtimeHash) == true)
                {
                    // .trvgdag 加载与缺失时的补烘触发都已在 LoadLevelFromCache 内完成
                    return;
                }
            }
            
            var IdMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.TtMaterialIdMapNode;
            //if (IdMapNode != null)
            //    await IdMapNode.SureMaterialResources();

            var noise1 = terrainGen.AssetGraph.FindFirstNode("NoisePerlin1") as Procedure.Node.TtNoisePerlin;
            if(noise1!=null)
            {
                noise1.StartPosition = Level.StartPosition;
                noise1.StartPosition.X -= noise1.Border;
                noise1.StartPosition.Z -= noise1.Border;
            }
            //var noise2 = terrainGen.AssetGraph.FindFirstNode("NoisePerlin2") as Procedure.Node.TtNoisePerlin;
            //if (noise2 != null)
            //{
            //    noise2.StartPosition = Level.StartPosition;
            //    noise2.StartPosition.X -= noise1.Border;
            //    noise2.StartPosition.Z -= noise1.Border;
            //}

            var trans = terrainGen.AssetGraph.FindFirstNode("Plants") as Procedure.Node.TtTransformBuilder;
            if (trans != null)
            {
                trans.FinalRandomSeed = trans.RandomSeed + Level.LevelZ * Level.Node.NumOfLevelX + Level.LevelX;
                trans.GridSize = Level.Node.GridSize;
                trans.Offset = Level.StartPosition;
            }

            var root = terrainGen.AssetGraph.Root;
            terrainGen.Compile(root, pgcContext);
            var hMap = root.GetResultBuffer("Height");
            var norMap = root.GetResultBuffer("Normal") as Procedure.TtSuperBuffer<Vector3, Procedure.FFloat3Operator>;
            if (norMap == null)
            {
                var creator = Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<Vector3, Procedure.FFloat3Operator>>(nodeData.LevelSideX, nodeData.LevelSideZ, 1);
                norMap = Procedure.TtBufferComponent.CreateInstance(creator) as Procedure.TtSuperBuffer<Vector3, Procedure.FFloat3Operator>;
            }
            var idMap = root.GetResultBuffer("MatId") as Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>;
            if (idMap==null)
            {
                var creator = Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>>(nodeData.LevelSideX, nodeData.LevelSideZ, 1);
                idMap = Procedure.TtBufferComponent.CreateInstance(creator) as Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>;
            }
            var waterMap = root.GetResultBuffer("Water") as Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>;
            var transform = root.GetResultBuffer("Transform") as Procedure.TtSuperBuffer<FTransform, Procedure.FTransformOperator>;
            var plants = root.GetResultBuffer("Plants") as Procedure.TtSuperBuffer<Vector2i, Procedure.FInt2Operator>;
            CreateFromBuffer(hMap, norMap, waterMap, idMap, transform, plants, (finalHMap) =>
            {
                // bake 异步化: 钩子线程只拷高度场, .trlvl 立即落盘, DAG 烘完后另存 .trvgdag
                BakeVgDag(vgFile, runtimeHash, finalHMap);
            });
            var grassPin = root.FindPinIn("Grass");
            if (grassPin != null)
            {
                var tagNode = root.GetInputNode(root.ParentGraph as Procedure.TtPgcGraph, grassPin) as Procedure.Node.TtGrassNode;
                if(tagNode != null)
                    UpdateGrass(tagNode);
            }
            
            SaveLevelToCache(lvlFile, runtimeHash);
            terrainGen.AssetGraph.BufferCache.ResetCache();
        }
        protected void BuildEmptyLevelData(int xSize, int ySize)
        {
            var creator = Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>>(xSize, ySize, 1);
            var hMap = Procedure.TtBufferComponent.CreateInstance(creator);
            var waterMap = Procedure.TtBufferComponent.CreateInstance(creator);
            creator = Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<Vector3, Procedure.FFloat3Operator>>(xSize, ySize, 1);
            var norMap = Procedure.TtBufferComponent.CreateInstance(creator);
            creator = Procedure.TtBufferCreator.CreateInstance<Procedure.TtSuperBuffer<float, Procedure.FFloatOperator>>(xSize, ySize, 1);
            var idMap = Procedure.TtBufferComponent.CreateInstance(creator);

            CreateFromBuffer(hMap, norMap, waterMap, idMap, null, null);
        }
        #region Px & Collide
        public unsafe void InitPhysics(TtTerrainLevel level)
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            int texSize = level.Node.TexSizePerPatch * level.Node.PatchSide;
            fixed (PhyHeightFieldSample* pPixelData = &PxHeightfieldSamples[0])
            {
                PhyHeightfield = pc.CookHeightfield(texSize, texSize, pPixelData, 0, false);
                var materials = new List<Bricks.PhysicsCore.TtPhyMaterial>() { TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial };
                var terrainShape = pc.CreateShapeHeightfield(materials,
                    PhyHeightfield, PxHeightfieldScale, in Vector3.One);
                var SimulationFilterData = new FPhyFilterData();
                terrainShape.mCoreObject.SetQueryFilterData(SimulationFilterData);
                terrainShape.mCoreObject.SetSimulationFilterData(SimulationFilterData);
                PhyActor = pc.CreateActor(EPhyActorType.PAT_Static, in Level.Node.Placement.AbsTransform.mPosition, in Quaternion.Identity);
                Vector3 shapeCenter;
                shapeCenter.X = (float)Level.LevelX * Level.Node.LevelSize;
                shapeCenter.Z = (float)Level.LevelZ * Level.Node.LevelSize;
                shapeCenter.Y = (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
                PhyActor.AddShape(terrainShape, in shapeCenter, in Quaternion.Identity);

                //var planeShape = pc.CreateShapePlane(materials[0]);
                //PhyActor.AddShape(planeShape, in StartPosition, in Quaternion.Identity);
            }
        }
        public float GetAltitude(float x, float z, float gridSize)
        {
            //这里因为physx的heightfield奇怪的拧巴了一下 
            var xGrid = (int)(z / gridSize);
            var zGrid = (int)(x / gridSize);
            if (xGrid < 0 || xGrid >= HeightfieldWidth || zGrid < 0 || zGrid >= HeightfieldHeight)
                return float.MinValue;

            var xLerp = z % gridSize;
            var zLerp = x % gridSize;

            var x2 = xGrid + 1;
            if (x2 >= HeightfieldWidth - 1)
                x2 = xGrid;
            var z2 = zGrid + 1;
            if (z2 >= HeightfieldHeight - 1)
                z2 = zGrid;

            //todo:也许后面要改成和绘制一致的三角形内插
            var h0 = (float)(PxHeightfieldSamples[zGrid * HeightfieldWidth + xGrid].height) * PxHeightfieldScale;
            var h1 = (float)(PxHeightfieldSamples[(z2) * HeightfieldWidth + xGrid].height) * PxHeightfieldScale;
            var h2 = (float)(PxHeightfieldSamples[(z2) * HeightfieldWidth + x2].height) * PxHeightfieldScale;
            var h3 = (float)(PxHeightfieldSamples[zGrid * HeightfieldWidth + x2].height) * PxHeightfieldScale;
            var zT1 = MathHelper.Lerp(h0, h1, zLerp);
            var zT2 = MathHelper.Lerp(h3, h2, zLerp);
            return MathHelper.Lerp(zT1, zT2, xLerp) + (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
        }
        #endregion
        public void SetAcceptShadow(bool value)
        {
            foreach (var i in TiledPatch)
            {
                if (i==null)
                    continue;
                i.SetAcceptShadow(value);
            }
        }
        public void OnAbsTransformChanged(TtTerrainNode node, GamePlay.TtWorld world)
        {
            foreach (var i in TiledPatch)
            {
                if (i==null)
                    continue;
                i.OnAbsTransformChanged(node, world);
            }
        }
        public void UpdateCameraOffset(GamePlay.TtWorld world)
        {
            foreach (var i in TiledPatch)
            {
                if (i==null)
                    continue;
                i.UpdateCameraOffset(world);
            }
        }
        public unsafe void OnGatherVisibleMeshes_OnRenderThread(GamePlay.TtWorld.TtVisParameter rp)
        {
            foreach (var i in TiledPatch)
            {
                if (i==null)
                    continue;
                if (rp.CullCamera.WhichContainTypeFast(rp.World, i.AABB, false) == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                    continue;
                i.OnGatherVisibleMeshes_OnRenderThread(rp);
            }
        }
        public void UpdateAfterCameraOffset(TtTerrainNode node, in DVector3 CameraOffset)
        {
            foreach (var i in TiledPatch)
            {
                if (i==null)
                    continue;
                i.UpdateAfterCameraOffset(node, in CameraOffset);
            }
        }
        /// <summary>发布点: LOD 分配完成后, 逻辑线程一次性把快照交给渲染线程</summary>
        public void PublishRenderLOD_OnRenderThread()
        {
            foreach (var i in TiledPatch)
            {
                if (i==null)
                    continue;
                i.PublishRenderLOD_OnRenderThread();
            }
        }
        public void OnParentSceneChanged(GamePlay.Scene.TtScene prev, GamePlay.Scene.TtScene cur)
        {
            if (PhyActor == null)
                return;
            //if (TtEngine.Instance != null)
            {
                //if (TtEngine.Instance.PlayMode != EPlayMode.Editor || TtEngine.Instance.PlayMode != EPlayMode.Cook)
                {
                    if (cur != null)
                    {
                        PhyActor.AddToScene(cur.PxSceneMB.PxScene);
                    }
                    else
                    {
                        PhyActor.AddToScene(null);
                    }
                }
            }
        }

        public void FrustumCull(GamePlay.TtWorld.TtVisParameter rp, List<TtPatch> patches)
        {
            foreach (var i in TiledPatch)
            {
                if (i==null)
                    continue;
                if (rp.CullCamera == null)
                    continue;
                if (rp.CullCamera.WhichContainTypeFast(rp.World, i.AABB, false) == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                    continue;

                patches.Add(i);
            }
        }
        #region CDLOD
        public void SetLODLevel(int level)
        {
            foreach(var i in TiledPatch)
            {
                i.CurrentLOD = level;
            }
        }
        // SphereCover 已删除: MorphRange.X 改为直接取上一圈半径, 不再逐 patch 统计实际覆盖距离

        /// <summary>
        /// 逐 patch 的 LOD 降级判定 (原 SphereCover 顺带做的事, 去掉角点距离统计):
        /// 尚未分配 (int.MaxValue) 且与圈球相交的 patch 降为当前圈 LOD。
        /// 只处理未分配的保证由近及远逐圈占位, 近圈结果不被远圈覆盖。
        /// </summary>
        public void AssignLODLevel(in DBoundingSphere sphere, int level)
        {
            foreach (var i in TiledPatch)
            {
                if (i.CurrentLOD == int.MaxValue && DBoundingBox.Intersects(in i.AABB, sphere))
                    i.CurrentLOD = level;
                //else
                //    i.CurrentLOD = 0;
            }
        }

        // int.MaxValue 哨兵不在生产侧收尾: 消费侧 TtPatch.OnGatherVisibleMeshes_OnRenderThread 把
        // 越界 CurrentLOD 钳到最粗糙网格 (TerrainMesh.Length - 1)
        #endregion
    }
}

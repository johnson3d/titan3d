using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// 材质 ID 笔刷参数。
    ///
    /// 没有 Strength: ID 图是整数下标, 写入是"覆盖"而不是"累积", 强度无处可施。
    /// Falloff 也不再是插值权重, 而是抖动覆盖的软化比例 (见 ApplyMaterialBrush)。
    /// </summary>
    public struct FTerrainMaterialBrushParam
    {
        /// <summary>
        /// 目标材质在 UTerrainMaterialIdManager.MaterialIdArray 里的下标。
        /// </summary>
        public byte MaterialId;
        /// <summary>
        /// 笔刷半径, 世界单位 (与高度笔刷同义)。
        /// </summary>
        public float Radius;
        /// <summary>
        /// 边缘软化比例 0~1。0 = 硬边, 1 = 从圆心就开始衰减。
        /// </summary>
        public float Falloff;
        /// <summary>
        /// true = 擦除: 把 texel 写回 PGC 基底 ID, 而不是写 MaterialId。
        /// </summary>
        public bool bErase;

        public static FTerrainMaterialBrushParam Default
        {
            get
            {
                FTerrainMaterialBrushParam result;
                result.MaterialId = 0;
                result.Radius = 8.0f;
                result.Falloff = 0.5f;
                result.bErase = false;
                return result;
            }
        }
    }

    public partial class UTerrainLevelData
    {
        #region SourceBuffers
        /// <summary>
        /// CPU 侧当前生效的材质 ID (行主序 mMaterialIdWidth × mMaterialIdHeight)。
        /// 由 UpdateMaterialIdMap 在覆盖层已叠加之后抓取, 所以它是"基底 + 手绘"的结果。
        ///
        /// ID 图是 PXF_R8G8B8A8_UNORM 但只有 R 通道有意义 (值 = MaterialIdArray 的整数下标),
        /// 所以 CPU 侧只留 byte[] 就够, 1MB/level。
        /// </summary>
        public byte[] SourceMaterialIdMap;
        /// <summary>
        /// PGC 基底 ID, 供 Shift 擦除回退。取值有两条路径:
        ///   1. 有覆盖层时由 ApplyMaterialIdOverlayIfAny 在**叠加之前**抓;
        ///   2. 没有覆盖层时由首次落笔的 SureBaseMaterialIdMap 从 SourceMaterialIdMap 克隆 ——
        ///      此时 Source 还没被编辑过, 等于基底。
        /// </summary>
        byte[] mBaseMaterialIdMap;
        int mMaterialIdWidth;
        int mMaterialIdHeight;

        public int MaterialIdWidth
        {
            get { return mMaterialIdWidth; }
        }
        public int MaterialIdHeight
        {
            get { return mMaterialIdHeight; }
        }
        /// <summary>
        /// MaterialIdMap 为 null 说明纹理还没建 (或 level 已卸载), SourceMaterialIdMap
        /// 为 null 说明不在编辑器态。两者缺一都不能刷。
        /// </summary>
        public bool IsMaterialIdEditable
        {
            get { return SourceMaterialIdMap != null && MaterialIdMap != null; }
        }

        /// <summary>
        /// 由 UpdateMaterialIdMap 调用 (idMap 已经叠过覆盖层)。
        /// </summary>
        internal void CaptureSourceMaterialIdMap(Procedure.TtBufferComponent idMap)
        {
            if (idMap == null)
                return;
            mMaterialIdWidth = idMap.Width;
            mMaterialIdHeight = idMap.Height;
            SourceMaterialIdMap = CopyIdsFromBuffer(idMap);
        }
        /// <summary>
        /// float buffer → byte[]。取整方式必须和 UImage2D.CreateRGBA8Texture2D(bNormalized: false)
        /// 里的 (byte)GetPixel&lt;float&gt; 完全一致, 否则 CPU 副本和 GPU 上的整层数据会差一档。
        /// </summary>
        static byte[] CopyIdsFromBuffer(Procedure.TtBufferComponent idMap)
        {
            int w = idMap.Width;
            int h = idMap.Height;
            var result = new byte[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    result[y * w + x] = (byte)idMap.GetPixel<float>(x, y);
                }
            }
            return result;
        }
        byte[] SureBaseMaterialIdMap()
        {
            if (mBaseMaterialIdMap == null && SourceMaterialIdMap != null)
            {
                mBaseMaterialIdMap = (byte[])SourceMaterialIdMap.Clone();
            }
            return mBaseMaterialIdMap;
        }
        #endregion

        #region Overlay
        TtTerrainLevelMaterialIdOverlay mMaterialIdOverlay = null;

        TtTerrainLevelMaterialIdOverlay SureMaterialIdOverlay()
        {
            if (mMaterialIdOverlay == null)
            {
                if (Level == null || Level.Node == null)
                    return null;
                mMaterialIdOverlay = Level.Node.MaterialIdOverlay.GetOrCreateLevel(Level.LevelX, Level.LevelZ);
            }
            return mMaterialIdOverlay;
        }
        /// <summary>
        /// 所有改写 SourceMaterialIdMap 的地方都必须过一遍这里。
        ///
        /// 统一规则: 新 ID 等于基底就 Clear, 否则 Set(绝对 ID)。这样 Paint / Shift 擦除 / Undo
        /// 共用一条分支, 且"刷回原材质"不会在覆盖层里留下冗余记录。
        ///
        /// 存绝对 ID 而不是 delta 是刻意的 —— ID 没有可加性。PGC 基底变更后手绘区仍是用户
        /// 指定的材质, 未手绘区跟着新基底走。
        /// </summary>
        void RecordOverlayMaterialId(int x, int y, byte newId)
        {
            var overlay = SureMaterialIdOverlay();
            if (overlay == null)
                return;

            var baseIds = SureBaseMaterialIdMap();
            byte baseId = baseIds != null ? baseIds[y * mMaterialIdWidth + x] : (byte)0;
            if (newId == baseId)
                overlay.Clear(x, y);
            else
                overlay.Set(x, y, newId);
        }

        /// <summary>
        /// 把本 level 的材质覆盖层叠到 ID 图上, 供 CreateFromBuffer 在建纹理之前调用。
        /// 返回叠加后的**副本** (调用方负责释放), 没有覆盖层时返回 null。
        ///
        /// 必须返回副本的理由与高度侧完全相同: 传进来的 idMap 就是 PGC 的 result buffer,
        /// 它随后会被 SaveLevelToCache 从 root.GetResultBuffer("MatId") 取出写进 .trlvl ——
        /// 原地叠加会把手绘材质烤进基底。
        /// </summary>
        internal Procedure.TtBufferComponent ApplyMaterialIdOverlayIfAny(Procedure.TtBufferComponent idMap)
        {
            if (idMap == null || Level == null || Level.Node == null)
                return null;

            var overlay = Level.Node.MaterialIdOverlay.TryLoadLevel(Level.LevelX, Level.LevelZ);
            if (overlay == null || overlay.IsEmpty)
                return null;

            // 基底副本必须在叠加之前抓 —— 叠加之后就再也拿不到 PGC 原值了。
            if (IsTerrainEditSupported)
            {
                mMaterialIdWidth = idMap.Width;
                mMaterialIdHeight = idMap.Height;
                mBaseMaterialIdMap = CopyIdsFromBuffer(idMap);
            }

            var overlaid = idMap.Clone();
            var applied = overlay.ApplyTo(overlaid);
            if (applied.IsValid == false)
            {
                CoreSDK.DisposeObject(ref overlaid);
                return null;
            }

            mMaterialIdOverlay = overlay;
            return overlaid;
        }
        #endregion

        #region Dirty
        FTerrainDirtyRect mMaterialIdDirtyRect = FTerrainDirtyRect.Empty;

        public bool IsMaterialIdDirty
        {
            get { return mMaterialIdDirtyRect.IsValid; }
        }
        public FTerrainDirtyRect MaterialIdDirtyRect
        {
            get { return mMaterialIdDirtyRect; }
        }
        public void MarkMaterialIdDirty(in FTerrainDirtyRect rect)
        {
            mMaterialIdDirtyRect.Union(in rect);
        }
        void ClampToMaterialId(ref FTerrainDirtyRect rect)
        {
            rect.ClampTo(0, 0, mMaterialIdWidth - 1, mMaterialIdHeight - 1);
        }
        /// <summary>
        /// 把累积的脏区上传到 GPU。
        ///
        /// 与高度侧的 FlushDirty 不同, 这里没有 EditSession / headroom / 整层重建那一套:
        /// ID 图是 R8 原值, 没有 min/max 编码基准, 局部上传恒定合法。也不触碰法线、
        /// patch AABB、physx —— 所以这个函数极轻, 可以每帧调。
        /// </summary>
        public void FlushMaterialIdDirty()
        {
            if (IsMaterialIdEditable == false)
                return;
            if (mMaterialIdDirtyRect.IsValid == false)
                return;

            var rect = mMaterialIdDirtyRect;
            mMaterialIdDirtyRect = FTerrainDirtyRect.Empty;
            ClampToMaterialId(ref rect);
            if (rect.IsValid == false)
                return;

            UploadMaterialIdRegion(in rect);
        }
        #endregion

        #region Upload
        public unsafe void UploadMaterialIdRegion(in FTerrainDirtyRect rect)
        {
            if (IsMaterialIdEditable == false || rect.IsValid == false)
                return;

            int w = rect.Width;
            int h = rect.Height;
            var pixels = new Byte4[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Byte4 px = new Byte4();
                    // 与 CreateRGBA8Texture2D 在 CompY/CompZ == null、CompW == null 时的产物位级一致:
                    // R = ID, G/B = 0, A = 255。shader 端只读 .r。
                    px.X = SourceMaterialIdMap[(rect.MinY + y) * mMaterialIdWidth + (rect.MinX + x)];
                    px.Y = 0;
                    px.Z = 0;
                    px.W = 255;
                    pixels[y * w + x] = px;
                }
            }

            var fp = new NxRHI.FSubResourceFootPrint();
            fp.SetDefault();
            fp.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            fp.X = rect.MinX;
            fp.Y = rect.MinY;
            fp.Z = 0;
            fp.Width = (uint)w;
            fp.Height = (uint)h;
            fp.Depth = 1;
            fp.RowPitch = (uint)(w * sizeof(Byte4));
            fp.TotalSize = (uint)(w * h * sizeof(Byte4));

            fixed (Byte4* p = &pixels[0])
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Transfer, "TerrainEdit.UploadMaterialId"))
                {
                    MaterialIdMap.UpdateGpuData(tsCmd.CmdList, 0, p, &fp);
                }
            }

            MarkMaterialIdRvtDirty();
        }
        /// <summary>
        /// 与 MarkHeightmapRvtDirty 同构: RVT 开启时 shader 采的是 atlas 副本, 而 atlas 只在
        /// TtRVT.IsDirty 时才会重新拷贝。不标脏的话局部上传对画面完全没有效果。
        /// </summary>
        void MarkMaterialIdRvtDirty()
        {
            if (TtEngine.Instance.Config.Feature_UseRVT == false)
                return;
            var rvt = GetMaterialIdRVT();
            if (rvt != null)
                rvt.IsDirty = true;
        }
        #endregion

        #region Block
        /// <summary>
        /// 取出一块材质 ID (行主序, 长度 rect.Width * rect.Height)。供 Undo 快照使用。
        /// </summary>
        public byte[] GetMaterialIdBlock(in FTerrainDirtyRect rect)
        {
            if (IsMaterialIdEditable == false || rect.IsValid == false)
                return null;

            var clamped = rect;
            ClampToMaterialId(ref clamped);
            if (clamped.IsValid == false)
                return null;

            int w = clamped.Width;
            int h = clamped.Height;
            var result = new byte[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    result[y * w + x] = SourceMaterialIdMap[(clamped.MinY + y) * mMaterialIdWidth + (clamped.MinX + x)];
                }
            }
            return result;
        }
        /// <summary>
        /// 整块写回材质 ID 并触发上传。Undo/Redo 直接用这个。
        /// </summary>
        public void SetMaterialIdBlock(in FTerrainDirtyRect rect, byte[] ids, bool bFlushNow = true)
        {
            if (IsMaterialIdEditable == false || ids == null || rect.IsValid == false)
                return;

            var clamped = rect;
            ClampToMaterialId(ref clamped);
            if (clamped.IsValid == false)
                return;

            int w = clamped.Width;
            int h = clamped.Height;
            if (ids.Length < w * h)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, "TerrainEdit",
                    $"SetMaterialIdBlock: buffer too small, need {w * h} got {ids.Length}");
                return;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int tx = clamped.MinX + x;
                    int ty = clamped.MinY + y;
                    int i = ty * mMaterialIdWidth + tx;
                    byte newId = ids[y * w + x];
                    if (newId == SourceMaterialIdMap[i])
                        continue;
                    SourceMaterialIdMap[i] = newId;
                    // Undo/Redo 也得进覆盖层, 否则回退后保存, 盘上的记录会和实际地形对不上。
                    RecordOverlayMaterialId(tx, ty, newId);
                }
            }

            MarkMaterialIdDirty(in clamped);
            if (bFlushNow)
            {
                FlushMaterialIdDirty();
            }
        }
        #endregion

        #region Brush
        /// <summary>
        /// 算出一笔材质笔刷会覆盖到的 texel 区域 (已 clamp 到 level 内)。
        /// 抽出来是为了让 undo 的拖动记录器能在落笔之前先把这一笔的原值补记下来。
        /// </summary>
        public FTerrainDirtyRect CalcMaterialBrushRect(float localX, float localZ, in FTerrainMaterialBrushParam param)
        {
            if (IsMaterialIdEditable == false || param.Radius <= 0.0f)
                return FTerrainDirtyRect.Empty;

            float gridSize = GetTerrainNode().GridSize;
            if (gridSize <= 0.0f)
                return FTerrainDirtyRect.Empty;

            float centerTexX = localX / gridSize;
            float centerTexY = localZ / gridSize;
            float radiusTex = param.Radius / gridSize;

            var rect = FTerrainDirtyRect.FromBound(
                MathHelper.FloorToInt(centerTexX - radiusTex),
                MathHelper.FloorToInt(centerTexY - radiusTex),
                (int)Math.Ceiling(centerTexX + radiusTex),
                (int)Math.Ceiling(centerTexY + radiusTex));
            ClampToMaterialId(ref rect);
            return rect;
        }

        /// <summary>
        /// 在 level 内相对坐标 (localX, localZ) 刷一笔材质。返回实际影响到的 texel 区域。
        /// 只改 CPU 数据 + 累积脏区, 不上传 —— 由调用方决定何时 FlushMaterialIdDirty。
        ///
        /// 软边靠**抖动覆盖**: ID 是整数下标, 没法在两个材质之间插值, 所以把距离权重当成
        /// "该 texel 写不写"的概率。哈希只依赖 texel 坐标 (不带随机数状态), 所以反复刷
        /// 同一处结果稳定, 不会越刷越糊。散点边缘经 shader 的 4-tap 颜色混合后即为过渡。
        /// </summary>
        public FTerrainDirtyRect ApplyMaterialBrush(float localX, float localZ, in FTerrainMaterialBrushParam param)
        {
            if (IsMaterialIdEditable == false)
                return FTerrainDirtyRect.Empty;
            if (param.Radius <= 0.0f)
                return FTerrainDirtyRect.Empty;

            float gridSize = GetTerrainNode().GridSize;
            if (gridSize <= 0.0f)
                return FTerrainDirtyRect.Empty;

            var rect = CalcMaterialBrushRect(localX, localZ, in param);
            if (rect.IsValid == false)
                return FTerrainDirtyRect.Empty;

            byte[] baseIds = null;
            if (param.bErase)
            {
                baseIds = SureBaseMaterialIdMap();
                if (baseIds == null)
                    return FTerrainDirtyRect.Empty;
            }

            float centerTexX = localX / gridSize;
            float centerTexY = localZ / gridSize;
            float radiusTex = param.Radius / gridSize;
            float radiusTexSq = radiusTex * radiusTex;

            var affected = FTerrainDirtyRect.Empty;
            for (int y = rect.MinY; y <= rect.MaxY; y++)
            {
                for (int x = rect.MinX; x <= rect.MaxX; x++)
                {
                    float dx = x - centerTexX;
                    float dy = y - centerTexY;
                    float distSq = dx * dx + dy * dy;
                    if (distSq > radiusTexSq)
                        continue;

                    float weight = GetBrushWeight((float)Math.Sqrt(distSq) / radiusTex, param.Falloff);
                    if (weight <= 0.0f)
                        continue;
                    if (weight < 1.0f && TexelDither(x, y) >= weight)
                        continue;

                    int i = y * mMaterialIdWidth + x;
                    byte newId = param.bErase ? baseIds[i] : param.MaterialId;
                    if (newId == SourceMaterialIdMap[i])
                        continue;

                    SourceMaterialIdMap[i] = newId;
                    RecordOverlayMaterialId(x, y, newId);
                    affected.Include(x, y);
                }
            }

            if (affected.IsValid)
            {
                MarkMaterialIdDirty(in affected);
            }
            return affected;
        }

        /// <summary>
        /// texel 坐标 → [0, 1) 的确定性抖动值。
        /// 用空间哈希而不是随机数: 同一个 texel 每次算出的值必须一样, 否则反复刷同一处
        /// 会不断有新 texel 被点亮, 软边区最终会被填满 (越刷越糊)。
        /// </summary>
        static float TexelDither(int x, int y)
        {
            uint h = (uint)(x * 73856093) ^ (uint)(y * 19349663);
            // murmur3 finalizer, 把低位的规律性打散
            h ^= h >> 16;
            h *= 0x7feb352d;
            h ^= h >> 15;
            h *= 0x846ca68b;
            h ^= h >> 16;
            return (h & 0xFFFFFFu) * (1.0f / 16777216.0f);
        }
        #endregion
    }
}

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public partial class TtTerrainNode
    {
        TtTerrainMaterialIdOverlay mMaterialIdOverlay = null;
        /// <summary>
        /// 本节点所有 level 的材质 ID 覆盖层管理器。与 HeightOverlay 一样是懒创建, 同样
        /// 不能拿它是不是 null 当"有没有刷过材质"的判据。
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public TtTerrainMaterialIdOverlay MaterialIdOverlay
        {
            get
            {
                if (mMaterialIdOverlay == null)
                    mMaterialIdOverlay = new TtTerrainMaterialIdOverlay(this);
                return mMaterialIdOverlay;
            }
        }
        /// <summary>
        /// 供 OnSaveNodeExtraData 用: 拿字段而不是属性, 避免每次保存都凭空造一个空管理器。
        /// </summary>
        internal TtTerrainMaterialIdOverlay MaterialIdOverlayIfCreated
        {
            get { return mMaterialIdOverlay; }
        }

        /// <summary>
        /// 在世界坐标处刷一笔材质。跨 level 的笔刷只作用于圆心所在的 level。
        ///
        /// 与 ApplyHeightBrushAtWorld 的差别: 不需要 BeginEditSession (ID 图没有编码基准
        /// 可冻结), 也不需要重建物理。
        /// </summary>
        public UTerrainLevelData ApplyMaterialBrushAtWorld(in DVector3 worldPos, in FTerrainMaterialBrushParam param, bool bFlushNow)
        {
            var levelData = GetLevelDataAtWorld(in worldPos, out var localX, out var localZ);
            if (levelData == null)
                return null;

            var affected = levelData.ApplyMaterialBrush(localX, localZ, in param);
            if (affected.IsValid == false)
                return levelData;

            if (bFlushNow)
            {
                levelData.FlushMaterialIdDirty();
            }
            return levelData;
        }
    }
}

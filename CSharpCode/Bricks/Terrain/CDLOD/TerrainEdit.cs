using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public enum ETerrainBrushTool
    {
        Raise,
        Lower,
        Smooth,
        Flatten,
    }

    public struct FTerrainBrushParam
    {
        public ETerrainBrushTool Tool;
        /// <summary>
        /// 笔刷半径, 世界单位。
        /// </summary>
        public float Radius;
        /// <summary>
        /// Raise/Lower: 单次落笔的高度增量 (世界单位)。
        /// Smooth/Flatten: 单次落笔向目标值插值的权重 (0~1)。
        /// </summary>
        public float Strength;
        /// <summary>
        /// 边缘软化比例 0~1。0 = 硬边, 1 = 从圆心就开始衰减。
        /// </summary>
        public float Falloff;
        /// <summary>
        /// Flatten 的目标高度 (level 内相对高度, 与 SourceHeightMap 同一基准)。
        /// </summary>
        public float TargetHeight;

        public static FTerrainBrushParam Default
        {
            get
            {
                FTerrainBrushParam result;
                result.Tool = ETerrainBrushTool.Raise;
                result.Radius = 8.0f;
                result.Strength = 1.0f;
                result.Falloff = 0.5f;
                result.TargetHeight = 0.0f;
                return result;
            }
        }
    }

    /// <summary>
    /// texel 空间的闭区间矩形 [MinX, MaxX] × [MinY, MaxY]。
    /// 坐标系与 UBufferComponent.GetPixel(x, y) 一致, 即 X 对应世界 X, Y 对应世界 Z。
    /// </summary>
    public struct FTerrainDirtyRect
    {
        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;

        public static FTerrainDirtyRect Empty
        {
            get
            {
                FTerrainDirtyRect result;
                result.MinX = int.MaxValue;
                result.MinY = int.MaxValue;
                result.MaxX = int.MinValue;
                result.MaxY = int.MinValue;
                return result;
            }
        }
        public static FTerrainDirtyRect FromBound(int minX, int minY, int maxX, int maxY)
        {
            FTerrainDirtyRect result;
            result.MinX = minX;
            result.MinY = minY;
            result.MaxX = maxX;
            result.MaxY = maxY;
            return result;
        }
        public bool IsValid
        {
            get { return MaxX >= MinX && MaxY >= MinY; }
        }
        public int Width
        {
            get { return MaxX - MinX + 1; }
        }
        public int Height
        {
            get { return MaxY - MinY + 1; }
        }
        public void Include(int x, int y)
        {
            if (x < MinX) MinX = x;
            if (y < MinY) MinY = y;
            if (x > MaxX) MaxX = x;
            if (y > MaxY) MaxY = y;
        }
        public void Union(in FTerrainDirtyRect other)
        {
            if (other.IsValid == false)
                return;
            Include(other.MinX, other.MinY);
            Include(other.MaxX, other.MaxY);
        }
        public void Expand(int num)
        {
            if (IsValid == false)
                return;
            MinX -= num;
            MinY -= num;
            MaxX += num;
            MaxY += num;
        }
        /// <summary>
        /// 收缩到 [loX, hiX] × [loY, hiY] 闭区间内。收缩后可能变为非法 (完全在界外)。
        /// </summary>
        public void ClampTo(int loX, int loY, int hiX, int hiY)
        {
            if (MinX < loX) MinX = loX;
            if (MinY < loY) MinY = loY;
            if (MaxX > hiX) MaxX = hiX;
            if (MaxY > hiY) MaxY = hiY;
        }
        public override string ToString()
        {
            return IsValid ? $"[{MinX},{MinY}]-[{MaxX},{MaxY}]({Width}x{Height})" : "Empty";
        }
    }

    public partial class UTerrainLevelData
    {
        /// <summary>
        /// 地形编辑只在编辑器态可用: 需要额外常驻一份 CPU 侧高度源缓冲 (约 4MB 每个已加载 level),
        /// 运行时保留会造成内存回归。
        /// </summary>
        public static bool IsTerrainEditSupported
        {
            get { return TtEngine.Instance.PlayMode == EPlayMode.Editor; }
        }

        #region SourceBuffers
        /// <summary>
        /// CPU 侧 float 高度源数据, 由 CreateFromBuffer 里 Clone 得来。
        /// 不能从 PxHeightfieldSamples 反推 (short 量化会累积误差)。
        ///
        /// 注意不保留法线/材质 ID/水高的 CPU 副本: 法线是高度的纯派生数据, 编辑时现算
        /// 并直接上传即可 (省 12MB/level); 材质 ID 等到做材质笔刷时再单独引入。
        /// </summary>
        public Procedure.TtBufferComponent SourceHeightMap;

        public bool IsEditable
        {
            get
            {
                // SuperPixels 为 null 说明底层数据已被 Dispose (空壳), 此时 GetPixel 会直接 NRE,
                // 必须当不可编辑处理 —— 否则 IsEditable 会是个假阳性。
                return SourceHeightMap != null && SourceHeightMap.SuperPixels != null && HeightMap != null;
            }
        }
        #endregion

        #region Overlay
        /// <summary>
        /// 本 level 的高度覆盖层 (相对 PGC 基底的 delta)。首次需要时才向节点索取,
        /// 因为绝大多数 level 一辈子不会被编辑, 没必要每层都挂一个空对象。
        /// </summary>
        TtTerrainLevelHeightOverlay mHeightOverlay = null;

        TtTerrainLevelHeightOverlay SureHeightOverlay()
        {
            if (mHeightOverlay == null)
            {
                if (Level == null || Level.Node == null)
                    return null;
                mHeightOverlay = Level.Node.HeightOverlay.GetOrCreateLevel(Level.LevelX, Level.LevelZ);
            }
            return mHeightOverlay;
        }
        /// <summary>
        /// 所有改写 SourceHeightMap 的地方都必须过一遍这里, 覆盖层才能跟着 scene 一起保存。
        /// delta 传的是"本次写入相对写入前"的差值 —— 因为 oldHeight / newHeight 同处
        /// "基底 + 已有 delta" 域, 逐次累加的结果正好是相对基底的偏移。
        /// </summary>
        void AccumulateOverlayDelta(int x, int y, float delta)
        {
            if (delta == 0.0f)
                return;
            SureHeightOverlay()?.Accumulate(x, y, delta);
        }

        /// <summary>
        /// 把本 level 的覆盖层叠到高度图上, 供 CreateFromBuffer 在建立一切派生数据之前调用。
        /// 返回叠加后的**副本** (调用方负责释放), 没有覆盖层时返回 null。
        ///
        /// 必须返回副本而不能原地改 hMap: BuildLevelDataFromPGC 传进来的 hMap 就是 PGC 的
        /// result buffer, 它随后会被 SaveLevelToCache 写进 .trlvl 基底缓存 —— 原地叠加会把
        /// delta 烤进基底, 下次加载再叠一次, 高度直接翻倍。
        ///
        /// norMap 则可以原地改: 法线是高度的纯派生量, 重算是幂等覆盖写, 即便被缓存下来
        /// 也仍然是"当时那份叠加高度"对应的正确值。
        /// </summary>
        internal Procedure.TtBufferComponent ApplyHeightOverlayIfAny(Procedure.TtBufferComponent hMap, Procedure.TtBufferComponent norMap)
        {
            if (hMap == null || Level == null || Level.Node == null)
                return null;

            var overlay = Level.Node.HeightOverlay.TryLoadLevel(Level.LevelX, Level.LevelZ);
            if (overlay == null || overlay.IsEmpty)
                return null;

            var overlaid = hMap.Clone();
            var applied = overlay.ApplyTo(overlaid);
            if (applied.IsValid == false)
            {
                CoreSDK.DisposeObject(ref overlaid);
                return null;
            }

            mHeightOverlay = overlay;
            PatchNormalsFromOverlay(overlaid, norMap, in applied);
            return overlaid;
        }
        /// <summary>
        /// 覆盖区的法线要基于叠加后的高度重算, 否则 PGC 生成的法线还是基底的形状,
        /// 光照会和几何不匹配。外扩 1 是因为改一个 texel 的高度会影响相邻 texel 的法线。
        /// </summary>
        void PatchNormalsFromOverlay(Procedure.TtBufferComponent hMap, Procedure.TtBufferComponent norMap, in FTerrainDirtyRect rect)
        {
            if (norMap == null || hMap == null || rect.IsValid == false)
                return;

            var norRect = rect;
            norRect.Expand(1);
            // CalcNormalAt 采 6 邻域, 最外一圈算不了 (PGC 也是直接跳过那一圈保持初值)。
            norRect.ClampTo(1, 1, hMap.Width - 2, hMap.Height - 2);
            if (norRect.IsValid == false)
                return;

            float gridSize = GetTerrainNode().GridSize;
            for (int y = norRect.MinY; y <= norRect.MaxY; y++)
            {
                for (int x = norRect.MinX; x <= norRect.MaxX; x++)
                {
                    norMap.SetPixel(x, y, CalcNormalAt(hMap, x, y, gridSize));
                }
            }
        }
        #endregion

        #region EditSession
        bool mInEditSession = false;
        bool mHasFrozenHeightRange = false;
        float mFrozenMinHeight = 0.0f;
        float mFrozenMaxHeight = 0.0f;

        public bool IsInEditSession
        {
            get { return mInEditSession; }
        }

        /// <summary>
        /// 高度纹理是 R16_FLOAT 存 (h - HeightMapMinHeight), shader 端再加回
        /// TtPatch.StartPosition.Y (= HeightMapMinHeight)。一旦雕刻改变了 min/max,
        /// 整层已上传的 texel 编码会同时失效, 局部上传就没有意义了。
        /// 所以编辑会话期间预留 headroom 把编码基准钉死, 只要笔刷没超出 headroom
        /// 就可以一直做局部上传。
        /// </summary>
        public void BeginEditSession(float headroom = 256.0f)
        {
            if (IsTerrainEditSupported == false || IsEditable == false)
                return;
            if (mInEditSession)
                return;

            mInEditSession = true;
            mFrozenMinHeight = HeightMapMinHeight - headroom;
            mFrozenMaxHeight = HeightMapMaxHeight + headroom;
            mHasFrozenHeightRange = true;

            // 编码基准变了, 必须把整层重建一次 (高度纹理 / physx 样本 / patch AABB 全都依赖 min/max)。
            RebuildHeightMapFromSource();
        }
        public void EndEditSession()
        {
            if (mInEditSession == false)
                return;

            FlushDirty(true);

            mInEditSession = false;
            mHasFrozenHeightRange = false;
            // 恢复成源数据的真实范围, 让编码回到最高精度。
            RebuildHeightMapFromSource();
        }
        /// <summary>
        /// 由 UpdateHeightMap 在 GetRangeUnsafe 之后调用, 覆盖回冻结的编码基准。
        /// </summary>
        internal void ApplyFrozenHeightRangeIfEditing()
        {
            if (mHasFrozenHeightRange == false)
                return;
            HeightMapMinHeight = mFrozenMinHeight;
            HeightMapMaxHeight = mFrozenMaxHeight;
        }
        /// <summary>
        /// 整层重建高度相关的一切。InitPhysics 每次都会新建 actor 并覆盖 PhyActor 字段
        /// 而不移除旧的, 所以必须自己先把旧 actor 摘出场景, 重建后再挂回去。
        /// </summary>
        public void RebuildHeightMapFromSource()
        {
            if (IsEditable == false)
                return;

            PhyActor?.AddToScene(null);
            UpdateHeightMap(SourceHeightMap);
            OnParentSceneChanged(null, Level.Node.ParentScene);

            RecalcAndUploadNormalRegion(FullRect());

            MarkHeightmapRvtDirty();
            mHeightDirtyRect = FTerrainDirtyRect.Empty;
        }
        #endregion

        #region Dirty
        FTerrainDirtyRect mHeightDirtyRect = FTerrainDirtyRect.Empty;

        public bool IsHeightDirty
        {
            get { return mHeightDirtyRect.IsValid; }
        }
        public FTerrainDirtyRect HeightDirtyRect
        {
            get { return mHeightDirtyRect; }
        }
        public void MarkHeightDirty(in FTerrainDirtyRect rect)
        {
            mHeightDirtyRect.Union(in rect);
        }
        public void MarkHeightDirty(int x, int y)
        {
            mHeightDirtyRect.Include(x, y);
        }
        FTerrainDirtyRect FullRect()
        {
            return FTerrainDirtyRect.FromBound(0, 0, SourceHeightMap.Width - 1, SourceHeightMap.Height - 1);
        }
        void ClampToLevel(ref FTerrainDirtyRect rect)
        {
            rect.ClampTo(0, 0, SourceHeightMap.Width - 1, SourceHeightMap.Height - 1);
        }

        /// <summary>
        /// 把累积的脏区刷到 GPU / 物理 / 包围盒。
        /// </summary>
        /// <param name="bRebuildPhysics">
        /// 是否重 cook physx heightfield。整层 cook 开销大 (1024×1024), 只在抬手时传 true,
        /// 拖动过程中传 false。
        /// </param>
        public void FlushDirty(bool bRebuildPhysics)
        {
            if (IsEditable == false)
                return;
            if (mHeightDirtyRect.IsValid == false)
            {
                if (bRebuildPhysics == false)
                    return;
            }

            var rect = mHeightDirtyRect;
            mHeightDirtyRect = FTerrainDirtyRect.Empty;

            if (rect.IsValid)
            {
                ClampToLevel(ref rect);
            }

            if (rect.IsValid)
            {
                if (IsEncodeRangeExceeded(in rect))
                {
                    // 笔刷冲出了 headroom, 编码基准整体失效, 只能整层重建。
                    // 顺便把冻结范围重新拉开, 避免接下来每笔都走这条慢路。
                    if (mInEditSession)
                    {
                        SourceHeightMap.GetRangeUnsafe<float, Procedure.FFloatOperator>(out var srcMin, out var srcMax);
                        var headroom = (mFrozenMaxHeight - mFrozenMinHeight) * 0.5f;
                        mFrozenMinHeight = srcMin - headroom;
                        mFrozenMaxHeight = srcMax + headroom;
                    }
                    RebuildHeightMapFromSource();
                    return;
                }

                UploadHeightRegion(in rect);

                // 高度改了 (x, y) 会影响 (x±1, y±1) 的法线, 所以法线脏区要外扩 1。
                var norRect = rect;
                norRect.Expand(1);
                ClampToLevel(ref norRect);
                RecalcAndUploadNormalRegion(in norRect);

                RefreshPatchAABBInRect(in rect);
            }

            if (bRebuildPhysics)
            {
                RebuildPhysicsHeightfield();
            }
        }
        #endregion

        #region HeightEncode
        /// <summary>
        /// 复现 CreateAsHeightMapTexture2D 开头那个 quirk: 当 min/max 都落在 [0,1] 内时
        /// 会被强制改写成 0/1。局部上传必须和整层创建的编码位级一致, 否则脏区会出现台阶。
        /// </summary>
        float GetHeightEncodeMin()
        {
            float min = HeightMapMinHeight;
            float max = HeightMapMaxHeight;
            if (min >= 0 && min <= 1 && max >= 0 && max <= 1)
            {
                min = 0;
            }
            return min;
        }
        /// <summary>
        /// R16_FLOAT 在 (h - encodeMin) 较大时精度会掉。这里只做范围检查:
        /// 判断脏区内的高度是否已经跑出当前编码基准。
        /// </summary>
        bool IsEncodeRangeExceeded(in FTerrainDirtyRect rect)
        {
            float min = HeightMapMinHeight;
            float max = HeightMapMaxHeight;
            for (int y = rect.MinY; y <= rect.MaxY; y++)
            {
                for (int x = rect.MinX; x <= rect.MaxX; x++)
                {
                    float h = SourceHeightMap.GetPixel<float>(x, y);
                    if (h < min || h > max)
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Upload
        public unsafe void UploadHeightRegion(in FTerrainDirtyRect rect)
        {
            if (IsEditable == false || rect.IsValid == false)
                return;

            int w = rect.Width;
            int h = rect.Height;
            float encodeMin = GetHeightEncodeMin();
            var pixels = new Half[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var alt = SourceHeightMap.GetPixel<float>(rect.MinX + x, rect.MinY + y);
                    pixels[y * w + x] = HalfHelper.SingleToHalf(alt - encodeMin);
                }
            }

            var fp = new NxRHI.FSubResourceFootPrint();
            fp.SetDefault();
            fp.Format = EPixelFormat.PXF_R16_FLOAT;
            fp.X = rect.MinX;
            fp.Y = rect.MinY;
            fp.Z = 0;
            fp.Width = (uint)w;
            fp.Height = (uint)h;
            fp.Depth = 1;
            fp.RowPitch = (uint)(w * sizeof(Half));
            fp.TotalSize = (uint)(w * h * sizeof(Half));

            fixed (Half* p = &pixels[0])
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Transfer, "TerrainEdit.UploadHeight"))
                {
                    HeightMap.UpdateGpuData(tsCmd.CmdList, 0, p, &fp);
                }
            }

            MarkHeightmapRvtDirty();
        }
        /// <summary>
        /// 按脏区重算法线并直接上传, 不落 CPU 副本。
        /// PGC 对最外一圈 texel 直接 return 保持初值, 所以这里把上传区域收缩掉最外圈 ——
        /// 那一圈永远不会变, 让 GPU 上保持整层创建时的原值即可。
        /// </summary>
        public unsafe void RecalcAndUploadNormalRegion(in FTerrainDirtyRect rect)
        {
            if (NormalMap == null || IsEditable == false || rect.IsValid == false)
                return;

            int x0 = Math.Max(1, rect.MinX);
            int y0 = Math.Max(1, rect.MinY);
            int x1 = Math.Min(SourceHeightMap.Width - 2, rect.MaxX);
            int y1 = Math.Min(SourceHeightMap.Height - 2, rect.MaxY);
            // 脏区完全落在最外圈上时收缩后会变空。
            if (x1 < x0 || y1 < y0)
                return;

            int w = x1 - x0 + 1;
            int h = y1 - y0 + 1;
            float gridSize = GetTerrainNode().GridSize;
            var pixels = new Byte4[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var n = CalcNormalAt(SourceHeightMap, x0 + x, y0 + y, gridSize);
                    Byte4 px = new Byte4();
                    px.X = EncodeNormalComponent(n.X);
                    px.Y = EncodeNormalComponent(n.Y);
                    px.Z = EncodeNormalComponent(n.Z);
                    // UImage2D.CreateRGBA8Texture2DAsNormal 里 CompW 为 null, W 保持 0。
                    px.W = 0;
                    pixels[y * w + x] = px;
                }
            }

            var fp = new NxRHI.FSubResourceFootPrint();
            fp.SetDefault();
            fp.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            fp.X = x0;
            fp.Y = y0;
            fp.Z = 0;
            fp.Width = (uint)w;
            fp.Height = (uint)h;
            fp.Depth = 1;
            fp.RowPitch = (uint)(w * sizeof(Byte4));
            fp.TotalSize = (uint)(w * h * sizeof(Byte4));

            fixed (Byte4* p = &pixels[0])
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Transfer, "TerrainEdit.UploadNormal"))
                {
                    NormalMap.UpdateGpuData(tsCmd.CmdList, 0, p, &fp);
                }
            }

            MarkNormalmapRvtDirty();
        }
        static byte EncodeNormalComponent(float v)
        {
            v = (v + 1.0f) * 0.5f;
            if (v < 0.0f) v = 0.0f;
            if (v > 1.0f) v = 1.0f;
            return (byte)(v * 255.0f);
        }

        /// <summary>
        /// RVT 开启时 (DX12/DX11 的默认引擎配置) shader 采的是 atlas 副本, 而 atlas 只在
        /// TtRVT.IsDirty 时才会重新拷贝。不标脏的话局部上传对画面完全没有效果。
        /// </summary>
        void MarkHeightmapRvtDirty()
        {
            if (TtEngine.Instance.Config.Feature_UseRVT == false)
                return;
            var rvt = GetHeightmapRVT();
            if (rvt != null)
                rvt.IsDirty = true;
        }
        void MarkNormalmapRvtDirty()
        {
            if (TtEngine.Instance.Config.Feature_UseRVT == false)
                return;
            var rvt = GetNormalmapRVT();
            if (rvt != null)
                rvt.IsDirty = true;
        }
        #endregion

        #region Normal
        /// <summary>
        /// 单点法线, 6 邻域三角形法线累加。算法必须和 PGC 的 UFloat3HeightToNormal 完全一致
        /// (包括它那处 v4 被用了两次的既有笔误), 否则脏区边界会出现法线突变的接缝。
        /// 调用方保证 (x, y) 不在最外一圈上 (邻域采样不会越界)。
        ///
        /// 高度源作参数传入而不是直接用 SourceHeightMap: level 构建期应用覆盖层时
        /// SourceHeightMap 还没建好, 那时要基于临时的叠加副本算法线。
        /// </summary>
        static Vector3 CalcNormalAt(Procedure.TtBufferComponent hMap, int x, int y, float gridSize)
        {
            /* It's terrain mesh topology
             *-1-2
             |/|/|
             6-0-3
             |/|/|
             5-4-*
             */
            float h0 = hMap.GetPixel<float>(x, y);
            float h1 = hMap.GetPixel<float>(x, y + 1) - h0;
            float h2 = hMap.GetPixel<float>(x + 1, y + 1) - h0;
            float h3 = hMap.GetPixel<float>(x + 1, y) - h0;
            float h4 = hMap.GetPixel<float>(x, y - 1) - h0;
            float h5 = hMap.GetPixel<float>(x - 1, y - 1) - h0;
            float h6 = hMap.GetPixel<float>(x - 1, y) - h0;

            Vector3 v0 = new Vector3(0, 0, 0);
            Vector3 v1 = new Vector3(0, h1, gridSize);
            Vector3 v2 = new Vector3(gridSize, h2, gridSize);
            Vector3 v3 = new Vector3(gridSize, h3, 0);
            Vector3 v4 = new Vector3(0, h4, -gridSize);
            Vector3 v5 = new Vector3(-gridSize, h5, -gridSize);
            Vector3 v6 = new Vector3(-gridSize, h6, 0);

            Vector3 n = Vector3.Zero;
            var A = v1 - v0;
            var B = v2 - v0;
            n += Vector3.Normalize(Vector3.Cross(A, B));

            A = B;
            B = v3 - v0;
            n += Vector3.Normalize(Vector3.Cross(A, B));

            A = B;
            B = v4 - v0;
            n += Vector3.Normalize(Vector3.Cross(A, B));

            A = B;
            B = v4 - v0;
            n += Vector3.Normalize(Vector3.Cross(A, B));

            A = B;
            B = v5 - v0;
            n += Vector3.Normalize(Vector3.Cross(A, B));

            A = B;
            B = v6 - v0;
            n += Vector3.Normalize(Vector3.Cross(A, B));

            n /= 6.0f;
            return Vector3.Normalize(n);
        }
        #endregion

        #region AABB
        /// <summary>
        /// 刷新脏区覆盖到的 patch 的包围盒。
        /// TtPatch.UpdateAABB 只做 Max/Min 累积不做重置, 而 TtPatch.Initialize 是在
        /// 调完 UpdateAABB 之后才把 placement 偏移加进去的。所以这里必须自己
        /// 重置 Y → 扫描 → 再补偏移, 不能直接调 UpdateAABB。
        /// </summary>
        public void RefreshPatchAABBInRect(in FTerrainDirtyRect rect)
        {
            if (TiledPatch == null || rect.IsValid == false)
                return;

            var node = GetTerrainNode();
            int texSizePerPatch = node.TexSizePerPatch;
            int patchSide = Level.PatchSide;

            int px0 = Math.Max(0, rect.MinX / texSizePerPatch);
            int px1 = Math.Min(patchSide - 1, rect.MaxX / texSizePerPatch);
            int pz0 = Math.Max(0, rect.MinY / texSizePerPatch);
            int pz1 = Math.Min(patchSide - 1, rect.MaxY / texSizePerPatch);

            var offsetY = node.Placement.AbsTransform.mPosition.Y;

            for (int z = pz0; z <= pz1; z++)
            {
                for (int x = px0; x <= px1; x++)
                {
                    // TiledPatch[i, j] 装的是 Initialize(this, j, i, ...), 即 [z, x]。
                    var patch = TiledPatch[z, x];
                    if (patch == null)
                        continue;
                    patch.AABB.Minimum.Y = double.MaxValue;
                    patch.AABB.Maximum.Y = double.MinValue;
                    patch.UpdateAABB(SourceHeightMap, null);
                    patch.AABB.Minimum.Y += offsetY;
                    patch.AABB.Maximum.Y += offsetY;
                }
            }
        }
        #endregion

        #region Physics
        /// <summary>
        /// 从源高度重 cook physx heightfield。整层 cook, 开销较大, 只在抬手时调。
        /// </summary>
        public void RebuildPhysicsHeightfield()
        {
            if (IsEditable == false || PxHeightfieldSamples == null)
                return;

            float midHeight = (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
            for (int i = 0; i < HeightfieldHeight; i++)
            {
                for (int j = 0; j < HeightfieldWidth; j++)
                {
                    // 和 UpdateHeightMap 保持一致: physx 的 heightfield 在这里是转置存放的,
                    // sample[i * W + j] 对应 buffer(x = i, y = j)。
                    float height = SourceHeightMap.GetPixel<float>(i, j);
                    float localHeight = height - midHeight;
                    PxHeightfieldSamples[i * HeightfieldWidth + j].height = (short)(localHeight / PxHeightfieldScale);
                }
            }

            // InitPhysics 会覆盖 PhyActor 字段而不移除旧 actor, 必须自己摘干净。
            PhyActor?.AddToScene(null);
            InitPhysics(Level);
            OnParentSceneChanged(null, Level.Node.ParentScene);
        }
        #endregion

        #region Block
        /// <summary>
        /// 取出一块高度数据 (行主序, 长度 rect.Width * rect.Height)。供 Undo 快照使用。
        /// </summary>
        public float[] GetHeightBlock(in FTerrainDirtyRect rect)
        {
            if (IsEditable == false || rect.IsValid == false)
                return null;

            var clamped = rect;
            ClampToLevel(ref clamped);
            if (clamped.IsValid == false)
                return null;

            int w = clamped.Width;
            int h = clamped.Height;
            var result = new float[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    result[y * w + x] = SourceHeightMap.GetPixel<float>(clamped.MinX + x, clamped.MinY + y);
                }
            }
            return result;
        }
        /// <summary>
        /// 整块写回高度数据并触发脏区刷新。Undo/Redo 直接用这个。
        /// </summary>
        public void SetHeightBlock(in FTerrainDirtyRect rect, float[] heights, bool bFlushNow = true, bool bRebuildPhysics = true)
        {
            if (IsEditable == false || heights == null || rect.IsValid == false)
                return;

            var clamped = rect;
            ClampToLevel(ref clamped);
            if (clamped.IsValid == false)
                return;

            int w = clamped.Width;
            int h = clamped.Height;
            if (heights.Length < w * h)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, "TerrainEdit",
                    $"SetHeightBlock: buffer too small, need {w * h} got {heights.Length}");
                return;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int tx = clamped.MinX + x;
                    int ty = clamped.MinY + y;
                    float oldHeight = SourceHeightMap.GetPixel<float>(tx, ty);
                    float newHeight = heights[y * w + x];
                    if (newHeight == oldHeight)
                        continue;
                    SourceHeightMap.SetPixel(tx, ty, newHeight);
                    // Undo/Redo 也得进覆盖层, 否则回退后保存, 盘上的 delta 会和实际地形对不上。
                    AccumulateOverlayDelta(tx, ty, newHeight - oldHeight);
                }
            }

            MarkHeightDirty(in clamped);
            if (bFlushNow)
            {
                FlushDirty(bRebuildPhysics);
            }
        }
        #endregion

        #region Sample
        /// <summary>
        /// 在 SourceHeightMap 上双线性采样, 返回 level 相对高度; 不可编辑时返回 float.MinValue。
        ///
        /// 视口拾取用这个而不是 GetAltitude: 后者走 PxHeightfieldSamples, 高度被量化成 short,
        /// 而且依赖物理 heightfield 已经构建过。SourceHeightMap 才是编辑器态的权威源。
        /// </summary>
        public float GetSourceAltitude(float localX, float localZ, float gridSize)
        {
            if (IsEditable == false || gridSize <= 0.0f)
                return float.MinValue;

            float fx = localX / gridSize;
            float fy = localZ / gridSize;
            int w = SourceHeightMap.Width;
            int h = SourceHeightMap.Height;
            int x0 = MathHelper.FloorToInt(fx);
            int y0 = MathHelper.FloorToInt(fy);
            if (x0 < 0 || y0 < 0 || x0 >= w || y0 >= h)
                return float.MinValue;

            int x1 = x0 + 1 < w ? x0 + 1 : x0;
            int y1 = y0 + 1 < h ? y0 + 1 : y0;
            float tx = fx - x0;
            float ty = fy - y0;

            float h00 = SourceHeightMap.GetPixel<float>(x0, y0);
            float h10 = SourceHeightMap.GetPixel<float>(x1, y0);
            float h01 = SourceHeightMap.GetPixel<float>(x0, y1);
            float h11 = SourceHeightMap.GetPixel<float>(x1, y1);
            return MathHelper.Lerp(MathHelper.Lerp(h00, h10, tx), MathHelper.Lerp(h01, h11, tx), ty);
        }
        #endregion

        #region Brush
        /// <summary>
        /// 算出一笔笔刷会覆盖到的 texel 区域 (已 clamp 到 level 内)。
        /// 抽出来是为了让 undo 的拖动记录器能在落笔之前先把这一笔的原值补记下来。
        /// </summary>
        public FTerrainDirtyRect CalcBrushRect(float localX, float localZ, in FTerrainBrushParam param)
        {
            if (IsEditable == false || param.Radius <= 0.0f)
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
            ClampToLevel(ref rect);
            return rect;
        }
        /// <summary>
        /// 在 level 内相对坐标 (localX, localZ) 落一笔。返回实际影响到的 texel 区域,
        /// 非法表示这一笔没有落到任何 texel 上。
        /// 只改 SourceHeightMap 并累积脏区, 不做上传 —— 由调用方决定何时 FlushDirty,
        /// 这样拖动过程中的连续多笔可以合并成一次上传。
        /// </summary>
        public FTerrainDirtyRect ApplyHeightBrush(float localX, float localZ, in FTerrainBrushParam param)
        {
            if (IsEditable == false)
                return FTerrainDirtyRect.Empty;
            if (param.Radius <= 0.0f)
                return FTerrainDirtyRect.Empty;

            float gridSize = GetTerrainNode().GridSize;
            if (gridSize <= 0.0f)
                return FTerrainDirtyRect.Empty;

            float centerTexX = localX / gridSize;
            float centerTexY = localZ / gridSize;
            float radiusTex = param.Radius / gridSize;

            var rect = CalcBrushRect(localX, localZ, in param);
            if (rect.IsValid == false)
                return FTerrainDirtyRect.Empty;

            // Smooth 必须基于落笔前的快照采样, 否则边扫边改会产生沿扫描方向的偏移。
            float[] snapshot = null;
            FTerrainDirtyRect snapRect = FTerrainDirtyRect.Empty;
            if (param.Tool == ETerrainBrushTool.Smooth)
            {
                snapRect = rect;
                snapRect.Expand(1);
                ClampToLevel(ref snapRect);
                snapshot = GetHeightBlock(in snapRect);
                if (snapshot == null)
                    return FTerrainDirtyRect.Empty;
            }

            var affected = FTerrainDirtyRect.Empty;
            float radiusTexSq = radiusTex * radiusTex;

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

                    float oldHeight = SourceHeightMap.GetPixel<float>(x, y);
                    float newHeight = oldHeight;

                    switch (param.Tool)
                    {
                        case ETerrainBrushTool.Raise:
                            newHeight = oldHeight + param.Strength * weight;
                            break;
                        case ETerrainBrushTool.Lower:
                            newHeight = oldHeight - param.Strength * weight;
                            break;
                        case ETerrainBrushTool.Flatten:
                            {
                                float t = MathHelper.Clamp(param.Strength * weight, 0.0f, 1.0f);
                                newHeight = MathHelper.Lerp(oldHeight, param.TargetHeight, t);
                            }
                            break;
                        case ETerrainBrushTool.Smooth:
                            {
                                float avg = SampleSnapshotAverage(snapshot, in snapRect, x, y);
                                float t = MathHelper.Clamp(param.Strength * weight, 0.0f, 1.0f);
                                newHeight = MathHelper.Lerp(oldHeight, avg, t);
                            }
                            break;
                    }

                    if (newHeight == oldHeight)
                        continue;

                    SourceHeightMap.SetPixel(x, y, newHeight);
                    AccumulateOverlayDelta(x, y, newHeight - oldHeight);
                    affected.Include(x, y);
                }
            }

            if (affected.IsValid)
            {
                MarkHeightDirty(in affected);
            }
            return affected;
        }

        /// <summary>
        /// 距离权重。ratio 是 dist/radius (0~1), falloff 是软化比例。
        /// 用 smoothstep 而不是线性衰减, 否则内圈边界会留下可见的折线。
        /// </summary>
        static float GetBrushWeight(float ratio, float falloff)
        {
            if (ratio >= 1.0f)
                return 0.0f;
            falloff = MathHelper.Clamp(falloff, 0.0f, 1.0f);
            float innerRatio = 1.0f - falloff;
            if (ratio <= innerRatio)
                return 1.0f;
            float t = (ratio - innerRatio) / (1.0f - innerRatio);
            // 反向 smoothstep: t=0 → 1, t=1 → 0
            return 1.0f - (t * t * (3.0f - 2.0f * t));
        }
        /// <summary>
        /// 在快照上取 3×3 均值。越界的邻居用中心值补 (等价于 clamp 寻址)。
        /// </summary>
        static float SampleSnapshotAverage(float[] snapshot, in FTerrainDirtyRect snapRect, int x, int y)
        {
            int w = snapRect.Width;
            int h = snapRect.Height;
            float center = snapshot[(y - snapRect.MinY) * w + (x - snapRect.MinX)];
            float sum = 0.0f;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int sx = x + dx - snapRect.MinX;
                    int sy = y + dy - snapRect.MinY;
                    if (sx < 0 || sx >= w || sy < 0 || sy >= h)
                        sum += center;
                    else
                        sum += snapshot[sy * w + sx];
                }
            }
            return sum / 9.0f;
        }
        #endregion
    }
}

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public partial class TtTerrainNode
    {
        TtTerrainHeightOverlay mHeightOverlay = null;
        /// <summary>
        /// 本节点所有 level 的高度覆盖层管理器。懒创建 —— 但只要有 level 开始查盘上
        /// 有没有 delta 就会建出来, 所以不能拿它是不是 null 当"有没有编辑过"的判据。
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public TtTerrainHeightOverlay HeightOverlay
        {
            get
            {
                if (mHeightOverlay == null)
                    mHeightOverlay = new TtTerrainHeightOverlay(this);
                return mHeightOverlay;
            }
        }

        /// <summary>
        /// 高度 / 材质 ID 覆盖层都不进 .node 文件 (它们是上百 MB 量级的数据, 且需要按 level
        /// 懒加载), 走 scene 目录下的旁路存。
        /// </summary>
        public override void OnSaveNodeExtraData(GamePlay.Scene.TtScene scene)
        {
            base.OnSaveNodeExtraData(scene);
            // 用字段而不是属性: 避免每次保存都凭空造一个空管理器。
            mHeightOverlay?.Save(scene);
            MaterialIdOverlayIfCreated?.Save(scene);
        }

        /// <summary>
        /// 把世界坐标换算成所在 level 与 level 内相对坐标。
        /// 注意 TtTerrainNode.GetAltitude 里 xInLevel 是直接对世界坐标取模的 (没减 placement),
        /// placement 非零时会算错; 这里按正确方式减掉。
        /// </summary>
        public UTerrainLevelData GetLevelDataAtWorld(in DVector3 worldPos, out float localX, out float localZ)
        {
            localX = 0.0f;
            localZ = 0.0f;

            var nsPos = worldPos - Placement.AbsTransform.mPosition;
            int levelX = (int)(nsPos.X / LevelSize);
            int levelZ = (int)(nsPos.Z / LevelSize);
            if (levelX < 0 || levelX >= NumOfLevelX || levelZ < 0 || levelZ >= NumOfLevelZ)
                return null;

            var levelData = Levels[levelZ, levelX].LevelData;
            if (levelData == null)
                return null;

            localX = (float)(nsPos.X - (double)levelX * LevelSize);
            localZ = (float)(nsPos.Z - (double)levelZ * LevelSize);
            return levelData;
        }

        /// <summary>
        /// 世界坐标处的地形绝对高度, 基于 SourceHeightMap (编辑器态权威源)。
        /// 该处没有已加载 level 或 level 不可编辑时返回 double.MinValue。
        /// </summary>
        public double GetEditAltitudeAtWorld(in DVector3 worldPos)
        {
            var levelData = GetLevelDataAtWorld(in worldPos, out var localX, out var localZ);
            if (levelData == null)
                return double.MinValue;

            float localHeight = levelData.GetSourceAltitude(localX, localZ, GridSize);
            if (localHeight == float.MinValue)
                return double.MinValue;
            return (double)localHeight + Placement.AbsTransform.Position.Y;
        }

        /// <summary>
        /// 在世界坐标处落一笔。跨 level 的笔刷目前只作用于圆心所在的 level,
        /// level 边界的接缝处理留给后续任务。
        /// </summary>
        public UTerrainLevelData ApplyHeightBrushAtWorld(in DVector3 worldPos, in FTerrainBrushParam param, bool bFlushNow, bool bRebuildPhysics)
        {
            var levelData = GetLevelDataAtWorld(in worldPos, out var localX, out var localZ);
            if (levelData == null)
                return null;

            if (levelData.IsInEditSession == false)
            {
                levelData.BeginEditSession();
            }

            var affected = levelData.ApplyHeightBrush(localX, localZ, in param);
            if (affected.IsValid == false)
                return levelData;

            if (bFlushNow)
            {
                levelData.FlushDirty(bRebuildPhysics);
            }
            return levelData;
        }
    }
}

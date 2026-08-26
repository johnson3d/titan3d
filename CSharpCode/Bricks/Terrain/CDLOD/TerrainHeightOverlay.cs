using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// 单个 level 的高度覆盖层: 相对 PGC 基底的 delta。
    ///
    /// 地形高度本身不是资产 —— 它由 TtTerrainData.PgcName 指向的 PGC 程序化生成, .trlvl 只是
    /// 加速缓存。手绘的高度没法反写回算法, 所以只记录"相对基底的增量", 加载时叠回去。
    ///
    /// 只保留一个包围盒 + 盒内 float 块 (盒外视为 0), 按需分配、随编辑扩张。
    /// 不保留基底副本: 四种笔刷最终都落到 SourceHeightMap.SetPixel(x, y, newHeight), 且
    /// oldHeight / newHeight 同处"基底 + 已有 delta"域, 所以在同一处累积 (newHeight - oldHeight)
    /// 得到的就是相对基底的 delta。
    /// </summary>
    public class TtTerrainLevelHeightOverlay
    {
        public const uint CurrentVersion = 1;
        /// <summary>
        /// 包围盒扩张的对齐粒度。Accumulate 是每 texel 一次的调用频率, 不对齐的话
        /// 一次拖动会触发上万次重分配。
        /// </summary>
        const int AllocAlign = 64;

        public int LevelX { get; private set; }
        public int LevelZ { get; private set; }

        FTerrainDirtyRect mRect = FTerrainDirtyRect.Empty;
        float[] mDeltas = null;
        /// <summary>
        /// 上次落盘时的内容 hash。为 null 表示内容已变 (或从未落盘), 需要重新写。
        /// </summary>
        string mSaveHash = null;

        public TtTerrainLevelHeightOverlay(int levelX, int levelZ)
        {
            LevelX = levelX;
            LevelZ = levelZ;
        }

        public FTerrainDirtyRect Rect
        {
            get { return mRect; }
        }
        public bool IsEmpty
        {
            get { return mDeltas == null || mRect.IsValid == false; }
        }
        public string SaveHash
        {
            get { return mSaveHash; }
        }

        static int AlignDown(int v)
        {
            // C# 的整数除法对负数是向零取整, 直接 v / AllocAlign * AllocAlign 会把 -1 对齐到 0。
            int q = v >= 0 ? v / AllocAlign : (v - AllocAlign + 1) / AllocAlign;
            return q * AllocAlign;
        }
        void SureCapacity(int x, int y)
        {
            if (mRect.IsValid && x >= mRect.MinX && x <= mRect.MaxX && y >= mRect.MinY && y <= mRect.MaxY)
                return;

            int ax = AlignDown(x);
            int ay = AlignDown(y);
            var newRect = FTerrainDirtyRect.FromBound(ax, ay, ax + AllocAlign - 1, ay + AllocAlign - 1);
            newRect.Union(in mRect);

            var newDeltas = new float[newRect.Width * newRect.Height];
            if (mDeltas != null && mRect.IsValid)
            {
                int oldW = mRect.Width;
                int newW = newRect.Width;
                for (int sy = mRect.MinY; sy <= mRect.MaxY; sy++)
                {
                    Array.Copy(mDeltas, (sy - mRect.MinY) * oldW,
                        newDeltas, (sy - newRect.MinY) * newW + (mRect.MinX - newRect.MinX), oldW);
                }
            }

            mRect = newRect;
            mDeltas = newDeltas;
        }

        /// <summary>
        /// 累加一个 texel 的 delta。delta 是本次写入相对写入前的差值, 不是相对基底的绝对差值 ——
        /// 多次编辑同一 texel 靠累加得到最终相对基底的偏移。
        /// </summary>
        public void Accumulate(int x, int y, float delta)
        {
            if (delta == 0.0f)
                return;

            SureCapacity(x, y);
            mDeltas[(y - mRect.MinY) * mRect.Width + (x - mRect.MinX)] += delta;
            mSaveHash = null;
        }
        public float GetDelta(int x, int y)
        {
            if (IsEmpty)
                return 0.0f;
            if (x < mRect.MinX || x > mRect.MaxX || y < mRect.MinY || y > mRect.MaxY)
                return 0.0f;
            return mDeltas[(y - mRect.MinY) * mRect.Width + (x - mRect.MinX)];
        }

        /// <summary>
        /// 把 delta 叠加到高度图上。返回实际被改动的 texel 区域 (非法表示什么都没改)。
        /// 调用方必须保证 hMap 不是会被写进 .trlvl 缓存的那份原始 buffer, 否则 delta 会被
        /// 烤进基底, 下次加载再叠一次。
        /// </summary>
        public FTerrainDirtyRect ApplyTo(Procedure.TtBufferComponent hMap)
        {
            var applied = FTerrainDirtyRect.Empty;
            if (hMap == null || IsEmpty)
                return applied;

            var rect = mRect;
            rect.ClampTo(0, 0, hMap.Width - 1, hMap.Height - 1);
            if (rect.IsValid == false)
                return applied;

            int w = mRect.Width;
            for (int y = rect.MinY; y <= rect.MaxY; y++)
            {
                for (int x = rect.MinX; x <= rect.MaxX; x++)
                {
                    float d = mDeltas[(y - mRect.MinY) * w + (x - mRect.MinX)];
                    if (d == 0.0f)
                        continue;
                    hMap.SetPixel(x, y, hMap.GetPixel<float>(x, y) + d);
                    applied.Include(x, y);
                }
            }
            return applied;
        }

        string ComputeContentHash()
        {
            if (IsEmpty)
                return null;

            var header = new int[6] { LevelX, LevelZ, mRect.MinX, mRect.MinY, mRect.MaxX, mRect.MaxY };
            var bytes = new byte[header.Length * sizeof(int) + mDeltas.Length * sizeof(float)];
            Buffer.BlockCopy(header, 0, bytes, 0, header.Length * sizeof(int));
            Buffer.BlockCopy(mDeltas, 0, bytes, header.Length * sizeof(int), mDeltas.Length * sizeof(float));
            return IO.TtFileInfo.ComputeSHA256Hash(bytes);
        }

        /// <summary>
        /// 写出 .thl。返回内容 hash (供 levellist.txt 记录), 内容未变且文件仍在时跳过写盘。
        /// </summary>
        public unsafe string SaveToFile(string file)
        {
            if (IsEmpty)
                return null;

            var hash = ComputeContentHash();
            if (hash == mSaveHash && IO.TtFileManager.FileExists(file))
                return hash;

            using (var xnd = new IO.TtXndHolder("TrHeightOverlay", CurrentVersion, 0))
            {
                using (var attr = xnd.NewAttribute("Desc", CurrentVersion, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    using (var ar = attr.GetWriter(32))
                    {
                        ar.Write(CurrentVersion);
                        ar.Write(LevelX);
                        ar.Write(LevelZ);
                        ar.Write(mRect.MinX);
                        ar.Write(mRect.MinY);
                        ar.Write(mRect.MaxX);
                        ar.Write(mRect.MaxY);
                    }
                }
                using (var attr = xnd.NewAttribute("Delta", CurrentVersion, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    using (var ar = attr.GetWriter((ulong)(mDeltas.Length * sizeof(float))))
                    {
                        fixed (float* p = &mDeltas[0])
                        {
                            ar.WritePtr(p, mDeltas.Length * sizeof(float));
                        }
                    }
                }
                xnd.SaveXnd(file);
            }

            mSaveHash = hash;
            return hash;
        }
        public unsafe bool LoadFromFile(string file)
        {
            if (IO.TtFileManager.FileExists(file) == false)
                return false;

            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return false;

                var descAttr = xnd.RootNode.TryGetAttribute("Desc");
                if (descAttr.IsValidPointer == false)
                    return false;

                uint version;
                int levelX, levelZ, minX, minY, maxX, maxY;
                using (var ar = descAttr.GetReader(null))
                {
                    ar.Read(out version);
                    ar.Read(out levelX);
                    ar.Read(out levelZ);
                    ar.Read(out minX);
                    ar.Read(out minY);
                    ar.Read(out maxX);
                    ar.Read(out maxY);
                }
                if (version > CurrentVersion)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, "TerrainOverlay",
                        $"unsupported overlay version {version} in {file}");
                    return false;
                }

                var rect = FTerrainDirtyRect.FromBound(minX, minY, maxX, maxY);
                if (rect.IsValid == false)
                    return false;

                var deltaAttr = xnd.RootNode.TryGetAttribute("Delta");
                if (deltaAttr.IsValidPointer == false)
                    return false;

                var deltas = new float[rect.Width * rect.Height];
                using (var ar = deltaAttr.GetReader(null))
                {
                    fixed (float* p = &deltas[0])
                    {
                        ar.ReadPtr(p, deltas.Length * sizeof(float));
                    }
                }

                LevelX = levelX;
                LevelZ = levelZ;
                mRect = rect;
                mDeltas = deltas;
                mSaveHash = ComputeContentHash();
            }
            return true;
        }
    }

    /// <summary>
    /// 一个地形节点的全部 level 覆盖层。
    ///
    /// 落盘位置在 scene 目录下, 组织方式仿 nodes (不进资产系统, 没有 ameta):
    ///   xxx.scene/terrainheight/{TerrainNodeId}/{levelX}_{levelZ}.thl
    ///   xxx.scene/terrainheight/{TerrainNodeId}/levellist.txt   ({levelX}_{levelZ}#{SHA256} 每行一条)
    ///
    /// level 文件按需懒加载 —— 100×100 个 level 全读一遍是不可接受的。levellist.txt 的作用
    /// 就是让"某个 level 到底有没有 delta"这个判断不需要碰文件系统。
    /// </summary>
    public class TtTerrainHeightOverlay
    {
        public const string OverlayExt = ".thl";
        public const string OverlayDirName = "terrainheight";
        public const string LevelListName = "levellist.txt";

        TtTerrainNode mNode;
        Dictionary<long, TtTerrainLevelHeightOverlay> mLevels = new Dictionary<long, TtTerrainLevelHeightOverlay>();
        /// <summary>
        /// 磁盘上已有的 level → hash。未加载进内存的 level 也在这里, 保存时要原样带上,
        /// 否则一次编辑就会把没碰过的 level 从 levellist.txt 里抹掉。
        /// </summary>
        Dictionary<long, string> mDiskHashes = new Dictionary<long, string>();
        bool mDiskListLoaded = false;

        public TtTerrainHeightOverlay(TtTerrainNode node)
        {
            mNode = node;
        }

        static long MakeKey(int levelX, int levelZ)
        {
            return ((long)levelZ << 32) | (uint)levelX;
        }
        static string MakeLevelKeyString(int levelX, int levelZ)
        {
            return $"{levelX}_{levelZ}";
        }
        static bool ParseLevelKeyString(string text, out int levelX, out int levelZ)
        {
            levelX = 0;
            levelZ = 0;
            var idx = text.IndexOf('_');
            if (idx <= 0 || idx >= text.Length - 1)
                return false;
            return int.TryParse(text.Substring(0, idx), out levelX)
                && int.TryParse(text.Substring(idx + 1), out levelZ);
        }

        string GetOverlayDir(GamePlay.Scene.TtScene scene)
        {
            if (scene == null || scene.AssetName == null)
                return null;
            var dir = IO.TtFileManager.CombinePath(scene.AssetName.Address, OverlayDirName);
            return IO.TtFileManager.CombinePath(dir, mNode.NodeId.ToString());
        }
        string GetOverlayDirForLoad()
        {
            return GetOverlayDir(mNode.ParentScene);
        }

        void SureDiskList()
        {
            if (mDiskListLoaded)
                return;
            mDiskListLoaded = true;

            var dir = GetOverlayDirForLoad();
            if (dir == null)
            {
                // scene 还没挂上时不能把"没有列表"当成结论, 下次再试。
                mDiskListLoaded = false;
                return;
            }

            var listFile = IO.TtFileManager.CombinePath(dir, LevelListName);
            if (IO.TtFileManager.FileExists(listFile) == false)
                return;

            var text = IO.TtFileManager.ReadAllText(listFile);
            if (string.IsNullOrEmpty(text))
                return;

            var lines = text.Split('\n');
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0)
                    continue;
                var sp = line.Split('#');
                if (ParseLevelKeyString(sp[0], out var levelX, out var levelZ) == false)
                    continue;
                mDiskHashes[MakeKey(levelX, levelZ)] = sp.Length > 1 ? sp[1] : "";
            }
        }

        public TtTerrainLevelHeightOverlay FindLevel(int levelX, int levelZ)
        {
            mLevels.TryGetValue(MakeKey(levelX, levelZ), out var result);
            return result;
        }
        /// <summary>
        /// 取出 (必要时创建) 某个 level 的覆盖层, 供编辑累积 delta。
        /// </summary>
        public TtTerrainLevelHeightOverlay GetOrCreateLevel(int levelX, int levelZ)
        {
            var key = MakeKey(levelX, levelZ);
            if (mLevels.TryGetValue(key, out var result))
                return result;

            // 先看盘上有没有已存的 delta, 有就接着它累积, 不然一次编辑会把旧 delta 冲掉。
            result = TryLoadLevel(levelX, levelZ);
            if (result == null)
            {
                result = new TtTerrainLevelHeightOverlay(levelX, levelZ);
                mLevels.Add(key, result);
            }
            return result;
        }
        /// <summary>
        /// 供 level 构建时使用: 已在内存里就直接返回, 否则按 levellist 判断有没有盘上数据再懒加载。
        /// 没有 delta 时返回 null, 不会凭空建一个空对象。
        /// </summary>
        public TtTerrainLevelHeightOverlay TryLoadLevel(int levelX, int levelZ)
        {
            var key = MakeKey(levelX, levelZ);
            if (mLevels.TryGetValue(key, out var cached))
                return cached;

            SureDiskList();
            if (mDiskHashes.ContainsKey(key) == false)
                return null;

            var dir = GetOverlayDirForLoad();
            if (dir == null)
                return null;

            var file = IO.TtFileManager.CombinePath(dir, MakeLevelKeyString(levelX, levelZ) + OverlayExt);
            var overlay = new TtTerrainLevelHeightOverlay(levelX, levelZ);
            if (overlay.LoadFromFile(file) == false)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, "TerrainOverlay",
                    $"level overlay listed but unreadable: {file}");
                return null;
            }

            mLevels.Add(key, overlay);
            return overlay;
        }

        /// <summary>
        /// 随 scene 保存。由 TtTerrainNode.OnSaveNodeExtraData 调用。
        /// </summary>
        public void Save(GamePlay.Scene.TtScene scene)
        {
            if (mLevels.Count == 0)
                return;

            var dir = GetOverlayDir(scene);
            if (dir == null)
                return;

            SureDiskList();
            IO.TtFileManager.SureDirectory(dir);

            // 从盘上已有的条目起底, 只覆盖本次内存里有的 level。
            var finalHashes = new Dictionary<long, string>(mDiskHashes);
            foreach (var kv in mLevels)
            {
                var overlay = kv.Value;
                if (overlay.IsEmpty)
                    continue;

                var file = IO.TtFileManager.CombinePath(dir, MakeLevelKeyString(overlay.LevelX, overlay.LevelZ) + OverlayExt);
                var hash = overlay.SaveToFile(file);
                if (hash == null)
                    continue;
                TtEngine.Instance.SourceControlModule.AddFile(file, true);
                finalHashes[kv.Key] = hash;
            }

            string list = "";
            foreach (var kv in finalHashes)
            {
                int levelX = (int)(uint)(kv.Key & 0xFFFFFFFF);
                int levelZ = (int)(kv.Key >> 32);
                list += MakeLevelKeyString(levelX, levelZ) + '#' + kv.Value + '\n';
            }
            var listFile = IO.TtFileManager.CombinePath(dir, LevelListName);
            IO.TtFileManager.WriteAllText(listFile, list);
            TtEngine.Instance.SourceControlModule.AddFile(listFile, true);

            mDiskHashes = finalHashes;
        }
    }
}

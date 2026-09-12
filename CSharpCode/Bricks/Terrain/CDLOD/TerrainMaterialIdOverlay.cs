using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// 单个 level 的材质 ID 覆盖层: 手绘 texel 的**绝对 ID**。
    ///
    /// 与高度覆盖层的 delta 不同 —— ID 是 MaterialIdArray 的整数下标, 没有可加性,
    /// 存"相对基底的增量"没有意义。存绝对值的语义也更合理: PGC 基底变更 (改了高度分层
    /// 规则、换了材质列表) 之后, 手绘区仍然是用户当初指定的那个材质, 未手绘区跟着新基底走。
    ///
    /// 需要 mMask 而不能靠"ID != 0 表示有效": 0 是一个完全合法的材质下标, 用户完全可能
    /// 刻意把某块地刷成 0 号材质, 而基底是别的 ID。
    /// </summary>
    public class TtTerrainLevelMaterialIdOverlay : ITtTerrainLevelOverlay
    {
        public const uint CurrentVersion = 1;
        /// <summary>
        /// 包围盒扩张的对齐粒度。Set 是每 texel 一次的调用频率, 不对齐的话
        /// 一次拖动会触发上万次重分配。
        /// </summary>
        const int AllocAlign = 64;

        public int LevelX { get; private set; }
        public int LevelZ { get; private set; }

        FTerrainDirtyRect mRect = FTerrainDirtyRect.Empty;
        byte[] mIds = null;
        byte[] mMask = null;
        /// <summary>
        /// 已置位的 texel 数。让 IsEmpty 精确 —— 全部 Clear 掉之后应该等同于没有覆盖层,
        /// 否则会往盘上写一个纯零文件, 并且 ApplyTo 白跑一遍整个包围盒。
        /// </summary>
        int mSetCount = 0;
        /// <summary>
        /// 上次落盘时的内容 hash。为 null 表示内容已变 (或从未落盘), 需要重新写。
        /// </summary>
        string mSaveHash = null;

        public TtTerrainLevelMaterialIdOverlay(int levelX, int levelZ)
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
            get { return mIds == null || mRect.IsValid == false || mSetCount == 0; }
        }
        public int SetCount
        {
            get { return mSetCount; }
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

            var newIds = new byte[newRect.Width * newRect.Height];
            var newMask = new byte[newRect.Width * newRect.Height];
            if (mIds != null && mRect.IsValid)
            {
                int oldW = mRect.Width;
                int newW = newRect.Width;
                for (int sy = mRect.MinY; sy <= mRect.MaxY; sy++)
                {
                    int srcOff = (sy - mRect.MinY) * oldW;
                    int dstOff = (sy - newRect.MinY) * newW + (mRect.MinX - newRect.MinX);
                    Array.Copy(mIds, srcOff, newIds, dstOff, oldW);
                    Array.Copy(mMask, srcOff, newMask, dstOff, oldW);
                }
            }

            mRect = newRect;
            mIds = newIds;
            mMask = newMask;
        }

        /// <summary>
        /// 记录一个 texel 的手绘 ID。
        /// </summary>
        public void Set(int x, int y, byte id)
        {
            SureCapacity(x, y);
            int i = (y - mRect.MinY) * mRect.Width + (x - mRect.MinX);
            if (mMask[i] != 0 && mIds[i] == id)
                return;
            if (mMask[i] == 0)
                mSetCount++;
            mMask[i] = 1;
            mIds[i] = id;
            mSaveHash = null;
        }
        /// <summary>
        /// 撤掉一个 texel 的手绘记录 (该 texel 回归 PGC 基底)。
        /// 盒外的 texel 本来就没有记录, 不为它扩容。
        /// </summary>
        public void Clear(int x, int y)
        {
            if (mIds == null || mRect.IsValid == false)
                return;
            if (x < mRect.MinX || x > mRect.MaxX || y < mRect.MinY || y > mRect.MaxY)
                return;

            int i = (y - mRect.MinY) * mRect.Width + (x - mRect.MinX);
            if (mMask[i] == 0)
                return;
            mMask[i] = 0;
            mIds[i] = 0;
            mSetCount--;
            mSaveHash = null;
        }
        public bool HasId(int x, int y)
        {
            if (mIds == null || mRect.IsValid == false)
                return false;
            if (x < mRect.MinX || x > mRect.MaxX || y < mRect.MinY || y > mRect.MaxY)
                return false;
            return mMask[(y - mRect.MinY) * mRect.Width + (x - mRect.MinX)] != 0;
        }
        public byte GetId(int x, int y)
        {
            if (HasId(x, y) == false)
                return 0;
            return mIds[(y - mRect.MinY) * mRect.Width + (x - mRect.MinX)];
        }

        /// <summary>
        /// 把手绘 ID 覆盖到 ID 图上。返回实际被改动的 texel 区域 (非法表示什么都没改)。
        /// 调用方必须保证 idMap 不是会被写进 .trlvl 缓存的那份原始 buffer。
        /// </summary>
        public FTerrainDirtyRect ApplyTo(Procedure.TtBufferComponent idMap)
        {
            var applied = FTerrainDirtyRect.Empty;
            if (idMap == null || IsEmpty)
                return applied;

            var rect = mRect;
            rect.ClampTo(0, 0, idMap.Width - 1, idMap.Height - 1);
            if (rect.IsValid == false)
                return applied;

            int w = mRect.Width;
            for (int y = rect.MinY; y <= rect.MaxY; y++)
            {
                for (int x = rect.MinX; x <= rect.MaxX; x++)
                {
                    int i = (y - mRect.MinY) * w + (x - mRect.MinX);
                    if (mMask[i] == 0)
                        continue;
                    idMap.SetPixel(x, y, (float)mIds[i]);
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
            var bytes = new byte[header.Length * sizeof(int) + mIds.Length + mMask.Length];
            Buffer.BlockCopy(header, 0, bytes, 0, header.Length * sizeof(int));
            Buffer.BlockCopy(mIds, 0, bytes, header.Length * sizeof(int), mIds.Length);
            Buffer.BlockCopy(mMask, 0, bytes, header.Length * sizeof(int) + mIds.Length, mMask.Length);
            return IO.TtFileInfo.ComputeSHA256Hash(bytes);
        }

        /// <summary>
        /// 写出 .tml。返回内容 hash (供 levellist.txt 记录), 内容未变且文件仍在时跳过写盘。
        /// </summary>
        public unsafe string SaveToFile(string file)
        {
            if (IsEmpty)
                return null;

            var hash = ComputeContentHash();
            if (hash == mSaveHash && IO.TtFileManager.FileExists(file))
                return hash;

            using (var xnd = new IO.TtXndHolder("TrMaterialIdOverlay", CurrentVersion, 0))
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
                using (var attr = xnd.NewAttribute("Ids", CurrentVersion, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    using (var ar = attr.GetWriter((ulong)mIds.Length))
                    {
                        fixed (byte* p = &mIds[0])
                        {
                            ar.WritePtr(p, mIds.Length);
                        }
                    }
                }
                using (var attr = xnd.NewAttribute("Mask", CurrentVersion, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    using (var ar = attr.GetWriter((ulong)mMask.Length))
                    {
                        fixed (byte* p = &mMask[0])
                        {
                            ar.WritePtr(p, mMask.Length);
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

                var idsAttr = xnd.RootNode.TryGetAttribute("Ids");
                var maskAttr = xnd.RootNode.TryGetAttribute("Mask");
                if (idsAttr.IsValidPointer == false || maskAttr.IsValidPointer == false)
                    return false;

                int count = rect.Width * rect.Height;
                var ids = new byte[count];
                var mask = new byte[count];
                using (var ar = idsAttr.GetReader(null))
                {
                    fixed (byte* p = &ids[0])
                    {
                        ar.ReadPtr(p, count);
                    }
                }
                using (var ar = maskAttr.GetReader(null))
                {
                    fixed (byte* p = &mask[0])
                    {
                        ar.ReadPtr(p, count);
                    }
                }

                int setCount = 0;
                for (int i = 0; i < count; i++)
                {
                    if (mask[i] != 0)
                        setCount++;
                }

                LevelX = levelX;
                LevelZ = levelZ;
                mRect = rect;
                mIds = ids;
                mMask = mask;
                mSetCount = setCount;
                mSaveHash = ComputeContentHash();
            }
            return true;
        }
    }

    /// <summary>
    /// 材质 ID 覆盖层的节点级管理器。落盘到 xxx.scene/terrainmaterialid/{NodeId}/*.tml。
    /// </summary>
    public class TtTerrainMaterialIdOverlay : TtTerrainOverlayManager<TtTerrainLevelMaterialIdOverlay>
    {
        public TtTerrainMaterialIdOverlay(TtTerrainNode node)
            : base(node)
        {
        }

        protected override string OverlayDirName
        {
            get { return "terrainmaterialid"; }
        }
        protected override string OverlayExt
        {
            get { return ".tml"; }
        }
        protected override TtTerrainLevelMaterialIdOverlay CreateLevel(int levelX, int levelZ)
        {
            return new TtTerrainLevelMaterialIdOverlay(levelX, levelZ);
        }
    }
}

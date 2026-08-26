using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// 一次地形高度编辑的可撤销命令。粒度是"一次拖动 = 一步", 不是每笔一步 ——
    /// 笔刷在拖动中每帧会落好几笔, 逐笔入栈会让 Ctrl+Z 变成一次只退一个像素点的噩梦。
    ///
    /// 高度块的读写直接复用 UTerrainLevelData.GetHeightBlock / SetHeightBlock,
    /// 后者内部会走 MarkHeightDirty + FlushDirty (GPU 局部上传 + 法线重算 + patch AABB
    /// + physx heightfield 重建), 所以 Undo 之后画面和物理都是一致的。
    /// </summary>
    public class TtTerrainHeightCommand : Editor.Infrastructure.TtEditorCommand
    {
        UTerrainLevelData mLevelData;
        FTerrainDirtyRect mRect;
        float[] mBefore;
        float[] mAfter;

        public TtTerrainHeightCommand(string name, UTerrainLevelData levelData, in FTerrainDirtyRect rect, float[] before, float[] after)
        {
            Name = name;
            mLevelData = levelData;
            mRect = rect;
            mBefore = before;
            mAfter = after;
        }

        public override void Do()
        {
            Apply(mAfter);
        }
        public override void Undo()
        {
            Apply(mBefore);
        }
        void Apply(float[] heights)
        {
            if (mLevelData == null || heights == null)
                return;
            // level 可能已经被流式卸载 (SourceHeightMap 被 Dispose 成空壳), 此时静默跳过:
            // 重新加载时会从覆盖层重建, 结果与这条命令的目标状态一致。
            if (mLevelData.IsEditable == false)
                return;
            mLevelData.SetHeightBlock(in mRect, heights, true, true);
        }
    }

    /// <summary>
    /// 拖动期间的 undo 快照记录器。
    ///
    /// 难点在于"拖动结束后的包围盒"在拖动开始时是未知的, 而为了拿 before 去全量复制整个
    /// level (1024² × float = 4MB) 又太浪费。做法是增量补记: 每落一笔之前, 先把这一笔
    /// **新扩进包围盒的那部分 texel** 的当前值补记进 before 块 —— 已经在盒里的 texel
    /// 保持首次记录的原值不动。
    /// </summary>
    public class TtTerrainHeightDragRecorder
    {
        UTerrainLevelData mLevelData;
        FTerrainDirtyRect mRect = FTerrainDirtyRect.Empty;
        float[] mBefore = null;

        public bool IsRecording
        {
            get { return mLevelData != null; }
        }
        public UTerrainLevelData LevelData
        {
            get { return mLevelData; }
        }

        public void Reset()
        {
            mLevelData = null;
            mRect = FTerrainDirtyRect.Empty;
            mBefore = null;
        }

        /// <summary>
        /// 在 ApplyHeightBrush 之前调用, 传入这一笔将要覆盖的 texel 区域。
        ///
        /// 跨 level 的拖动直接从新 level 重新开始记录 (返回 true 表示调用方应该先把
        /// 之前那段封成一条命令) —— 一条命令只描述一个 level 的一块矩形。
        /// </summary>
        public bool PreStroke(UTerrainLevelData levelData, in FTerrainDirtyRect strokeRect)
        {
            if (levelData == null || strokeRect.IsValid == false)
                return false;

            bool levelChanged = mLevelData != null && mLevelData != levelData;
            if (levelChanged)
                return true;

            mLevelData = levelData;

            var newRect = strokeRect;
            newRect.Union(in mRect);
            if (mBefore != null && newRect.MinX == mRect.MinX && newRect.MinY == mRect.MinY
                && newRect.MaxX == mRect.MaxX && newRect.MaxY == mRect.MaxY)
            {
                // 盒子没变, before 已经覆盖到这一笔的全部 texel。
                return false;
            }

            // 先按新盒子整体取一份当前高度 (新扩进来的 texel 得到的就是正确的原值),
            // 再把旧盒子那块用旧记录覆盖回去 (它们已经被改过, 当前值不是原值)。
            var newBefore = mLevelData.GetHeightBlock(in newRect);
            if (newBefore == null)
                return false;

            if (mBefore != null && mRect.IsValid)
            {
                int oldW = mRect.Width;
                int newW = newRect.Width;
                for (int y = mRect.MinY; y <= mRect.MaxY; y++)
                {
                    Array.Copy(mBefore, (y - mRect.MinY) * oldW,
                        newBefore, (y - newRect.MinY) * newW + (mRect.MinX - newRect.MinX), oldW);
                }
            }

            mRect = newRect;
            mBefore = newBefore;
            return false;
        }

        /// <summary>
        /// 抬手时调用。取当前高度作 after, 与 before 比对; 完全没变化则返回 null (不产生历史记录)。
        /// 无论返回什么, 记录器都会被重置。
        /// </summary>
        public TtTerrainHeightCommand BuildCommand(string name)
        {
            var levelData = mLevelData;
            var rect = mRect;
            var before = mBefore;
            Reset();

            if (levelData == null || before == null || rect.IsValid == false)
                return null;

            var after = levelData.GetHeightBlock(in rect);
            if (after == null || after.Length != before.Length)
                return null;

            bool changed = false;
            for (int i = 0; i < after.Length; i++)
            {
                if (after[i] != before[i])
                {
                    changed = true;
                    break;
                }
            }
            if (changed == false)
                return null;

            return new TtTerrainHeightCommand(name, levelData, in rect, before, after);
        }
    }
}

using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// 一次地形材质 ID 编辑的可撤销命令。粒度与高度一致: "一次拖动 = 一步"。
    ///
    /// 与 TtTerrainHeightCommand 同构, 只是数据类型换成 byte[] (ID 是 MaterialIdArray 的下标)。
    /// 读写走 UTerrainLevelData.GetMaterialIdBlock / SetMaterialIdBlock, 后者内部
    /// MarkMaterialIdDirty + FlushMaterialIdDirty (GPU 局部上传 + RVT 标脏), 并且会按
    /// "新 ID == 基底 ID 就 Clear, 否则 Set" 同步维护覆盖层, 所以 Undo 之后画面与落盘状态一致。
    /// 材质 ID 不参与高度编码, 无需重算法线 / patch AABB / physx。
    /// </summary>
    public class TtTerrainMaterialIdCommand : Editor.Infrastructure.TtEditorCommand
    {
        UTerrainLevelData mLevelData;
        FTerrainDirtyRect mRect;
        byte[] mBefore;
        byte[] mAfter;

        public TtTerrainMaterialIdCommand(string name, UTerrainLevelData levelData, in FTerrainDirtyRect rect, byte[] before, byte[] after)
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
        void Apply(byte[] ids)
        {
            if (mLevelData == null || ids == null)
                return;
            // level 可能已经被流式卸载 (SourceMaterialIdMap 置空), 此时静默跳过:
            // 重新加载时会从覆盖层重建, 结果与这条命令的目标状态一致。
            if (mLevelData.IsMaterialIdEditable == false)
                return;
            mLevelData.SetMaterialIdBlock(in mRect, ids, true);
        }
    }

    /// <summary>
    /// 材质 ID 拖动期间的 undo 快照记录器。与 TtTerrainHeightDragRecorder 同构:
    /// 每落一笔之前, 只把这一笔**新扩进包围盒的那部分 texel** 的当前值补记进 before 块。
    /// </summary>
    public class TtTerrainMaterialIdDragRecorder
    {
        UTerrainLevelData mLevelData;
        FTerrainDirtyRect mRect = FTerrainDirtyRect.Empty;
        byte[] mBefore = null;

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
        /// 在 ApplyMaterialBrush 之前调用, 传入这一笔将要覆盖的 texel 区域。
        /// 跨 level 的拖动返回 true, 表示调用方应该先把之前那段封成一条命令。
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

            // 先按新盒子整体取一份当前 ID (新扩进来的 texel 得到的就是正确的原值),
            // 再把旧盒子那块用旧记录覆盖回去 (它们已经被改过, 当前值不是原值)。
            var newBefore = mLevelData.GetMaterialIdBlock(in newRect);
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
        /// 抬手时调用。取当前 ID 作 after, 与 before 比对; 完全没变化则返回 null。
        /// 无论返回什么, 记录器都会被重置。
        /// </summary>
        public TtTerrainMaterialIdCommand BuildCommand(string name)
        {
            var levelData = mLevelData;
            var rect = mRect;
            var before = mBefore;
            Reset();

            if (levelData == null || before == null || rect.IsValid == false)
                return null;

            var after = levelData.GetMaterialIdBlock(in rect);
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

            return new TtTerrainMaterialIdCommand(name, levelData, in rect, before, after);
        }
    }
}

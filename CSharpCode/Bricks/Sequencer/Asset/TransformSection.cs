using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer.Asset
{
    /// <summary>
    /// 节点 Transform 片段: 位置三条标量通道 + 缩放三条标量通道 + 一条四元数旋转通道。
    ///
    /// 通道里的关键帧时刻是序列的绝对 tick, 不是相对 Section 起点的偏移。这样拖动
    /// Section 与拖动关键帧是两件互不影响的事, 编辑器实现简单; 代价是移动 Section
    /// 时要连带平移它所有关键帧 (见 TtTransformSection.ShiftKeys)。
    ///
    /// 某个分量没有任何关键帧时不覆盖目标的该分量: 只 K 了 Z 高度的轨道不应该把
    /// X/Y 拽回 0。做法是先把目标当前值读出来当底, 再覆盖有关键帧的分量。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtTransformSection : TtSequenceSection
    {
        [Rtti.Meta, System.ComponentModel.Category("Position")]
        public TtScalarChannel PositionX { get; set; } = new TtScalarChannel();
        [Rtti.Meta, System.ComponentModel.Category("Position")]
        public TtScalarChannel PositionY { get; set; } = new TtScalarChannel();
        [Rtti.Meta, System.ComponentModel.Category("Position")]
        public TtScalarChannel PositionZ { get; set; } = new TtScalarChannel();

        [Rtti.Meta, System.ComponentModel.Category("Scale")]
        public TtScalarChannel ScaleX { get; set; } = new TtScalarChannel();
        [Rtti.Meta, System.ComponentModel.Category("Scale")]
        public TtScalarChannel ScaleY { get; set; } = new TtScalarChannel();
        [Rtti.Meta, System.ComponentModel.Category("Scale")]
        public TtScalarChannel ScaleZ { get; set; } = new TtScalarChannel();

        [Rtti.Meta, System.ComponentModel.Category("Rotation")]
        public TtQuatChannel Rotation { get; set; } = new TtQuatChannel();

        [System.ComponentModel.Browsable(false)]
        public bool HasPositionKey { get => PositionX.KeyCount > 0 || PositionY.KeyCount > 0 || PositionZ.KeyCount > 0; }
        [System.ComponentModel.Browsable(false)]
        public bool HasScaleKey { get => ScaleX.KeyCount > 0 || ScaleY.KeyCount > 0 || ScaleZ.KeyCount > 0; }
        [System.ComponentModel.Browsable(false)]
        public bool HasRotationKey { get => Rotation.KeyCount > 0; }

        public override void Evaluate(in FSectionEvalContext ctx, object target)
        {
            if (target == null || ctx.Table == null || ctx.Registry == null)
                return;

            // 阶段 1 不用 ctx.Weight: 交叉淡化要求先有"目标属性的当前值"作为混合基准,
            // 而基准应该是下层 Section 的求值结果而不是目标对象的当前值 (后者已经被上一帧
            // 写过了, 拿它混会自激)。这需要中间表支持分层, 留到阶段 3。在那之前
            // GetEaseWeight 只算不用 —— 不要以为设了 EaseIn 就已经生效。

            if (HasPositionKey)
            {
                var accessor = ctx.Registry.Find(TtNodeTransformAccessors.PositionId);
                if (accessor != null && accessor.Read(target) is DVector3 pos)
                {
                    if (PositionX.KeyCount > 0)
                        pos.X = PositionX.Evaluate(ctx.Time, ctx.TickResolution);
                    if (PositionY.KeyCount > 0)
                        pos.Y = PositionY.Evaluate(ctx.Time, ctx.TickResolution);
                    if (PositionZ.KeyCount > 0)
                        pos.Z = PositionZ.Evaluate(ctx.Time, ctx.TickResolution);
                    ctx.Table.Write(target, accessor, pos, OverlapPriority);
                }
            }
            if (HasScaleKey)
            {
                var accessor = ctx.Registry.Find(TtNodeTransformAccessors.ScaleId);
                if (accessor != null && accessor.Read(target) is Vector3 scale)
                {
                    if (ScaleX.KeyCount > 0)
                        scale.X = (float)ScaleX.Evaluate(ctx.Time, ctx.TickResolution);
                    if (ScaleY.KeyCount > 0)
                        scale.Y = (float)ScaleY.Evaluate(ctx.Time, ctx.TickResolution);
                    if (ScaleZ.KeyCount > 0)
                        scale.Z = (float)ScaleZ.Evaluate(ctx.Time, ctx.TickResolution);
                    ctx.Table.Write(target, accessor, scale, OverlapPriority);
                }
            }
            if (HasRotationKey)
            {
                var accessor = ctx.Registry.Find(TtNodeTransformAccessors.QuatId);
                if (accessor != null)
                    ctx.Table.Write(target, accessor, Rotation.Evaluate(ctx.Time), OverlapPriority);
            }
        }
        public override long GetMaxKeyTime()
        {
            long max = 0;
            max = Math.Max(max, PositionX.GetMaxTime());
            max = Math.Max(max, PositionY.GetMaxTime());
            max = Math.Max(max, PositionZ.GetMaxTime());
            max = Math.Max(max, ScaleX.GetMaxTime());
            max = Math.Max(max, ScaleY.GetMaxTime());
            max = Math.Max(max, ScaleZ.GetMaxTime());
            max = Math.Max(max, Rotation.GetMaxTime());
            return max;
        }

        /// <summary>本 Section 的全部通道, 顺序固定, 供编辑器按下标索引</summary>
        public override void GatherChannels(List<ISequenceChannel> result)
        {
            result.Add(PositionX);
            result.Add(PositionY);
            result.Add(PositionZ);
            result.Add(ScaleX);
            result.Add(ScaleY);
            result.Add(ScaleZ);
            result.Add(Rotation);
        }
    }
}

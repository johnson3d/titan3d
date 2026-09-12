using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer.Asset
{
    /// <summary>Section 结束后目标属性怎么处理, 对标 UE 的 EMovieSceneCompletionMode</summary>
    public enum ESectionCompletionMode
    {
        /// <summary>停在最后一帧的值上</summary>
        KeepState,
        /// <summary>恢复成播放之前的值</summary>
        RestoreState,
    }

    /// <summary>
    /// 一次 Section 求值需要的全部上下文。做成结构体传引用而不是一堆参数, 是因为
    /// 以后加东西 (曲线缓存、层级权重) 不必改所有 Section 子类的签名。
    /// </summary>
    public struct FSectionEvalContext
    {
        /// <summary>当前播放位置 (TickResolution 时基)</summary>
        public long Time;
        /// <summary>本 Section 在当前时刻的进出淡化权重, 0~1</summary>
        public float Weight;
        /// <summary>所属序列的内部时基, 三次插值算切线要用</summary>
        public TtFrameRate TickResolution;
        public TtSequenceEvalTable Table;
        public TtSequencePropertyRegistry Registry;
    }

    /// <summary>
    /// 轨道上的一个片段。时间区间、进出淡化、重叠优先级在这里, 具体动什么由子类定。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public abstract class TtSequenceSection : IO.BaseSerializer
    {
        [Rtti.Meta, System.ComponentModel.Category("Time")]
        public long StartTick { get; set; } = 0;
        [Rtti.Meta, System.ComponentModel.Category("Time")]
        public long EndTick { get; set; } = 0;
        /// <summary>进入淡化的长度 (tick), 0 表示不淡化</summary>
        [Rtti.Meta, System.ComponentModel.Category("Blend")]
        public long EaseInDuration { get; set; } = 0;
        [Rtti.Meta, System.ComponentModel.Category("Blend")]
        public long EaseOutDuration { get; set; } = 0;
        /// <summary>同一轨道里 Section 画在第几行, 用于让重叠的 Section 错开显示</summary>
        [Rtti.Meta, System.ComponentModel.Category("Blend")]
        public int RowIndex { get; set; } = 0;
        /// <summary>多个 Section 写同一属性时谁赢, 大的赢</summary>
        [Rtti.Meta, System.ComponentModel.Category("Blend")]
        public int OverlapPriority { get; set; } = 0;
        [Rtti.Meta, System.ComponentModel.Category("Blend")]
        public ESectionCompletionMode CompletionMode { get; set; } = ESectionCompletionMode.KeepState;

        [System.ComponentModel.Browsable(false)]
        public long Duration { get => EndTick - StartTick; }

        /// <summary>
        /// 时间区间取闭区间 [StartTick, EndTick], 两端都算命中。
        ///
        /// 这里没有跟 UE 一样用半开区间: UE 求值的是一个时间"范围"而不是一个时刻,
        /// 最后一帧走的是 [End-1, End)。我们按单个时刻求值, 用半开区间的话播放头停在
        /// 序列末尾 (通常正好等于末尾 Section 的 EndTick) 时会一个 Section 都命中不上,
        /// 表现成最后一帧突然弹回原值。闭区间的代价是首尾相接的两个 Section 会在接缝
        /// 那一个 tick 上同时命中, 由 OverlapPriority 决定谁赢, 这个代价可接受。
        /// </summary>
        public bool Contains(long time)
        {
            return time >= StartTick && time <= EndTick;
        }
        /// <summary>进出淡化权重。没设淡化长度时恒为 1。</summary>
        public float GetEaseWeight(long time)
        {
            float weight = 1.0f;
            if (EaseInDuration > 0)
            {
                var delta = time - StartTick;
                if (delta < EaseInDuration)
                    weight = delta > 0 ? (float)((double)delta / EaseInDuration) : 0.0f;
            }
            if (EaseOutDuration > 0)
            {
                var delta = EndTick - time;
                if (delta < EaseOutDuration)
                {
                    var w = delta > 0 ? (float)((double)delta / EaseOutDuration) : 0.0f;
                    if (w < weight)
                        weight = w;
                }
            }
            return weight;
        }
        /// <summary>把本 Section 在 ctx.Time 的求值结果写进 ctx.Table, 不要直接改 target</summary>
        public abstract void Evaluate(in FSectionEvalContext ctx, object target);
        /// <summary>本 Section 内部关键帧的最大时刻 (相对 Section 起点), 用于自动收拢 EndTick</summary>
        public virtual long GetMaxKeyTime()
        {
            return 0;
        }
        /// <summary>
        /// 本 Section 当前在用的通道。做成抽象方法而不是各子类自己一个同名方法:
        /// 编辑器的拖关键帧、删关键帧、快照 Undo 都只靠这一条拿通道, 不该知道
        /// Section 的子类型。顺序对同一个 Section 必须稳定 (快照按下标对应回写)。
        /// </summary>
        public abstract void GatherChannels(List<ISequenceChannel> result);
        /// <summary>
        /// 整体平移本 Section 所有通道的关键帧。关键帧存的是序列绝对 tick,
        /// 拖动 Section 必须连带平移它们。
        /// </summary>
        public virtual void ShiftKeys(long deltaTick)
        {
            if (deltaTick == 0)
                return;
            var channels = new List<ISequenceChannel>();
            GatherChannels(channels);
            for (int i = 0; i < channels.Count; ++i)
                channels[i].ShiftKeys(deltaTick);
        }
    }
}

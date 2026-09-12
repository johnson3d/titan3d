using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer.Asset
{
    /// <summary>
    /// 通用属性片段: 驱动任意一条注册过访问器的属性。
    ///
    /// 和 TtTransformSection 的分工: Transform 是"一次写三个属性 (位置/缩放/旋转)"的
    /// 复合片段, 通道是固定的七条命名成员; 本片段是"一条属性一个片段", 通道数量由
    /// 属性的值类型 (适配器) 决定。Transform 没有被本片段替代 —— 它把常用的三件事合成
    /// 一条轨道, 编辑体验更好。
    ///
    /// 三种通道容器都始终非 null 而不是按需分配: Rtti 的 List 序列化不写 null 标记,
    /// 可空引用会给存盘埋坑, 而空通道 (空 List) 的存储开销可以忽略。一个片段实际只会
    /// 用其中一种 —— 值类型决定了它落在标量、四元数还是资产引用上, 不会混用。
    ///
    /// 通道里的关键帧时刻是序列的绝对 tick (与 TtTransformSection 一致), 所以移动片段
    /// 必须连带平移关键帧, 见 ShiftKeys。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtPropertySection : TtSequenceSection
    {
        /// <summary>驱动哪条属性, 对应 ISequencePropertyAccessor.PropertyId</summary>
        [Rtti.Meta, System.ComponentModel.Category("Property")]
        [System.ComponentModel.ReadOnly(true)]
        public string PropertyId { get; set; } = "";
        /// <summary>
        /// 值怎么拆成通道, 对应 ISequenceValueAdapter.AdapterId。
        /// 和 PropertyId 分开存是因为一个适配器服务很多属性 (所有 DVector3 属性共用一个)。
        /// </summary>
        [Rtti.Meta, System.ComponentModel.Category("Property")]
        [System.ComponentModel.ReadOnly(true)]
        public string AdapterId { get; set; } = "";

        /// <summary>标量通道, 条数与顺序由适配器定义 (DVector3 就是 X/Y/Z 三条)</summary>
        [Rtti.Meta, System.ComponentModel.Browsable(false)]
        public List<TtScalarChannel> ScalarChannels { get; set; } = new List<TtScalarChannel>();
        [Rtti.Meta, System.ComponentModel.Browsable(false)]
        public TtQuatChannel QuatChannel { get; set; } = new TtQuatChannel();
        [Rtti.Meta, System.ComponentModel.Browsable(false)]
        public TtNameChannel NameChannel { get; set; } = new TtNameChannel();

        [System.ComponentModel.Browsable(false)]
        public bool HasAnyKey
        {
            get
            {
                for (int i = 0; i < ScalarChannels.Count; ++i)
                {
                    if (ScalarChannels[i] != null && ScalarChannels[i].KeyCount > 0)
                        return true;
                }
                return QuatChannel.KeyCount > 0 || NameChannel.KeyCount > 0;
            }
        }

        public ISequenceValueAdapter FindAdapter()
        {
            return TtSequenceValueAdapters.FindById(AdapterId);
        }
        /// <summary>
        /// 按适配器把标量通道补齐。只补不删 —— 老资产里多出来的通道留着, 删掉等于
        /// 静默丢用户的关键帧, 而适配器换了之后那些数据可能还有救。
        /// </summary>
        public void EnsureChannels(ISequenceValueAdapter adapter)
        {
            if (adapter == null)
                return;
            AdapterId = adapter.AdapterId;
            while (ScalarChannels.Count < adapter.ScalarChannelCount)
                ScalarChannels.Add(new TtScalarChannel());
        }
        /// <summary>在 tick 上按给定值打一个关键帧。value 一般是从访问器 Read 出来的当前值。</summary>
        public bool AddKeyFromValue(long tick, object value, in TtFrameRate tickResolution)
        {
            var adapter = FindAdapter();
            if (adapter == null || value == null)
                return false;
            EnsureChannels(adapter);
            adapter.AddKey(this, tick, value, in tickResolution);
            return true;
        }

        public override void Evaluate(in FSectionEvalContext ctx, object target)
        {
            if (target == null || ctx.Table == null || ctx.Registry == null)
                return;

            var accessor = ctx.Registry.Find(PropertyId);
            if (accessor == null)
                return;
            var adapter = FindAdapter();
            if (adapter == null)
                return;

            // current 给适配器兜没有关键帧的分量。它可能是 null: target 类型跟属性不匹配
            // (访问器约定返回 null), 也可能属性本来就是空引用 (RName)。两者的区别交给
            // 适配器判断 —— 标量适配器缺了底就不写, 资产适配器不需要底。
            var current = accessor.Read(target);
            var value = adapter.Evaluate(this, current, in ctx);
            if (value == null)
                return;
            ctx.Table.Write(target, accessor, value, OverlapPriority);
        }
        public override long GetMaxKeyTime()
        {
            long max = 0;
            for (int i = 0; i < ScalarChannels.Count; ++i)
            {
                if (ScalarChannels[i] != null)
                    max = Math.Max(max, ScalarChannels[i].GetMaxTime());
            }
            max = Math.Max(max, QuatChannel.GetMaxTime());
            max = Math.Max(max, NameChannel.GetMaxTime());
            return max;
        }
        /// <summary>
        /// 本片段的全部通道, 顺序固定 (标量按适配器顺序, 然后四元数, 然后资产引用),
        /// 供编辑器按下标索引。只给实际在用的那一种 —— 空通道不该占一行界面。
        /// </summary>
        public override void GatherChannels(List<ISequenceChannel> result)
        {
            var adapter = FindAdapter();
            var kind = adapter != null ? adapter.Kind : EChannelKind.Scalar;
            if (kind == EChannelKind.Scalar)
            {
                for (int i = 0; i < ScalarChannels.Count; ++i)
                {
                    if (ScalarChannels[i] != null)
                        result.Add(ScalarChannels[i]);
                }
            }
            else if (kind == EChannelKind.Quat)
                result.Add(QuatChannel);
            else
                result.Add(NameChannel);
        }
        /// <summary>
        /// 整体平移所有关键帧, 见类注释里关键帧用绝对 tick 的说明。
        ///
        /// 不走基类那份基于 GatherChannels 的实现: 那个只平移"当前适配器在用的"
        /// 通道, 而适配器找不到时它会当成标量通道 —— 那样旧资产里落在资产引用
        /// 通道上的关键帧会留在原地, 与 Section 脱节。
        /// </summary>
        public override void ShiftKeys(long deltaTick)
        {
            if (deltaTick == 0)
                return;
            for (int i = 0; i < ScalarChannels.Count; ++i)
            {
                var channel = ScalarChannels[i];
                if (channel == null)
                    continue;
                for (int j = 0; j < channel.Times.Count; ++j)
                    channel.Times[j] = channel.Times[j] + deltaTick;
            }
            for (int i = 0; i < QuatChannel.Times.Count; ++i)
                QuatChannel.Times[i] = QuatChannel.Times[i] + deltaTick;
            for (int i = 0; i < NameChannel.Times.Count; ++i)
                NameChannel.Times[i] = NameChannel.Times[i] + deltaTick;
        }
    }
}

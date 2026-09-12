using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer.Asset
{
    /// <summary>
    /// 一条轨道, 持有若干时间上不重叠 (或重叠但有优先级) 的 Section。
    ///
    /// 轨道自己不存关键帧, 只负责挑出当前时刻命中的 Section 并转发求值。分成
    /// 轨道/片段两层是为了让"同一个属性在时间轴上分段, 段与段之间能空开"成为
    /// 数据结构本身就支持的事, 而不是靠通道里补关键帧硬凑。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public abstract class TtSequenceTrack : IO.BaseSerializer
    {
        [Rtti.Meta, System.ComponentModel.Category("Option")]
        public string DisplayName { get; set; } = "Track";
        /// <summary>关掉后整条轨道不参与求值, 但数据保留</summary>
        [Rtti.Meta, System.ComponentModel.Category("Option")]
        public bool Muted { get; set; } = false;

        /// <summary>
        /// 元素类型是抽象基类, 靠 DataCopyer 的 WriteMetaObject 写实际类型 hash 实现多态。
        /// 注意它不写 null 标记, 所以这个列表里绝对不能出现 null 元素, 否则存盘时抛。
        /// </summary>
        [Rtti.Meta, System.ComponentModel.Browsable(false)]
        public List<TtSequenceSection> Sections { get; set; } = new List<TtSequenceSection>();

        [System.ComponentModel.Browsable(false)]
        public abstract string TrackTypeName { get; }

        public void AddSection(TtSequenceSection section)
        {
            if (section == null)
                return;
            Sections.Add(section);
        }
        public bool RemoveSection(TtSequenceSection section)
        {
            return Sections.Remove(section);
        }
        /// <summary>
        /// 把所有 Section 并进第一个: 关键帧全搬过去, 范围取并集, 切线重算。返回宿主
        /// Section (不足两个 Section 时直接返回现有的那个或 null, 不做任何改动)。
        ///
        /// 为何需要它: 一个 Section 只装一个关键帧时, 它的通道求值只能是常量 —— N 个各含
        /// 1 个关键帧的 Section 摊在时间轴上看起来和「N 个关键帧」一模一样, 但拖播放头时
        /// 值是阶梯式突变, 且突变点落在 Section 边界而不是关键帧上。早期的打点逻辑会攒出
        /// 这种形状, 已攒出来的资产靠这里救回来。
        ///
        /// skipped 是搬不动关键帧的通道数 (见 MoveKeys)。它不为 0 时调用方应该告知用户,
        /// 那意味着有关键帧跟着被删的 Section 一起没了。
        /// </summary>
        public TtSequenceSection MergeSectionsIntoFirst(in TtFrameRate tickResolution, out int skipped)
        {
            skipped = 0;
            if (Sections.Count < 2)
                return Sections.Count == 1 ? Sections[0] : null;

            var host = Sections[0];
            var hostChannels = new List<ISequenceChannel>();
            host.GatherChannels(hostChannels);

            long start = host.StartTick;
            long end = host.EndTick;
            for (int si = 1; si < Sections.Count; ++si)
            {
                var src = Sections[si];
                if (src == null)
                    continue;
                if (src.StartTick < start)
                    start = src.StartTick;
                if (src.EndTick > end)
                    end = src.EndTick;

                var srcChannels = new List<ISequenceChannel>();
                src.GatherChannels(srcChannels);
                // 通道按 GatherChannels 的固定顺序一一对应。数量不等说明两个 Section 不是同一
                // 类型, 那就不能按下标搬 —— 硬搬会把值错位到别的分量上。
                if (srcChannels.Count != hostChannels.Count)
                {
                    skipped += srcChannels.Count;
                    continue;
                }
                for (int ci = 0; ci < srcChannels.Count; ++ci)
                {
                    if (MoveKeys(hostChannels[ci], srcChannels[ci]) == false)
                        ++skipped;
                }
            }

            for (int si = Sections.Count - 1; si >= 1; --si)
                Sections.RemoveAt(si);

            host.StartTick = start;
            host.EndTick = end;
            // 切线一定要重算: 每个 Section 原先只有一个关键帧, 那种情况下 Auto 切线算出来是 0,
            // 直接留着的话合并完曲线在每个关键帧处都是平的, 看着仍像阶梯。
            for (int ci = 0; ci < hostChannels.Count; ++ci)
            {
                if (hostChannels[ci] != null)
                    hostChannels[ci].AutoComputeAllTangents(tickResolution);
            }
            return host;
        }
        /// <summary>
        /// 把 src 的全部关键帧搬进 dst。搬不了返回 false。
        ///
        /// 标量帧连 InterpMode 和切线一起搬 (走 AddKey 的 FScalarKey 重载), 否则合并会把
        /// 每个关键帧的插值方式重置成通道默认值。
        ///
        /// 别的通道类型 (如资产名) 没有类型无关的搬帧入口, 宁可报告搬不动也不要靠
        /// CaptureKeys/RestoreKeys 整块回写 —— 那会把宿主已有的关键帧顶掉。
        /// </summary>
        static bool MoveKeys(ISequenceChannel dst, ISequenceChannel src)
        {
            if (dst == null || src == null)
                return false;

            var dstScalar = dst as TtScalarChannel;
            var srcScalar = src as TtScalarChannel;
            if (dstScalar != null && srcScalar != null)
            {
                for (int i = 0; i < srcScalar.KeyCount; ++i)
                    dstScalar.AddKey(srcScalar.GetKeyTime(i), srcScalar.GetKey(i));
                return true;
            }

            var dstQuat = dst as TtQuatChannel;
            var srcQuat = src as TtQuatChannel;
            if (dstQuat != null && srcQuat != null)
            {
                for (int i = 0; i < srcQuat.KeyCount; ++i)
                    dstQuat.AddKey(srcQuat.GetKeyTime(i), srcQuat.GetKey(i));
                return true;
            }
            return false;
        }
        /// <summary>
        /// 求值当前时刻。命中多个 Section 时全部都求, 由中间表的 Priority 决定谁最终生效
        /// —— 这里不做"只挑优先级最高的那个"的短路, 因为不同 Section 可能动的是不同分量。
        /// </summary>
        public void Evaluate(in FSectionEvalContext ctx, object target)
        {
            if (Muted)
                return;
            for (int i = 0; i < Sections.Count; ++i)
            {
                var section = Sections[i];
                if (section == null || section.Contains(ctx.Time) == false)
                    continue;
                var subCtx = ctx;
                subCtx.Weight = section.GetEaseWeight(ctx.Time);
                section.Evaluate(in subCtx, target);
            }
        }
        /// <summary>本轨道所有 Section 的最大 EndTick, 用于自动推算序列长度</summary>
        public long GetMaxTick()
        {
            long max = 0;
            for (int i = 0; i < Sections.Count; ++i)
            {
                if (Sections[i] == null)
                    continue;
                max = Math.Max(max, Sections[i].EndTick);
            }
            return max;
        }
    }

    /// <summary>节点 Transform 轨道, Section 类型固定为 TtTransformSection</summary>
    [Rtti.Meta("")]
    public class TtTransformTrack : TtSequenceTrack
    {
        public TtTransformTrack()
        {
            DisplayName = "Transform";
        }
        public override string TrackTypeName { get => "Transform"; }

        /// <summary>
        /// 拿到该把关键帧打进哪个 Section: 优先覆盖 tick 的, 否则是最接近 tick 的那个 (由调用方
        /// 负责把它的范围扩到 tick), 轨道上一个 Section 都没有时才新建。编辑器打关键帧时用:
        /// 用户在空轨道上按 K 应该直接出关键帧, 而不是先要求他手工创建一个 Section。
        ///
        /// 不能"找不到覆盖的就新建": 在已有 Section 范围之外打点会造出一串互相重叠、每个只含
        /// 一个关键帧的 Section。它们在时间轴上重叠成一条看不出异常的条, 但求值时只有一个能
        /// Contains 命中, 而单关键帧通道恒返回那个值 —— 整条轨道就退化成关键帧间的阶梯跳变。
        /// </summary>
        public TtTransformSection GetOrCreateSectionAt(long tick)
        {
            TtTransformSection nearest = null;
            long nearestDistance = long.MaxValue;
            for (int i = 0; i < Sections.Count; ++i)
            {
                var section = Sections[i] as TtTransformSection;
                if (section == null)
                    continue;
                if (section.Contains(tick))
                    return section;
                var distance = tick < section.StartTick ? section.StartTick - tick : tick - section.EndTick;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = section;
                }
            }
            if (nearest != null)
                return nearest;

            var newSection = new TtTransformSection()
            {
                StartTick = 0,
                EndTick = tick,
            };
            Sections.Add(newSection);
            return newSection;
        }
    }

    /// <summary>
    /// 通用属性轨道: 一条轨道对一条属性, Section 类型固定为 TtPropertySection。
    ///
    /// PropertyId / AdapterId 在轨道和 Section 上都存了一份。不是冗余: Section 要能脱离
    /// 轨道独立求值 (求值路径只拿到 Section), 而轨道要在还没有任何 Section 的时候就知道
    /// 自己是哪条属性 —— 界面上要显示名字, 新建 Section 时也要把这两个 Id 填进去。
    /// </summary>
    [Rtti.Meta("")]
    public class TtPropertyTrack : TtSequenceTrack
    {
        /// <summary>对应 ISequencePropertyAccessor.PropertyId</summary>
        [Rtti.Meta, System.ComponentModel.Category("Property")]
        [System.ComponentModel.ReadOnly(true)]
        public string PropertyId { get; set; } = "";
        /// <summary>对应 ISequenceValueAdapter.AdapterId</summary>
        [Rtti.Meta, System.ComponentModel.Category("Property")]
        [System.ComponentModel.ReadOnly(true)]
        public string AdapterId { get; set; } = "";

        public override string TrackTypeName { get => "Property"; }

        public ISequenceValueAdapter FindAdapter()
        {
            return TtSequenceValueAdapters.FindById(AdapterId);
        }
        /// <summary>
        /// 挑该把关键帧打进哪个 Section, 规则与 TtTransformTrack.GetOrCreateSectionAt 完全一致
        /// (含"不能找不到覆盖的就新建"那条, 原因见那边的注释)。
        /// </summary>
        public TtPropertySection GetOrCreateSectionAt(long tick)
        {
            TtPropertySection nearest = null;
            long nearestDistance = long.MaxValue;
            for (int i = 0; i < Sections.Count; ++i)
            {
                var section = Sections[i] as TtPropertySection;
                if (section == null)
                    continue;
                if (section.Contains(tick))
                    return section;
                var distance = tick < section.StartTick ? section.StartTick - tick : tick - section.EndTick;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = section;
                }
            }
            if (nearest != null)
                return nearest;

            var newSection = new TtPropertySection()
            {
                StartTick = 0,
                EndTick = tick,
                PropertyId = PropertyId,
                AdapterId = AdapterId,
            };
            newSection.EnsureChannels(FindAdapter());
            Sections.Add(newSection);
            return newSection;
        }
    }
}

using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// (目标对象, 属性) 二元组。目标用引用相等比较 —— 场景里两个节点即使数据一样也是
    /// 两个动画目标。
    /// </summary>
    public struct FAnimatedPropertyKey : IEquatable<FAnimatedPropertyKey>
    {
        public object Target;
        public string PropertyId;

        public FAnimatedPropertyKey(object target, string propertyId)
        {
            Target = target;
            PropertyId = propertyId;
        }
        public bool Equals(FAnimatedPropertyKey other)
        {
            return ReferenceEquals(Target, other.Target) && PropertyId == other.PropertyId;
        }
        public override bool Equals(object obj)
        {
            return obj is FAnimatedPropertyKey other && Equals(other);
        }
        public override int GetHashCode()
        {
            var h1 = Target != null ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Target) : 0;
            var h2 = PropertyId != null ? PropertyId.GetHashCode() : 0;
            return HashCode.Combine(h1, h2);
        }
    }

    /// <summary>
    /// 一次求值的中间表。所有 Section 先往这里写, 最后一次性 Flush 到目标对象上。
    ///
    /// 为什么不让 Section 直接写目标: 同一个属性可能被多个 Section 覆盖 (交叉淡化、
    /// 多轨道叠加), 直接写就变成"最后一个执行的赢", 而执行顺序取决于轨道遍历顺序,
    /// 是个隐式且不稳定的规则。走中间表后, 谁赢由显式的 Priority 决定。
    ///
    /// 中间表的 Value 是 object, 值类型会装箱。阶段 1 不优化这一点: 一帧的写入量是
    /// "被动画的属性条数"级别 (几十), 装箱开销远小于把接口层做成泛型带来的复杂度。
    /// </summary>
    public class TtSequenceEvalTable
    {
        struct FEntry
        {
            public object Target;
            public ISequencePropertyAccessor Accessor;
            public object Value;
            public int Priority;
            /// <summary>写入者的 CompletionMode 是不是 RestoreState</summary>
            public bool WantsRestore;
        }

        List<FEntry> mEntries = new List<FEntry>();
        Dictionary<FAnimatedPropertyKey, int> mIndices = new Dictionary<FAnimatedPropertyKey, int>();
        List<FAnimatedPropertyKey> mRestoreScratch = new List<FAnimatedPropertyKey>();

        public int Count { get => mEntries.Count; }

        /// <summary>
        /// 写入一条求值结果。同一 (target, 属性) 已经有更高或相同 Priority 的结果时忽略本次
        /// —— 相同 Priority 保留先写入的那个, 避免结果随遍历顺序摆动。
        ///
        /// wantsRestore 不跟着 Priority 走: 只要本帧有任何一个写入者是 KeepState, 这个属性
        /// 就不该被恢复。要是让它跟着赢家走, 两个 Section 重叠时输家的 KeepState 意图就
        /// 悄无声息地丢了。
        /// </summary>
        public void Write(object target, ISequencePropertyAccessor accessor, object value, int priority, bool wantsRestore)
        {
            if (target == null || accessor == null || value == null)
                return;

            var key = new FAnimatedPropertyKey(target, accessor.PropertyId);
            int index;
            if (mIndices.TryGetValue(key, out index))
            {
                var exist = mEntries[index];
                var merged = exist.WantsRestore && wantsRestore;
                if (priority <= exist.Priority)
                {
                    if (merged != exist.WantsRestore)
                    {
                        exist.WantsRestore = merged;
                        mEntries[index] = exist;
                    }
                    return;
                }
                exist.Value = value;
                exist.Priority = priority;
                exist.WantsRestore = merged;
                mEntries[index] = exist;
                return;
            }

            mIndices.Add(key, mEntries.Count);
            mEntries.Add(new FEntry()
            {
                Target = target,
                Accessor = accessor,
                Value = value,
                Priority = priority,
                WantsRestore = wantsRestore,
            });
        }
        /// <summary>
        /// 把中间表落到目标对象上。落之前先让 store 记住原值, 这样停止播放后能恢复。
        /// 落完之后还要处理 RestoreState —— 见下面的注释。
        /// store 传 null 表示不需要恢复 (比如游戏内一次性播放且所有 Section 都是 KeepState)。
        /// </summary>
        public void Flush(TtPreAnimatedStore store)
        {
            for (int i = 0; i < mEntries.Count; ++i)
            {
                var e = mEntries[i];
                if (store != null)
                    store.CaptureIfFirst(e.Target, e.Accessor, e.WantsRestore);
                e.Accessor.Write(e.Target, e.Value);
            }
            if (store == null)
                return;

            // 本帧没有任何 Section 写、但之前被 RestoreState 的 Section 写过的属性: 把原值写回去。
            //
            // 用"本帧有没有人写"的差集来判定, 而不是去检测"哪个 Section 刚从激活变成不激活":
            // 后者要记住上一帧的状态, 会破掉 TtSequencePlayer 那条"求值只由当前 tick 决定"的
            // 约定 (拖播放头和顺序播放就会走出不同结果)。差集的写法还顺带盖住了一帧直接
            // 跳过整个 Section、从来没在它范围内求值过的情况。
            mRestoreScratch.Clear();
            store.CollectRestorable(mRestoreScratch);
            for (int i = 0; i < mRestoreScratch.Count; ++i)
            {
                if (mIndices.ContainsKey(mRestoreScratch[i]))
                    continue;
                store.RestoreOne(mRestoreScratch[i]);
            }
        }
        public void Clear()
        {
            mEntries.Clear();
            mIndices.Clear();
        }
    }
}

using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 资产引用通道: 关键帧值是 RName。
    ///
    /// 没有插值 —— 两个资产路径之间不存在"中间值"。求值取"最后一个不晚于当前时刻的
    /// 关键帧", 对标 UE 的 FMovieSceneObjectPathChannel.Evaluate (Algo::UpperBound - 1)。
    /// 不可插值的数据不需要另一套架构, 只是插值退化成保持。
    ///
    /// 不存 DefaultValue: 没有关键帧时按"不覆盖目标当前值"处理, 与 Transform 各分量
    /// 一致。给个默认值反而危险 —— 那会让一条刚建好还没打点的轨道把目标的资产清空。
    /// </summary>
    [Rtti.Meta("")]
    public class TtNameChannel : IO.BaseSerializer, ISequenceChannel
    {
        /// <summary>关键帧时刻 (序列绝对 tick), 升序</summary>
        [Rtti.Meta("")]
        public List<long> Times { get; set; } = new List<long>();
        /// <summary>
        /// 与 Times 一一对应。不允许 null 元素 —— 序列化 List 时不写 null 标记, 存盘会抛。
        /// 想表达"这段没有资产"目前没有办法, 只能不打这个点 (见 AddKey)。
        /// </summary>
        [Rtti.Meta("")]
        public List<RName> Values { get; set; } = new List<RName>();

        public int KeyCount { get => Times.Count; }
        public long GetKeyTime(int index) { return Times[index]; }
        public RName GetKey(int index) { return Values[index]; }
        /// <summary>value 为 null 时静默忽略, 保留原值</summary>
        public void SetKey(int index, RName value)
        {
            if (value == null)
                return;
            Values[index] = value;
        }
        public long GetMaxTime()
        {
            if (Times.Count == 0)
                return 0;
            return Times[Times.Count - 1];
        }
        /// <summary>命中返回下标, 未命中返回 ~插入位置 (与 TtScalarChannel 一致)</summary>
        public int FindKeyIndex(long time)
        {
            int lo = 0;
            int hi = Times.Count - 1;
            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                var t = Times[mid];
                if (t == time)
                    return mid;
                if (t < time)
                    lo = mid + 1;
                else
                    hi = mid - 1;
            }
            return ~lo;
        }
        /// <summary>
        /// 打一个关键帧, 同一时刻已有关键帧时改值。value 为 null 直接返回 -1 不打点:
        /// null 进了 Values 会让整个序列存不下来, 宁可这个点打不上。
        /// </summary>
        public int AddKey(long time, RName value)
        {
            if (value == null)
                return -1;
            var found = FindKeyIndex(time);
            if (found >= 0)
            {
                Values[found] = value;
                return found;
            }
            var insert = ~found;
            Times.Insert(insert, time);
            Values.Insert(insert, value);
            return insert;
        }
        public void RemoveKey(int index)
        {
            Times.RemoveAt(index);
            Values.RemoveAt(index);
        }
        public void Clear()
        {
            Times.Clear();
            Values.Clear();
        }
        /// <summary>改关键帧时刻并保持升序, 返回新下标。挪到已有关键帧的时刻上时那个关键帧被顶掉。</summary>
        public int SetKeyTime(int index, long time)
        {
            if (Times[index] == time)
                return index;

            var value = Values[index];
            Times.RemoveAt(index);
            Values.RemoveAt(index);

            var found = FindKeyIndex(time);
            if (found >= 0)
            {
                Values[found] = value;
                return found;
            }
            var insert = ~found;
            Times.Insert(insert, time);
            Values.Insert(insert, value);
            return insert;
        }
        /// <summary>
        /// 阶梯取值: 取最后一个时刻不晚于 time 的关键帧。没有关键帧, 或者 time 在第一个
        /// 关键帧之前时返回 null, 由调用方理解成"不覆盖目标当前值"。
        ///
        /// 前半段不做外推 (不把第一个关键帧的值往前铺): 资产切换轨道的常见用法是
        /// "第 30 帧换成另一张贴图", 把第一个关键帧往前铺会让第 0 帧就已经换过了。
        /// </summary>
        public RName Evaluate(long time)
        {
            if (Times.Count == 0)
                return null;

            var found = FindKeyIndex(time);
            if (found >= 0)
                return Values[found];
            var insert = ~found;
            if (insert == 0)
                return null;
            return Values[insert - 1];
        }

        public void ShiftKeys(long deltaTick)
        {
            if (deltaTick == 0)
                return;
            for (int i = 0; i < Times.Count; ++i)
                Times[i] = Times[i] + deltaTick;
        }
        /// <summary>资产引用不插值, 没有切线 —— 空实现是为了让编辑器不必按通道类型分支</summary>
        public void AutoComputeAllTangents(in TtFrameRate tickResolution)
        {
        }

        class FKeySnapshot
        {
            public List<long> Times;
            public List<RName> Values;
        }
        public object CaptureKeys()
        {
            return new FKeySnapshot()
            {
                Times = new List<long>(Times),
                Values = new List<RName>(Values),
            };
        }
        public void RestoreKeys(object captured)
        {
            var snapshot = captured as FKeySnapshot;
            if (snapshot == null)
                return;
            Times = new List<long>(snapshot.Times);
            Values = new List<RName>(snapshot.Values);
        }
        public bool KeysEqualTo(object captured)
        {
            var snapshot = captured as FKeySnapshot;
            if (snapshot == null || snapshot.Times.Count != Times.Count)
                return false;
            for (int i = 0; i < Times.Count; ++i)
            {
                if (Times[i] != snapshot.Times[i])
                    return false;
                // 比字符串而不是引用: 不依赖 RNameManager 是不是把同名资产缓成同一实例。
                // Values 里不会有 null (AddKey 拒了), 但这里仍然容错: 旧资产可能是
                // 别的路径存进去的。
                var mine = Values.Count > i ? Values[i] : null;
                var other = snapshot.Values.Count > i ? snapshot.Values[i] : null;
                if (mine == null || other == null)
                {
                    if (ReferenceEquals(mine, other) == false)
                        return false;
                    continue;
                }
                if (mine.ToString() != other.ToString())
                    return false;
            }
            return true;
        }
    }
}

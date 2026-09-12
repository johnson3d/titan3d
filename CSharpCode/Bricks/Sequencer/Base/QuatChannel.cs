using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 旋转关键帧通道。旋转单独立一个通道而不是拆成三条欧拉角标量通道:
    /// 欧拉角插值会在万向锁附近抽风, 而且同一姿态有多组等价欧拉值, 打完关键帧再
    /// 回读会跳变。这里全程只有四元数, 插值用 Slerp。
    ///
    /// 代价是不能像标量通道那样编辑曲线与切线 —— 旋转轨道只有阶梯与 Slerp 两种插值,
    /// 这与 UE 的做法不同 (UE 拆三条 float 通道), 是有意的取舍。
    /// </summary>
    [Rtti.Meta("")]
    public class TtQuatChannel : IO.BaseSerializer, ISequenceChannel
    {
        /// <summary>关键帧时刻 (TickResolution 时基), 严格升序。公开仅为序列化。</summary>
        [Rtti.Meta("")]
        public List<long> Times { get; set; } = new List<long>();
        /// <summary>与 Times 一一对应的旋转。公开仅为序列化。</summary>
        [Rtti.Meta("")]
        public List<Quaternion> Values { get; set; } = new List<Quaternion>();
        [Rtti.Meta("")]
        public Quaternion DefaultValue { get; set; } = Quaternion.Identity;
        /// <summary>true 表示关键帧之间不插值, 直接跳</summary>
        [Rtti.Meta("")]
        public bool IsStepped { get; set; } = false;

        public int KeyCount { get => Times.Count; }
        public long GetKeyTime(int index) { return Times[index]; }
        public Quaternion GetKey(int index) { return Values[index]; }
        public void SetKey(int index, in Quaternion value) { Values[index] = value; }
        public long GetMaxTime()
        {
            return Times.Count > 0 ? Times[Times.Count - 1] : 0;
        }

        /// <summary>与 TtScalarChannel.FindKeyIndex 语义一致</summary>
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
        public int AddKey(long time, in Quaternion value)
        {
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

        public Quaternion Evaluate(long time)
        {
            var count = Times.Count;
            if (count == 0)
                return DefaultValue;
            if (count == 1 || time <= Times[0])
                return Values[0];
            if (time >= Times[count - 1])
                return Values[count - 1];

            var found = FindKeyIndex(time);
            if (found >= 0)
                return Values[found];

            int right = ~found;
            int left = right - 1;
            if (IsStepped)
                return Values[left];

            var tickSpan = Times[right] - Times[left];
            if (tickSpan <= 0)
                return Values[left];
            var alpha = (float)((double)(time - Times[left]) / tickSpan);
            return Quaternion.Slerp(Values[left], Values[right], alpha);
        }

        public void ShiftKeys(long deltaTick)
        {
            if (deltaTick == 0)
                return;
            for (int i = 0; i < Times.Count; ++i)
                Times[i] = Times[i] + deltaTick;
        }
        /// <summary>旋转走 Slerp, 没有切线可算 —— 空实现是为了让编辑器不必按通道类型分支</summary>
        public void AutoComputeAllTangents(in TtFrameRate tickResolution)
        {
        }

        class FKeySnapshot
        {
            public List<long> Times;
            public List<Quaternion> Values;
        }
        public object CaptureKeys()
        {
            return new FKeySnapshot()
            {
                Times = new List<long>(Times),
                Values = new List<Quaternion>(Values),
            };
        }
        public void RestoreKeys(object captured)
        {
            var snapshot = captured as FKeySnapshot;
            if (snapshot == null)
                return;
            Times = new List<long>(snapshot.Times);
            Values = new List<Quaternion>(snapshot.Values);
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
                var mine = Values[i];
                var other = snapshot.Values[i];
                // 逐分量比而不用 Quaternion 的相等判定: 那里可能带容差, 而这里要的是
                // "位模式一模一样", 否则微小的旋转改动会被当成没改而丢掉历史。
                if (mine.X != other.X || mine.Y != other.Y || mine.Z != other.Z || mine.W != other.W)
                    return false;
            }
            return true;
        }
    }
}

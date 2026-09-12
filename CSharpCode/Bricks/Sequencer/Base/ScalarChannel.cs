using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 标量关键帧通道, 对标 UE 的 FMovieSceneFloatChannel。
    ///
    /// SoA 布局: 时刻与值分两个 List 存。求值时热的是 Times 的二分查找, 单独一条
    /// long 数组比在 AoS 结构里跳着读 cache 友好得多。
    ///
    /// 两个 List 的长度必须始终相等, Times 必须严格升序 —— 所有增删改都只能走本类的
    /// 方法, 不要在外面直接改 Times/Values (它们公开只是为了序列化)。
    ///
    /// 切线的单位是"值/秒", 不是"值/tick": tick 数值随 TickResolution 变, 存成
    /// 值/秒后换时基不用改切线。代价是求值与算切线都要知道 TickResolution, 由调用方传进来。
    /// </summary>
    [Rtti.Meta("")]
    public class TtScalarChannel : IO.BaseSerializer, ISequenceChannel
    {
        /// <summary>关键帧时刻 (TickResolution 时基), 严格升序。公开仅为序列化。</summary>
        [Rtti.Meta("")]
        public List<long> Times { get; set; } = new List<long>();
        /// <summary>与 Times 一一对应的关键帧值。公开仅为序列化。</summary>
        [Rtti.Meta("")]
        public List<FScalarKey> Values { get; set; } = new List<FScalarKey>();
        /// <summary>没有任何关键帧时的取值</summary>
        [Rtti.Meta("")]
        public double DefaultValue { get; set; } = 0.0;
        [Rtti.Meta("")]
        public EChannelExtrapMode PreExtrap { get; set; } = EChannelExtrapMode.Constant;
        [Rtti.Meta("")]
        public EChannelExtrapMode PostExtrap { get; set; } = EChannelExtrapMode.Constant;

        public int KeyCount { get => Times.Count; }
        public long GetKeyTime(int index) { return Times[index]; }
        public FScalarKey GetKey(int index) { return Values[index]; }
        public void SetKey(int index, in FScalarKey key) { Values[index] = key; }
        public void SetKeyValue(int index, double value)
        {
            var key = Values[index];
            key.Value = value;
            Values[index] = key;
        }
        public long GetMaxTime()
        {
            return Times.Count > 0 ? Times[Times.Count - 1] : 0;
        }

        /// <summary>
        /// 找第一个时刻 &gt;= time 的关键帧下标。命中时返回该下标, 未命中返回 ~插入位置
        /// (与 List.BinarySearch 一致)。
        /// </summary>
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
        /// 打一个关键帧。同一时刻已有关键帧时只改值 (不动切线设置), 返回该关键帧下标。
        /// </summary>
        public int AddKey(long time, double value)
        {
            var found = FindKeyIndex(time);
            if (found >= 0)
            {
                SetKeyValue(found, value);
                return found;
            }
            var insert = ~found;
            Times.Insert(insert, time);
            Values.Insert(insert, new FScalarKey(value));
            return insert;
        }
        public int AddKey(long time, in FScalarKey key)
        {
            var found = FindKeyIndex(time);
            if (found >= 0)
            {
                Values[found] = key;
                return found;
            }
            var insert = ~found;
            Times.Insert(insert, time);
            Values.Insert(insert, key);
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
        /// <summary>
        /// 改关键帧时刻并保持升序, 返回新下标。挪到已有关键帧的时刻上时, 那个关键帧被顶掉。
        /// </summary>
        public int SetKeyTime(int index, long time)
        {
            if (Times[index] == time)
                return index;

            var key = Values[index];
            Times.RemoveAt(index);
            Values.RemoveAt(index);

            var found = FindKeyIndex(time);
            if (found >= 0)
            {
                Values[found] = key;
                return found;
            }
            var insert = ~found;
            Times.Insert(insert, time);
            Values.Insert(insert, key);
            return insert;
        }

        /// <summary>
        /// 取值。没有关键帧时给 DefaultValue; 落在首尾之外时按 Constant 外推
        /// (阶段 1 不实现 Linear/Cycle 外推)。
        /// </summary>
        public double Evaluate(long time, in TtFrameRate tickResolution)
        {
            var count = Times.Count;
            if (count == 0)
                return DefaultValue;
            if (count == 1 || time <= Times[0])
                return Values[0].Value;
            if (time >= Times[count - 1])
                return Values[count - 1].Value;

            var found = FindKeyIndex(time);
            if (found >= 0)
                return Values[found].Value;

            // ~found 是第一个时刻 > time 的关键帧, 所以左端点是它前一个
            int right = ~found;
            int left = right - 1;
            var leftKey = Values[left];
            if (leftKey.InterpMode == EChannelInterpMode.Constant)
                return leftKey.Value;

            var rightKey = Values[right];
            var tickSpan = Times[right] - Times[left];
            if (tickSpan <= 0)
                return leftKey.Value;
            var alpha = (double)(time - Times[left]) / tickSpan;

            if (leftKey.InterpMode == EChannelInterpMode.Linear)
                return leftKey.Value + (rightKey.Value - leftKey.Value) * alpha;

            // Cubic: Hermite。切线是值/秒, 乘上区间秒数换成"值/单位alpha"
            var seconds = tickResolution.AsSeconds(tickSpan);
            var m0 = leftKey.LeaveTangent * seconds;
            var m1 = rightKey.ArriveTangent * seconds;
            var t2 = alpha * alpha;
            var t3 = t2 * alpha;
            var h00 = 2.0 * t3 - 3.0 * t2 + 1.0;
            var h10 = t3 - 2.0 * t2 + alpha;
            var h01 = -2.0 * t3 + 3.0 * t2;
            var h11 = t3 - t2;
            return h00 * leftKey.Value + h10 * m0 + h01 * rightKey.Value + h11 * m1;
        }

        /// <summary>
        /// 按相邻关键帧重算一个 Auto 切线。User/Break 的关键帧不动。
        /// 端点切线给 0 (不冲出首尾关键帧的值域)。
        /// </summary>
        public void AutoComputeTangents(int index, in TtFrameRate tickResolution)
        {
            if (index < 0 || index >= Values.Count)
                return;
            var key = Values[index];
            if (key.TangentMode != EChannelTangentMode.Auto)
                return;

            double tangent = 0.0;
            if (index > 0 && index < Values.Count - 1)
            {
                var span = tickResolution.AsSeconds(Times[index + 1] - Times[index - 1]);
                if (span > MathHelper.Epsilon)
                    tangent = (Values[index + 1].Value - Values[index - 1].Value) / span;
            }
            key.ArriveTangent = tangent;
            key.LeaveTangent = tangent;
            Values[index] = key;
        }
        /// <summary>
        /// 重算 index 及其左右邻居的 Auto 切线。加/删/挪关键帧后必须调这个: 只算自己
        /// 会让邻居的切线停在旧值上, 曲线会出现看不出原因的拐折。
        /// </summary>
        public void AutoComputeTangentsAround(int index, in TtFrameRate tickResolution)
        {
            AutoComputeTangents(index - 1, tickResolution);
            AutoComputeTangents(index, tickResolution);
            AutoComputeTangents(index + 1, tickResolution);
        }
        public void AutoComputeAllTangents(in TtFrameRate tickResolution)
        {
            for (int i = 0; i < Values.Count; ++i)
                AutoComputeTangents(i, tickResolution);
        }

        public void ShiftKeys(long deltaTick)
        {
            if (deltaTick == 0)
                return;
            for (int i = 0; i < Times.Count; ++i)
                Times[i] = Times[i] + deltaTick;
        }

        /// <summary>快照体。故意做成私有嵌套类: 外面拿到的只是 object, 没办法绕过接口改里面的表</summary>
        class FKeySnapshot
        {
            public List<long> Times;
            public List<FScalarKey> Values;
        }
        public object CaptureKeys()
        {
            return new FKeySnapshot()
            {
                Times = new List<long>(Times),
                Values = new List<FScalarKey>(Values),
            };
        }
        public void RestoreKeys(object captured)
        {
            var snapshot = captured as FKeySnapshot;
            if (snapshot == null)
                return;
            Times = new List<long>(snapshot.Times);
            Values = new List<FScalarKey>(snapshot.Values);
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
                // 切线与插值模式也要比: 改插值模式、手拖切线不会动值, 只比值的话
                // 这两类编辑会被当成"没变化"而进不了历史。
                if (mine.Value != other.Value)
                    return false;
                if (mine.ArriveTangent != other.ArriveTangent || mine.LeaveTangent != other.LeaveTangent)
                    return false;
                if (mine.InterpMode != other.InterpMode || mine.TangentMode != other.TangentMode)
                    return false;
            }
            return true;
        }
    }
}

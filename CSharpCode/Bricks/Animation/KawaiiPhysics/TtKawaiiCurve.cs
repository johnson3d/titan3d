using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EngineNS.Bricks.Animation.KawaiiPhysics
{
    /// <summary>
    /// 一条沿骨骼链归一化位置(0..1)采样的浮点曲线, 用于逐骨骼调节 Kawaii 物理参数
    /// (Stiffness / Damping / Radius ...)。运行时求解在 native 侧做线性插值
    /// (KawaiiTypes.h 的 FKawaiiCurve), 这里是编辑器/序列化侧的托管镜像。
    ///
    /// 序列化形态: 对外只暴露一个 [Rtti.Meta] string KeysData ("t0:v0;t1:v1;..."),
    /// 关键点列表存私有字段。这样做是因为 DesignMacross GenCode 的
    /// TtASTBuildUtil.CreateItem 不能序列化 List&lt;T&gt; 属性(会把 List 当对象递归),
    /// 但能处理 string 属性。编辑控件通过下面的 internal 方法(不是属性, CreateItem 不会碰)
    /// 读写关键点。
    /// </summary>
    public class TtKawaiiCurve : IO.BaseSerializer
    {
        public struct FKey
        {
            public float Time;   // 归一化链位置 0..1
            public float Value;  // 采样值(会与基准标量相乘)
            public FKey(float t, float v) { Time = t; Value = v; }
        }

        // 关键点始终按 Time 升序。私有 => CreateItem 只看到 KeysData 一个属性。
        private List<FKey> mKeys = new List<FKey>();

        /// <summary>
        /// GenCode / 序列化唯一出口: "t0:v0;t1:v1;..." (InvariantCulture)。
        /// 空曲线序列化为空串, 反序列化为无关键点(运行时视为恒等 1.0)。
        /// </summary>
        [Rtti.Meta]
        public string KeysData
        {
            get
            {
                if (mKeys.Count == 0)
                    return "";
                var sb = new StringBuilder();
                for (int i = 0; i < mKeys.Count; i++)
                {
                    if (i > 0)
                        sb.Append(';');
                    sb.Append(mKeys[i].Time.ToString("R", CultureInfo.InvariantCulture));
                    sb.Append(':');
                    sb.Append(mKeys[i].Value.ToString("R", CultureInfo.InvariantCulture));
                }
                return sb.ToString();
            }
            set
            {
                mKeys.Clear();
                if (string.IsNullOrEmpty(value))
                    return;
                var pairs = value.Split(';');
                foreach (var pair in pairs)
                {
                    if (string.IsNullOrEmpty(pair))
                        continue;
                    var kv = pair.Split(':');
                    if (kv.Length != 2)
                        continue;
                    if (float.TryParse(kv[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var t) &&
                        float.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                    {
                        mKeys.Add(new FKey(t, v));
                    }
                }
                SortKeys();
            }
        }

        public bool IsEmpty => mKeys.Count == 0;
        internal int KeyCount => mKeys.Count;
        internal FKey GetKey(int index) => mKeys[index];

        // ─── 编辑量程(可序列化, 每条曲线独立) ────────────────────────────
        // attribute 上的 YMin/YMax 是 C# 编译期常量, 美术改不了; 这两个字段让每条
        // 曲线自己带量程并随资产持久化。ViewYMax <= ViewYMin 表示"未指定", 回落到
        // attribute 的默认值 —— 旧资产反序列化时不写这两个字段, 自然就是未指定。
        [Rtti.Meta]
        public float ViewYMin { get; set; } = 0.0f;
        [Rtti.Meta]
        public float ViewYMax { get; set; } = 0.0f;

        /// <summary>
        /// 取实际生效的量程: 曲线自带的优先, 否则用传入的 attribute 默认值。
        /// </summary>
        public void GetViewRange(float fallbackMin, float fallbackMax, out float min, out float max)
        {
            if (ViewYMax > ViewYMin)
            {
                min = ViewYMin;
                max = ViewYMax;
            }
            else
            {
                min = fallbackMin;
                max = fallbackMax;
            }
        }

        /// <summary>
        /// 编辑器写回量程。max 必须大于 min, 否则视为未指定(清除自带量程)。
        /// </summary>
        public void SetViewRange(float min, float max)
        {
            if (max > min)
            {
                ViewYMin = min;
                ViewYMax = max;
            }
            else
            {
                ViewYMin = 0.0f;
                ViewYMax = 0.0f;
            }
        }

        internal void SetKey(int index, float time, float value)
        {
            if (index < 0 || index >= mKeys.Count)
                return;
            mKeys[index] = new FKey(Math.Clamp(time, 0.0f, 1.0f), value);
            SortKeys();
        }

        internal int AddKey(float time, float value)
        {
            mKeys.Add(new FKey(Math.Clamp(time, 0.0f, 1.0f), value));
            SortKeys();
            for (int i = 0; i < mKeys.Count; i++)
                if (mKeys[i].Time == time)
                    return i;
            return mKeys.Count - 1;
        }

        internal void RemoveKey(int index)
        {
            if (index >= 0 && index < mKeys.Count)
                mKeys.RemoveAt(index);
        }

        void SortKeys()
        {
            mKeys.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        /// <summary>
        /// 与 native FKawaiiCurve::Evaluate 一致的线性插值(空曲线返回 1.0), 供编辑器预览。
        /// </summary>
        public float Evaluate(float time)
        {
            if (mKeys.Count == 0)
                return 1.0f;
            if (mKeys.Count == 1)
                return mKeys[0].Value;
            if (time <= mKeys[0].Time)
                return mKeys[0].Value;
            if (time >= mKeys[mKeys.Count - 1].Time)
                return mKeys[mKeys.Count - 1].Value;
            for (int i = 0; i + 1 < mKeys.Count; i++)
            {
                if (time >= mKeys[i].Time && time <= mKeys[i + 1].Time)
                {
                    float span = mKeys[i + 1].Time - mKeys[i].Time;
                    float alpha = span > 1e-6f ? (time - mKeys[i].Time) / span : 0.0f;
                    return mKeys[i].Value + alpha * (mKeys[i + 1].Value - mKeys[i].Value);
                }
            }
            return mKeys[mKeys.Count - 1].Value;
        }

        /// <summary>
        /// 导出为 native 下推用的两个并列数组 (times / values), 均按 Time 升序。
        /// </summary>
        public void ToArrays(out float[] times, out float[] values)
        {
            times = new float[mKeys.Count];
            values = new float[mKeys.Count];
            for (int i = 0; i < mKeys.Count; i++)
            {
                times[i] = mKeys[i].Time;
                values[i] = mKeys[i].Value;
            }
        }

        public TtKawaiiCurve Clone()
        {
            var r = new TtKawaiiCurve();
            r.mKeys = new List<FKey>(mKeys);
            r.ViewYMin = ViewYMin;
            r.ViewYMax = ViewYMax;
            return r;
        }
    }
}

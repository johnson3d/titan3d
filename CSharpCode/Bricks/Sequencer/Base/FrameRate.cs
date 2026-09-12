using System;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 有理数帧率。分子分母都存下来是为了 29.97 (30000/1001) 这类非整数帧率能精确表达,
    /// 用 float 存 29.97 累加几千帧后就会漂。
    ///
    /// 序列里有两个帧率:
    /// - TickResolution: 内部存储时基, 所有关键帧时刻都是这个时基下的 Int64 tick。缺省 24000/1,
    ///   能被 24/25/30/48/50/60/120 整除, 换帧率不丢关键帧。
    /// - DisplayRate: 界面上显示与步进的帧率, 缺省 30/1。
    ///
    /// 注意: 这是可 blit 的值类型, 序列化走 DataCopyer 的 IsUnmanagedType 分支 (整块内存直写),
    /// 所以字段一旦增删或改序, 已存盘的资产就读不回来了。
    /// </summary>
    [Rtti.Meta("")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct TtFrameRate
    {
        public TtFrameRate()
        {
        }
        public TtFrameRate(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        [Rtti.Meta("")]
        public int Numerator { get; set; } = 24000;
        [Rtti.Meta("")]
        public int Denominator { get; set; } = 1;

        /// <summary>缺省内部时基, 对标 UE 工程默认的 24000fps TickResolution</summary>
        public static TtFrameRate DefaultTickResolution { get => new TtFrameRate(24000, 1); }
        /// <summary>缺省显示帧率</summary>
        public static TtFrameRate DefaultDisplayRate { get => new TtFrameRate(30, 1); }

        public bool IsValid { get => Numerator > 0 && Denominator > 0; }
        /// <summary>一个 tick 有多少秒</summary>
        public double AsInterval { get => IsValid ? (double)Denominator / Numerator : 0.0; }
        /// <summary>每秒多少 tick</summary>
        public double AsDecimal { get => IsValid ? (double)Numerator / Denominator : 0.0; }

        public double AsSeconds(long ticks)
        {
            if (IsValid == false)
                return 0.0;
            return (double)ticks * Denominator / Numerator;
        }
        public long FromSeconds(double seconds)
        {
            if (IsValid == false)
                return 0;
            return (long)Math.Round(seconds * Numerator / Denominator, MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// 本时基下的一个 displayRate 帧占多少 tick。displayRate 不能整除时向下取整,
        /// 只用于界面步进与网格绘制, 不参与关键帧存储。
        /// </summary>
        public long TicksPerFrame(in TtFrameRate displayRate)
        {
            if (IsValid == false || displayRate.IsValid == false)
                return 1;
            var ticks = (long)((double)Numerator * displayRate.Denominator / ((double)Denominator * displayRate.Numerator));
            return ticks > 0 ? ticks : 1;
        }
        /// <summary>
        /// 把本时基下的 tick 对齐到 displayRate 的整帧边界。FrameLocked 求值与界面吸附都走这里。
        /// </summary>
        public long SnapTo(long ticks, in TtFrameRate displayRate)
        {
            if (IsValid == false || displayRate.IsValid == false)
                return ticks;
            // frameIndex = ticks * Denominator * displayRate.Numerator / (Numerator * displayRate.Denominator)
            var frameNum = (double)ticks * Denominator * displayRate.Numerator;
            var frameDen = (double)Numerator * displayRate.Denominator;
            var frame = Math.Round(frameNum / frameDen, MidpointRounding.AwayFromZero);
            var back = frame * displayRate.Denominator * Numerator / ((double)displayRate.Numerator * Denominator);
            return (long)Math.Round(back, MidpointRounding.AwayFromZero);
        }
        /// <summary>把 tick 从本时基换算到 target 时基</summary>
        public long ConvertTo(long ticks, in TtFrameRate target)
        {
            if (IsValid == false || target.IsValid == false)
                return ticks;
            var v = (double)ticks * Denominator * target.Numerator / ((double)Numerator * target.Denominator);
            return (long)Math.Round(v, MidpointRounding.AwayFromZero);
        }
        public override string ToString()
        {
            return Denominator == 1 ? $"{Numerator}fps" : $"{Numerator}/{Denominator}fps";
        }
    }
}

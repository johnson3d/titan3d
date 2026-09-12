using System;

namespace EngineNS.Sequencer
{
    /// <summary>关键帧之间的插值方式</summary>
    public enum EChannelInterpMode
    {
        /// <summary>阶梯: 保持左侧关键帧的值直到下一帧</summary>
        Constant,
        Linear,
        /// <summary>三次 Hermite, 用左右关键帧的切线</summary>
        Cubic,
    }
    /// <summary>切线来源</summary>
    public enum EChannelTangentMode
    {
        /// <summary>由相邻关键帧自动算</summary>
        Auto,
        /// <summary>用户指定, 进出切线保持一致</summary>
        User,
        /// <summary>用户指定, 进出切线可以不一致</summary>
        Break,
    }
    /// <summary>首尾关键帧之外怎么取值。阶段 1 只实现 Constant, 其余按 Constant 处理。</summary>
    public enum EChannelExtrapMode
    {
        Constant,
        Linear,
        Cycle,
    }

    /// <summary>
    /// 标量通道的一个关键帧值。时刻不在这里, 在通道的 Times 里 (SoA 布局, 对标
    /// UE 的 FMovieSceneFloatChannel): 求值时只需要二分 Times, 不必把整个 Value
    /// 结构拖进 cache。
    ///
    /// 值一律用 double: 引擎的 TtPlacement.Position 是 DVector3, 用 float 存位置
    /// 在大世界坐标下会掉精度。
    ///
    /// 注意: 可 blit 的值类型, 序列化是整块内存直写 (DataCopyer 的 IsUnmanagedType 分支),
    /// 字段增删改序会让已存盘的资产读不回来。也因此这里不放 bool (Marshal 下 bool 是 4 字节, 易踩坑)。
    /// </summary>
    [Rtti.Meta("")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FScalarKey
    {
        public FScalarKey()
        {
        }
        public FScalarKey(double value)
        {
            Value = value;
        }

        [Rtti.Meta("")]
        public double Value { get; set; } = 0.0;
        /// <summary>进入本关键帧的切线 (单位: 值/秒)</summary>
        [Rtti.Meta("")]
        public double ArriveTangent { get; set; } = 0.0;
        /// <summary>离开本关键帧的切线 (单位: 值/秒)</summary>
        [Rtti.Meta("")]
        public double LeaveTangent { get; set; } = 0.0;
        /// <summary>本关键帧到下一关键帧之间的插值方式</summary>
        [Rtti.Meta("")]
        public EChannelInterpMode InterpMode { get; set; } = EChannelInterpMode.Cubic;
        [Rtti.Meta("")]
        public EChannelTangentMode TangentMode { get; set; } = EChannelTangentMode.Auto;
    }

    /// <summary>
    /// 通道的类型无关面, 编辑器拖关键帧时不必知道通道存的是什么值。
    /// </summary>
    public interface ISequenceChannel
    {
        int KeyCount { get; }
        long GetKeyTime(int index);
        /// <summary>
        /// 改关键帧时刻。改完必须仍是严格升序, 所以实现方要负责重排并返回该关键帧的新下标。
        /// </summary>
        int SetKeyTime(int index, long time);
        void RemoveKey(int index);
        void Clear();
        /// <summary>该通道最后一个关键帧的时刻, 没有关键帧时返回 0</summary>
        long GetMaxTime();
        /// <summary>
        /// 把所有关键帧整体平移 deltaTick。关键帧时刻是序列绝对 tick, 拖动 Section
        /// 必须连带平移它们, 否则 Section 挨走了关键帧还留在原地。
        /// </summary>
        void ShiftKeys(long deltaTick);
        /// <summary>
        /// 重算所有 Auto 切线。没有切线概念的通道 (阶梯类) 实现成空操作 —— 编辑器
        /// 挨完关键帧要统一调一次, 不应该在调用点按通道类型分支。
        /// </summary>
        void AutoComputeAllTangents(in TtFrameRate tickResolution);
        /// <summary>
        /// 深拷贝本通道的全部关键帧, 供编辑器做整段快照式 Undo。
        ///
        /// 返回值的具体形状由实现决定, 调用方只能把它原样交回 RestoreKeys / KeysEqualTo,
        /// 不要试图解读 —— 这样加新通道类型时编辑器的快照代码一行都不用改。
        /// </summary>
        object CaptureKeys();
        /// <summary>用快照覆盖当前关键帧。快照要能被 Undo/Redo 反复用, 所以实现必须再拷一份而不是直接引用。</summary>
        void RestoreKeys(object captured);
        /// <summary>当前关键帧是否与快照完全一致。Undo 靠它去重, 免得"拖了一下又拖回来"也占一步历史。</summary>
        bool KeysEqualTo(object captured);
    }
}

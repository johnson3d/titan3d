using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>属性值最终落在哪种通道上</summary>
    public enum EChannelKind
    {
        /// <summary>一到四条标量通道 (值一律用 double 存)</summary>
        Scalar,
        /// <summary>一条四元数通道</summary>
        Quat,
        /// <summary>一条资产引用通道</summary>
        Name,
    }

    /// <summary>
    /// 一种属性值类型怎么拆成通道、又怎么从通道组装回来。
    ///
    /// 为什么不把这件事交给 ISequencePropertyAccessor: 访问器是"某个属性"的入口, 而
    /// "DVector3 拆成 X/Y/Z 三条标量通道"对所有 DVector3 属性都一样。放进访问器会让
    /// 每加一个属性都要重抄一遍拆装逻辑。
    ///
    /// 对标 UE 的做法: UE 靠 TMovieSceneChannelTraits 加一堆自由函数模板 (TEnableIf
    /// 分派) 复用通道操作, C# 没有 SFINAE, 等价物就是这个按值类型注册的接口。
    /// </summary>
    public interface ISequenceValueAdapter
    {
        /// <summary>
        /// 存进资产的稳定字符串, 一旦发布不能改 —— Section 存的是它而不是类型名。
        ///
        /// 不存类型名是因为 Rtti 的类型名往返 (TypeStr/TypeOf) 对 double/bool 这类基础
        /// 类型不保证覆盖, 而适配器 Id 是我们自己控制的。
        /// </summary>
        string AdapterId { get; }
        /// <summary>本适配器负责的属性值类型</summary>
        Type ValueType { get; }
        EChannelKind Kind { get; }
        /// <summary>Kind 为 Scalar 时需要几条通道 (1~4), 其余 Kind 返回 0</summary>
        int ScalarChannelCount { get; }
        /// <summary>通道在界面上的名字, 如 "X"/"Y"/"Z"; 单通道给 "Value"</summary>
        string GetChannelName(int index);
        /// <summary>
        /// 新建标量通道的关键帧用哪种插值。整型/布尔/枚举必须是 Constant —— 让 2 和 3
        /// 之间插出 2.5 再截断, 会在半程就跳到 3。
        /// </summary>
        EChannelInterpMode DefaultInterpMode { get; }
        /// <summary>
        /// 把一个属性值拆进 section 的通道, 打出关键帧。value 类型不匹配时静默跳过。
        /// 要 tickResolution 是因为 Cubic 通道打点后必须重算切线, 而算切线要知道时基。
        /// </summary>
        void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution);
        /// <summary>
        /// 从 section 的通道组装出属性值。currentValue 是目标属性当前的值, 用来给没有
        /// 关键帧的分量兜底 (只 K 了 Z 的轨道不该把 X/Y 拽回 0)。
        /// 返回 null 表示这一刻没有任何通道该生效, 调用方不要写。
        /// </summary>
        object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx);
    }

    /// <summary>
    /// 值适配器注册表。
    ///
    /// 做成静态而不是挂在 TtSequencerModule 上 (属性访问器注册表是挂模块的): 适配器是
    /// 纯类型知识、无状态、进程内唯一, 而它的两个调用方 —— Section 求值和编辑器打点
    /// —— 一个在 FSectionEvalContext 里、一个不在, 挂模块就得给 ctx 加字段并改所有构造点。
    ///
    /// 代价: C# 热重载后游戏侧自定义适配器的 Type 键会失效, 表现成那条轨道查不到适配器
    /// 而不动。这是降级不是崩, 且引擎内置适配器不受影响。
    /// </summary>
    public static class TtSequenceValueAdapters
    {
        static Dictionary<string, ISequenceValueAdapter> mById = new Dictionary<string, ISequenceValueAdapter>();
        static Dictionary<Type, ISequenceValueAdapter> mByType = new Dictionary<Type, ISequenceValueAdapter>();

        public static IReadOnlyDictionary<string, ISequenceValueAdapter> Adapters { get => mById; }

        /// <summary>撞 Id 时只报 Warning 并保留先注册的那个, 与属性访问器注册表一致</summary>
        public static bool Register(ISequenceValueAdapter adapter)
        {
            if (adapter == null || string.IsNullOrEmpty(adapter.AdapterId) || adapter.ValueType == null)
                return false;
            if (mById.ContainsKey(adapter.AdapterId))
            {
                Profiler.Log.WriteLine<TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequenceValueAdapters.Register: AdapterId({adapter.AdapterId}) already registered, keep the first one");
                return false;
            }
            mById.Add(adapter.AdapterId, adapter);
            mByType[adapter.ValueType] = adapter;
            return true;
        }
        /// <summary>Section 存盘用的路径: 按 AdapterId 找。找不到返回 null (老资产引用了已删的适配器)。</summary>
        public static ISequenceValueAdapter FindById(string adapterId)
        {
            if (string.IsNullOrEmpty(adapterId))
                return null;
            ISequenceValueAdapter result;
            mById.TryGetValue(adapterId, out result);
            return result;
        }
        /// <summary>
        /// 编辑器建轨道用的路径: 按属性的值类型找。
        /// 枚举走同一个适配器 —— 枚举类型是开放集合, 不可能每个都注册一遍。
        /// </summary>
        public static ISequenceValueAdapter FindByValueType(Type valueType)
        {
            if (valueType == null)
                return null;
            ISequenceValueAdapter result;
            if (mByType.TryGetValue(valueType, out result))
                return result;
            if (valueType.IsEnum)
                return FindById(TtEnumValueAdapter.Id);
            return null;
        }
    }
}

using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 标量类适配器的公共部分: 打点时统一设好插值方式并按需重算切线。
    ///
    /// 没有把"值 ↔ 分量数组"抽成抽象方法: 那需要每帧过一个 double[] 或 Span 中转,
    /// 在求值路径上多一次分配/拷贝, 而各子类的拆装本身只有两三行。
    /// </summary>
    public abstract class TtScalarValueAdapterBase : ISequenceValueAdapter
    {
        public abstract string AdapterId { get; }
        public abstract Type ValueType { get; }
        public EChannelKind Kind { get => EChannelKind.Scalar; }
        public abstract int ScalarChannelCount { get; }
        public virtual string GetChannelName(int index) { return "Value"; }
        public virtual EChannelInterpMode DefaultInterpMode { get => EChannelInterpMode.Cubic; }

        public abstract void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution);
        public abstract object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx);

        /// <summary>
        /// 往第 index 条通道打点, 并把插值方式设成本适配器要求的那种。
        ///
        /// Cubic 时顺带重算相邻切线: 打完点忘了算切线, 曲线会退化成折线 —— 这是个只在
        /// 画面上才看得出来的静默错误, 所以放在打点这一步一起做掉, 不留给调用方。
        /// </summary>
        protected void AddScalarKey(Asset.TtPropertySection section, int index, long tick, double value, in TtFrameRate tickResolution)
        {
            if (index < 0 || index >= section.ScalarChannels.Count)
                return;
            var channel = section.ScalarChannels[index];
            if (channel == null)
                return;
            var keyIndex = channel.AddKey(tick, value);
            var key = channel.GetKey(keyIndex);
            if (key.InterpMode != DefaultInterpMode)
            {
                key.InterpMode = DefaultInterpMode;
                channel.SetKey(keyIndex, key);
            }
            if (DefaultInterpMode == EChannelInterpMode.Cubic)
                channel.AutoComputeTangentsAround(keyIndex, tickResolution);
        }
        /// <summary>第 index 条通道有没有关键帧</summary>
        protected static bool HasKey(Asset.TtPropertySection section, int index)
        {
            if (index < 0 || index >= section.ScalarChannels.Count)
                return false;
            var channel = section.ScalarChannels[index];
            return channel != null && channel.KeyCount > 0;
        }
        /// <summary>取第 index 条通道在 ctx.Time 的值, 调用方要先用 HasKey 判过</summary>
        protected static double EvalScalar(Asset.TtPropertySection section, int index, in Asset.FSectionEvalContext ctx)
        {
            return section.ScalarChannels[index].Evaluate(ctx.Time, ctx.TickResolution);
        }
    }

    /// <summary>单值标量适配器 (double/float/int/bool/枚举): 只差一个类型转换</summary>
    public abstract class TtSingleScalarAdapterBase : TtScalarValueAdapterBase
    {
        public override int ScalarChannelCount { get => 1; }

        /// <summary>value 不是本适配器的类型时返回 false, 不打点</summary>
        protected abstract bool TryToDouble(object value, out double result);
        /// <summary>把通道值还原成属性值。currentValue 只有枚举适配器要用 (从它拿具体枚举类型)。</summary>
        protected abstract object FromDouble(double value, object currentValue);

        public override void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            double d;
            if (TryToDouble(value, out d) == false)
                return;
            AddScalarKey(section, 0, tick, d, tickResolution);
        }
        public override object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if (HasKey(section, 0) == false)
                return null;
            return FromDouble(EvalScalar(section, 0, in ctx), currentValue);
        }
    }

    public class TtDoubleValueAdapter : TtSingleScalarAdapterBase
    {
        public const string Id = "Double";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(double); }

        protected override bool TryToDouble(object value, out double result)
        {
            if (value is double d)
            {
                result = d;
                return true;
            }
            result = 0.0;
            return false;
        }
        protected override object FromDouble(double value, object currentValue) { return value; }
    }

    public class TtFloatValueAdapter : TtSingleScalarAdapterBase
    {
        public const string Id = "Float";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(float); }

        protected override bool TryToDouble(object value, out double result)
        {
            if (value is float f)
            {
                result = f;
                return true;
            }
            result = 0.0;
            return false;
        }
        protected override object FromDouble(double value, object currentValue) { return (float)value; }
    }

    /// <summary>
    /// 整型适配器。插值用 Constant: 在 2 和 3 之间插出 2.5 再取整, 会让值在半程就跳到 3,
    /// 比"到点才跳"更难理解。想要连续变化的整数请把属性改成浮点。
    /// </summary>
    public class TtIntValueAdapter : TtSingleScalarAdapterBase
    {
        public const string Id = "Int";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(int); }
        public override EChannelInterpMode DefaultInterpMode { get => EChannelInterpMode.Constant; }

        protected override bool TryToDouble(object value, out double result)
        {
            if (value is int i)
            {
                result = i;
                return true;
            }
            result = 0.0;
            return false;
        }
        protected override object FromDouble(double value, object currentValue) { return (int)Math.Round(value); }
    }

    /// <summary>
    /// 布尔适配器, 存 0/1。
    ///
    /// 阈值取 0.5 而不是 "!= 0": Constant 插值下通道只会给出打过的 0 或 1, 但用户可以
    /// 在曲线编辑器里把关键帧改成 Linear, 那时 0.5 才是"翻转发生在两帧正中间"。
    /// </summary>
    public class TtBoolValueAdapter : TtSingleScalarAdapterBase
    {
        public const string Id = "Bool";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(bool); }
        public override EChannelInterpMode DefaultInterpMode { get => EChannelInterpMode.Constant; }

        protected override bool TryToDouble(object value, out double result)
        {
            if (value is bool b)
            {
                result = b ? 1.0 : 0.0;
                return true;
            }
            result = 0.0;
            return false;
        }
        protected override object FromDouble(double value, object currentValue) { return value >= 0.5; }
    }

    /// <summary>
    /// 枚举适配器。枚举类型是开放集合, 不可能一个个注册, 所以它按 typeof(Enum) 注册,
    /// 由注册表的 IsEnum 分支兜底命中 (见 TtSequenceValueAdapters.FindByValueType)。
    ///
    /// 组装时要有具体枚举类型, 只能从目标属性的当前值上拿 —— 拿不到就不写, 好过写一个
    /// 类型不对的值进去让 Write 静默丢弃。
    /// </summary>
    public class TtEnumValueAdapter : TtSingleScalarAdapterBase
    {
        public const string Id = "Enum";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(Enum); }
        public override EChannelInterpMode DefaultInterpMode { get => EChannelInterpMode.Constant; }

        protected override bool TryToDouble(object value, out double result)
        {
            if (value is Enum)
            {
                // 底层类型可能是 byte/int/long, 统一走 Convert
                result = Convert.ToInt64(value);
                return true;
            }
            result = 0.0;
            return false;
        }
        protected override object FromDouble(double value, object currentValue)
        {
            if ((currentValue is Enum) == false)
                return null;
            // 通道值可能落在没有定义的整数上 (用户手改过曲线)。这里不校验合法性:
            // 引擎里的枚举属性不少是位标记, 校验反而会拦掉正常用法。
            return Enum.ToObject(currentValue.GetType(), (long)Math.Round(value));
        }
    }

    /// <summary>
    /// 三分量向量适配器。没有关键帧的分量保持目标当前值 —— 只 K 了 Z 的轨道不该把 X/Y
    /// 拽回 0 (与 TtTransformSection 同一套语义)。
    /// 需要四分量 (Vector4/颜色) 时照这个类抄一份即可。
    /// </summary>
    public class TtDVector3ValueAdapter : TtScalarValueAdapterBase
    {
        public const string Id = "DVector3";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(DVector3); }
        public override int ScalarChannelCount { get => 3; }
        public override string GetChannelName(int index)
        {
            return index == 0 ? "X" : (index == 1 ? "Y" : "Z");
        }

        public override void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            if ((value is DVector3) == false)
                return;
            var v = (DVector3)value;
            AddScalarKey(section, 0, tick, v.X, tickResolution);
            AddScalarKey(section, 1, tick, v.Y, tickResolution);
            AddScalarKey(section, 2, tick, v.Z, tickResolution);
        }
        public override object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if ((currentValue is DVector3) == false)
                return null;
            var v = (DVector3)currentValue;
            bool any = false;
            if (HasKey(section, 0)) { v.X = EvalScalar(section, 0, in ctx); any = true; }
            if (HasKey(section, 1)) { v.Y = EvalScalar(section, 1, in ctx); any = true; }
            if (HasKey(section, 2)) { v.Z = EvalScalar(section, 2, in ctx); any = true; }
            return any ? (object)v : null;
        }
    }

    public class TtVector3ValueAdapter : TtScalarValueAdapterBase
    {
        public const string Id = "Vector3";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(Vector3); }
        public override int ScalarChannelCount { get => 3; }
        public override string GetChannelName(int index)
        {
            return index == 0 ? "X" : (index == 1 ? "Y" : "Z");
        }

        public override void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            if ((value is Vector3) == false)
                return;
            var v = (Vector3)value;
            AddScalarKey(section, 0, tick, v.X, tickResolution);
            AddScalarKey(section, 1, tick, v.Y, tickResolution);
            AddScalarKey(section, 2, tick, v.Z, tickResolution);
        }
        public override object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if ((currentValue is Vector3) == false)
                return null;
            var v = (Vector3)currentValue;
            bool any = false;
            if (HasKey(section, 0)) { v.X = (float)EvalScalar(section, 0, in ctx); any = true; }
            if (HasKey(section, 1)) { v.Y = (float)EvalScalar(section, 1, in ctx); any = true; }
            if (HasKey(section, 2)) { v.Z = (float)EvalScalar(section, 2, in ctx); any = true; }
            return any ? (object)v : null;
        }
    }

    /// <summary>两分量向量适配器。语义同 TtVector3ValueAdapter —— 没有关键帧的分量保持目标当前值。</summary>
    public class TtVector2ValueAdapter : TtScalarValueAdapterBase
    {
        public const string Id = "Vector2";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(Vector2); }
        public override int ScalarChannelCount { get => 2; }
        public override string GetChannelName(int index)
        {
            return index == 0 ? "X" : "Y";
        }

        public override void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            if ((value is Vector2) == false)
                return;
            var v = (Vector2)value;
            AddScalarKey(section, 0, tick, v.X, tickResolution);
            AddScalarKey(section, 1, tick, v.Y, tickResolution);
        }
        public override object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if ((currentValue is Vector2) == false)
                return null;
            var v = (Vector2)currentValue;
            bool any = false;
            if (HasKey(section, 0)) { v.X = (float)EvalScalar(section, 0, in ctx); any = true; }
            if (HasKey(section, 1)) { v.Y = (float)EvalScalar(section, 1, in ctx); any = true; }
            return any ? (object)v : null;
        }
    }

    /// <summary>
    /// 四分量向量适配器。这个类顺带把“颜色”也覆盖了 —— 引擎里的 RGBA 颜色
    /// 属性大多数是 Vector4 加一个 [TtColor4PickerEditor] 标注 (见 MeshDataProvider
    /// 里的 Color、Editor.StudioFloorColor), 而不是 Color4f。
    ///
    /// 不对分量做钳制: 颜色可以是 HDR 的 (大于 1), 封顶反而是错的。代价是 Cubic 插值
    /// 在两个关键帧之间可能过冲出负值, 需要时在曲线编辑器里把关键帧改成 Linear。
    /// 不为颜色单独改默认插值模式: 同一个 Vector4 既可能是颜色也可能是普通四元组,
    /// 适配器看不到 [TtColor4PickerEditor] 那个标注, 猜不出区别。
    /// </summary>
    public class TtVector4ValueAdapter : TtScalarValueAdapterBase
    {
        public const string Id = "Vector4";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(Vector4); }
        public override int ScalarChannelCount { get => 4; }
        public override string GetChannelName(int index)
        {
            switch (index)
            {
                case 0: return "X";
                case 1: return "Y";
                case 2: return "Z";
                default: return "W";
            }
        }

        public override void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            if ((value is Vector4) == false)
                return;
            var v = (Vector4)value;
            AddScalarKey(section, 0, tick, v.X, tickResolution);
            AddScalarKey(section, 1, tick, v.Y, tickResolution);
            AddScalarKey(section, 2, tick, v.Z, tickResolution);
            AddScalarKey(section, 3, tick, v.W, tickResolution);
        }
        public override object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if ((currentValue is Vector4) == false)
                return null;
            var v = (Vector4)currentValue;
            bool any = false;
            if (HasKey(section, 0)) { v.X = (float)EvalScalar(section, 0, in ctx); any = true; }
            if (HasKey(section, 1)) { v.Y = (float)EvalScalar(section, 1, in ctx); any = true; }
            if (HasKey(section, 2)) { v.Z = (float)EvalScalar(section, 2, in ctx); any = true; }
            if (HasKey(section, 3)) { v.W = (float)EvalScalar(section, 3, in ctx); any = true; }
            return any ? (object)v : null;
        }
    }

    /// <summary>
    /// Color3f 适配器。引擎里真用 Color3f 作属性类型的地方不多 (颜色多半走 Vector3
    /// 加 [TtColor3PickerEditor]), 但类型匹配是精确的 —— 缺了它, 写成 Color3f 的属性就
    /// 在“加属性轨”列表里根本不会出现。字段名是 Red/Green/Blue, 不是 X/Y/Z。
    /// </summary>
    public class TtColor3fValueAdapter : TtScalarValueAdapterBase
    {
        public const string Id = "Color3f";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(Color3f); }
        public override int ScalarChannelCount { get => 3; }
        public override string GetChannelName(int index)
        {
            return index == 0 ? "R" : (index == 1 ? "G" : "B");
        }

        public override void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            if ((value is Color3f) == false)
                return;
            var c = (Color3f)value;
            AddScalarKey(section, 0, tick, c.Red, tickResolution);
            AddScalarKey(section, 1, tick, c.Green, tickResolution);
            AddScalarKey(section, 2, tick, c.Blue, tickResolution);
        }
        public override object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if ((currentValue is Color3f) == false)
                return null;
            var c = (Color3f)currentValue;
            bool any = false;
            if (HasKey(section, 0)) { c.Red = (float)EvalScalar(section, 0, in ctx); any = true; }
            if (HasKey(section, 1)) { c.Green = (float)EvalScalar(section, 1, in ctx); any = true; }
            if (HasKey(section, 2)) { c.Blue = (float)EvalScalar(section, 2, in ctx); any = true; }
            return any ? (object)c : null;
        }
    }

    /// <summary>Color4f 适配器。同 TtColor3fValueAdapter, 多一条 Alpha 通道。</summary>
    public class TtColor4fValueAdapter : TtScalarValueAdapterBase
    {
        public const string Id = "Color4f";
        public override string AdapterId { get => Id; }
        public override Type ValueType { get => typeof(Color4f); }
        public override int ScalarChannelCount { get => 4; }
        public override string GetChannelName(int index)
        {
            switch (index)
            {
                case 0: return "R";
                case 1: return "G";
                case 2: return "B";
                default: return "A";
            }
        }

        public override void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            if ((value is Color4f) == false)
                return;
            var c = (Color4f)value;
            AddScalarKey(section, 0, tick, c.Red, tickResolution);
            AddScalarKey(section, 1, tick, c.Green, tickResolution);
            AddScalarKey(section, 2, tick, c.Blue, tickResolution);
            AddScalarKey(section, 3, tick, c.Alpha, tickResolution);
        }
        public override object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if ((currentValue is Color4f) == false)
                return null;
            var c = (Color4f)currentValue;
            bool any = false;
            if (HasKey(section, 0)) { c.Red = (float)EvalScalar(section, 0, in ctx); any = true; }
            if (HasKey(section, 1)) { c.Green = (float)EvalScalar(section, 1, in ctx); any = true; }
            if (HasKey(section, 2)) { c.Blue = (float)EvalScalar(section, 2, in ctx); any = true; }
            if (HasKey(section, 3)) { c.Alpha = (float)EvalScalar(section, 3, in ctx); any = true; }
            return any ? (object)c : null;
        }
    }

    /// <summary>旋转适配器, 走四元数通道 (球面插值在通道里做)</summary>
    public class TtQuaternionValueAdapter : ISequenceValueAdapter
    {
        public const string Id = "Quaternion";
        public string AdapterId { get => Id; }
        public Type ValueType { get => typeof(Quaternion); }
        public EChannelKind Kind { get => EChannelKind.Quat; }
        public int ScalarChannelCount { get => 0; }
        public string GetChannelName(int index) { return "Rotation"; }
        public EChannelInterpMode DefaultInterpMode { get => EChannelInterpMode.Cubic; }

        public void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            if ((value is Quaternion) == false)
                return;
            section.QuatChannel.AddKey(tick, (Quaternion)value);
        }
        public object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            if (section.QuatChannel.KeyCount == 0)
                return null;
            return section.QuatChannel.Evaluate(ctx.Time);
        }
    }

    /// <summary>
    /// 资产引用适配器。走 TtNameChannel 的阶梯求值 —— 两个资产路径之间没有中间值。
    ///
    /// 通道给出 null (没打过点, 或者当前时刻在第一个关键帧之前) 时这里也返回 null,
    /// 由 Section 理解成"不覆盖目标当前值", 而不是把目标的资产清空。
    /// </summary>
    public class TtRNameValueAdapter : ISequenceValueAdapter
    {
        public const string Id = "RName";
        public string AdapterId { get => Id; }
        public Type ValueType { get => typeof(RName); }
        public EChannelKind Kind { get => EChannelKind.Name; }
        public int ScalarChannelCount { get => 0; }
        public string GetChannelName(int index) { return "Asset"; }
        public EChannelInterpMode DefaultInterpMode { get => EChannelInterpMode.Constant; }

        public void AddKey(Asset.TtPropertySection section, long tick, object value, in TtFrameRate tickResolution)
        {
            var name = value as RName;
            if (name == null)
                return;
            section.NameChannel.AddKey(tick, name);
        }
        public object Evaluate(Asset.TtPropertySection section, object currentValue, in Asset.FSectionEvalContext ctx)
        {
            return section.NameChannel.Evaluate(ctx.Time);
        }
    }

    /// <summary>
    /// 引擎内置适配器, 在 TtSequencerModule.Initialize 里注册。
    ///
    /// 注册表是按值类型精确匹配的 (只有枚举走 IsEnum 兜底, 见 FindByValueType),
    /// 所以这个列表就是“哪些属性能加轨道”的完整白名单。还没覆盖到的常见类型:
    /// string (需要一条类似 TtNameChannel 的阶梯通道, 不能插值)、long/uint/byte 等其余
    /// 整型、整型向量 (Vector2i/Vector4i 之类)、DVector2/DVector4。它们目前在引擎里
    /// 没有想做动画的属性用例, 真要用时照上面同分量数的适配器抄一份,
    /// 再在这里注册一行。
    /// </summary>
    public static class TtBuiltinValueAdapters
    {
        public static void RegisterAll()
        {
            TtSequenceValueAdapters.Register(new TtDoubleValueAdapter());
            TtSequenceValueAdapters.Register(new TtFloatValueAdapter());
            TtSequenceValueAdapters.Register(new TtIntValueAdapter());
            TtSequenceValueAdapters.Register(new TtBoolValueAdapter());
            TtSequenceValueAdapters.Register(new TtEnumValueAdapter());
            TtSequenceValueAdapters.Register(new TtDVector3ValueAdapter());
            TtSequenceValueAdapters.Register(new TtVector2ValueAdapter());
            TtSequenceValueAdapters.Register(new TtVector3ValueAdapter());
            TtSequenceValueAdapters.Register(new TtVector4ValueAdapter());
            TtSequenceValueAdapters.Register(new TtColor3fValueAdapter());
            TtSequenceValueAdapters.Register(new TtColor4fValueAdapter());
            TtSequenceValueAdapters.Register(new TtQuaternionValueAdapter());
            TtSequenceValueAdapters.Register(new TtRNameValueAdapter());
        }
    }
}

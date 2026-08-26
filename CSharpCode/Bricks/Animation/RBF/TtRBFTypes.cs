using System;
using System.Collections.Generic;

namespace EngineNS.Animation.RBF
{
    /// <summary>
    /// RBF 求解器类型。
    /// </summary>
    public enum ERBFSolverType : byte
    {
        /// <summary>
        /// 累加式: 把每个 target 的贡献直接相加。速度快, 但需要更多 target 才能覆盖完整,
        /// 且依赖归一化步骤才能得到平滑结果。
        /// </summary>
        Additive,
        /// <summary>
        /// 插值式: 按距离对各 target 的值做插值。只要输入落在 target 围成的区域内,
        /// 插值行为良好、无需归一化即可落在 0..1。结果更平滑、需要的 target 更少, 但计算量更大。
        /// </summary>
        Interpolative,
    }

    /// <summary>
    /// 每个 target 的衰减(falloff)函数。
    /// </summary>
    public enum ERBFFunctionType : byte
    {
        Gaussian,
        Exponential,
        Linear,
        Cubic,
        Quintic,
        /// <summary> 沿用父级容器(TtRBFParams)的设置。 </summary>
        DefaultFunction,
    }

    /// <summary>
    /// 输入到各 target 的距离度量方式。
    /// </summary>
    public enum ERBFDistanceMethod : byte
    {
        /// <summary> 标准 n 维距离。 </summary>
        Euclidean,
        /// <summary> 把输入当四元数, 取两者夹角。 </summary>
        Quaternion,
        /// <summary> 把输入当四元数, 取绕 TwistAxis 分解后 swing 部分的夹角。 </summary>
        SwingAngle,
        /// <summary> 把输入当四元数, 取绕 TwistAxis 的扭转角之差。 </summary>
        TwistAngle,
        /// <summary> 沿用父级容器(TtRBFParams)的设置。 </summary>
        DefaultMethod,
    }

    /// <summary>
    /// 权重归一化方式。
    /// </summary>
    public enum ERBFNormalizeMethod : byte
    {
        /// <summary> 仅当权重和超过 1 时才归一化。 </summary>
        OnlyNormalizeAboveOne,
        /// <summary> 总是归一化(权重和为 0 时保持 0)。 </summary>
        AlwaysNormalize,
        /// <summary>
        /// 仅在参考中值锥体内归一化。中值是一个带最小/最大角度的锥体, 在该区间内
        /// 于"不归一化"与"完全归一化"之间插值, 用于界定必须归一化的区域。
        /// </summary>
        NormalizeWithinMedian,
        /// <summary>
        /// 完全不归一化。仅当使用 Interpolative 且确信所有输入都落在 target 围成的
        /// 区域内时才应使用。
        /// </summary>
        NoNormalization,
    }

    /// <summary>
    /// SwingAngle / TwistAngle 使用的扭转基准轴。
    /// </summary>
    public enum ERBFTwistAxis : byte
    {
        X,
        Y,
        Z,
    }

    /// <summary>
    /// RBF 空间中的一个条目(输入或 target 的坐标)。
    ///
    /// 数据布局与 UE FRBFEntry 一致: Values 是一串 float, 每 3 个为一组, 代表一个
    /// 三维量。语义由调用方(驱动源)决定:
    ///   - 驱动源为位移时, 一组 = 一根源骨骼的 position(引擎单位: 米);
    ///   - 驱动源为旋转时, 一组 = 一根源骨骼的欧拉角。
    ///
    /// 单位约定: 欧拉角一律以**弧度**存放, 与本仓库 FRotator.Yaw/Pitch/Roll 一致
    /// (注意 UE 的 FRotator 用度数, 这里不同)。距离函数对外返回度数, 以便与
    /// TtRBFParams.Radius(度数)直接比较。
    /// </summary>
    public class TtRBFEntry
    {
        public List<float> Values { get; set; } = new List<float>();

        /// <summary> 维度数(= Values.Count)。 </summary>
        public int Dimensions => Values.Count;

        /// <summary> 组数(每 3 个 float 一组)。 </summary>
        public int GroupCount => Values.Count / 3;

        /// <summary>
        /// 把第 index 组解释为欧拉角(弧度)。越界时返回零旋转。
        /// </summary>
        public FRotator AsRotator(int index)
        {
            var result = new FRotator();
            int baseIndex = index * 3;
            if (Values.Count >= baseIndex + 3)
            {
                result.Roll = Values[baseIndex + 0];
                result.Pitch = Values[baseIndex + 1];
                result.Yaw = Values[baseIndex + 2];
            }
            return result;
        }

        /// <summary>
        /// 把第 index 组解释为欧拉角并转成四元数。
        /// </summary>
        public Quaternion AsQuat(int index)
        {
            var rotator = AsRotator(index);
            return Quaternion.FromEuler(in rotator);
        }

        /// <summary>
        /// 把第 index 组解释为三维向量。越界时返回零向量。
        /// </summary>
        public Vector3 AsVector(int index)
        {
            int baseIndex = index * 3;
            if (Values.Count >= baseIndex + 3)
                return new Vector3(Values[baseIndex + 0], Values[baseIndex + 1], Values[baseIndex + 2]);
            return Vector3.Zero;
        }

        /// <summary> 追加一组欧拉角(弧度)。 </summary>
        public void AddFromRotator(in FRotator rotator)
        {
            Values.Add(rotator.Roll);
            Values.Add(rotator.Pitch);
            Values.Add(rotator.Yaw);
        }

        /// <summary> 追加一组三维向量。 </summary>
        public void AddFromVector(in Vector3 v)
        {
            Values.Add(v.X);
            Values.Add(v.Y);
            Values.Add(v.Z);
        }

        /// <summary> 追加一组由四元数转来的欧拉角(弧度)。 </summary>
        public void AddFromQuat(in Quaternion quat)
        {
            var rotator = quat.ToEuler();
            AddFromRotator(in rotator);
        }

        public void Reset()
        {
            Values.Clear();
        }

        public void CopyFrom(TtRBFEntry other)
        {
            Values.Clear();
            for (int i = 0; i < other.Values.Count; ++i)
                Values.Add(other.Values[i]);
        }
    }

    /// <summary>
    /// RBF 中的一个 target: 在 Entry 坐标之外附带缩放、自定义曲线与可覆盖的度量/衰减设置。
    /// </summary>
    public class TtRBFTarget : TtRBFEntry
    {
        /// <summary> 该 target 的影响范围倍率(Additive 下会乘到半径上)。 </summary>
        public float ScaleFactor { get; set; } = 1.0f;

        /// <summary>
        /// 激活该 target 时是否额外套一条自定义曲线做权重重映射。Interpolative 下忽略。
        /// </summary>
        public bool ApplyCustomCurve { get; set; } = false;

        /// <summary>
        /// 自定义曲线求值委托(输入原始权重, 输出重映射后的权重), ApplyCustomCurve 为真时生效。
        /// 数学层不绑定具体曲线类型, 由上层(节点)注入, 例如注入 TtKawaiiCurve.Evaluate。
        /// Interpolative 下忽略。
        /// </summary>
        public Func<float, float> CustomCurveEvaluator { get; set; } = null;

        /// <summary>
        /// 覆盖该 target 使用的距离度量。DefaultMethod 表示沿用 TtRBFParams 的设置。
        /// Interpolative 下忽略。
        /// </summary>
        public ERBFDistanceMethod DistanceMethod { get; set; } = ERBFDistanceMethod.DefaultMethod;

        /// <summary>
        /// 覆盖该 target 使用的衰减函数。DefaultFunction 表示沿用 TtRBFParams 的设置。
        /// Interpolative 下忽略。
        /// </summary>
        public ERBFFunctionType FunctionType { get; set; } = ERBFFunctionType.DefaultFunction;
    }

    /// <summary>
    /// RBF 求解输出: target 下标与对应权重。
    /// </summary>
    public struct TtRBFOutputWeight
    {
        public int TargetIndex;
        public float TargetWeight;

        public TtRBFOutputWeight(int targetIndex, float targetWeight)
        {
            TargetIndex = targetIndex;
            TargetWeight = targetWeight;
        }
    }

    /// <summary>
    /// RBF 求解参数。
    /// </summary>
    public class TtRBFParams
    {
        /// <summary> 输入数据的维度数(应与 TtRBFEntry.Dimensions 一致)。 </summary>
        public int TargetDimensions { get; set; } = 3;

        /// <summary> 求解器类型。 </summary>
        public ERBFSolverType SolverType { get; set; } = ERBFSolverType.Additive;

        /// <summary> 各 target 的默认半径, 单位为度。AutomaticRadius 为真时忽略。 </summary>
        public float Radius { get; set; } = 1.0f;

        /// <summary> 是否按 target 间平均距离自动取半径。 </summary>
        public bool AutomaticRadius { get; set; } = false;

        /// <summary> 默认衰减函数。 </summary>
        public ERBFFunctionType Function { get; set; } = ERBFFunctionType.Gaussian;

        /// <summary> 默认距离度量。 </summary>
        public ERBFDistanceMethod DistanceMethod { get; set; } = ERBFDistanceMethod.Euclidean;

        /// <summary> SwingAngle / TwistAngle 使用的扭转轴。 </summary>
        public ERBFTwistAxis TwistAxis { get; set; } = ERBFTwistAxis.X;

        /// <summary> 低于该权重的 target 不计入输出。 </summary>
        public float WeightThreshold { get; set; } = 1e-4f;

        /// <summary> 权重归一化方式。 </summary>
        public ERBFNormalizeMethod NormalizeMethod { get; set; } = ERBFNormalizeMethod.OnlyNormalizeAboveOne;

        /// <summary> 中值参考(NormalizeWithinMedian 用), 语义与 Entry 的一组三维量相同。 </summary>
        public Vector3 MedianReference { get; set; } = Vector3.Zero;

        /// <summary> 中值最小角(度), NormalizeWithinMedian 用。 </summary>
        public float MedianMin { get; set; } = 45.0f;

        /// <summary> 中值最大角(度), NormalizeWithinMedian 用。 </summary>
        public float MedianMax { get; set; } = 60.0f;

        /// <summary> 取扭转轴的单位方向向量。 </summary>
        public Vector3 GetTwistAxisVector()
        {
            switch (TwistAxis)
            {
                default:
                case ERBFTwistAxis.X:
                    return new Vector3(1.0f, 0.0f, 0.0f);
                case ERBFTwistAxis.Y:
                    return new Vector3(0.0f, 1.0f, 0.0f);
                case ERBFTwistAxis.Z:
                    return new Vector3(0.0f, 0.0f, 1.0f);
            }
        }

        public TtRBFParams Clone()
        {
            return new TtRBFParams()
            {
                TargetDimensions = TargetDimensions,
                SolverType = SolverType,
                Radius = Radius,
                AutomaticRadius = AutomaticRadius,
                Function = Function,
                DistanceMethod = DistanceMethod,
                TwistAxis = TwistAxis,
                WeightThreshold = WeightThreshold,
                NormalizeMethod = NormalizeMethod,
                MedianReference = MedianReference,
                MedianMin = MedianMin,
                MedianMax = MedianMax,
            };
        }
    }
}

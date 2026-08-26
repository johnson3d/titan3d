using System;
using System.Collections.Generic;

namespace EngineNS.Animation.RBF
{
    /// <summary>
    /// RBF 距离度量与衰减核。
    ///
    /// 数值形式对标 UE RBFDistanceMetric / RBFKernel(AnimGraphRuntime), 以保证与 UE
    /// PoseDriver 行为一致。两处与 UE 的差异已在对应函数注释中标明:
    ///   1. 本仓库 FRotator 的 Yaw/Pitch/Roll 是弧度(UE 是度数), 故 Euclidean 不需要
    ///      再做一次度->弧度换算;
    ///   2. 距离对外统一返回度数, 以便与 TtRBFParams.Radius(度数)直接相除。
    /// </summary>
    public static class TtRBFMath
    {
        public const float RadToDeg = 180.0f / MathF.PI;
        public const float DegToRad = MathF.PI / 180.0f;

        /// <summary> 与 UE KINDA_SMALL_NUMBER 对齐。 </summary>
        public const float KindaSmallNumber = 1e-4f;

        #region 四元数辅助

        /// <summary>
        /// 两个四元数之间的夹角(弧度)。对标 UE FQuat::AngularDistance:
        /// acos(2 * dot^2 - 1)。输入需为单位四元数。
        /// </summary>
        public static float AngularDistance(in Quaternion a, in Quaternion b)
        {
            float innerProd = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
            float v = (2.0f * innerProd * innerProd) - 1.0f;
            // acos 定义域保护: 浮点误差可能让 v 略微越界导致 NaN
            v = Math.Clamp(v, -1.0f, 1.0f);
            return MathF.Acos(v);
        }

        /// <summary>
        /// 把四元数按给定扭转轴分解为 swing 与 twist。对标 UE FQuat::ToSwingTwist:
        /// 把虚部投影到扭转轴得到 twist, 再由 swing = q * twist^-1 得到 swing。
        /// </summary>
        public static void ToSwingTwist(in Quaternion quat, in Vector3 twistAxis,
            out Quaternion outSwing, out Quaternion outTwist)
        {
            // 虚部投影到扭转轴
            var imaginary = new Vector3(quat.X, quat.Y, quat.Z);
            float projScale = Vector3.Dot(in twistAxis, in imaginary);
            var projection = twistAxis * projScale;

            outTwist = new Quaternion(projection.X, projection.Y, projection.Z, quat.W);

            // 接近 180 度处的奇点
            if (outTwist.LengthSquared() <= 0.0f)
                outTwist = Quaternion.Identity;
            else
                outTwist = Quaternion.Normalize(outTwist);

            var twistInv = Quaternion.Invert(outTwist);
            outSwing = Quaternion.Multiply(in quat, in twistInv);
        }

        /// <summary>
        /// 四元数绕给定轴的扭转角(弧度, 已归入 -PI..PI)。
        /// 对标 UE FQuat::GetTwistAngle: UnwindRadians(2 * atan2(dot(axis, xyz), W))。
        /// </summary>
        public static float GetTwistAngle(in Quaternion quat, in Vector3 twistAxis)
        {
            var imaginary = new Vector3(quat.X, quat.Y, quat.Z);
            float xyz = Vector3.Dot(in twistAxis, in imaginary);
            return UnwindRadians(2.0f * MathF.Atan2(xyz, quat.W));
        }

        /// <summary>
        /// 把弧度归入 -PI..PI。对标 UE FMath::UnwindRadians。
        /// </summary>
        public static float UnwindRadians(float value)
        {
            while (value > MathF.PI)
                value -= 2.0f * MathF.PI;
            while (value < -MathF.PI)
                value += 2.0f * MathF.PI;
            return value;
        }

        #endregion

        #region 距离度量

        /// <summary>
        /// 两个条目之间的距离, 返回**弧度**。对标 UE GetDistanceBetweenEntries:
        /// 逐组算距离后平方累加, 最后取平方根。
        /// </summary>
        public static float GetDistanceBetweenEntries(TtRBFEntry a, TtRBFEntry b,
            ERBFDistanceMethod distanceMethod, in Vector3 twistAxis)
        {
            System.Diagnostics.Debug.Assert(a.Dimensions == b.Dimensions);

            int groupCount = a.GroupCount;
            float totalDistance = 0.0f;

            for (int i = 0; i < groupCount; ++i)
            {
                float distance = 0.0f;
                switch (distanceMethod)
                {
                    case ERBFDistanceMethod.Euclidean:
                        {
                            // 本仓库 FRotator 已是弧度, 直接作为三维量取 L2 距离
                            var ra = a.AsRotator(i);
                            var rb = b.AsRotator(i);
                            var va = new Vector3(ra.Roll, ra.Pitch, ra.Yaw);
                            var vb = new Vector3(rb.Roll, rb.Pitch, rb.Yaw);
                            distance = (va - vb).Length();
                        }
                        break;

                    case ERBFDistanceMethod.Quaternion:
                        {
                            var qa = Quaternion.Normalize(a.AsQuat(i));
                            var qb = Quaternion.Normalize(b.AsQuat(i));
                            distance = AngularDistance(in qa, in qb);
                        }
                        break;

                    // 与 UE 一致: DefaultMethod 落到 SwingAngle
                    case ERBFDistanceMethod.SwingAngle:
                    case ERBFDistanceMethod.DefaultMethod:
                        {
                            var qa = a.AsQuat(i);
                            var qb = b.AsQuat(i);
                            ToSwingTwist(in qa, in twistAxis, out var aSwing, out _);
                            ToSwingTwist(in qb, in twistAxis, out var bSwing, out _);
                            distance = AngularDistance(in aSwing, in bSwing);
                        }
                        break;

                    case ERBFDistanceMethod.TwistAngle:
                        {
                            var qa = a.AsQuat(i);
                            var qb = b.AsQuat(i);
                            distance = MathF.Abs(GetTwistAngle(in qa, in twistAxis) - GetTwistAngle(in qb, in twistAxis));
                        }
                        break;
                }

                totalDistance += distance * distance;
            }

            return MathF.Sqrt(totalDistance);
        }

        /// <summary>
        /// 两个条目之间的距离, 返回**度数**(与 Radius 单位一致)。
        /// overrideMethod 为 DefaultMethod 时沿用 params 的设置。
        /// 对标 UE FRBFSolver::FindDistanceBetweenEntries。
        /// </summary>
        public static float FindDistanceBetweenEntries(TtRBFEntry a, TtRBFEntry b,
            TtRBFParams rbfParams, ERBFDistanceMethod overrideMethod = ERBFDistanceMethod.DefaultMethod)
        {
            var distanceMethod = (overrideMethod == ERBFDistanceMethod.DefaultMethod)
                ? rbfParams.DistanceMethod
                : overrideMethod;

            float distance = GetDistanceBetweenEntries(a, b, distanceMethod, rbfParams.GetTwistAxisVector());
            return distance * RadToDeg;
        }

        #endregion

        #region 衰减核

        // 以下 5 个核对标 UE RBFKernel 命名空间。sigma 即核宽度(KernelWidth)。
        // 约定: 输入 0 时返回 1, 随距离单调递减。

        public static float KernelLinear(float value, float sigma)
        {
            if (sigma == 0.0f)
                return 0.0f;
            return (sigma - Math.Clamp(value, 0.0f, sigma)) / sigma;
        }

        /// <summary>
        /// 注意: 对标 UE 的写法 Exp(-Value * Square(1/Sigma)), 指数上是 value 的一次方
        /// 乘以 (1/sigma)^2, 并非常见的 Exp(-(value/sigma)^2)。
        /// </summary>
        public static float KernelGaussian(float value, float sigma)
        {
            if (sigma == 0.0f)
                return 0.0f;
            float invSigma = 1.0f / sigma;
            return MathF.Exp(-value * (invSigma * invSigma));
        }

        public static float KernelExponential(float value, float sigma)
        {
            if (sigma == 0.0f)
                return 0.0f;
            return MathF.Exp(-2.0f * value / sigma);
        }

        public static float KernelCubic(float value, float sigma)
        {
            if (sigma == 0.0f)
                return 0.0f;
            float v = value / sigma;
            return MathF.Max(1.0f - (v * v * v), 0.0f);
        }

        public static float KernelQuintic(float value, float sigma)
        {
            if (sigma == 0.0f)
                return 0.0f;
            float v = value / sigma;
            return MathF.Max(1.0f - MathF.Pow(v, 5.0f), 0.0f);
        }

        /// <summary>
        /// 按衰减函数类型求权重。
        ///
        /// backCompFix 对应 UE 的同名开关: Additive 求解器走该分支(sigma 固定为 1, 改用
        /// 半径去缩放距离), 其 Linear / Gaussian / Exponential 使用较宽的旧公式;
        /// Interpolative 走非 backComp 分支, 由 sigma(核宽度)控制衰减宽度。
        /// Cubic / Quintic 两者共用同一公式。
        /// </summary>
        public static float GetWeightedValue(float value, float kernelWidth,
            ERBFFunctionType functionType, bool backCompFix = false)
        {
            if (value < 0.0f)
                return 0.0f;

            switch (functionType)
            {
                default:
                case ERBFFunctionType.Linear:
                case ERBFFunctionType.DefaultFunction:
                    // 旧公式没有衰减宽度控制, 直接忽略距离大于 1 的部分
                    return backCompFix ? MathF.Max(1.0f - value, 0.0f) : KernelLinear(value, kernelWidth);

                case ERBFFunctionType.Gaussian:
                    // 旧公式的衰减比新公式宽得多
                    return backCompFix ? MathF.Exp(-value * value) : KernelGaussian(value, kernelWidth);

                case ERBFFunctionType.Exponential:
                    return backCompFix ? (1.0f / MathF.Exp(value)) : KernelExponential(value, kernelWidth);

                case ERBFFunctionType.Cubic:
                    return KernelCubic(value, kernelWidth);

                case ERBFFunctionType.Quintic:
                    return KernelQuintic(value, kernelWidth);
            }
        }

        #endregion

        #region 半径

        /// <summary>
        /// 取某个 target 的有效半径(度)。Additive 下会乘上该 target 的 ScaleFactor。
        /// 对标 UE FRBFSolver::GetRadiusForTarget。
        /// </summary>
        public static float GetRadiusForTarget(TtRBFTarget target, TtRBFParams rbfParams)
        {
            float radius = rbfParams.Radius;
            if (rbfParams.SolverType == ERBFSolverType.Additive)
                radius *= target.ScaleFactor;

            return MathF.Max(radius, KindaSmallNumber);
        }

        /// <summary>
        /// 所有 target 两两距离的平均值(**弧度**), 用作自动核宽度。
        /// 对标 UE GetOptimalKernelWidth。
        /// </summary>
        public static float GetOptimalKernelWidth(TtRBFParams rbfParams, in Vector3 twistAxis,
            IReadOnlyList<TtRBFEntry> targets)
        {
            float sum = 0.0f;
            int count = 0;

            for (int i = 0; i < targets.Count; ++i)
            {
                for (int j = 0; j < targets.Count; ++j)
                {
                    if (i == j)
                        continue;
                    sum += GetDistanceBetweenEntries(targets[i], targets[j], rbfParams.DistanceMethod, twistAxis);
                    ++count;
                }
            }

            if (count == 0)
                return 0.0f;
            return sum / count;
        }

        /// <summary>
        /// 按 target 分布推荐的最优半径(**度**)。对标 UE FRBFSolver::GetOptimalRadiusForTargets。
        /// </summary>
        public static float GetOptimalRadiusForTargets(TtRBFParams rbfParams, IReadOnlyList<TtRBFTarget> targets)
        {
            var entries = new List<TtRBFEntry>(targets.Count);
            for (int i = 0; i < targets.Count; ++i)
                entries.Add(targets[i]);

            float kernelWidth = GetOptimalKernelWidth(rbfParams, rbfParams.GetTwistAxisVector(), entries);
            return kernelWidth * RadToDeg;
        }

        #endregion
    }
}

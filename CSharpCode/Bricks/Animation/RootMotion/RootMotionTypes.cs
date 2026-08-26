using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.RootMotion
{
    /// <summary>
    /// RootMotion的来源过滤, 语义对齐UE的ERootMotionMode
    /// </summary>
    public enum ERootMotionMode
    {
        /// <summary>
        /// 不提取RootMotion, 动画的根骨骼位移保留在Pose中(原地位移由美术烘进动画)
        /// </summary>
        Ignore,
        /// <summary>
        /// 所有开启了EnableRootMotion的动画都贡献RootMotion
        /// </summary>
        FromEverything,
        /// <summary>
        /// 只有Montage中的动画贡献RootMotion, 基础Locomotion不贡献
        /// </summary>
        FromMontagesOnly,
    }

    /// <summary>
    /// 提取RootMotion后, 输出Pose中根骨骼被锁定到哪个变换, 防止位移被应用两次
    /// </summary>
    public enum ERootMotionRootLock
    {
        /// <summary>
        /// 锁到骨骼的绑定姿势(InitMatrix)
        /// </summary>
        RefPose,
        /// <summary>
        /// 锁到该动画第一帧的根骨骼变换
        /// </summary>
        AnimFirstFrame,
        /// <summary>
        /// 锁到Identity
        /// </summary>
        Zero,
    }

    /// <summary>
    /// 随Pose一起在命令图中流动的RootMotion数据。
    /// 采样节点写入, 混合节点按与Pose相同的权重混合, 最终由Movement消费。
    /// </summary>
    public struct FRootMotionData
    {
        public bool HasRootMotion;
        /// <summary>
        /// 该位移是否来自Montage, 供ERootMotionMode.FromMontagesOnly过滤使用
        /// </summary>
        public bool FromMontage;
        /// <summary>
        /// 本帧的位移增量, 表达在Actor(根骨骼的父)空间
        /// </summary>
        public FTransform Delta;

        public static readonly FRootMotionData Empty = new FRootMotionData() { HasRootMotion = false, FromMontage = false, Delta = FTransform.Identity };

        public static FRootMotionData FromDelta(in FTransform delta, bool fromMontage = false)
        {
            FRootMotionData result;
            result.HasRootMotion = true;
            result.FromMontage = fromMontage;
            result.Delta = delta;
            return result;
        }
    }

    /// <summary>
    /// RootMotion的变换运算。
    ///
    /// 本引擎的变换约定(与UE不同, 不要直接照搬UE公式):
    ///   FTransform.Multiply(out, A, B) 语义为"A是局部(子), B是父", 结果 = A在B空间中的表达;
    ///   Quaternion的 operator* 实际是 DXQuaternionMultiply(q1,q2) = Hamilton(q2 ⊗ q1),
    ///   即 q1 * q2 表示"先转q1再转q2"。
    /// 因此Delta的定义为: Multiply(Delta, Start) == End, 即Delta是施加在Start之前的增量。
    /// 该关系由RootMotionUTest锁定, 修改本文件必须跑单测。
    /// </summary>
    public static class TtRootMotionUtil
    {
        static Vector3 SafeScaleReciprocal(in Vector3 scale)
        {
            Vector3 result;
            result.X = Math.Abs(scale.X) <= MathHelper.Epsilon ? 0.0f : 1.0f / scale.X;
            result.Y = Math.Abs(scale.Y) <= MathHelper.Epsilon ? 0.0f : 1.0f / scale.Y;
            result.Z = Math.Abs(scale.Z) <= MathHelper.Epsilon ? 0.0f : 1.0f / scale.Z;
            return result;
        }

        /// <summary>
        /// 求从start到end的增量, 满足 ApplyDelta(start, CalcDelta(start, end)) == end。
        /// Scale不参与RootMotion, 输出恒为One。
        /// </summary>
        public static FTransform CalcDelta(in FTransform start, in FTransform end)
        {
            var deltaQuat = end.Quat * start.Quat.Inverse();
            deltaQuat.Normalize();
            // Multiply中父的位置计算为 B.Quat * (A.Position * B.Scale) + B.Position, 这里反解A.Position
            var deltaPos = Quaternion.UnrotateVector3(start.Quat, end.Position - start.Position) * SafeScaleReciprocal(start.Scale);
            return FTransform.CreateTransform(deltaPos, Vector3.One, deltaQuat);
        }

        /// <summary>
        /// 把增量施加到基准变换上, 增量表达在基准变换的父空间中的局部含义
        /// </summary>
        public static FTransform ApplyDelta(in FTransform baseTransform, in FTransform delta)
        {
            FTransform result;
            FTransform.Multiply(out result, in delta, in baseTransform);
            // RootMotion不携带缩放, 保持基准的缩放不被Multiply改写
            result.Scale = baseTransform.Scale;
            return result;
        }

        /// <summary>
        /// 顺序复合两个增量: 先first再second, 结果等价于两步依次ApplyDelta
        /// </summary>
        public static FTransform Combine(in FTransform first, in FTransform second)
        {
            FTransform result;
            FTransform.Multiply(out result, in second, in first);
            result.Scale = Vector3.One;
            return result;
        }

        /// <summary>
        /// 增量之间的线性混合, alpha为b的权重。与Pose混合(位置Lerp+旋转Slerp)保持一致
        /// </summary>
        public static FTransform BlendDelta(in FTransform a, in FTransform b, float alpha)
        {
            // 属性不能用in传引用(CS8156), 先取到局部变量
            var aQuat = a.Quat;
            var bQuat = b.Quat;
            var pos = DVector3.Lerp(a.Position, b.Position, alpha);
            var quat = Quaternion.Slerp(in aQuat, in bQuat, alpha);
            quat.Normalize();
            return FTransform.CreateTransform(pos, Vector3.One, quat);
        }

        /// <summary>
        /// 两路RootMotion按alpha混合(alpha为b的权重)。缺失的一路视为Identity增量。
        /// </summary>
        public static FRootMotionData Blend(in FRootMotionData a, in FRootMotionData b, float alpha)
        {
            if (!a.HasRootMotion && !b.HasRootMotion)
                return FRootMotionData.Empty;

            FRootMotionData result;
            result.HasRootMotion = true;
            result.FromMontage = (a.HasRootMotion && a.FromMontage) || (b.HasRootMotion && b.FromMontage);
            var aDelta = a.HasRootMotion ? a.Delta : FTransform.Identity;
            var bDelta = b.HasRootMotion ? b.Delta : FTransform.Identity;
            result.Delta = BlendDelta(in aDelta, in bDelta, alpha);
            return result;
        }

        /// <summary>
        /// 多路加权累加。权重之和应为1(由调用方保证), 与TtRuntimePoseUtility.BlendPoses的多路混合语义一致:
        /// 位置加权求和, 旋转做符号对齐后的加权和再归一化(NLERP)。
        /// </summary>
        public static void AccumulateWeighted(ref FRootMotionData accumulator, ref bool hasRefQuat, ref Quaternion refQuat, in FRootMotionData source, float weight)
        {
            if (!source.HasRootMotion || weight == 0.0f)
                return;

            var delta = source.Delta;
            if (!accumulator.HasRootMotion)
            {
                // 首个有效来源作为累加起点, 位置与旋转都从零开始累加
                accumulator.HasRootMotion = true;
                accumulator.Delta = FTransform.CreateTransform(DVector3.Zero, Vector3.One, new Quaternion(0, 0, 0, 0));
            }
            accumulator.FromMontage |= source.FromMontage;

            var quat = delta.Quat;
            if (!hasRefQuat)
            {
                refQuat = quat;
                hasRefQuat = true;
            }
            else if (Quaternion.Dot(refQuat, quat) < 0.0f)
            {
                quat = Quaternion.MultiplyFloat(in quat, -1.0f);
            }

            var acc = accumulator.Delta;
            acc.Position += delta.Position * weight;
            var accQuat = acc.Quat;
            acc.Quat = Quaternion.Add(in accQuat, Quaternion.MultiplyFloat(in quat, weight));
            accumulator.Delta = acc;
        }

        /// <summary>
        /// 结束多路加权累加, 归一化旋转。累加过程中旋转是未归一化的四元数和, 必须调用本函数收尾。
        /// </summary>
        public static void FinishAccumulate(ref FRootMotionData accumulator)
        {
            if (!accumulator.HasRootMotion)
                return;

            var acc = accumulator.Delta;
            if (acc.Quat.LengthSquared() > 1e-8f)
                acc.Quat = Quaternion.Normalize(acc.Quat);
            else
                acc.Quat = Quaternion.Identity;
            acc.Scale = Vector3.One;
            accumulator.Delta = acc;
        }

        /// <summary>
        /// 把Actor空间的位移增量转换为世界空间位移
        /// </summary>
        public static DVector3 ConvertDeltaToWorldTranslation(in FTransform delta, in Quaternion actorQuat)
        {
            var localTranslation = delta.Position;
            return Quaternion.RotateVector3(in actorQuat, in localTranslation);
        }
    }
}

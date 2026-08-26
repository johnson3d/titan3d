using System;
using System.Collections.Generic;

namespace EngineNS.Animation.RBF
{
    /// <summary>
    /// RBF 插值器(Interpolative 求解器的核心)。对标 UE TRBFInterpolator + FRBFInterpolatorBase。
    ///
    /// 原理: 常规 RBF 是解 A * w = T (A 为各节点间距离构成的对称核矩阵)。而这里每个节点的
    /// 输出激活值被视为 N 维向量中的一个分量, 即目标矩阵为单位矩阵, 于是问题化简为
    ///   A * w = I   =>   w = A^-1
    /// 因此只需对核矩阵求逆并缓存系数即可。
    ///
    /// 与 UE 的两点实现差异(有意为之, 已验证行为等价或更稳健):
    ///   1. UE 用 Eigen 的 MatrixXf(float) 求逆并以 determinant 近零判定不可逆。这里用
    ///      高斯-约当消元 + 部分主元, 以"消元过程中主元近零"判定奇异 —— 对较大的 N
    ///      比行列式判据更数值稳健(行列式在高维下量级会失真)。
    ///   2. 求逆的中间运算用 double, 结果再转回 float, 以减少精度损失。
    /// </summary>
    public class TtRBFInterpolator
    {
        /// <summary> 求解出的系数矩阵(行主序, NodeCount * NodeCount)。 </summary>
        public float[] Coeffs { get; private set; } = null;

        /// <summary> 核矩阵是否可逆(不可逆时 Interpolate 全部输出 0)。 </summary>
        public bool IsValid { get; private set; } = false;

        public int NodeCount => mNodes != null ? mNodes.Count : 0;

        IReadOnlyList<TtRBFEntry> mNodes = null;
        Func<TtRBFEntry, TtRBFEntry, float> mWeightFunc = null;

        public TtRBFInterpolator()
        {
        }

        /// <summary>
        /// 用一组节点与一个对称权重函数构建插值器。权重函数需满足对称性
        /// (weightFunc(a,b) == weightFunc(b,a))。
        /// </summary>
        public TtRBFInterpolator(IReadOnlyList<TtRBFEntry> nodes, Func<TtRBFEntry, TtRBFEntry, float> weightFunc)
        {
            Build(nodes, weightFunc);
        }

        public void Build(IReadOnlyList<TtRBFEntry> nodes, Func<TtRBFEntry, TtRBFEntry, float> weightFunc)
        {
            mNodes = nodes;
            mWeightFunc = weightFunc;
            Coeffs = null;
            IsValid = false;

            int n = (nodes != null) ? nodes.Count : 0;
            // 少于两个节点时无需求解: 整个空间的插值结果一致, 在 Interpolate 内直接处理
            if (n < 2)
            {
                IsValid = true;
                return;
            }

            // 构建完整对称核矩阵。对角线也要用权重函数求值 —— 不能假定同坐标节点的
            // 权重恒为 1(取决于所用的核与半径)。
            var full = new double[n * n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = i; j < n; ++j)
                {
                    double w = weightFunc(nodes[i], nodes[j]);
                    full[i * n + j] = w;
                    full[j * n + i] = w;
                }
            }

            var inv = new double[n * n];
            if (InvertMatrix(full, n, inv))
            {
                Coeffs = new float[n * n];
                for (int i = 0; i < n * n; ++i)
                    Coeffs[i] = (float)inv[i];
                IsValid = true;
            }
            else
            {
                IsValid = false;
            }
        }

        /// <summary>
        /// 给定一个输入值, 求各节点对该值的贡献权重。
        /// 对标 UE TRBFInterpolator::Interpolate。
        /// </summary>
        /// <param name="outWeights">输出权重(会被清空重填)</param>
        /// <param name="value">输入条目</param>
        /// <param name="clip">是否把权重裁剪到 0..1(输入落在节点凸包之外时会外插出界)</param>
        /// <param name="normalize">是否归一化(会先整体抬升以消除负值, 再按总和归一)</param>
        public void Interpolate(List<float> outWeights, TtRBFEntry value, bool clip = true, bool normalize = false)
        {
            outWeights.Clear();

            int n = NodeCount;
            if (!IsValid)
            {
                for (int i = 0; i < n; ++i)
                    outWeights.Add(0.0f);
                return;
            }

            if (n > 1)
            {
                var valueWeights = new float[n];
                for (int i = 0; i < n; ++i)
                    valueWeights[i] = mWeightFunc(value, mNodes[i]);

                for (int i = 0; i < n; ++i)
                {
                    int rowBase = i * n;
                    float w = 0.0f;
                    for (int j = 0; j < n; ++j)
                        w += Coeffs[rowBase + j] * valueWeights[j];
                    outWeights.Add(w);
                }

                if (normalize)
                {
                    // 归一化时的裁剪语义与不归一化时不同: 不是直接截断, 而是先按最小负值
                    // 整体抬升, 再靠归一化把值拉回 0..1
                    if (clip)
                    {
                        float maxNegative = 0.0f;
                        for (int i = 0; i < n; ++i)
                        {
                            if (outWeights[i] < maxNegative)
                                maxNegative = outWeights[i];
                        }
                        for (int i = 0; i < n; ++i)
                            outWeights[i] -= maxNegative;
                    }

                    float totalWeight = 0.0f;
                    for (int i = 0; i < n; ++i)
                        totalWeight += outWeights[i];

                    if (MathF.Abs(totalWeight) > TtRBFMath.KindaSmallNumber)
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            // 再夹一次以消除精度问题。权重和可能不精确等于 1, 对本用途足够。
                            outWeights[i] = Math.Clamp(outWeights[i] / totalWeight, 0.0f, 1.0f);
                        }
                    }
                }
                else if (clip)
                {
                    // 输入落在节点围成的凸包之外时会变成外插, 很容易出界
                    for (int i = 0; i < n; ++i)
                        outWeights[i] = Math.Clamp(outWeights[i], 0.0f, 1.0f);
                }
            }
            else if (n == 1)
            {
                outWeights.Add(1.0f);
            }
        }

        /// <summary>
        /// 找出权重与"同一节点自身的权重"几乎相同的节点对。这类重复节点会让核矩阵奇异,
        /// 导致插值失效。调用方可据此移除其中之一或提示用户配置有误。
        /// 对标 UE TRBFInterpolator::GetIdenticalNodePairs。
        /// </summary>
        /// <returns>存在非法节点对时返回 true</returns>
        public static bool GetIdenticalNodePairs(IReadOnlyList<TtRBFEntry> nodes,
            Func<TtRBFEntry, TtRBFEntry, float> weightFunc, List<(int, int)> outInvalidPairs)
        {
            outInvalidPairs.Clear();

            int n = (nodes != null) ? nodes.Count : 0;
            if (n < 2)
                return false;

            // 前提是权重函数对称, 因此可用"节点与自身"的权重作为恒等权重的等价物
            float identityWeight = weightFunc(nodes[0], nodes[0]);

            for (int i = 0; i < n - 1; ++i)
            {
                for (int j = i + 1; j < n; ++j)
                {
                    float weight = weightFunc(nodes[i], nodes[j]);
                    // 比逐位相等宽松一些: 求逆本身会损失一部分浮点精度
                    if (MathF.Abs(weight - identityWeight) <= 1e-5f * MathF.Max(1.0f, MathF.Abs(identityWeight)))
                        outInvalidPairs.Add((i, j));
                }
            }

            return outInvalidPairs.Count != 0;
        }

        /// <summary>
        /// N x N 稠密矩阵求逆(高斯-约当消元 + 部分主元)。矩阵按行主序存放。
        /// 主元近零时判定为奇异并返回 false(此时 outInv 内容无意义)。
        /// </summary>
        static bool InvertMatrix(double[] m, int n, double[] outInv)
        {
            // 增广矩阵 [A | I], 消元后右半变为 A^-1
            int stride = 2 * n;
            var a = new double[n * stride];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    a[i * stride + j] = m[i * n + j];
                a[i * stride + n + i] = 1.0;
            }

            for (int col = 0; col < n; ++col)
            {
                // 部分主元: 选当前列绝对值最大的行
                int pivotRow = col;
                double maxAbs = Math.Abs(a[col * stride + col]);
                for (int r = col + 1; r < n; ++r)
                {
                    double v = Math.Abs(a[r * stride + col]);
                    if (v > maxAbs)
                    {
                        maxAbs = v;
                        pivotRow = r;
                    }
                }

                // 主元近零 => 奇异矩阵
                if (maxAbs <= 1e-12)
                    return false;

                if (pivotRow != col)
                {
                    for (int j = 0; j < stride; ++j)
                    {
                        double tmp = a[col * stride + j];
                        a[col * stride + j] = a[pivotRow * stride + j];
                        a[pivotRow * stride + j] = tmp;
                    }
                }

                // 主元行归一
                double pivot = a[col * stride + col];
                double invPivot = 1.0 / pivot;
                for (int j = 0; j < stride; ++j)
                    a[col * stride + j] *= invPivot;

                // 消去其它行该列
                for (int r = 0; r < n; ++r)
                {
                    if (r == col)
                        continue;
                    double factor = a[r * stride + col];
                    if (factor == 0.0)
                        continue;
                    for (int j = 0; j < stride; ++j)
                        a[r * stride + j] -= factor * a[col * stride + j];
                }
            }

            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    outInv[i * n + j] = a[i * stride + n + j];
            }
            return true;
        }
    }
}

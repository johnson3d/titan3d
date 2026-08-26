using System;
using System.Collections.Generic;

namespace EngineNS.Animation.RBF
{
    /// <summary>
    /// Interpolative 求解所需的预计算数据。由 TtRBFSolver.InitSolver 生成并缓存,
    /// 只要 target 与相关参数没变就可复用(用 IsSolverDataValid 判断)。
    /// 对标 UE FRBFSolverData。
    /// </summary>
    public class TtRBFSolverData
    {
        public TtRBFParams Params { get; set; } = null;
        public List<TtRBFEntry> EntryTargets { get; set; } = new List<TtRBFEntry>();
        public TtRBFInterpolator Interpolator { get; set; } = null;
    }

    /// <summary>
    /// 径向基函数(RBF)求解器: 给定一组 target 与一个输入, 求各 target 的激活权重。
    /// 对标 UE FRBFSolver(AnimGraphRuntime)。
    ///
    /// 用法:
    ///   var data = TtRBFSolver.InitSolver(params, targets);      // target 变化时重建
    ///   TtRBFSolver.Solve(data, params, targets, input, outWeights);
    /// </summary>
    public static class TtRBFSolver
    {
        /// <summary>
        /// Interpolative 求解使用的权重函数(距离 -> 核衰减)。
        /// 半径来源: AutomaticRadius 为真时取 target 间平均距离, 否则取 Params.Radius
        /// (度转弧度, 因为这里的距离以弧度计算)。
        /// </summary>
        static Func<TtRBFEntry, TtRBFEntry, float> MakeInterpolativeWeightFunc(
            TtRBFParams rbfParams, IReadOnlyList<TtRBFEntry> targets)
        {
            var twistAxis = rbfParams.GetTwistAxisVector();

            float kernelWidth;
            if (rbfParams.AutomaticRadius)
                kernelWidth = TtRBFMath.GetOptimalKernelWidth(rbfParams, in twistAxis, targets);
            else
                kernelWidth = rbfParams.Radius * TtRBFMath.DegToRad;

            var distanceMethod = rbfParams.DistanceMethod;
            var function = rbfParams.Function;

            return (a, b) =>
            {
                float distance = TtRBFMath.GetDistanceBetweenEntries(a, b, distanceMethod, in twistAxis);
                return TtRBFMath.GetWeightedValue(distance, kernelWidth, function);
            };
        }

        /// <summary>
        /// 校验 target 配置是否可用于求解。主要是剔除会让 Interpolative 求解失效的重复
        /// target。全部合法时返回 true, 否则 outInvalidTargets 给出非法 target 的下标。
        /// 对标 UE FRBFSolver::ValidateTargets。
        /// </summary>
        public static bool ValidateTargets(TtRBFParams rbfParams, IReadOnlyList<TtRBFTarget> targets,
            List<int> outInvalidTargets)
        {
            outInvalidTargets.Clear();

            // Additive 不在意重复 target
            if (rbfParams.SolverType != ERBFSolverType.Interpolative)
                return true;

            var entries = new List<TtRBFEntry>(targets.Count);
            for (int i = 0; i < targets.Count; ++i)
                entries.Add(targets[i]);

            var invalidPairs = new List<(int, int)>();
            if (TtRBFInterpolator.GetIdenticalNodePairs(entries, MakeInterpolativeWeightFunc(rbfParams, entries), invalidPairs))
            {
                // 标记每对中的后一个为非法(遍历覆盖了所有可能的配对, 足以全部抓到)
                var set = new HashSet<int>();
                for (int i = 0; i < invalidPairs.Count; ++i)
                    set.Add(invalidPairs[i].Item2);

                foreach (var index in set)
                    outInvalidTargets.Add(index);
                outInvalidTargets.Sort();
            }

            return outInvalidTargets.Count == 0;
        }

        /// <summary>
        /// 初始化求解数据。Additive 无需预计算; Interpolative 会在此构建并求逆核矩阵。
        /// 对标 UE FRBFSolver::InitSolver。
        /// </summary>
        public static TtRBFSolverData InitSolver(TtRBFParams rbfParams, IReadOnlyList<TtRBFTarget> targets)
        {
            var data = new TtRBFSolverData();
            data.Params = rbfParams.Clone();

            if (rbfParams.SolverType == ERBFSolverType.Interpolative)
            {
                for (int i = 0; i < targets.Count; ++i)
                    data.EntryTargets.Add(targets[i]);

                data.Interpolator = new TtRBFInterpolator(data.EntryTargets,
                    MakeInterpolativeWeightFunc(rbfParams, data.EntryTargets));
            }
            else
            {
                // Additive 也记录一份 target 坐标, 供 IsSolverDataValid 判断是否需要重建
                for (int i = 0; i < targets.Count; ++i)
                {
                    var copy = new TtRBFEntry();
                    copy.CopyFrom(targets[i]);
                    data.EntryTargets.Add(copy);
                }
            }

            return data;
        }

        /// <summary>
        /// 判断已有求解数据是否仍与当前 target/参数匹配。不匹配时调用方需重新 InitSolver。
        /// 对标 UE FRBFSolver::IsSolverDataValid。
        /// </summary>
        public static bool IsSolverDataValid(TtRBFSolverData solverData, TtRBFParams rbfParams,
            IReadOnlyList<TtRBFTarget> targets)
        {
            if (solverData == null || solverData.Params == null)
                return false;
            if (solverData.EntryTargets.Count != targets.Count)
                return false;

            for (int i = 0; i < targets.Count; ++i)
            {
                var valuesA = targets[i].Values;
                var valuesB = solverData.EntryTargets[i].Values;
                if (valuesA.Count != valuesB.Count)
                    return false;

                for (int j = 0; j < valuesA.Count; ++j)
                {
                    if (valuesA[j] != valuesB[j])
                        return false;
                }
            }

            return solverData.Params.SolverType == rbfParams.SolverType
                && solverData.Params.Radius == rbfParams.Radius
                && solverData.Params.AutomaticRadius == rbfParams.AutomaticRadius
                && solverData.Params.Function == rbfParams.Function
                && solverData.Params.DistanceMethod == rbfParams.DistanceMethod
                && solverData.Params.TwistAxis == rbfParams.TwistAxis;
        }

        /// <summary>
        /// Additive 求解: 逐 target 累加贡献。
        /// 距离按该 target 的有效半径缩放后过衰减函数; 这里核宽度固定为 1, 改由半径缩放
        /// 距离(与 UE 一致, 且 Linear/Gaussian/Exponential 走 UE 的旧公式分支)。
        /// 对标 UE SolveAdditive。
        /// </summary>
        static void SolveAdditive(TtRBFParams rbfParams, IReadOnlyList<TtRBFTarget> targets,
            TtRBFEntry input, float[] allWeights)
        {
            for (int targetIdx = 0; targetIdx < targets.Count; ++targetIdx
                )
            {
                var target = targets[targetIdx];
                var functionType = (target.FunctionType == ERBFFunctionType.DefaultFunction)
                    ? rbfParams.Function
                    : target.FunctionType;

                float distance = TtRBFMath.FindDistanceBetweenEntries(target, input, rbfParams, target.DistanceMethod);
                float scaling = TtRBFMath.GetRadiusForTarget(target, rbfParams);
                float x = distance / scaling;

                float weight = TtRBFMath.GetWeightedValue(x, 1.0f, functionType, true);

                // 自定义曲线重映射
                if (target.ApplyCustomCurve && target.CustomCurveEvaluator != null)
                    weight = target.CustomCurveEvaluator(weight);

                // 先不做阈值过滤, 等归一化之后再筛
                allWeights[targetIdx] = weight;
            }
        }

        /// <summary>
        /// 求解: 给定一组 target 与输入, 输出激活的 target 及其权重。
        /// 对标 UE FRBFSolver::Solve。
        /// </summary>
        public static void Solve(TtRBFSolverData solverData, TtRBFParams rbfParams,
            IReadOnlyList<TtRBFTarget> targets, TtRBFEntry input, List<TtRBFOutputWeight> outputWeights)
        {
            outputWeights.Clear();

            if (solverData == null || targets == null || targets.Count == 0)
                return;
            if (rbfParams.TargetDimensions != input.Dimensions)
            {
                System.Diagnostics.Debug.Assert(false, "RBF: 输入维度与 TargetDimensions 不一致");
                return;
            }

            var allWeights = new float[targets.Count];

            switch (solverData.Params.SolverType)
            {
                default:
                case ERBFSolverType.Additive:
                    SolveAdditive(rbfParams, targets, input, allWeights);
                    break;

                case ERBFSolverType.Interpolative:
                    {
                        if (solverData.Interpolator == null)
                            return;

                        var interpWeights = new List<float>(targets.Count);
                        solverData.Interpolator.Interpolate(interpWeights, input);

                        int count = Math.Min(interpWeights.Count, targets.Count);
                        for (int i = 0; i < count; ++i)
                        {
                            // Interpolative 下 ScaleFactor 直接缩放权重(而非缩放半径)
                            allWeights[i] = interpWeights[i] * targets[i].ScaleFactor;
                        }
                    }
                    break;
            }

            float totalWeight = 0.0f;
            for (int i = 0; i < allWeights.Length; ++i)
                totalWeight += allWeights[i];

            // 完全没有权重时不输出任何 target
            if (totalWeight <= TtRBFMath.KindaSmallNumber)
                return;

            float weightScale = 1.0f;
            if (totalWeight > 1.0f)
            {
                // 权重和超过 1 时一律归一化, 与 NormalizeMethod 无关
                weightScale = 1.0f / totalWeight;
            }
            else
            {
                switch (rbfParams.NormalizeMethod)
                {
                    case ERBFNormalizeMethod.OnlyNormalizeAboveOne:
                    case ERBFNormalizeMethod.NoNormalization:
                        // 保持 1.0, 不归一化
                        break;

                    case ERBFNormalizeMethod.AlwaysNormalize:
                        weightScale = 1.0f / totalWeight;
                        break;

                    case ERBFNormalizeMethod.NormalizeWithinMedian:
                        {
                            if (rbfParams.MedianMax < rbfParams.MedianMin)
                                break;

                            // 用 MedianReference 重复填充成与输入同维度的条目
                            var medianEntry = new TtRBFEntry();
                            var medianRef = rbfParams.MedianReference;
                            while (input.Dimensions > medianEntry.Dimensions)
                                medianEntry.AddFromVector(in medianRef);

                            float medianDistance = TtRBFMath.FindDistanceBetweenEntries(input, medianEntry, rbfParams);
                            if (medianDistance > rbfParams.MedianMax)
                                break;
                            if (medianDistance <= rbfParams.MedianMin)
                            {
                                weightScale = 1.0f / totalWeight;
                                break;
                            }

                            float bias = Math.Clamp(
                                (medianDistance - rbfParams.MedianMin) / (rbfParams.MedianMax - rbfParams.MedianMin),
                                0.0f, 1.0f);
                            float normalized = 1.0f / totalWeight;
                            weightScale = normalized + (1.0f - normalized) * bias;
                        }
                        break;
                }
            }

            for (int targetIdx = 0; targetIdx < targets.Count; ++targetIdx)
            {
                float normalizedWeight = allWeights[targetIdx] * weightScale;
                if (normalizedWeight > rbfParams.WeightThreshold)
                    outputWeights.Add(new TtRBFOutputWeight(targetIdx, normalizedWeight));
            }
        }

        /// <summary>
        /// 求每个 target 到最近邻 target 的距离(度)。可用于编辑器提示半径取值。
        /// target 少于两个时返回 false。
        /// 对标 UE FRBFSolver::FindTargetNeighbourDistances。
        /// </summary>
        public static bool FindTargetNeighbourDistances(TtRBFParams rbfParams,
            IReadOnlyList<TtRBFTarget> targets, List<float> outNeighbourDists)
        {
            outNeighbourDists.Clear();

            int numTargets = targets.Count;
            for (int i = 0; i < numTargets; ++i)
                outNeighbourDists.Add(0.0f);

            if (numTargets <= 1)
                return false;

            for (int targetIdx = 0; targetIdx < numTargets; ++targetIdx)
            {
                float nearestDist = float.MaxValue;
                for (int otherIdx = 0; otherIdx < numTargets; ++otherIdx)
                {
                    if (otherIdx == targetIdx)
                        continue;
                    float dist = TtRBFMath.FindDistanceBetweenEntries(targets[targetIdx], targets[otherIdx],
                        rbfParams, targets[targetIdx].DistanceMethod);
                    nearestDist = MathF.Min(dist, nearestDist);
                }

                // 所有 target 完全重合时避免零距离
                outNeighbourDists[targetIdx] = MathF.Max(nearestDist, TtRBFMath.KindaSmallNumber);
            }

            return true;
        }
    }
}

using EngineNS.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.RootMotion
{
    /// <summary>
    /// RootMotion增量运算的单测。
    /// 本引擎的四元数与FTransform乘法约定与UE不同(见TtRootMotionUtil注释),
    /// 这些断言就是把"Delta满足 ApplyDelta(Start, Delta) == End"这一约定钉死。
    /// 改动TtRootMotionUtil必须让本测试全绿。
    /// </summary>
    [UnitTest.TtTest]
    public class UTest_RootMotion
    {
        const float Tolerance = 1e-3f;

        static bool NearlyEqual(in DVector3 a, in DVector3 b)
        {
            return Math.Abs(a.X - b.X) < Tolerance && Math.Abs(a.Y - b.Y) < Tolerance && Math.Abs(a.Z - b.Z) < Tolerance;
        }
        static bool NearlyEqual(in Quaternion a, in Quaternion b)
        {
            // q与-q表示同一旋转, 比较时取绝对点积
            return Math.Abs(Math.Abs(Quaternion.Dot(a, b)) - 1.0f) < Tolerance;
        }
        static bool NearlyEqual(in FTransform a, in FTransform b)
        {
            return NearlyEqual(a.Position, b.Position) && NearlyEqual(a.Quat, b.Quat);
        }
        static FTransform MakeTransform(float x, float y, float z, float yawDegree)
        {
            var quat = Quaternion.FromEuler(new FRotator(yawDegree * MathHelper.Deg2Rad, 0.0f, 0.0f));
            return FTransform.CreateTransform(new DVector3(x, y, z), Vector3.One, quat);
        }

        public void UnitTestEntrance()
        {
            TestDeltaRoundTrip();
            TestSubStepComposition();
            TestPureTranslationInRotatedFrame();
            TestBlend();
            TestWeightedAccumulate();
        }

        /// <summary>
        /// ApplyDelta(start, CalcDelta(start, end)) 必须回到end
        /// </summary>
        void TestDeltaRoundTrip()
        {
            var cases = new List<FTransform>()
            {
                MakeTransform(0, 0, 0, 0),
                MakeTransform(1, 0, 0, 0),
                MakeTransform(0, 0, 2, 90),
                MakeTransform(-3, 1, 4, -45),
            };

            for (int i = 0; i < cases.Count; ++i)
            {
                for (int j = 0; j < cases.Count; ++j)
                {
                    var start = cases[i];
                    var end = cases[j];
                    var delta = TtRootMotionUtil.CalcDelta(in start, in end);
                    var applied = TtRootMotionUtil.ApplyDelta(in start, in delta);
                    TtUnitTestManager.TAssert(NearlyEqual(in applied, in end),
                        $"RootMotion delta round trip failed: case({i},{j}) applied={applied} end={end}");
                }
            }
        }

        /// <summary>
        /// 分步增量顺序复合后必须等于整步增量, 否则子步进(Section边界/循环回绕)会产生位移偏差
        /// </summary>
        void TestSubStepComposition()
        {
            var a = MakeTransform(0, 0, 0, 0);
            var b = MakeTransform(1, 0, 0, 30);
            var c = MakeTransform(2, 0, 1, 75);

            var d1 = TtRootMotionUtil.CalcDelta(in a, in b);
            var d2 = TtRootMotionUtil.CalcDelta(in b, in c);
            var combined = TtRootMotionUtil.Combine(in d1, in d2);
            var whole = TtRootMotionUtil.CalcDelta(in a, in c);
            TtUnitTestManager.TAssert(NearlyEqual(in combined, in whole),
                $"RootMotion substep composition failed: combined={combined} whole={whole}");

            // 逐步施加与一次施加结果一致
            var stepwise = TtRootMotionUtil.ApplyDelta(TtRootMotionUtil.ApplyDelta(in a, in d1), in d2);
            TtUnitTestManager.TAssert(NearlyEqual(in stepwise, in c),
                $"RootMotion stepwise apply failed: stepwise={stepwise} expected={c}");
        }

        /// <summary>
        /// 增量表达在start的局部坐标系: start朝向旋转90度时, 世界+X的位移对应局部的另一个轴
        /// </summary>
        void TestPureTranslationInRotatedFrame()
        {
            var start = MakeTransform(0, 0, 0, 90);
            var end = FTransform.CreateTransform(new DVector3(1, 0, 0), Vector3.One, start.Quat);
            var delta = TtRootMotionUtil.CalcDelta(in start, in end);

            TtUnitTestManager.TAssert(NearlyEqual(delta.Quat, Quaternion.Identity),
                $"RootMotion pure translation should not produce rotation: {delta.Quat}");
            TtUnitTestManager.TAssert(Math.Abs(delta.Position.Length() - 1.0) < Tolerance,
                $"RootMotion pure translation length should be preserved: {delta.Position}");

            // 把局部增量转回世界必须还原出世界位移
            var world = TtRootMotionUtil.ConvertDeltaToWorldTranslation(in delta, start.Quat);
            TtUnitTestManager.TAssert(NearlyEqual(world, new DVector3(1, 0, 0)),
                $"RootMotion local->world translation failed: {world}");
        }

        void TestBlend()
        {
            var delta = TtRootMotionUtil.CalcDelta(MakeTransform(0, 0, 0, 0), MakeTransform(1, 0, 0, 0));
            var data = FRootMotionData.FromDelta(delta);

            var blendedFull = TtRootMotionUtil.Blend(FRootMotionData.Empty, data, 1.0f);
            TtUnitTestManager.TAssert(blendedFull.HasRootMotion && NearlyEqual(blendedFull.Delta, data.Delta),
                "RootMotion blend with alpha=1 should take the second source");

            var blendedNone = TtRootMotionUtil.Blend(FRootMotionData.Empty, data, 0.0f);
            TtUnitTestManager.TAssert(blendedNone.Delta.Position.Length() < Tolerance,
                "RootMotion blend with alpha=0 should fall back to identity");

            var blendedHalf = TtRootMotionUtil.Blend(FRootMotionData.Empty, data, 0.5f);
            TtUnitTestManager.TAssert(Math.Abs(blendedHalf.Delta.Position.X - 0.5) < Tolerance,
                $"RootMotion blend with alpha=0.5 should halve the translation: {blendedHalf.Delta.Position}");

            var empty = TtRootMotionUtil.Blend(FRootMotionData.Empty, FRootMotionData.Empty, 0.5f);
            TtUnitTestManager.TAssert(!empty.HasRootMotion, "Blending two empty root motions must stay empty");
        }

        void TestWeightedAccumulate()
        {
            var delta = TtRootMotionUtil.CalcDelta(MakeTransform(0, 0, 0, 0), MakeTransform(2, 0, 0, 0));
            var source = FRootMotionData.FromDelta(delta);

            var accumulator = FRootMotionData.Empty;
            bool hasRefQuat = false;
            var refQuat = Quaternion.Identity;
            TtRootMotionUtil.AccumulateWeighted(ref accumulator, ref hasRefQuat, ref refQuat, source, 0.5f);
            TtRootMotionUtil.AccumulateWeighted(ref accumulator, ref hasRefQuat, ref refQuat, source, 0.5f);
            TtRootMotionUtil.FinishAccumulate(ref accumulator);

            TtUnitTestManager.TAssert(accumulator.HasRootMotion, "Weighted accumulate should produce root motion");
            TtUnitTestManager.TAssert(NearlyEqual(accumulator.Delta, delta),
                $"Two half-weighted identical sources should equal the source: {accumulator.Delta} vs {delta}");

            // 权重为0的来源不参与累加
            var zeroWeighted = FRootMotionData.Empty;
            bool zeroHasRef = false;
            var zeroRefQuat = Quaternion.Identity;
            TtRootMotionUtil.AccumulateWeighted(ref zeroWeighted, ref zeroHasRef, ref zeroRefQuat, source, 0.0f);
            TtRootMotionUtil.FinishAccumulate(ref zeroWeighted);
            TtUnitTestManager.TAssert(!zeroWeighted.HasRootMotion, "Zero weighted source must not contribute");
        }
    }
}

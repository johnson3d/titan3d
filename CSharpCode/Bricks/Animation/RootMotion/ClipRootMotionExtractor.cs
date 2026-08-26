using EngineNS.Animation.Animatable;
using EngineNS.Animation.Asset;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.RootMotion
{
    /// <summary>
    /// 从AnimationClip中提取根骨骼的位移。
    /// 单独克隆一份可动画Pose只驱动根骨骼曲线, 走的是与整体Pose采样完全相同的
    /// TtBindedCurveUtil路径, 保证欧拉角->四元数的转换语义一致(引擎的旋转曲线值是欧拉角)。
    /// </summary>
    public class TtClipRootMotionExtractor
    {
        TtAnimatableSkeletonPose mSamplePose = null;
        TtCurveBindedObject mRootBindedCurves = null;
        IAnimatableLimbPose mRootLimbPose = null;
        FTransform mFirstFrameTransform = FTransform.Identity;

        /// <summary>
        /// 动画中是否真的有根骨骼曲线, 没有则本提取器不产出任何位移
        /// </summary>
        public bool IsValid { get => mRootBindedCurves != null && mRootLimbPose != null; }
        /// <summary>
        /// 动画第一帧的根骨骼变换, 供ERootMotionRootLock.AnimFirstFrame使用
        /// </summary>
        public FTransform FirstFrameTransform { get => mFirstFrameTransform; }

        public bool Initialize(TtAnimationClip clip, TtAnimatableSkeletonPose bindingPose)
        {
            mSamplePose = null;
            mRootBindedCurves = null;
            mRootLimbPose = null;
            mFirstFrameTransform = FTransform.Identity;

            if (clip == null || clip.AnimationChunk == null || bindingPose == null)
                return false;

            mSamplePose = bindingPose.Clone() as TtAnimatableSkeletonPose;
            if (mSamplePose == null || mSamplePose.Root == null)
                return false;

            mRootLimbPose = mSamplePose.Root;
            var bindedObjects = TtBindedCurveUtil.BindingCurves(clip, mSamplePose);
            foreach (var bindedObject in bindedObjects)
            {
                if (bindedObject.AnimatableObject == mRootLimbPose)
                {
                    mRootBindedCurves = bindedObject;
                    break;
                }
            }
            if (mRootBindedCurves == null)
                return false;

            mFirstFrameTransform = Sample(0.0f);
            return true;
        }

        /// <summary>
        /// 采样指定时刻的根骨骼变换(相对Actor空间, 即根骨骼的父空间)
        /// </summary>
        public FTransform Sample(float time)
        {
            if (!IsValid)
                return FTransform.Identity;

            mRootBindedCurves.Evaluate(time);
            return mRootLimbPose.Transtorm;
        }

        /// <summary>
        /// 提取[prevTime, curTime]区间的位移增量。isLoop且发生回绕时拆成
        /// [prevTime, duration] 与 [0, curTime] 两段顺序复合, 避免回绕产生一个反向大跳。
        /// </summary>
        public FRootMotionData Extract(float prevTime, float curTime, bool isLoop, float duration, bool fromMontage = false)
        {
            if (!IsValid)
                return FRootMotionData.Empty;

            if (curTime >= prevTime)
            {
                if (curTime == prevTime)
                    return FRootMotionData.Empty;

                var start = Sample(prevTime);
                var end = Sample(curTime);
                return FRootMotionData.FromDelta(TtRootMotionUtil.CalcDelta(in start, in end), fromMontage);
            }

            if (!isLoop || duration <= 0.0f)
            {
                // 非循环却出现时间倒退, 视为跳转, 不产生位移
                return FRootMotionData.Empty;
            }

            var tailStart = Sample(prevTime);
            var tailEnd = Sample(duration);
            var tailDelta = TtRootMotionUtil.CalcDelta(in tailStart, in tailEnd);

            var headStart = Sample(0.0f);
            var headEnd = Sample(curTime);
            var headDelta = TtRootMotionUtil.CalcDelta(in headStart, in headEnd);

            return FRootMotionData.FromDelta(TtRootMotionUtil.Combine(in tailDelta, in headDelta), fromMontage);
        }
    }
}

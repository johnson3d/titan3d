using EngineNS.Animation.Animatable;
using EngineNS.Animation.Asset;
using EngineNS.Animation.RootMotion;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Sampler
{
    /// <summary>
    /// 单个AnimationClip的采样器: 曲线绑定 + 任意时刻求值 + RootMotion提取 + 根锁定。
    /// BlendTree的Clip节点、简单播放器、Montage的段落共用这一份实现,
    /// 保证三条路径的采样语义(含RootMotion)完全一致。
    /// </summary>
    public class TtClipPoseSampler
    {
        TtAnimationClip mClip = null;
        TtAnimatableSkeletonPose mSamplePose = null;
        List<TtCurveBindedObject> mBindedCurves = new List<TtCurveBindedObject>();
        TtClipRootMotionExtractor mRootMotionExtractor = null;

        public TtAnimationClip Clip { get => mClip; }
        public float Duration { get => mClip != null ? mClip.Duration : 0.0f; }
        public bool IsValid { get => mClip != null && mSamplePose != null; }
        /// <summary>
        /// 资产开启了RootMotion且动画中确实存在根骨骼曲线
        /// </summary>
        public bool HasRootMotion { get => mRootMotionExtractor != null && mRootMotionExtractor.IsValid; }

        public bool Initialize(TtAnimationClip clip, TtAnimatableSkeletonPose bindingPose)
        {
            mClip = clip;
            mBindedCurves.Clear();
            mSamplePose = null;
            mRootMotionExtractor = null;

            if (clip == null || bindingPose == null)
                return false;

            mSamplePose = bindingPose.Clone() as TtAnimatableSkeletonPose;
            mBindedCurves = TtBindedCurveUtil.BindingCurves(clip, mSamplePose);

            if (clip.EnableRootMotion)
            {
                var extractor = new TtClipRootMotionExtractor();
                if (extractor.Initialize(clip, bindingPose))
                    mRootMotionExtractor = extractor;
            }
            return true;
        }

        /// <summary>
        /// 创建一个与本采样器骨骼结构匹配的输出Pose
        /// </summary>
        public TtLocalSpaceRuntimePose CreateOutPose()
        {
            return TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(mSamplePose);
        }

        /// <summary>
        /// 采样到指定时刻并写入outPose(不含RootMotion, RootMotion由ExtractRootMotion单独取)
        /// </summary>
        public void Sample(float time, ref TtLocalSpaceRuntimePose outPose)
        {
            if (!IsValid)
                return;

            for (int i = 0; i < mBindedCurves.Count; ++i)
            {
                mBindedCurves[i].Evaluate(time);
            }
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref outPose, mSamplePose);
        }

        /// <summary>
        /// 提取[prevTime, curTime]的RootMotion。未开启RootMotion时返回Empty。
        /// </summary>
        public FRootMotionData ExtractRootMotion(float prevTime, float curTime, bool isLoop, bool fromMontage = false)
        {
            if (!HasRootMotion)
                return FRootMotionData.Empty;

            return mRootMotionExtractor.Extract(prevTime, curTime, isLoop, Duration, fromMontage);
        }

        /// <summary>
        /// 按资产设置锁定输出Pose的根骨骼。rootMotionConsumed为false且未设ForceRootLock时不做任何事。
        /// </summary>
        public void ApplyRootLock(ref TtLocalSpaceRuntimePose outPose, bool rootMotionConsumed)
        {
            if (mClip == null || outPose == null)
                return;
            if (!rootMotionConsumed && !mClip.ForceRootLock)
                return;

            var firstFrame = mRootMotionExtractor != null ? mRootMotionExtractor.FirstFrameTransform : FTransform.Identity;
            TtRuntimePoseUtility.ApplyRootLock(ref outPose, mClip.RootMotionRootLock, in firstFrame);
        }

        /// <summary>
        /// 采样并按RootMotion模式一次性完成: 求值Pose、提取位移、锁定根骨骼
        /// </summary>
        public void SampleWithRootMotion(float prevTime, float curTime, bool isLoop, ERootMotionMode mode, bool fromMontage, ref TtLocalSpaceRuntimePose outPose)
        {
            Sample(curTime, ref outPose);
            if (outPose == null)
                return;

            bool extract = HasRootMotion && IsRootMotionEnabled(mode, fromMontage);
            var rootMotion = extract ? ExtractRootMotion(prevTime, curTime, isLoop, fromMontage) : FRootMotionData.Empty;
            outPose.RootMotion = rootMotion;
            ApplyRootLock(ref outPose, extract);
        }

        /// <summary>
        /// 当前RootMotion模式下该来源是否参与位移贡献
        /// </summary>
        public static bool IsRootMotionEnabled(ERootMotionMode mode, bool fromMontage)
        {
            switch (mode)
            {
                case ERootMotionMode.FromEverything:
                    return true;
                case ERootMotionMode.FromMontagesOnly:
                    return fromMontage;
                default:
                    return false;
            }
        }
    }
}

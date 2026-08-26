using EngineNS.Animation.Animatable;
using EngineNS.Animation.RootMotion;
using EngineNS.Animation.Sampler;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Command
{
    public class TtExtractPoseFromClipCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        /// <summary>
        /// 上一帧的采样时刻, RootMotion需要用[PrevTime, Time]区间求增量
        /// </summary>
        public float PrevTime { get; set; } = 0;
        public float Time { get; set; } = 0;
        public bool IsLoop { get; set; } = true;
        public ERootMotionMode RootMotionMode { get; set; } = ERootMotionMode.Ignore;
        Asset.TtAnimationClip AnimationClip = null;
        TtClipPoseSampler mSampler = null;

        /// <summary>
        /// 资产开启了RootMotion且动画里确实有根骨骼曲线
        /// </summary>
        public bool HasRootMotion { get => mSampler != null && mSampler.HasRootMotion; }

        public TtExtractPoseFromClipCommand(in Asset.TtAnimationClip skeletonAnimClip)
        {
            AnimationClip = skeletonAnimClip;
        }

        public TtExtractPoseFromClipCommand(ref TtAnimatableSkeletonPose bindeddPose, in Asset.TtAnimationClip skeletonAnimClip)
        {
            AnimationClip = skeletonAnimClip;
            SetExtractedPose(ref bindeddPose);
        }

        public TtExtractPoseFromClipCommand()
        {
        }

        public void SetExtractedPose(ref TtAnimatableSkeletonPose extractedPose)
        {
            mSampler = null;
            if (extractedPose == null)
                return;

            var sampler = new TtClipPoseSampler();
            if (!sampler.Initialize(AnimationClip, extractedPose))
                return;

            mSampler = sampler;
            mOutPose = sampler.CreateOutPose();
        }

        public override void Execute()
        {
            if (mSampler == null || !mSampler.IsValid)
                return;
            mSampler.SampleWithRootMotion(PrevTime, Time, IsLoop, RootMotionMode, false, ref mOutPose);
        }
    }
}

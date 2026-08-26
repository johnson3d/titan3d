using EngineNS.Animation.Animatable;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Command;
using EngineNS.Animation.Player;
using EngineNS.Animation.RootMotion;
using EngineNS.Animation.Sampler;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtAnimationClipCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        public TtAnimationClip AnimationClip { get; set; } = null;
        public TtAnimationClipCommandDesc Desc { get; set; }
        TtClipPoseSampler mSampler = null;
        /// <summary>
        /// 资产开启了RootMotion且动画里确实有根骨骼曲线
        /// </summary>
        public bool HasRootMotion { get => mSampler != null && mSampler.HasRootMotion; }
        public override void Execute()
        {
            if (mSampler == null || !mSampler.IsValid)
                return;
            mSampler.SampleWithRootMotion(Desc.PrevTime, Desc.Time, Desc.IsLoop, Desc.RootMotionMode, false, ref mOutPose);
        }
        public void SetExtractedPose(TtAnimatableSkeletonPose extractedPose)
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
    }
    public class TtAnimationClipCommandDesc : IAnimationCommandDesc
    {
        /// <summary>
        /// 上一帧的采样时刻, RootMotion需要用[PrevTime, Time]区间求增量
        /// </summary>
        public float PrevTime { get; set; } = 0;
        public float Time { get; set; } = 0;
        public bool IsLoop { get; set; } = false;
        public ERootMotionMode RootMotionMode { get; set; } = ERootMotionMode.Ignore;
    }
    public class TtBlendTree_AnimationClip<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        TtAnimationClip mClip = null;

        public TtAnimationClip Clip
        {
            get =>mClip;
            set
            {
                mClip = value;
            }
        }
        public bool IsLoop { get; set; } = false;
        public float Time { get; set; }
        //public ClipWarpMode WarpMode { get; set; } = ClipWarpMode.Loop;
        TtAnimationClipCommand<S> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new();
            mAnimationCommand.Desc = new();
            mAnimationCommand.Desc.IsLoop = IsLoop;
            mAnimationCommand.Desc.RootMotionMode = context.RootMotionMode;
            mAnimationCommand.AnimationClip = mClip;
            mAnimationCommand.SetExtractedPose(context.AnimatableSkeletonPose);
            await base.Initialize(context);
            return true;
        }
        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            base.ConstructAnimationCommandTree(parentNode, ref context);
            context.AddCommand(context.TreeDepth, mAnimationCommand);
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            var lastTime = mAnimationCommand.Desc.Time;
            var currentTime = lastTime + elapseSecond;
            mAnimationCommand.Desc.PrevTime = lastTime;
            mAnimationCommand.Desc.RootMotionMode = context.RootMotionMode;
            if(IsLoop)
            {
                mAnimationCommand.Desc.Time = currentTime % mAnimationCommand.AnimationClip.Duration;
            }
            else
            {
                if (currentTime >= mAnimationCommand.AnimationClip.Duration)
                {
                    mAnimationCommand.Desc.Time = mAnimationCommand.AnimationClip.Duration;
                }
                else
                {
                    mAnimationCommand.Desc.Time = currentTime;
                }
            }
        }
    }
}

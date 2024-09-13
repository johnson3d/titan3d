using EngineNS.Animation.Animatable;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Command;
using EngineNS.Animation.Player;
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
        TtAnimatableSkeletonPose mExtractedPose = null;
        public override void Execute()
        {
            if (mExtractedPose == null)
                return;
            CurveEvaluate(Desc.Time);
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose , mExtractedPose);
        }
        List<TtCurveBindedObject> BindedCurves = new List<TtCurveBindedObject>();
        public void SetExtractedPose(TtAnimatableSkeletonPose extractedPose)
        {
            BindedCurves.Clear();
            mExtractedPose = extractedPose.Clone() as TtAnimatableSkeletonPose;
            BindedCurves = TtBindedCurveUtil.BindingCurves(AnimationClip, mExtractedPose);
            mOutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(extractedPose);
        }

        void CurveEvaluate(float time)
        {
            foreach (var curve in BindedCurves)
            {
                curve.Evaluate(time);
            }
        }
    }
    public class TtAnimationClipCommandDesc : IAnimationCommandDesc
    {
        public float Time { get; set; }
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
        public float Time { get; set; }
        //public ClipWarpMode WarpMode { get; set; } = ClipWarpMode.Loop;
        TtAnimationClipCommand<S> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new();
            mAnimationCommand.Desc = new();
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
            mAnimationCommand.Desc.Time = currentTime % mAnimationCommand.AnimationClip.Duration;
        }
    }
}

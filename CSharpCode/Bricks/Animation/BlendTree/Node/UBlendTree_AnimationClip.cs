using EngineNS.Animation.Animatable;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Command;
using EngineNS.Animation.Player;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UAnimationClipCommand : UAnimationCommand<ULocalSpaceRuntimePose>
    {
        public UAnimationClip AnimationClip { get; set; } = null;
        public UAnimationClipCommandDesc Desc { get; set; }
        UAnimatableSkeletonPose ExtractedPose = null;
        public UAnimationPropertiesSetter AnimationPropertiesSetter;
        public override void Execute()
        {
            if (AnimationPropertiesSetter == null && ExtractedPose != null)
                return;
            AnimationPropertiesSetter.Evaluate(Desc.Time);
            URuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose ,ExtractedPose);
        }
    }
    public class UAnimationClipCommandDesc : IAnimationCommandDesc
    {
        public float Time { get; set; }
    }
    public class UBlendTree_AnimationClip : UBlendTree<ULocalSpaceRuntimePose>
    {
        UAnimationClip mClip = null;
        public UAnimationClip Clip
        {
            get =>mClip;
            set
            {
                mClip = value;
            }
        }
        public float Time { get; set; }
        //public ClipWarpMode WarpMode { get; set; } = ClipWarpMode.Loop;
        UAnimationClipCommand mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new UAnimationClipCommand();
            base.Initialize();
        }
        public override UAnimationCommand<ULocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var desc = new UAnimationClipCommandDesc();
            mAnimationCommand.Desc = desc;
            mAnimationCommand.AnimationClip = mClip;
            context.AddCommand(context.TreeDepth, mAnimationCommand);
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond)
        {
            mAnimationCommand.Desc.Time = Time;
        }
    }
}

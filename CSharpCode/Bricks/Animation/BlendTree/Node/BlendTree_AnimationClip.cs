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
    public class TtAnimationClipCommand : TtAnimationCommand<TtLocalSpaceRuntimePose>
    {
        public TtAnimationClip AnimationClip { get; set; } = null;
        public TtAnimationClipCommandDesc Desc { get; set; }
        TtAnimatableSkeletonPose ExtractedPose = null;
        public override void Execute()
        {
            if (ExtractedPose == null)
                return;
            System.Diagnostics.Debug.Assert(false);
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose ,ExtractedPose);
        }
    }
    public class TtAnimationClipCommandDesc : IAnimationCommandDesc
    {
        public float Time { get; set; }
    }
    public class TtBlendTree_AnimationClip : TtBlendTree<TtLocalSpaceRuntimePose>
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
        TtAnimationClipCommand mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new TtAnimationClipCommand();
            base.Initialize();
        }
        public override TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var desc = new TtAnimationClipCommandDesc();
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

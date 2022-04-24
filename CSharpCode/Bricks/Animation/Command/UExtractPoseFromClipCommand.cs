using EngineNS.Animation.Animatable;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Command
{
    public class UExtractPoseFromClipCommand : UAnimationCommand<ULocalSpaceRuntimePose>
    {
        public UAnimationPropertiesSetter AnimationPropertiesSetter;
        public float Time;
        UAnimatableSkeletonPose ExtractedPose = null;
        Asset.UAnimationClip AnimationClip = null;

        public UExtractPoseFromClipCommand(in Asset.UAnimationClip skeletonAnimClip)
        {
            AnimationClip = skeletonAnimClip;
        }

        public UExtractPoseFromClipCommand(ref UAnimatableSkeletonPose extractedPose, in Asset.UAnimationClip skeletonAnimClip)
        {
            AnimationClip = skeletonAnimClip;
            SetExtractedPose(ref extractedPose);
        }

        public UExtractPoseFromClipCommand()
        {
        }

        public void SetExtractedPose(ref UAnimatableSkeletonPose extractedPose)
        {
            ExtractedPose = extractedPose;
            AnimationPropertiesSetter = UAnimationPropertiesSetter.Binding(AnimationClip, ExtractedPose);
            mOutPose = URuntimePoseUtility.CreateLocalSpaceRuntimePose(extractedPose);
        }


        public override void Execute()
        {
            if (AnimationPropertiesSetter == null && ExtractedPose != null)
                return;
            AnimationPropertiesSetter.Evaluate(Time);
            URuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose, ExtractedPose);
        }
    }
}

using EngineNS.Animation.Animatable;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Command
{
    public class TtExtractPoseFromClipCommand : TtAnimationCommand<TtLocalSpaceRuntimePose>
    {
        public float Time { get; set; } = 0;
        TtAnimatableSkeletonPose ExtractedPose = null;
        Asset.TtAnimationClip AnimationClip = null;
        TtLocalSpaceRuntimePose BindedLocalSpaceRuntimePose = null;

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
        List<TtCurveBindedObject> BindedCurves = new List<TtCurveBindedObject>();
        public void SetExtractedPose(ref TtAnimatableSkeletonPose extractedPose)
        {
            BindedCurves.Clear();
            ExtractedPose = extractedPose;
            BindedCurves = TtBindedCurveUtil.BindingCurves(AnimationClip, ExtractedPose);
            
            //AnimationPropertiesSetter = TtAnimationPropertiesSetter.Binding(AnimationClip, ExtractedPose);
            BindedLocalSpaceRuntimePose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(extractedPose);
            mOutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(extractedPose);
        }

        void CurveEvaluate(float time)
        {
            foreach(var curve in BindedCurves)
            {
                curve.Evaluate(time);
            }
        }


        public override void Execute()
        {
            if (ExtractedPose == null)
                return;
            CurveEvaluate(Time);
            //var extractedPose = URuntimePoseUtility.CreateLocalSpaceRuntimePose(ExtractedPose);
            //URuntimePoseUtility.AddPoses(ref mOutPose, extractedPose, BindedLocalSpaceRuntimePose);
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose, ExtractedPose);
        }
    }
}

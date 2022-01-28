using EngineNS.Animation.Animatable;
using EngineNS.Animation.Pipeline;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Player
{
    public class USkeletonAnimationPlayer : IAnimationPlayer
    {
        public float Time { get; set; } = 0;
        public float Duration { get => SkeletonAnimClip.Duration; }

        public Asset.UAnimationClip SkeletonAnimClip { get; protected set; } = null;
        protected UAnimationPropertiesSetter AnimationPropertiesSetter = null;
        public UAnimatableSkeletonPose BindedPose { get; protected set; } = null;
        public UMeshSpaceRuntimePose RuntimePose = null;
        public ULocalSpaceRuntimePose LocalPose = null;
        public USkeletonAnimationPlayer(Asset.UAnimationClip skeletonAnimClip)
        {
            System.Diagnostics.Debug.Assert(skeletonAnimClip != null);
            SkeletonAnimClip = skeletonAnimClip;
        }
        public void BindingPose(UAnimatableSkeletonPose bindedPose)
        {
            System.Diagnostics.Debug.Assert(bindedPose != null);
            AnimationPropertiesSetter = UAnimationPropertiesSetter.Binding(SkeletonAnimClip, bindedPose);
            BindedPose = bindedPose;
            LocalPose = URuntimePoseUtility.CreateLocalSpaceRuntimePose(BindedPose);
        }
        public void Update(float elapse)
        {
            System.Diagnostics.Debug.Assert(SkeletonAnimClip.Duration != 0.0f);

            Time += elapse;
            Time %= SkeletonAnimClip.Duration;
        }

        public void Evaluate()
        {
            //make command
            UPropertiesSettingCommand command = new UPropertiesSettingCommand()
            { 
                AnimationPropertiesSetter = AnimationPropertiesSetter,
                Time = Time
            };
            //if(IsImmediate)
            command.Execute();
            URuntimePoseUtility.CopyPose(ref LocalPose, BindedPose);
            URuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref RuntimePose ,LocalPose);
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();
        }
    }
}

using EngineNS.Animation.Animatable;
using EngineNS.Animation.Command;
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

        public ULocalSpaceRuntimePose OutPose
        {
            get 
            {
                if (mAnimEvaluateCommand == null)
                    return null;

                return mAnimEvaluateCommand.OutPose;
            }
        }
        protected UAnimationPropertiesSetter AnimationPropertiesSetter = null;
        public USkeletonAnimationPlayer(Asset.UAnimationClip skeletonAnimClip)
        {
            System.Diagnostics.Debug.Assert(skeletonAnimClip != null);
            SkeletonAnimClip = skeletonAnimClip;
        }
        public void BindingPose(UAnimatableSkeletonPose bindedPose)
        {
            System.Diagnostics.Debug.Assert(bindedPose != null);
            mAnimEvaluateCommand = new UExtractPoseFromClipCommand(ref bindedPose, SkeletonAnimClip);
        }

        UExtractPoseFromClipCommand mAnimEvaluateCommand = null;
        public void Update(float elapse)
        {
            System.Diagnostics.Debug.Assert(SkeletonAnimClip.Duration != 0.0f);
            float beforeTime = Time;
            Time += elapse;
            Time %= SkeletonAnimClip.Duration;

            //make command
            mAnimEvaluateCommand.Time = Time;

            foreach(var notify in SkeletonAnimClip.Notifies)
            {
                var before = (Int64)(beforeTime * 1000);
                var after = (Int64)(Time * 1000);
                //ms
                if (notify.CanTrigger(before, after))
                {
                    notify.Trigger(before, after);
                }
            }
        }

        public void Evaluate()
        {
            //make command
            //if(IsImmediate)
            mAnimEvaluateCommand.Execute();
           
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();
        }
    }
}

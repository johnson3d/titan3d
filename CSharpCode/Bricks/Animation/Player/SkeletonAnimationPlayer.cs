using EngineNS.Animation.Animatable;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Player
{
    public class TtSkeletonAnimationPlayer : IAnimationPlayer
    {
        public float Time { get; set; } = 0;
        public float Duration { get => SkeletonAnimClip.Duration; }

        public Asset.TtAnimationClip SkeletonAnimClip { get; protected set; } = null;

        public TtLocalSpaceRuntimePose OutPose
        {
            get 
            {
                if (mAnimEvaluateCommand == null)
                    return null;

                return mAnimEvaluateCommand.OutPose;
            }
        }
        //protected TtAnimationPropertiesSetter AnimationPropertiesSetter = null;
        public TtSkeletonAnimationPlayer(Asset.TtAnimationClip skeletonAnimClip)
        {
            System.Diagnostics.Debug.Assert(skeletonAnimClip != null);
            SkeletonAnimClip = skeletonAnimClip;
        }
        public void BindingPose(TtAnimatableSkeletonPose bindedPose)
        {
            System.Diagnostics.Debug.Assert(bindedPose != null);
            mAnimEvaluateCommand = new TtExtractPoseFromClipCommand<TtDefaultCenterData>(ref bindedPose, SkeletonAnimClip);
        }

        TtExtractPoseFromClipCommand<TtDefaultCenterData> mAnimEvaluateCommand = null;
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

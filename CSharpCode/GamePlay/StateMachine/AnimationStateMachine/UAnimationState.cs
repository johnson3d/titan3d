using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;

namespace EngineNS.GamePlay.StateMachine.AnimationStateMachine
{
    public class UAnimationState : UTimedState , IAnimationState
    {
        public Animation.Asset.UAnimationClip Animation { get; set; }
        public UExtractPoseFromClipCommand mExtractPoseFromClipCommand { get; set; } = null;
        IBlendTree<ULocalSpaceRuntimePose> BlendTreeRoot = null;

        public UAnimationState(UGamePlayStateMachine context, string name = "AnimationState") : base(context, name)
        {
        }

        public override void Initialize()
        {
            StateTimeDuration = Animation.Duration;
            mExtractPoseFromClipCommand = new UExtractPoseFromClipCommand(Animation);
            BlendTreeRoot = new UBlendTree_BindedPose();
        }

        public override void Update(float elapseSecond)
        {
            base.Update(elapseSecond);
            mExtractPoseFromClipCommand.Time = StateTime;
        }

        public UAnimationCommand<ULocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            context.AddCommand(context.TreeDepth, mExtractPoseFromClipCommand);
            context.TreeDepth++;
            return mExtractPoseFromClipCommand;
        }

        public void Evaluate()
        {
            mExtractPoseFromClipCommand.Execute();
        }
    }
}

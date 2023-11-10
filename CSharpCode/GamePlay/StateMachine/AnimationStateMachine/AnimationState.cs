using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using NPOI.SS.Formula.Functions;

namespace EngineNS.GamePlay.StateMachine.AnimationStateMachine
{
    public class TtAnimationState<T> : TtTimedState<T>, IAnimationState
    {
        public Animation.Asset.UAnimationClip Animation { get; set; }
        public UExtractPoseFromClipCommand mExtractPoseFromClipCommand { get; set; } = null;
        IBlendTree<ULocalSpaceRuntimePose> BlendTreeRoot = null;

        public TtAnimationState(TtGamePlayStateMachine<T> stateMachine, string name = "AnimationState") : base(stateMachine, name)
        {
        }

        public override bool Initialize()
        {
            StateTimeDuration = Animation.Duration;
            mExtractPoseFromClipCommand = new UExtractPoseFromClipCommand(Animation);
            BlendTreeRoot = new UBlendTree_BindedPose();
            return true;
        }

        public override void Update(float elapseSecond, in TtStateMachineContext context)
        {
            base.Update(elapseSecond, context);
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

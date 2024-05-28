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
        public Animation.Asset.TtAnimationClip Animation { get; set; }
        public TtExtractPoseFromClipCommand mExtractPoseFromClipCommand { get; set; } = null;
        IBlendTree<TtLocalSpaceRuntimePose> BlendTreeRoot = null;

        public TtAnimationState(TtGamePlayStateMachine<T> stateMachine, string name = "AnimationState") : base(stateMachine, name)
        {
        }

        public override bool Initialize()
        {
            Duration = Animation.Duration;
            mExtractPoseFromClipCommand = new TtExtractPoseFromClipCommand(Animation);
            BlendTreeRoot = new TtBlendTree_BindedPose();
            return true;
        }

        public override void Update(float elapseSecond, in TtStateMachineContext context)
        {
            base.Update(elapseSecond, context);
            mExtractPoseFromClipCommand.Time = StateTime;
        }

        public TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
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

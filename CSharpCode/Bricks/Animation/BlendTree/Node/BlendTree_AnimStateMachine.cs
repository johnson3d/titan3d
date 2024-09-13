using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtAnimStateMachineExtractPoseCommand<S, T> : TtAnimationCommand<S, T> where T : IRuntimePose
    {
        public TtAnimationCommand<S, T> AnimStateMachineCmd { get; set; }
        
        public override void Execute()
        {
            TtRuntimePoseUtility.CopyPose(ref mOutPose, AnimStateMachineCmd.OutPose);
        }
    }
    public class TtAnimStateMachineExtractPoseCommandDesc : IAnimationCommandDesc
    {

    }
    public class TtLocalSpaceBlendTree_AnimStateMachine<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        TtAnimStateMachineExtractPoseCommand<S, TtLocalSpaceRuntimePose> mAnimationCommand = null;
        public TtAnimStateMachine<S> AnimStateMachine { get; set; } = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtAnimStateMachineExtractPoseCommand<S, TtLocalSpaceRuntimePose>();
            mAnimationCommand.OutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(context.AnimatableSkeletonPose);
            return true;
        }
        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            System.Diagnostics.Debug.Assert(AnimStateMachine != null);
            if (AnimStateMachine == null)
                return null;

            context.AddCommand(context.TreeDepth, mAnimationCommand);
            context.TreeDepth++;
            mAnimationCommand.AnimStateMachineCmd = AnimStateMachine.BlendTree.ConstructAnimationCommandTree(parentNode, ref context); 

            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            System.Diagnostics.Debug.Assert(AnimStateMachine != null);
            TtAnimStateMachineContext stateMachineContext = new();
            stateMachineContext.BlendTreeContext = context;
            AnimStateMachine.Tick(elapseSecond, stateMachineContext);
        }
    }
}

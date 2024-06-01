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
    public class TtAnimStateMachineExtractPoseCommand<T> : TtAnimationCommand<T> where T : IRuntimePose
    {
        public TtAnimationCommand<T> AnimStateMachineCmd { get; set; }
        
        public override void Execute()
        {
            TtRuntimePoseUtility.CopyPose(ref mOutPose, AnimStateMachineCmd.OutPose);
        }
    }
    public class TtAnimStateMachineExtractPoseCommandDesc : IAnimationCommandDesc
    {

    }
    public class TtBlendTree_AnimStateMachine<S> : TtBlendTree<TtLocalSpaceRuntimePose>
    {
        TtBindedPoseCommand<TtLocalSpaceRuntimePose> mAnimationCommand = null;
        TtAnimStateMachine<S> AnimStateMachine;
        public override void Initialize()
        {
            mAnimationCommand = new TtBindedPoseCommand<TtLocalSpaceRuntimePose>();
        }
        public override TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var mAnimationCommand = new TtAnimStateMachineExtractPoseCommand<TtLocalSpaceRuntimePose>();
            context.AddCommand(context.TreeDepth, mAnimationCommand);
            context.TreeDepth++;
            mAnimationCommand.AnimStateMachineCmd = AnimStateMachine.BlendTree.ConstructAnimationCommandTree(parentNode, ref context); 

            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, in FAnimBlendTreeTickContext context)
        {
            
            AnimStateMachine.Tick(elapseSecond, context);
        }
    }
}

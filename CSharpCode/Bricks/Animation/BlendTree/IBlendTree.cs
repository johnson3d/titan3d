using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Thread.Async;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree
{
    public struct FConstructAnimationCommandTreeContext
    {
        public int TreeDepth;
        public TtAnimationCommandExecuteStack CmdExecuteStack;

        public void AddCommand(int depth, IAnimationCommand cmd)
        {
            CmdExecuteStack.AddCommand(depth, cmd);
        }
        public void Create()
        {
            TreeDepth = 0;
            CmdExecuteStack = new TtAnimationCommandExecuteStack();
        }
    }
    public struct FAnimBlendTreeContext
    {
        public TtAnimatableSkeletonPose AnimatableSkeletonPose;
    }

    public interface IBlendTree<S, T> where T : IRuntimePose
    {
        public S CenterData { get; set; }
        Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context);
        void Tick(float elapseSecond, ref FAnimBlendTreeContext context);
        //void Notifying();
        TtAnimationCommand<S, T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context);
    }

    public class TtBlendTree<S, T> : IBlendTree<S, T> where T : IRuntimePose
    {
        public S CenterData { get; set; }
        internal bool mIsInitialized = false;
        public virtual async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            return false;
        }

        public virtual void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {

        }

        public virtual TtAnimationCommand<S, T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            return null;
        }
    }

    public class TtLocalSpacePoseBlendTree<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {

    }
    public class TtFinalBlendTreeCommand<S, T> : TtAnimationCommand<S, T> where T : IRuntimePose
    {
        public TtAnimationCommand<S, T> FromCommand { get; set; } = null;
        public override void Execute()
        {
            if (FromCommand == null)
                return;

            TtRuntimePoseUtility.CopyPose(ref mOutPose, FromCommand.OutPose);
        }
    }

    public class TtLocalSpacePoseBlendTree : TtBlendTree<Bricks.StateMachine.TimedSM.TtDefaultCenterData, TtLocalSpaceRuntimePose>
    {

    }

    public class TtLocalSpacePoseFinalBlendTree<S> : TtLocalSpacePoseBlendTree<S>
    {
        public IBlendTree<S, TtLocalSpaceRuntimePose> FromNode { get; set; }
        public TtFinalBlendTreeCommand<S, TtLocalSpaceRuntimePose> BlendTreeCmd { get; set; }
        public void SetPose(TtLocalSpaceRuntimePose pose)
        {
            BlendTreeCmd.OutPose= pose;
        }
        public override async TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            BlendTreeCmd = new TtFinalBlendTreeCommand<S, TtLocalSpaceRuntimePose>();
            return await base.Initialize(context);
        }

        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            base.Tick(elapseSecond, ref context);
            FromNode.Tick(elapseSecond, ref context);

            FConstructAnimationCommandTreeContext constructContext = new();
            constructContext.CmdExecuteStack = new();
            var commandTree = ConstructAnimationCommandTree(null, ref constructContext);
            var stack = constructContext.CmdExecuteStack;
            stack.Execute();

        }

        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            base.ConstructAnimationCommandTree(parentNode, ref context);
            context.AddCommand(context.TreeDepth, BlendTreeCmd);

            context.TreeDepth++;
            if (FromNode != null)
            {
                BlendTreeCmd.FromCommand = FromNode.ConstructAnimationCommandTree(BlendTreeCmd, ref context);
            }
            return BlendTreeCmd;
        }
    }
    public class TtLocalSpacePoseFinalBlendTree : TtLocalSpacePoseFinalBlendTree<Bricks.StateMachine.TimedSM.TtDefaultCenterData>
    {

    }
}

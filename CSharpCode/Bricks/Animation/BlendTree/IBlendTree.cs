using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
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

    public interface IBlendTree<T> where T : IRuntimePose
    {
        void Initialize(ref FAnimBlendTreeContext context);
        void Tick(float elapseSecond, ref FAnimBlendTreeContext context);
        //void Notifying();
        TtAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context);
    }

    public class TtBlendTree<T> : IBlendTree<T> where T : IRuntimePose
    {
        bool mIsInitialized = false;
        public virtual void Initialize(ref FAnimBlendTreeContext context)
        {
            
        }

        public virtual void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {

        }

        public virtual TtAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            return null;
        }
    }

    public class TtLocalSpacePoseBlendTree :TtBlendTree<TtLocalSpaceRuntimePose>
    {

    }
}

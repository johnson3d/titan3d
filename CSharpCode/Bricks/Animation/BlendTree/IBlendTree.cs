using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree
{
    public struct FConstructAnimationCommandTreeContext
    {
        public int TreeDepth;
        public UAnimationCommandExecuteStack CmdExecuteStack;
        public void AddCommand(int depth, IAnimationCommand cmd)
        {
            CmdExecuteStack.AddCommand(depth, cmd);
        }
        public void Create()
        {
            TreeDepth = 0;
            CmdExecuteStack = new UAnimationCommandExecuteStack();
        }
    }
    public interface IBlendTree<T> where T : IRuntimePose
    {
        void Initialize();
        void Tick(float elapseSecond);
        //void Notifying();
        UAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context);
    }

    public class UBlendTree<T> : IBlendTree<T> where T : IRuntimePose
    {
        public virtual void Initialize()
        {

        }
        public virtual void Tick(float elapseSecond)
        {

        }
        public virtual UAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            return null;
        }
    }
}

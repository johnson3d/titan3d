using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtZeroPoseCommand<S, T> : TtAnimationCommand<S, T> where T : IRuntimePose
    {
       public TtZeroPoseCommandDesc Desc { get; set; }

        public override void Execute()
        {
            TtRuntimePoseUtility.ZeroPose(ref mOutPose);
        }
    }
    public class TtZeroPoseCommandDesc : IAnimationCommandDesc
    {

    }
    public class TtBlendTree_ZeroPose<S, T> : TtBlendTree<S, T> where T : IRuntimePose
    {
        TtZeroPoseCommand<S, T> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtZeroPoseCommand<S, T>();
            return false;
        }
        public override TtAnimationCommand<S, T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var depth = context.TreeDepth;
            mAnimationCommand.Desc = new TtZeroPoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);
            return mAnimationCommand;
        }
    }
}

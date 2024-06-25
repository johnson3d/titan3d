using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtZeroPoseCommand<T> : TtAnimationCommand<T> where T : IRuntimePose
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
    public class TtBlendTree_ZeroPose<T> : TtBlendTree<T> where T : IRuntimePose
    {
        TtZeroPoseCommand<T> mAnimationCommand = null;
        public override void Initialize(ref FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtZeroPoseCommand<T>();
        }
        public override TtAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var depth = context.TreeDepth;
            mAnimationCommand.Desc = new TtZeroPoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);
            return mAnimationCommand;
        }
    }
}

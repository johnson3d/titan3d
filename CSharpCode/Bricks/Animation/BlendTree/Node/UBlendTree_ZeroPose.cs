using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UZeroPoseCommand<T> : UAnimationCommand<T> where T : IRuntimePose
    {
       public UZeroPoseCommandDesc Desc { get; set; }

        public override void Execute()
        {
            URuntimePoseUtility.ZeroPose(ref mOutPose);
        }
    }
    public class UZeroPoseCommandDesc : IAnimationCommandDesc
    {

    }
    public class UBlendTree_ZeroPose<T> : UBlendTree<T> where T : IRuntimePose
    {
        UZeroPoseCommand<T> mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new UZeroPoseCommand<T>();
        }
        public override UAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var depth = context.TreeDepth;
            mAnimationCommand.Desc = new UZeroPoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);
            return mAnimationCommand;
        }
    }
}

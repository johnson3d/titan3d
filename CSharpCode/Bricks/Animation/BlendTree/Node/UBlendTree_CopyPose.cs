using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UCopyPoseCommand<T> : UAnimationCommand<T> where T : IRuntimePose
    {
        public UAnimationCommand<T> FromCommand { get; set; } = null;
        public UCopyPoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (FromCommand == null)
                return;
            URuntimePoseUtility.CopyTransforms(ref mOutPose, FromCommand.OutPose);
        }
    }
    public class UCopyPoseCommandDesc : IAnimationCommandDesc
    {
    }
    public class UBlendTree_CopyPose<T> : UBlendTree<T> where T : IRuntimePose
    {
        protected UCopyPoseCommandDesc Desc = null;
        public IBlendTree<T> FromNode { get; set; }

        UCopyPoseCommand<T> mAnimationCommand = null;
        public override UAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new UCopyPoseCommand<T>();
            mAnimationCommand.Desc = new UCopyPoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            mAnimationCommand.FromCommand = FromNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
    }
}

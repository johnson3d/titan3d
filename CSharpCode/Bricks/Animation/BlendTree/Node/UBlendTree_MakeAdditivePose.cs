using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UMakeAdditivePoseCommand : UAnimationCommand<ULPosMRotRuntimePose>
    {
        public UAnimationCommand<ULocalSpaceRuntimePose> AdditiveNode { get; set; }
        public UAnimationCommand<ULocalSpaceRuntimePose> RefNode { get; set; }
        ULPosMRotRuntimePose mAddPose = null;
        ULPosMRotRuntimePose mRefPose = null;
        public UMakeAdditivePoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (RefNode == null || AdditiveNode == null)
            {
                URuntimePoseUtility.ZeroPose(ref mOutPose);
                return;
            }
            URuntimePoseUtility.ConvetToLPosMRotRuntimePose(ref mAddPose, AdditiveNode.OutPose);
            URuntimePoseUtility.ConvetToLPosMRotRuntimePose(ref mRefPose, RefNode.OutPose);
            URuntimePoseUtility.MinusPoses(ref mOutPose, mRefPose, mAddPose);
        }
    }
    public class UMakeAdditivePoseCommandDesc : IAnimationCommandDesc
    {
     
    }
    public class UBlendTree_MakeAdditivePose : UBlendTree<ULPosMRotRuntimePose>
    {
        public IBlendTree<ULocalSpaceRuntimePose> AdditiveNode { get; set; }
        public IBlendTree<ULocalSpaceRuntimePose> RefNode { get; set; }

        UMakeAdditivePoseCommand mAnimationCommand = null;
        public override UAnimationCommand<ULPosMRotRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new UMakeAdditivePoseCommand();
            mAnimationCommand.Desc = new UMakeAdditivePoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            mAnimationCommand.AdditiveNode = AdditiveNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            mAnimationCommand.RefNode = RefNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond)
        {

        }
    }
}

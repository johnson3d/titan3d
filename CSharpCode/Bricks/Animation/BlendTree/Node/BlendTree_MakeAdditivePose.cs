using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtMakeAdditivePoseCommand : TtAnimationCommand<TtLPosMRotRuntimePose>
    {
        public TtAnimationCommand<TtLocalSpaceRuntimePose> AdditiveNode { get; set; }
        public TtAnimationCommand<TtLocalSpaceRuntimePose> RefNode { get; set; }
        TtLPosMRotRuntimePose mAddPose = null;
        TtLPosMRotRuntimePose mRefPose = null;
        public TtMakeAdditivePoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (RefNode == null || AdditiveNode == null)
            {
                TtRuntimePoseUtility.ZeroPose(ref mOutPose);
                return;
            }
            TtRuntimePoseUtility.ConvetToLPosMRotRuntimePose(ref mAddPose, AdditiveNode.OutPose);
            TtRuntimePoseUtility.ConvetToLPosMRotRuntimePose(ref mRefPose, RefNode.OutPose);
            TtRuntimePoseUtility.MinusPoses(ref mOutPose, mRefPose, mAddPose);
        }
    }
    public class TtMakeAdditivePoseCommandDesc : IAnimationCommandDesc
    {
     
    }
    public class TtBlendTree_MakeAdditivePose : TtBlendTree<TtLPosMRotRuntimePose>
    {
        public IBlendTree<TtLocalSpaceRuntimePose> AdditiveNode { get; set; }
        public IBlendTree<TtLocalSpaceRuntimePose> RefNode { get; set; }

        TtMakeAdditivePoseCommand mAnimationCommand = null;
        public override TtAnimationCommand<TtLPosMRotRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new TtMakeAdditivePoseCommand();
            mAnimationCommand.Desc = new TtMakeAdditivePoseCommandDesc();
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

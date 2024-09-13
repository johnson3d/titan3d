using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtMakeAdditivePoseCommand<S> : TtAnimationCommand<S, TtLPosMRotRuntimePose>
    {
        public TtAnimationCommand<S, TtLocalSpaceRuntimePose> AdditiveNode { get; set; }
        public TtAnimationCommand<S, TtLocalSpaceRuntimePose> RefNode { get; set; }
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
    public class TtBlendTree_MakeAdditivePose<S> : TtBlendTree<S, TtLPosMRotRuntimePose>
    {
        public IBlendTree<S, TtLocalSpaceRuntimePose> AdditiveNode { get; set; }
        public IBlendTree<S, TtLocalSpaceRuntimePose> RefNode { get; set; }

        TtMakeAdditivePoseCommand<S> mAnimationCommand = null;
        public override TtAnimationCommand<S, TtLPosMRotRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            System.Diagnostics.Debug.Assert(AdditiveNode != null && RefNode != null);
            if (AdditiveNode == null || RefNode == null)
                return null;

            mAnimationCommand = new TtMakeAdditivePoseCommand<S>();
            mAnimationCommand.Desc = new TtMakeAdditivePoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            mAnimationCommand.AdditiveNode = AdditiveNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            mAnimationCommand.RefNode = RefNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {

        }
    }
}

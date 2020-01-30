using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_CopyPose : IBlendTree
    {
        protected Pose.CGfxSkeletonPose mOutPose = null;
        public Pose.CGfxSkeletonPose OutPose
        {
            get => mOutPose;
            set
            {
                mOutPose = value;
            }
        }
        protected Pose.CGfxSkeletonPose mInPose = null;
        public Pose.CGfxSkeletonPose InPose
        {
            get => mInPose;
            set
            {
                mInPose = value;
            }
        }

        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
            //InPose = pose.Clone();
        }
        public void Evaluate(float timeInSecond)
        {
            if (InPose == null)
                return;
            Runtime.CGfxAnimationRuntime.CopyPose(OutPose,InPose);
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
          
        }
    }
}

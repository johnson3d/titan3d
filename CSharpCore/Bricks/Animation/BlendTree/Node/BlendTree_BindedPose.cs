using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_BindedPose : IBlendTree
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

        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
           
        }
        public void Evaluate(float timeInSecond)
        {
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
        }
    }
}

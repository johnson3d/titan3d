using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree
{
    //public class BlendTreeNode
    //{
    //    public IBlendTree BlentTree { get; set; } = null;

    //}
    public interface IBlendTree
    {
        //BlendTreeNode OutNode { get; set; }
        Pose.CGfxSkeletonPose OutPose { get; set; }
        void Evaluate(float timeInSecond);
        void Notifying(GamePlay.Component.GComponent component);
        void InitializePose(Pose.CGfxSkeletonPose pose);
    }
}

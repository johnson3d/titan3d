using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Pose
{
    public class CGfxCachedAnimationPose : ILogicTick
    {
        //CachedPose
        public CGfxSkeletonPose CachedAnimationPose
        {
            get;
        }
        //Copy the CachedPose to this
        public CGfxSkeletonPose AnimationPose
        {
            get;
        }
        public CGfxCachedAnimationPose(CGfxSkeletonPose cachedAnimationPose)
        {
            CachedAnimationPose = cachedAnimationPose;
            AnimationPose = CachedAnimationPose.Clone();
        }
        public bool EnableTick { get; set; } = true;

        public void TickLogic()
        {
            if (!EnableTick)
                return;
            if (AnimationPose == null)
                return;
            //AnimationPose.BlendWithTargetPose(CachedAnimationPose, 1);
            //AnimationPose.TransitionFixedWeight(0.1f);
        }
    }
}

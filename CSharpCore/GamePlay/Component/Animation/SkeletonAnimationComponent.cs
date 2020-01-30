using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    public class SkeletonAnimationComponent :GComponent
    {
        [Browsable(false)]
        public CGfxSkeletonPose Pose { get; set; }

    }
}

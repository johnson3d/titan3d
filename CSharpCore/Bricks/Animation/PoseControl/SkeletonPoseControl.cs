using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.PoseControl
{
    public class SkeletonPoseControl
    {
        public Pose.CGfxSkeletonPose OutPose { get; set; }
        public void Tick()
        {
            Update();
        }
        public virtual void Update()
        {

        }
    }
}

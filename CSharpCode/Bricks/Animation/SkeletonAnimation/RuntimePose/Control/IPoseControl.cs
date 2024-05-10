using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Runtime.Control
{
    public abstract class TtRuntimePoseControl
    {
        public Pose.TtLocalSpaceRuntimePose mOutPose = null;
        public Pose.TtLocalSpaceRuntimePose OutPose { get => mOutPose; set => mOutPose = value; }
        public void Tick(float elapseSecond)
        {
            Update(elapseSecond);
        }
        public virtual void Update(float elapseSecond)
        {

        }
    }
}

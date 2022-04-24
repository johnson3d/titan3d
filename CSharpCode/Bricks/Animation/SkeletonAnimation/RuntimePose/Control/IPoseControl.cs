using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Runtime.Control
{
    public abstract class URuntimePoseControl
    {
        public Pose.ULocalSpaceRuntimePose mOutPose = null;
        public Pose.ULocalSpaceRuntimePose OutPose { get => mOutPose; set => mOutPose = value; }
        public void Tick(float elapseSecond)
        {
            Update(elapseSecond);
        }
        public virtual void Update(float elapseSecond)
        {

        }
    }
}

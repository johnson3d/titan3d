using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Pose
{
    public class CGfxAnimationPoseProxy
    {
        public CGfxSkeletonPose Pose { get; set; }
        public bool IsNullPose
        {
            get
            {
                if (Pose == null )
                    return true;
                if (Pose.BoneNumber == 0)
                    return true;
                if (Pose.CoreObject.Pointer == IntPtr.Zero)
                    return true;
                return false;
            }
        }
        public string Name { get; set; }
        public CGfxAnimationPoseProxy()
        {

        }
        public CGfxAnimationPoseProxy(string name)
        {
            Name = name;
        }
    }
}

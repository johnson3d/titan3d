using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class CSkinModifier : AuxPtrType<ISkinModifier>
    {
        public Animation.SkeletonAnimation.Runtime.Pose.UMeshSpaceRuntimePose RuntimeMeshSpacePose;
        public CSkinModifier()
        {
            mCoreObject = ISkinModifier.CreateInstance();
        }
    }
}

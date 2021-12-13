using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class CSkinModifier : AuxPtrType<ISkinModifier>
    {
        public Animation.SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose AnimatableSkeletonPose { get; set; }
        public CSkinModifier()
        {
            mCoreObject = ISkinModifier.CreateInstance();
        }
    }
}

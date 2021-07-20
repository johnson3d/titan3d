using EngineNS.Animation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Pose
{
    public class USkeletonPose : ILimbPose
    {
        private Skeleton.UFullSkeletonDesc mDesc;
        public ILimbDesc Desc => mDesc;

        public Transform Transtorm { get; set; }

        public List<ILimbPose> Children { get; set; } = new List<ILimbPose>();
        
    }
}

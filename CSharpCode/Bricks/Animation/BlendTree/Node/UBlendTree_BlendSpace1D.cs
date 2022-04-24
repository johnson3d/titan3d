using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UBlendTree_BlendSpace1D : UBlendTree<ULocalSpaceRuntimePose>
    {
        public Func<Vector3> EvaluateInput { get; set; } = null;

        public string SyncPlayPercentGrop { get; set; } = "";
        UBlendSpace1D mBlendSpace = null;
        public UBlendSpace1D BlendSpace
        {
            get => mBlendSpace;
            set
            {
                mBlendSpace = value;
            }
        }
    }
}

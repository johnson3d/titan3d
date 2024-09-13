using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBlendTree_BlendSpace1D<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        public Func<Vector3> EvaluateInput { get; set; } = null;

        public string SyncPlayPercentGrop { get; set; } = "";
        TtBlendSpace1D mBlendSpace = null;
        public TtBlendSpace1D BlendSpace
        {
            get => mBlendSpace;
            set
            {
                mBlendSpace = value;
            }
        }
    }
}

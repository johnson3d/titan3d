using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.Animation.Macross.BlendTree
{
    public class TtPosePinDescription : TtPinDescription
    {
        public TtPosePinDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<TtLocalSpaceRuntimePose>();
        }
    }

    [GraphElement(typeof(TtGraphElement_PosePin))]
    public class TtPoseInPinDescription : TtPosePinDescription
    {
        
    }

    [GraphElement(typeof(TtGraphElement_PosePin))]
    public class TtPoseOutPinDescription : TtPosePinDescription
    {
        
    }


    [GraphElement(typeof(TtGraphElement_PoseLine))]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtPoseLineDescription : TtLineDescription
    {

    }
}

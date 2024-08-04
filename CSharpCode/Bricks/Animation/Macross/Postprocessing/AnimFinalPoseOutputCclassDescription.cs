using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Macross.BlendTree;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Macross.Postprocessing
{
    [OutlineElement_Leaf(typeof(TtOutlineElement_AnimFinalPoseOutput))]
    [Designable(typeof(TtLocalSpacePoseBlendTree), "AnimFinalPose")]
    [Graph(typeof(TtGraph_BlendTree))]
    public class TtAnimFinalPoseOutputCclassDescription : TtBlendTreeClassDescription
    {

    }
}

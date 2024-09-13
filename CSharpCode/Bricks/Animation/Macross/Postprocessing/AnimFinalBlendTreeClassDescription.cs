using EngineNS.Animation.Asset;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Macross.BlendTree;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Animation.Macross.Postprocessing
{
    [OutlineElement_Leaf(typeof(TtOutlineElement_AnimFinalPoseOutput))]
    [Designable(typeof(TtLocalSpacePoseFinalBlendTree), "FinalBlendTree")]
    [Graph(typeof(TtGraph_BlendTree))]
    public class TtAnimFinalBlendTreeClassDescription : TtBlendTreeClassDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "AnimFinalBlendTree";
    }
}

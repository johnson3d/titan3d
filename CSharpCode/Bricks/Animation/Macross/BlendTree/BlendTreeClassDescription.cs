using EngineNS.Animation.BlendTree;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Animation.Macross.BlendTree
{
    [OutlineElement_Leaf(typeof(TtOutlineElement_BlendTreeGraph))]
    [Designable(typeof(TtLocalSpacePoseBlendTree), "BlendTree")]
    [Graph(typeof(TtGraph_BlendTree))]
    public class TtBlendTreeClassDescription : TtDesignableVariableDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "BlendTree";
    }
}

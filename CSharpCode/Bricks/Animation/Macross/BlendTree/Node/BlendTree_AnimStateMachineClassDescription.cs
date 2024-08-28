using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.StateAttachment;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System.ComponentModel;
using System.Net.Mail;

namespace EngineNS.Animation.Macross.BlendTree
{
    [AnimBlendTreeContextMenu("StateMachine", "BlendTreeNode\\StateMachine", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BlendTree_AnimStateMachine))]
    public class TtBlendTree_AnimStateMachineClassDescription : TtBlendTreeNodeClassDescription
    {
        public override string Name { get => "StateMachine"; }
        public TtBlendTree_AnimStateMachineClassDescription()
        {
            AddPoseOutPin(new TtPoseOutPinDescription());
        }
    }
}

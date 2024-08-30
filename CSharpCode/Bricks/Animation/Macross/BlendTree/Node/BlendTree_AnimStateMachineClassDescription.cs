using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.Macross.StateMachine;
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
        public override string Name { get => "BlendTree_StateMachine"; }
        [Rtti.Meta]
        [Category("Option")]
        [PGStateMachineSelect()]
        public Guid AnimStateMachineId { get; set; } = Guid.Empty;
        public TtBlendTree_AnimStateMachineClassDescription()
        {
            AddPoseOutPin(new TtPoseOutPinDescription());
        }
        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            var mainClass = classBuildContext.MainClassDescription as TtClassDescription;
            IDesignableVariableDescription stateMachine = null;
            foreach(var designVar in mainClass.DesignableVariables)
            {
                if(designVar.Id == AnimStateMachineId)
                {
                    stateMachine = designVar;
                }
            }
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.Node.TtBlendTree_AnimStateMachine<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            if(stateMachine != null)
            {
                thisClassDeclaration.Properties.Add(stateMachine.BuildVariableDeclaration(ref classBuildContext));
            }
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }
        public override void GenerateCodeInClass(UClassDeclaration classDeclaration)
        {
            base.GenerateCodeInClass(classDeclaration);


        }
    }
}

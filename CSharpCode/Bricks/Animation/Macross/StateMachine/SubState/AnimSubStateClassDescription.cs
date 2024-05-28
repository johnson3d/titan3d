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
using System.ComponentModel;

namespace EngineNS.Bricks.Animation.Macross.StateMachine.SubState
{
    [StateMachineContextMenu("AnimState", "AnimStateMachine\\AnimState", UDesignMacross.MacrossAnimEditorKeyword)]
    [OutlineElement_Leaf(typeof(TtOutlineElement_TimedSubState))]
    [GraphElement(typeof(TtGraphElement_AnimSubState))]
    public class TtAnimSubStateClassDescription : TtTimedSubStateClassDescription
    {
        [Rtti.Meta]
        public override string Name { get; set; } = "AnimSubState";
        
        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedState<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new();
            UClassDeclaration thisClassDeclaration = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(this, ref classBuildContext);
            
            foreach (var transition in Transitions)
            {
                classDeclarationsBuilded.AddRange(transition.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(transition.BuildVariableDeclaration(ref classBuildContext));
            }
            foreach (var attachment in Attachments)
            {
                classDeclarationsBuilded.AddRange(attachment.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(attachment.BuildVariableDeclaration(ref classBuildContext));
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod());
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtDescriptionASTBuildUtil.BuildDefaultPartForVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private UMethodDeclaration BuildOverrideInitializeMethod()
        {
            UMethodDeclaration methodDeclaration = new UMethodDeclaration();
            methodDeclaration.IsOverride = true;
            methodDeclaration.MethodName = "Initialize";
            methodDeclaration.ReturnValue = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(bool)),
                InitValue = new UDefaultValueExpression(typeof(bool)),
                VariableName = "result"
            };

            foreach(var attachment in Attachments)
            {
                var stateAddAttachMentMethodInvoke = new UMethodInvokeStatement();
                stateAddAttachMentMethodInvoke.Host = new USelfReferenceExpression();
                stateAddAttachMentMethodInvoke.MethodName = "AddAttachment";
                stateAddAttachMentMethodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression(attachment.VariableName) });
                methodDeclaration.MethodBody.Sequence.Add(stateAddAttachMentMethodInvoke);
            }

            UAssignOperatorStatement returnValueAssign = new UAssignOperatorStatement();
            returnValueAssign.To = new UVariableReferenceExpression("result");
            returnValueAssign.From = new UPrimitiveExpression(true);
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

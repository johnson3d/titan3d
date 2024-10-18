using EngineNS.Animation;
using EngineNS.Animation.Macross;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.StateAttachment;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.Graphics.Mesh;
using EngineNS.Rtti;
using System.ComponentModel;

namespace EngineNS.Bricks.Animation.Macross.StateMachine.SubState
{
    [StateMachineContextMenu("AnimState", "AnimStateMachine\\AnimState", UDesignMacross.MacrossAnimEditorKeyword)]
    [OutlineElement_Leaf(typeof(TtOutlineElement_TimedSubState))]
    [GraphElement(typeof(TtGraphElement_AnimSubState))]
    public class TtAnimSubStateClassDescription : TtTimedSubStateClassDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "AnimSubState";
        
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtAnimState<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            
            foreach (var transition in Transitions)
            {
                classDeclarationsBuilded.AddRange(transition.BuildClassDeclarations(ref classBuildContext));
                //thisClassDeclaration.Properties.Add(transition.BuildVariableDeclaration(ref classBuildContext));
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

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod()
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateStateMachineOverridedInitMethodStatement();

            foreach (var attachment in Attachments)
            {
                TtAnimASTBuildUtil.CreateNewThenCenterDataAssignThenInitInvokeStatement(attachment, methodDeclaration);
                CreateAddAttachmentMethodStatement(attachment, methodDeclaration);
            }

            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        private void CreateAddAttachmentMethodStatement(TtDesignableVariableDescription desc,TtMethodDeclaration method)
        {
            var stateAddAttachMentMethodInvoke = new TtMethodInvokeStatement();
            stateAddAttachMentMethodInvoke.Host = new TtSelfReferenceExpression();
            stateAddAttachMentMethodInvoke.MethodName = "AddAttachment";
            stateAddAttachMentMethodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(desc.VariableName) });
            method.MethodBody.Sequence.Add(stateAddAttachMentMethodInvoke);
        }
        #endregion
    }
}

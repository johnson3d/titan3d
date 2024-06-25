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
        
        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtAnimState<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new();
            UClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            
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
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private UMethodDeclaration BuildOverrideInitializeMethod()
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var args = new List<UMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtAnimStateMachineContext>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true);

            foreach (var attachment in Attachments)
            {
                var attachmentAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new UVariableReferenceExpression(attachment.Name), new UCreateObjectExpression(attachment.VariableType.TypeFullName));
                methodDeclaration.MethodBody.Sequence.Add(attachmentAssign);

                var stateAddAttachMentMethodInvoke = new UMethodInvokeStatement();
                stateAddAttachMentMethodInvoke.Host = new USelfReferenceExpression();
                stateAddAttachMentMethodInvoke.MethodName = "AddAttachment";
                stateAddAttachMentMethodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression(attachment.VariableName) });
                methodDeclaration.MethodBody.Sequence.Add(stateAddAttachMentMethodInvoke);
            }

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new UVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new UPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

using EngineNS.Animation.Macross;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.StateAttachment;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using System.ComponentModel;

namespace EngineNS.Bricks.StateMachine.Macross.SubState
{
    [StateMachineContextMenu("State", "StateMachine\\State", UDesignMacross.MacrossScriptEditorKeyword)]
    [OutlineElement_Leaf(typeof(TtOutlineElement_TimedSubState))]
    [GraphElement(typeof(TtGraphElement_TimedSubState))]
    public class TtTimedSubStateClassDescription : TtDesignableVariableDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "TimedSubState";
        [Rtti.Meta]
        [Category("Option")]
        public bool bInitialActive { get; set; } = false;
        [Rtti.Meta]
        [Category("Option")]
        public float Duration { get; set; } = 1;
        [DrawInGraph]
        [Rtti.Meta]
        public List<TtTimedStateTransitionClassDescription> Transitions { get; set; } = new List<TtTimedStateTransitionClassDescription>();
        [DrawInGraph]
        [Rtti.Meta]
        public List<TtTimedStateAttachmentClassDescription> Attachments { get; set; } = new List<TtTimedStateAttachmentClassDescription>();
        public bool AddTransition(TtTimedStateTransitionClassDescription transition)
        {
            Transitions.Add(transition);
            transition.Parent = this;
            return true;
        }
        public bool RemoveTransition(TtTimedStateTransitionClassDescription transition)
        {
            Transitions.Remove(transition);
            transition.Parent = null;
            return true;
        }
        public bool AddAttachment(TtTimedStateAttachmentClassDescription attachment)
        {
            Attachments.Add(attachment); 
            attachment.Parent = this;
            return true;   
        }
        public bool RemoveAttachment(TtTimedStateAttachmentClassDescription attachment)
        {
            Attachments.Remove(attachment);
            attachment.Parent = null;
            return true;
        }
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedState<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            
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

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod()
        {
            var methodDeclaration = TtStateMachineASTBuildUtil.CreateOverridedInitMethodStatement();

            foreach (var attachment in Attachments)
            {
                var stateAddAttachMentMethodInvoke = new TtMethodInvokeStatement();
                stateAddAttachMentMethodInvoke.Host = new TtSelfReferenceExpression();
                stateAddAttachMentMethodInvoke.MethodName = "AddAttachment";
                stateAddAttachMentMethodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(attachment.VariableName) });
                methodDeclaration.MethodBody.Sequence.Add(stateAddAttachMentMethodInvoke);
            }

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

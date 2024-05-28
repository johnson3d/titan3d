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
        public override string Name { get; set; } = "TimedSubState";
        [Rtti.Meta]
        public bool bInitialActive { get; set; } = false;
        [Rtti.Meta]
        public float Duration { get; set; } = 1;
        [DrawInGraph]
        [Browsable(false)]
        [Rtti.Meta]
        public List<TtTimedStateTransitionClassDescription> Transitions { get; set; } = new List<TtTimedStateTransitionClassDescription>();
        [DrawInGraph]
        [Browsable(false)]
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

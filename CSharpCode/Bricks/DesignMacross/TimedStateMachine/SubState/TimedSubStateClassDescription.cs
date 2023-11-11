using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [StateMachineContextMenu("State", "StateMachine\\State", UDesignMacross.MacrossEditorKeyword)]
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
        public List<ITimedStateAttachmentClassDescription> Attachments { get; set; } = new List<ITimedStateAttachmentClassDescription>();
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
        public bool AddAttachment(ITimedStateAttachmentClassDescription attachment)
        {
            Attachments.Add(attachment); 
            attachment.Parent = this;
            return true;   
        }
        public bool RemoveAttachment(ITimedStateAttachmentClassDescription attachment)
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
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtDescriptionASTBuildUtil.BuildDefaultPartForVariableDeclaration(this, ref classBuildContext);
        }
    }
}

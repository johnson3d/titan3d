using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.TimedStateMachine.StateAttachment.Scrpit;
using EngineNS.Rtti;
using System;
using System.Reflection;

namespace EngineNS.DesignMacross.TimedStateMachine.StateAttachment
{
    [TimedStateAttachmentContextMenu("Script", "Attachment\\Script", UDesignMacross.MacrossEditorKeyword)]
    [Graph(typeof(TtGraph_TimedStateScriptAttachment))]
    [GraphElement(typeof(TtGraphElement_TimedStateScriptAttachment))]
    public class TtTimedStateScriptAttachmentClassDescription : TtTimedStateAttachmentClassDescription
    {

        //public TtDelegateEventDescription OnBeginDesc { get; set; } = new TtDelegateEventDescription{ Name = "OnBegin" };
        //public TtDelegateEventDescription OnTickDesc;
        //public TtDelegateEventDescription OnEndDesc;

        public TtTimedStateScriptAttachmentClassDescription()
        {
            TickMethodDescription = new TtMethodDescription()
            {
                Name = "Update",
                Parent = this,
                IsOverride = true
            };
            var elapseSecondMethodArgument = new UMethodArgumentDeclaration { VariableName = "elapseSecond", VariableType = new UTypeReference(typeof(float)) };
            var contextMethodArgument = new UMethodArgumentDeclaration { OperationType = EMethodArgumentAttribute.In, VariableType = new UTypeReference(UTypeDesc.TypeOf<TtStateMachineContext>()), VariableName = "context" };
            TickMethodDescription.Arguments.Add(elapseSecondMethodArgument);
            TickMethodDescription.Arguments.Add(contextMethodArgument);
        }

        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateAttachment<{classBuildContext.MainClassDescription.ClassName}>");
            UClassDeclaration thisClassDeclaration = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(this, ref classBuildContext);
            FClassBuildContext transitionClassBuildContext = new FClassBuildContext()
            {
                MainClassDescription = classBuildContext.MainClassDescription,
                ClassDeclaration = thisClassDeclaration,
            };
            thisClassDeclaration.AddMethod(TickMethodDescription.BuildMethodDeclaration(ref transitionClassBuildContext));
            return new List<UClassDeclaration>() { thisClassDeclaration };
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return base.BuildVariableDeclaration(ref classBuildContext);
        }
    }
}

using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
{
    [Graph(typeof(TtGraph_TimedStateScriptAttachmentMethod))]
    public class TtTimedStateScriptMethodDescription : TtMethodDescription
    {

    }
    [TimedStateAttachmentContextMenu("Script", "Attachment\\Script", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_TimedStateScriptAttachment))]
    public class TtTimedStateScriptAttachmentClassDescription : TtTimedStateAttachmentClassDescription
    {
        //public TtDelegateEventDescription OnBeginDesc { get; set; } = new TtDelegateEventDescription{ Name = "OnBegin" };
        //public TtDelegateEventDescription OnTickDesc;
        //public TtDelegateEventDescription OnEndDesc;

        public TtTimedStateScriptAttachmentClassDescription()
        {
            TickMethodDescription = new TtTimedStateScriptMethodDescription()
            {
                Name = "Update",
                Parent = this,
                IsOverride = true
            };
            var elapseSecondMethodArgument = new TtMethodArgumentDescription { Name = "elapseSecond", VariableType = UTypeDesc.TypeOf<float>()};
            var contextMethodArgument = new TtMethodArgumentDescription { OperationType = EMethodArgumentAttribute.In, VariableType = UTypeDesc.TypeOf<TtStateMachineContext>(), Name = "context" };
            TickMethodDescription.AddArgument(elapseSecondMethodArgument);
            TickMethodDescription.AddArgument(contextMethodArgument);
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

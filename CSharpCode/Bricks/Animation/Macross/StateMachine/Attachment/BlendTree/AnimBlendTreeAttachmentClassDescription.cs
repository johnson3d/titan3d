﻿using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
{
    [Graph(typeof(TtGraph_AnimBlendTreeAttachmentMethod))]
    public class TtAnimBlendTreeMethodDescription : TtTimedStateScriptMethodDescription
    {

    }
    [TimedStateAttachmentContextMenu("BlendTree", "Attachment\\BlendTree", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_AnimBlendTreeAttachment))]
    public class TtAnimBlendTreeAttachmentClassDescription : TtTimedStateAttachmentClassDescription
    {
        public override string Name { get; set; } = "BlendTree";
        public TtAnimBlendTreeAttachmentClassDescription()
        {
            TickMethodDescription = new TtAnimBlendTreeMethodDescription()
            {
                Name = "Update",
                Parent = this,
                IsOverride = true
            };
            var elapseSecondMethodArgument = new TtMethodArgumentDescription { Name = "elapseSecond", VariableType = TtTypeDesc.TypeOf<float>() };
            var contextMethodArgument = new TtMethodArgumentDescription { OperationType = EMethodArgumentAttribute.In, VariableType = TtTypeDesc.TypeOf<TtStateMachineContext>(), Name = "context" };
            TickMethodDescription.AddArgument(elapseSecondMethodArgument);
            TickMethodDescription.AddArgument(contextMethodArgument);
        }

        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateAttachment<{classBuildContext.MainClassDescription.ClassName}>");
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            FClassBuildContext transitionClassBuildContext = new FClassBuildContext()
            {
                MainClassDescription = classBuildContext.MainClassDescription,
                ClassDeclaration = thisClassDeclaration,
            };
            thisClassDeclaration.AddMethod(TickMethodDescription.BuildMethodDeclaration(ref transitionClassBuildContext));
            return new List<TtClassDeclaration>() { thisClassDeclaration };
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return base.BuildVariableDeclaration(ref classBuildContext);
        }

    }
}

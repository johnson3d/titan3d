using EngineNS.Animation.Macross;
using EngineNS.Animation.StateMachine;
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

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
{
    [TimedStateAttachmentContextMenu("AnimPlay", "Attachment\\AnimPlay", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_AnimPlayAttachment))]
    public class TtAnimPlayAttachmentClassDescription : TtTimedStateAttachmentClassDescription
    {
        public override string Name { get; set; } = "AnimPlay";
        [Rtti.Meta]
        [RName.PGRName(FilterExts = EngineNS.Animation.Asset.TtAnimationClip.AssetExt)]
        [Category("Option")]

        public RName AnimationClip { get; set; }
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);

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

            var getClipRName = new TtMethodInvokeStatement("ParseFrom", 
                TtASTBuildUtil.CreateVariableDeclaration("AnimationClipName", new TtTypeReference(typeof(RName)),null), 
                new TtClassReferenceExpression(UTypeDesc.TypeOf<RName>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtPrimitiveExpression(AnimationClip.ToString())});
            methodDeclaration.MethodBody.Sequence.Add(getClipRName);

            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

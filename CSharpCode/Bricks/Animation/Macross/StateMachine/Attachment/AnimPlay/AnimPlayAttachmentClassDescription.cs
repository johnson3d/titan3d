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
        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new();
            UClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);

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
            var contextMethodArgument = TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtAnimStateMachineContext>()), EMethodArgumentAttribute.Default);
            var args = new List<UMethodArgumentDeclaration>
            {
                contextMethodArgument
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true);

            var getClipRName = new UMethodInvokeStatement("GetRName", TtASTBuildUtil.CreateVariableDeclaration("AnimationClipName", new UTypeReference(typeof(RName)),null), new UClassReferenceExpression(UTypeDesc.TypeOf<RName>()), new UMethodInvokeArgumentExpression { Expression = new UPrimitiveExpression(AnimationClip.ToString())});
            methodDeclaration.MethodBody.Sequence.Add(getClipRName);
            //var clipAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
            //                            new UVariableReferenceExpression("AnimationClipName"),
            //                            new UVariableReferenceExpression(getClipRName.ReturnValue.VariableName));
            
            //methodDeclaration.MethodBody.Sequence.Add(clipAssign);

            var baseInvoke = new UMethodInvokeStatement("Initialize",
                                null, new UBaseReferenceExpression(), 
                                new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression(contextMethodArgument.VariableName) });
            methodDeclaration.MethodBody.Sequence.Add(baseInvoke);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new UVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new UPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

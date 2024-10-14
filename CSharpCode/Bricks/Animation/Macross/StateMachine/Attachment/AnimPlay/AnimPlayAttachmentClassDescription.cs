using EngineNS.Animation;
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

    public class TtAnimPlayAttachmentClassDescription : TtTimedStateAttachmentClassDescription
    {

    }
    [TimedStateAttachmentContextMenu("ClipPlay", "Attachment\\ClipPlay", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_AnimPlayAttachment))]
    public class TtClipPlayAttachmentClassDescription : TtAnimPlayAttachmentClassDescription
    {
        public override string Name { get; set; } = "ClipPlay";
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
                TtASTBuildUtil.CreateVariableDeclaration("AnimationClipName", new TtTypeReference(typeof(RName)), null),
                new TtClassReferenceExpression(TtTypeDesc.TypeOf<RName>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtPrimitiveExpression(AnimationClip.ToString()) });
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

    [TimedStateAttachmentContextMenu("BlendSpacePlay", "Attachment\\BlendSpacePlay", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_AnimPlayAttachment))]
    public class TtBlendSpacePlayAttachmentClassDescription : TtAnimPlayAttachmentClassDescription
    {
        public override string Name { get; set; } = "BlendSpacePlay";
        [Rtti.Meta]
        [RName.PGRName(FilterExts = EngineNS.Animation.Asset.BlendSpace.TtBlendSpace2D.AssetExt)]
        [Category("Option")]
        public RName Animation { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [PGBlendSpaceValueBindSelect]
        public Guid XBind { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [PGBlendSpaceValueBindSelect]
        public Guid YBind { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [PGBlendSpaceValueBindSelect]
        public Guid ZBind { get; set; }
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtBlendSpacePlayStateAttachment<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);

            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod(ref classBuildContext));
            thisClassDeclaration.AddMethod(BuildOverridTickMethod(ref classBuildContext));
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateStateMachineOverridedInitMethodStatement();

            var getClipRName = new TtMethodInvokeStatement("ParseFrom",
                TtASTBuildUtil.CreateVariableDeclaration("AnimationName", new TtTypeReference(typeof(RName)), null),
                new TtClassReferenceExpression(TtTypeDesc.TypeOf<RName>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtPrimitiveExpression(Animation.ToString()) });
            methodDeclaration.MethodBody.Sequence.Add(getClipRName);

            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        private TtMethodDeclaration BuildOverridTickMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateStateMachineOverridedTickMethodStatement();
            IVariableDescription xVar = null, yVar = null, zVar = null;
            foreach(var variable in classBuildContext.MainClassDescription.Variables)
            {
                if(variable.Id == XBind)
                {
                    xVar = variable;
                }
                if (variable.Id == YBind)
                {
                    yVar = variable;
                }
                if (variable.Id == ZBind)
                {
                    zVar = variable;
                }
            }
            if(xVar != null)
            {
                var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("X", new TtVariableReferenceExpression("Input")),
                new TtVariableReferenceExpression(xVar.Name, new TtVariableReferenceExpression("CenterData")));
                methodDeclaration.MethodBody.Sequence.Add(assign);
            }
            if (yVar != null)
            {
                var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("Y", new TtVariableReferenceExpression("Input")),
                new TtVariableReferenceExpression(yVar.Name, new TtVariableReferenceExpression("CenterData")));
                methodDeclaration.MethodBody.Sequence.Add(assign);
            }
            if (zVar != null)
            {
                var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("Z", new TtVariableReferenceExpression("Input")),
                new TtVariableReferenceExpression(zVar.Name, new TtVariableReferenceExpression("CenterData")));
                methodDeclaration.MethodBody.Sequence.Add(assign);
            }
            TtAnimASTBuildUtil.CreateBaseTickInvokeStatement(methodDeclaration);
            return methodDeclaration;
        }
        #endregion
    }
}

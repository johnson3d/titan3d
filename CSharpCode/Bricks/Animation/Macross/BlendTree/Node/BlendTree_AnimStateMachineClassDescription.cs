using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.Macross.StateMachine;
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

namespace EngineNS.Animation.Macross.BlendTree
{
    [AnimBlendTreeContextMenu("StateMachine", "BlendTreeNode\\StateMachine", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BlendTree_AnimStateMachine))]
    public class TtBlendTree_AnimStateMachineClassDescription : TtBlendTreeNodeClassDescription
    {
        public override string Name { get => "BlendTree_StateMachine"; }
        [Rtti.Meta]
        [Category("Option")]
        [PGStateMachineSelect()]
        public Guid AnimStateMachineId { get; set; } = Guid.Empty;
        public TtBlendTree_AnimStateMachineClassDescription()
        {
            AddPoseOutPin(new TtPoseOutPinDescription());
        }
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            var mainClass = classBuildContext.MainClassDescription as TtClassDescription;
            IDesignableVariableDescription stateMachine = null;
            foreach(var designVar in mainClass.DesignableVariables)
            {
                if(designVar.Id == AnimStateMachineId)
                {
                    stateMachine = designVar;
                }
            }
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_AnimStateMachine<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            if(stateMachine != null)
            {
                thisClassDeclaration.Properties.Add(stateMachine.BuildVariableDeclaration(ref classBuildContext));
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod(stateMachine));
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }
        public override void GenerateCodeInClass(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            base.GenerateCodeInClass(classDeclaration, ref classBuildContext);
        }

        public override TtStatementBase BuildBlendTreeStatement(ref FBlendTreeBuildContext blendTreeBuildContext)
        {
            return base.BuildBlendTreeStatement(ref blendTreeBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod(IDesignableVariableDescription stateMachine)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedInitMethodStatement();
            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);

            var stateMachineAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression(stateMachine.VariableName),
                new TtVariableReferenceExpression(stateMachine.VariableName, new TtVariableReferenceExpression("CenterData")));
            methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);

            TtAnimASTBuildUtil.CreateCenterDataAssignStatement(stateMachine, methodDeclaration);

            var interalStateMachineAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("AnimStateMachine"),
                new TtVariableReferenceExpression(stateMachine.VariableName));
            methodDeclaration.MethodBody.Sequence.Add(interalStateMachineAssin);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

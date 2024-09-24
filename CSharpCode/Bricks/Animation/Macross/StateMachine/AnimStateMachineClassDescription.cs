using EngineNS.Animation.BlendTree;
using EngineNS.Animation;
using EngineNS.Animation.Macross;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System.ComponentModel;

namespace EngineNS.Bricks.Animation.Macross.StateMachine
{
    [OutlineElement_Branch(typeof(TtOutlineElement_AnimStateMachine))]
    [Designable(typeof(TtAnimStateMachine), "AnimStateMachine")]
    public class TtAnimStateMachineClassDescription : TtTimedStateMachineClassDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "AnimStateMachine";
        [Rtti.Meta]
        [OutlineElement_List(typeof(TtOutlineElementsList_AnimCompoundStates), true)]
        public override List<TtTimedCompoundStateClassDescription> CompoundStates { get; set; } = new();
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtAnimStateMachine<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);

            foreach (var compoundState in CompoundStates)
            {
                classDeclarationsBuilded.AddRange(compoundState.BuildClassDeclarations(ref classBuildContext));
                var compoundStateProperty = compoundState.BuildVariableDeclaration(ref classBuildContext);
                compoundStateProperty.VisitMode = EVisisMode.Public;
                thisClassDeclaration.Properties.Add(compoundStateProperty);
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod(ref classBuildContext));
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }
        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        void GenerateCodeInMainClassInitMethod(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            var initMethod = classDeclaration.FindMethod("Initialize");
            if (initMethod == null)
            {
                initMethod = TtASTBuildUtil.CreateInitMethodDeclaration();
                classDeclaration.AddMethod(initMethod);
            }

            var animSMContext_VarName = "animStateMachineContext" + VariableName;

            var animSMContextCreate = TtASTBuildUtil.CreateVariableDeclaration(animSMContext_VarName, 
                new TtTypeReference(typeof(TtAnimStateMachineContext)), 
                new TtCreateObjectExpression(typeof(TtAnimStateMachineContext).FullName));

            initMethod.MethodBody.Sequence.Add(animSMContextCreate);

            var blendTreeContext_VarName = "blendTreeContext" + VariableName;

            var blendTreeContextCreate = TtASTBuildUtil.CreateVariableDeclaration(blendTreeContext_VarName, 
                new TtTypeReference(typeof(FAnimBlendTreeContext)), 
                new TtCreateObjectExpression(typeof(FAnimBlendTreeContext).FullName));

            initMethod.MethodBody.Sequence.Add(blendTreeContextCreate);

            var animatableSkeletonPose_VarName = "animatableSkeletonPose" + VariableName;

            var animatableSkeletonPoseCreate = TtASTBuildUtil.CreateVariableDeclaration(animatableSkeletonPose_VarName, 
                new TtTypeReference(typeof(TtAnimatableSkeletonPose)), new TtNullValueExpression());
            initMethod.MethodBody.Sequence.Add(animatableSkeletonPoseCreate);

            var getAnimatablePoseFromNode = new TtMethodInvokeStatement("CreateAnimatableSkeletonPoseFromeNode",
                animatableSkeletonPoseCreate,
                new TtClassReferenceExpression(TtTypeDesc.TypeOf<TtAnimUtil>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("MacrossNode") }
                );
            initMethod.MethodBody.Sequence.Add(getAnimatablePoseFromNode);

            var blendTreeContextAnimatablePoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("AnimatableSkeletonPose", new TtVariableReferenceExpression(blendTreeContext_VarName)),
                new TtVariableReferenceExpression(animatableSkeletonPose_VarName));
            initMethod.MethodBody.Sequence.Add(blendTreeContextAnimatablePoseAssign);

            var animSMBlendTreeAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("BlendTreeContext", new TtVariableReferenceExpression(animSMContext_VarName)),
                new TtVariableReferenceExpression(blendTreeContext_VarName));
            initMethod.MethodBody.Sequence.Add(animSMBlendTreeAssign);

            var finalPoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(VariableName), 
                new TtCreateObjectExpression(VariableType.TypeFullName));
            initMethod.MethodBody.Sequence.Add(finalPoseAssign);

            var centerDataAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("CenterData", new TtVariableReferenceExpression(VariableName)),
                new TtSelfReferenceExpression());
            initMethod.MethodBody.Sequence.Add(centerDataAssign);

            var finalPoseInitializeInvoke = new TtMethodInvokeStatement("Initialize",
                null, new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Default, Expression = new TtVariableReferenceExpression(animSMContext_VarName) });
            finalPoseInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(finalPoseInitializeInvoke);

        }
        public override void GenerateCodeInClass(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            base.GenerateCodeInClass(classDeclaration, ref classBuildContext);

            GenerateCodeInMainClassInitMethod(classDeclaration, ref classBuildContext);

        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateStateMachineOverridedInitMethodStatement();
            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);
            foreach (var compoundState in CompoundStates)
            {
                var compoundStateAssignLH = new TtVariableReferenceExpression(compoundState.VariableName);
                var compoundStateAssignRH = new TtCreateObjectExpression(compoundState.VariableType.TypeFullName);
                var compoundStateAssign = TtASTBuildUtil.CreateAssignOperatorStatement(compoundStateAssignLH, compoundStateAssignRH);
                methodDeclaration.MethodBody.Sequence.Add(compoundStateAssign);

                var stateMachineAssignLH = new TtVariableReferenceExpression("StateMachine", new TtVariableReferenceExpression(compoundState.VariableName));
                var stateMachineAssignRH = new TtSelfReferenceExpression();
                var stateMachineAssign = TtASTBuildUtil.CreateAssignOperatorStatement(stateMachineAssignLH, stateMachineAssignRH);
                methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);

                TtAnimASTBuildUtil.CreateCenterDataAssignStatement(compoundState, methodDeclaration);

            }
            foreach (var compoundState in CompoundStates)
            {
                var initializeMethodInvoke = new TtMethodInvokeStatement();
                initializeMethodInvoke.Host = new TtVariableReferenceExpression(compoundState.VariableName);
                initializeMethodInvoke.MethodName = "Initialize";
                initializeMethodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("context") });
                initializeMethodInvoke.IsAsync = true;
                methodDeclaration.MethodBody.Sequence.Add(initializeMethodInvoke);
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

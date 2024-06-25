using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.Rtti;

namespace EngineNS.Bricks.Animation.Macross.StateMachine
{
    [OutlineElement_Branch(typeof(TtOutlineElement_AnimStateMachine))]
    [Designable(typeof(TtAnimStateMachine), "AnimStateMachine")]
    public class TtAnimStateMachineClassDescription : TtTimedStateMachineClassDescription
    {
        [Rtti.Meta]
        public override string Name { get; set; } = "AnimStateMachine";
        [Rtti.Meta]
        [OutlineElement_List(typeof(TtOutlineElementsList_AnimCompoundStates), true)]
        public override List<TtTimedCompoundStateClassDescription> CompoundStates { get; set; } = new();
        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtAnimStateMachine<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            UClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);


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

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private UMethodDeclaration BuildOverrideInitializeMethod(ref FClassBuildContext classBuildContext)
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var args = new List<UMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtAnimStateMachineContext>()), EMethodArgumentAttribute.Ref)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true, true);

            foreach (var compoundState in CompoundStates)
            {
                var compoundStateAssignLH = new UVariableReferenceExpression(compoundState.VariableName);
                var compoundStateAssignRH = new UCreateObjectExpression(compoundState.VariableType.TypeFullName);
                var compoundStateAssign = TtASTBuildUtil.CreateAssignOperatorStatement(compoundStateAssignLH, compoundStateAssignRH);
                methodDeclaration.MethodBody.Sequence.Add(compoundStateAssign);

                var stateMachineAssignLH = new UVariableReferenceExpression("StateMachine", new UVariableReferenceExpression(compoundState.VariableName));
                var stateMachineAssignRH = new USelfReferenceExpression();
                var stateMachineAssign = TtASTBuildUtil.CreateAssignOperatorStatement(stateMachineAssignLH, stateMachineAssignRH);
                methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);
            }
            foreach (var compoundState in CompoundStates)
            {
                var initializeMethodInvoke = new UMethodInvokeStatement();
                initializeMethodInvoke.Host = new UVariableReferenceExpression(compoundState.VariableName);
                initializeMethodInvoke.MethodName = "Initialize";
                methodDeclaration.MethodBody.Sequence.Add(initializeMethodInvoke);
            }
            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new UVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new UPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;

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
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateMachine<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            UClassDeclaration thisClassDeclaration = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(this, ref classBuildContext);


            foreach (var compoundState in CompoundStates)
            {
                classDeclarationsBuilded.AddRange(compoundState.BuildClassDeclarations(ref classBuildContext));
                var compoundStateProperty = compoundState.BuildVariableDeclaration(ref classBuildContext);
                compoundStateProperty.VisitMode = EVisisMode.Public;
                thisClassDeclaration.Properties.Add(compoundStateProperty);
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod());
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtDescriptionASTBuildUtil.BuildDefaultPartForVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private UMethodDeclaration BuildOverrideInitializeMethod()
        {
            UMethodDeclaration methodDeclaration = new UMethodDeclaration();
            methodDeclaration.IsOverride = true;
            methodDeclaration.MethodName = "Initialize";
            methodDeclaration.ReturnValue = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(bool)),
                InitValue = new UDefaultValueExpression(typeof(bool)),
                VariableName = "result"
            };
            foreach (var compoundState in CompoundStates)
            {
                UAssignOperatorStatement compoundStateAssign = new();
                compoundStateAssign.To = new UVariableReferenceExpression(compoundState.VariableName);
                compoundStateAssign.From = new UCreateObjectExpression(compoundState.VariableType.TypeFullName);
                methodDeclaration.MethodBody.Sequence.Add(compoundStateAssign);

                UAssignOperatorStatement stateMachineAssign = new();
                stateMachineAssign.To = new UVariableReferenceExpression("StateMachine", new UVariableReferenceExpression(compoundState.VariableName));
                stateMachineAssign.From = new USelfReferenceExpression();
                methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);
            }
            foreach (var compoundState in CompoundStates)
            {
                var initializeMethodInvoke = new UMethodInvokeStatement();
                initializeMethodInvoke.Host = new UVariableReferenceExpression(compoundState.VariableName);
                initializeMethodInvoke.MethodName = "Initialize";
                methodDeclaration.MethodBody.Sequence.Add(initializeMethodInvoke);
            }
            UAssignOperatorStatement returnValueAssign = new UAssignOperatorStatement();
            returnValueAssign.To = new UVariableReferenceExpression("result");
            returnValueAssign.From = new UPrimitiveExpression(true);
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}

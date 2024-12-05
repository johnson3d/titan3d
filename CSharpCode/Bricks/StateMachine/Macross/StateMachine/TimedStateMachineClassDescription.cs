using EngineNS.Animation.Macross;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;

namespace EngineNS.Bricks.StateMachine.Macross
{
    [OutlineElement_Branch(typeof(TtOutlineElement_TimedStateMachine))]
    [Designable(typeof(TtTimedStateMachine), "TimedStateMachine")]
    public class TtTimedStateMachineClassDescription : TtDesignableVariableDescription
    {
        [Rtti.Meta]
        public override string Name { get; set; } = "TimeStateMachine";
        [Rtti.Meta]
        [OutlineElement_List(typeof(TtOutlineElementsList_TimedCompoundStates), true)]
        public virtual List<TtTimedCompoundStateClassDescription> CompoundStates { get; set; } = new List<TtTimedCompoundStateClassDescription>();
        public TtTimedStateMachineClassDescription()
        {
            
        }
        public override void UpdateData(ref FDescriptionUpdateContext updateContext)
        {
            base.UpdateData(ref updateContext);
            foreach(var compoundState in CompoundStates)
            {
                compoundState.UpdateData(ref updateContext);
            }
        }
        public bool AddCompoundState(TtTimedCompoundStateClassDescription compoundState)
        {
            CompoundStates.Add(compoundState);
            compoundState.Parent = this;
            return true;
        }
        public bool RemoveCompoundState(TtTimedCompoundStateClassDescription compoundState)
        {
            CompoundStates.Remove(compoundState);
            compoundState.Parent = null;
            return true;
        }
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateMachine<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);


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

            var stateMachineContext_VarName = "stateMachineContext" + VariableName;

            var stateMachineContextCreate = TtASTBuildUtil.CreateVariableDeclaration(stateMachineContext_VarName,
                new TtTypeReference(typeof(TtStateMachineContext)),
                new TtCreateObjectExpression(typeof(TtStateMachineContext).FullName));

            initMethod.MethodBody.Sequence.Add(stateMachineContextCreate);

            var stateMachineAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(VariableName),
                new TtCreateObjectExpression(VariableType.TypeFullName));
            initMethod.MethodBody.Sequence.Add(stateMachineAssign);

            var centerDataAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("CenterData", new TtVariableReferenceExpression(VariableName)),
                new TtSelfReferenceExpression());
            initMethod.MethodBody.Sequence.Add(centerDataAssign);

            var stateMachineInitializeInvoke = new TtMethodInvokeStatement("Initialize",
                null, new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Default, Expression = new TtVariableReferenceExpression(stateMachineContext_VarName) });
            stateMachineInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(stateMachineInitializeInvoke);

            var baseInitializeInvoke = new TtMethodInvokeStatement("Initialize", null, new TtBaseReferenceExpression());
            baseInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(baseInitializeInvoke);
        }

        void GenerateCodeInMainClassAfterTickMethod(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            var afterTickMethod = classDeclaration.FindMethod("AfterTick");
            if (afterTickMethod == null)
            {
                afterTickMethod = TtASTBuildUtil.CreateAfterTickMethodDeclaration();
                classDeclaration.AddMethod(afterTickMethod);
            }
            var stateMachineContext_VarName = "stateMachine" + VariableName;

            var stateMachineContextCreate = TtASTBuildUtil.CreateVariableDeclaration(stateMachineContext_VarName,
                new TtTypeReference(typeof(TtStateMachineContext)),
                new TtCreateObjectExpression(typeof(TtStateMachineContext).FullName));
            afterTickMethod.MethodBody.Sequence.Add(stateMachineContextCreate);

            var stateMachineTickInvoke = new TtMethodInvokeStatement("Tick",
                null, new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Default, Expression = new TtVariableReferenceExpression("elapseSecond") },
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.In, Expression = new TtVariableReferenceExpression(stateMachineContext_VarName) });
            afterTickMethod.MethodBody.Sequence.Add(stateMachineTickInvoke);

        }
        public override void GenerateCodeInClass(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            base.GenerateCodeInClass(classDeclaration, ref classBuildContext);

            GenerateCodeInMainClassInitMethod(classDeclaration, ref classBuildContext);
            GenerateCodeInMainClassAfterTickMethod(classDeclaration, ref classBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod()
        {
            var methodDeclaration = TtStateMachineASTBuildUtil.CreateOverridedInitMethodStatement();

            foreach (var compoundState in CompoundStates)
            {
                TtAssignOperatorStatement compoundStateAssign = new();
                compoundStateAssign.To = new TtVariableReferenceExpression(compoundState.VariableName);
                compoundStateAssign.From = new TtCreateObjectExpression(compoundState.VariableType.TypeFullName);
                methodDeclaration.MethodBody.Sequence.Add(compoundStateAssign);

                TtAssignOperatorStatement stateMachineAssign = new();
                stateMachineAssign.To = new TtVariableReferenceExpression("StateMachine", new TtVariableReferenceExpression(compoundState.VariableName));
                stateMachineAssign.From = new TtSelfReferenceExpression();
                methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);

                TtASTBuildUtil.CreateCenterDataAssignStatement(compoundState, methodDeclaration);
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

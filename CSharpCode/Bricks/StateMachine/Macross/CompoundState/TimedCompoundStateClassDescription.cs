using EngineNS.Animation.Macross;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using System.ComponentModel;
using System.Diagnostics;

namespace EngineNS.Bricks.StateMachine.Macross.CompoundState
{
    [OutlineElement_Branch(typeof(TtOutlineElement_TimedCompoundStateGraph))]
    [Graph(typeof(TtGraph_TimedCompoundState))]
    public class TtTimedCompoundStateClassDescription : TtDesignableVariableDescription
    {
        [Rtti.Meta]
        public override string Name { get; set; } = "TimedStatesHub";
        [Rtti.Meta]
        [DrawInGraph]
        public TtTimedCompoundStateEntryClassDescription Entry { get; set; } = new TtTimedCompoundStateEntryClassDescription();
        [Rtti.Meta]
        [DrawInGraph]
        public List<TtTimedSubStateClassDescription> States { get; set; } = new List<TtTimedSubStateClassDescription>();
        [Rtti.Meta]
        [DrawInGraph]
        public List<TtTimedCompoundStateHubClassDescription> Hubs { get; set; } = new();
        public TtTimedStateMachineClassDescription StateMachineClassDescription { get=> Parent as TtTimedStateMachineClassDescription;  }
        public TtTimedCompoundStateClassDescription()
        {
            Entry.Parent = this;
        }
        public bool AddState(TtTimedSubStateClassDescription state)
        {
            States.Add(state);
            state.Parent = this;
            return true;
        }
        public bool RemoveState(TtTimedSubStateClassDescription state)
        {
            States.Remove(state);
            state.Parent = null;
            return true;
        }
        public bool AddHub(TtTimedCompoundStateHubClassDescription hub)
        {
            Hubs.Add(hub);
            hub.Parent = this;
            return true;
        }
        public bool RemoveHub(TtTimedCompoundStateHubClassDescription hub)
        {
            Hubs.Remove(hub);
            hub.Parent = null;
            return true;
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedCompoundState<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new List<TtClassDeclaration>();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            foreach (var state in States)
            {
                classDeclarationsBuilded.AddRange(state.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(state.BuildVariableDeclaration(ref classBuildContext));
            }
            foreach (var transition in Entry.Transitions)
            {
                classDeclarationsBuilded.AddRange(transition.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(transition.BuildVariableDeclaration(ref classBuildContext));
            }
            //foreach (var transition in Transitions_EndToThis)
            //{
            //    classDeclarationsBuilded.AddRange(transition.BuildClassDeclarations(ref classBuildContext));
            //    thisClassDeclaration.Properties.Add(transition.BuildVariableDeclaration(ref classBuildContext));
            //}
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod());
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod()
        {
            var methodDeclaration = TtStateMachineASTBuildUtil.CreateOverridedInitMethodStatement();

            bool bIsSetInitialActiveState = false;
            foreach (var state in States)
            {
                TtStateMachineASTBuildUtil.CreateNewAndInitInvokeStatement(state, methodDeclaration);

                var stateMachineAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                            new TtVariableReferenceExpression("StateMachine", new TtVariableReferenceExpression(state.VariableName)),
                                            new TtVariableReferenceExpression("mStateMachine"));
                methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);

                if (state.bInitialActive)
                {
                    Debug.Assert(!bIsSetInitialActiveState);
                    TtCastExpression castToSMExp = new TtCastExpression();
                    castToSMExp.TargetType = StateMachineClassDescription.VariableType;
                    castToSMExp.Expression = new TtVariableReferenceExpression("mStateMachine");
                    var setDefaultStateMethodInvoke = new TtMethodInvokeStatement();
                    setDefaultStateMethodInvoke.Host = castToSMExp;
                    setDefaultStateMethodInvoke.MethodName = "SetDefaultState";
                    setDefaultStateMethodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(state.VariableName) });
                    methodDeclaration.MethodBody.Sequence.Add(setDefaultStateMethodInvoke);
                }
            }


            foreach (var transition in Entry.Transitions)
            {
                var transitionFromVariableName = VariableName;
                var state_TransitionTo = States.Find((candidate) => { return candidate.Id == transition.ToId; });
                Debug.Assert(state_TransitionTo != null);
                var transitionToVariableName = state_TransitionTo.VariableName;

                var attachmentAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(transition.Name), new TtCreateObjectExpression(transition.VariableType.TypeFullName));
                methodDeclaration.MethodBody.Sequence.Add(attachmentAssign);

                var tansitionFromAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                            new TtVariableReferenceExpression("From", new TtVariableReferenceExpression(transition.VariableName)),
                                            new TtSelfReferenceExpression());
                methodDeclaration.MethodBody.Sequence.Add(tansitionFromAssign);

                var tansitionToAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                            new TtVariableReferenceExpression("To", new TtVariableReferenceExpression(transition.VariableName)),
                                            new TtVariableReferenceExpression(transitionToVariableName));
                methodDeclaration.MethodBody.Sequence.Add(tansitionToAssign);

                var hubAddTransionMethodInvoke = new TtMethodInvokeStatement();
                hubAddTransionMethodInvoke.Host = new TtSelfReferenceExpression();
                hubAddTransionMethodInvoke.MethodName = "AddTransition";
                hubAddTransionMethodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(transition.VariableName) });
                methodDeclaration.MethodBody.Sequence.Add(hubAddTransionMethodInvoke);
            }

            foreach (var state in States)
            {
                foreach (var transition in state.Transitions)
                {
                    var transitionFrom = States.Find((candidate) => { return candidate.Id == transition.FromId; });
                    Debug.Assert(transitionFrom != null);
                    var transitionFromVariableName = transitionFrom.VariableName;

                    TtExpressionBase tansitionToAssignFrom = null;
                    {
                        string transitionToVariableName = string.Empty;
                        var state_TransitionTo = States.Find((candidate) => { return candidate.Id == transition.ToId; });
                        if (state_TransitionTo != null)
                        {
                            transitionToVariableName = state_TransitionTo.VariableName;
                            tansitionToAssignFrom = new TtVariableReferenceExpression(transitionToVariableName);
                        }
                        else
                        {
                            var hub = Hubs.Find((candidate) => { return candidate.Id == transition.ToId; });
                            Debug.Assert(hub != null);
                            var compoundState_TransitionTo = StateMachineClassDescription.CompoundStates.Find((candidate) => { return candidate.Id == hub.TimedCompoundStateClassDescriptionId; });
                            if (compoundState_TransitionTo != null)
                            {
                                transitionToVariableName = compoundState_TransitionTo.VariableName;
                                TtCastExpression castToSMExp = new TtCastExpression();
                                castToSMExp.TargetType = StateMachineClassDescription.VariableType;
                                castToSMExp.Expression = new TtVariableReferenceExpression("mStateMachine");
                                tansitionToAssignFrom = new TtVariableReferenceExpression(transitionToVariableName, castToSMExp);
                            }
                        }
                        Debug.Assert(!string.IsNullOrEmpty(transitionToVariableName));
                    }

                    var tansitionDec = TtASTBuildUtil.CreateVariableDeclaration(transition.VariableName, transition.VariableType, new TtCreateObjectExpression(transition.VariableType.TypeFullName));
                    methodDeclaration.MethodBody.Sequence.Add(tansitionDec);

                    var tansitionFromAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                                    new TtVariableReferenceExpression("From", new TtVariableReferenceExpression(transition.VariableName)),
                                                    new TtVariableReferenceExpression(transitionFromVariableName));
                    methodDeclaration.MethodBody.Sequence.Add(tansitionFromAssign);

                    var tansitionToAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                                    new TtVariableReferenceExpression("To", new TtVariableReferenceExpression(transition.VariableName)),
                                                    tansitionToAssignFrom);
                    methodDeclaration.MethodBody.Sequence.Add(tansitionToAssign);

                    var stateAddTransionMethodInvoke = new TtMethodInvokeStatement();
                    stateAddTransionMethodInvoke.Host = new TtVariableReferenceExpression(state.VariableName);
                    stateAddTransionMethodInvoke.MethodName = "AddTransition";
                    stateAddTransionMethodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(transition.VariableName) });
                    methodDeclaration.MethodBody.Sequence.Add(stateAddTransionMethodInvoke);
                }
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

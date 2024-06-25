using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;

namespace EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState
{
    [OutlineElement_Branch(typeof(TtOutlineElement_AnimCompoundStateGraph))]
    [Graph(typeof(TtGraph_AnimCompoundState))]
    public class TtAnimCompoundStateClassDescription : TtTimedCompoundStateClassDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "TimedStatesHub";

        public TtAnimCompoundStateClassDescription()
        {
            Entry = new TtAnimCompoundStateEntryClassDescription();
            Entry.Parent = this;
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtAnimCompoundState<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
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
        private UMethodDeclaration BuildOverrideInitializeMethod()
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var contextMethodArgument = TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtAnimStateMachineContext>()), EMethodArgumentAttribute.Default);
            var args = new List<UMethodArgumentDeclaration>
            {
                contextMethodArgument
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true);



            bool bIsSetInitialActiveState = false;
            foreach (var state in States)
            {
                var subStateVarAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new UVariableReferenceExpression(state.VariableName), new UCreateObjectExpression(state.VariableType.TypeFullName));
                methodDeclaration.MethodBody.Sequence.Add(subStateVarAssign);


                var stateMachineAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                            new UVariableReferenceExpression("StateMachine", new UVariableReferenceExpression(state.VariableName)),
                                            new UVariableReferenceExpression("mStateMachine"));
                methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);

                if (state.bInitialActive)
                {
                    Debug.Assert(!bIsSetInitialActiveState);
                    UCastExpression castToSMExp = new UCastExpression();
                    castToSMExp.TargetType = StateMachineClassDescription.VariableType;
                    castToSMExp.Expression = new UVariableReferenceExpression("mStateMachine");
                    var setDefaultStateMethodInvoke = new UMethodInvokeStatement();
                    setDefaultStateMethodInvoke.Host = castToSMExp;
                    setDefaultStateMethodInvoke.MethodName = "SetDefaultState";
                    setDefaultStateMethodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression(state.VariableName) });
                    methodDeclaration.MethodBody.Sequence.Add(setDefaultStateMethodInvoke);
                }
            }


            foreach (var transition in Entry.Transitions)
            {
                var transitionFromVariableName = VariableName;
                var state_TransitionTo = States.Find((candidate) => { return candidate.Id == transition.ToId; });
                Debug.Assert(state_TransitionTo != null);
                var transitionToVariableName = state_TransitionTo.VariableName;

                var tansitionVarAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new UVariableReferenceExpression(transition.VariableName), new UCreateObjectExpression(transition.VariableType.TypeFullName));
                methodDeclaration.MethodBody.Sequence.Add(tansitionVarAssign);

                var tansitionFromAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                            new UVariableReferenceExpression("From", new UVariableReferenceExpression(transition.VariableName)),
                                            new USelfReferenceExpression());
                methodDeclaration.MethodBody.Sequence.Add(tansitionFromAssign);

                var tansitionToAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                            new UVariableReferenceExpression("To", new UVariableReferenceExpression(transition.VariableName)),
                                            new UVariableReferenceExpression(transitionToVariableName));
                methodDeclaration.MethodBody.Sequence.Add(tansitionToAssign);

                var hubAddTransionMethodInvoke = new UMethodInvokeStatement();
                hubAddTransionMethodInvoke.Host = new USelfReferenceExpression();
                hubAddTransionMethodInvoke.MethodName = "AddTransition";
                hubAddTransionMethodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression(transition.VariableName) });
                methodDeclaration.MethodBody.Sequence.Add(hubAddTransionMethodInvoke);
            }

            foreach (var state in States)
            {
                foreach (var transition in state.Transitions)
                {
                    var transitionFrom = States.Find((candidate) => { return candidate.Id == transition.FromId; });
                    Debug.Assert(transitionFrom != null);
                    var transitionFromVariableName = transitionFrom.VariableName;

                    UExpressionBase tansitionToAssignFrom = null;
                    {
                        string transitionToVariableName = string.Empty;
                        var state_TransitionTo = States.Find((candidate) => { return candidate.Id == transition.ToId; });
                        if (state_TransitionTo != null)
                        {
                            transitionToVariableName = state_TransitionTo.VariableName;
                            tansitionToAssignFrom = new UVariableReferenceExpression(transitionToVariableName);
                        }
                        else
                        {
                            var hub = Hubs.Find((candidate) => { return candidate.Id == transition.ToId; });
                            Debug.Assert(hub != null);
                            var compoundState_TransitionTo = StateMachineClassDescription.CompoundStates.Find((candidate) => { return candidate.Id == hub.TimedCompoundStateClassDescriptionId; });
                            if (compoundState_TransitionTo != null)
                            {
                                transitionToVariableName = compoundState_TransitionTo.VariableName;
                                UCastExpression castToSMExp = new UCastExpression();
                                castToSMExp.TargetType = StateMachineClassDescription.VariableType;
                                castToSMExp.Expression = new UVariableReferenceExpression("mStateMachine");
                                tansitionToAssignFrom = new UVariableReferenceExpression(transitionToVariableName, castToSMExp);
                            }
                        }
                        Debug.Assert(!string.IsNullOrEmpty(transitionToVariableName));
                    }

                    var tansitionVarAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new UVariableReferenceExpression(transition.VariableName), new UCreateObjectExpression(transition.VariableType.TypeFullName));
                    methodDeclaration.MethodBody.Sequence.Add(tansitionVarAssign);

                    var tansitionFromAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                                    new UVariableReferenceExpression("From", new UVariableReferenceExpression(transition.VariableName)),
                                                    new UVariableReferenceExpression(transitionFromVariableName));
                    methodDeclaration.MethodBody.Sequence.Add(tansitionFromAssign);

                    var tansitionToAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                                    new UVariableReferenceExpression("To", new UVariableReferenceExpression(transition.VariableName)),
                                                    tansitionToAssignFrom);
                    methodDeclaration.MethodBody.Sequence.Add(tansitionToAssign);

                    var stateAddTransionMethodInvoke = new UMethodInvokeStatement();
                    stateAddTransionMethodInvoke.Host = new UVariableReferenceExpression(state.VariableName);
                    stateAddTransionMethodInvoke.MethodName = "AddTransition";
                    stateAddTransionMethodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression(transition.VariableName) });
                    methodDeclaration.MethodBody.Sequence.Add(stateAddTransionMethodInvoke);
                }
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

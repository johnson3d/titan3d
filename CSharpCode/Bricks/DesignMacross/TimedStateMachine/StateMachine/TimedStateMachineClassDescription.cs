using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Outline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Linq;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [OutlineElement(typeof(TtOutlineElement_TimedStateMachine))]
    [Designable(typeof(TtTimedStateMachine), "TimedStateMachine")]
    public class TtTimedStateMachineClassDescription : IDesignableVariableDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "TimeStateMachine";
        public string VariableName { get => TtDescriptionUtil.VariableNamePrefix + Name; }
        public string ClassName { get => TtDescriptionUtil.ClassNamePrefix + Name; }
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        public UCommentStatement Comment { get; set; }
        public UTypeReference VariableType { get => new UTypeReference(ClassName); set { } }
        public UExpressionBase InitValue { get; set; }
        public UNamespaceDeclaration Namespace { get; set; }
        public bool IsStruct { get; set; }
        public List<string> SupperClassNames { get; set; } = new List<string>();
        public ObservableCollection<IVariableDescription> Variables { get; set; } = new ObservableCollection<IVariableDescription>();
        public ObservableCollection<IMethodDescription> Methods { get; set; } = new ObservableCollection<IMethodDescription>();

        [OutlineElementsList(typeof(TtOutlineElementsList_TimedStatesHubs))]
        public ObservableCollection<TtTimedStatesHubClassDescription> Hubs { get; set; } = new ObservableCollection<TtTimedStatesHubClassDescription>();

        public List<UClassDeclaration> BuildClassDeclarations()
        {
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            UClassDeclaration thisClassDeclaration = new UClassDeclaration();
            TtDescriptionUtil.BuildDefaultPartForClassDeclaration(this, ref thisClassDeclaration);

            foreach (var hub in Hubs)
            {
                classDeclarationsBuilded.AddRange(hub.BuildClassDeclarations());
                thisClassDeclaration.Properties.Add(hub.BuildVariableDeclaration());
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod());
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public UVariableDeclaration BuildVariableDeclaration()
        {
            return TtDescriptionUtil.BuildDefaultPartForVariableDeclaration(this);
        }

        #region Internal AST Build
        private UMethodDeclaration BuildOverrideInitializeMethod()
        {
            UMethodDeclaration methodDeclaration = new UMethodDeclaration();
            methodDeclaration.IsOverride = true;
            methodDeclaration.MethodName = "Initialize";
            foreach(var hub in Hubs)
            {
                UAssignOperatorStatement hubAssign = new();
                hubAssign.To = new UVariableReferenceExpression(hub.VariableName);
                hubAssign.From = new UCreateObjectExpression(hub.VariableType.TypeFullName);
                methodDeclaration.MethodBody.Sequence.Add(hubAssign);

                foreach(var state in hub.States)
                {
                    var hubStateVarDec = new UVariableDeclaration();
                    hubStateVarDec.VariableName = state.VariableName;
                    hubStateVarDec.VariableType = state.VariableType;
                    hubStateVarDec.InitValue = new UCreateObjectExpression(state.VariableType.TypeFullName);
                    methodDeclaration.MethodBody.Sequence.Add(hubStateVarDec);
                }
                foreach (var state in hub.States)
                {
                    foreach (var transition in state.Transitions)
                    {
                        var tansitionVarDec = new UVariableDeclaration();
                        tansitionVarDec.VariableName = transition.VariableName;
                        tansitionVarDec.VariableType = transition.VariableType;
                        tansitionVarDec.InitValue = new UCreateObjectExpression(transition.VariableType.TypeFullName);
                        methodDeclaration.MethodBody.Sequence.Add(tansitionVarDec);

                        UAssignOperatorStatement tansitionFromAssign = new();
                        tansitionFromAssign.To = new UVariableReferenceExpression("From", new UVariableReferenceExpression(state.VariableName));
                        tansitionFromAssign.From = new UVariableReferenceExpression(transition.From.Name);
                        methodDeclaration.MethodBody.Sequence.Add(tansitionFromAssign);

                        UAssignOperatorStatement tansitionToAssign = new();
                        tansitionToAssign.To = new UVariableReferenceExpression("To", new UVariableReferenceExpression(state.VariableName));
                        tansitionToAssign.From = new UVariableReferenceExpression(transition.To.Name);
                        methodDeclaration.MethodBody.Sequence.Add(tansitionToAssign);

                        var stateAddTransionMethodInvoke = new UMethodInvokeStatement();
                        stateAddTransionMethodInvoke.Host = new UVariableReferenceExpression(state.VariableName);
                        stateAddTransionMethodInvoke.MethodName = "AddTransition";
                        methodDeclaration.MethodBody.Sequence.Add(stateAddTransionMethodInvoke);
                    }
                }
                    
            }
            foreach (var hub in Hubs)
            {
                
                foreach (var transition in hub.Transitions_StartFromThis)
                {
                    var tansitionVarDec = new UVariableDeclaration();
                    tansitionVarDec.VariableName = transition.VariableName;
                    tansitionVarDec.VariableType = transition.VariableType;
                    tansitionVarDec.InitValue = new UCreateObjectExpression(transition.VariableType.TypeFullName);
                    methodDeclaration.MethodBody.Sequence.Add(tansitionVarDec);

                    UAssignOperatorStatement tansitionFromAssign = new();
                    tansitionFromAssign.To = new UVariableReferenceExpression("From", new UVariableReferenceExpression(hub.VariableName));
                    tansitionFromAssign.From = new UVariableReferenceExpression(transition.From.Name);
                    methodDeclaration.MethodBody.Sequence.Add(tansitionFromAssign);

                    UAssignOperatorStatement tansitionToAssign = new();
                    tansitionToAssign.To = new UVariableReferenceExpression("To", new UVariableReferenceExpression(hub.VariableName));
                    tansitionToAssign.From = new UVariableReferenceExpression(transition.To.Name);
                    methodDeclaration.MethodBody.Sequence.Add(tansitionToAssign);

                    var hubAddTransionMethodInvoke = new UMethodInvokeStatement();
                    hubAddTransionMethodInvoke.Host = new UVariableReferenceExpression(hub.VariableName);
                    hubAddTransionMethodInvoke.MethodName = "AddTransition";
                    methodDeclaration.MethodBody.Sequence.Add(hubAddTransionMethodInvoke);
                }

                foreach (var tansition in hub.Transitions_EndToThis)
                {

                }
            }
            return methodDeclaration;
        }
        #endregion
    }
}

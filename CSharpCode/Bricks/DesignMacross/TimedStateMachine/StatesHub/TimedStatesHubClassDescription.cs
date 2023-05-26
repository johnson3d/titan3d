using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Outline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [OutlineElement(typeof(TtOutlineElement_TimedStatesHubGraph))]
    [GraphElement(typeof(TtGraph_TimedStatesHub))]
    public class TtTimedStatesHubClassDescription : IDesignableVariableDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "TimedStatesHub";
        public string VariableName { get => TtDescriptionUtil.VariableNamePrefix + Name; }
        public string ClassName { get => TtDescriptionUtil.ClassNamePrefix + Name; }

        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        public UCommentStatement Comment { get; set; }
        public UTypeReference VariableType { get => new UTypeReference(ClassName); set { } }
        public UExpressionBase InitValue { get; set; }
        public UNamespaceDeclaration Namespace { get; set; }
        public bool IsStruct { get; set; } = false;
        public List<string> SupperClassNames { get; set; } = new List<string>();
        public ObservableCollection<IVariableDescription> Variables { get; set; } = new ObservableCollection<IVariableDescription>();
        public ObservableCollection<IMethodDescription> Methods { get; set; } = new ObservableCollection<IMethodDescription>();
        public ObservableCollection<TtTimedStateClassDescription> States { get; set; } = new ObservableCollection<TtTimedStateClassDescription>();
        public ObservableCollection<TtTimedStatesHubBridgeClassDescription> StatesHubBridges { get; set; } = new();
        public ObservableCollection<TtTimedStateTransitionClassDescription> Transitions_StartFromThis = new();
        public ObservableCollection<TtTimedStateTransitionClassDescription> Transitions_EndToThis = new();
        public TtTimedStateMachineClassDescription StateMachineClassDescription { get; set; } = null;
        public UVariableDeclaration BuildVariableDeclaration()
        {
            return TtDescriptionUtil.BuildDefaultPartForVariableDeclaration(this);
        }

        public List<UClassDeclaration> BuildClassDeclarations()
        {
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            UClassDeclaration thisClassDeclaration = new UClassDeclaration();
            TtDescriptionUtil.BuildDefaultPartForClassDeclaration(this, ref thisClassDeclaration);
            foreach (var state in States)
            {
                classDeclarationsBuilded.AddRange(state.BuildClassDeclarations());
                thisClassDeclaration.Properties.Add(state.BuildVariableDeclaration());
            }
            foreach (var transition in Transitions_StartFromThis)
            {
                classDeclarationsBuilded.AddRange(transition.BuildClassDeclarations());
                thisClassDeclaration.Properties.Add(transition.BuildVariableDeclaration());
            }
            foreach (var transition in Transitions_EndToThis)
            {
                classDeclarationsBuilded.AddRange(transition.BuildClassDeclarations());
                thisClassDeclaration.Properties.Add(transition.BuildVariableDeclaration());
            }
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }
    }
}

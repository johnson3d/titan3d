using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [GraphElement(typeof(TtGraphElement_TimedStateTransition))]
    public class TtTimedStateTransitionClassDescription : IDesignableVariableDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
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

        public IDescription From { get; set; }
        public IDescription To { get; set; }
        public List<UClassDeclaration> BuildClassDeclarations()
        {
            UClassDeclaration thisClassDeclaration = new UClassDeclaration();
            TtDescriptionUtil.BuildDefaultPartForClassDeclaration(this, ref thisClassDeclaration);
            return new List<UClassDeclaration>() {  thisClassDeclaration };
        }

        public UVariableDeclaration BuildVariableDeclaration()
        {
            return TtDescriptionUtil.BuildDefaultPartForVariableDeclaration(this);
        }
    }
}

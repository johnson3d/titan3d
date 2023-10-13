using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.TimedStateMachine.StateTransition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [Graph(typeof(TtGraph_TimedStateTransition))]
    [GraphElement(typeof(TtGraphElement_TimedStateTransition))]
    public class TtTimedStateTransitionClassDescription : IDesignableVariableDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        string mName = null;
        public string Name
        { 
            get
            {
                if (!string.IsNullOrEmpty(mName))
                    return mName;
                if(From != null && To != null)
                    return "transiton_from_" + From.Name + "_to_" + To.Name;
                return null;
            }
            set
            {
                mName = value;
            }
        }
        [Rtti.Meta]
        public Vector2 Location { get; set; }
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

        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}

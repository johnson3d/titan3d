using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Collections.Specialized;
using EngineNS.DesignMacross.TimedStateMachine;
using EngineNS.DesignMacross.Base.Description;

namespace EngineNS.DesignMacross.Design
{
    public class TtClassDescription : IClassDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "Class";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public string ClassName { get => TtDescriptionUtil.ClassNamePrefix + Name; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public UNamespaceDeclaration Namespace { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public bool IsStruct { get; set; } = false;
        [Rtti.Meta]
        public List<string> SupperClassNames { get; set; } = new List<string>();
        [Rtti.Meta]
        public ObservableCollection<IVariableDescription> Variables { get; set; } = new ObservableCollection<IVariableDescription>();
        [Rtti.Meta]
        public ObservableCollection<IMethodDescription> Methods { get; set; } = new ObservableCollection<IMethodDescription>();
        [OutlineElementsList(typeof(TtOutlineElementsList_DesignableVariables))]
        [Rtti.Meta]
        public ObservableCollection<IDesignableVariableDescription> DesignableVariables { get; set; } = new ObservableCollection<IDesignableVariableDescription>();

        public List<UClassDeclaration> BuildClassDeclarations()
        {
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            UClassDeclaration thisClassDeclaration = new UClassDeclaration();
            TtDescriptionUtil.BuildDefaultPartForClassDeclaration(this, ref thisClassDeclaration);
            foreach (var designVarDesc in DesignableVariables)
            {
                classDeclarationsBuilded.AddRange(designVarDesc.BuildClassDeclarations());
                thisClassDeclaration.Properties.Add(designVarDesc.BuildVariableDeclaration());
            }
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
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

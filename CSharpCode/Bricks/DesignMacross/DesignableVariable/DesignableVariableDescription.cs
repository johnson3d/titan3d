using EngineNS.Bricks.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EngineNS.DesignMacross.Description
{
    public class TtDesignableVariableDescription : IClassDescription, IVariableDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "DesignableVariable";
        public string VariableName { get => TtDescriptionUtil.VariableNamePrefix + Name; }
        public string ClassName { get => TtDescriptionUtil.ClassNamePrefix + Name; }
        [Rtti.Meta]
        public UTypeReference VariableType { get => new UTypeReference(ClassName); set { } }
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public IClassDescription DesignedClassDescription { get; set; }
        public UNamespaceDeclaration Namespace { get; set; }
        public bool IsStruct { get; set; }
        public List<string> SupperClassNames { get; set; } = new List<string>();
        public ObservableCollection<IVariableDescription> Variables { get; set; } = new ObservableCollection<IVariableDescription>();
        public ObservableCollection<IMethodDescription> Methods { get; set; } = new ObservableCollection<IMethodDescription>();

        public List<UClassDeclaration> BuildClassDeclarations()
        {
            List<UClassDeclaration> classDeclarationsBuilded = new();
            classDeclarationsBuilded.AddRange(DesignedClassDescription.BuildClassDeclarations());
            return classDeclarationsBuilded;
        }

        public UVariableDeclaration BuildVariableDeclaration()
        {
            return TtDescriptionUtil.BuildDefaultPartForVariableDeclaration(this);
        }
    }
}

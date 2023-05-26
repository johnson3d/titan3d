using EngineNS.Bricks.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EngineNS.DesignMacross.Description
{
    public interface IDescription
    {
        public Guid Id { get; set; }
        public string Name { get; set; }


    }
    public interface IClassDescription : IDescription
    {
        public string ClassName { get; }
        public EVisisMode VisitMode { get; set; }
        public UCommentStatement Comment { get; set; }
        public UNamespaceDeclaration Namespace { get; set; }
        public bool IsStruct { get; set; } 
        public List<string> SupperClassNames { get; set; } 
        public ObservableCollection<IVariableDescription> Variables { get; set; } 
        public ObservableCollection<IMethodDescription> Methods { get; set; }
        public List<UClassDeclaration> BuildClassDeclarations();
    }

    public interface IVariableDescription : IDescription
    {
        public string VariableName { get; }
        public EVisisMode VisitMode { get; set; }
        public UCommentStatement Comment { get; set; }
        public UTypeReference VariableType { get; set; }
        public UExpressionBase InitValue { get; set; }
        public UVariableDeclaration BuildVariableDeclaration();
    }

    public interface IMethodDescription : IDescription
    {
        public EVisisMode VisitMode { get; set; }
        public UCommentStatement Comment { get; set; }
        public UVariableDeclaration ReturnValue { get; set; }
        public string MethodName { get; }
        public List<UMethodArgumentDeclaration> Arguments { get; set; } 
        public List<UVariableDeclaration> LocalVariables { get; set; } 
        public bool IsOverride { get; set; } 
        public bool IsAsync { get; set; }
        public UMethodDeclaration BuildMethodDeclaration();
    }

    public interface IDesignableVariableDescription : IVariableDescription, IClassDescription
    {

    }
}

using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Base.Description
{
    public struct FClassBuildContext
    {
        public IClassDescription MainClassDescription { get; set; } 
    }
    public interface IDescription  : IO.ISerializer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IDescription Parent { get; set; }
    }
    public interface IClassDescription : IDescription
    {
        public string ClassName { get; }
        public EVisisMode VisitMode { get; set; }
        public UCommentStatement Comment { get; set; }
        public UNamespaceDeclaration Namespace { get; set; }
        public bool IsStruct { get; set; } 
        public List<string> SupperClassNames { get; set; } 
        public List<IVariableDescription> Variables { get; set; } 
        public List<IMethodDescription> Methods { get; set; }
        public List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext);
    }

    public interface IVariableDescription : IDescription
    {
        public string VariableName { get; }
        public EVisisMode VisitMode { get; set; }
        public UCommentStatement Comment { get; set; }
        public UTypeReference VariableType { get; set; }
        public UExpressionBase InitValue { get; set; }
        public UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext);
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
        public UMethodDeclaration BuildMethodDeclaration(ref FClassBuildContext classBuildContext);
    }

    public interface IDesignableVariableDescription : IVariableDescription, IClassDescription
    {

    }
}

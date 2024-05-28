using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Base.Description
{
    public struct FClassBuildContext
    {
        public IClassDescription MainClassDescription { get; set; } 
        public UClassDeclaration ClassDeclaration { get; set; }
    }

    public struct FMethodBuildContext
    {
        public IMethodDescription MethodDescription { get; set; } 
        public UMethodDeclaration MethodDeclaration { get; set; }
    }

    public struct FStatementBuildContext
    {
        public IMethodDescription MethodDescription { get; set; }
        public UExecuteSequenceStatement ExecuteSequenceStatement { get; set; }
        public void AddStatement(UStatementBase statement)
        {
            if (ExecuteSequenceStatement == null)
                return;
            ExecuteSequenceStatement.Sequence.Add(statement);
        }
    }
    public struct FExpressionBuildContext
    {
        public IMethodDescription MethodDescription { get; set; }
        public UExecuteSequenceStatement Sequence { get; set; }
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
        public UTypeDesc ReturnValueType { get; set; }
        public string MethodName { get; }
        public List<TtMethodArgumentDescription> Arguments { get; set; } 
        public List<TtVariableDescription> LocalVariables { get; set; } 
        public bool IsOverride { get; set; } 
        public bool IsAsync { get; set; }
        public UMethodDeclaration BuildMethodDeclaration(ref FClassBuildContext classBuildContext);
    }

    public interface IDesignableVariableDescription : IVariableDescription, IClassDescription
    {

    }

    public interface IExpressionDescription : IDescription
    {
        public UExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext);
    }
    public interface IStatementDescription : IDescription
    {
        public UStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext);
    }
}

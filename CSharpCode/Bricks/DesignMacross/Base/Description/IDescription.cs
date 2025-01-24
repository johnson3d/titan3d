using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Base.Description
{
    public struct FClassBuildContext
    {
        public IClassDescription MainClassDescription;
        public IClassDescription ClassDescription;
        public TtClassDeclaration ClassDeclaration;
    }

    //public struct FMethodBuildContext
    //{
    //    public FClassBuildContext ClassBuildContext;
    //    public IMethodDescription MethodDescription { get; set; } 
    //    public TtMethodDeclaration MethodDeclaration { get; set; }
    //}

    public struct FStatementBuildContext
    {
        public FClassBuildContext ClassBuildContext;
        public IMethodDescription MethodDescription;
        public TtExecuteSequenceStatement ExecuteSequenceStatement { get; set; }
        public void AddStatement(TtStatementBase statement)
        {
            if (ExecuteSequenceStatement == null)
                return;
            ExecuteSequenceStatement.Sequence.Add(statement);
        }
    }
    public struct FExpressionBuildContext
    {
        public FClassBuildContext ClassBuildContext;
        public IMethodDescription MethodDescription;
        public TtExecuteSequenceStatement Sequence;
        public TtDataPinDescription SelfPin;
        public TtDataPinDescription TargetPin;
    }
    public struct FDescriptionUpdateContext
    {
        public IClassDescription ClassDescription;
    }
    public interface IDescription  : IO.ISerializer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IDescription Parent { get; set; }
        public void UpdateData(ref FDescriptionUpdateContext updateContext);
    }
    public interface IClassDescription : IDescription
    {
        public string ClassName { get; }
        public EVisisMode VisitMode { get; set; }
        public TtCommentStatement Comment { get; set; }
        public TtNamespaceDeclaration Namespace { get; set; }
        public bool IsStruct { get; set; } 
        public List<string> SupperClassNames { get; set; } 
        public List<IVariableDescription> Variables { get; set; } 
        public List<IMethodDescription> Methods { get; set; }
        public List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext);
    }

    public interface IVariableDescription : IDescription
    {
        public string VariableName { get; }
        public EVisisMode VisitMode { get; set; }
        public TtCommentStatement Comment { get; set; }
        public TtTypeReference VariableType { get; set; }
        public TtExpressionBase InitValue { get; set; }
        public TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext);
    }

    public interface IMethodDescription : IDescription
    {
        public EVisisMode VisitMode { get; set; }
        public TtCommentStatement Comment { get; set; }
        public TtTypeDesc ReturnValueType { get; set; }
        public string MethodName { get; }
        public List<TtMethodArgumentDescription> Arguments { get; set; } 
        public List<TtVariableDescription> LocalVariables { get; set; } 
        public bool IsOverride { get; set; } 
        public TtMethodDeclaration.EAsyncType AsyncType { get; set; }
        public TtMethodDeclaration BuildMethodDeclaration(ref FClassBuildContext classBuildContext);
        public TtExecutionPinDescription GetLinkedExecutionPin(TtExecutionPinDescription execPin);
        public TtDataPinDescription GetLinkedDataPin(TtDataPinDescription dataPin);
        public TtDataLineDescription GetDataLineWithPin(TtDataPinDescription dataPin);
        public TtExecutionLineDescription GetExecutionLineWithPin(TtExecutionPinDescription execPin);
    }

    public interface IDesignableVariableDescription : IVariableDescription, IClassDescription
    {
        public void GenerateCodeInClass(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext);
    }

    public interface IExpressionDescription : IDescription
    {
        public TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext);
    }
    public interface IStatementDescription : IDescription
    {
        public TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext);
    }
    public class TtPinsCheckContext
    {
        public IMethodDescription MethodDescription { get; set; }
        public List<IDescription> ErrorDescriptions { get; set; } = new();
    }
}

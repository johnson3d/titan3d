using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using System.Diagnostics;
using System.Reflection;
using EngineNS.DesignMacross.Design.Statement;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System.Linq.Expressions;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design
{
    public partial class TtMethodArgumentDescription : IDescription
    {
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public UTypeDesc VariableType { get; set; } = Rtti.UTypeDesc.TypeOf<int>();
        [Rtti.Meta]
        public EMethodArgumentAttribute OperationType { get; set; } = EMethodArgumentAttribute.Default;
        public Guid Id { get; set; } = Guid.NewGuid();
        public IDescription Parent { get; set; } = null;

        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            if (hostObject is IDescription parentDescription)
            {
                Parent = parentDescription;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer 
    }

    [GraphElement(typeof(TtGraphElement_MethodStartDescription), 20, 400)]
    public partial class TtMethodStartDescription : TtStatementDescription
    {
        public override string Name { get => Parent.Name; set => Parent.Name = value; }
        public TtMethodStartDescription()
        {
            AddExecutionOutPin(new());
        }
        public override UStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var methodDesc = statementBuildContext.MethodDescription as TtMethodDescription;
            var executionOutPin = ExecutionOutPins[0];
            var linkedExecPin = methodDesc.GetLinkedExecutionPin(executionOutPin);
            if(linkedExecPin != null)
            {
                System.Diagnostics.Debug.Assert(linkedExecPin is TtExecutionInPinDescription);
                FStatementBuildContext buildContext = new() { ExecuteSequenceStatement = new(), MethodDescription = statementBuildContext.MethodDescription };
                var statement = (linkedExecPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                statementBuildContext.AddStatement(buildContext.ExecuteSequenceStatement);
                return statement;
            }
            System.Diagnostics.Debug.Assert(false);
            return null;
        }
    }
    [GraphElement(typeof(TtGraphElement_MethodEndDescription), 500, 400)]
    public partial class TtMethodEndDescription : TtStatementDescription
    {
        public TtMethodEndDescription()
        {
            Name = "Return";
            AddExecutionInPin(new());
        }
    }

    [Graph(typeof(TtGraph_Method))]
    [OutlineElement_Leaf(typeof(TtOutlineElement_Method))]
    public partial class TtMethodDescription : IMethodDescription, Bricks.NodeGraph.UEditableValue.IValueEditNotify
    {
        public IDescription Parent { get; set; }
        public virtual string MethodName { get=> TtDescriptionASTBuildUtil.GenerateMethodName(this);}
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "Method";
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public bool IsOverride { get; set; } = false;
        [Rtti.Meta]
        public bool IsAsync { get; set; } = false;
        [Rtti.Meta]
        public List<TtMethodArgumentDescription> Arguments { get; set; } = new List<TtMethodArgumentDescription>();
        [Rtti.Meta]
        public List<TtVariableDescription> LocalVariables { get; set; } = new List<TtVariableDescription>();
        [Rtti.Meta]
        public UTypeDesc ReturnValueType { get; set; } = null;
        [Rtti.Meta, DrawInGraph]
        public TtMethodStartDescription Start { get; set; } = null;
        [Rtti.Meta, DrawInGraph]
        public List<TtExpressionDescription> Expressions { get; set; } = new();
        [Rtti.Meta, DrawInGraph]
        public List<TtStatementDescription> Statements { get; set; } = new();
        [Rtti.Meta, DrawInGraph]
        public List<TtExecutionLineDescription> ExecutionLines { get; set; } = new();
        [Rtti.Meta, DrawInGraph]
        public List<TtDataLineDescription> DataLines { get; set; } = new();

        public void AddArgument(TtMethodArgumentDescription argument)
        {
            Arguments.Add(argument);
            argument.Parent = this;
            Start.AddDataOutPin(new() { Id = argument.Id, Name = argument.Name, TypeDesc = argument.VariableType });
        }
        public bool RemoveArgument(TtMethodArgumentDescription argument)
        {
            Arguments.Remove(argument);
            argument.Parent = null;
            Start.RemoveDataOutPin(argument.Name);
            return true;
        }

        public void SetReturnValue(UTypeDesc returnValueType)
        {
            if (returnValueType == null)
                return;

            ReturnValueType = ReturnValueType;
            var returnDesc = new TtReturnStatementDescription();
            returnDesc.SetReturnType(returnValueType);
            AddStatement(returnDesc);
        }

        public void AddExpression(TtExpressionDescription expression)
        {
            Expressions.Add(expression);
            expression.Parent = this;
        }
        public bool RemoveExpression(TtExpressionDescription expression)
        {
            Expressions.Remove(expression);
            expression.Parent = null;
            return true;
        }
        public void AddStatement(TtStatementDescription statement)
        {
            Statements.Add(statement);
            statement.Parent = this;
        }
        public bool RemoveStatement(TtStatementDescription statement)
        {
            Statements.Remove(statement);
            statement.Parent = null;
            return true;
        }
        public void AddExecutionLine(TtExecutionLineDescription executionLine)
        {
            ExecutionLines.Add(executionLine);
            executionLine.Parent = this;
        }

        public bool RemoveExecutionLine(TtExecutionLineDescription executionLine)
        {
            ExecutionLines.Remove(executionLine);
            executionLine.Parent = null;
            return true;
        }
        public void AddDataLine(TtDataLineDescription dataLine)
        {
            DataLines.Add(dataLine);
            dataLine.Parent = this;
        }

        public bool RemoveDataLine(TtDataLineDescription dataLine)
        {
            DataLines.Remove(dataLine);
            dataLine.Parent = null;
            return true;
        }
        
        public TtMethodDescription()
        {
            Start = new() { Parent = this };
        }
        public virtual UMethodDeclaration BuildMethodDeclaration(ref FClassBuildContext classBuildContext)
        {
            UMethodDeclaration declaration = new UMethodDeclaration();
            TtDescriptionASTBuildUtil.BuildDefaultPartForMethodDeclaration(this, ref declaration, ref classBuildContext);
            FStatementBuildContext buildContext = new() { ExecuteSequenceStatement = new(), MethodDescription = this };
            Start.BuildStatement(ref buildContext);
            declaration.MethodBody.Sequence.Add(buildContext.ExecuteSequenceStatement);
            return declaration;
        }

        public TtDataPinDescription GetLinkedDataPin(TtDataPinDescription dataPin)
        {
            var linkedPinId = Guid.Empty;
            foreach (var dataLine in DataLines)
            {
                if(dataLine.FromId == dataPin.Id)
                {
                    linkedPinId = dataLine.ToId;
                    break;
                }
                if(dataLine.ToId == dataPin.Id)
                {
                    linkedPinId = dataLine.FromId;
                    break;
                }
            }
            foreach(var expression in Expressions)
            {
                if(expression.TryGetDataPin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            foreach(var statement in Statements)
            {
                if (statement.TryGetDataPin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            return null;
        }
        public TtExecutionPinDescription GetLinkedExecutionPin(TtExecutionPinDescription execPin)
        {
            var linkedPinId = Guid.Empty;
            foreach (var executeLine in ExecutionLines)
            {
                if (executeLine.FromId == execPin.Id)
                {
                    linkedPinId = executeLine.ToId;
                    break;
                }
                if (executeLine.ToId == execPin.Id)
                {
                    linkedPinId = executeLine.FromId;
                    break;
                }
            }
            foreach (var expression in Expressions)
            {
                if (expression.TryGetExecutePin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            foreach (var statement in Statements)
            {
                if (statement.TryGetExecutePin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            
            return null;
        }

        public void OnValueChanged(UEditableValue ev)
        {
            var v = ev;
        }

        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            if (hostObject is IDescription parentDescription)
            {
                Parent = parentDescription;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}

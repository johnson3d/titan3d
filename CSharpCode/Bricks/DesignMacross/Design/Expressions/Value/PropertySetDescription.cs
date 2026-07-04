using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarSet))]
    public class TtPropertySetDescription : TtStatementDescription
    {
        public Guid HostReferenceId { get; set; } = Guid.Empty;
        public TtTypeDesc VarTypeDesc { get; set; } = null;
        public TtTypeDesc HostReferenceTypeDesc { get; set; } = null;

        public TtPropertySetDescription()
        {
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDataInPin(new() { Name = "Host", TypeDesc = HostReferenceTypeDesc });
            AddDataInPin(new() { Name = "Set", TypeDesc = VarTypeDesc });
            AddDataOutPin(new() { Name = "Get", TypeDesc = VarTypeDesc });
        }
        public TtPropertySetDescription(TtTypeDesc hostReferenceTypeDesc, TtTypeDesc varTypeDesc)
        {
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDataInPin(new() { Name = "Host", TypeDesc = hostReferenceTypeDesc });
            AddDataInPin(new() { Name = "Set", TypeDesc = varTypeDesc });
            AddDataOutPin(new() { Name = "Get", TypeDesc = varTypeDesc });
        }
        public TtDataInPinDescription GetHostPin()
        {
            return DataInPins[0];
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var graphDesc = statementBuildContext.OwnerDescription;
            
            TtExpressionBase hostExp = null;
            var hostPin = DataInPins[0];
            if (hostPin != null)
            {
                var linkedHostPin = IDataLineOperator.GetLinkedDataPin(graphDesc,hostPin);
                var buildContext = new FExpressionBuildContext() { OwnerDescription = statementBuildContext.OwnerDescription, Sequence = statementBuildContext.ExecuteSequenceStatement, ClassBuildContext = statementBuildContext.ClassBuildContext };
                var linkedDesc = linkedHostPin.Parent;
                if (linkedDesc is TtExpressionDescription linkedExpressionDesc)
                {
                    hostExp = linkedExpressionDesc.BuildExpression(ref buildContext);
                }
                if (linkedDesc is TtStatementDescription linkedStatementDesc)
                {
                    hostExp = linkedStatementDesc.BuildExpressionForOutPin(linkedHostPin);
                }
            }
            var leftSideExp = new TtVariableReferenceExpression(Name, hostExp);

            TtExpressionBase rightSideExp = null;
            var otherInPin = DataInPins[1];
            var linkedDataPin = IDataLineOperator.GetLinkedDataPin(graphDesc, otherInPin);
            if (linkedDataPin == null)
            {
                //TODO: TtMethodInvokeReflectedDescription 要报错
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                var buildContext = new FExpressionBuildContext() { OwnerDescription = statementBuildContext.OwnerDescription, Sequence = statementBuildContext.ExecuteSequenceStatement, ClassBuildContext = statementBuildContext.ClassBuildContext };
                var linkedDesc = linkedDataPin.Parent;
                if (linkedDesc is TtExpressionDescription linkedExpressionDesc)
                {
                    rightSideExp = linkedExpressionDesc.BuildExpression(ref buildContext);
                }
                if (linkedDesc is TtStatementDescription linkedStatementDesc)
                {
                    rightSideExp = linkedStatementDesc.BuildExpressionForOutPin(linkedDataPin);
                }
            }

            var propertySetStatement = TtASTBuildUtil.CreateAssignOperatorStatement(leftSideExp, rightSideExp);
            statementBuildContext.AddStatement(propertySetStatement);
            var executionOutPin = ExecutionOutPins[0];
            var linkedExecPin = IExecutionLineOperator.GetLinkedExecutionPin(graphDesc,executionOutPin);
            if (linkedExecPin == null)
            {
                //空语句
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedExecPin is TtExecutionInPinDescription);
                (linkedExecPin.Parent as TtStatementDescription).BuildStatement(ref statementBuildContext);
            }
            return propertySetStatement;
        }
        public override TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin)
        {
            return base.BuildExpressionForOutPin(pin);
        }
    }
}

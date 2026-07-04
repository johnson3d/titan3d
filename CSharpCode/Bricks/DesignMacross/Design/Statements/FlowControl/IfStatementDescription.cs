using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Statement
{
    [GraphElement(typeof(TtGraphElement_StatementDescription))]
    [ContextMenu("If", "FlowControl\\Branch", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtIfStatementDescription : TtStatementDescription
    {
        public TtIfStatementDescription() 
        {
            Name = "Branch";
            AddExecutionInPin(new());
            AddExecutionOutPin(new() { Name = "True" });
            AddExecutionOutPin(new() { Name = "False" });
            AddDataInPin(new() { TypeDesc = TtTypeDesc.TypeOf<bool>()});
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var ifStatement = new TtIfStatement();
            var graphDesc = statementBuildContext.OwnerDescription;          
            
            var linkedDataPin = IDataLineOperator.GetLinkedDataPin(graphDesc, DataInPins[0]);
            if(linkedDataPin == null)
            {
                //默认为false
                ifStatement.Condition = new TtPrimitiveExpression(false);
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { OwnerDescription = statementBuildContext.OwnerDescription, ClassBuildContext = statementBuildContext.ClassBuildContext };
                var condition = (linkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                ifStatement.Condition = condition;
            }

            var executionOutPin_True = ExecutionOutPins[0];
            var linkedTrueExecPin = IExecutionLineOperator.GetLinkedExecutionPin(graphDesc, executionOutPin_True);
            if(linkedTrueExecPin == null)
            {
                //空语句
                //ifStatement.TrueStatement = new UEmptyStatement()

                ifStatement.TrueStatement = new TtExecuteSequenceStatement();
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedTrueExecPin is TtExecutionInPinDescription);
                FStatementBuildContext buildContext = new() {
                    ExecuteSequenceStatement = new(),
                    OwnerDescription = statementBuildContext.OwnerDescription,
                    ClassBuildContext = statementBuildContext.ClassBuildContext
                };
                (linkedTrueExecPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                ifStatement.TrueStatement = buildContext.ExecuteSequenceStatement;
            }
            var executionOutPin_False = ExecutionOutPins[1];
            var linkedFalseExecPin = IExecutionLineOperator.GetLinkedExecutionPin(graphDesc, executionOutPin_False);
            if(linkedFalseExecPin == null)
            {
                //空
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedFalseExecPin is TtExecutionInPinDescription);
                FStatementBuildContext buildContext = new() {
                    ExecuteSequenceStatement = new(), 
                    OwnerDescription = statementBuildContext.OwnerDescription,
                    ClassBuildContext = statementBuildContext.ClassBuildContext
                };
                (linkedFalseExecPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                ifStatement.FalseStatement = buildContext.ExecuteSequenceStatement;
            }
            statementBuildContext.AddStatement(ifStatement);
            return ifStatement;
        }
    }
}

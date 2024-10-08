using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design.Statements
{
    [GraphElement(typeof(TtGraphElement_StatementDescription))]
    [ContextMenu("While", "FlowControl\\While", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtWhileLoopStatementDescription : TtStatementDescription
    {
        public TtWhileLoopStatementDescription()
        {
            Name = "While";
            AddExecutionInPin(new());
            AddDataInPin(new() { Name = "Condition", TypeDesc = TtTypeDesc.TypeOf<bool>() });
            AddExecutionOutPin(new());
            AddExecutionOutPin(new() { Name = "Loop Body" });
        }

        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var whileStatement = new TtWhileLoopStatement();
            var methodDesc = statementBuildContext.MethodDescription as TtMethodDescription;

            var linkedDataPin = methodDesc.GetLinkedDataPin(DataInPins[0]);
            if (linkedDataPin == null)
            {
                // 默认为false
                whileStatement.Condition = new TtPrimitiveExpression(false);
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                var buildContext = new FExpressionBuildContext() { MethodDescription = statementBuildContext.MethodDescription };
                var condition = (linkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                whileStatement.Condition = condition;
            }

            var executionOutPin_LoopBody = ExecutionOutPins[1];
            var linkedLoopBodyExecPin = methodDesc.GetLinkedExecutionPin(executionOutPin_LoopBody);
            if (linkedLoopBodyExecPin == null)
            {
                whileStatement.LoopBody = new TtExecuteSequenceStatement();
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedLoopBodyExecPin is TtExecutionInPinDescription);
                var buildContext = new FStatementBuildContext()
                {
                    ExecuteSequenceStatement = new(),
                    MethodDescription = statementBuildContext.MethodDescription,
                };
                whileStatement.LoopBody = (linkedLoopBodyExecPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
            }
            statementBuildContext.AddStatement(whileStatement);

            var executionOutPin_Next = ExecutionOutPins[0];
            var linkedNextPin = methodDesc.GetLinkedExecutionPin(executionOutPin_Next);
            if(linkedNextPin != null)
            {
                System.Diagnostics.Debug.Assert(linkedNextPin is TtExecutionInPinDescription);
                (linkedNextPin.Parent as TtStatementDescription).BuildStatement(ref statementBuildContext);
            }

            return whileStatement;
        }
    }
}

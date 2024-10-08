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
    [ContextMenu("For", "FlowControl\\For", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtForLoopStatementDescription : TtStatementDescription
    {
        [Rtti.Meta]
        public bool IncludeEnd { get; set; } = true;

        public TtForLoopStatementDescription()
        {
            Name = "For";
            AddExecutionInPin(new());
            AddDataInPin(new() { Name = "BeginIdx", TypeDesc = TtTypeDesc.TypeOf<int>() });
            AddDataInPin(new() { Name = "EndIdx", TypeDesc = TtTypeDesc.TypeOf<int>() });
            AddDataInPin(new() { Name = "Step", TypeDesc = TtTypeDesc.TypeOf<int>() });

            AddExecutionOutPin(new());
            AddExecutionOutPin(new() { Name = "Loop Body" });
            AddDataOutPin(new() { Name = "Index", TypeDesc = TtTypeDesc.TypeOf<int>() });
        }

        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var forStatement = new TtForLoopStatement()
            {
                LoopIndexName = "__loopIndex_" + this.Id.ToString().GetHashCode(),
                IncludeEnd = IncludeEnd,
            };
            var methodDesc = statementBuildContext.MethodDescription as TtMethodDescription;

            var linkedDataPin = methodDesc.GetLinkedDataPin(DataInPins[0]);
            if (linkedDataPin == null)
            {
                // Begin index default 0
                forStatement.BeginExpression = new TtPrimitiveExpression(0);
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                var buildContext = new FExpressionBuildContext() { MethodDescription = statementBuildContext.MethodDescription };
                var linkedDesc = linkedDataPin.Parent;
                if(linkedDesc is TtExpressionDescription linkedExpressionDesc)
                {
                    var exp = linkedExpressionDesc.BuildExpression(ref buildContext);
                    forStatement.BeginExpression = exp;
                }
                if(linkedDesc is TtStatementDescription linkedStatementDesc)
                {
                    var exp = linkedStatementDesc.BuildExpressionForOutPin(linkedDataPin);
                    forStatement.BeginExpression = exp;
                }             
            }

            linkedDataPin = methodDesc.GetLinkedDataPin(DataInPins[1]);
            if(linkedDataPin == null)
            {
                // end index default 1
                forStatement.EndExpression = new TtPrimitiveExpression(1);
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                var buildContext = new FExpressionBuildContext() { MethodDescription = statementBuildContext.MethodDescription };
                var linkedDesc = linkedDataPin.Parent;
                if (linkedDesc is TtExpressionDescription linkedExpressionDesc)
                {
                    var exp = linkedExpressionDesc.BuildExpression(ref buildContext);
                    forStatement.EndExpression = exp;
                }
                if (linkedDesc is TtStatementDescription linkedStatementDesc)
                {
                    var exp = linkedStatementDesc.BuildExpressionForOutPin(linkedDataPin);
                    forStatement.EndExpression = exp;
                }
            }

            linkedDataPin = methodDesc.GetLinkedDataPin(DataInPins[2]);
            if(linkedDataPin == null)
            {
                // step default 1
                forStatement.StepExpression = new TtPrimitiveExpression(1);
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                var buildContext = new FExpressionBuildContext() { MethodDescription = statementBuildContext.MethodDescription };
                var linkedDesc = linkedDataPin.Parent;
                if (linkedDesc is TtExpressionDescription linkedExpressionDesc)
                {
                    var exp = linkedExpressionDesc.BuildExpression(ref buildContext);
                    forStatement.StepExpression = exp;
                }
                if (linkedDesc is TtStatementDescription linkedStatementDesc)
                {
                    var exp = linkedStatementDesc.BuildExpressionForOutPin(linkedDataPin);
                    forStatement.StepExpression = exp;
                }
            }

            var executionOutPin_LoopBody = ExecutionOutPins[1];
            var linkedLoopBodyExecPin = methodDesc.GetLinkedExecutionPin(executionOutPin_LoopBody);
            if(linkedLoopBodyExecPin == null)
            {
                forStatement.LoopBody = new TtExecuteSequenceStatement();
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedLoopBodyExecPin is TtExecutionInPinDescription);
                var buildContext = new FStatementBuildContext()
                {
                    ExecuteSequenceStatement = new(),
                    MethodDescription = statementBuildContext.MethodDescription,
                };
                (linkedLoopBodyExecPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                forStatement.LoopBody = buildContext.ExecuteSequenceStatement;
            }
            statementBuildContext.AddStatement(forStatement);

            var executionOutPin_Next = ExecutionOutPins[0];
            var linkedNextPin = methodDesc.GetLinkedExecutionPin(executionOutPin_Next);
            if (linkedNextPin != null)
            {
                System.Diagnostics.Debug.Assert(linkedNextPin is TtExecutionInPinDescription);
                (linkedNextPin.Parent as TtStatementDescription).BuildStatement(ref statementBuildContext);
            }

            return forStatement;
        }

        public override TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin)
        {
            if(pin == DataOutPins[0])
            {
                return new TtVariableReferenceExpression("__loopIndex_" + this.Id.ToString().GetHashCode());
            }
            return null;
        }
    }
}

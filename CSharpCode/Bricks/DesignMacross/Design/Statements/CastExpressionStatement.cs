using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.DesignMacross.Design.Statements;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ContextMenu("Cast", "Data\\Cast", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_CastStatementDescription))]
    public class TtCastStatementDescription : TtStatementDescription
    {
        public TtTypeDesc TargetType
        {
            get
            {
                return DataOutPins[0].TypeDesc;
            }
            set
            {
                DataOutPins[0].TypeDesc = value;
            }
        }
        public TtTypeDesc SourceType
        {
            get
            {
                return DataInPins[0].TypeDesc;
            }
            set
            {
                DataInPins[0].TypeDesc = value;
            }
        }
        public TtDataInPinDescription SourcePin
        {
            get=> DataInPins[0];
        }
        public TtCastStatementDescription() 
        {
            Name = "Cast";
            AddExecutionInPin(new());
            AddExecutionOutPin(new() { Name = "Succeed" });
            AddExecutionOutPin(new() { Name = "Failed" });

            AddDataInPin(new());
            AddDataOutPin(new());
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var ifStatement = new TtIfStatement();
            var binaryOP = new TtBinaryOperatorExpression()
            {
                Operation = TtBinaryOperatorExpression.EBinaryOperation.Is,
            };
            var linkedDataPin = statementBuildContext.MethodDescription.GetLinkedDataPin(DataInPins[0]);
            if (linkedDataPin == null) 
            {

            }
            else
            {
                if(linkedDataPin.Parent is TtExpressionDescription expressionDescription)
                {
                    FExpressionBuildContext buildContext = new() { MethodDescription = statementBuildContext.MethodDescription };
                    var left = expressionDescription.BuildExpression(ref buildContext);
                    binaryOP.Left = left;
                }
                if (linkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    FExpressionBuildContext buildContext = new() { MethodDescription = statementBuildContext.MethodDescription };
                    var left = statementDescription.BuildExpressionForOutPin(linkedDataPin);
                    binaryOP.Left = left;
                }
            }
            binaryOP.Right = new TtPrimitiveExpression(TargetType, false);
            ifStatement.Condition = binaryOP;

            var castedVarName = "result_" + Name + "_" + (uint)Id.ToString().GetHashCode();
            TtVariableDeclaration casted = new TtVariableDeclaration
            {
                VariableType = new TtTypeReference(TargetType),
                VariableName = castedVarName
            };
            statementBuildContext.AddStatement(casted);
            var executionOutPin_True = ExecutionOutPins[0];
            var linkedTrueExecPin = statementBuildContext.MethodDescription.GetLinkedExecutionPin(executionOutPin_True);
            if (linkedTrueExecPin == null)
            {
                //空语句
                //ifStatement.TrueStatement = new UEmptyStatement()

                ifStatement.TrueStatement = new TtExecuteSequenceStatement();
            }
            else
            {
                if(linkedDataPin == null)
                {

                }
                else
                {
                    var trueExecuteSequenceStatement = new TtExecuteSequenceStatement();
                    TtCastExpression castExpression = new();
                    castExpression.SourceType = new TtTypeReference(linkedDataPin.TypeDesc);
                    castExpression.TargetType = new TtTypeReference(TargetType);
                    if (linkedDataPin.Parent is TtExpressionDescription expressionDescription)
                    {
                        FExpressionBuildContext buildContext = new() { MethodDescription = statementBuildContext.MethodDescription };
                        castExpression.Expression = expressionDescription.BuildExpression(ref buildContext);
                    }
                    if (linkedDataPin.Parent is TtStatementDescription statementDescription)
                    {
                        FExpressionBuildContext buildContext = new() { MethodDescription = statementBuildContext.MethodDescription };
                        castExpression.Expression = statementDescription.BuildExpressionForOutPin(linkedDataPin);
                    }
                    var assign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(castedVarName), castExpression);
                    trueExecuteSequenceStatement.Sequence.Add(assign);
                    FStatementBuildContext trueStatementBuildContext = new() { ExecuteSequenceStatement = new(), MethodDescription = statementBuildContext.MethodDescription };
                    (linkedTrueExecPin.Parent as TtStatementDescription).BuildStatement(ref trueStatementBuildContext);
                    trueExecuteSequenceStatement.Sequence.Add(trueStatementBuildContext.ExecuteSequenceStatement);
                    ifStatement.TrueStatement = trueExecuteSequenceStatement;
                }
            }
            var executionOutPin_False = ExecutionOutPins[1];
            var linkedFalseExecPin = statementBuildContext.MethodDescription.GetLinkedExecutionPin(executionOutPin_False);
            if (linkedFalseExecPin == null)
            {
                //空
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedFalseExecPin is TtExecutionInPinDescription);
                FStatementBuildContext buildContext = new() { ExecuteSequenceStatement = new(), MethodDescription = statementBuildContext.MethodDescription };
                (linkedFalseExecPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                ifStatement.FalseStatement = buildContext.ExecuteSequenceStatement;
            }
            statementBuildContext.AddStatement(ifStatement);
            return base.BuildStatement(ref statementBuildContext);
        }
        public override TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin)
        {
            return new TtVariableReferenceExpression("result_" + Name + "_" + (uint)Id.ToString().GetHashCode());
        }
        public override bool IsPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            if(SourcePin == selfPin)
            {
                if(selfPin.TypeDesc == null)
                {
                    return true;
                }
                return selfPin.TypeDesc == targetPin.TypeDesc;
            }
            return false;
        }
        public override void OnPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {
            if (SourcePin == selfPin)
            {
                SourceType = connectedPin.TypeDesc;
            }
        }
        public override void OnPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {
            if (SourcePin == selfPin)
            {
                SourceType = null;
            }
        }
    }
}

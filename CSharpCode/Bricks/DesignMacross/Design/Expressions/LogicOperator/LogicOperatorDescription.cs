using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace EngineNS.DesignMacross.Design.Expressions
{
    #region UnaryLogicOperator
    public class TtUnaryLogicOperatorDescription : TtExpressionDescription
    {
        [Rtti.Meta]
        public TtUnaryOperatorExpression.EUnaryOperation Op { get; set; }
        public TtUnaryLogicOperatorDescription()
        {
            AddDataInPin(new() { Name = "" });
            AddDataOutPin(new() { Name = "" });
        }
    }

    #endregion UnaryLogicOperator

    #region BinaryLogicOperator
    public class TtBinaryLogicOperatorDescription : TtExpressionDescription
    {
        [Rtti.Meta]
        public TtBinaryOperatorExpression.EBinaryOperation Op { get; set; }
        public TtBinaryLogicOperatorDescription()
        {
            AddDataInPin(new() { Name = "" });
            AddDataInPin(new() { Name = "" });
            AddDataOutPin(new() { Name = "Result", TypeDesc = TtTypeDesc.TypeOf<bool>() });
        }

        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            var methodDesc = expressionBuildContext.MethodDescription as TtMethodDescription;
            
            TtBinaryOperatorExpression expression = new();
            expression.Operation = Op;
            var dataInPin_Left = DataInPins[0];
            var dataInPin_Right = DataInPins[1];
            var leftLinkedDataPin = methodDesc.GetLinkedDataPin(dataInPin_Left);
            if(leftLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(leftLinkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                if(leftLinkedDataPin.Parent is TtExpressionDescription expressionDescription)
                {
                    expression.Left = (leftLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                }
                if(leftLinkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    expression.Left = (leftLinkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(leftLinkedDataPin);
                }
            }
            else
            {
                expression.Left = new TtDefaultValueExpression() { Type = new TtTypeReference(dataInPin_Left.TypeDesc) };
            }
            var rightLinkedDataPin = methodDesc.GetLinkedDataPin(dataInPin_Right);
            if (rightLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(rightLinkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                if (rightLinkedDataPin.Parent is TtExpressionDescription expressionDescription)
                {
                    expression.Right = (rightLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                }
                if (rightLinkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    expression.Right = (rightLinkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(rightLinkedDataPin);
                }
                
            }
            else
            {
                expression.Right = new TtDefaultValueExpression() { Type = new TtTypeReference(dataInPin_Right.TypeDesc) };
            }
            return expression;
        }
        public void PinTypeSpreading(TtDataPinDescription dataPin, TtMethodDescription methodDescription)
        {
            foreach (var otherPin in DataInPins)
            {
                if (otherPin == dataPin)
                    continue;

                if (dataPin.TypeDesc != null)
                {
                    if (otherPin.TypeDesc == null)
                    {
                        otherPin.TypeDesc = dataPin.TypeDesc;
                    }
                }
                else
                {
                    if (otherPin.TypeDesc != null)
                    {
                        dataPin.TypeDesc = otherPin.TypeDesc;
                    }
                }
            }
            foreach (var otherPin in DataOutPins)
            {
                if (otherPin == dataPin)
                    continue;

                //if (dataPin.TypeDesc != null)
                //{
                //    if (otherPin.TypeDesc == null)
                //    {
                //        otherPin.TypeDesc = dataPin.TypeDesc;
                //        var linkedPin = methodDescription.GetLinkedDataPin(dataPin);
                //        if (linkedPin != null && linkedPin.TypeDesc == null)
                //        {
                //            if (linkedPin.Parent is TtBinaryArithmeticOperatorDescription valueOperatorDescription)
                //            {
                //                linkedPin.TypeDesc = dataPin.TypeDesc;
                //                valueOperatorDescription.PinTypeSpreading(linkedPin, methodDescription);
                //            }
                //            if (linkedPin.Parent is TtBinaryLogicOperatorDescription logicOperatorDescription)
                //            {
                //                linkedPin.TypeDesc = dataPin.TypeDesc;
                //                logicOperatorDescription.PinTypeSpreading(linkedPin, methodDescription);
                //            }

                //        }
                //    }
                //}
            }
        }
        public override bool IsPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            return selfPin.TypeDesc == targetPin.TypeDesc || selfPin.TypeDesc == null || targetPin.TypeDesc == null;
        }
        public override void OnPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {
            if (selfPin.TypeDesc == null)
            {
                selfPin.TypeDesc = connectedPin.TypeDesc;
                PinTypeSpreading(selfPin, methodDescription);
            }
        }
        public override void OnPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription disConnectedPin, TtMethodDescription methodDescription)
        {
            selfPin.TypeDesc = null;
        }
    }

    [ContextMenu("equal,==", "LogicOperation\\==", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtEqualOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtEqualOperatorDescription()
        {
            Name = "==";
            Op = TtBinaryOperatorExpression.EBinaryOperation.Equality;
        }
    }

    [ContextMenu("notequal,!=", "LogicOperation\\!=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtNotEqualOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtNotEqualOperatorDescription()
        {
            Name = "!=";
            Op = TtBinaryOperatorExpression.EBinaryOperation.NotEquality;
        }
    }

    [ContextMenu("greate,>", "LogicOperation\\>", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtGreateOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtGreateOperatorDescription()
        {
            Name = ">";
            Op = TtBinaryOperatorExpression.EBinaryOperation.GreaterThan;
        }
    }

    [ContextMenu("greateequal,>=", "LogicOperation\\>=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtGreateEqualOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtGreateEqualOperatorDescription()
        {
            Name = ">=";
            Op = TtBinaryOperatorExpression.EBinaryOperation.GreaterThanOrEqual;
        }
    }

    [ContextMenu("less,<", "LogicOperation\\<", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtLessOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtLessOperatorDescription()
        {
            Name = "<";
            Op = TtBinaryOperatorExpression.EBinaryOperation.LessThan;
        }
    }

    [ContextMenu("lessequal,<=", "LogicOperation\\<=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtLessEqualOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtLessEqualOperatorDescription()
        {
            Name = "<=";
            Op = TtBinaryOperatorExpression.EBinaryOperation.LessThanOrEqual;
        }
    }

    [ContextMenu("and,&&", "LogicOperation\\&&", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtAndOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtAndOperatorDescription()
        {
            Name = "And";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BooleanAnd;
        }
    }

    [ContextMenu("or,||", "LogicOperation\\||", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BinaryLogicOperator))]
    public class TtOrOperatorDescription : TtBinaryLogicOperatorDescription
    {
        public TtOrOperatorDescription()
        {
            Name = "Or";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BooleanOr;
        }
    }
    #endregion BinaryLogicOperator
}

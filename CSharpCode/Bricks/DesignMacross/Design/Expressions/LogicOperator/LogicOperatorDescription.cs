using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace EngineNS.DesignMacross.Design.Expressions
{
    public class TtLogicOperatorDescription : TtExpressionDescription
    {
        [Rtti.Meta]
        public TtBinaryOperatorExpression.EBinaryOperation Op { get; set; }
        public TtLogicOperatorDescription()
        {
            AddDtaInPin(new() { Name = "L" });
            AddDtaInPin(new() { Name = "R" });
            AddDtaOutPin(new() { Name = "=" });
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
                expression.Left = (leftLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
            }
            var rightLinkedDataPin = methodDesc.GetLinkedDataPin(dataInPin_Right);
            if (rightLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(rightLinkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                expression.Right = (rightLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
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

                if (dataPin.TypeDesc != null)
                {
                    if (otherPin.TypeDesc == null)
                    {
                        otherPin.TypeDesc = dataPin.TypeDesc;
                        var linkedPin = methodDescription.GetLinkedDataPin(dataPin);
                        if (linkedPin != null && linkedPin.TypeDesc == null)
                        {
                            if (linkedPin.Parent is TtValueOperatorDescription valueOperatorDescription)
                            {
                                linkedPin.TypeDesc = dataPin.TypeDesc;
                                valueOperatorDescription.PinTypeSpreading(linkedPin, methodDescription);
                            }
                            if (linkedPin.Parent is TtLogicOperatorDescription logicOperatorDescription)
                            {
                                linkedPin.TypeDesc = dataPin.TypeDesc;
                                logicOperatorDescription.PinTypeSpreading(linkedPin, methodDescription);
                            }

                        }
                    }
                }
            }
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
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtEqualOperatorDescription()
        {
            Name = "==";
            Op = TtBinaryOperatorExpression.EBinaryOperation.Equality;
        }
    }

    [ContextMenu("notequal,!=", "LogicOperation\\!=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtNotEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtNotEqualOperatorDescription()
        {
            Name = "!=";
            Op = TtBinaryOperatorExpression.EBinaryOperation.NotEquality;
        }
    }

    [ContextMenu("greate,>", "LogicOperation\\>", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtGreateOperatorDescription : TtLogicOperatorDescription
    {
        public TtGreateOperatorDescription()
        {
            Name = ">";
            Op = TtBinaryOperatorExpression.EBinaryOperation.GreaterThan;
        }
    }

    [ContextMenu("greateequal,>=", "LogicOperation\\>=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtGreateEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtGreateEqualOperatorDescription()
        {
            Name = ">=";
            Op = TtBinaryOperatorExpression.EBinaryOperation.GreaterThanOrEqual;
        }
    }

    [ContextMenu("less,<", "LogicOperation\\<", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtLessOperatorDescription : TtLogicOperatorDescription
    {
        public TtLessOperatorDescription()
        {
            Name = "<";
            Op = TtBinaryOperatorExpression.EBinaryOperation.LessThan;
        }
    }

    [ContextMenu("lessequal,<=", "LogicOperation\\<=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtLessEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtLessEqualOperatorDescription()
        {
            Name = "<=";
            Op = TtBinaryOperatorExpression.EBinaryOperation.LessThanOrEqual;
        }
    }

    [ContextMenu("and,&&", "LogicOperation\\&&", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtAndOperatorDescription : TtLogicOperatorDescription
    {
        public TtAndOperatorDescription()
        {
            Name = "And";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BooleanAnd;
        }
    }

    [ContextMenu("or,||", "LogicOperation\\||", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtOrOperatorDescription : TtLogicOperatorDescription
    {
        public TtOrOperatorDescription()
        {
            Name = "Or";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BooleanOr;
        }
    }
}

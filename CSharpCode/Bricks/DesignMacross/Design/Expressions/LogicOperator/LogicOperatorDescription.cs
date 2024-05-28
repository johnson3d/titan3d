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
        public UBinaryOperatorExpression.EBinaryOperation Op { get; set; }
        public TtLogicOperatorDescription()
        {
            AddDtaInPin(new() { Name = "L" });
            AddDtaInPin(new() { Name = "R" });
            AddDtaOutPin(new() { Name = "=" });
        }

        public override UExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            var methodDesc = expressionBuildContext.MethodDescription as TtMethodDescription;
            
            UBinaryOperatorExpression expression = new();
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
    }

    [ContextMenu("equal,==", "LogicOperation\\==", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtEqualOperatorDescription()
        {
            Name = "==";
            Op = UBinaryOperatorExpression.EBinaryOperation.Equality;
        }
    }

    [ContextMenu("notequal,!=", "LogicOperation\\!=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtNotEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtNotEqualOperatorDescription()
        {
            Name = "!=";
            Op = UBinaryOperatorExpression.EBinaryOperation.NotEquality;
        }
    }

    [ContextMenu("greate,>", "LogicOperation\\>", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtGreateOperatorDescription : TtLogicOperatorDescription
    {
        public TtGreateOperatorDescription()
        {
            Name = ">";
            Op = UBinaryOperatorExpression.EBinaryOperation.GreaterThan;
        }
    }

    [ContextMenu("greateequal,>=", "LogicOperation\\>=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtGreateEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtGreateEqualOperatorDescription()
        {
            Name = ">=";
            Op = UBinaryOperatorExpression.EBinaryOperation.GreaterThanOrEqual;
        }
    }

    [ContextMenu("less,<", "LogicOperation\\<", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtLessOperatorDescription : TtLogicOperatorDescription
    {
        public TtLessOperatorDescription()
        {
            Name = "<";
            Op = UBinaryOperatorExpression.EBinaryOperation.LessThan;
        }
    }

    [ContextMenu("lessequal,<=", "LogicOperation\\<=", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtLessEqualOperatorDescription : TtLogicOperatorDescription
    {
        public TtLessEqualOperatorDescription()
        {
            Name = "<=";
            Op = UBinaryOperatorExpression.EBinaryOperation.LessThanOrEqual;
        }
    }

    [ContextMenu("and,&&", "LogicOperation\\&&", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtAndOperatorDescription : TtLogicOperatorDescription
    {
        public TtAndOperatorDescription()
        {
            Name = "And";
            Op = UBinaryOperatorExpression.EBinaryOperation.BooleanAnd;
        }
    }

    [ContextMenu("or,||", "LogicOperation\\||", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_LogicOperator))]
    public class TtOrOperatorDescription : TtLogicOperatorDescription
    {
        public TtOrOperatorDescription()
        {
            Name = "Or";
            Op = UBinaryOperatorExpression.EBinaryOperation.BooleanOr;
        }
    }
}

using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_DataPin))]
    public class TtValueDataInPinDescription : TtDataInPinDescription
    {

    }
    public class TtValueOperatorDescription : TtExpressionDescription
    {
        [Rtti.Meta]
        public TtBinaryOperatorExpression.EBinaryOperation Op { get; set; }
        public TtValueOperatorDescription()
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
            if (leftLinkedDataPin != null)
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
}

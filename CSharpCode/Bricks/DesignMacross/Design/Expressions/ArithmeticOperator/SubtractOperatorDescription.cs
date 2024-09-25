using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions.ValueOperator
{
    [ContextMenu("subtraction,-", "ValueOperation\\-", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtSubtractOperatorDescription : TtBinaryArithmeticOperatorDescription
    {
        public TtSubtractOperatorDescription()
        {
            Name = "Subtract";
            Op = TtBinaryOperatorExpression.EBinaryOperation.Subtract;
        }
    }
}

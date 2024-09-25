using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions.ValueOperator
{
    [ContextMenu("bitxor,^", "ValueOperation\\^", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtBitXorOperatorDescription : TtBinaryArithmeticOperatorDescription
    {
        public TtBitXorOperatorDescription()
        {
            Name = "BitXor";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BitwiseXOR;
        }
    }
}

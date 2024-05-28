using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions.ValueOperator
{
    [ContextMenu("bitxor,^", "ValueOperation\\^", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtBitXorOperatorDescription : TtValueOperatorDescription
    {
        public TtBitXorOperatorDescription()
        {
            Name = "BitXor";
            Op = UBinaryOperatorExpression.EBinaryOperation.BitwiseXOR;
        }
    }
}

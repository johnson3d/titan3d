using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ContextMenu("bitor,|", "ValueOperation\\|", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtBitOrOperatorDescription : TtBinaryArithmeticOperatorDescription
    {
        public TtBitOrOperatorDescription()
        {
            Name = "BitOr";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BitwiseOr;
        }
    }
}

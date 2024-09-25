using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions.ValueOperator
{
    [ContextMenu("bitrightshift,|", "ValueOperation\\>>", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtBitRightshiftOperatorDescription : TtBinaryArithmeticOperatorDescription
    {
        public TtBitRightshiftOperatorDescription()
        {
            Name = "BitRightshift";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BitwiseRightShift;
        }
    }
}

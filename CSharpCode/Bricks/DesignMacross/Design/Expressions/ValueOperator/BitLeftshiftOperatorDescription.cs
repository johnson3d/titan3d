using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ContextMenu("bitleftshift,|", "ValueOperation\\<<", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtBitLeftshiftOperatorDescription : TtValueOperatorDescription
    {
        public TtBitLeftshiftOperatorDescription()
        {
            Name = "BitLeftshift";
            Op = TtBinaryOperatorExpression.EBinaryOperation.BitwiseLeftShift;
        }
    }
}

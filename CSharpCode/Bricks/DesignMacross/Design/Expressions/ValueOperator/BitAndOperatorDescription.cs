using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ContextMenu("bitand,&", "ValueOperation\\&", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtBitAndOperatorDescription : TtValueOperatorDescription
    {
        public TtBitAndOperatorDescription()
        {
            Name = "BitAnd";
            Op = UBinaryOperatorExpression.EBinaryOperation.BitwiseAnd;
        }
    }
}

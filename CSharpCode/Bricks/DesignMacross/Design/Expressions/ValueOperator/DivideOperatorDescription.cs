using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions.ValueOperator
{
    [ContextMenu("division,/", "ValueOperation\\/", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtDivideOperatorDescription : TtValueOperatorDescription
    {
        public TtDivideOperatorDescription()
        {
            Name = "Divide";
            Op = UBinaryOperatorExpression.EBinaryOperation.Divide;
        }
    }
}

using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions.ValueOperator
{
    [ContextMenu("multiplication,*", "ValueOperation\\*", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtMultiplyOperatorDescription : TtValueOperatorDescription
    {
        public TtMultiplyOperatorDescription()
        {
            Name = "Multiply";
            Op = UBinaryOperatorExpression.EBinaryOperation.Multiply;
        }
    }
}

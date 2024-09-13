using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ContextMenu("add,+", "ValueOperation\\+", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtAddOperatorDescription : TtValueOperatorDescription
    {
        public TtAddOperatorDescription()
        {
            Name = "Add";
            Op = TtBinaryOperatorExpression.EBinaryOperation.Add;
        }
    }
}

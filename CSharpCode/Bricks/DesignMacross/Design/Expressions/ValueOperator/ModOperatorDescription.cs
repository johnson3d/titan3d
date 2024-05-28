using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.DesignMacross.Design.Expressions.ValueOperator
{
    [ContextMenu("mod,%", "ValueOperation\\%", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtModOperatorDescription : TtValueOperatorDescription
    {
        public TtModOperatorDescription() 
        {
            Name = "Mod";
            Op = UBinaryOperatorExpression.EBinaryOperation.Modulus;
        }
    }
}

using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_DataPin))]
    public class TtSelfReferenceDataPin : TtDataOutPinDescription
    {
    }
    [GraphElement(typeof(TtGraphElement_VarGet))]
    public class TtSelfReferenceDescription : TtExpressionDescription
    {
        public override string Name { get => "Self"; set { } }
        public TtSelfReferenceDescription()
        {
            AddDataOutPin(new TtSelfReferenceDataPin() { Name = "Get" });
        }
        public TtSelfReferenceDescription(TtTypeDesc typeDesc)
        {
            AddDataOutPin(new TtSelfReferenceDataPin() { Name = "Get", TypeDesc = typeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtVariableReferenceExpression("this");
        }
    }
}

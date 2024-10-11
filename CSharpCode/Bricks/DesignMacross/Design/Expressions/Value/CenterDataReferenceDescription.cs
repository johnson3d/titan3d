using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_DataPin))]
    public class TtCenterDataReferenceDataPin : TtDataOutPinDescription
    {
    }
    [GraphElement(typeof(TtGraphElement_VarGet))]
    public class TtCenterDataReferenceDescription : TtExpressionDescription
    {
        public override string Name { get => "CenterData"; set { } }
        public TtCenterDataReferenceDescription()
        {
            AddDataOutPin(new TtCenterDataReferenceDataPin() { Name = "Get" });
        }
        public TtCenterDataReferenceDescription(TtTypeDesc typeDesc)
        {
            AddDataOutPin(new TtCenterDataReferenceDataPin() { Name = "Get", TypeDesc = typeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtVariableReferenceExpression("CenterData");
        }
    }
}

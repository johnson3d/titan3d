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
            AddDtaOutPin(new TtSelfReferenceDataPin() { Name = "Get" });
        }
        public TtSelfReferenceDescription(UTypeDesc typeDesc)
        {
            AddDtaOutPin(new TtSelfReferenceDataPin() { Name = "Get", TypeDesc = typeDesc });
        }
    }
}

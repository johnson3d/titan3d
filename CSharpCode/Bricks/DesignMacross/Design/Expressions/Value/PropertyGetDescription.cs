using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarGet))]
    public class TtPropertyGetDescription : TtExpressionDescription
    {
        public Guid HostReferenceId { get; set; } = Guid.Empty;
        public TtTypeDesc VarTypeDesc { get; set; } = null;
        public TtTypeDesc HostReferenceTypeDesc { get; set; } = null;

        public TtPropertyGetDescription()
        {
            AddDataInPin(new() { Name = "Host", TypeDesc = HostReferenceTypeDesc });
            AddDataOutPin(new() { Name = "Get", TypeDesc = VarTypeDesc });
        }

        public TtPropertyGetDescription(TtTypeDesc hostReferenceTypeDesc, TtTypeDesc varTypeDesc)
        {
            AddDataInPin(new() { Name = "Host", TypeDesc = hostReferenceTypeDesc });
            AddDataOutPin(new() { Name = "Get", TypeDesc = varTypeDesc });
        }
        public TtDataInPinDescription GetHostPin()
        {
            return DataInPins[0];
        }
    }
}

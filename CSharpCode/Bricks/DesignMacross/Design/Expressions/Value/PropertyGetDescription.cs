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
        public UTypeDesc VarTypeDesc { get; set; } = null;
        public UTypeDesc HostReferenceTypeDesc { get; set; } = null;

        public TtPropertyGetDescription()
        {
            AddDtaInPin(new() { Name = "Host", TypeDesc = HostReferenceTypeDesc });
            AddDtaOutPin(new() { Name = "Get", TypeDesc = VarTypeDesc });
        }

        public TtPropertyGetDescription(UTypeDesc hostReferenceTypeDesc, UTypeDesc varTypeDesc)
        {
            AddDtaInPin(new() { Name = "Host", TypeDesc = hostReferenceTypeDesc });
            AddDtaOutPin(new() { Name = "Get", TypeDesc = varTypeDesc });
        }
    }
}

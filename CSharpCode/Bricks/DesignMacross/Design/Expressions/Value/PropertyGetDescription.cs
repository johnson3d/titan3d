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
            AddDtaInPin(new() { Name = "Host", TypeDesc = HostReferenceTypeDesc });
            AddDtaOutPin(new() { Name = "Get", TypeDesc = VarTypeDesc });
        }

        public TtPropertyGetDescription(TtTypeDesc hostReferenceTypeDesc, TtTypeDesc varTypeDesc)
        {
            AddDtaInPin(new() { Name = "Host", TypeDesc = hostReferenceTypeDesc });
            AddDtaOutPin(new() { Name = "Get", TypeDesc = varTypeDesc });
        }
    }
}

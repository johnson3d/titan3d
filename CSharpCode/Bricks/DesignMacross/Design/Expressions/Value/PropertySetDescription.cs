using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarSet))]
    public class TtPropertySetDescription : TtExpressionDescription
    {
        public Guid HostReferenceId { get; set; } = Guid.Empty;
        public TtTypeDesc VarTypeDesc { get; set; } = null;
        public TtTypeDesc HostReferenceTypeDesc { get; set; } = null;

        public TtPropertySetDescription()
        {
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDtaInPin(new() { Name = "Host", TypeDesc = HostReferenceTypeDesc });
            AddDtaInPin(new() { Name = "Set", TypeDesc = VarTypeDesc });
            AddDtaOutPin(new() { Name = "Get", TypeDesc = VarTypeDesc });
        }
        public TtPropertySetDescription(TtTypeDesc hostReferenceTypeDesc, TtTypeDesc varTypeDesc)
        {
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDtaInPin(new() { Name = "Host", TypeDesc = hostReferenceTypeDesc });
            AddDtaInPin(new() { Name = "Set", TypeDesc = varTypeDesc });
            AddDtaOutPin(new() { Name = "Get", TypeDesc = varTypeDesc });
        }

        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return base.BuildExpression(ref expressionBuildContext);
        }
    }
}

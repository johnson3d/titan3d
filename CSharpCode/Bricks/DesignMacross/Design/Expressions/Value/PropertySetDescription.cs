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
            AddDataInPin(new() { Name = "Host", TypeDesc = HostReferenceTypeDesc });
            AddDataInPin(new() { Name = "Set", TypeDesc = VarTypeDesc });
            AddDataOutPin(new() { Name = "Get", TypeDesc = VarTypeDesc });
        }
        public TtPropertySetDescription(TtTypeDesc hostReferenceTypeDesc, TtTypeDesc varTypeDesc)
        {
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDataInPin(new() { Name = "Host", TypeDesc = hostReferenceTypeDesc });
            AddDataInPin(new() { Name = "Set", TypeDesc = varTypeDesc });
            AddDataOutPin(new() { Name = "Get", TypeDesc = varTypeDesc });
        }
        public TtDataInPinDescription GetHostPin()
        {
            return DataInPins[0];
        }

        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return base.BuildExpression(ref expressionBuildContext);
        }
    }
}

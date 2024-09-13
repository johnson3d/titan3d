using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarSet))]
    public class TtVarSetDescription : TtExpressionDescription
    {
        public override string Name { get => VariableDescription.Name; set => VariableDescription.Name = value; }
        public IVariableDescription VariableDescription { get; set; }
        public Guid VariableId { get; set; } = Guid.Empty;
        public UTypeDesc VarTypeDesc { get => VariableDescription.VariableType.TypeDesc; }

        public TtVarSetDescription()
        {
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDtaInPin(new() { Name = "Set", TypeDesc = UTypeDesc.TypeOf<bool>() });
            AddDtaOutPin(new() { Name = "Get", TypeDesc = UTypeDesc.TypeOf<bool>() });
        }

        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return base.BuildExpression(ref expressionBuildContext);
        }
    }
}

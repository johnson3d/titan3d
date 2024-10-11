using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
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
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            var linkedDataPin = expressionBuildContext.MethodDescription.GetLinkedDataPin(DataInPins[0]);
            if(linkedDataPin == null)
            {

            }
            else
            {
                if(linkedDataPin.Parent is TtExpressionDescription expressionDescription)
                {
                    FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                    var hostExpression = expressionDescription.BuildExpression(ref buildContext);
                    return new TtVariableReferenceExpression(Name, hostExpression);
                }
                if (linkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    var hostExpression = statementDescription.BuildExpressionForOutPin(linkedDataPin);
                    return new TtVariableReferenceExpression(Name, hostExpression);
                }
            }
            return null;
        }
    }
}

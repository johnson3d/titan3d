using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarGet))]
    public class TtVarGetDescription : TtExpressionDescription
    {
        [Rtti.Meta("")]
        public Guid VariableId { get; set; } = Guid.Empty;
        public override string Name
        {
            get
            {
                if (mVariableDescription != null)
                {
                    return "Var" + mVariableDescription.Name;
                }
                return "";
            }
        }
        TtVariableDescription mVariableDescription = null;
        
        
        public TtTypeDesc VarTypeDesc { get => mVariableDescription?.VariableType.TypeDesc; }
        public TtVarGetDescription()
        {
            AddDataOutPin(new() { Name = "Get", TypeDesc = TtTypeDesc.TypeOf<bool>() });
        }

        public override void UpdateData(ref FDescriptionUpdateContext updateContext)
        {
            if (mVariableDescription == null && updateContext.ClassDescription is TtClassDescription classDesc)
            {
                foreach (var variable in classDesc.Variables)
                {
                    if (variable.Id == VariableId)
                    {
                        mVariableDescription = variable as TtVariableDescription;
                    }
                }
            }
            var outPin = DataOutPins[0];
            if( VarTypeDesc!= null && outPin.TypeDesc != VarTypeDesc)
            {
                outPin.TypeDesc = VarTypeDesc;
            }
            base.UpdateData(ref updateContext);
        }

        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            if(expressionBuildContext.ClassBuildContext.ClassDescription != expressionBuildContext.ClassBuildContext.MainClassDescription)
            {
                return new TtVariableReferenceExpression(mVariableDescription.VariableName, new TtVariableReferenceExpression("CenterData"));
            }
            else
            {
                return new TtVariableReferenceExpression(mVariableDescription.VariableName);
            }
        }
    }
}

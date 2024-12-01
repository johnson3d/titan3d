using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarGet))]
    public class TtVarGetDescription : TtExpressionDescription
    {
        [Rtti.Meta]
        public Guid VariableId { get; set; } = Guid.Empty;
        public override string Name
        {
            get
            {
                if (VariableDescription != null)
                {
                    return VariableDescription.Name;
                }
                return "";
            }
        }
        public TtVariableDescription VariableDescription
        {
            get
            {
                if (Parent is TtClassDescription classDesc)
                {
                    foreach(var variable in classDesc.Variables)
                    {
                        if(variable.Id == VariableId)
                        {
                            return variable as TtVariableDescription;
                        }
                    }
                }
                return null;
            }
        }
        
        public TtTypeDesc VarTypeDesc { get => VariableDescription?.VariableType.TypeDesc; }
        public TtVarGetDescription()
        {
            AddDataOutPin(new() { Name = "Get", TypeDesc = TtTypeDesc.TypeOf<bool>() });
        }

        public IVariableDescription GetVariableDescription()
        {
            return null;
        }
    }
}

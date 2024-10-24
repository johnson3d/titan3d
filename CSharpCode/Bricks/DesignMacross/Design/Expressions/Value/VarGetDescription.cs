﻿using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarGet))]
    public class TtVarGetDescription : TtExpressionDescription
    {
        public override string Name
        {
            get
            {
                if(GetVariableDescription() != null)
                {

                }
                return "";
            }
        }
        public IVariableDescription VariableDescription { get; set; }
        public Guid VariableId { get; set; } = Guid.Empty;
        public TtTypeDesc VarTypeDesc { get=>VariableDescription.VariableType.TypeDesc; }
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

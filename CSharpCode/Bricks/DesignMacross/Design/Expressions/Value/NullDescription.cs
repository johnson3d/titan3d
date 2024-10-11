using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design.Expressions.Value
{
    [ContextMenu("null", "Data\\null", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_VarGet))]
    public class TtNullDescription : TtExpressionDescription
    {
        public override string Name
        {
            get
            {
                return "null";
            }
        }
        public TtNullDescription()
        {
            AddDataOutPin(new() { Name = "null", TypeDesc = null });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
           var expr = new TtNullValueExpression()
           {

           };
            return expr;
        }
    }
}

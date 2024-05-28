using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExpressionDescription))]
    public class TtGraphElement_LogicOperator : TtGraphElement_ExpressionDescription
    {
        public TtLogicOperatorDescription LogicOperatorDescription { get => Description as TtLogicOperatorDescription; }
        public TtGraphElement_LogicOperator(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

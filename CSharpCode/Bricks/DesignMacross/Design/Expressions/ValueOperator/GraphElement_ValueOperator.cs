using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExpressionDescription))]
    public class TtGraphElement_ValueOperator : TtGraphElement_ExpressionDescription
    {
        public TtValueOperatorDescription ValueOperatorDescription { get => Description as TtValueOperatorDescription; }
        public TtGraphElement_ValueOperator(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

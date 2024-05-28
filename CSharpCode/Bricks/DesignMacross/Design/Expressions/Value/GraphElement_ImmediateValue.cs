using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExpressionDescription))]
    public class TtGraphElement_ImmediateValue : TtGraphElement_ExpressionDescription
    {
        public TtImmediateValueDescription ImmediateValueDescription { get => Description as TtImmediateValueDescription; }
        public TtGraphElement_ImmediateValue(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

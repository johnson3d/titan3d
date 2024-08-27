using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExpressionDescription))]
    public class TtGraphElement_SelfReference : TtGraphElement_ExpressionDescription
    {
        public TtSelfReferenceDescription SelfReferenceDescription { get => Description as TtSelfReferenceDescription; }
        public TtGraphElement_SelfReference(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExpressionDescription))]
    public class TtGraphElement_VarGet : TtGraphElement_ExpressionDescription
    {
        public TtVarGetDescription VarGetDescription { get => Description as TtVarGetDescription; }
        public TtGraphElement_VarGet(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

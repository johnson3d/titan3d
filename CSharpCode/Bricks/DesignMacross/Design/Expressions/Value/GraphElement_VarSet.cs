using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExpressionDescription))]
    public class TtGraphElement_VarSet : TtGraphElement_ExpressionDescription
    {
        public TtVarSetDescription VarSetDescription { get => Description as TtVarSetDescription; }
        public TtGraphElement_VarSet(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

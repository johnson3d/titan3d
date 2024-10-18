using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_VarSet : TtGraphElement_StatementDescription
    {
        public TtGraphElement_VarSet(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

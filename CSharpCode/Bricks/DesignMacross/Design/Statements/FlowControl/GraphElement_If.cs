using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Statement
{
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_If : TtGraphElement_StatementDescription
    {
        public TtIfStatementDescription IfStatementDescription { get => Description as TtIfStatementDescription; }
        public TtGraphElement_If(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

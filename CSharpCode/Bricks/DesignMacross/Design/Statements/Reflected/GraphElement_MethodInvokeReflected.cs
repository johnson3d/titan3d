using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Statement
{
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_MethodInvokeReflected : TtGraphElement_StatementDescription
    {
        public TtMethodInvokeReflectedDescription MethodInvokeDescription { get => Description as TtMethodInvokeReflectedDescription; }
        public TtGraphElement_MethodInvokeReflected(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}

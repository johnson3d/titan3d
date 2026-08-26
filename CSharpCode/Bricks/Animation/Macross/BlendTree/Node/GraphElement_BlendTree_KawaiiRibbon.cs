using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtGraphElementRender_BlendTreeNode))]
    public class TtGraphElement_BlendTree_KawaiiRibbon : TtGraphElement_BlendTreeNode
    {
        public TtGraphElement_BlendTree_KawaiiRibbon(IDescription description, IGraphElementStyle style) : base(description, style)
        {

        }
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            base.ConstructElements(ref context);
        }
    }
}

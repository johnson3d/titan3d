using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
{

    [ImGuiElementRender(typeof(TtGraph_MethodRender))]
    public class TtGraph_AnimBlendTreeAttachmentMethod : TtGraph_Method
    {
        public TtGraph_AnimBlendTreeAttachmentMethod(IDescription description) : base(description)
        {
            
        }

        #region IContextMeunable
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = true;
            base.ConstructContextMenu(ref context, popupMenu);
        }
        #endregion IContextMeunable

    }

}

using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Base.Outline
{
    public interface IOutlineContextMeunable
    {
        public void ConstructContextMenu(ref FOutlineElementRenderingContext context, TtPopupMenu popupMenu);
        public void SetContextMenuableId(TtPopupMenu popupMenu);
        //public void OpenContextMeun();
        //public void DrawContextMenu(ref FGraphElementRenderingContext context);
    }
    public class TtOutlineContextMenuHandler
    {
        static TtOutlineContextMenuHandler mInstance = null;
        public static TtOutlineContextMenuHandler Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new TtOutlineContextMenuHandler();
                return mInstance;
            }
        }

        bool NeedOpenContextMenu = false;
        IOutlineElement ContextMenuElement = null;
        public virtual TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("Graph_PopupMenu" + Guid.NewGuid());
        public void OpenContextMenu(IOutlineElement element, ref FOutlineElementRenderingContext elementRenderingContext)
        {
            ContextMenuElement = element;
            NeedOpenContextMenu = true;
        }
        public void HandleContextMenu(ref FOutlineElementRenderingContext elementRenderingContext)
        {
            if (ContextMenuElement is IOutlineContextMeunable meunable)
            {
                if (NeedOpenContextMenu)
                {
                    meunable.SetContextMenuableId(PopupMenu);
                    if (ContextMenuElement is IOutlilneElementSelectable selectable)
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        PopupMenu.StringId = ContextMenuElement.Name + "_" + ContextMenuElement.Id + "_" + "ContextMenu";
                        PopupMenu.Reset();
                        meunable.ConstructContextMenu(ref elementRenderingContext, PopupMenu);
                        PopupMenu.OpenPopup();
                    }
                    NeedOpenContextMenu = false;
                }
                PopupMenu.Draw(ref elementRenderingContext);
                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) || ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    NeedOpenContextMenu = false;
                }
            }
        }
    }
}

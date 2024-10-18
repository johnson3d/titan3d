using EngineNS.DesignMacross.Design;

namespace EngineNS.DesignMacross.Base.Graph
{
    public interface IMouseEvent
    {

    }

    public struct FMargin
    {
        public static readonly FMargin Default = new FMargin(0, 0, 0, 0);
        public FMargin(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }
    }

    public class TtGraphContextMenuHandler
    {
        static TtGraphContextMenuHandler mInstance = null;
        public static TtGraphContextMenuHandler Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new TtGraphContextMenuHandler();
                return mInstance;
            }
        }

        bool NeedOpenContextMenu = false;
        IGraphElement ContextMenuElement = null;
        public virtual TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("Graph_PopupMenu" + Guid.NewGuid());
        public void HandleContextMenu(IGraphElement graphElement, ref FGraphElementRenderingContext elementRenderingContext)
        {
            if (graphElement is IContextMeunable meunable)
            {

                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    NeedOpenContextMenu = true;
                }
                if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Right, -1.0f))
                {
                    NeedOpenContextMenu = false;
                }
                if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Right)
                    && NeedOpenContextMenu
                    && elementRenderingContext.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                {
                    ContextMenuElement = graphElement;
                    meunable.SetContextMenuableId(PopupMenu);
                    var pos = elementRenderingContext.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    if (ContextMenuElement is IGraphElementSelectable selectable && selectable.HitCheck(pos))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        PopupMenu.StringId = graphElement.Name + "_" + graphElement.Id + "_" + "ContextMenu";
                        PopupMenu.Reset();
                        meunable.ConstructContextMenu(ref elementRenderingContext, PopupMenu);
                        PopupMenu.OpenPopup();
                    }
                    NeedOpenContextMenu = false;
                }
                PopupMenu.Draw(ref elementRenderingContext);
            }
        }
        public void HandleLinkedPinContextMenu(TtGraph graph, ref FGraphElementRenderingContext elementRenderingContext)
        {
            if (graph is IContextMeunable meunable)
            {
                ContextMenuElement = graph;
                meunable.SetContextMenuableId(PopupMenu);
                var pos = elementRenderingContext.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                if (ContextMenuElement is IGraphElementSelectable selectable && selectable.HitCheck(pos))
                {
                    ImGuiAPI.CloseCurrentPopup();
                    PopupMenu.StringId = graph.Name + "_" + graph.Id + "_" + "ContextMenu";
                    PopupMenu.Reset();
                    PopupMenu.bHasSearchBox = true;
                    graph.ConstructLinkedPinContextMenu(ref elementRenderingContext, PopupMenu);
                    PopupMenu.OpenPopup();
                }
                PopupMenu.Draw(ref elementRenderingContext);
            }
        }
    }
}

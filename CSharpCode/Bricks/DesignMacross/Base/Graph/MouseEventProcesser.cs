using System.Collections.Generic;

namespace EngineNS.DesignMacross.Base.Graph
{

    public class TtMouseEventProcesser
    {
        static TtMouseEventProcesser mInstance = null;
        public static TtMouseEventProcesser Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new TtMouseEventProcesser();
                return mInstance;
            }
        }

        bool ElementHitCheck(IGraphElement element, Vector2 pos, ref FGraphElementRenderingContext context)
        {
            var start = context.ViewportTransform(element.AbsLocation);
            var end = context.ViewportTransform(element.AbsLocation + new Vector2(element.Size.Width, element.Size.Height));
            Rect rect = new Rect(start.X, start.Y, end.X - start.X, end.Y - start.Y);
            //冗余一点
            Rect mouseRect = new Rect(pos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(mouseRect);
        }

        IGraphElement ProcessSelectableElementHitCheck(IGraphElement element, Vector2 pos, ref FMouseEventContext context)
        {
            if (!context.GraphElementRenderingContext.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
            {
                return null;
            }

            IGraphElement finalHit = null;
            if (element is IGraphElementSelectable selectableElement)
            {
                if (element.HitCheck(ref context))
                {
                    //selectableElement.OnSelected();
                    finalHit = element;
                    HitElementStack.Push(element);
                    if (element is IEnumChild enumChild)
                    {
                        var children = enumChild.EnumerateChildRverse<IGraphElement>();
                        foreach (var child in children)
                        {
                            var hit = ProcessSelectableElementHitCheck(child, pos, ref context);
                            if (hit != null)
                            {
                                finalHit = hit;
                                break;
                            }
                        }
                    }
                }
            }
            return finalHit;
        }
        public List<IGraphElement> ProcessSelectableElementsHitCheck(TtGraph graph, Vector2 pos, ref FMouseEventContext context)
        {
            List<IGraphElement> Hits = new();
            if (!context.GraphElementRenderingContext.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
            {
                return null;
            }

            if (graph is IGraphElementSelectable selectableElement)
            {
                if (graph is IEnumChild enumChild)
                {
                    var children = enumChild.EnumerateChildRverse<IGraphElement>();
                    foreach (var child in children)
                    {
                        if (child.HitCheck(ref context))
                        {
                            Hits.Add(child);
                        }
                    }
                }
            }
            return Hits;
        }

        internal IContextMeunable mOpenedContextMenu = null;
        public Stack<IGraphElement> HitElementStack = new Stack<IGraphElement>();
        public IGraphElement LastElement = null;
        private IGraphElement DraggingElement = null;
        ImGuiMouseButton_ DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_COUNT;
        private bool IsDragging = false;
        public void Processing(IGraph graph, ref FGraphElementRenderingContext context)
        {
            // 有任意 popup/模态打开时(如 KawaiiPhysics 曲线编辑器的模态大窗、属性面板弹窗),
            // 不处理图的鼠标事件 —— 否则点击会因位置几何上落在图视口内而"点透"触发
            // 选择/右键菜单(本框架只用 IsInViewport 判定, 不看 hover/焦点/popup)。
            // 与下方 Zooming 已有的 IsPopupOpen 门控同理, 这里提前覆盖所有按键/拖拽/命中。
            // 保留 !IsDragging: 图内已发起的拖拽即使此时弹窗也能把拖拽正常结束, 不卡死状态。
            if (!IsDragging && ImGuiAPI.IsPopupOpen("", ImGuiPopupFlags_.ImGuiPopupFlags_AnyPopup))
                return;

            var mouseEventContext = new FMouseEventContext();
            mouseEventContext.GraphElementRenderingContext = context;
            var MousePos = ImGuiAPI.GetMousePos();
            mouseEventContext.MouseAbsPos = MousePos;
            mouseEventContext.MouseAbsRect = new Rect(MousePos - Vector2.One, new SizeF(1.0f, 1.0f));
            //var topmostElement = ProcessGraphHitCheck(graph, context.ViewPort.ViewportInverseTransform(context.Camera.Location, MousePos), ref context);
            var topmostElement = ProcessSelectableElementHitCheck(graph, MousePos, ref mouseEventContext);
            if (topmostElement != null)
            {
                if (LastElement != null && LastElement != topmostElement)
                {
                    LastElement.OnMouseLeave(ref mouseEventContext);
                }
                topmostElement.OnMouseOver(ref mouseEventContext);
                if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f))
                {
                    IsDragging = true;
                    DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_Left;
                    topmostElement.OnMouseMove(ref mouseEventContext);
                }
                if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Right, -1.0f))
                {
                    IsDragging = true;
                    DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_Right;
                    topmostElement.OnMouseMove(ref mouseEventContext);
                }
                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) && !IsDragging)
                {
                    if (context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                    {
                        topmostElement.OnMouseLeftButtonDown(ref mouseEventContext);
                        DraggingElement = topmostElement;
                    }
                }
                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right) && !IsDragging)
                {
                    if (context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                    {
                        topmostElement.OnMouseRightButtonDown(ref mouseEventContext);
                    }
                }
                if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    topmostElement.OnMouseLeftButtonUp(ref mouseEventContext);
                    if (IsDragging)
                    {
                        IsDragging = false;
                        DraggingElement = null;
                        DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_COUNT;
                    }
                }
                if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    topmostElement.OnMouseRightButtonUp(ref mouseEventContext);
                    if (IsDragging)
                    {
                        IsDragging = false;
                        DraggingElement = null;
                        DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_COUNT;
                    }
                }
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
                {
                    //foreach (var element in context.DesignedGraph.Elements)
                    //{
                    //    if (element == topmostElement)
                    //        continue;

                    //    if (element is TtDescriptionGraphElement descGraphElement)
                    //    {
                    //        descGraphElement.OnUnSelected(ref mouseEventContext);
                    //    }
                    //}
                    if (topmostElement is IGraphElementSelectable selectable)
                    {
                        if (!selectable.IsSelected)
                        {
                            topmostElement.OnSelected(ref mouseEventContext);
                        }
                    }
                }
                if (IsDragging)
                {
                    if (DraggingButton == ImGuiMouseButton_.ImGuiMouseButton_Left)
                    {
                        if (DraggingElement is IGraphElementDraggable draggableEle && draggableEle != graph)
                        {
                            var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f);
                            ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left);
                            //draggableEle.OnDragging(delta / context.Camera.Scale);
                            foreach (var selectedElement in mouseEventContext.GraphElementRenderingContext.DesignedGraph.SelectedElements)
                            {
                                if (selectedElement != draggableEle)
                                {
                                    if (selectedElement is IGraphElementDraggable draggableSelected)
                                    {
                                        draggableSelected.OnDragging(delta / context.Camera.Scale);
                                    }
                                }
                            }

                        }
                    }
                    if (DraggingButton == ImGuiMouseButton_.ImGuiMouseButton_Right)
                    {
                        var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right, -1.0f);
                        graph.OnDragging(delta);
                        ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right);
                    }

                }
                if (ImGuiAPI.GetIO().MouseWheel != 0 && !ImGuiAPI.IsPopupOpen("0", ImGuiPopupFlags_.ImGuiPopupFlags_AnyPopupId))
                {
                    graph.Zooming(ImGuiAPI.GetIO().MouseWheel, mouseEventContext);
                }

            }
            LastElement = topmostElement;
        }
    }
}

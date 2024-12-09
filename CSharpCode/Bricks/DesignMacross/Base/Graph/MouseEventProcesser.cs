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
                        var children = enumChild.EnumerateChild<IGraphElement>();
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

        internal IContextMeunable mOpenedContextMenu = null;
        public Stack<IGraphElement> HitElementStack = new Stack<IGraphElement>();
        public IGraphElement LastElement = null;
        private IGraphElement DraggingElement = null;
        ImGuiMouseButton_ DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_COUNT;
        private bool IsDragging = false;
        public void Processing(IGraph graph, ref FGraphElementRenderingContext context)
        {
            var mouseEventContext = new FMouseEventContext();
            mouseEventContext.GraphElementRenderingContext = context;
            var MousePos = ImGuiAPI.GetMousePos();
            mouseEventContext.MouseAbsPos = MousePos;
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
                }
                if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Right, -1.0f))
                {
                    IsDragging = true;
                    DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_Right;
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
                    topmostElement.OnSelected(ref mouseEventContext);
                }
                if (IsDragging)
                {
                    if (DraggingButton == ImGuiMouseButton_.ImGuiMouseButton_Left)
                    {
                        if (DraggingElement is IGraphElementDraggable draggableEle && draggableEle != graph)
                        {
                            var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f);
                            draggableEle.OnDragging(delta / context.Camera.Scale);
                            ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        }
                    }
                    if (DraggingButton == ImGuiMouseButton_.ImGuiMouseButton_Right)
                    {
                        var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right, -1.0f);
                        graph.OnDragging(delta);
                        ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right);
                    }

                }
                if (ImGuiAPI.GetIO().MouseWheel != 0)
                {
                    graph.Zooming(ImGuiAPI.GetIO().MouseWheel);
                }

            }
            LastElement = topmostElement;
        }
    }
}

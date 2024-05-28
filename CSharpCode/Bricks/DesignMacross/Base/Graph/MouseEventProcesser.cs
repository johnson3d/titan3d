namespace EngineNS.DesignMacross.Base.Graph
{

    public class TtMouseEventProcesser
    {
        static TtMouseEventProcesser mInstance = null;
        public static TtMouseEventProcesser Instance 
        { 
            get
            {
                if(mInstance == null)
                    mInstance = new TtMouseEventProcesser();
                return mInstance;
            }
        }

        IGraphElement ProcessSelectableElement(IGraphElement element, Vector2 pos)
        {
            IGraphElement finalHit = null;
            if (element is IGraphElementSelectable selectableElement)
            {
                if (selectableElement.HitCheck(pos))
                {
                    //selectableElement.OnSelected();
                    finalHit = element;
                    HitElementStack.Push(element);
                    if (element is IEnumChild enumChild)
                    {
                        var children = enumChild.EnumerateChild<IGraphElement>();
                        foreach (var child in children)
                        {
                            var  hit = ProcessSelectableElement(child, pos);
                            if(hit != null)
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
            var topmostElement = ProcessSelectableElement(graph, context.ViewPort.ViewportInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos()));
            if(topmostElement != null)
            {
                if (LastElement != null && LastElement != topmostElement)
                {
                    LastElement.OnMouseLeave(ref context);
                }
                topmostElement.OnMouseOver(ref context);
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
                    if(context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                    {
                        topmostElement.OnMouseLeftButtonDown(ref context);
                        DraggingElement = topmostElement;
                    }
                }
                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right) && !IsDragging)
                {
                    if (context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                    {
                        topmostElement.OnMouseRightButtonDown(ref context); 
                    }
                }
                if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    topmostElement.OnMouseLeftButtonUp(ref context);
                    if (IsDragging)
                    {
                        IsDragging = false;
                        DraggingElement = null;
                        DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_COUNT;
                    }
                }
                if(ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    topmostElement.OnMouseRightButtonUp(ref context);
                    if (IsDragging)
                    {
                        IsDragging = false;
                        DraggingElement = null;
                        DraggingButton = ImGuiMouseButton_.ImGuiMouseButton_COUNT;
                    }
                }
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
                {
                    topmostElement.OnSelected(ref context);
                }
                if (IsDragging)
                {
                    if (DraggingButton == ImGuiMouseButton_.ImGuiMouseButton_Left )
                    {
                        if(DraggingElement is IGraphElementDraggable draggableEle && draggableEle != graph)
                        {
                            var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f);
                            draggableEle.OnDragging(delta);
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
            }
            LastElement = topmostElement;
        }
    }
}

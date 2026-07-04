using EngineNS.Animation.Macross.BlendTree;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtGraphElement_PreviewLine : IGraphElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get; set; }
        public SizeF Size { get; set; }
        public IGraphElement Parent { get; set; }
        public IGraphElementStyle Style { get; set; }

        public TtPinDescription StartPin { get; set; } = null;

        public bool IsSelected => false;

        public bool CanMultiSelect => false;

        #region Selectable
        public bool CanDrag()
        {
            return false;
        }

        public void OnDragging(Vector2 delta)
        {

        }

        public bool HitCheck(ref FMouseEventContext context)
        {
            return false;
        }

        public void OnSelected(ref FMouseEventContext context)
        {
        }

        public void OnUnSelected(ref FMouseEventContext context)
        {
        }

        public void OnMouseOver(ref FMouseEventContext context)
        {
        }

        public void OnMouseLeave(ref FMouseEventContext context)
        {
        }

        public void OnMouseLeftButtonDown(ref FMouseEventContext context)
        {
        }

        public void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
        }

        public void OnMouseRightButtonDown(ref FMouseEventContext context)
        {
        }

        public void OnMouseRightButtonUp(ref FMouseEventContext context)
        {
        }

        public void OnMouseMove(ref FMouseEventContext context)
        {

        }
        #endregion
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_SelectingRect))]
    public class TtGraphElement_SelectingRect : IGraphElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get; set; }
        public SizeF Size { get; set; }
        public IGraphElement Parent { get; set; }
        public IGraphElementStyle Style { get; set; }

        public TtPinDescription StartPin { get; set; } = null;

        public bool IsSelected => false;
        public bool CanMultiSelect => false;

        #region Selectable
        public bool CanDrag()
        {
            return false;
        }

        public void OnDragging(Vector2 delta)
        {

        }

        public bool HitCheck(ref FMouseEventContext context)
        {
            return false;
        }

        public void OnSelected(ref FMouseEventContext context)
        {
        }

        public void OnUnSelected(ref FMouseEventContext context)
        {
        }

        public void OnMouseOver(ref FMouseEventContext context)
        {
        }

        public void OnMouseLeave(ref FMouseEventContext context)
        {
        }

        public void OnMouseLeftButtonDown(ref FMouseEventContext context)
        {
        }

        public void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
        }

        public void OnMouseRightButtonDown(ref FMouseEventContext context)
        {
        }

        public void OnMouseRightButtonUp(ref FMouseEventContext context)
        {
        }

        public void OnMouseMove(ref FMouseEventContext context)
        {

        }
        #endregion
    }
    public abstract class TtGraph : IGraph, IGraphElementSelectable, IEnumChild, IContextMeunable
    {
        public Guid Id { get; set; } = Guid.Empty;
        public TtGraphViewport ViewPort { get; set; } = new TtGraphViewport();
        public TtGraphCamera Camera { get; set; } = new TtGraphCamera();
        public TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();
        public string Name { get => Description.Name; set => Description.Name = value; }
        public Vector2 Location { get; set; } = Vector2.Zero;
        public Vector2 AbsLocation { get => CalculateAbsLocation(this); }
        public SizeF Size { get; set; } = new SizeF(100, 100);

        public IGraphElement Parent { get; set; } = null;
        public virtual IDescription Description { get; set; } = null;
        [Rtti.Meta("")]
        public List<IDescriptionGraphElement> Elements { get; set; } = new List<IDescriptionGraphElement>();
        public IGraphElementStyle Style { get; set; } = null;

        public TtGraphElement_PreviewLine PreviewLine { get; set; } = null;
        public TtGraphElement_SelectingRect SelectingRect { get; set; } = null;

        public List<TtDescriptionGraphElement> SelectedElements { get; set; } = new();

        public bool IsSelected => false;
        public bool CanMultiSelect { get; set; } = false;

        public TtGraph(IDescription description)
        {
            Id = description.Id;
            Description = description;
        }

        public abstract void ConstructElements(ref FGraphRenderingContext context);
        public abstract void AfterConstructElements(ref FGraphRenderingContext context);

        public IDescriptionGraphElement GetGraphElementByDescriptionId(Guid descId)
        {
            foreach (var element in Elements)
            {
                if (element.Id == descId) return element;
            }
            return null;
        }
        public void SelecteGraphElement(TtDescriptionGraphElement graphElement)
        {
            if(!CanMultiSelect)
            {
                foreach (var selectedElement in SelectedElements)
                {
                    selectedElement.IsSelected = false;
                }
                SelectedElements.Clear();
            }
            graphElement.IsSelected = true;
            SelectedElements.Add(graphElement);
        }
        public void UnSelecteGraphElement(TtDescriptionGraphElement graphElement)
        {
            if (!CanMultiSelect)
            {
                foreach (var selectedElement in SelectedElements)
                {
                    selectedElement.IsSelected = false;
                }
                SelectedElements.Clear();
            }
            else
            {
                graphElement.IsSelected = false;
                SelectedElements.Remove(graphElement);
            }
            
        }

        public void HighLightLine(TtGraphElement_Line highlightLine)
        {
            var highLightList = new List<TtDescriptionGraphElement>();
            highlightLine.HighLight();
            highLightList.Add(highlightLine);
            if (highlightLine.From is TtDescriptionGraphElement fromElement)
            {
                highLightList.Add(fromElement);
                fromElement.HighLight();
                if (GetGraphElementByDescriptionId(fromElement.Description.Parent.Id) is TtDescriptionGraphElement parentElement)
                {
                    highLightList.Add(parentElement);
                    parentElement.HighLight();
                }
            }
            if (highlightLine.To is TtDescriptionGraphElement toElement)
            {
                highLightList.Add(toElement);
                toElement.HighLight();
                if (GetGraphElementByDescriptionId(toElement.Description.Parent.Id) is TtDescriptionGraphElement parentElement)
                {
                    highLightList.Add(parentElement);
                    parentElement.HighLight();
                }
            }
            foreach (var element in Elements)
            {
                if (!highLightList.Contains(element))
                {
                    if (element is TtDescriptionGraphElement graphElement)
                    {
                        graphElement.LowLight();
                        if (GetGraphElementByDescriptionId(graphElement.Description.Parent.Id) is TtDescriptionGraphElement parentElement)
                        {
                            parentElement.LowLight();
                        }
                    }
                }
            }
        }
        public void UnHighLightLine(TtGraphElement_Line highlightLine)
        {
            foreach (var element in Elements)
            {
                if (element is TtDescriptionGraphElement graphElement)
                {
                    graphElement.NormalLight();
                }
            }
        }

        #region IDraggable
        public bool CanDrag()
        {
            return true;
        }
        public void OnDragging(Vector2 delta)
        {
            Camera.Location += delta;
        }
        #endregion IDraggable

        #region IZoomable
        public void Zooming(float delta, FMouseEventContext eventContext)
        {
            var mousePos = eventContext.GraphElementRenderingContext.CameraTransform(eventContext.MouseAbsPos);
            var newScale = Camera.Scale + delta * 0.1f;
            Camera.Scale = Math.Clamp(newScale, 0.5f, 2.0f);
            var absPosAfter = eventContext.GraphElementRenderingContext.ViewportTransform(mousePos);
            var deltaMove = eventContext.MouseAbsPos - absPosAfter;
            Camera.Location += deltaMove;
        }
        #endregion
        #region ISelectable

        public bool HitCheck(ref FMouseEventContext context)
        {
            return true;
        }
        public virtual void OnSelected(ref FMouseEventContext context)
        {
            if (!CanMultiSelect)
            {
                foreach (var selectedElement in SelectedElements)
                {
                    selectedElement.IsSelected = false;
                }
                SelectedElements.Clear();
            }
            else
            {
               
            }
        }
        public virtual void OnUnSelected(ref FMouseEventContext context)
        {

        }
        public virtual void OnMouseOver(ref FMouseEventContext context)
        {

        }

        public virtual void OnMouseLeave(ref FMouseEventContext context)
        {

        }
        public Vector2 SelectingRectStartAbsPos = Vector2.Zero;
        public virtual void OnMouseLeftButtonDown(ref FMouseEventContext context)
        {
            SelectingRectStartAbsPos = context.MouseAbsPos;
            SelectingRect = new TtGraphElement_SelectingRect
            {
                AbsLocation = context.GraphElementRenderingContext.CameraTransform(context.MouseAbsPos)
            };
            ElementSelectedStatus.Clear();
            if(context.GraphElementRenderingContext.DesignedGraph.CanMultiSelect)
            {
                foreach(var element in context.GraphElementRenderingContext.DesignedGraph.Elements)
                {
                    ElementSelectedStatus.Add(element.Id, element.IsSelected);
                }
            }
        }

        public virtual void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
            if (SelectingRect != null)
            {
                SelectingRect = null;
            }
            if (PreviewLine != null)
            {
                var renderContext = context.GraphElementRenderingContext;
                TtGraphContextMenuHandler.Instance.HandleLinkedPinContextMenu(this, ref renderContext);
                PreviewLine = null;
            }
            ElementSelectedStatus.Clear();
        }

        public virtual void OnMouseRightButtonDown(ref FMouseEventContext context)
        {

        }

        public virtual void OnMouseRightButtonUp(ref FMouseEventContext context)
        {

        }
        Dictionary<Guid, bool> ElementSelectedStatus = new();
        public virtual void OnMouseMove(ref FMouseEventContext context)
        {
            if (SelectingRect != null)
            {
                var movePos = context.MouseAbsPos;
                var min = new Vector2(MathF.Min(SelectingRectStartAbsPos.X, movePos.X), MathF.Min(SelectingRectStartAbsPos.Y, movePos.Y));
                var max = new Vector2(MathF.Max(SelectingRectStartAbsPos.X, movePos.X), MathF.Max(SelectingRectStartAbsPos.Y, movePos.Y));
                context.MouseAbsRect = new Rect(min, new SizeF(max - min));
                if(!CanMultiSelect)
                {
                    foreach (var selectedElement in SelectedElements)
                    {
                        selectedElement.IsSelected = false;
                    }
                    SelectedElements.Clear();
                }
                var hitElements = TtMouseEventProcesser.Instance.ProcessSelectableElementsHitCheck(this, context.MouseAbsPos, ref context);
                foreach (var hit in hitElements)
                {
                    if (hit is IGraph)
                    {
                        continue;
                    }
                    
                    if (hit is TtDescriptionGraphElement hitGraphElement)
                    {
                        if(CanMultiSelect)
                        {
                            foreach (var element in context.GraphElementRenderingContext.DesignedGraph.Elements)
                            {
                                if(!hitElements.Contains(element) && element is TtDescriptionGraphElement graphElement)
                                {
                                    SelectedElements.Remove(graphElement);
                                    graphElement.IsSelected = ElementSelectedStatus[graphElement.Id];
                                }
                            }
                            if (ElementSelectedStatus.ContainsKey(hitGraphElement.Id))
                            {
                                hitGraphElement.IsSelected = !ElementSelectedStatus[hitGraphElement.Id];
                                SelectedElements.Add(hitGraphElement);
                            }
                        }
                        else
                        {
                            hitGraphElement.IsSelected = true;
                            SelectedElements.Add(hitGraphElement);
                        }
                    }

                }
            }
        }
        #endregion ISelectable
        #region IContextMeunable
        public virtual void ConstructLinkedPinContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {

        }
        public virtual void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {

        }
        public void SetContextMenuableId(TtPopupMenu popupMenu)
        {
            popupMenu.StringId = Name + "_" + Id + "_" + "ContextMenu";
        }
        #endregion IContextMeunable
        public virtual List<IGraphElement> EnumerateChild<T>() where T : class
        {
            List<IGraphElement> list = new List<IGraphElement>();
            foreach (var element in Elements)
            {
                if (element is T)
                {
                    list.Add(element);
                }
            }
            return list;
        }
        public virtual List<IGraphElement> EnumerateChildRverse<T>() where T : class
        {
            List<IGraphElement> list = EnumerateChild<T>();
            list.Reverse();
            return list;
        }
        public Vector2 CalculateAbsLocation(IGraphElement element)
        {
            if (element.Parent != null)
            {
                return element.Location + CalculateAbsLocation(element.Parent);
            }
            return element.Location;
        }
    }
}

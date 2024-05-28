using EngineNS.DesignMacross.Base.Description;

namespace EngineNS.DesignMacross.Base.Graph
{
    public abstract class TtGraph : IGraph, IGraphElementSelectable, IEnumChild, IContextMeunable
    {
        public Guid Id { get; set; } = Guid.Empty;
        public TtGraphViewport ViewPort { get; set; } = new TtGraphViewport();
        public TtGraphCamera Camera { get; set; } = new TtGraphCamera();
        public TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();
        public string Name { get=>Description.Name; set=>Description.Name = value; }
        public Vector2 Location { get; set; } = Vector2.Zero;
        public Vector2 AbsLocation { get => CalculateAbsLocation(this); }
        public SizeF Size { get; set; } = new SizeF(100,100);

        public IGraphElement Parent { get; set; } = null;
        public virtual IDescription Description { get; set; } = null;
        [Rtti.Meta]
        public List<IDescriptionGraphElement> Elements { get; set; } = new List<IDescriptionGraphElement>();
        public IGraphElementStyle Style { get; set; } = null;

        public TtGraph(IDescription description)
        {
            Id = description.Id;
            Description = description;
        }

        public abstract void ConstructElements(ref FGraphRenderingContext context);
        public abstract void AfterConstructElements(ref FGraphRenderingContext context);
        #region IDraggable
        public bool CanDrag()
        {
            return true;
        }
        public void OnDragging(Vector2 delta)
        {
            Camera.Location -= delta;
        }
        #endregion IDraggable
        #region ISelectable

        public bool HitCheck(Vector2 pos)
        {
            Rect rect = new Rect(Location, Size);
            //冗余一点
            Rect mouseRect = new Rect(pos - Vector2.One, new SizeF(1.0f, 1.0f));
            //return rect.IntersectsWith(mouseRect);
            return true;
        }
        public virtual void OnSelected(ref FGraphElementRenderingContext context)
        {

        }
        public virtual void OnUnSelected()
        {
           
        }
        public virtual void OnMouseOver(ref FGraphElementRenderingContext context)
        {
            
        }

        public virtual void OnMouseLeave(ref FGraphElementRenderingContext context)
        {
            
        }

        public virtual void OnMouseLeftButtonDown(ref FGraphElementRenderingContext context)
        {
            
        }

        public virtual void OnMouseLeftButtonUp(ref FGraphElementRenderingContext context)
        {
            
        }

        public virtual void OnMouseRightButtonDown(ref FGraphElementRenderingContext context)
        {
            
        }

        public virtual void OnMouseRightButtonUp(ref FGraphElementRenderingContext context)
        {
            
        }
        #endregion ISelectable
        #region IContextMeunable

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

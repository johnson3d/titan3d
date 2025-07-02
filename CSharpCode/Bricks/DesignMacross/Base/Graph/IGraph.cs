using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Base.Graph
{
    public class GraphAttribute : Attribute
    {
        public TtTypeDesc ClassType { get; set; }
        public GraphAttribute(Type type)
        {
            ClassType = TtTypeDesc.TypeOf(type);
        }
        public static GraphAttribute GetAttributeWithSpecificClassType<T>(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(GraphAttribute), false);
            foreach (var attr in attrs)
            {
                var graphAttribute = attr as GraphAttribute;
                if (graphAttribute.ClassType.SystemType.IsAssignableTo(typeof(T)))
                {
                    return graphAttribute;
                }
            }
            return null;
        }
    }
    public class GraphElementAttribute : Attribute
    {
        public TtTypeDesc ClassType { get; set; }
        public Vector2 DefaultLocation { get; set; }
        public GraphElementAttribute(Type type)
        {
            ClassType = TtTypeDesc.TypeOf(type);
            DefaultLocation = Vector2.Zero;
        }
        public GraphElementAttribute(Type type, float defaultLocationX, float defaultLocationY)
        {
            ClassType = TtTypeDesc.TypeOf(type);
            DefaultLocation = new Vector2(defaultLocationX, defaultLocationY);
        }
        public static GraphElementAttribute GetAttributeWithSpecificClassType<T>(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(GraphElementAttribute), false);
            foreach (var attr in attrs)
            {
                var graphElementAttribute = attr as GraphElementAttribute;
                if (graphElementAttribute.ClassType.SystemType.IsAssignableTo(typeof(T)))
                {
                    return graphElementAttribute;
                }
            }
            return null;
        }
    }
    public class DrawInGraphAttribute : Attribute
    {
        
    }

    public enum EHorizontalAlignment
    {
        Left,
        Center,
        Right,
        Stretch
    }
    public enum EVerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Stretch
    }

    public interface ILayoutable
    {
        public FMargin Margin { get; set; }
        public SizeF Size { get; set; }
        public SizeF MinSize { get; set; }
        public SizeF MaxSize { get; set; }
        public EHorizontalAlignment HorizontalAlignment { get; set; }
        public EVerticalAlignment VerticalAlignment { get; set; } 
        public SizeF Measuring(SizeF availableSize);
        public SizeF Arranging(Rect finalRect);
    }

    public interface IEnumChild
    {
        public List<IGraphElement> EnumerateChild<T>() where T : class;
    }
    public interface IZoomable
    {
        public void Zooming(float delta)
        {

        }
    }
    public interface IGraphElementDraggable
    {
        public bool CanDrag();
        public void OnDragging(Vector2 delta);
    }
    public struct FMouseEventContext
    {
        public FGraphElementRenderingContext GraphElementRenderingContext;
        public Vector2 MouseAbsPos;
    }
    public interface IGraphElementSelectable : IGraphElementDraggable
    {
        public bool HitCheck(ref FMouseEventContext context);
        public void OnSelected(ref FMouseEventContext context);
        public void OnUnSelected();
        public void OnMouseOver(ref FMouseEventContext context);
        public void OnMouseLeave(ref FMouseEventContext context);
        public void OnMouseLeftButtonDown(ref FMouseEventContext context);
        public void OnMouseLeftButtonUp(ref FMouseEventContext context);
        public void OnMouseRightButtonDown(ref FMouseEventContext context);
        public void OnMouseRightButtonUp(ref FMouseEventContext context);
    }
    public interface IResizebale
    {
    }
    public interface IErrorable
    {
        public bool HasError { get; set; }
    }
    public interface IContextMeunable
    {
        public void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu);
        public void SetContextMenuableId(TtPopupMenu popupMenu);
        //public void OpenContextMeun();
        //public void DrawContextMenu(ref FGraphElementRenderingContext context);
    }

    public interface IGraphElement : IRenderableElement, IGraphElementSelectable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get;}
        public SizeF Size { get; set; }
        public IGraphElement Parent { get; set; }
        public IGraphElementStyle Style { get; set; }
    }
    public class GraphElementStyleAttribute : Attribute
    {
        public TtTypeDesc GraphElementType { get; set; }
        public GraphElementStyleAttribute(Type graphElementType)
        {
            GraphElementType = TtTypeDesc.TypeOf(graphElementType);
        }
    }
    public interface IGraphElementStyle : IO.ISerializer
    {
        [Rtti.Meta("")]
        public Vector2 Location { get; set; }
        [Rtti.Meta("")]
        public SizeF Size { get; set; }
        public Color4f BackgroundColor { get; set; }

    }

    public interface IDescriptionGraphElement : IGraphElement
    {
        public IDescription Description { get; set; }
        public void ConstructElements(ref FGraphElementRenderingContext context);
        public void AfterConstructElements(ref FGraphElementRenderingContext context);
    }
    public interface IWidgetGraphElement : IGraphElement
    {

    }

    public interface IGraph : IGraphElement, IZoomable
    {
        public void ConstructElements(ref FGraphRenderingContext context);
        public void AfterConstructElements(ref FGraphRenderingContext context);
    }
}

using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using NPOI.OpenXml4Net.OPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Base.Graph
{
    public class GraphAttribute : Attribute
    {
        public UTypeDesc ClassType { get; set; }
        public GraphAttribute(Type type)
        {
            ClassType = UTypeDesc.TypeOf(type);
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
        public UTypeDesc ClassType { get; set; }
        public GraphElementAttribute(Type type)
        {
            ClassType = UTypeDesc.TypeOf(type);
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

    public interface ILayoutable
    {
        public FMargin Margin { get; set; }
        public SizeF Size { get; set; }
        public SizeF Measuring(SizeF availableSize);
        public SizeF Arranging(Rect finalRect);
    }

    public interface IEnumChild
    {
        public List<IGraphElement> EnumerateChild<T>() where T : class;
    }
    public interface IZoomable
    {
    }
    public interface IGraphElementDraggable
    {
        public bool CanDrag();
        public void OnDragging(Vector2 delta);
    }
    public interface IGraphElementSelectable : IGraphElementDraggable
    {
        public bool HitCheck(Vector2 pos);
        public void OnSelected(ref FGraphElementRenderingContext context);
        public void OnUnSelected();
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
        public void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu);
        public void SetContextMenuableId(TtPopupMenu PopupMenu);
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
    public interface IGraphElementStyle : IO.ISerializer
    {
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        [Rtti.Meta]
        public SizeF Size { get; set; }

    }

    public interface IDescriptionGraphElement : IGraphElement
    {
        public IDescription Description { get; set; }
        public void ConstructElements(ref FGraphElementRenderingContext context);
    }
    public interface IWidgetGraphElement : IGraphElement
    {

    }

    public interface IGraph : IGraphElement
    {
        public void ConstructElements(ref FGraphRenderingContext context);
    }
}

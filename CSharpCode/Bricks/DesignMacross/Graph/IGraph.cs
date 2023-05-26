using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Render;
using EngineNS.Rtti;
using NPOI.OpenXml4Net.OPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Graph
{
    public class GraphElementAttribute : Attribute
    {
        public Type ClassType { get; set; }
        public GraphElementAttribute(Type type)
        {
            ClassType = type;
        }
    }
    public interface IMouseEvent
    {

    }
    public interface IEnumChild
    {
        public List<IGraphElement> EnumerateChild<T>() where T : class;
    }
    public interface IZoomable
    {
    }
    public interface IDraggable
    {
        public bool CanDrag();
        public void OnDragging(Vector2 delta);
    }
    public interface ISelectable
    {
        public bool HitCheck(Vector2 pos);
        public void OnSelected();
        public void OnUnSelected();
    }
    public interface IResizebale
    {
    }
    public interface IContextMeunable
    {
        public TtPopupMenu PopupMenu { get; set; }
        public void UpdateContextMenu(ref FGraphElementRenderingContext context);
        public void OpenContextMeun();
        public void DrawContextMenu(ref FGraphElementRenderingContext context);
    }
    public struct FMargin
    {
        public static readonly FMargin Default = new FMargin(0,0,0,0);

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
    public interface ILayoutable
    {
        public FMargin Margin { get; set; }
        public SizeF Size { get; set; }
        public SizeF Measuring(SizeF availableSize);
        public SizeF Arranging(Rect finalRect);
    }
    
    public interface IGraphElement : IRenderableElement, ISelectable
    {
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get; }
        public SizeF Size { get; set; }

        public IGraphElement Parent { get; set; }
        public IDescription Description { get; set; }
        public void Construct();

    }

    public interface IGraph : IGraphElement
    {

    }
    public class TtGraphCamera
    {
        public Vector2 Location { get; set; } = Vector2.Zero;
        public SizeF Size { get; set; } = new SizeF(100, 100);
        public Vector2 Scale { get; set; } = Vector2.One;
    }
    public class TtGraphViewPort
    {
        //视口 screen space
        public Vector2 Location { get; set; } = Vector2.Zero;
        public SizeF Size { get; set; } = new SizeF(100, 100);
        public bool IsInViewPort(Vector2 screenPos)
        {
            Rect veiwPortRect = new Rect(Location, Size);
            return veiwPortRect.Contains(screenPos);
        }

        public Vector2 ViewPortTransform(Vector2 cameraPos, Vector2 pos)
        {
            return pos - cameraPos + Location;
        }
        public Vector2 ViewPortInverseTransform(Vector2 cameraPos, Vector2 pos)
        {
            return pos + cameraPos - Location;
        }
    }
}

using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Render;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Base.Graph
{

    public struct FGraphRenderingContext
    {
        public TtCommandHistory CommandHistory { get; set; }
        public TtEditorInteroperation EditorInteroperation { get; set; }
        public TtGraphViewport ViewPort { get; set; }
        public TtGraphCamera Camera { get; set; }
        public TtGraphElementStyleCollection GraphElementStyleManager { get; set; }
        public Dictionary<Guid, IGraphElement> DescriptionsElement { get; set; }
        public Vector2 ViewPortTransform(Vector2 pos)
        {
            return ViewPort.ViewportTransform(Camera.Location, pos);
        }
        public Vector2 ViewPortInverseTransform(Vector2 pos)
        {
            return ViewPort.ViewportInverseTransform(Camera.Location, pos);
        }
    }
    public struct FGraphElementRenderingContext
    {
        public TtCommandHistory CommandHistory { get; set; }
        public TtEditorInteroperation EditorInteroperation { get; set; }
        public TtGraphViewport ViewPort { get; set; }
        public TtGraphCamera Camera { get; set; }
        public TtGraphElementStyleCollection GraphElementStyleManager { get; set; }
        public Dictionary<Guid, IGraphElement> DescriptionsElement { get; set; }
        public Vector2 ViewPortTransform(Vector2 pos)
        {
            return ViewPort.ViewportTransform(Camera.Location, pos);
        }
        public Vector2 ViewPortInverseTransform(Vector2 pos)
        {
            return ViewPort.ViewportInverseTransform(Camera.Location, pos);
        }
        public float ViewPortScale(float value)
        {
            return Camera.Scale.X * value;
        }
    }
    public interface IGraphElementRender : IElementRender<FGraphElementRenderingContext>
    {
        //public void Draw(ref FGraphElementRenderingContext context);
    }

    public interface IGraphRender : IElementRender<FGraphRenderingContext>
    {
        //public IGraph Graph { get; set; }
        //public void Draw(ref FGraphRenderingContext context);
    }
}

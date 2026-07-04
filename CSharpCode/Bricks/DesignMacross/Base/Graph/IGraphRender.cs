using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Editor;
using NPOI.SS.Formula.Functions;

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
        public TtClassDescription DesignedClassDescription { get; set; }
        public Vector2 ViewPortTransform(Vector2 pos)
        {
            return ViewPort.ViewportTransform(Camera.Location, pos);
        }
        public Vector2 ScreenTransform(Vector2 viewportTransform)
        {
            return ((viewportTransform - ViewPort.Location) + Camera.Location) / Camera.Scale;
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
        public TtClassDescription DesignedClassDescription { get; set; }
        public TtGraph DesignedGraph { get; set; }
        public Vector2 ViewportTransform(Vector2 pos)
        {
            return ViewPort.ViewportTransform(Camera.Location, pos * Camera.Scale);
        }
        public Vector2 CameraTransform(Vector2 viewportTransform)
        {
            return ((viewportTransform - ViewPort.Location) - Camera.Location) / Camera.Scale;
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

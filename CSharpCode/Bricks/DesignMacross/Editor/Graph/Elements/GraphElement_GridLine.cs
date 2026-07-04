using Assimp;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.NxRHI;

namespace EngineNS.DesignMacross.Editor
{
    [ImGuiElementRender(typeof(TtGraphElementRender_GridLine))]
    public class TtGraphElement_GridLine : TtWidgetGraphElement
    {
        public bool IsInfinitiSize = true;

        public override bool CanDrag()
        {
            return false;
        }

        public override bool HitCheck(ref FMouseEventContext context)
        {
            return false;
        }

        public override void OnDragging(Vector2 delta)
        {

        }

        public override void OnSelected(ref FMouseEventContext context)
        {

        }

        public override void OnUnSelected(ref FMouseEventContext context)
        {

        }

    }
    public class TtGraphElementRender_GridLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var gridLine = renderableElement as TtGraphElement_GridLine;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var styles = UNodeGraphStyles.DefaultStyles;

            var step = styles.GridStep * context.Camera.Scale;
            var hCount = (int)(context.Camera.Size.Width / step);
            var vCount = (int)(context.Camera.Size.Height / step);

            if (gridLine.IsInfinitiSize)
            {
                var gridRect = new Rect(context.ViewPort.Location, gridLine.Size);
                cmd.AddRectFilled(gridRect.TopLeft, gridRect.BottomRight, styles.GridBackgroundColor, 0, ImDrawFlags_.ImDrawFlags_None);

                var gridStart = (context.ViewPort.Location - context.ViewPort.Location) - context.Camera.Location;
                var gridLineRect = new Rect(gridStart, new SizeF(context.Camera.Size.Width, context.Camera.Size.Height));
                var offSet = gridStart / step;
                var startX = (int)Math.Ceiling(offSet.X);
                for (int i = startX; i < hCount + startX; i++)
                {
                    cmd.AddLine(
                          context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(i * step, gridLineRect.Top)),
                          context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(i * step, gridLineRect.Bottom)),
                          (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, (i % 8 == 0) ? 1.5f : 1.0f);
                }
                var startY = (int)Math.Ceiling(offSet.Y);
                for (int i = startY; i < vCount + startY; i++)
                {
                    cmd.AddLine(
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(gridLineRect.Left, i * step)),
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(gridLineRect.Right, i * step)),
                        (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, (i % 8 == 0) ? 1.5f : 1.0f);
                }
            }
            else
            {
                var cameraRect = new Rect(context.Camera.Location, context.Camera.Size);
                var viewPortLoc = context.ViewportTransform(gridLine.Location);
                var gridRect = new Rect(viewPortLoc, gridLine.Size);
                cmd.AddRectFilled(gridRect.TopLeft, gridRect.BottomRight, styles.GridBackgroundColor, 0, ImDrawFlags_.ImDrawFlags_None);
                for (int i = 0; i < hCount; i++)
                {
                    cmd.AddLine(
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(gridLine.Location.X + i * step, cameraRect.Top)),
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(gridLine.Location.X + i * step, cameraRect.Bottom)),
                        (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, 1.0f);
                }
                for (int i = 0; i < vCount; i++)
                {
                    cmd.AddLine(
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(cameraRect.Left, gridLine.Location.Y + i * step)),
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(cameraRect.Right, gridLine.Location.Y + i * step)),
                        (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, 1.0f);
                }
            }
        }
    }
}

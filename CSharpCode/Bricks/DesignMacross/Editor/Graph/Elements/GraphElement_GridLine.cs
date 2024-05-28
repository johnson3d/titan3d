using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

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

        public override bool HitCheck(Vector2 pos)
        {
            return false;
        }

        public override void OnDragging(Vector2 delta)
        {

        }

        public override void OnSelected(ref FGraphElementRenderingContext context)
        {

        }

        public override void OnUnSelected()
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
            var gridRect = new Rect(gridLine.Location, gridLine.Size);
            cmd.AddRectFilled(gridRect.TopLeft, gridRect.BottomRight, styles.GridBackgroundColor, 0, ImDrawFlags_.ImDrawFlags_None);
            var step = styles.GridStep;
            var hCount = (int)(context.Camera.Size.Width / step);
            var vCount = (int)(context.Camera.Size.Height / step);
            var cameraRect = new Rect(context.Camera.Location, context.Camera.Size);
            var gridStart = context.Camera.Location;
            if (gridLine.IsInfinitiSize)
            {
                var offSet = gridStart / step;
                var startX = (int)Math.Ceiling(offSet.X);
                for (int i = startX; i < hCount + startX; i++)
                {
                    cmd.AddLine(
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(i * step, cameraRect.Top)),
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(i * step, cameraRect.Bottom)),
                        (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, 1.0f);
                }
                var startY = (int)Math.Ceiling(offSet.Y);
                for (int i = startY; i < vCount + startY; i++)
                {
                    cmd.AddLine(
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(cameraRect.Left, i * step)),
                        context.ViewPort.ViewportTransform(context.Camera.Location, new Vector2(cameraRect.Right, i * step)),
                        (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, 1.0f);
                }
            }
            else
            {
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

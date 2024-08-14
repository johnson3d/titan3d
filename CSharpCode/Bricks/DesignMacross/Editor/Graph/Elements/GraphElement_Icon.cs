using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.DesignMacross.Editor
{
    [ImGuiElementRender(typeof(TtGraphElementRender_Icon))]
    public class TtGraphElement_Icon : TtWidgetGraphElement, ILayoutable
    {
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Left;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Top;
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0, 0);
        public float Rounding { get; set; } = 5;
        public RName IconName { get; set; }
        public Vector2 AbsCenter { get { return AbsLocation + new Vector2(Size.Width, Size.Height) / 2.0f; } }
        public TtGraphElement_Icon()
        {

        }

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

        #region ILayoutable
        public FMargin Margin { get; set; } = FMargin.Default;
        public override SizeF Size { get; set; }

        public SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(Size.Width + Margin.Left + Margin.Right, Size.Height + Margin.Top + Margin.Bottom);
        }

        public SizeF Arranging(Rect finalRect)
        {
            Size = new SizeF(finalRect.Width, finalRect.Height);
            Location = finalRect.Location + new Vector2(Margin.Left, Margin.Top);
            return finalRect.Size;
        }  
        #endregion ILayoutable

    }
    public class TtGraphElementRender_Icon : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pinElement = renderableElement as TtGraphElement_Icon;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(pinElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(pinElement.AbsLocation + new Vector2(pinElement.Size.Width, pinElement.Size.Height));
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(pinElement.BackgroundColor), pinElement.Rounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            var styles = UNodeGraphStyles.DefaultStyles;
            EGui.TtUVAnim icon = new EGui.TtUVAnim();
            icon.TextureName = pinElement.IconName;
            icon.Size = new Vector2(pinElement.Size.Width, pinElement.Size.Height);
            icon.OnDraw(cmdlist, nodeStart, nodeEnd, 0);
        }
    }
}
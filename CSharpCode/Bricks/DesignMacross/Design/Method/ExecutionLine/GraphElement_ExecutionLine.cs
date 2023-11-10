using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    public class TtGraphElement_ExecutionLine
    {
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_ExecutionPin))]
    public class TtGraphElement_ExecutionPin : TtDescriptionGraphElement
    {
        public override FMargin Margin { get; set; } = new FMargin(5, 8, 0, 5);
        public TtGraphElement_ExecutionPin(IDescription description, IGraphElementStyle style) : base(description, style)
        {

        }
        public override SizeF Arranging(Rect finalRect)
        {
            var hLocation = finalRect.Width - Size.Width;
            Location = new Vector2(hLocation + finalRect.X, Location.Y);
            return finalRect.Size;
        }

        public override SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(Size.Width + Margin.Left + Margin.Right, Size.Height + Margin.Top + Margin.Bottom);
        }

        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            
        }
    }
    public class TtGraphElementRender_ExecutionLine
    {
    }
    public class TtGraphElementRender_ExecutionPin : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pinElement = renderableElement as TtGraphElement_ExecutionPin;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(pinElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(pinElement.AbsLocation + new Vector2(pinElement.Size.Width, pinElement.Size.Height));
           
            var styles = UNodeGraphStyles.DefaultStyles;
            EGui.UUvAnim icon = new EGui.UUvAnim();
            icon.TextureName = RName.GetRName(styles.PinDisconnectedExecImg, RName.ERNameType.Engine);
            icon.Size = new Vector2(pinElement.Size.Width, pinElement.Size.Height);
            icon.OnDraw(cmdlist, nodeStart, nodeEnd, 0);
        }
    }
}

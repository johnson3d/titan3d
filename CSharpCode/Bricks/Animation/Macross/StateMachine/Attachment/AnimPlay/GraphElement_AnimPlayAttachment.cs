using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
{
    [ImGuiElementRender(typeof(TtGraphElementRender_AnimPlayAttachment))]
    public class TtGraphElement_AnimPlayAttachment : TtDescriptionGraphElement, ILayoutable
    {
        public float Duration { get; set; } = 3.0f;
        public override FMargin Margin { get; set; } = new FMargin(0, 0, 0, 5);
        public Color4f BackgroundColor { get; set; } = new Color4f(240f / 255, 225f / 255, 102f / 255);
        public TtGraphElement_AnimPlayAttachment(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            Size = new SizeF(140, 30);
        }
        public override SizeF Arranging(Rect finalRect)
        {
            Location = finalRect.TopLeft + new Vector2(Margin.Left, Margin.Top);
            return finalRect.Size;
        }

        public override SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(Size.Width + Margin.Left + Margin.Right, Size.Height + Margin.Top + Margin.Bottom);
        }
    }

    public class TtGraphElementRender_AnimPlayAttachment : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var attachmentElement = renderableElement as TtGraphElement_AnimPlayAttachment;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var start = context.ViewportTransform(attachmentElement.AbsLocation);
            var end = context.ViewportTransform(attachmentElement.AbsLocation + new Vector2(attachmentElement.Size.Width, attachmentElement.Size.Height));
            var elementHeight = end.Y - start.Y;
            cmd.AddRectFilled(start, end, ImGuiAPI.ColorConvertFloat4ToU32(attachmentElement.BackgroundColor), 0, ImDrawFlags_.ImDrawFlags_RoundCornersNone);

            var oldScale = ImGuiAPI.GetFont().Scale;
            var font = ImGuiAPI.GetFont();
            font.Scale = oldScale * context.Camera.Scale;
            ImGuiAPI.PushFont(font);
            //cmd.AddText(start, textBlock.TextColor, textBlock.Content, null);
            var nameSize = ImGuiAPI.CalcTextSize(attachmentElement.Name, false, 0);
            var nameTextLocation = start;
            nameTextLocation.Y += (elementHeight - nameSize.Y) / 2;
            cmd.AddText(nameTextLocation, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(0, 0, 0)), attachmentElement.Name, null);
            font.Scale = oldScale;
            ImGuiAPI.PopFont();

        }
    }
}

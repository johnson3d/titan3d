using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.EGui.Controls;

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
{
    [ImGuiElementRender(typeof(TtGraphElementRender_TimedStateScriptAttachment))]
    public class TtGraphElement_TimedStateScriptAttachment : TtDescriptionGraphElement
    {        
        public TtTimedStateScriptAttachmentClassDescription TimedStateScriptAttachmentClassDescription { get=> Description as TtTimedStateScriptAttachmentClassDescription; }
        public float Duration { get; set; } = 3.0f;
        public override FMargin Margin { get; set; } = new FMargin(0, 0, 0, 5);
        public Color4f BackgroundColor { get; set; } = new Color4f(240f / 255, 225f / 255, 102f / 255);
        public TtGraphElement_TimedStateScriptAttachment(IDescription description, IGraphElementStyle style) : base(description, style)
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

        #region IContextMeunable
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var editorInteroperation = context.EditorInteroperation;
            parentMenu.AddMenuItem("Open Script Init Graph", null, (TtMenuItem item, object sender) =>
            {
                editorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(TimedStateScriptAttachmentClassDescription.InitMethodDescription);
            });
            parentMenu.AddMenuItem("Open Script Tick Graph", null, (TtMenuItem item, object sender) =>
            {
                editorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(TimedStateScriptAttachmentClassDescription.TickMethodDescription);
            });
        }
        #endregion IContextMeunable

        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            base.ConstructElements(ref context);
        }
    }

    public class TtGraphElementRender_TimedStateScriptAttachment : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var attachmentElement = renderableElement as TtGraphElement_TimedStateScriptAttachment;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var start = context.ViewPortTransform(attachmentElement.AbsLocation);
            var end = context.ViewPortTransform(attachmentElement.AbsLocation + new Vector2(attachmentElement.Size.Width, attachmentElement.Size.Height));
            cmd.AddRectFilled(start, end, ImGuiAPI.ColorConvertFloat4ToU32(attachmentElement.BackgroundColor), 0, ImDrawFlags_.ImDrawFlags_RoundCornersNone);
            var nameSize = ImGuiAPI.CalcTextSize(attachmentElement.Name, false, 0);
            var nameTextLocation = start;
            nameTextLocation.Y += (attachmentElement.Size.Height - nameSize.Y) / 2;
            cmd.AddText(nameTextLocation, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(0, 0, 0)), attachmentElement.Name, null);
        }
    }
}

using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.TimedStateMachine.StateAttachment;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [ImGuiElementRender(typeof(TtGraphElementRender_TimedStateScriptAttachment))]
    public class TtGraphElement_TimedStateScriptAttachment : TtDescriptionGraphElement
    {        
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
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {;
            PopupMenu.Reset();
            var parentMenu = PopupMenu.Menu;
            var editorInteroperation = context.EditorInteroperation;
            parentMenu.AddMenuItem("Open Script Graph", null, (UMenuItem item, object sender) =>
            {
                editorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(Description);
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
        TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("GraphElementRender_TimedStateScriptAttachment");
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

            if (attachmentElement is IContextMeunable meunablenode)
            {
                meunablenode.SetContextMenuableId(PopupMenu);
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                    && context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                {
                    var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    if (attachmentElement.HitCheck(pos))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        meunablenode.ConstructContextMenu(ref context, PopupMenu);
                        PopupMenu.OpenPopup();
                    }
                }
                PopupMenu.Draw(ref context);
            }
        }
    }
}

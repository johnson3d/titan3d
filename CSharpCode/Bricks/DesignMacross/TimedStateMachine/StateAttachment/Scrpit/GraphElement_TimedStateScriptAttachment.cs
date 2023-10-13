using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Graph.Elements;
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
    public class TtGraphElement_TimedStateScriptAttachment : IGraphElement_TimedStateAttachment, IContextMeunable
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get => Description.Name; set => Description.Name = value; }
        public float Duration { get; set; } = 3.0f;
        public Vector2 Location { get => Description.Location; set => Description.Location = value; }
        public Vector2 AbsLocation { get => TtGraphMisc.CalculateAbsLocation(this); }
        public SizeF Size { get; set; } = new SizeF(140, 30);
        public IGraphElement Parent { get; set; }
        public IDescription Description { get; set; }
        public FMargin Margin { get; set; } = new FMargin(0, 0, 0, 5);
        public Color4f BackgroundColor { get; set; } = new Color4f(240f / 255, 225f / 255, 102f / 255);
        public TtGraphElement_TimedStateScriptAttachment(IDescription description)
        {
            Id = description.Id;
            Description = description;
        }

        public SizeF Arranging(Rect finalRect)
        {
            Location = finalRect.TopLeft + new Vector2(Margin.Left, Margin.Top); 
            return finalRect.Size;
        }
        public void Construct()
        {
            
        }


        public bool HitCheck(Vector2 pos)
        {
            Rect rect = new Rect(AbsLocation, Size);
            //冗余一点
            Rect mouseRect = new Rect(pos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(mouseRect);
        }

        public SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(Size.Width + Margin.Left + Margin.Right, Size.Height + Margin.Top + Margin.Bottom);
        }

        public void OnSelected()
        {
            throw new NotImplementedException();
        }

        public void OnUnSelected()
        {
            throw new NotImplementedException();
        }
        #region IContextMeunable
        public TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("TimedStateAttachmentContextMenu");
        public void OpenContextMeun()
        {
            PopupMenu.OpenPopup();
        }

        public void UpdateContextMenu(ref FGraphElementRenderingContext context)
        {
            PopupMenu.StringId = Name + "_ContextMenu";
            PopupMenu.Reset();
            var parentMenu = PopupMenu.Menu;
            var editorInteroperation = context.EditorInteroperation;
            parentMenu.AddMenuItem("Open Script Graph", null, (UMenuItem item, object sender) =>
            {
                editorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(Description);
            });
        }
        public void DrawContextMenu(ref FGraphElementRenderingContext context)
        {
            PopupMenu.Draw(ref context);
        }

        #endregion IContextMeunable
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

            if (attachmentElement is IContextMeunable meunablenode)
            {
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                    && context.ViewPort.IsInViewPort(ImGuiAPI.GetMousePos()))
                {
                    var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    if (attachmentElement.HitCheck(pos))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        meunablenode.UpdateContextMenu(ref context);
                        meunablenode.OpenContextMeun();
                    }
                }
                meunablenode.DrawContextMenu(ref context);
            }
        }
    }
}

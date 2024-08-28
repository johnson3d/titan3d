﻿using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtGraphElementRender_BlendTreeNode))]
    public class TtGraphElement_BlendTree_AnimStateMachine : TtGraphElement_BlendTreeNode
    {
        public TtGraphElement_BlendTree_AnimStateMachine(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            
        }

    }

    public class TtGraphElementRender_BlendTree_AnimStateMachine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var attachmentElement = renderableElement as TtGraphElement_BlendTree_AnimStateMachine;
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

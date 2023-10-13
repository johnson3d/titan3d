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
    public class TtGraphElement_ExecutionPin : IGraphElement, ILayoutable
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get => Description.Name; set => Description.Name = value; }
        public Vector2 Location { get => Description.Location; set => Description.Location = value; }

        public Vector2 AbsLocation => TtGraphMisc.CalculateAbsLocation(this);

        public SizeF Size { get; set; } = new SizeF(15, 15);
        public IGraphElement Parent { get; set; } = null;
        public IDescription Description { get; set; } = null;
        public FMargin Margin { get; set; } = new FMargin(5, 8, 0, 5);
        public TtGraphElement_ExecutionPin(IDescription description)
        {
            Id = description.Id;
            Description = description;
        }
        public SizeF Arranging(Rect finalRect)
        {
            var hLocation = finalRect.Width - Size.Width;
            Location = new Vector2(hLocation + finalRect.X, Location.Y);
            return finalRect.Size;
        }

        public void Construct()
        {
            
        }

        public bool HitCheck(Vector2 pos)
        {
            throw new NotImplementedException();
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

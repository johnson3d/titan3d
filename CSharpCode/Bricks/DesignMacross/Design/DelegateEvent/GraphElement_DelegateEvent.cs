using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.DesignMacross.Design
{
    [ImGuiElementRender(typeof(TtGraphElementRender_DelegateEvent))]
    public class TtGraphElement_DelegateEvent : TtDescriptionGraphElement
    {
        public float Rounding { get; set; } = 15;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(255f / 255, 168f / 255, 219f / 255);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.6f, 0.6f, 0.6f);
        public float BorderThickness { get; set; } = 4;
        public TtDelegateEventDescription DelegateEventDescription { get => Description as TtDelegateEventDescription; }

        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel PinsStackPanel = new TtGraphElement_StackPanel();
        
        public TtGraphElement_DelegateEvent(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            NameTextBlock.Content = Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.FontScale = 1.2f;
            ElementContainer.AddElement(NameTextBlock);

            ElementContainer.AddElement(PinsStackPanel);

        }


        public void Construct()
        {
            //PinsStackPanel.Clear();
            //{
            //    var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(DelegateEventDescription.OutExecutionPin.GetType());
            //    var instance = UTypeDescManager.CreateInstance(graphElementAttribute.ClassType, new object[] { DelegateEventDescription.OutExecutionPin }) as IGraphElement;
            //    instance.Parent = this;
            //    instance.Description = DelegateEventDescription.OutExecutionPin;
            //    instance.Construct();
            //    PinsStackPanel.AddElement(instance);
            //}
            //foreach (var dataPin in DelegateEventDescription.DataPins)
            //{
            //    var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(dataPin.GetType());
            //    if (graphElementAttribute != null)
            //    {
            //        var instance = UTypeDescManager.CreateInstance(graphElementAttribute.ClassType, new object[] { dataPin }) as IGraphElement;
            //        instance.Parent = this;
            //        instance.Description = dataPin;
            //        instance.Construct();
            //        PinsStackPanel.AddElement(instance);
            //    }
            //}
        }
        public override SizeF Arranging(Rect finalRect)
        {
            Size = new SizeF(finalRect.Width, finalRect.Height);
            ElementContainer.Arranging(new Rect(Vector2.Zero, finalRect.Size));
            return finalRect.Size;
        }
        public override SizeF Measuring(SizeF availableSize)
        {
            var childrenDesiredSize = ElementContainer.Measuring(availableSize);
            var size = new SizeF();
            size.Width = Size.Width > childrenDesiredSize.Width ? Size.Width : childrenDesiredSize.Width;
            size.Height = childrenDesiredSize.Height;
            return new SizeF(size.Width + Margin.Left + Margin.Right, size.Height + Margin.Top + Margin.Bottom);
        }

    }

    public class TtGraphElementRender_DelegateEvent : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var delegateElement = renderableElement as TtGraphElement_DelegateEvent;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(delegateElement.AbsLocation);
            //Element.Size = new SizeF(nameTextSize.Width , nameTextSize.Height < minHeight ? minHeight : nameTextSize.Height);
            var nodeEnd = context.ViewPortTransform(delegateElement.AbsLocation + new Vector2(delegateElement.Size.Width, delegateElement.Size.Height));
            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersTopRight;
            cmdlist.AddRect(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(delegateElement.BorderColor), delegateElement.Rounding, roundCornerFlags, delegateElement.BorderThickness * 2);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(delegateElement.BackgroundColor), delegateElement.Rounding, roundCornerFlags);
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(delegateElement.ElementContainer);
            elementContainerRender.Draw(delegateElement.ElementContainer, ref context);
            
        }
    }
}

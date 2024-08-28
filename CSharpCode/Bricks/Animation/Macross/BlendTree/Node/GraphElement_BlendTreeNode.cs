using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Editor;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtGraphElementRender_BlendTreeNode))]
    public class TtGraphElement_BlendTreeNode : TtDescriptionGraphElement, IEnumChild
    {
        public TtBlendTreeNodeClassDescription BlendTreeNodeClassDescription { get => Description as TtBlendTreeNodeClassDescription; }
        public float Rounding { get; set; } = 5;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(0.5f, 188f / 255, 212f / 255, 240f / 255);
        //public Color4f BackgroundColor { get; set; } = new Color4f(0.5f, 0.9f, 0.9f, 0.9f);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.9f, 0.9f, 0.9f);
        public float BorderThickness { get; set; } = 2;
        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel BlendTreeNodeDescStackPanel = new TtGraphElement_StackPanel();
        //public TtGraphElement_StackPanel PinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_DockPanel PinsPanel = new();
        public TtGraphElement_StackPanel LeftSidePinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel RightSidePinsStackPanel = new TtGraphElement_StackPanel();

        public TtGraphElement_BlendTreeNode(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            MinSize = new SizeF(100, 80);
            MaxSize = new SizeF(200, 160);
            Size = new SizeF(100, 80);
            ElementContainer.HorizontalAlignment = EHorizontalAlignment.Stretch;
            ElementContainer.VerticalAlignment = EVerticalAlignment.Stretch;

            BlendTreeNodeDescStackPanel.Margin = new FMargin(2, 2, 2, 2);
            BlendTreeNodeDescStackPanel.HorizontalAlignment = EHorizontalAlignment.Left;
            BlendTreeNodeDescStackPanel.Parent = ElementContainer;
            ElementContainer.AddElement(BlendTreeNodeDescStackPanel);

            NameTextBlock.Content = BlendTreeNodeClassDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.BackgroundColor = new Color4f(0, 0.8f, 0);
            BlendTreeNodeDescStackPanel.AddElement(NameTextBlock);

            PinsPanel.Margin = new FMargin(0, 0, 0, 0);
            PinsPanel.Parent = ElementContainer;
            PinsPanel.VerticalAlignment = EVerticalAlignment.Stretch;
            ElementContainer.AddElement(PinsPanel);

            LeftSidePinsStackPanel.Margin = new FMargin(0, 0, 0, 0);
            LeftSidePinsStackPanel.Parent = PinsPanel;
            LeftSidePinsStackPanel.Orientation = EOrientation.Vertical;
            PinsPanel.AddElement(EDockPosition.Left, LeftSidePinsStackPanel);

            RightSidePinsStackPanel.Margin = new FMargin(0, 0, 0, 0);
            RightSidePinsStackPanel.Parent = PinsPanel;
            RightSidePinsStackPanel.Orientation = EOrientation.Vertical;
            RightSidePinsStackPanel.HorizontalAlignment = EHorizontalAlignment.Right;
            PinsPanel.AddElement(EDockPosition.Right, RightSidePinsStackPanel);

            ElementContainer.Parent = this;
        }

        private Dictionary<ILayoutable, SizeF> ChildrenMeasuringSize = new Dictionary<ILayoutable, SizeF>();
        public override SizeF Measuring(SizeF availableSize)
        {
            var childrenDesiredSize = ElementContainer.Measuring(MinSize);
            var size = new SizeF();
            size.Width = Size.Width > childrenDesiredSize.Width ? Size.Width : childrenDesiredSize.Width;
            size.Height = childrenDesiredSize.Height;
            return new SizeF(size.Width + Margin.Left + Margin.Right, size.Height + Margin.Top + Margin.Bottom);
        }

        public override SizeF Arranging(Rect finalRect)
        {
            //Location = finalRect.Location; Location have already set
            Size = new SizeF(finalRect.Width, finalRect.Height);
            ElementContainer.Arranging(new Rect(Vector2.Zero, finalRect.Size));
            return finalRect.Size;
        }

        public List<IGraphElement> EnumerateChild<T>() where T : class
        {
            List<IGraphElement> list = new List<IGraphElement>();
            list.Add(NameTextBlock);
            foreach (var element in LeftSidePinsStackPanel.Children)
            {
                if (element is T)
                {
                    list.Add(element);
                }
            }
            foreach (var element in RightSidePinsStackPanel.Children)
            {
                if (element is T)
                {
                    list.Add(element);
                }
            }
            return list;
        }

        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            LeftSidePinsStackPanel.Clear();
            {
                foreach (var posePin in BlendTreeNodeClassDescription.PoseInPins)
                {
                    var posePinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_PosePin>(posePin.GetType());
                    if (posePinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(posePinAttribute.ClassType, posePin, context.GraphElementStyleManager.GetOrAdd(posePin.Id)) as TtGraphElement_PosePin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(posePin.Id, instance);
                    }
                }
            }
            RightSidePinsStackPanel.Clear();
            {
                foreach (var posePin in BlendTreeNodeClassDescription.PoseOutPins)
                {
                    var posePinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_PosePin>(posePin.GetType());
                    if (posePinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(posePinAttribute.ClassType, posePin, context.GraphElementStyleManager.GetOrAdd(posePin.Id)) as TtGraphElement_PosePin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        RightSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(posePin.Id, instance);
                    }
                }
            }
        }
    }

    public class TtGraphElementRender_BlendTreeNode : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var blendTreeNodeElement = renderableElement as TtGraphElement_BlendTreeNode;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(blendTreeNodeElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(blendTreeNodeElement.AbsLocation + new Vector2(blendTreeNodeElement.Size.Width, blendTreeNodeElement.Size.Height));
            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersAll;
            var borderOffset = new Vector2(blendTreeNodeElement.BorderThickness / 2, blendTreeNodeElement.BorderThickness / 2);
            cmdlist.AddRect(nodeStart - borderOffset, nodeEnd + borderOffset, ImGuiAPI.ColorConvertFloat4ToU32(blendTreeNodeElement.BorderColor), blendTreeNodeElement.Rounding, roundCornerFlags, blendTreeNodeElement.BorderThickness);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(blendTreeNodeElement.BackgroundColor), blendTreeNodeElement.Rounding, roundCornerFlags);
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(blendTreeNodeElement.ElementContainer);
            elementContainerRender.Draw(blendTreeNodeElement.ElementContainer, ref context);
        }
    }
}

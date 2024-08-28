using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using System.Drawing;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtGraphElementRender_BlendTree_PoseOutput))]
    public class TtGraphElement_BlendTree_PoseOutput : TtDescriptionGraphElement, IEnumChild
    {
        public TtGraphElement_Icon PoseOutputIcon = new();
        public TtBlendTreeNodeClassDescription BlendTreeNodeClassDescription { get => Description as TtBlendTreeNodeClassDescription; }
        public float Rounding { get; set; } = 5;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(0.5f, 188f / 255, 212f / 255, 240f / 255);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.9f, 0.9f, 0.9f);
        public float BorderThickness { get; set; } = 2;
        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel BlendTreeNodeDescStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_DockPanel PinsPanel = new();
        public TtGraphElement_DockPanel LeftSidePinsDockPanel = new TtGraphElement_DockPanel();
        public TtGraphElement_StackPanel RightSidePinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_BlendTree_PoseOutput(IDescription description, IGraphElementStyle style) : base(description, style)
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

            LeftSidePinsDockPanel.Margin = new FMargin(0, 0, 0, 0);
            LeftSidePinsDockPanel.Parent = PinsPanel;
            LeftSidePinsDockPanel.HorizontalAlignment = EHorizontalAlignment.Center;
            LeftSidePinsDockPanel.VerticalAlignment = EVerticalAlignment.Center;
            PinsPanel.AddElement(EDockPosition.Left, LeftSidePinsDockPanel);

            RightSidePinsStackPanel.Margin = new FMargin(0, 0, 0, 0);
            RightSidePinsStackPanel.Parent = PinsPanel;
            RightSidePinsStackPanel.Orientation = EOrientation.Vertical;
            RightSidePinsStackPanel.HorizontalAlignment = EHorizontalAlignment.Right;
            PinsPanel.AddElement(EDockPosition.Right, RightSidePinsStackPanel);

            PoseOutputIcon.IconName = RName.GetRName(UNodeGraphStyles.DefaultStyles.AnimationFinalPosePin, RName.ERNameType.Engine);
            PoseOutputIcon.Size = new SizeF(64, 64);
            PoseOutputIcon.Rounding = 0;
            RightSidePinsStackPanel.AddElement(PoseOutputIcon);

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
            foreach (var element in LeftSidePinsDockPanel.Children)
            {
                if (element.Value is T)
                {
                    list.Add(element.Value);
                }
            }
            return list;
        }
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            LeftSidePinsDockPanel.Clear();
            {
                foreach (var posePin in BlendTreeNodeClassDescription.PoseInPins)
                {
                    var posePinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_PosePin>(posePin.GetType());
                    if (posePinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(posePinAttribute.ClassType, posePin, context.GraphElementStyleManager.GetOrAdd(posePin.Id)) as TtGraphElement_PosePin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsDockPanel.AddElement(EDockPosition.Left,instance);
                        context.DescriptionsElement.Add(posePin.Id, instance);
                    }
                }
            }
        }
    }

    public class TtGraphElementRender_BlendTree_PoseOutput : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var blendTreeNodeElement = renderableElement as TtGraphElement_BlendTree_PoseOutput;
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

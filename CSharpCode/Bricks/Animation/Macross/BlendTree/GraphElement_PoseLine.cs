using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtGraphElementRender_PoseLine))]
    public class TtGraphElement_PoseLine : TtDescriptionGraphElement
    {
        public TtPoseLineDescription PoseLineDescription { get => Description as TtPoseLineDescription; }
        public IGraphElement From { get; set; } = null;
        public IGraphElement To { get; set; } = null;

        public TtGraphElement_PoseLine(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }

        public override SizeF Measuring(SizeF availableSize)
        {
            return SizeF.Empty;
        }

        public override SizeF Arranging(Rect finalRect)
        {
            return SizeF.Empty;
        }
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {

            base.ConstructElements(ref context);
        }
        public override void AfterConstructElements(ref FGraphElementRenderingContext context)
        {
            From = context.DescriptionsElement[PoseLineDescription.FromId];
            To = context.DescriptionsElement[PoseLineDescription.ToId];
            base.AfterConstructElements(ref context);
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PreviewPoseLine))]
    public class TtGraphElement_PreviewPoseLine : IGraphElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get; set; }
        public SizeF Size { get; set; }
        public IGraphElement Parent { get; set; }
        public IGraphElementStyle Style { get; set; }

        public TtPosePinDescription StartPin { get; set; } = null;

        #region Selectable
        public bool CanDrag()
        {
            return false;
        }

        public void OnDragging(Vector2 delta)
        {

        }

        public bool HitCheck(Vector2 pos)
        {
            return false;
        }

        public void OnSelected(ref FGraphElementRenderingContext context)
        {
        }

        public void OnUnSelected()
        {
        }

        public void OnMouseOver(ref FGraphElementRenderingContext context)
        {
        }

        public void OnMouseLeave(ref FGraphElementRenderingContext context)
        {
        }

        public void OnMouseLeftButtonDown(ref FGraphElementRenderingContext context)
        {
        }

        public void OnMouseLeftButtonUp(ref FGraphElementRenderingContext context)
        {
        }

        public void OnMouseRightButtonDown(ref FGraphElementRenderingContext context)
        {
        }

        public void OnMouseRightButtonUp(ref FGraphElementRenderingContext context)
        {
        }
        #endregion

    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PosePin))]
    public class TtGraphElement_PosePin : TtDescriptionGraphElement
    {
        public TtPosePinDescription PosePinDescription { get => Description as TtPosePinDescription; }
        public override FMargin Margin { get; set; } = new FMargin(2, 2, 2, 2);
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel ElementContainer = new();
        public TtGraphElement_Icon Icon = new();
        public string DisconnectedPinIconName { get; set; } = UNodeGraphStyles.DefaultStyles.PinDisconnectedVarImg;
        public string ConnectedPinIconName { get; set; } = UNodeGraphStyles.DefaultStyles.PinConnectedVarImg;
        public SizeF IconSize { get; set; } = new SizeF(18, 15);
        public Color4f BackgroundColor
        {
            get => Style.BackgroundColor;
            set => Style.BackgroundColor = value;
        }
        public float Rounding { get; set; } = 5;
        public TtGraphElement_PosePin(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            bool isIconAtLeft = false;
            if (PosePinDescription is TtPoseInPinDescription)
            {
                isIconAtLeft = true;
            }
            if (PosePinDescription is TtPoseOutPinDescription)
            {
                isIconAtLeft = false;
            }
            ElementContainer.Parent = this;
            ElementContainer.Orientation = EOrientation.Horizontal;
            NameTextBlock.Content = PosePinDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.BackgroundColor = BackgroundColor;
            NameTextBlock.Rounding = 0;
            ElementContainer.AddElement(NameTextBlock);

            Icon.BackgroundColor = BackgroundColor;
            Icon.Rounding = 0;
            if (isIconAtLeft)
            {
                ElementContainer.InsertElement(0, Icon);
            }
            else
            {
                ElementContainer.AddElement(Icon);
            }
        }
        public override SizeF Measuring(SizeF availableSize)
        {
            var desiredSize = ElementContainer.Measuring(MinSize);
            return new SizeF(desiredSize.Width + Margin.Left + Margin.Right, desiredSize.Height + Margin.Top + Margin.Bottom);
        }
        public override SizeF Arranging(Rect finalRect)
        {
            Size = new SizeF(finalRect.Width, finalRect.Height);
            Location = finalRect.Location + new Vector2(Margin.Left, Margin.Top);
            ElementContainer.Arranging(new Rect(Vector2.Zero, finalRect.Size));
            return finalRect.Size;
        }

        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            ElementContainer.Clear();
            bool isIconAtLeft = false;
            if (PosePinDescription is TtPoseInPinDescription)
            {
                isIconAtLeft = true;
            }
            if (PosePinDescription is TtPoseOutPinDescription)
            {
                isIconAtLeft = false;
            }
            NameTextBlock.Content = PosePinDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.BackgroundColor = BackgroundColor;
            NameTextBlock.Rounding = 0;
            ElementContainer.AddElement(NameTextBlock);

            var styles = UNodeGraphStyles.DefaultStyles;
            Icon.IconName = RName.GetRName(DisconnectedPinIconName, RName.ERNameType.Engine);
            Icon.Size = IconSize;
            Icon.BackgroundColor = BackgroundColor;
            Icon.Rounding = 0;
            if (isIconAtLeft)
            {
                ElementContainer.InsertElement(0, Icon);
            }
            else
            {
                ElementContainer.AddElement(Icon);
            }
        }
        public override void AfterConstructElements(ref FGraphElementRenderingContext context)
        {
            var styles = UNodeGraphStyles.DefaultStyles;
            var IconName = RName.GetRName(DisconnectedPinIconName, RName.ERNameType.Engine);
            foreach (var element in context.DescriptionsElement)
            {
                if (element.Value is TtGraphElement_PoseLine line)
                {
                    if (line.From == this || line.To == this)
                    {
                        IconName = RName.GetRName(ConnectedPinIconName, RName.ERNameType.Engine);
                        break;
                    }
                }
            }
            Icon.IconName = IconName;
            base.AfterConstructElements(ref context);
        }
        public override void OnMouseOver(ref FGraphElementRenderingContext context)
        {
            BackgroundColor = new Color4f(0.5, 1, 1, 1);
        }
        public override void OnMouseLeave(ref FGraphElementRenderingContext context)
        {
            BackgroundColor = new Color4f(0, 0, 0, 0);
        }
        public override void OnMouseLeftButtonDown(ref FGraphElementRenderingContext context)
        {
            var methodGraph = context.DesignedGraph as TtGraph_BlendTree;
            methodGraph.PreviewPoseLine = new TtGraphElement_PreviewPoseLine();
            methodGraph.PreviewPoseLine.AbsLocation = Icon.AbsCenter;
            methodGraph.PreviewPoseLine.StartPin = PosePinDescription;
        }

        public override void OnMouseLeftButtonUp(ref FGraphElementRenderingContext context)
        {
            var blendTreeGraph = context.DesignedGraph as TtGraph_BlendTree;
            if (blendTreeGraph.PreviewPoseLine != null)
            {
                var startPin = blendTreeGraph.PreviewPoseLine.StartPin;
                if (startPin != PosePinDescription && startPin.Parent != PosePinDescription.Parent)
                {
                    var fromId = Guid.Empty;
                    var fromDescName = "";
                    var toId = Guid.Empty;
                    var toDescName = "";
                    bool validLine = false;
                    if (blendTreeGraph.PreviewPoseLine.StartPin is TtPoseInPinDescription)
                    {
                        if (PosePinDescription is TtPoseOutPinDescription)
                        {
                            validLine = true;
                            fromId = PosePinDescription.Id;
                            fromDescName = PosePinDescription.Parent.Name;
                            toId = blendTreeGraph.PreviewPoseLine.StartPin.Id;
                            toDescName = blendTreeGraph.PreviewPoseLine.StartPin.Parent.Name;
                        }
                    }
                    else
                    {
                        if (PosePinDescription is TtPoseInPinDescription)
                        {
                            validLine = true;
                            System.Diagnostics.Debug.Assert(blendTreeGraph.PreviewPoseLine.StartPin is TtPoseOutPinDescription);
                            fromId = blendTreeGraph.PreviewPoseLine.StartPin.Id;
                            fromDescName = blendTreeGraph.PreviewPoseLine.StartPin.Parent.Name;
                            toId = PosePinDescription.Id;
                            toDescName = PosePinDescription.Parent.Name;
                        }
                    }
                    if (validLine)
                    {
                        var line = new TtPoseLineDescription() { Name = "Data_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                        context.CommandHistory.CreateAndExtuteCommand("AddPoseLine",
                            (data) => { blendTreeGraph.BlendTreeClassDescription.AddPoseLine(line); },
                            (data) => { blendTreeGraph.BlendTreeClassDescription.RemovePoseLine(line); });
                    }
                }
            }
            blendTreeGraph.PreviewPoseLine = null;
        }
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");

            if (context.DesignedGraph is TtGraph_BlendTree blendTreeGraph)
            {
                foreach (var line in blendTreeGraph.BlendTreeClassDescription.PoseLines)
                {
                    if (line.FromId == Id || line.ToId == Id)
                    {
                        parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                        {
                            cmdHistory.CreateAndExtuteCommand("Break Link",
                                (data) => { blendTreeGraph.BlendTreeClassDescription.RemovePoseLine(line); },
                                (data) => { blendTreeGraph.BlendTreeClassDescription.AddPoseLine(line); }
                                );
                        });
                    }
                }

            }

        }
    }

    public class TtGraphElementRender_PoseLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_PoseLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var fromPin = line.From as TtGraphElement_PosePin;
            var toPin = line.To as TtGraphElement_PosePin;
            var nodeStart = context.ViewPortTransform(fromPin.Icon.AbsCenter);
            var nodeEnd = context.ViewPortTransform(toPin.Icon.AbsCenter);
            cmdlist.AddLine(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(1, 1, 1, 1)), 5);
        }
    }
    public class TtGraphElementRender_PreviewPoseLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_PreviewPoseLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(line.AbsLocation);
            var mousePosInViewPort = context.ViewPort.ViewportInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos());
            var nodeEnd = context.ViewPortTransform(mousePosInViewPort);
            cmdlist.AddLine(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(1, 1, 1, 1)), 5);
        }
    }
    public class TtGraphElementRender_PosePin : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pin = renderableElement as TtGraphElement_PosePin;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(pin.AbsLocation);
            var nodeEnd = context.ViewPortTransform(pin.AbsLocation + new Vector2(pin.Size.Width, pin.Size.Height));
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(pin.ElementContainer);
            elementContainerRender.Draw(pin.ElementContainer, ref context);
        }
    }
}

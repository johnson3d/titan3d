using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using SixLabors.Fonts;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtGraphElementRender_PoseLine))]
    public class TtGraphElement_PoseLine : TtGraphElement_Line
    {
        public TtPoseLineDescription PoseLineDescription { get => Description as TtPoseLineDescription; }
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
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");

            if (context.DesignedGraph is TtGraph graph)
            {
                var graphDesc = graph.Description as IPoseLineOperator;
                if (graphDesc != null)
                {
                    parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                    {
                        cmdHistory.CreateAndExtuteCommand("Break Link",
                            (data) => { graphDesc.RemovePoseLine(PoseLineDescription); },
                            (data) => { graphDesc.AddPoseLine(PoseLineDescription); }
                            );
                    });
                }
            }
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PreviewPoseLine))]
    public class TtGraphElement_PreviewPoseLine : TtGraphElement_PreviewLine
    {

    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PosePin))]
    public class TtGraphElement_PosePin : TtGraphElement_Pin
    {
        public TtPosePinDescription PosePinDescription { get => Description as TtPosePinDescription; }
        public string DisconnectedPinIconName { get; set; } = UNodeGraphStyles.DefaultStyles.AnimationPosePinDisConnected;
        public string ConnectedPinIconName { get; set; } = UNodeGraphStyles.DefaultStyles.AnimationPosePinConnected;
        public SizeF IconSize { get; set; } = new SizeF(15, 28);
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
            ElementContainer.VerticalAlignment = EVerticalAlignment.Center;
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
        public override void OnMouseOver(ref FMouseEventContext context)
        {
            BackgroundColor = new Color4f(0.5, 1, 1, 1);
        }
        public override void OnMouseLeave(ref FMouseEventContext context)
        {
            BackgroundColor = new Color4f(0, 0, 0, 0);
        }
        public override void OnMouseLeftButtonDown(ref FMouseEventContext context)
        {
            var renderContext = context.GraphElementRenderingContext;
            var methodGraph = renderContext.DesignedGraph as TtGraph_BlendTree;
            methodGraph.PreviewLine = new TtGraphElement_PreviewPoseLine
            {
                AbsLocation = Icon.AbsCenter,
                StartPin = PosePinDescription
            };
        }

        public override void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
            var renderContext = context.GraphElementRenderingContext;
            var graph = renderContext.DesignedGraph as TtGraph;
            if (graph.PreviewLine != null && graph.PreviewLine.StartPin is TtPosePinDescription startPin)
            {
                if (startPin != PosePinDescription && startPin.Parent != PosePinDescription.Parent)
                {
                    var fromId = Guid.Empty;
                    var fromDescName = "";
                    var toId = Guid.Empty;
                    var toDescName = "";
                    TtPoseOutPinDescription fromPin = null;
                    TtPoseInPinDescription toPin = null;
                    bool validLine = false;
                    if (startPin is TtPoseInPinDescription)
                    {
                        if (PosePinDescription is TtPoseOutPinDescription)
                        {
                            validLine = true;
                            fromId = PosePinDescription.Id;
                            fromPin = PosePinDescription as TtPoseOutPinDescription;
                            fromDescName = PosePinDescription.Parent.Name;
                            toId = startPin.Id;
                            toPin = startPin as TtPoseInPinDescription;
                            toDescName = startPin.Parent.Name;
                        }
                    }
                    else
                    {
                        if (PosePinDescription is TtPoseInPinDescription)
                        {
                            validLine = true;
                            System.Diagnostics.Debug.Assert(startPin is TtPoseOutPinDescription);
                            fromId = startPin.Id;
                            fromPin = startPin as TtPoseOutPinDescription;
                            fromDescName = startPin.Parent.Name;
                            toId = PosePinDescription.Id;
                            toPin = PosePinDescription as TtPoseInPinDescription;
                            toDescName = PosePinDescription.Parent.Name;
                        }
                    }

                    var graphDesc = graph.Description as IPoseLineOperator;
                    if (validLine && !graphDesc.ContainsPoseLineBetweenPins(fromId, toId))
                    { 
                        var linkedLineFromPin = graphDesc.GetPoseLineWithPin(fromPin);
                        var linkedLineToPin = graphDesc.GetPoseLineWithPin(toPin);
                        var line = new TtPoseLineDescription() { Name = "Data_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                        renderContext.CommandHistory.CreateAndExtuteCommand("AddPoseLine",
                            (data) =>
                            {
                                if (linkedLineFromPin != null)
                                {
                                    graphDesc.RemovePoseLine(linkedLineFromPin);
                                }
                                if (linkedLineToPin != null)
                                {
                                    graphDesc.RemovePoseLine(linkedLineToPin);
                                }
                                graphDesc.AddPoseLine(line); 
                            },
                            (data) => 
                            {
                                if (linkedLineFromPin != null)
                                {
                                    graphDesc.AddPoseLine(linkedLineFromPin);
                                }
                                if (linkedLineToPin != null)
                                {
                                    graphDesc.AddPoseLine(linkedLineToPin);
                                }
                                graphDesc.RemovePoseLine(line); 
                            });
                    }
                }
            }
            graph.PreviewLine = null;
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
            if (fromPin == null || toPin == null)
                return;

            var nodeStart = context.ViewportTransform(fromPin.Icon.AbsCenter);
            var nodeEnd = context.ViewportTransform(toPin.Icon.AbsCenter);
            var p1 = nodeStart;
            var p4 = nodeEnd;
            var delta = p4 - p1;
            var ctDelta = Math.Min(TtDesignMacrossGraphStyles.LineBezierMaxDelta, Math.Max(TtDesignMacrossGraphStyles.LineBezierMinDelta, Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) * 0.5f));

            var p2 = new Vector2(p1.X + ctDelta, p1.Y);
            var p3 = new Vector2(p4.X - ctDelta, p4.Y);

            var lineColor = new Color4f(1, 1, 1, 1);
            var thickness = TtDesignMacrossGraphStyles.LineNormalThickness;
            if (line.HighLightState == EHighLigthState.HighLigth)
            {
                lineColor.Alpha = 1.0f;
                thickness = TtDesignMacrossGraphStyles.LineHighLightThickness;
            }
            else if (line.HighLightState == EHighLigthState.LowLight)
            {
                lineColor.Alpha = 0.6f;
                thickness = TtDesignMacrossGraphStyles.LineLowLightThickness;
            }
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, ImGuiAPI.ColorConvertFloat4ToU32(lineColor), thickness * context.Camera.Scale, 100);

        }
    }
    public class TtGraphElementRender_PreviewPoseLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_PreviewPoseLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = Vector2.Zero;
            var nodeEnd = Vector2.Zero;
            if (line.StartPin is TtPoseOutPinDescription)
            {
                nodeStart = context.ViewportTransform(line.AbsLocation);
                //var mousePosInViewPort = context.ViewPort.ViewportInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos());
                //nodeEnd = context.ViewportTransform(mousePosInViewPort);
                nodeEnd = ImGuiAPI.GetMousePos();
            }
            else
            {
                nodeEnd = context.ViewportTransform(line.AbsLocation);
                //var mousePosInViewPort = context.ViewPort.ViewportInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos());
                //nodeStart = context.ViewportTransform(mousePosInViewPort);
                nodeStart = ImGuiAPI.GetMousePos();
            }

            var p1 = nodeStart;
            var p4 = nodeEnd;
            var delta = p4 - p1;
            var ctDelta = Math.Min(TtDesignMacrossGraphStyles.LineBezierMaxDelta, Math.Max(TtDesignMacrossGraphStyles.LineBezierMinDelta, Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) * 0.5f));

            var p2 = new Vector2(p1.X + ctDelta, p1.Y);
            var p3 = new Vector2(p4.X - ctDelta, p4.Y);
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(1, 1, 1, 1)), TtDesignMacrossGraphStyles.LineNormalThickness * context.Camera.Scale, 100);

        }
    }
    public class TtGraphElementRender_PosePin : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pin = renderableElement as TtGraphElement_PosePin;
            if (pin.HighLightState == EHighLigthState.HighLigth || pin.HighLightState == EHighLigthState.Normal)
            {
                pin.Icon.TintColor = new Color4f(pin.Icon.TintColor, 1.0f);
            }
            else if (pin.HighLightState == EHighLigthState.LowLight)
            {
                pin.Icon.TintColor = new Color4f(pin.Icon.TintColor, 0.6f);
            }
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewportTransform(pin.AbsLocation);
            var nodeEnd = context.ViewportTransform(pin.AbsLocation + new Vector2(pin.Size.Width, pin.Size.Height));
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(pin.ElementContainer);
            elementContainerRender.Draw(pin.ElementContainer, ref context);
        }
    }
}

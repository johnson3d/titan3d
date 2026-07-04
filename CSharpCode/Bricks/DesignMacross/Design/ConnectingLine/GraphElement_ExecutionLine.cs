using EngineNS.Animation.Macross.BlendTree;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using NPOI.SS.UserModel;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExecutionLine))]
    public class TtGraphElement_ExecutionLine : TtGraphElement_Line
    {
        public TtExecutionLineDescription ExecutionLineDescription { get => Description as TtExecutionLineDescription; }

        public TtGraphElement_ExecutionLine(IDescription description, IGraphElementStyle style) : base(description, style)
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
            if (context.DescriptionsElement.ContainsKey(ExecutionLineDescription.FromId))
            {
                From = context.DescriptionsElement[ExecutionLineDescription.FromId];
            }
            if (context.DescriptionsElement.ContainsKey(ExecutionLineDescription.ToId))
            {
                To = context.DescriptionsElement[ExecutionLineDescription.ToId];
            }
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
                var graphDesc = graph.Description as IExecutionLineOperator;
                if (graphDesc != null)
                {
                    parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                    {
                        cmdHistory.CreateAndExtuteCommand("Break Link",
                            (data) => { graphDesc.RemoveExecutionLine(ExecutionLineDescription); },
                            (data) => { graphDesc.AddExecutionLine(ExecutionLineDescription); }
                            );
                    });
                }
            }
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PreviewExecutionLine))]
    public class TtGraphElement_PreviewExecutionLine : TtGraphElement_PreviewLine
    {


    }
    [ImGuiElementRender(typeof(TtGraphElementRender_ExecutionPin))]
    public class TtGraphElement_ExecutionPin : TtGraphElement_Pin
    {
        public TtExecutionPinDescription ExecutionPinDescription { get => Description as TtExecutionPinDescription; }
        public TtGraphElement_ExecutionPin(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            bool isIconAtLeft = false;
            if (ExecutionPinDescription is TtExecutionInPinDescription)
            {
                isIconAtLeft = true;
            }
            if (ExecutionPinDescription is TtExecutionOutPinDescription)
            {
                isIconAtLeft = false;
            }
            ElementContainer.Parent = this;
            ElementContainer.Orientation = EOrientation.Horizontal;
            ElementContainer.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.Content = ExecutionPinDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.BackgroundColor = BackgroundColor;
            NameTextBlock.Rounding = 0;
            ElementContainer.AddElement(NameTextBlock);
            var styles = UNodeGraphStyles.DefaultStyles;
            Icon.IconName = RName.GetRName(styles.PinDisconnectedExecImg, RName.ERNameType.Engine);
            Icon.Size = new SizeF(12, 15);
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
            if (ExecutionPinDescription is TtExecutionInPinDescription)
            {
                isIconAtLeft = true;
            }
            if (ExecutionPinDescription is TtExecutionOutPinDescription)
            {
                isIconAtLeft = false;
            }
            NameTextBlock.Content = ExecutionPinDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.BackgroundColor = BackgroundColor;
            ElementContainer.AddElement(NameTextBlock);

            Icon.Size = new SizeF(12, 15);
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
            var IconName = RName.GetRName(styles.PinDisconnectedExecImg, RName.ERNameType.Engine);
            foreach (var element in context.DescriptionsElement)
            {
                if (element.Value is TtGraphElement_ExecutionLine line)
                {
                    if (line.From == this || line.To == this)
                    {
                        IconName = RName.GetRName(styles.PinConnectedExecImg, RName.ERNameType.Engine);
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
            var start = context.GraphElementRenderingContext.ViewportTransform(Icon.AbsLocation);
            var end = context.GraphElementRenderingContext.ViewportTransform(Icon.AbsLocation + new Vector2(Icon.Size.Width, Icon.Size.Height));
            Rect iconRect = new Rect(start.X, start.Y, end.X - start.X, end.Y - start.Y);
            if (iconRect.Contains(context.MouseAbsPos))
            {
                var renderContext = context.GraphElementRenderingContext;
                var methodGraph = renderContext.DesignedGraph as TtGraph_Method;
                methodGraph.PreviewLine = new TtGraphElement_PreviewExecutionLine
                {
                    AbsLocation = Icon.AbsCenter,
                    StartPin = ExecutionPinDescription
                };
            }
        }

        public override void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
            var renderContext = context.GraphElementRenderingContext;
            var graph = renderContext.DesignedGraph as TtGraph;
            if (graph.PreviewLine != null && graph.PreviewLine.StartPin is TtExecutionPinDescription startPin)
            {
                if (startPin != ExecutionPinDescription && startPin.Parent != ExecutionPinDescription.Parent)
                {
                    var fromId = Guid.Empty;
                    var fromDescName = "";
                    var toId = Guid.Empty;
                    var toDescName = "";
                    TtExecutionOutPinDescription fromPin = null;
                    TtExecutionInPinDescription toPin = null;
                    bool validLine = false;
                    if (startPin is TtExecutionInPinDescription)
                    {
                        if (ExecutionPinDescription is TtExecutionOutPinDescription)
                        {
                            validLine = true;
                            fromId = ExecutionPinDescription.Id;
                            fromPin = ExecutionPinDescription as TtExecutionOutPinDescription;
                            fromDescName = ExecutionPinDescription.Parent.Name;
                            toId = startPin.Id;
                            toPin = startPin as TtExecutionInPinDescription;
                            toDescName = startPin.Parent.Name;
                        }
                    }
                    else
                    {
                        if (ExecutionPinDescription is TtExecutionInPinDescription)
                        {
                            validLine = true;
                            System.Diagnostics.Debug.Assert(startPin is TtExecutionOutPinDescription);
                            fromId = startPin.Id;
                            fromPin = startPin as TtExecutionOutPinDescription;
                            fromDescName = startPin.Parent.Name;
                            toId = ExecutionPinDescription.Id;
                            toPin = ExecutionPinDescription as TtExecutionInPinDescription;
                            toDescName = ExecutionPinDescription.Parent.Name;
                        }
                    }
                    var graphDesc = graph.Description as IExecutionLineOperator;
                    if (validLine && !graphDesc.ContainsExecutionLineBetweenPins(fromId, toId))
                    {
                        var linkedLineFromPin = graphDesc.GetExecutionLineWithPin(fromPin);
                        var linkedLineToPin = graphDesc.GetExecutionLineWithPin(toPin);
                        var line = new TtExecutionLineDescription() { Name = "Exec_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                        renderContext.CommandHistory.CreateAndExtuteCommand("AddExecutionLine",
                                (data) =>
                                {
                                    if (linkedLineFromPin != null && !fromPin.CanMutilLink)
                                    {
                                        graphDesc.RemoveExecutionLine(linkedLineFromPin);
                                    }
                                    if (linkedLineToPin != null && !toPin.CanMutilLink)
                                    {
                                        graphDesc.RemoveExecutionLine(linkedLineToPin);
                                    }
                                    graphDesc.AddExecutionLine(line); 
                                },
                                (data) => 
                                {
                                    if (linkedLineFromPin != null)
                                    {
                                        graphDesc.AddExecutionLine(linkedLineFromPin);
                                    }
                                    if (linkedLineToPin != null)
                                    {
                                        graphDesc.AddExecutionLine(linkedLineToPin);
                                    }
                                    graphDesc.RemoveExecutionLine(line); 
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

            if (context.DesignedGraph is TtGraph graph)
            {
                var graphDesc = graph.Description as IExecutionLineOperator;
                foreach (var line in graphDesc.ExecutionLines)
                {
                    if (line.FromId == Id || line.ToId == Id)
                    {
                        parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                        {
                            cmdHistory.CreateAndExtuteCommand("Break Link",
                                (data) => { graphDesc.RemoveExecutionLine(line); },
                                (data) => { graphDesc.AddExecutionLine(line); }
                                );
                        });
                    }
                }

            }

        }
    }
    public class TtGraphElementRender_ExecutionLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_ExecutionLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var fromPin = line.From as TtGraphElement_ExecutionPin;
            var toPin = line.To as TtGraphElement_ExecutionPin;
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
    public class TtGraphElementRender_PreviewExecutionLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_PreviewExecutionLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = Vector2.Zero;
            var nodeEnd = Vector2.Zero;
            if (line.StartPin is TtExecutionOutPinDescription)
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
    public class TtGraphElementRender_ExecutionPin : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pin = renderableElement as TtGraphElement_ExecutionPin;
            if (pin.HighLightState == EHighLigthState.HighLigth || pin.HighLightState == EHighLigthState.Normal)
            {
                pin.Icon.TintColor = new Color4f(pin.Icon.TintColor, 1.0f);
            }
            else if (pin.HighLightState == EHighLigthState.LowLight)
            {
                pin.Icon.TintColor = new Color4f(pin.Icon.TintColor, TtDesignMacrossGraphStyles.LowLigthAlpha);
            }
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewportTransform(pin.AbsLocation);
            var nodeEnd = context.ViewportTransform(pin.AbsLocation + new Vector2(pin.Size.Width, pin.Size.Height));
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(pin.ElementContainer);
            elementContainerRender.Draw(pin.ElementContainer, ref context);
        }
    }
}

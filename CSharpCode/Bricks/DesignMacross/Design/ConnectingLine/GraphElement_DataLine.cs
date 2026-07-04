using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    [ImGuiElementRender(typeof(TtGraphElementRender_DataLine))]
    public class TtGraphElement_DataLine: TtGraphElement_Line
    {
        public TtDataLineDescription DataLineDescription { get => Description as TtDataLineDescription; }

        public TtGraphElement_DataLine(IDescription description, IGraphElementStyle style) : base(description, style)
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
            if(context.DescriptionsElement.ContainsKey(DataLineDescription.FromId))
            {
                From = context.DescriptionsElement[DataLineDescription.FromId];
            }
            if(context.DescriptionsElement.ContainsKey(DataLineDescription.ToId))
            {
                To = context.DescriptionsElement[DataLineDescription.ToId];
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
                var graphDesc = graph.Description as IDataLineOperator;
                if (graphDesc != null)
                {
                    parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                    {
                        cmdHistory.CreateAndExtuteCommand("Break Link",
                            (data) => { graphDesc.RemoveDataLine(DataLineDescription); },
                            (data) => { graphDesc.AddDataLine(DataLineDescription); }
                            );
                    });
                }
            }
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PreviewDataLine))]
    public class TtGraphElement_PreviewDataLine: TtGraphElement_PreviewLine
    {
        
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_DataPin))]
    public class TtGraphElement_DataPin : TtGraphElement_Pin
    {
        public TtDataPinDescription DataPinDescription { get => Description as TtDataPinDescription; }

       
        public TtGraphElement_DataPin(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            bool isIconAtLeft = false;
            if (DataPinDescription is TtDataInPinDescription)
            {
                isIconAtLeft = true;
            }
            if (DataPinDescription is TtDataOutPinDescription)
            {
                isIconAtLeft = false;
            }
            ElementContainer.Parent = this;
            ElementContainer.Orientation = EOrientation.Horizontal;
            ElementContainer.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.Content = DataPinDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 0.9f;
            NameTextBlock.BackgroundColor = BackgroundColor;
            NameTextBlock.Rounding = 0;
            ElementContainer.AddElement(NameTextBlock);
            var styles = UNodeGraphStyles.DefaultStyles;
            Icon.IconName = RName.GetRName(styles.PinDisconnectedVarImg, RName.ERNameType.Engine);
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
            if (DataPinDescription is TtDataInPinDescription)
            {
                isIconAtLeft = true;
            }
            if (DataPinDescription is TtDataOutPinDescription)
            {
                isIconAtLeft = false;
            }
            NameTextBlock.Content = DataPinDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 0.9f;
            NameTextBlock.BackgroundColor = BackgroundColor;
            NameTextBlock.Rounding = 0;
            ElementContainer.AddElement(NameTextBlock);

            Icon.Size = new SizeF(12, 10);
            Icon.BackgroundColor = BackgroundColor;
            var lineStyle = TtDesignMacrossGraphStyles.GetDataLineStyle(DataPinDescription.TypeDesc);
            Icon.TintColor = lineStyle.Normal;
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
            var IconName = RName.GetRName(styles.PinDisconnectedVarImg, RName.ERNameType.Engine);
            foreach (var element in context.DescriptionsElement)
            {
                if (element.Value is TtGraphElement_DataLine line)
                {
                    if (line.From == this || line.To == this)
                    {
                        IconName = RName.GetRName(styles.PinConnectedVarImg, RName.ERNameType.Engine);
                        break;
                    }
                }
            }
            Icon.IconName = IconName;
            base.AfterConstructElements(ref context);
        }
        public override void OnMouseOver(ref FMouseEventContext context)
        {
            var renderingContext = context.GraphElementRenderingContext;
            BackgroundColor = new Color4f(0.5, 1, 1, 1);
            var graph = renderingContext.DesignedGraph as TtGraph;
            if(graph.PreviewLine != null)
            {
                if (graph.PreviewLine.StartPin is not TtDataPinDescription)
                {
                    BackgroundColor = new Color4f(0.5, 1, 0, 0);
                    return;
                }
                var startPin = graph.PreviewLine.StartPin as TtDataPinDescription;
                if (startPin != DataPinDescription)
                {
                    bool isLinkable = true;
                    if (startPin.Parent == DataPinDescription.Parent)
                    {
                        isLinkable = false;
                    }
                    if (startPin is TtDataOutPinDescription && DataPinDescription is TtDataOutPinDescription)
                    {
                        isLinkable = false;
                    }
                    if (startPin is TtDataInPinDescription && DataPinDescription is TtDataInPinDescription)
                    {
                        isLinkable = false;
                    }
                    if (!CheckPinsLinkable(startPin, DataPinDescription))
                    {
                        isLinkable = false;
                    }

                    if (!isLinkable)
                    {
                        BackgroundColor = new Color4f(0.5, 1, 0, 0);
                    }
                    else
                    {
                        BackgroundColor = new Color4f(0.5, 0, 1, 0);
                    }
                }
            }

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
            if(iconRect.Contains(context.MouseAbsPos))
            {
                var renderContext = context.GraphElementRenderingContext;
                var graph = renderContext.DesignedGraph as TtGraph;
                graph.PreviewLine = new TtGraphElement_PreviewDataLine
                {
                    AbsLocation = Icon.AbsCenter,
                    StartPin = DataPinDescription
                };
            }
        }
        public bool CheckPinsLinkable(TtDataPinDescription startPin, TtDataPinDescription endPin)
        {
            if (endPin.Parent is IDataPinOperator pinOperator)
            {
                if (pinOperator.IsDataPinsLinkable(endPin, startPin))
                {
                    return true;
                }
            }
            return false;
        }
        public override void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
            var renderContext = context.GraphElementRenderingContext;
            var graph = renderContext.DesignedGraph as TtGraph;
            if (graph.PreviewLine != null) 
            {
                if (graph.PreviewLine.StartPin is TtDataPinDescription startPin)
                {
                    if (startPin != DataPinDescription && startPin.Parent != DataPinDescription.Parent)
                    {
                        TtDataOutPinDescription fromPin = null;
                        TtDataInPinDescription toPin = null;
                        if (startPin is TtDataOutPinDescription)
                        {
                            fromPin = (TtDataOutPinDescription)startPin;
                        }
                        if (DataPinDescription is TtDataOutPinDescription)
                        {
                            fromPin = (TtDataOutPinDescription)DataPinDescription;
                        }
                        if (startPin is TtDataInPinDescription)
                        {
                            toPin = (TtDataInPinDescription)startPin;
                        }
                        if (DataPinDescription is TtDataInPinDescription)
                        {
                            toPin = (TtDataInPinDescription)DataPinDescription;
                        }
                        if (fromPin == null || toPin == null)
                        {
                            return;
                        }
                        if (!CheckPinsLinkable(startPin, DataPinDescription))
                        {
                            return;
                        }

                        var graphDesc = graph.Description as IDataLineOperator;
                        if(!graphDesc.ContainsDataLineBetweenPins(fromPin, toPin))
                        {
                            var linkedLineFromPin = graphDesc.GetDataLineWithPin(fromPin);
                            var linkedLineToPin = graphDesc.GetDataLineWithPin(toPin);
                            var line = new TtDataLineDescription() { Name = "Data_" + fromPin.Parent.Name + "_To_" + toPin.Parent.Name, FromId = fromPin.Id, ToId = toPin.Id };
                            renderContext.CommandHistory.CreateAndExtuteCommand("AddDataLine",
                                (data) =>
                                {
                                    if (linkedLineFromPin != null)
                                    {
                                        graphDesc.RemoveDataLine(linkedLineFromPin);
                                    }
                                    if (linkedLineToPin != null)
                                    {
                                        graphDesc.RemoveDataLine(linkedLineToPin);
                                    }
                                    graphDesc.AddDataLine(line);
                                },
                                (data) =>
                                {
                                    if (linkedLineFromPin != null)
                                    {
                                        graphDesc.AddDataLine(linkedLineFromPin);
                                    }
                                    if (linkedLineToPin != null)
                                    {
                                        graphDesc.AddDataLine(linkedLineToPin);
                                    }
                                    graphDesc.RemoveDataLine(line);
                                });
                        }
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

            if(context.DesignedGraph is TtGraph graph)
            {
                var graphDesc = graph.Description as IDataLineOperator;
                foreach (var line in graphDesc.DataLines)
                {
                    if(line.FromId == Id || line.ToId == Id)
                    {
                        parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                        {
                            cmdHistory.CreateAndExtuteCommand("Break Link",
                                (data) => { graphDesc.RemoveDataLine(line); },
                                (data) => { graphDesc.AddDataLine(line); }
                                );
                        });
                    }
                }
                
            }

        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_DataPin))]
    public class TtGraphElement_DataInPin : TtGraphElement_DataPin
    {
        public TtDataInPinDescription DataInPinDescription { get => Description as TtDataInPinDescription; }

        public TtGraphElement_PinEditableBox PinEditableBox
        {
            get
            {
                var style = Style as TtDataInPinDescriptionElementStyle;
                return style.PinEditableBox;
            }
            set
            {
                var style = Style as TtDataInPinDescriptionElementStyle;
                style.PinEditableBox = value;
            }
        }
        public TtGraphElement_DataInPin(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }

        public void OnPinEditableBoxValueChange(string oldValue, string newValue)
        {
            DataInPinDescription.TypeVaule = newValue;
        }

        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            base.ConstructElements(ref context);
            var linkedPin = IDataLineOperator.GetLinkedDataPin(context.DesignedGraph.Description, DataInPinDescription);
            if (linkedPin == null && DataInPinDescription.TypeDesc != null)
            {
                PinEditableBox.HorizontalAlignment =  EHorizontalAlignment.Left;
                PinEditableBox.VerticalAlignment = EVerticalAlignment.Center;
                PinEditableBox.ValueType = DataInPinDescription.TypeDesc;
                PinEditableBox.Content = DataInPinDescription.TypeVaule?.ToString();
                PinEditableBox.FontScale = 0.5f;
                PinEditableBox.OnValueChange -= OnPinEditableBoxValueChange;
                PinEditableBox.OnValueChange += OnPinEditableBoxValueChange;
                
                ElementContainer.AddElement(PinEditableBox);

               
            }

        }
    }
    public class TtGraphElementRender_DataLine : IGraphElementRender
    {

        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_DataLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var fromPin = line.From as TtGraphElement_DataPin;
            var toPin = line.To as TtGraphElement_DataPin;
            if(fromPin == null || toPin == null)
            {
                return;
            }
            var nodeStart = context.ViewportTransform(fromPin.Icon.AbsCenter);
            var nodeEnd = context.ViewportTransform(toPin.Icon.AbsCenter);
            var p1 = nodeStart;
            var p4 = nodeEnd;
            var delta = p4 - p1;
            var ctDelta = Math.Min(TtDesignMacrossGraphStyles.LineBezierMaxDelta, Math.Max(TtDesignMacrossGraphStyles.LineBezierMinDelta, Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) * 0.5f));

            var p2 = new Vector2(p1.X + ctDelta, p1.Y);
            var p3 = new Vector2(p4.X - ctDelta, p4.Y);
            var lineStyle = TtDesignMacrossGraphStyles.GetDataLineStyle(fromPin.DataPinDescription.TypeDesc);
            var lineColor = lineStyle.Normal.ToColor4Float();
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
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, ImGuiAPI.ColorConvertFloat4ToU32(lineColor), thickness * context.Camera.Scale * 0.8f, 100);
        }
    }
    public class TtGraphElementRender_PreviewDataLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_PreviewDataLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = Vector2.Zero;
            var nodeEnd = Vector2.Zero;
            if (line.StartPin is TtDataOutPinDescription)
            {
                nodeStart = context.ViewportTransform(line.AbsLocation);
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
            var lineStyle = TtDesignMacrossGraphStyles.GetDataLineStyle(line.StartPin.TypeDesc);
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, ImGuiAPI.ColorConvertFloat4ToU32(lineStyle.Normal.ToColor4Float()), TtDesignMacrossGraphStyles.LineNormalThickness * context.Camera.Scale, 100);
        }
    }
    public class TtGraphElementRender_SelectingRect : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var rect = renderableElement as TtGraphElement_SelectingRect;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewportTransform(rect.AbsLocation);
            var nodeEnd = ImGuiAPI.GetMousePos();


            var color = TtDesignMacrossGraphStyles.SelectingRectColor;
            var thickness = TtDesignMacrossGraphStyles.SelectingRectThickness;
            cmdlist.AddRect(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(color), 0, ImDrawFlags_.ImDrawFlags_None, thickness * context.Camera.Scale);
        }
    }
    public class TtGraphElementRender_DataPin : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pin = renderableElement as TtGraphElement_DataPin;
            if(pin.HighLightState == EHighLigthState.HighLigth || pin.HighLightState == EHighLigthState.Normal)
            {
                pin.Icon.TintColor = new Color4f(pin.Icon.TintColor, 1.0f);
            }
            else if(pin.HighLightState == EHighLigthState.LowLight)
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

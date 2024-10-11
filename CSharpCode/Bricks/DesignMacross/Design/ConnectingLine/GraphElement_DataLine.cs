using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui;
using EngineNS.EGui.Controls;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    [ImGuiElementRender(typeof(TtGraphElementRender_DataLine))]
    public class TtGraphElement_DataLine: TtDescriptionGraphElement
    {
        public TtDataLineDescription DataLineDescription { get => Description as TtDataLineDescription; }
        public IGraphElement From { get; set; } = null;
        public IGraphElement To { get; set; } = null;

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
            From = context.DescriptionsElement[DataLineDescription.FromId];
            To = context.DescriptionsElement[DataLineDescription.ToId];
            base.AfterConstructElements(ref context);
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PreviewDataLine))]
    public class TtGraphElement_PreviewDataLine: IGraphElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get; set; }
        public SizeF Size { get; set; }
        public IGraphElement Parent { get; set; }
        public IGraphElementStyle Style { get; set; }

        public TtDataPinDescription StartPin { get; set; } = null;

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
    [ImGuiElementRender(typeof(TtGraphElementRender_DataPin))]
    public class TtGraphElement_DataPin : TtDescriptionGraphElement
    {
        public TtDataPinDescription DataPinDescription { get => Description as TtDataPinDescription; }
        public override FMargin Margin { get; set; } = new FMargin(2, 2, 2, 2);
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel ElementContainer = new();
        public TtGraphElement_Icon Icon= new();
        public Color4f BackgroundColor
        {
            get => Style.BackgroundColor;
            set => Style.BackgroundColor = value;
        }
        public float Rounding { get; set; } = 5;
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
            NameTextBlock.Content = DataPinDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
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
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.BackgroundColor = BackgroundColor;
            NameTextBlock.Rounding = 0;
            ElementContainer.AddElement(NameTextBlock);

            Icon.Size = new SizeF(18, 15);
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
        public override void OnMouseOver(ref FGraphElementRenderingContext context)
        {
            BackgroundColor = new Color4f(0.5, 1, 1, 1);
            var methodGraph = context.DesignedGraph as TtGraph_Method;
            if (methodGraph.PreviewExecutionLine != null)
            {
                BackgroundColor = new Color4f(0.5, 1, 0, 0);
            }
            if (methodGraph.PreviewDataLine != null)
            {
                var startPin = methodGraph.PreviewDataLine.StartPin;
                if (startPin != DataPinDescription)
                {
                    bool isLinkable = true;
                    if(startPin.Parent == DataPinDescription.Parent)
                    {
                        isLinkable = false;
                    }
                    if(startPin is TtDataOutPinDescription && DataPinDescription is TtDataOutPinDescription)
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
            else
            {
                if (DataPinDescription.TypeDesc != null)
                {
                    EGui.Controls.CtrlUtility.DrawHelper($"{DataPinDescription.TypeDesc.ToString()}");
                }
            }
        }
        public override void OnMouseLeave(ref FGraphElementRenderingContext context)
        {
            BackgroundColor = new Color4f(0, 0, 0, 0);
        }
        public override void OnMouseLeftButtonDown(ref FGraphElementRenderingContext context)
        {
            var methodGraph = context.DesignedGraph as TtGraph_Method;
            methodGraph.PreviewDataLine = new TtGraphElement_PreviewDataLine();
            methodGraph.PreviewDataLine.AbsLocation = Icon.AbsCenter;
            methodGraph.PreviewDataLine.StartPin = DataPinDescription;
        }
        public bool CheckPinsLinkable(TtDataPinDescription startPin, TtDataPinDescription endPin)
        {
            if (endPin.Parent is TtStatementDescription toStatementDesc)
            {
                if (toStatementDesc.IsPinsLinkable(endPin, startPin))
                {
                    return true;
                }
            }
            if (endPin.Parent is TtExpressionDescription toExpressionDesc)
            {
                if (toExpressionDesc.IsPinsLinkable(endPin, startPin))
                {
                    return true;
                }
            }
            return false;
        }
        public override void OnMouseLeftButtonUp(ref FGraphElementRenderingContext context)
        {
            var methodGraph = context.DesignedGraph as TtGraph_Method;
            if (methodGraph.PreviewExecutionLine != null)
            {
                methodGraph.PreviewExecutionLine = null;
            }
            if (methodGraph.PreviewDataLine != null)
            {
                var startPin = methodGraph.PreviewDataLine.StartPin;
                if (startPin != DataPinDescription && startPin.Parent != DataPinDescription.Parent)
                {
                    TtDataOutPinDescription fromPin = null;
                    TtDataInPinDescription toPin = null;
                    if(startPin is TtDataOutPinDescription)
                    {
                        fromPin = (TtDataOutPinDescription)startPin;
                        
                    }
                    if(DataPinDescription is TtDataOutPinDescription)
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
                    if(fromPin == null || toPin == null)
                    {
                        return;
                    }
                    if (!CheckPinsLinkable(startPin, DataPinDescription))
                    {
                        return;
                    }
                    
                    var line = new TtDataLineDescription() { Name = "Data_" + fromPin.Parent.Name + "_To_" + toPin.Parent.Name, FromId = fromPin.Id, ToId = toPin.Id };
                    context.CommandHistory.CreateAndExtuteCommand("AddDataLine",
                        (data) => { methodGraph.MethodDescription.AddDataLine(line); },
                        (data) => { methodGraph.MethodDescription.RemoveDataLine(line); });
                }
            }
            methodGraph.PreviewDataLine = null;
        }
        
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");

            if(context.DesignedGraph is TtGraph_Method methodGraph)
            {
                foreach(var line in methodGraph.MethodDescription.DataLines)
                {
                    if(line.FromId == Id || line.ToId == Id)
                    {
                        parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                        {
                            cmdHistory.CreateAndExtuteCommand("Break Link",
                                (data) => { methodGraph.MethodDescription.RemoveDataLine(line); },
                                (data) => { methodGraph.MethodDescription.AddDataLine(line); }
                                );
                        });
                    }
                }
                
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
            var nodeStart = context.ViewPortTransform(fromPin.Icon.AbsCenter);
            var nodeEnd = context.ViewPortTransform(toPin.Icon.AbsCenter);
            cmdlist.AddLine(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(1,1,1,1)), 5);
        }
    }
    public class TtGraphElementRender_PreviewDataLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_PreviewDataLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(line.AbsLocation);
            var mousePosInViewPort = context.ViewPort.ViewportInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos());
            var nodeEnd = context.ViewPortTransform(mousePosInViewPort);
            cmdlist.AddLine(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(1,1,1,1)), 5);
        }
    }
    public class TtGraphElementRender_DataPin : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pin = renderableElement as TtGraphElement_DataPin;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(pin.AbsLocation);
            var nodeEnd = context.ViewPortTransform(pin.AbsLocation + new Vector2(pin.Size.Width, pin.Size.Height));
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(pin.ElementContainer);
            elementContainerRender.Draw(pin.ElementContainer, ref context);
        }
    }
}

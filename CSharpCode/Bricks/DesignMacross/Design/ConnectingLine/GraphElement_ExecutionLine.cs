using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExecutionLine))]
    public class TtGraphElement_ExecutionLine : TtDescriptionGraphElement
    {
        public TtExecutionLineDescription ExecutionLineDescription { get => Description as TtExecutionLineDescription; }
        public IGraphElement From { get; set; } = null;
        public IGraphElement To { get; set; } = null;

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
            From = context.DescriptionsElement[ExecutionLineDescription.FromId];
            To = context.DescriptionsElement[ExecutionLineDescription.ToId];
            base.AfterConstructElements(ref context);
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_PreviewExecutionLine))]
    public class TtGraphElement_PreviewExecutionLine: IGraphElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get; set; }
        public SizeF Size { get; set; }
        public IGraphElement Parent { get; set; }
        public IGraphElementStyle Style { get; set; }

        public TtExecutionPinDescription StartPin { get; set; } = null;

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
    [ImGuiElementRender(typeof(TtGraphElementRender_ExecutionPin))]
    public class TtGraphElement_ExecutionPin : TtDescriptionGraphElement
    {
        public TtExecutionPinDescription ExecutionPinDescription { get => Description as TtExecutionPinDescription; }
        public override FMargin Margin { get; set; } = new FMargin(2, 2, 2, 2);
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel ElementContainer = new();
        public TtGraphElement_Icon Icon = new();
        public Color4f BackgroundColor 
        { 
            get=> Style.BackgroundColor; 
            set=> Style.BackgroundColor = value;
        }
        public float Rounding { get; set; } = 5;
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
            var methodGraph = context.DesignedGraph as TtGraph_Method;
            methodGraph.PreviewExecutionLine = new TtGraphElement_PreviewExecutionLine();
            methodGraph.PreviewExecutionLine.AbsLocation = Icon.AbsCenter;
            methodGraph.PreviewExecutionLine.StartPin = ExecutionPinDescription;
        }

        public override void OnMouseLeftButtonUp(ref FGraphElementRenderingContext context)
        {
            var methodGraph = context.DesignedGraph as TtGraph_Method;
            if (methodGraph.PreviewDataLine != null)
            {
                methodGraph.PreviewDataLine = null;
            }
            if (methodGraph.PreviewExecutionLine != null)
            {
                var startPin = methodGraph.PreviewExecutionLine.StartPin;
                if (startPin != ExecutionPinDescription && startPin.Parent != ExecutionPinDescription.Parent)
                {
                    var fromId = Guid.Empty;
                    var fromDescName = "";
                    var toId = Guid.Empty;
                    var toDescName = "";
                    bool validLine = false;
                    if (methodGraph.PreviewExecutionLine.StartPin is TtExecutionInPinDescription)
                    {
                        if(ExecutionPinDescription is TtExecutionOutPinDescription)
                        {
                            validLine = true;
                            fromId = ExecutionPinDescription.Id;
                            fromDescName = ExecutionPinDescription.Parent.Name;
                            toId = methodGraph.PreviewExecutionLine.StartPin.Id;
                            toDescName = methodGraph.PreviewExecutionLine.StartPin.Parent.Name;
                        }
                    }
                    else
                    {
                        if (ExecutionPinDescription is TtExecutionInPinDescription)
                        {
                            validLine = true;
                            System.Diagnostics.Debug.Assert(methodGraph.PreviewExecutionLine.StartPin is TtExecutionOutPinDescription);
                            fromId = methodGraph.PreviewExecutionLine.StartPin.Id;
                            fromDescName = methodGraph.PreviewExecutionLine.StartPin.Parent.Name;
                            toId = ExecutionPinDescription.Id;
                            toDescName = ExecutionPinDescription.Parent.Name;
                        }
                    }
                    if(validLine)
                    {
                        var line = new TtExecutionLineDescription() { Name = "Exec_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                        context.CommandHistory.CreateAndExtuteCommand("AddExecutionLine",
                                (data) => { methodGraph.MethodDescription.AddExecutionLine(line); },
                                (data) => { methodGraph.MethodDescription.RemoveExecutionLine(line); });
                    }
                }
            }
            methodGraph.PreviewExecutionLine = null;
        }
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");

            if (context.DesignedGraph is TtGraph_Method methodGraph)
            {
                foreach (var line in methodGraph.MethodDescription.ExecutionLines)
                {
                    if (line.FromId == Id || line.ToId == Id)
                    {
                        parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                        {
                            cmdHistory.CreateAndExtuteCommand("Break Link",
                                (data) => { methodGraph.MethodDescription.RemoveExecutionLine(line); },
                                (data) => { methodGraph.MethodDescription.AddExecutionLine(line); }
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
            var nodeStart = context.ViewPortTransform(fromPin.Icon.AbsCenter);
            var nodeEnd = context.ViewPortTransform(toPin.Icon.AbsCenter);
            cmdlist.AddLine(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(1,1,1,1)), 5);
        }
    }
    public class TtGraphElementRender_PreviewExecutionLine : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var line = renderableElement as TtGraphElement_PreviewExecutionLine;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(line.AbsLocation);
            var mousePosInViewPort = context.ViewPort.ViewportInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos());
            var nodeEnd = context.ViewPortTransform(mousePosInViewPort);
            cmdlist.AddLine(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(new Color4f(1,1,1,1)), 5);
        }
    }
    public class TtGraphElementRender_ExecutionPin : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pin = renderableElement as TtGraphElement_ExecutionPin;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(pin.AbsLocation);
            var nodeEnd = context.ViewPortTransform(pin.AbsLocation + new Vector2(pin.Size.Width, pin.Size.Height));
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(pin.ElementContainer);
            elementContainerRender.Draw(pin.ElementContainer, ref context);
        }
    }
}

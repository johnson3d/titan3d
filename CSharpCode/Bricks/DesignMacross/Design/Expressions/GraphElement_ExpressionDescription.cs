using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;

namespace EngineNS.DesignMacross.Design.Expressions
{
    public class TtGraphElement_ExpressionDescription : TtDescriptionGraphElement, IEnumChild
    {
        public TtExpressionDescription ExpressionDescription { get => Description as TtExpressionDescription; }
        public float Rounding { get; set; } = 5;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(0.5f, 188f / 255, 212f / 255, 240f / 255);
        //public Color4f BackgroundColor { get; set; } = new Color4f(0.5f, 0.9f, 0.9f, 0.9f);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.9f, 0.9f, 0.9f);
        public float BorderThickness { get; set; } = 2;
        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel ExpressionDescStackPanel = new TtGraphElement_StackPanel();
        //public TtGraphElement_StackPanel PinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_DockPanel PinsPanel = new();
        public TtGraphElement_StackPanel LeftSidePinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel RightSidePinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_ExpressionDescription(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            MinSize = new SizeF(100, 80);
            MaxSize = new SizeF(200, 160);
            Size = new SizeF(100, 80);
            ElementContainer.HorizontalAlignment = EHorizontalAlignment.Stretch;
            ElementContainer.VerticalAlignment = EVerticalAlignment.Stretch;

            ExpressionDescStackPanel.Margin = new FMargin(2, 2, 2, 2);
            ExpressionDescStackPanel.HorizontalAlignment = EHorizontalAlignment.Left;
            ExpressionDescStackPanel.Parent = ElementContainer;
            ElementContainer.AddElement(ExpressionDescStackPanel);

            NameTextBlock.Content = ExpressionDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.BackgroundColor = new Color4f(0, 0.8f, 0);
            ExpressionDescStackPanel.AddElement(NameTextBlock);

            PinsPanel.Margin = new FMargin(0, 0, 0, 0);
            PinsPanel.Parent = ElementContainer;
            PinsPanel.VerticalAlignment = EVerticalAlignment.Stretch;
            ElementContainer.AddElement(PinsPanel);

            LeftSidePinsStackPanel.Margin = new FMargin(0, 0, 0, 0);
            LeftSidePinsStackPanel.Parent = PinsPanel;
            LeftSidePinsStackPanel.Orientation = EOrientation.Vertical;
            PinsPanel.AddElement(EDockPosition.Left,LeftSidePinsStackPanel);

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
                foreach(var execInPin in ExpressionDescription.ExecutionInPins)
                {
                    var execPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_ExecutionPin>(execInPin.GetType());
                    if (execPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(execPinAttribute.ClassType, execInPin, context.GraphElementStyleManager.GetOrAdd(execInPin.Id)) as TtGraphElement_ExecutionPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(execInPin.Id, instance);
                    }
                }
                foreach (var dataPin in ExpressionDescription.DataInPins)
                {
                    var dataPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_DataPin>(dataPin.GetType());
                    if (dataPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(dataPinAttribute.ClassType, dataPin, context.GraphElementStyleManager.GetOrAdd(dataPin.Id)) as TtGraphElement_DataPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(dataPin.Id, instance);
                    }
                }
            }
            RightSidePinsStackPanel.Clear();
            {
                foreach(var execOutPin in ExpressionDescription.ExecutionOutPins)
                {
                    var execPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_ExecutionPin>(execOutPin.GetType());
                    if (execPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(execPinAttribute.ClassType, execOutPin, context.GraphElementStyleManager.GetOrAdd(execOutPin.Id)) as TtGraphElement_ExecutionPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        RightSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(execOutPin.Id, instance);
                    }
                }
                foreach (var dataPin in ExpressionDescription.DataOutPins)
                {
                    var dataPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_DataPin>(dataPin.GetType());
                    if (dataPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(dataPinAttribute.ClassType, dataPin, context.GraphElementStyleManager.GetOrAdd(dataPin.Id)) as TtGraphElement_DataPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        RightSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(dataPin.Id, instance);
                    }
                }
            }
        }

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");
            parentMenu.AddMenuItem(
               "Delete", null,
               (TtMenuItem item, object sender) =>
               {
                   List<TtExecutionLineDescription> executionLinesToBeRemoved = new();
                   List<TtDataLineDescription> dataLinesToBeRemoved = new();
                   foreach (var pin in ExpressionDescription.DataPins)
                   {
                       if (ExpressionDescription.Parent is IMethodDescription methodDescription)
                       {
                           var line = methodDescription.GetDataLineWithPin(pin);
                           if (line != null)
                           {
                               dataLinesToBeRemoved.Add(line);
                           }
                       }

                   }
                   foreach (var pin in ExpressionDescription.ExecutionPins)
                   {
                       if (ExpressionDescription.Parent is IMethodDescription methodDescription)
                       {
                           var line = methodDescription.GetExecutionLineWithPin(pin);
                           if (line != null)
                           {
                               executionLinesToBeRemoved.Add(line);
                           }
                       }

                   }

                   cmdHistory.CreateAndExtuteCommand("DeleteExpression",
                       (data) =>
                       {
                           if (ExpressionDescription.Parent is TtMethodDescription methodDescription)
                           {
                               methodDescription.Expressions.Remove(ExpressionDescription);
                               foreach (var line in executionLinesToBeRemoved)
                               {
                                   methodDescription.ExecutionLines.Remove(line);
                               }
                               foreach (var line in dataLinesToBeRemoved)
                               {
                                   methodDescription.DataLines.Remove(line);
                               }
                           }

                       },
                       (data) =>
                       {
                           if (ExpressionDescription.Parent is TtMethodDescription methodDescription)
                           {
                               methodDescription.Expressions.Add(ExpressionDescription);
                               foreach (var line in executionLinesToBeRemoved)
                               {
                                   methodDescription.ExecutionLines.Add(line);
                               }
                               foreach (var line in dataLinesToBeRemoved)
                               {
                                   methodDescription.DataLines.Add(line);
                               }
                           }
                       });
               });

        }
    }
    public class TtGraphElementRender_ExpressionDescription : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var expressionElement = renderableElement as TtGraphElement_ExpressionDescription;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(expressionElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(expressionElement.AbsLocation + new Vector2(expressionElement.Size.Width, expressionElement.Size.Height));
            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersAll;
            var borderOffset = new Vector2(expressionElement.BorderThickness / 2, expressionElement.BorderThickness / 2);
            cmdlist.AddRect(nodeStart - borderOffset, nodeEnd + borderOffset, ImGuiAPI.ColorConvertFloat4ToU32(expressionElement.BorderColor), expressionElement.Rounding, roundCornerFlags, expressionElement.BorderThickness);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(expressionElement.BackgroundColor), expressionElement.Rounding, roundCornerFlags);
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(expressionElement.ElementContainer);
            elementContainerRender.Draw(expressionElement.ElementContainer, ref context);

        }
    }
}

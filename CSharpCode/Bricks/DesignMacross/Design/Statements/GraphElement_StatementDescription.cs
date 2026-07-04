using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using System.Collections.Generic;

namespace EngineNS.DesignMacross.Design.Statement
{
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_StatementDescription : TtDescriptionGraphElement, IEnumChild
    {
        public TtStatementDescription StatementDescription { get => Description as TtStatementDescription; }
        public float Rounding { get; set; } = 5;
        public Color4f NameColor { get; set; } = TtDesignMacrossGraphStyles.StatementTitleForegroundColor;
        public Color4f BackgroundColor { get; set; } = TtDesignMacrossGraphStyles.StatementBackgroundColor;
        public Color4f BorderColor { get; set; } = TtDesignMacrossGraphStyles.GraphElementBorderColor;
        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel ExpressionDescStackPanel = new TtGraphElement_StackPanel();
        //public TtGraphElement_StackPanel PinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_DockPanel PinsPanel = new();
        public TtGraphElement_StackPanel LeftSidePinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel RightSidePinsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StatementDescription(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            MinSize = new SizeF(100, 80);
            MaxSize = new SizeF(200, 160);
            Size = new SizeF(100, 80);
            ElementContainer.HorizontalAlignment = EHorizontalAlignment.Stretch;
            ElementContainer.VerticalAlignment = EVerticalAlignment.Stretch;

            ExpressionDescStackPanel.HorizontalAlignment = EHorizontalAlignment.Left;
            ExpressionDescStackPanel.Parent = ElementContainer;
            ExpressionDescStackPanel.BackgroundColor = TtDesignMacrossGraphStyles.StatementTitleBackgroundColor;
            ExpressionDescStackPanel.Rounding = Rounding;
            ExpressionDescStackPanel.CornerType = ERoundCornerType.RoundCornersTop;
            ElementContainer.AddElement(ExpressionDescStackPanel);

            NameTextBlock.Content = StatementDescription.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.HorizontalAlignment = EHorizontalAlignment.Left;
            NameTextBlock.FontScale = 1.2f;
            NameTextBlock.Rounding = Rounding;
            NameTextBlock.TextColor = NameColor;
            ExpressionDescStackPanel.AddElement(NameTextBlock);

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

        public List<IGraphElement> EnumerateChildRverse<T>() where T : class
        {
            List<IGraphElement> list = EnumerateChild<T>();
            list.Reverse();
            return list;
        }

        public override void OnSelected(ref FMouseEventContext context)
        {
            context.GraphElementRenderingContext.EditorInteroperation.PGMember.Target = Description;
            base.OnSelected(ref context);
        }
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            NameColor = TtDesignMacrossGraphStyles.StatementTitleForegroundColor;
            BackgroundColor = TtDesignMacrossGraphStyles.StatementBackgroundColor;
            BorderColor = TtDesignMacrossGraphStyles.GraphElementBorderColor;
            ExpressionDescStackPanel.BackgroundColor = TtDesignMacrossGraphStyles.StatementTitleBackgroundColor;
            if(IsSelected)
            {
                BorderColor = TtDesignMacrossGraphStyles.GraphElementSelectedColor;
            }
            if (HighLightState == EHighLigthState.LowLight)
            {
                NameColor = new Color4f(NameColor.ToColor3f(), TtDesignMacrossGraphStyles.LowLigthAlpha);
                BackgroundColor = new Color4f(BackgroundColor.ToColor3f(), TtDesignMacrossGraphStyles.LowLigthAlpha);
                BorderColor = new Color4f(BorderColor.ToColor3f(), TtDesignMacrossGraphStyles.LowLigthAlpha);
                ExpressionDescStackPanel.BackgroundColor = new Color4f(ExpressionDescStackPanel.BackgroundColor.ToColor3f(), TtDesignMacrossGraphStyles.LowLigthAlpha); 
            }

            LeftSidePinsStackPanel.Clear();
            {
                foreach (var execInPin in StatementDescription.ExecutionInPins)
                {
                    var execPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_ExecutionPin>(execInPin.GetType());
                    if (execPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(execPinAttribute.ClassType, execInPin, context.GraphElementStyleManager.GetOrAdd(execInPin)) as TtGraphElement_ExecutionPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(execInPin.Id, instance);
                    }
                }
                foreach (var dataPin in StatementDescription.DataInPins)
                {
                    var dataPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_DataPin>(dataPin.GetType());
                    if (dataPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(dataPinAttribute.ClassType, dataPin, context.GraphElementStyleManager.GetOrAdd(dataPin)) as TtGraphElement_DataPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(dataPin.Id, instance);
                    }
                }
            }
            RightSidePinsStackPanel.Clear();
            {
                foreach (var execOutPin in StatementDescription.ExecutionOutPins)
                {
                    var execPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_ExecutionPin>(execOutPin.GetType());
                    if (execPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(execPinAttribute.ClassType, execOutPin, context.GraphElementStyleManager.GetOrAdd(execOutPin)) as TtGraphElement_ExecutionPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        RightSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(execOutPin.Id, instance);
                    }
                }
                foreach (var dataPin in StatementDescription.DataOutPins)
                {
                    var dataPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_DataPin>(dataPin.GetType());
                    if (dataPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(dataPinAttribute.ClassType, dataPin, context.GraphElementStyleManager.GetOrAdd(dataPin)) as TtGraphElement_DataPin;
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
                   foreach (var pin in StatementDescription.DataPins)
                   {
                       var line = IDataLineOperator.GetDataLineWithPin(StatementDescription.Parent, pin);
                       if (line != null)
                       {
                           dataLinesToBeRemoved.Add(line);
                       }
                   }
                   foreach (var pin in StatementDescription.ExecutionPins)
                   {
                       var line = IExecutionLineOperator.GetExecutionLineWithPin(StatementDescription.Parent, pin);
                       if (line != null)
                       {
                           executionLinesToBeRemoved.Add(line);
                       }
                   }

                   var statementDescriptionParent = StatementDescription.Parent;
                   cmdHistory.CreateAndExtuteCommand("DeleteStatement",
                       (data) =>
                       {
                           IStatementOperator.RemoveStatement(statementDescriptionParent, StatementDescription);
                           foreach (var line in executionLinesToBeRemoved)
                           {
                               IExecutionLineOperator.RemoveExecutionLine(statementDescriptionParent, line);
                           }
                           foreach (var line in dataLinesToBeRemoved)
                           {
                               IDataLineOperator.RemoveDataLine(statementDescriptionParent, line);
                           }
                       },
                       (data) =>
                       {
                           IStatementOperator.AddStatement(statementDescriptionParent, StatementDescription);
                           foreach (var line in executionLinesToBeRemoved)
                           {
                               IExecutionLineOperator.AddExecutionLine(statementDescriptionParent, line);
                           }
                           foreach (var line in dataLinesToBeRemoved)
                           {
                               IDataLineOperator.AddDataLine(statementDescriptionParent, line);
                           }
                       });
               });

        }
    }
    public class TtGraphElementRender_StatementDescription : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var statementElement = renderableElement as TtGraphElement_StatementDescription;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewportTransform(statementElement.AbsLocation);
            var nodeEnd = context.ViewportTransform(statementElement.AbsLocation + new Vector2(statementElement.Size.Width, statementElement.Size.Height));
            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersAll;
            var borderOffset = new Vector2(statementElement.BorderThickness / 2, statementElement.BorderThickness / 2);
            cmdlist.AddRect(nodeStart - borderOffset, nodeEnd + borderOffset, ImGuiAPI.ColorConvertFloat4ToU32(statementElement.BorderColor), statementElement.Rounding, roundCornerFlags, statementElement.BorderThickness);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(statementElement.BackgroundColor), statementElement.Rounding, roundCornerFlags);
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(statementElement.ElementContainer);
            elementContainerRender.Draw(statementElement.ElementContainer, ref context);

        }
    }
}

using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.EGui.Controls;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;

namespace EngineNS.Bricks.StateMachine.Macross.CompoundState
{
    [ImGuiElementRender(typeof(TtGraphElement_TimedCompoundStateEntryRender))]
    public class TtGraphElement_TimedCompoundStateEntry : TtDescriptionGraphElement, IStateTransitionInitial, IEnumChild
    {
        public TtTimedCompoundStateEntryClassDescription TimedCompoundStateEntryClassDescription { get => Description as TtTimedCompoundStateEntryClassDescription; }
        public float Rounding { get; set; } = 15;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(188f / 255, 212f / 255, 240f / 255);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.6f, 0.6f, 0.6f);
        public float BorderThickness { get; set; } = 4;

        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel StateDescStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel TransitionsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_TimedCompoundStateEntry(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            Size = new SizeF(100, 80);
            NameTextBlock.Content = description.Name;
            NameTextBlock.VerticalAlignment = EVerticalAlignment.Center;
            NameTextBlock.FontScale = 1.2f;
            ElementContainer.HorizontalAlignment = EHorizontalAlignment.Stretch;

            StateDescStackPanel.AddElement(NameTextBlock);
            StateDescStackPanel.Margin = new FMargin(8, 5, 5, 0);
            StateDescStackPanel.Parent = ElementContainer;
            ElementContainer.AddElement(StateDescStackPanel);

            TransitionsStackPanel.Margin = new FMargin(0, 0, 5, 5);
            TransitionsStackPanel.HorizontalAlignment = EHorizontalAlignment.Stretch;
            TransitionsStackPanel.Parent = ElementContainer;
            ElementContainer.AddElement(TransitionsStackPanel);
            ElementContainer.Parent = this;
        }
        List<IDescriptionGraphElement> ChildrenDescriptionGraphElements = new List<IDescriptionGraphElement>();
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            TransitionsStackPanel.Clear();
            NameTextBlock.Content = Description.Name;
            var compoundState = Parent as TtGraph_TimedCompoundState;
            foreach (var transition in TimedCompoundStateEntryClassDescription.Transitions)
            {
                var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_TimedStateTransition>(transition.GetType());
                if (graphElementAttribute != null)
                {
                    var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, transition, context.GraphElementStyleManager.GetOrAdd(transition.Id)) as TtGraphElement_TimedStateTransition;
                    instance.Parent = this;
                    TransitionsStackPanel.AddElement(instance);
                    ChildrenDescriptionGraphElements.Add(instance);
                }
            }

        }
        #region IContextMeunable
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");
            if (Parent is TtGraph_TimedCompoundState parent)
            {
                var statesInParent = parent.TimedCompoundStateClassDescription.States;
                if (statesInParent != null)
                {
                    var transitionItem = parentMenu.AddMenuItem("TransitionTo", null, null);
                    foreach (var state in statesInParent)
                    {
                        if (Description == state)
                            continue;

                        transitionItem.AddMenuItem(state.Name, null, (TtMenuItem item, object sender) =>
                        {
                            var transitionDesc = new TtTimedStateTransitionClassDescription() { FromId = Description.Id, ToId = state.Id};
                            cmdHistory.CreateAndExtuteCommand("Transition From" + this.Name + " To " + state.Name,
                                (data) => { TimedCompoundStateEntryClassDescription.AddTransition(transitionDesc); },
                                (data) => { TimedCompoundStateEntryClassDescription.RemoveTransition(transitionDesc); }
                                );
                        });
                    }
                }
            }
        }
        #endregion IContextMeunable
        private Dictionary<ILayoutable, SizeF> ChildrenMeasuringSize = new Dictionary<ILayoutable, SizeF>();
        public override SizeF Measuring(SizeF availableSize)
        {
            var childrenDesiredSize = ElementContainer.Measuring(availableSize);
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
            foreach (var element in ChildrenDescriptionGraphElements)
            {
                if (element is T)
                {
                    list.Add(element);
                }
            }
            return list;
        }
    }

    public class TtGraphElement_TimedCompoundStateEntryRender : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var entryElement = renderableElement as TtGraphElement_TimedCompoundStateEntry;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(entryElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(entryElement.AbsLocation + new Vector2(entryElement.Size.Width, entryElement.Size.Height));
            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersBottomLeft | ImDrawFlags_.ImDrawFlags_RoundCornersTopRight;
            cmdlist.AddRect(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(entryElement.BorderColor), entryElement.Rounding, roundCornerFlags, entryElement.BorderThickness * 2);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(entryElement.BackgroundColor), entryElement.Rounding, roundCornerFlags);

            for (int i = entryElement.TransitionsStackPanel.Children.Count - 1; i >= 0; i--)
            {
                var transitionPanelElement = entryElement.TransitionsStackPanel.Children[i];
                if (transitionPanelElement is TtGraphElement_TimedStateTransition transition)
                {
                    if (i == entryElement.TransitionsStackPanel.Children.Count - 1)
                    {
                        transition.TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_RoundCornersBottomLeft;
                        transition.TimeDurationBarRounding = entryElement.Rounding;
                    }
                    else
                    {
                        transition.TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_Closed;
                        transition.TimeDurationBarRounding = 0;
                    }
                }
            }
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(entryElement.ElementContainer);
            elementContainerRender.Draw(entryElement.ElementContainer, ref context);

        }
    }
}

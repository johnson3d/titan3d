using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph.Elements;
using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EngineNS.Rtti;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.DesignMacross.TimedStateMachine.StatesHub
{
    [ImGuiElementRender(typeof(TtGraphElement_TimedStatesHubEntryRender))]
    public class TtGraphElement_TimedStatesHubEntry : IGraphElement, IContextMeunable, IDraggable, ILayoutable, IStateTransitionInitial
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get => "Entry"; set { } }

        public IGraphElement Parent { get; set; }
        private IDescription mDescription = null;
        public IDescription Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                mDescription = value;
                TimedStatesHubClassDescription.Transitions_StartFromThis.CollectionChanged -= Transitions_StartFromThis_CollectionChanged;
                TimedStatesHubClassDescription.Transitions_StartFromThis.CollectionChanged += Transitions_StartFromThis_CollectionChanged;
            }
        }

        private void Transitions_StartFromThis_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //TODO: Transitions_CollectionChanged
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
            }
            Construct();
        }

        public TtTimedStatesHubClassDescription TimedStatesHubClassDescription { get => Description as TtTimedStatesHubClassDescription; }
        public Vector2 Location { get => Description.Location; set => Description.Location = value; }
        public Vector2 AbsLocation { get => TtGraphMisc.CalculateAbsLocation(this); }
        public SizeF Size { get; set; } = new SizeF(100, 80);
        public float Rounding { get; set; } = 15;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(188f/255, 212f/255, 240f/255);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.6f, 0.6f, 0.6f);
        public float BorderThickness { get; set; } = 4;

        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel StateDescStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel TransitionsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_TimedStatesHubEntry(IDescription description)
        {
            Id = description.Id;
            Description = description;
            NameTextBlock.Content = Name;
            NameTextBlock.VerticalAlign = EVerticalAlignment.Center;
            NameTextBlock.FontScale = 1.2f;

            StateDescStackPanel.AddElement(NameTextBlock);
            StateDescStackPanel.Margin = new FMargin(8, 5, 5, 0);
            TransitionsStackPanel.Parent = ElementContainer;
            TransitionsStackPanel.Margin = new FMargin(0, 0, 5, 5);
            ElementContainer.AddElement(StateDescStackPanel);
            ElementContainer.AddElement(TransitionsStackPanel);
            ElementContainer.Parent = this;
        }
        public void Construct()
        {
            TransitionsStackPanel.Clear();
            foreach (var transition in TimedStatesHubClassDescription.Transitions_StartFromThis)
            {
                var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_TimedStateTransition>(transition.GetType());
                if (graphElementAttribute != null)
                {
                    var instance = UTypeDescManager.CreateInstance(graphElementAttribute.ClassType) as TtGraphElement_TimedStateTransition;
                    IGraphElement GetElementByDesc(IDescription description)
                    {
                        if (Parent is TtGraph_TimedStatesHub parent)
                        {
                            var elementsInParent = parent.Elements;
                            foreach (var element in elementsInParent)
                            {
                                if (element.Description == description)
                                    return element;
                            }
                        }
                        return null;
                    }
                    instance.From = GetElementByDesc(transition.From);
                    instance.To = GetElementByDesc(transition.To);
                    instance.Description = transition;
                    instance.Construct();
                    instance.Parent = this;
                    TransitionsStackPanel.AddElement(instance);
                }
            }
        }
        #region ISelectable
        public bool HitCheck(Vector2 pos)
        {
            Rect rect = new Rect(AbsLocation, Size);
            //冗余一点
            Rect mouseRect = new Rect(pos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(mouseRect);
        }

        public void OnSelected()
        {

        }

        public void OnUnSelected()
        {

        }
        #endregion ISelectable
        #region IDraggable
        public bool CanDrag()
        {
            return true;
        }
        public void OnDragging(Vector2 delta)
        {
            Location += delta;
        }
        #endregion IDraggable
        #region IContextMeunable
        public TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("TimedStateHubEntryNodeContextMenu");
        public FMargin Margin { get; set; } = FMargin.Default;


        public void UpdateContextMenu(ref FGraphElementRenderingContext context)
        {
            PopupMenu.StringId = Name + "_ContextMenu";
            List<IGraphElement> elementsInParent = null;
            if (Parent is TtGraph_TimedStatesHub parent)
            {
                elementsInParent = parent.Elements;
            }
            PopupMenu.Reset();
            var parentMenu = PopupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            if (elementsInParent != null)
            {
                var transitionItem = parentMenu.AddMenuItem("TransitionTo", null, null);
                foreach (IGraphElement element in elementsInParent)
                {
                    if (element == this || !(element is TtGraphElement_TimedState))
                        continue;

                    transitionItem.AddMenuItem(element.Name, null, (UMenuItem item, object sender) =>
                    {
                        var transitionDesc = new TtTimedStateTransitionClassDescription() { From = this.Description, To = element.Description };
                        cmdHistory.CreateAndExtuteCommand("Transition From" + this.Name + " To " + element.Name,
                            (data) => { TimedStatesHubClassDescription.Transitions_StartFromThis.Add(transitionDesc); },
                            (data) => { TimedStatesHubClassDescription.Transitions_StartFromThis.Remove(transitionDesc); }
                            );
                    });
                }
            }



        }
        public void OpenContextMeun()
        {
            PopupMenu.OpenPopup();
        }

        public void DrawContextMenu(ref FGraphElementRenderingContext context)
        {
            PopupMenu.Draw(ref context);
        }
        private Dictionary<ILayoutable, SizeF> ChildrenMeasuringSize = new Dictionary<ILayoutable, SizeF>();
        public SizeF Measuring(SizeF availableSize)
        {
            var childrenDesiredSize = ElementContainer.Measuring(availableSize);
            var size = new SizeF();
            size.Width = Size.Width > childrenDesiredSize.Width ? Size.Width : childrenDesiredSize.Width;
            size.Height = childrenDesiredSize.Height;
            return new SizeF(size.Width + Margin.Left + Margin.Right, size.Height + Margin.Top + Margin.Bottom);
        }

        public SizeF Arranging(Rect finalRect)
        {
            //Location = finalRect.Location; Location have already set
            Size = new SizeF(finalRect.Width, finalRect.Height);
            ElementContainer.Arranging(new Rect(Vector2.Zero, finalRect.Size));
            return finalRect.Size;
        }

        #endregion IContextMeunable
    }

    public class TtGraphElement_TimedStatesHubEntryRender : IGraphElementRender
    {
        public IGraphElement Element { get; set; } = null;
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var statesHubEntryElement = renderableElement as TtGraphElement_TimedStatesHubEntry;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(statesHubEntryElement.AbsLocation);

            //Element.Size = new SizeF(nameTextSize.Width , nameTextSize.Height < minHeight ? minHeight : nameTextSize.Height);

            var nodeEnd = context.ViewPortTransform(statesHubEntryElement.AbsLocation + new Vector2(statesHubEntryElement.Size.Width, statesHubEntryElement.Size.Height));

            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersBottomLeft | ImDrawFlags_.ImDrawFlags_RoundCornersTopRight;
            cmdlist.AddRect(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(statesHubEntryElement.BorderColor), statesHubEntryElement.Rounding, roundCornerFlags, statesHubEntryElement.BorderThickness * 2);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(statesHubEntryElement.BackgroundColor), statesHubEntryElement.Rounding, roundCornerFlags);
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(statesHubEntryElement.ElementContainer);
            elementContainerRender.Draw(statesHubEntryElement.ElementContainer, ref context);
            for (int i = statesHubEntryElement.TransitionsStackPanel.Children.Count - 1; i >= 0; i--)
            {
                var transitionPanelElement = statesHubEntryElement.TransitionsStackPanel.Children[i];
                if (transitionPanelElement is TtGraphElement_TimedStateTransition transition)
                {
                    if (i == statesHubEntryElement.TransitionsStackPanel.Children.Count - 1)
                    {
                        transition.TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_RoundCornersBottomLeft;
                        transition.TimeDurationBarRounding = statesHubEntryElement.Rounding;
                    }
                    else
                    {
                        transition.TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_Closed;
                        transition.TimeDurationBarRounding = 0;
                    }
                }
            }

            if (statesHubEntryElement is IContextMeunable meunablenode)
            {
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                    && context.ViewPort.IsInViewPort(ImGuiAPI.GetMousePos()))
                {
                    var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    if (statesHubEntryElement.HitCheck(pos))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        meunablenode.UpdateContextMenu(ref context);
                        meunablenode.OpenContextMeun();
                    }
                }
                meunablenode.DrawContextMenu(ref context);
            }
        }
    }
}

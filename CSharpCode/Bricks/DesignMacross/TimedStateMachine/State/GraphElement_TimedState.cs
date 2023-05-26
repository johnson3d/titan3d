using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Editor.DeclarationPanel;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Graph;
using Org.BouncyCastle.Crypto.Agreement;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.DesignMacross.Graph.Elements;
using EngineNS.DesignMacross.Graph;
using NPOI.POIFS.Properties;
using EngineNS.DesignMacross.Outline;
using EngineNS.DesignMacross.Render;
using EngineNS.DesignMacross.Description;
using System.Reflection;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [ImGuiElementRender(typeof(TtGraphElement_TimedStateRender))]
    public class TtGraphElement_TimedState : IGraphElement, IContextMeunable, IDraggable, ILayoutable, IStateTransitionAcceptable, IStateTransitionInitial
    {
        public string Name { get => Description.Name; set => Description.Name = value; }
        public float Duration { get; set; } = 3.0f;
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
                TimedStateClassDescription.Transitions.CollectionChanged -= Transitions_CollectionChanged;
                TimedStateClassDescription.Transitions.CollectionChanged += Transitions_CollectionChanged;
            }
        }

        private void Transitions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        public TtTimedStateClassDescription TimedStateClassDescription { get => Description as TtTimedStateClassDescription; }
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get => TtGraphMisc.CalculateAbsLocation(this); }
        public SizeF Size { get; set; } = new SizeF(100, 80);
        public float Rounding { get; set; } = 15;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(4f/255, 118f / 255, 208f / 255);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.6f, 0.6f, 0.6f);
        public float BorderThickness { get; set; } = 4;

        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_TextBlock DurationTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel StateDescStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel TransitionsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_TimedState(IDescription description)
        {
            Description = description;
            NameTextBlock.Content = Name;
            NameTextBlock.VerticalAlign = EVerticalAlignment.Center;
            NameTextBlock.FontScale = 1.2f;

            DurationTextBlock.Content = "Duration : " + Duration.ToString() + "s";
            DurationTextBlock.VerticalAlign = EVerticalAlignment.Center;

            StateDescStackPanel.AddElement(NameTextBlock);
            StateDescStackPanel.AddElement(DurationTextBlock);
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
            foreach (var transition in TimedStateClassDescription.Transitions)
            {
                var graphElementAttribute = transition.GetType().GetCustomAttribute<GraphElementAttribute>();
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
                                if(element.Description == description)
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
            Rect rect = new Rect(Location, Size);
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
        public TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("TimedStateNodeContextMenu");
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
                            () => { TimedStateClassDescription.Transitions.Add(transitionDesc); },
                            () => { TimedStateClassDescription.Transitions.Remove(transitionDesc); }
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

        public Vector2 GetTransitionLinkPosition(ELineDirection lineDirection)
        {
            if (lineDirection == ELineDirection.East)
            {
                return AbsLocation + new Vector2(0, Size.Height * 0.5f);
            }
            return AbsLocation;
        }

        #endregion IContextMeunable
    }
    public class TtGraphElement_TimedStateRender : IGraphElementRender
    {
        public IGraphElement Element { get; set; } = null;
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var stateElement = renderableElement as TtGraphElement_TimedState;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(stateElement.AbsLocation);

            //Element.Size = new SizeF(nameTextSize.Width , nameTextSize.Height < minHeight ? minHeight : nameTextSize.Height);

            var nodeEnd = context.ViewPortTransform(stateElement.AbsLocation + new Vector2(stateElement.Size.Width, stateElement.Size.Height));

            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersBottomRight | ImDrawFlags_.ImDrawFlags_RoundCornersTopLeft;
            cmdlist.AddRect(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(stateElement.BorderColor), stateElement.Rounding, roundCornerFlags, stateElement.BorderThickness * 2);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(stateElement.BackgroundColor), stateElement.Rounding, roundCornerFlags);
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(stateElement.ElementContainer);
            elementContainerRender.Draw(stateElement.ElementContainer, ref context);
            for(int i = stateElement.TransitionsStackPanel.Children.Count - 1; i >=0; i--)
            {
                var transitionPanelElement = stateElement.TransitionsStackPanel.Children[i];
                if (transitionPanelElement is TtGraphElement_TimedStateTransition transition)
                {
                    if(i == stateElement.TransitionsStackPanel.Children.Count - 1)
                    {
                        transition.TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_RoundCornersBottomRight;
                        transition.TimeDurationBarRounding = stateElement.Rounding;
                    }
                    else
                    {
                        transition.TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_Closed;
                        transition.TimeDurationBarRounding = 0;
                    }
                }
            }

            if (stateElement is IContextMeunable meunablenode)
            {
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                    && context.ViewPort.IsInViewPort(ImGuiAPI.GetMousePos()))
                {
                    var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    if (stateElement.HitCheck(pos))
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

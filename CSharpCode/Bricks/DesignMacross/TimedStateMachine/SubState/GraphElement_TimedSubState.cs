using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.TimedStateMachine.CompoundState;
using EngineNS.EGui.Controls;
using System.Collections.Generic;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [ImGuiElementRender(typeof(TtGraphElementRender_TimedSubState))]
    public class TtGraphElement_TimedSubState : TtDescriptionGraphElement, IStateTransitionAcceptable, IStateTransitionInitial, IEnumChild
    {
        public TtTimedSubStateClassDescription TimedSubStateClassDescription { get => Description as TtTimedSubStateClassDescription; }
        TtTimedCompoundStateClassDescription ParentTimedCompoundStateClassDescription { get => Description.Parent as TtTimedCompoundStateClassDescription; }
        public float Rounding { get; set; } = 15;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(111f / 255, 168f / 255, 219f / 255);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.6f, 0.6f, 0.6f);
        public float BorderThickness { get; set; } = 4;

        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_TextBlock DurationTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel StateDescStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel AttachmentsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel TransitionsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_TimedSubState(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            Size = new SizeF(150, 80);
            NameTextBlock.Content = Name;
            NameTextBlock.VerticalAlign = EVerticalAlignment.Center;
            NameTextBlock.FontScale = 1.2f;

            DurationTextBlock.Content = "Duration : " + TimedSubStateClassDescription.Duration.ToString() + "s";
            DurationTextBlock.VerticalAlign = EVerticalAlignment.Center;

            StateDescStackPanel.AddElement(NameTextBlock);
            StateDescStackPanel.AddElement(DurationTextBlock);

            StateDescStackPanel.Parent = ElementContainer;
            StateDescStackPanel.Margin = new FMargin(8, 8, 5, 0);
            ElementContainer.AddElement(StateDescStackPanel);

            AttachmentsStackPanel.Parent = ElementContainer;
            AttachmentsStackPanel.Margin = new FMargin(8, 8, 5, 0);
            ElementContainer.AddElement(AttachmentsStackPanel);

            TransitionsStackPanel.Parent = ElementContainer;
            TransitionsStackPanel.Margin = new FMargin(0, 0, 5, 5);
            ElementContainer.AddElement(TransitionsStackPanel);

            ElementContainer.Parent = this;
        }

        #region IContextMeunable
        TtTimedSubStateClassDescription Copy()
        {
            return new TtTimedSubStateClassDescription();
        }
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {
            PopupMenu.StringId = Name + "_" + Id + "_" + "ContextMenu";
            PopupMenu.Reset();
            PopupMenu.bHasSearchBox = false;
            var parentMenu = PopupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");
            parentMenu.AddMenuItem(
               "Delete", null,
               (TtMenuItem item, object sender) =>
               {
                   Dictionary<TtTimedSubStateClassDescription, TtTimedStateTransitionClassDescription> transitionsToBeRemoved = new();
                   foreach (var state in ParentTimedCompoundStateClassDescription.States)
                   {
                       if (state == TimedSubStateClassDescription)
                           continue;
                       foreach (var transition in state.Transitions)
                       {
                           if (transition.To == TimedSubStateClassDescription)
                           {
                               transitionsToBeRemoved.Add(state, transition);
                           }
                       }
                   }
                   TtTimedStateTransitionClassDescription transitionsToBeRemovedInEntry = null;
                   foreach (var transition in ParentTimedCompoundStateClassDescription.Entry.Transitions)
                   {
                       if (transition.To == TimedSubStateClassDescription)
                       {
                           transitionsToBeRemovedInEntry = transition;
                       }
                   }
                   cmdHistory.CreateAndExtuteCommand("DeleteSubState",
                       (data) =>
                       {
                           ParentTimedCompoundStateClassDescription.States.Remove(TimedSubStateClassDescription);
                           foreach (var transitionToBeRemoved in transitionsToBeRemoved)
                           {
                               transitionToBeRemoved.Key.RemoveTransition(transitionToBeRemoved.Value);
                           }
                           if(transitionsToBeRemovedInEntry != null)
                           {
                               ParentTimedCompoundStateClassDescription.Entry.RemoveTransition(transitionsToBeRemovedInEntry);
                           }
                       },
                       (data) => 
                       {
                           ParentTimedCompoundStateClassDescription.States.Add(TimedSubStateClassDescription);
                           foreach (var transitionToBeRemoved in transitionsToBeRemoved)
                           {
                               transitionToBeRemoved.Key.AddTransition(transitionToBeRemoved.Value);
                           }
                           if (transitionsToBeRemovedInEntry != null)
                           {
                               ParentTimedCompoundStateClassDescription.Entry.AddTransition(transitionsToBeRemovedInEntry);
                           }
                       });
               });
            parentMenu.AddMenuItem(
                "Duplicate", null,
                (TtMenuItem item, object sender) =>
                {
                    //var copied = Copy();
                    //cmdHistory.CreateAndExtuteCommand("DuplicateSubState",
                    //   (data) => { ParentTimedCompoundStateClassDescription.States.Add(copied); },
                    //   (data) => { ParentTimedCompoundStateClassDescription.States.Remove(copied); });
                });
            if (Parent is TtGraph_TimedCompoundState parent)
            {
                var statesInParent = parent.TimedCompoundStateClassDescription.States;
                var hubsInParent = parent.TimedCompoundStateClassDescription.Hubs;
                if (statesInParent != null)
                {
                    var transitionItem = parentMenu.AddMenuItem("TransitionTo", null, null);
                    foreach (var state in statesInParent)
                    {
                        if (Description == state)
                            continue;
                        transitionItem.AddMenuItem(state.Name, null, (TtMenuItem item, object sender) =>
                        {
                            var transitionDesc = new TtTimedStateTransitionClassDescription() { FromId = this.Description.Id, ToId = state.Id };
                            cmdHistory.CreateAndExtuteCommand("Transition From" + this.Name + " To " + state.Name,
                                (data) => { TimedSubStateClassDescription.AddTransition(transitionDesc); },
                                (data) => { TimedSubStateClassDescription.RemoveTransition(transitionDesc); }
                                );
                        });
                    }
                    var hubTransitionItem = parentMenu.AddMenuItem("TransitionTo", null, null);
                    foreach (var hub in hubsInParent)
                    {
                        transitionItem.AddMenuItem(hub.Name, null, (TtMenuItem item, object sender) =>
                        {
                            var transitionDesc = new TtTimedStateTransitionClassDescription() { FromId = this.Description.Id, ToId = hub.Id };
                            cmdHistory.CreateAndExtuteCommand("Transition From" + this.Name + " To " + hub.Name,
                                (data) => { TimedSubStateClassDescription.AddTransition(transitionDesc); },
                                (data) => { TimedSubStateClassDescription.RemoveTransition(transitionDesc); }
                                );
                        });
                    }
                }
            }
            var types = TypeHelper.CollectTypesByAttribute<TimedStateAttachmentContextMenuAttribute>();
            foreach (var type in types)
            {
                if (type.AttributeInstance != null)
                {
                    TtMenuUtil.ConstructMenuItem(PopupMenu.Menu, type.TypeDesc, type.AttributeInstance.MenuPaths, type.AttributeInstance.FilterStrings,
                         (TtMenuItem item, object sender) =>
                         {
                             var popMenu = sender as TtPopupMenu;
                             if (Rtti.UTypeDescManager.CreateInstance(type.TypeDesc) is TtTimedStateAttachmentClassDescription attachment)
                             {
                                 attachment.Name = GetValidAttachmenName(attachment.Name);
                                 TimedSubStateClassDescription.AddAttachment(attachment);
                             }
                         });

                }
            }

        }
        public string GetValidAttachmenName(string name)
        {
            int index = 0;
            var newName = name;
            var nameCheckValid = (string newName) =>
            {
                foreach (var attachment in TimedSubStateClassDescription.Attachments)
                {
                    if (attachment.Name == newName)
                    {
                        return false;
                    }
                }
                return true;
            };
            while (!nameCheckValid(newName))
            {
                index++;
                newName = name + "_" + index;
            }
            return newName;
        }
        #endregion IContextMeunable

        private Dictionary<ILayoutable, SizeF> ChildrenMeasuringSize = new Dictionary<ILayoutable, SizeF>();
        #region ILayoutable
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
        #endregion ILayoutable
        public Vector2 GetTransitionLinkPosition(ELineDirection lineDirection)
        {
            if (lineDirection == ELineDirection.East)
            {
                return AbsLocation + new Vector2(0, Size.Height * 0.5f);
            }
            return AbsLocation;
        }
        List<IDescriptionGraphElement> ChildrenDescriptionGraphElements = new List<IDescriptionGraphElement>();
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            NameTextBlock.Content = Name;
            DurationTextBlock.Content = "Duration : " + TimedSubStateClassDescription.Duration.ToString() + "s";
            AttachmentsStackPanel.Clear();
            TransitionsStackPanel.Clear();
            ChildrenDescriptionGraphElements.Clear();

            foreach (var attachment in TimedSubStateClassDescription.Attachments)
            {
                var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtDescriptionGraphElement>(attachment.GetType());
                if (graphElementAttribute != null)
                {
                    var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, attachment, context.GraphElementStyleManager.GetOrAdd(attachment.Id));
                    instance.Parent = this;
                    AttachmentsStackPanel.AddElement(instance);
                    ChildrenDescriptionGraphElements.Add(instance);
                }
            }

            foreach (var transition in TimedSubStateClassDescription.Transitions)
            {
                var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_TimedStateTransition>(transition.GetType());
                if (graphElementAttribute != null)
                {
                    var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, transition, context.GraphElementStyleManager.GetOrAdd(transition.Id));
                    instance.Parent = this;
                    TransitionsStackPanel.AddElement(instance);
                    ChildrenDescriptionGraphElements.Add(instance);
                }
            }
            foreach (var element in ChildrenDescriptionGraphElements)
            {
                element.ConstructElements(ref context);
            }

            base.ConstructElements(ref context);
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
    public class TtGraphElementRender_TimedSubState : IGraphElementRender
    {
        TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("GraphElementRender_TimedSubState");
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var stateElement = renderableElement as TtGraphElement_TimedSubState;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(stateElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(stateElement.AbsLocation + new Vector2(stateElement.Size.Width, stateElement.Size.Height));
            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersBottomRight | ImDrawFlags_.ImDrawFlags_RoundCornersTopLeft;
            cmdlist.AddRect(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(stateElement.BorderColor), stateElement.Rounding, roundCornerFlags, stateElement.BorderThickness * 2);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(stateElement.BackgroundColor), stateElement.Rounding, roundCornerFlags);

            if (stateElement is IContextMeunable meunablenode)
            {
                meunablenode.SetContextMenuableId(PopupMenu);
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                    && context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                {
                    var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    if (stateElement.HitCheck(pos))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        meunablenode.ConstructContextMenu(ref context, PopupMenu);
                        PopupMenu.OpenPopup();
                    }
                }
                PopupMenu.Draw(ref context);
            }

            for (int i = stateElement.TransitionsStackPanel.Children.Count - 1; i >= 0; i--)
            {
                var transitionPanelElement = stateElement.TransitionsStackPanel.Children[i];
                if (transitionPanelElement is TtGraphElement_TimedStateTransition transition)
                {
                    if (i == stateElement.TransitionsStackPanel.Children.Count - 1)
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
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(stateElement.ElementContainer);
            elementContainerRender.Draw(stateElement.ElementContainer, ref context);



        }
    }
}

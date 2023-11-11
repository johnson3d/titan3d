using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.DesignMacross.TimedStateMachine.CompoundState
{
    [ImGuiElementRender(typeof(TtGraphElement_TimedCompoundStateHubRender))]
    public class TtGraphElement_TimedCompoundStateHub : TtDescriptionGraphElement, IStateTransitionAcceptable
    {
        public TtTimedCompoundStateHubClassDescription CompoundStateHubClassDescription { get => Description as TtTimedCompoundStateHubClassDescription; }
        TtTimedCompoundStateClassDescription ParentTimedCompoundStateClassDescription { get => Description.Parent as TtTimedCompoundStateClassDescription; }
        public float Rounding { get; set; } = 5;
        public Color4f NameColor { get; set; } = new Color4f(0.0f, 0.0f, 0.0f);
        public Color4f BackgroundColor { get; set; } = new Color4f(158 / 255, 194f / 255, 229f / 255);
        public Color4f BorderColor { get; set; } = new Color4f(0.5f, 0.6f, 0.6f, 0.6f);
        public float BorderThickness { get; set; } = 4;

        public override SizeF Size 
        {
            get
            {
                return Style.Size;
            }
            set
            {
                Style.Size = value;
            }
        }
        public TtGraphElement_StackPanel ElementContainer = new TtGraphElement_StackPanel();
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel CompoundStatesDescStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_StackPanel TransitionsStackPanel = new TtGraphElement_StackPanel();
        public TtGraphElement_TimedCompoundStateHub(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            Size = new SizeF(100, 80);
            NameTextBlock.Content = CompoundStateHubClassDescription.Name;
            NameTextBlock.VerticalAlign = EVerticalAlignment.Center;
            NameTextBlock.FontScale = 1.2f;
            CompoundStatesDescStackPanel.AddElement(NameTextBlock);

            CompoundStatesDescStackPanel.Parent = ElementContainer;
            CompoundStatesDescStackPanel.Margin = new FMargin(8, 5, 5, 0);
            ElementContainer.AddElement(CompoundStatesDescStackPanel);

            ElementContainer.Parent = this;
        }
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            NameTextBlock.Content = CompoundStateHubClassDescription.Name;
            base.ConstructElements(ref context);
        }

        #region IContextMeunable

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
               (UMenuItem item, object sender) =>
               {
                   Dictionary<TtTimedSubStateClassDescription, TtTimedStateTransitionClassDescription> transitionsToBeRemoved = new();
                   foreach (var state in ParentTimedCompoundStateClassDescription.States)
                   {
                       foreach (var transition in state.Transitions)
                       {
                           if (transition.To == Description)
                           {
                               transitionsToBeRemoved.Add(state, transition);
                           }
                       }
                   }
                   TtTimedStateTransitionClassDescription transitionsToBeRemovedInEntry = null;
                   foreach (var transition in ParentTimedCompoundStateClassDescription.Entry.Transitions)
                   {
                       if (transition.To == ParentTimedCompoundStateClassDescription)
                       {
                           transitionsToBeRemovedInEntry = transition;
                       }
                   }
                   cmdHistory.CreateAndExtuteCommand("DeleteHub",
                       (data) => 
                       {
                           ParentTimedCompoundStateClassDescription.Hubs.Remove(CompoundStateHubClassDescription);
                           foreach (var transitionToBeRemoved in transitionsToBeRemoved)
                           {
                               transitionToBeRemoved.Key.RemoveTransition(transitionToBeRemoved.Value);
                           }
                           if (transitionsToBeRemovedInEntry != null)
                           {
                               ParentTimedCompoundStateClassDescription.Entry.RemoveTransition(transitionsToBeRemovedInEntry);
                           }
                       },
                       (data) => 
                       { 
                           ParentTimedCompoundStateClassDescription.Hubs.Add(CompoundStateHubClassDescription);
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
            //if (Parent is TtGraph_TimedCompoundState parent)
            //{
            //    var elementsInParent = parent.Elements;

            //    if (elementsInParent != null)
            //    {
            //        var transitionItem = parentMenu.AddMenuItem("TransitionTo", null, null);
            //        foreach (IGraphElement element in elementsInParent)
            //        {
            //            if (element == this || !(element is TtGraphElement_TimedSubState))
            //                continue;
            //            if (element is TtGraphElement_TimedSubState timedStateElement)
            //            {
            //                transitionItem.AddMenuItem(element.Name, null, (UMenuItem item, object sender) =>
            //                {
            //                    var elementDesc = timedStateElement.TimedSubStateClassDescription;
            //                    var transitionDesc = new TtTimedStateTransitionClassDescription() { FromId = timedStateElement.TimedSubStateClassDescription.Id, ToId = this.Description.Id };
            //                    cmdHistory.CreateAndExtuteCommand("Transition From" + element.Name + " To " + this.Name,
            //                        (data) => { timedStateElement.TimedSubStateClassDescription.AddTransition(transitionDesc);},
            //                        (data) => { timedStateElement.TimedSubStateClassDescription.RemoveTransition(transitionDesc);}
            //                        );
            //                });
            //            }
            //        }
            //    }
            //}
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
        public Vector2 GetTransitionLinkPosition(ELineDirection lineDirection)
        {
            if (lineDirection == ELineDirection.East)
            {
                return AbsLocation + new Vector2(0, Size.Height * 0.5f);
            }
            return AbsLocation;
        }
        
    }

    public class TtGraphElement_TimedCompoundStateHubRender : IGraphElementRender
    {
        TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("GraphElement_TimedCompoundStateHubRender");
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var hubElement = renderableElement as TtGraphElement_TimedCompoundStateHub;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(hubElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(hubElement.AbsLocation + new Vector2(hubElement.Size.Width, hubElement.Size.Height));
            var roundCornerFlags = ImDrawFlags_.ImDrawFlags_RoundCornersAll;
            cmdlist.AddRect(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(hubElement.BorderColor), hubElement.Rounding, roundCornerFlags, hubElement.BorderThickness * 2);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(hubElement.BackgroundColor), hubElement.Rounding, roundCornerFlags);

            if (hubElement is IContextMeunable meunableNode)
            {
                meunableNode.SetContextMenuableId(PopupMenu);
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                    && context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                {
                    var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    if (hubElement.HitCheck(pos))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        meunableNode.ConstructContextMenu(ref context, PopupMenu);
                        PopupMenu.OpenPopup();
                    }
                }
                PopupMenu.Draw(ref context);
            }
            var elementContainerRender = TtElementRenderDevice.CreateGraphElementRender(hubElement.ElementContainer);
            elementContainerRender.Draw(hubElement.ElementContainer, ref context);
        }
    }
}

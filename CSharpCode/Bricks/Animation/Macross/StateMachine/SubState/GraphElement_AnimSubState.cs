using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.Bricks.StateMachine.Macross.StateAttachment;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;

namespace EngineNS.Bricks.StateMachine.Macross.SubState{
    [ImGuiElementRender(typeof(TtGraphElementRender_TimedSubState))]
    public class TtGraphElement_AnimSubState : TtGraphElement_TimedSubState
    {
        public TtGraphElement_AnimSubState(IDescription description, IGraphElementStyle style) : base(description, style)
        {

        }

        #region IContextMeunable
        TtTimedSubStateClassDescription Copy()
        {
            return new TtTimedSubStateClassDescription();
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
                            var transitionDesc = new TtAnimStateTransitionClassDescription() { FromId = this.Description.Id, ToId = state.Id };
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
                            var transitionDesc = new TtAnimStateTransitionClassDescription() { FromId = this.Description.Id, ToId = hub.Id };
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
                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, type.TypeDesc, type.AttributeInstance.MenuPaths, type.AttributeInstance.FilterStrings,
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
        #endregion IContextMeunable



    }
}

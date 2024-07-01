using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.EGui.Controls;

namespace EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState
{
    [ImGuiElementRender(typeof(TtGraphElement_TimedCompoundStateEntryRender))]
    public class TtGraphElement_AnimCompoundStateEntry : TtGraphElement_TimedCompoundStateEntry
    {
        public TtGraphElement_AnimCompoundStateEntry(IDescription description, IGraphElementStyle style) : base(description, style)
        {

        }
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
                            var transitionDesc = new TtAnimStateTransitionClassDescription() { FromId = Description.Id, ToId = state.Id };
                            cmdHistory.CreateAndExtuteCommand("Transition From" + this.Name + " To " + state.Name,
                                (data) => { TimedCompoundStateEntryClassDescription.AddTransition(transitionDesc); },
                                (data) => { TimedCompoundStateEntryClassDescription.RemoveTransition(transitionDesc); }
                                );
                        });
                    }
                }
            }
        }
    }

    
}

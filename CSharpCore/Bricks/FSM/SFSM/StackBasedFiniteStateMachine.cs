using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM.SFSM
{
    public class StackBasedFiniteStateMachine : FiniteStateMachine
    {
        protected Stack<StackBasedState> StatesStack = new Stack<StackBasedState>();
        public StackBasedFiniteStateMachine()
        {

        }
        public StackBasedFiniteStateMachine(string name) : base(name)
        {

        }
        public override void SetCurrentStateByTransition(Transition transition)
        {
            mPostState = transition.To;
            OnTransitionEexecuteActions = transition.OnTransitionEexecuteActions;
            if (StateChangeMode == StateChangeMode.Immediately)
            {
                SwapPostToCurrent();
                mCurrentState.Tick();
            }
        }
        protected override void SwapPostToCurrent()
        {
            if (mPostState == null)
                return;
            mPreviousState?.Exit();
            mPreviousState = mCurrentState;
            mCurrentState = mPostState;
            mPostState = null;
            CurrentState.Enter();
            OnStateChange();
            if (OnTransitionEexecuteActions != null)
            {
                for (int i = 0; i < OnTransitionEexecuteActions.Count; ++i)
                {
                    OnTransitionEexecuteActions[i]?.Invoke();
                }
            }
        }
    }
}

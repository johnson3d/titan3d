using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM
{
    public class Transition
    {
        public IState From { get; set; } = null;
        public IState To { get; set; } = null;
        public List<Action> OnTransitionEexecuteActions { get; set; } = new List<Action>();
        internal virtual bool CheckCondition()
        {
            return CanTransit();
        }
        public virtual bool CanTransit() { return true; }
    }
}

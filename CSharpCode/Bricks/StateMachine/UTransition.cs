using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public class UTransition
    {
        public IState From { get; set; } = null;
        public IState To { get; set; } = null;
        public virtual void OnTransition() { }
        public virtual bool Check() { return true; }
    }
}

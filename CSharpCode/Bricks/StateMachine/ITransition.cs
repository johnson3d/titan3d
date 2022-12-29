using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface ITransition
    {
        public IState From { get; set; } 
        public IState To { get; set; }
        public void OnTransition();
        public bool Check();
    }
}

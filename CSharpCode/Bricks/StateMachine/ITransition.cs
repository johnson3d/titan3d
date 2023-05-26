using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface ITransition<T>
    {
        public IState<T> From { get; set; }
        public IState<T> To { get; set; }
        public void OnTransition();
        public bool Check();
    }
}

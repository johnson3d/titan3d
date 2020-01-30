using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM
{
    public interface IState
    {
        string Name { get; set; }
        void Initialize();
        void Enter();
        void Exit();
        void Tick();
        bool AddTransition(Transition transition);
        bool RemoveTransition(Transition transition);
    }
}

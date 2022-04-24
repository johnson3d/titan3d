using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface IState
    {
        string Name { get; set; }
        void Initialize();
        void Enter();
        void Exit();
        void Tick(float elapseSecond);
        bool AddTransition(UTransition transition);
        bool RemoveTransition(UTransition transition);
    }
}

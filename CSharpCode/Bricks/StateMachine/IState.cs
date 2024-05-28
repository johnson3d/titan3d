using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface IState<S,T>
    {
        public S CenterData { get; set; }
        string Name { get; set; }
        bool Initialize();
        void Enter();
        void Exit();
        /// <summary>
        /// Tick this state node every frame
        /// </summary>
        /// <param name="elapseSecond"></param>
        void Tick(float elapseSecond, in T context);
        /// <summary>
        /// update game logic ,can be paused
        /// </summary>
        /// <param name="elapseSecond"></param>
        void Update(float elapseSecond, in T context);
        bool TryCheckTransitions(in T context, out List<ITransition<S, T>> transitions);
        bool ShouldUpdate();
        bool AddTransition(ITransition<S, T> transition);
        bool RemoveTransition(ITransition<S, T> transition);
        bool AddAttachment(IAttachment<S, T> attachment);
        bool RemoveAttachment(IAttachment<S, T> attachment);
    }
}

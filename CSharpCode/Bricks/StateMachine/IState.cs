using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface IState<T>
    {
        string Name { get; set; }
        void Initialize();
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
        bool TryCheckTransitions(out List<ITransition<T>> transitions);
        bool ShouldUpdate();
        bool AddTransition(ITransition<T> transition);
        bool RemoveTransition(ITransition<T> transition);
        bool AddAttachment(IAttachmentRule<T> attachment);
        bool RemoveAttachment(IAttachmentRule<T> attachment);
    }
}

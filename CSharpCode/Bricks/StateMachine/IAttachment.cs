using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface IAttachment<S, T>
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
        bool ShouldUpdate();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface IAttachment
    {
        string Name { get; set; }
        void Initialize();
        void Enter();
        void Exit();
        /// <summary>
        /// Tick this state node every frame
        /// </summary>
        /// <param name="elapseSecond"></param>
        void Tick(float elapseSecond);
        /// <summary>
        /// update game logic ,can be paused
        /// </summary>
        /// <param name="elapseSecond"></param>
        void Update(float elapseSecond);
        bool ShouldUpdate();
    }
}

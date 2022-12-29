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
        bool AddTransition(ITransition transition);
        bool RemoveTransition(ITransition transition);
        bool AddAttachment(IAttachmentRule attachment);
        bool RemoveAttachment(IAttachmentRule attachment);
    }
}

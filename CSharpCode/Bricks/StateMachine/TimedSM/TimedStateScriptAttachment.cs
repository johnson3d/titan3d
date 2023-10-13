using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public delegate void EventOnEnter();
    public class TtTimedStateScriptAttachment<T> : IAttachment<T>
    {
        public event EventOnEnter OnEnter;
        public string Name { get; set; }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public void Initialize()
        {
        }

        public bool ShouldUpdate()
        {
            return true;
        }

        public void Tick(float elapseSecond, in T context)
        {
        }

        public void Update(float elapseSecond, in T context)
        {
        }
    }
    public class TtTimedStateScriptAttachment: TtTimedStateScriptAttachment<FStateMachineContext>
    {

    }
}

using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS0067

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public delegate void EventOnEnter();
    public class TtTimedStateScriptAttachment<S, T> : IAttachment<S, T>
    {
        public S CenterData { get; set; }
        public event EventOnEnter OnEnter;
        public string Name { get; set; }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public async Thread.Async.TtTask<bool> Initialize(T context)
        {
            return false;
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
    public class TtTimedStateScriptAttachment<S> : TtTimedStateScriptAttachment<S, TtStateMachineContext>
    {

    }

    public class TtTimedStateScriptAttachment: TtTimedStateScriptAttachment<TtDefaultCenterData, TtStateMachineContext>
    {

    }
}

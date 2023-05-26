using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class TtTimedStateAttachment<T> : IAttachment<T>
    {
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
    public class TtTimedStateAttachmentQueue<T> : IAttachment<T>
    {
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

    public class TtTimedStateAttachment : TtTimedStateAttachment<FStateMachineContext>
    {

    }

    public class TtTimedStateAttachmentQueue : TtTimedStateAttachmentQueue<FStateMachineContext>
    {

    }
}

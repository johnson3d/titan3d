using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class TtTimedStateAttachment<S, T> : IAttachment<S, T>
    {
        public S CenterData { get; set; }
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
    public class TtTimedStateAttachmentQueue<S, T> : IAttachment<S, T>
    {
        public S CenterData { get; set; }
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

    public class TtTimedStateAttachment<S> : TtTimedStateAttachment<S, TtStateMachineContext>
    {

    }

    public class TtTimedStateAttachmentQueue<S> : TtTimedStateAttachmentQueue<S, TtStateMachineContext>
    {

    }

    public class TtTimedStateAttachment : TtTimedStateAttachment<TtDefaultCenterData, TtStateMachineContext>
    {

    }

    public class TtTimedStateAttachmentQueue : TtTimedStateAttachmentQueue<TtDefaultCenterData, TtStateMachineContext>
    {

    }
}

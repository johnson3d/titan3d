using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class TtTimedStateAttachment<S, T> : IAttachment<S, T>
    {
        public S CenterData { get; set; }
        public string Name { get; set; }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual bool Initialize()
        {
            return false;
        }

        public virtual bool ShouldUpdate()
        {
            return true;
        }

        public virtual void Tick(float elapseSecond, in T context)
        {
            
        }

        public virtual void PostTick(float elapseSecond, in T context)
        {
            if (ShouldUpdate())
            {
                Update(elapseSecond, context);
            }
        }

        public virtual void Update(float elapseSecond, in T context)
        {
        }
    }
    public class TtTimedStateAttachmentQueue<S, T> : IAttachment<S, T>
    {
        public S CenterData { get; set; }
        public string Name { get; set; }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual bool Initialize()
        {
            return false;
        }

        public virtual bool ShouldUpdate()
        {
            return true;
        }

        public virtual void Tick(float elapseSecond, in T context)
        {

        }

        public virtual void PostTick(float elapseSecond, in T context)
        {
            if (ShouldUpdate())
            {
                Update(elapseSecond, context);
            }
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

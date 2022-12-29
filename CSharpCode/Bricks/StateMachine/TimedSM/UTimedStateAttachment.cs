using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class UTimedStateAttachment : IAttachment
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

        public void Tick(float elapseSecond)
        {
        }

        public void Update(float elapseSecond)
        {
        }
    }
    public class UTimedStateAttachmentQueue : IAttachment
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

        public void Tick(float elapseSecond)
        {
        }

        public void Update(float elapseSecond)
        {
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface IAttachmentRule<T>
    {
        public IAttachment<T> Attachment { get; set; }
        public void Tick(float elapseSecond, in T context);
        public bool Check();
    }
}

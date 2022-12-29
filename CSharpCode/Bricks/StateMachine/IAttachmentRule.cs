using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface IAttachmentRule
    {
        public IAttachment Attachment { get; set; }
        public void Tick(float elapseSecond);
        public bool Check();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Notify
{
    public interface IAnimNotify : IO.ISerializer
    {
        public bool CanTrigger(Int64 beforeTime, Int64 afterTime);
        public void Trigger(long beforeTime, long afterTime);
        public Guid ID { get; set; }
    }
}

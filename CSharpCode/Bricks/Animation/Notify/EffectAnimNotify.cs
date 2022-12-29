using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Notify
{
    [Rtti.Meta]
    public class SoundAnimNotify : TransientAnimNotify
    {
        [Rtti.Meta]
        public RName Sound { get; set; }
        public override void Trigger(long beforeTime, long afterTime)
        {
            base.Trigger(beforeTime, afterTime);
            //Sound trigger
        }
    }
}

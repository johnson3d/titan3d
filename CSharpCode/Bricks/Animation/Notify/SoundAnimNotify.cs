using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Notify
{
    [Rtti.Meta]
    public class EffectAnimNotify : TransientAnimNotify
    {
        [Rtti.Meta]
        public RName Particle { get; set; }
        [Rtti.Meta]
        public Vector3 Position { get; set; }
        [Rtti.Meta]
        public Vector3 Rotation { get; set; }
        [Rtti.Meta]
        public Vector3 Scale { get; set; }
        [Rtti.Meta]
        public bool WorldSpace { get; set; } = false;
        [Rtti.Meta]
        public string SocketName { get; set; } = "";
        public override void Trigger(long beforeTime, long afterTime)
        {
            base.Trigger(beforeTime, afterTime);
            //particle trigger
        }
    }
}

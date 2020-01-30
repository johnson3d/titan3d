using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class TriggerEvent
    {
        float Tick = 0;
        float PreTriggerTick = 0;

        bool IsFirst = true;

        public CGfxParticlePose Pose;
        public TriggerNode.Data TriggerData;

        public bool CanTriggerParticle(CGfxParticleSystem sys)
        {
            if (Tick > sys.LiveTime)
                return false;

            if (Tick <= sys.FireDelay)
                return false;

            if (IsFirst)
            {
                IsFirst = false;
                return true;
            }

            return Tick - PreTriggerTick > sys.FireInterval;
        }

        public bool IsDeath(CGfxParticleSystem sys)
        {
            return Tick > sys.LiveTime;
        }

        public void ResetPreTriggerTick()
        {
            PreTriggerTick = Tick;
        }

        public void Update(float e)
        {
            Tick += e;
        }
    }
}

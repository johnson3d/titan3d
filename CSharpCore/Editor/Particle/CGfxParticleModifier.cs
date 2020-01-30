using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public partial class CGfxParticleModifier
    {
        public float CurrentTime = 0.0f;

        partial void UpdateTime()
        {
            CurrentTime += CEngine.Instance.EngineElapseTimeSecond * Speed;
        }

        public string CurrentTimeToString()
        {
            return $"{Math.Round(CurrentTime, 2, MidpointRounding.AwayFromZero)} s";
        }
    }
}

using EngineNS.Animation.Animatable;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Pipeline
{
    public class UPropertiesSettingCommand : IAnimationCommand
    {
        public UAnimationPropertiesSetter AnimationPropertiesSetter;
        public float Time;

        public void Execute()
        {
            if (AnimationPropertiesSetter == null)
                return;
            AnimationPropertiesSetter.Evaluate(Time);
        }
    }
}

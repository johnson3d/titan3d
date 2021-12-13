using EngineNS.Animation.Animatable;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Pipeline
{
    public struct PropertiesSettingCommandContext
    {
        public UAnimationPropertiesSetter AnimationPropertiesSetter;
        public float Time;
        //clamp pingpong repeat

        public PropertiesSettingCommandContext(UAnimationPropertiesSetter animationPropertiesSetter, float time)
        {
            Time = time;
            AnimationPropertiesSetter = animationPropertiesSetter;
        }
    }

    public class UPropertiesSettingCommand : IAnimationCommand
    {
        public PropertiesSettingCommandContext Context { get; set; } = default;

        public void Excute()
        {
            if (Context.AnimationPropertiesSetter == null)
                return;
            Context.AnimationPropertiesSetter.Evaluate(Context.Time);
        }
    }
}

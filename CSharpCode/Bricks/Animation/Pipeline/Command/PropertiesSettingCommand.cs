using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Pipeline
{
    public struct PropertiesSettingCommandContext
    {
        public Dictionary<Animatable.IPropertySetter, Curve.ICurve> PropertySetFuncMapping;
        public float Time;
        //clamp pingpong repeat

        public PropertiesSettingCommandContext(Dictionary<Animatable.IPropertySetter, Curve.ICurve> propertySetFuncMapping, float time)
        {
            Time = time;
            PropertySetFuncMapping = propertySetFuncMapping;
        }
    }

    public class PropertiesSettingCommand : IAnimationCommand
    {
        public PropertiesSettingCommandContext Context { get; set; } = default;

        public void Excute()
        {
            var it = Context.PropertySetFuncMapping.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Key.SetValue(it.Current.Value, Context.Time);
            }
        }
    }
}

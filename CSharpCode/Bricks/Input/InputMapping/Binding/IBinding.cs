using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Binding
{
    public interface IBindingData
    {
    }
    public interface IBinding
    {
        public static T Create<T>(IBindingData data) where T : IBinding
        {
            var obj = Activator.CreateInstance<T>();
            obj.Initialize(data);
            return obj;
        }
        public void Initialize(IBindingData data);
        public void Tick();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Action
{
    public interface IActionData
    {
    }
    public interface IAction
    {
        public static T Create<T>(IActionData data) where T : IAction
        {
            var obj = Activator.CreateInstance<T>();
            obj.Initialize(data);
            TtEngine.Instance.InputSystem.RegAction(obj);
            return obj;
        }
        public void Initialize(IActionData data);
        public void Tick();
    }

}

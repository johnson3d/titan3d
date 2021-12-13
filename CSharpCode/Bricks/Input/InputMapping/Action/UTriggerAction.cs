using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Action
{
    public delegate void TriggerActionEvent(UTriggerAction sender);
    public class UTriggerAction : IAction
    {
        public class UTriggerActionData : IActionData
        {

        }
        public event TriggerActionEvent OnPress;
        public event TriggerActionEvent OnRelease;

        public Binding.UTriggerBinding Binding { get; set; }
        public void Tick()
        {
            Binding?.Tick();
            if (Binding.WasPressedThisFrame)
            {
                OnPress?.Invoke(this);
            }
            if (Binding.WasPressedThisFrame)
            {
                OnRelease?.Invoke(this);
            }
        }

        public void Initialize(IActionData data)
        {

        }
    }
}

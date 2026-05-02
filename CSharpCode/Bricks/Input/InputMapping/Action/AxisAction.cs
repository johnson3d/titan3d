using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Action
{
    public delegate void AxisActionEvent(UAxisAction sender, float value);
    public class UAxisAction : IAction
    {
        public class UAxisAction1DData : IActionData
        {

        }
        public event AxisActionEvent OnValueUpdate;
        public Binding.UAxisBinding Binding { get; set; }
        public void Initialize(IActionData data)
        {

        }

        public void Tick()
        {
            Binding?.Tick();
            OnValueUpdate?.Invoke(this, Binding.Value);
        }
    }
    public delegate void Axis2DActionEvent(UAxis2DAction sender, Vector2 value);
    public class UAxis2DAction : IAction
    {
        public class UAxis2DActionData : IActionData
        {

        }
        public event Axis2DActionEvent OnValueUpdate;
        public Binding.UAxis2DBinding Binding { get; set; }
        public void Initialize(IActionData data)
        {

        }

        public void Tick()
        {
            Binding?.Tick();
            OnValueUpdate?.Invoke(this, Binding.Value);
        }
    }
}

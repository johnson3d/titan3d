using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Action
{
    public delegate void Value1DActionEvent(UValue1DAction sender, float value);
    public class UValue1DAction : IAction
    {
        public class UValueAction1DData : IActionData
        {

        }
        public event Value1DActionEvent OnValueUpdate;
        public Binding.UValue1DBinding Binding { get; set; }
        public void Initialize(IActionData data)
        {

        }

        public void Tick()
        {
            Binding?.Tick();
            OnValueUpdate?.Invoke(this, Binding.Value);
        }
    }
    public delegate void Value2DActionEvent(UValueAction2D sender, Vector2 value);
    public class UValueAction2D : IAction
    {
        public class UValueAction2DData : IActionData
        {

        }
        public event Value2DActionEvent OnValueUpdate;
        public Binding.UValue2DBinding Binding { get; set; }
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

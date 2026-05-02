using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Binding
{
    public class UValue1DBinding : IBinding
    {
        public class UValue1DBindingData : IBindingData
        {
            public Control.IValue1DControl ValueControl { get; set; } = null;
        }
        public UValue1DBindingData BindingData { get; set; } = null;
        public Control.IValue1DControl ValueControl
        {
            get => BindingData.ValueControl;
            set => BindingData.ValueControl = value;
        }
        public float Scale { get; set; } = 1.0f;
        public float Value { get; set; } = 0.0f;
        public void Tick()
        {
            Value = ValueControl.Value * Scale;
        }

        public void Initialize(IBindingData data)
        {
            System.Diagnostics.Debug.Assert(data is UValue1DBindingData);
            BindingData = data as UValue1DBindingData;
        }
    }

    public class UValue2DBinding : IBinding
    {
        public class UValue2DBindingData : IBindingData
        {
            public Control.IValue2DControl ValueControl { get; set; } = null;
        }
        public UValue2DBindingData BindingData { get; set; } = null;
        public Control.IValue2DControl ValueControl
        {
            get => BindingData.ValueControl;
            set => BindingData.ValueControl = value;
        }
        public float Scale { get; set; } = 1.0f;
        public Vector2 Value { get; set; } = Vector2.Zero;
        public void Tick()
        {
            Value = ValueControl.Value * Scale;
        }

        public void Initialize(IBindingData data)
        {
            System.Diagnostics.Debug.Assert(data is UValue2DBindingData);
            BindingData = data as UValue2DBindingData;
        }
    }
}

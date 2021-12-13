using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Binding
{
    public class UAxisBinding : IBinding
    {
        public class UAxisBindingData : IBindingData
        {
            public Control.IValue1DControl PositiveControl { get; set; } = null;
            public Control.IValue1DControl NegativeControl { get; set; } = null;
        }
        public UAxisBindingData BindingData { get; set; } = null;
        public Control.IValue1DControl PositiveControl
        {
            get => BindingData.PositiveControl;
            set => BindingData.PositiveControl = value;
        }
        public Control.IValue1DControl NegativeControl
        {
            get => BindingData.NegativeControl;
            set => BindingData.NegativeControl = value;
        }
        public float Scale { get; set; } = 1.0f;
        public float Value { get; set; } = 0.0f;
        public void Tick()
        {
            Value = (PositiveControl.Value + (-NegativeControl.Value)) * Scale;
        }

        public void Initialize(IBindingData data)
        {
            System.Diagnostics.Debug.Assert(data is UAxisBindingData);
            BindingData = data as UAxisBindingData;
        }
    }
    public class UAxis2DBinding : IBinding
    {
        public class UAxis2DBindingData : IBindingData
        {
            public Control.IValue1DControl UpControl { get; set; } = null;
            public Control.IValue1DControl DownControl { get; set; } = null;
            public Control.IValue1DControl LeftControl { get; set; } = null;
            public Control.IValue1DControl RightControl { get; set; } = null;
        }
        public UAxis2DBindingData BindingData { get; set; } = null;
        public Control.IValue1DControl UpControl
        {
            get => BindingData.UpControl;
            set => BindingData.UpControl = value;
        }
        public Control.IValue1DControl DownControl
        {
            get => BindingData.DownControl;
            set => BindingData.DownControl = value;
        }
        public Control.IValue1DControl LeftControl
        {
            get => BindingData.LeftControl;
            set => BindingData.LeftControl = value;
        }
        public Control.IValue1DControl RightControl
        {
            get => BindingData.RightControl;
            set => BindingData.RightControl = value;
        }
        public float Scale { get; set; } = 1.0f;
        public Vector2 Value { get; set; } = Vector2.Zero;
        public void Tick()
        {
            Value = new Vector2(UpControl.Value - DownControl.Value * Scale, (RightControl.Value - LeftControl.Value) * Scale);
        }

        public void Initialize(IBindingData data)
        {
            System.Diagnostics.Debug.Assert(data is UAxis2DBindingData);
            BindingData = data as UAxis2DBindingData;
        }
    }
}

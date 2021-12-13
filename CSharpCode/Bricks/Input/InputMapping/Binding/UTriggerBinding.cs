using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.InputMapping.Binding
{
    public class UTriggerBinding : IBinding
    {
        public class UTriggerBindingData : IBindingData
        {
            public Control.ITriggerControl TriggerCtrl { get; set; } = null;
        }
        public UTriggerBindingData BindingData { get; set; } = null;
        public Control.ITriggerControl TriggerCtrl
        {
            get => BindingData.TriggerCtrl;
            set => BindingData.TriggerCtrl = value;
        }
        public bool WasPressedThisFrame { get => TriggerCtrl.WasPressedThisFrame; }
        public bool IsPressed { get => TriggerCtrl.IsPressed; }
        public bool WasReleasedThisFrame { get => TriggerCtrl.WasReleasedThisFrame; }

        public void Initialize(IBindingData data)
        {
            System.Diagnostics.Debug.Assert(data is UTriggerBindingData);
            BindingData = data as UTriggerBindingData;
        }

        public void Tick()
        {

        }
    }
}

using EngineNS.Bricks.Input.Control;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.Device.Keyboard
{
    public delegate void KeyDownEvent(UControlEvent sender);
    public class UKeyDownEvent : UControlEvent
    {
        public class UKeyDownEventData : UControlEventData
        {
            public Keycode Keycode { get; set; } = Keycode.KEY_UNKNOWN;
        }

        public event KeyDownEvent EventFire;
        public UKeyDownEventData KeyDownData
        {
            get { return mControlData as UKeyDownEventData; }
        }
        public Keycode Keycode
        {
            get
            {
                return KeyDownData.Keycode;
            }
            set
            {
                KeyDownData.Keycode = value;
            }
        }

        public override bool CanTrigging(ref Event e)
        {
            return EventType.KEYDOWN == e.Type && e.Keyboard.Keysym.Sym == Keycode;
        }

        public override void OnTrigging(ref Event e)
        {
            EventFire?.Invoke(this);
        }
        public override void RegSelf(UInputSystem inputSystem)
        {
            inputSystem.RegEvent(EventType.KEYDOWN, this);
        }
    }
    public delegate void KeyUpEvent(UControlEvent sender);
    public class UKeyUpEvent : UControlEvent
    {
        public class UKeyUpEventData : UControlEventData
        {
            public Keycode Keycode { get; set; } = Keycode.KEY_UNKNOWN;
        }

        public event KeyUpEvent EventFire;
        public UKeyUpEventData KeyUpData
        {
            get { return mControlData as UKeyUpEventData; }
        }
        public Keycode Keycode
        {
            get
            {
                return KeyUpData.Keycode;
            }
            set
            {
                KeyUpData.Keycode = value;
            }
        }
        public override bool CanTrigging(ref Event e)
        {
            return EventType.KEYUP == e.Type && e.Keyboard.Keysym.Sym == Keycode;
        }

        public override void OnTrigging(ref Event e)
        {
            EventFire?.Invoke(this);
        }
        public override void RegSelf(UInputSystem inputSystem)
        {
            inputSystem.RegEvent(EventType.KEYUP, this);
        }

    }

    public class UKey : IControl, ITriggerControl, IValue1DControl
    {
        public class UKeyData : IControlData
        {
            public Keycode Keycode { get; set; } = Keycode.KEY_UNKNOWN;
        }
        public UKeyData KeyData { get; set; }
        public Keycode Keycode
        {
            get
            {
                return KeyData.Keycode;
            }
            set
            {
                KeyData.Keycode = value;
            }
        }

        public bool WasPressedThisFrame { get ; set ; }
        public bool IsPressed { get; set; }
        public bool WasReleasedThisFrame { get; set; }
        public float Value { get; set; }

        protected UKeyDownEvent KeyDownEvent = null;
        protected UKeyUpEvent KeyUpEvent = null;

        public event TriggerEvent TriggerPress;
        public event TriggerEvent TriggerRelease;

        public void Initialize(IControlData controlData)
        {
            System.Diagnostics.Debug.Assert(controlData is UKeyData);
            KeyData = controlData as UKeyData;
            {
                var data = new UKeyDownEvent.UKeyDownEventData();
                data.Keycode = Keycode;
                KeyDownEvent = UControlEvent.Create<UKeyDownEvent>(data);
                KeyDownEvent.EventFire += OnKeyDownEvent_KeyDown;
            }
            {
                var data = new UKeyUpEvent.UKeyUpEventData();
                data.Keycode = Keycode;
                KeyUpEvent = UControlEvent.Create<UKeyUpEvent>(data);
                KeyUpEvent.EventFire += OnKeyUpEvent_EventFire;
            }
        }

        private void OnKeyUpEvent_EventFire(UControlEvent sender)
        {
            WasReleasedThisFrame = true;
        }

        private void OnKeyDownEvent_KeyDown(UControlEvent sender)
        {
            WasPressedThisFrame = true;
        }
        public void BeforeTick()
        {
            Value = 0;
        }
        public void Tick()
        {
            if (IsPressed && !UEngine.Instance.InputSystem.IsKeyDown(Keycode))
            {
                WasReleasedThisFrame = true;
            }

            if (WasPressedThisFrame)
            {
                Value = 1.0f;
                TriggerPress?.Invoke(this);
            }
            if (WasReleasedThisFrame)
            {
                Value = 0.0f;
                IsPressed = false;
                TriggerRelease?.Invoke(this);
            }
            if(IsPressed /*&& continuous*/)
            {
                Value = 1.0f;
                //KeyDown.Invoke(this);
            }
        }
        public void AfterTick()
        {

            if (WasPressedThisFrame)
            {
                IsPressed = true;
                WasPressedThisFrame = false;
            }
            if (WasReleasedThisFrame)
            {
                IsPressed = false;
                WasReleasedThisFrame = false;
            }
        }

    }
}

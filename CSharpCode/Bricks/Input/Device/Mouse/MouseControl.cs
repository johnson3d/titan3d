using EngineNS.Bricks.Input.Control;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.Device.Mouse
{
    public delegate void MouseButtonDownEvent(UControlEvent sender);
    public class UMouseButtonDownEvent : UControlEvent
    {
        public class UMouseButtonDownEventData : UControlEventData
        {
            public MouseButton Button { get; set; } = MouseButton.BUTTON_UNKNOWN;
        }

        public event MouseButtonDownEvent EventFire;
        public UMouseButtonDownEventData EventData
        {
            get { return mControlData as UMouseButtonDownEventData; }
        }
        public MouseButton Button
        {
            get
            {
                return EventData.Button;
            }
            set
            {
                EventData.Button = value;
            }
        }

        public override bool CanTrigging(ref Event e)
        {
            return EventType.MOUSEBUTTONDOWN == e.Type && e.MouseButton.Button == (byte)Button;
        }

        public override void OnTrigging(ref Event e)
        {
            EventFire?.Invoke(this);
        }
        public override void RegSelf(TtInputSystem inputSystem)
        {
            inputSystem.RegEvent(EventType.MOUSEBUTTONDOWN, this);
        }
    }
    public delegate void MouseButtonUpEvent(UControlEvent sender);
    public class UMouseButtonUpEvent : UControlEvent
    {
        public class UMouseButtonUpEventData : UControlEventData
        {
            public MouseButton Button { get; set; } = MouseButton.BUTTON_UNKNOWN;
        }

        public event MouseButtonUpEvent EventFire;
        public UMouseButtonUpEventData EventData
        {
            get { return mControlData as UMouseButtonUpEventData; }
        }
        public MouseButton Button
        {
            get
            {
                return EventData.Button;
            }
            set
            {
                EventData.Button = value;
            }
        }
        public override bool CanTrigging(ref Event e)
        {
            return EventType.MOUSEBUTTONUP == e.Type && e.MouseButton.Button == (byte)Button;
        }

        public override void OnTrigging(ref Event e)
        {
            EventFire?.Invoke(this);
        }
        public override void RegSelf(TtInputSystem inputSystem)
        {
            inputSystem.RegEvent(EventType.MOUSEBUTTONUP, this);
        }

    }

    public delegate void MouseMotionEvent(UControlEvent sender, int X, int Y, int DeltaX, int DeltaY);
    public class UMouseMotionEvent : UControlEvent
    {
        public class UMouseMotionEventData : UControlEventData
        {
            
        }

        public event MouseMotionEvent EventFire;
        public UMouseMotionEventData EventData
        {
            get { return mControlData as UMouseMotionEventData; }
        }

        public override bool CanTrigging(ref Event e)
        {
            return EventType.MOUSEMOTION == e.Type;
        }

        public override void OnTrigging(ref Event e)
        {
            EventFire?.Invoke(this, e.MouseMotion.X, e.MouseMotion.Y, e.MouseMotion.xRel, e.MouseMotion.yRel);
        }
        public override void RegSelf(TtInputSystem inputSystem)
        {
            inputSystem.RegEvent(EventType.MOUSEMOTION, this);
        }
    }

    public class UMouseButton : IControl, ITriggerControl, IValue1DControl
    {
        public class UMouseButtonData : IControlData
        {
            public MouseButton Button { get; set; } = MouseButton.BUTTON_UNKNOWN;
        }
        public UMouseButtonData EventData { get; set; }
        public MouseButton Button
        {
            get
            {
                return EventData.Button;
            }
            set
            {
                EventData.Button = value;
            }
        }

        public bool WasPressedThisFrame { get; set; }
        public bool IsPressed { get; set; }
        public bool WasReleasedThisFrame { get; set; }
        public float Value { get; set; }

        protected UMouseButtonDownEvent MouseButtonDownEvent = null;
        protected UMouseButtonUpEvent MouseButtonUpEvent = null;

        public event TriggerEvent TriggerPress;
        public event TriggerEvent TriggerRelease;

        public void Initialize(IControlData controlData)
        {
            System.Diagnostics.Debug.Assert(controlData is UMouseButtonData);
            EventData = controlData as UMouseButtonData;
            {
                var data = new UMouseButtonDownEvent.UMouseButtonDownEventData();
                data.Button = Button;
                MouseButtonDownEvent = UControlEvent.Create<UMouseButtonDownEvent>(data);
                MouseButtonDownEvent.EventFire += OnMouseButtonDownEvent_MouseButtonDown;
            }
            {
                var data = new UMouseButtonUpEvent.UMouseButtonUpEventData();
                data.Button = Button;
                MouseButtonUpEvent = UControlEvent.Create<UMouseButtonUpEvent>(data);
                MouseButtonUpEvent.EventFire += OnMouseButtonUpEvent_EventFire;
            }
        }

        private void OnMouseButtonUpEvent_EventFire(UControlEvent sender)
        {
            WasReleasedThisFrame = true;
        }

        private void OnMouseButtonDownEvent_MouseButtonDown(UControlEvent sender)
        {
            WasPressedThisFrame = true;
        }
        public void BeforeTick()
        {
            Value = 0;
        }
        public void Tick()
        {
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
            if (IsPressed /*&& continuous*/)
            {
                Value = 1.0f;
                //MouseButtonDown.Invoke(this);
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

    public class UMouseMotion : IControl, IValue2DControl
    {
        public class UMouseMotionData : IControlData
        {

        }
        public UMouseMotionData EventData
        {
            get; set;
        }
        protected UMouseMotionEvent MouseMotionEvent = null;
        public Vector2 Value { get; set; }
        public void Initialize(IControlData controlData)
        {
            System.Diagnostics.Debug.Assert(controlData is UMouseMotionData);
            EventData = controlData as UMouseMotionData;
            {
                var data = new UMouseMotionEvent.UMouseMotionEventData();
                MouseMotionEvent = UControlEvent.Create<UMouseMotionEvent>(data);
                MouseMotionEvent.EventFire += OnMouseMotionEvent_EventFire; ;
            }
        }

        private void OnMouseMotionEvent_EventFire(UControlEvent sender, int X, int Y, int DeltaX, int DeltaY)
        {
            Value = new Vector2(DeltaX, DeltaY);
        }

        public void BeforeTick()
        {
            
        }
        public void Tick()
        {
            
        }
        public void AfterTick()
        {
            
        }
    }
}

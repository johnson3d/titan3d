using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.Control
{
    #region Event
    public abstract class UControlEventData : IEventData
    {

    }
    public abstract class UControlEvent : IEvent
    {
        public static T Create<T>() where T : UControlEvent
        {
            var evt = Activator.CreateInstance<T>();
            evt.RegSelf(TtEngine.Instance.InputSystem);
            return evt;
        }
        public static T Create<T>(UControlEventData controlData) where T : UControlEvent
        {
            var evt = Activator.CreateInstance<T>();
            evt.RegSelf(TtEngine.Instance.InputSystem);
            evt.Initialize(controlData);
            return evt;
        }
        protected UControlEventData mControlData = null;
        public abstract bool CanTrigging(ref Event e);
        public abstract void OnTrigging(ref Event e);
        public virtual void Initialize(UControlEventData controlData)
        {
            mControlData = controlData;
        }
        public abstract void RegSelf(UInputSystem inputSystem);
    }

    #endregion Event

    #region Control
    public enum ETriggerType
    {
        Once,
        Continuous,
    }

    public interface IControlData
    {

    }

    public interface IControl
    {
        public static T Create<T>(IControlData controlData) where T : IControl
        {
            var obj = Activator.CreateInstance<T>();
            obj.Initialize(controlData);
            TtEngine.Instance.InputSystem.RegControl(obj);
            return obj;
        }
        public void Initialize(IControlData controlData);
        public void BeforeTick();
        public void Tick();
        public void AfterTick();
    }

    public interface IValue1DControl
    {
        public float Value{ get; set; }
    }
    public interface IValue2DControl
    {
        public Vector2 Value { get; set; }
    }
    public delegate void TriggerEvent(ITriggerControl sender);
    public interface ITriggerControl
    {
        public event TriggerEvent TriggerPress;
        public event TriggerEvent TriggerRelease;
        public bool WasPressedThisFrame { get; set; }
        public bool IsPressed { get; set; } 
        public bool WasReleasedThisFrame { get; set; } 
    }

    #endregion Control
  
}

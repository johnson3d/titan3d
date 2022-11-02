using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Input
{
    #region EventTrigger
    internal class UEventTrigger
    {
        public List<IEvent> Events = new List<IEvent>();
        public void Trigging(Event e)
        {
            foreach (var evt in Events)
            {
                if(evt.CanTrigging(ref e))
                {
                    evt.OnTrigging(ref e);
                }
            }
        }
    }
    #endregion EventTrigger

    public partial class UInputSystem 
    {
        Dictionary<EventType, UEventTrigger> EventTriggerDic = new Dictionary<EventType, UEventTrigger>();

        public bool[] OldKeyboards = new bool[(int)Scancode.NUM_SCANCODES];
        public bool[] Keyboards = new bool[(int)Scancode.NUM_SCANCODES];
        public List<string> DropFiles = new List<string>();
        public bool IsDropFiles = false;
        public void ClearFilesDrop()
        {
            IsDropFiles = false;
            DropFiles.Clear();
        }

        public Device.Mouse.UMouse Mouse;

        public UInputSystem()
        {
            Initialize();
        }

        public bool Initialize()
        {
            var enumValues = Enum.GetValues(typeof(EventType));
            foreach (var enumValue in enumValues)
            {
                EventTriggerDic.Add((EventType)enumValue, new UEventTrigger());
            }

            Mouse = new Device.Mouse.UMouse();
            return true;
        }
        public void BeforeTick()
        {
            foreach (var control in TickableControls)
            {
                control.BeforeTick();
            }
        }

        public unsafe int Tick(UEngine engine)
        {
            if (mKeyboardStateDirty)
            {
                Keyboards.CopyTo(OldKeyboards, 0);
                mKeyboardStateDirty = false;
            }

            Input.Event evt = new Event();
            while (this.PullEvent(ref evt))
            {
                switch(evt.Type)
                {
                    case EventType.QUIT:
                        return - 1;
                    case EventType.DROPFILE:
                        {
                            var str = Marshal.PtrToStringUTF8(evt.Drop.File);
                            DropFiles.Add(str);
                        }
                        break;
                    case EventType.DROPBEGIN:
                        {
                            DropFiles.Clear();
                        }
                        break;
                    case EventType.DROPCOMPLETE:
                        {
                            IsDropFiles = true;
                        }
                        break;
                }

                engine.EventProcessorManager.TickEvent(in evt);
                UpdateKeyboardState(evt);
                if (EventTriggerDic.ContainsKey(evt.Type))
                {
                    if (EventTriggerDic.TryGetValue(evt.Type, out var trigger))
                    {
                        trigger.Trigging(evt);
                    }
                }
            }

            foreach (var control in TickableControls)
            {
                control.Tick();
            }

            foreach (var action in TickableActions)
            {
                action.Tick();
            }
            Mouse.Tick();
            return 0;
        }

        public void AfterTick()
        {
            foreach (var control in TickableControls)
            {
                control.AfterTick();
            }
        }

        bool mKeyboardStateDirty = false;
        public void UpdateKeyboardState(Event evt)
        {
            var key = (int)evt.Keyboard.Keysym.Scancode;
            if (evt.Type == EventType.KEYDOWN)
            {
                OldKeyboards[key] = Keyboards[key];
                Keyboards[key] = true;
                mKeyboardStateDirty = true;
            }
            else if (evt.Type == EventType.KEYUP)
            {
                OldKeyboards[key] = Keyboards[key];
                Keyboards[key] = false;
                mKeyboardStateDirty = true;
            }
        }
        public bool IsKeyDown(Keycode key)
        {
            return Keyboards[(int)GetScancodeFromKey(key)];
        }
        public bool IsKeyDown(Scancode key)
        {
            return Keyboards[(int)key];
        }
        public bool IsKeyPressed(Keycode key)
        {
            var keycode = (int)GetScancodeFromKey(key);
            return (!OldKeyboards[keycode] && Keyboards[keycode]);
        }

        public bool RegEvent(EventType eventType, IEvent evt)
        {
            if (EventTriggerDic.ContainsKey(eventType))
            {
                if (EventTriggerDic.TryGetValue(eventType, out var trigger))
                {
                    trigger.Events.Add(evt);
                    return true;
                }
            }
            return false;
        }
        public void UnRegEvent(IEvent evt)
        {

        }
        List<Control.IControl> TickableControls = new List<Control.IControl>();
        public bool RegControl(Control.IControl control)
        {
            if(!TickableControls.Contains(control))
            {
                TickableControls.Add(control);
                return true;
            }
            return false;
        }
        public void UnRegControl(Control.IControl control)
        {

        }

        List<InputMapping.Action.IAction> TickableActions = new List<InputMapping.Action.IAction>();
        public bool RegAction(InputMapping.Action.IAction action)
        {
            if (!TickableActions.Contains(action))
            {
                TickableActions.Add(action);
                return true;
            }
            return false;
        }
        public void UnRegAction(InputMapping.Action.IAction action)
        {

        }
    }
}
namespace EngineNS
{
    partial class UEngine
    {
        public Bricks.Input.UInputSystem InputSystem
        {
            get;
        } = new Bricks.Input.UInputSystem();
    }
}

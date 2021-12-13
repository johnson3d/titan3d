using SDL2;
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

    public class UInputSystem 
    {
        Dictionary<EventType, UEventTrigger> EventTriggerDic = new Dictionary<EventType, UEventTrigger>();


        
        public bool[] Keyboards = new bool[(int)Scancode.NUM_SCANCODES];

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
            return true;
        }
        public Scancode GetScancodeFromKey(Keycode keyCode)
        {
          return  (Scancode)SDL.SDL_GetScancodeFromKey((SDL.SDL_Keycode)keyCode);
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
            SDL.SDL_Event sdlEvt;
            Event evt;
            while (SDL.SDL_PollEvent(out sdlEvt) != 0)
            {
                if (ImGuiAPI.GetCurrentContext() != (void*)0)
                {
                    EGui.UDockWindowSDL.ImGui_ImplSDL2_ProcessEvent(in sdlEvt);
                }
                MappedEvent(in sdlEvt, out evt);
                if (evt.Type == EventType.QUIT)
                {
                    return -1;
                }
                engine.EventProcessorManager.TickSDLEvent(sdlEvt);
                UpdateKeyboardState(evt);
                if (EventTriggerDic.ContainsKey(evt.Type))
                {
                    UEventTrigger trigger = null;
                    if (EventTriggerDic.TryGetValue(evt.Type, out trigger))
                    {
                        trigger.Trigging(evt);
                    }
                }
            }
            foreach(var control in TickableControls)
            {
                control.Tick();
            }

            foreach (var action in TickableActions)
            {
                action.Tick();
            }
            return 0;
        }

        public void AfterTick()
        {
            foreach (var control in TickableControls)
            {
                control.AfterTick();
            }
        }

        public void UpdateKeyboardState(Event evt)
        {
            if (evt.Type == EventType.KEYDOWN)
            {
                Keyboards[(int)evt.Keyboard.Keysym.Scancode] = true;
            }
            else if (evt.Type == EventType.KEYUP)
            {
                Keyboards[(int)evt.Keyboard.Keysym.Scancode] = false;
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

        public bool RegEvent(EventType eventType, IEvent evt)
        {
            if (EventTriggerDic.ContainsKey(eventType))
            {
                UEventTrigger trigger = null;
                if (EventTriggerDic.TryGetValue(eventType, out trigger))
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

        public static void MappedEvent(in SDL.SDL_Event source, out Input.Event target)
        {
            //can't using Event inputEvent = *(Event*)(p); cause of SDL_Event have non-public members
            target.Type = (EventType)source.type;
            #region DollarGesture
            target.DollarGesture.Type = source.dgesture.type;
            target.DollarGesture.Timestamp = source.dgesture.timestamp;
            target.DollarGesture.TouchId = source.dgesture.touchId;
            target.DollarGesture.GestureId = source.dgesture.gestureId;
            target.DollarGesture.NumFingers = source.dgesture.numFingers;
            target.DollarGesture.Error = source.dgesture.error;
            target.DollarGesture.X = source.dgesture.x;
            target.DollarGesture.Y = source.dgesture.y;
            #endregion DollarGesture
            #region MultiGestureEvent
            target.MultiGesture.Type = source.mgesture.type;
            target.MultiGesture.Timestamp = source.mgesture.timestamp;
            target.MultiGesture.TouchId = source.mgesture.touchId;
            target.MultiGesture.dTheta = source.mgesture.dTheta;
            target.MultiGesture.dDist = source.mgesture.dDist;
            target.MultiGesture.X = source.mgesture.x;
            target.MultiGesture.Y = source.mgesture.y;
            target.MultiGesture.NumFingers = source.mgesture.numFingers;
            target.MultiGesture.Padding = source.mgesture.padding;
            #endregion MultiGestureEvent
            #region TouchFinger
            target.TouchFinger.Type = source.tfinger.type;
            target.TouchFinger.Timestamp = source.tfinger.timestamp;
            target.TouchFinger.TouchId = source.tfinger.touchId;
            target.TouchFinger.FingerId = source.tfinger.fingerId;
            target.TouchFinger.X = source.tfinger.x;
            target.TouchFinger.Y = source.tfinger.y;
            target.TouchFinger.dx = source.tfinger.dx;
            target.TouchFinger.dy = source.tfinger.dy;
            target.TouchFinger.Pressure = source.tfinger.pressure;
            target.TouchFinger.WindowID = source.tfinger.windowID;
            #endregion TouchFinger
            #region SysWM
            target.SysWM.Type = (EventType)source.syswm.type;
            target.SysWM.Timestamp = source.syswm.timestamp;
            target.SysWM.Msg = source.syswm.msg;
            #endregion SysWM
            #region User
            target.User.Type = source.user.type;
            target.User.Timestamp = source.user.timestamp;
            target.User.WindowID = source.user.windowID;
            target.User.Code = source.user.code;
            target.User.Data1 = source.user.data1;
            target.User.Data2 = source.user.data2;
            #endregion User
            #region Quit
            target.Quit.Type = (EventType)source.quit.type;
            target.Quit.Timestamp = source.quit.timestamp;
            #endregion Quit
            #region Sensor
            target.Sensor.Type = (EventType)source.sensor.type;
            target.Sensor.Timestamp = source.sensor.timestamp;
            target.Sensor.Which = source.sensor.which;
            //target.Sensor.Data = source.sensor.data;
            #endregion Sensor
            #region AudioDevice
            target.AudioDevice.Type = source.adevice.type;
            target.AudioDevice.Timestamp = source.adevice.timestamp;
            target.AudioDevice.Which = source.adevice.which;
            target.AudioDevice.IsCapture = source.adevice.iscapture;
            #endregion AudioDevice
            #region ControllerDeviceSensor
            target.ControllerDeviceSensor.Type = (EventType)source.csensor.type;
            target.ControllerDeviceSensor.Timestamp = source.csensor.timestamp;
            target.ControllerDeviceSensor.Which = source.csensor.which;
            #endregion ControllerDeviceSensor
            #region ControllerDeviceTouchpad
            target.ControllerDeviceTouchpad.Type = (EventType)source.ctouchpad.type;
            target.ControllerDeviceTouchpad.Timestamp = source.ctouchpad.timestamp;
            target.ControllerDeviceTouchpad.Which = source.ctouchpad.which;
            #endregion ControllerDeviceTouchpad
            #region ControllerDeviceDevice
            target.ControllerDevice.Type = (EventType)source.cdevice.type;
            target.ControllerDevice.Timestamp = source.cdevice.timestamp;
            target.ControllerDevice.Which = source.cdevice.which;
            #endregion ControllerDeviceDevice
            #region ControllerButton
            target.ControllerButton.Type = (EventType)source.cbutton.type;
            target.ControllerButton.Timestamp = source.cbutton.timestamp;
            target.ControllerButton.Which = source.cbutton.which;
            target.ControllerButton.Button = source.cbutton.button;
            target.ControllerButton.State = source.cbutton.state;
            #endregion ControllerButton
            #region ControllerAxis
            target.ControllerAxis.Type = (EventType)source.caxis.type;
            target.ControllerAxis.Timestamp = source.caxis.timestamp;
            target.ControllerAxis.Which = source.caxis.which;
            target.ControllerAxis.Axis = source.caxis.axis;
            target.ControllerAxis.AxisValue = source.caxis.axisValue;
            #endregion ControllerAxis
            #region JoyDevice
            target.JoyDevice.Type = (EventType)source.jdevice.type;
            target.JoyDevice.Timestamp = source.jdevice.timestamp;
            target.JoyDevice.Which = source.jdevice.which;
            #endregion JoyDevice
            #region JoyButton
            target.JoyButton.Type = (EventType)source.jbutton.type;
            target.JoyButton.Timestamp = source.jbutton.timestamp;
            target.JoyButton.Which = source.jbutton.which;
            target.JoyButton.Button = source.jbutton.button;
            target.JoyButton.State = source.jbutton.state;
            #endregion JoyButton
            #region JoyHat
            target.JoyHat.Type = (EventType)source.jhat.type;
            target.JoyHat.Timestamp = source.jhat.timestamp;
            target.JoyHat.Which = source.jhat.which;
            target.JoyHat.Hat = source.jhat.hat;
            target.JoyHat.HatValue = source.jhat.hatValue;
            #endregion JoyHat
            #region JoyBall
            target.JoyBall.Type = (EventType)source.jball.type;
            target.JoyBall.Timestamp = source.jball.timestamp;
            target.JoyBall.Which = source.jball.which;
            target.JoyBall.Ball = source.jball.ball;
            target.JoyBall.xRel = source.jball.xrel;
            target.JoyBall.yRel = source.jball.yrel;
            #endregion JoyBall
            #region JoyAxis
            target.JoyAxis.Type = (EventType)source.jaxis.type;
            target.JoyAxis.Timestamp = source.jaxis.timestamp;
            target.JoyAxis.Which = source.jaxis.which;
            target.JoyAxis.Axis = source.jaxis.axis;
            target.JoyAxis.AxisValue = source.jaxis.axisValue;
            target.JoyAxis.Padding4 = source.jaxis.padding4;
            #endregion JoyAxis
            #region MouseWheel
            target.MouseWheel.Type = (EventType)source.wheel.type;
            target.MouseWheel.Timestamp = source.wheel.timestamp;
            target.MouseWheel.WindowID = source.wheel.windowID;
            target.MouseWheel.Which = source.wheel.which;
            target.MouseWheel.X = source.wheel.x;
            target.MouseWheel.Y = source.wheel.y;
            target.MouseWheel.Direction = source.wheel.direction;
            #endregion MouseWheel
            #region MouseButton
            target.MouseButton.Type = (EventType)source.button.type;
            target.MouseButton.Timestamp = source.button.timestamp;
            target.MouseButton.WindowID = source.button.windowID;
            target.MouseButton.Which = source.button.which;
            target.MouseButton.Button = source.button.button;
            target.MouseButton.State = source.button.state;
            target.MouseButton.Clicks = source.button.clicks;
            target.MouseButton.X = source.button.x;
            target.MouseButton.Y = source.button.y;
            #endregion MouseButton
            #region MouseMotion
            target.MouseMotion.Type = (EventType)source.motion.type;
            target.MouseMotion.Timestamp = source.motion.timestamp;
            target.MouseMotion.WindowID = source.motion.windowID;
            target.MouseMotion.Which = source.motion.which;
            target.MouseMotion.State = source.motion.state;
            target.MouseMotion.X = source.motion.x;
            target.MouseMotion.Y = source.motion.y;
            target.MouseMotion.xRel = source.motion.xrel;
            target.MouseMotion.yRel = source.motion.yrel;
            #endregion MouseMotion
            #region TextInput
            target.TextInput.Type = (EventType)source.text.type;
            target.TextInput.Timestamp = source.text.timestamp;
            target.TextInput.WindowID = source.text.windowID;
            //target.TextInput.Text = source.text.text;
            #endregion TextInput
            #region TextEdit
            target.TextEdit.Type = (EventType)source.edit.type;
            target.TextEdit.Timestamp = source.edit.timestamp;
            target.TextEdit.WindowID = source.edit.windowID;
            //target.TextEdit.Text = source.edit.text;
            target.TextEdit.Start = source.edit.start;
            target.TextEdit.Length = source.edit.length;
            #endregion TextEdit
            #region Keyboard
            target.Keyboard.Type = (EventType)source.key.type;
            target.Keyboard.Timestamp = source.key.timestamp;
            target.Keyboard.WindowID = source.key.windowID;
            target.Keyboard.State = source.key.state;
            target.Keyboard.Repeat = source.key.repeat;
            target.Keyboard.Keysym.Scancode = (Scancode)source.key.keysym.scancode;
            target.Keyboard.Keysym.Sym = (Keycode)source.key.keysym.sym;
            target.Keyboard.Keysym.Mod = (Keymod)source.key.keysym.mod;
            target.Keyboard.Keysym.Unicode = source.key.keysym.unicode;
            #endregion Keyboard
            #region Window
            target.Window.Type = (EventType)source.window.type;
            target.Window.Timestamp = source.window.timestamp;
            target.Window.WindowID = source.window.windowID;
            target.Window.WindowEventID = (WindowEventID)source.window.windowEvent;
            target.Window.Data1 = source.window.data1;
            target.Window.Data2 = source.window.data2;
            #endregion Window
            #region Display
            target.Display.Type = (EventType)source.display.type;
            target.Display.Timestamp = source.display.timestamp;
            target.Display.Display = source.display.display;
            target.Display.DisplayEventID = (DisplayEventID)source.display.displayEvent;
            target.Display.Data1 = source.display.data1;
            #endregion Display
            #region TypeFSharp
            target.TypeFSharp = (EventType)source.typeFSharp;
            #endregion TypeFSharp
            #region Drop
            target.Drop.Type = (EventType)source.drop.type;
            target.Drop.Timestamp = source.drop.timestamp;
            target.Drop.File = source.drop.file;
            target.Drop.WindowID = source.drop.windowID;
            #endregion Drop
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

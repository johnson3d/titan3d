using SDL2;
using SixLabors.ImageSharp.Advanced;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.Input
{
    public partial class UInputSystem
    {
        public static unsafe void MappedEvent(in SDL.SDL_Event source, ref Input.Event target)
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
            {
                target.TextInput.Type = (EventType)source.text.type;
                target.TextInput.Timestamp = source.text.timestamp;
                target.TextInput.WindowID = source.text.windowID;
                fixed (SDL2.SDL.SDL_TextInputEvent* ptr = &source.text)
                {
                    var text = System.Text.Encoding.UTF8.GetString(ptr->text, 32);
                    var endIdx = text.IndexOf('\0');
                    if (endIdx >= 0)
                        text = text.Substring(0, endIdx);
                    target.TextInput.Text = text;
                }
                //target.TextInput.Text = source.text.text;
            }
            #endregion TextInput
            #region TextEdit
            {
                target.TextEdit.Type = (EventType)source.edit.type;
                target.TextEdit.Timestamp = source.edit.timestamp;
                target.TextEdit.WindowID = source.edit.windowID;
                fixed (SDL2.SDL.SDL_TextInputEvent* ptr = &source.text)
                {
                    var text = System.Text.Encoding.UTF8.GetString(ptr->text, 32);
                    var endIdx = text.IndexOf('\0');
                    if (endIdx >= 0)
                        text = text.Substring(0, endIdx);
                    target.TextEdit.Text = text;
                }
                //target.TextEdit.Text = source.edit.text;
                target.TextEdit.Start = source.edit.start;
                target.TextEdit.Length = source.edit.length;

            }
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
        public Scancode GetScancodeFromKey(Keycode keyCode)
        {
            return (Scancode)SDL.SDL_GetScancodeFromKey((SDL.SDL_Keycode)keyCode);
        }
        public unsafe bool PullEvent(ref Input.Event evt)
        {
            SDL.SDL_Event sdlEvt;
            bool ret = (SDL.SDL_PollEvent(out sdlEvt) != 0);
            if (ret)
            {
                if (ImGuiAPI.GetCurrentContext() != (void*)0)
                {
                    EGui.UDockWindowSDL.ImGui_ImplSDL2_ProcessEvent(in sdlEvt);
                }
                MappedEvent(sdlEvt, ref evt);
            }
            return ret;
        }
        public static void PostQuitMessage()
        {
            var closeEvent = new SDL2.SDL.SDL_Event();
            closeEvent.type = SDL2.SDL.SDL_EventType.SDL_QUIT;
            SDL2.SDL.SDL_PushEvent(ref closeEvent);
        }
        public static void StartTextInput()
        {
            SDL2.SDL.SDL_StartTextInput();
        }
        public static void StopTextInput()
        {
            SDL2.SDL.SDL_StopTextInput();
        }
        public static void SetTextInputRect(in EngineNS.Rectangle rect)
        {
            SDL2.SDL.SDL_Rect sdlRect = new SDL.SDL_Rect()
            {
                x = rect.X,
                y = rect.Y,
                w = rect.Width,
                h = rect.Height,
            };
            SDL2.SDL.SDL_SetTextInputRect(ref sdlRect);
        }
        public static void SetTextInputRect(in EngineNS.RectangleF rect)
        {
            SDL2.SDL.SDL_Rect sdlRect = new SDL.SDL_Rect()
            {
                x = (int)rect.X,
                y = (int)rect.Y,
                w = (int)rect.Width,
                h = (int)rect.Height,
            };
            SDL2.SDL.SDL_SetTextInputRect(ref sdlRect);
        }

        #region IME

        public class IMEHandler
        {
            [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr ImmGetContext(IntPtr hWnd);

            [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
            private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

            [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
            private static extern bool ImmGetOpenStatus(IntPtr hIMC);

            [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
            private static extern bool ImmSetOpenStatus(IntPtr hIMC, bool b);

            [DllImport("Imm32.dll")]
            private static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

            [StructLayout(LayoutKind.Sequential)]
            struct COMPOSITIONFORM
            {
                public uint dwStyle;
                public POINT ptCurrentPos;
                public RECT rcArea;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            // 获取当前的IME状态
            public static bool GetIMEStatus(IntPtr hwnd)
            {
                IntPtr hIMC = ImmGetContext(hwnd);
                bool isOpen = ImmGetOpenStatus(hIMC);
                ImmReleaseContext(hwnd, hIMC);
                return isOpen;
            }

            // 设置IME状态
            public static unsafe void SetIMEStatus(bool open, in Vector2 pos)
            {
                var hwnd = new IntPtr(EngineNS.TtEngine.Instance.GfxDevice.SlateApplication.NativeWindow.HWindow.ToPointer());
                IntPtr hIMC = ImmGetContext(hwnd);
                if(hIMC != IntPtr.Zero)
                {
                    var result = ImmSetOpenStatus(hIMC, open);
                    var compos = new COMPOSITIONFORM()
                    {
                        dwStyle = 2,
                        ptCurrentPos = new POINT()
                        {
                            x = (int)pos.X,
                            y = (int)pos.Y,
                        },
                        rcArea = new RECT()
                        {
                            left = 0,
                            top = 0,
                            right = 1920,
                            bottom = 1080,
                        },
                    };
                    ImmSetCompositionWindow(hIMC, ref compos);
                    ImmReleaseContext(hwnd, hIMC);
                }
            }
        }

        #endregion
    }
}

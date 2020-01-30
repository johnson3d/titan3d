using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Input.Device
{
    public class Keyboard : InputDevice
    {
        #region 数据结构定义
        public enum Keys
        {
            Modifiers = -65536,
            None = 0,
            LButton = 1,
            RButton = 2,
            Cancel = 3,
            MButton = 4,
            XButton1 = 5,
            XButton2 = 6,
            Back = 8,
            Tab = 9,
            LineFeed = 10,
            Clear = 12,
            Return = 13,
            Enter = 13,
            ShiftKey = 16,
            ControlKey = 17,
            Menu = 18,
            Pause = 19,
            Capital = 20,
            CapsLock = 20,
            KanaMode = 21,
            HanguelMode = 21,
            HangulMode = 21,
            JunjaMode = 23,
            FinalMode = 24,
            HanjaMode = 25,
            KanjiMode = 25,
            Escape = 27,
            IMEConvert = 28,
            IMENonconvert = 29,
            IMEAccept = 30,
            IMEAceept = 30,
            IMEModeChange = 31,
            Space = 32,
            Prior = 33,
            PageUp = 33,
            Next = 34,
            PageDown = 34,
            End = 35,
            Home = 36,
            Left = 37,
            Up = 38,
            Right = 39,
            Down = 40,
            Select = 41,
            Print = 42,
            Execute = 43,
            Snapshot = 44,
            PrintScreen = 44,
            Insert = 45,
            Delete = 46,
            Help = 47,
            D0 = 48,
            D1 = 49,
            D2 = 50,
            D3 = 51,
            D4 = 52,
            D5 = 53,
            D6 = 54,
            D7 = 55,
            D8 = 56,
            D9 = 57,
            A = 65,
            B = 66,
            C = 67,
            D = 68,
            E = 69,
            F = 70,
            G = 71,
            H = 72,
            I = 73,
            J = 74,
            K = 75,
            L = 76,
            M = 77,
            N = 78,
            O = 79,
            P = 80,
            Q = 81,
            R = 82,
            S = 83,
            T = 84,
            U = 85,
            V = 86,
            W = 87,
            X = 88,
            Y = 89,
            Z = 90,
            LWin = 91,
            RWin = 92,
            Apps = 93,
            Sleep = 95,
            NumPad0 = 96,
            NumPad1 = 97,
            NumPad2 = 98,
            NumPad3 = 99,
            NumPad4 = 100,
            NumPad5 = 101,
            NumPad6 = 102,
            NumPad7 = 103,
            NumPad8 = 104,
            NumPad9 = 105,
            Multiply = 106,
            Add = 107,
            Separator = 108,
            Subtract = 109,
            Decimal = 110,
            Divide = 111,
            F1 = 112,
            F2 = 113,
            F3 = 114,
            F4 = 115,
            F5 = 116,
            F6 = 117,
            F7 = 118,
            F8 = 119,
            F9 = 120,
            F10 = 121,
            F11 = 122,
            F12 = 123,
            F13 = 124,
            F14 = 125,
            F15 = 126,
            F16 = 127,
            F17 = 128,
            F18 = 129,
            F19 = 130,
            F20 = 131,
            F21 = 132,
            F22 = 133,
            F23 = 134,
            F24 = 135,
            NumLock = 144,
            Scroll = 145,
            LShiftKey = 160,
            RShiftKey = 161,
            LControlKey = 162,
            RControlKey = 163,
            LMenu = 164,
            RMenu = 165,
            BrowserBack = 166,
            BrowserForward = 167,
            BrowserRefresh = 168,
            BrowserStop = 169,
            BrowserSearch = 170,
            BrowserFavorites = 171,
            BrowserHome = 172,
            VolumeMute = 173,
            VolumeDown = 174,
            VolumeUp = 175,
            MediaNextTrack = 176,
            MediaPreviousTrack = 177,
            MediaStop = 178,
            MediaPlayPause = 179,
            LaunchMail = 180,
            SelectMedia = 181,
            LaunchApplication1 = 182,
            LaunchApplication2 = 183,
            OemSemicolon = 186,
            Oem1 = 186,
            Oemplus = 187,
            Oemcomma = 188,
            OemMinus = 189,
            OemPeriod = 190,
            OemQuestion = 191,
            Oem2 = 191,
            Oemtilde = 192,
            Oem3 = 192,
            OemOpenBrackets = 219,
            Oem4 = 219,
            OemPipe = 220,
            Oem5 = 220,
            OemCloseBrackets = 221,
            Oem6 = 221,
            OemQuotes = 222,
            Oem7 = 222,
            Oem8 = 223,
            OemBackslash = 226,
            Oem102 = 226,
            ProcessKey = 229,
            Packet = 231,
            Attn = 246,
            Crsel = 247,
            Exsel = 248,
            EraseEof = 249,
            Play = 250,
            Zoom = 251,
            NoName = 252,
            Pa1 = 253,
            OemClear = 254,
            KeyCode = 65535,
            Shift = 65536,
            Control = 131072,
            Alt = 262144
        }
        public enum KeyState
        {
            Press = 0,
            Release
        }
        public struct KeyboardEventArgs : IDeviceEventArgs
        {
            public KeyState KeyState;
            public bool Alt;
            public bool Control;
            public bool Shift;
            public Keys KeyCode;
            public int KeyValue;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PointX => 0;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PointY => 0;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float DeltaX => 0;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float DeltaY => 0;
        }
        public class KeyboardInputEventArgs : DeviceInputEventArgs
        {
            public KeyboardEventArgs KeyboardEvent;
        }
        #endregion
        #region Scan code & window message stuff
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;

        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        /// <summary>
        /// Alt key
        /// </summary>
        public const int VK_MENU = 0x12;

        /// <summary>
        /// List of scan codes for standard 104-key keyboard US English keyboard
        /// </summary>
        enum OemScanCode
        {
            /// <summary>
            /// ` ~
            /// </summary>
            BacktickTilde = 0x29,
            /// <summary>
            /// 1 !
            /// </summary>
            N1 = 2,
            /// <summary>
            /// 2 @
            /// </summary>
            N2 = 3,
            /// <summary>
            /// 3 #
            /// </summary>
            N3 = 4,
            /// <summary>
            /// 4 $
            /// </summary>
            N4 = 5,
            /// <summary>
            /// 5 %
            /// </summary>
            N5 = 6,
            /// <summary>
            /// 6 ^
            /// </summary>
            N6 = 7,
            /// <summary>
            /// 7 &
            /// </summary>
            N7 = 8,
            /// <summary>
            /// 8 *
            /// </summary>
            N8 = 9,
            /// <summary>
            /// 9 (
            /// </summary>
            N9 = 0x0A,
            /// <summary>
            /// 0 )
            /// </summary>
            N0 = 0x0B,
            /// <summary>
            /// - _
            /// </summary>
            MinusDash = 0x0C,
            /// <summary>
            /// = +
            /// </summary>
            Equals = 0x0D,
            Backspace = 0x0E,
            Tab = 0x0F,
            Q = 0x10,
            W = 0x11,
            E = 0x12,
            R = 0x13,
            T = 0x14,
            Y = 0x15,
            U = 0x16,
            I = 0x17,
            O = 0x18,
            P = 0x19,
            /// <summary>
            /// [ {
            /// </summary>
            LBracket = 0x1A,
            /// <summary>
            /// ] }
            /// </summary>
            RBracket = 0x1B,
            /// <summary>
            /// | \ (same as pipe)
            /// </summary>
            VerticalBar = 0x2B,
            /// <summary>
            /// | \ (same as vertical bar)
            /// </summary>
            Pipe = 0x2B,
            CapsLock = 0x3A,
            A = 0x1E,
            S = 0x1F,
            D = 0x20,
            F = 0x21,
            G = 0x22,
            H = 0x23,
            J = 0x24,
            K = 0x25,
            L = 0x26,
            /// <summary>
            /// ; :
            /// </summary>
            SemiColon = 0x27,
            /// <summary>
            /// ' "
            /// </summary>
            Quotes = 0x28,
            // Unused
            Enter = 0x1C,
            LShift = 0x2A,
            Z = 0x2C,
            X = 0x2D,
            C = 0x2E,
            V = 0x2F,
            B = 0x30,
            N = 0x31,
            M = 0x32,
            /// <summary>
            /// , <
            /// </summary>
            Comma = 0x33,
            /// <summary>
            /// . >
            /// </summary>
            Period = 0x34,
            /// <summary>
            /// / ?
            /// </summary>
            Slash = 0x35,
            RShift = 0x36,
            LControl = 0x1D,
            LAlternate = 0x38,
            SpaceBar = 0x39,
            RAlternate = 0x138,
            RControl = 0x11D,
            /// <summary>
            /// The menu key thingy
            /// </summary>
            Application = 0x15D,
            Insert = 0x152,
            Delete = 0x153,
            Home = 0x147,
            End = 0x14F,
            PageUp = 0x149,
            PageDown = 0x151,
            UpArrow = 0x148,
            DownArrow = 0x150,
            LeftArrow = 0x14B,
            RightArrow = 0x14D,
            NumLock = 0x145,
            NumPad0 = 0x52,
            NumPad1 = 0x4F,
            NumPad2 = 0x50,
            NumPad3 = 0x51,
            NumPad4 = 0x4B,
            NumPad5 = 0x4C,
            NumPad6 = 0x4D,
            NumPad7 = 0x47,
            NumPad8 = 0x48,
            NumPad9 = 0x49,
            NumPadDecimal = 0x53,
            NumPadEnter = 0x11C,
            NumPadPlus = 0x4E,
            NumPadMinus = 0x4A,
            NumPadAsterisk = 0x37,
            NumPadSlash = 0x135,
            Escape = 1,
            PrintScreen = 0x137,
            ScrollLock = 0x46,
            PauseBreak = 0x45,
            LeftWindows = 0x15B,
            RightWindows = 0x15C,
            F1 = 0x3B,
            F2 = 0x3C,
            F3 = 0x3D,
            F4 = 0x3E,
            F5 = 0x3F,
            F6 = 0x40,
            F7 = 0x41,
            F8 = 0x42,
            F9 = 0x43,
            F10 = 0x44,
            F11 = 0x57,
            F12 = 0x58,
        }
        //
        // Summary:
        //     Implements a Windows message.
        public struct WindowsMessage
        {
            public IntPtr HWnd { get; set; }
            public int Msg { get; set; }
            public IntPtr WParam { get; set; }
            public IntPtr LParam { get; set; }
            public IntPtr Result { get; set; }
        }
        public static KeyboardEventArgs ProcessWindowsKeyMessage(ref WindowsMessage m)
        {
            var val = m.LParam.ToInt64();
            KeyboardEventArgs e = new KeyboardEventArgs();
            e.KeyCode = Keys.None;
            if ((m.Msg == WM_KEYDOWN || m.Msg == WM_KEYUP) && ((int)m.WParam == VK_CONTROL || (int)m.WParam == VK_SHIFT))
            {
                switch ((OemScanCode)(((Int64)m.LParam >> 16) & 0x1FF))
                {
                    case OemScanCode.LControl:
                        {
                            e.KeyCode = Keys.LControlKey;
                        }
                        break;
                    case OemScanCode.RControl:
                        e.KeyCode = Keys.RControlKey;
                        break;
                    case OemScanCode.LShift:
                        e.KeyCode = Keys.LShiftKey;
                        break;
                    case OemScanCode.RShift:
                        e.KeyCode = Keys.RShiftKey;
                        break;
                    default:
                        if ((int)m.WParam == VK_SHIFT)
                            e.KeyCode = Keys.ShiftKey;
                        else if ((int)m.WParam == VK_CONTROL)
                            e.KeyCode = Keys.ControlKey;
                        break;
                }
            }
            return e;
        }
        #endregion

        public event EventHandler<KeyboardEventArgs> OnKeyDown;
        public event EventHandler<KeyboardEventArgs> OnKeyUp;
        #region InputDevice
        public override void OnInputEvent(DeviceInputEventArgs e)
        {
            if (e.DeviceType != Type)
                return;
            var inputEvent = e as KeyboardInputEventArgs;
            if (inputEvent.KeyboardEvent.KeyState == KeyState.Press)
            {
                if (!mThePressKeys.Contains(inputEvent.KeyboardEvent.KeyCode))
                {
                    mThePressKeys.Add(inputEvent.KeyboardEvent.KeyCode);
                    mKeysStateDic[inputEvent.KeyboardEvent.KeyCode] = KeyState.Press;
                    TriggerActionMapping((int)inputEvent.KeyboardEvent.KeyCode, KeyState.Press);
                    OnKeyDown?.Invoke(this, inputEvent.KeyboardEvent);
                }
            }
            else
            {
                mKeysStateDic[inputEvent.KeyboardEvent.KeyCode] = KeyState.Release;
                mThePressKeys.Remove(inputEvent.KeyboardEvent.KeyCode);
                PulseEndAxisMapping((int)inputEvent.KeyboardEvent.KeyCode);
                TriggerActionMapping((int)inputEvent.KeyboardEvent.KeyCode, KeyState.Release);
                OnKeyUp?.Invoke(this, inputEvent.KeyboardEvent);
            }
        }
        //make sure the key state is right
        List<Keys> mThePressKeys = new List<Keys>();
        List<Keys> mTheReleaseKeys = new List<Keys>();
        public override void Tick()
        {
            for (int i = 0; i < mThePressKeys.Count; ++i)
            {
                //正常情况下用该用IsPlatformKeyDown 但是android的IsPlatformKeyDown 没有找到接口
                if (!IsKeyDown(mThePressKeys[i]))
                {
                    mTheReleaseKeys.Add(mThePressKeys[i]);
                }
                //else
                //{
                //    PulseAxisMapping((int)mThePressKeys[i]);
                //}
            }
            for (int i = 0; i < mTheReleaseKeys.Count; ++i)
            {
                mThePressKeys.Remove(mTheReleaseKeys[i]);
                mKeysStateDic[mTheReleaseKeys[i]] = KeyState.Release;
                var keyEvent = new KeyboardEventArgs();
                keyEvent.KeyCode = mTheReleaseKeys[i];
                keyEvent.KeyValue = (int)mTheReleaseKeys[i];
                keyEvent.KeyState = KeyState.Release;
                keyEvent.Alt = IsPlatformKeyDown(Keys.Menu);
                keyEvent.Shift = IsPlatformKeyDown(Keys.ShiftKey);
                keyEvent.Control = IsPlatformKeyDown(Keys.ControlKey);
                //PulseEndAxisMapping((int)mTheReleaseKeys[i]);
                TriggerActionMapping((int)mTheReleaseKeys[i], KeyState.Release);
                OnKeyUp?.Invoke(this, keyEvent);
            }
            mTheReleaseKeys.Clear();
            PulseAxisMapping();
        }
        public void TriggerActionMapping(int keyCode, KeyState state)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        //value值在 axis 中代表Scale,在TriggerAction中代表 Press 和 Release
                        if (mapping.KeyCode == keyCode && mapping.MappingType == InputMappingType.Action && state == (KeyState)mapping.Value)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(mapping.Value);
                            }
                        }
                    }
                }
            }
        }
        public void PulseAxisMapping()
        {
            if (mKeysStateDic.Count == 0)
                return;
            ResetFucInvoke();
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.MappingType == InputMappingType.Axis)
                        {
                            if (mKeysStateDic.ContainsKey((Keys)mapping.KeyCode))
                            {
                                if (mKeysStateDic[(Keys)mapping.KeyCode] == KeyState.Press)
                                {
                                    for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                                    {
                                        valueMapping.Funtions[k]?.Invoke(mapping.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        void ResetFucInvoke()
        {
            if (mKeysStateDic.Count == 0)
                return;
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.MappingType == InputMappingType.Axis)
                        {
                            if (mKeysStateDic.ContainsKey((Keys)mapping.KeyCode))
                            {
                                if (mKeysStateDic[(Keys)mapping.KeyCode] == KeyState.Release)
                                {
                                    for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                                    {
                                        valueMapping.Funtions[k]?.Invoke(0);
                                    }
                                }
                            }
                            //for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            //{
                            //    valueMapping.Funtions[k]?.Invoke(0);
                            //}
                        }
                    }
                }
            }
        }
        public void PulseAxisMapping(int keyCode)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.KeyCode == keyCode && mapping.MappingType == InputMappingType.Axis)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(mapping.Value);
                            }
                        }
                    }
                }
            }
        }
        void PulseEndAxisMapping(int keyCode)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.KeyCode == keyCode)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(0);
                            }
                        }
                    }
                }
            }
        }

        #endregion
        Dictionary<Keys, KeyState> mKeysStateDic = new Dictionary<Keys, KeyState>();
        public Keyboard()
        {
            mName = "Keyboard";
            mType = DeviceType.Keyboard;
            mID = Guid.NewGuid();
        }
        public override void InitDevice()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                mKeysStateDic.Add(key, KeyState.Release);
            }
        }
        public bool IsPlatformKeyDown(Keys key)
        {
            return EngineNS.CIPlatform.Instance.IsKeyDown(key);
        }
        public bool IsKeyDown(Keys key)
        {
#if PWindow
            return IsPlatformKeyDown(key);
#else
            if (mKeysStateDic.ContainsKey(key))
            {
                return mKeysStateDic[key] == KeyState.Press;
            }
            return false;
#endif
        }
        public KeyState GetKeyState(Keys key)
        {
            if (mKeysStateDic.ContainsKey(key))
            {
                return mKeysStateDic[key];
            }
            return KeyState.Release;
        }

    }
}

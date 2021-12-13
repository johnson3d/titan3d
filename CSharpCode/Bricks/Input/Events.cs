using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Bricks.Input
{
    #region Event Structs and Enums
    public enum EventType : uint
    {
        FIRSTEVENT = 0,
        QUIT = 256,
        APP_TERMINATING = 257,
        APP_LOWMEMORY = 258,
        APP_WILLENTERBACKGROUND = 259,
        APP_DIDENTERBACKGROUND = 260,
        APP_WILLENTERFOREGROUND = 261,
        APP_DIDENTERFOREGROUND = 262,
        LOCALECHANGED = 263,
        DISPLAYEVENT = 336,
        WINDOWEVENT = 512,
        SYSWMEVENT = 513,
        KEYDOWN = 768,
        KEYUP = 769,
        TEXTEDITING = 770,
        TEXTINPUT = 771,
        KEYMAPCHANGED = 772,
        MOUSEMOTION = 1024,
        MOUSEBUTTONDOWN = 1025,
        MOUSEBUTTONUP = 1026,
        MOUSEWHEEL = 1027,
        JOYAXISMOTION = 1536,
        JOYBALLMOTION = 1537,
        JOYHATMOTION = 1538,
        JOYBUTTONDOWN = 1539,
        JOYBUTTONUP = 1540,
        JOYDEVICEADDED = 1541,
        JOYDEVICEREMOVED = 1542,
        CONTROLLERAXISMOTION = 1616,
        CONTROLLERBUTTONDOWN = 1617,
        CONTROLLERBUTTONUP = 1618,
        CONTROLLERDEVICEADDED = 1619,
        CONTROLLERDEVICEREMOVED = 1620,
        CONTROLLERDEVICEREMAPPED = 1621,
        CONTROLLERTOUCHPADDOWN = 1622,
        CONTROLLERTOUCHPADMOTION = 1623,
        CONTROLLERTOUCHPADUP = 1624,
        CONTROLLERSENSORUPDATE = 1625,
        FINGERDOWN = 1792,
        FINGERUP = 1793,
        FINGERMOTION = 1794,
        DOLLARGESTURE = 2048,
        DOLLARRECORD = 2049,
        MULTIGESTURE = 2050,
        CLIPBOARDUPDATE = 2304,
        DROPFILE = 4096,
        DROPTEXT = 4097,
        DROPBEGIN = 4098,
        DROPCOMPLETE = 4099,
        AUDIODEVICEADDED = 4352,
        AUDIODEVICEREMOVED = 4353,
        SENSORUPDATE = 4608,
        RENDER_TARGETS_RESET = 8192,
        RENDER_DEVICE_RESET = 8193,
        USEREVENT = 32768,
        LASTEVENT = 65535
    }
    public struct DollarGestureEvent
    {
        public uint Type;
        public uint Timestamp;
        public long TouchId;
        public long GestureId;
        public uint NumFingers;
        public float Error;
        public float X;
        public float Y;
    }
    public struct MultiGestureEvent
    {
        public uint Type;
        public uint Timestamp;
        public long TouchId;
        public float dTheta;
        public float dDist;
        public float X;
        public float Y;
        public ushort NumFingers;
        public ushort Padding;
    }
    public struct TouchFingerEvent
    {
        public uint Type;
        public uint Timestamp;
        public long TouchId;
        public long FingerId;
        public float X;
        public float Y;
        public float dx;
        public float dy;
        public float Pressure;
        public uint WindowID;
    }
    public struct SysWMEvent
    {
        public EventType Type;
        public uint Timestamp;
        public IntPtr Msg;
    }
    public struct UserEvent
    {
        public uint Type;
        public uint Timestamp;
        public uint WindowID;
        public int Code;
        public IntPtr Data1;
        public IntPtr Data2;
    }
    public struct QuitEvent
    {
        public EventType Type;
        public uint Timestamp;
    }
    public struct SensorEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
        public unsafe fixed float Data[6];
    }
    public struct AudioDeviceEvent
    {
        public uint Type;
        public uint Timestamp;
        public uint Which;
        public byte IsCapture;
    }
    public struct ControllerDeviceEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
    }
    public struct ControllerButtonEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
        public byte Button;
        public byte State;
    }
    public struct ControllerAxisEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
        public byte Axis;
        public short AxisValue;
    }
    public struct JoyDeviceEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
    }
    public struct JoyAxisEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
        public byte Axis;
        public short AxisValue;
        public ushort Padding4;
    }
    public struct JoyBallEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
        public byte Ball;
        public short xRel;
        public short yRel;
    }
    public struct JoyButtonEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
        public byte Button;
        public byte State;
    }
    public struct JoyHatEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
        public byte Hat;
        public byte HatValue;
    }
    public struct MouseWheelEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public uint Which;
        public int X;
        public int Y;
        public uint Direction;
    }
    public struct MouseMotionEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public uint Which;
        public byte State;
        public int X;
        public int Y;
        public int xRel;
        public int yRel;
    }
    public struct MouseButtonEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public uint Which;
        public byte Button;
        public byte State;
        public byte Clicks;
        public int X;
        public int Y;
    }
    public struct TextInputEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public unsafe fixed byte Text[64];
    }
    public struct TextEditingEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public unsafe fixed byte Text[64];
        public int Start;
        public int Length;
    }
    public struct KeyboardEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public byte State;
        public byte Repeat;
        public Keysym Keysym;
    }
    public enum Scancode
    {
        SCANCODE_UNKNOWN = 0,
        SCANCODE_A = 4,
        SCANCODE_B = 5,
        SCANCODE_C = 6,
        SCANCODE_D = 7,
        SCANCODE_E = 8,
        SCANCODE_F = 9,
        SCANCODE_G = 10,
        SCANCODE_H = 11,
        SCANCODE_I = 12,
        SCANCODE_J = 13,
        SCANCODE_K = 14,
        SCANCODE_L = 15,
        SCANCODE_M = 16,
        SCANCODE_N = 17,
        SCANCODE_O = 18,
        SCANCODE_P = 19,
        SCANCODE_Q = 20,
        SCANCODE_R = 21,
        SCANCODE_S = 22,
        SCANCODE_T = 23,
        SCANCODE_U = 24,
        SCANCODE_V = 25,
        SCANCODE_W = 26,
        SCANCODE_X = 27,
        SCANCODE_Y = 28,
        SCANCODE_Z = 29,
        SCANCODE_1 = 30,
        SCANCODE_2 = 31,
        SCANCODE_3 = 32,
        SCANCODE_4 = 33,
        SCANCODE_5 = 34,
        SCANCODE_6 = 35,
        SCANCODE_7 = 36,
        SCANCODE_8 = 37,
        SCANCODE_9 = 38,
        SCANCODE_0 = 39,
        SCANCODE_RETURN = 40,
        SCANCODE_ESCAPE = 41,
        SCANCODE_BACKSPACE = 42,
        SCANCODE_TAB = 43,
        SCANCODE_SPACE = 44,
        SCANCODE_MINUS = 45,
        SCANCODE_EQUALS = 46,
        SCANCODE_LEFTBRACKET = 47,
        SCANCODE_RIGHTBRACKET = 48,
        SCANCODE_BACKSLASH = 49,
        SCANCODE_NONUSHASH = 50,
        SCANCODE_SEMICOLON = 51,
        SCANCODE_APOSTROPHE = 52,
        SCANCODE_GRAVE = 53,
        SCANCODE_COMMA = 54,
        SCANCODE_PERIOD = 55,
        SCANCODE_SLASH = 56,
        SCANCODE_CAPSLOCK = 57,
        SCANCODE_F1 = 58,
        SCANCODE_F2 = 59,
        SCANCODE_F3 = 60,
        SCANCODE_F4 = 61,
        SCANCODE_F5 = 62,
        SCANCODE_F6 = 63,
        SCANCODE_F7 = 64,
        SCANCODE_F8 = 65,
        SCANCODE_F9 = 66,
        SCANCODE_F10 = 67,
        SCANCODE_F11 = 68,
        SCANCODE_F12 = 69,
        SCANCODE_PRINTSCREEN = 70,
        SCANCODE_SCROLLLOCK = 71,
        SCANCODE_PAUSE = 72,
        SCANCODE_INSERT = 73,
        SCANCODE_HOME = 74,
        SCANCODE_PAGEUP = 75,
        SCANCODE_DELETE = 76,
        SCANCODE_END = 77,
        SCANCODE_PAGEDOWN = 78,
        SCANCODE_RIGHT = 79,
        SCANCODE_LEFT = 80,
        SCANCODE_DOWN = 81,
        SCANCODE_UP = 82,
        SCANCODE_NUMLOCKCLEAR = 83,
        SCANCODE_KP_DIVIDE = 84,
        SCANCODE_KP_MULTIPLY = 85,
        SCANCODE_KP_MINUS = 86,
        SCANCODE_KP_PLUS = 87,
        SCANCODE_KP_ENTER = 88,
        SCANCODE_KP_1 = 89,
        SCANCODE_KP_2 = 90,
        SCANCODE_KP_3 = 91,
        SCANCODE_KP_4 = 92,
        SCANCODE_KP_5 = 93,
        SCANCODE_KP_6 = 94,
        SCANCODE_KP_7 = 95,
        SCANCODE_KP_8 = 96,
        SCANCODE_KP_9 = 97,
        SCANCODE_KP_0 = 98,
        SCANCODE_KP_PERIOD = 99,
        SCANCODE_NONUSBACKSLASH = 100,
        SCANCODE_APPLICATION = 101,
        SCANCODE_POWER = 102,
        SCANCODE_KP_EQUALS = 103,
        SCANCODE_F13 = 104,
        SCANCODE_F14 = 105,
        SCANCODE_F15 = 106,
        SCANCODE_F16 = 107,
        SCANCODE_F17 = 108,
        SCANCODE_F18 = 109,
        SCANCODE_F19 = 110,
        SCANCODE_F20 = 111,
        SCANCODE_F21 = 112,
        SCANCODE_F22 = 113,
        SCANCODE_F23 = 114,
        SCANCODE_F24 = 115,
        SCANCODE_EXECUTE = 116,
        SCANCODE_HELP = 117,
        SCANCODE_MENU = 118,
        SCANCODE_SELECT = 119,
        SCANCODE_STOP = 120,
        SCANCODE_AGAIN = 121,
        SCANCODE_UNDO = 122,
        SCANCODE_CUT = 123,
        SCANCODE_COPY = 124,
        SCANCODE_PASTE = 125,
        SCANCODE_FIND = 126,
        SCANCODE_MUTE = 127,
        SCANCODE_VOLUMEUP = 128,
        SCANCODE_VOLUMEDOWN = 129,
        SCANCODE_KP_COMMA = 133,
        SCANCODE_KP_EQUALSAS400 = 134,
        SCANCODE_INTERNATIONAL1 = 135,
        SCANCODE_INTERNATIONAL2 = 136,
        SCANCODE_INTERNATIONAL3 = 137,
        SCANCODE_INTERNATIONAL4 = 138,
        SCANCODE_INTERNATIONAL5 = 139,
        SCANCODE_INTERNATIONAL6 = 140,
        SCANCODE_INTERNATIONAL7 = 141,
        SCANCODE_INTERNATIONAL8 = 142,
        SCANCODE_INTERNATIONAL9 = 143,
        SCANCODE_LANG1 = 144,
        SCANCODE_LANG2 = 145,
        SCANCODE_LANG3 = 146,
        SCANCODE_LANG4 = 147,
        SCANCODE_LANG5 = 148,
        SCANCODE_LANG6 = 149,
        SCANCODE_LANG7 = 150,
        SCANCODE_LANG8 = 151,
        SCANCODE_LANG9 = 152,
        SCANCODE_ALTERASE = 153,
        SCANCODE_SYSREQ = 154,
        SCANCODE_CANCEL = 155,
        SCANCODE_CLEAR = 156,
        SCANCODE_PRIOR = 157,
        SCANCODE_RETURN2 = 158,
        SCANCODE_SEPARATOR = 159,
        SCANCODE_OUT = 160,
        SCANCODE_OPER = 161,
        SCANCODE_CLEARAGAIN = 162,
        SCANCODE_CRSEL = 163,
        SCANCODE_EXSEL = 164,
        SCANCODE_KP_00 = 176,
        SCANCODE_KP_000 = 177,
        SCANCODE_THOUSANDSSEPARATOR = 178,
        SCANCODE_DECIMALSEPARATOR = 179,
        SCANCODE_CURRENCYUNIT = 180,
        SCANCODE_CURRENCYSUBUNIT = 181,
        SCANCODE_KP_LEFTPAREN = 182,
        SCANCODE_KP_RIGHTPAREN = 183,
        SCANCODE_KP_LEFTBRACE = 184,
        SCANCODE_KP_RIGHTBRACE = 185,
        SCANCODE_KP_TAB = 186,
        SCANCODE_KP_BACKSPACE = 187,
        SCANCODE_KP_A = 188,
        SCANCODE_KP_B = 189,
        SCANCODE_KP_C = 190,
        SCANCODE_KP_D = 191,
        SCANCODE_KP_E = 192,
        SCANCODE_KP_F = 193,
        SCANCODE_KP_XOR = 194,
        SCANCODE_KP_POWER = 195,
        SCANCODE_KP_PERCENT = 196,
        SCANCODE_KP_LESS = 197,
        SCANCODE_KP_GREATER = 198,
        SCANCODE_KP_AMPERSAND = 199,
        SCANCODE_KP_DBLAMPERSAND = 200,
        SCANCODE_KP_VERTICALBAR = 201,
        SCANCODE_KP_DBLVERTICALBAR = 202,
        SCANCODE_KP_COLON = 203,
        SCANCODE_KP_HASH = 204,
        SCANCODE_KP_SPACE = 205,
        SCANCODE_KP_AT = 206,
        SCANCODE_KP_EXCLAM = 207,
        SCANCODE_KP_MEMSTORE = 208,
        SCANCODE_KP_MEMRECALL = 209,
        SCANCODE_KP_MEMCLEAR = 210,
        SCANCODE_KP_MEMADD = 211,
        SCANCODE_KP_MEMSUBTRACT = 212,
        SCANCODE_KP_MEMMULTIPLY = 213,
        SCANCODE_KP_MEMDIVIDE = 214,
        SCANCODE_KP_PLUSMINUS = 215,
        SCANCODE_KP_CLEAR = 216,
        SCANCODE_KP_CLEARENTRY = 217,
        SCANCODE_KP_BINARY = 218,
        SCANCODE_KP_OCTAL = 219,
        SCANCODE_KP_DECIMAL = 220,
        SCANCODE_KP_HEXADECIMAL = 221,
        SCANCODE_LCTRL = 224,
        SCANCODE_LSHIFT = 225,
        SCANCODE_LALT = 226,
        SCANCODE_LGUI = 227,
        SCANCODE_RCTRL = 228,
        SCANCODE_RSHIFT = 229,
        SCANCODE_RALT = 230,
        SCANCODE_RGUI = 231,
        SCANCODE_MODE = 257,
        SCANCODE_AUDIONEXT = 258,
        SCANCODE_AUDIOPREV = 259,
        SCANCODE_AUDIOSTOP = 260,
        SCANCODE_AUDIOPLAY = 261,
        SCANCODE_AUDIOMUTE = 262,
        SCANCODE_MEDIASELECT = 263,
        SCANCODE_WWW = 264,
        SCANCODE_MAIL = 265,
        SCANCODE_CALCULATOR = 266,
        SCANCODE_COMPUTER = 267,
        SCANCODE_AC_SEARCH = 268,
        SCANCODE_AC_HOME = 269,
        SCANCODE_AC_BACK = 270,
        SCANCODE_AC_FORWARD = 271,
        SCANCODE_AC_STOP = 272,
        SCANCODE_AC_REFRESH = 273,
        SCANCODE_AC_BOOKMARKS = 274,
        SCANCODE_BRIGHTNESSDOWN = 275,
        SCANCODE_BRIGHTNESSUP = 276,
        SCANCODE_DISPLAYSWITCH = 277,
        SCANCODE_KBDILLUMTOGGLE = 278,
        SCANCODE_KBDILLUMDOWN = 279,
        SCANCODE_KBDILLUMUP = 280,
        SCANCODE_EJECT = 281,
        SCANCODE_SLEEP = 282,
        SCANCODE_APP1 = 283,
        SCANCODE_APP2 = 284,
        SCANCODE_AUDIOREWIND = 285,
        SCANCODE_AUDIOFASTFORWARD = 286,
        NUM_SCANCODES = 512
    }
    public enum Keycode
    {
        KEY_UNKNOWN = 0,
        KEY_BACKSPACE = 8,
        KEY_TAB = 9,
        KEY_RETURN = 13,
        KEY_ESCAPE = 27,
        KEY_SPACE = 32,
        KEY_EXCLAIM = 33,
        KEY_QUOTEDBL = 34,
        KEY_HASH = 35,
        KEY_DOLLAR = 36,
        KEY_PERCENT = 37,
        KEY_AMPERSAND = 38,
        KEY_QUOTE = 39,
        KEY_LEFTPAREN = 40,
        KEY_RIGHTPAREN = 41,
        KEY_ASTERISK = 42,
        KEY_PLUS = 43,
        KEY_COMMA = 44,
        KEY_MINUS = 45,
        KEY_PERIOD = 46,
        KEY_SLASH = 47,
        KEY_0 = 48,
        KEY_1 = 49,
        KEY_2 = 50,
        KEY_3 = 51,
        KEY_4 = 52,
        KEY_5 = 53,
        KEY_6 = 54,
        KEY_7 = 55,
        KEY_8 = 56,
        KEY_9 = 57,
        KEY_COLON = 58,
        KEY_SEMICOLON = 59,
        KEY_LESS = 60,
        KEY_EQUALS = 61,
        KEY_GREATER = 62,
        KEY_QUESTION = 63,
        KEY_AT = 64,
        KEY_LEFTBRACKET = 91,
        KEY_BACKSLASH = 92,
        KEY_RIGHTBRACKET = 93,
        KEY_CARET = 94,
        KEY_UNDERSCORE = 95,
        KEY_BACKQUOTE = 96,
        KEY_a = 97,
        KEY_b = 98,
        KEY_c = 99,
        KEY_d = 100,
        KEY_e = 101,
        KEY_f = 102,
        KEY_g = 103,
        KEY_h = 104,
        KEY_i = 105,
        KEY_j = 106,
        KEY_k = 107,
        KEY_l = 108,
        KEY_m = 109,
        KEY_n = 110,
        KEY_o = 111,
        KEY_p = 112,
        KEY_q = 113,
        KEY_r = 114,
        KEY_s = 115,
        KEY_t = 116,
        KEY_u = 117,
        KEY_v = 118,
        KEY_w = 119,
        KEY_x = 120,
        KEY_y = 121,
        KEY_z = 122,
        KEY_DELETE = 127,
        KEY_CAPSLOCK = 1073741881,
        KEY_F1 = 1073741882,
        KEY_F2 = 1073741883,
        KEY_F3 = 1073741884,
        KEY_F4 = 1073741885,
        KEY_F5 = 1073741886,
        KEY_F6 = 1073741887,
        KEY_F7 = 1073741888,
        KEY_F8 = 1073741889,
        KEY_F9 = 1073741890,
        KEY_F10 = 1073741891,
        KEY_F11 = 1073741892,
        KEY_F12 = 1073741893,
        KEY_PRINTSCREEN = 1073741894,
        KEY_SCROLLLOCK = 1073741895,
        KEY_PAUSE = 1073741896,
        KEY_INSERT = 1073741897,
        KEY_HOME = 1073741898,
        KEY_PAGEUP = 1073741899,
        KEY_END = 1073741901,
        KEY_PAGEDOWN = 1073741902,
        KEY_RIGHT = 1073741903,
        KEY_LEFT = 1073741904,
        KEY_DOWN = 1073741905,
        KEY_UP = 1073741906,
        KEY_NUMLOCKCLEAR = 1073741907,
        KEY_KP_DIVIDE = 1073741908,
        KEY_KP_MULTIPLY = 1073741909,
        KEY_KP_MINUS = 1073741910,
        KEY_KP_PLUS = 1073741911,
        KEY_KP_ENTER = 1073741912,
        KEY_KP_1 = 1073741913,
        KEY_KP_2 = 1073741914,
        KEY_KP_3 = 1073741915,
        KEY_KP_4 = 1073741916,
        KEY_KP_5 = 1073741917,
        KEY_KP_6 = 1073741918,
        KEY_KP_7 = 1073741919,
        KEY_KP_8 = 1073741920,
        KEY_KP_9 = 1073741921,
        KEY_KP_0 = 1073741922,
        KEY_KP_PERIOD = 1073741923,
        KEY_APPLICATION = 1073741925,
        KEY_POWER = 1073741926,
        KEY_KP_EQUALS = 1073741927,
        KEY_F13 = 1073741928,
        KEY_F14 = 1073741929,
        KEY_F15 = 1073741930,
        KEY_F16 = 1073741931,
        KEY_F17 = 1073741932,
        KEY_F18 = 1073741933,
        KEY_F19 = 1073741934,
        KEY_F20 = 1073741935,
        KEY_F21 = 1073741936,
        KEY_F22 = 1073741937,
        KEY_F23 = 1073741938,
        KEY_F24 = 1073741939,
        KEY_EXECUTE = 1073741940,
        KEY_HELP = 1073741941,
        KEY_MENU = 1073741942,
        KEY_SELECT = 1073741943,
        KEY_STOP = 1073741944,
        KEY_AGAIN = 1073741945,
        KEY_UNDO = 1073741946,
        KEY_CUT = 1073741947,
        KEY_COPY = 1073741948,
        KEY_PASTE = 1073741949,
        KEY_FIND = 1073741950,
        KEY_MUTE = 1073741951,
        KEY_VOLUMEUP = 1073741952,
        KEY_VOLUMEDOWN = 1073741953,
        KEY_KP_COMMA = 1073741957,
        KEY_KP_EQUALSAS400 = 1073741958,
        KEY_ALTERASE = 1073741977,
        KEY_SYSREQ = 1073741978,
        KEY_CANCEL = 1073741979,
        KEY_CLEAR = 1073741980,
        KEY_PRIOR = 1073741981,
        KEY_RETURN2 = 1073741982,
        KEY_SEPARATOR = 1073741983,
        KEY_OUT = 1073741984,
        KEY_OPER = 1073741985,
        KEY_CLEARAGAIN = 1073741986,
        KEY_CRSEL = 1073741987,
        KEY_EXSEL = 1073741988,
        KEY_KP_00 = 1073742000,
        KEY_KP_000 = 1073742001,
        KEY_THOUSANDSSEPARATOR = 1073742002,
        KEY_DECIMALSEPARATOR = 1073742003,
        KEY_CURRENCYUNIT = 1073742004,
        KEY_CURRENCYSUBUNIT = 1073742005,
        KEY_KP_LEFTPAREN = 1073742006,
        KEY_KP_RIGHTPAREN = 1073742007,
        KEY_KP_LEFTBRACE = 1073742008,
        KEY_KP_RIGHTBRACE = 1073742009,
        KEY_KP_TAB = 1073742010,
        KEY_KP_BACKSPACE = 1073742011,
        KEY_KP_A = 1073742012,
        KEY_KP_B = 1073742013,
        KEY_KP_C = 1073742014,
        KEY_KP_D = 1073742015,
        KEY_KP_E = 1073742016,
        KEY_KP_F = 1073742017,
        KEY_KP_XOR = 1073742018,
        KEY_KP_POWER = 1073742019,
        KEY_KP_PERCENT = 1073742020,
        KEY_KP_LESS = 1073742021,
        KEY_KP_GREATER = 1073742022,
        KEY_KP_AMPERSAND = 1073742023,
        KEY_KP_DBLAMPERSAND = 1073742024,
        KEY_KP_VERTICALBAR = 1073742025,
        KEY_KP_DBLVERTICALBAR = 1073742026,
        KEY_KP_COLON = 1073742027,
        KEY_KP_HASH = 1073742028,
        KEY_KP_SPACE = 1073742029,
        KEY_KP_AT = 1073742030,
        KEY_KP_EXCLAM = 1073742031,
        KEY_KP_MEMSTORE = 1073742032,
        KEY_KP_MEMRECALL = 1073742033,
        KEY_KP_MEMCLEAR = 1073742034,
        KEY_KP_MEMADD = 1073742035,
        KEY_KP_MEMSUBTRACT = 1073742036,
        KEY_KP_MEMMULTIPLY = 1073742037,
        KEY_KP_MEMDIVIDE = 1073742038,
        KEY_KP_PLUSMINUS = 1073742039,
        KEY_KP_CLEAR = 1073742040,
        KEY_KP_CLEARENTRY = 1073742041,
        KEY_KP_BINARY = 1073742042,
        KEY_KP_OCTAL = 1073742043,
        KEY_KP_DECIMAL = 1073742044,
        KEY_KP_HEXADECIMAL = 1073742045,
        KEY_LCTRL = 1073742048,
        KEY_LSHIFT = 1073742049,
        KEY_LALT = 1073742050,
        KEY_LGUI = 1073742051,
        KEY_RCTRL = 1073742052,
        KEY_RSHIFT = 1073742053,
        KEY_RALT = 1073742054,
        KEY_RGUI = 1073742055,
        KEY_MODE = 1073742081,
        KEY_AUDIONEXT = 1073742082,
        KEY_AUDIOPREV = 1073742083,
        KEY_AUDIOSTOP = 1073742084,
        KEY_AUDIOPLAY = 1073742085,
        KEY_AUDIOMUTE = 1073742086,
        KEY_MEDIASELECT = 1073742087,
        KEY_WWW = 1073742088,
        KEY_MAIL = 1073742089,
        KEY_CALCULATOR = 1073742090,
        KEY_COMPUTER = 1073742091,
        KEY_AC_SEARCH = 1073742092,
        KEY_AC_HOME = 1073742093,
        KEY_AC_BACK = 1073742094,
        KEY_AC_FORWARD = 1073742095,
        KEY_AC_STOP = 1073742096,
        KEY_AC_REFRESH = 1073742097,
        KEY_AC_BOOKMARKS = 1073742098,
        KEY_BRIGHTNESSDOWN = 1073742099,
        KEY_BRIGHTNESSUP = 1073742100,
        KEY_DISPLAYSWITCH = 1073742101,
        KEY_KBDILLUMTOGGLE = 1073742102,
        KEY_KBDILLUMDOWN = 1073742103,
        KEY_KBDILLUMUP = 1073742104,
        KEY_EJECT = 1073742105,
        KEY_SLEEP = 1073742106,
        KEY_APP1 = 1073742107,
        KEY_APP2 = 1073742108,
        KEY_AUDIOREWIND = 1073742109,
        KEY_AUDIOFASTFORWARD = 1073742110
    }
    public enum Keymod : ushort
    {
        KMOD_NONE = 0,
        KMOD_LSHIFT = 1,
        KMOD_RSHIFT = 2,
        KMOD_SHIFT = 3,
        KMOD_LCTRL = 64,
        KMOD_RCTRL = 128,
        KMOD_CTRL = 192,
        KMOD_LALT = 256,
        KMOD_RALT = 512,
        KMOD_ALT = 768,
        KMOD_LGUI = 1024,
        KMOD_RGUI = 2048,
        KMOD_GUI = 3072,
        KMOD_NUM = 4096,
        KMOD_CAPS = 8192,
        KMOD_MODE = 16384,
        KMOD_RESERVED = 32768
    }
    public enum MouseButton
    {
        BUTTON_UNKNOWN = 0,
        BUTTON_LEFT = 1,
        BUTTON_MIDDLE = 2,
        BUTTON_RIGHT = 3,
        BUTTON_X1 = 4,
        BUTTON_X2 = 5,
    }
    public struct Keysym
    {
        public Scancode Scancode;
        public Keycode Sym;
        public Keymod Mod;
        public uint Unicode;
    }
    public enum WindowEventID : byte
    {
        WINDOWEVENT_NONE = 0,
        WINDOWEVENT_SHOWN = 1,
        WINDOWEVENT_HIDDEN = 2,
        WINDOWEVENT_EXPOSED = 3,
        WINDOWEVENT_MOVED = 4,
        WINDOWEVENT_RESIZED = 5,
        WINDOWEVENT_SIZE_CHANGED = 6,
        WINDOWEVENT_MINIMIZED = 7,
        WINDOWEVENT_MAXIMIZED = 8,
        WINDOWEVENT_RESTORED = 9,
        WINDOWEVENT_ENTER = 10,
        WINDOWEVENT_LEAVE = 11,
        WINDOWEVENT_FOCUS_GAINED = 12,
        WINDOWEVENT_FOCUS_LOST = 13,
        WINDOWEVENT_CLOSE = 14,
        WINDOWEVENT_TAKE_FOCUS = 15,
        WINDOWEVENT_HIT_TEST = 16
    }
    public struct WindowEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public WindowEventID WindowEventID;
        public int Data1;
        public int Data2;
    }
    public enum DisplayEventID : byte
    {
        DISPLAYEVENT_NONE = 0,
        DISPLAYEVENT_ORIENTATION = 1,
        DISPLAYEVENT_CONNECTED = 2,
        DISPLAYEVENT_DISCONNECTED = 3
    }
    public struct DisplayEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint Display;
        public DisplayEventID DisplayEventID;
        public int Data1;
    }
    public struct DropEvent
    {
        public EventType Type;
        public uint Timestamp;
        public IntPtr File;
        public uint WindowID;
    }
    public struct Event
    {
        public EventType Type;
        public DollarGestureEvent DollarGesture;
        public MultiGestureEvent MultiGesture;
        public TouchFingerEvent TouchFinger;
        public SysWMEvent SysWM;
        public UserEvent User;
        public QuitEvent Quit;
        public SensorEvent Sensor;
        public AudioDeviceEvent AudioDevice;
        public ControllerDeviceEvent ControllerDeviceSensor;
        public ControllerDeviceEvent ControllerDeviceTouchpad;
        public ControllerDeviceEvent ControllerDevice;
        public ControllerButtonEvent ControllerButton;
        public ControllerAxisEvent ControllerAxis;
        public JoyDeviceEvent JoyDevice;
        public JoyButtonEvent JoyButton;
        public JoyHatEvent JoyHat;
        public JoyBallEvent JoyBall;
        public JoyAxisEvent JoyAxis;
        public MouseWheelEvent MouseWheel;
        public MouseButtonEvent MouseButton;
        public MouseMotionEvent MouseMotion;
        public TextInputEvent TextInput;
        public TextEditingEvent TextEdit;
        public KeyboardEvent Keyboard;
        public WindowEvent Window;
        public DisplayEvent Display;
        public EventType TypeFSharp;
        public DropEvent Drop;
    }
    #endregion Events

    public interface IEventData //: IO.ISerializer
    {

    }

    public interface IEvent
    {
        public abstract bool CanTrigging(ref Event e);
        public void OnTrigging(ref Event e);
        public void RegSelf(UInputSystem inputSystem);
    }

    public interface IMessageData : IEventData
    {

    }

    public interface IMessage : IEvent
    {

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS
{
    public unsafe partial struct ImGuiAPI
    {
        public static unsafe Graphics.Pipeline.TtPresentWindow GetWindowViewportData()
        {
            var viewport = ImGuiAPI.GetWindowViewport();
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return null;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;

            return myWindow;
        }
        public static void GotoColumns(int index)
        {
            index = index % GetColumnsCount();
            var cur = GetColumnIndex();
            while (cur != index)
            {
                NextColumn();
                cur = GetColumnIndex();
            }
        }
        //这个文件是手撸代码，做一些marshal
        public static bool Combo(string label, ref int current_item, List<string> items, int items_count, int popup_max_height_in_items)
        {
            var ppStrings = stackalloc SByte*[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                ppStrings[i] = (SByte*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(items[i]).ToPointer();
            }
            var result = Combo(label, ref current_item, ppStrings, items_count, popup_max_height_in_items);
            for (int i = 0; i < items.Count; i++)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)ppStrings[i]);
            }
            return result;
        }
        public static bool PointInRect(ref Vector2 pt, ref Vector2 min, ref Vector2 max)
        {
            if (pt.X > max.X || pt.X < min.X || pt.Y > max.Y || pt.Y < min.Y)
                return false;
            return true;
        }
        public static bool InputText(string label, ref string text, 
            ImGuiInputTextFlags_ flags = ImGuiInputTextFlags_.ImGuiInputTextFlags_None, 
            FDelegate_ImGuiInputTextCallback callback = null, object user_data = null)
        {
            if (text == null)
                text = "";
            using (var buffer = BigStackBuffer.CreateInstance(128))
            {
                buffer.SetTextUtf8(text);
                bool changed = false;
                if (user_data == null)
                {
                    changed = InputText(label, buffer.GetBuffer(), (uint)buffer.GetSize(), flags, callback, (void*)0);
                }
                else
                {
                    // user_data to gchandle
                    changed = InputText(label, buffer.GetBuffer(), (uint)buffer.GetSize(), flags, callback, IntPtr.Zero.ToPointer());
                }
                if (changed)
                {
                    text = buffer.AsTextUtf8();
                    return true;
                }
                return false;
            }   
        }
        public static bool Button(string label)
        {
            var sz = new Vector2();
            return Button(label, in sz);
        }

        public static ImGuiKey GetImGuiKey(Bricks.Input.Keycode code)
        {
            switch(code)
            {
                case Bricks.Input.Keycode.KEY_UNKNOWN:
                    return ImGuiKey.ImGuiKey_None;
                case Bricks.Input.Keycode.KEY_BACKSPACE:
                    return ImGuiKey.ImGuiKey_Backspace;
                case Bricks.Input.Keycode.KEY_TAB:
                    return ImGuiKey.ImGuiKey_Tab;
                case Bricks.Input.Keycode.KEY_RETURN:
                    return ImGuiKey.ImGuiKey_Enter;
                case Bricks.Input.Keycode.KEY_ESCAPE:
                    return ImGuiKey.ImGuiKey_Escape;
                case Bricks.Input.Keycode.KEY_SPACE:
                    return ImGuiKey.ImGuiKey_Space;
                case Bricks.Input.Keycode.KEY_QUOTE:
                    return ImGuiKey.ImGuiKey_Apostrophe;
                case Bricks.Input.Keycode.KEY_COMMA:
                    return ImGuiKey.ImGuiKey_Comma;
                case Bricks.Input.Keycode.KEY_MINUS:
                    return ImGuiKey.ImGuiKey_Minus;
                case Bricks.Input.Keycode.KEY_PERIOD:
                    return ImGuiKey.ImGuiKey_Period;
                case Bricks.Input.Keycode.KEY_SLASH:
                    return ImGuiKey.ImGuiKey_Slash;
                case Bricks.Input.Keycode.KEY_0:
                    return ImGuiKey.ImGuiKey_0;
                case Bricks.Input.Keycode.KEY_1:
                    return ImGuiKey.ImGuiKey_1;
                case Bricks.Input.Keycode.KEY_2:
                    return ImGuiKey.ImGuiKey_2;
                case Bricks.Input.Keycode.KEY_3:
                    return ImGuiKey.ImGuiKey_3;
                case Bricks.Input.Keycode.KEY_4:
                    return ImGuiKey.ImGuiKey_4;
                case Bricks.Input.Keycode.KEY_5:
                    return ImGuiKey.ImGuiKey_5;
                case Bricks.Input.Keycode.KEY_6:
                    return ImGuiKey.ImGuiKey_6;
                case Bricks.Input.Keycode.KEY_7:
                    return ImGuiKey.ImGuiKey_7;
                case Bricks.Input.Keycode.KEY_8:
                    return ImGuiKey.ImGuiKey_8;
                case Bricks.Input.Keycode.KEY_9:
                    return ImGuiKey.ImGuiKey_9;
                case Bricks.Input.Keycode.KEY_SEMICOLON:
                    return ImGuiKey.ImGuiKey_Semicolon;
                case Bricks.Input.Keycode.KEY_EQUALS:
                    return ImGuiKey.ImGuiKey_Equal;
                case Bricks.Input.Keycode.KEY_LEFTBRACKET:
                    return ImGuiKey.ImGuiKey_LeftBracket;
                case Bricks.Input.Keycode.KEY_BACKSLASH:
                    return ImGuiKey.ImGuiKey_Backslash;
                case Bricks.Input.Keycode.KEY_RIGHTBRACKET:
                    return ImGuiKey.ImGuiKey_RightBracket;
                case Bricks.Input.Keycode.KEY_BACKQUOTE:
                    return ImGuiKey.ImGuiKey_GraveAccent;
                case Bricks.Input.Keycode.KEY_a:
                    return ImGuiKey.ImGuiKey_A;
                case Bricks.Input.Keycode.KEY_b:
                    return ImGuiKey.ImGuiKey_B;
                case Bricks.Input.Keycode.KEY_c:
                    return ImGuiKey.ImGuiKey_C;
                case Bricks.Input.Keycode.KEY_d:
                    return ImGuiKey.ImGuiKey_D;
                case Bricks.Input.Keycode.KEY_e:
                    return ImGuiKey.ImGuiKey_E;
                case Bricks.Input.Keycode.KEY_f:
                    return ImGuiKey.ImGuiKey_F;
                case Bricks.Input.Keycode.KEY_g:
                    return ImGuiKey.ImGuiKey_G;
                case Bricks.Input.Keycode.KEY_h:
                    return ImGuiKey.ImGuiKey_H;
                case Bricks.Input.Keycode.KEY_i:
                    return ImGuiKey.ImGuiKey_I;
                case Bricks.Input.Keycode.KEY_j:
                    return ImGuiKey.ImGuiKey_J;
                case Bricks.Input.Keycode.KEY_k:
                    return ImGuiKey.ImGuiKey_K;
                case Bricks.Input.Keycode.KEY_l:
                    return ImGuiKey.ImGuiKey_L;
                case Bricks.Input.Keycode.KEY_m:
                    return ImGuiKey.ImGuiKey_M;
                case Bricks.Input.Keycode.KEY_n:
                    return ImGuiKey.ImGuiKey_N;
                case Bricks.Input.Keycode.KEY_o:
                    return ImGuiKey.ImGuiKey_O;
                case Bricks.Input.Keycode.KEY_p:
                    return ImGuiKey.ImGuiKey_P;
                case Bricks.Input.Keycode.KEY_q:
                    return ImGuiKey.ImGuiKey_Q;
                case Bricks.Input.Keycode.KEY_r:
                    return ImGuiKey.ImGuiKey_R;
                case Bricks.Input.Keycode.KEY_s:
                    return ImGuiKey.ImGuiKey_S;
                case Bricks.Input.Keycode.KEY_t:
                    return ImGuiKey.ImGuiKey_T;
                case Bricks.Input.Keycode.KEY_u:
                    return ImGuiKey.ImGuiKey_U;
                case Bricks.Input.Keycode.KEY_v:
                    return ImGuiKey.ImGuiKey_V;
                case Bricks.Input.Keycode.KEY_w:
                    return ImGuiKey.ImGuiKey_W;
                case Bricks.Input.Keycode.KEY_x:
                    return ImGuiKey.ImGuiKey_X;
                case Bricks.Input.Keycode.KEY_y:
                    return ImGuiKey.ImGuiKey_Y;
                case Bricks.Input.Keycode.KEY_z:
                    return ImGuiKey.ImGuiKey_Z;
                case Bricks.Input.Keycode.KEY_DELETE:
                    return ImGuiKey.ImGuiKey_Delete;
                case Bricks.Input.Keycode.KEY_CAPSLOCK:
                    return ImGuiKey.ImGuiKey_CapsLock;
                case Bricks.Input.Keycode.KEY_F1:
                    return ImGuiKey.ImGuiKey_F1;
                case Bricks.Input.Keycode.KEY_F2:
                    return ImGuiKey.ImGuiKey_F2;
                case Bricks.Input.Keycode.KEY_F3:
                    return ImGuiKey.ImGuiKey_F3;
                case Bricks.Input.Keycode.KEY_F4:
                    return ImGuiKey.ImGuiKey_F4;
                case Bricks.Input.Keycode.KEY_F5:
                    return ImGuiKey.ImGuiKey_F5;
                case Bricks.Input.Keycode.KEY_F6:
                    return ImGuiKey.ImGuiKey_F6;
                case Bricks.Input.Keycode.KEY_F7:
                    return ImGuiKey.ImGuiKey_F7;
                case Bricks.Input.Keycode.KEY_F8:
                    return ImGuiKey.ImGuiKey_F8;
                case Bricks.Input.Keycode.KEY_F9:
                    return ImGuiKey.ImGuiKey_F9;
                case Bricks.Input.Keycode.KEY_F10:
                    return ImGuiKey.ImGuiKey_F10;
                case Bricks.Input.Keycode.KEY_F11:
                    return ImGuiKey.ImGuiKey_F11;
                case Bricks.Input.Keycode.KEY_F12:
                    return ImGuiKey.ImGuiKey_F12;
                case Bricks.Input.Keycode.KEY_PRINTSCREEN:
                    return ImGuiKey.ImGuiKey_PrintScreen;
                case Bricks.Input.Keycode.KEY_SCROLLLOCK:
                    return ImGuiKey.ImGuiKey_ScrollLock;
                case Bricks.Input.Keycode.KEY_PAUSE:
                    return ImGuiKey.ImGuiKey_Pause;
                case Bricks.Input.Keycode.KEY_INSERT:
                    return ImGuiKey.ImGuiKey_Insert;
                case Bricks.Input.Keycode.KEY_HOME:
                    return ImGuiKey.ImGuiKey_Home;
                case Bricks.Input.Keycode.KEY_PAGEUP:
                    return ImGuiKey.ImGuiKey_PageUp;
                case Bricks.Input.Keycode.KEY_END:
                    return ImGuiKey.ImGuiKey_End;
                case Bricks.Input.Keycode.KEY_PAGEDOWN:
                    return ImGuiKey.ImGuiKey_PageDown;
                case Bricks.Input.Keycode.KEY_RIGHT:
                    return ImGuiKey.ImGuiKey_RightArrow;
                case Bricks.Input.Keycode.KEY_LEFT:
                    return ImGuiKey.ImGuiKey_LeftArrow;
                case Bricks.Input.Keycode.KEY_DOWN:
                    return ImGuiKey.ImGuiKey_DownArrow;
                case Bricks.Input.Keycode.KEY_UP:
                    return ImGuiKey.ImGuiKey_UpArrow;
                case Bricks.Input.Keycode.KEY_NUMLOCKCLEAR:
                    return ImGuiKey.ImGuiKey_NumLock;
                case Bricks.Input.Keycode.KEY_KP_DIVIDE:
                    return ImGuiKey.ImGuiKey_KeypadDivide;
                case Bricks.Input.Keycode.KEY_KP_MULTIPLY:
                    return ImGuiKey.ImGuiKey_KeypadMultiply;
                case Bricks.Input.Keycode.KEY_KP_MINUS:
                    return ImGuiKey.ImGuiKey_KeypadSubtract;
                case Bricks.Input.Keycode.KEY_KP_PLUS:
                    return ImGuiKey.ImGuiKey_KeypadAdd;
                case Bricks.Input.Keycode.KEY_KP_ENTER:
                    return ImGuiKey.ImGuiKey_KeypadEnter;
                case Bricks.Input.Keycode.KEY_KP_1:
                    return ImGuiKey.ImGuiKey_Keypad1;
                case Bricks.Input.Keycode.KEY_KP_2:
                    return ImGuiKey.ImGuiKey_Keypad2;
                case Bricks.Input.Keycode.KEY_KP_3:
                    return ImGuiKey.ImGuiKey_Keypad3;
                case Bricks.Input.Keycode.KEY_KP_4:
                    return ImGuiKey.ImGuiKey_Keypad4;
                case Bricks.Input.Keycode.KEY_KP_5:
                    return ImGuiKey.ImGuiKey_Keypad5;
                case Bricks.Input.Keycode.KEY_KP_6:
                    return ImGuiKey.ImGuiKey_Keypad6;
                case Bricks.Input.Keycode.KEY_KP_7:
                    return ImGuiKey.ImGuiKey_Keypad7;
                case Bricks.Input.Keycode.KEY_KP_8:
                    return ImGuiKey.ImGuiKey_Keypad8;
                case Bricks.Input.Keycode.KEY_KP_9:
                    return ImGuiKey.ImGuiKey_Keypad9;
                case Bricks.Input.Keycode.KEY_KP_0:
                    return ImGuiKey.ImGuiKey_Keypad0;
                case Bricks.Input.Keycode.KEY_KP_PERIOD:
                    return ImGuiKey.ImGuiKey_KeypadDecimal;
                case Bricks.Input.Keycode.KEY_KP_EQUALS:
                    return ImGuiKey.ImGuiKey_KeypadEqual;
                case Bricks.Input.Keycode.KEY_F13:
                    return ImGuiKey.ImGuiKey_F13;
                case Bricks.Input.Keycode.KEY_F14:
                    return ImGuiKey.ImGuiKey_F14;
                case Bricks.Input.Keycode.KEY_F15:
                    return ImGuiKey.ImGuiKey_F15;
                case Bricks.Input.Keycode.KEY_F16:
                    return ImGuiKey.ImGuiKey_F16;
                case Bricks.Input.Keycode.KEY_F17:
                    return ImGuiKey.ImGuiKey_F17;
                case Bricks.Input.Keycode.KEY_F18:
                    return ImGuiKey.ImGuiKey_F18;
                case Bricks.Input.Keycode.KEY_F19:
                    return ImGuiKey.ImGuiKey_F19;
                case Bricks.Input.Keycode.KEY_F20:
                    return ImGuiKey.ImGuiKey_F20;
                case Bricks.Input.Keycode.KEY_F21:
                    return ImGuiKey.ImGuiKey_F21;
                case Bricks.Input.Keycode.KEY_F22:
                    return ImGuiKey.ImGuiKey_F22;
                case Bricks.Input.Keycode.KEY_F23:
                    return ImGuiKey.ImGuiKey_F23;
                case Bricks.Input.Keycode.KEY_F24:
                    return ImGuiKey.ImGuiKey_F24;
                case Bricks.Input.Keycode.KEY_LCTRL:
                    return ImGuiKey.ImGuiKey_LeftCtrl;
                case Bricks.Input.Keycode.KEY_LSHIFT:
                    return ImGuiKey.ImGuiKey_LeftShift;
                case Bricks.Input.Keycode.KEY_LALT:
                    return ImGuiKey.ImGuiKey_LeftAlt;
                case Bricks.Input.Keycode.KEY_LGUI:
                    return ImGuiKey.ImGuiKey_LeftSuper;
                case Bricks.Input.Keycode.KEY_RCTRL:
                    return ImGuiKey.ImGuiKey_RightCtrl;
                case Bricks.Input.Keycode.KEY_RSHIFT:
                    return ImGuiKey.ImGuiKey_RightShift;
                case Bricks.Input.Keycode.KEY_RALT:
                    return ImGuiKey.ImGuiKey_RightAlt;
                case Bricks.Input.Keycode.KEY_RGUI:
                    return ImGuiKey.ImGuiKey_RightSuper;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    return ImGuiKey.ImGuiKey_None;
            }
        }
        public static ImGuiKey GetImGuiKey(Bricks.Input.Scancode code)
        {
            switch (code)
            {
                case Bricks.Input.Scancode.SCANCODE_UNKNOWN:
                    return ImGuiKey.ImGuiKey_None;
                case Bricks.Input.Scancode.SCANCODE_A:
                    return ImGuiKey.ImGuiKey_A;
                case Bricks.Input.Scancode.SCANCODE_B:
                    return ImGuiKey.ImGuiKey_B;
                case Bricks.Input.Scancode.SCANCODE_C:
                    return ImGuiKey.ImGuiKey_C;
                case Bricks.Input.Scancode.SCANCODE_D:
                    return ImGuiKey.ImGuiKey_D;
                case Bricks.Input.Scancode.SCANCODE_E:
                    return ImGuiKey.ImGuiKey_E;
                case Bricks.Input.Scancode.SCANCODE_F:
                    return ImGuiKey.ImGuiKey_F;
                case Bricks.Input.Scancode.SCANCODE_G:
                    return ImGuiKey.ImGuiKey_G;
                case Bricks.Input.Scancode.SCANCODE_H:
                    return ImGuiKey.ImGuiKey_H;
                case Bricks.Input.Scancode.SCANCODE_I:
                    return ImGuiKey.ImGuiKey_I;
                case Bricks.Input.Scancode.SCANCODE_J:
                    return ImGuiKey.ImGuiKey_J;
                case Bricks.Input.Scancode.SCANCODE_K:
                    return ImGuiKey.ImGuiKey_K;
                case Bricks.Input.Scancode.SCANCODE_L:
                    return ImGuiKey.ImGuiKey_L;
                case Bricks.Input.Scancode.SCANCODE_M:
                    return ImGuiKey.ImGuiKey_M;
                case Bricks.Input.Scancode.SCANCODE_N:
                    return ImGuiKey.ImGuiKey_N;
                case Bricks.Input.Scancode.SCANCODE_O:
                    return ImGuiKey.ImGuiKey_O;
                case Bricks.Input.Scancode.SCANCODE_P:
                    return ImGuiKey.ImGuiKey_P;
                case Bricks.Input.Scancode.SCANCODE_Q:
                    return ImGuiKey.ImGuiKey_Q;
                case Bricks.Input.Scancode.SCANCODE_R:
                    return ImGuiKey.ImGuiKey_R;
                case Bricks.Input.Scancode.SCANCODE_S:
                    return ImGuiKey.ImGuiKey_S;
                case Bricks.Input.Scancode.SCANCODE_T:
                    return ImGuiKey.ImGuiKey_T;
                case Bricks.Input.Scancode.SCANCODE_U:
                    return ImGuiKey.ImGuiKey_U;
                case Bricks.Input.Scancode.SCANCODE_V:
                    return ImGuiKey.ImGuiKey_V;
                case Bricks.Input.Scancode.SCANCODE_W:
                    return ImGuiKey.ImGuiKey_W;
                case Bricks.Input.Scancode.SCANCODE_X:
                    return ImGuiKey.ImGuiKey_X;
                case Bricks.Input.Scancode.SCANCODE_Y:
                    return ImGuiKey.ImGuiKey_Y;
                case Bricks.Input.Scancode.SCANCODE_Z:
                    return ImGuiKey.ImGuiKey_Z;
                case Bricks.Input.Scancode.SCANCODE_1:
                    return ImGuiKey.ImGuiKey_1;
                case Bricks.Input.Scancode.SCANCODE_2:
                    return ImGuiKey.ImGuiKey_2;
                case Bricks.Input.Scancode.SCANCODE_3:
                    return ImGuiKey.ImGuiKey_3;
                case Bricks.Input.Scancode.SCANCODE_4:
                    return ImGuiKey.ImGuiKey_4;
                case Bricks.Input.Scancode.SCANCODE_5:
                    return ImGuiKey.ImGuiKey_5;
                case Bricks.Input.Scancode.SCANCODE_6:
                    return ImGuiKey.ImGuiKey_6;
                case Bricks.Input.Scancode.SCANCODE_7:
                    return ImGuiKey.ImGuiKey_7;
                case Bricks.Input.Scancode.SCANCODE_8:
                    return ImGuiKey.ImGuiKey_8;
                case Bricks.Input.Scancode.SCANCODE_9:
                    return ImGuiKey.ImGuiKey_9;
                case Bricks.Input.Scancode.SCANCODE_0:
                    return ImGuiKey.ImGuiKey_0;
                case Bricks.Input.Scancode.NUM_SCANCODES:
                    return ImGuiKey.ImGuiKey_COUNT;
                case Bricks.Input.Scancode.SCANCODE_RETURN:
                    return ImGuiKey.ImGuiKey_Enter;
                case Bricks.Input.Scancode.SCANCODE_ESCAPE:
                    return ImGuiKey.ImGuiKey_Escape;
                case Bricks.Input.Scancode.SCANCODE_BACKSPACE:
                    return ImGuiKey.ImGuiKey_Backspace;
                case Bricks.Input.Scancode.SCANCODE_TAB:
                    return ImGuiKey.ImGuiKey_Tab;
                case Bricks.Input.Scancode.SCANCODE_SPACE:
                    return ImGuiKey.ImGuiKey_Space;
                case Bricks.Input.Scancode.SCANCODE_MINUS:
                    return ImGuiKey.ImGuiKey_Minus;
                case Bricks.Input.Scancode.SCANCODE_EQUALS:
                    return ImGuiKey.ImGuiKey_Equal;
                case Bricks.Input.Scancode.SCANCODE_LEFTBRACKET:
                    return ImGuiKey.ImGuiKey_LeftBracket;
                case Bricks.Input.Scancode.SCANCODE_RIGHTBRACKET:
                    return ImGuiKey.ImGuiKey_RightBracket;
                case Bricks.Input.Scancode.SCANCODE_BACKSLASH:
                    return ImGuiKey.ImGuiKey_Backslash;
                case Bricks.Input.Scancode.SCANCODE_SEMICOLON:
                    return ImGuiKey.ImGuiKey_Semicolon;
                case Bricks.Input.Scancode.SCANCODE_APOSTROPHE:
                    return ImGuiKey.ImGuiKey_Apostrophe;
                case Bricks.Input.Scancode.SCANCODE_GRAVE:
                    return ImGuiKey.ImGuiKey_GraveAccent;
                case Bricks.Input.Scancode.SCANCODE_COMMA:
                    return ImGuiKey.ImGuiKey_Comma;
                case Bricks.Input.Scancode.SCANCODE_PERIOD:
                    return ImGuiKey.ImGuiKey_Period;
                case Bricks.Input.Scancode.SCANCODE_SLASH:
                    return ImGuiKey.ImGuiKey_Slash;
                case Bricks.Input.Scancode.SCANCODE_CAPSLOCK:
                    return ImGuiKey.ImGuiKey_CapsLock;
                case Bricks.Input.Scancode.SCANCODE_F1:
                    return ImGuiKey.ImGuiKey_F1;
                case Bricks.Input.Scancode.SCANCODE_F2:
                    return ImGuiKey.ImGuiKey_F2;
                case Bricks.Input.Scancode.SCANCODE_F3:
                    return ImGuiKey.ImGuiKey_F3;
                case Bricks.Input.Scancode.SCANCODE_F4:
                    return ImGuiKey.ImGuiKey_F4;
                case Bricks.Input.Scancode.SCANCODE_F5:
                    return ImGuiKey.ImGuiKey_F5;
                case Bricks.Input.Scancode.SCANCODE_F6:
                    return ImGuiKey.ImGuiKey_F6;
                case Bricks.Input.Scancode.SCANCODE_F7:
                    return ImGuiKey.ImGuiKey_F7;
                case Bricks.Input.Scancode.SCANCODE_F8:
                    return ImGuiKey.ImGuiKey_F8;
                case Bricks.Input.Scancode.SCANCODE_F9:
                    return ImGuiKey.ImGuiKey_F9;
                case Bricks.Input.Scancode.SCANCODE_F10:
                    return ImGuiKey.ImGuiKey_F10;
                case Bricks.Input.Scancode.SCANCODE_F11:
                    return ImGuiKey.ImGuiKey_F11;
                case Bricks.Input.Scancode.SCANCODE_F12:
                    return ImGuiKey.ImGuiKey_F12;
                case Bricks.Input.Scancode.SCANCODE_PRINTSCREEN:
                    return ImGuiKey.ImGuiKey_PrintScreen;
                case Bricks.Input.Scancode.SCANCODE_SCROLLLOCK:
                    return ImGuiKey.ImGuiKey_ScrollLock;
                case Bricks.Input.Scancode.SCANCODE_PAUSE:
                    return ImGuiKey.ImGuiKey_Pause;
                case Bricks.Input.Scancode.SCANCODE_INSERT:
                    return ImGuiKey.ImGuiKey_Insert;
                case Bricks.Input.Scancode.SCANCODE_HOME:
                    return ImGuiKey.ImGuiKey_Home;
                case Bricks.Input.Scancode.SCANCODE_PAGEUP:
                    return ImGuiKey.ImGuiKey_PageUp;
                case Bricks.Input.Scancode.SCANCODE_DELETE:
                    return ImGuiKey.ImGuiKey_Delete;
                case Bricks.Input.Scancode.SCANCODE_END:
                    return ImGuiKey.ImGuiKey_End;
                case Bricks.Input.Scancode.SCANCODE_PAGEDOWN:
                    return ImGuiKey.ImGuiKey_PageDown;
                case Bricks.Input.Scancode.SCANCODE_RIGHT:
                    return ImGuiKey.ImGuiKey_RightArrow;
                case Bricks.Input.Scancode.SCANCODE_LEFT:
                    return ImGuiKey.ImGuiKey_LeftArrow;
                case Bricks.Input.Scancode.SCANCODE_DOWN:
                    return ImGuiKey.ImGuiKey_DownArrow;
                case Bricks.Input.Scancode.SCANCODE_UP:
                    return ImGuiKey.ImGuiKey_UpArrow;
                case Bricks.Input.Scancode.SCANCODE_NUMLOCKCLEAR:
                    return ImGuiKey.ImGuiKey_NumLock;
                case Bricks.Input.Scancode.SCANCODE_KP_DIVIDE:
                    return ImGuiKey.ImGuiKey_KeypadDivide;
                case Bricks.Input.Scancode.SCANCODE_KP_MULTIPLY:
                    return ImGuiKey.ImGuiKey_KeypadMultiply;
                case Bricks.Input.Scancode.SCANCODE_KP_MINUS:
                    return ImGuiKey.ImGuiKey_KeypadSubtract;
                case Bricks.Input.Scancode.SCANCODE_KP_PLUS:
                    return ImGuiKey.ImGuiKey_KeypadAdd;
                case Bricks.Input.Scancode.SCANCODE_KP_ENTER:
                    return ImGuiKey.ImGuiKey_KeypadEnter;
                case Bricks.Input.Scancode.SCANCODE_KP_1:
                    return ImGuiKey.ImGuiKey_Keypad1;
                case Bricks.Input.Scancode.SCANCODE_KP_2:
                    return ImGuiKey.ImGuiKey_Keypad2;
                case Bricks.Input.Scancode.SCANCODE_KP_3:
                    return ImGuiKey.ImGuiKey_Keypad3;
                case Bricks.Input.Scancode.SCANCODE_KP_4:
                    return ImGuiKey.ImGuiKey_Keypad4;
                case Bricks.Input.Scancode.SCANCODE_KP_5:
                    return ImGuiKey.ImGuiKey_Keypad5;
                case Bricks.Input.Scancode.SCANCODE_KP_6:
                    return ImGuiKey.ImGuiKey_Keypad6;
                case Bricks.Input.Scancode.SCANCODE_KP_7:
                    return ImGuiKey.ImGuiKey_Keypad7;
                case Bricks.Input.Scancode.SCANCODE_KP_8:
                    return ImGuiKey.ImGuiKey_Keypad8;
                case Bricks.Input.Scancode.SCANCODE_KP_9:
                    return ImGuiKey.ImGuiKey_Keypad9;
                case Bricks.Input.Scancode.SCANCODE_KP_0:
                    return ImGuiKey.ImGuiKey_Keypad0;
                case Bricks.Input.Scancode.SCANCODE_KP_PERIOD:
                    return ImGuiKey.ImGuiKey_KeypadDecimal;
                case Bricks.Input.Scancode.SCANCODE_NONUSBACKSLASH:
                    return ImGuiKey.ImGuiKey_Backslash; // This is a guess, as SCANCODE_NONUSBACKSLASH is not a standard key
                case Bricks.Input.Scancode.SCANCODE_F13:
                    return ImGuiKey.ImGuiKey_F13;
                case Bricks.Input.Scancode.SCANCODE_F14:
                    return ImGuiKey.ImGuiKey_F14;
                case Bricks.Input.Scancode.SCANCODE_F15:
                    return ImGuiKey.ImGuiKey_F15;
                case Bricks.Input.Scancode.SCANCODE_F16:
                    return ImGuiKey.ImGuiKey_F16;
                case Bricks.Input.Scancode.SCANCODE_F17:
                    return ImGuiKey.ImGuiKey_F17;
                case Bricks.Input.Scancode.SCANCODE_F18:
                    return ImGuiKey.ImGuiKey_F18;
                case Bricks.Input.Scancode.SCANCODE_F19:
                    return ImGuiKey.ImGuiKey_F19;
                case Bricks.Input.Scancode.SCANCODE_F20:
                    return ImGuiKey.ImGuiKey_F20;
                case Bricks.Input.Scancode.SCANCODE_F21:
                    return ImGuiKey.ImGuiKey_F21;
                case Bricks.Input.Scancode.SCANCODE_F22:
                    return ImGuiKey.ImGuiKey_F22;
                case Bricks.Input.Scancode.SCANCODE_F23:
                    return ImGuiKey.ImGuiKey_F23;
                case Bricks.Input.Scancode.SCANCODE_F24:
                    return ImGuiKey.ImGuiKey_F24;
                case Bricks.Input.Scancode.SCANCODE_LCTRL:
                    return ImGuiKey.ImGuiKey_LeftCtrl;
                case Bricks.Input.Scancode.SCANCODE_LSHIFT:
                    return ImGuiKey.ImGuiKey_LeftShift;
                case Bricks.Input.Scancode.SCANCODE_LALT:
                    return ImGuiKey.ImGuiKey_LeftAlt;
                case Bricks.Input.Scancode.SCANCODE_LGUI:
                    return ImGuiKey.ImGuiKey_LeftSuper;
                case Bricks.Input.Scancode.SCANCODE_RCTRL:
                    return ImGuiKey.ImGuiKey_RightCtrl;
                case Bricks.Input.Scancode.SCANCODE_RSHIFT:
                    return ImGuiKey.ImGuiKey_RightShift;
                case Bricks.Input.Scancode.SCANCODE_RALT:
                    return ImGuiKey.ImGuiKey_RightAlt;
                case Bricks.Input.Scancode.SCANCODE_RGUI:
                    return ImGuiKey.ImGuiKey_RightSuper;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    return ImGuiKey.ImGuiKey_None;
            }
        }
    }
}

public unsafe partial struct ImDrawList
{
    public ImDrawCmd* CmdBufferData
    {
        get
        {
            int size = 0;
            return GetCmdBuffer(&size);
        }
    }
    public int CmdBufferSize
    {
        get
        {
            int size = 0;
            GetCmdBuffer(&size);
            return size;
        }
    }
    public ImDrawVert* VtxBufferData
    {
        get
        {
            int size = 0;
            return GetVtxBuffer(&size);
        }
    }
    public int VtxBufferSize
    {
        get
        {
            int size = 0;
            GetVtxBuffer(&size);
            return size;
        }
    }
    public UInt16* IdxBufferData
    {
        get
        {
            int size = 0;
            return GetIdxBuffer(&size);
        }
    }
    public int IdxBufferSize
    {
        get
        {
            int size = 0;
            GetIdxBuffer(&size);
            return size;
        }
    }
}
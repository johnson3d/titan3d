using EngineNS.Graphics.Pipeline;
using MathNet.Numerics;
using SDL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class TtDockWindowSDL
    {
        #region SDL
        public static unsafe void ImGui_ImplSDL3_UpdateMonitors()
        {
            var platform_io = ImGuiAPI.GetPlatformIO();
            ImGuiAPI.PlatformIO_Monitor_Resize(platform_io, 0);
            //int display_count = SDL.SDL3.SDL_GetDisplays();
            var dspl = SDL.SDL3.SDL_GetDisplays();
            for (int n = 0; n < dspl.Count; n++)
            {
                // Warning: the validity of monitor DPI information on Windows depends on the application DPI awareness settings, which generally needs to be set in the manifest or at runtime.
                ImGuiPlatformMonitor monitor = new ImGuiPlatformMonitor();
                monitor.UnsafeCallConstructor();
                SDL.SDL_Rect r;
                SDL.SDL3.SDL_GetDisplayBounds(dspl[n], &r);
                monitor.MainPos = monitor.WorkPos = new Vector2((float)r.x, (float)r.y);
                monitor.MainSize = monitor.WorkSize = new Vector2((float)r.w, (float)r.h);
                if (SDL.SDL3.SDL_GetDisplayUsableBounds(dspl[n], &r))
                {
                    monitor.WorkPos = new Vector2((float)r.x, (float)r.y);
                    monitor.WorkSize = new Vector2((float)r.w, (float)r.h);
                }
                var dpi = SDL.SDL3.SDL_GetDisplayContentScale(dspl[n]);
                monitor.DpiScale = dpi;
                if (monitor.DpiScale <= 0.0f)
                    continue; // Some accessibility applications are declaring virtual monitors with a DPI of 0, see #7902.
                ImGuiAPI.PlatformIO_Monitor_PushBack(platform_io, monitor);
            }
            dspl.Dispose();
            //SDL.SDL3.SDL_free(dspl);
        }
        public static unsafe void ImGui_ImplSDL3_InitPlatformInterface()
        {
            // Register platform interface (will be coupled with a renderer interface)
            var platform_io = ImGuiAPI.GetPlatformIO();
            platform_io.Platform_CreateWindow = (ImGui_ImplSDL2_CreateWindow);
            platform_io.Platform_DestroyWindow = (ImGui_ImplSDL2_DestroyWindow);
            platform_io.Platform_ShowWindow = (ImGui_ImplSDL2_ShowWindow);
            platform_io.Platform_SetWindowPos = (ImGui_ImplSDL2_SetWindowPos);
            platform_io.Platform_GetWindowPos = (ImGui_ImplSDL2_GetWindowPos);
            platform_io.Platform_SetWindowSize = (ImGui_ImplSDL2_SetWindowSize);
            platform_io.Platform_GetWindowSize = (ImGui_ImplSDL2_GetWindowSize);
            platform_io.Platform_SetWindowFocus = (ImGui_ImplSDL2_SetWindowFocus);
            platform_io.Platform_GetWindowFocus = (ImGui_ImplSDL2_GetWindowFocus);
            platform_io.Platform_GetWindowMinimized = (ImGui_ImplSDL2_GetWindowMinimized);
            platform_io.Platform_SetWindowTitle = (ImGui_ImplSDL2_SetWindowTitle);
            platform_io.Platform_RenderWindow = (ImGui_ImplSDL2_RenderWindow);
            platform_io.Platform_SwapBuffers = (ImGui_ImplSDL2_SwapBuffers);
            platform_io.Platform_SetWindowAlpha = (ImGui_ImplSDL2_SetWindowAlpha);

            platform_io.Renderer_CreateWindow = (ImGui_Renderer_CreateWindow);
            platform_io.Renderer_DestroyWindow = (ImGui_Renderer_DestroyWindow);
            platform_io.Renderer_SetWindowSize = (ImGui_Renderer_SetWindowSize);
            platform_io.Renderer_RenderWindow = (ImGui_Renderer_RenderWindow);
            platform_io.Renderer_SwapBuffers = (ImGui_Renderer_SwapBuffers);

            //platform_io.Platform_CreateVkSurface = ImGui_ImplSDL2_CreateVkSurface;

            // SDL2 by default doesn't pass mouse clicks to the application when the click focused a window. This is getting in the way of our interactions and we disable that behavior.
            SDL.SDL3.SDL_SetHint(SDL.SDL3.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
        }
        public static ImGuiKey ImGui_ImplSDL3_KeyEventToImGuiKey(SDL_Keycode keycode, SDL_Scancode scancode)
        {
            // Keypad doesn't have individual key values in SDL3
            switch (scancode)
            {
                case SDL_Scancode.SDL_SCANCODE_KP_0: return ImGuiKey.ImGuiKey_Keypad0;
                case SDL_Scancode.SDL_SCANCODE_KP_1: return ImGuiKey.ImGuiKey_Keypad1;
                case SDL_Scancode.SDL_SCANCODE_KP_2: return ImGuiKey.ImGuiKey_Keypad2;
                case SDL_Scancode.SDL_SCANCODE_KP_3: return ImGuiKey.ImGuiKey_Keypad3;
                case SDL_Scancode.SDL_SCANCODE_KP_4: return ImGuiKey.ImGuiKey_Keypad4;
                case SDL_Scancode.SDL_SCANCODE_KP_5: return ImGuiKey.ImGuiKey_Keypad5;
                case SDL_Scancode.SDL_SCANCODE_KP_6: return ImGuiKey.ImGuiKey_Keypad6;
                case SDL_Scancode.SDL_SCANCODE_KP_7: return ImGuiKey.ImGuiKey_Keypad7;
                case SDL_Scancode.SDL_SCANCODE_KP_8: return ImGuiKey.ImGuiKey_Keypad8;
                case SDL_Scancode.SDL_SCANCODE_KP_9: return ImGuiKey.ImGuiKey_Keypad9;
                case SDL_Scancode.SDL_SCANCODE_KP_PERIOD: return ImGuiKey.ImGuiKey_KeypadDecimal;
                case SDL_Scancode.SDL_SCANCODE_KP_DIVIDE: return ImGuiKey.ImGuiKey_KeypadDivide;
                case SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY: return ImGuiKey.ImGuiKey_KeypadMultiply;
                case SDL_Scancode.SDL_SCANCODE_KP_MINUS: return ImGuiKey.ImGuiKey_KeypadSubtract;
                case SDL_Scancode.SDL_SCANCODE_KP_PLUS: return ImGuiKey.ImGuiKey_KeypadAdd;
                case SDL_Scancode.SDL_SCANCODE_KP_ENTER: return ImGuiKey.ImGuiKey_KeypadEnter;
                case SDL_Scancode.SDL_SCANCODE_KP_EQUALS: return ImGuiKey.ImGuiKey_KeypadEqual;
                default: break;
            }
            switch (keycode)
            {
                case SDL_Keycode.SDLK_TAB: return ImGuiKey.ImGuiKey_Tab;
                case SDL_Keycode.SDLK_LEFT: return ImGuiKey.ImGuiKey_LeftArrow;
                case SDL_Keycode.SDLK_RIGHT: return ImGuiKey.ImGuiKey_RightArrow;
                case SDL_Keycode.SDLK_UP: return ImGuiKey.ImGuiKey_UpArrow;
                case SDL_Keycode.SDLK_DOWN: return ImGuiKey.ImGuiKey_DownArrow;
                case SDL_Keycode.SDLK_PAGEUP: return ImGuiKey.ImGuiKey_PageUp;
                case SDL_Keycode.SDLK_PAGEDOWN: return ImGuiKey.ImGuiKey_PageDown;
                case SDL_Keycode.SDLK_HOME: return ImGuiKey.ImGuiKey_Home;
                case SDL_Keycode.SDLK_END: return ImGuiKey.ImGuiKey_End;
                case SDL_Keycode.SDLK_INSERT: return ImGuiKey.ImGuiKey_Insert;
                case SDL_Keycode.SDLK_DELETE: return ImGuiKey.ImGuiKey_Delete;
                case SDL_Keycode.SDLK_BACKSPACE: return ImGuiKey.ImGuiKey_Backspace;
                case SDL_Keycode.SDLK_SPACE: return ImGuiKey.ImGuiKey_Space;
                case SDL_Keycode.SDLK_RETURN: return ImGuiKey.ImGuiKey_Enter;
                case SDL_Keycode.SDLK_ESCAPE: return ImGuiKey.ImGuiKey_Escape;
                //case SDL_Keycode.SDLK_APOSTROPHE: return ImGuiKey_Apostrophe;
                case SDL_Keycode.SDLK_COMMA: return ImGuiKey.ImGuiKey_Comma;
                //case SDLK_MINUS: return ImGuiKey_Minus;
                case SDL_Keycode.SDLK_PERIOD: return ImGuiKey.ImGuiKey_Period;
                //case SDLK_SLASH: return ImGuiKey_Slash;
                case SDL_Keycode.SDLK_SEMICOLON: return ImGuiKey.ImGuiKey_Semicolon;
                //case SDLK_EQUALS: return ImGuiKey_Equal;
                //case SDLK_LEFTBRACKET: return ImGuiKey_LeftBracket;
                //case SDLK_BACKSLASH: return ImGuiKey_Backslash;
                //case SDLK_RIGHTBRACKET: return ImGuiKey_RightBracket;
                //case SDLK_GRAVE: return ImGuiKey_GraveAccent;
                case SDL_Keycode.SDLK_CAPSLOCK: return ImGuiKey.ImGuiKey_CapsLock;
                case SDL_Keycode.SDLK_SCROLLLOCK: return ImGuiKey.ImGuiKey_ScrollLock;
                case SDL_Keycode.SDLK_NUMLOCKCLEAR: return ImGuiKey.ImGuiKey_NumLock;
                case SDL_Keycode.SDLK_PRINTSCREEN: return ImGuiKey.ImGuiKey_PrintScreen;
                case SDL_Keycode.SDLK_PAUSE: return ImGuiKey.ImGuiKey_Pause;
                case SDL_Keycode.SDLK_LCTRL: return ImGuiKey.ImGuiKey_LeftCtrl;
                case SDL_Keycode.SDLK_LSHIFT: return ImGuiKey.ImGuiKey_LeftShift;
                case SDL_Keycode.SDLK_LALT: return ImGuiKey.ImGuiKey_LeftAlt;
                case SDL_Keycode.SDLK_LGUI: return ImGuiKey.ImGuiKey_LeftSuper;
                case SDL_Keycode.SDLK_RCTRL: return ImGuiKey.ImGuiKey_RightCtrl;
                case SDL_Keycode.SDLK_RSHIFT: return ImGuiKey.ImGuiKey_RightShift;
                case SDL_Keycode.SDLK_RALT: return ImGuiKey.ImGuiKey_RightAlt;
                case SDL_Keycode.SDLK_RGUI: return ImGuiKey.ImGuiKey_RightSuper;
                case SDL_Keycode.SDLK_APPLICATION: return ImGuiKey.ImGuiKey_Menu;
                case SDL_Keycode.SDLK_0: return ImGuiKey.ImGuiKey_0;
                case SDL_Keycode.SDLK_1: return ImGuiKey.ImGuiKey_1;
                case SDL_Keycode.SDLK_2: return ImGuiKey.ImGuiKey_2;
                case SDL_Keycode.SDLK_3: return ImGuiKey.ImGuiKey_3;
                case SDL_Keycode.SDLK_4: return ImGuiKey.ImGuiKey_4;
                case SDL_Keycode.SDLK_5: return ImGuiKey.ImGuiKey_5;
                case SDL_Keycode.SDLK_6: return ImGuiKey.ImGuiKey_6;
                case SDL_Keycode.SDLK_7: return ImGuiKey.ImGuiKey_7;
                case SDL_Keycode.SDLK_8: return ImGuiKey.ImGuiKey_8;
                case SDL_Keycode.SDLK_9: return ImGuiKey.ImGuiKey_9;
                case SDL_Keycode.SDLK_A: return ImGuiKey.ImGuiKey_A;
                case SDL_Keycode.SDLK_B: return ImGuiKey.ImGuiKey_B;
                case SDL_Keycode.SDLK_C: return ImGuiKey.ImGuiKey_C;
                case SDL_Keycode.SDLK_D: return ImGuiKey.ImGuiKey_D;
                case SDL_Keycode.SDLK_E: return ImGuiKey.ImGuiKey_E;
                case SDL_Keycode.SDLK_F: return ImGuiKey.ImGuiKey_F;
                case SDL_Keycode.SDLK_G: return ImGuiKey.ImGuiKey_G;
                case SDL_Keycode.SDLK_H: return ImGuiKey.ImGuiKey_H;
                case SDL_Keycode.SDLK_I: return ImGuiKey.ImGuiKey_I;
                case SDL_Keycode.SDLK_J: return ImGuiKey.ImGuiKey_J;
                case SDL_Keycode.SDLK_K: return ImGuiKey.ImGuiKey_K;
                case SDL_Keycode.SDLK_L: return ImGuiKey.ImGuiKey_L;
                case SDL_Keycode.SDLK_M: return ImGuiKey.ImGuiKey_M;
                case SDL_Keycode.SDLK_N: return ImGuiKey.ImGuiKey_N;
                case SDL_Keycode.SDLK_O: return ImGuiKey.ImGuiKey_O;
                case SDL_Keycode.SDLK_P: return ImGuiKey.ImGuiKey_P;
                case SDL_Keycode.SDLK_Q: return ImGuiKey.ImGuiKey_Q;
                case SDL_Keycode.SDLK_R: return ImGuiKey.ImGuiKey_R;
                case SDL_Keycode.SDLK_S: return ImGuiKey.ImGuiKey_S;
                case SDL_Keycode.SDLK_T: return ImGuiKey.ImGuiKey_T;
                case SDL_Keycode.SDLK_U: return ImGuiKey.ImGuiKey_U;
                case SDL_Keycode.SDLK_V: return ImGuiKey.ImGuiKey_V;
                case SDL_Keycode.SDLK_W: return ImGuiKey.ImGuiKey_W;
                case SDL_Keycode.SDLK_X: return ImGuiKey.ImGuiKey_X;
                case SDL_Keycode.SDLK_Y: return ImGuiKey.ImGuiKey_Y;
                case SDL_Keycode.SDLK_Z: return ImGuiKey.ImGuiKey_Z;
                case SDL_Keycode.SDLK_F1: return ImGuiKey.ImGuiKey_F1;
                case SDL_Keycode.SDLK_F2: return ImGuiKey.ImGuiKey_F2;
                case SDL_Keycode.SDLK_F3: return ImGuiKey.ImGuiKey_F3;
                case SDL_Keycode.SDLK_F4: return ImGuiKey.ImGuiKey_F4;
                case SDL_Keycode.SDLK_F5: return ImGuiKey.ImGuiKey_F5;
                case SDL_Keycode.SDLK_F6: return ImGuiKey.ImGuiKey_F6;
                case SDL_Keycode.SDLK_F7: return ImGuiKey.ImGuiKey_F7;
                case SDL_Keycode.SDLK_F8: return ImGuiKey.ImGuiKey_F8;
                case SDL_Keycode.SDLK_F9: return ImGuiKey.ImGuiKey_F9;
                case SDL_Keycode.SDLK_F10: return ImGuiKey.ImGuiKey_F10;
                case SDL_Keycode.SDLK_F11: return ImGuiKey.ImGuiKey_F11;
                case SDL_Keycode.SDLK_F12: return ImGuiKey.ImGuiKey_F12;
                case SDL_Keycode.SDLK_F13: return ImGuiKey.ImGuiKey_F13;
                case SDL_Keycode.SDLK_F14: return ImGuiKey.ImGuiKey_F14;
                case SDL_Keycode.SDLK_F15: return ImGuiKey.ImGuiKey_F15;
                case SDL_Keycode.SDLK_F16: return ImGuiKey.ImGuiKey_F16;
                case SDL_Keycode.SDLK_F17: return ImGuiKey.ImGuiKey_F17;
                case SDL_Keycode.SDLK_F18: return ImGuiKey.ImGuiKey_F18;
                case SDL_Keycode.SDLK_F19: return ImGuiKey.ImGuiKey_F19;
                case SDL_Keycode.SDLK_F20: return ImGuiKey.ImGuiKey_F20;
                case SDL_Keycode.SDLK_F21: return ImGuiKey.ImGuiKey_F21;
                case SDL_Keycode.SDLK_F22: return ImGuiKey.ImGuiKey_F22;
                case SDL_Keycode.SDLK_F23: return ImGuiKey.ImGuiKey_F23;
                case SDL_Keycode.SDLK_F24: return ImGuiKey.ImGuiKey_F24;
                case SDL_Keycode.SDLK_AC_BACK: return ImGuiKey.ImGuiKey_AppBack;
                case SDL_Keycode.SDLK_AC_FORWARD: return ImGuiKey.ImGuiKey_AppForward;
                default: break;
            }

            // Fallback to scancode
            switch (scancode)
            {
                case SDL_Scancode.SDL_SCANCODE_GRAVE: return ImGuiKey.ImGuiKey_GraveAccent;
                case SDL_Scancode.SDL_SCANCODE_MINUS: return ImGuiKey.ImGuiKey_Minus;
                case SDL_Scancode.SDL_SCANCODE_EQUALS: return ImGuiKey.ImGuiKey_Equal;
                case SDL_Scancode.SDL_SCANCODE_LEFTBRACKET: return ImGuiKey.ImGuiKey_LeftBracket;
                case SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET: return ImGuiKey.ImGuiKey_RightBracket;
                case SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH: return ImGuiKey.ImGuiKey_Oem102;
                case SDL_Scancode.SDL_SCANCODE_BACKSLASH: return ImGuiKey.ImGuiKey_Backslash;
                case SDL_Scancode.SDL_SCANCODE_SEMICOLON: return ImGuiKey.ImGuiKey_Semicolon;
                case SDL_Scancode.SDL_SCANCODE_APOSTROPHE: return ImGuiKey.ImGuiKey_Apostrophe;
                case SDL_Scancode.SDL_SCANCODE_COMMA: return ImGuiKey.ImGuiKey_Comma;
                case SDL_Scancode.SDL_SCANCODE_PERIOD: return ImGuiKey.ImGuiKey_Period;
                case SDL_Scancode.SDL_SCANCODE_SLASH: return ImGuiKey.ImGuiKey_Slash;
                default: break;
            }
            return ImGuiKey.ImGuiKey_None;
        }
        static void ImGui_ImplSDL3_UpdateKeyModifiers(ref ImGuiIO io, SDL_Keymod sdl_key_mods)
        {
            io.AddKeyEvent(ImGuiKey.ImGuiMod_Ctrl, (sdl_key_mods & SDL_Keymod.SDL_KMOD_CTRL) != 0);
            io.AddKeyEvent(ImGuiKey.ImGuiMod_Shift, (sdl_key_mods & SDL_Keymod.SDL_KMOD_SHIFT) != 0);
            io.AddKeyEvent(ImGuiKey.ImGuiMod_Alt, (sdl_key_mods & SDL_Keymod.SDL_KMOD_ALT) != 0);
            io.AddKeyEvent(ImGuiKey.ImGuiMod_Super, (sdl_key_mods & SDL_Keymod.SDL_KMOD_GUI) != 0);
        }
        public static unsafe bool ImGui_ImplSDL3_ProcessEvent(in SDL.SDL_Event ev)
        {
            var io = ImGuiAPI.GetIO();
            switch (ev.type)
            {
                case (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.wheel.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        Vector2 mouse_pos = new Vector2((float)ev.motion.x, (float)ev.motion.y);
                        if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable)!=0)
                        {
                            int window_x, window_y;
                            SDL.SDL3.SDL_GetWindowPosition(SDL.SDL3.SDL_GetWindowFromID(ev.motion.windowID), &window_x, &window_y);
                            mouse_pos.X += window_x;
                            mouse_pos.Y += window_y;
                        }
                        io.AddMouseSourceEvent(ev.motion.which == SDL.SDL3.SDL_TOUCH_MOUSEID ? ImGuiMouseSource.ImGuiMouseSource_TouchScreen : ImGuiMouseSource.ImGuiMouseSource_Mouse);
                        io.AddMousePosEvent(mouse_pos.X, mouse_pos.Y);
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.wheel.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        //IMGUI_DEBUG_LOG("wheel %.2f %.2f, precise %.2f %.2f\n", (float)event->wheel.x, (float)event->wheel.y, event->wheel.preciseX, event->wheel.preciseY);
                        float wheel_x = -ev.wheel.x;
                        float wheel_y = ev.wheel.y;
                        io.AddMouseSourceEvent(ev.wheel.which == SDL.SDL3.SDL_TOUCH_MOUSEID ? ImGuiMouseSource.ImGuiMouseSource_TouchScreen : ImGuiMouseSource.ImGuiMouseSource_Mouse);
                        io.AddMouseWheelEvent(wheel_x, wheel_y);
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                case (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.wheel.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        int mouse_button = -1;
                        if (ev.button.button == SDL3.SDL_BUTTON_LEFT) { mouse_button = 0; }
                        if (ev.button.button == SDL3.SDL_BUTTON_RIGHT) { mouse_button = 1; }
                        if (ev.button.button == SDL3.SDL_BUTTON_MIDDLE) { mouse_button = 2; }
                        if (ev.button.button == SDL3.SDL_BUTTON_X1) { mouse_button = 3; }
                        if (ev.button.button == SDL3.SDL_BUTTON_X2) { mouse_button = 4; }
                        if (mouse_button == -1)
                            break;
                        io.AddMouseSourceEvent(ev.button.which == SDL3.SDL_TOUCH_MOUSEID ? ImGuiMouseSource.ImGuiMouseSource_TouchScreen : ImGuiMouseSource.ImGuiMouseSource_Mouse);
                        io.AddMouseButtonEvent(mouse_button, (ev.type == (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN));
                        var bd = TtEngine.Instance.GfxDevice.SlateApplication.ImGuiData;
                        bd.MouseButtonsDown = (ev.type == (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN) ? (bd.MouseButtonsDown | (1 << mouse_button)) : (bd.MouseButtonsDown & ~(1 << mouse_button));
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_WINDOW_MOUSE_ENTER:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.window.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        var bd = TtEngine.Instance.GfxDevice.SlateApplication.ImGuiData;
                        bd.MouseWindowID = ev.window.windowID;
                        bd.MousePendingLeaveFrame = 0;
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.window.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        var bd = TtEngine.Instance.GfxDevice.SlateApplication.ImGuiData;
                        bd.MousePendingLeaveFrame = ImGuiAPI.GetFrameCount() + 1;
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
                case (int)SDL.SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.window.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        //IMGUI_DEBUG_LOG("%s: windowId %d, viewport: %08X\n", (event->type == SDL_EVENT_WINDOW_FOCUS_GAINED) ? "SDL_EVENT_WINDOW_FOCUS_GAINED" : "SDL_WINDOWEVENT_FOCUS_LOST", event->window.windowID, viewport ? viewport->ID : 0);
                        io.AddFocusEvent(ev.type == (int)SDL.SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED);
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_TEXT_INPUT:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.wheel.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        var text = ev.text;
                        var pText = (sbyte*)text.text;
                        io.AddInputCharactersUTF8(pText);
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN:
                case (int)SDL.SDL_EventType.SDL_EVENT_KEY_UP:
                    {
                        //if (ImGui_ImplSDL3_GetViewportForWindowID(ev.wheel.windowID) == (ImGuiViewport*)0)
                        //    return false;
                        ImGui_ImplSDL3_UpdateKeyModifiers(ref io, ev.key.mod);
                        var key = ImGui_ImplSDL3_KeyEventToImGuiKey(ev.key.key, ev.key.scancode);
                        io.AddKeyEvent(key, ev.type == (int)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN);
                        io.SetKeyEventNativeData(key, (int)ev.key.key, (int)ev.key.scancode, (int)ev.key.scancode); // To support legacy indexing (<1.87 user code). Legacy backend uses SDLK_*** as indices to IsKeyXXX() functions.
                        return true;
                    }
                //case (int)SDL.SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                //case (int)SDL.SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                //    {
                //        var window = SDL.SDL3.SDL_GetWindowFromID(ev.window.windowID);
                //        var display = SDL.SDL3.SDL_GetDisplayForWindow(window);
                //        ImGuiViewport* viewport = ImGuiAPI.FindViewportByPlatformHandle(window);
                //        if(viewport != null)
                //        {
                //            var dpi = SDL.SDL3.SDL_GetDisplayContentScale(display);
                //            viewport->DpiScale = dpi;
                //        }
                //        return true;
                //    }
                // Multi-viewport support
                default:
                    {
                        if (ev.type >= (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_FIRST && ev.type <= (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_LAST)
                        {
                            var window_event = ev.window.type;
                            if (window_event == SDL.SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED ||
                                window_event == SDL.SDL_EventType.SDL_EVENT_WINDOW_MOVED ||
                                window_event == SDL.SDL_EventType.SDL_EVENT_WINDOW_RESIZED)
                            {
                                ImGuiViewport* viewport = ImGuiAPI.FindViewportByPlatformHandle((void*)SDL.SDL3.SDL_GetWindowFromID(ev.window.windowID));
                                if (viewport != (ImGuiViewport*)0)
                                {
                                    if (window_event == SDL.SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED)
                                    {
                                        viewport->PlatformRequestClose = true;
                                        if ((IntPtr)viewport->PlatformUserData != IntPtr.Zero)
                                        {
                                            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
                                            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
                                            myWindow.IsClosed = true;
                                        }
                                        //var closeEvent = new SDL.SDL_Event();
                                        //closeEvent.type = SDL.SDL_EventType.SDL_QUIT;
                                        //SDL.SDL_PushEvent(ref closeEvent);
                                    }
                                    if (window_event == SDL.SDL_EventType.SDL_EVENT_WINDOW_MOVED)
                                        viewport->PlatformRequestMove = true;
                                    if (window_event == SDL.SDL_EventType.SDL_EVENT_WINDOW_RESIZED)
                                        viewport->PlatformRequestResize = true;
                                    return true;
                                }
                            }
                        }
                    }
                    break;
            }
            return false;
        }
        public static unsafe ImGuiViewport* ImGui_ImplSDL3_GetViewportForWindowID(SDL_WindowID window_id)
        {
            return ImGuiAPI.FindViewportByPlatformHandle((void*)(uint)window_id);
        }
        public static unsafe void ImGui_ImplSDL3_UpdateMouseData(ImGuiIO io)
        {
            var bd = TtEngine.Instance.GfxDevice.SlateApplication.ImGuiData;
            // We forward mouse input when hovered or captured (via SDL_EVENT_MOUSE_MOTION) or when focused (below)
#if true
            // - SDL_CaptureMouse() let the OS know e.g. that our drags can extend outside of parent boundaries (we want updated position) and shouldn't trigger other operations outside.
            // - Debuggers under Linux tends to leave captured mouse on break, which may be very inconvenient, so to mitigate the issue we wait until mouse has moved to begin capture.
            if (bd.MouseCanUseCapture)
            {
                bool want_capture = false;
                for (ImGuiMouseButton_ button_n = (ImGuiMouseButton_)0; button_n < ImGuiMouseButton_.ImGuiMouseButton_COUNT && !want_capture; button_n++)
                {
                    if (ImGuiAPI.IsMouseDragging(button_n, 1.0f))
                        want_capture = true;
                }
                SDL.SDL3.SDL_CaptureMouse(want_capture);
            }

            SDL.SDL_Window* focused_window = SDL.SDL3.SDL_GetKeyboardFocus();
            var viewport = ImGui_ImplSDL3_GetViewportForWindowID(SDL.SDL3.SDL_GetWindowID(focused_window));
            bool is_app_focused = (focused_window!=(SDL.SDL_Window*)0 && (bd.ImGuiMainWindow == focused_window || viewport != (ImGuiViewport*)0));
#else
            //SDL_Window* focused_window = bd->Window;
            //const bool is_app_focused = (SDL_GetWindowFlags(bd->Window) & SDL_WINDOW_INPUT_FOCUS) != 0; // SDL 2.0.3 and non-windowed systems: single-viewport only
#endif
            if (is_app_focused)
            {
                // (Optional) Set OS mouse position from Dear ImGui if requested (rarely used, only when io.ConfigNavMoveSetMousePos is enabled by user)
                if (io.WantSetMousePos)
                {
#if true
                    if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable)!=0)
                        SDL.SDL3.SDL_WarpMouseGlobal(io.MousePos.X, io.MousePos.Y);
                    else
#endif
                        SDL.SDL3.SDL_WarpMouseInWindow(bd.ImGuiMainWindow, io.MousePos.X, io.MousePos.Y);
                }

                // (Optional) Fallback to provide mouse position when focused (SDL_EVENT_MOUSE_MOTION already provides this when hovered or captured)
                bool is_relative_mouse_mode = SDL.SDL3.SDL_GetWindowRelativeMouseMode(bd.ImGuiMainWindow);
                if (bd.MouseCanUseGlobalState && bd.MouseButtonsDown == 0 && !is_relative_mouse_mode)
                {
                    // Single-viewport mode: mouse position in client window coordinates (io.MousePos is (0,0) when the mouse is on the upper-left corner of the app window)
                    // Multi-viewport mode: mouse position in OS absolute coordinates (io.MousePos is (0,0) when the mouse is on the upper-left of the primary monitor)
                    float mouse_x, mouse_y;
                    int window_x, window_y;
                    SDL.SDL3.SDL_GetGlobalMouseState(&mouse_x, &mouse_y);
                    if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) == 0)
                    {
                        SDL.SDL3.SDL_GetWindowPosition(focused_window, &window_x, &window_y);
                        mouse_x -= window_x;
                        mouse_y -= window_y;
                    }
                    io.AddMousePosEvent((float)mouse_x, (float)mouse_y);
                }
            }

            // (Optional) When using multiple viewports: call io.AddMouseViewportEvent() with the viewport the OS mouse cursor is hovering.
            // If ImGuiBackendFlags_HasMouseHoveredViewport is not set by the backend, Dear imGui will ignore this field and infer the information using its flawed heuristic.
            // - [!] SDL backend does NOT correctly ignore viewports with the _NoInputs flag.
            //       Some backend are not able to handle that correctly. If a backend report an hovered viewport that has the _NoInputs flag (e.g. when dragging a window
            //       for docking, the viewport has the _NoInputs flag in order to allow us to find the viewport under), then Dear ImGui is forced to ignore the value reported
            //       by the backend, and use its flawed heuristic to guess the viewport behind.
            // - [X] SDL backend correctly reports this regardless of another viewport behind focused and dragged from (we need this to find a useful drag and drop target).
            if ((io.BackendFlags & ImGuiBackendFlags_.ImGuiBackendFlags_HasMouseHoveredViewport) != 0)
            {
                uint mouse_viewport_id = 0;
                ImGuiViewport* mouse_viewport = ImGui_ImplSDL3_GetViewportForWindowID(bd.MouseWindowID);
                if (mouse_viewport!=(ImGuiViewport*)0)
                    mouse_viewport_id = mouse_viewport->ID;
                io.AddMouseViewportEvent(mouse_viewport_id);
            }
        }
        public static unsafe void ImGui_ImplSDL3_UpdateMouseCursor(ImGuiIO io)
        {
            if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_NoMouseCursorChange)!=0)
                return;
            var bd = TtEngine.Instance.GfxDevice.SlateApplication.ImGuiData;

            ImGuiMouseCursor_ imgui_cursor = ImGuiAPI.GetMouseCursor();
            if (io.MouseDrawCursor || imgui_cursor == ImGuiMouseCursor_.ImGuiMouseCursor_None)
            {
                // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                SDL.SDL3.SDL_HideCursor();
            }
            else
            {
                // Show OS mouse cursor
                SDL.SDL_Cursor* expected_cursor = (bd.MouseCursors[(int)imgui_cursor]!=(SDL_Cursor*)0) ? bd.MouseCursors[(int)imgui_cursor] : bd.MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_Arrow];
                if (bd.MouseLastCursor != expected_cursor)
                {
                    SDL.SDL3.SDL_SetCursor(expected_cursor); // SDL function doesn't have an early out (see #6113)
                    bd.MouseLastCursor = expected_cursor;
                }
                SDL.SDL3.SDL_ShowCursor();
            }
        }
        #endregion

        #region CallBack
        #region SDL
        public static unsafe ImGuiIO.FDelegate_GetClipboardTextFn ImGui_ImplSDL2_GetClipboardText = ImGui_ImplSDL3_GetClipboardText_Impl;
        static unsafe sbyte* ImGui_ImplSDL3_GetClipboardText_Impl(void* dummy)
        {
            var bd = TtEngine.Instance.GfxDevice.SlateApplication.ImGuiData;
            if (bd.ClipboardTextData != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(bd.ClipboardTextData);
            }
            var text = SDL.SDL3.SDL_GetClipboardText();
            bd.ClipboardTextData = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(text);
            return (sbyte*)bd.ClipboardTextData.ToPointer();
        }
        public static unsafe ImGuiIO.FDelegate_SetClipboardTextFn ImGui_ImplSDL2_SetClipboardText = ImGui_ImplSDL3_SetClipboardText_Impl;
        static unsafe void ImGui_ImplSDL3_SetClipboardText_Impl(void* dummy, sbyte* text)
        {
            SDL.SDL3.SDL_SetClipboardText(System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)text));
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_CreateWindow ImGui_ImplSDL2_CreateWindow = ImGui_ImplSDL2_CreateWindow_Impl;
        static unsafe void ImGui_ImplSDL2_CreateWindow_Impl(ImGuiViewport* viewport)
        {
            SDL.SDL_WindowFlags sdl_flags = 0;

            sdl_flags |= (SDL.SDL_WindowFlags)SDL.SDL3.SDL_GetWindowFlags(TtEngine.Instance.GfxDevice.SlateApplication.NativeWindow.WindowSDL) & SDL.SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
            sdl_flags |= ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoDecoration) != 0) ? SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS : 0;
            sdl_flags |= ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoDecoration) != 0) ? 0 : SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            sdl_flags |= (viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_TopMost) != 0 ? SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP : 0;
            var myWindow = new Graphics.Pipeline.TtPresentWindow();
            myWindow.IsCreatedByImGui = true;
            myWindow.CreateNativeWindow("No Title Yet", (int)viewport->Pos.X, (int)viewport->Pos.Y, (int)viewport->Size.X, (int)viewport->Size.Y, (uint)sdl_flags).ToPointer();
            viewport->PlatformUserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(myWindow)).ToPointer();
            viewport->PlatformHandle = myWindow.Window.ToPointer();
            viewport->PlatformHandleRaw = myWindow.HWindow.ToPointer();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_DestroyWindow ImGui_ImplSDL2_DestroyWindow = ImGui_ImplSDL2_DestroyWindow_Impl;
        static unsafe void ImGui_ImplSDL2_DestroyWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            if (myWindow.IsCreatedByImGui == false)
            {
                var closeEvent = new SDL.SDL_Event();
                closeEvent.type = (uint)SDL.SDL_EventType.SDL_EVENT_QUIT;
                SDL.SDL3.SDL_PushEvent(&closeEvent);
                return;
            }
            myWindow.Cleanup();
            viewport->PlatformUserData = IntPtr.Zero.ToPointer();
            viewport->PlatformHandle = null;

            gcHandle.Free();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_ShowWindow ImGui_ImplSDL2_ShowWindow = ImGui_ImplSDL2_ShowWindow_Impl;
        unsafe static void ImGui_ImplSDL2_ShowWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;

#if PWindow
            var hwnd = viewport->PlatformHandleRaw;

            // SDL hack: Hide icon from task bar
            // Note: SDL 2.0.6+ has a SDL_WINDOW_SKIP_TASKBAR flag which is supported under Windows but the way it create the window breaks our seamless transition.
            if ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoTaskBarIcon) != 0)
            {
                //LONG ex_style = ::GetWindowLong(hwnd, GWL_EXSTYLE);
                //ex_style &= ~WS_EX_APPWINDOW;
                //ex_style |= WS_EX_TOOLWINDOW;
                //::SetWindowLong(hwnd, GWL_EXSTYLE, ex_style);
            }

            // SDL hack: SDL always activate/focus windows :/
            if ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoFocusOnAppearing) != 0)
            {
                //::ShowWindow(hwnd, SW_SHOWNA);
                myWindow.ShowNativeWindow();
                return;
            }
#endif

            myWindow.ShowNativeWindow();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowPos ImGui_ImplSDL2_SetWindowPos = ImGui_ImplSDL2_SetWindowPos_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowPos_Impl(ImGuiViewport* viewport, Vector2 pos)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            myWindow.SetWindowPosition((int)pos.X, (int)pos.Y);
        }
        static unsafe ImGuiPlatformIO.FDelegate_Platform_GetWindowPos ImGui_ImplSDL2_GetWindowPos = ImGui_ImplSDL2_GetWindowPos_Impl;
        unsafe static Vector2 ImGui_ImplSDL2_GetWindowPos_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return new Vector2(0);
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            return myWindow.GetWindowPosition();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowSize ImGui_ImplSDL2_SetWindowSize = ImGui_ImplSDL2_SetWindowSize_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowSize_Impl(ImGuiViewport* viewport, Vector2 size)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            myWindow.SetWindowSize((int)size.X, (int)size.Y);
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_GetWindowSize ImGui_ImplSDL2_GetWindowSize = ImGui_ImplSDL2_GetWindowSize_Impl;
        unsafe static Vector2 ImGui_ImplSDL2_GetWindowSize_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return new Vector2(0);
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            return myWindow.GetWindowSize();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowFocus ImGui_ImplSDL2_SetWindowFocus = ImGui_ImplSDL2_SetWindowFocus_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowFocus_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            myWindow.SetWindowFocus();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_GetWindowFocus ImGui_ImplSDL2_GetWindowFocus = ImGui_ImplSDL2_GetWindowFocus_Impl;
        unsafe static bool ImGui_ImplSDL2_GetWindowFocus_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return false;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            return myWindow.GetWindowFocus();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_GetWindowMinimized ImGui_ImplSDL2_GetWindowMinimized = ImGui_ImplSDL2_GetWindowMinimized_Impl;
        unsafe static bool ImGui_ImplSDL2_GetWindowMinimized_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return false;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            return myWindow.GetWindowMinimized();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowTitle ImGui_ImplSDL2_SetWindowTitle = ImGui_ImplSDL2_SetWindowTitle_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowTitle_Impl(ImGuiViewport* viewport, sbyte* title)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            myWindow.SetWindowTitle(System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)title));
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_RenderWindow ImGui_ImplSDL2_RenderWindow = ImGui_ImplSDL2_RenderWindow_Impl;
        unsafe static void ImGui_ImplSDL2_RenderWindow_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SwapBuffers ImGui_ImplSDL2_SwapBuffers = ImGui_ImplSDL2_SwapBuffers_Impl;
        unsafe static void ImGui_ImplSDL2_SwapBuffers_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowAlpha ImGui_ImplSDL2_SetWindowAlpha = ImGui_ImplSDL2_SetWindowAlpha_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowAlpha_Impl(ImGuiViewport* viewport, float alpha)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
            myWindow.SetWindowOpacity(alpha);
        }
        #endregion
        #region Renderer
        public class ViewportData : IDisposable
        {
            public Graphics.Pipeline.TtPresentWindow PresentWindow;

            public EGui.TtImDrawDataRHI DrawData = new EGui.TtImDrawDataRHI();

            public void Dispose()
            {
                //PresentWindow?.Cleanup();
                PresentWindow = null;
                DrawData?.Dispose();
                DrawData = null;
            }
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_CreateWindow ImGui_Renderer_CreateWindow = ImGui_Renderer_CreateWindow_Impl;
        unsafe static void ImGui_Renderer_CreateWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;

            //Create SwapChain
            var vpData = new ViewportData();
            vpData.PresentWindow = myWindow;
            viewport->RendererUserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(vpData)).ToPointer();


            vpData.PresentWindow.InitSwapChain(TtEngine.Instance.GfxDevice.RenderContext);
            vpData.DrawData.InitializeGraphics(vpData.PresentWindow.GetSwapchainFormat(), vpData.PresentWindow.GetSwapchainDSFormat());
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_DestroyWindow ImGui_Renderer_DestroyWindow = ImGui_Renderer_DestroyWindow_Impl;
        unsafe static void ImGui_Renderer_DestroyWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;
            vpData.Dispose();
            gcHandle.Free();
            viewport->RendererUserData = IntPtr.Zero.ToPointer();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_SetWindowSize ImGui_Renderer_SetWindowSize = ImGui_Renderer_SetWindowSize_Impl;
        unsafe static void ImGui_Renderer_SetWindowSize_Impl(ImGuiViewport* viewport, Vector2 size)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;
            vpData.PresentWindow.OnResize(size.X, size.Y);
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_RenderWindow ImGui_Renderer_RenderWindow = ImGui_Renderer_RenderWindow_Impl;
        unsafe static void ImGui_Renderer_RenderWindow_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;

            //ImGui_ImplOpenGL3_RenderDrawData(viewport->DrawData);
            var draw_data = viewport->DrawData;
            EGui.TtImDrawDataRHI.RenderImDrawData(ref *draw_data, vpData.PresentWindow, vpData.DrawData);
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_SwapBuffers ImGui_Renderer_SwapBuffers = ImGui_Renderer_SwapBuffers_Impl;
        unsafe static void ImGui_Renderer_SwapBuffers_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;

            vpData.PresentWindow.SwapChain.Present(0, 0);
        }
        #endregion
        #endregion
    }
    public partial class TtSlateApplication
    {
        public unsafe class TtImGuiData : IDisposable
        {
            public unsafe SDL.SDL_Window* ImGuiMainWindow;
            public SDL.SDL_WindowID MouseWindowID;
            public int MouseButtonsDown = 0;
            public int MousePendingLeaveFrame;
            public bool MouseCanUseGlobalState = true;
            public bool MouseCanUseCapture = true;
            public IntPtr ClipboardTextData;
            public SDL_Cursor*[] MouseCursors = new SDL_Cursor*[(int)ImGuiMouseCursor_.ImGuiMouseCursor_COUNT];
            public SDL_Cursor* MouseLastCursor;

            public void Dispose()
            {
                FreeMouseCursors();
                if (ClipboardTextData != IntPtr.Zero)
                {
                    SDL.SDL3.SDL_free(ClipboardTextData);
                    ClipboardTextData = IntPtr.Zero;
                }
            }
            public void LoadMouseCursors()
            {
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_Arrow] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_DEFAULT);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_TextInput] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_TEXT);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeAll] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_E_RESIZE);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNS] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NS_RESIZE);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeEW] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_EW_RESIZE);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNESW] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NESW_RESIZE);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNWSE] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NWSE_RESIZE);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_Hand] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_MOVE);
                MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_NotAllowed] = SDL.SDL3.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NOT_ALLOWED);

                var sdl_backend = SDL.SDL3.SDL_GetCurrentVideoDriver();
                string[] global_mouse_whitelist = new string[] { "windows", "cocoa", "x11", "DIVE", "VMAN" };

                MouseCanUseGlobalState = false;
                for (int n = 0; n < global_mouse_whitelist.Length; n++)
                {
                    if (sdl_backend == global_mouse_whitelist[n])
                    {
                        MouseCanUseGlobalState = true;
                    }
                }

                // Check and store if we are on Wayland
                //g_MouseCanUseGlobalState = strncmp(SDL_GetCurrentVideoDriver(), "wayland", 7) != 0;
            }
            public void FreeMouseCursors()
            {
                for (var i = 0; i<MouseCursors.Length; i++)
                {
                    if (MouseCursors[i] != (SDL_Cursor*)0)
                    {
                        SDL.SDL3.SDL_DestroyCursor(MouseCursors[i]);
                        MouseCursors[i] = (SDL_Cursor*)0;
                    }
                }
            }
        }
        public TtImGuiData ImGuiData { get; private set; } = new TtImGuiData();
        public bool CreateNativeWindow(TtEngine engine, string title, int x, int y, int w, int h)
        {
            NativeWindow = new TtPresentWindow();
            SDL.SDL_WindowFlags sdl_flags = 0;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;
            if (engine.Config.SupportMultWindows)
            {
                sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
                sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            }
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            return NativeWindow.CreateNativeWindow(title, x, y, w, h, (uint)sdl_flags) != IntPtr.Zero;
        }
        public unsafe void ImGui_Init_SDL(ImGuiIO io, IntPtr window)
        {
            // Setup backend capabilities flags
            io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_HasMouseCursors;       // We can honor GetMouseCursor() values (optional)
            io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_HasSetMousePos;        // We can honor io.WantSetMousePos requests (optional, rarely used)
            io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_PlatformHasViewports;  // We can create multi-viewports on the Platform side (optional)

            io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_RendererHasVtxOffset;
            io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_RendererHasViewports;

            io.BackendPlatformName = "imgui_impl_sdl";

            io.SetClipboardTextFn = TtDockWindowSDL.ImGui_ImplSDL2_SetClipboardText;
            io.GetClipboardTextFn = TtDockWindowSDL.ImGui_ImplSDL2_GetClipboardText;
            io.ClipboardUserData = (void*)0;

            io.MouseDoubleClickTime = 0.5f;
            io.ConfigViewportsNoDecoration = false;

            //// Load mouse cursors
            ImGuiData.LoadMouseCursors();

            ImGuiViewport* main_viewport = ImGuiAPI.GetMainViewport();
            main_viewport->PlatformHandle = (void*)window;

            ImGuiData.ImGuiMainWindow = (SDL.SDL_Window*)window.ToPointer();

            main_viewport->PlatformHandleRaw = TtNativeWindow.GetWindowHandle((SDL.SDL_Window*)window.ToPointer()).ToPointer();

            // Update monitors
            TtDockWindowSDL.ImGui_ImplSDL3_UpdateMonitors();

            if (((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) != 0)
                && ((io.BackendFlags & ImGuiBackendFlags_.ImGuiBackendFlags_PlatformHasViewports) != 0))
            {
                TtDockWindowSDL.ImGui_ImplSDL3_InitPlatformInterface();
            }

            //main_viewport->PlatformUserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this.NativeWindow)).ToPointer();
        }
    }
}


namespace EngineNS.Bricks.Input.Device.Mouse
{
    public partial class TtMouse
    {
        partial void OnSetShowCursor()
        {
            unsafe
            {
                if (bShowCursor && !SDL.SDL3.SDL_CursorVisible())
                {
                    SDL.SDL3.SDL_ShowCursor();
                }

                if(!bShowCursor && SDL.SDL3.SDL_CursorVisible())
                {
                    SDL.SDL3.SDL_HideCursor();
                }
            }
        }
        partial void WarpMouseInWindow(int x, int y)
        {
            unsafe
            {
                
                SDL.SDL3.SDL_WarpMouseInWindow(null, x, y);
            }
        }
    }
}

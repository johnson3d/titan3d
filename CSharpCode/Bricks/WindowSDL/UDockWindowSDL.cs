using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.EGui
{
    public class UDockWindowSDL
    {
        #region SDL
        public static unsafe void ImGui_ImplSDL2_UpdateMonitors(IntPtr window)
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
                SDL.SDL3.SDL_GetDisplayUsableBounds(dspl[n], &r);
                monitor.WorkPos = new Vector2((float)r.x, (float)r.y);
                monitor.WorkSize = new Vector2((float)r.w, (float)r.h);
                var dpi = SDL.SDL3.SDL_GetDisplayContentScale(dspl[n]);
                monitor.DpiScale = dpi;
                ImGuiAPI.PlatformIO_Monitor_PushBack(platform_io, monitor);
            }
        }
        public static unsafe void ImGui_ImplSDL2_InitPlatformInterface()
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
        public static unsafe ImGuiIO.FDelegate_GetClipboardTextFn ImGui_ImplSDL2_GetClipboardText = ImGui_ImplSDL2_GetClipboardText_Impl;
        static unsafe sbyte* ImGui_ImplSDL2_GetClipboardText_Impl(void* dummy)
        {
            if (g_ClipboardTextData != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(g_ClipboardTextData);
            }
            var text = SDL.SDL3.SDL_GetClipboardText();
            g_ClipboardTextData = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(text);
            return (sbyte*)g_ClipboardTextData.ToPointer();
        }
        unsafe static IntPtr g_ClipboardTextData;
        public static unsafe ImGuiIO.FDelegate_SetClipboardTextFn ImGui_ImplSDL2_SetClipboardText = ImGui_ImplSDL2_SetClipboardText_Impl;
        static unsafe void ImGui_ImplSDL2_SetClipboardText_Impl(void* dummy, sbyte* text)
        {
            SDL.SDL3.SDL_SetClipboardText(System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)text));
        }
        static bool[] g_MousePressed = new bool[]{ false, false, false };
        public static unsafe bool ImGui_ImplSDL2_ProcessEvent(in SDL.SDL_Event ev)
        {
            var io = ImGuiAPI.GetIO();
            switch (ev.type)
            {
                case (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    {
                        if (ev.wheel.x > 0) io.MouseWheelH += 1;
                        if (ev.wheel.x < 0) io.MouseWheelH -= 1;
                        if (ev.wheel.y > 0) io.MouseWheel += 1;
                        if (ev.wheel.y < 0) io.MouseWheel -= 1;
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                    {
                        if (ev.button.button == (byte)SDL.SDLButton.SDL_BUTTON_LEFT)
                            g_MousePressed[0] = true;
                        if (ev.button.button == (byte)SDL.SDLButton.SDL_BUTTON_RIGHT) 
                            g_MousePressed[1] = true;
                        if (ev.button.button == (byte)SDL.SDLButton.SDL_BUTTON_MIDDLE) 
                            g_MousePressed[2] = true;
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_TEXT_INPUT:
                    {
                        var text = ev.text;
                        var pText = (sbyte*)text.text;
                        io.AddInputCharactersUTF8(pText);
                        return true;
                    }
                case (int)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN:
                case (int)SDL.SDL_EventType.SDL_EVENT_KEY_UP:
                    {
                        var key = (int)ev.key.scancode;
                        bool* keysDown = (bool*)&io.UnsafeAsLayout->KeysDown;
                        //IM_ASSERT(key >= 0 && key<IM_ARRAYSIZE(io.KeysDown));
                        keysDown[key] = (ev.type == (uint)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN);
                        io.KeyShift = ((SDL.SDL3.SDL_GetModState() & SDL.SDL_Keymod.SDL_KMOD_SHIFT) != 0);
                        io.KeyCtrl = ((SDL.SDL3.SDL_GetModState() & SDL.SDL_Keymod.SDL_KMOD_CTRL) != 0);
                        io.KeyAlt = ((SDL.SDL3.SDL_GetModState() & SDL.SDL_Keymod.SDL_KMOD_ALT) != 0);
                        io.KeySuper = false;
                        //io.KeySuper = ((SDL_GetModState() & KMOD_GUI) != 0);
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
        #endregion

        #region CallBack
        #region SDL
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
            
            public UImDrawDataRHI DrawData = new UImDrawDataRHI();

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
            UImDrawDataRHI.RenderImDrawData(ref *draw_data, vpData.PresentWindow, vpData.DrawData);
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
}

namespace EngineNS.Bricks.Input.Device.Mouse
{
    public partial class UMouse
    {
        partial void OnSetShowCursor()
        {
            if (bShowCursor)
                SDL.SDL3.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_FALSE);
            else
                SDL.SDL3.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_TRUE);
        }
        partial void WarpMouseInWindow(IntPtr window, int x, int y)
        {
            unsafe
            {
                SDL.SDL3.SDL_WarpMouseInWindow((SDL.SDL_Window*)window.ToPointer(), x, y);
            }
        }
    }
}

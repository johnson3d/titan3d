using EngineNS.Graphics.Pipeline;
using SDL2;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS
{
    partial class TtEngine
    {
        public EPlatformType CurrentPlatform
        {
            get
            {
                return EPlatformType.PLTF_Windows;
            }
        }
    }

    public partial class TtNativeWindow
    {
        public enum DwmWindowAttribute : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation,
            PassiveUpdateMode,
            UseHostBackdropBrush,
            UseImmersiveDarkMode = 20,
            WindowCornerPreference = 33,
            BorderColor,
            CaptionColor,
            TextColor,
            VisibleFrameBorderThickness,
            SystemBackdropType,
            Last
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

        ~TtNativeWindow()
        {
            Cleanup();
        }
        public IntPtr Window;
        public IntPtr HWindow
        {
            get
            {
                SDL.SDL_SysWMinfo wmInfo = new SDL.SDL_SysWMinfo();
                SDL.SDL_GetVersion(out wmInfo.version);
                SDL.SDL_GetWindowWMInfo(Window, ref wmInfo);
                return wmInfo.info.win.window;
            }
        }
        public bool IsMinimized
        {
            get
            {
                return (SDL.SDL_GetWindowFlags(Window) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0;
            }
        }
        public static bool IsInputFocus(IntPtr handle)
        {
            var flags = SDL.SDL_GetWindowFlags(handle);
            return (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;
        }
        HashSet<IEventProcessor> mEventProcessors;
        public void RegEventProcessor(IEventProcessor proc)
        {
            if (mEventProcessors == null)
                mEventProcessors = new HashSet<IEventProcessor>();
            if (mEventProcessors.Contains(proc))
                return;
            mEventProcessors.Add(proc);
        }
        public void UnregEventProcessor(IEventProcessor proc)
        {
            if (mEventProcessors == null)
                return;
            mEventProcessors.Remove(proc);
        }
        private IntPtr ThisHandle;
        public Vector2 WindowSize
        {
            get
            {
                int w, h;
                SDL.SDL_GetWindowSize(Window, out w, out h);
                return new Vector2((float)w, (float)h);
            }
        }

        public virtual void Cleanup()
        {
            if (ThisHandle != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ThisHandle);
                SDL.SDL_SetWindowData(Window, "UPresentWindow", IntPtr.Zero);
                handle.Free();
                ThisHandle = IntPtr.Zero;
            }
            if (Window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(Window);
                Window = IntPtr.Zero;
            }
        }

        public virtual async System.Threading.Tasks.Task<bool> Initialize(string title, int x, int y, int w, int h)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            SDL.SDL_WindowFlags sdl_flags = 0;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            //sdl_flags |= (viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_TopMost) != 0 ? SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP : 0;

            Window = SDL.SDL_CreateWindow(title, 0, 0, 1, 1, sdl_flags);
            //Window = SDL.SDL_CreateWindow(title, x, y, w, h, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            ThisHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this));
            SDL.SDL_SetWindowData(Window, "UNativeWindow", ThisHandle);

            return true;
        }
        public IntPtr CreateNativeWindow(string title, int x, int y, int w, int h, uint sdl_flags)
        {
            Window = SDL.SDL_CreateWindow(title, x, y, w, h, (SDL.SDL_WindowFlags)sdl_flags);
            ThisHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this));
            SDL.SDL_SetWindowData(Window, "UNativeWindow", ThisHandle);
            SDL.SDL_SysWMinfo wmInfo = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetWindowWMInfo(Window, ref wmInfo);
            var hwnd = wmInfo.info.win.window;
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, (DwmWindowAttribute)20, ref darkMode, sizeof(int));
            return Window;
        }
        public void ShowNativeWindow()
        {
            SDL.SDL_ShowWindow(Window);
        }
        public void SetWindowPosition(int x, int y)
        {
            SDL.SDL_SetWindowPosition(Window, x, y);
        }
        public Vector2 GetWindowPosition()
        {
            int x = 0, y = 0;
            SDL.SDL_GetWindowPosition(Window, out x, out y);
            return new Vector2((float)x, (float)y);
        }
        public void SetWindowSize(int x, int y)
        {
            SDL.SDL_SetWindowSize(Window, x, y);
        }
        public Vector2 GetWindowSize()
        {
            int x = 0, y = 0;
            SDL.SDL_GetWindowSize(Window, out x, out y);
            return new Vector2((float)x, (float)y);
        }
        public void SetWindowFocus()
        {
            SDL.SDL_RaiseWindow(Window);
        }
        public bool GetWindowFocus()
        {
            return (SDL.SDL_GetWindowFlags(Window) & (int)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;
        }
        public bool GetWindowMinimized()
        {
            return (SDL.SDL_GetWindowFlags(Window) & (int)SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0;
        }
        public void SetWindowTitle(string title)
        {
            SDL.SDL_SetWindowTitle(Window, title);
        }
        public void SetWindowOpacity(float alpha)
        {
            SDL.SDL_SetWindowOpacity(Window, alpha);
        }
        public virtual unsafe void OnEvent(in Bricks.Input.Event e)
        {
            switch (e.Window.WindowEventID)
            {
                case Bricks.Input.WindowEventID.WINDOWEVENT_SIZE_CHANGED:
                    {
                        OnResize(e.Window.Data1, e.Window.Data2);
                    }
                    break;
                case Bricks.Input.WindowEventID.WINDOWEVENT_CLOSE:
                    {

                    }
                    break;
            }

            if (mEventProcessors != null)
            {
                foreach (var i in mEventProcessors)
                {
                    i.OnEvent(in e);
                }
            }
        }
        public unsafe virtual void OnResize(float x, float y)
        {

        }
    }

    public partial class TtEventProcessorManager
    {
        partial void OnTickWindow(in Bricks.Input.Event evt)
        {
            var targetWindow = SDL.SDL_GetWindowFromID(evt.Window.WindowID);

            if (targetWindow != IntPtr.Zero)
            {
                var pHandle = SDL.SDL_GetWindowData(targetWindow, "UNativeWindow");
                if (pHandle != IntPtr.Zero)
                {
                    var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(pHandle);
                    var presentWindow = handle.Target as Graphics.Pipeline.UPresentWindow;
                    if (presentWindow != null)
                    {
                        presentWindow.OnEvent(in evt);
                    }
                }
            }
        }
    }
}

namespace EngineNS
{
    public partial class TtSlateApplication
    {
        public bool CreateNativeWindow(TtEngine engine, string title, int x, int y, int w, int h)
        {
            NativeWindow = new UPresentWindow();
            SDL.SDL_WindowFlags sdl_flags = 0;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
            if (engine.Config.SupportMultWindows)
            {
                sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
                sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            }
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            return NativeWindow.CreateNativeWindow(title, x, y, w, h, (uint)sdl_flags) != IntPtr.Zero;
        }
        partial void LoadMouseCursors()
        {
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_Arrow] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_TextInput] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeAll] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNS] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeEW] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNESW] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNWSE] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_Hand] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND);
            MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_NotAllowed] = SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO);

            var sdl_backend = SDL.SDL_GetCurrentVideoDriver();
            string[] global_mouse_whitelist = new string[] { "windows", "cocoa", "x11", "DIVE", "VMAN" };

            for (int n = 0; n < global_mouse_whitelist.Length; n++)
            {
                if (sdl_backend == global_mouse_whitelist[n])
                {
                    g_MouseCanUseGlobalState = true;
                }
            }
        }
        partial void FreeMouseCursors()
        {
            foreach (var i in MouseCursors)
            {
                if (i != IntPtr.Zero)
                {
                    SDL.SDL_FreeCursor(i);
                }
            }
        }
        partial void MapKeys(ImGuiIO io)
        {
            unsafe
            {
                var pKeyMap = (int*)&io.UnsafeAsLayout->KeyMap;
                // Keyboard mapping. ImGui will use those indices to peek into the io.KeysDown[] array.
                pKeyMap[(int)ImGuiKey.ImGuiKey_Tab] = (int)SDL.SDL_Scancode.SDL_SCANCODE_TAB;
                pKeyMap[(int)ImGuiKey.ImGuiKey_LeftArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT;
                pKeyMap[(int)ImGuiKey.ImGuiKey_RightArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT;
                pKeyMap[(int)ImGuiKey.ImGuiKey_UpArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_UP;
                pKeyMap[(int)ImGuiKey.ImGuiKey_DownArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN;
                pKeyMap[(int)ImGuiKey.ImGuiKey_PageUp] = (int)SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP;
                pKeyMap[(int)ImGuiKey.ImGuiKey_PageDown] = (int)SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Home] = (int)SDL.SDL_Scancode.SDL_SCANCODE_HOME;
                pKeyMap[(int)ImGuiKey.ImGuiKey_End] = (int)SDL.SDL_Scancode.SDL_SCANCODE_END;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Insert] = (int)SDL.SDL_Scancode.SDL_SCANCODE_INSERT;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Delete] = (int)SDL.SDL_Scancode.SDL_SCANCODE_DELETE;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Backspace] = (int)SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Space] = (int)SDL.SDL_Scancode.SDL_SCANCODE_SPACE;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Enter] = (int)SDL.SDL_Scancode.SDL_SCANCODE_RETURN;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Escape] = (int)SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE;
                pKeyMap[(int)ImGuiKey.ImGuiKey_KeyPadEnter] = (int)SDL.SDL_Scancode.SDL_SCANCODE_KP_ENTER;
                pKeyMap[(int)ImGuiKey.ImGuiKey_A] = (int)SDL.SDL_Scancode.SDL_SCANCODE_A;
                pKeyMap[(int)ImGuiKey.ImGuiKey_C] = (int)SDL.SDL_Scancode.SDL_SCANCODE_C;
                pKeyMap[(int)ImGuiKey.ImGuiKey_V] = (int)SDL.SDL_Scancode.SDL_SCANCODE_V;
                pKeyMap[(int)ImGuiKey.ImGuiKey_X] = (int)SDL.SDL_Scancode.SDL_SCANCODE_X;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Y] = (int)SDL.SDL_Scancode.SDL_SCANCODE_Y;
                pKeyMap[(int)ImGuiKey.ImGuiKey_Z] = (int)SDL.SDL_Scancode.SDL_SCANCODE_Z;
            }
        }

        partial void ImGui_UpdateMousePosAndButtons(ImGuiIO io)
        {
            unsafe
            {
                // Set OS mouse position if requested (rarely used, only when ImGuiConfigFlags_NavEnableSetMousePos is enabled by user)
                if (io.WantSetMousePos)
                    SDL.SDL_WarpMouseInWindow(NativeWindow.Window, (int)io.MousePos.X, (int)io.MousePos.Y);
                else
                    io.MousePos = new Vector2(-float.MaxValue, -float.MaxValue);

                int mx, my;
                var mouse_buttons = SDL.SDL_GetMouseState(out mx, out my);
                var mouseDown = (bool*)&io.UnsafeAsLayout->MouseDown;
                mouseDown[0] = (MousePressed[0] || (mouse_buttons & SDL.SDL_BUTTON(SDL.SDL_BUTTON_LEFT)) != 0);
                mouseDown[1] = (MousePressed[1] || (mouse_buttons & SDL.SDL_BUTTON(SDL.SDL_BUTTON_RIGHT)) != 0);
                mouseDown[2] = (MousePressed[2] || (mouse_buttons & SDL.SDL_BUTTON(SDL.SDL_BUTTON_MIDDLE)) != 0);
                MousePressed[0] = MousePressed[1] = MousePressed[2] = false;

                if (g_MouseCanUseGlobalState)
                {
                    // SDL 2.0.4 and later has SDL_GetGlobalMouseState() and SDL_CaptureMouse()
                    int mouse_x_global, mouse_y_global;
                    SDL.SDL_GetGlobalMouseState(out mouse_x_global, out mouse_y_global);
                    TtEngine.Instance.InputSystem.Mouse.GlobalMouseX = mouse_x_global;
                    TtEngine.Instance.InputSystem.Mouse.GlobalMouseY = mouse_y_global;

                    if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) != 0)
                    {
                        // Multi-viewport mode: mouse position in OS absolute coordinates (io.MousePos is (0,0) when the mouse is on the upper-left of the primary monitor)
                        IntPtr focused_window = SDL.SDL_GetKeyboardFocus();
                        if (focused_window != IntPtr.Zero)
                            if ((IntPtr)ImGuiAPI.FindViewportByPlatformHandle((void*)focused_window) != IntPtr.Zero)
                                io.MousePos = new Vector2((float)mouse_x_global, (float)mouse_y_global);
                    }
                    else
                    {
                        // Single-viewport mode: mouse position in client window coordinatesio.MousePos is (0,0) when the mouse is on the upper-left corner of the app window)
                        if ((SDL.SDL_GetWindowFlags(NativeWindow.Window) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0)
                        {
                            int window_x, window_y;
                            SDL.SDL_GetWindowPosition(NativeWindow.Window, out window_x, out window_y);
                            io.MousePos = new Vector2((float)(mouse_x_global - window_x), (float)(mouse_y_global - window_y));
                        }
                    }
                }
                else
                {
                    if ((SDL.SDL_GetWindowFlags(NativeWindow.Window) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0)
                        io.MousePos = new Vector2((float)mx, (float)my);
                }

                if ((SDL.SDL_GetWindowFlags(NativeWindow.Window) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0)
                    io.MousePos = new Vector2((float)mx, (float)my);
            }
        }
        partial void ImGui_UpdateMouseCursor(ImGuiIO io)
        {
            unsafe
            {
                if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_NoMouseCursorChange) != 0)
                    return;

                var imgui_cursor = ImGuiAPI.GetMouseCursor();
                if (io.MouseDrawCursor || imgui_cursor == ImGuiMouseCursor_.ImGuiMouseCursor_None)
                {
                    // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                    SDL.SDL_ShowCursor((int)SDL.SDL_bool.SDL_FALSE);
                }
                else
                {
                    // Show OS mouse cursor
                    SDL.SDL_SetCursor((MouseCursors[(int)imgui_cursor] != IntPtr.Zero) ? MouseCursors[(int)imgui_cursor] : MouseCursors[(int)ImGuiMouseCursor_.ImGuiMouseCursor_Arrow]);
                    SDL.SDL_ShowCursor((int)SDL.SDL_bool.SDL_TRUE);
                }
            }
        }
        partial void ImGui_Init(ImGuiIO io, IntPtr window)
        {
            unsafe
            {
                // Setup backend capabilities flags
                io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_HasMouseCursors;       // We can honor GetMouseCursor() values (optional)
                io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_HasSetMousePos;        // We can honor io.WantSetMousePos requests (optional, rarely used)
                io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_PlatformHasViewports;  // We can create multi-viewports on the Platform side (optional)

                io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_RendererHasVtxOffset;
                io.UnsafeAsLayout->BackendFlags |= ImGuiBackendFlags_.ImGuiBackendFlags_RendererHasViewports;

                io.BackendPlatformName = "imgui_impl_sdl";

                io.SetClipboardTextFn = EGui.UDockWindowSDL.ImGui_ImplSDL2_SetClipboardText;
                io.GetClipboardTextFn = EGui.UDockWindowSDL.ImGui_ImplSDL2_GetClipboardText;
                io.ClipboardUserData = (void*)0;

                io.MouseDoubleClickTime = 0.5f;
                io.ConfigViewportsNoDecoration = false;

                var pKeyMap = (int*)&io.UnsafeAsLayout->KeyMap;
                // Keyboard mapping. ImGui will use those indices to peek into the io.KeysDown[] array.
                MapKeys(io);

                //// Load mouse cursors
                LoadMouseCursors();

                // Check and store if we are on Wayland
                //g_MouseCanUseGlobalState = strncmp(SDL_GetCurrentVideoDriver(), "wayland", 7) != 0;

                ImGuiViewport* main_viewport = ImGuiAPI.GetMainViewport();
                main_viewport->PlatformHandle = (void*)window;

                //#if PWindow
                SDL.SDL_SysWMinfo wmInfo = new SDL.SDL_SysWMinfo();
                SDL.SDL_VERSION(out wmInfo.version);
                if (SDL.SDL_GetWindowWMInfo(window, ref wmInfo) != SDL.SDL_bool.SDL_FALSE)
                    main_viewport->PlatformHandleRaw = wmInfo.info.win.window.ToPointer();
                //#endif

                // Update monitors
                EGui.UDockWindowSDL.ImGui_ImplSDL2_UpdateMonitors();

                if (((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) != 0)
                    && ((io.BackendFlags & ImGuiBackendFlags_.ImGuiBackendFlags_PlatformHasViewports) != 0))
                {
                    EGui.UDockWindowSDL.ImGui_ImplSDL2_InitPlatformInterface();
                }

                //main_viewport->PlatformUserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this.NativeWindow)).ToPointer();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDL2;

namespace EngineNS.Graphics.Pipeline
{
    public interface IGuiModule
    {
        void OnDraw();
    }
    public interface IRootForm : IGuiModule
    {
        bool Visible { get; set; }
        uint DockId { get; set; }
        ImGuiCond_ DockCond { get; set; }
        Task<bool> Initialize();
        void Cleanup();
    }
    public class URootFormManager
    {
        internal static URootFormManager Insance = new URootFormManager();
        private URootFormManager()
        {

        }
        #region RootForms
        private List<WeakReference<IRootForm>> AppendForms { get; } = new List<WeakReference<IRootForm>>();
        private List<WeakReference<IRootForm>> RootForms { get; } = new List<WeakReference<IRootForm>>();
        public void RegRootForm(IRootForm form)
        {
            foreach (var i in AppendForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                        return;
                }
            }
            foreach (var i in RootForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                        return;
                }
            }
            AppendForms.Add(new WeakReference<IRootForm>(form));
        }
        public void UnregRootForm(IRootForm form)
        {
            foreach (var i in AppendForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                    {
                        AppendForms.Remove(i);
                        break;
                    }
                }
            }
            foreach (var i in RootForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                    {
                        AppendForms.Remove(i);
                        break;
                    }
                }
            }
        }
        public void DrawRootForms()
        {
            if (AppendForms.Count > 0)
            {
                RootForms.AddRange(AppendForms);
                AppendForms.Clear();
            }

            for (int i = 0; i < RootForms.Count; i++)
            {
                IRootForm rf;
                if (RootForms[i].TryGetTarget(out rf))
                {
                    if (rf.Visible == false)
                        continue;
                }
                else
                {
                    RootForms.RemoveAt(i);
                    i--;
                }

                rf.OnDraw();
            }
        }
        public void ClearRootForms()
        {
            for (int i = 0; i < AppendForms.Count; i++)
            {
                IRootForm rf;
                if (AppendForms[i].TryGetTarget(out rf))
                {
                    rf.Cleanup();
                }
            }
            AppendForms.Clear();
            for (int i = 0; i < RootForms.Count; i++)
            {
                IRootForm rf;
                if (RootForms[i].TryGetTarget(out rf))
                {
                    rf.Cleanup();
                }
            }
            RootForms.Clear();
        }
        #endregion
    }

    public class USlateApplication
    {
        public UPresentWindow NativeWindow;
        //public virtual EGui.Slate.UWorldViewportSlate GetWorldViewportSlate() { return null; }

        public IntPtr mImGuiContext;
        public EGui.UDockWindowSDL.UImDrawDataRHI mDrawData = new EGui.UDockWindowSDL.UImDrawDataRHI();
        
        bool[] MousePressed = new bool[3] { false, false, false };
        IntPtr[] MouseCursors = new IntPtr[(int)ImGuiMouseCursor_.ImGuiMouseCursor_COUNT];

        public bool CreateNativeWindow(UEngine engine, string title, int x, int y, int w, int h)
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
            return NativeWindow.CreateNativeWindow(title, x, y, w, h, sdl_flags) != IntPtr.Zero;
        }
        public virtual async Task<bool> InitializeApplication(NxRHI.UGpuDevice rc, RName rpName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            NativeWindow.InitSwapChain(rc);
            unsafe
            {
                mDrawData.InitializeGraphics(NativeWindow.GetSwapchainFormat(), NativeWindow.GetSwapchainDSFormat());
                
                mImGuiContext = (IntPtr)ImGuiAPI.CreateContext(new ImFontAtlas((void*)0));
                ImGuiAPI.SetCurrentContext(mImGuiContext.ToPointer());
                UEngine.Instance.GfxDevice.SlateRenderer.RecreateFontDeviceTexture();

                var io = ImGuiAPI.GetIO();

                ImGuiConfigFlags_ configFlags = ImGuiConfigFlags_.ImGuiConfigFlags_None;
                configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_NavEnableKeyboard;       // Enable Keyboard Controls
                //configFlags |= ImGuiConfigFlags_NavEnableGamepad;      // Enable Gamepad Controls
                configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_DockingEnable;           // Enable Docking
                if (UEngine.Instance.Config.SupportMultWindows)
                    configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable;         // Enable Multi-Viewport / Platform Windows
                //io.ConfigViewportsNoAutoMerge = true;
                //io.ConfigViewportsNoTaskBarIcon = true;
                io.ConfigFlags = configFlags;

                ImGuiAPI.StyleColorsDark((ImGuiStyle*)0);

                var style = ImGuiAPI.GetStyle();
                if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) != 0)
                {
                    style->WindowRounding = 0.0f;
                    style->Colors[(int)ImGuiCol_.ImGuiCol_WindowBg].W = 1.0f;
                }

                ImGui_ImplSDL2_Init(ImGuiAPI.GetIO(), NativeWindow.Window);

                SetPerFrameImGuiData(1f / 60f);
            }
            return true;
        }
        public virtual void Cleanup()
        {
            mDrawData.Cleanup();
            foreach(var i in MouseCursors)
            {
                if (i != IntPtr.Zero)
                {
                    SDL.SDL_FreeCursor(i);
                }
            }
            MouseCursors = new IntPtr[(int)ImGuiMouseCursor_.ImGuiMouseCursor_COUNT];

            NativeWindow?.Cleanup();
            NativeWindow = null;
            //unsafe
            //{
            //    ImGuiViewport* main_viewport = ImGuiAPI.GetMainViewport();
            //    if ((IntPtr)main_viewport->m_PlatformUserData != IntPtr.Zero)
            //    {
            //        var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)main_viewport->PlatformUserData);
            //        System.Diagnostics.Debug.Assert(gcHandle.Target == this);

            //        main_viewport->PlatformUserData = IntPtr.Zero.ToPointer();
            //        main_viewport->PlatformHandle = IntPtr.Zero.ToPointer();

            //        gcHandle.Free();
            //    }
            //}
        }
        
        #region ImGUI & SDL
        
        bool g_MouseCanUseGlobalState = false;
        private unsafe bool ImGui_ImplSDL2_Init(ImGuiIO io, IntPtr window)
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

            var pKeyMap = (int*)&io.UnsafeAsLayout->KeyMap;
            // Keyboard mapping. ImGui will use those indices to peek into the io.KeysDown[] array.
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Tab] = (int)SDL.SDL_Scancode.SDL_SCANCODE_TAB;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_LeftArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_RightArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_UpArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_UP;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_DownArrow] = (int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_PageUp] = (int)SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_PageDown] = (int)SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Home] = (int)SDL.SDL_Scancode.SDL_SCANCODE_HOME;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_End] = (int)SDL.SDL_Scancode.SDL_SCANCODE_END;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Insert] = (int)SDL.SDL_Scancode.SDL_SCANCODE_INSERT;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Delete] = (int)SDL.SDL_Scancode.SDL_SCANCODE_DELETE;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Backspace] = (int)SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Space] = (int)SDL.SDL_Scancode.SDL_SCANCODE_SPACE;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Enter] = (int)SDL.SDL_Scancode.SDL_SCANCODE_RETURN;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Escape] = (int)SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_KeyPadEnter] = (int)SDL.SDL_Scancode.SDL_SCANCODE_KP_ENTER;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_A] = (int)SDL.SDL_Scancode.SDL_SCANCODE_A;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_C] = (int)SDL.SDL_Scancode.SDL_SCANCODE_C;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_V] = (int)SDL.SDL_Scancode.SDL_SCANCODE_V;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_X] = (int)SDL.SDL_Scancode.SDL_SCANCODE_X;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Y] = (int)SDL.SDL_Scancode.SDL_SCANCODE_Y;
            pKeyMap[(int)ImGuiKey_.ImGuiKey_Z] = (int)SDL.SDL_Scancode.SDL_SCANCODE_Z;


            //// Load mouse cursors
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

            return true;
        }
        private unsafe void ImGui_ImplSDL2_UpdateMousePosAndButtons(ImGuiIO io)
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
        private unsafe void ImGui_ImplSDL2_UpdateMouseCursor(ImGuiIO io)
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
        public virtual void OnResize(float x, float y)
        {
            NativeWindow.OnResize(x, y);
        }
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            var io = ImGuiAPI.GetIO();

            int w, h;
            SDL.SDL_GetWindowSize(NativeWindow.Window, out w, out h);
            if ((SDL.SDL_GetWindowFlags(NativeWindow.Window) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0)
            {
                w = h = 0;
            }
            io.DisplaySize = new Vector2((float)w, (float)h);
            if (w > 0 && h > 0)
            {
                io.DisplayFramebufferScale = new Vector2(1, 1);
            }
            else
            {
                io.DisplayFramebufferScale = new Vector2(1, 1);
            }
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }
        private void Update(float deltaSeconds)
        {
            SetPerFrameImGuiData(deltaSeconds);

            ImGui_ImplSDL2_UpdateMousePosAndButtons(ImGuiAPI.GetIO());
            ImGui_ImplSDL2_UpdateMouseCursor(ImGuiAPI.GetIO());

            ImGuiAPI.NewFrame();
        }
        #endregion

        public unsafe virtual void OnDrawSlate()
        {
            if (mImGuiContext == IntPtr.Zero)
                return;
            ImGuiAPI.SetCurrentContext(mImGuiContext.ToPointer());

            Update((UEngine.Instance.ElapseTickCount) / 1000.0f);

            OnDrawUI();

            {
                ImGuiAPI.Render();
                unsafe
                {
                    if (UEngine.Instance.Config.SupportMultWindows == false)
                    {
                        var draw_data = ImGuiAPI.GetDrawData();
                        EGui.UDockWindowSDL.RenderImDrawData(ref *draw_data, NativeWindow, mDrawData);
                        NativeWindow.SwapChain.Present(0, 0);
                    }
                }

                // Update and Render additional Platform Windows
                var io = ImGuiAPI.GetIO();
                if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) != 0)
                {
                    ImGuiAPI.UpdatePlatformWindows();
                    ImGuiAPI.RenderPlatformWindowsDefault((void*)0, (void*)0);
                }
            }
        }
        protected unsafe virtual void OnDrawUI()
        {
            var visible = true;
            Vector2 sz = new Vector2(300, 600);
            ImGuiAPI.SetNextWindowSize(in sz, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.Begin("Slate", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {

            }
            ImGuiAPI.End();
        }
    }

    public class USlateAppBase : USlateApplication, ITickable
    {
        public override void Cleanup()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            base.Cleanup();
        }
        public override async System.Threading.Tasks.Task<bool> InitializeApplication(NxRHI.UGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);
            
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        protected bool Visible = true;
        protected unsafe override void OnDrawUI()
        {
            if (Visible == false)
            {
                var num = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                if (num == 1)
                {//只剩下被特意隐藏的主Viewport了
                    UEngine.Instance.PostQuitMessage();
                }
                return;
            }
            Vector2 sz = new Vector2(300, 600);
            ImGuiAPI.SetNextWindowSize(in sz, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (ImGuiAPI.Begin("Slate", ref Visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {

            }
            ImGuiAPI.End();

            UEngine.RootFormManager.DrawRootForms();
        }
        #region Tick
        public virtual void TickLogic(int ellapse)
        {

        }
        public virtual void TickRender(int ellapse)
        {

        }
        public virtual void TickSync(int ellapse)
        {
            //OnDrawSlate();
        }
        #endregion
    }
}

namespace EngineNS 
{ 
    partial class UEngine
    {
        public static Graphics.Pipeline.URootFormManager RootFormManager 
        { 
            get
            {
                return Graphics.Pipeline.URootFormManager.Insance;
            }
        }
    }
}
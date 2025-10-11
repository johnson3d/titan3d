using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL;

namespace EngineNS
{
    //https://github.com/libsdl-org/SDL/blob/main/docs/README-migration.md
    public partial class TtNativeWindow
    {
        public static SDL.SDL_PropertiesID PropertiesID_WindowData;

        public string WindowName { get; set; }

        ~TtNativeWindow()
        {
            Cleanup();
        }
        public IntPtr Window;
        public unsafe SDL.SDL_Window* WindowSDL
        {
            get
            {
                return (SDL.SDL_Window*)Window.ToPointer();
            }
        }
        public unsafe SDL.SDL_WindowID WindowID
        {
            get
            {
                return SDL.SDL3.SDL_GetWindowID(WindowSDL);
            }
        }
        public unsafe static IntPtr GetWindowHandle(SDL.SDL_Window* WinSDL)
        {
            return SDL.SDL3.SDL_GetPointerProperty(SDL.SDL3.SDL_GetWindowProperties(WinSDL), SDL.SDL3.SDL_PROP_WINDOW_WIN32_HWND_POINTER, 0);
        }
        public unsafe IntPtr HWindow
        {
            get
            {
                return GetWindowHandle(WindowSDL);
            }
        }
        public unsafe bool IsMinimized
        {
            get
            {
                return (SDL.SDL3.SDL_GetWindowFlags(WindowSDL) & SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0;
            }
        }
        public unsafe static bool IsInputFocus(IntPtr handle)
        {
            var flags = SDL.SDL3.SDL_GetWindowFlags((SDL.SDL_Window*)handle.ToPointer());
            return (flags & SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;
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
        public unsafe Vector2 WindowSize
        {
            get
            {
                int w, h;
                SDL.SDL3.SDL_GetWindowSize(WindowSDL, &w, &h);
                return new Vector2((float)w, (float)h);
            }
        }

        public unsafe virtual void Cleanup()
        {
            if (ThisHandle != IntPtr.Zero)
            {
                WindowName = $"NativeWindow_{Window}";
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ThisHandle);
                SDL.SDL3.SDL_SetPointerProperty(PropertiesID_WindowData, this.WindowID.ToString(), IntPtr.Zero);
                handle.Free();
                ThisHandle = IntPtr.Zero;
            }
            if (Window != IntPtr.Zero)
            {
                SDL.SDL3.SDL_StopTextInput(WindowSDL);
                SDL.SDL3.SDL_DestroyWindow(WindowSDL);
                Window = IntPtr.Zero;
            }
        }

        public virtual async Thread.Async.TtTask<bool> Initialize(string title, int x, int y, int w, int h)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            SDL.SDL_WindowFlags sdl_flags = 0;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            //sdl_flags |= (viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_TopMost) != 0 ? SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP : 0;

            System.Diagnostics.Debug.Assert(PropertiesID_WindowData != 0);
            PropertiesID_WindowData = SDL.SDL3.SDL_CreateProperties();
            unsafe
            {
                Window = (IntPtr)SDL.SDL3.SDL_CreateWindow(title, w, h, sdl_flags);
                WindowName = $"NativeWindow_{WindowID}";
                //Window = SDL.SDL_CreateWindow(title, x, y, w, h, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
                ThisHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this));
                //SDL.SDL3.SDL_SetWindowData(WindowSDL, "UNativeWindow", ThisHandle);
                SDL.SDL3.SDL_SetPointerProperty(PropertiesID_WindowData, this.WindowID.ToString(), ThisHandle);
                SDL.SDL3.SDL_StartTextInput(WindowSDL);
            }

            return true;
        }
        public unsafe IntPtr CreateNativeWindow(string title, int x, int y, int w, int h, uint sdl_flags)
        {
            Window = (IntPtr)SDL.SDL3.SDL_CreateWindow(title, w, h, (SDL.SDL_WindowFlags)sdl_flags);
            SDL.SDL3.SDL_SetWindowPosition(WindowSDL, x, y);
            ThisHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this));
            //SDL.SDL3.SDL_SetWindowData(Window, "UNativeWindow", ThisHandle);
            System.Diagnostics.Debug.Assert(PropertiesID_WindowData != 0);
            WindowName = $"NativeWindow_{WindowID}";
            SDL.SDL3.SDL_SetPointerProperty(PropertiesID_WindowData, this.WindowID.ToString(), ThisHandle);
            SDL.SDL3.SDL_StartTextInput(WindowSDL);
            var hwnd = GetWindowHandle(WindowSDL);
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, (DwmWindowAttribute)20, ref darkMode, sizeof(int));
            return Window;
        }
        public unsafe void ShowNativeWindow()
        {
            SDL.SDL3.SDL_ShowWindow(WindowSDL);
        }
        public unsafe void SetWindowPosition(int x, int y)
        {
            SDL.SDL3.SDL_SetWindowPosition(WindowSDL, x, y);
        }
        public unsafe Vector2 GetWindowPosition()
        {
            int x = 0, y = 0;
            SDL.SDL3.SDL_GetWindowPosition(WindowSDL, &x, &y);
            return new Vector2((float)x, (float)y);
        }
        public unsafe void SetWindowSize(int x, int y)
        {
            SDL.SDL3.SDL_SetWindowSize(WindowSDL, x, y);
        }
        public unsafe Vector2 GetWindowSize()
        {
            int x = 0, y = 0;
            SDL.SDL3.SDL_GetWindowSize(WindowSDL, &x, &y);
            return new Vector2((float)x, (float)y);
        }
        public unsafe void SetWindowFocus()
        {
            SDL.SDL3.SDL_RaiseWindow(WindowSDL);
        }
        public unsafe bool GetWindowFocus()
        {
            return (SDL.SDL3.SDL_GetWindowFlags(WindowSDL) & SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;
        }
        public unsafe bool GetWindowMinimized()
        {
            return (SDL.SDL3.SDL_GetWindowFlags(WindowSDL) & SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0;
        }
        public unsafe void SetWindowTitle(string title)
        {
            SDL.SDL3.SDL_SetWindowTitle(WindowSDL, title);
        }
        public unsafe void SetWindowOpacity(float alpha)
        {
            SDL.SDL3.SDL_SetWindowOpacity(WindowSDL, alpha);
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

    public interface IEventProcessor
    {
        unsafe bool OnEvent(in Bricks.Input.Event e);
    }
    public partial class TtEventProcessorManager
    {
        public List<IEventProcessor> Processors { get; } = new List<IEventProcessor>();
        private List<IEventProcessor> WaitRemoved { get; } = new List<IEventProcessor>();

        public void RegProcessor(IEventProcessor ep)
        {
            lock (this)
            {
                if (Processors.Contains(ep))
                    return;
                Processors.Add(ep);
            }
        }
        public void UnregProcessor(IEventProcessor ep)
        {
            lock (this)
            {
                WaitRemoved.Add(ep);
            }
        }
        public void TickEvent(in Bricks.Input.Event evt)
        {
            if (evt.Type == Bricks.Input.EventType.KEYDOWN && evt.Keyboard.Keysym.Scancode == Bricks.Input.Scancode.SCANCODE_F1)
            {
                unsafe
                {
                    IRenderDocTool.GetInstance().SetGpuDevice(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject);
                    //IRenderDocTool.GetInstance().SetActiveWindow(HWindow.ToPointer());
                    TtEngine.Instance.GfxDevice.RenderSwapQueue.CaptureRenderDocFrame = true;
                }
            }

            OnTickWindow(in evt);
            
            for (int i = 0; i < Processors.Count; i++)
            {
                try
                {
                    if (Processors[i].OnEvent(in evt) == false)
                        break;
                }
                catch (Exception e)
                {
                    Profiler.Log.WriteException(e);
                }
            }
            lock (this)
            {
                foreach (var i in WaitRemoved)
                {
                    Processors.Remove(i);
                }
                WaitRemoved.Clear();
            }
        }

        private unsafe void OnTickWindow(in Bricks.Input.Event evt)
        {
            var targetWindow = SDL.SDL3.SDL_GetWindowFromID((SDL.SDL_WindowID)evt.Window.WindowID);

            if (targetWindow != IntPtr.Zero.ToPointer())
            {
                var pHandle = SDL.SDL3.SDL_GetPointerProperty(TtNativeWindow.PropertiesID_WindowData, evt.Window.WindowID.ToString(), IntPtr.Zero);
                //var pHandle = SDL.SDL3.SDL_GetWindowData(targetWindow, "UNativeWindow");
                if (pHandle != IntPtr.Zero)
                {
                    var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(pHandle);
                    var presentWindow = handle.Target as Graphics.Pipeline.TtPresentWindow;
                    if (presentWindow != null)
                    {
                        presentWindow.OnEvent(in evt);
                    }
                }
            }
        }
    }
}

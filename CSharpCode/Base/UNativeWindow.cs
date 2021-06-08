using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS
{
    public class UNativeWindow
    {
        ~UNativeWindow()
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
            await Thread.AsyncDummyClass.DummyFunc();

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
        public IntPtr CreateNativeWindow(string title, int x, int y, int w, int h, SDL.SDL_WindowFlags sdl_flags)
        {
            Window = SDL.SDL_CreateWindow(title, x, y, w, h, sdl_flags);
            ThisHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this));
            SDL.SDL_SetWindowData(Window, "UNativeWindow", ThisHandle);
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
        public virtual void OnEvent(ref SDL.SDL_Event e)
        {
            switch (e.window.windowEvent)
            {
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                    {
                        OnResize(e.window.data1, e.window.data2);
                    }
                    break;
            }

            if (mEventProcessors != null)
            {
                foreach (var i in mEventProcessors)
                {
                    i.OnEvent(ref e);
                }
            }
        }
        public unsafe virtual void OnResize(float x, float y)
        {
            
        }
    }

    public interface IEventProcessor
    {
        bool OnEvent(ref SDL2.SDL.SDL_Event e);
    }
    public class UEventProcessorManager
    {
        public List<IEventProcessor> Processors { get; } = new List<IEventProcessor>();
        private List<IEventProcessor> WaitRemoved { get; } = new List<IEventProcessor>();
        public bool[] Keyboards = new bool[(int)SDL.SDL_Scancode.SDL_NUM_SCANCODES];
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
        public int Tick(UEngine engine)
        {
            SDL.SDL_Event evt;
            while (SDL.SDL_PollEvent(out evt) != 0)
            {
                unsafe
                {
                    if (ImGuiAPI.GetCurrentContext() != (void*)0)
                        EGui.UDockWindowSDL.ImGui_ImplSDL2_ProcessEvent(ref evt);
                }

                if (evt.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    return -1;
                }
                else if(evt.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    Keyboards[(int)evt.key.keysym.scancode] = true;
                }
                else if (evt.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    Keyboards[(int)evt.key.keysym.scancode] = false;
                }

                var targetWindow = SDL.SDL_GetWindowFromID(evt.window.windowID);

                if (targetWindow != IntPtr.Zero)
                {
                    var pHandle = SDL.SDL_GetWindowData(targetWindow, "UNativeWindow");
                    if (pHandle != IntPtr.Zero)
                    {
                        var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(pHandle);
                        var presentWindow = handle.Target as Graphics.Pipeline.UPresentWindow;
                        if (presentWindow != null)
                        {
                            presentWindow.OnEvent(ref evt);
                        }
                    }
                }
                for (int i = 0; i < Processors.Count; i++)
                {
                    if (Processors[i].OnEvent(ref evt) == false)
                        break;
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
            return 0;
        }
    }
}

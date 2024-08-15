using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    partial class TtEngine
    {
        public EPlatformType CurrentPlatform
        {
            get
            {
                return EPlatformType.PLTF_Android;
            }
        }
    }
    public partial class UNativeWindow
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
                return IntPtr.Zero;
            }
        }
        public bool IsMinimized
        {
            get
            {
                return true;
            }
        }
        public static bool IsInputFocus(IntPtr handle)
        {
            return false;
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
                return Vector2.Zero;
            }
        }

        public virtual void Cleanup()
        {
            if (ThisHandle != IntPtr.Zero)
            {
                ThisHandle = IntPtr.Zero;
            }
            if (Window != IntPtr.Zero)
            {
                Window = IntPtr.Zero;
            }
        }

        public virtual async System.Threading.Tasks.Task<bool> Initialize(string title, int x, int y, int w, int h)
        {
            return true;
        }
        public IntPtr CreateNativeWindow(string title, int x, int y, int w, int h, uint sdl_flags)
        {
            return Window;
        }
        public void ShowNativeWindow()
        {
            
        }
        public void SetWindowPosition(int x, int y)
        {
            
        }
        public Vector2 GetWindowPosition()
        {
            int x = 0, y = 0;
            
            return new Vector2((float)x, (float)y);
        }
        public void SetWindowSize(int x, int y)
        {
            
        }
        public Vector2 GetWindowSize()
        {
            int x = 0, y = 0;
            
            return new Vector2((float)x, (float)y);
        }
        public void SetWindowFocus()
        {
            
        }
        public bool GetWindowFocus()
        {
            return false;
        }
        public bool GetWindowMinimized()
        {
            return false;
        }
        public void SetWindowTitle(string title)
        {
            
        }
        public void SetWindowOpacity(float alpha)
        {
            
        }
        public virtual void OnEvent(in Bricks.Input.Event e)
        {
            //switch (e.Window.WindowEventID)
            //{
            //    //case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
            //    //    {
            //    //        OnResize(e.window.data1, e.window.data2);
            //    //    }
            //    //    break;
            //}

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

    public partial class USlateApplication
    {
        public bool CreateNativeWindow(TtEngine engine, string title, int x, int y, int w, int h)
        {
            return false;
        }
    }
}




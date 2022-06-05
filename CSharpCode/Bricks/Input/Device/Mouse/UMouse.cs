using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.Device.Mouse
{
    public class UMouse : IInputDevice
    {
        bool bShowCursor = true;
        public bool ShowCursor 
        { 
            get=>bShowCursor; 
            set
            {
                bShowCursor = value;
                if (bShowCursor)
                    SDL2.SDL.SDL_SetRelativeMouseMode(SDL2.SDL.SDL_bool.SDL_FALSE);
                else
                    SDL2.SDL.SDL_SetRelativeMouseMode(SDL2.SDL.SDL_bool.SDL_TRUE);

            }
        }

        public void Tick()
        {
            if(!ShowCursor)
            {
                var windowPos = UEngine.Instance.GameInstance.WorldViewportSlate.ViewportPos;
                var windowSize = UEngine.Instance.GameInstance.WorldViewportSlate.ClientSize;
                SDL2.SDL.SDL_WarpMouseInWindow(IntPtr.Zero, (int)windowSize.X / 2, (int)(windowSize.Y) / 2);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input.Device.Mouse
{
    public partial class UMouse : IInputDevice
    {
        bool bShowCursor = true;
        public bool ShowCursor 
        { 
            get=>bShowCursor; 
            set
            {
                bShowCursor = value;

                OnSetShowCursor();
            }
        }
        partial void OnSetShowCursor();

        public int MouseX;
        public int MouseY;

        public void Tick()
        {
            if(!ShowCursor)
            {
                if (UEngine.Instance.GameInstance != null)
                {
                    var windowPos = UEngine.Instance.GameInstance.WorldViewportSlate.ViewportPos;
                    var windowSize = UEngine.Instance.GameInstance.WorldViewportSlate.ClientSize;
                    WarpMouseInWindow(IntPtr.Zero, (int)windowSize.X / 2, (int)(windowSize.Y) / 2);
                }
            }
        }
        partial void WarpMouseInWindow(IntPtr window, int x, int y);
    }
}

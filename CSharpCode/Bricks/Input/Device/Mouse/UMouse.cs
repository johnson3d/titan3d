using Org.BouncyCastle.Asn1.Crmf;
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
        byte mOldMouseButtons = 0;
        byte mMouseButtons = 0;

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

        bool mMouseKeyStateDirty = false;
        public void MouseKeyStateDirtyProcess() 
        {
            if (mMouseKeyStateDirty)
            {
                mOldMouseButtons = mMouseButtons;
                mMouseKeyStateDirty = false;
            }
        }
        public void UpdateMouseState(Event evt)
        {
            if (evt.Type == EventType.MOUSEBUTTONDOWN)
            {
                //MouseX = evt.MouseButton.X;
                //MouseY = evt.MouseButton.Y;
                mOldMouseButtons = mMouseButtons;
                mMouseButtons |= (byte)(1 << evt.MouseButton.Button);
                mMouseKeyStateDirty = true;
            }
            else if (evt.Type == EventType.MOUSEBUTTONUP)
            {
                //MouseX = evt.MouseButton.X;
                //MouseY = evt.MouseButton.Y;
                mOldMouseButtons = mMouseButtons;
                mMouseButtons &= (byte)(~(1 << evt.MouseButton.Button));
                mMouseKeyStateDirty = true;
            }
            else if (evt.Type == EventType.MOUSEMOTION)
            {
                //MouseX = evt.MouseMotion.X;
                //MouseY = evt.MouseMotion.X;
            }
        }
        public bool IsMouseButtonDown(EMouseButton button)
        {
            return (mMouseButtons & (1 << (int)button)) != 0;
        }
        public bool IsMouseButtonUp(EMouseButton button)
        {
            return (mMouseButtons & (1 << (int)button)) == 0;
        }

    }
}

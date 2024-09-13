using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Input
{
    public partial class TtInputSystem
    {
        public unsafe bool PullEvent(ref Input.Event evt)
        {
            return false;
        }
        public static void PostQuitMessage()
        {
            
        }
        public Scancode GetScancodeFromKey(Keycode keyCode)
        {
            return Scancode.SCANCODE_UNKNOWN;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorCommon
{
    public class GameForm : System.Windows.Forms.Form
    {
        internal int PrevMouseX;
        internal int PrevMouseY;
        protected override bool ProcessKeyMessage(ref Message m)
        {
            var message = new EngineNS.Input.Device.Keyboard.WindowsMessage();
            message.HWnd = m.HWnd;
            message.Msg = m.Msg;
            message.WParam = m.WParam;
            message.LParam = m.LParam;
            message.Result = m.Result;
            var keyboardE = EngineNS.Input.Device.Keyboard.ProcessWindowsKeyMessage(ref message);
            KeyEventArgs e = null;
            if (keyboardE.KeyCode != EngineNS.Input.Device.Keyboard.Keys.None)
            {
                e = new KeyEventArgs((Keys)keyboardE.KeyCode);
                if (e.KeyData != Keys.None)
                {
                    if (m.Msg == EngineNS.Input.Device.Keyboard.WM_KEYDOWN)
                        OnKeyDown(e);
                    else
                        OnKeyUp(e);
                    return true;
                }
            }
            return base.ProcessKeyMessage(ref m);
        }
    }
}

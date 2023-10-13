using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIHost
    {
        public bool IsInputActive = false;  // true才能接受消息

        //public override void ProcessMessage(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    base.ProcessMessage(eventArgs, e);
        //}

        public void ProcessHotKey(in Bricks.Input.Event e)
        {

        }
    }
}

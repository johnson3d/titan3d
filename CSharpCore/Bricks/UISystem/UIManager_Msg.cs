using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UISystem
{
    public partial class UIManager
    {
        public void DispatchInputEvent(EngineNS.Input.Device.DeviceInputEventArgs args)
        {
            var ite = mUIHostList.GetEnumerator();
            while(ite.MoveNext())
            {
                if (ite.Current.Value.IsInputActive)
                {
                    ite.Current.Value.DispatchInputEvent(args);
                }
            }
        }
    }
}

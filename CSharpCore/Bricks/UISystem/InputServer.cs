using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Input
{
    public partial class InputServer
    {
        partial void UIInputProcess(EngineNS.Input.Device.DeviceInputEventArgs e)
        {
            EngineNS.CEngine.Instance.UIManager.DispatchInputEvent(e);
        }
    }
}

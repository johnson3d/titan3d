using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public interface IUIProxyBase
    {
        void OnDraw(ref ImDrawList drawList);
    }
}

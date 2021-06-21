using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public interface IUIProxyBase
    {
        bool OnDraw(ref ImDrawList drawList);
    }
}

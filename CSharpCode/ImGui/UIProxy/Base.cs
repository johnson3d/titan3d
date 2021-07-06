using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public interface IUIProxyBase
    {
        void Cleanup();
        System.Threading.Tasks.Task<bool> Initialize();
        bool OnDraw(ref ImDrawList drawList, ref Support.UAnyPointer drawData);
    }
}

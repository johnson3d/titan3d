using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugins.CSCommon
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct FSyncUniqueId
    {
        public uint IndexInLevel;
        public byte Level;
    }
}

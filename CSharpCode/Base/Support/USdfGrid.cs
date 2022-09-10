using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class USdfGrid : AuxPtrType<EngineNS.SdfGrid>
    {
        public USdfGrid()
        {
            mCoreObject = EngineNS.SdfGrid.CreateInstance();
        }
    }
}

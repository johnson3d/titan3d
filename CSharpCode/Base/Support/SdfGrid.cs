using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class TtSdfGrid : AuxPtrType<EngineNS.SdfGrid>
    {
        public TtSdfGrid()
        {
            mCoreObject = EngineNS.SdfGrid.CreateInstance();
        }
    }
}

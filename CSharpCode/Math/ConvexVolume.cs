using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class TtConvexVolume : AuxPtrType<EngineNS.ConvexVolume>
    {
        public TtConvexVolume()
        {
            mCoreObject = new EngineNS.ConvexVolume();
        }
    }
}

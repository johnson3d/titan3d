using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyHeightfield : AuxPtrType<PhyHeightfield>
    {
        public TtPhyHeightfield(PhyHeightfield self)
        {
            mCoreObject = self;
        }
    }
}

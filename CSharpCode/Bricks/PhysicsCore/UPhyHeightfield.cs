using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class UPhyHeightfield : AuxPtrType<PhyHeightfield>
    {
        public UPhyHeightfield(PhyHeightfield self)
        {
            mCoreObject = self;
        }
    }
}

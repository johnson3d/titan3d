using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyConvexMesh : AuxPtrType<PhyConvexMesh>
    {
        public TtPhyConvexMesh(PhyConvexMesh self)
        {
            mCoreObject = self;
        }
    }
}

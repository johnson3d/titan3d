using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class UPhyConvexMesh : AuxPtrType<PhyConvexMesh>
    {
        public UPhyConvexMesh(PhyConvexMesh self)
        {
            mCoreObject = self;
        }
    }
}

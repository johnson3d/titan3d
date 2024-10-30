using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyCapsuleControllerDesc : AuxPtrType<PhyCapsuleControllerDesc>
    {
        public TtPhyCapsuleControllerDesc()
        {
            mCoreObject = PhyCapsuleControllerDesc.CreateInstance();
        }
        public void SetMaterial(TtPhyMaterial mtl)
        {
            mCoreObject.NativeSuper.SetMaterial(mtl.mCoreObject);
        }
    }
    public class TtPhyBoxControllerDesc : AuxPtrType<PhyBoxControllerDesc>
    {
        public TtPhyBoxControllerDesc()
        {
            mCoreObject = PhyBoxControllerDesc.CreateInstance();
        }
        public void SetMaterial(TtPhyMaterial mtl)
        {
            mCoreObject.NativeSuper.SetMaterial(mtl.mCoreObject);
        }
    }
    public class TtPhyController : AuxPtrType<PhyController>
    {
        public TtPhyController(PhyController self)
        {
            mCoreObject = self;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class UPhyCapsuleControllerDesc : AuxPtrType<PhyCapsuleControllerDesc>
    {
        public UPhyCapsuleControllerDesc()
        {
            mCoreObject = PhyCapsuleControllerDesc.CreateInstance();
        }
        public void SetMaterial(UPhyMaterial mtl)
        {
            mCoreObject.NativeSuper.SetMaterial(mtl.mCoreObject);
        }
    }
    public class UPhyBoxControllerDesc : AuxPtrType<PhyBoxControllerDesc>
    {
        public UPhyBoxControllerDesc()
        {
            mCoreObject = PhyBoxControllerDesc.CreateInstance();
        }
        public void SetMaterial(UPhyMaterial mtl)
        {
            mCoreObject.NativeSuper.SetMaterial(mtl.mCoreObject);
        }
    }
    public class UPhyController : AuxPtrType<PhyController>
    {
        public UPhyController(PhyController self)
        {
            mCoreObject = self;
        }
    }
}

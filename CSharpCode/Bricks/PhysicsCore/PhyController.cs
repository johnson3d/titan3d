using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyCapsuleControllerDesc : AuxPtrType<PhyCapsuleControllerDesc>
    {
        public TtPhyCapsuleControllerDesc(PhyCapsuleControllerDesc ptr)
        {
            mCoreObject = ptr;
        }
        public void SetMaterial(TtPhyMaterial mtl)
        {
            mCoreObject.SetMaterial(mtl.mCoreObject);
        }
    }
    public class TtPhyBoxControllerDesc : AuxPtrType<PhyBoxControllerDesc>
    {
        public TtPhyBoxControllerDesc(PhyBoxControllerDesc ptr)
        {
            mCoreObject = ptr;
        }
        public void SetMaterial(TtPhyMaterial mtl)
        {
            mCoreObject.SetMaterial(mtl.mCoreObject);
        }
    }
    public class TtPhyController : AuxPtrType<PhyController>
    {
        public GamePlay.Scene.TtNode TagNode;
        public TtPhyController(PhyController self)
        {
            mCoreObject = self;
            var gchandle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak);
            unsafe
            {
                var super = mCoreObject.NativeSuper;
                super.mCSharpHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(gchandle).ToPointer();
            }
        }
        ~TtPhyController()
        {
            unsafe
            {
                var super = mCoreObject.NativeSuper;
                var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)super.mCSharpHandle);
                super.mCSharpHandle = (void*)0;
                gchandle.Free();
            }
        }

        public static TtPhyController GetPhyController(PhyController controller)
        {
            unsafe
            {
                var ptr = (IntPtr)controller.NativeSuper.mCSharpHandle;
                if (ptr == IntPtr.Zero)
                    return null;
                var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ptr);
                return gchandle.Target as TtPhyController;
            }
        }
        public void Cleanup()
        {
            mCoreObject.NativeSuper.NativeSuper.Cleanup();
            TagNode = null;
        }
    }
}

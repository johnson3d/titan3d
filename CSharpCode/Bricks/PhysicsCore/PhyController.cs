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
        public TtPhyScene mScene;
        public TtPhyController(TtPhyScene scene, PhyController self)
        {
            mCoreObject = self;
            mScene = scene;
            mCoreObject.NativeSuper.BindObject(mScene, this);
        }
        public unsafe static TtPhyController GetPhyController(PhyController controller)
        {
            return controller.NativeSuper.GetCSharpHandle() as TtPhyController;
        }
        public unsafe override void Dispose()
        {
            mCoreObject.NativeSuper.UnbindObject(mScene);
            mScene = null;
            base.Dispose();
            TagNode = null;
        }
    }
}

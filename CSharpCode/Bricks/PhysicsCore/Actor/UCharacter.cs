using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Character
{
    partial class TtCharacter
    {
        public partial class TtCharacterData
        {
            
        }
        public Bricks.PhysicsCore.TtPhyController PhyController { get; set; }
        partial void CreatePxCapsuleController(ref bool result, Scene.TtScene scene, float radius, float height)
        {
            var desc = new Bricks.PhysicsCore.TtPhyCapsuleControllerDesc();
            desc.mCoreObject.SetCapsuleHeight(height);
            desc.mCoreObject.SetCapsuleRadius(radius);
            Bricks.PhysicsCore.TtPhyMaterial mtl;
            if (this.PlayerData.PxMaterial!=null)
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(this.PlayerData.PxMaterial);
            else
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
            desc.SetMaterial(mtl);
            PhyController = scene.PxSceneMB.PxScene.CreateCapsuleController(desc.mCoreObject);
            if (PhyController == null)
            {
                result = false;
                return;
            }
            //var mass =  PhyController.mCoreObject.mActor.GetMass();
            //PhyController.mCoreObject.Move()
            result = true;
            return;
        }
    }
}

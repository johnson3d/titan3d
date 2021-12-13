using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Character
{
    partial class UCharacter
    {
        public partial class UCharacterData
        {
            
        }
        public Bricks.PhysicsCore.UPhyController PhyController { get; set; }
        partial void CreatePxCapsuleController(ref bool result, Scene.UScene scene, float radius, float height)
        {
            var desc = new Bricks.PhysicsCore.UPhyCapsuleControllerDesc();
            desc.mCoreObject.SetCapsuleHeight(height);
            desc.mCoreObject.SetCapsuleRadius(radius);
            Bricks.PhysicsCore.UPhyMaterial mtl;
            if (this.PlayerData.PxMaterial!=null)
                mtl = UEngine.Instance.PhyModue.PhyContext.PhyMaterialManager.GetMaterialSync(this.PlayerData.PxMaterial);
            else
                mtl = UEngine.Instance.PhyModue.PhyContext.PhyMaterialManager.DefaultMaterial;
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

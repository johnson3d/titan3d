using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene.Actor
{
    partial class UActor
    {
        partial class UActorData
        {
            [Rtti.Meta]
            public RName PxMaterial { get; set; }
        }
        public Bricks.PhysicsCore.UPhyActor PhyActor{ get; set; }
        public Bricks.PhysicsCore.UPhyScene GetPxScene() {
            return this.ParentScene?.PxSceneMB.PxScene;
        }
        partial void CreatePxCapsuleActor(ref bool result, Scene.UScene scene, float radius, float height)
        {
            var pc = UEngine.Instance.PhyModule.PhyContext;
            ref var transform = ref Placement.TransformRef;
            PhyActor = pc.CreateActor(EPhyActorType.PAT_Dynamic, in transform.mPosition, in transform.mQuat);
            Bricks.PhysicsCore.UPhyMaterial mtl;
            
            if (this.ActorData.PxMaterial != null)
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(this.ActorData.PxMaterial);
            else
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;

            //var shape = pc.CreateShapeCapsule(mtl, radius, height / 2.0f);
            var shape = pc.CreateShapeSphere(mtl, radius);
            shape.mCoreObject.AddToActor(PhyActor.mCoreObject, in Vector3.Zero, in Quaternion.Identity);
            PhyActor.mCoreObject.SetMass(3.0f);
            var mass = PhyActor.mCoreObject.GetMass();
            PhyActor.TagNode = this;
            PhyActor.mCoreObject.SetMinCCDAdvanceCoefficient(0);
            PhyActor.AddToScene(scene.PxSceneMB.PxScene);
        }
        protected override void OnAbsTransformChanged()
        {
            var pxScene = GetPxScene();
            if (pxScene != null)
            {
                if (PhyActor != null && pxScene.IsPxFetchingPose == false)
                {
                    ref var transform = ref Placement.TransformRef;
                    PhyActor.SetPose2Physics(in transform.mPosition, in transform.mQuat, true);
                }
            }
            base.OnAbsTransformChanged();
        }
    }
}

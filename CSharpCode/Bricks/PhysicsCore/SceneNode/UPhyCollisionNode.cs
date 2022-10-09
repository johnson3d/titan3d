using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.Bricks.PhysicsCore.SceneNode.UCapsulePhyControllerNode;
using static EngineNS.Bricks.PhysicsCore.SceneNode.UPhyCollisionNode;

namespace EngineNS.Bricks.PhysicsCore.SceneNode
{
    public abstract class UPhyCollisionNode : GamePlay.Scene.ULightWeightNodeBase
    {
        public class UPhyCollisionNodeData : GamePlay.Scene.UNodeData
        {
           
        }
        public Bricks.PhysicsCore.UPhyActor PhyActor { get; set; }
        protected override void OnParentChanged(UNode prev, UNode cur)
        {
            base.OnParentChanged(prev, cur);
            PhyActor.TagNode = Parent;
            Parent.UpdateAbsTransform();
            OnAbsTransformChanged();
            PhyActor.AddToScene(ParentScene.PxSceneMB.PxScene);
        }

        protected override void OnParentSceneChanged(UScene prev, UScene cur)
        {
            base.OnParentSceneChanged(prev, cur);
            //TODO: Remove from Prev-Scene if exist
            PhyActor.AddToScene(ParentScene.PxSceneMB.PxScene);
        }
        protected override void OnAbsTransformChanged()
        {
            var pxScene = ParentScene?.PxSceneMB.PxScene;
            if (pxScene != null)
            {
                if (PhyActor != null && pxScene.IsPxFetchingPose == false)
                {
                    ref var transform = ref Placement.AbsTransform;
                    PhyActor.SetPose2Physics(in transform.mPosition, in transform.mQuat, true);
                }
            }
            base.OnAbsTransformChanged();
        }
    }

    //only contians one shape
    public class UPhySingleShapeCollisionNode : UPhyCollisionNode
    {
        public class UPhySingleShapeCollisionNodeData : UPhyCollisionNodeData
        {
            [Rtti.Meta]
            public EPhyActorType PhyActorType { get; set; } = EPhyActorType.PAT_Dynamic;
            [Rtti.Meta]
            public RName PxMaterial { get; set; }
            [Rtti.Meta]
            public float Mass { get; set; } = 10;
            [Rtti.Meta]
            public PhyFilterData QueryFilterData { get; set; }
            [Rtti.Meta]
            public PhyFilterData SimulationFilterData { get; set; }
        }
    }

    //contain some shapes
    public class UPhyMutiShapesCollisionNode : UPhyCollisionNode
    {

    }

    [Bricks.CodeBuilder.ContextMenu("SphereCollisionNode", "SphereCollisionNode", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(UPhySphereCollisionNode.UPhySphereCollisionNodeData), DefaultNamePrefix = "SphereCollisionNode")]
    public class UPhySphereCollisionNode : UPhySingleShapeCollisionNode
    {
        public class UPhySphereCollisionNodeData : UPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public float Radius = 1.0f;
        }
        public UPhySphereCollisionNodeData CollisionNodeData
        {
            get => NodeData as UPhySphereCollisionNodeData;
        }
        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            var pc = UEngine.Instance.PhyModule.PhyContext;
            var transform = Placement.TransformData;
            PhyActor = pc.CreateActor(CollisionNodeData.PhyActorType, in transform.mPosition, in transform.mQuat);
            PhyActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);

            Bricks.PhysicsCore.UPhyMaterial mtl;
            if (CollisionNodeData.PxMaterial != null)
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(CollisionNodeData.PxMaterial);
            else
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;

            var shape = pc.CreateShapeSphere(mtl, CollisionNodeData.Radius);
            shape.mCoreObject.SetQueryFilterData(CollisionNodeData.QueryFilterData);
            shape.mCoreObject.SetSimulationFilterData(CollisionNodeData.SimulationFilterData);

            shape.mCoreObject.AddToActor(PhyActor.mCoreObject, in Vector3.Zero, in Quaternion.Identity);
            PhyActor.mCoreObject.SetMass(CollisionNodeData.Mass);
            PhyActor.mCoreObject.SetMinCCDAdvanceCoefficient(0);

            return true;
        }
    }
    public class UPhyBoxCollisionNode : UPhySingleShapeCollisionNode
    {
        public class UPhyBoxCollisionNodeData : UPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public Vector3 HalfExtent = Vector3.One;
        }
        public UPhyBoxCollisionNodeData CollisionNodeData
        {
            get => NodeData as UPhyBoxCollisionNodeData;
        }
        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            var pc = UEngine.Instance.PhyModule.PhyContext;
            var transform = Placement.TransformData;
            PhyActor = pc.CreateActor(CollisionNodeData.PhyActorType, in transform.mPosition, in transform.mQuat);
            PhyActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);

            Bricks.PhysicsCore.UPhyMaterial mtl;
            if (CollisionNodeData.PxMaterial != null)
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(CollisionNodeData.PxMaterial);
            else
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;

            var shape = pc.CreateShapeBox(mtl, CollisionNodeData.HalfExtent);
            shape.mCoreObject.SetQueryFilterData(CollisionNodeData.QueryFilterData);
            shape.mCoreObject.SetSimulationFilterData(CollisionNodeData.SimulationFilterData);

            shape.mCoreObject.AddToActor(PhyActor.mCoreObject, in Vector3.Zero, in Quaternion.Identity);
            PhyActor.mCoreObject.SetMass(CollisionNodeData.Mass);
            PhyActor.mCoreObject.SetMinCCDAdvanceCoefficient(0);

            return true;
        }
    }
    public class UPhyPlaneCollisionNode : UPhySingleShapeCollisionNode
    {
        public class UPhySphereCollisionNodeData : UPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public Vector3 HalfExtent = Vector3.One * 0.5f;
        }
    }

    public class UPhyCapsuleCollisionNode : UPhySingleShapeCollisionNode
    {
        public class UPhyCapsuleCollisionNodeData : UPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public float Radius = 1.0f;
            [Rtti.Meta]
            public float HalfHeight = 0.5f;
        }
    }

    public class UPhyConvexCollisionNode : UPhySingleShapeCollisionNode
    {
        public class UPhyConvexCollisionNodeData : UPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public RName ConvexSource;
        }
    }
    public class UPhyTriMeshCollisionNode : UPhySingleShapeCollisionNode
    {
        public class UPhyTriMeshCollisionNodeData : UPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public RName TriMeshSource;
        }
    }
}

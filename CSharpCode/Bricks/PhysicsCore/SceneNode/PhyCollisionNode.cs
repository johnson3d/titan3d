using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.Bricks.PhysicsCore.SceneNode.TtCapsulePhyControllerNode;
using static EngineNS.Bricks.PhysicsCore.SceneNode.TtPhyCollisionNode;

namespace EngineNS.Bricks.PhysicsCore.SceneNode
{

    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public abstract class TtPhyCollisionNode : GamePlay.Scene.TtLightWeightNodeBase
    {
        public class TtPhyCollisionNodeData : GamePlay.Scene.TtNodeData
        {
           
        }
        public Bricks.PhysicsCore.TtPhyActor PhyActor { get; set; }
        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            base.OnParentChanged(prev, cur);
            PhyActor.TagNode = Parent;
            Parent.UpdateAbsTransform();
            OnAbsTransformChanged();
            PhyActor.AddToScene(ParentScene.PxSceneMB.PxScene);
        }

        protected override void OnParentSceneChanged(TtScene prev, TtScene cur)
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
    public class TtPhySingleShapeCollisionNode : TtPhyCollisionNode
    {
        public class TtPhySingleShapeCollisionNodeData : TtPhyCollisionNodeData
        {
            [Rtti.Meta]
            public EPhyActorType PhyActorType { get; set; } = EPhyActorType.PAT_Static;
            [Rtti.Meta]
            public RName PxMaterial { get; set; }
            [Rtti.Meta]
            public float Mass { get; set; } = 10;
            [Rtti.Meta]
            public PhyFilterData QueryFilterData { get; set; }
            [Rtti.Meta]
            public PhyFilterData SimulationFilterData { get; set; }
        }
        public TtPhySingleShapeCollisionNodeData SingleShapeCollisionNodeData
        {
            get => NodeData as TtPhySingleShapeCollisionNodeData;
        }
        [Category("Option")]
        public EPhyActorType PhyActorType { get => SingleShapeCollisionNodeData.PhyActorType; set => SingleShapeCollisionNodeData.PhyActorType = value; }
        [Category("Option")]
        public RName PxMaterial { get => SingleShapeCollisionNodeData.PxMaterial; set => SingleShapeCollisionNodeData.PxMaterial = value; }
        [Category("Option")]
        public float Mass { get => SingleShapeCollisionNodeData.Mass; set => SingleShapeCollisionNodeData.Mass = value; }
        [Category("Option")]
        public PhyFilterData QueryFilterData { get => SingleShapeCollisionNodeData.QueryFilterData; set => SingleShapeCollisionNodeData.QueryFilterData = value; }
        [Category("Option")]
        public PhyFilterData SimulationFilterData { get => SingleShapeCollisionNodeData.SimulationFilterData; set => SingleShapeCollisionNodeData.SimulationFilterData = value; }
    }

    //contain some shapes
    public class TtPhyMutiShapesCollisionNode : TtPhyCollisionNode
    {

    }

    [Bricks.CodeBuilder.ContextMenu("SphereCollision", "Collision\\SphereCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "SphereCollision")]
    public class TtPhySphereCollisionNode : TtPhySingleShapeCollisionNode
    {
        public class TtPhySphereCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            [Category("Option")]
            public float Radius = 1.0f;
        }
        
        public TtPhySphereCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhySphereCollisionNodeData;
        }
        [Category("Option")]
        public float Radius { get => CollisionNodeData.Radius; set => CollisionNodeData.Radius = value; }
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var transform = Placement.TransformData;
            PhyActor = pc.CreateActor(CollisionNodeData.PhyActorType, in transform.mPosition, in transform.mQuat);
            PhyActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);

            Bricks.PhysicsCore.TtPhyMaterial mtl;
            if (CollisionNodeData.PxMaterial != null)
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(CollisionNodeData.PxMaterial);
            else
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;

            var shape = pc.CreateShapeSphere(mtl, CollisionNodeData.Radius);
            shape.mCoreObject.SetQueryFilterData(CollisionNodeData.QueryFilterData);
            shape.mCoreObject.SetSimulationFilterData(CollisionNodeData.SimulationFilterData);

            shape.mCoreObject.AddToActor(PhyActor.mCoreObject, in Vector3.Zero, in Quaternion.Identity);
            PhyActor.mCoreObject.SetMass(CollisionNodeData.Mass);
            PhyActor.mCoreObject.SetMinCCDAdvanceCoefficient(0);

            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("BoxCollision", "Collision\\BoxCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "BoxCollision")]
    public class TtPhyBoxCollisionNode : TtPhySingleShapeCollisionNode
    {
        public class TtPhyBoxCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            public Vector3 HalfExtent = Vector3.One;
        }
        public TtPhyBoxCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhyBoxCollisionNodeData;
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var transform = Placement.TransformData;
            PhyActor = pc.CreateActor(CollisionNodeData.PhyActorType, in transform.mPosition, in transform.mQuat);
            PhyActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);

            Bricks.PhysicsCore.TtPhyMaterial mtl;
            if (CollisionNodeData.PxMaterial != null)
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(CollisionNodeData.PxMaterial);
            else
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;

            var shape = pc.CreateShapeBox(mtl, CollisionNodeData.HalfExtent);
            shape.mCoreObject.SetQueryFilterData(CollisionNodeData.QueryFilterData);
            shape.mCoreObject.SetSimulationFilterData(CollisionNodeData.SimulationFilterData);

            shape.mCoreObject.AddToActor(PhyActor.mCoreObject, in Vector3.Zero, in Quaternion.Identity);
            PhyActor.mCoreObject.SetMass(CollisionNodeData.Mass);
            PhyActor.mCoreObject.SetMinCCDAdvanceCoefficient(0);

            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("PlaneCollision", "Collision\\PlaneCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "PlaneCollision")]
    public class TtPhyPlaneCollisionNode : TtPhySingleShapeCollisionNode
    {
        public class TtPhySphereCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            public Vector3 HalfExtent = Vector3.One * 0.5f;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("CapsuleCollision", "Collision\\CapsuleCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "CapsuleCollision")]
    public class TtPhyCapsuleCollisionNode : TtPhySingleShapeCollisionNode
    {
        public class TtPhyCapsuleCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            public float Radius = 1.0f;
            public float HalfHeight = 0.5f;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("ConvexCollision", "Collision\\ConvexCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "ConvexCollision")]
    public class TtPhyConvexCollisionNode : TtPhySingleShapeCollisionNode
    {
        public class TtPhyConvexCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            public RName ConvexSource;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("TriMeshCollision", "Collision\\TriMeshCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "TriMeshCollision")]
    public class TtPhyTriMeshCollisionNode : TtPhySingleShapeCollisionNode
    {
        public class TtPhyTriMeshCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            public RName TriMeshSource;
        }
    }
}

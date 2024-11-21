using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
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
        [Rtti.Meta]
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
        [Rtti.Meta]
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
        public Bricks.PhysicsCore.TtPhyShape PhyShape { get; set; }
        [Category("Option")]
        public EPhyActorType PhyActorType 
        { 
            get => SingleShapeCollisionNodeData.PhyActorType;
            set
            {
                var oldValue = SingleShapeCollisionNodeData.PhyActorType;
                SingleShapeCollisionNodeData.PhyActorType = value;
                if(oldValue != value)
                {
                    OnActorTypeChange(value);
                }
            }
        }
        [Category("Option")]
        public RName Material { get => SingleShapeCollisionNodeData.PxMaterial; set => SingleShapeCollisionNodeData.PxMaterial = value; }
        public TtPhyMaterial PhyMaterial
        {
            get
            {
                Bricks.PhysicsCore.TtPhyMaterial mtl;
                if (SingleShapeCollisionNodeData.PxMaterial != null)
                    mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(SingleShapeCollisionNodeData.PxMaterial);
                else
                    mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
                return mtl;
            }
        }
        [Category("Option")]
        public float Mass { get => SingleShapeCollisionNodeData.Mass; set => SingleShapeCollisionNodeData.Mass = value; }
        [Category("Option")]
        public PhyFilterData QueryFilterData { get => SingleShapeCollisionNodeData.QueryFilterData; set => SingleShapeCollisionNodeData.QueryFilterData = value; }
        [Category("Option")]
        public PhyFilterData SimulationFilterData { get => SingleShapeCollisionNodeData.SimulationFilterData; set => SingleShapeCollisionNodeData.SimulationFilterData = value; }
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;

            CreatePhyActor();

            return true;
        }
        public virtual void OnActorTypeChange(EPhyActorType newType)
        {
            PhyActor.RemoveFromScene(ParentScene.PxSceneMB.PxScene);
            CreatePhyActor();
            if(Parent != null)
            {
                PhyActor.TagNode = Parent;
            }
            if(ParentScene != null && ParentScene.PxSceneMB.PxScene != null)
            {
                PhyActor.AddToScene(ParentScene.PxSceneMB.PxScene);
            }
        }
        public virtual TtPhyShape CreatePhyShape()
        {
            return null;
        }

        public void CreatePhyActor()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var transform = Placement.TransformData;
            PhyActor = pc.CreateActor(SingleShapeCollisionNodeData.PhyActorType, in transform.mPosition, in transform.mQuat);
            PhyActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);
            
            Bricks.PhysicsCore.TtPhyMaterial mtl;
            if (SingleShapeCollisionNodeData.PxMaterial != null)
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(SingleShapeCollisionNodeData.PxMaterial);
            else
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;

            PhyShape = CreatePhyShape();
            if (PhyShape != null)
            {
                PhyShape.mCoreObject.SetQueryFilterData(SingleShapeCollisionNodeData.QueryFilterData);
                PhyShape.mCoreObject.SetSimulationFilterData(SingleShapeCollisionNodeData.SimulationFilterData);
                PhyShape.mCoreObject.AddToActor(PhyActor.mCoreObject, in Vector3.Zero, in Quaternion.Identity);
            }

            PhyActor.mCoreObject.SetMass(SingleShapeCollisionNodeData.Mass);
            PhyActor.mCoreObject.SetMinCCDAdvanceCoefficient(0);

            
        }
    }

    //contain some shapes
    public class TtPhyMutiShapesCollisionNode : TtPhyCollisionNode
    {

    }

    [Bricks.CodeBuilder.ContextMenu("SphereCollision", "Collision\\SphereCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "SphereCollision")]
    public class TtPhySphereCollisionNode : TtPhySingleShapeCollisionNode
    {
        [Rtti.Meta]
        public class TtPhySphereCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public float Radius { get; set; } = 0.5f;
        }
        
        public TtPhySphereCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhySphereCollisionNodeData;
        }
        [Category("Option")]
        public float Radius 
        { 
            get => CollisionNodeData.Radius;
            set
            {
                CollisionNodeData.Radius = value;
                PhyShape.RemoveFromActor();
                PhyShape = CreatePhyShape();
                if(PhyActor != null)
                {
                    PhyShape.AddToActor(PhyActor, in Vector3.Zero, in Quaternion.Identity);
                }
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            return pc.CreateShapeSphere(PhyMaterial, CollisionNodeData.Radius);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("BoxCollision", "Collision\\BoxCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyBoxCollisionNode.TtPhyBoxCollisionNodeData), DefaultNamePrefix = "BoxCollision")]
    public class TtPhyBoxCollisionNode : TtPhySingleShapeCollisionNode
    {
        [Rtti.Meta]
        public class TtPhyBoxCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public Vector3 HalfExtent { get; set; } = Vector3.One * 0.5f;
        }
        public TtPhyBoxCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhyBoxCollisionNodeData;
        }
        [Category("Option")]
        public Vector3 HalfExtent
        {
            get => CollisionNodeData.HalfExtent;
            set
            {
                CollisionNodeData.HalfExtent = value;
                PhyShape.RemoveFromActor();
                PhyShape = CreatePhyShape();
                if (PhyActor != null)
                {
                    PhyShape.AddToActor(PhyActor, in Vector3.Zero, in Quaternion.Identity);
                }
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            return pc.CreateShapeBox(PhyMaterial, CollisionNodeData.HalfExtent);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("PlaneCollision", "Collision\\PlaneCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyPlaneCollisionNode.TtPhyPlaneCollisionNodeData), DefaultNamePrefix = "PlaneCollision")]
    public class TtPhyPlaneCollisionNode : TtPhySingleShapeCollisionNode
    {
        [Rtti.Meta]
        public class TtPhyPlaneCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
           
        }
        public TtPhyPlaneCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhyPlaneCollisionNodeData;
        }

        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            return pc.CreateShapePlane(PhyMaterial);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("CapsuleCollision", "Collision\\CapsuleCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyCapsuleCollisionNode.TtPhyCapsuleCollisionNodeData), DefaultNamePrefix = "CapsuleCollision")]
    public class TtPhyCapsuleCollisionNode : TtPhySingleShapeCollisionNode
    {
        [Rtti.Meta]
        public class TtPhyCapsuleCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public float Radius { get; set; } = 0.5f;
            [Rtti.Meta]
            public float HalfHeight { get; set; } = 0.5f;
        }
        public TtPhyCapsuleCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhyCapsuleCollisionNodeData;
        }
        [Category("Option")]
        public float Radius
        {
            get => CollisionNodeData.Radius;
            set
            {
                CollisionNodeData.Radius = value;
                PhyShape.RemoveFromActor();
                PhyShape = CreatePhyShape();
                if (PhyActor != null)
                {
                    PhyShape.AddToActor(PhyActor, in Vector3.Zero, in Quaternion.Identity);
                }
            }
        }
        [Category("Option")]
        public float HalfHeight
        {
            get => CollisionNodeData.HalfHeight;
            set
            {
                CollisionNodeData.HalfHeight = value;
                PhyShape.RemoveFromActor();
                PhyShape = CreatePhyShape();
                if (PhyActor != null)
                {
                    PhyShape.AddToActor(PhyActor, in Vector3.Zero, in Quaternion.Identity);
                }
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            return pc.CreateShapeCapsule(PhyMaterial, CollisionNodeData.Radius, CollisionNodeData.HalfHeight);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("ConvexCollision", "Collision\\ConvexCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyConvexCollisionNode.TtPhyConvexCollisionNodeData), DefaultNamePrefix = "ConvexCollision")]
    public class TtPhyConvexCollisionNode : TtPhySingleShapeCollisionNode
    {
        [Rtti.Meta]
        public class TtPhyConvexCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public RName ConvexSource { get; set; }
        }
        public TtPhyConvexCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhyConvexCollisionNodeData;
        }
        [Category("Option")]
        public RName ConvexSource
        {
            get => CollisionNodeData.ConvexSource;
            set
            {
                CollisionNodeData.ConvexSource = value;
                PhyShape.RemoveFromActor();
                PhyShape = CreatePhyShape();
                if (PhyActor != null)
                {
                    PhyShape.AddToActor(PhyActor, in Vector3.Zero, in Quaternion.Identity);
                }
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            BoundingBox boundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
            TtMeshDataProvider mesh = TtMeshDataProvider.MakeBox(boundingBox);
            var convexMesh = pc.CookConvexMesh(mesh);
            System.Diagnostics.Debug.Assert(false); //need cook convex from mesh
            return pc.CreateShapeConvex(PhyMaterial, convexMesh, in Vector3.Zero, in Quaternion.Identity);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("TriMeshCollision", "Collision\\TriMeshCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyTriMeshCollisionNode.TtPhyTriMeshCollisionNodeData), DefaultNamePrefix = "TriMeshCollision")]
    public class TtPhyTriMeshCollisionNode : TtPhySingleShapeCollisionNode
    {
        [Rtti.Meta]
        public class TtPhyTriMeshCollisionNodeData : TtPhySingleShapeCollisionNodeData
        {
            [Rtti.Meta]
            public RName TriMeshSource { get; set; }
        }
        public TtPhyTriMeshCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhyTriMeshCollisionNodeData;
        }
        [Category("Option")]
        public RName TriMeshSource
        {
            get => CollisionNodeData.TriMeshSource;
            set
            {
                CollisionNodeData.TriMeshSource = value;
                PhyShape.RemoveFromActor();
                PhyShape = CreatePhyShape();
                if (PhyActor != null)
                {
                    PhyShape.AddToActor(PhyActor, in Vector3.Zero, in Quaternion.Identity);
                }
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            BoundingBox boundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
            TtMeshDataProvider mesh = TtMeshDataProvider.MakeBox(boundingBox);
            System.Diagnostics.Debug.Assert(false); //need cook convex from mesh
            var triMesh = pc.CookTriMesh(mesh,null,null,null);
            List<TtPhyMaterial> materials = new List<TtPhyMaterial>();
            return pc.CreateShapeTriMesh(materials, triMesh, in Vector3.Zero, in Quaternion.Identity);
        }
    }
}

using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using System.ComponentModel;

namespace EngineNS.Bricks.PhysicsCore.SceneNode
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]

    //only contians one shape
    public class TtPhyCollisionNode : GamePlay.Scene.TtLightWeightNodeBase
    {
        [Rtti.Meta("")]
        public class TtPhyCollisionNodeData : GamePlay.Scene.TtNodeData
        {
            [Rtti.Meta("")]
            public RName PxMaterial { get; set; }
            [Rtti.Meta("")]
            public Vector3 Center { get; set; }
            [Rtti.Meta("")]
            public FRotator Rotator { get; set; }
            [Rtti.Meta("")]
            public bool IsTrigger { get; set; } = false;
            [Rtti.Meta("")]
            public PhyFilterData QueryFilterData { get; set; }
            [Rtti.Meta("")]
            public PhyFilterData SimulationFilterData { get; set; }
        }
        public TtPhyCollisionNodeData CollisionNodeData
        {
            get => NodeData as TtPhyCollisionNodeData;
        }
        public TtPhyActor PhyActor
        {
            get
            {
                if (Parent is TtPhyRigidbodyNode rigidNode)
                {
                    return rigidNode.PhyActor;
                }
                return null;
            }
        }
        [Category("Option")]
        public RName Material { get => CollisionNodeData.PxMaterial; set => CollisionNodeData.PxMaterial = value; }
        public TtPhyMaterial PhyMaterial
        {
            get
            {
                Bricks.PhysicsCore.TtPhyMaterial mtl;
                if (CollisionNodeData.PxMaterial != null)
                    mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(CollisionNodeData.PxMaterial);
                else
                    mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
                return mtl;
            }
        }
        [Category("Option")]
        public Vector3 Center { get => CollisionNodeData.Center; set => CollisionNodeData.Center = value; }
        [Category("Option")]
        public FRotator Rotator { get => CollisionNodeData.Rotator; set => CollisionNodeData.Rotator = value; }
        [Category("Option")]
        public bool IsTrigger
        {
            get => CollisionNodeData.IsTrigger;
            set
            {
                CollisionNodeData.IsTrigger = value;
                SetTriggerFlag(value, PhyShape);
            }
        }
        [Category("Option")]
        public PhyFilterData QueryFilterData { get => CollisionNodeData.QueryFilterData; set => CollisionNodeData.QueryFilterData = value; }
        [Category("Option")]
        public PhyFilterData SimulationFilterData { get => CollisionNodeData.SimulationFilterData; set => CollisionNodeData.SimulationFilterData = value; }
        protected override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            PhyShape = CreatePhyShape();
            return true;
        }
        public virtual TtPhyShape CreatePhyShape()
        {
            return null;
        }
        public virtual bool ChangeShape()
        {
            var shape = CreatePhyShape();
            if(shape != null)
            {
                PhyShape = shape;
                PhyShape.RemoveFromActor();
                AddToActor();
                return true;
            }
            return false;
        }
        protected void SetTriggerFlag(bool isTrigger, TtPhyShape shape)
        {
            shape.mCoreObject.SetFlag(EPhysShapeFlag.eSIMULATION_SHAPE, !IsTrigger);
            shape.mCoreObject.SetFlag(EPhysShapeFlag.eTRIGGER_SHAPE, IsTrigger);
        }
        public Bricks.PhysicsCore.TtPhyShape PhyShape { get; set; } = null;
        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            base.OnParentChanged(prev, cur);
            AddToActor();
        }
        protected void AddToActor()
        {
            if (PhyShape != null && PhyActor != null)
            {
                PhyShape.AddToActor(PhyActor, CollisionNodeData.Center, Quaternion.FromEuler(CollisionNodeData.Rotator));
            }
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.PhyxDebug) == 0)
                return;

            rp.AddVisibleNode(this);
            PhyShape.DebugMesh.SetWorldTransform(in this.Placement.AbsTransform, rp.World, true);
            rp.AddVisibleMesh(PhyShape.DebugMesh);
            
        }
    }

    [Bricks.CodeBuilder.ContextMenu("SphereCollision", "Collision\\SphereCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhySphereCollisionNode.TtPhySphereCollisionNodeData), DefaultNamePrefix = "SphereCollision")]
    public class TtPhySphereCollisionNode : TtPhyCollisionNode
    {
        [Rtti.Meta("")]
        public class TtPhySphereCollisionNodeData : TtPhyCollisionNodeData
        {
            [Rtti.Meta("")]
            public float Radius { get; set; } = 0.5f;
        }

        public TtPhySphereCollisionNodeData SphereCollisionNodeData
        {
            get => NodeData as TtPhySphereCollisionNodeData;
        }
        [Category("Option")]
        public float Radius
        {
            get => SphereCollisionNodeData.Radius;
            set
            {
                if (value <= 0)
                    return;
                SphereCollisionNodeData.Radius = value;
                ChangeShape();
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var shape = pc.CreateShapeSphere(PhyMaterial, SphereCollisionNodeData.Radius);
            SetTriggerFlag(IsTrigger, shape);
            return shape;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("BoxCollision", "Collision\\BoxCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyBoxCollisionNode.TtPhyBoxCollisionNodeData), DefaultNamePrefix = "BoxCollision")]
    public class TtPhyBoxCollisionNode : TtPhyCollisionNode
    {
        [Rtti.Meta("")]
        public class TtPhyBoxCollisionNodeData : TtPhyCollisionNodeData
        {
            [Rtti.Meta("")]
            public Vector3 HalfExtent { get; set; } = Vector3.One * 0.5f;
        }
        public TtPhyBoxCollisionNodeData BoxCollisionNodeData
        {
            get => NodeData as TtPhyBoxCollisionNodeData;
        }
        [Category("Option")]
        public Vector3 HalfExtent
        {
            get => BoxCollisionNodeData.HalfExtent;
            set
            {
                BoxCollisionNodeData.HalfExtent = value;
                if (value.x <= 0 || value.y <= 0 || value.z <= 0)
                    return;
                ChangeShape();
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var shape = pc.CreateShapeBox(PhyMaterial, BoxCollisionNodeData.HalfExtent);
            SetTriggerFlag(IsTrigger, shape);
            return shape;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("PlaneCollision", "Collision\\PlaneCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyPlaneCollisionNode.TtPhyPlaneCollisionNodeData), DefaultNamePrefix = "PlaneCollision")]
    public class TtPhyPlaneCollisionNode : TtPhyCollisionNode
    {
        [Rtti.Meta("")]
        public class TtPhyPlaneCollisionNodeData : TtPhyCollisionNodeData
        {

        }
        public TtPhyPlaneCollisionNodeData PlaneCollisionNodeData
        {
            get => NodeData as TtPhyPlaneCollisionNodeData;
        }

        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var shape = pc.CreateShapePlane(PhyMaterial);
            SetTriggerFlag(IsTrigger, shape);
            return shape;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("CapsuleCollision", "Collision\\CapsuleCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyCapsuleCollisionNode.TtPhyCapsuleCollisionNodeData), DefaultNamePrefix = "CapsuleCollision")]
    public class TtPhyCapsuleCollisionNode : TtPhyCollisionNode
    {
        [Rtti.Meta("")]
        public class TtPhyCapsuleCollisionNodeData : TtPhyCollisionNodeData
        {
            [Rtti.Meta("")]
            public float Radius { get; set; } = 0.5f;
            [Rtti.Meta("")]
            public float HalfHeight { get; set; } = 0.5f;
        }
        public TtPhyCapsuleCollisionNodeData CapsuleCollisionNodeData
        {
            get => NodeData as TtPhyCapsuleCollisionNodeData;
        }
        [Category("Option")]
        public float Radius
        {
            get => CapsuleCollisionNodeData.Radius;
            set
            {
                if (value <= 0)
                    return;
                CapsuleCollisionNodeData.Radius = value;
                ChangeShape();
            }
        }
        [Category("Option")]
        public float HalfHeight
        {
            get => CapsuleCollisionNodeData.HalfHeight;
            set
            {
                if (value <= 0)
                    return;
                CapsuleCollisionNodeData.HalfHeight = value;
                ChangeShape();
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var shape = pc.CreateShapeCapsule(PhyMaterial, CapsuleCollisionNodeData.Radius, CapsuleCollisionNodeData.HalfHeight);
            SetTriggerFlag(IsTrigger, shape);
            return shape;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("ConvexCollision", "Collision\\ConvexCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyConvexCollisionNode.TtPhyConvexCollisionNodeData), DefaultNamePrefix = "ConvexCollision")]
    public class TtPhyConvexCollisionNode : TtPhyCollisionNode
    {
        [Rtti.Meta("")]
        public class TtPhyConvexCollisionNodeData : TtPhyCollisionNodeData
        {
            [Rtti.Meta("")]
            public RName ConvexSource { get; set; }
        }
        public TtPhyConvexCollisionNodeData ConvexCollisionNodeData
        {
            get => NodeData as TtPhyConvexCollisionNodeData;
        }
        [Category("Option")]
        public RName ConvexSource
        {
            get => ConvexCollisionNodeData.ConvexSource;
            set
            {
                ConvexCollisionNodeData.ConvexSource = value;
                ChangeShape();
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            BoundingBox boundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
            TtMeshDataProvider mesh = TtMeshDataProvider.MakeBox(boundingBox);
            var convexMesh = pc.CookConvexMesh(mesh);
            System.Diagnostics.Debug.Assert(false); //need cook convex from mesh
            var shape = pc.CreateShapeConvex(PhyMaterial, convexMesh, in Vector3.Zero, in Quaternion.Identity);
            SetTriggerFlag(IsTrigger, shape);
            return shape;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("TriMeshCollision", "Collision\\TriMeshCollision", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyTriMeshCollisionNode.TtPhyTriMeshCollisionNodeData), DefaultNamePrefix = "TriMeshCollision")]
    public class TtPhyTriMeshCollisionNode : TtPhyCollisionNode
    {
        [Rtti.Meta("")]
        public class TtPhyTriMeshCollisionNodeData : TtPhyCollisionNodeData
        {
            [Rtti.Meta("")]
            public RName TriMeshSource { get; set; }
        }
        public TtPhyTriMeshCollisionNodeData TriMeshCollisionNodeData
        {
            get => NodeData as TtPhyTriMeshCollisionNodeData;
        }
        [Category("Option")]
        public RName TriMeshSource
        {
            get => TriMeshCollisionNodeData.TriMeshSource;
            set
            {
                TriMeshCollisionNodeData.TriMeshSource = value;
                ChangeShape();
            }
        }
        public override TtPhyShape CreatePhyShape()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            BoundingBox boundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
            TtMeshDataProvider mesh = TtMeshDataProvider.MakeBox(boundingBox);
            System.Diagnostics.Debug.Assert(false); //need cook convex from mesh
            var triMesh = pc.CookTriMesh(mesh, null, null, null);
            List<TtPhyMaterial> materials = new List<TtPhyMaterial>();
            var shape = pc.CreateShapeTriMesh(materials, triMesh, in Vector3.Zero, in Quaternion.Identity);
            SetTriggerFlag(IsTrigger, shape);
            return shape;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
namespace EngineNS.Bricks.PhysicsCore.CollisionComponent
{
    [Rtti.MetaClass]
    public class GPhysicsCapsuleCollisionInitializer : GPhysicsCollisionComponentInitializer
    {
        private float mRadius = 0.5f;
        [Rtti.MetaData]
        public float Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;

            }
        }
        private float mHalfHeight = 0.5f;
        [Rtti.MetaData]
        public float HalfHeight
        {
            get
            {
                return mHalfHeight;
            }
            set
            {
                mHalfHeight = value;

            }
        }
        [Rtti.MetaData]
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Capsule;
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPhysicsCapsuleCollisionInitializer), "胶囊体物理组件", "Collision", "CapsuleCollision")]
    public class GPhysicsCapsuleCollision : GPhysicsCollisionComponent
    {
        [Browsable(false)]
        public GPhysicsCapsuleCollisionInitializer CapsuleCollisionInitializer
        {
            get
            {
                var v = Initializer as GPhysicsCapsuleCollisionInitializer;
                if (v != null)
                    return v;
                return null;
            }
        }
        [Browsable(false)]
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Capsule;
        private float mRadius = 0.5f;
        public float Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;
                CapsuleCollisionInitializer.Radius = value;
                RefreshShape();
            }
        }
        private float mHalfHeight = 0.5f;
        public float HalfHeight
        {
            get
            {
                return mHalfHeight;
            }
            set
            {
                mHalfHeight = value;
                CapsuleCollisionInitializer.HalfHeight = value;
                RefreshShape();
            }
        }
        protected override void RefreshShape()
        {
            if (HostShape != null)
                HostShape.RemoveFraomActor();
            var mtl = CEngine.Instance.PhyContext.LoadMaterial(PhyMtlName);
            Vector3 worldLoc, worldScale;
            Quaternion worldQuat;
            HostPlaceable.Placement.WorldMatrix.Decompose(out worldScale, out worldQuat, out worldLoc);
            var shape = CEngine.Instance.PhyContext.CreateShapeCapsule(mtl, Radius * worldScale.X, HalfHeight * worldScale.X);
            if (shape == null)
                return;
            SetShapeData(shape);
            var relativePose = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(Placement.Location * worldScale, Placement.Rotation);
            shape.AddToActor(this.RigidBody, ref relativePose);
            //shape.ShapeDesc = sd;
            HostShape = shape;
            //是否成为动态阻挡
            shape.AreaType = AreaType;
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            var actor = CreateCPhyActor(Host, hostContainer, v);
            var init = v as GPhysicsCapsuleCollisionInitializer;
            mRadius  = init.Radius;
            mHalfHeight = init.HalfHeight;

            RefreshShape();
            return true;
        }
        public override void _OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CEngine.PhysicsDebug == false)
                return;
            if (mDebugActor == null && mReady == false)
            {
                mReady = true;
                var test = CreateDefaultCapsule();

                test = SetMaterial();
            }

            if (mDebugActor != null)
            {
                float r = 0.5f;
                float halfHeight = 0.5f;
                HostShape.IfGetCapsule(ref r,ref halfHeight);
                var trans = Placement.Transform * SocketMatrix * HostPlaceable.Placement.WorldMatrix;
                //mDebugActor.Placement.Location = RigidBody.Position;
                Vector3 scale, loc;//mDebugActor.Placement.Scale;
                Quaternion rotate;
                trans.Decompose(out scale, out rotate, out loc);
                var y = (r + halfHeight) * 2;
                scale.SetValue(scale.X * r * 2, scale.Y * y, scale.Z * r * 2);
                mDebugActor.Placement.Scale = scale;
            }
        }
    }
}

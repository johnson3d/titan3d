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
    public class GPhysicsSphereCollisionInitialize : GPhysicsCollisionComponentInitializer
    {
        private float mRadius = 1.0f;
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
        [Rtti.MetaData]
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Sphere;
    }
    [Editor.Editor_PlantAbleActor("Physics", "GPhysicsSphereCollision")]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPhysicsSphereCollisionInitialize), "球体物理组件", "Collision", "SphereCollision")]
    
    public class GPhysicsSphereCollision : GPhysicsCollisionComponent
    {
        [Browsable(false)]
        public GPhysicsSphereCollisionInitialize SphereCollisionInitializer
        {
            get
            {
                var v = Initializer as GPhysicsSphereCollisionInitialize;
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
        } = EPhysShapeType.PST_Sphere;
        private float mRadius = 1.0f;
        public float Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;
                SphereCollisionInitializer.Radius = value;
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
            var shape = CEngine.Instance.PhyContext.CreateShapeSphere(mtl, Radius * worldScale.X);
            if (shape == null)
                return;
            SetShapeData(shape);
            var relativePose = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(Placement.Location * worldScale, Placement.Rotation);
            shape.AddToActor(this.RigidBody, ref relativePose);
            HostShape = shape;
            //是否成为动态阻挡
            shape.AreaType = AreaType;
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            var actor = CreateCPhyActor(Host, hostContainer, v);
            var init = v as GPhysicsSphereCollisionInitialize;
            mRadius = init.Radius;
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
                var test = CreateDefaultSphere();
                test = SetMaterial();
            }

            if (mDebugActor != null)
            {
                float r = 0.5f;
                HostShape.IfGetSphere(ref r);
                var trans = Placement.Transform * SocketMatrix * HostPlaceable.Placement.WorldMatrix;
                //mDebugActor.Placement.Location = RigidBody.Position;
                Vector3 scale, loc;//mDebugActor.Placement.Scale;
                Quaternion rotate;
                trans.Decompose(out scale, out rotate, out loc);
                mDebugActor.Placement.Scale = Vector3.UnitXYZ * 2 * r;
            }
        }
    }
}

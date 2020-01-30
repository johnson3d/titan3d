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
    public class GPhysicsBoxCollisionInitializer : GPhysicsCollisionComponentInitializer
    {
        Vector3 mExtend = Vector3.UnitXYZ;
        [Rtti.MetaData]
        public Vector3 Extend
        {
            get => mExtend;
            set
            {
                mExtend = value;
            }
        }
        [Rtti.MetaData]
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Box;
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPhysicsBoxCollisionInitializer), "Box物理组件", "Collision", "BoxCollision")]
    public class GPhysicsBoxCollision : GPhysicsCollisionComponent
    {
        [Browsable(false)]
        public GPhysicsBoxCollisionInitializer BoxCollisionInitializer
        {
            get
            {
                var v = Initializer as GPhysicsBoxCollisionInitializer;
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
        } = EPhysShapeType.PST_Box;
        Vector3 mExtend = Vector3.UnitXYZ;
        public Vector3 Extend
        {
            get => mExtend;
            set
            {
                mExtend = value;
                BoxCollisionInitializer.Extend = value;
                RefreshShape();
            }
        }
        protected override void RefreshShape()
        {
            if(HostShape!= null)
                HostShape.RemoveFraomActor();
            var mtl = CEngine.Instance.PhyContext.LoadMaterial(PhyMtlName);
            Vector3 worldLoc, worldScale,ypr;
            Quaternion worldQuat;
            HostPlaceable.Placement.WorldMatrix.Decompose(out worldScale, out worldQuat, out worldLoc);
            worldQuat.GetYawPitchRoll(out ypr.Y, out ypr.X, out ypr.Z);
            var shape = CEngine.Instance.PhyContext.CreateShapeBox(mtl, Extend.X * Placement.Scale.X * worldScale.X, Extend.Y * Placement.Scale.Y * worldScale.Y, Extend.Z * Placement.Scale.Z* worldScale.Z);
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
            CreateCPhyActor(Host,hostContainer,v);
            var init = v as GPhysicsBoxCollisionInitializer;
            mExtend = init.Extend;
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
                var test = CreateDefaultBox();

                test = SetMaterial();
            }

            if (mDebugActor != null)
            {
                float w = 1.0f;
                float h = 1.0f;
                float l = 1.0f;
                HostShape.SDK_PhyShape_IfGetBox(ref w, ref h, ref l);
                var trans = Placement.Transform * SocketMatrix * HostPlaceable.Placement.WorldMatrix  ;
                //mDebugActor.Placement.Location = RigidBody.Position;
                Vector3 scale, loc;//mDebugActor.Placement.Scale;
                Quaternion rotate;
                trans.Decompose(out scale,out rotate,out loc);
                //scale.SetValue(scale.X * w, scale.Y * h, scale.Z * l);
                mDebugActor.Placement.Scale = scale;
            }
        }

    }
}

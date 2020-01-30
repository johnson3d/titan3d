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
    public class GPhysicsHeightFieldCollisionInitializer : GPhysicsCollisionComponentInitializer
    {
        private RName mHeightField;
        [Rtti.MetaData]
        public RName HeightField
        {
            get
            {
                return mHeightField;
            }
            set
            {
                mHeightField = value;

            }
        }
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPhysicsHeightFieldCollisionInitializer), "高度场物理组件", "Collision", "HeightFieldCollision")]
    public class GPhysicsHeightFieldCollision : GPhysicsCollisionComponent
    {
        [Browsable(false)]
        public GPhysicsHeightFieldCollisionInitializer HeightFieldCollisionInitializer
        {
            get
            {
                var v = Initializer as GPhysicsHeightFieldCollisionInitializer;
                if (v != null)
                    return v;
                return null;
            }
        }
        EPhysShapeType mShapeType = EPhysShapeType.PST_TriangleMesh;
        [Browsable(false)]
        public override EPhysShapeType ShapeType
        {
            get => mShapeType;
            set
            {
                mShapeType = value;
                HeightFieldCollisionInitializer.ShapeType = value;
            }
        }
        private RName mHeightField;
        [Rtti.MetaData]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyHeightFieldGeom)]
        [Editor.Editor_PackData()]
        public RName HeightField
        {
            get
            {
                return mHeightField;
            }
            set
            {
                mHeightField = value;
                HeightFieldCollisionInitializer.HeightField = value;
                RefreshShape();
            }
        }

        protected override void RefreshShape()
        {
            if (HeightField == null || string.IsNullOrEmpty(HeightField.Name))
                return;
            Action action = async () =>
            {
                if (HostShape != null)
                    HostShape.RemoveFraomActor();
                var mtl = CEngine.Instance.PhyContext.LoadMaterial(PhyMtlName);
                Vector3 worldLoc, worldScale;
                Quaternion worldQuat;
                HostPlaceable.Placement.WorldMatrix.Decompose(out worldScale, out worldQuat, out worldLoc);
                CPhyShape shape = null;

                shape = await CEngine.Instance.PhyContext.CreateShapeTriMesh(mtl, HeightField, Placement.Scale * worldScale, Quaternion.Identity);
                if (shape == null)
                    return;

                SetShapeData(shape);
                var relativePose = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(Placement.Location * worldScale, Placement.Rotation);
                shape.AddToActor(this.RigidBody, ref relativePose);
                //shape.ShapeDesc = sd;
                HostShape = shape;
                //是否成为动态阻挡
                shape.AreaType = AreaType;
                mDebugActor = null;
            };
            action.Invoke();

        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            var actor = CreateCPhyActor(Host, hostContainer, v);
            var init = v as GPhysicsHeightFieldCollisionInitializer;
            mHeightField = init.HeightField;
            mShapeType = init.ShapeType;

            RefreshShape();
            return true;
        }
        public override void _OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CEngine.PhysicsDebug == false || HostShape == null || HeightField == null || string.IsNullOrEmpty(HeightField.Name))
                return;
            var rc = CEngine.Instance.RenderContext;
            if (mDebugActor == null)
            {
                Graphics.Mesh.CGfxMeshPrimitives mp = HostShape.IfGetTriMesh(rc);
                if (mp != null)
                {
                    mDebugActor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(CEngine.Instance.MeshManager.CreateMesh(rc, mp));
                    EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                    var test = SetMaterial();
                }
            }
            if (mDebugActor != null)
            {
                var trans = Placement.Transform * SocketMatrix * HostPlaceable.Placement.WorldMatrix;
                //mDebugActor.Placement.Location = RigidBody.Position;
                Vector3 scale, loc;//mDebugActor.Placement.Scale;
                Quaternion rotate;
                trans.Decompose(out scale, out rotate, out loc);
                //scale.SetValue(scale.X * w, scale.Y * h, scale.Z * l);
                mDebugActor.Placement.Scale = scale;
            }
        }
    }
}

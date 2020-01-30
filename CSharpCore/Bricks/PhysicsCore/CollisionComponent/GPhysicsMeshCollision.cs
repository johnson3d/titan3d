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
    public class GPhysicsMeshCollisionInitializer : GPhysicsCollisionComponentInitializer
    {
        private RName mMeshName;
        [Rtti.MetaData]
        public RName MeshName
        {
            get
            {
                return mMeshName;
            }
            set
            {
                mMeshName = value;

            }
        }
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPhysicsMeshCollisionInitializer), "网格物理组件", "Collision", "TriangleMeshCollision")]
    public class GPhysicsMeshCollision : GPhysicsCollisionComponent
    {
        [Browsable(false)]
        public GPhysicsMeshCollisionInitializer MeshCollisionInitializer
        {
            get
            {
                var v = Initializer as GPhysicsMeshCollisionInitializer;
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
                MeshCollisionInitializer.ShapeType = value;
            }
        }
        private RName mTriangleMesh;
        [Rtti.MetaData]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyTriangleMeshGeom)]
        [Editor.Editor_PackData()]
        public RName TriangleMesh
        {
            get
            {
                return mTriangleMesh;
            }
            set
            {
                mTriangleMesh = value;
                MeshCollisionInitializer.MeshName = value;
                RefreshShape();
            }
        }

        protected override void RefreshShape()
        {
            if (TriangleMesh == null || string.IsNullOrEmpty(TriangleMesh.Name))
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

                shape = await CEngine.Instance.PhyContext.CreateShapeTriMesh(mtl, TriangleMesh, Placement.Scale * worldScale, Quaternion.Identity);
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
            var init = v as GPhysicsMeshCollisionInitializer;
            mTriangleMesh = init.MeshName;
            mShapeType = init.ShapeType;

            RefreshShape();
            return true;
        }
        public override void _OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CEngine.PhysicsDebug == false || HostShape == null || TriangleMesh == null || string.IsNullOrEmpty(TriangleMesh.Name))
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

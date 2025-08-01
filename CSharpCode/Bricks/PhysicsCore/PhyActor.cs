using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyActor : AuxPtrType<PhyActor>
    {
        public GamePlay.Scene.TtNode TagNode;
        public PhysicsCore.SceneNode.TtPhyRigidbodyNode RigidBodyNode;
        public unsafe TtPhyActor(PhyActor self)
        {
            mCoreObject = self;
            self.NativeSuper.BindObject(null, this);
        }
        public override void Dispose()
        {
            var scene = this.GetScene();
            if (scene!=null)
            {
                mCoreObject.RemoveFromScene(scene.mCoreObject);
                mCoreObject.NativeSuper.UnbindObject(scene);
            }
            else
            {
                mCoreObject.NativeSuper.UnbindObject(scene);
            }
            base.Dispose();
        }
        public unsafe static TtPhyActor GetActor(PhyActor actor)
        {
            return actor.NativeSuper.GetCSharpHandle() as TtPhyActor;
        }
        public List<TtPhyShape> Shapes { get; } = new List<TtPhyShape>();
        public bool AddToScene(TtPhyScene scene)
        {
            if (scene == null)
            {
                return mCoreObject.AddToScene(new PhyScene());
            }
            else
            {
                return mCoreObject.AddToScene(scene.mCoreObject);
            }
        }
        public void RemoveFromScene(TtPhyScene scene)
        {
            if (scene != null)
            {
                mCoreObject.RemoveFromScene(scene.mCoreObject);
            }
        }
        public TtPhyScene GetScene()
        {
            return mCoreObject.NativeSuper.GetCSharpHandle() as TtPhyScene;
        }
        public ref Vector3 Position
        {
            get
            {
                unsafe
                {
                    return ref *mCoreObject.GetPostion();
                }
            }
        }
        public ref Quaternion Rotation
        {
            get
            {
                unsafe
                {
                    return ref *mCoreObject.GetRotation();
                }
            }
        }
        public bool SetPose2Physics(in DVector3 p, in Quaternion q, bool autowake)
        {
            return mCoreObject.SetPose2Physics(p.ToSingleVector3(), in q, autowake);
        }
        public bool AttachShape(TtPhyShape shape, in Vector3 p, in Quaternion q)
        {
            return mCoreObject.AttachShape(shape.mCoreObject, in p, in q);
        }
        public void DetachShape(TtPhyShape shape, bool wakeOnLostTouch)
        {
            mCoreObject.DetachShape(shape.mCoreObject, wakeOnLostTouch);
        }
        public bool SetRigidBodyFlag(EPhyRigidBodyFlag flag, bool value)
        {
            return mCoreObject.SetRigidBodyFlag(flag, value);
        }
        public bool SetActorFlag(EPhyActorFlag flag, bool value)
        {
            return mCoreObject.SetActorFlag(flag, value);
        }
        public void AddShape(TtPhyShape shape, in Vector3 p, in Quaternion q)
        {
            if (Shapes.Contains(shape))
                return;
            Shapes.Add(shape);
            shape.mCoreObject.AddToActor(mCoreObject, in p, in q);
        }
        public void RemoveShape(TtPhyShape shape)
        {
            Shapes.Remove(shape);
            shape.mCoreObject.RemoveFromActor();
        }
    }
}

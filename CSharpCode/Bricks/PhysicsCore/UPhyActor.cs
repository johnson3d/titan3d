using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyActor : AuxPtrType<PhyActor>
    {
        public GamePlay.Scene.UNode TagNode;
        public TtPhyActor(PhyActor self)
        {
            mCoreObject = self;
            var gchandle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak);
            unsafe
            {
                var super = mCoreObject.NativeSuper;
                super.mCSharpHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(gchandle).ToPointer();
            }
        }
        ~TtPhyActor()
        {
            unsafe
            {
                var super = mCoreObject.NativeSuper;
                var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)super.mCSharpHandle);
                super.mCSharpHandle = (void*)0;
                gchandle.Free();
            }
        }
        public static TtPhyActor GetActor(PhyActor actor)
        {
            unsafe
            {
                var ptr = (IntPtr)actor.NativeSuper.mCSharpHandle;
                if (ptr == IntPtr.Zero)
                    return null;
                var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ptr);
                return gchandle.Target as TtPhyActor;
            }
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
        public TtPhyScene GetScene()
        {
            unsafe
            {
                var scene = mCoreObject.GetScene();
                if (scene.IsValidPointer == false)
                    return null;
                if (scene.NativeSuper.mCSharpHandle == (void*)0)
                {
                    return null;
                }
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)scene.NativeSuper.mCSharpHandle);
                return handle.Target as TtPhyScene;
            }
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

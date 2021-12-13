using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class UPhyActor : AuxPtrType<PhyActor>
    {
        public GamePlay.Scene.UNode TagNode;
        public UPhyActor(PhyActor self)
        {
            mCoreObject = self;
            var gchandle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak);
            unsafe
            {
                var super = mCoreObject.NativeSuper;
                super.mCSharpHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(gchandle).ToPointer();
            }
        }
        ~UPhyActor()
        {
            unsafe
            {
                var super = mCoreObject.NativeSuper;
                var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)super.mCSharpHandle);
                super.mCSharpHandle = (void*)0;
                gchandle.Free();
            }
        }
        public static UPhyActor GetActor(PhyActor actor)
        {
            unsafe
            {
                var ptr = (IntPtr)actor.NativeSuper.mCSharpHandle;
                if (ptr == IntPtr.Zero)
                    return null;
                var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ptr);
                return gchandle.Target as UPhyActor;
            }
        }
        public List<UPhyShape> Shapes { get; } = new List<UPhyShape>();
        public bool AddToScene(UPhyScene scene)
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
        public UPhyScene GetScene()
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
                return handle.Target as UPhyScene;
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
        public bool AttachShape(UPhyShape shape, in Vector3 p, in Quaternion q)
        {
            return mCoreObject.AttachShape(shape.mCoreObject, in p, in q);
        }
        public void DetachShape(UPhyShape shape, bool wakeOnLostTouch)
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
        public void AddShape(UPhyShape shape, in Vector3 p, in Quaternion q)
        {
            if (Shapes.Contains(shape))
                return;
            Shapes.Add(shape);
            shape.mCoreObject.AddToActor(mCoreObject, in p, in q);
        }
        public void RemoveShape(UPhyShape shape)
        {
            Shapes.Remove(shape);
            shape.mCoreObject.RemoveFromActor();
        }
    }
}

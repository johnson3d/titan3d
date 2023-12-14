using EngineNS.NxPhysics;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxPhysics
{
    public interface TtActor
    {
        EngineNS.NxPhysics.NxActor NativeActor { get; }
    }
    public class TtRigidBody : AuxPtrType<EngineNS.NxPhysics.NxRigidBody>, TtActor
    {
        public List<TtShape> Shapes { get; } = new List<TtShape>();
        public TtRigidBody(EngineNS.NxPhysics.NxRigidBody ptr)
        {
            mCoreObject = ptr;
            unsafe
            {
                var handle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak);
                var pBase0 = mCoreObject.NativeSuper;
                var pBase1 = pBase0.NativeSuper;
                pBase1.UserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(handle).ToPointer();
            }
        }
        ~TtRigidBody()
        {
            unsafe
            {
                var pBase0 = mCoreObject.NativeSuper;
                var pBase1 = pBase0.NativeSuper;
                pBase0.DetachScene();
                if (pBase1.UserData != IntPtr.Zero.ToPointer())
                {
                    var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)pBase1.UserData);
                    handle.Free();
                    pBase1.UserData = IntPtr.Zero.ToPointer();
                }
            }
        }
        public unsafe static TtRigidBody FromNativePtr(EngineNS.NxPhysics.NxRigidBody ptr)
        {
            var pBase0 = ptr.NativeSuper;
            var pBase1 = pBase0.NativeSuper;
            if (pBase1.UserData != IntPtr.Zero.ToPointer())
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)pBase1.UserData);
                return handle.Target as TtRigidBody;
            }
            return null;
        }
        public EngineNS.NxPhysics.NxActor NativeActor { get => mCoreObject.NativeSuper; }
        public ref PxVector3 Velocity
        {
            get
            {
                unsafe
                {
                    return ref *(mCoreObject.GetVelocity());
                }
            }
        }
        public void SetVelocity(in PxVector3 v)
        {
            mCoreObject.SetVelocity(in v);
        }
        public void SetAngularVelocity(in PxVector3 v)
        {
            mCoreObject.SetAngularVelocity(in v);
        }
        public void SetInertia(in PxVector3 v)
        {
            mCoreObject.SetInertia(in v);
        }
        public void AddShape(TtSphereShape shape)
        {
            if (Shapes.Contains(shape))
                return;
            Shapes.Add(shape);
            mCoreObject.AddShape(shape.NativeShape);
        }
    }
}

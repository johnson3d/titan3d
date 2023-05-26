using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public interface IPtrType
    {
        IntPtr NativePointer { get; set; }
    }
    public class AuxPtrType<T> : IDisposable where T : unmanaged, IPtrType
    {
        public T mCoreObject;
        ~AuxPtrType()
        {
            Dispose();
        }
        public bool IsDisposed
        {
            get
            {
                return mCoreObject.NativePointer == IntPtr.Zero;
            }
        }
        public virtual void Dispose()
        {
            unsafe
            {
                if (mCoreObject.NativePointer != IntPtr.Zero)
                {
                    var saved = mCoreObject.NativePointer;
                    mCoreObject.NativePointer = IntPtr.Zero;
                    
                    CoreSDK.IUnknown_Release(saved.ToPointer());
                }
            }
        }
        public int Core_AddRef()
        {
            unsafe
            {
                return CoreSDK.IUnknown_AddRef(mCoreObject.NativePointer.ToPointer());
            }
        }
        public void Core_Release()
        {
            unsafe
            {
                CoreSDK.IUnknown_Release(mCoreObject.NativePointer.ToPointer());
            }
        }
        public int Core_UnsafeGetRefCount()
        {
            unsafe
            {
                return CoreSDK.IUnknown_UnsafeGetRefCount(mCoreObject.NativePointer.ToPointer());
            }
        }
        public unsafe void Core_SetMemDebugInfo(string info)
        {
            CoreSDK.SetMemDebugInfo(mCoreObject.NativePointer.ToPointer(), info);
        }
    }
}

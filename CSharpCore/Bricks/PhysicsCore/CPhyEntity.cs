using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.PhysicsCore
{
    public enum PhyEntityType
    {
        PET_Context,
        PET_Scene,
        PET_Material,
        PET_Controller,
        PET_Actor,
        PET_Shape,
        Unknown
    };
    public class CPhyEntity : AuxCoreObject<CPhyEntity.NativePointer>, IDisposable
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public PhyEntityType EntityType
        {
            get { return SDK_PhyEntity_GetEntityType(CoreObject); }
        }
        System.Runtime.InteropServices.GCHandle mWeakHandle;
        public CPhyEntity(NativePointer self)
        {
            mCoreObject = self;
            mWeakHandle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.WeakTrackResurrection);
            SDK_PhyEntity_SetCSharpHandle(CoreObject, System.Runtime.InteropServices.GCHandle.ToIntPtr(mWeakHandle));
        }
        public static CPhyEntity PtrAsPhyEntity(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ptr);
            return handle.Target as CPhyEntity;
        }
        //public CPhyEntity(string nativeName)
        //{
        //    mCoreObject = NewNativeObjectByNativeName<NativePointer>(nativeName);
        //    mWeakHandle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.WeakTrackResurrection);
        //    SDK_PhyEntity_SetCSharpHandle(CoreObject, System.Runtime.InteropServices.GCHandle.ToIntPtr(mWeakHandle));
        //}
        ~CPhyEntity()
        {
            Dispose();
        }
        public override void Dispose()
        {
            if (mWeakHandle.IsAllocated)
            {
                SDK_PhyEntity_SetCSharpHandle(CoreObject, IntPtr.Zero);
                mWeakHandle.Free();
            }
            base.Dispose();
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyEntity_SetCSharpHandle(NativePointer self, IntPtr gcHandle);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_PhyEntity_GetCSharpHandle(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static PhyEntityType SDK_PhyEntity_GetEntityType(NativePointer self);
        #endregion
    }
}

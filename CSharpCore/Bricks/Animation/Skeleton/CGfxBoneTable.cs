using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.Animation.Skeleton
{
    public class CGfxBoneTable : AuxCoreObject<CGfxBoneTable.NativePointer>
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

        //public CGfxBoneTable()
        //{
        //    mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxBoneTable");
        //}
        //public CGfxBoneTable(NativePointer self)
        //{
        //    mCoreObject = self;
        //    Core_AddRef();
        //}
        //public CGfxBone Root
        //{
        //    get
        //    {
        //        var ptr = SDK_GfxBoneTable_GetRoot(CoreObject);
        //        if (ptr.Pointer == IntPtr.Zero)
        //            return null;
        //        return new CGfxBone(ptr);
        //    }
        //}
        //public bool SetRoot(string name)
        //{
        //    return (bool)SDK_GfxBoneTable_SetRoot(CoreObject, name);
        //}
        ////public CGfxBone FindBone(string name)
        ////{
        ////    var ptr = SDK_GfxBoneTable_FindBone(CoreObject, name);
        ////    if (ptr.Pointer == IntPtr.Zero)
        ////        return null;
        ////    return new CGfxBone(ptr);
        ////}
        //public UInt32 BoneNumber
        //{
        //    get
        //    {
        //        return SDK_GfxBoneTable_GetBoneNumber(CoreObject);
        //    }
        //}
        //public CGfxBone GetBone(UInt32 index)
        //{
        //    var ptr = SDK_GfxBoneTable_GetBone(CoreObject, index);
        //    if (ptr.Pointer == IntPtr.Zero)
        //        return null;
        //    return new CGfxBone(ptr);
        //}
        //public CGfxBone NewBone(CGfxBoneDesc desc)
        //{
        //    var result = new CGfxBone(SDK_GfxBoneTable_NewBone(CoreObject, desc.CoreObject));
        //    result.Core_Release();
        //    return result;
        //}
        //public void GenerateHierarchy()
        //{
        //    SDK_GfxBoneTable_GenerateHierarchy(CoreObject);
        //}
        //#region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static CGfxBone.NativePointer SDK_GfxBoneTable_GetRoot(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxBoneTable_SetRoot(NativePointer self, string name);

        ////[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        ////public extern static CGfxBone.NativePointer SDK_GfxBoneTable_FindBone(NativePointer self, string name);

        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxBoneTable_GetBoneNumber(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static CGfxBone.NativePointer SDK_GfxBoneTable_GetBone(NativePointer self, UInt32 index);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static CGfxBone.NativePointer SDK_GfxBoneTable_NewBone(NativePointer self, CGfxBoneDesc.NativePointer pBone);
        
        ////[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        ////public extern static void SDK_GfxBoneTable_GenerateHierarchy(NativePointer self);
        
        //#endregion
    }
}

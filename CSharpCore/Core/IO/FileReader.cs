using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.IO
{
    public class FileReader : FileBase
    {
        public FileReader()
        {
            mCoreObject = SDK_VFile_New();
        }
        public unsafe UIntPtr Read(void* lpBuf, UIntPtr nCount)
        {
            return SDK_VFile_Read(mCoreObject, lpBuf, nCount);
        }
        public bool OpenRead(string fileName)
        {
            if (SDK_VFile_Open(mCoreObject, fileName, (UInt32)(OpenFlags.modeRead)) == 0)
                return false;
            return true;
        }
    }
    public class Resource2Memory : AuxCoreObject<Resource2Memory.NativePointer>
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
        public EFileType mType = EFileType.Unknown;

        public Resource2Memory(NativePointer self, bool managed)
        {
            mCoreObject = self;
        }
        public UIntPtr GetLength()
        {
            return SDK_VRes2Memory_Length(CoreObject);
        }
        public string Name
        {
            get
            {
                return SDK_VRes2Memory_Name(CoreObject);
            }
        }
        IntPtr mMemPtr;
        public IntPtr NativePtr
        {
            get { return mMemPtr; }
        }
        public void BeginRead(uint offset = 0, int size = 0)
        {
            mMemPtr = SDK_VRes2Memory_Ptr(CoreObject, (UIntPtr)offset, (UIntPtr)size);
        }
        public void EndRead()
        {
            SDK_VRes2Memory_Free(CoreObject);
        }
        public void TryReleaseHolder()
        {
            SDK_VRes2Memory_TryReleaseHolder(CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_VRes2Memory_Ptr(NativePointer self, UIntPtr offset, UIntPtr size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_VRes2Memory_Free(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UIntPtr SDK_VRes2Memory_Length(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static string SDK_VRes2Memory_Name(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_VRes2Memory_TryReleaseHolder(NativePointer self);
        #endregion
    }
}

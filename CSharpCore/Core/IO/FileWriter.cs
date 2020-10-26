using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.IO
{
    public class FileBase
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

        protected NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }
        ~FileBase()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            if (CoreObject.Pointer != IntPtr.Zero)
            {
                SDK_VFile_Close(CoreObject);
                SDK_VFile_Delete(CoreObject);
                mCoreObject.Pointer = IntPtr.Zero;
            }
        }
        public void Close()
        {
            SDK_VFile_Close(mCoreObject);
        }
        public UIntPtr GetLength()
        {
            return SDK_VFile_GetLength(mCoreObject);
        }
        public enum ESeekPosition : uint
        {
            begin = 0x0,
            current = 0x1,
            end = 0x2
        }
        public UIntPtr Seek(UIntPtr lOff, ESeekPosition nFrom)
        {
            return SDK_VFile_Seek(CoreObject, lOff, nFrom);
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static FileBase.NativePointer SDK_VFile_New();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_VFile_Delete(FileBase.NativePointer vFile);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_VFile_Open(FileBase.NativePointer vFile, string fileName, UInt32 openFlags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_VFile_Close(FileBase.NativePointer vFile);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static UIntPtr SDK_VFile_Write(FileBase.NativePointer vFile, void* lpBuf, UIntPtr nCount);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static UIntPtr SDK_VFile_Read(FileBase.NativePointer vFile, void* lpBuf, UIntPtr nCount);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UIntPtr SDK_VFile_GetLength(FileBase.NativePointer vFile);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UIntPtr SDK_VFile_Seek(FileBase.NativePointer vFile, UIntPtr lOff, ESeekPosition nFrom);
        #endregion
    }
    public class FileWriter : FileBase
    {
        public FileWriter()
        {
            mCoreObject = SDK_VFile_New();
        }

        public bool OpenWrite(string fileName)
        {
            if (SDK_VFile_Open(mCoreObject, fileName, (UInt32)(OpenFlags.modeWrite | OpenFlags.modeCreate)) == 0)
                return false;
            return true;
        }
        
        public unsafe UIntPtr Write(void* lpBuf, UIntPtr nCount)
        {
            return SDK_VFile_Write(mCoreObject, lpBuf, nCount);
        }
    }
}

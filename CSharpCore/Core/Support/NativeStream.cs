using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Support
{
    public struct NativeStreamWriter
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
        public NativePointer CoreObject;
        public NativeStreamWriter(UInt32 reserve)
        {
            CoreObject = SDK_CsStreamWriter_New();
            SDK_CsStreamWriter_Reserve(CoreObject, reserve);
        }
        public void Dispose()
        {
            if (CoreObject.GetPointer() != IntPtr.Zero)
            {
                CoreObjectBase.SDK_VIUnknown_Release(CoreObject.GetPointer());
                CoreObject.SetPointer(IntPtr.Zero);
            }
        }
        public unsafe void PushData(byte* p, UInt32 size)
        {
            SDK_CsStreamWriter_PushData(CoreObject, p, size);
        }
        public void Clear()
        {
            SDK_CsStreamWriter_Clear(CoreObject);
        }
        public IntPtr Pointer
        {
            get
            {
                unsafe
                {
                    return (IntPtr)SDK_CsStreamWriter_GetPointer(CoreObject);
                }
            }
        }
        public UInt32 Size
        {
            get
            {
                unsafe
                {
                    return SDK_CsStreamWriter_GetSize(CoreObject);
                }
            }
        }
        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_CsStreamWriter_New();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsStreamWriter_Reserve(NativePointer self, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_CsStreamWriter_PushData(NativePointer self, byte* p, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsStreamWriter_Clear(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe byte* SDK_CsStreamWriter_GetPointer(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CsStreamWriter_GetSize(NativePointer self);
        #endregion
    }

    public struct NativeStreamReader
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
        public NativePointer CoreObject;
        public NativeStreamReader(IntPtr ptr, UInt32 size)
        {
            CoreObject = SDK_CsStreamReader_New();
            unsafe
            {
                SDK_CsStreamReader_InitData(CoreObject, (byte*)ptr, size);
            }
        }
        public void Dispose()
        {
            if (CoreObject.GetPointer() != IntPtr.Zero)
            {
                CoreObjectBase.SDK_VIUnknown_Release(CoreObject.GetPointer());
                CoreObject.SetPointer(IntPtr.Zero);
            }
        }
        public IntPtr Pointer
        {
            get
            {
                unsafe
                {
                    return (IntPtr)SDK_CsStreamReader_GetPointer(CoreObject);
                }
            }
        }
        public UInt32 Size
        {
            get
            {
                unsafe
                {
                    return SDK_CsStreamReader_GetSize(CoreObject);
                }
            }
        }
        public UInt32 Position
        {
            get
            {
                return SDK_CsStreamReader_GetPosition(CoreObject);
            }
        }
        public unsafe bool Read(byte* ptr, UInt32 size)
        {
            return SDK_CsStreamReader_Read(CoreObject, ptr, size);
        }
        public void ResetReader()
        {
            SDK_CsStreamReader_ResetReader(CoreObject);
        }
        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_CsStreamReader_New();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_CsStreamReader_InitData(NativePointer self, byte* p, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe byte* SDK_CsStreamReader_GetPointer(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CsStreamReader_GetSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_CsStreamReader_Read(NativePointer self, byte* ptr, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsStreamReader_ResetReader(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CsStreamReader_GetPosition(NativePointer self);
        #endregion
    }
}

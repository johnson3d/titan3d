using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.TexCompressor
{
    public struct ETCLayer
    {
        public int Width;
        public int Height;
        public UInt32 Size;
    }
    public class CTexCompressor : AuxCoreObject<CTexCompressor.NativePointer>
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

        public CTexCompressor()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("TexCompressor");
        }
        
        public bool EncodePng2ETC(IntPtr ptr, UInt32 size, ETCFormat fmt, int mipmap, Support.CBlobProxy2 blob)
        {
            return SDK_TexCompressor_EncodePng2ETC(CoreObject, ptr, size, fmt, mipmap, blob.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_TexCompressor_EncodePng2ETC(NativePointer self, IntPtr ptr, UInt32 size, ETCFormat fmt, int mipmap, Support.CBlobObject.NativePointer blob);
        #endregion
    }
}

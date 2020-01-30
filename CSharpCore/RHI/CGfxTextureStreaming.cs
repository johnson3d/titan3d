using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class CGfxTextureStreaming : AuxCoreObject<CGfxTextureStreaming.NativePointer>
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
        public CGfxTextureStreaming(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        public int FullMipCount
        {
            get
            {
                return SDK_GfxTextureStreaming_GetFullMipCount(CoreObject);
            }
        }
        public int LoadedMipCount
        {
            get
            {
                return SDK_GfxTextureStreaming_GetLoadedMipCount(CoreObject);
            }
        }
        //public bool LoadAllMips(CRenderContext rc, CShaderResourceView srv)
        //{
        //    return SDK_GfxTextureStreaming_LoadAllMips(CoreObject, rc.CoreObject, srv.CoreObject);
        //}
        public bool LoadNextMip(CRenderContext rc, CShaderResourceView srv)
        {
            return SDK_GfxTextureStreaming_LoadNextMip(CoreObject, rc.CoreObject, srv.CoreObject);
        }
        public enum EPixelCompressMode : int
        {
            None,
            ETC2,
            ASTC,
        };
        static EPixelCompressMode mTryCompressFormat = EPixelCompressMode.None;
        public static EPixelCompressMode TryCompressFormat
        {
            get
            {
                return mTryCompressFormat;
            }
            set
            {
                mTryCompressFormat = value;
                SDK_GfxTextureStreaming_TryCompressFormat(value);
            }
        }
        #region SDK                                                     
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxTextureStreaming_TryCompressFormat(EPixelCompressMode compressMode);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxTextureStreaming_GetFullMipCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxTextureStreaming_GetLoadedMipCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxTextureStreaming_LoadAllMips(NativePointer self, CRenderContext.NativePointer rc, CShaderResourceView.NativePointer srv);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxTextureStreaming_LoadNextMip(NativePointer self, CRenderContext.NativePointer rc, CShaderResourceView.NativePointer srv);
        #endregion
    }
}

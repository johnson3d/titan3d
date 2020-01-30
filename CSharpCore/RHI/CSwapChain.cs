using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public struct CSwapChainDesc
    {
        public UInt32 Width;
        public UInt32 Height;
        public EPixelFormat Format;
        public EColorSpace ColorSpace;
        public IntPtr WindowHandle;
    }
    public class CSwapChain : AuxCoreObject<CSwapChain.NativePointer>
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
        public CSwapChain(NativePointer self)
        {
            mCoreObject = self;
            var tex = SDK_ISwapChain_GetTexture2D(CoreObject);
            mTexture2D = new CTexture2D(tex);
            if (tex.Pointer != IntPtr.Zero)
            {
                SDK_VIUnknown_AddRef(mTexture2D.CoreObject.Pointer);
            }
        }

        protected CTexture2D mTexture2D;
        public CTexture2D Texture2D
        {
            get { return mTexture2D; }
        }
        public void GetDesc(ref CSwapChainDesc desc)
        {
            unsafe
            {
                fixed (CSwapChainDesc* p = &desc)
                {
                    SDK_ISwapChain_GetDesc(CoreObject, p);
                }
            }
        }
        public void Present(UInt32 SyncInterval = 1, UInt32 Flags = 0)
        {
            SDK_ISwapChain_Present(CoreObject, SyncInterval, Flags);
        }
        public void OnLost()
        {
            SDK_ISwapChain_OnLost(CoreObject);
        }
        public bool OnRestore(ref CSwapChainDesc desc)
        {
            unsafe
            {
                fixed(CSwapChainDesc* p = &desc)
                {
                    return (bool)SDK_ISwapChain_OnRestore(CoreObject, p);
                }
            }
        }

        public void OnResize(UInt32 width, UInt32 height)
        {
            CSwapChainDesc desc = new CSwapChainDesc();
            GetDesc(ref desc);
            OnLost();
            mTexture2D?.Cleanup();
            desc.Width = width;
            desc.Height = height;
            OnRestore(ref desc);
            var tex = SDK_ISwapChain_GetTexture2D(CoreObject);
            mTexture2D = new CTexture2D(tex);
            if (tex.Pointer != IntPtr.Zero)
            {
                SDK_VIUnknown_AddRef(mTexture2D.CoreObject.Pointer);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CTexture2D.NativePointer SDK_ISwapChain_GetTexture2D(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ISwapChain_OnLost(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_ISwapChain_OnRestore(NativePointer self, CSwapChainDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ISwapChain_GetDesc(NativePointer self, CSwapChainDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ISwapChain_Present(NativePointer self, UInt32 SyncInterval, UInt32 Flags);
        #endregion
    }
}

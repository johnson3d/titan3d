using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public struct ImageInitData
    {
        public Support.CBlobObject.NativePointer pSysMem;
        public UInt32 SysMemPitch;
        public UInt32 SysMemSlicePitch;
    }
    public struct CTexture2DDesc
    {
        public void Init()
        {
            Width = 0;
            Height = 0;
            MipLevels = 1;
            ArraySize = 1;
            Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            BindFlags = (UInt32)EBindFlags.BF_SHADER_RES;
            CPUAccess = 0;// CAS_WRITE | CAS_READ;
        }
        public UInt32 Width;
        public UInt32 Height;
        public UInt32 MipLevels;
        public UInt32 ArraySize;
        public EPixelFormat Format;
        public UInt32 BindFlags;
        public UInt32 CPUAccess;
        public ImageInitData InitData;
    }
    public class CTexture2D : AuxCoreObject<CTexture2D.NativePointer>
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

        public CTexture2D(NativePointer self)
        {
            mCoreObject = self;
        }
        public void UnsafeSetNativePointer(NativePointer self)
        {
            mCoreObject = self;
        }
        public unsafe bool Map(CCommandList cmd, int MipLevel, void** ppData, uint* pRowPitch, uint* pDepthPitch)
        {
            return SDK_ITexture2D_Map(CoreObject, cmd.CoreObject, MipLevel, ppData, pRowPitch, pDepthPitch);
        }
        public void Unmap(CCommandList cmd, int MipLevel)
        {
            SDK_ITexture2D_Unmap(CoreObject, cmd.CoreObject, MipLevel);
        }
        public unsafe void BuildImageBlob(Support.CBlobObject blob, void* pData, uint RowPitch)
        {
            SDK_ITexture2D_BuildImageBlob(CoreObject, blob.CoreObject, pData, RowPitch);
        }
        public unsafe void UpdateMipData(CCommandList cmd, uint level, void* pData, uint width, uint height, uint pitch)
        {
            SDK_ITexture2D_UpdateMipData(CoreObject, cmd.CoreObject, level, pData, width, height, pitch);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static vBOOL SDK_ITexture2D_Map(NativePointer self, CCommandList.NativePointer cmd, int MipLevel, void** ppData, uint* pRowPitch, uint* pDepthPitch);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static void SDK_ITexture2D_Unmap(NativePointer self, CCommandList.NativePointer cmd, int MipLevel);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static void SDK_ITexture2D_BuildImageBlob(NativePointer self, Support.CBlobObject.NativePointer blob, void* pData, uint RowPitch);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static void SDK_ITexture2D_UpdateMipData(NativePointer self, CCommandList.NativePointer cmd, uint level, void* pData, uint width, uint height, uint pitch);
        #endregion
    }
}

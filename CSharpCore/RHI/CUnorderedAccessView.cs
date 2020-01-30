using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EGpuUsage : int
    {
        USAGE_DEFAULT = 0,
        USAGE_IMMUTABLE = 1,
        USAGE_DYNAMIC = 2,
        USAGE_STAGING = 3
    }
    [Flags]
    public enum EBindFlag
    {
        VERTEX_BUFFER = 0x1,
        INDEX_BUFFER = 0x2,
        CONSTANT_BUFFER = 0x4,
        SHADER_RESOURCE = 0x8,
        STREAM_OUTPUT = 0x10,
        RENDER_TARGET = 0x20,
        DEPTH_STENCIL = 0x40,
        UNORDERED_ACCESS = 0x80,
        DECODER = 0x200,
        VIDEO_ENCODER = 0x400
    }
    [Flags]
    public enum EResourceMiscFlag
    {
        GENERATE_MIPS = 0x1,
        SHARED = 0x2,
        TEXTURECUBE = 0x4,
        DRAWINDIRECT_ARGS = 0x10,
        BUFFER_ALLOW_RAW_VIEWS = 0x20,
        BUFFER_STRUCTURED = 0x40,
        RESOURCE_CLAMP = 0x80,
        SHARED_KEYEDMUTEX = 0x100,
        GDI_COMPATIBLE = 0x200,
        SHARED_NTHANDLE = 0x800,
        RESTRICTED_CONTENT = 0x1000,
        RESTRICT_SHARED_RESOURCE = 0x2000,
        RESTRICT_SHARED_RESOURCE_DRIVER = 0x4000,
        GUARDED = 0x8000,
        TILE_POOL = 0x20000,
        TILED = 0x40000,
        HW_PROTECTED = 0x80000
    }
    [Flags]
    public enum EUAVBufferFlag
    {
        UAV_FLAG_RAW = 0x1,
        UAV_FLAG_APPEND = 0x2,
        UAV_FLAG_COUNTER = 0x4
    }
    public struct CGpuBufferDesc
    {
        public void SetMode(bool isCpuWritable, bool isGpuWritable)
        {
            if ((!isCpuWritable) && (!isGpuWritable))
            {
                CPUAccessFlags = 0;
                BindFlags = (UInt32)(EBindFlag.SHADER_RESOURCE);
                Usage = EGpuUsage.USAGE_IMMUTABLE;
            }
            else if (isCpuWritable && (!isGpuWritable))
            {
                CPUAccessFlags = (UInt32)ECpuAccess.CAS_WRITE;
                BindFlags = (UInt32)(EBindFlag.SHADER_RESOURCE);
                Usage = EGpuUsage.USAGE_DYNAMIC;
            }
            else if ((!isCpuWritable) && isGpuWritable)
            {
                CPUAccessFlags = 0;
                BindFlags = (UInt32)(EBindFlag.UNORDERED_ACCESS | EBindFlag.SHADER_RESOURCE);
                Usage = EGpuUsage.USAGE_DEFAULT;
            }
            else
            {
                System.Diagnostics.Debug.Assert((!(isCpuWritable && isGpuWritable)));
            }
            MiscFlags = (UInt32)EResourceMiscFlag.BUFFER_STRUCTURED;
        }
        public UInt32 ByteWidth;
        public EGpuUsage Usage;
        public UInt32 BindFlags;
        public UInt32 CPUAccessFlags;
        public UInt32 MiscFlags;
        public UInt32 StructureByteStride;
    }

    public enum EGpuMAP
    {
        MAP_READ = 1,
        MAP_WRITE = 2,
        MAP_READ_WRITE = 3,
        MAP_WRITE_DISCARD = 4,
        MAP_WRITE_NO_OVERWRITE = 5
    };

    public struct CMappedSubResource
    {
        public IntPtr pData;
        public UInt32 RowPitch;
        public UInt32 DepthPitch;
    };

    public class CGpuBuffer : AuxCoreObject<CGpuBuffer.NativePointer>
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
        public CGpuBuffer(NativePointer self)
        {
            mCoreObject = self;
        }

        public bool GetBufferData(CRenderContext rc, Support.CBlobObject blob)
        {
            return SDK_IGpuBuffer_GetBufferData(CoreObject, rc.CoreObject, blob.CoreObject);
        }
        public bool UpdateBufferData(CCommandList cmd, IntPtr data, UInt32 size)
        {
            lock (cmd)
            {
                return SDK_IGpuBuffer_UpdateBufferData(CoreObject, cmd.CoreObject, data, size);
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IGpuBuffer_GetBufferData(NativePointer self, CRenderContext.NativePointer rc, Support.CBlobObject.NativePointer blob);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IGpuBuffer_Map(NativePointer self, 
            CRenderContext.NativePointer rc, 
            UInt32 Subresource, 
            EGpuMAP MapType,
            UInt32 MapFlags,
            CMappedSubResource* mapRes);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IGpuBuffer_Unmap(NativePointer self, CRenderContext.NativePointer rc, UInt32 Subresource);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IGpuBuffer_UpdateBufferData(NativePointer self, CCommandList.NativePointer cmd, IntPtr data, UInt32 size);
        #endregion
    }

    #region For SRV Desc
    public enum EResourceDimension
    {
        RESOURCE_DIMENSION_UNKNOWN = 0,
        RESOURCE_DIMENSION_BUFFER = 1,
        RESOURCE_DIMENSION_TEXTURE1D = 2,
        RESOURCE_DIMENSION_TEXTURE2D = 3,
        RESOURCE_DIMENSION_TEXTURE3D = 4
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct BUFFER_SRV
    {
        [FieldOffset(0)]
        public UInt32 FirstElement;
        [FieldOffset(0)]
        public UInt32 ElementOffset;

        [FieldOffset(4)]
        public UInt32 NumElements;
        [FieldOffset(4)]
        public UInt32 ElementWidth;
    }

    public struct TEX1D_SRV
    {
        public UInt32 MostDetailedMip;
        public UInt32 MipLevels;
    }

    public struct TEX1D_ARRAY_SRV
    {
        public UInt32 MostDetailedMip;
        public UInt32 MipLevels;
        public UInt32 FirstArraySlice;
        public UInt32 ArraySize;
    };
    public struct TEX2D_SRV
    {
        public UInt32 MostDetailedMip;
        public UInt32 MipLevels;
    }

    public struct TEX2D_ARRAY_SRV
    {
        public UInt32 MostDetailedMip;
        public UInt32 MipLevels;
        public UInt32 FirstArraySlice;
        public UInt32 ArraySize;
    }
    public struct TEX2DMS_SRV
    {
        public UInt32 UnusedField_NothingToDefine;
    }

    public struct TEX2DMS_ARRAY_SRV
    {
        public UInt32 FirstArraySlice;
        public UInt32 ArraySize;
    }

    public struct TEX3D_SRV
    {
        public UInt32 MostDetailedMip;
        public UInt32 MipLevels;
    }

    public struct TEXCUBE_SRV
    {
        public UInt32 MostDetailedMip;
        public UInt32 MipLevels;
    }

    public struct TEXCUBE_ARRAY_SRV
    {
        public UInt32 MostDetailedMip;
        public UInt32 MipLevels;
        public UInt32 First2DArrayFace;
        public UInt32 NumCubes;
    }

    public struct BUFFEREX_SRV
    {
        public UInt32 FirstElement;
        public UInt32 NumElements;
        public UInt32 Flags;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct ISRVDesc
    {
        public void ToDefault()
        {
            Format = EPixelFormat.PXF_UNKNOWN;
            ViewDimension = EResourceDimension.RESOURCE_DIMENSION_BUFFER;
            Buffer.FirstElement = 0;
        }
        [FieldOffset(0)]
        public EPixelFormat Format;
        [FieldOffset(4)]
        public EResourceDimension ViewDimension;
        //union
        //{
        [FieldOffset(8)]
        public BUFFER_SRV Buffer;
        [FieldOffset(8)]
        public TEX1D_SRV Texture1D;
        [FieldOffset(8)]
        public TEX1D_ARRAY_SRV Texture1DArray;
        [FieldOffset(8)]
        public TEX2D_SRV Texture2D;
        [FieldOffset(8)]
        public TEX2D_ARRAY_SRV Texture2DArray;
        [FieldOffset(8)]
        public TEX2DMS_SRV Texture2DMS;
        [FieldOffset(8)]
        public TEX2DMS_ARRAY_SRV Texture2DMSArray;
        [FieldOffset(8)]
        public TEX3D_SRV Texture3D;
        [FieldOffset(8)]
        public TEXCUBE_SRV TextureCube;
        [FieldOffset(8)]
        public TEXCUBE_ARRAY_SRV TextureCubeArray;
        [FieldOffset(8)]
        public BUFFEREX_SRV BufferEx;
        //};
    }
    #endregion

    public struct CBufferUAV
    {
        public UInt32 FirstElement;
        public UInt32 NumElements;
        public UInt32 Flags;
    }

    public struct CTex1DUAV
    {
        public UInt32 MipSlice;
    }

    public struct CTex1DArrayUAV
    {
        public UInt32 MipSlice;
        public UInt32 FirstArraySlice;
        public UInt32 ArraySize;
    }

    public struct CTex2DUAV
    {
        public UInt32 MipSlice;
    }

    public struct CTex2DArrayUAV
    {
        public UInt32 MipSlice;
        public UInt32 FirstArraySlice;
        public UInt32 ArraySize;
    }

    public struct CTex3DUAV
    {
        public UInt32 MipSlice;
        public UInt32 FirstWSlice;
        public UInt32 WSize;
    }

    public enum EDimensionUAV
    {
        UAV_DIMENSION_UNKNOWN = 0,
        UAV_DIMENSION_BUFFER = 1,
        UAV_DIMENSION_TEXTURE1D = 2,
        UAV_DIMENSION_TEXTURE1DARRAY = 3,
        UAV_DIMENSION_TEXTURE2D = 4,
        UAV_DIMENSION_TEXTURE2DARRAY = 5,
        UAV_DIMENSION_TEXTURE3D = 8,
        UAV_DIMENSION_BUFFEREX = 11,
    };

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct CUnorderedAccessViewDesc
    {
        public void ToDefault()
        {
            Format = EPixelFormat.PXF_UNKNOWN;
            ViewDimension = EDimensionUAV.UAV_DIMENSION_BUFFER;
            //MiscFlags = (UInt32)EResourceMiscFlag.BUFFER_STRUCTURED;
            Buffer.FirstElement = 0;
            //Buffer.NumElements = descBuf.ByteWidth / descBuf.StructureByteStride;
        }
        [FieldOffset(0)]
        public EPixelFormat Format;
        [FieldOffset(4)]
        public EDimensionUAV ViewDimension;
        //union
        //{
        [FieldOffset(8)]
        public CBufferUAV Buffer;
        [FieldOffset(8)]
        public CTex1DUAV Texture1D;
        [FieldOffset(8)]
        public CTex1DArrayUAV Texture1DArray;
        [FieldOffset(8)]
        public CTex2DUAV Texture2D;
        [FieldOffset(8)]
        public CTex2DArrayUAV Texture2DArray;
        [FieldOffset(8)]
        public CTex3DUAV Texture3D;
        //};
    }

    public partial class CUnorderedAccessView : AuxCoreObject<CUnorderedAccessView.NativePointer>
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
        public CUnorderedAccessView(NativePointer self)
        {
            mCoreObject = self;
        }
    }
}

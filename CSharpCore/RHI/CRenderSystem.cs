using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum ERHIType
    {
        RHT_VirtualDevice,
        RHT_D3D11,
        RHT_VULKAN,
        RHT_OGL,
        RHIType_Metal,
    };

    public struct CRenderSystemDesc
    {
        public void SetDefault()
        {
            CreateDebugLayer = vBOOL.FromBoolean(true);
            WindowHandle = IntPtr.Zero;
        }
        public vBOOL CreateDebugLayer;
        public IntPtr WindowHandle;
    };

    public unsafe struct CRenderContextDesc
    {
        public void SetDefault()
        {
            AdapterId = 0;
            AppHandle = IntPtr.Zero;
            DeviceId = 0;
            VendorId = 0;
            Revision = 0;
            VideoMemory = 0;
            SysMemory = 0;
            SharedMemory = 0;
            CreateDebugLayer = vBOOL.FromBoolean(false);
            //WindowHandle = nullptr;
            //Width = 0;
            //Height = 0;
        }
        public int AdapterId;
        public UInt32 DeviceId;
        public UInt32 VendorId;
        public UInt32 Revision;
        public UInt64 VideoMemory;
        public UInt64 SysMemory;
        public UInt64 SharedMemory;
        public IntPtr AppHandle;
        public vBOOL CreateDebugLayer;
        public fixed byte DeviceName[256];
        public string DeviceNameString
        {
            get
            {
                unsafe
                {
                    fixed (byte* p = &DeviceName[0])
                    {
                        return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)p);
                    }
                }
            }
        }
    }

    public class CRenderSystem : AuxCoreObject<CRenderSystem.NativePointer>
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
        ERHIType mRHIType;
        public ERHIType RHIType
        {
            get { return mRHIType; }
        }
        public bool Init(ERHIType type, CRenderSystemDesc desc)
        {
            mRHIType = type;
            if (mCoreObject.Pointer!=IntPtr.Zero)
                return false;

            unsafe
            {
                mCoreObject = SDK_CreateRenderSystem(type, &desc);
            }
            return true;
        }

        public CRenderContext CreateRenderContext(UInt32 Adapter, IntPtr windows, bool createDebugLayer)
        {
            var context = SDK_IRenderSystem_CreateContext(CoreObject, Adapter, windows, vBOOL.FromBoolean(createDebugLayer));
            if (context.Pointer == IntPtr.Zero)
                return null;
            return new CRenderContext(context);
        }
        public UInt32 GetContextNumber()
        {
            return SDK_IRenderSystem_GetContextNumber(CoreObject);
        }
        public unsafe CRenderContextDesc GetContextDesc(UInt32 index)
        {
            CRenderContextDesc desc = new CRenderContextDesc();
            desc.SetDefault();
            if (false == SDK_IRenderSystem_GetContextDesc(CoreObject, index, &desc))
            {
                return desc;
            }
            return desc;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe NativePointer SDK_CreateRenderSystem(ERHIType type, CRenderSystemDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CRenderContext.NativePointer SDK_IRenderSystem_CreateContext(NativePointer self, UInt32 Adapter, IntPtr windows, vBOOL createDebugLayer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IRenderSystem_GetContextNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IRenderSystem_GetContextDesc(NativePointer self, UInt32 index, void* desc);
        #endregion
    }
}

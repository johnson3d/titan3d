using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EIMAGE_FILE_FORMAT
    {
        BMP = 0,
        JPG = 1,
        PNG = 3,
        DDS = 4,
        TIFF = 10,
        GIF = 11,
    };
    public enum ETCFormat
    {
        UNKNOWN,
        //
        ETC1,
        //
        // ETC2 formats
        RGB8,
        SRGB8,
        RGBA8,
        SRGBA8,
        R11,
        SIGNED_R11,
        RG11,
        SIGNED_RG11,
        RGB8A1,
        SRGB8A1,
        //
        FORMATS,
        //
        DEFAULT = SRGB8
    }
    public struct CTxPicDesc
    {
        public void SetDefault()
        {
            sRGB = 1;
            EtcFormat = ETCFormat.RGBA8;
            MipLevel = 0;
            Width = 0;
            Height = 0;
        }
        public int sRGB;
        public ETCFormat EtcFormat;
        public int MipLevel;
        public int Width;
        public int Height;
    };

    public struct CShaderResourceViewDesc
    {
        public void Init()
        {
            mFormat = EPixelFormat.PXF_R8G8B8A8_UNORM;
            mTexture2D = CTexture2D.GetEmptyNativePointer();
        }
        public EPixelFormat mFormat;
        public CTexture2D.NativePointer mTexture2D;
    }
    public partial class CShaderResourceView : AuxCoreObject<CShaderResourceView.NativePointer>
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

        public CShaderResourceView(NativePointer self)
        {
            mCoreObject = self;
            if (self.Pointer != IntPtr.Zero)
            {
                mResourceState = new CResourceState(SDK_VIUnknown_GetResourceState(self.Pointer));
                mTexSteaming = new CGfxTextureStreaming(SDK_IShaderResourceView_GetTexStreaming(self));
            }
        }

        private CShaderResourceView()
        {

        }

        EngineNS.RName mName;
        public RName Name
        {
            get => mName;
            set => mName = value;
        }

        public CTxPicDesc TxPicDesc
        {
            get
            {
                var result = new CTxPicDesc();
                unsafe
                {
                    SDK_IShaderResourceView_GetTxDesc(CoreObject, &result);
                }
                return result;
            }
        }
        public EPixelFormat TextureFormat
        {
            get
            {
                return SDK_IShaderResourceView_GetTextureFormat(CoreObject);
            }
        }
        public CShaderResourceView CloneTexture()
        {
            var result = new CShaderResourceView();
            result.Name = mName;
            result.mCoreObject = CoreObject;
            result.mResourceState = ResourceState;
            result.Core_AddRef();
            return result;
        }

        private CResourceState mResourceState;
        public CResourceState ResourceState
        {
            get { return mResourceState; }
        }
        private CGfxTextureStreaming mTexSteaming;
        public CGfxTextureStreaming TexSteaming
        {
            get { return mTexSteaming; }
        }

        public override string ToString()
        {
            return mName.Name;
        }
        
        public bool Save2Memory(CRenderContext rc, Support.CBlobObject data, EIMAGE_FILE_FORMAT Type)
        {
            return (bool)SDK_IShaderResourceView_Save2Memory(CoreObject, rc.CoreObject, data.CoreObject, Type);
        }

        public bool GetTexture2DData(CRenderContext rc, Support.CBlobObject data, int MipLevel, int CpuTexWidth, int CpuTexHeight)
        {
            return SDK_IShaderResourceView_GetTexture2DData(CoreObject, rc.CoreObject, data.CoreObject, MipLevel, CpuTexWidth, CpuTexHeight);
        }

        public void RefreshResource()
        {
            SDK_IShaderResourceView_RefreshResource(CoreObject);
        }
        public void PreUse(bool isSync = false)
        {
            ResourceState.AccessTime = CEngine.Instance.EngineTime;
            if (Thread.ContextThread.CurrentContext == CEngine.Instance.ThreadAsync)
            {
                PreUseInAsyncThread(isSync);
                return;
            }
            lock (this)
            {
                switch (ResourceState.StreamState)
                {
                    case EStreamingState.SS_Valid:
                        {
                            return;
                        }
                    case EStreamingState.SS_Invalid:
                        {
                            if (isSync)
                            {
                                //ResourceState.StreamState = EStreamingState.SS_Streaming;
                                //if (false == this.RestoreResource())
                                //{
                                //    ResourceState.StreamState = EStreamingState.SS_Invalid;
                                //}
                                //else
                                //{
                                //    ResourceState.StreamState = EStreamingState.SS_Valid;
                                //}
                                WaitAsyncIO();
                            }
                        }
                        break;
                    case EStreamingState.SS_Pending:
                        {
                            if (isSync)
                            {
                                WaitAsyncIO();
                            }
                        }
                        break;
                    case EStreamingState.SS_Streaming:
                        {
                            if (isSync)
                            {
                                WaitAsyncIO();
                            }
                        }
                        break;
                    case EStreamingState.SS_Killing:
                        {
                            if (isSync)
                            {
                                WaitAsyncIO();
                                PreUse(true);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private void WaitAsyncIO()
        {
            var rc = CEngine.Instance.RenderContext;
            bool bOutput = false;
            while (ResourceState.StreamState != EStreamingState.SS_Valid)
            {
                System.Threading.Thread.Sleep(0);
                if (CEngine.Instance.TextureManager.Textures.ContainsKey(this.Name) == false)
                {
                    CEngine.Instance.TextureManager.GetShaderRView(rc, Name, true);
                }
                //else if (CEngine.Instance.TextureManager.WaitStreamingCount == 0)
                //{
                //    System.Diagnostics.Debug.Assert(false);
                //    break;
                //}
                this.ResourceState.AccessTime = CEngine.Instance.EngineTime;
                if (bOutput == false)
                {
                    System.Diagnostics.Debug.WriteLine($"Block PreUse True: Texture {Name} is streaming by async thread");
                    bOutput = true;
                }
            }
        }
        private void PreUseInAsyncThread(bool isSync)
        {
            if (isSync)
            {
                if (ResourceState.StreamState == EStreamingState.SS_Invalid)
                {
                    ResourceState.StreamState = EStreamingState.SS_Streaming;
                    if (false == this.RestoreResource())
                    {
                        ResourceState.StreamState = EStreamingState.SS_Invalid;
                    }
                    else
                    {
                        ResourceState.StreamState = EStreamingState.SS_Valid;
                    }
                }
            }
        }

        public static bool SaveETC2(string file, IO.XndAttrib attr, int mipLevel, bool sRGB)
        {
            return SDK_ImageEncoder_SaveETC2(file, attr.CoreObject, mipLevel, vBOOL.FromBoolean(sRGB));
        }
        public static bool SaveETC2_Png(byte[] pngData, IO.XndAttrib attr, int mipLevel, bool sRGB)
        {
            unsafe
            {
                fixed (byte* ptr = &pngData[0])
                {
                    return SDK_ImageEncoder_SaveETC2_PNG(ptr, (UInt32)pngData.Length, attr.CoreObject, mipLevel, vBOOL.FromBoolean(sRGB));
                }
            }
        }

        #region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_IShaderResourceView_GetName(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_IShaderResourceView_SetName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IShaderResourceView_GetTxDesc(NativePointer self, CTxPicDesc* desc);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EPixelFormat SDK_IShaderResourceView_GetTextureFormat(NativePointer self);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_IShaderResourceView_Save2Memory(NativePointer self, CRenderContext.NativePointer rc, Support.CBlobObject.NativePointer data, EIMAGE_FILE_FORMAT Type);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_IShaderResourceView_GetTexture2DData(NativePointer self, CRenderContext.NativePointer rc, Support.CBlobObject.NativePointer data, int level, int CpuTexWidth, int CpuTexHeight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderResourceView_RefreshResource(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxTextureStreaming.NativePointer SDK_IShaderResourceView_GetTexStreaming(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_ImageEncoder_SaveETC2(string file, IO.XndAttrib.NativePointer attr, int mipLevel, vBOOL sRGB);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_ImageEncoder_SaveETC2_PNG(byte* pBuffer, UInt32 length, IO.XndAttrib.NativePointer attr, int mipLevel, vBOOL sRGB);
        #endregion
    }
}

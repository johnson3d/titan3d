using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class CGraphicsProfiler : AuxCoreObject<CGraphicsProfiler.NativePointer>
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
        public CGraphicsProfiler()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GraphicsProfiler");
        }
        public bool Init(bool noPixelShader, bool noPixelWrite, EPlatformType platforms)
        {
            RName noPsShader = RName.GetRName("Shaders/ShadingEnv/Sys/NoPixelShader.shadingenv");
            var rc = CEngine.Instance.RenderContext;
            var shaderDesc = rc.CreateShaderDesc(noPsShader, "PS_Main", EShaderType.EST_PixelShader, new CShaderDefinitions(), platforms);
            if (shaderDesc == null)
                return false;

            var ps = rc.CreatePixelShader(shaderDesc);

            SDK_GraphicsProfiler_SetEmptyPixelShader(CoreObject, ps.CoreObject);

            if(noPixelWrite)
            {
                CFrameBuffersDesc FBDesc = new CFrameBuffersDesc();
                FBDesc.IsSwapChainBuffer = vBOOL.FromBoolean(false);
                FBDesc.UseDSV = vBOOL.FromBoolean(false);
                var mFrameBuffer = rc.CreateFrameBuffers(FBDesc);
                if (mFrameBuffer == null)
                {
                    return false;
                }

                CTexture2DDesc TexDesc = new CTexture2DDesc();
                TexDesc.Init();
                TexDesc.Width = 1;
                TexDesc.Height = 1;
                TexDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                TexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_RENDER_TARGET);
                var Tex2D = rc.CreateTexture2D(TexDesc);

                CShaderResourceViewDesc SRVDesc = new CShaderResourceViewDesc();
                SRVDesc.mTexture2D = Tex2D.CoreObject;
                var SRV = rc.CreateShaderResourceView(SRVDesc);
                mFrameBuffer.BindSRV_RT((UInt32)0, SRV);

                CRenderTargetViewDesc RTVDesc = new CRenderTargetViewDesc();
                RTVDesc.mTexture2D = Tex2D.CoreObject;
                var RTV = rc.CreateRenderTargetView(RTVDesc);
                mFrameBuffer.BindRenderTargetView((UInt32)0, RTV);

                //depth stencil view;
                CDepthStencilViewDesc mDSVDesc = new CDepthStencilViewDesc();
                mDSVDesc.Width = 1;
                mDSVDesc.Height = 1;
                var mDepthStencilView = rc.CreateDepthStencilView(mDSVDesc);

                mFrameBuffer.BindDepthStencilView(mDepthStencilView);

                SDK_GraphicsProfiler_SetOnePixelFrameBuffers(CoreObject, mFrameBuffer.CoreObject);
            }

            NoPixelShader = noPixelShader;
            NoPixelWrite = noPixelWrite;

            return true;
        }

        public bool NoPixelWrite
        {
            get
            {
                return SDK_GraphicsProfiler_GetNoPixelWrite(CoreObject);
            }
            set
            {
                SDK_GraphicsProfiler_SetNoPixelWrite(CoreObject, vBOOL.FromBoolean(value));
            }
        }
        public bool NoPixelShader
        {
            get
            {
                return SDK_GraphicsProfiler_GetNoPixelShader(CoreObject);
            }
            set
            {
                SDK_GraphicsProfiler_SetNoPixelShader(CoreObject, vBOOL.FromBoolean(value));
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GraphicsProfiler_SetEmptyPixelShader(NativePointer self, CPixelShader.NativePointer ps);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GraphicsProfiler_SetOnePixelFrameBuffers(NativePointer self, CFrameBuffer.NativePointer fb);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GraphicsProfiler_GetNoPixelWrite(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GraphicsProfiler_SetNoPixelWrite(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GraphicsProfiler_GetNoPixelShader(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GraphicsProfiler_SetNoPixelShader(NativePointer self, vBOOL value);
        #endregion
    }
}

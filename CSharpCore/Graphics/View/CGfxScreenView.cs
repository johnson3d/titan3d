using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.GamePlay.Component;
using EngineNS.Graphics.Mesh;

namespace EngineNS.Graphics.View
{
    public class CGfxScreenViewDesc
    {
        private bool mIsSwapChainBuffer = false;
        public bool IsSwapChainBuffer
        {
            get { return mIsSwapChainBuffer; }
            set { mIsSwapChainBuffer = value; }
        }
        
        private bool mUseDepthStencilView = true;
        public bool UseDepthStencilView
        {
            get { return mUseDepthStencilView; }
            set { mUseDepthStencilView = value; }
        }

        private UInt32 mWidth;
        public UInt32 Width
        {
            get { return mWidth; }
            set { mWidth = value; }
        }

        private UInt32 mHeight;
        public UInt32 Height
        {
            get { return mHeight; }
            set { mHeight = value; }
        }
        
        public List<CRenderTargetViewDesc> mRTVDescArray = new List<CRenderTargetViewDesc>();
        
        public CDepthStencilViewDesc mDSVDesc = new CDepthStencilViewDesc();
        
    }


    public class CGfxScreenView
    {
        public CGfxScreenViewDesc mViewInfo;
        
        private CFrameBuffer mFrameBuffer;
        public CFrameBuffer FrameBuffer
        {
            get { return mFrameBuffer; }
        }

        private List<CTexture2D> mTex2dArray = new List<CTexture2D>();
        private List<CShaderResourceView> mSRVArray = new List<CShaderResourceView>();
        private List<CRenderTargetView> mRTVArray = new List<CRenderTargetView>();
        
        private CDepthStencilView mDepthStencilView;
        
        private CViewport mViewport;
        public CViewport Viewport
        {
            get { return mViewport; }
        }

        private CConstantBuffer mScreenViewCB;
        public CConstantBuffer ScreenViewCB
        {
            get { return mScreenViewCB; }
        }
        
        //the viewport mesh is always one pass;
        private CPass mPass = null;
        private List<CPass> mPassArray  = new List<CPass>();

        public CRasterizerState mRasterState;
        public CDepthStencilState mDepthStencilState;
        public CBlendState mBlendState;
        
        #region CBuffer
        private int mIDViewportSizeAndRcp;
        private Vector4 mViewportSizeAndRcp;
        public Vector4 ViewportSizeAndRcp
        {
            get
            {
                return mViewportSizeAndRcp;
            }
            set
            {
                mViewportSizeAndRcp = value;
                mScreenViewCB.SetValue(mIDViewportSizeAndRcp, value, 0);
            }
        }

        private int mID_SunPosNDC;
        private Vector4 mSunPosNDC;
        public Vector4 SunPosNDC
        {
            get { return mSunPosNDC; }
            set
            {
                mSunPosNDC = value;
                mScreenViewCB.SetValue(mID_SunPosNDC, value, 0);
            }
        }

        private int mID_DirLightColor_Intensity;
        private Vector4 mDirLightColor_Intensity;
        public Vector4 DirLightColor_Intensity
        {
            get { return mDirLightColor_Intensity; }
            set
            {
                mDirLightColor_Intensity = value;
                mScreenViewCB.SetValue(mID_DirLightColor_Intensity, value, 0);
            }
        }

        private int mID_AoParam;
        private Vector4 mAoParam;
        public Vector4 AoParam
        {
            get { return mAoParam; }
            set
            {
                mAoParam = value;
                mScreenViewCB.SetValue(mID_AoParam, value, 0);
            }
        }

        #endregion

        public async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, CSwapChain SwapChain,
            CGfxScreenViewDesc ViewInfo, CGfxShadingEnv ShadingEnv, CGfxMaterialInstance MtlInst, CGfxMesh ViewportMesh)
        {
            if (ShadingEnv == null)
                return false;
            if(ViewportMesh == null)
            {
                var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.ScreenAlignedTriangleName, true);
                ViewportMesh = CEngine.Instance.MeshManager.CreateMesh(RHICtx, ScreenAlignedTriangle);
                ViewportMesh.SetMaterialInstance(RHICtx, 0, await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx,
                    RName.GetRName("Material/defaultmaterial.instmtl")),
                    CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
                //await ViewportMesh.AwaitEffects();
            }
            mViewInfo = ViewInfo;

            mViewport = new CViewport();
            mViewport.Width = ViewInfo.Width;
            mViewport.Height = ViewInfo.Height;
            mViewport.TopLeftX = 0.0f;
            mViewport.TopLeftY = 0.0f;
            mViewport.MinDepth = 0.0f;
            mViewport.MaxDepth = 1.0f;
            
            var ShaderProgram = CEngine.Instance.EffectManager.DefaultEffect.ShaderProgram;
            mScreenViewCB = RHICtx.CreateConstantBuffer(ShaderProgram, CEngine.Instance.EffectManager.DefaultEffect.CacheData.CBID_View);
            if (mScreenViewCB == null)
            {
                return false;
            }
            
            mIDViewportSizeAndRcp = mScreenViewCB.FindVar("gViewportSizeAndRcp");
            mID_SunPosNDC = mScreenViewCB.FindVar("gSunPosNDC");
            mID_DirLightColor_Intensity = mScreenViewCB.FindVar("gDirLightColor_Intensity");
            mID_AoParam = mScreenViewCB.FindVar("gAoParam");
            
            CRasterizerStateDesc RSDesc = new CRasterizerStateDesc();
            RSDesc.InitForCustom();
            mRasterState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, RSDesc);

            CDepthStencilStateDesc DSSDesc = new CDepthStencilStateDesc();
            DSSDesc.InitForCustomLayers();
            mDepthStencilState = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, DSSDesc);

            CBlendStateDesc BlendDesc = new CBlendStateDesc();
            BlendDesc.InitForCustomLayers();
            mBlendState = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, BlendDesc);

            mPass = RHICtx.CreatePass();
            if (false == await mPass.InitPassForViewportView(RHICtx, ShadingEnv, MtlInst, ViewportMesh))
            {
                return false;
            }
            
            return OnResize(RHICtx, SwapChain, ViewInfo.Width, ViewInfo.Height);
        }

        public async System.Threading.Tasks.Task<bool> InitForMultiPassMode(CRenderContext RHICtx, CGfxScreenViewDesc ViewInfo, List<CGfxShadingEnv> ShadingEnvArray, 
            CGfxMaterialInstance MtlInst, CGfxMesh ViewportMesh)
        {
            if (ShadingEnvArray == null)
            {
                return false;
            }
                
            if (ViewportMesh == null)
            {
                var RectMesh = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.FullScreenRectName, true);
                ViewportMesh = CEngine.Instance.MeshManager.CreateMesh(RHICtx, RectMesh);
                ViewportMesh.SetMaterialInstance(RHICtx, 0, await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx,
                    RName.GetRName("Material/defaultmaterial.instmtl")),
                    CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
                //await ViewportMesh.AwaitEffects();
            }
            mViewInfo = ViewInfo;
       
            mViewport = new CViewport();
            mViewport.Width = ViewInfo.Width;
            mViewport.Height = ViewInfo.Height;
            mViewport.TopLeftX = 0.0f;
            mViewport.TopLeftY = 0.0f;
            mViewport.MinDepth = 0.0f;
            mViewport.MaxDepth = 1.0f;

            var ShaderProgram = CEngine.Instance.EffectManager.DefaultEffect.ShaderProgram;
            mScreenViewCB = RHICtx.CreateConstantBuffer(ShaderProgram, CEngine.Instance.EffectManager.DefaultEffect.CacheData.CBID_View);
            if (mScreenViewCB == null)
            {
                return false;
            }
            mIDViewportSizeAndRcp = mScreenViewCB.FindVar("gViewportSizeAndRcp");
            mID_SunPosNDC = mScreenViewCB.FindVar("gSunPosNDC");
            mID_AoParam = mScreenViewCB.FindVar("gAoParam");

            CRasterizerStateDesc RSDesc = new CRasterizerStateDesc();
            RSDesc.InitForCustom();
            mRasterState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, RSDesc);

            CDepthStencilStateDesc DSSDesc = new CDepthStencilStateDesc();
            DSSDesc.InitForCustomLayers();
            mDepthStencilState = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, DSSDesc);

            CBlendStateDesc BlendDesc = new CBlendStateDesc();
            BlendDesc.InitForCustomLayers();
            mBlendState = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, BlendDesc);

            for (UInt32 idx = 0; idx < ShadingEnvArray.Count; idx++)
            {
                var refPass = RHICtx.CreatePass();
                if (false == await refPass.InitPassForViewportView(RHICtx, ShadingEnvArray[(int)idx], MtlInst, ViewportMesh))
                {
                    return false;
                }
                mPassArray.Add(refPass);
            }
            
            return OnResize(RHICtx, null, ViewInfo.Width, ViewInfo.Height);
        }


        public bool OnResize(CRenderContext RHICtx, CSwapChain SwapChain, UInt32 width, UInt32 height)
        {
            if (mViewport == null || width == 0 || height == 0)
            {
                return false;
            }

            mViewport.Width = width;
            mViewport.Height = height;

            //frame buffer;
            mFrameBuffer?.Cleanup();
            mFrameBuffer = null;

            CFrameBuffersDesc FBDesc = new CFrameBuffersDesc();
            FBDesc.IsSwapChainBuffer = vBOOL.FromBoolean(mViewInfo.IsSwapChainBuffer);
            FBDesc.UseDSV = (mViewInfo.UseDepthStencilView ? vBOOL.FromBoolean(true) : vBOOL.FromBoolean(false));
            mFrameBuffer = RHICtx.CreateFrameBuffers(FBDesc);
            if (mFrameBuffer == null)
            {
                return false;
            }

            for (int idx = 0; idx < mTex2dArray.Count; idx++)
            {
                mTex2dArray[idx]?.Cleanup();
            }
            mTex2dArray.Clear();

            for (int idx = 0; idx < mSRVArray.Count; idx++)
            {
                mSRVArray[idx]?.Cleanup();
            }
            mSRVArray.Clear();
            
            //render target view;
            for (int idx = 0; idx < mRTVArray.Count; idx++)
            {
                mRTVArray[idx]?.Cleanup();
            }
            mRTVArray.Clear();

            mDepthStencilView?.Cleanup();
            mDepthStencilView = null;

            if (mViewInfo.IsSwapChainBuffer == true && SwapChain != null)
            {
                for (int idx = 0; idx < mViewInfo.mRTVDescArray.Count; idx++)
                {
                    var RTVDesc = mViewInfo.mRTVDescArray[idx];
                    //gles3.1 can not sample swap chain buffer;
                    RTVDesc.mCanBeSampled = vBOOL.FromBoolean(false);
                    RTVDesc.mTexture2D = SwapChain.Texture2D.CoreObject;
                    //this is for editor to use(snapshot needs this);
                    if (CEngine.Instance.Desc.RHIType == ERHIType.RHT_D3D11)
                    {
                        CShaderResourceViewDesc SRVDesc = new CShaderResourceViewDesc();
                        SRVDesc.Init();
                        SRVDesc.mTexture2D = SwapChain.Texture2D.CoreObject;
                        var SRV = RHICtx.CreateShaderResourceView(SRVDesc);
                        mSRVArray.Add(SRV);
                        mFrameBuffer.BindSRV_RT((UInt32)idx, SRV);
                    }
                    var RTV = RHICtx.CreateRenderTargetView(RTVDesc);
                    mRTVArray.Add(RTV);
                    mFrameBuffer.BindRenderTargetView((UInt32)idx, RTV);
                }
            }
            else
            {
                for (int idx = 0; idx < mViewInfo.mRTVDescArray.Count; idx++)
                {
                    var refRTVDesc = mViewInfo.mRTVDescArray[idx];
                    if (refRTVDesc.mCanBeSampled == true)
                    {
                        CTexture2DDesc TexDesc = new CTexture2DDesc();
                        TexDesc.Init();
                        TexDesc.Width = width;
                        TexDesc.Height = height;
                        TexDesc.Format = refRTVDesc.Format;
                        TexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_RENDER_TARGET);
                        var Tex2D = RHICtx.CreateTexture2D(TexDesc);
                        mTex2dArray.Add(Tex2D);

                        CShaderResourceViewDesc SRVDesc = new CShaderResourceViewDesc();
                        SRVDesc.Init();
                        SRVDesc.mTexture2D = Tex2D.CoreObject;
                        var SRV = RHICtx.CreateShaderResourceView(SRVDesc);
                        mSRVArray.Add(SRV);
                        mFrameBuffer.BindSRV_RT((UInt32)idx, SRV);

                        refRTVDesc.mTexture2D = Tex2D.CoreObject;
                        var RTV = RHICtx.CreateRenderTargetView(refRTVDesc);
                        mRTVArray.Add(RTV);
                        mFrameBuffer.BindRenderTargetView((UInt32)idx, RTV);
                    }
                    else
                    {
                        CTexture2DDesc TexDesc = new CTexture2DDesc();
                        TexDesc.Init();
                        TexDesc.Width = width;
                        TexDesc.Height = height;
                        TexDesc.Format = refRTVDesc.Format;
                        TexDesc.BindFlags = (UInt32)(EBindFlags.BF_RENDER_TARGET);
                        var Tex2D = RHICtx.CreateTexture2D(TexDesc);
                        mTex2dArray.Add(Tex2D);

                        refRTVDesc.mTexture2D = Tex2D.CoreObject;
                        var RTV = RHICtx.CreateRenderTargetView(refRTVDesc);
                        mRTVArray.Add(RTV);

                        mFrameBuffer.BindRenderTargetView((UInt32)idx, RTV);
                    }
                }
            }
            
            mViewInfo.mDSVDesc.Width = width;
            mViewInfo.mDSVDesc.Height = height;
            if (mViewInfo.UseDepthStencilView == true)
            {
                if (mViewInfo.mDSVDesc.mCanBeSampled == true)
                {
                    CTexture2DDesc Tex2dDesc = new CTexture2DDesc();
                    Tex2dDesc.Init();
                    Tex2dDesc.Width = width;
                    Tex2dDesc.Height = height;
                    Tex2dDesc.Format = mViewInfo.mDSVDesc.Format;
                    Tex2dDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_DEPTH_STENCIL);
                    var Tex2d = RHICtx.CreateTexture2D(Tex2dDesc);
                    mTex2dArray.Add(Tex2d);

                    CShaderResourceViewDesc SRVDesc = new CShaderResourceViewDesc();
                    SRVDesc.Init();
                    SRVDesc.mTexture2D = Tex2d.CoreObject;
                    var SRV = RHICtx.CreateShaderResourceView(SRVDesc);
                    mSRVArray.Add(SRV);
                    mFrameBuffer.BindSRV_DS(SRV);

                    mViewInfo.mDSVDesc.mTexture2D = Tex2d.CoreObject;
                    mDepthStencilView = RHICtx.CreateDepthStencilView(mViewInfo.mDSVDesc);
                    mFrameBuffer.BindDepthStencilView(mDepthStencilView);
                }
                else
                {
                    CTexture2DDesc Tex2dDesc = new CTexture2DDesc();
                    Tex2dDesc.Init();
                    Tex2dDesc.Width = width;
                    Tex2dDesc.Height = height;
                    Tex2dDesc.Format = mViewInfo.mDSVDesc.Format;
                    Tex2dDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_DEPTH_STENCIL);
                    var Tex2d = RHICtx.CreateTexture2D(Tex2dDesc);
                    mTex2dArray.Add(Tex2d);
                    
                    mViewInfo.mDSVDesc.mTexture2D = Tex2d.CoreObject;
                    mDepthStencilView = RHICtx.CreateDepthStencilView(mViewInfo.mDSVDesc);
                    mFrameBuffer.BindDepthStencilView(mDepthStencilView);
                }
            }
            else
            {
                mDepthStencilView = null;
            }

            return true;
        }
        
        private CShaderSamplers mCookVPMeshSampler = new CShaderSamplers();
        public void CookViewportMeshToPass(CRenderContext RHICtx, CGfxShadingEnv ShaderEnv, CGfxCamera Camera, CGfxMesh ViewportMesh)
        {
            if (ViewportMesh == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"ViewportViewError!!!", "");
                System.Diagnostics.Debug.Assert(false);
                return;
            }
            if (mPass == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }
            
            ViewportMesh.MeshPrimitives.PreUse(false);
            
            var refMtlMesh = ViewportMesh.MtlMeshArray[0];
            if (refMtlMesh == null)
            {
                return;
            }

            mPass.RenderPipeline.RasterizerState = mRasterState;
            mPass.RenderPipeline.DepthStencilState = mDepthStencilState;
            mPass.RenderPipeline.BlendState = mBlendState;

            mPass.BindShaderTextures(refMtlMesh);

            //if(mPass.ShaderSamplerBinder==null)
            //{
            //    mPass.ShaderSamplerBinder = new CShaderSamplers();
            //}
            //refMtlMesh.GetSamplerBinder(RHICtx, mPass.Effect.ShaderProgram, mPass.ShaderSamplerBinder);
            //var tempSampler = new CShaderSamplers();
            var tempSampler = mCookVPMeshSampler;
            refMtlMesh.GetSamplerBinder(RHICtx, mPass.Effect.ShaderProgram, tempSampler);
            mPass.ShaderSamplerBinder = tempSampler;

            ShaderEnv.BindResources(ViewportMesh, mPass);

            mPass.BindGeometry(ViewportMesh.MeshPrimitives, 0, 0);

            mPass.ViewPort = mViewport;

            mPass.BindCBuffer(mPass.Effect.ShaderProgram, mPass.Effect.CacheData.CBID_Camera, Camera.CBuffer);
            mPass.BindCBuffer(mPass.Effect.ShaderProgram, mPass.Effect.CacheData.CBID_View, mScreenViewCB);

            mPass.OnCookRenderData(ShaderEnv, PrebuildPassIndex.PPI_Default);
            return;
        }

        CShaderSamplers mSampler4Cook = new CShaderSamplers();
        public void CookViewportMeshToPassInMultiPassMode(CRenderContext RHICtx, CGfxShadingEnv ShaderEnv, UInt32 ActivePassIndex, CGfxCamera Camera, CGfxMesh ViewportMesh)
        {
            if (ViewportMesh == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"ViewportViewError!!!", "");
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            if (ActivePassIndex > mPassArray.Count - 1)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            ViewportMesh.MeshPrimitives.PreUse(false);

            var refMtlMesh = ViewportMesh.MtlMeshArray[0];
            if (refMtlMesh == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            mPassArray[(int)ActivePassIndex].RenderPipeline.RasterizerState = mRasterState;
            mPassArray[(int)ActivePassIndex].RenderPipeline.DepthStencilState = mDepthStencilState;
            mPassArray[(int)ActivePassIndex].RenderPipeline.BlendState = mBlendState;

            mPassArray[(int)ActivePassIndex].BindShaderTextures(refMtlMesh);
            
            refMtlMesh.GetSamplerBinder(RHICtx, mPassArray[(int)ActivePassIndex].Effect.ShaderProgram, mSampler4Cook);
            mPassArray[(int)ActivePassIndex].ShaderSamplerBinder = mSampler4Cook;

            ShaderEnv.BindResources(ViewportMesh, mPassArray[(int)ActivePassIndex]);

            mPassArray[(int)ActivePassIndex].BindGeometry(ViewportMesh.MeshPrimitives, 0, 0);

            mPassArray[(int)ActivePassIndex].ViewPort = mViewport;

            mPassArray[(int)ActivePassIndex].BindCBuffer(mPassArray[(int)ActivePassIndex].Effect.ShaderProgram, mPassArray[(int)ActivePassIndex].Effect.CacheData.CBID_Camera, Camera.CBuffer);
            mPassArray[(int)ActivePassIndex].BindCBuffer(mPassArray[(int)ActivePassIndex].Effect.ShaderProgram, mPassArray[(int)ActivePassIndex].Effect.CacheData.CBID_View, mScreenViewCB);

            return;
        }
        
        public void PushPassToRHI(CCommandList CmdList)
        {
            if (CmdList == null || mPass == null)
            {
#if DEBUG
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"NullPtr here", "");
                System.Diagnostics.Debug.Assert(false);
#endif
                return;
            }

            //the shader is still on loading state...
            if (mPass.GpuProgram == null || mPass.ViewPort == null)
            {
                return;
            }
            CmdList.PushPass(mPass);
        }

        public void PushPassToRHIInMultiPassMode(CCommandList CmdList, UInt32 ActivePassIndex)
        {
            if (ActivePassIndex > mPassArray.Count - 1)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            if (CmdList == null || mPassArray[(int)ActivePassIndex] == null)
            {
#if DEBUG
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"NullPtr here", "");
#endif
                return;
            }
            
            //the shader is still on loading state...
            if (mPassArray[(int)ActivePassIndex].GpuProgram == null || mPassArray[(int)ActivePassIndex].ViewPort == null)
            {
                return;
            }
            CmdList.PushPass(mPassArray[(int)ActivePassIndex]);
        }
        
    }
    
}

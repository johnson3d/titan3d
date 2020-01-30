using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Graphics.Mesh;
using System.Runtime.CompilerServices;

namespace EngineNS.Graphics.View
{
    [IO.Serializer.EnumSizeAttribute(typeof(IO.Serializer.SByteEnum))]
    public enum ERenderLayer
    {
        RL_Opaque,
        RL_CustomOpaque,
        RL_Translucent,
        RL_CustomTranslucent,
        //for editor to use;this layer should always be the last layer to send to renderer;
        RL_Gizmos,
        RL_Shadow,
        RL_Sky,

        RL_Num,
    };
    

    //public class RenderLayerAtom
    //{
    //    public CGfxMtlMesh mMtlMesh; //MtlMesh has no geometry data,so we need GameMesh ptr for help;
    //}

    public class CGfxRenderLayer
    {
        protected List<CGfxMtlMesh> mRenderLayerAtomArray = new List<CGfxMtlMesh>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRenderLayerAtom(CGfxMtlMesh atom)
        {
            if (atom == null)
                return;
            mRenderLayerAtomArray.Add(atom);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRenderLayerAtomNum()
        {
            return mRenderLayerAtomArray.Count;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CGfxMtlMesh GetMtlMeshFromArray(uint index)
        {
            if (index >= mRenderLayerAtomArray.Count)
                return null;
            return mRenderLayerAtomArray[(int)index];
        }
        public void Clear()
        {
            mRenderLayerAtomArray.Clear();
        }
    }

    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public class CGfxSceneViewInfo : IO.Serializer.Serializer
    {
        public bool mUseDSV = true;
        public UInt32 Width
        {
            get;
            set;
        }
        public UInt32 Height
        {
            get;
            set;
        }
        [Editor.Editor_Enumerable]
        public List<CRenderTargetViewDesc> mRTVDescArray
        {
            get;
            set;
        } = new List<CRenderTargetViewDesc>();

        [Editor.Editor_ShowInPropertyGrid]
        public CDepthStencilViewDesc mDSVDesc = new CDepthStencilViewDesc();
    }


    public partial class CGfxSceneView : AuxCoreObject<CGfxSceneView.NativePointer>
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

        public override void Cleanup()
        {
            if (UIHost != null)
            {
                UIHost.Cleanup();
                UIHost = null;
            }
            base.Cleanup();
        }

        CGfxSceneViewInfo _mViewInfo;
        public CGfxSceneViewInfo mViewInfo
        {
            get { return _mViewInfo; }
        }

        #region CBuffer
        int mIDDirLightColor_Intensity;
        int mIDDirLightDirection_Leak;
        

        int mIDSkyLightColor;
        int mID_GroundLightColor;

        int mIDViewportSizeAndRcp;
        int mIDEnvMapMipMaxLevel;

        int mIDEyeEnvMapMipMaxLevel;

        int mID_DepthBiasAndZFarRcp;
        int mID_FadeParam;
        int mID_ShadowTransitionScale;
        int mID_ShadowMapSizeAndRcp;
        int mID_Viewer2ShadowMtx;
        int mID_ShadowDistance;

        int mID_ShadowTransitionScaleArrayEditor;
        int mID_CsmNum;
        int mID_Viewer2ShadowMtxArrayEditor;
        int mID_CsmDistanceArray;

        int mID_PointLightPos_RadiusInv;
        int mID_PointLightColor_Intensity;

        //deprecated stuff and will be deleted someday;
        int DirLightSpecularIntensityId;
        int DirLightingAmbientId;
        int DirLightingDiffuseId;
        int DirLightingSpecularId;
        int DirLightShadingSSSId;

        public void SetPointLight(UInt32 index, ref GamePlay.Component.GPointLightComponent.GPointLightComponentInitializer.PointLightDesc lightDesc)
        {
            unsafe
            {
                fixed (GamePlay.Component.GPointLightComponent.GPointLightComponentInitializer.PointLightDesc* p = &lightDesc)
                {
                    this.mSceneViewCB.SetVarValue(mID_PointLightPos_RadiusInv, (byte*)p, 
                        sizeof(Vector4), index);

                    this.mSceneViewCB.SetVarValue(mID_PointLightColor_Intensity, (byte*)p + sizeof(Vector4),
                        sizeof(Vector4), index);
                }
            }
        }

        private Vector4 mDirLightColor_Intensity;
        public Vector4 DirLightColor_Intensity
        {
            get
            {
                return mDirLightColor_Intensity;
            }
            set
            {
                mDirLightColor_Intensity = value;
                mSceneViewCB.SetValue(mIDDirLightColor_Intensity, value, 0);
            }
        }

        private Vector4 _mDirLightDirection_Leak;
        public Vector4 mDirLightDirection_Leak
        {
            get
            {
                return _mDirLightDirection_Leak;
            }
            set
            {
                Vector3 TempDir = new Vector3(value.X, value.Y, value.Z);
                TempDir.Normalize();
                Vector4 TempValue = new Vector4(TempDir, value.W);

                _mDirLightDirection_Leak = TempValue;
                mSceneViewCB.SetValue(mIDDirLightDirection_Leak, TempValue, 0);
            }
        }

        
        private Vector3 _mSkyLightColor;
        public Vector3 mSkyLightColor
        {
            get
            {
                return _mSkyLightColor;
            }
            set
            {
                _mSkyLightColor = value;
                mSceneViewCB.SetValue(mIDSkyLightColor, value, 0);
            }
        }
        
        private Vector3 mGroundLightColor;
        public Vector3 GroundLightColor
        {
            get
            {
                return mGroundLightColor;
            }
            set
            {
                mGroundLightColor = value;
                mSceneViewCB.SetValue(mID_GroundLightColor, value, 0);
            }
        }


        private Vector4 _mViewportSizeAndRcp;
        public Vector4 mViewportSizeAndRcp
        {
            get
            {
                return _mViewportSizeAndRcp;
            }
            set
            {
                _mViewportSizeAndRcp = value;
                mSceneViewCB.SetValue(mIDViewportSizeAndRcp, value, 0);
            }
        }

        private float _mEnvMapMipMaxLevel;
        public float mEnvMapMipMaxLevel
        {
            get
            {
                return _mEnvMapMipMaxLevel;
            }
            set
            {
                _mEnvMapMipMaxLevel = value;
                mSceneViewCB.SetValue(mIDEnvMapMipMaxLevel, value, 0);
            }
        }

        private float _mEyeEnvMapMaxMipLevel;
        public float mEyeEnvMapMaxMipLevel
        {
            get
            {
                return _mEyeEnvMapMaxMipLevel;
            }
            set
            {
                _mEyeEnvMapMaxMipLevel = value;
                mSceneViewCB.SetValue(mIDEyeEnvMapMipMaxLevel, value, 0);
            }
        }

        private Vector2 _mDepthBiasAndZFarRcp;
        public Vector2 mDepthBiasAndZFarRcp
        {
            get
            {
                return _mDepthBiasAndZFarRcp;
            }
            set
            {
                _mDepthBiasAndZFarRcp = value;
                mSceneViewCB.SetValue(mID_DepthBiasAndZFarRcp, value, 0);
            }
        }

        private Vector2 _mFadeParam;
        public Vector2 mFadeParam
        {
            get
            {
                return _mFadeParam;
            }
            set
            {
                _mFadeParam = value;
                mSceneViewCB.SetValue(mID_FadeParam, value, 0);
            }
        }

        private float _mShadowTransitionScale;
        public float mShadowTransitionScale
        {
            get
            {
                return _mShadowTransitionScale;
            }
            set
            {
                _mShadowTransitionScale = value;
                mSceneViewCB.SetValue(mID_ShadowTransitionScale, value, 0);
            }
        }

        private Vector4 mShadowTransitionScaleArrayEditor;
        public Vector4 ShadowTransitionScaleArrayEditor
        {
            get { return mShadowTransitionScaleArrayEditor; }
            set
            {
                mShadowTransitionScaleArrayEditor = value;
                mSceneViewCB.SetValue(mID_ShadowTransitionScaleArrayEditor, value, 0);
            }
        }
        
        private int mCsmNum;
        public int CsmNum
        {
            get { return mCsmNum; }
            set
            {
                mCsmNum = value;
                mSceneViewCB.SetValue(mID_CsmNum, value, 0);
            }
        }

        private Vector4 mCsmDistanceArray;
        public Vector4 CsmDistanceArray
        {
            get { return mCsmDistanceArray; }
            set
            {
                mCsmDistanceArray = value;
                mSceneViewCB.SetValue(mID_CsmDistanceArray, value, 0);
            }
        }
        
        private Vector4 _mShadowMapSizeAndRcp;
        public Vector4 mShadowMapSizeAndRcp
        {
            get
            {
                return _mShadowMapSizeAndRcp;
            }
            set
            {
                _mShadowMapSizeAndRcp = value;
                mSceneViewCB.SetValue(mID_ShadowMapSizeAndRcp, value, 0);
            }
        }

        private Matrix _mViewer2ShadowMtx;
        public Matrix mViewer2ShadowMtx
        {
            get
            {
                return _mViewer2ShadowMtx;
            }
            set
            {
                _mViewer2ShadowMtx = value;
                mSceneViewCB.SetValue(mID_Viewer2ShadowMtx, value, 0);
            }
        }

        private Matrix mViewer2ShadowMtxArrayEditor0;
        public Matrix Viewer2ShadowMtxArrayEditor0
        {
            get { return mViewer2ShadowMtxArrayEditor0; }
            set
            {
                mViewer2ShadowMtxArrayEditor0 = value;
                mSceneViewCB.SetValue(mID_Viewer2ShadowMtxArrayEditor, value, 0);
            }
        }

        private Matrix mViewer2ShadowMtxArrayEditor1;
        public Matrix Viewer2ShadowMtxArrayEditor1
        {
            get { return mViewer2ShadowMtxArrayEditor1; }
            set
            {
                mViewer2ShadowMtxArrayEditor1 = value;
                mSceneViewCB.SetValue(mID_Viewer2ShadowMtxArrayEditor, value, 1);
            }
        }

        private Matrix mViewer2ShadowMtxArrayEditor2;
        public Matrix Viewer2ShadowMtxArrayEditor2
        {
            get { return mViewer2ShadowMtxArrayEditor2; }
            set
            {
                mViewer2ShadowMtxArrayEditor2 = value;
                mSceneViewCB.SetValue(mID_Viewer2ShadowMtxArrayEditor, value, 2);
            }
        }

        private Matrix mViewer2ShadowMtxArrayEditor3;
        public Matrix Viewer2ShadowMtxArrayEditor3
        {
            get { return mViewer2ShadowMtxArrayEditor3; }
            set
            {
                mViewer2ShadowMtxArrayEditor3 = value;
                mSceneViewCB.SetValue(mID_Viewer2ShadowMtxArrayEditor, value, 3);
            }
        }


        private float _mShadowDistance;
        public float mShadowDistance
        {
            get { return _mShadowDistance; }
            set
            {
                _mShadowDistance = value;
                mSceneViewCB.SetValue(mID_ShadowDistance, value, 0);
            }
        }


        #region TO_BE_DEL_LATER
        private float mDirLightSpecularIntensity;
        public float DirLightSpecularIntensity
        {
            get { return mDirLightSpecularIntensity; }
            set
            {
                mDirLightSpecularIntensity = value;
                mSceneViewCB.SetValue(DirLightSpecularIntensityId, value, 0);
            }
        }
        private Color4 mDirLightingAmbient;
        public Color4 DirLightingAmbient
        {
            get { return mDirLightingAmbient; }
            set
            {
                mDirLightingAmbient = value;
                mSceneViewCB.SetValue(DirLightingAmbientId, value, 0);
            }
        }
        private Color4 mDirLightingDiffuse;
        public Color4 DirLightingDiffuse
        {
            get { return mDirLightingDiffuse; }
            set
            {
                mDirLightingDiffuse = value;
                mSceneViewCB.SetValue(DirLightingDiffuseId, value, 0);
            }
        }
        private Color4 mDirLightingSpecular;
        public Color4 DirLightingSpecular
        {
            get { return mDirLightingSpecular; }
            set
            {
                mDirLightingSpecular = value;
                mSceneViewCB.SetValue(DirLightingSpecularId, value, 0);
            }
        }
        private float mDirLightShadingSSS;
        public float DirLightShadingSSS
        {
            get { return mDirLightShadingSSS; }
            set
            {
                mDirLightShadingSSS = value;
                mSceneViewCB.SetValue(DirLightShadingSSSId, value, 0);
            }
        }
        #endregion

        #endregion

        public bool OnResize(CRenderContext RHICtx, CSwapChain swapChain, UInt32 width, UInt32 height)
        {
            if (mViewport == null)
                return false;

            if (width == 0 || height == 0)
                return false;

            mViewport.Width = width;
            mViewport.Height = height;
            SDK_GfxSceneView_ResizeViewport(CoreObject, (UInt32)mViewport.TopLeftX, (UInt32)mViewport.TopLeftY, width, height);

            mFrameBuffer?.Cleanup();
            mFrameBuffer = null;

            CFrameBuffersDesc fbDesc = new CFrameBuffersDesc();
            fbDesc.IsSwapChainBuffer = vBOOL.FromBoolean(false);
            fbDesc.UseDSV = (mViewInfo.mUseDSV ? vBOOL.FromBoolean(true) : vBOOL.FromBoolean(false));
            mFrameBuffer = RHICtx.CreateFrameBuffers(fbDesc);
            if (mFrameBuffer == null)
                return false;

            SDK_GfxSceneView_SetFrameBuffers(CoreObject, FrameBuffer.CoreObject);
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
            
            for (int idx = 0;idx < mRTVArray.Count; idx++)
            {
                mRTVArray[idx]?.Cleanup();
            }
            mRTVArray.Clear();

            mDSV?.Cleanup();
            mDSV = null;
            
            for (int i = 0; i < mViewInfo.mRTVDescArray.Count; i++)
            {
                var refRTVDesc = mViewInfo.mRTVDescArray[i];
                if (refRTVDesc.mCanBeSampled == true)
                {
                    CTexture2DDesc Tex2dDesc = new CTexture2DDesc();
                    Tex2dDesc.Init();
                    Tex2dDesc.Width = width;
                    Tex2dDesc.Height = height;
                    Tex2dDesc.Format = refRTVDesc.Format;
                    Tex2dDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_RENDER_TARGET);
                    var Tex2d = RHICtx.CreateTexture2D(Tex2dDesc);
                    mTex2dArray.Add(Tex2d);

                    CShaderResourceViewDesc srvDesc = new CShaderResourceViewDesc();
                    srvDesc.Init();
                    srvDesc.mTexture2D = Tex2d.CoreObject;
                    var SRV = RHICtx.CreateShaderResourceView(srvDesc);
                    mSRVArray.Add(SRV);
                    mFrameBuffer.BindSRV_RT((UInt32)i, SRV);

                    refRTVDesc.mTexture2D = Tex2d.CoreObject;
                    var RTV = RHICtx.CreateRenderTargetView(refRTVDesc);
                    mRTVArray.Add(RTV);
                    mFrameBuffer.BindRenderTargetView((UInt32)i, RTV);
                }
                else
                {
                    CTexture2DDesc Tex2dDesc = new CTexture2DDesc();
                    Tex2dDesc.Init();
                    Tex2dDesc.Width = width;
                    Tex2dDesc.Height = height;
                    Tex2dDesc.Format = refRTVDesc.Format;
                    Tex2dDesc.BindFlags = (UInt32)(EBindFlags.BF_RENDER_TARGET);
                    var Tex2d = RHICtx.CreateTexture2D(Tex2dDesc);
                    mTex2dArray.Add(Tex2d);
                    
                    CRenderTargetViewDesc rtDesc = new CRenderTargetViewDesc();
                    rtDesc.mTexture2D = Tex2d.CoreObject;
                    var RTV = RHICtx.CreateRenderTargetView(rtDesc);
                    mRTVArray.Add(RTV);
                    mFrameBuffer.BindRenderTargetView((UInt32)i, RTV);
                }
            }
            
            mViewInfo.mDSVDesc.Width = width;
            mViewInfo.mDSVDesc.Height = height;
            if (mViewInfo.mUseDSV)
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
                    mDSV = RHICtx.CreateDepthStencilView(mViewInfo.mDSVDesc);
                    mFrameBuffer.BindDepthStencilView(mDSV);
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
                    mDSV = RHICtx.CreateDepthStencilView(mViewInfo.mDSVDesc);
                    mFrameBuffer.BindDepthStencilView(mDSV);
                }
            }
            else
            {
                mDSV = null;
            }

            return true;
        }

        public bool Init(CRenderContext rc, CSwapChain swapChain, CGfxSceneViewInfo ViewInfo)
        {
            _mViewInfo = ViewInfo;
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxSceneView");
            if (SDK_GfxSceneView_Init(mCoreObject, rc.CoreObject, ViewInfo.Width, ViewInfo.Height) == false)
                return false;

            var shaderProgram = CEngine.Instance.EffectManager.DefaultEffect.ShaderProgram;
            mSceneViewCB = rc.CreateConstantBuffer(shaderProgram, CEngine.Instance.EffectManager.DefaultEffect.CacheData.CBID_View);
            if (mSceneViewCB == null)
                return false;
            SDK_GfxSceneView_BindConstBuffer(CoreObject, mSceneViewCB.CoreObject);

            mViewport = new CViewport();
            mViewport.Width = ViewInfo.Width;
            mViewport.Height = ViewInfo.Height;
            SDK_GfxSceneView_SetViewport(CoreObject, mViewport.CoreObject);

            //code which is deprecated and will be delete someday;
            {
                DirLightSpecularIntensityId = mSceneViewCB.FindVar("mDirLightSpecularIntensity");
                DirLightingAmbientId = mSceneViewCB.FindVar("mDirLightingAmbient");
                DirLightingDiffuseId = mSceneViewCB.FindVar("mDirLightingDiffuse");
                DirLightingSpecularId = mSceneViewCB.FindVar("mDirLightingSpecular");
                DirLightShadingSSSId = mSceneViewCB.FindVar("mDirLightShadingSSS");
            }

            mIDDirLightColor_Intensity = mSceneViewCB.FindVar("gDirLightColor_Intensity");
            mIDDirLightDirection_Leak = mSceneViewCB.FindVar("gDirLightDirection_Leak");
            
            mIDSkyLightColor = mSceneViewCB.FindVar("mSkyLightColor");
            mID_GroundLightColor = mSceneViewCB.FindVar("mGroundLightColor");

            mIDViewportSizeAndRcp = mSceneViewCB.FindVar("gViewportSizeAndRcp");
            mIDEnvMapMipMaxLevel = mSceneViewCB.FindVar("gEnvMapMaxMipLevel");

            mIDEyeEnvMapMipMaxLevel = mSceneViewCB.FindVar("gEyeEnvMapMaxMipLevel");

            mID_DepthBiasAndZFarRcp = mSceneViewCB.FindVar("gDepthBiasAndZFarRcp");
            mID_FadeParam = mSceneViewCB.FindVar("gFadeParam");
            mID_ShadowTransitionScale = mSceneViewCB.FindVar("gShadowTransitionScale");
            mID_ShadowMapSizeAndRcp = mSceneViewCB.FindVar("gShadowMapSizeAndRcp");
            mID_Viewer2ShadowMtx = mSceneViewCB.FindVar("gViewer2ShadowMtx");
            mID_ShadowDistance = mSceneViewCB.FindVar("gShadowDistance");

            mID_ShadowTransitionScaleArrayEditor = mSceneViewCB.FindVar("gShadowTransitionScaleArrayEditor");
            mID_CsmNum = mSceneViewCB.FindVar("gCsmNum");
            mID_Viewer2ShadowMtxArrayEditor = mSceneViewCB.FindVar("gViewer2ShadowMtxArrayEditor");
            mID_CsmDistanceArray = mSceneViewCB.FindVar("gCsmDistanceArray");

            mID_PointLightPos_RadiusInv = mSceneViewCB.FindVar("PointLightPos_RadiusInv");
            mID_PointLightColor_Intensity = mSceneViewCB.FindVar("PointLightColor_Intensity");

            return OnResize(rc, swapChain, ViewInfo.Width, ViewInfo.Height);
        }

        public static Profiler.TimeScope ScopeCook2Pass = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxSceneView), nameof(CookSpecRenderLayerDataToPass));
        public void CookSpecRenderLayerDataToPass(CRenderContext RHICtx, ERenderLayer RenderLayer, CGfxCamera ViewerCamera, CGfxShadingEnv ShadingEnv, PrebuildPassIndex ppi)
        {
            ScopeCook2Pass.Begin();
            if (ViewerCamera == null || RenderLayer == ERenderLayer.RL_Shadow)
            {
                return;
            }

            if (RenderLayer == ERenderLayer.RL_Num)
            {
                RenderLayer = ERenderLayer.RL_Num - 1;
                return;
            }
            
            var SpecRenderLayer = ViewerCamera.mSceneRenderLayer[(int)RenderLayer];
            int Count = SpecRenderLayer.GetRenderLayerAtomNum();
            for (uint idx = 0; idx < Count; idx++)
            {
                var MtlMesh = SpecRenderLayer.GetMtlMeshFromArray(idx);
                if (MtlMesh == null)
                    break;
                var SceneMesh = MtlMesh.mRootSceneMesh;
                
                if (MtlMesh == null || SceneMesh == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"NullPtr here", "");
                    continue;
                }

                if (ppi == PrebuildPassIndex.PPI_HitProxy && SceneMesh.mMeshVars.mHitProxyId == 0)
                {
                    continue;
                }

                if (ppi == PrebuildPassIndex.PPI_PickedEditor)
                {
                    if (SceneMesh.mSelected == false)
                    {
                        continue;
                    }
                }

                MtlMesh.UpdatePerMtlMeshCBuffer(RHICtx, ViewerCamera);

                CPass refPass = MtlMesh.GetPass(ppi);
                if(refPass==null)
                {
                    continue;
                }
                if(false == refPass.PreUse())
                {
                    continue;
                }

                refPass.OnCookRenderData(ShadingEnv, ppi);

                switch (ppi)
                {
                    case PrebuildPassIndex.PPI_PickedEditor:
                        refPass.BindShaderTextures(MtlMesh);
                        refPass.ShaderSamplerBinder = MtlMesh.GetSamplerBinder_PickedEditor(RHICtx, refPass.Effect.ShaderProgram);
                        ShadingEnv.BindResources(SceneMesh, refPass);
                        break;
                    case PrebuildPassIndex.PPI_HitProxy:
                        {
                            refPass.BindShaderTextures(MtlMesh);
                            refPass.ShaderSamplerBinder = MtlMesh.GetSamplerBinder_HitProxy(RHICtx, refPass.Effect.ShaderProgram);
                            ShadingEnv.BindResources(SceneMesh, refPass);
                        }
                        break;
                    case PrebuildPassIndex.PPI_SceneCapture:
                    case PrebuildPassIndex.PPI_OpaquePbr:
                    case PrebuildPassIndex.PPI_TransparentPbr:
                    case PrebuildPassIndex.PPI_CustomTranslucentPbr:
                    case PrebuildPassIndex.PPI_OpaquePbrEditor:
                    case PrebuildPassIndex.PPI_CustomTranslucentPbrEditor:
                    case PrebuildPassIndex.PPI_Gizmos:
                    case PrebuildPassIndex.PPI_Snapshot:
                    case PrebuildPassIndex.PPI_Sky:
                        {
                            refPass.BindShaderTextures(MtlMesh);
                            //refPass.ShaderSamplerBinder = MtlMesh.GetSamplerBinder(RHICtx, refPass.Effect.ShaderProgram);
                            ShadingEnv.BindResources(SceneMesh, refPass);
                            //var sEnv = refPass.Effect.Desc.EnvShaderPatch;
                            //sEnv.BindResources(SceneMesh, refPass);
                        }
                        break;
                    default:
                        break;
                }
                
                refPass.ViewPort = Viewport;
#if DEBUG
                if (ViewerCamera.CBuffer == null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }
#endif
                refPass.BindCBuffer(refPass.Effect.ShaderProgram, refPass.Effect.CacheData.CBID_Camera, ViewerCamera.CBuffer);

                refPass.BindCBuffer(refPass.Effect.ShaderProgram, refPass.Effect.CacheData.CBID_View, SceneViewCB);
                
                SceneMesh.MdfQueue.OnSetPassData(refPass, false);
                
                SendPassToCorrectRenderLayer(RenderLayer, refPass);
            }
            
            ScopeCook2Pass.End();
        }

        public void CookShadowLayerData2Pass(CRenderContext RHICtx, CGfxCamera ViewerCamera, CGfxCamera ShadowCamera, CGfxShadingEnv ShadingEnv)
        {
            if (ViewerCamera == null || ShadowCamera == null || RHICtx == null || ShadingEnv == null)
            {
                return;
            }
            
            var ShadowRenderLayer = ViewerCamera.mSceneRenderLayer[(int)ERenderLayer.RL_Shadow];
            int Count = ShadowRenderLayer.GetRenderLayerAtomNum();
            for (uint idx = 0; idx < Count; idx++)
            {
                var MtlMesh = ShadowRenderLayer.GetMtlMeshFromArray(idx);
                if (MtlMesh == null)
                    return;
                var SceneMesh = MtlMesh.mRootSceneMesh;

                if (MtlMesh == null || SceneMesh == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"NullPtr here", "");
                    continue;
                }
                
                MtlMesh.UpdatePerMtlMeshCBufferForShadow(RHICtx, ShadowCamera);

                CPass refPass = MtlMesh.GetPass(PrebuildPassIndex.PPI_SSM);
                if (refPass == null)
                {
                    continue;
                }
                if(refPass.PreUse()==false)
                {
                    continue;
                }
                refPass.BindShaderTextures(MtlMesh);

                refPass.ShaderSamplerBinder = MtlMesh.GetSamplerBinder_Shadow(RHICtx, refPass.Effect.ShaderProgram);
                ShadingEnv.BindResources(SceneMesh, refPass);
                
                refPass.ViewPort = mViewport;
                refPass.BindCBuffer(refPass.Effect.ShaderProgram, refPass.Effect.CacheData.CBID_Camera, ShadowCamera.CBuffer);
                refPass.BindCBuffer(refPass.Effect.ShaderProgram, refPass.Effect.CacheData.CBID_View, SceneViewCB);
                
                SceneMesh.MdfQueue.OnSetPassData(refPass, true);

                SendPassToCorrectRenderLayer(ERenderLayer.RL_Shadow, refPass);
            }
        }

        
        public void SendPassToCorrectRenderLayer(ERenderLayer index, CPass pass)
        {
            SDK_GfxSceneView_SendPassToCorrectRenderLayer(mCoreObject, index, pass.CoreObject);
        }
        

        public static Profiler.TimeScope ScopePush2RHI = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxSceneView), nameof(PushSpecRenderLayerDataToRHI));
        public void PushSpecRenderLayerDataToRHI(CCommandList cmd, ERenderLayer index)
        {
            ScopePush2RHI.Begin();
            SDK_GfxSceneView_PushSpecRenderLayerDataToRHI(CoreObject, cmd.CoreObject, index);
            ScopePush2RHI.End();
        }


        //public void DrawFrameBuffer(CCommandList cmd, CGfxRenderPolicy policy)
        //{
        //    if (policy == null)
        //        policy = new CGfxRenderPolicy();
        //    policy.DoPolicy(cmd, this);
        //}

        protected CViewport mViewport;
        public CViewport Viewport
        {
            get { return mViewport; }
        }

        protected CFrameBuffer mFrameBuffer;
        public CFrameBuffer FrameBuffer
        {
            get { return mFrameBuffer; }
        }

        public UInt32 RenderLayerCount
        {
            get
            {
                return SDK_GfxSceneView_GetRenderLayerSize(CoreObject);
            }
        }

        private List<CTexture2D> mTex2dArray = new List<CTexture2D>();
        private List<CShaderResourceView> mSRVArray = new List<CShaderResourceView>();

        private List<CRenderTargetView> mRTVArray
        {
            get;
        } = new List<CRenderTargetView>();

        private CDepthStencilView mDSV;
        
        protected CConstantBuffer mSceneViewCB;
        public CConstantBuffer SceneViewCB
        {
            get
            {
                return mSceneViewCB;
            }
        }

        public void ResizeViewport(UInt32 StartX, UInt32 StartY, UInt32 width, UInt32 height)
        {
            mViewport.TopLeftX = StartX;
            mViewport.TopLeftY = StartY;
            mViewport.Width = width;
            mViewport.Height = height;
            SDK_GfxSceneView_ResizeViewport(CoreObject, (UInt32)mViewport.TopLeftX, (UInt32)mViewport.TopLeftY, width, height);
        }

        
#region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxSceneView_Init(NativePointer self, CRenderContext.NativePointer context, UInt32 width, UInt32 height, bool isDefault);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSceneView_Init(NativePointer self, CRenderContext.NativePointer context, UInt32 width, UInt32 height);

        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe void SDK_GfxSceneView_ClearMRT(NativePointer self, CCommandList.NativePointer pCmdList, CMRTClearColor* ClearColors, int ColorNum, bool bClearDepth, float Depth, bool bClearStencil, UInt32 Stencil);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxSceneView_CmdClearMRT(NativePointer self, CCommandList.NativePointer cmd);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSceneView_SetViewport(NativePointer self, CViewport.NativePointer vp);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSceneView_SetFrameBuffers(NativePointer self, CFrameBuffer.NativePointer fb);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
       // public extern static void SDK_GfxSceneView_ClearPasses(NativePointer self, ERenderLayer index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSceneView_SendPassToCorrectRenderLayer(NativePointer self, ERenderLayer index, CPass.NativePointer passPtr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSceneView_BindConstBuffer(NativePointer self, CConstantBuffer.NativePointer cbuffer);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxSceneView_UpdateConstBufferData(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSceneView_ResizeViewport(NativePointer self, UInt32 TopLeftX, UInt32 TopLeftY, UInt32 width, UInt32 height);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxSceneView_GetRenderLayerSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSceneView_PushSpecRenderLayerDataToRHI(NativePointer self, CCommandList.NativePointer cmd, ERenderLayer index);
#endregion
    }
}

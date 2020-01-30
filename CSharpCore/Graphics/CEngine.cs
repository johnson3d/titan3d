using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    //one scene view env shader => one prebuild pass;
    public enum PrebuildPassIndex : int
    {
        PPI_OpaquePbr = 0,
        PPI_TransparentPbr,
        PPI_CustomTranslucentPbr,
        
        PPI_OpaquePbrEditor,
        PPI_CustomTranslucentPbrEditor,
        
        PPI_HitProxy,
        PPI_PickedEditor,

        PPI_Gizmos,
        PPI_Snapshot,

        PPI_SSM,
        PPI_Sky,

        PPI_SceneCapture,
        
        PPI_Num,
        PPI_Default = PPI_OpaquePbr,
    };

    [EngineNS.Editor.Editor_NoDefaultObject]
    public partial class CEngine : AuxCoreObject<CEngine.NativePointer>
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
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly, "Engine.Instance", "引擎对象，万恶之源")]
        public static CEngine Instance;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public static GamePlay.GGameInstance Game
        {
            get
            {
                return Instance.GameInstance;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public static GamePlay.GWorld GameWorld
        {
            get
            {
                return Instance.GameInstance.World;
            }
        }
        public void GfxCleanup()
        {
            GpuFetchManager.Cleanup();
            MeshPrimitivesManager.Cleanup();
            MeshManager.Cleanup();
            DepthStencilStateManager.Cleanup();
            RasterizerStateManager.Cleanup();
            BlendStateManager.Cleanup();
            SamplerStateManager.Cleanup();
            if (mRenderContext != null)
            {
                mRenderContext.Cleanup();
                mRenderContext = null;
            }
            RenderSystem.Cleanup();
            base.Cleanup();
        }
        partial void Cleanup_UISystem();

        public bool GfxInitEngine(ERHIType type, UInt32 Adapter, IntPtr window, bool createDebugLayer=false)
        {
            mCoreObject = SDK_GfxEngine_NewObject();
            CRenderSystemDesc desc;
            desc.CreateDebugLayer = vBOOL.FromBoolean(createDebugLayer);
            desc.WindowHandle = window;
            if (false == RenderSystem.Init(type, desc))
                return false;

            var deviceNum = RenderSystem.GetContextNumber();
            if (Adapter >= deviceNum)
                return false;

            var deviceDesc = RenderSystem.GetContextDesc(Adapter);
            var devName = deviceDesc.DeviceNameString;
            mRenderContext = RenderSystem.CreateRenderContext(Adapter, window, createDebugLayer);
            if (mRenderContext == null)
                return false;

            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"GPU MaxShaderModel = {mRenderContext.DeviceShaderModel}");
            if( CRenderContext.ShaderModel> mRenderContext.DeviceShaderModel)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"GPU不能提供选择的ShaderModel，将自动降低: {CRenderContext.ShaderModel}到{mRenderContext.DeviceShaderModel}");
                CRenderContext.ShaderModel = mRenderContext.DeviceShaderModel;
            }

            mDesc.RHIType = type;
            SDK_GfxEngine_Init(mCoreObject, type, RenderSystem.CoreObject, RenderContext.CoreObject, FileManager.ProjectContent, FileManager.EngineContent, FileManager.EditorContent);

            mDefaultLayoutDesc = new CInputLayoutDesc();

            return true;
        }
        protected EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task AwaitEngineInited()
        {
            await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }

        protected CConstantBuffer mPerFrameCBuffer;
        public CConstantBuffer PerFrameCBuffer
        {
            get
            {
                return mPerFrameCBuffer;
            }
        }
        private struct PerFrameCBVarTable
        {
            public void SetDefault()
            {
                TimeId = -1;
                TimeSinId = -1;
                TimeCosId = -1;
            }
            public int TimeId;
            public int TimeSinId;
            public int TimeCosId;
        }
        private PerFrameCBVarTable mPerFrameCBVarTable = new PerFrameCBVarTable();
        public virtual void SetPerFrameCBuffer()
        {
            if (this.EffectManager.DefaultEffect == null ||
                this.EffectManager.DefaultEffect.IsValid == false )
                return;
            if (mPerFrameCBuffer == null)
            {
                var idx = this.EffectManager.DefaultEffect.ShaderProgram.FindCBuffer("cbPerFrame");
                if ((int)idx > 0)
                {
                    mPerFrameCBuffer = this.RenderContext.CreateConstantBuffer(this.EffectManager.DefaultEffect.ShaderProgram, idx);
                    mPerFrameCBVarTable.TimeId = mPerFrameCBuffer.FindVar("Time");
                    mPerFrameCBVarTable.TimeSinId = mPerFrameCBuffer.FindVar("TimeSin");
                    mPerFrameCBVarTable.TimeCosId = mPerFrameCBuffer.FindVar("TimeCos");
                }
            }
            else
            {
                float time = (float)this.EngineTime / 1000.0f;
                mPerFrameCBuffer.SetValue(mPerFrameCBVarTable.TimeId, time, 0);
                mPerFrameCBuffer.SetValue(mPerFrameCBVarTable.TimeSinId, (float)System.Math.Sin((double)time), 0);
                mPerFrameCBuffer.SetValue(mPerFrameCBVarTable.TimeCosId, (float)System.Math.Cos((double)time), 0);
            }
        }
        public virtual async System.Threading.Tasks.Task InitDefaultResource(GamePlay.GGameInstanceDesc gameDesc)
        {            
            var mtl = await this.MaterialManager.GetMaterialAsync(RenderContext, gameDesc.DefaultMaterialName);
            if (mtl == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Resource", $"{gameDesc.DefaultMaterialName} not found");
            }

            var tempMtlInst = await this.MaterialInstanceManager.GetMaterialInstanceAsync(RenderContext, gameDesc.DefaultMaterialInstanceName);
            if (tempMtlInst == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Resource", $"{gameDesc.DefaultMaterialInstanceName} not found");
            }

            await this.MaterialManager.Init(RenderContext, gameDesc.DefaultMaterialName);
            await this.MaterialInstanceManager.Init(RenderContext, gameDesc.DefaultMaterialInstanceName);
            this.ShadingEnvManager.Init();
            await this.EffectManager.Init(RenderContext);

            var fullScreenMesh = CEngine.Instance.MeshPrimitivesManager.NewMeshPrimitives(RenderContext, CEngineDesc.FullScreenRectName, 1);
            Graphics.Mesh.CGfxMeshCooker.MakeRect2D(RenderContext, fullScreenMesh, -1, -1, 2, 2, 0.0F, false);
            fullScreenMesh.ResourceState.KeepValid = true;

            var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.NewMeshPrimitives(RenderContext, CEngineDesc.ScreenAlignedTriangleName, 1);
            Graphics.Mesh.CGfxMeshCooker.MakeScreenAlignedTriangle(RenderContext, ScreenAlignedTriangle);
            ScreenAlignedTriangle.ResourceState.KeepValid = true;

            SetPerFrameCBuffer();
        }
        public async System.Threading.Tasks.Task RefreshAllSaveFiles()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            var files = FileManager.GetFiles(FileManager.ProjectContent, "*.gms", System.IO.SearchOption.AllDirectories);
            foreach (var i in files)
            {
                var rn = FileManager._GetRelativePathFromAbsPath(i, FileManager.ProjectContent);
                var obj = await MeshManager.CreateMeshAsync(RenderContext, RName.GetRName(rn));
                if (obj != null)
                    obj.SaveMesh();
            }
            files = FileManager.GetFiles(FileManager.ProjectContent, "*.material", System.IO.SearchOption.AllDirectories);
            foreach (var i in files)
            {
                var rn = FileManager._GetRelativePathFromAbsPath(i, FileManager.ProjectContent);
                var obj = await MaterialManager.GetMaterialAsync(RenderContext, RName.GetRName(rn));
                if (obj != null)
                    obj.SaveMaterial();
            }
            files = FileManager.GetFiles(FileManager.ProjectContent, "*.instmtl", System.IO.SearchOption.AllDirectories);
            foreach (var i in files)
            {
                var rn = FileManager._GetRelativePathFromAbsPath(i, FileManager.ProjectContent);
                var obj = await MaterialInstanceManager.GetMaterialInstanceAsync(RenderContext, RName.GetRName(rn));
                if (obj != null)
                    obj.SaveMaterialInstance();
            }
        }
        private long DelayInvalidResourceTime = 0;
        public void GfxTickSync()
        {
            if (DelayInvalidResourceTime == 0)
            {
                MeshPrimitivesManager.Tick();
                TextureManager.Tick();
            }
            else
            {
                DelayInvalidResourceTime -= this.EngineElapseTime;
                if (DelayInvalidResourceTime < 0)
                    DelayInvalidResourceTime = 0;
            }

            GpuFetchManager.TickSync();
        }
        public Graphics.CGfxGpuFetchManager GpuFetchManager
        {
            get;
        } = new Graphics.CGfxGpuFetchManager();

        public CInputLayoutDesc mDefaultLayoutDesc;
        public CInputLayoutDesc DefaultLayoutDesc
        {
            get { return mDefaultLayoutDesc; }
        }
        public CRenderSystem RenderSystem
        {
            get;
        } = new CRenderSystem();
        private CRenderContext mRenderContext;
        public CRenderContext RenderContext
        {
            get { return mRenderContext; }
        }
        public CDepthStencilStateManager DepthStencilStateManager
        {
            get;
        } = new CDepthStencilStateManager();
        public CRasterizerStateManager RasterizerStateManager
        {
            get;
        } = new CRasterizerStateManager();
        public CBlendStateManager BlendStateManager
        {
            get;
        } = new CBlendStateManager();
        public CSamplerStateManager SamplerStateManager
        {
            get;
        } = new CSamplerStateManager(); 
        public EngineNS.Graphics.CGfxEffectManager EffectManager
        {
            get;
        } = new EngineNS.Graphics.CGfxEffectManager();

        public EngineNS.Graphics.CGfxMaterialManager MaterialManager
        {
            get;
        } = new EngineNS.Graphics.CGfxMaterialManager();
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Graphics.CGfxMaterialInstanceManager MaterialInstanceManager
        {
            get;
        } = new EngineNS.Graphics.CGfxMaterialInstanceManager();
        public EngineNS.Graphics.Mesh.CGfxMeshPrimitivesManager MeshPrimitivesManager
        {
            get;
        } = new Graphics.Mesh.CGfxMeshPrimitivesManager();
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Graphics.Mesh.CGfxMeshManager MeshManager
        {
            get;
        } = new Graphics.Mesh.CGfxMeshManager();
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Bricks.Animation.Skeleton.CGfxSkeletonActionManager SkeletonActionManager
        {
            get;
        } = new Bricks.Animation.Skeleton.CGfxSkeletonActionManager();
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Bricks.Animation.Skeleton.CGfxSkeletonAssetManager SkeletonAssetManager
        {
            get;
        } = new Bricks.Animation.Skeleton.CGfxSkeletonAssetManager();
        public EngineNS.Graphics.CGfxShadingEnvManager ShadingEnvManager
        {
            get;
        } = new Graphics.CGfxShadingEnvManager();
        public EngineNS.Graphics.CGfxTextureManager TextureManager
        {
            get;
        } = new Graphics.CGfxTextureManager();
        public CPrebuildPassData PrebuildPassData
        {
            get;
        } = new CPrebuildPassData();

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_GfxEngine_NewObject();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxEngine_Init(NativePointer self, ERHIType type, CRenderSystem.NativePointer rs, CRenderContext.NativePointer rc, string contentPath, string enginePath, string editorPath);
        #endregion
    }

    public partial class CPrebuildPassData
    {
        public Graphics.CGfxShadingEnv[] DefaultShadingEnvs = new Graphics.CGfxShadingEnv[(int)PrebuildPassIndex.PPI_Num];
        public CInputLayout mDefaultInputLayout;
        public CInputLayout DefaultInputLayout
        {
            get
            {
                if (mDefaultInputLayout==null)
                {
                    mDefaultInputLayout = CEngine.Instance.RenderContext.CreateInputLayout(CEngine.Instance.DefaultLayoutDesc);
                }
                return mDefaultInputLayout;
            }
        }

        public Graphics.CGfxShadingEnv[] InitEditorMobileShadingEnv(CRenderContext RHICtx)
        {
            InitSystemStates(RHICtx);

            var result = new Graphics.CGfxShadingEnv[(int)PrebuildPassIndex.PPI_Num];

            result[(int)EngineNS.PrebuildPassIndex.PPI_OpaquePbr] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxMobileOpaqueSE>();
            result[(int)EngineNS.PrebuildPassIndex.PPI_TransparentPbr] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxMobileTranslucentSE>();
            result[(int)EngineNS.PrebuildPassIndex.PPI_CustomTranslucentPbr] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxMobileCustomTranslucentSE>();
            
            result[(int)EngineNS.PrebuildPassIndex.PPI_OpaquePbrEditor] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxMobileOpaqueEditorSE>();
            result[(int)EngineNS.PrebuildPassIndex.PPI_CustomTranslucentPbrEditor] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxMobileCustomTranslucentEditorSE>();
            
            result[(int)EngineNS.PrebuildPassIndex.PPI_HitProxy] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxHitProxySE>();
            
            result[(int)EngineNS.PrebuildPassIndex.PPI_PickedEditor] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxPickedSetUpSE>();

            result[(int)EngineNS.PrebuildPassIndex.PPI_Gizmos] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxGizmosSE>();
            result[(int)EngineNS.PrebuildPassIndex.PPI_Snapshot] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxSnapshotSE>();

            result[(int)EngineNS.PrebuildPassIndex.PPI_SSM] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxSE_SSM>();

            result[(int)EngineNS.PrebuildPassIndex.PPI_Sky] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxSE_MobileSky>();

            result[(int)EngineNS.PrebuildPassIndex.PPI_SceneCapture] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxSceneCaptureSE>();

            return result;
        }
        
        public void InitSystemStates(CRenderContext RHICtx)
        {
            //raster stat;
            CRasterizerStateDesc RSDescOpaque = new CRasterizerStateDesc();
            RSDescOpaque.InitForOpaque();
            mOpaqueRasterStat = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, RSDescOpaque);
            
            CRasterizerStateDesc RSDescTranslucent = new CRasterizerStateDesc();
            RSDescTranslucent.InitForTranslucent();
            mTranslucentRasterStat = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, RSDescTranslucent);
            
            {
                CRasterizerStateDesc rsDesc = new CRasterizerStateDesc();
                rsDesc.InitForTranslucent();
                rsDesc.CullMode = ECullMode.CMD_NONE;
                mNoCullingRasterStat = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, rsDesc);
            }

            //depth stencil stat;
            CDepthStencilStateDesc OpaqueDSSDesc = new CDepthStencilStateDesc();
            OpaqueDSSDesc.InitForOpacity();
            mOpaqueDSStat = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, OpaqueDSSDesc);

            CDepthStencilStateDesc TranslucentDSSDesc = new CDepthStencilStateDesc();
            TranslucentDSSDesc.InitForTranslucency();
            mTranslucentDSStat = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, TranslucentDSSDesc);
            
            CDepthStencilStateDesc dsDesc = new CDepthStencilStateDesc();
            dsDesc.InitForTranslucency();
            dsDesc.DepthEnable = 0;
            mDisableDepthStencilStat = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, dsDesc);
            
            CDepthStencilStateDesc DSDescEqual = new CDepthStencilStateDesc();
            DSDescEqual.DepthEnable = 1;
            DSDescEqual.DepthWriteMask = EDepthWriteMask.DSWM_ALL;
            DSDescEqual.DepthFunc = EComparisionMode.CMP_EQUAL;
            DSDescEqual.StencilEnable = 0;
            DSDescEqual.StencilReadMask = 0xFF;
            DSDescEqual.StencilWriteMask = 0xFF;
            DSDescEqual.FrontFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            DSDescEqual.FrontFace.StencilFailOp = EStencilOp.STOP_KEEP;
            DSDescEqual.FrontFace.StencilFunc = EComparisionMode.CMP_NEVER;
            DSDescEqual.BackFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            DSDescEqual.BackFace.StencilFailOp = EStencilOp.STOP_KEEP;
            DSDescEqual.BackFace.StencilFunc = EComparisionMode.CMP_NEVER;
            DSDescEqual.StencilRef = 0;
            mDSEqual = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, DSDescEqual);

            //blend state
            CBlendStateDesc ShadowBlendDesc = new CBlendStateDesc();
            ShadowBlendDesc.InitForShadow();
            mShadowBlendStat = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, ShadowBlendDesc);

            CBlendStateDesc OpaqueBlendDesc = new CBlendStateDesc();
            OpaqueBlendDesc.InitForOpacity();
            mOpaqueBlendStat = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, OpaqueBlendDesc);

            CBlendStateDesc TranslucentBlendDesc = new CBlendStateDesc();
            TranslucentBlendDesc.InitForTranslucency();
            mTranslucentBlendStat = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, TranslucentBlendDesc);

            {
                CBlendStateDesc bldDesc = new CBlendStateDesc();
                bldDesc.InitForTranslucency();
                bldDesc.RenderTarget0.SrcBlend = EBlend.BLD_ONE;
                bldDesc.RenderTarget0.DestBlend = EBlend.BLD_ONE;
                mAddColorBlendStat = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, bldDesc);
            }

            CBlendStateDesc SnapshotBlendDesc = new CBlendStateDesc();
            SnapshotBlendDesc.InitForSnapshot();
            mSnapshotBlendStat = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, SnapshotBlendDesc);
            

            Image2DShadingEnvs[(int)PrebuildPassIndex.PPI_Default] = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.Mesh.CImage2DShadingEnv>();
            mOtherShadingEnvs.Add(Image2DShadingEnvs[(int)PrebuildPassIndex.PPI_Default]);

            Font2DShadingEnvs[(int)PrebuildPassIndex.PPI_Default] = Bricks.FreeTypeFont.CFTShadingEnv.GetFTShadingEnv();
            mOtherShadingEnvs.Add(Font2DShadingEnvs[(int)PrebuildPassIndex.PPI_Default]);

            Graphics.CGfxShadingEnv temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EngineNS.Graphics.EnvShader.CGfxDefaultSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileBloomUSSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileBloomDSSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileBloomSetUpSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileBloomBlurHSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileBloomBlurVSE>();
            mOtherShadingEnvs.Add(temp);

            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileCopySE>();
            mOtherShadingEnvs.Add(temp);

            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxHitProxyAxisSE>();
            mOtherShadingEnvs.Add(temp);

            //picked editor;
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxPickedBlurHSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxPickedBlurVSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxPickedHollowSE>();
            mOtherShadingEnvs.Add(temp);

            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileCopyEditorSE>();
            mOtherShadingEnvs.Add(temp);
            
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileSunShaftMaskSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileSunShaftBlurSE>();
            mOtherShadingEnvs.Add(temp);

            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileAoMaskSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileAoBlurHSE>();
            mOtherShadingEnvs.Add(temp);
            temp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Graphics.EnvShader.CGfxMobileAoBlurVSE>();
            mOtherShadingEnvs.Add(temp);
        }

        List<Graphics.CGfxShadingEnv> mOtherShadingEnvs = new List<Graphics.CGfxShadingEnv>();
        
        public CRasterizerState mOpaqueRasterStat;
        public CRasterizerState mTranslucentRasterStat;
        public CRasterizerState mNoCullingRasterStat;
        
        public CDepthStencilState mOpaqueDSStat;
        public CDepthStencilState mTranslucentDSStat;
        public CDepthStencilState mDisableDepthStencilStat;
        public CDepthStencilState mDSEqual;

        public CBlendState mShadowBlendStat;
        public CBlendState mOpaqueBlendStat;
        public CBlendState mTranslucentBlendStat;
        public CBlendState mAddColorBlendStat;
        public CBlendState mSnapshotBlendStat;
        
        public Graphics.CGfxShadingEnv[] Font2DShadingEnvs = new Graphics.CGfxShadingEnv[(int)PrebuildPassIndex.PPI_Num];
        public Graphics.CGfxShadingEnv[] Image2DShadingEnvs = new Graphics.CGfxShadingEnv[(int)PrebuildPassIndex.PPI_Num];
        
    }
}

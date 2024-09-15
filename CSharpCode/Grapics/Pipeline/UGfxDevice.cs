using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.NxRHI;

namespace EngineNS.Graphics.Pipeline
{
    public partial class TtGfxDevice : TtModule<TtEngine>
    {
        public override int GetOrder()
        {
            return 0;
        }
        public readonly NxRHI.TtRenderSwapQueue RenderSwapQueue = new NxRHI.TtRenderSwapQueue();
        public override async System.Threading.Tasks.Task<bool> Initialize(TtEngine engine)
        {
            if(engine.Config.RHIType == NxRHI.ERhiType.RHI_VirtualDevice)
            {
                if (await InitGPU(engine, 0, NxRHI.ERhiType.RHI_VirtualDevice, IntPtr.Zero, engine.Config.HasDebugLayer, false) == false)
                {
                    return false;
                }
                return true;
            }

#if PWindow
            if (SDL.SDL3.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_TIMER| SDL.SDL_InitFlags.SDL_INIT_EVENTS) == -1)
                return false;
            TtNativeWindow.PropertiesID_WindowData = SDL.SDL3.SDL_CreateProperties();
#endif

            var wtType = Rtti.TtTypeDesc.TypeOf(engine.Config.MainWindowType).SystemType;
            if (wtType == null)
            {
                wtType = typeof(EngineNS.Editor.UMainEditorApplication);
            }
            SlateApplication = Rtti.TtTypeDescManager.CreateInstance(wtType) as TtSlateApplication;
            var winRect = engine.Config.MainWindow;

            if(engine.Config.SupportMultWindows)
            {
                if (false == SlateApplication.CreateNativeWindow(engine, "T3D", (int)winRect.X, (int)winRect.Y, (int)10, (int)10))
                {
                    return false;
                }
            }
            else
            {
                if (false == SlateApplication.CreateNativeWindow(engine, "T3D", (int)winRect.X, (int)winRect.Y, (int)winRect.Z, (int)winRect.W))
                {
                    return false;
                }
            }
            
            if (await InitGPU(engine, engine.Config.AdaperId, engine.Config.RHIType, 
                SlateApplication.NativeWindow.HWindow, engine.Config.HasDebugLayer, engine.Config.UseRenderDoc) == false)
            {
                return false;
            }

            SlateRenderer = new EGui.Slate.UBaseRenderer();
            await SlateRenderer.Initialize();

            //engine.Config.MainRPolicyName = RName.GetRName("UTest/testrendergraph.rpolicy");
            await SlateApplication.InitializeApplication(RenderContext, engine.Config.MainRPolicyName);
            var ws = SlateApplication.NativeWindow.WindowSize;
            SlateApplication.OnResize(ws.X, ws.Y);

            engine.TickableManager.AddTickable(TextureManager);

            await this.MeshPrimitiveManager.Initialize();
            await this.ClusteredMeshManager.Initialize();

            return true;
        }
        public override void TickModule(TtEngine engine)
        {
            var binder = CoreShaderBinder;
            if (PerFrameCBuffer != null)
            {
                var tm = engine.CurrentTick24BitMS;
                var timeOfSecend = (float)tm * 0.001f;
                PerFrameCBuffer.SetValue(binder.CBPerFrame.Time, in timeOfSecend);
                var fracTime = (float)(tm % 1000) * 0.001f;
                PerFrameCBuffer.SetValue(binder.CBPerFrame.TimeFracSecond, in fracTime);
                var timeSin = (float)Math.Sin(fracTime * Math.PI * 2);
                PerFrameCBuffer.SetValue(binder.CBPerFrame.TimeSin, in timeSin);
                var timeCos = (float)Math.Cos(fracTime * Math.PI * 2);
                PerFrameCBuffer.SetValue(binder.CBPerFrame.TimeCos, in timeCos);

                float elapsed = (float)engine.ElapseTickCountMS / 1000.0f;
                PerFrameCBuffer.SetValue(binder.CBPerFrame.ElapsedTime, in elapsed);
            }
        }
        public override void TickLogic(TtEngine host)
        {
            RenderSwapQueue.TickLogic(host.ElapsedSecond);
        }
        public override void TickRender(TtEngine host)
        {
            RenderSwapQueue.TickRender(host.ElapsedSecond);
        }
        public void TickSync(TtEngine host)
        {
            var testTime = Support.TtTime.GetTickCount();
            TtEngine.Instance.EventPoster.TickPostTickSyncEvents(testTime);
            TtEngine.Instance.GfxDevice.RenderContext.TickPostEvents();

            AttachBufferManager.Tick();

            RenderSwapQueue.TickSync(host.ElapsedSecond);
            CbvUpdater.UpdateCBVs();
        }
        public override void EndFrame(TtEngine engine)
        {
            
        }
        public override void Cleanup(TtEngine engine)
        {
            TtEngine.Instance.EventPoster.TickPostTickSyncEvents(long.MaxValue);

            AttachBufferManager?.Dispose();
            TextureManager?.Cleanup();
            MaterialManager?.Cleanup();
            MaterialManager = null;
            MaterialInstanceManager?.Cleanup();
            MaterialInstanceManager = null;

            PipelineManager.Cleanup();
            SamplerStateManager.Cleanup();
            RenderPassManager.Cleanup();
            InputLayoutManager.Cleanup();

            EffectManager.Dispose();

            HitproxyManager.Cleanup();
            
            SlateRenderer?.Cleanup();
            SlateRenderer = null;
            SlateApplication?.Cleanup();
            SlateApplication = null;

            Editor.ShaderCompiler.TtShaderCodeManager.Instance.Dispose();

            RenderContext.mCoreObject.TryFinalizeDevice(RenderSystem.mCoreObject);
            while (RenderContext.mCoreObject.IsFinalized() == false)
            {
                AttachBufferManager.Tick();
                var testTime = Support.TtTime.GetTickCount();
                TtEngine.Instance.EventPoster.TickPostTickSyncEvents(testTime);
                RenderContext.GpuQueue.Flush(NxRHI.EQueueType.QU_ALL);
                RenderContext.TickPostEvents();
            }
            RenderContext?.Dispose();
            RenderContext = null;
            RenderSystem?.Dispose();
            RenderSystem = null;
#if PWindow
            SDL.SDL3.SDL_Quit();
#endif
        }
        public TtSlateApplication SlateApplication { get; set; }
        public NxRHI.TtGpuSystem RenderSystem { get; private set; }
        public NxRHI.TtGpuDevice RenderContext { get; private set; }
        protected async System.Threading.Tasks.Task<bool> InitGPU(TtEngine engine, int Adapter, NxRHI.ERhiType rhi, IntPtr window, bool bDebugLayer, bool useRenderDoc)
        {
            if (TtEngine.Instance.PlayMode != EPlayMode.Game)
            {
                Editor.ShaderCompiler.TtShaderCodeManager.Instance.Initialize(RName.GetRName("shaders/", RName.ERNameType.Engine));
            }

            unsafe
            {
                //var sysDesc = new IRenderSystemDesc();
                //sysDesc.WindowHandle = window.ToPointer();
                //sysDesc.CreateDebugLayer = bDebugLayer ? 1 : 0;
                var gpuDesc = new NxRHI.FGpuSystemDesc();
                if (useRenderDoc)
                    gpuDesc.UseRenderDoc = 1;
                gpuDesc.CreateDebugLayer = bDebugLayer ? 1 : 0;
                gpuDesc.GpuBaseValidation = engine.Config.IsGpuBaseValidation ? 1 : 0;
                gpuDesc.WindowHandle = window.ToPointer();
                var renderDoc = engine.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource);
                renderDoc += "3rd/native/renderdoc/bin/renderdoc.dll";
                gpuDesc.RenderDocPath = VNameString.FromString(renderDoc);
                RenderSystem = NxRHI.TtGpuSystem.CreateGpuSystem(rhi, in gpuDesc);
                if (RenderSystem == null)
                    return false;

                var deviceNum = RenderSystem.NumOfContext;
                if (Adapter >= deviceNum)
                    return false;
                var rcDesc = new NxRHI.FGpuDeviceDesc();
                rcDesc.SetDefault();
                if (Adapter < 0)
                {
                    int AdapterScore = 0;
                    for (int i = 0; i < RenderSystem.NumOfContext; i++)
                    {
                        var caps = new NxRHI.FGpuDeviceDesc();
                        RenderSystem.GetDeviceDesc(i, ref caps);
                        var score = RenderSystem.GetAdapterScore(in caps);
                        if (score > AdapterScore)
                        {
                            Adapter = i;
                            AdapterScore = score;
                        }
                    }
                }
                RenderSystem.GetDeviceDesc(Adapter, ref rcDesc);
                rcDesc.GpuDump = engine.Config.IsGpuDump;
                rcDesc.CreateDebugLayer = bDebugLayer;
                //rcDesc.Han = window.ToPointer();
                rcDesc.AdapterId = (int)Adapter;
                rcDesc.RhiType = rhi;
                RenderContext = RenderSystem.CreateGpuDevice(in rcDesc);
                if (RenderContext == null)
                    return false;
            }

            RenderPassManager.Initialize(engine);
            await TextureManager.Initialize(engine);    
            await MaterialManager.Initialize(this);
            await MaterialInstanceManager.Initialize(engine);
            await EffectManager.Initialize(this);
            
            return true;
        }

        #region Manager

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public NxRHI.UTextureManager TextureManager { get; } = new NxRHI.UTextureManager();
        public Shader.TtMaterialManager MaterialManager { get; private set; } = new Shader.TtMaterialManager();
在此之后:
        public NxRHI.TtTextureManager TextureManager { get; } = new NxRHI.UTextureManager();
        public Shader.TtMaterialManager MaterialManager { get; private set; } = new Shader.TtMaterialManager();
*/
        public NxRHI.TtTextureManager TextureManager { get; } = new NxRHI.TtTextureManager();
        public Shader.TtMaterialManager MaterialManager { get; private set; } = new Shader.TtMaterialManager();
        public Shader.TtMaterialInstanceManager MaterialInstanceManager { get; private set; } = new Shader.TtMaterialInstanceManager();
        public EGui.Slate.UBaseRenderer SlateRenderer { get; private set; }
        public Graphics.Pipeline.Shader.TtEffectManager EffectManager
        {
            get;
        } = new Graphics.Pipeline.Shader.TtEffectManager();
        public Graphics.Pipeline.UGpuPipelineManager PipelineManager
        {
            get;
        } = new Graphics.Pipeline.UGpuPipelineManager();
        public Graphics.Pipeline.USamplerStateManager SamplerStateManager
        {
            get;
        } = new Graphics.Pipeline.USamplerStateManager();
        public Graphics.Pipeline.URenderPassManager RenderPassManager
        {
            get;
        } = new Graphics.Pipeline.URenderPassManager();
        public Graphics.Pipeline.UInputLayoutManager InputLayoutManager
        {
            get;
        } = new Graphics.Pipeline.UInputLayoutManager();
        public Graphics.Pipeline.UHitproxyManager HitproxyManager
        {
            get;
        } = new Graphics.Pipeline.UHitproxyManager();
        public Graphics.Pipeline.TtAttachBufferManager AttachBufferManager
        {
            get;
        } = new TtAttachBufferManager();
        public TtCbView.TrCbcUpdater CbvUpdater
        {
            get;
        } = new TtCbView.TrCbcUpdater();
        #endregion

        #region GraphicsData
        NxRHI.TtCbView mPerFrameCBuffer;
        public NxRHI.TtCbView PerFrameCBuffer 
        { 
            get
            {
                if (mPerFrameCBuffer == null)
                {
                    if (TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerFrame.Binder != null)
                    {
                        mPerFrameCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerFrame.Binder.mCoreObject);
                    }
                    else
                    {
                        return null;
                    }
                }
                return mPerFrameCBuffer;
            }
        }
        #endregion
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        public static System.Type UGfxDeviceType = typeof(Graphics.Pipeline.TtGfxDevice);
        private Graphics.Pipeline.TtGfxDevice mGfxDevice;
        public Graphics.Pipeline.TtGfxDevice GfxDevice 
        { 
            get
            {
                if (mGfxDevice == null)
                {
                    mGfxDevice = Rtti.TtTypeDescManager.CreateInstance(UGfxDeviceType) as Graphics.Pipeline.TtGfxDevice;
                }
                return mGfxDevice;
            }
        }
    }
}

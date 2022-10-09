using System;
using System.Collections.Generic;
using System.Text;
using SDL2;

namespace EngineNS.Graphics.Pipeline
{
    public partial class UGfxDevice : UModule<UEngine>
    {
        public readonly NxRHI.URenderCmdQueue RenderCmdQueue = new NxRHI.URenderCmdQueue();
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine engine)
        {
            if(engine.Config.RHIType == NxRHI.ERhiType.RHI_VirtualDevice)
            {
                if (await InitGPU(engine, 0, NxRHI.ERhiType.RHI_VirtualDevice, IntPtr.Zero, engine.Config.HasDebugLayer, false) == false)
                {
                    return false;
                }
                return true;
            }

            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) == -1)
                return false;

            var wtType = Rtti.UTypeDesc.TypeOf(engine.Config.MainWindowType).SystemType;
            if (wtType == null)
            {
                wtType = typeof(EngineNS.Editor.UMainEditorApplication);
            }
            SlateApplication = Rtti.UTypeDescManager.CreateInstance(wtType) as Graphics.Pipeline.USlateApplication;
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

            engine.TickableManager.AddTickable(RenderCmdQueue);
            return true;
        }
        public override void Tick(UEngine engine)
        {
            var binder = CoreShaderBinder;
            if (PerFrameCBuffer != null)
            {
                var timeOfSecend = (float)engine.CurrentTickCount * 0.001f;
                PerFrameCBuffer.SetValue(binder.CBPerFrame.Time, in timeOfSecend);
                var timeSin = (float)Math.Sin(timeOfSecend);
                PerFrameCBuffer.SetValue(binder.CBPerFrame.TimeSin, in timeSin);
                var timeCos = (float)Math.Cos(timeOfSecend);
                PerFrameCBuffer.SetValue(binder.CBPerFrame.TimeCos, in timeCos);

                float elapsed = (float)engine.ElapseTickCount / 1000.0f;
                PerFrameCBuffer.SetValue(binder.CBPerFrame.ElapsedTime, in elapsed);
            }
        }
        public void TickSync()
        {
            var testTime = Support.Time.GetTickCount();
            UEngine.Instance.EventPoster.TickPostTickSyncEvents(testTime);
            UEngine.Instance.GfxDevice.RenderContext.TickPostEvents();
        }
        public override void EndFrame(UEngine engine)
        {
            
        }
        public override void Cleanup(UEngine engine)
        {
            UEngine.Instance.EventPoster.TickPostTickSyncEvents(long.MaxValue);

            engine.TickableManager.RemoveTickable(RenderCmdQueue);

            TextureManager?.Cleanup();
            MaterialManager?.Cleanup();
            MaterialManager = null;
            MaterialInstanceManager?.Cleanup();
            MaterialInstanceManager = null;

            PipelineManager.Cleanup();
            SamplerStateManager.Cleanup();
            RenderPassManager.Cleanup();
            InputLayoutManager.Cleanup();

            EffectManager.Cleanup();

            HitproxyManager.Cleanup();
            SlateRenderer?.Cleanup();
            SlateRenderer = null;
            RenderContext?.Dispose();
            RenderContext = null;
            RenderSystem?.Dispose();
            RenderSystem = null;
            SlateApplication?.Cleanup();
            SlateApplication = null;

            Editor.ShaderCompiler.UShaderCodeManager.Instance.Cleanup();
            SDL.SDL_Quit();
        }
        public USlateApplication SlateApplication { get; set; }
        public NxRHI.UGpuSystem RenderSystem { get; private set; }
        public NxRHI.UGpuDevice RenderContext { get; private set; }
        protected async System.Threading.Tasks.Task<bool> InitGPU(UEngine engine, UInt32 Adapter, NxRHI.ERhiType rhi, IntPtr window, bool bDebugLayer, bool useRenderDoc)
        {
            if (UEngine.Instance.PlayMode != EPlayMode.Game)
            {
                Editor.ShaderCompiler.UShaderCodeManager.Instance.Initialize(RName.GetRName("shaders/", RName.ERNameType.Engine));
            }

            unsafe
            {
                //var sysDesc = new IRenderSystemDesc();
                //sysDesc.WindowHandle = window.ToPointer();
                //sysDesc.CreateDebugLayer = bDebugLayer ? 1 : 0;
                var gpuDesc = new NxRHI.FGpuSystemDesc();
                if (useRenderDoc)
                    gpuDesc.UseRenderDoc = 1;
                gpuDesc.CreateDebugLayer = 1;
                gpuDesc.WindowHandle = window.ToPointer();
                RenderSystem = NxRHI.UGpuSystem.CreateGpuSystem(rhi, in gpuDesc);
                if (RenderSystem == null)
                    return false;

                var deviceNum = RenderSystem.NumOfContext;
                if (Adapter >= deviceNum)
                    return false;
                var rcDesc = new NxRHI.FGpuDeviceDesc();
                rcDesc.SetDefault();
                var caps = new NxRHI.FGpuDeviceCaps();
                //RenderSystem.GetContextDesc(Adapter, ref rcDesc);

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
        public NxRHI.UTextureManager TextureManager { get; } = new NxRHI.UTextureManager();
        public Shader.UMaterialManager MaterialManager { get; private set; } = new Shader.UMaterialManager();
        public Shader.UMaterialInstanceManager MaterialInstanceManager { get; private set; } = new Shader.UMaterialInstanceManager();
        public EGui.Slate.UBaseRenderer SlateRenderer { get; private set; }
        public Graphics.Pipeline.Shader.UEffectManager EffectManager
        {
            get;
        } = new Graphics.Pipeline.Shader.UEffectManager();
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
        #endregion

        #region GraphicsData
        NxRHI.UCbView mPerFrameCBuffer;
        public NxRHI.UCbView PerFrameCBuffer 
        { 
            get
            {
                if (mPerFrameCBuffer == null)
                {
                    if (UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerFrame.Binder != null)
                    {
                        mPerFrameCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerFrame.Binder.mCoreObject);
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
    public partial class UEngine
    {
        public static System.Type UGfxDeviceType = typeof(Graphics.Pipeline.UGfxDevice);
        private Graphics.Pipeline.UGfxDevice mGfxDevice;
        public Graphics.Pipeline.UGfxDevice GfxDevice 
        { 
            get
            {
                if (mGfxDevice == null)
                {
                    mGfxDevice = Rtti.UTypeDescManager.CreateInstance(UGfxDeviceType) as Graphics.Pipeline.UGfxDevice;
                }
                return mGfxDevice;
            }
        }
    }
}

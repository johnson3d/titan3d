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
            if (SDL.SDL3.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_EVENTS) == false)
                return false;
            TtNativeWindow.PropertiesID_WindowData = SDL.SDL3.SDL_CreateProperties();
#endif

            var wtType = Rtti.TtTypeDesc.TypeOf(engine.Config.MainWindowType).SystemType;
            if (wtType == null)
            {
                wtType = typeof(EngineNS.Editor.TtMainEditorApplication);
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
            if (PerFrameCBuffer != null)
            {
                var tm = engine.CurrentTick24BitMS;
                var timeOfSecend = (float)tm * 0.001f;
                PerFrameCBuffer.SetValue(TtCoreShaderBinder.TtPerFrameCBufferVarIndexer.Instance.Time, in timeOfSecend);
                var fracTime = (float)(tm % 1000) * 0.001f;
                PerFrameCBuffer.SetValue(TtCoreShaderBinder.TtPerFrameCBufferVarIndexer.Instance.TimeFracSecond, in fracTime);
                var timeSin = (float)Math.Sin(fracTime * Math.PI * 2);
                PerFrameCBuffer.SetValue(TtCoreShaderBinder.TtPerFrameCBufferVarIndexer.Instance.TimeSin, in timeSin);
                var timeCos = (float)Math.Cos(fracTime * Math.PI * 2);
                PerFrameCBuffer.SetValue(TtCoreShaderBinder.TtPerFrameCBufferVarIndexer.Instance.TimeCos, in timeCos);

                float elapsed = (float)engine.ElapseTickCountMS / 1000.0f;
                PerFrameCBuffer.SetValue(TtCoreShaderBinder.TtPerFrameCBufferVarIndexer.Instance.ElapsedTime, in elapsed);
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
        //bool bSetBreakOnId = false;
        public void TickSync(TtEngine host)
        {
            var testTime = Support.TtTime.GetTickCount();
            TtEngine.Instance.EventPoster.TickPostTickSyncEvents(testTime);
            TtEngine.Instance.GfxDevice.RenderContext.TickPostEvents();

            AttachBufferManager.Tick();

            RenderSwapQueue.TickSync(host.ElapsedSecond);
            CbvUpdater.UpdateCBVs();

            //RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATE_HEAP, bSetBreakOnId);
        }
        public override void EndFrame(TtEngine engine)
        {
            
        }
        public override void Cleanup(TtEngine engine)
        {
            TtEngine.Instance.EventPoster.TickPostTickSyncEvents(long.MaxValue);

            AttachBufferManager?.Dispose();
            TextureManager?.Cleanup();
            MaterialManager?.Dispose();
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
                rcDesc.IsGpuDred = engine.Config.IsGpuDred;
                rcDesc.IsAftermath = engine.Config.IsAftermath;
                rcDesc.CreateDebugLayer = bDebugLayer;
                //rcDesc.Han = window.ToPointer();
                rcDesc.AdapterId = (int)Adapter;
                rcDesc.RhiType = rhi;
                RenderContext = RenderSystem.CreateGpuDevice(in rcDesc);
                if (RenderContext == null)
                    return false;

                RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CLEARRENDERTARGETVIEW_MISMATCHINGCLEARVALUE, false);
                RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.DRAW_EMPTY_SCISSOR_RECTANGLE, false);
                RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.GPU_BASED_VALIDATION_INCOMPATIBLE_RESOURCE_STATE, false);
                RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CLEARDEPTHSTENCILVIEW_MISMATCHINGCLEARVALUE, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATE_COMMANDLIST12, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.DESTROY_COMMANDLIST12, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATE_RESOURCE, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.DESTROY_RESOURCE, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATE_HEAP, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.DESTROY_HEAP, false);                
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATE_COMMANDALLOCATOR, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATE_DESCRIPTORHEAP, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATE_PIPELINESTATE, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATE_QUERYHEAP, false);
                //RenderContext.ShowDX12DeviceMessage(NxRHI.EDx12MessageId.CREATEGRAPHICSPIPELINESTATE_RENDERTARGETVIEW_NOT_SET, false);

                RenderContext.SetDX12BreakOnId(EDx12MessageId.COMMAND_LIST_OUTOFMEMORY, true);

                RenderContext.SetDX12BreakOnId(EDx12MessageId.COMMAND_ALLOCATOR_SYNC, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.COMMAND_LIST_CLOSED, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.COMMAND_ALLOCATOR_RESET, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.RENDER_TARGET_FORMAT_MISMATCH_PIPELINE_STATE, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.INVALID_SUBRESOURCE_STATE, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.MAP_INVALIDHEAP, true);
                //RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATEGRAPHICSPIPELINESTATE_RENDERTARGETVIEW_NOT_SET, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATEUNORDEREDACCESSVIEW_INVALIDFORMAT, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATERESOURCEANDHEAP_INVALIDHEAPPROPERTIES, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATE_CONSTANT_BUFFER_VIEW_INVALID_RESOURCE, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.RESOURCE_BARRIER_INVALID_HEAP, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.COPYRESOURCE_INVALIDDSTRESOURCE, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.RESOURCE_BARRIER_MATCHING_STATES, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.OBJECT_DELETED_WHILE_STILL_IN_USE, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATESHADERRESOURCEVIEW_INVALIDFORMAT, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATEUNORDEREDACCESSVIEW_INVALIDDIMENSIONS, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.DEPTH_STENCIL_FORMAT_MISMATCH_PIPELINE_STATE, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.CREATESHADERRESOURCEVIEW_INVALIDFORMAT, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.SET_DESCRIPTOR_HEAP_INVALID, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.SET_DESCRIPTOR_TABLE_INVALID, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.INVALID_DESCRIPTOR_HANDLE, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.RESOURCE_BARRIER_BEFORE_AFTER_MISMATCH, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.DEVICE_REMOVAL_PROCESS_AT_FAULT, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.UNMAP_RANGE_NOT_EMPTY, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.EXECUTECOMMANDLISTS_OPENCOMMANDLIST, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.COPYTEXTUREREGION_INVALIDSRCDIMENSIONS, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.EXECUTECOMMANDLISTS_FAILEDCOMMANDLIST, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.DEVICE_REMOVAL_PROCESS_AT_FAULT, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.DEVICE_REMOVAL_PROCESS_POSSIBLY_AT_FAULT, true);
                RenderContext.SetDX12BreakOnId(EDx12MessageId.DEVICE_REMOVAL_PROCESS_NOT_AT_FAULT, true);
            }

            RenderPassManager.Initialize(engine);
            await TextureManager.Initialize(engine);    
            await MaterialManager.Initialize(this);
            await MaterialInstanceManager.Initialize(engine);
            await EffectManager.Initialize(this);
            
            return true;
        }

        #region Manager
        public NxRHI.TtTextureManager TextureManager { get; } = new NxRHI.TtTextureManager();
        [Rtti.Meta("")]
        public Shader.TtMaterialManager MaterialManager { get; private set; } = new Shader.TtMaterialManager();
        [Rtti.Meta("")]
        public Shader.TtMaterialInstanceManager MaterialInstanceManager { get; private set; } = new Shader.TtMaterialInstanceManager();
        [Rtti.Meta("")]
        public Shader.TtMaterialFunctionManager MaterialFunctionManager { get; private set; } = new Shader.TtMaterialFunctionManager();
        public EGui.Slate.UBaseRenderer SlateRenderer { get; private set; }
        public Graphics.Pipeline.Shader.TtEffectManager EffectManager
        {
            get;
        } = new Graphics.Pipeline.Shader.TtEffectManager();
        public Graphics.Pipeline.TtGpuPipelineManager PipelineManager
        {
            get;
        } = new Graphics.Pipeline.TtGpuPipelineManager();
        public Graphics.Pipeline.TtSamplerStateManager SamplerStateManager
        {
            get;
        } = new Graphics.Pipeline.TtSamplerStateManager();
        public Graphics.Pipeline.TtRenderPassManager RenderPassManager
        {
            get;
        } = new Graphics.Pipeline.TtRenderPassManager();
        public Graphics.Pipeline.TtInputLayoutManager InputLayoutManager
        {
            get;
        } = new Graphics.Pipeline.TtInputLayoutManager();
        public Graphics.Pipeline.TtHitproxyManager HitproxyManager
        {
            get;
        } = new Graphics.Pipeline.TtHitproxyManager();
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
                    if (TtCoreShaderBinder.TtPerFrameCBufferVarIndexer.Instance.Binder != null)
                    {
                        mPerFrameCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(TtCoreShaderBinder.TtPerFrameCBufferVarIndexer.Instance.Binder.mCoreObject);
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
        [Rtti.Meta("")]
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

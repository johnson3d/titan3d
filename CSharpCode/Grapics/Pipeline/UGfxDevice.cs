using System;
using System.Collections.Generic;
using System.Text;
using SDL2;

namespace EngineNS.Graphics.Pipeline
{
    public partial class UGfxDevice : UModule<UEngine>
    {
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine engine)
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) == -1)
                return false;

            var wtType = Rtti.UTypeDesc.TypeOf(engine.Config.MainWindowType).SystemType;
            if (wtType == null)
            {
                wtType = typeof(EngineNS.Editor.UMainEditorApplication);
            }
            MainWindow = Rtti.UTypeDescManager.CreateInstance(wtType) as Graphics.Pipeline.USlateApplication;
            var winRect = engine.Config.MainWindow;

            if(engine.Config.SupportMultWindows)
            {
                if (false == MainWindow.CreateNativeWindow(engine, "T3D", (int)winRect.X, (int)winRect.Y, (int)10, (int)10))
                {
                    return false;
                }
            }
            else
            {
                if (false == MainWindow.CreateNativeWindow(engine, "T3D", (int)winRect.X, (int)winRect.Y, (int)winRect.Z, (int)winRect.W))
                {
                    return false;
                }
            }
            
            if (await InitGPU(engine, engine.Config.AdaperId, engine.Config.RHIType, MainWindow.NativeWindow.HWindow, engine.Config.HasDebugLayer) == false)
            {
                return false;
            }

            SlateRenderer = new EGui.Slate.UBaseRenderer();
            await SlateRenderer.Initialize();

            var rpType = Rtti.UTypeDesc.TypeOf(engine.Config.MainWindowRPolicy).SystemType;
            await MainWindow.InitializeApplication(RenderContext, rpType);
            var ws = MainWindow.NativeWindow.WindowSize;
            MainWindow.OnResize(ws.X, ws.Y);

            engine.TickableManager.AddTickable(TextureManager);
            return true;
        }
        public override void Tick(UEngine engine)
        {
            if (PerFrameCBuffer != null)
            {
                var timeOfSecend = (float)engine.CurrentTickCount * 0.001f;
                PerFrameCBuffer.SetValue(PerFrameCBuffer.PerFrameIndexer.Time, in timeOfSecend);
                var timeSin = Math.Sin(timeOfSecend);
                PerFrameCBuffer.SetValue(PerFrameCBuffer.PerFrameIndexer.TimeSin, in timeSin);
                var timeCos = Math.Cos(timeOfSecend);
                PerFrameCBuffer.SetValue(PerFrameCBuffer.PerFrameIndexer.TimeSin, in timeCos);

                float elapsed = (float)engine.ElapseTickCount / 1000.0f;
                PerFrameCBuffer.SetValue(PerFrameCBuffer.PerFrameIndexer.ElapsedTime, in elapsed);
            }
        }
        public delegate void Delegate_OnFenceCompetion(RHI.CFence fence);
        private List<KeyValuePair<RHI.CFence, Delegate_OnFenceCompetion>> mQueryFences = new List<KeyValuePair<RHI.CFence, Delegate_OnFenceCompetion>>();
        public void RegFenceQuery(RHI.CFence fence, Delegate_OnFenceCompetion cb)
        {
            lock (mQueryFences)
            {
                mQueryFences.Add(new KeyValuePair<RHI.CFence, Delegate_OnFenceCompetion>(fence, cb));
            }
        }
        public void TickSync()
        {
            lock (mQueryFences)
            {
                for (int i = 0; i < mQueryFences.Count; i++)
                {
                    if (mQueryFences[i].Key.mCoreObject.IsCompletion())
                    {
                        var dlgt = mQueryFences[i].Value;
                        dlgt(mQueryFences[i].Key);
                        mQueryFences.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        public override void EndFrame(UEngine engine)
        {
            RenderContext?.mCoreObject.EndFrame();
        }
        public override void Cleanup(UEngine engine)
        {
            TextureManager?.Cleanup();
            MaterialManager?.Cleanup();
            MaterialManager = null;
            MaterialInstanceManager?.Cleanup();
            MaterialInstanceManager = null;

            BlendStateManager.Cleanup();
            RasterizerStateManager.Cleanup();
            DepthStencilStateManager.Cleanup();
            SamplerStateManager.Cleanup();
            RenderPassManager.Cleanup();
            InputLayoutManager.Cleanup();
            HitproxyManager.Cleanup();

            EffectManager.Cleanup();

            SlateRenderer?.Cleanup();
            SlateRenderer = null;
            RenderContext?.Dispose();
            RenderContext = null;
            RenderSystem?.Dispose();
            RenderSystem = null;
            MainWindow?.Cleanup();
            MainWindow = null;

            Editor.ShaderCompiler.UShaderCodeManager.Instance.Cleanup();
            SDL.SDL_Quit();
        }
        public USlateApplication MainWindow;
        public RHI.CRenderSystem RenderSystem { get; private set; }
        public RHI.CRenderContext RenderContext { get; private set; }
        protected async System.Threading.Tasks.Task<bool> InitGPU(UEngine engine, UInt32 Adapter, ERHIType rhi, IntPtr window, bool bDebugLayer)
        {
            if (UEngine.Instance.PlayMode != EPlayMode.Game)
            {
                Editor.ShaderCompiler.UShaderCodeManager.Instance.Initialize(RName.GetRName("shaders/", RName.ERNameType.Engine));
            }

            unsafe
            {
                var sysDesc = new IRenderSystemDesc();
                sysDesc.WindowHandle = window.ToPointer();
                sysDesc.CreateDebugLayer = bDebugLayer ? 1 : 0;
                RenderSystem = RHI.CRenderSystem.CreateRenderSystem(rhi, ref sysDesc);
                if (RenderSystem == null)
                    return false;

                var deviceNum = RenderSystem.NumOfContext;
                if (Adapter >= deviceNum)
                    return false;
                var rcDesc = new IRenderContextDesc();
                rcDesc.SetDefault();
                RenderSystem.GetContextDesc(Adapter, ref rcDesc);

                rcDesc.CreateDebugLayer = bDebugLayer ? 1 : 0;
                rcDesc.AppHandle = window.ToPointer();
                RenderContext = RenderSystem.CreateContext(ref rcDesc);
                if (RenderContext == null)
                    return false;
            }

            await MaterialManager.Initialize(this);
            await MaterialInstanceManager.Initialize(engine);
            await EffectManager.Initialize(this);
            
            return true;
        }

        #region Manager
        public RHI.UTextureManager TextureManager { get; } = new RHI.UTextureManager();
        public Shader.UMaterialManager MaterialManager { get; private set; } = new Shader.UMaterialManager();
        public Shader.UMaterialInstanceManager MaterialInstanceManager { get; private set; } = new Shader.UMaterialInstanceManager();
        public EGui.Slate.UBaseRenderer SlateRenderer { get; private set; }
        public Graphics.Pipeline.Shader.UEffectManager EffectManager
        {
            get;
        } = new Graphics.Pipeline.Shader.UEffectManager();
        public Graphics.Pipeline.UBlendStateManager BlendStateManager
        {
            get;
        } = new Graphics.Pipeline.UBlendStateManager();
        public Graphics.Pipeline.URasterizerStateManager RasterizerStateManager
        {
            get;
        } = new Graphics.Pipeline.URasterizerStateManager();
        public Graphics.Pipeline.UDepthStencilStateManager DepthStencilStateManager
        {
            get;
        } = new Graphics.Pipeline.UDepthStencilStateManager();
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
        RHI.CConstantBuffer mPerFrameCBuffer;
        public RHI.CConstantBuffer PerFrameCBuffer 
        { 
            get
            {
                if (mPerFrameCBuffer == null)
                {
                    var effect = UEngine.Instance.GfxDevice.EffectManager.DummyEffect;
                    if (effect != null)
                        mPerFrameCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(effect.ShaderProgram, effect.CBPerFrameIndex);
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

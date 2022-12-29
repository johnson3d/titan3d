using System;
using System.Collections.Generic;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class EnginCoreAssemblyDesc : UAssemblyDesc
        {
            public override string Name { get => "EngineCore"; }
            public override string Service { get { return "Global"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Windows"; } }
        }
        static EnginCoreAssemblyDesc AssmblyDesc = new EnginCoreAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS
{
    public enum EPlayMode : int
    {
        Game,
        Editor,
        PlayerInEditor,
        Cook,
        Server,
    }
    [Rtti.Meta]
    public partial class UEngineConfig
    {
        public bool UseRenderDoc { get; set; } = false;
        public string ConfigName;
        [Rtti.Meta]
        public int NumOfThreadPool { get; set; } = -1;
        int mInterval = 15;
        [Rtti.Meta]
        public int Interval {
            get => mInterval;
            set
            {
                mInterval = value;
                mTargetFps = 1000 / value;
            }
        }
        private int mTargetFps;
        public int TargetFps
        {
            get => mTargetFps;
        }
        [Rtti.Meta]
        public RName DefaultTexture { get; set; }
        [Rtti.Meta]
        public int AdaperId { get; set; }
        [Rtti.Meta]
        public Vector4 MainWindow { get; set; } = new Vector4(100, 100, 1280, 720);
        [Rtti.Meta]
        public bool SupportMultWindows { get; set; } = true;
        [Rtti.Meta]
        public bool DoUnitTest { get; set; } = true;
        [Rtti.Meta]
        public NxRHI.ERhiType RHIType { get; set; } = NxRHI.ERhiType.RHI_D3D11;
        [Rtti.Meta]
        public bool HasDebugLayer { get; set; } = false;
        [Rtti.Meta]
        public bool IsDebugShader { get; set; } = false;
        [Rtti.Meta]
        public string MainWindowType { get; set; }// = Rtti.TypeManager.Instance.GetTypeStringFromType(typeof(Editor.MainEditorWindow));
        [Rtti.Meta]
        public RName MainRPolicyName { get; set; }
        [Rtti.Meta]
        public string RpcRootType { get; set; } = Rtti.UTypeDesc.TypeStr(typeof(EngineNS.UTest.UTest_Rpc));
        [Rtti.Meta]
        public bool CookDXBC { get; set; } = true;
        [Rtti.Meta]
        public bool CookDXIL { get; set; } = false;
        [Rtti.Meta]
        public bool CookSPIRV { get; set; } = false;
        [Rtti.Meta]
        public bool CookGLSL { get; set; } = false;
        [Rtti.Meta]
        public bool CookMETAL { get; set; } = false;
        [Rtti.Meta]
        public bool CompressDxt { get; set; } = true;
        [Rtti.Meta]
        public bool CompressEtc { get; set; } = false;
        [Rtti.Meta]
        public bool CompressAstc { get; set; } = false;
        [Rtti.Meta]
        public RName DefaultMaterial { get; set; }// = RName.GetRName("UTest/ttt.material");
        [Rtti.Meta]
        public RName DefaultMaterialInstance { get; set; }// = RName.GetRName("UTest/box_wite.uminst");
        [RName.PGRName(FilterExts = Bricks.CodeBuilder.UMacross.AssetExt, MacrossType = typeof(GamePlay.UMacrossGame))]
        [Rtti.Meta]
        public RName PlayGameName { get; set; }
        [Rtti.Meta]
        public string RootServerURL { get; set; } = "127.0.0.1:2333";
        [Rtti.Meta]
        public Bricks.Network.RPC.EAuthority DefaultAuthority { get; set; } = Bricks.Network.RPC.EAuthority.Server;
    }
    public partial class URuntimeConfig
    {
        public bool VS_StructureBuffer { get; set; } = false;
    }
    public partial class UEngine : UModuleHost<UEngine>
    {
        private static UEngine mInstance;
        public static UEngine Instance { get => mInstance; }
        public EPlayMode PlayMode { get; set; } = EPlayMode.Editor;
        [Rtti.Meta]
        public UEngineConfig Config { get; set; } = new UEngineConfig();
        public URuntimeConfig RuntimeConfig { get; } = new URuntimeConfig();
        public IO.FileManager FileManager
        {
            get;
        } = new IO.FileManager();
        [Rtti.Meta]
        public UTickableManager TickableManager
        {
            get;
        } = new UTickableManager();
        public UEventProcessorManager EventProcessorManager 
        { 
            get;
        } = new UEventProcessorManager();

        public ulong CurrentTickFrame { get; set; } = 0;
        public long CurrentTickCount { get; set; }
        public int ElapseTickCount { get; set; }
        public int FrameCount { get; set; }
        public float TickCountSecond { get; set; }
        public float ElapsedSecond { get; set; }
        protected override UEngine GetHost()
        {
            return this;
        }
        public Profiler.UNativeMemory NativeMemory
        {
            get;
        } = new Profiler.UNativeMemory();
        public static async System.Threading.Tasks.Task<bool> StartEngine(UEngine engine, string cfgFile, bool useRenderDoc)
        {
            System.Threading.Thread.CurrentThread.Name = "Main";
            mInstance = engine;
            return await mInstance.PreInitEngine(cfgFile, useRenderDoc);
        }
        static unsafe void NativeAssertEvent(void* arg0, void* arg1, int arg2)
        {
            System.Diagnostics.Debug.Assert(false);
        }
        static unsafe CoreSDK.FDelegate_FAssertEvent OnNativeAssertEvent = NativeAssertEvent;
        public async System.Threading.Tasks.Task<bool> PreInitEngine(string cfgFile, bool useRenderDoc)
        {
            RttiStructManager.GetInstance().BuildRtti();

            CoreSDK.SetAssertEvent(OnNativeAssertEvent);
            CoreSDK.InitF2MManager();
            NativeMemory.BeginProfiler();

            var t1 = Support.Time.HighPrecision_GetTickCount();
            EngineNS.Rtti.UTypeDescManager.Instance.InitTypes();
            var t2 = Support.Time.HighPrecision_GetTickCount();

            Rtti.UMissingTypeManager.Instance.Initialize();
            EngineNS.Rtti.UClassMetaManager.Instance.LoadMetas();
            var t3 = Support.Time.HighPrecision_GetTickCount();
            
            EngineNS.Profiler.Log.InitLogger();
            UEngine.Instance.AssetMetaManager.LoadMetas();
            var t4 = Support.Time.HighPrecision_GetTickCount();
            
            EngineNS.UCs2CppBase.InitializeNativeCoreProvider();

            StartSystemThreads();

            if (cfgFile == null)
                cfgFile = FileManager.GetRoot(IO.FileManager.ERootDir.Game) + "EngineConfig.cfg";
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "System", $"Load Application Config:{cfgFile}");

            Config = IO.FileManager.LoadXmlToObject<UEngineConfig>(cfgFile);
            if (Config == null)
            {
                Config = new UEngineConfig();
                Config.DefaultTexture = RName.GetRName("texture/checkboard.txpic", RName.ERNameType.Engine);
                Config.DefaultMaterial = RName.GetRName("material/SysDft.material", RName.ERNameType.Engine);
                Config.DefaultMaterialInstance = RName.GetRName("material/box_wite.uminst", RName.ERNameType.Game);
                Config.MainWindowType = Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Editor.UMainEditorApplication));
                Config.MainRPolicyName = RName.GetRName("utest/deferred.rpolicy", RName.ERNameType.Game);
                IO.FileManager.SaveObjectToXml(cfgFile, Config);
            }
            Config.ConfigName = "Titan3D  [" + IO.FileManager.GetPureName(cfgFile) + "]";
            Config.UseRenderDoc = useRenderDoc;

            GatherModules();

            await base.InitializeModules();

            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "System", $"Collect Type Info:{(t2 - t1) / 1000} ms");

                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "System", $"Load Rtti MetaDatas:{(t3 - t2) / 1000} ms");

                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "System", $"Load AssetMetas:{(t4 - t3) / 1000} ms");

                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "System", "PreInitEngine OK");
            }

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            if (Config.DoUnitTest)
            {
                t2 = Support.Time.HighPrecision_GetTickCount();
                EngineNS.UTest.UnitTestManager.DoUnitTests();
                t3 = Support.Time.HighPrecision_GetTickCount();
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "System", $"Unit Test:{(t3 - t2) / 1000} ms");
            }

            var tEnd = Support.Time.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "System", $"Engine PreInit Time:{(tEnd - t1) / 1000} ms");

            this.PluginModuleManager.InitPlugins(this);
            return true;
        }
        [ThreadStatic]
        static Profiler.TimeScope Scope_Tick = Profiler.TimeScopeManager.GetTimeScope(typeof(UEngine), nameof(Tick));
        public uint CaptureRenderDocFrame = uint.MaxValue;
        public bool Tick()
        {
            using(new Profiler.TimeScopeHelper(Scope_Tick))
            {
                var t1 = Support.Time.HighPrecision_GetTickCount();
                ElapseTickCount = (int)((t1 - CurrentTickCount) / 1000);
                CurrentTickCount = t1;
                CurrentTickFrame++;
                CoreSDK.UpdateEngineFrame(CurrentTickFrame);
                InputSystem.BeforeTick();
                if (-1 == InputSystem.Tick(this))
                {
                    return false;
                }

                base.TickModules();

                if (CaptureRenderDocFrame == 0)
                    IRenderDocTool.GetInstance().StartFrameCapture();
                this.ThreadMain.Tick();
                if (CaptureRenderDocFrame == 0)
                {
                    IRenderDocTool.GetInstance().EndFrameCapture();
                    ulong timeStamp = 0;
                    var file = IRenderDocTool.GetInstance().GetCapture(0, ref timeStamp);
                    if (!string.IsNullOrEmpty(file))
                    {
                        var tarFile = IO.FileManager.GetPureName(file) + ".rdc";
                        System.IO.File.Move(file, FileManager.GetPath(IO.FileManager.ERootDir.Cache, IO.FileManager.ESystemDir.RenderDoc) + tarFile); 
                    }
                    CaptureRenderDocFrame = uint.MaxValue;
                }
                CaptureRenderDocFrame--;

                base.EndFrameModules();

                InputSystem.AfterTick();

                Profiler.TimeScopeManager.UpdateAllInstance();

                var t2 = Support.Time.HighPrecision_GetTickCount();
                var delta = (int)((t2 - t1) / 1000);
                var idleTime = Config.Interval - delta;
                if (idleTime > 0)
                {
                    System.Threading.Thread.Sleep(idleTime);
                }

                TickCountSecond = ((float)CurrentTickCount) * 0.001f;
                ElapsedSecond = ((float)ElapseTickCount) * 0.001f;

                FrameCount++;
                return true;
            }
        }
        public void PostQuitMessage()
        {
            Bricks.Input.UInputSystem.PostQuitMessage();
        }
        public void FinalCleanup()
        {
            GfxDevice.RenderCmdQueue.Reset();
            base.CleanupModules();
            TickableManager.Cleanup();
            StopSystemThreads();

            Profiler.TimeScopeManager.FinalCleanup();
            AssetMetaManager.Cleanup();
            EngineNS.Profiler.Log.FinalLogger();

            EngineNS.UCs2CppBase.FinalCleanupNativeCoreProvider();
            CoreSDK.FinalF2MManager();
            RootFormManager.ClearRootForms();
            mInstance = null;
        }
    }
}

using System;
using System.Collections.Generic;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class EnginCoreAssemblyDesc : AssemblyDesc
        {
            public override string Name { get => "EngineCore"; }
            public override string Service { get { return "Global"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Windows"; } }
        }
        static EnginCoreAssemblyDesc AssmblyDesc = new EnginCoreAssemblyDesc();
        public static AssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS
{
    public enum EPlayMode
    {
        Game,
        Editor,
        PlayerInEditor,
        Cook,
    }
    [Rtti.Meta]
    public class UEngineConfig
    {
        [Rtti.Meta]
        public int NumOfThreadPool { get; set; } = 3;
        [Rtti.Meta]
        public int Interval { get; set; } = 15;
        [Rtti.Meta]
        public RName DefaultTexture { get; set; }
        [Rtti.Meta]
        public UInt32 AdaperId { get; set; }
        [Rtti.Meta]
        public Vector4 MainWindow { get; set; } = new Vector4(100, 100, 1280, 720);
        [Rtti.Meta]
        public bool DoUnitTest { get; set; } = true;
        [Rtti.Meta]
        public ERHIType RHIType { get; set; } = ERHIType.RHT_D3D11;
        [Rtti.Meta]
        public bool HasDebugLayer { get; set; } = false;
        [Rtti.Meta]
        public string MainWindowType { get; set; }// = Rtti.TypeManager.Instance.GetTypeStringFromType(typeof(Editor.MainEditorWindow));
        [Rtti.Meta]
        public string MainWindowRPolicy { get; set; }// = Rtti.TypeManager.Instance.GetTypeStringFromType(typeof(Graphics.Pipeline.Mobile.UMobileFSPolicy));
        [Rtti.Meta]
        public string RpcRootType { get; set; } = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(EngineNS.UTest.UTest_Rpc));
        [Rtti.Meta]
        public bool CookDXBC { get; set; } = true;
        [Rtti.Meta]
        public bool CookSPIRV { get; set; } = false;
        [Rtti.Meta]
        public bool CookGLSL { get; set; } = false;
        [Rtti.Meta]
        public bool CookMETAL { get; set; } = false;
    }
    public partial class UEngine : UModuleHost<UEngine>
    {
        private static UEngine mInstance;
        public static UEngine Instance { get => mInstance; }
        public EPlayMode PlayMode { get; set; } = EPlayMode.Editor;
        [Rtti.Meta]
        public UEngineConfig Config { get; set; } = new UEngineConfig();
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
        public long CurrentTickCount { get; set; }
        public int ElapseTickCount { get; set; }
        public int FrameCount { get; set; }
        protected override UEngine GetHost()
        {
            return this;
        }
        public Profiler.UNativeMemory NativeMemory
        {
            get;
        } = new Profiler.UNativeMemory();
        public static void StartEngine(UEngine engine, string cfgFile = null)
        {
            System.Threading.Thread.CurrentThread.Name = "Main";
            mInstance = engine;
            mInstance.PreInitEngine(cfgFile);
        }
        public bool PreInitEngine(string cfgFile=null)
        {
            NativeMemory.BeginProfiler();

            EngineNS.Rtti.UTypeDescManager.Instance.InitTypes();
            EngineNS.Rtti.UClassMetaManager.Instance.LoadMetas();
            EngineNS.Profiler.Log.InitLogger();
            UEngine.Instance.AssetMetaManager.LoadMetas();

            EngineNS.UCs2CppBase.InitializeNativeCoreProvider();

            StartSystemThreads();

            if (cfgFile == null)
                cfgFile = FileManager.GetRoot(IO.FileManager.ERootDir.Game) + "EngineConfig.cfg";
            Config = IO.FileManager.LoadXmlToObject<UEngineConfig>(cfgFile);
            if (Config == null)
            {
                Config = new UEngineConfig();
                Config.DefaultTexture = RName.GetRName("texture/checkboard.txpic", RName.ERNameType.Engine);
                IO.FileManager.SaveObjectToXml(cfgFile, Config);
            }

            GatherModules();

            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "PreInitEngine OK");
            System.Action action = async () =>
            {
                await base.InitializeModules();
                if (Config.DoUnitTest)
                {
                    EngineNS.UTest.UnitTestManager.DoUnitTests();
                }
            };
            action();

            return true;
        }
        [ThreadStatic]
        static Profiler.TimeScope Scope_Tick = Profiler.TimeScopeManager.GetTimeScope(typeof(UEngine), nameof(Tick));
        public bool Tick()
        {
            using(new Profiler.TimeScopeHelper(Scope_Tick))
            {
                var t1 = Support.Time.HighPrecision_GetTickCount();
                ElapseTickCount = (int)((t1 - CurrentTickCount) / 1000);
                CurrentTickCount = t1;
                CoreSDK.UpdateEngineTick(CurrentTickCount);

                if (-1 == EventProcessorManager.Tick(this))
                    return false;

                base.TickModules();

                this.ThreadMain.Tick();

                base.EndFrameModules();

                Profiler.TimeScopeManager.UpdateAllInstance();

                var t2 = Support.Time.HighPrecision_GetTickCount();
                var delta = (int)((t2 - t1) / 1000);
                var idleTime = Config.Interval - delta;
                if (idleTime > 0)
                {
                    System.Threading.Thread.Sleep(idleTime);
                }

                FrameCount++;
                return true;
            }
        }
        public void PostQuitMessage()
        {
            var closeEvent = new SDL2.SDL.SDL_Event();
            closeEvent.type = SDL2.SDL.SDL_EventType.SDL_QUIT;
            SDL2.SDL.SDL_PushEvent(ref closeEvent);
        }
        public void FinalCleanup()
        {
            StopSystemThreads();

            base.CleanupModules();

            TickableManager.Cleanup();
            Profiler.TimeScopeManager.FinalCleanup();
            AssetMetaManager.Cleanup();
            EngineNS.Profiler.Log.FinalLogger();

            EngineNS.UCs2CppBase.FinalCleanupNativeCoreProvider();
            mInstance = null;
        }
    }
}

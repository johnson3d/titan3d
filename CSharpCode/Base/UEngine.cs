using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
    public enum EMultiRenderMode
    {
        None,
        Queue,
        QueueNextFrame,
    }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.UEngineConfig@EngineCore" })]
    public partial class TtEngineConfig
    {
        public const int MajorVersion = 1;
        public const int MiniVersion = 4;
        public void SaveConfig(string sltFile)
        {
            IO.TtFileManager.SaveObjectToXml(sltFile, this);
        }
        [Rtti.Meta]
        public EMultiRenderMode MultiRenderMode { get; set; } = EMultiRenderMode.QueueNextFrame;
        [Rtti.Meta]
        public bool UsePhysxMT { get; set; } = true;
        [Rtti.Meta]
        public bool UseRenderDoc { get; set; } = false;
        [Rtti.Meta]
        public bool Feature_UseRVT { get; set; } = false;
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
        public bool IsGpuBaseValidation { get; set; } = false;
        [Rtti.Meta]
        public bool IsDebugShader { get; set; } = false;
        [Rtti.Meta]
        public bool IsGpuDump { get; set; } = true;//if true, engine will disable debuglayer&renderdoc
        [Rtti.Meta]
        public string MainWindowType { get; set; }// = Rtti.TypeManager.Instance.GetTypeStringFromType(typeof(Editor.MainEditorWindow));
        [Rtti.Meta]
        public RName MainRPolicyName { get; set; }
        [Rtti.Meta]
        public RName SimpleRPolicyName { get; set; }
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
        [Rtti.Meta]
        public List<TtGlobalConfig> GlobalConfigs { get; set; } = new List<TtGlobalConfig>();
        [Rtti.Meta]
        public RName EditorFont { get; set; }
        public string EditorLanguage { get; set; } = "English";
        [Rtti.Meta]
        public RName UIDefaultTexture { get; set; }
        [Rtti.Meta]
        public bool IsWriteShaderDebugFile { get; set; } = false;
        public TtEngineConfig()
        {
            EditorFont = RName.GetRName("fonts/Roboto-Regular.ttf", RName.ERNameType.Engine);
            UIDefaultTexture = RName.GetRName("texture/white.srv", RName.ERNameType.Engine);
        }
    }
    public partial class URuntimeConfig
    {
        public bool VS_StructureBuffer { get; set; } = false;
    }
    [Rtti.Meta]
    public partial class TtEngine : UModuleHost<TtEngine>
    {
        public static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                {
                    return i.Substring(startWith.Length);
                }
            }
            return null;
        }
        public TtEngine(string[] args)
        {
            mFileManager = new IO.TtFileManager(args);
        }
        private static TtEngine mInstance;
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.Unserializable | Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public static TtEngine Instance { get => mInstance; }
        public EPlayMode PlayMode { get; set; } = EPlayMode.Editor;
        [Rtti.Meta]
        public TtEngineConfig Config { get; set; }
        public URuntimeConfig RuntimeConfig { get; set; }
        private IO.TtFileManager mFileManager;
        public IO.TtFileManager FileManager
        {
            get => mFileManager;
        }
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
        public long EngineStartTickCountUS { get; set; }
        public long CurrentTickCountUS { get; set; }
        public uint CurrentTick24BitMS
        {
            get
            {
                return ((uint)(CurrentTickCountUS / 1000)) % 0x00ffffff;
            }
        }
        public float ElapseTickCountMS { get; set; }  // 毫秒
        public int FrameCount { get; set; }
        [Rtti.Meta]
        public float TickCountSecond { get; set; }
        public float ElapsedSecond { get; set; }
        protected override TtEngine GetHost()
        {
            return this;
        }
        public Profiler.UNativeMemory NativeMemory
        {
            get;
        } = new Profiler.UNativeMemory();
        public static async System.Threading.Tasks.Task<bool> StartEngine(TtEngine engine, string cfgFile)
        {
            System.Threading.Thread.CurrentThread.Name = "Main";
            mInstance = engine;
            engine.Config = new TtEngineConfig();
            engine.RuntimeConfig = new URuntimeConfig();

            return await mInstance.PreInitEngine(cfgFile);
        }
        #region callback
        private static unsafe void NativeAssertEvent(void* arg0, void* arg1, int arg2)
        {
            System.Diagnostics.Debug.Assert(false);
        }
        private static unsafe CoreSDK.FDelegate_FAssertEvent OnNativeAssertEvent = NativeAssertEvent;
        private static bool IsDredDump = false;
        private static void NativeOnGpuDeviceRemoved(EngineNS.NxRHI.IGpuDevice arg0)
        {
            if (IsDredDump)
                return;
            IsDredDump = true;
            var tmpDir = $"{System.DateTime.Now.Year}_{System.DateTime.Now.Month}_{System.DateTime.Now.Day}_{System.DateTime.Now.Hour}_{System.DateTime.Now.Minute}_{System.DateTime.Now.Second}";
            var tmpFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "dred/" + tmpDir + "/";
            IO.TtFileManager.SureDirectory(tmpFile);
            GpuDump.NvAftermath.OnDredDump(arg0, tmpFile);
        }
        private static CoreSDK.FDelegate_FOnGpuDeviceRemoved OnGpuDeviceRemoved = NativeOnGpuDeviceRemoved;
        #endregion
        public async System.Threading.Tasks.Task<bool> PreInitEngine(string cfgFile)
        {
            EngineStartTickCountUS = Support.Time.HighPrecision_GetTickCount();
            RttiStructManager.GetInstance().BuildRtti();

            CoreSDK.SetAssertEvent(OnNativeAssertEvent);
            CoreSDK.InitF2MManager();
            NativeMemory.BeginProfiler();

            var t1 = Support.Time.HighPrecision_GetTickCount();
            EngineNS.Rtti.UTypeDescManager.Instance.InitTypes();
            var t2 = Support.Time.HighPrecision_GetTickCount();

            EngineNS.Rtti.TtClassMetaManager.Instance.LoadMetas();
            var t3 = Support.Time.HighPrecision_GetTickCount();
            
            EngineNS.Profiler.Log.InitLogger();
            TtEngine.Instance.AssetMetaManager.LoadMetas();
            var t4 = Support.Time.HighPrecision_GetTickCount();
            
            EngineNS.UCs2CppBase.InitializeNativeCoreProvider();

            StartSystemThreads();

            if (cfgFile == null)
                cfgFile = FileManager.GetRoot(IO.TtFileManager.ERootDir.Game) + "EngineConfig.cfg";
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Load Application Config:{cfgFile}");

            Config = IO.TtFileManager.LoadXmlToObject<TtEngineConfig>(cfgFile);
            if (Config == null)
            {
                System.Diagnostics.Debug.Assert(false);
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Load failed: {cfgFile}");
                Config = new TtEngineConfig();
                Config.DefaultTexture = RName.GetRName("texture/checkboard.txpic", RName.ERNameType.Engine);
                Config.DefaultMaterial = RName.GetRName("material/SysDft.material", RName.ERNameType.Engine);
                Config.DefaultMaterialInstance = RName.GetRName("material/box_wite.uminst", RName.ERNameType.Game);
                Config.MainWindowType = Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Editor.UMainEditorApplication));
                Config.MainRPolicyName = RName.GetRName("utest/deferred.rpolicy", RName.ERNameType.Game);
                Config.SimpleRPolicyName = RName.GetRName("graphics/deferred_simple.rpolicy", RName.ERNameType.Engine);                
                Config.GlobalConfigs.Add(new TtGlobalConfig()
                {
                    Name = "RenderDocCallStacks",
                    Value = 1.ToString(),
                    ValueType = NxRHI.EShaderVarType.SVT_Int
                });
                Config.GlobalConfigs.Add(new TtGlobalConfig()
                {
                    Name = "RenderDocSaveAllInitials",
                    Value = 1.ToString(),
                    ValueType = NxRHI.EShaderVarType.SVT_Int
                });
                Config.SaveConfig(cfgFile);
            }

            if (Config.IsGpuDump)
            {
                if (Config.HasDebugLayer)
                {
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"Config: GpuDump = true; HasDebugLayer will be set as false");
                    Config.HasDebugLayer = false;
                }
                if (Config.UseRenderDoc)
                {
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"Config: GpuDump = true; UseRenderDoc will be set as false");
                    Config.UseRenderDoc = false;
                }
            }
            Config.ConfigName = $"Titan3D.{TtEngineConfig.MajorVersion}.{TtEngineConfig.MiniVersion} [{IO.TtFileManager.GetPureName(cfgFile)}]";

            CoreSDK.SetOnGpuDeviceRemovedCallBack(OnGpuDeviceRemoved);
            
            //Config.UseRenderDoc = useRenderDoc;
            foreach(var i in Config.GlobalConfigs)
            {
                i.SetToGlobalConfig();
            }

            this.PluginModuleManager.InitPlugins(this);

            GatherModules();

            await base.InitializeModules();

            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Collect Type Info:{(t2 - t1) / 1000} ms");

                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Load Rtti MetaDatas:{(t3 - t2) / 1000} ms");

                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Load AssetMetas:{(t4 - t3) / 1000} ms");

                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "PreInitEngine OK");
            }

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            if (Config.DoUnitTest)
            {
                t2 = Support.Time.HighPrecision_GetTickCount();
                EngineNS.UTest.UnitTestManager.DoUnitTests();
                t3 = Support.Time.HighPrecision_GetTickCount();
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Unit Test:{(t3 - t2) / 1000} ms");
            }

            InitPreIntegratedDF();

            var tEnd = Support.Time.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Engine PreInit Time:{(tEnd - t1) / 1000} ms");

            return true;
        }
        [ThreadStatic]
        static Profiler.TimeScope Scope_Tick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtEngine), nameof(Tick));
        public bool Tick()
        {
            using(new Profiler.TimeScopeHelper(Scope_Tick))
            {
                var t1 = Support.Time.HighPrecision_GetTickCount();
                var newTickCount = t1 - EngineStartTickCountUS;
                ElapseTickCountMS = (newTickCount - CurrentTickCountUS) * 0.001f;
                CurrentTickCountUS = newTickCount;
                CurrentTickFrame++;
                CoreSDK.UpdateEngineFrame(CurrentTickFrame);
                InputSystem.BeforeTick();
                if (-1 == InputSystem.Tick(this))
                {
                    return false;
                }

                var bCapturing = GfxDevice.RenderSwapQueue.BeginFrameCapture();

                //Do engine frame tick
                {
                    TickBeginFrame();
                    base.TickModules();
                    this.ThreadMain.Tick();
                    TickSync();
                    FContextTickableManager.GetInstance().ThreadTick();
                    base.EndFrameModules();
                }

                if (bCapturing)
                    GfxDevice.RenderSwapQueue.EndFrameCapture();

                InputSystem.AfterTick();

                Profiler.TimeScopeManager.UpdateAllInstance();

                var t2 = Support.Time.HighPrecision_GetTickCount();
                var delta = (int)((t2 - t1) / 1000);
                var idleTime = Config.Interval - delta;
                if (idleTime > 0)
                {
                    System.Threading.Thread.Sleep(idleTime);
                }

                TickCountSecond = ((float)CurrentTickCountUS) * 0.001f;
                ElapsedSecond = ((float)ElapseTickCountMS) * 0.001f;

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
            GfxDevice.RenderSwapQueue.Reset();
            TickableManager.Cleanup();
            StopSystemThreads();

            Profiler.TimeScopeManager.FinalCleanup();
            AssetMetaManager.Cleanup();
            EngineNS.Profiler.Log.FinalLogger();

            EngineNS.UCs2CppBase.FinalCleanupNativeCoreProvider();
            CoreSDK.FinalF2MManager();
            RootFormManager.ClearRootForms();

            TtObjectPoolManager.Instance.Cleanup();
            base.CleanupModules();

            lock (FinalCleanupActions)
            {
                foreach (var i in FinalCleanupActions)
                {
                    i();
                }
                FinalCleanupActions.Clear();
            }

            mInstance = null;
        }
        List<System.Action> FinalCleanupActions = new List<Action>();
        public void RegFinalCleanupAction(Action act)
        {
            lock(FinalCleanupActions)
            {
                FinalCleanupActions.Add(act);
            }
        }
    }
}

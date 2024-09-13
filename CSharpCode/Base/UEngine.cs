using NPOI.SS.Formula.Functions;
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
        public bool IsReverseZ { get; set; } = true;
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
        [Rtti.Meta]
        public bool IsParrallelWorldGather { get; set; } = true;
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
            //EditorFont = RName.GetRName("fonts/Roboto-Regular.ttf", RName.ERNameType.Engine);
            EditorFont = RName.GetRName("fonts/NotoSansSC-Regular.otf", RName.ERNameType.Engine);
            UIDefaultTexture = RName.GetRName("texture/white.srv", RName.ERNameType.Engine);
        }
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
        public TtEventProcessorManager EventProcessorManager
        {
            get;
        } = new TtEventProcessorManager();

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
        public float FPS { get; set; }
        private int FPSCounter = 10;
        private long FPSBeginTime = 0;
        protected override TtEngine GetHost()
        {
            return this;
        }
        public Profiler.TtNativeMemory NativeMemory
        {
            get;
        } = new Profiler.TtNativeMemory();
        public static async System.Threading.Tasks.Task<bool> StartEngine(TtEngine engine, string cfgFile)
        {
            System.Threading.Thread.CurrentThread.Name = "Main";
            mInstance = engine;
            engine.Config = new TtEngineConfig();

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
            EngineStartTickCountUS = Support.TtTime.HighPrecision_GetTickCount();
            RttiStructManager.GetInstance().BuildRtti();

            CoreSDK.SetAssertEvent(OnNativeAssertEvent);
            CoreSDK.InitF2MManager();
            NativeMemory.BeginProfiler();

            var t1 = Support.TtTime.HighPrecision_GetTickCount();
            EngineNS.Rtti.UTypeDescManager.Instance.InitTypes();
            var t2 = Support.TtTime.HighPrecision_GetTickCount();

            EngineNS.Rtti.TtClassMetaManager.Instance.LoadMetas();
            var t3 = Support.TtTime.HighPrecision_GetTickCount();
            
            EngineNS.Profiler.Log.InitLogger();
            TtEngine.Instance.AssetMetaManager.LoadMetas();
            var t4 = Support.TtTime.HighPrecision_GetTickCount();

            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Collect Type Info:{(t2 - t1) / 1000} ms");

                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Load Rtti MetaDatas:{(t3 - t2) / 1000} ms");

                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Load AssetMetas:{(t4 - t3) / 1000} ms");
            }

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

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            if (Config.DoUnitTest)
            {
                t2 = Support.TtTime.HighPrecision_GetTickCount();
                EngineNS.UTest.UnitTestManager.DoUnitTests();
                t3 = Support.TtTime.HighPrecision_GetTickCount();
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Unit Test:{(t3 - t2) / 1000} ms");
            }

            InitPreIntegratedDF();

            var tEnd = Support.TtTime.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Engine PreInit Time:{(tEnd - t1) / 1000} ms");
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "PreInitEngine OK");

            return true;
        }
        [ThreadStatic]
        static Profiler.TimeScope mScope_Tick;
        static Profiler.TimeScope Scope_Tick
        {
            get
            {
                if (mScope_Tick == null)
                    mScope_Tick = new Profiler.TimeScope(typeof(TtEngine), nameof(Tick));
                return mScope_Tick;
            }
        } 
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTickModules;
        private static Profiler.TimeScope ScopeTickModules
        {
            get
            {
                if (mScopeTickModules == null)
                    mScopeTickModules = new Profiler.TimeScope(typeof(TtEngine), nameof(TickModules));
                return mScopeTickModules;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeInputSystem;
        private static Profiler.TimeScope ScopeInputSystem
        {
            get
            {
                if (mScopeInputSystem == null)
                    mScopeInputSystem = new Profiler.TimeScope(typeof(TtEngine), nameof(InputSystem));
                return mScopeInputSystem;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeSleep;
        private static Profiler.TimeScope ScopeSleep
        {
            get
            {
                if (mScopeSleep == null)
                    mScopeSleep = new Profiler.TimeScope(typeof(TtEngine), nameof(Tick) + ".Sleep");
                return mScopeSleep;
            }
        }
        int QuitFrame = -1;
        public bool Tick()
        {
            using(new Profiler.TimeScopeHelper(Scope_Tick))
            {
                var t1 = Support.TtTime.HighPrecision_GetTickCount();
                var newTickCount = t1 - EngineStartTickCountUS;
                ElapseTickCountMS = (newTickCount - CurrentTickCountUS) * 0.001f;
                CurrentTickCountUS = newTickCount;
                CurrentTickFrame++;
                FPSCounter--;
                if (FPSCounter == 0)
                {
                    FPS = (float)((double)(10) / ((double)(newTickCount - FPSBeginTime) / 1000000.0));
                    FPSBeginTime = newTickCount;
                    FPSCounter = 10;
                }

                using (new Profiler.TimeScopeHelper(ScopeInputSystem))
                {
                    CoreSDK.UpdateEngineFrame(CurrentTickFrame);
                    InputSystem.BeforeTick();
                    if (-1 == InputSystem.Tick(this))
                    {
                        QuitFrame = 2;
                    }
                }
                
                var bCapturing = GfxDevice.RenderSwapQueue.BeginFrameCapture();

                //Do engine frame tick
                {
                    TickBeginFrame();

                    using (new Profiler.TimeScopeHelper(ScopeTickModules))
                    {
                        base.TickModules();
                    }
                    
                    this.ThreadMain.Tick();

                    TickSync();
                    
                    using (new Profiler.TimeScopeHelper(ScopeTickModules))
                    {
                        FContextTickableManager.GetInstance().ThreadTick();
                        base.EndFrameModules();
                    }   
                }

                if (bCapturing)
                    GfxDevice.RenderSwapQueue.EndFrameCapture();

                using (new Profiler.TimeScopeHelper(ScopeInputSystem))
                {
                    InputSystem.AfterTick();

                    Profiler.TimeScopeManager.UpdateAllInstance();
                }

                var t2 = Support.TtTime.HighPrecision_GetTickCount();
                var delta = (int)((t2 - t1) / 1000);
                var idleTime = Config.Interval - delta;
                if (idleTime > 0)
                {
                    using (new Profiler.TimeScopeHelper(ScopeSleep))
                    {
                        System.Threading.Thread.Sleep(idleTime);
                    }   
                }

                TickCountSecond = ((float)CurrentTickCountUS) * 0.001f;
                ElapsedSecond = ((float)ElapseTickCountMS) * 0.001f;

                FrameCount++;
                if (QuitFrame < 0)
                {
                    return true;
                }
                else if (QuitFrame == 0)
                {
                    return false;
                }
                else
                {
                    QuitFrame--;
                    return true;
                }
            }
        }
        public void PostQuitMessage()
        {
            Bricks.Input.TtInputSystem.PostQuitMessage();
        }
        public void FinalCleanup()
        {
            GfxDevice.RenderSwapQueue.Reset();
            TickableManager.Cleanup();
            StopSystemThreads();

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
            
            Profiler.TimeScopeManager.FinalCleanup();
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

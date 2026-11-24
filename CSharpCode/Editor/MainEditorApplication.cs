//#define UseWindowTest

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EngineNS.UI.Editor;
using EngineNS.Macross;
using System.Runtime.InteropServices;
using EngineNS.GamePlay.Scene;
using System.Diagnostics;
//using SDL2;

namespace EngineNS.Editor
{
    /// <summary>
    /// Manages the "Plugins" main menu. Plugins that implement
    /// <see cref="Bricks.AssemblyLoader.IPluginMenu"/> are automatically
    /// collected after plugin loading and drawn as sub-menus.
    /// Owned by <see cref="TtMainEditorApplication"/>.
    /// </summary>
    public class TtPluginMenuManager
    {
        private readonly List<Bricks.AssemblyLoader.IPluginMenu> mMenuProviders = new();
        private int mLastPluginCount = -1;

        public void RegisterMenuProvider(Bricks.AssemblyLoader.IPluginMenu provider)
        {
            if (!mMenuProviders.Contains(provider))
                mMenuProviders.Add(provider);
        }

        public void UnregisterMenuProvider(Bricks.AssemblyLoader.IPluginMenu provider)
        {
            mMenuProviders.Remove(provider);
        }

        /// <summary>
        /// Re-scan loaded plugin modules when the count changes (new plugin loaded / hot-reload).
        /// </summary>
        private void SyncFromPluginModules()
        {
            var manager = TtEngine.Instance?.PluginModuleManager;
            if (manager == null)
                return;

            int currentCount = manager.PluginModules.Count;
            if (currentCount == mLastPluginCount)
                return;
            mLastPluginCount = currentCount;

            foreach (var kv in manager.PluginModules)
            {
                var plugin = kv.Value.GetPluginObject<Bricks.AssemblyLoader.IPlugin>();
                if (plugin is Bricks.AssemblyLoader.IPluginMenu pluginMenu && !mMenuProviders.Contains(pluginMenu))
                    mMenuProviders.Add(pluginMenu);
            }
        }

        public unsafe void DrawPluginsMenu()
        {
            SyncFromPluginModules();

            if (mMenuProviders.Count == 0)
                return;

            if (ImGuiAPI.BeginMenu(TtLocalization.Tr("Plugins"), true))
            {
                for (int i = 0; i < mMenuProviders.Count; i++)
                {
                    var provider = mMenuProviders[i];
                    if (ImGuiAPI.BeginMenu(provider.PluginMenuName, true))
                    {
                        provider.OnDrawPluginMenu();
                        ImGuiAPI.EndMenu();
                    }
                }
                ImGuiAPI.EndMenu();
            }
        }
    }

    public class TtRenderDocFrameCountMenuItem : EGui.UIProxy.IUIProxyBase
    {
        public void Cleanup() { }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public unsafe bool OnDraw(in ImDrawList drawList, in Support.TtAnyPointer drawData)
        {
            ImGuiAPI.Text(TtLocalization.Tr("Frames:"));
            ImGuiAPI.SameLine(0, 4);
            ImGuiAPI.SetNextItemWidth(120);
            int frameCount = TtEngine.Instance.EditorInstance.RenderDocCaptureFrameCount;
            if (ImGuiAPI.InputInt("##CapFrames", (int*)&frameCount, 1, 1, ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
            {
                TtEngine.Instance.EditorInstance.RenderDocCaptureFrameCount = Math.Max(1, frameCount);
            }
            return false;
        }
    }

    public partial class TtMainEditorApplication : TtSlateApplication, ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public TtAssetEditorManager AssetEditorManager { get; } = new TtAssetEditorManager();
        
        public TtMainEditorApplication()
        {
            mLogWatcher = new EGui.Controls.TtLogWatcher();
            mCpuProfiler = new Editor.Forms.TtCpuProfilerForm();
            mGpuProfiler = new Editor.Forms.TtGpuProfiler();
            mMemProfiler = new Forms.TtMemoryProfiler();
            mMainInspector = new Forms.TtInspector();
            mMetaViewer = new TtMetaVersionViewer();
            //WorldViewportSlate = new UEditorWorldViewportSlate(true);
            //mWorldOutliner = new Editor.Forms.UWorldOutliner(WorldViewportSlate);

            mBrickManager = new Bricks.ProjectGen.UBrickManager();
            mEditorSettings = new Forms.TtEditorSettings();
            mAndroidBuildPanel = new Forms.TtAndroidBuildPanel();
            mCookPanel = new Forms.TtCookManagerPanel();
            mCookProgress = new Forms.TtCookProgressForm();
            mPIEController = new TtPIEController();
        }
        private bool IsVisible = true;
        //public Editor.Forms.UWorldOutliner mWorldOutliner;
        public EGui.Controls.TtLogWatcher mLogWatcher;
        public Editor.Forms.TtCpuProfilerForm mCpuProfiler;
        public Editor.Forms.TtGpuProfiler mGpuProfiler;
        public Editor.Forms.TtMemoryProfiler mMemProfiler;
        public Editor.Forms.TtInspector mMainInspector;
        public TtMetaVersionViewer mMetaViewer;
        public Bricks.ProjectGen.UBrickManager mBrickManager = null;
        public Editor.Forms.TtEditorSettings mEditorSettings;
        public Editor.Forms.TtAndroidBuildPanel mAndroidBuildPanel;
        public Editor.Forms.TtCookManagerPanel mCookPanel;
        public Editor.Forms.TtCookProgressForm mCookProgress;
        public TtPIEController mPIEController;
        public TtPluginMenuManager PluginMenuManager { get; } = new TtPluginMenuManager();

        //public UEditorWorldViewportSlate WorldViewportSlate = null;
        //public override EGui.Slate.UWorldViewportSlate GetWorldViewportSlate()
        //{
        //    return WorldViewportSlate;
        //}        
        ///////////////////////////////////////////
        public EGui.Controls.TtContentBrowser ContentBrowser = new EGui.Controls.TtContentBrowser();
        public override void Cleanup()
        {
            TtEngine.Instance?.TickableManager.RemoveTickable(this);
            // cook worker 是子进程: 不显式收尾的话它要等 stdin EOF 且在飞 job 跑完才退, 编辑器关掉后还会挂很久
            Cook.TtCookManager.ShutdownIfCreated();
#if (UseWindowTest)
            mWinTest.Cleanup();
#endif
#if PWindow
            CoreCLRManager.Stop();
            CoreSDK.DisposeObject(ref mClrProfiler);
#endif
            base.Cleanup();
        }
        public override async Thread.Async.TtTask<bool> InitializeApplication(NxRHI.TtGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);

            await ContentBrowser.Initialize();
            TtEngine.RootFormManager.RegRootForm(ContentBrowser);

            //await WorldViewportSlate.Initialize(this, rpName, 0, 1);

            await mMainInspector.Initialize();

            await mEditorSettings.Initialize();
            await mAndroidBuildPanel.Initialize();
            await mCookPanel.Initialize();
            await mCookProgress.Initialize();
            // 两个 cook 窗体常驻 RootForms: 批次由 TickableManager 驱动, 表单不可见时只是不绘制, 不会停摆
            TtEngine.RootFormManager.RegRootForm(mCookPanel);
            TtEngine.RootFormManager.RegRootForm(mCookProgress);
            mCookPanel.Visible = false;
            mCookProgress.Visible = false;
            //TtEngine.Instance.Config.PlayGameName = RName.GetRName("utest/test_game01.macross");

            mMainInspector.PropertyGrid.PGName = TtLocalization.Label("MainInspector", "###MainInspector");
            mMainInspector.PropertyGrid.Target = EGui.UIProxy.StyleConfig.Instance;// WorldViewportSlate;

            TtEngine.Instance.TickableManager.AddTickable(this);

            EGui.UIProxy.StyleConfig.Instance.ResetStyle();
            /////////////////////////////////
#if (UseWindowTest)
            mWinTest.Initialize();
            Editor.UMainEditorApplication.RegRootForm(mWinTest);
#endif
            /////////////////////////////////

            InitMainMenu();

            await AssetEditorManager.Initialize();

            // MCP 工具服务(日志/打开场景/地形 goto), 供 AI 代理或调试脚本远程调用
            TtEditorMCPTools.RegisterAndStartup();
            return true;
        }
        protected override void PrepareImGuiIniFile(string imguiIniFile)
        {
            var stampFile = imguiIniFile + ".main-dock-layout-v1";
            if (System.IO.File.Exists(stampFile))
                return;

            try
            {
                if (System.IO.File.Exists(imguiIniFile))
                {
                    var text = System.IO.File.ReadAllText(imguiIniFile);
                    if (IsLegacyScatteredMainEditorLayout(text))
                    {
                        var backupFile = imguiIniFile + ".legacy-scattered.bak";
                        System.IO.File.Copy(imguiIniFile, backupFile, true);
                        System.IO.File.Delete(imguiIniFile);
                        Console.WriteLine($"Reset legacy ImGui layout cache: {imguiIniFile}");
                    }
                }
                System.IO.File.WriteAllText(stampFile, "main-dock-layout-v1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Prepare ImGui layout cache failed: {ex.Message}");
            }
        }
        static bool IsLegacyScatteredMainEditorLayout(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            if (text.Contains("[Window][ContentBrowser]") == false)
                return false;
            if (text.Contains("[Docking][Data]") == false)
                return false;
            if (text.Contains("DockId="))
                return false;

            var count = 0;
            var index = 0;
            while ((index = text.IndexOf("ViewportId=", index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += "ViewportId=".Length;
            }
            return count >= 3;
        }
#if PWindow
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif
        public void InitMainMenu()
        {
            mMenuItems = new List<EGui.UIProxy.MenuItemProxy>()
            {
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = TtLocalization.Tr("File"),
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.NamedMenuSeparator()
                        {
                            Name = "OPEN",
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("Load..."),
                            Shortcut = "Ctrl+L",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(140.0f/1024, 265.0f/1024),
                                UVMax = new Vector2(156.0f/1024, 281.0f/1024),
                            },
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                // Do sth
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("Save..."),
                            Shortcut = "Ctrl+S",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(124.0f/1024, 305.0f/1024),
                                UVMax = new Vector2(140.0f/1024, 321.0f/1024),
                            },
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                // Do sth
                            },
                        },
                    },
                },
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = TtLocalization.Tr("Tools"),
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("Cook Manager"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mCookPanel.Visible = !mCookPanel.Visible;
                                item.Selected = mCookPanel.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("Cook Progress"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mCookProgress.Visible = !mCookProgress.Visible;
                                item.Selected = mCookProgress.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("CompileMacross"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                var csFilesPath = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
                                var projectFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameProject;
                                var assemblyFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameAssembly;

                                if (TtMacrossModule.CompileGameProject(csFilesPath, projectFile, assemblyFile, TtEngine.Instance.CurrentPlatform))
                                {
                                    TtEngine.Instance.MacrossModule.ReloadAssembly(assemblyFile);
                                }
                                //var gameAssembly = TtEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Root) + TtEngine.Instance.EditorInstance.Config.GameAssembly;

                                //TtEngine.Instance.MacrossModule.ReloadAssembly(gameAssembly);
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("PrintAttachmentPool"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                TtEngine.Instance.GfxDevice.AttachBufferManager.PrintCachedBuffer = true;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("Cap"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                unsafe
                                {
#if PWindow
                                    var targetWindow = GetRenderDocTargetWindow();
                                    if (targetWindow == null)
                                    {
                                        Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error,
                                            "RenderDoc 抓帧失败：找不到可呈现的焦点视口。");
                                        return;
                                    }
                                    IRenderDocTool.GetInstance().SetGpuDevice(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject);
                                    IRenderDocTool.GetInstance().SetActiveWindow(targetWindow.HWindow.ToPointer());
#endif
                                    var frameCount = Math.Max(1, TtEngine.Instance.EditorInstance.RenderDocCaptureFrameCount);
                                    TtEngine.Instance.GfxDevice.RenderQueue.RemainingCaptureFrames = frameCount - 1;
                                    TtEngine.Instance.GfxDevice.RenderQueue.CaptureRenderDocFrame = true;
                                }
                            },
                        },
                        new TtRenderDocFrameCountMenuItem(),
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("CapMem"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                var save = PrevMemCapture;
                                PrevMemCapture = new EngineNS.Profiler.TtNativeMemCapture();
                                PrevMemCapture.CaptureNativeMemoryState();
                                if (save != null)
                                    PrevMemCapture.GetIncreaseTypes(save);
                                    
                                //TtEngine.Instance.EventPoster.RunOn((state)=>
                                //{
                                    //return true;
                                //}, Thread.Async.EAsyncTarget.Main);
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("ShowConsole"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                item.Selected = !item.Selected;
#if PWindow
                                var handle = GetConsoleWindow();
                                ShowWindow(handle, item.Selected ? 1 : 0);
#endif
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("OpenTracer"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                // 注意: 这条路径上的代码必须一直留在引擎线程上。
                                // 引擎的 async 续体只有在 TtContextThread.CurrentContext 非空时才会被排队
                                // (ContextThread.cs: if(e.ContinueThread != null) 否则静默丢弃)。一旦用 await Task.Delay
                                // 把控制权交给线程池, 后续 OpenTrace 的 await 就再也不会回来——现象是原生 log 已经
                                // connect success, 但 Enabled 永远 false, 一帧都发不出去。所以等待子进程启动交给 TickTracerLaunch 轮询。
                                Action action = async ()=>
                                {
                                    if (msTracerLaunching)
                                        return;     // 上一次启动还在等, 不要叠着再起一个
                                    if (await TtEngine.Instance.Tracer.OpenTrace())
                                        return;
                                    var apps = Process.GetProcessesByName("Tracer");
                                    if (apps.Length != 0)
                                    {
                                        Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Tracer server down!");
                                        return;
                                    }
                                    StartTracerProcess();
                                };
                                action();
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("CaptureTraceEvents"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                // 事件流量是快照通道的几十倍, 只能显式抓 N 帧, 抓满自停
                                TtEngine.Instance.Tracer.StartCapture();
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("StopCaptureTraceEvents"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                TtEngine.Instance.Tracer.StopCapture();
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("StartTraceRecord"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                // 对齐 UE 的 Trace.Start: 引擎进程自己开 trace writer 边跑边写,
                                // 不依赖 Tracer 是否连着
                                var tracer = TtEngine.Instance.Tracer;
                                if (tracer.StartRecord(null, out var err) == false)
                                {
                                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Trace record start failed: {err}");
                                    return;
                                }
                                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Trace recording -> {tracer.RecordFile}");
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("StopTraceRecord"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                var tracer = TtEngine.Instance.Tracer;
                                int frames = tracer.RecordedFrames;
                                var path = tracer.StopRecord();
                                if (string.IsNullOrEmpty(path))
                                {
                                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, "Trace record is not running");
                                    return;
                                }
                                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Trace recorded {frames} frames -> {path}");
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("CloseTracer"),
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                TtEngine.Instance.Tracer.CloseTrace();
                            },
                        },
                    },
                },
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = TtLocalization.Tr("Windows"),
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("GC"),
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                System.GC.Collect();
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("ContentBrowser"),
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                ContentBrowser.Visible = !ContentBrowser.Visible;
                                var application = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                                item.Selected = ContentBrowser.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("LogWatcher"),
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mLogWatcher.Visible = !mLogWatcher.Visible;
                                var application = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                                item.Selected = mLogWatcher.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("CpuProfiler"),
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mCpuProfiler.Visible = !mCpuProfiler.Visible;
                                var application = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                                //mCpuProfiler.DockId = application.CenterDockId;
                                item.Selected = mCpuProfiler.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("GpuProfiler"),
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mGpuProfiler.Visible = !mGpuProfiler.Visible;
                                var application = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                                //mCpuProfiler.DockId = application.CenterDockId;
                                item.Selected = mGpuProfiler.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("MemProfiler"),
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mMemProfiler.Visible = !mMemProfiler.Visible;
                                var application = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                                item.Selected = mMemProfiler.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("MainInspector"),
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mMainInspector.Visible = !mMainInspector.Visible;
                                item.Selected = mMainInspector.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("MetaViewer"),
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mMetaViewer.Visible = !mMetaViewer.Visible;
                                item.Selected = mMetaViewer.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("BrickManager"),
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mBrickManager.Visible = !mBrickManager.Visible;
                                item.Selected = mBrickManager.Visible;
                                if(mBrickManager.Visible)
                                {
                                    var task = mBrickManager.Initialize();
                                }
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("PIE Controller"),
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mPIEController.Visible = !mPIEController.Visible;
                                item.Selected = mPIEController.Visible;
                                if(mPIEController.Visible)
                                    TtEngine.Instance.TaskCollector.AddWaitTask(mPIEController.Initialize());
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("Settings"),
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mEditorSettings.Visible = !mEditorSettings.Visible;
                                item.Selected = mEditorSettings.Visible;
                                if(mEditorSettings.Visible)
                                    TtEngine.Instance.TaskCollector.AddWaitTask(mEditorSettings.Initialize());
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = TtLocalization.Tr("Android Build"),
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                mAndroidBuildPanel.Visible = !mAndroidBuildPanel.Visible;
                                item.Selected = mAndroidBuildPanel.Visible;
                                if(mAndroidBuildPanel.Visible)
                                    TtEngine.Instance.TaskCollector.AddWaitTask(mAndroidBuildPanel.Initialize());
                            },
                        },
                    },
                },
            };
        }

        #region DrawGui
        bool _showDemoWindow = false;
        public float LeftWidth = 0;
        public float CenterWidth = 0;
        static string mNeedFocusWindowName;
        public static string NeedFocusPanelName;
        static int mFrameDelay = 0;
        // 由编辑器拉起的 Tracer 进程必须一直持有引用: Process 被回收会关掉重定向管道,
        // 子进程后续写 stdout 就会因管道断开而抛异常
        static Process msTracerProcess;
        static bool msTracerLaunching;
        static bool msTracerExitReported;
        static volatile bool msTracerStartSignaled;
        // 子进程 stdout/stderr 的尾部: 它崩在原生层时不会有托管异常, 唯一的线索就是崩之前打了什么
        static readonly System.Text.StringBuilder msTracerErr = new System.Text.StringBuilder();
        static int msTracerWaitFrames;
        // 按帧数而不是墙钟估超时: 主线程跑满时帧数增长慢, 但子进程启动也是同量级的慢
        const int TracerWaitMaxFrames = 2400;
        public static string NeedFocusWindowName
        {
            get => mNeedFocusWindowName;
            set
            {
                mNeedFocusWindowName = value;
                mFrameDelay = 2;
            }
        }

        ////////////////////////
#if (UseWindowTest)
        UIWindowsTest mWinTest = new UIWindowsTest();
#endif
        ////////////////////////

        //public uint LeftDockId { get; private set; } = 0;
        //public uint CenterDockId { get; private set; } = 0;

        public UInt32 ActiveViewportId = UInt32.MaxValue;
        internal unsafe Graphics.Pipeline.TtPresentWindow GetRenderDocTargetWindow()
        {
            if (TtEngine.Instance.Config.SupportMultWindows == false)
                return NativeWindow?.CanRender == true ? NativeWindow : null;
            // MCP 调用时编辑器可能已失焦，此时使用最近活动的真实视口。
            Graphics.Pipeline.TtPresentWindow activeWindow = null;
            var platformIO = ImGuiAPI.GetPlatformIO();
            var mainViewport = ImGuiAPI.GetMainViewport();
            var count = ImGuiAPI.PlatformIO_Viewports_Size(platformIO);
            for (int i = 0; i < count; i++)
            {
                var viewport = ImGuiAPI.PlatformIO_Viewports_Get(platformIO, i);
                // 多窗口模式的 main viewport 是不 Present 的隐藏占位窗。
                if (viewport == mainViewport || viewport->RendererUserData == IntPtr.Zero.ToPointer())
                    continue;
                var gcHandle = GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
                var viewportData = gcHandle.Target as TtDockWindowSDL.ViewportData;
                var presentWindow = viewportData?.PresentWindow;
                if (presentWindow?.CanRender != true)
                    continue;
                if (TtNativeWindow.IsInputFocus((IntPtr)viewport->PlatformHandle))
                    return presentWindow;
                if (viewport->ID == ActiveViewportId)
                    activeWindow = presentWindow;
            }
            return activeWindow;
        }
        protected unsafe override void OnDrawUI()
        {
            {
                var count = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                for (int i = 0; i < count; i++)
                {
                    var pViewport = ImGuiAPI.PlatformIO_Viewports_Get(ImGuiAPI.GetPlatformIO(), i);
                    var nativeWindow = pViewport->PlatformHandle;
                    if (TtNativeWindow.IsInputFocus((IntPtr)nativeWindow))
                    {
                        ActiveViewportId = pViewport->ID;
                    }
                }
            }
            if (IsVisible == false)
            {
                AssetEditorManager.CloseAll();
                var num = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                if (num == 1)
                {//只剩下被特意隐藏的主Viewport了
                    TtEngine.Instance.PostQuitMessage();
                }
                return;
            }

//            if (LeftDockId == 0)
//            {
//                LeftDockId = ImGuiAPI.GetID("LeftDocker");
//                CenterDockId = ImGuiAPI.GetID("CenterDocker");

//                mCpuProfiler.DockId = CenterDockId;
//#if PWindow
//                mClrProfiler.DockId = CenterDockId;
//#endif
//                //WorldViewportSlate.DockId = CenterDockId;
//                //mWorldOutliner.DockId = LeftDockId;
//                ContentBrowser.DockId = CenterDockId;
//                mBrickManager.DockId = CenterDockId;
//                mEditorSettings.DockId = LeftDockId;
//            }

            var io = ImGuiAPI.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_DockingEnable) != 0)
            {
                //ImGuiAPI.DockSpaceOverViewport(0, ImGuiAPI.GetMainViewport(), ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, null);
                
                //var dockspace_id = ImGuiAPI.GetID("MyDockSpace");
                //ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                //var winClass = new ImGuiWindowClass();
                //winClass.UnsafeCallConstructor();
                //var sz = new Vector2(0.0f, 0.0f);                
                //ImGuiAPI.DockSpace(dockspace_id, ref sz, dockspace_flags, ref winClass);
                //winClass.UnsafeCallDestructor();
            }
            try
            {
                var mainPos = new Vector2(0);
                ImGuiAPI.SetNextWindowPos(in mainPos, ImGuiCond_.ImGuiCond_FirstUseEver, in mainPos);
                var wsz = new Vector2(1290, 800);
                ImGuiAPI.SetNextWindowSize(in wsz, ImGuiCond_.ImGuiCond_FirstUseEver);
                uint dockId = 0;
                var result = EGui.UIProxy.DockProxy.BeginMainForm(TtEngine.Instance.Config.ConfigName, ref IsVisible, ref dockId, //ImGuiWindowFlags_.ImGuiWindowFlags_NoMove |
                                                                                                                                 //ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                    ImGuiWindowFlags_.ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                    ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar);
                if (result)
                {
                    wsz = ImGuiAPI.GetWindowSize();
                    mainPos = ImGuiAPI.GetWindowPos();
                    //DrawToolBar();
                    DrawMainMenu();

                    //ImGuiAPI.Separator();
                    //ImGuiAPI.Columns(2, null, true);
                    //if (LeftWidth == 0)
                    //{
                    //    ImGuiAPI.SetColumnWidth(0, wsz.X * 0.15f);
                    //}
                    //var min = ImGuiAPI.GetWindowContentRegionMin();
                    //var max = ImGuiAPI.GetWindowContentRegionMax();

                    //DrawLeft(ref min, ref max);
                    //LeftWidth = ImGuiAPI.GetColumnWidth(0);
                    //ImGuiAPI.NextColumn();

                    //DrawCenter(ref min, ref max);
                    //CenterWidth = ImGuiAPI.GetColumnWidth(1);
                    //ImGuiAPI.NextColumn();

                    //ImGuiAPI.Columns(1, null, true);
                    if (ImGuiAPI.BeginChild("CenterWindow", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                    {
                        ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                        var sz = new Vector2(0.0f, 0.0f);
                        //var winClass = new ImGuiWindowClass();
                        //winClass.UnsafeCallConstructor();
                        //ImGuiAPI.DockSpace(CenterDockId, ref sz, dockspace_flags, ref winClass);
                        //winClass.UnsafeCallDestructor();

                        fixed(ImGuiWindowClass* dockclsPtr = &EGui.UIProxy.DockProxy.MainFormDockClass)
                        {
                            ImGuiAPI.DockSpace(EGui.UIProxy.DockProxy.MainFormDockClass.m_ClassId, &sz, dockspace_flags, dockclsPtr);
                        }
                    }
                    ImGuiAPI.EndChild();
                }
                EGui.UIProxy.DockProxy.EndMainForm(result);

                //WorldViewportSlate.DockId = CenterDockId;

                TtEngine.RootFormManager.DrawRootForms();
#if (UseWindowTest)
                mWinTest.OnDraw();
#endif
                AssetEditorManager.OnDraw();

                //TtEngine.Instance.StopOperateCover.OnDraw(in mainPos, in wsz);
            }
            catch
            {

            }

            if (_showDemoWindow)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                var pos = new Vector2(650, 20);
                var pivot = new Vector2(0, 0);
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_FirstUseEver, in pivot);
                ImGuiAPI.ShowDemoWindow(ref _showDemoWindow);
            }

            TickToOperationMenus();
            TickTracerLaunch();

            if(!string.IsNullOrEmpty(NeedFocusWindowName))
            {
                // 延迟几帧重新focus，避免打开的dock窗口没有显示在前面
                mFrameDelay--;
                if(mFrameDelay <= 0)
                {
                    ImGuiAPI.SetWindowFocus(NeedFocusWindowName);
                    NeedFocusWindowName = null;
                }
            }
        }
        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        List<EGui.UIProxy.MenuItemProxy> mMenusToRemove = new List<EGui.UIProxy.MenuItemProxy>();
        List<EGui.UIProxy.MenuItemProxy> mMenusToAdd = new List<EGui.UIProxy.MenuItemProxy>();
        byte mDelayFrame = 0;
        public void AppendToMainMenu(params EGui.UIProxy.MenuItemProxy[] menus)
        {
            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                mMenusToAdd.AddRange(menus);
                mDelayFrame = 2;
                return true;
            }, Thread.Async.EAsyncTarget.Main);
        }
        public void AppendToMainMenu(List<EGui.UIProxy.MenuItemProxy> menus)
        {
            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                mMenusToAdd.AddRange(menus);
                mDelayFrame = 2;
                return true;
            }, Thread.Async.EAsyncTarget.Main);
        }
        public void RemoveFromMainMenu(params EGui.UIProxy.MenuItemProxy[] menus)
        {
            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                mMenusToRemove.AddRange(menus);
                mDelayFrame = 2;
                return true;
            }, Thread.Async.EAsyncTarget.Main);
        }
        public void RemoveFromMainMenu(List<EGui.UIProxy.MenuItemProxy> menus)
        {
            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                mMenusToRemove.AddRange(menus);
                mDelayFrame = 2;
                return true;
            }, Thread.Async.EAsyncTarget.Main);
        }
        void TickTracerLaunch()
        {
            var proc = msTracerProcess;
            if (proc == null)
            {
                msTracerLaunching = false;
                return;
            }
            bool exited;
            try
            {
                exited = proc.HasExited;
            }
            catch
            {
                exited = true;
            }

            if (msTracerLaunching == false)
            {
                // 连上之后也要盯一眼: 子进程静默崩掉时必须把原因(exit code + stderr)写进日志,
                // 否则只会表现为"跟踪器窗口没了、一帧也不来", 完全无从下手
                if (exited && msTracerExitReported == false)
                {
                    msTracerExitReported = true;
                    int code = 0;
                    try
                    {
                        code = proc.ExitCode;
                    }
                    catch
                    {
                    }
                    var errText = TracerOutputTail();
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning,
                        $"Tracer process exited after connect(code={code}), output:{errText}");
                }
                return;
            }

            if (msTracerStartSignaled == false && exited == false && --msTracerWaitFrames > 0)
                return;

            msTracerLaunching = false;
            if (msTracerStartSignaled)
            {
                // 这里不能丢引用: Process 被回收时终结器会关掉重定向管道, 而且异步读取也挂着它。
                // 一旦管道断了, 子进程下一次写 stdout 就抛异常退出(上一次就是这么把 Tracer 弄死的)。
                // 必须在本函数所处的主线程上发起: OpenTrace 内部的 await 依赖 CurrentContext
                ConnectTracerAsync();
                return;
            }
            msTracerProcess = null;
            var err = TracerOutputTail();
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning,
                $"Tracer server start failed(exit={exited}), output:{err}");
            try
            {
                proc.Dispose();
            }
            catch
            {
            }
        }
        async void ConnectTracerAsync()
        {
            if (await TtEngine.Instance.Tracer.OpenTrace() == false)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Tracer server start failed!");
            }
            else
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"Tracer connected, ready to CaptureTraceEvents");
            }
        }
        static void AppendTracerOutput(string line)
        {
            // 诊断性输出必须实时可见: 子进程只要不退出, 攒在缓冲里的东西就永远看不到
            if (line.Contains("[Net]") || line.Contains("[Tracer]") || line.Contains("Exception") || line.Contains("   at "))
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"tracer:{line}");
            }
            lock (msTracerErr)
            {
                // 只留尾部: 崩溃原因永远在最后几行, 而 stdout 会一直流
                msTracerErr.AppendLine(line);
                if (msTracerErr.Length > 8192)
                    msTracerErr.Remove(0, msTracerErr.Length - 4096);
            }
        }
        static string TracerOutputTail()
        {
            lock (msTracerErr)
            {
                // 堆栈可能很长, 但比起丢掉真正的原因, 多刷几行日志是值得的
                return msTracerErr.ToString();
            }
        }
        void StartTracerProcess()
        {
            var startInfo = new ProcessStartInfo();
            var dir = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource);
            startInfo.FileName = IO.TtFileManager.CombinePath(dir, $"Binaries/Tracer/debug/{TtEngine.DotNetVersion}/Tracer.exe");
            startInfo.Arguments = $"EngineRoot={dir} config={dir}content\\config\\engineconfig_windows_tracer.jscfg NativeDLL={TtEngine.Instance.Config.NativeDll}";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            // 必须接住 stdout/stderr: 子进程崩在启动阶段(比如缺原生 DLL)或原生层时,
            // 原因只会出现在这两处输出里, 而它自己不会把异常传上来
            startInfo.RedirectStandardError = true;
            var proc = new Process();
            proc.StartInfo = startInfo;
            proc.EnableRaisingEvents = true;
            msTracerStartSignaled = false;
            lock (msTracerErr)
            {
                msTracerErr.Clear();
            }
            proc.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    return;
                if (e.Data.Contains("Tracer Server start success."))
                {
                    msTracerStartSignaled = true;
                    return;
                }
                // 不能只挑启动标记: Tracer 自己打的诊断(比如 drop broken frame)也只在 stdout 上
                AppendTracerOutput($"[out]{e.Data}");
            };
            proc.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    AppendTracerOutput($"[err]{e.Data}");
            };
            try
            {
                proc.Start();
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Tracer process start exception: {ex.Message}");
                proc.Dispose();
                return;
            }
            msTracerProcess = proc;
            msTracerLaunching = true;
            msTracerExitReported = false;
            msTracerWaitFrames = TracerWaitMaxFrames;
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }
        void TickToOperationMenus()
        {
            if (mDelayFrame > 0)
            {
                mDelayFrame--;
                return;
            }
            for (int i=0; i< mMenusToRemove.Count; i++)
            {
                if (mMenuItems.Contains(mMenusToRemove[i]))
                    mMenuItems.Remove(mMenusToRemove[i]);
                else
                {
                    for(int j=0; j<mMenuItems.Count; j++)
                    {
                        if (mMenuItems[j].MenuName == mMenusToRemove[i].MenuName)
                        {
                            mMenuItems[j].Eliminate(mMenusToRemove[i]);
                            break;
                        }
                    }
                }
            }
            mMenusToRemove.Clear();

            for (int i = 0; i < mMenusToAdd.Count; i++)
            {
                if (mMenuItems.Contains(mMenusToAdd[i]))
                    continue;

                bool hasSameName = false;
                for (int j = 0; j < mMenuItems.Count; j++)
                {
                    if (mMenuItems[j].MenuName == mMenusToAdd[i].MenuName)
                    {
                        hasSameName = true;
                        mMenuItems[j].Merge(mMenusToAdd[i]);
                        break;
                    }
                }
                if (!hasSameName)
                    mMenuItems.Add(mMenusToAdd[i]);
            }
            mMenusToAdd.Clear();
        }
        private void DrawMainMenu()
        {
            if (ImGuiAPI.BeginMenuBar())
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                for (int i = 0; i < mMenuItems.Count; i++)
                {
                    mMenuItems[i].OnDraw(in drawList, in Support.TtAnyPointer.Default);
                }

                PluginMenuManager.DrawPluginsMenu();

                ImGuiAPI.EndMenuBar();
            }
            //if (ImGuiAPI.BeginMenuBar())
            //{
            //    if (ImGuiAPI.BeginMenu("File", true))
            //    {
            //        if (ImGuiAPI.MenuItem("Load", null, false, true))
            //        {
                        
            //        }
            //        if (ImGuiAPI.MenuItem("Save", null, false, true))
            //        {
                        
            //        }
            //        ImGuiAPI.EndMenu();
            //    }
            //    if (ImGuiAPI.BeginMenu("Windows", true))
            //    {
            //        var check = mWorldOutliner.Visible;
            //        ImGuiAPI.Checkbox("##mWorldOutliner", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if ( ImGuiAPI.MenuItem("WorldOutliner", null, false, true))
            //        {
            //            mWorldOutliner.Visible = !mWorldOutliner.Visible;
            //        }
            //        check = mCpuProfiler.Visible;
            //        ImGuiAPI.Checkbox("##mCpuProfiler", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("CpuProfiler", null, false, true))
            //        {
            //            mCpuProfiler.Visible = !mCpuProfiler.Visible;
            //        }
            //        check = mMainInspector.Visible;
            //        ImGuiAPI.Checkbox("##mMainInspector", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("MainInspector", null, false, true))
            //        {
            //            mMainInspector.Visible = !mMainInspector.Visible;
            //        }
            //        check = mMetaViewer.Visible;
            //        ImGuiAPI.Checkbox("##mMetaViewer", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("MetaViewer", null, false, true))
            //        {
            //            mMetaViewer.Visible = !mMetaViewer.Visible;
            //        }
            //        ImGuiAPI.EndMenu();
            //    }
            //    if (ImGuiAPI.BeginMenu("View", true))
            //    {
            //        var check = this.WorldViewportSlate.RenderPolicy.DisableShadow;
            //        ImGuiAPI.Checkbox("##DisableShadow", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("DisableShadow", null, false, true))
            //        {
            //            this.WorldViewportSlate.RenderPolicy.DisableShadow = !check;
            //        }

            //        check = this.WorldViewportSlate.RenderPolicy.DisableAO;
            //        ImGuiAPI.Checkbox("##DisableAO", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("DisableAO", null, false, true))
            //        {
            //            this.WorldViewportSlate.RenderPolicy.DisableAO = !check;
            //        }

            //        ImGuiAPI.EndMenu();
            //    }
            //    ImGuiAPI.EndMenuBar();
            //}
        }
        //bool[] mToolBtn_IsMouseDown = new bool[4];
        //bool[] mToolBtn_IsMouseHover = new bool[4];
        //private unsafe void DrawToolBar()
        //{
        //    var drawList = ImGuiAPI.GetWindowDrawList();
        //    EGui.UIProxy.Toolbar.BeginToolbar(drawList);

        //    if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(drawList, ref mToolBtn_IsMouseDown[1], ref mToolBtn_IsMouseHover[1], null, "Play"))
        //    {
        //        var task = OnPlayGame(TtEngine.Instance.Config.PlayGameName);
        //    }
        //    if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(drawList, ref mToolBtn_IsMouseDown[2], ref mToolBtn_IsMouseHover[2], null, "Stop"))
        //    {

        //    }

        //    EGui.UIProxy.Toolbar.EndToolbar();
        //}
        //protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        //{
        //    var size = new Vector2(-1, -1);
        //    if (ImGuiAPI.BeginChild("LeftWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
        //    {
        //        ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
        //        var winClass = new ImGuiWindowClass();
        //        winClass.UnsafeCallConstructor();
        //        var sz = new Vector2(0.0f, 0.0f);
        //        ImGuiAPI.DockSpace(LeftDockId, in sz, dockspace_flags, in winClass);
        //        winClass.UnsafeCallDestructor();
        //    }
        //    ImGuiAPI.EndChild();
        //}
        //protected unsafe void DrawCenter(ref Vector2 min, ref Vector2 max)
        //{
        //    var size = new Vector2(-1, -1);
        //    if (ImGuiAPI.BeginChild("CenterWindow", in size, false, 
        //        ImGuiWindowFlags_.ImGuiWindowFlags_None))
        //    {
        //        ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
        //        var sz = new Vector2(0.0f, 0.0f);
        //        //var winClass = new ImGuiWindowClass();
        //        //winClass.UnsafeCallConstructor();
        //        //ImGuiAPI.DockSpace(CenterDockId, ref sz, dockspace_flags, ref winClass);
        //        //winClass.UnsafeCallDestructor();

        //        ImGuiAPI.DockSpace(CenterDockId, &sz, dockspace_flags, (ImGuiWindowClass*)0);
        //    }
        //    ImGuiAPI.EndChild();
        //}
        //protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        //{
        //    //var size = new Vector2(-1, -1);
        //    //if (ImGuiAPI.BeginChild("RightWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
        //    //{
        //    //    ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
        //    //    var winClass = new ImGuiWindowClass();
        //    //    winClass.UnsafeCallConstructor();
        //    //    var sz = new Vector2(0.0f, 0.0f);
        //    //    ImGuiAPI.DockSpace(RightDockId, in sz, dockspace_flags, in winClass);
        //    //    winClass.UnsafeCallDestructor();
        //    //}
        //    //ImGuiAPI.EndChild();
        //}
        #endregion

        //async System.Threading.Tasks.Task OnPlayGame(RName assetName)
        //{
        //    await TtEngine.Instance.StartPlayInEditor(TtEngine.Instance.GfxDevice.SlateApplication, assetName);
        //}
        #region TestCode
        public static async System.Threading.Tasks.Task TestCreateScene(Graphics.Pipeline.TtViewportSlate vpSlate,GamePlay.TtWorld world, GamePlay.Scene.TtNode root, bool hideTerrain = false)
        {
            var materials = new Graphics.Pipeline.Shader.TtMaterialInstance[2];
            materials[0] = await RName.GetRName("utest/ddd.uminst").GetAsset<Graphics.Pipeline.Shader.TtMaterialInstance>();
            materials[1] = await RName.GetRName("utest/ground.uminst").GetAsset<Graphics.Pipeline.Shader.TtMaterialInstance>(   );
            if (materials[0] == null)
                return;
            {
                var meshData = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
                meshData.MeshName = RName.GetRName("utest/mesh/skysphere001.ums");
                var meshNode = await TtNode.SpawnNode<GamePlay.Scene.TtSkyNode>(root, async (nd) =>
                {
                    nd.Parent = root;
                    nd.Placement.Scale = new Vector3(800.0f);
                    nd.Placement.Position = DVector3.Zero;
                    nd.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
                    nd.NodeData.Name = "SkySphere";
                    nd.IsAcceptShadow = false;
                    nd.IsCastShadow = false;
                }, meshData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.TtPlacement));
            }

            {
                var meshData = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
                meshData.MeshName = RName.GetRName("utest/puppet/mesh/puppet.ums");
                meshData.CollideName = RName.GetRName("utest/puppet/mesh/puppet.vms");
                var meshNode = await TtNode.SpawnNode<GamePlay.Scene.TtMeshNode>(root, async (nd)=>
                {
                    nd.Placement.SetTransform(new DVector3(0, 0, 0), new Vector3(0.01f), Quaternion.Identity);
                    nd.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                    nd.NodeData.Name = "Robot0";
                    nd.IsAcceptShadow = false;
                    nd.IsCastShadow = true;
                }, meshData);

                {
                    var mesh1 = new Graphics.Mesh.TtRenderMesh();
                    await mesh1.Initialize(RName.GetRName("utest/puppet/mesh/puppet.ums"), Rtti.TtTypeDesc.TypeOf(typeof(Graphics.Mesh.TtMdfSkinMesh)));
                    var meshData1 = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
                    var meshNode1 = await TtNode.SpawnNode<GamePlay.Scene.TtMeshNode>(meshNode, async (nd) =>
                    {
                        nd.RenderMesh = mesh1;
                        nd.NodeData.Name = "Robot1";
                        nd.Parent = meshNode;
                        nd.Placement.SetTransform(new DVector3(3, 3, 3), new Vector3(0.01f), Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI / 4));
                        nd.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent;
                        nd.IsAcceptShadow = false;
                        nd.IsCastShadow = true;

                        nd.MeshName = RName.GetRName("utest/puppet/mesh/puppet.ums");
                        (nd.NodeData as GamePlay.Scene.TtMeshNode.TtMeshNodeData).MdfQueueType = Rtti.TtTypeDesc.TypeStr(typeof(Graphics.Mesh.TtMdfSkinMesh));
                        (nd.NodeData as GamePlay.Scene.TtMeshNode.TtMeshNodeData).AtomType = Rtti.TtTypeDesc.TypeStr(typeof(Graphics.Mesh.TtRenderMesh.TtAtom));
                    }, meshData1);
                }
            }

            {
                var meshData = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
                meshData.MeshName = RName.GetRName("utest/brdf_test/chair2.ums");
                //meshData.CollideName = RName.GetRName("utest/puppet/mesh/puppet.vms");
                var meshNode = await TtNode.SpawnNode<GamePlay.Scene.TtMeshNode>(root, async (nd)=>
                {
                    nd.Placement.SetTransform(new DVector3(0, 10, 0), new Vector3(1.0f), Quaternion.Identity);
                    nd.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                    nd.NodeData.Name = "Robot_Chair";
                    nd.IsAcceptShadow = false;
                    nd.IsCastShadow = true;
                }, meshData);
            }

            {
                var meshData = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
                meshData.MeshName = RName.GetRName("utest/brdf_test/chair2.ums");
                //meshData.CollideName = RName.GetRName("utest/puppet/mesh/puppet.vms");
                var meshNode = await TtNode.SpawnNode<GamePlay.Scene.TtMeshNode>(root, async (nd)=>
                {
                    nd.Placement.SetTransform(new DVector3(0, 10, 0), new Vector3(1.0f), Quaternion.Identity);
                    nd.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                    nd.NodeData.Name = "Robot_Chair_Far";
                    nd.IsAcceptShadow = false;
                    nd.IsCastShadow = true;

                    nd.Placement.Position = new DVector3(1024 * 100, 0, 0);
                }, meshData);
            }

            {
                var nebulaData = new Bricks.Particle.TtNebulaNode.TtNebulaNodeData();
                //nebulaData.MeshName = RName.GetRName("utest/mesh/unit_sphere.ums");
                nebulaData.NebulaName = RName.GetRName("utest/particle001.nebula");
                var meshNode = await TtNode.SpawnNode<Bricks.Particle.TtNebulaNode>(root, async (nd)=>
                {
                    nd.Placement.Position = DVector3.Zero;
                    nd.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
                    nd.NodeData.Name = "NebulaParticle";
                    nd.IsAcceptShadow = false;
                    nd.IsCastShadow = false;
                }, nebulaData);
            }

            {
                var lightData = new GamePlay.Scene.TtPointLightNode.TtLightNodeData();
                lightData.Name = "PointLight0";
                lightData.Intensity = 100.0f;
                lightData.Radius = 20.0f;
                lightData.Color = new Vector3(1, 0, 0);
                var lightNode = GamePlay.Scene.TtPointLightNode.AddPointLightNode(world, root, lightData, new DVector3(10, 10, 10));
            }

            if (hideTerrain == false)
            {
                var terrainData = new Bricks.Terrain.CDLOD.TtTerrainNode.TtTerrainData();
                terrainData.Name = "TerrainGen";
                terrainData.PgcName = RName.GetRName("UTest/terraingen.pgc");
                var terrainNode = await TtNode.SpawnNode<Bricks.Terrain.CDLOD.TtTerrainNode>(root, null, terrainData);
                terrainNode.Placement.Position = DVector3.Zero;
                terrainNode.IsAcceptShadow = true;
                terrainNode.SetActiveCenter(in DVector3.Zero);
            }

            var gridNode = await GamePlay.Scene.TtGridNode.AddGridNode(world, root);
            //gridNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.Invisible);

            gridNode.ViewportSlate = vpSlate;

        }

        public static async System.Threading.Tasks.Task TestCreateCharacter(GamePlay.TtWorld world, GamePlay.Scene.TtNode root, bool hideTerrain = false)
        {
            var characterController = new GamePlay.Controller.TtCharacterController();
            var playerData = new GamePlay.Player.TtPlayer.TtPlayerData() { CharacterController = characterController };
            var player = await TtNode.SpawnNode<GamePlay.Player.TtPlayer>(root, null, playerData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.TtPlacement));
            
            var character = await TtNode.SpawnNode<GamePlay.Character.TtCharacter>(null, null, null, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.TtPlacement), world);
            characterController.ControlledCharacter = character;
            var movement = new GamePlay.Movemnet.TtMovement();
            movement.Parent = character;
        }
        #endregion

        #region Tick
        public void TickLogic(float ellapse)
        {
            //WorldViewportSlate.TickLogic(ellapse);
        }
        public void TickRender(float ellapse)
        {
            //WorldViewportSlate.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            //WorldViewportSlate.TickSync(ellapse);

            //OnDrawSlate();
        }
        #endregion

        public EngineNS.Profiler.TtNativeMemCapture PrevMemCapture;
    }
}

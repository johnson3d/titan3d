using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoreEditor
{
    /// <summary>
    /// EditorLoading.xaml 的交互逻辑
    /// </summary>
    public partial class EditorLoading : ResourceLibrary.WindowBase
    {
        public EditorLoading()
        {
            Program.LoadingWindow = this;

            InitializeComponent();

            EngineNS.Profiler.Log.OnReportLog += async (EngineNS.Profiler.ELogTag tag, string category, string format, object[] args)=>
            {
                await UpdateProcess(tag, $"{tag.ToString()}  {category}:{format}", 0);
            };
        }
        
        public bool InitializeRuning = false;
        bool mInitializeFinished = false;
        
        public async System.Threading.Tasks.Task UpdateProcess(EngineNS.Profiler.ELogTag tag, string info, double progressChange)
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(()=>
            {
                var tb = new TextBlock()
                {
                    Text = info,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    LineHeight = 10,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                };
                switch(tag)
                {
                    case EngineNS.Profiler.ELogTag.Error:
                        Foreground = Brushes.Red;
                        break;
                    case EngineNS.Profiler.ELogTag.Warning:
                        Foreground = Brushes.Yellow;
                        break;
                    case EngineNS.Profiler.ELogTag.Fatal:
                        Foreground = Brushes.Orange;
                        break;
                    default:
                        break;
                }
                StackPanel_Infos.Children.Add(tb);

                var count = 10;
                if (StackPanel_Infos.Children.Count > count)
                    StackPanel_Infos.Children.RemoveAt(0);

                int invisibleCount = StackPanel_Infos.Children.Count / 2;
                for(int i=0; i<invisibleCount + 1; i++)
                {
                    var tx = StackPanel_Infos.Children[i] as TextBlock;
                    tx.Opacity = (i + 0.2) / invisibleCount;
                }

                //TextBlock_Info.Text = info;
                return true;
            },EngineNS.Thread.Async.EAsyncTarget.Main);
        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (mInitializeFinished)
                return;
            // 控件在主线程创建
            //var pmwIns = Program.MainWinInstance.PluginWin;
            //var vcmIns = EditorCommon.VersionControl.VersionControlManager.Instance;

            ////Program.MainWinInstance = new MainWindow();
            ////Program.MainWinInstance.Show();

            var noUse = AsyncInitializeProcess();
        }



        private async System.Threading.Tasks.Task AsyncInitializeProcess()
        {
            try
            {
                InitializeRuning = true;

                //GenerateToolMenu(0.05);

                //var assembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Server, EngineNS.CEngine.Instance.FileManager.Root + EngineNS.CEngine.Instance.Desc.Server_Directory + "/ServerCommon.dll");
                //EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly(EngineNS.ECSType.Server, EngineNS.EPlatformType.PLATFORM_WIN, "cscommon", assembly);
                //assembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Server, EngineNS.CEngine.Instance.FileManager.Root + EngineNS.CEngine.Instance.Desc.Server_Directory + "/Server.dll");
                //EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly(EngineNS.ECSType.Server, EngineNS.EPlatformType.PLATFORM_WIN, "game", assembly);

                var assembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, EngineNS.CEngine.Instance.FileManager.Bin + "CodeGenerateSystem.dll");
                EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("CodeGenerateSystem", assembly);
                assembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, EngineNS.CEngine.Instance.FileManager.Bin + "Macross.dll");
                EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("Macross", assembly);
                EngineNS.CEngine.Instance.MetaClassManager.CheckNewMetaClass();
                EngineNS.IO.Serializer.TypeDescGenerator.Instance.BuildTypes(assembly);

                var gameDllKeyName = EditorCommon.GameProjectConfig.Instance.GameDllFileName;// "Game.Windows";
                var gameDllDir = EditorCommon.GameProjectConfig.Instance.GameDllDir;// EngineNS.CEngine.Instance.FileManager.Bin + "Batman/";
                // 清理复制出来的文件
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.Bin, gameDllKeyName + "*", System.IO.SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    EngineNS.CEngine.Instance.FileManager.DeleteFile(file);
                }

                //var id = "_" + Guid.NewGuid().ToString().Replace("-", "_");
                //EngineNS.CEngine.Instance.FileManager.CopyFile(gameDllDir + gameDllKeyName + ".dll", EngineNS.CEngine.Instance.FileManager.Bin + gameDllKeyName + id + ".dll", true);
                //EngineNS.CEngine.Instance.FileManager.CopyFile(gameDllDir + gameDllKeyName + ".pdb", EngineNS.CEngine.Instance.FileManager.Bin + gameDllKeyName + id + ".pdb", true);
                //EditorCommon.GamePlay.Instance.GameDllAbsFileName = EngineNS.CEngine.Instance.FileManager.Bin + gameDllKeyName + id + ".dll";
                //assembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, EditorCommon.GamePlay.Instance.GameDllAbsFileName, "", false, false);
                //EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("Game", assembly);
                // 计算游戏dll文件Hash
                //var array = EngineNS.IO.FileManager.ReadFile(gameDllDir + gameDllKeyName + ".dll");
                //var algo = new System.Security.Cryptography.MD5CryptoServiceProvider();// .SHA256Managed();
                //mGameDllHash = algo.ComputeHash(array);
                mGameDllHash = new byte[16];

                // 监控游戏代码文件变化
                var csWatcher = new System.IO.FileSystemWatcher();
                csWatcher.Path = (EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot).Replace("/", "\\");
                csWatcher.Filter = "*.cs";
                csWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime | System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.LastAccess;
                csWatcher.IncludeSubdirectories = true;
                csWatcher.Changed += CSWatcher_Changed;
                csWatcher.EnableRaisingEvents = true;

                // 监控文件改变
                var watcher = new System.IO.FileSystemWatcher();
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(gameDllDir);
                watcher.Path = gameDllDir.Replace("/", "\\");  
                watcher.Filter = "dcf_*.dbg";
                watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                watcher.Changed += GameDllWatcher_Changed;
                watcher.EnableRaisingEvents = true;

                var codeGenerator = new Macross.CodeGenerator();
                // 生成Collection文件
                await codeGenerator.GenerateAndSaveMacrossCollector(EngineNS.ECSType.Client);
                // 收集所有Macross文件，放入游戏共享工程中
                List<string> macrossfiles = codeGenerator.CollectionMacrossProjectFiles(EngineNS.ECSType.Client);
                codeGenerator.GenerateMacrossProject(macrossfiles.ToArray(), EngineNS.ECSType.Client);
                // 编译游戏dll
                if (await EditorCommon.Program.BuildGameDllImmediately(true) == false)
                {
                    var result = EditorCommon.MessageBox.Show("游戏工程未编译通过，查看详细信息请手动编译游戏工程!是否继续运行编辑器?", EditorCommon.MessageBox.enMessageBoxButton.YesNo);
                    if (result == EditorCommon.MessageBox.enMessageBoxResult.No)
                    {
                        CEditorEngine.EditorEngine?.Cleanup();
                        System.Environment.Exit(0);
                    }
                    else
                    {
                        // 加载旧dll
                        string lastAssemblyFileName = "";
                        var curDir = EngineNS.CEngine.Instance.FileManager.Bin;
                        var gamebuildDataFile = curDir + "GameBuilder.data";
                        if (System.IO.File.Exists(gamebuildDataFile))
                        {
                            using (var r = new System.IO.StreamReader(gamebuildDataFile))
                            {
                                lastAssemblyFileName = r.ReadLine();
                            }
                        }
                        if(!string.IsNullOrEmpty(lastAssemblyFileName))
                        {
                            await GameDllWatcher_Changed_Async_Impl(curDir + lastAssemblyFileName + ".dll");
                        }
                    }
                }

                //await GameDllWatcher_Changed_Async_Impl(gameDllDir + "Game.Windows_b6a6132d_68a7_451f_9b91_5e2d8ef17aa9.dll");

                //EditorCommon.GamePlay.Instance.GameInstanceType = typeof(Game.CGameInstance);

                await EditorCommon.PluginAssist.PluginManager.Instance.InitializePlugins(0.9, new Action<string, double>(async (info, progressChange) =>
                {
                    await UpdateProcess(EngineNS.Profiler.ELogTag.Info, info, progressChange);
                }));

                //EditorCommon.VersionControl.VersionControlManager.Instance.InitializePlugins(EditorCommon.PluginAssist.PluginManager.Instance.Plugins);
                //EditorCommon.VersionControl.VersionControlManager.Instance.ActiveVersionControlSystem("SVN");

                // 生成Metadata
                //EditorCommon.ClassMetadataGenerator.ProcessMetadata(0.1, new Action<string, double>((info, progressChange) =>
                //{
                //    EditorLoading.Instance.UpdateProcess(info, progressChange);
                //}));

                //await CompileMacross();

                await UpdateProcess(EngineNS.Profiler.ELogTag.Info, "Initializing...", 0);

                var edCAssembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, EngineNS.CEngine.Instance.FileManager.Bin + "EditorCommon.dll");
                EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(edCAssembly.Assembly);

                //await EngineNS.CEngine.Instance.EventPoster.Post(EngineNS.Thread.EAsyncContinueType.Sync, () => { return true; });

                //await EditorCommon.SnapshotProcess.SnapshotCreator.Instance.InitD3DEnviroment();
                //DockControl.DockManager.Instance.LayoutConfigFileName = mConfigFileName;
                //LoadLayoutConfig(mConfigFileName);

                // 处理文件删除列表
                //Program.LoadDeleteFileList();
                //Program.DelFileInFileList();

                //UpdateArrangementConfigs();

                //FinishInitialize();

                // 重新读取一遍Metadata，以便编辑器中对象可以正常自动存取
                //CSUtility.Support.ClassInfoManager.Instance.Load(CSUtility.Program.FinalRelease);

                Program.MainWinInstance = new MainWindow();
                Program.MainWinInstance.Show();
                Program.MainWinInstance.IsEnabled = false;

                // 创建引用关系表
                var noUse = EngineNS.CEngine.Instance.GameEditorInstance.InitializeResourceReferenceDictionary();

                mInitializeFinished = true;
                //this.Close();

                EngineNS.McEngine.mOpenEditor = CoreEditor.WorldEditor.SceneResourceInfo.OpenEditorByRName;
                //CEditorEngine.Instance.McEngineGetter?.Get()?.OnEditorStarted(CEditorEngine.Instance);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        //async Task CompileMacross()
        //{
        //    var codeGenerator = new Macross.CodeGenerator();
        //    // 编译Macross
        //    // 收集所有的macross代码
        //    var refAssemblys = new string[]
        //    {
        //        "System.dll",
        //        EngineNS.CEngine.Instance.FileManager.Bin + "CoreClient.Windows.dll",
        //        EngineNS.CEngine.Instance.FileManager.Root + EditorCommon.GameProjectConfig.Instance.AbsGameDllFileName + ".dll",
        //    };
        //    var codeFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.Content, $"*_{EngineNS.ECSType.Client.ToString()}.cs", System.IO.SearchOption.AllDirectories);
        //    var newDll = EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript_new.dll";
        //    var result = codeGenerator.CompileCode(newDll, codeFiles, refAssemblys.ToArray(), "/define:MacrossDebug", true);
        //    if (result.Errors.HasErrors)
        //    {
        //        EngineNS.CEngine.Instance.FileManager.DeleteFile(newDll);
        //    }
        //    else
        //    {
        //        var tagFile = EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll";
        //        EngineNS.CEngine.Instance.FileManager.CopyFile(newDll, tagFile, true);
        //        EngineNS.CEngine.Instance.FileManager.DeleteFile(newDll);
        //        EngineNS.CEngine.Instance.FileManager.CopyFile(newDll.Replace(".dll", ".vpdb"), tagFile.Replace(".dll", ".vpdb"), true);
        //        EngineNS.CEngine.Instance.FileManager.DeleteFile(newDll.Replace(".dll", ".vpdb"));
        //    }
        //    // MacrossCollection
        //    var newCDll = EngineNS.CEngine.Instance.FileManager.Bin + "MacrossCollector_new.dll";
        //    var collectionRefAssemblys = new string[]
        //    {
        //        "System.dll",
        //        EngineNS.CEngine.Instance.FileManager.Bin + "CoreClient.Windows.dll",
        //        EngineNS.CEngine.Instance.FileManager.Root + EditorCommon.GameProjectConfig.Instance.AbsGameDllFileName + ".dll",
        //    };
        //    result = await codeGenerator.CompileMacrossCollector(EngineNS.ECSType.Client, newCDll, collectionRefAssemblys, "/define:MacrossDebug", true);
        //    if (result.Errors.HasErrors)
        //        EngineNS.CEngine.Instance.FileManager.DeleteFile(newDll);
        //    else
        //    {
        //        var tagCFile = EngineNS.CEngine.Instance.FileManager.Bin + EngineNS.Macross.MacrossDataManager.MacrossCollectorDllName;
        //        EngineNS.CEngine.Instance.FileManager.CopyFile(newCDll, tagCFile, true);
        //        EngineNS.CEngine.Instance.FileManager.DeleteFile(newCDll);
        //        EngineNS.CEngine.Instance.FileManager.CopyFile(newCDll.Replace(".dll", ".vpdb"), tagCFile.Replace(".dll", ".vpdb"), true);
        //        EngineNS.CEngine.Instance.FileManager.DeleteFile(newCDll.Replace(".dll", ".vpdb"));
        //    }
        //}

        private void CSWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            EditorCommon.Program.NeedBuildGameDll = true;//.BuildGameDll();
        }

        byte[] mGameDllHash;
        private void GameDllWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            var noUse = GameDllWatcher_Changed_Async(sender, e);
        }
        private async Task GameDllWatcher_Changed_Async(object sender, System.IO.FileSystemEventArgs e)
        {
            await GameDllWatcher_Changed_Async_Impl(e.Name);
        }
        private async Task GameDllWatcher_Changed_Async_Impl(string fullFileName)
        {
            var gameDllDir = EditorCommon.GameProjectConfig.Instance.GameDllDir;// EngineNS.CEngine.Instance.FileManager.Bin + "Batman/";
            var keyName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(fullFileName, false);
            var index = keyName.IndexOf('_');
            keyName = EditorCommon.GameProjectConfig.Instance.GameDllFileName + keyName.Substring(index);

            //var id = "_" + Guid.NewGuid().ToString().Replace("-", "_");

            try
            {
                var array = EngineNS.IO.FileManager.ReadFile(gameDllDir + keyName + ".dll");
                var algo = new System.Security.Cryptography.MD5CryptoServiceProvider();// .SHA256Managed();
                var hash = algo.ComputeHash(array);
                if (Enumerable.SequenceEqual(hash, mGameDllHash))
                    return;
                mGameDllHash = hash;

                EngineNS.CEngine.Instance.FileManager.CopyFile(gameDllDir + keyName + ".dll", EngineNS.CEngine.Instance.FileManager.Bin + keyName + ".dll", true);
                EngineNS.CEngine.Instance.FileManager.CopyFile(gameDllDir + keyName + ".pdb", EngineNS.CEngine.Instance.FileManager.Bin + keyName + ".pdb", true);
            }
            catch (System.Exception)
            {

            }

            EngineNS.Rtti.RttiHelper.Lock();
            EngineNS.Rtti.RttiHelper.UnRegisterAnalyseAssembly(EngineNS.ECSType.Client, "Game");
            EditorCommon.GamePlay.Instance.GameDllAbsFileName = EngineNS.CEngine.Instance.FileManager.Bin + keyName + ".dll";
            var assembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, EditorCommon.GamePlay.Instance.GameDllAbsFileName, "", false, false);
            EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("Game", assembly);
            EngineNS.CEngine.Instance.MetaClassManager.RefreshMetaClass(assembly);
            EngineNS.Rtti.RttiHelper.UnLock();
            //await CompileMacross();
            EngineNS.CEngine.Instance.MacrossDataManager.RefreshMacrossCollector();
        }
    }
}

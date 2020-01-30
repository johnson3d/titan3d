using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EngineNS;

namespace CoreEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : DockControl.Controls.DockAbleWindowBase
    {
        public MainWindow()
        {
            InitializeComponent();
            Program.MainWinInstance = this;
            EditorCommon.FileSystemWatcherProcess.InitializeFileSystemWatcher();

            //EditorCommon.TickInfo.Instance.AddTickInfo(VPort);

            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(Macross.ResourceInfos.MacrossResourceInfo));
            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(CoreEditor.WorldEditor.SceneResourceInfo));
        }

        //private System.Windows.Forms.Timer mTickTimer;

        bool EditorInited = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow = this;
            //CEditorEngine.Instance = new CEditorEngine();

            if(Program.LoadingWindow != null)
                Program.LoadingWindow.Owner = this;

            this.MainCtrl.PG.Instance = this;
            this.MainCtrl.VP1.ViewPortName = "MainEditorView";

            //在这里选择渲染API和是否启动调试层
            EngineNS.ERHIType rhiType = EngineNS.ERHIType.RHT_D3D11;//RHT_OGL;//
            bool bDebugLayer = false;
            if(rhiType == EngineNS.ERHIType.RHT_OGL)
            {
                bDebugLayer = true;
            }
            var editorEngine = (CEditorEngine)CEditorEngine.Instance;
            editorEngine.InitSystem(rhiType, this.MainCtrl.VP1.DrawHandle, bDebugLayer);//debug device = true

            if (true)
            {
                this.MainCtrl.VP1.OnWindowCreated = async (EditorCommon.ViewPort.ViewPortControl vp) =>
                {
                    if (EditorInited == true)
                        return;
                    EditorInited = true;

                    await editorEngine.OnMainWindowCreated(this.MainCtrl.VP1);

                    Program.LoadingWindow?.Close();
                    Program.LoadingWindow = null;
                    Program.MainWinInstance.IsEnabled = true;
                };
            }

            // Test Only /////////////////////////////////////////////
            //var folderPath = EngineNS.CEngine.Instance.FileManager.Content + "Test";
            //var folder = System.IO.Directory.CreateDirectory(folderPath);
            //for(int i=0; i<200; i++)
            //{
            //    System.IO.Directory.CreateDirectory(folderPath + "/" + i);
            //}
            //////////////////////////////////////////////////////////
            //mTickTimer = new System.Windows.Forms.Timer();
            //mTickTimer.Interval = 50;
            //mTickTimer.Enabled = true;
            //mTickTimer.Tick += new EventHandler(DoTick);

            InitGeneralMenus();

            // test only ////////////////////
            //var win = new Window_Test();
            //win.Show();
            /////////////////////////////////
        }

        //private void DoTick(object obj, EventArgs e)
        //{
        //    CEditorEngine.EditorEngine.MainTick();
        //}

        void InitGeneralMenus()
        {
            var dataBase = new EditorCommon.Menu.MenuItemData_ShowHideControl("ContentBrowser");
            dataBase.MenuNames = new string[] { "Window", "General|Content Browser", "Content Browser" };
            dataBase.Count = 4;
            dataBase.OperationControlType = typeof(EditorCommon.Controls.ResourceBrowser.BrowserControl);
            dataBase.Icons = new ImageSource[] { new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_ContentBrowser_40x.png", UriKind.Relative)) };
            EditorCommon.Menu.GeneralMenuManager.Instance.RegisterMenuItem(dataBase);

            var types = EngineNS.Rtti.RttiHelper.GetTypes();
            var tt = new List<Type>(types);
            tt.AddRange(this.GetType().Assembly.GetTypes());
            foreach(var type in tt)
            {
                foreach(var method in type.GetMethods())
                {
                    if (!method.IsStatic)
                        continue;

                    var atts = method.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MenuMethod), true);
                    if(atts != null && atts.Length > 0)
                    {
                        var att = atts[0] as EngineNS.Editor.Editor_MenuMethod;
                        var menuData = new EditorCommon.Menu.MenuItemData_Function(method.Name);
                        menuData.MenuNames = att.MenuNames;
                        menuData.Count = 1;
                        menuData.Method = method;
                        EditorCommon.Menu.GeneralMenuManager.Instance.RegisterMenuItem(menuData);
                    }
                }
                foreach(var pro in type.GetProperties())
                {
                    if (pro.GetMethod == null || pro.SetMethod == null)
                        continue;
                    if (!pro.GetMethod.IsStatic || !pro.SetMethod.IsStatic)
                        continue;
                    if (pro.PropertyType != typeof(bool))
                        continue;

                    var atts = pro.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MenuMethod), true);
                    if(atts != null && atts.Length > 0)
                    {
                        var att = atts[0] as EngineNS.Editor.Editor_MenuMethod;
                        var menuData = new EditorCommon.Menu.MenuItemData_CheckAble(pro.Name);
                        menuData.MenuNames = att.MenuNames;
                        menuData.Count = 1;
                        menuData.ProInfo = pro;
                        EditorCommon.Menu.GeneralMenuManager.Instance.RegisterMenuItem(menuData);
                    }
                }
            }

            // PVS debug
            var pvsDataBase = new EditorCommon.Menu.MenuItemData_ShowHideControl("PVSDebugger");
            pvsDataBase.MenuNames = new string[] { "Debug", "General|PVSDebugger" };
            pvsDataBase.Count = 1;
            pvsDataBase.OperationControlType = typeof(EditorCommon.Controls.Debugger.PVSDebugger);
            pvsDataBase.Icons = new ImageSource[] { new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_tab_SceneOutliner_40x.png", UriKind.Relative)) };
            EditorCommon.Menu.GeneralMenuManager.Instance.RegisterMenuItem(pvsDataBase);
        }

        private void Changed_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CEditorEngine.EditorEngine?.OnWindowsSizeChanged(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CEditorEngine.EditorEngine?.Cleanup();
            System.Environment.Exit(0);
            if(mPluginWin != null)
            {
                mPluginWin.NeedClose = true;
                mPluginWin.Close();
            }
        }

        [EngineNS.Editor.Editor_MenuMethod("Debug", "Snapshot|RefreshSnapshot")]
        public static void RefreshSnapshot()
        {
            Action action = async () =>
            {
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*.rinfo", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file, null);
                    resInfo?.GetSnapshotImage(false);
                }
                EditorCommon.MessageBox.Show("刷新缩略图操作完成");
            };
            action.Invoke();
        }
        [EngineNS.Editor.Editor_MenuMethod("Debug", "Snapshot|ForceRefreshAllSnapshot")]
        public static void ForceRefreshSnapshot()
        {
            Action action = async () =>
            {
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*.rinfo", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file, null);
                    resInfo?.GetSnapshotImage(true);
                }
                EditorCommon.MessageBox.Show("刷新所有缩略图操作完成");
            };
            action.Invoke();
        }
    }
}

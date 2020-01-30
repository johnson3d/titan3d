using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CoreEditor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //private System.Windows.Forms.Timer mTickTimer;
        //private System.Windows.Threading.DispatcherTimer mTickTimer;

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            EngineNS.CIPlatform.Instance.PlayMode = EngineNS.CIPlatform.enPlayMode.Editor;

            CEditorEngine.Instance = new CEditorEngine();
            CEditorEngine.Instance.Interval = 25;
            //EngineNS.CEngine.Instance.Desc.GameMacross = EngineNS.RName.GetRName("Macross/mygame.macross");
            CEditorEngine.Instance.PreInitEngine();
            CEditorEngine.Instance.InitEngine("Game", null);
            EngineNS.CEngine.mShowPropertyGridInWindows = EditorCommon.Program.ShowPropertyGridInWindows;
            CEditorEngine.Instance.GameEditorInstance = new CEditorInstance();

            //mTickTimer = new System.Windows.Forms.Timer();
            //mTickTimer.Interval = 50;
            //mTickTimer.Enabled = true;
            //mTickTimer.Tick += new EventHandler(DoTick);
            System.Windows.Interop.ComponentDispatcher.ThreadIdle += this.DoTick;

            //mTickTimer = new System.Windows.Threading.DispatcherTimer();
            //mTickTimer.Tick += new EventHandler(DoTick);
            //mTickTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);

            //mTickTimer.Start();
        }
        bool IsDoing = false;
        private void DoTick(object obj, EventArgs e)
        {
            if (IsDoing == false)
            {
                IsDoing = true;
                CEditorEngine.EditorEngine.MainTick();
                IsDoing = false;
            }
            
            CEditorEngine.EditorEngine.EditorTick();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            EngineNS.Profiler.Log.WriteException(e.Exception);
            MessageBox.Show("程序运行错误:" + Environment.NewLine + e.Exception.Message);
            //Shutdown(1);
            e.Handled = true;
        }

        [System.Security.SecurityCritical, System.Security.Permissions.UIPermission(System.Security.Permissions.SecurityAction.LinkDemand, Unrestricted = true)]
        public new int Run()
        {
            EngineNS.CIPlatform.Instance.WindowsRun(this.Dispatcher);
            return 0;   
        }
    }
}

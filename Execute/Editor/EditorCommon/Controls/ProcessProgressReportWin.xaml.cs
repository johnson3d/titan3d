using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace EditorCommon.Controls
{
    /// <summary>
    /// ProcessProgressReportWin.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessProgressReportWin : Window, INotifyPropertyChanged, EngineNS.Editor.IEditorInstanceObject
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        double mProgress = 0;
        public double Progress
        {
            get => mProgress;
            set
            {
                mProgress = value;
                TextBlock_Percent.Text = (int)(mProgress * 100) + "%";
                PB_Percent.Value = mProgress;
            }
        }

        string mInfo = "";
        public string Info
        {
            get => mInfo;
            set
            {
                mInfo = value;
                OnPropertyChanged("Info");
            }
        }

        public static ProcessProgressReportWin Instance
        {
            get
            {
                var name = typeof(ProcessProgressReportWin).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new ProcessProgressReportWin();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        public void FinalCleanup()
        {
            this.Close();
        }

        private ProcessProgressReportWin()
        {
            InitializeComponent();
        }

        public void ShowReportWin(bool show)
        {
            if (show)
            {
                Info = "";
                Progress = 0;
                this.Owner = DockControl.DockManager.Instance.CurrentActiveWindow;
                this.Show();

                foreach(var win in DockControl.DockManager.Instance.DockableWindows)
                {
                    win.IsEnabled = false;
                }
            }
            else
            {
                this.Hide();

                foreach (var win in DockControl.DockManager.Instance.DockableWindows)
                {
                    win.IsEnabled = true;
                }
            }
        }
    }
}

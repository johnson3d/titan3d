using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AutoBuildingEditor
{
    /// <summary>
    /// Progress.xaml 的交互逻辑
    /// </summary>
    public partial class Progress : UserControl
    {
        public class BuildInfo
        {
            public string Info
            {
                get;
                set;
            } = "";

            public System.DateTime DataTime
            {
                get;
                set;
            }

            public string Color
            {
                get;
                set;
            }
        }

        public delegate void DelegateCancle();
        public event DelegateCancle CancleEvent;
        public double Value
        {
            get
            {
                return Progress1.Value;
            }
            set
            {
                Progress1.Value = value;
            }
        }

        ObservableCollection<BuildInfo> Infos = new ObservableCollection<BuildInfo>();
        public Progress()
        {
            InitializeComponent();
        }

        public void AddInfo(string info, double value, string color = "White")
        {

            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                if (value > 0)
                {
                    Progress1.Value = value / 100.0f;
                }
                
                Infos.Add( new BuildInfo(){ Info = info, DataTime = System.DateTime.Now, Color = color });
                if (value >= 100)
                    CancelBtn.Content = "Finish";
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }

        public bool IsCancel = false;
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelBtn.Content = "Cancel";
            IsCancel = true;
            Infos.Clear();
            CancleEvent?.Invoke();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressInfo.ItemsSource = Infos;
            //ProgressInfo.LineHeight = 1000;
            //ProgressInfo.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
           // ProgressInfo.Text = "asdasdasdasdas\nasdfwefrewrwe\nasfdasfsdfews\nasdas";
        }

        public void SaveInfoToTxt(string path)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                System.DateTime datatime = System.DateTime.UtcNow;
                string file = path + "/" + datatime.Year.ToString() + datatime.Month.ToString() + datatime.Day.ToString() +
                    datatime.Hour.ToString() + datatime.Minute.ToString() + datatime.Second.ToString() + ".txt";
                System.IO.FileStream fsWrite = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                string text = "";
                for (int i = 0; i < Infos.Count; i++)
                {
                    text += Infos[i].DataTime + " : " + Infos[i].Info + "\n";
                }
                byte[] buffer = Encoding.Default.GetBytes(text);
                fsWrite.Write(buffer, 0, buffer.Length);
                fsWrite.Close();
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
            
        }
    }
}

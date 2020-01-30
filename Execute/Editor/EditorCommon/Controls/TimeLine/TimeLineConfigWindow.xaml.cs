using System;
using System.Windows;
using System.ComponentModel;

namespace EditorCommon.Controls.TimeLine
{
    /// <summary>
    /// Interaction logic for TimeLineConfigWindow.xaml
    /// </summary>
    public partial class TimeLineConfigWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        Int64 mTotalFrame = 100;
        public Int64 TotalFrame
        {
            get { return mTotalFrame; }
            set
            {
                mTotalFrame = value;
                OnPropertyChanged("TotalFrame");
            }
        }

        Int64 mFPS = 30;
        public Int64 FPS
        {
            get { return mFPS; }
            set
            {
                mFPS = value;
                OnPropertyChanged("FPS");
            }
        }

        private TimeLineControl mTimeLineControl = null;

        public TimeLineConfigWindow(TimeLineControl ctrl)
        {
            InitializeComponent();

            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(this);

            mTimeLineControl = ctrl;
        }

        private void Button_OK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mTimeLineControl.TotalFrame = TotalFrame;
            mTimeLineControl.FPS = FPS;

            this.Hide();

            //  刷新时间轴
            mTimeLineControl.UpdateSliderItemAndFrameShow();
            mTimeLineControl.UpdateTimeLineTrackShow() ;
        }

        private void Button_Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}

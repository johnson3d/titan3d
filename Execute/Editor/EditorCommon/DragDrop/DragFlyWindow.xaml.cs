using System.Windows;
using System.ComponentModel;
using System;

namespace EditorCommon
{
    /// <summary>
    /// DragFlyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DragFlyWindow : Window, INotifyPropertyChanged
	{
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        int mCount = 0;
        public int Count
        {
            get { return mCount; }
            set
            {
                mCount = value;
                if (mCount < 2)
                {
                    CountVisibility = Visibility.Collapsed;
                }
                else
                    CountVisibility = Visibility.Visible;
                OnPropertyChanged("Count");
            }
        }

        Visibility mCountVisibility = Visibility.Collapsed;
        public Visibility CountVisibility
        {
            get { return mCountVisibility; }
            set
            {
                mCountVisibility = value;
                OnPropertyChanged("CountVisibility");
            }
        }

        string mInfoString = "";
        public string InfoString
        {
            get { return mInfoString; }
            set
            {
                mInfoString = value;

                if (string.IsNullOrEmpty(mInfoString))
                {
                    InfoVisibility = Visibility.Collapsed;
                    InfoStringBrush = System.Windows.Media.Brushes.White;
                }
                else
                {
                    InfoVisibility = Visibility.Visible;
                    InfoStringBrush = System.Windows.Media.Brushes.White;
                }

                OnPropertyChanged("InfoString");
            }
        }

        System.Windows.Media.Brush mInfoStringBrush = System.Windows.Media.Brushes.White;
        public System.Windows.Media.Brush InfoStringBrush
        {
            get => mInfoStringBrush;
            set
            {
                mInfoStringBrush = value;
                OnPropertyChanged("InfoStringBrush");
            }
        }

        Visibility mInfoVisibility = Visibility.Collapsed;
        public Visibility InfoVisibility
        {
            get { return mInfoVisibility; }
            set
            {
                mInfoVisibility = value;
                OnPropertyChanged("InfoVisibility");
            }
        }

		public DragFlyWindow()
		{
			this.InitializeComponent();
			
			// 在此点之下插入创建对象所需的代码。
		}
	}
}
using System;
using System.Windows;
using System.ComponentModel;

namespace UVAnimEditor
{
    /// <summary>
    /// Interaction logic for AutoGridSetWindow.xaml
    /// </summary>
    public partial class AutoGridSetWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        UInt16 mGridRow = 0;
        public UInt16 GridRow
        {
            get { return mGridRow; }
            set
            {
                mGridRow = value;
                OnPropertyChanged("GridRow");
            }
        }

        UInt16 mGridColumn = 0;
        public UInt16 GridColumn
        {
            get { return mGridColumn; }
            set
            {
                mGridColumn = value;
                OnPropertyChanged("GridColumn");
            }
        }

        public AutoGridSetWindow()
        {
            InitializeComponent();
        }

        private void Button_OK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

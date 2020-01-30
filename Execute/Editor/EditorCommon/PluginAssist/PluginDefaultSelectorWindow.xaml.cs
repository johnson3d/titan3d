using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EditorCommon.PluginAssist
{
    /// <summary>
    /// PluginDefaultSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PluginDefaultSelectorWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        bool mNeverShow = false;
        public bool NeverShow
        {
            get { return mNeverShow; }
            set
            {
                mNeverShow = value;

                if(mNeverShow)
                {
                    EditorCommon.MessageBox.Show("此窗口不再显示并且默认使用当前选择的工具，可以在插件管理中设置默认使用的工具");
                }

                OnPropertyChanged("NeverShow");
            }
        }

        ObservableCollection<EditorCommon.PluginAssist.PluginItem> mPluginItems = new ObservableCollection<EditorCommon.PluginAssist.PluginItem>();
        public ObservableCollection<EditorCommon.PluginAssist.PluginItem> PluginItems
        {
            get { return mPluginItems; }
            set
            {
                mPluginItems = value;
                OnPropertyChanged("PluginItems");
            }
        }

        public EditorCommon.PluginAssist.PluginItem SelectedItem
        {
            get;
            protected set;
        } = null;

        public PluginDefaultSelectorWindow()
        {
            InitializeComponent();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedItem == null)
            {
                EditorCommon.MessageBox.Show("请先选择工具");
                return;
            }

            var anim = TryFindResource("Storyboard_End") as Storyboard;

            anim.Completed += (senderObj, eArg) =>
            {
                this.Close();
            };
            anim.Begin(this);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = ListView_Plugins.SelectedItem as EditorCommon.PluginAssist.PluginItem;
        }
    }
}

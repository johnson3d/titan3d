using EnsureFile;
using System;
using System.Collections.Generic;
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
    /// GameResControl.xaml 的交互逻辑
    /// </summary>
    public partial class GameResControl : UserControl
    {
        public class BindData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string HighLightString
            {
                get { return (string)GetValue(HighLightStringProperty); }
                set { SetValue(HighLightStringProperty, value); }
            }

            //public string ClassName
            //{
            //    get { return (string)GetValue(ClassNameProperty); }
            //    set { SetValue(ClassNameProperty, value); }
            //}
            public string ShowName
            {
                get;
                set;
            } = "";

            public string DeviceID
            {
                get;
                set;
            } = "";

            public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(BindData), new FrameworkPropertyMetadata(null));
            //public static readonly DependencyProperty ClassNameProperty = DependencyProperty.Register("ClassName", typeof(string), typeof(BindData), new FrameworkPropertyMetadata(true));


            public BindData()
            {
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }

        public MainControl HostControl;
        public GameResControl()
        {
            InitializeComponent();
        }

        List<BindData> AllDevices = new List<BindData>();
        public void GetDevice()
        {
            DeviceNodes.Clear();
            AllDevices.Clear();
            string[] devices = AccessGameRes.GetDeviceName();
            string[] ids = AccessGameRes.GetDeviceID();
            int length = Math.Min(devices.Length, ids.Length);
            for (int i = 0; i < length; i++)
            {
                BindData data = new BindData();
                data.DeviceID = ids[i];
                data.ShowName = devices[i] + " " + data.DeviceID;
                DeviceNodes.Add(data);
                AllDevices.Add(data);
            }
        }

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> DeviceNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UIDevice.TreeListItemsSource = DeviceNodes;
            GetDevice();

            UIRes.TreeListItemsSource = ResNodes;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetDevice();
        }

        private void ADB_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            //ofd.Multiselect = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UIADBAdrress.Text = ofd.FileName;
                if (HostControl != null)
                {
                    HostControl.PConfig.ADBAddress = UIADBAdrress.Text;
                    AccessGameRes.InitADBAddress(HostControl.PConfig.ADBAddress);
                }
            }
        }

        private void ADB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (HostControl != null)
            {
                HostControl.PConfig.ADBAddress = UIADBAdrress.Text;
                AccessGameRes.InitADBAddress(HostControl.PConfig.ADBAddress);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = UIName.Text.ToString();
            DeviceNodes.Clear();
            foreach (var i in AllDevices)
            {
                if (text.Equals("") || i.ShowName.ToString().Equals(text))
                {
                    DeviceNodes.Add(i);
                }
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UIDevice.Visibility = Visibility.Visible;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UIDevice.Visibility = Visibility.Collapsed;
        }


        private void TreeView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UIDevice.Visibility = Visibility.Collapsed;
            if (UIDevice.SelectedItem == null)
                return;

            EditorCommon.TreeListView.TreeNode node = UIDevice.SelectedItem as EditorCommon.TreeListView.TreeNode;
            if (node == null)
                return;

            BindData data = node.Tag as BindData;
            if (data == null)
                return;

            UIName.Text = data.ShowName;


        }

        private void CheckRes_Click(object sender, RoutedEventArgs e)
        {
            if (HostControl == null)
                return;

            HostControl.CreateNewAndroidInfos();

            HostControl.UnbundProgress1();
            HostControl.UIGameResPanel.Children.Clear();
            HostControl.UIGameResPanel.Children.Add(HostControl.progress);
        }

        private void ClearRes_Click(object sender, RoutedEventArgs e)
        {
            ResNodes.Clear();
        }

        public void UnbundProgress()
        {
            HostControl.UIGameResPanel.Children.Clear();
            HostControl.UIGameResPanel.Children.Add(this);
        }

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> ResNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        public void FinishCreateNewAndroidInfos(string[] files)
        {
            ResNodes.Clear();
            foreach (var i in files)
            {
                BindData data = new BindData();
                data.ShowName = i;
                ResNodes.Add(data);
            }
        }
    }
}

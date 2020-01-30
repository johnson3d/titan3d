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

using EngineNS.Editor;
using System.ComponentModel;

namespace AutoBuildingEditor
{
    /// <summary>
    /// AndroidConfig.xaml 的交互逻辑
    /// </summary>
    public partial class AndroidConfig : UserControl
    {
        public class LoadKeyStoreConfig : INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            string mKeyStoreAddress = "";
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public string KeyStoreAddress
            {
                get
                {
                    return mKeyStoreAddress;
                }
                set
                {
                    mKeyStoreAddress = value;
                    OnPropertyChanged("KeyStoreAddress");
                }
            
            }

            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public string KeyStorePassWord
            {
                get;
                set;
            }

            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public string Alias
            {
                get;
                set;
            }

            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public string AliasPassWord
            {
                get;
                set;
            }
        }

        public class CreateKeyStoreConfig
        {
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [DisplayName("单位")]
            public string Department
            {
                get;
                set;
            }

            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [DisplayName("组织名称")]
            public string Organisation
            {
                get;
                set;
            }

            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [DisplayName("城市")]
            public string CityOfLocality
            {
                get;
                set;
            }

            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [DisplayName("省（州）")]
            public string StateOrProvince
            {
                get;
                set;
            }

            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [DisplayName("国家")]
            public string CountryCode
            {
                get;
                set;
            }
        }

        public LoadKeyStoreConfig LoadConfig = new LoadKeyStoreConfig();
        public CreateKeyStoreConfig CreateConfig = new CreateKeyStoreConfig();
        public AndroidConfig()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadKeyStore.Instance = LoadConfig;
            CreateKeyStore.Instance = CreateConfig;
        }

        private void LoadKeyStore_Btn(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "|*.keystore";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadConfig.KeyStoreAddress = ofd.FileName;
            }
        }

        private void CreateKeyStore_Btn(object sender, RoutedEventArgs e)
        {
            LoadKeyStore.Instance = LoadConfig;
            CreateKeyStore.Instance = CreateConfig;
        }
    }
}

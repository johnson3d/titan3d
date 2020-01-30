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
using EditorCommon.Resources;
using InputWindow;

namespace CoreEditor.WorldEditor
{
    /// <summary>
    /// CreateScene.xaml 的交互逻辑
    /// </summary>
    public partial class CreateScene : ResourceLibrary.WindowBase, EditorCommon.Resources.ICustomCreateDialog
    {
        public EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }
        public string ResourceName
        {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }

        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register("ResourceName", typeof(string), typeof(CreateScene), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnResourceNameChanged)));
        public static void OnResourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = d as CreateScene;
            win.mCreateData.ResourceName = (string)e.NewValue;
        }

        public IResourceCreateData GetCreateData()
        {
            return mCreateData;
        }

        public bool? ShowDialog(Delegate_ValidateCheck onCheck)
        {
            try
            {
                if (TextBox_Name != null)
                {
                    var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
                    if (bindExp != null)
                    {
                        if (bindExp.ParentBinding.ValidationRules.Count > 0)
                        {
                            var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                            if (rule != null)
                                rule.OnValidateCheck = onCheck;
                        }
                    }
                }

                return this.ShowDialog();
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Create Resource Exception", ex.ToString());
            }
            return false;
        }

        SceneResourceInfo.SceneResourceInfoCreateData mCreateData = new SceneResourceInfo.SceneResourceInfoCreateData();
        public CreateScene()
        {
            InitializeComponent();
        }

        class MapData
        {
            public EngineNS.RName MapRName;
            public override string ToString()
            {
                return MapRName.PureName();
            }
        }
        List<MapData> mMapDatas = new List<MapData>();

        void InitMaps()
        {
            mMapDatas.Clear();
            var mapFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent + "editor/map/";
            foreach(var mapDir in EngineNS.CEngine.Instance.FileManager.GetDirectories(mapFolder, "*.map", System.IO.SearchOption.TopDirectoryOnly))
            {
                var rInfoFile = mapDir + EditorCommon.Program.ResourceInfoExt;
                if (!EngineNS.CEngine.Instance.FileManager.FileExists(rInfoFile))
                    continue;

                var rName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(mapDir);
                mMapDatas.Add(new MapData()
                {
                    MapRName = rName
                });
            }
            ListBox_Maps.ItemsSource = mMapDatas;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitMaps();

            var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
            if (bindExp != null)
            {
                if (bindExp.ParentBinding.ValidationRules.Count > 0)
                {
                    var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                    if (rule != null)
                    {
                        rule.OnValidateCheck = (object value, System.Globalization.CultureInfo cultureInfo) =>
                        {
                            var val = (string)value;
                            if (string.IsNullOrEmpty(val))
                                return new ValidationResult(false, "名称不能为空!");
                            if (EditorCommon.Program.IsValidRName(val) == false)
                                return new ValidationResult(false, "名称不合法!");

                            return new ValidationResult(true, null);
                        };
                    }
                }
            }

            ListBox_Maps.SelectedIndex = 0;
        }

        private void ListBox_Maps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var noUse = ShowMapInfo();
        }
        async Task ShowMapInfo()
        {
            var item = ListBox_Maps.SelectedItem as MapData;
            var imgs = await EditorCommon.ImageInit.GetImage(item.MapRName.Address + EditorCommon.Program.SnapshotExt);
            if (imgs != null && imgs.Length > 0)
            {
                Image_Show.Source = imgs[0];
            }
            var rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(item.MapRName.Address + EditorCommon.Program.ResourceInfoExt, null) as SceneResourceInfo;
            if(rInfo != null)
            {
                TextBlock_Desc.Text = rInfo.Description;
            }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Name != null)
            {
                if (Validation.GetHasError(TextBox_Name))
                    return;

                var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
                if (bindExp != null)
                {
                    if (bindExp.ParentBinding.ValidationRules.Count > 0)
                    {
                        var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                        if (rule != null)
                        {
                            rule.OnValidateCheck = null;
                        }
                    }
                }

                var bindingExpression = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                    bindingExpression.UpdateSource();
            }

            if (ListBox_Maps.SelectedIndex < 0)
            {
                EditorCommon.MessageBox.Show("请先选择一张地图再进行创建");
                return;
            }
            var item = ListBox_Maps.SelectedItem as MapData;
            mCreateData.SrcSceneName = item.MapRName;
            DialogResult = true;
            this.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

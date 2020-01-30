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
using System.Windows.Shapes;
using EditorCommon.Resources;
using InputWindow;
using EditorCommon;
using EngineNS.Bricks.ExcelTable;
using System.Reflection;
using System.ComponentModel;

namespace ExcelViewEditor
{
    /// <summary>
    /// CreateExcelFromMacross.xaml 的交互逻辑
    /// </summary>
    public partial class CreateDataSetter : ResourceLibrary.WindowBase, INotifyPropertyChanged, EditorCommon.Resources.ICustomCreateDialog
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region binddata
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
            public string ClassName
            {
                get;
                set;
            }

            public EngineNS.RName MacrossName
            {
                get;
                set;
            }

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
        #endregion
        #region interface
        public string ResourceName
        {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }

        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register("ResourceName", typeof(string), typeof(CreateDataSetter), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnResourceNameChanged)));
        public static void OnResourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = d as CreateDataSetter;
            win.mCreateData.ResourceName = (string)e.NewValue;
        }

        ExcelResourceInfo.ExcelCreateData mCreateData = new ExcelResourceInfo.ExcelCreateData();
        public IResourceCreateData GetCreateData()
        {
            return mCreateData;
        }

        public bool? ShowDialog(Delegate_ValidateCheck onCheck)
        {
            //try
            //{
            //    if (TextBox_Name != null)
            //    {
            //        var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
            //        if (bindExp != null)
            //        {
            //            if (bindExp.ParentBinding.ValidationRules.Count > 0)
            //            {
            //                var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
            //                if (rule != null)
            //                    rule.OnValidateCheck = onCheck;
            //            }
            //        }
            //    }

            //    return this.ShowDialog();
            //}
            //catch (System.Exception ex)
            //{
            //    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Create Resource Exception", ex.ToString());
            //}
            //return false;

            return this.ShowDialog();
        }
        public EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }
        #endregion

        public CreateDataSetter()
        {
            InitializeComponent();
        }

        ResourceInfo mExcelResourceInfo = null;
        public ResourceInfo HostResourceInfo
        {
            set
            {
                mExcelResourceInfo = value;
            }
        }

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> TreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        public void GetAllMacross()
        {
            TreeViewItemsNodes.Clear();
            var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;// EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Common, EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll", "", true);
            Type[] types = assembly.GetTypes();
            Type MacrossType = typeof(EngineNS.Macross.IMacrossType);
            foreach (var i in types)
            {
                if (i.GetInterface(MacrossType.FullName) == null)
                    continue;

                if (i.IsClass && i.IsPublic && i.FullName.ToLower().IndexOf(mSearchText) != -1)
                {
                    BindData data = new BindData();
                    data.ClassName = i.FullName;
                    data.MacrossName = EngineNS.Macross.MacrossFactory.Instance.GetMacrossRName(i);

                    //BindingOperations.SetBinding(data, BindData.ClassNameProperty, new Binding("FullName") { Source = i });
                    //BindingOperations.SetBinding(actorData, ActorData.HighLightStringProperty, new Binding("FilterString") { Source = this });

                    TreeViewItemsNodes.Add(data);
                }

            }
        }

        #region event
        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UIMacrossTrees.TreeListItemsSource = TreeViewItemsNodes;
            GetAllMacross();
        }
        private void Button_SaveXls(object sender, System.Windows.RoutedEventArgs e)
        {
            //rinfo为空后不可用
            if (mExcelResourceInfo == null)
                return;

            if (UIMacrossTrees.SelectedItem == null)
                return;

            Type type = UIMacrossTrees.SelectedItem.GetType();
            EditorCommon.TreeListView.TreeNode TreeModel = UIMacrossTrees.SelectedItem as EditorCommon.TreeListView.TreeNode;
            BindData data = TreeModel.Tag as BindData;
            //ExcelExporter export = new ExcelExporter();
            //export.CreateHeaderForSheet(0, data._Type);
            //export.Save(mExcelResourceInfo.ResourceName.Address);
            mCreateData.MacrossName = data.MacrossName;
            mCreateData.ExcelName = UIName.Text;
            DialogResult = true;
            //PropertyInfo[] propertys = data._Type.GetProperties();
            //foreach (var i in propertys)
            //{
            //    export.a
            //}

        }

        string mSearchText = "";
        public string SearchText
        {
            get
            {
                return mSearchText;
            }
            set
            {
                mSearchText = value.ToLower();
                GetAllMacross();
                OnPropertyChanged("SearchText");
            }
        }
        #endregion

    }
}

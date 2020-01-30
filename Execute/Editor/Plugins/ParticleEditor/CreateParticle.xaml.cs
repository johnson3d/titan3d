using EditorCommon.Resources;
using ResourceLibrary;
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
using System.Windows.Shapes;

namespace ParticleEditor
{
    public class CreateData : DependencyObject
    {
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(CreateData), new UIPropertyMetadata(""));
        public string HightLightString
        {
            get { return (string)GetValue(HightLightStringProperty); }
            set { SetValue(HightLightStringProperty, value); }
        }
        public static readonly DependencyProperty HightLightStringProperty = DependencyProperty.Register("HightLightString", typeof(string), typeof(CreateData), new UIPropertyMetadata(""));
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(CreateData), new UIPropertyMetadata(false));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CreateData), new UIPropertyMetadata(null));

        public Type ClassType;
        public EngineNS.ECSType CSType;
        public bool IsMacrossType;
        
        public void CopyTo(CreateData data)
        {
            data.Name = Name;
            data.Icon = Icon;
            data.ClassType = ClassType;
            data.CSType = CSType;
            data.IsMacrossType = IsMacrossType;
        }
    }

    /// <summary>
    /// CreateParticle.xaml 的交互逻辑
    /// </summary>
    public partial class CreateParticle : WindowBase, INotifyPropertyChanged, EditorCommon.Resources.ICustomCreateDialog
    {

       public class BindData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string FileName
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public string FullName
            {
                get;
                set;
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }
        public string ResourceName
        {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }

        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register("ResourceName", typeof(string), typeof(CreateParticle), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnResourceNameChanged)));
        public static void OnResourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = d as CreateParticle;
            win.mCreateData.ResourceName = (string)e.NewValue;
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(CreateParticle), new FrameworkPropertyMetadata(""));

        //public static readonly DependencyProperty FilterStringProperty = DependencyProperty.Register("FilterString", typeof(string), typeof(CreateMacrossWindow), new UIPropertyMetadata("", OnFilterStringPropertyChanged));
        //private static void OnFilterStringPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var win = sender as CreateParticle;
        //    win.ShowItemWithFilter();
        //}

        //string mResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Particle;

        Macross.ResourceInfos.MacrossResourceInfo mMacrossResourceInfo = null;
        public Macross.ResourceInfos.MacrossResourceInfo HostResourceInfo
        {
            set
            {
                mMacrossResourceInfo = value;
                //var type = mMacrossResourceInfo.GetType();
                //var att = Attribute.GetCustomAttribute(type, typeof(ResourceInfoAttribute)) as EditorCommon.Resources.ResourceInfoAttribute;
                //mResourceInfoType = att.ResourceInfoType;
                //if (mResourceInfoType == EngineNS.Editor.Editor_RNameTypeAttribute.AnimationMacross)
                //{
                //}
            }
        }

        public CreateParticle()
        {
            InitializeComponent();
        }

        ParticleResourceInfo.ParticleCreateData mCreateData = new ParticleResourceInfo.ParticleCreateData();
        public IResourceCreateData GetCreateData()
        {
            mCreateData.Description = Description;
            //mCreateData.ResourceName = 
            return mCreateData;
        }

        public bool? ShowDialog(InputWindow.Delegate_ValidateCheck onCheck)
        {
            try
            {
                mCreateData = new ParticleResourceInfo.ParticleCreateData();
                return this.ShowDialog();
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Create Resource Exception", ex.ToString());
            }
            return false;
        }

        string mFilterString = "";
        public string FilterString
        {
            get
            {
                return mFilterString;
            }
            set
            {
                mFilterString = value;
                RefreshParticleTemplates();
                OnPropertyChanged("FilterString");
            }
        }
        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> TreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        public void RefreshParticleTemplates()
        {
            TreeViewItemsNodes.Clear();
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*.macross.rinfo", System.IO.SearchOption.AllDirectories);
            if (files == null || files.Count == 0)
                return;

            for (int i = 0; i < files.Count; i++)
            {

                byte[] bytes = EngineNS.IO.FileManager.ReadFile(files[i]);
                string str = System.Text.Encoding.UTF8.GetString(bytes);
                if (str.IndexOf("ParticleEditor.ParticleResourceInfo") != -1)
                {
                    BindData data = new BindData();
                    data.Description = "Particle Template";
                    data.FullName = files[i].Replace("\\", "/");
                    data.FileName = data.FullName.Replace(EngineNS.CEngine.Instance.FileManager.ProjectContent, "").Replace(".rinfo", "");
                    if (string.IsNullOrEmpty(mFilterString) || data.FileName.ToLower().IndexOf(mFilterString.ToLower()) != -1)
                    {
                        TreeViewItemsNodes.Add(data);
                    }
                }
            }

        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            UIList.TreeListItemsSource = TreeViewItemsNodes;

            RefreshParticleTemplates();
        }

        private void UIList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UIList.SelectedItem == null)
                return;

            EditorCommon.TreeListView.TreeNode treenode = UIList.SelectedItem as EditorCommon.TreeListView.TreeNode;
            BindData data = treenode.Tag as BindData;
            if (data == null)
            {
                return;
            }

            UITemplate.Text = data.FileName;

        }

        private void UICancel_Click(object sender, RoutedEventArgs e)
        {

            DialogResult = false;
            this.Close();
        }

        private void UISelect_Click(object sender, RoutedEventArgs e)
        {
            mCreateData.ResourceName = UIName.Text;
            if (UIList.SelectedItem == null)
            {
                DialogResult = true;
                mCreateData.ClassType = typeof(EngineNS.Bricks.Particle.McParticleEffector);
                mMacrossResourceInfo.BaseTypeSaveName = mCreateData.ClassType.FullName;
                //HostResourceInfo.
                this.Close();
                return;
            }

            EditorCommon.TreeListView.TreeNode treenode = UIList.SelectedItem as EditorCommon.TreeListView.TreeNode;
            BindData data = treenode.Tag as BindData;
            if (data == null)
            {

                DialogResult = true;
                this.Close();
                return;
            }
            mCreateData.TemplateName = data.FileName;

            DialogResult = true;
            this.Close();
        }
    }
}

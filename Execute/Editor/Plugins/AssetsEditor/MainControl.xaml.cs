using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
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
using SearchBox;

namespace AssetsEditor
{
    public class AssetsItemView : SearchBox.IFilterTreeItem<AssetsItemView>, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        string mName = null;
        public string Name
        {
            get => mName;
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }
        EngineNS.RName mRName = EngineNS.RName.EmptyName;
        public EngineNS.RName RName
        {
            get => mRName;
            set
            {
                mRName = value;
                OnPropertyChanged("RName");
            }
        }
        string mHighLightString = null;
        public string HighLightString
        {
            get => mHighLightString;
            set
            {
                mHighLightString = value;
                OnPropertyChanged("HighLightString");
            }
        }
        bool mIsExpanded = false;
        public bool IsExpanded
        {
            get => mIsExpanded;
            set
            {
                mIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        bool mIsSelected = false;
        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        ImageSource mIcon = null;
        public ImageSource Icon
        {
            get => mIcon;
            set
            {
                mIcon = value;
                OnPropertyChanged("Icon");
            }
        }
        public string Path;

        ObservableCollection<AssetsItemView> mChildren = new ObservableCollection<AssetsItemView>();
        public ObservableCollection<AssetsItemView> Children
        {
            get => mChildren;
            set
            {
                mChildren = value;
            }
        }

        public AssetsItemView Parent { get; set; }

        public EditorCommon.Resources.ResourceInfo AssetsResInfo;
        public AssetsItemView()
        {

        }
        public AssetsItemView(string name)
        {
            Name = name;
        }
        public AssetsItemView(EngineNS.RName rName)
        {
            RName = rName;
            //BindingOperations.SetBinding(this, NameProperty, new Binding("RName") { Source = this, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
            Name = RName.PureName();
        }

        public void CopyTo(AssetsItemView item)
        {
            item.AssetsResInfo = AssetsResInfo;
            item.Name = Name;
            item.RName = RName;
            item.Icon = Icon;

            item.IsExpanded = IsExpanded;
        }
        public bool ContainsFilterString(string filterString)
        {
            return (RName.PureName().IndexOf(filterString, StringComparison.OrdinalIgnoreCase) > -1);
        }
        public override int GetHashCode()
        {
            if (RName != null)
                return RName.GetHashCode();
            if (string.IsNullOrEmpty(Path))
                return Path.GetHashCode();
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var item = obj as AssetsItemView;
            if (item == null)
                return false;
            return Equals(item);
        }
        public bool Equals(AssetsItemView item)
        {
            if (RName != null && RName.Equals(item.RName))
                return true;
            if (Path == item.Path)
                return true;

            return false;                
        }
    }

    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "AssetsEditor")]
    [EditorCommon.PluginAssist.PluginMenuItem(EditorCommon.Menu.MenuItemDataBase.enMenuItemType.OneClick, new string[] { "Window", "General|资源管理" })]
    [Guid("0C3D7DEE-1BDB-4652-A5BA-192DE298049A")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MainControl : UserControl, EditorCommon.PluginAssist.IEditorPlugin, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        #region IEditorPlugin
        public string PluginName => "资源管理器";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MainControl), new FrameworkPropertyMetadata(null));
        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/WidgetBlueprint_64x.png", UriKind.Absolute));
        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MainControl), new FrameworkPropertyMetadata(null));
        public UIElement InstructionControl => new System.Windows.Controls.TextBlock()
        {
            Text = PluginName,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public bool? CanClose()
        {
            return true;
        }
        public void Closed()
        {
        }

        public void StartDrag()
        {
        }
        public void EndDrag()
        {
        }

        public void SaveElement(global::EngineNS.IO.XmlNode node, global::EngineNS.IO.XmlHolder holder)
        {
        }
        public DockControl.IDockAbleControl LoadElement(global::EngineNS.IO.XmlNode node)
        {
            return null;
        }

        public bool OnActive()
        {
            return true;
        }

        public bool OnDeactive()
        {
            return true;
        }

        public async Task SetObjectToEdit(EditorCommon.Resources.ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }

        #endregion

        public double ProgressPercent
        {
            get { return (double)GetValue(ProgressPercentProperty); }
            set { SetValue(ProgressPercentProperty, value); }
        }
        public static readonly DependencyProperty ProgressPercentProperty = DependencyProperty.Register("ProgressPercent", typeof(double), typeof(MainControl), new UIPropertyMetadata(0.0));
        public Visibility ProgressVisible
        {
            get { return (Visibility)GetValue(ProgressVisibleProperty); }
            set { SetValue(ProgressVisibleProperty, value); }
        }
        public static readonly DependencyProperty ProgressVisibleProperty = DependencyProperty.Register("ProgressVisible", typeof(Visibility), typeof(MainControl), new UIPropertyMetadata(Visibility.Collapsed));
        public string ProgressInfo
        {
            get { return (string)GetValue(ProgressInfoProperty); }
            set { SetValue(ProgressInfoProperty, value); }
        }
        public static readonly DependencyProperty ProgressInfoProperty = DependencyProperty.Register("ProgressInfo", typeof(string), typeof(MainControl), new UIPropertyMetadata(""));

        public string Information
        {
            get { return (string)GetValue(InformationProperty); }
            set { SetValue(InformationProperty, value); }
        }
        public static readonly DependencyProperty InformationProperty = DependencyProperty.Register("Information", typeof(string), typeof(MainControl), new UIPropertyMetadata(""));


        public string FilterString
        {
            get { return (string)GetValue(FilterStringProperty); }
            set { SetValue(FilterStringProperty, value); }
        }

        public static readonly DependencyProperty FilterStringProperty = DependencyProperty.Register("FilterString", typeof(string), typeof(MainControl), new UIPropertyMetadata("", OnFilterStringPropertyChanged));
        private static void OnFilterStringPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var win = sender as MainControl;
            win.ShowItemWithFilter();
        }
        ObservableCollection<AssetsItemView> mAssetsItems = new ObservableCollection<AssetsItemView>();
        ObservableCollection<AssetsItemView> mAssetsItems_Show = new ObservableCollection<AssetsItemView>();
        void ShowItemWithFilter()
        {
            ListBox_Items.ItemsSource = null;
            SearchBox.SearchBox.FilterTreeItems<AssetsItemView>(null, mAssetsItems, ref mAssetsItems_Show, FilterString);
            ListBox_Items.ItemsSource = mAssetsItems_Show;

            UpdateInformation();
        }

        ObservableCollection<AssetsItemView> mSelectedItemViews = new ObservableCollection<AssetsItemView>();
        public ObservableCollection<AssetsItemView> SelectedItemViews
        {
            get => mSelectedItemViews;
            set
            {
                mSelectedItemViews = value;
                OnPropertyChanged("SelectedItemViews");
            }
        }

        public MainControl()
        {
            InitializeComponent();

            mDropAdorner = new EditorCommon.DragDrop.DropAdorner(ListBox_Items);
            SelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }

        private void SelectedItemViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateInformation();
        }
        void UpdateInformation()
        {
            int mAssetsCount = 0;
            UpdateCount(mAssetsItems_Show, ref mAssetsCount);

            if(SelectedItemViews.Count > 0)
            {
                int folderCount = 0, assetsCount = 0;
                foreach(var item in SelectedItemViews)
                {
                    if (item.AssetsResInfo == null)
                        folderCount++;
                    else
                        assetsCount++;
                }
                Information = $"共{mAssetsCount}个资源，选中{folderCount}个文件夹，{assetsCount}个资源";
            }
            else
            {
                Information = $"共{mAssetsCount}个资源";
            }
        }
        void UpdateCount(ObservableCollection<AssetsItemView> itemViews, ref int count)
        {
            foreach(var item in itemViews)
            {
                if (item.AssetsResInfo != null)
                    count++;

                UpdateCount(item.Children, ref count);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EditorCommon.Menu.GeneralMenuManager.Instance.GenerateGeneralMenuItems(Menu_Main);
        }

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            var noUse = RefreshAssets();
        }
        async Task RefreshAssets()
        {
            ProgressVisible = Visibility.Visible;
            int count = 0;
            UpdateCount(mAssetsItems, ref count);
            List<EditorCommon.Resources.ResourceInfo> rInfos = new List<EditorCommon.Resources.ResourceInfo>(count);
            CollectionResourceInfos(mAssetsItems, ref rInfos);

            var packager = new EditorCommon.Assets.AssetsPakage();
            await packager.RefreshAssets(rInfos, (progress, info) =>
            {
                ProgressPercent = progress;
                ProgressInfo = info;
            });
            ProgressVisible = Visibility.Collapsed;
        }

        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            var noUse = PackAssets();
        }
        async Task PackAssets()
        {
            var sfDialog = new System.Windows.Forms.SaveFileDialog();
            sfDialog.Filter = $"package files (*.{EditorCommon.Program.PackageExtension})|*.{EditorCommon.Program.PackageExtension}";
            sfDialog.RestoreDirectory = true;
            if (sfDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProgressVisible = Visibility.Visible;
                try
                {
                    var packager = new EditorCommon.Assets.AssetsPakage();

                    int count = 0;
                    UpdateCount(mAssetsItems, ref count);
                    List<EditorCommon.Resources.ResourceInfo> rInfos = new List<EditorCommon.Resources.ResourceInfo>(count);
                    CollectionResourceInfos(mAssetsItems, ref rInfos);
                    await packager.PackAssets(rInfos, sfDialog.FileName, (progress, info) =>
                    {
                        ProgressPercent = progress;
                        ProgressInfo = info;
                    });
                }
                catch(System.Exception e)
                {
                    EngineNS.Profiler.Log.WriteException(e);
                }
                ProgressVisible = Visibility.Collapsed;
                EditorCommon.MessageBox.Show("导出完成");
            }
        }
        void CollectionResourceInfos(ObservableCollection<AssetsItemView> items, ref List<EditorCommon.Resources.ResourceInfo> rInfos)
        {
            foreach(var item in items)
            {
                if (item.AssetsResInfo != null)
                    rInfos.Add(item.AssetsResInfo);

                CollectionResourceInfos(item.Children, ref rInfos);
            }
        }

        private bool CheckDropAvailable(DragEventArgs e)
        {
            if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
                return true;
            return false;
        }

        EditorCommon.DragDrop.DropAdorner mDropAdorner;
        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(ListBox_Items);
            mDropAdorner.IsAllowDrop = CheckDropAvailable(e);
        }

        private void ListBox_DragLeave(object sender, DragEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(ListBox_Items);
            layer.Remove(mDropAdorner);
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {

        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var noUse = ListBox_DropAsync(sender, e);
        }
        private async Task ListBox_DropAsync(object sender, DragEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(ListBox_Items);
            layer.Remove(mDropAdorner);

            var formats = e.Data.GetFormats();
            if (formats == null || formats.Length == 0)
                return;

            var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
            if (datas == null)
                return;

            ProgressVisible = Visibility.Visible;
            ProgressPercent = 0.0;

            if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                int i = 0;
                foreach(var data in datas)
                {
                    var resInfo = data as EditorCommon.Resources.ResourceInfo;
                    ProgressInfo = "正在处理 " + resInfo.ResourceName.Name;
                    var item = new AssetsItemView(resInfo.ResourceName);
                    item.Icon = resInfo.ResourceIcon;
                    item.AssetsResInfo = resInfo;

                    item.Path = resInfo.ResourceName.Name;
                    switch(resInfo.ResourceName.RNameType)
                    {
                        case EngineNS.RName.enRNameType.Game:
                            item.Path = "Content/" + item.Path;
                            break;
                        case EngineNS.RName.enRNameType.Editor:
                            item.Path = "EditorContent/" + item.Path;
                            break;
                        case EngineNS.RName.enRNameType.Engine:
                            item.Path = "EngineContent/" + item.Path;
                            break;
                    }
                    var path = new List<string>(item.Path.Split('/'));
                    await AddAssetsItem(path, item, null, mAssetsItems);

                    i++;
                    ProgressPercent = i * 1.0 / datas.Length;
                }
            }
            else if(EditorCommon.Controls.ResourceBrowser.BrowserControl.FolderDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                int count = 0;
                foreach(var data in datas)
                {
                    var folderView = data as EditorCommon.Controls.ResourceBrowser.BrowserControl.FolderView;
                    var files = EngineNS.CEngine.Instance.FileManager.GetFiles(folderView.FolderData.AbsFolder, "*.rinfo", System.IO.SearchOption.AllDirectories);
                    count += files.Count;
                }

                int i = 0;
                foreach (var data in datas)
                {
                    var folderView = data as EditorCommon.Controls.ResourceBrowser.BrowserControl.FolderView;
                    var files = EngineNS.CEngine.Instance.FileManager.GetFiles(folderView.FolderData.AbsFolder, "*.rinfo", System.IO.SearchOption.AllDirectories);
                    foreach(var file in files)
                    {
                        var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file, null);

                        ProgressInfo = "正在处理 " + resInfo.ResourceName.Name;

                        var item = new AssetsItemView(resInfo.ResourceName);
                        item.Icon = resInfo.ResourceIcon;
                        item.AssetsResInfo = resInfo;

                        item.Path = resInfo.ResourceName.Name;
                        switch(resInfo.ResourceName.RNameType)
                        {
                            case EngineNS.RName.enRNameType.Game:
                                item.Path = "Content/" + item.Path;
                                break;
                            case EngineNS.RName.enRNameType.Editor:
                                item.Path = "EditorContent/" + item.Path;
                                break;
                            case EngineNS.RName.enRNameType.Engine:
                                item.Path = "EngineContent/" + item.Path;
                                break;
                        }
                        var path = new List<string>(item.Path.Split('/'));
                        await AddAssetsItem(path, item, null, mAssetsItems);

                        i++;
                        ProgressPercent = i * 1.0 / count;
                    }
                }
            }

            ShowItemWithFilter();
            ProgressPercent = 1.0;
            ProgressVisible = Visibility.Collapsed;
        }
        async Task<bool> AddAssetsItem(List<string> path, AssetsItemView itemView, AssetsItemView parent, ObservableCollection<AssetsItemView> items)
        {
            if (path.Count == 0)
                return false;

            var curPathName = path[0];
            path.RemoveAt(0);
            foreach(var item in items)
            {
                // 已包含同名资源
                if (item.Name.Equals(itemView.Name))
                    return true;

                if(item.AssetsResInfo == null || item.AssetsResInfo.ResourceType == "Folder")
                {
                    var curName = item.Name;
                    if(string.Equals(curPathName, curName, StringComparison.OrdinalIgnoreCase))
                    {
                        return await AddAssetsItem(path, itemView, item, item.Children);
                    }
                }
            }

            if(path.Count == 0)
            {
                items.Add(itemView);
                itemView.Parent = parent;
                return true;
            }
            else
            {
                var item = new AssetsItemView(curPathName)
                {
                    Icon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Folders/Folder_BaseMix_512x.png", UriKind.Absolute)),
                };
                if (parent == null)
                    item.Path = curPathName;
                else
                    item.Path = parent.Path + "/" + curPathName;
                items.Add(item);
                item.Parent = parent;
                return await AddAssetsItem(path, itemView, item, item.Children);
            }
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            var selItemArray = SelectedItemViews.ToArray();
            foreach(var item in selItemArray)
            {
                DeleteItem(mAssetsItems, item);
                DeleteItem(mAssetsItems_Show, item);
            }
            UpdateInformation();
        }

        bool DeleteItem(ObservableCollection<AssetsItemView> items, AssetsItemView opItem)
        {
            var idx = items.IndexOf(opItem);
            if (idx >= 0)
            {
                opItem.IsSelected = false;
                items.RemoveAt(idx);
                return true;
            }
            else
            {
                foreach(var item in items)
                {
                    if (DeleteItem(item.Children, opItem))
                        return true;
                }
            }
            return false;
        }
    }
}

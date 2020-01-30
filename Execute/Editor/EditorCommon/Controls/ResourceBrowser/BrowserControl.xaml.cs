using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;
using EngineNS.IO;

namespace EditorCommon.Controls.ResourceBrowser
{
    /// <summary>
    /// BrowserControl.xaml 的交互逻辑
    /// </summary>
    public partial class BrowserControl : UserControl, INotifyPropertyChanged, DockControl.IDockAbleControl, IContentControlHost
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region IDockAbleControl
        bool mIsShowing = false;
        public bool IsShowing
        {
            get => mIsShowing;
            set
            {
                mIsShowing = value;
                OnPropertyChanged("IsShowing");
            }
        }
        public bool IsActive { get; set; } = false;
        int mIndex = 0;
        public int Index
        {
            get => mIndex;
            set
            {
                mIndex = value;
                KeyValue = "Content Browser " + (mIndex + 1);
                OnPropertyChanged("KeyValue");
            }
        }
        public string KeyValue
        {
            get;
            private set;
        } = "Content Browser";
        public void SaveElement(XmlNode node, XmlHolder holder)
        {
        }

        public DockControl.IDockAbleControl LoadElement(XmlNode node)
        {
            return null;
        }

        public void StartDrag()
        {
        }

        public void EndDrag()
        {
        }
        #endregion

        bool mIsLocked = false;
        public bool IsLocked
        {
            get => mIsLocked;
            set
            {
#warning 锁定后不再进行资源反查操作
                mIsLocked = value;
                OnPropertyChanged("Locked");
            }
        }

        Visibility mToolbarVisible;
        public Visibility ToolbarVisible
        {
            get => mToolbarVisible;
            set
            {
                mToolbarVisible = value;
                OnPropertyChanged("ToolbarVisible");
            }
        }

        public Visibility ProcessingVisible
        {
            get { return (Visibility)GetValue(ProcessingVisibleProperty); }
            set { SetValue(ProcessingVisibleProperty, value); }
        }
        public static readonly DependencyProperty ProcessingVisibleProperty = DependencyProperty.Register("ProcessingVisible", typeof(Visibility), typeof(BrowserControl), new FrameworkPropertyMetadata(Visibility.Collapsed));

        ContentControl.ShowSourcesInDirData mCurrentFolderData;
        public ContentControl.ShowSourcesInDirData CurrentFolderData
        {
            get => mCurrentFolderData;
            set
            {
                mCurrentFolderData = value;
                UpdateFolderShortcut();
                OnPropertyChanged("CurrentFolderData");
            }
        }

        bool mSearchSubFolder = false;
        public bool SearchSubFolder
        {
            get { return mSearchSubFolder; }
            set
            {
                mSearchSubFolder = value;
                OnPropertyChanged("SearchSubFolder");
            }
        }
        public Resources.ResourceInfo[] GetSelectedResourceInfos()
        {
            return ContentCtrl?.GetSelectedResourceInfos();
        }
        public void SelectResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            ContentCtrl?.SelectResourceInfos(resInfo);
        }
        public BrowserControl()
        {
            InitializeComponent();

            TreeView_Folders.ItemsSource = mAllFolders;
            //EditorCommon.PluginAssist.PluginOperation.OnRefreshBrowserSnapshot += RefreshBrowserSnapshot;

            ContentCtrl.HostControl = this;
            ContentCtrl.Button_ShowFoldersPanel.Click += Button_ShowFoldersPanel_Click;

            SetBinding(ShowEngineContentProperty, new Binding("ShowEngineContent") { Source = ContentCtrl, Mode = BindingMode.TwoWay });
            SetBinding(ShowEditorContentProperty, new Binding("ShowEditorContent") { Source = ContentCtrl, Mode = BindingMode.TwoWay });
        }
        bool mInitialized = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(!mInitialized)
            {
                //InitializeFileSystemWatcher(this);
                //InitializeCreateMenu();
                var folders = new List<ContentControl.ShowSourcesInDirData.FolderData>();
                folders.Add(new ContentControl.ShowSourcesInDirData.FolderData()
                {
                    AbsFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
                    RootFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
                });
                if (ShowEngineContent)
                {
                    folders.Add(new ContentControl.ShowSourcesInDirData.FolderData()
                    {
                        AbsFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                        RootFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                    });
                }
                if (ShowEditorContent)
                {
                    folders.Add(new ContentControl.ShowSourcesInDirData.FolderData()
                    {
                        AbsFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                        RootFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                    });
                }
                ShowFolderTree(folders.ToArray());
                mInitialized = true;
            }

            IsShowing = true;
            IsActive = true;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsActive = false;
        }
        public void AddResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            ContentCtrl.AddResourceInfo(resInfo);
        }
        public void RemoveResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            ContentCtrl.RemoveResourceInfo(resInfo);
        }
        public void UpdateFilter()
        {
            ContentCtrl.UpdateFilter();
        }

        public FrameworkElement GetContainerFromItem(EditorCommon.Resources.ResourceInfo info)
        {
            if (ContentCtrl.ListBox_Resources != null)
                return ContentCtrl.ListBox_Resources.ItemContainerGenerator.ContainerFromItem(info) as FrameworkElement;
            return null;
        }

        #region 资源操作

        UInt64 mShowSourceInDirSerialId = 0;
        public UInt64 ShowSourceInDirSerialId
        {
            get { return mShowSourceInDirSerialId; }
        }
        //public DateTime ShowSourcesInDirStartTime;
        public async System.Threading.Tasks.Task ShowSourcesInDir(ContentControl.ShowSourcesInDirData data)
        {
            // 正在显示当前文件夹，不用重新刷
            if (!data.ForceRefresh && CurrentFolderData != null)
            {
                if(CurrentFolderData.IsFolderDatasEqual(data.FolderDatas))
                    return;
            }

            CurrentFolderData = data;

            // 文件夹浏览历史
            if (data.ResetHistory)
            {
                ResetHistory(data);
            }

            // 设置选中文件夹树形列表
            var folders = data.FolderDatas.ToArray();
            foreach(var folderData in folders)
            {
                var folderString = folderData.AbsFolder;
                folderString = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(folderString, folderData.RootFolder);
                folderString = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(folderData.RootFolder) + "/" + folderString;
                var folderNames = folderString.Split('/');

                foreach (FolderView item in mAllFolders)
                {
                    var tempFolderNames = new List<string>(folderNames);
                    if (item.SelectFolder(tempFolderNames))
                        break;
                }
            }

            if(data.SearchSubFolder == null)
                data.SearchSubFolder = SearchSubFolder;
            //var showData = new ContentControl.ShowSourcesInDirData()
            //{
            //    SearchSubFolder = SearchSubFolder,
            //};
            //showData.FolderDatas = new List<ContentControl.ShowSourcesInDirData.FolderData>(folders);
            mShowSourceInDirSerialId++;
            await ContentCtrl.ShowSourcesInDir(mShowSourceInDirSerialId, data);

            //System.Diagnostics.Trace.WriteLine($"ShowSourcesInDir End: {System.DateTime.Now - ShowSourcesInDirStartTime}");
        }

        // 刷新浏览器资源对象
        public static async Task<bool> OnResourceChanged(EngineNS.RName resName)
        {
            if (resName == null)
                return false;
            bool bHas = false;
            var menuData = EditorCommon.Menu.GeneralMenuManager.Instance.GetMenuData("ContentBrowser");
            if (menuData != null)
            {
                foreach (var ctrl in menuData.OperationControls)
                {
                    var browserCtrl = ctrl as BrowserControl;
                    if (browserCtrl == null)
                        continue;
                    if (browserCtrl.IsLocked)
                        continue;

                    if (browserCtrl.CurrentFolderData == null)
                        continue;
                    bool resFolderIsShowing = false;
                    foreach(var folderData in browserCtrl.CurrentFolderData.FolderDatas)
                    {
                        if(folderData.AbsFolder.Equals(resName.GetDirectory(), StringComparison.OrdinalIgnoreCase))
                        {
                            resFolderIsShowing = true;
                            break;
                        }
                    }
                    if(resFolderIsShowing)
                    {
                        var curRes = browserCtrl.ContentCtrl.CurrentResources.ToArray();
                        foreach(var res in curRes)
                        {
                            if(res.ResourceName == resName)
                            {
                                var findRes = res as EditorCommon.Resources.IResourceInfoForceReload;
                                if(findRes != null)
                                {
                                    findRes.ForceReload();
                                    await browserCtrl.ContentCtrl.ReCreateSnapshot(res);
                                    bHas = true;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return bHas;
        }

        // 资源反查对象
        public static async System.Threading.Tasks.Task ShowResource(EngineNS.RName resName)
        {
            if (resName == null)
                return;
            var menuData = EditorCommon.Menu.GeneralMenuManager.Instance.GetMenuData("ContentBrowser");
            if (menuData != null)
            {
                foreach (var ctrl in menuData.OperationControls)
                {
                    var browserCtrl = ctrl as BrowserControl;
                    if (browserCtrl == null)
                        continue;
                    if (browserCtrl.IsLocked)
                        continue;

                    EditorCommon.Menu.GeneralMenuManager.ShowOperationControl(browserCtrl);
                    var data = new ContentControl.ShowSourcesInDirData();
                    data.FolderDatas.Add(new ContentControl.ShowSourcesInDirData.FolderData()
                    {
                        AbsFolder = resName.GetDirectory(),
                        RootFolder = resName.GetRootFolder(),
                    });
                    await browserCtrl.ShowSourcesInDir(data);

                    browserCtrl.ContentCtrl.SelectItem(resName);
                    break;
                }
            }
        }

        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFolderData == null)
            {
                EditorCommon.MessageBox.Show("请先选择一个文件夹后再进行导入操作!");
                return;
            }
            if(CurrentFolderData.FolderDatas.Count != 1)
            {
                EditorCommon.MessageBox.Show("当前选择了多个文件夹，请先选择一个文件夹后再进行导入操作!");
                return;
            }

            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = true;

            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //在这里处理是否需要打开批量导入配置窗口
                var nouse = ContentCtrl.CheckImportMode(ofd.FileNames, CurrentFolderData.FolderDatas[0]);
            }
        }
        #region 资源创建

        private void IconTextBtn_AddResource_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if(sender == e.OriginalSource)
                InitializeCreateMenu();
        }
        public void InitializeCreateMenu()
        {
            var itemsControl = AdvancedAssetMenuHead.Parent as ItemsControl;
            var baseIdx = itemsControl.Items.IndexOf(BasicAssetMenuHead);
            var advanceIdx = itemsControl.Items.IndexOf(AdvancedAssetMenuHead);
            for(int i=itemsControl.Items.Count - 1; i > advanceIdx; i--)
            {
                itemsControl.Items.RemoveAt(i);
            }
            for(int i=advanceIdx - 2; i > baseIdx; i--)
            {
                itemsControl.Items.RemoveAt(i);
            }
            var baseInsertIdx = baseIdx + 1;
            try
            {
                var infos = EditorCommon.Resources.ResourceInfoManager.Instance.GetConfirmTypeResources(typeof(EditorCommon.Resources.IResourceInfoCreateEmpty));
                foreach (var info in infos)
                {
                    var resCreateInfo = info.ResInfo as EditorCommon.Resources.IResourceInfoCreateEmpty;
                    if (resCreateInfo == null)
                        continue;

                    MenuItem baseMenuItem = null;
                    if(resCreateInfo.IsBaseResource)
                    {
                        baseMenuItem = new MenuItem()
                        {
                            Name = "MenuItem_BrowserControl_Resource",
                            Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            Header = new BaseResControl()
                            {
                                Icon = info.ResInfo.ResourceIcon,
                                ResourceBrush = info.ResInfo.ResourceTypeBrush,
                                Text = info.ResInfo.ResourceTypeName,
                            },
                        };
                        ResourceLibrary.Controls.Menu.MenuAssist.SetHasOffset(baseMenuItem, false);
                        itemsControl.Items.Insert(baseInsertIdx++, baseMenuItem);
                    }

                    if(!string.IsNullOrEmpty(resCreateInfo.CreateMenuPath))
                    {
                        var menuNames = new List<string>(resCreateInfo.CreateMenuPath.Split('/'));
                        GenerateCreateMenu(itemsControl.Items, menuNames, info, baseMenuItem);
                    }
                    else
                        GenerateCreateMenu(itemsControl.Items, null, info, baseMenuItem);
                }
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
        private void GenerateCreateMenu(ItemCollection items, List<string> menuNames, EditorCommon.Resources.ResourceInfoMetaData data, MenuItem baseMenuItem)
        {
            if (items.Count == 0 || menuNames == null)
            {
                GenerateCreateMenuItem(items, menuNames, data, baseMenuItem);
            }
            else
            {
                bool bFind = false;
                foreach (var item in items)
                {
                    var menuItem = item as MenuItem;
                    if (menuItem == null)
                        continue;

                    if (menuItem.Header.ToString() == menuNames[0])
                    {
                        if (menuNames.Count == 1)
                        {
                            GenerateCreateMenuItem(items, menuNames, data, baseMenuItem);
                        }
                        else
                        {
                            menuNames.RemoveAt(0);
                            GenerateCreateMenu(menuItem.Items, menuNames, data, baseMenuItem);
                        }

                        bFind = true;
                        break;
                    }
                }

                if (!bFind)
                {
                    GenerateCreateMenuItem(items, menuNames, data, baseMenuItem);
                }
            }
        }
        private void GenerateCreateMenuItem(ItemCollection items, List<string> menuNames, EditorCommon.Resources.ResourceInfoMetaData data, MenuItem baseMenuItem)
        {
            MenuItem item = null;
            if(menuNames != null)
            {
                item = new MenuItem()
                {
                    Name = "MenuItem_BrowserControl_CreateMenu_",// + menuNames[0],
                    Header = menuNames[0],
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style
                };
                items.Add(item);
            }
            if (menuNames != null && menuNames.Count > 1)
            {
                menuNames.RemoveAt(0);
                GenerateCreateMenu(item.Items, menuNames, data, baseMenuItem);
            }
            else
            {
                var action = new System.Windows.RoutedEventHandler(async ( sender, e) =>
                {
                    if (CurrentFolderData == null)
                    {
                        EditorCommon.MessageBox.Show("请先选择一个文件夹在进行创建！");
                        return;
                    }
                    if(CurrentFolderData.FolderDatas.Count != 1)
                    {
                        EditorCommon.MessageBox.Show("当前选择了多个文件夹，请先选择一个文件夹后再进行创建操作！");
                        return;
                    }

                    var resCreateInfo = data.ResInfo as EditorCommon.Resources.IResourceInfoCreateEmpty;
                    if (resCreateInfo == null)
                        return;

                    var curFolderData = CurrentFolderData.FolderDatas[0];
                    EditorCommon.Resources.IResourceCreateData createData = null;
                    var resCCInfo = data.ResInfo as EditorCommon.Resources.IResourceInfoCustomCreateDialog;
                    if (resCCInfo != null)
                    {
                        var win = resCCInfo.GetCustomCreateDialogWindow();
                        if (win == null)
                            return;
                        win.FolderData = curFolderData;
                        var resName = resCreateInfo.GetValidName(curFolderData.AbsFolder);
                        if (string.IsNullOrEmpty(resName))
                            win.ResourceName = $"new{data.ResInfo.ResourceTypeName}";
                        else
                            win.ResourceName = resName;
                        if (win.ShowDialog((value, cultureInfo) =>
                        {
                            if (value == null)
                                return new ValidationResult(false, "内容不合法");
                            return resCreateInfo.ResourceNameAvailable(curFolderData.AbsFolder, value.ToString());
                        }) == false)
                            return;

                        createData = win.GetCreateData();
                    }
                    else
                    {
                        var resCreatedata = resCreateInfo.GetResourceCreateData(curFolderData.AbsFolder);

                        if(resCreatedata == null)
                        {
                            var win = new InputWindow.InputWindow();
                            win.InformationVisible = true;
                            win.Title = $"创建{data.ResInfo.ResourceTypeName}";
                            var resName = resCreateInfo.GetValidName(curFolderData.AbsFolder);
                            if (string.IsNullOrEmpty(resName))
                                win.Value = $"new{data.ResInfo.ResourceTypeName}";
                            else
                                win.Value = resName;
                            win.Description = $"输入新{data.ResInfo.ResourceTypeName}的名称";

                            if (win.ShowDialog((value, cultureInfo) =>
                            {
                                if (value == null)
                                    return new ValidationResult(false, "内容不合法");
                                return resCreateInfo.ResourceNameAvailable(curFolderData.AbsFolder, value.ToString());
                            }) == false)
                                return;

                            createData = new EditorCommon.Resources.ResourceCreateDataBase();
                            createData.ResourceName = win.Value.ToString();
                            createData.Description = win.Information;
                        }
                        else
                        {
                            var win = new CreateResDialog();
                            win.Title = $"创建{data.ResInfo.ResourceTypeName}";
                            var resName = resCreateInfo.GetValidName(curFolderData.AbsFolder);
                            if (string.IsNullOrEmpty(resName))
                                win.ResourceName = $"new{data.ResInfo.ResourceTypeName}";
                            else
                                win.ResourceName = resName;
                            win.ResCreateData = resCreatedata;
                            resCreatedata.HostDialog = win;

                            if (win.ShowDialog((value, cultureInfo) =>
                             {
                                 if (value == null)
                                     return new ValidationResult(false, "内容不合法");
                                 return resCreateInfo.ResourceNameAvailable(curFolderData.AbsFolder, value.ToString());
                             }) == false)
                                return;

                            createData = win.GetCreateData();
                        }
                    }
                    createData.RNameType = EngineNS.RName.GetRNameTypeFromAbsFileName(curFolderData.AbsFolder);
                    var resourceInfo = await resCreateInfo.CreateEmptyResource(curFolderData.AbsFolder, curFolderData.RootFolder, createData);
                    resourceInfo.Description = createData.Description;
                    var pro = resourceInfo.GetType().GetProperty("ResourceType");
                    pro?.SetValue(resourceInfo, data.ResourceInfoTypeStr);
                    resourceInfo.ParentBrowser = this;
                    await resourceInfo.Save();
                    await resourceInfo.InitializeContextMenu();
                    ContentCtrl.AddResourceInfo(resourceInfo);
                    ContentCtrl.SelectResourceInfos(resourceInfo);

                    //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
                    //{
                    //    EditorCommon.VersionControl.VersionControlManager.Instance.Add((EditorCommon.VersionControl.VersionControlCommandResult result) =>
                    //    {
                    //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
                    //        {
                    //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{resourceInfo.ResourceType}{resourceInfo.Name} {resourceInfo.AbsInfoFileName}使用版本控制添加失败!");
                    //        }
                    //    }, resourceInfo.AbsInfoFileName, $"AutoCommit 创建{resourceInfo.ResourceTypeName}{resourceInfo.Name}");
                    //}
                });
                if(menuNames != null)
                {
                    item.Tag = data;
                    item.Header = new BaseResControl()
                    {
                        Icon = data.ResInfo.ResourceIcon,
                        ResourceBrush = data.ResInfo.ResourceTypeBrush,
                        Text = data.ResInfo.ResourceTypeName,
                    };
                    ResourceLibrary.Controls.Menu.MenuAssist.SetHasOffset(item, false);
                    item.Click += action;
                }
                if (baseMenuItem != null)
                    baseMenuItem.Click += action;
            }
        }

        #endregion
        //private void Button_CopySelected_Click(object sender, RoutedEventArgs e)
        //{
        //    foreach (var item in ListBox_Resources.SelectedItems)
        //    {
        //        var info = item as EditorCommon.Resources.ResourceInfo;
        //        var resInfo = item as EditorCommon.Resources.IResourceInfoCopy;
        //        if (resInfo == null)
        //        {
        //            if (info != null)
        //                EditorCommon.MessageBox.Show($"资源 {info.ResourceName} 不能复制");
        //            continue;
        //        }

        //        Action action = async () =>
        //        {
        //            var copyedRes = await resInfo.CopyResource();
        //            if (copyedRes != null)
        //            {
        //                copyedRes.Save();
        //                CurrentResources.Add(copyedRes);
        //                copyedRes.ParentBrowser = this;

        //                //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
        //                //{
        //                //    EditorCommon.VersionControl.VersionControlManager.Instance.Add((EditorCommon.VersionControl.VersionControlCommandResult result) =>
        //                //    {
        //                //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
        //                //        {
        //                //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{copyedRes.ResourceType}{copyedRes.Name} {copyedRes.AbsInfoFileName}使用版本控制添加失败!");
        //                //        }
        //                //    }, copyedRes.AbsInfoFileName, $"AutoCommit 从{info.ResourceTypeName}{info.Name}复制{copyedRes.ResourceTypeName}{copyedRes.Name}");
        //                //}
        //            }
        //        };
        //        action();
        //    }

        //    var noUse = UpdateCountString();
        //}
        //private void Button_SaveSelected_Click(object sender, RoutedEventArgs e)
        //{
        //    foreach (var item in ListBox_Resources.SelectedItems)
        //    {
        //        var resinfo = item as EditorCommon.Resources.ResourceInfo;
        //        if (resinfo == null)
        //            continue;

        //        resinfo.Save();
        //    }
        //}
        Dictionary<Guid, EditorCommon.Resources.ResourceInfo> mDirtyResources = new Dictionary<Guid, EditorCommon.Resources.ResourceInfo>();
        private void Button_SaveAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var res in mDirtyResources.Values)
            {
                var nouse = res.Save();
            }
        }
        //private void Button_DeleteSelected_Click(object sender, RoutedEventArgs e)
        //{
        //    if (ListBox_Resources.SelectedItems.Count <= 0)
        //        return;

        //    if (EditorCommon.MessageBox.Show($"是否删除选中的{ListBox_Resources.SelectedItems.Count}个资源?", EditorCommon.MessageBox.enMessageBoxButton.YesNo) != EditorCommon.MessageBox.enMessageBoxResult.Yes)
        //        return;

        //    var list = new List<EditorCommon.Resources.ResourceInfo>();
        //    foreach (var item in ListBox_Resources.SelectedItems)
        //    {
        //        var resInfo = item as EditorCommon.Resources.ResourceInfo;
        //        if (resInfo == null)
        //            continue;

        //        list.Add(resInfo);
        //    }

        //    //foreach (var resInfo in list)
        //    //{
        //    //    mDirtyResources.Remove(resInfo.Id);
        //    //    resInfo.DeleteResource();
        //    //    CurrentResources.Remove(resInfo);
        //    //    resInfo.ParentBrowser = null;
        //    //}

        //    mResourceWrapPanel.SetVerticalOffset(0);

        //    var noUse = UpdateCountString();
        //}

        #endregion

        #region 文件夹操作

        public bool ShowEngineContent
        {
            get { return (bool)GetValue(ShowEngineContentProperty); }
            set { SetValue(ShowEngineContentProperty, value); }
        }
        public static readonly DependencyProperty ShowEngineContentProperty = DependencyProperty.Register("ShowEngineContent", typeof(bool), typeof(BrowserControl), 
                                                                                                        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnShowEngineContentChanged)));
        public static void OnShowEngineContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as BrowserControl;
            var newValue = (bool)e.NewValue;
            if(newValue)
            {
                var folders = new ContentControl.ShowSourcesInDirData.FolderData[] {
                                            new ContentControl.ShowSourcesInDirData.FolderData()
                                            {
                                                AbsFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                                                RootFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                                            }};
                ctrl.ShowFolderTree(folders, false);
            }
            else
            {
                foreach(var item in ctrl.mAllFolders)
                {
                    if(string.Equals(item.FolderData.AbsFolder, EngineNS.CEngine.Instance.FileManager.EngineContent.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        ctrl.mAllFolders.Remove(item);
                        break;
                    }
                }
                foreach(var item in ctrl.mViewFolders)
                {
                    if (string.Equals(item.FolderData.AbsFolder, EngineNS.CEngine.Instance.FileManager.EngineContent.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        ctrl.mViewFolders.Remove(item);
                        break;
                    }
                }
            }
        }

        public bool ShowEditorContent
        {
            get { return (bool)GetValue(ShowEditorContentProperty); }
            set { SetValue(ShowEditorContentProperty, value); }
        }
        public static readonly DependencyProperty ShowEditorContentProperty = DependencyProperty.Register("ShowEditorContent", typeof(bool), typeof(BrowserControl), 
                                                                                                        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnShowEditorContentChanged)));
        public static void OnShowEditorContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as BrowserControl;
            var newValue = (bool)e.NewValue;
            if(newValue)
            {
                var folders = new ContentControl.ShowSourcesInDirData.FolderData[] {
                                                    new ContentControl.ShowSourcesInDirData.FolderData()
                                                    {
                                                        AbsFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                                                        RootFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                                                    }};
                ctrl.ShowFolderTree(folders, false);
            }
            else
            {
                foreach (var item in ctrl.mAllFolders)
                {
                    if (string.Equals(item.FolderData.AbsFolder, EngineNS.CEngine.Instance.FileManager.EditorContent.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        ctrl.mAllFolders.Remove(item);
                        break;
                    }
                }
                foreach (var item in ctrl.mViewFolders)
                {
                    if (string.Equals(item.FolderData.AbsFolder, EngineNS.CEngine.Instance.FileManager.EditorContent.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        ctrl.mViewFolders.Remove(item);
                        break;
                    }
                }
            }
        }

        public void ShowFolderTree(ContentControl.ShowSourcesInDirData.FolderData[] targetFolders, bool clearFolders = true)
        {
            if (clearFolders)
            {
                mAllFolders.Clear();
            }
            foreach (var targetFolder in targetFolders)
            {
                if (EngineNS.CEngine.Instance.FileManager.DirectoryExists(targetFolder.AbsFolder))
                {
                    if (!IsFolderValid(targetFolder.AbsFolder))
                        return;

                    var item = new FolderView(targetFolder, this);
                    mAllFolders.Add(item);
                    item.IsExpanded = true;
                    ContentCtrl.ClearResourcesShow();
                }
            }
        }
        // 判断文件夹的合法性
        public bool IsFolderValid(string absFolder)
        {
            var resInfoFile = absFolder + EditorCommon.Program.ResourceInfoExt;
            if (System.IO.File.Exists(resInfoFile))
                return false;

            var subFolderName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(absFolder);
            foreach (var keyword in ContentControl.FolderKeywords)
            {
                if (string.Equals(keyword, subFolderName, System.StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
        bool mFolderPreEnable = false;
        public bool FolderPreEnable
        {
            get { return mFolderPreEnable; }
            set
            {
                mFolderPreEnable = value;
                if (mFolderPreEnable)
                    Path_FolderPre.Foreground = TryFindResource("NormalFolderNavBrush") as Brush;
                else
                    Path_FolderPre.Foreground = TryFindResource("DisableFolderNavBrush") as Brush;
                OnPropertyChanged("FolderPreEnable");
            }
        }
        bool mFolderNextEnable = false;
        public bool FolderNextEnable
        {
            get { return mFolderNextEnable; }
            set
            {
                mFolderNextEnable = value;
                if (mFolderNextEnable)
                    Path_FolderNext.Foreground = TryFindResource("NormalFolderNavBrush") as Brush;
                else
                    Path_FolderNext.Foreground = TryFindResource("DisableFolderNavBrush") as Brush;
                OnPropertyChanged("FolderNextEnable");
            }
        }
        int mFolderHistoryCount = 20;
        int mFolderHistoryCurrentIndex = -1;

        List<ContentControl.ShowSourcesInDirData> mFolderHistory = new List<ContentControl.ShowSourcesInDirData>();
        private void ResetHistory(ContentControl.ShowSourcesInDirData data)
        {
            if (mFolderHistoryCurrentIndex >= 0)
            {
                for (int i = mFolderHistory.Count - 1; i > mFolderHistoryCurrentIndex; i--)
                {
                    mFolderHistory.RemoveAt(i);
                }
            }
            if (mFolderHistory.Count > mFolderHistoryCount)
                mFolderHistory.RemoveAt(0);
            mFolderHistory.Add(data);
            mFolderHistoryCurrentIndex = mFolderHistory.Count - 1;
            if (mFolderHistoryCurrentIndex > 0)
            {
                FolderPreEnable = true;
            }
            FolderNextEnable = false;
        }
        private void Button_FolderPre_Click(object sender, RoutedEventArgs e)
        {
            FolderNextEnable = true;

            if (mFolderHistory.Count < 1)
                return;

            mFolderHistoryCurrentIndex--;
            if (mFolderHistoryCurrentIndex < 0)
                mFolderHistoryCurrentIndex = mFolderHistory.Count - 2;
            if (mFolderHistoryCurrentIndex < 0)
                mFolderHistoryCurrentIndex = 0;
            if (mFolderHistoryCurrentIndex == 0)
                FolderPreEnable = false;

            var folderHistoryData = mFolderHistory[mFolderHistoryCurrentIndex];
            var data = new ContentControl.ShowSourcesInDirData()
            {
                ResetHistory = false,
            };
            data.FolderDatas = folderHistoryData.FolderDatas;
            var noUse = ShowSourcesInDir(data);
        }
        private void Button_FolderNext_Click(object sender, RoutedEventArgs e)
        {
            FolderPreEnable = true;

            if (mFolderHistoryCurrentIndex >= mFolderHistory.Count - 1)
                return;

            mFolderHistoryCurrentIndex++;
            if (mFolderHistoryCurrentIndex >= mFolderHistory.Count - 1)
                FolderNextEnable = false;

            var folderHistoryData = mFolderHistory[mFolderHistoryCurrentIndex];
            var data = new ContentControl.ShowSourcesInDirData()
            {
                ResetHistory = false,
            };
            data.FolderDatas = folderHistoryData.FolderDatas;
            var noUse = ShowSourcesInDir(data);

        }

        // 更新文件夹快捷方式访问，用于快速访问文件夹
        private void UpdateFolderShortcut()
        {
            StackPanel_Folders.Children.Clear();
            if (CurrentFolderData.FolderDatas.Count != 1)
                return;
            var folderData = CurrentFolderData.FolderDatas[0];

            var absFolder = folderData.AbsFolder.ToLower();
            absFolder.Replace('\\', '/');
            if (absFolder.EndsWith("/") == false)
                absFolder += '/';

            string root = EngineNS.CEngine.Instance.FileManager.ProjectContent;
            if (absFolder.StartsWith(EngineNS.CEngine.Instance.FileManager.ProjectContent))
            {
                root = EngineNS.CEngine.Instance.FileManager.ProjectContent;
            }
            else if (absFolder.StartsWith(EngineNS.CEngine.Instance.FileManager.EngineContent))
            {
                root = EngineNS.CEngine.Instance.FileManager.EngineContent;
            }
            else if (absFolder.StartsWith(EngineNS.CEngine.Instance.FileManager.EditorContent))
            {
                root = EngineNS.CEngine.Instance.FileManager.EditorContent;
            }

            var relDir = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(folderData.AbsFolder, root);
            relDir = relDir.Replace("\\", "/");
            var dirNames = relDir.Split('/');
            var dirs = "";
            if (dirNames.Length == 0)
                return;
            var firstName = dirNames[0];
            foreach (var dirName in dirNames)
            {
                if (string.IsNullOrEmpty(dirName))
                    continue;

                dirs += dirName + "/";

                var btn = new Button()
                {
                    Margin = new Thickness(2),
                    Tag = dirs,
                    Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ButtonStyle_Default")) as Style
                };
                var grid = new Grid()
                {
                    Margin = new Thickness(3,1,3,1),
                };
                // <TextBlock Text="Import" FontSize="14" Style="{DynamicResource {ComponentResourceKey ResourceId=TextBlockStyle_Default, TypeInTargetAssembly={x:Type res:CustomResources}}}" Margin="0,0,5,0" FontWeight="Bold"/>
                grid.Children.Add(new TextBlock()
                {
                    Margin = new Thickness(1, 1, 0, 0),
                    FontSize = 13,
                    Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default")) as Style,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Text = dirName,
                });
                grid.Children.Add(new TextBlock()
                {
                    FontSize = 13,
                    Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default")) as Style,
                    FontWeight = FontWeights.Bold,
                    Text = dirName,
                });
                btn.Content = grid;
                btn.Click += (sender, e) =>
                {
                    // 点击显示相应目录
                    var data = new ContentControl.ShowSourcesInDirData();
                    data.FolderDatas.Add(new ContentControl.ShowSourcesInDirData.FolderData()
                    {
                        AbsFolder = root + (string)(btn.Tag),
                        RootFolder = root + firstName,
                    });
                    var noUse = ShowSourcesInDir(data);
                };
                StackPanel_Folders.Children.Add(btn);

                var tBtn = new Button()
                {
                    Margin = new Thickness(2),
                    Tag = dirs,
                    Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ButtonStyle_Default")) as Style
                };
                //tBtn.Content = new TextBlock()
                //{
                //    Text = ">",
                //    VerticalAlignment = VerticalAlignment.Center,
                //    Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Icon")) as Style
                //};
                tBtn.Content = new Image()
                {
                    Source = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Common/SmallArrowRight.png", UriKind.Absolute)),
                    Width = 8,
                    Height = 8,
                    Margin = new Thickness(3,0,3,0),
                };
                StackPanel_Folders.Children.Add(tBtn);

                var folders = GetSubFolders(root + dirs, System.IO.SearchOption.TopDirectoryOnly);
                if (folders.Count > 0)
                {
                    var contextMenu = new ContextMenu()
                    {
                        StaysOpen = false,
                        Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as Style,
                        PlacementTarget = tBtn,
                        Placement = System.Windows.Controls.Primitives.PlacementMode.Top,
                    };

                    foreach (var folder in folders)
                    {
                        bool isCurrentFolder = folder.Replace("\\", "/").Equals(folderData.AbsFolder, StringComparison.CurrentCultureIgnoreCase);
                        var menuItem = new MenuItem()
                        {
                            Name = "MenuItem_BrowserControl_Folder",
                            IsChecked = isCurrentFolder,
                            Header = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(folder),
                            Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        menuItem.Click += (sender, e) =>
                        {
                            var data = new ContentControl.ShowSourcesInDirData();
                            data.FolderDatas.Add(new ContentControl.ShowSourcesInDirData.FolderData()
                            {
                                AbsFolder = folder,
                                RootFolder = root + firstName,
                            });
                            var nouse = ShowSourcesInDir(data);
                        };
                        contextMenu.Items.Add(menuItem);
                    }

                    tBtn.Click += (sender, e) =>
                    {
                        //点击打开次级目录结构
                        contextMenu.IsOpen = true;
                    };
                }
            }
        }

        #region 文件夹拖动
        #endregion

        #endregion

        //#region FileSystemWatcher

        //static FileSystemWatcher mResourceFilesWatcher;
        //public static void InitializeFileSystemWatcher(BrowserControl ctrl)
        //{
        //    if(mResourceFilesWatcher == null)
        //    {
        //        mResourceFilesWatcher = new FileSystemWatcher(EngineNS.CEngine.Instance.FileManager.Content);
        //        mResourceFilesWatcher.EnableRaisingEvents = true;
        //        mResourceFilesWatcher.IncludeSubdirectories = true;
        //        mResourceFilesWatcher.Renamed += (object sender, RenamedEventArgs e) =>
        //        {
        //            OnFileSystemWatcherEventRise(sender, e, ctrl);
        //        };
        //        mResourceFilesWatcher.Created += (object sender, FileSystemEventArgs e) =>
        //        {
        //            OnFileSystemWatcherEventRise(sender, e, ctrl);
        //        };
        //        mResourceFilesWatcher.Deleted += (object sender, FileSystemEventArgs e) =>
        //        {
        //            OnFileSystemWatcherEventRise(sender, e, ctrl);
        //        };
        //        mResourceFilesWatcher.Changed += (object sender, FileSystemEventArgs e) =>
        //        {
        //            OnFileSystemWatcherEventRise(sender, e, ctrl);
        //        };
        //    }
        //}

        //static void OnFileSystemWatcherEventRise(object sender, FileSystemEventArgs e, BrowserControl ctrl)
        //{
        //    EngineNS.CEngine.Instance.EventPoster.RunOn(async ()=>
        //    {
        //        switch (e.ChangeType)
        //        {
        //            case WatcherChangeTypes.Changed:
        //            case WatcherChangeTypes.Renamed:
        //            case WatcherChangeTypes.Created:
        //                {
        //                    var absFileName = e.FullPath.Replace("\\", "/");
        //                    absFileName = absFileName.Replace("//", "/");
        //                    var path = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(absFileName);
        //                    if (path.Length > 0 && path[path.Length - 1] == '/')
        //                        path = path.Remove(path.Length - 1);
        //                    if (ctrl.CurrentAbsFolder.Equals(path, StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        var curRes = ctrl.ContentCtrl.CurrentResources.ToArray();
        //                        foreach (var res in curRes)
        //                        {
        //                            if (res.ResourceName.Address.Equals(absFileName, StringComparison.OrdinalIgnoreCase))
        //                            {
        //                                var frlRes = res as EditorCommon.Resources.IResourceInfoForceReload;
        //                                if (frlRes != null)
        //                                {
        //                                    frlRes?.ForceReload();
        //                                    await ctrl.ContentCtrl.ReCreateSnapshot(res);
        //                                }
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var file = absFileName + EditorCommon.Program.ResourceInfoExt;
        //                        if (System.IO.File.Exists(file))
        //                        {
        //                            var resInfo = await ctrl.ContentCtrl.CreateResourceInfo(absFileName + EditorCommon.Program.ResourceInfoExt);
        //                            if (resInfo != null)
        //                            {
        //                                var frlRes = resInfo as EditorCommon.Resources.IResourceInfoForceReload;
        //                                if (frlRes != null)
        //                                {
        //                                    await resInfo.Save();
        //                                    frlRes?.ForceReload();
        //                                    await ctrl.ContentCtrl.ReCreateSnapshot(resInfo);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                break;
        //        }
        //        return true;
        //    },EngineNS.Thread.Async.EAsyncTarget.Main);
        //}

        //#endregion

        #region ShowHideFolderPanel
        public Visibility FolderPanelShow
        {
            get { return (Visibility)GetValue(FolderPanelShowProperty); }
            set { SetValue(FolderPanelShowProperty, value); }
        }

        public string DockGroup => DockControl.DockManager.DockableAllGroup;

        public static readonly DependencyProperty FolderPanelShowProperty =
            DependencyProperty.Register("FolderPanelShow", typeof(Visibility), typeof(BrowserControl), new FrameworkPropertyMetadata(Visibility.Visible));

        private void Button_HideFoldersPanel_Click(object sender, RoutedEventArgs e)
        {
            HideFolderPanel();
        }
        public void HideFolderPanel()
        {
            FolderPanelShow = Visibility.Collapsed;
            ContentCtrl.Button_ShowFoldersPanel.Visibility = Visibility.Visible;
            MainGrid.ColumnDefinitions[0].Width = new GridLength(0);
        }
        private void Button_ShowFoldersPanel_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderPanel();
        }
        public void ShowFolderPanel()
        {
            FolderPanelShow = Visibility.Visible;
            ContentCtrl.Button_ShowFoldersPanel.Visibility = Visibility.Collapsed;
            MainGrid.ColumnDefinitions[0].Width = new GridLength(MainGrid.ColumnDefinitions[2].Width.Value * 0.5, GridUnitType.Star);
        }

        public bool? CanClose()
        {
            return true;
        }

        public void Closed()
        {

        }
        #endregion

        Point mStartDragPoint;
        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var elm = sender as FrameworkElement;
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                mStartDragPoint = e.GetPosition(elm);
                Mouse.Capture(elm);
            }
        }

        private void TreeViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as FrameworkElement;
            if(e.LeftButton == MouseButtonState.Pressed && Mouse.Captured == elm)
            {
                var pt = e.GetPosition(elm);
                if(Math.Abs(pt.X - mStartDragPoint.X) > 5 || Math.Abs(pt.Y - mStartDragPoint.Y) > 5)
                {
                    var folderView = elm.DataContext as EditorCommon.Controls.ResourceBrowser.BrowserControl.FolderView;
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(BrowserControl.FolderDragType, new DragDrop.IDragAbleObject[] { folderView });
                }
            }
        }
    }
}

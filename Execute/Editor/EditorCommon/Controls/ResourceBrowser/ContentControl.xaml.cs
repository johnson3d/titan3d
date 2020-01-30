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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EditorCommon.Controls.ResourceBrowser
{
    public interface IContentControlHost
    {
        UInt64 ShowSourceInDirSerialId { get; }
        FrameworkElement GetContainerFromItem(EditorCommon.Resources.ResourceInfo info);
        void AddResourceInfo(EditorCommon.Resources.ResourceInfo resInfo);
        void RemoveResourceInfo(EditorCommon.Resources.ResourceInfo resInfo);
        Task ShowSourcesInDir(ContentControl.ShowSourcesInDirData data);
        void UpdateFilter();
        EditorCommon.Resources.ResourceInfo[] GetSelectedResourceInfos();
        void SelectResourceInfo(EditorCommon.Resources.ResourceInfo resInfo);
    }

    /// <summary>
    /// ContentControl.xaml 的交互逻辑
    /// </summary>
    public partial class ContentControl : UserControl, INotifyPropertyChanged, ResourceLibrary.Controls.Button.IIconTextBtn_CustomIsSubmenuOpen
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region IIconTextBtn_CustomIsSubmenuOpen
        public bool IsCustom => true;
        #endregion

        // 文件夹关键字（已被编辑器占用，不能创建及使用的文件夹名称）
        public static string[] FolderKeywords = new string[] { "MetaClasses", "Cache", "Shaders", "ShadingEnv" };

        double mChildWidth = 101;
        double mChildHeight = 137;
        public IContentControlHost HostControl;

        double mIconScale = 0.8;
        public double IconScale
        {
            get { return mIconScale; }
            set
            {
                mIconScale = value;

                if (mResourceWrapPanel == null)
                    return;

                mResourceWrapPanel.ChildWidth = mChildWidth * mIconScale;
                mResourceWrapPanel.ChildHeight = mChildHeight * mIconScale;
                mResourceWrapPanel.ScrollToIndex(ListBox_Resources.SelectedIndex);

                OnPropertyChanged("IconScale");
            }
        }
        bool mIconScaleEnable = true;
        public bool IconScaleEnable
        {
            get { return mIconScaleEnable; }
            set
            {
                mIconScaleEnable = value;
                OnPropertyChanged("IconScaleEnable");
            }
        }

        public static string SceneNodeDragType
        {
            get { return "EditorCommon.Controls.SceneNodes"; }
        }

        // 子对象数量统计
        string mSourceCountString = "";
        public string SourceCountString
        {
            get { return mSourceCountString; }
            set
            {
                mSourceCountString = value;
                OnPropertyChanged("SourceCountString");
            }
        }
        public void UpdateCountString()
        {
            SourceCountString = $"{CurrentResources.Count} items({ListBox_Resources.SelectedItems.Count} selected)";
        }

        public Visibility FilterVisible
        {
            get { return (Visibility)GetValue(FilterVisibleProperty); }
            set { SetValue(FilterVisibleProperty, value); }
        }
        public static readonly DependencyProperty FilterVisibleProperty = DependencyProperty.Register("FilterVisible", typeof(Visibility), typeof(ContentControl), new FrameworkPropertyMetadata(Visibility.Visible));

        public ContentControl()
        {
            InitializeComponent();

            mListBoxDropAdorner = new EditorCommon.DragDrop.DropAdorner(ListBox_Resources);
            this.SetBinding(ShowEditorContentProperty, new Binding("ShowEditorContent") { Source = EngineNS.CEngine.Instance.GameEditorInstance, Mode = BindingMode.TwoWay });
            this.SetBinding(ShowEngineContentProperty, new Binding("ShowEngineContent") { Source = EngineNS.CEngine.Instance.GameEditorInstance, Mode = BindingMode.TwoWay });

            ListBox_Resources.ItemsSource = CurrentResources;
        }

        public void FocusSearchBox()
        {
            SearchBoxCtrl.FocusInput();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeFilter();
        }

        VirtualizingWrapPanel mResourceWrapPanel;
        private void VirtualizingWrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            mResourceWrapPanel = sender as VirtualizingWrapPanel;
            if (mResourceWrapPanel != null)
            {
                mResourceWrapPanel.ChildWidth = mChildWidth * IconScale;
                mResourceWrapPanel.ChildHeight = mChildHeight * IconScale;
            }
        }

        #region 筛选

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;

                UpdateFilter();
                if (string.IsNullOrEmpty(mFilterString))
                {
                    ListBox_Resources.Items.Filter = null;
                }

                if (mResourceWrapPanel != null)
                {
                    mResourceWrapPanel.InvalidateMeasure();
                    mResourceWrapPanel.SetVerticalOffset(0);
                }

                OnPropertyChanged("FilterString");
            }
        }
        bool mIsCheckedAll = true;
        public bool IsCheckedAll
        {
            get { return mIsCheckedAll; }
            set
            {
                mIsCheckedAll = value;
                foreach (var i in mFilterItems)
                {
                    i.IsChecked = value;
                }
            }
        }
        ObservableCollection<FilterResourceItem> mFilterItems = new ObservableCollection<FilterResourceItem>();
        public void InitializeFilter()
        {
            mFilterItems.Clear();

            var datas = EditorCommon.Resources.ResourceInfoManager.Instance.GetAllResourceInfoDatas();
            foreach (var data in datas)
            {
                FilterResourceItem item = new FilterResourceItem(data, HostControl);
                mFilterItems.Add(item);
            }
            //ListBox_FilterItems.ItemsSource = mFilterItems;
        }
        public void UpdateFilter()
        {
            ListBox_Resources.Items.Filter = new Predicate<object>((object obj) =>
            {
                var retValue = true;

                var info = obj as EditorCommon.Resources.ResourceInfo;
                if (!string.IsNullOrEmpty(FilterString))
                {
                    if (info != null)
                    {
                        retValue = info.DoFilter(mFilterString);
                    }
                }

                foreach (var resItem in mFilterItems)
                {
                    if (resItem.ResourceType.Equals(info.ResourceType))
                    {
                        retValue = retValue && resItem.IsChecked;
                        break;
                    }
                }

                info.HightlightString = FilterString;
                mResourceWrapPanel?.SetVerticalOffset(0);

                return retValue;
            });
        }

        #endregion

        EditorCommon.DragDrop.DropAdorner mListBoxDropAdorner;
        // 当前显示的资源
        ObservableCollection<EditorCommon.Resources.ResourceInfo> mCurrentResources = new ObservableCollection<EditorCommon.Resources.ResourceInfo>();
        public ObservableCollection<EditorCommon.Resources.ResourceInfo> CurrentResources
        {
            get { return mCurrentResources; }
            set
            {
                mCurrentResources = value;
                OnPropertyChanged("CurrentResources");
            }
        }

        public async System.Threading.Tasks.Task<EditorCommon.Resources.ResourceInfo> CreateResourceInfo(string resourceInfoAbsFile)
        {
            return await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(resourceInfoAbsFile, HostControl);
        }
        public async System.Threading.Tasks.Task<EditorCommon.Resources.ResourceInfo> CreateResourceInfoFromResource(string resourceAbsFile)
        {
            var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(resourceAbsFile);
            resInfo.ParentBrowser = HostControl;

            ////((HwndSource)PresentationSource.FromVisual(uielement)).Handle;
            //var hwnd = new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle;
            //ResourceLibrary.Win32.PostMessage(hwnd, ResourceLibrary.Win32.WM_PRINT, 0, 0);
            return resInfo;
        }

        public void AddResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            CurrentResources.Add(resInfo);
            UpdateCountString();
        }
        public void RemoveResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            CurrentResources.Remove(resInfo);
        }

        public void ClearResourcesShow()
        {
            CurrentResources.Clear();
        }

        public class ShowSourcesInDirData
        {
            public class FolderData :IComparable
            {
                string mSourceAbsFolder;
                public string AbsFolder
                {
                    get => mSourceAbsFolder;
                    set
                    {
                        mSourceAbsFolder = value.Replace("\\", "/").ToLower();
                        mSourceAbsFolder = mSourceAbsFolder.TrimEnd('/');
                    }
                }
                string mRootFolder;
                public string RootFolder
                {
                    get => mRootFolder;
                    set
                    {
                        mRootFolder = value.Replace("\\", "/").ToLower();
                        mRootFolder = mRootFolder.TrimEnd('/');
                    }
                }
                public bool IsEqual(FolderData data)
                {
                    if (data.AbsFolder == AbsFolder)
                        return true;

                    return false;
                }

                public int CompareTo(object obj)
                {
                    var data = obj as FolderData;
                    return AbsFolder.CompareTo(data.AbsFolder);
                }
            }
            public List<FolderData> FolderDatas = new List<FolderData>();
            public bool? SearchSubFolder = false;
            public string[] FileExts = null;
            public Func<EditorCommon.Resources.ResourceInfo, bool> CompareFuction = null;

            public bool ResetHistory = true;
            public bool ForceRefresh = false;

            public bool IsFolderDatasEqual(List<FolderData> datas)
            {
                if (FolderDatas.Count != datas.Count)
                    return false;
                FolderDatas.Sort();
                datas.Sort();
                for (int i = 0; i < FolderDatas.Count; i++)
                {
                    if (!FolderDatas[i].IsEqual(datas[i]))
                        return false;
                }
                return true;
            }
        }

        List<ShowSourcesInDirData.FolderData> mFolderDatas = new List<ShowSourcesInDirData.FolderData>();
        public async System.Threading.Tasks.Task ShowSourcesInDir(UInt64 serialId, ShowSourcesInDirData data)
        {
            mFolderDatas.Clear();
            EditorCommon.Resources.ResourceInfoManager.Instance.ClearResourceInfoSnapshot();
            mFolderDatas.AddRange(data.FolderDatas);
            await EngineNS.CEngine.Instance.EventPoster.Post(async () =>
            {
                CurrentResources.Clear();
                await FilterSources(serialId, data);
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }

        public async System.Threading.Tasks.Task FilterSources(UInt64 serialId, ShowSourcesInDirData data)
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                ListBox_Resources.ItemsSource = null;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            if (data.SearchSubFolder != true)
            {
                for (int i = 0; i < data.FolderDatas.Count; i++)
                {
                    var folderData = data.FolderDatas[i];
                    var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(folderData.AbsFolder);
                    foreach (var dir in dirs)
                    {
                        if (HostControl.ShowSourceInDirSerialId != serialId)
                            return;
                        bool isKeyword = false;
                        var dirName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(dir);
                        foreach (var keyword in FolderKeywords)
                        {
                            if (string.Equals(dirName, keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                isKeyword = true;
                                break;
                            }
                        }
                        if (isKeyword)
                            continue;
                        var rInfoFile = dir + EditorCommon.Program.ResourceInfoExt;
                        if (EngineNS.CEngine.Instance.FileManager.FileExists(rInfoFile))
                            continue;
                        var info = await CreateResourceInfoFromResource(dir);
                        CurrentResources.Add(info);
                        info.ParentBrowser = HostControl;
                    }
                }
            }

            var tempFileExts = data.FileExts;
            if (data.FileExts == null)
                tempFileExts = new string[] { EditorCommon.Program.ResourceInfoExt };
            for(int f=0; f < data.FolderDatas.Count; f++)
            {
                var folderData = data.FolderDatas[f];
                foreach (var ext in tempFileExts)
                {
                    var files = new List<string>(EngineNS.CEngine.Instance.FileManager.GetFiles(folderData.AbsFolder, "*" + ext, (data.SearchSubFolder == true) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly));
                    files.AddRange(EngineNS.CEngine.Instance.FileManager.GetDirectories(folderData.AbsFolder, "*" + ext, (data.SearchSubFolder == true) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly));
                    var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(files.Count);
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        //要干掉！！
                        //if (file.IndexOf(".macross") != -1 && EngineNS.CEngine.Instance.FileManager.FileExists(file.Replace(".macross", ".particle")))
                        //{
                        //    continue;
                        //}
                        EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
                        {
                            if (data.FileExts != null)
                            {
                                file = file + EditorCommon.Program.ResourceInfoExt;
                                if (!EngineNS.CEngine.Instance.FileManager.FileExists(file))
                                {
                                    smp.Release();
                                    return false;
                                }
                            }
                            var info = await CreateResourceInfo(file);
                            if (HostControl?.ShowSourceInDirSerialId != serialId)
                            {
                                smp.Release();
                                return false;
                            }
                            if (info != null)
                            {
                                if (!(info.ResourceName.Address + Program.ResourceInfoExt).ToLower().Contains(folderData.AbsFolder.ToLower()))
                                {
                                    smp.Release();
                                    return false;
                                }

                                if (data.CompareFuction != null)
                                {
                                    if (!data.CompareFuction(info))
                                    {
                                        smp.Release();
                                        return false;
                                    }
                                }

                                await EngineNS.CEngine.Instance.EventPoster.Post(() =>
                                {
                                    CurrentResources.Add(info);
                                    info.ParentBrowser = HostControl;
                                    UpdateCountString();
                                    return true;
                                }, EngineNS.Thread.Async.EAsyncTarget.Main);
                            }
                            smp.Release();
                            return true;
                        }, EngineNS.Thread.Async.EAsyncTarget.TPools);
                    }
                    //await EngineNS.CEngine.Instance.EventPoster.AwaitSemaphore(smp);
                    await smp.Await();
                }
            }
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                UpdateCountString();
                Sort("ResourceTypeName", "ResourceName");
                mResourceWrapPanel?.SetVerticalOffset(0);
                ListBox_Resources.ItemsSource = CurrentResources;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }

        public EditorCommon.Resources.ResourceInfo[] GetSelectedResourceInfos()
        {
            var count = ListBox_Resources.SelectedItems.Count;
            var items = new EditorCommon.Resources.ResourceInfo[count];
            ListBox_Resources.SelectedItems.CopyTo(items, 0);
            return items;
        }
        public void SelectResourceInfos(EditorCommon.Resources.ResourceInfo resInfo)
        {
            ListBox_Resources.SelectedItem = resInfo;
            ListBox_Resources.ScrollIntoView(resInfo);
        }

        public Action<EditorCommon.Resources.ResourceInfo> OnResourceItemSelected;
        private void ListBox_Resources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCountString();

            if (ListBox_Resources.SelectedItems.Count == 0)
                return;

            var resinfo = ListBox_Resources.SelectedItems[0] as EditorCommon.Resources.ResourceInfo;
            if (resinfo != null)
            {
                resinfo.SetSelectedObjectData();
                OnResourceItemSelected?.Invoke(resinfo);
            }
        }
        private void ListBox_Resources_DragEnter(object sender, DragEventArgs e)
        {
            bool allowDrop = false;
            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var datas = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (datas != null && datas.Length > 0)
                {
                    allowDrop = CheckFileDropAvailable(datas);
                    if (!allowDrop)
                    {
                        allowDrop = CheckFileImprotAvailable(datas);
                    }
                }
            }
            else if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                // 资源文件拖动
                allowDrop = true;
            }
            else if (SceneNodeDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                //TODO..
                allowDrop = true;
            }

            if (allowDrop)
            {
                e.Effects = DragDropEffects.Copy;
                mListBoxDropAdorner.IsAllowDrop = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                mListBoxDropAdorner.IsAllowDrop = false;
            }

            var pos = e.GetPosition(ListBox_Resources);
            if (pos.X > 0 && pos.X < ListBox_Resources.ActualWidth &&
               pos.Y > 0 && pos.Y < ListBox_Resources.ActualHeight)
            {
                var layer = AdornerLayer.GetAdornerLayer(ListBox_Resources);
                layer.Add(mListBoxDropAdorner);
            }

            e.Handled = true;
        }
        private void ListBox_Resources_DragOver(object sender, DragEventArgs e)
        {

        }
        private void ListBox_Resources_Drop(object sender, DragEventArgs e)
        {
            var noUse = ListBox_Resources_Drop_Async(sender, e);
        }
        async Task ListBox_Resources_Drop_Async(object sender, DragEventArgs e)
        {
            if (mFolderDatas.Count == 0)
            {
                EditorCommon.MessageBox.Show("请先选择文件夹后再进行操作！");
                return;
            }
            if (mFolderDatas.Count > 1)
            {
                EditorCommon.MessageBox.Show("当前显示的内容在多个文件夹中，无法确定操作目录，请选择一个目录后再进行操作！");
                return;
            }
            var firstFolderData = mFolderDatas[0];

            var layer = AdornerLayer.GetAdornerLayer(ListBox_Resources);
            layer.Remove(mListBoxDropAdorner);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (datas == null)
                    return;
                if (datas.Length == 0)
                    return;

                if (datas.Length > 0)
                {
                    if (CheckFileDropAvailable(datas))
                    {
                        var noUse = CheckImportMode(datas, firstFolderData);
                    }
                    if (CheckFileImprotAvailable(datas))
                    {
                        var importWin = new Import.ResourceImportControl(HostControl, datas, firstFolderData);
                        //importWin.CurrentFolderData = firstFolderData;
                        importWin.Show();
                    }
                }

            }
            else if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                var formats = e.Data.GetFormats();
                if (formats == null || formats.Length == 0)
                    return;

                var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                if (datas == null)
                    return;

                foreach (var data in datas)
                {
                    var dragedItem = data as EditorCommon.Resources.ResourceInfo;
                    // 同目录不需要移动
                    if (string.Equals(dragedItem.AbsPath.TrimEnd('/'), firstFolderData.AbsFolder.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (await dragedItem.MoveToFolder(firstFolderData.AbsFolder))
                    {
                        if (dragedItem.ParentBrowser != null)
                            dragedItem.ParentBrowser.RemoveResourceInfo(dragedItem);
                        else
                            dragedItem.ParentBrowser = HostControl;
                        CurrentResources.Add(dragedItem);
                    }
                    else
                    {
                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "ImportResource", $"资源浏览器:移动资源{dragedItem.ResourceName}失败");
                    }
                }
            }
            else if (SceneNodeDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                e.Handled = true;
                var formats = e.Data.GetFormats();
                var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                if (datas == null)
                    return;

                List<EngineNS.GamePlay.Actor.GActor> actors = new List<EngineNS.GamePlay.Actor.GActor>();
                foreach (var data in datas)
                {
                    var ad = data as EditorCommon.Controls.SceneNodes.ActorData;
                    if (ad != null)
                    {
                        actors.Add(ad.Actor);
                    }
                    var actorItem = data as EditorCommon.Controls.Outliner.ActorOutlinerItem;
                    if (actorItem != null)
                    {
                        actors.Add(actorItem.Actor);
                    }
                    var prefabItem = data as EditorCommon.Controls.Outliner.PrefabOutlinerItem;
                    if (prefabItem != null)
                    {
                        actors.Add(prefabItem.Prefab);
                    }
                }

                if (actors.Count > 0)
                {
                    var win = new CreateResDialog();
                    win.Title = "创建Prefab";

                    win.ResourceName = "";

                    if (win.ShowDialog((value, cultureInfo) =>
                    {
                        if (value == null)
                            return new ValidationResult(false, "内容不合法");
                        return new ValidationResult(true, value.ToString());
                    }) == false)
                        return;

                    var curPath = System.IO.Directory.GetCurrentDirectory();

                    var prefab = await EngineNS.GamePlay.Actor.GPrefab.CreatePrefab(actors);
                    String prefabname;
                    if (firstFolderData.AbsFolder != null)
                        prefabname = firstFolderData.AbsFolder.Replace(firstFolderData.RootFolder.ToLower(), "") + "/" + win.ResourceName + ".prefab";
                    else
                        prefabname = "/" + win.ResourceName + ".prefab";
                    var rName = EngineNS.RName.GetRName(prefabname);
                    prefab.SpecialName = win.ResourceName;
                    prefab.SavePrefab(rName);

                    var prefabinfo = await CreateResourceInfoFromResource(rName.Address) as ResourceInfos.PrefabResourceInfo;
                    if (prefabinfo != null)
                    {
                        await prefabinfo.Save();
                        CurrentResources.Add(prefabinfo);
                    }

                }
            }
        }

        private void ListBox_Resources_DragLeave(object sender, DragEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(ListBox_Resources);
            layer.Remove(mListBoxDropAdorner);
        }

        public void SelectItem(EngineNS.RName resName)
        {
            foreach (var resInfo in CurrentResources)
            {
                if (resInfo.ResourceName == resName)
                {
                    ListBox_Resources.SelectedItem = resInfo;
                    mResourceWrapPanel.ScrollToIndex(ListBox_Resources.SelectedIndex);
                    break;
                    //var searchObj = ListBox_Resources.ItemContainerGenerator.ContainerFromItem(resInfo) as ListBoxItem;
                    //if(searchObj != null)
                    //{
                    //    searchObj.BringIntoView();
                    //    searchObj.IsSelected = true;
                    //}
                }
            }
        }

        public async System.Threading.Tasks.Task CheckImportMode(string[] fileNames, ShowSourcesInDirData.FolderData folderData)
        {
            //分析是否存在vms //如果存在查看是否有配置文件
            // 导入的文件检测非法文件名，有则停止导入
            foreach (var fileName in fileNames)
            {
                var pureFileName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(fileName);
                var fn = pureFileName.Replace("." + EngineNS.CEngine.Instance.FileManager.GetFileExtension(fileName), "");
                if (System.Text.RegularExpressions.Regex.IsMatch(fn, @"[\u4e00-\u9fa5]"))
                {
                    EditorCommon.MessageBox.Show("导入失败：" + pureFileName + ", 为保证系统兼容性，文件名称中不能包含中文");
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(fn, "^[a-zA-Z0-9_]*$"))
                {
                    EditorCommon.MessageBox.Show("导入失败：" + pureFileName + ", 文件名称包含非法字符，文件名只能包含字母数字和下划线");
                    return;
                }
            }

            List<string> ConfigFiles = new List<string>(); // .importconfig
            List<string> CommonFiles = new List<string>();
            List<string> orginFiles = new List<string>();
            List<string> packageFiles = new List<string>();
            int haveConfigVmsCount = 0;
            foreach (var fileName in fileNames)
            {
                var fileInfo = new System.IO.FileInfo(fileName);
                if (!fileInfo.Directory.FullName.Replace("/", "\\").Contains(folderData.RootFolder))
                {
                    var ext = fileInfo.Extension.ToLower();
                    if(ext == ".vms")
                    {
                        if (!orginFiles.Contains(fileName))
                            orginFiles.Add(fileName);
                        string configFileName = fileName + ".ImportConfig";
                        var configFileInfo = new System.IO.FileInfo(configFileName);
                        if (configFileInfo.Exists)
                        {
                            if (!ConfigFiles.Contains(configFileName))
                            {
                                ConfigFiles.Add(configFileName);
                                haveConfigVmsCount++;
                            }
                        }
                        else
                        {
                            if (!CommonFiles.Contains(fileName))
                                CommonFiles.Add(fileName);
                        }
                    }
                    else if(ext == ".importconfig")
                    {
                        //orginFiles.Add(fileName); //importconfig文件不做原始导入
                        if (!ConfigFiles.Contains(fileName))
                            ConfigFiles.Add(fileName);
                    }
                    else if(ext == "." + EditorCommon.Program.PackageExtension)
                    {
                        var ap = new EditorCommon.Assets.AssetsPakage();
                        var reses = await ap.UnPackAssets(fileName, folderData.AbsFolder);
                        foreach (var res in reses)
                        {
                            if(res.ResourceName.GetDirectory().ToLower().TrimEnd('/') == folderData.AbsFolder.ToLower().TrimEnd('/'))
                                CurrentResources.Add(res);
                        }
                        var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(folderData.AbsFolder);
                        foreach(var dir in dirs)
                        {
                            var dirRes = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(dir);
                            CurrentResources.Add(dirRes);
                        }
                    }
                    else
                    {
                        if (!orginFiles.Contains(fileName))
                            orginFiles.Add(fileName);
                        if (!CommonFiles.Contains(fileName))
                            CommonFiles.Add(fileName);
                    }
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "ImportResource", $"不能导入已在资源文件夹内的文件({fileInfo.Name})");
                }
            }
            if (ConfigFiles.Count > 0)
            {
                string outPutstring = "导入的文件存在配置文件，是否打开导入配置窗口？\n选择否将跳过批量导入";
                if (haveConfigVmsCount != 0)
                {
                    outPutstring = "导入的模型存在配置文件，可选操作？\n'是'打开导入配置窗口，进行相关资源的自动生成。\n'否'只导入拖动文件。";
                }
                //有配置文件打开窗口询问是否打开配置窗口
                var result = EditorCommon.MessageBox.Show(outPutstring, "敬告", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                if (result == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                {
                    //然后在打开批量导入生成窗口
                    var datas = EditorCommon.Resources.ResourceInfoManager.Instance.GetAllResourceInfoDatas();
                    ImportResource importWindow = new ImportResource(HostControl, datas);
                    importWindow.CurrentAbsFolder = folderData.AbsFolder;
                    importWindow.ConfigFileList = ConfigFiles;
                    await importWindow.StartLoadResource();
                    importWindow.Show();
                    //如果不是vms 就正常导入
                    if (CommonFiles.Count > 0)
                        await ImportResources(CommonFiles.ToArray(), folderData);

                }
                if (result == EditorCommon.MessageBox.enMessageBoxResult.No)
                {
                    if (orginFiles.Count > 0)
                        await ImportResources(orginFiles.ToArray(), folderData);
                }
                if (result == EditorCommon.MessageBox.enMessageBoxResult.Cancel)
                {

                }
            }
            else
            {
                if (orginFiles.Count > 0)
                    await ImportResources(orginFiles.ToArray(), folderData);
            }
        }
        float mProcessPercent = 0;
        public float ProcessPercent
        {
            get { return mProcessPercent; }
            set
            {
                mProcessPercent = value;
                OnPropertyChanged("ProcessPercent");
            }
        }
        string mProcessingInfo = "";
        public string ProcessingInfo
        {
            get { return mProcessingInfo; }
            set
            {
                mProcessingInfo = value;
                OnPropertyChanged("ProcessingInfo");
            }
        }
        public async System.Threading.Tasks.Task ImportResources(string[] resourceFiles, ShowSourcesInDirData.FolderData folderData)
        {
            if (resourceFiles == null)
                return;

            if (folderData == null || string.IsNullOrEmpty(folderData.AbsFolder))
            {
                EditorCommon.MessageBox.Show("请先选择一个文件夹后再进行导入操作!");
                return;
            }

            ProcessPercent = 0;
            ProcessingInfo = "正在导入资源...";

            var count = resourceFiles.Length;
            var delta = 1.0f / count;
            var messageBoxResult = EditorCommon.MessageBox.enMessageBoxResult.None;
            // 这里因为要弹一个MessageBox来让用户设置能否全部操作，所以不能用ASyncSemaphore
            foreach (var file in resourceFiles)
            {
                var fileInfo = new System.IO.FileInfo(file);
                if (fileInfo.Directory.FullName.Replace("/", "\\").Contains(folderData.RootFolder))
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "ImportResource", $"不能导入已在资源文件夹内的文件({fileInfo.Name})");
                    return;
                }

                var tagFile = (folderData.AbsFolder + "/" + fileInfo.Name).ToLower();
                var resInfo = await CreateResourceInfoFromResource(tagFile);
                if (resInfo == null)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "ImportResource", $"导入文件{file}失败，目标类型不支持");
                    return;
                }

                if (resInfo.IsExists(tagFile))
                {
                    switch (messageBoxResult)
                    {
                        case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
                            await resInfo.DoImport(file, tagFile, true);
                            break;
                        case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
                            return;
                        default:
                            {
                                messageBoxResult = EditorCommon.MessageBox.Show("文件" + fileInfo.Name + "已存在，是否覆盖", "警告", EditorCommon.MessageBox.enMessageBoxButton.Yes_YesAll_No_NoAll);
                            }
                            break;
                    }
                }
                switch (messageBoxResult)
                {
                    case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                        await resInfo.DoImport(file, tagFile, true);
                        break;
                    case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
                        await resInfo.DoImport(file, tagFile, true);
                        break;
                    case EditorCommon.MessageBox.enMessageBoxResult.No:
                    case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
                    case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                        return;
                    default:
                        await resInfo.DoImport(file, tagFile, false);
                        break;
                }

                await resInfo.Save();
                ProcessPercent += delta;
            }
            // 完成之后
            var showData = new ShowSourcesInDirData()
            {
                ResetHistory = true,
                ForceRefresh = true,
            };
            showData.FolderDatas.Add(folderData);
            await HostControl.ShowSourcesInDir(showData);
        }


        #region ViewOptions

        public bool ShowEngineContent
        {
            get { return (bool)GetValue(ShowEngineContentProperty); }
            set { SetValue(ShowEngineContentProperty, value); }
        }
        public static readonly DependencyProperty ShowEngineContentProperty = DependencyProperty.Register("ShowEngineContent", typeof(bool), typeof(ContentControl), new FrameworkPropertyMetadata(false));
        public bool ShowEditorContent
        {
            get { return (bool)GetValue(ShowEditorContentProperty); }
            set { SetValue(ShowEditorContentProperty, value); }
        }
        public static readonly DependencyProperty ShowEditorContentProperty = DependencyProperty.Register("ShowEditorContent", typeof(bool), typeof(ContentControl), new FrameworkPropertyMetadata(false));

        public async System.Threading.Tasks.Task<bool> ReCreateSnapshot(EditorCommon.Resources.ResourceInfo info)
        {
            var imgs = await info.GetSnapshotImage(true);
            info.Snapshot = imgs[0];
            return true;
        }
        void RefreshBrowserSnapshot(string infoFile, bool reCreate)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            {
                var resInfo = await CreateResourceInfo(infoFile);
                var imgs = await resInfo.GetSnapshotImage(reCreate);
                resInfo.Snapshot = imgs[0];
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }
        private void RadioButton_List_Checked(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedObject = null;
            if (ListBox_Resources.SelectedItems.Count > 0)
            {
                selectedObject = ListBox_Resources.ItemContainerGenerator.ContainerFromItem(ListBox_Resources.SelectedItems[0]) as ListBoxItem;
            }

            Grid_ListTitle.Visibility = Visibility.Visible;
            //ListBox_Resources.ItemsPanel = this.TryFindResource("StackPanelTemplate") as ItemsPanelTemplate;
            ListBox_Resources.ItemTemplate = this.TryFindResource("ResourceInfoDataTemplate_List") as DataTemplate;
            mResourceWrapPanel = VisualTreeChildSearch<VirtualizingWrapPanel>(ListBox_Resources) as VirtualizingWrapPanel;
            mResourceWrapPanel.WrapType = VirtualizingWrapPanel.enWrapType.List;
            mResourceWrapPanel.ChildHeight = 30;
            if (ListBox_Resources.SelectedIndex < 0)
            {
                mResourceWrapPanel.SetVerticalOffset(0);
            }
            else
            {
                mResourceWrapPanel.ScrollToIndex(ListBox_Resources.SelectedIndex);
            }
            IconScaleEnable = false;

            if (selectedObject != null)
                selectedObject.BringIntoView();
        }
        static DependencyObject VisualTreeChildSearch<T>(DependencyObject source)
        {
            if (source == null || source.GetType() == typeof(T))
                return source;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++)
            {
                var child = VisualTreeHelper.GetChild(source, i);
                var result = VisualTreeChildSearch<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }
        private void RadioButton_Tile_Checked(object sender, RoutedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(ListBox_Resources) > 0)
            {
                ListBoxItem selectedObject = null;
                if (ListBox_Resources.SelectedItems.Count > 0)
                {
                    selectedObject = ListBox_Resources.ItemContainerGenerator.ContainerFromItem(ListBox_Resources.SelectedItems[0]) as ListBoxItem;
                }

                Grid_ListTitle.Visibility = Visibility.Collapsed;
                //ListBox_Resources.ItemsPanel = this.TryFindResource("WrapPanelTemplate") as ItemsPanelTemplate;
                ListBox_Resources.ItemTemplate = this.TryFindResource("ResourceInfoDataTemplate_Wrap") as DataTemplate;
                mResourceWrapPanel = VisualTreeChildSearch<VirtualizingWrapPanel>(ListBox_Resources) as VirtualizingWrapPanel;
                mResourceWrapPanel.ChildWidth = mChildWidth * IconScale;
                mResourceWrapPanel.ChildHeight = mChildHeight * IconScale;
                mResourceWrapPanel.WrapType = VirtualizingWrapPanel.enWrapType.Tile;
                if (ListBox_Resources.SelectedIndex < 0)
                {
                    mResourceWrapPanel.SetVerticalOffset(0);
                }
                else
                {
                    mResourceWrapPanel.ScrollToIndex(ListBox_Resources.SelectedIndex);
                }
                IconScaleEnable = true;

                if (selectedObject != null)
                    selectedObject.BringIntoView();
            }
        }
        #endregion

        #region 排序
        SortDescription mCurrentSortDescription = new SortDescription("ResourceName", ListSortDirection.Ascending);
        private void Sort(params string[] sortDesc)
        {
            ListBox_Resources.Items.SortDescriptions.Clear();
            if (sortDesc == null)
                ListBox_Resources.Items.SortDescriptions.Add(mCurrentSortDescription);
            else
            {
                foreach (var desc in sortDesc)
                {
                    var sortDescription = new SortDescription(desc, ListSortDirection.Ascending);
                    ListBox_Resources.Items.SortDescriptions.Add(sortDescription);
                }
            }
        }
        private void StackPanel_ListType_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListSortDirection dir = ListSortDirection.Ascending;
            if (mCurrentSortDescription != null && mCurrentSortDescription.PropertyName.Equals("ResourceTypeName"))
            {
                switch (mCurrentSortDescription.Direction)
                {
                    case ListSortDirection.Ascending:
                        dir = ListSortDirection.Descending;
                        TypePathScale.ScaleY = -1;
                        break;
                    case ListSortDirection.Descending:
                        dir = ListSortDirection.Ascending;
                        TypePathScale.ScaleY = 1;
                        break;
                }
            }
            mCurrentSortDescription = new SortDescription("ResourceTypeName", dir);
            Path_ListName.Visibility = Visibility.Hidden;
            Path_ListType.Visibility = Visibility.Visible;
            Sort(null);
        }
        private void StackPanel_ListName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListSortDirection dir = ListSortDirection.Ascending;
            if (mCurrentSortDescription != null && mCurrentSortDescription.PropertyName.Equals("ResourceTypeName"))
            {
                switch (mCurrentSortDescription.Direction)
                {
                    case ListSortDirection.Ascending:
                        dir = ListSortDirection.Descending;
                        TypePathScale.ScaleY = -1;
                        break;
                    case ListSortDirection.Descending:
                        dir = ListSortDirection.Ascending;
                        TypePathScale.ScaleY = 1;
                        break;
                }
            }
            mCurrentSortDescription = new SortDescription("ResourceTypeName", dir);
            Path_ListName.Visibility = Visibility.Hidden;
            Path_ListType.Visibility = Visibility.Visible;
            Sort(null);
        }

        #endregion

        #region ItemOperation
        public bool CheckFileDropAvailable(string[] files)
        {
            if (EditorCommon.Resources.ResourceInfoManager.Instance.CheckFileExtAvaliable(files) == true)
                return true;

            foreach(var file in files)
            {
                var fileExt = EngineNS.CEngine.Instance.FileManager.GetFileExtension(file);
                if(string.Equals(fileExt, EditorCommon.Program.PackageExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        public bool CheckFileImprotAvailable(string[] files)
        {
            //var resInfoArray = mResourceInfoStrDic.ToArray();
            foreach (var file in files)
            {
                var fileExt = "." + EngineNS.CEngine.Instance.FileManager.GetFileExtension(file);
                if (!FBX.CGfxFBXManager.Instance.IsSupportFileFormat(fileExt))
                    return false;
            }
            return true;
        }
        private bool IsMouseInSelectedResourceItem()
        {
            foreach (var item in ListBox_Resources.SelectedItems)
            {
                var listBoxItem = ListBox_Resources.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (listBoxItem == null)
                    continue;

                var pos = Mouse.GetPosition(listBoxItem);
                if (pos.X >= 0 && pos.X < listBoxItem.ActualWidth &&
                    pos.Y >= 0 && pos.Y < listBoxItem.ActualHeight)
                {
                    return true;
                }
            }

            return false;
        }

        bool mStartItemDrag = false;
        Point mStartDragPoint = new Point();

        void ListBox_ResourceItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            if (item != null)
            {
                var info = item.DataContext as EditorCommon.Resources.IResourceInfoEditor;
                info?.OpenEditor();
            }
        }
        private void ListBox_ResourceItem_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mStartDragPoint = e.GetPosition(ListBox_Resources);
                if (ListBox_Resources.SelectedItems.Count > 1 && IsMouseInSelectedResourceItem() &&
                    !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) &&
                    !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    e.Handled = true;
                }
            }
        }
        private void ListBox_ResourceItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pt = e.GetPosition(ListBox_Resources);
                if (Math.Abs(pt.X - mStartDragPoint.X) > 5 || Math.Abs(pt.Y - mStartDragPoint.Y) > 5)
                {
                    mStartItemDrag = true;
                    var resItems = new EditorCommon.DragDrop.IDragAbleObject[ListBox_Resources.SelectedItems.Count];
                    for (int i = 0; i < ListBox_Resources.SelectedItems.Count; i++)
                    {
                        resItems[i] = ListBox_Resources.SelectedItems[i] as EditorCommon.DragDrop.IDragAbleObject;
                    }
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(EditorCommon.Program.ResourcItemDragType, resItems);

                    mStartItemDrag = false;
                }
            }
        }
        private void ListBox_ResourceItem_MouseUp(object sender, MouseEventArgs e)
        {
            var item = sender as ListBoxItem;
            if (mStartItemDrag == true)
            {
                var events = EventManager.GetRoutedEvents();
                foreach (var evt in events)
                {
                    if (evt.Name == "MouseDown")
                    {
                        var eg = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
                        eg.RoutedEvent = evt;
                        item.RaiseEvent(eg);
                        break;
                    }
                }
            }
            mStartItemDrag = false;
        }
        private void ListBox_ResourceItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var item = sender as ListBoxItem;
            if (item == null)
                return;

            var info = item.DataContext as EditorCommon.Resources.ResourceInfo;
            info.UpdateVersionControlContextMenu();
            info.UpdateToolTip();
        }
        private void ListBox_ResourceItem_MouseDown(object sender, MouseEventArgs e)
        {
        }
        EditorCommon.DragDrop.DropAdorner mListBoxResourceItemDropAdorner;
        private void ListBox_ResourceItem_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var item = sender as FrameworkElement;

            mListBoxResourceItemDropAdorner = new EditorCommon.DragDrop.DropAdorner(item);
            var data = item.DataContext as EditorCommon.Resources.IResourceInfoDragDrop;
            if (data == null || data.DragEnter(e) == false)
                mListBoxResourceItemDropAdorner.IsAllowDrop = false;
            else
                mListBoxResourceItemDropAdorner.IsAllowDrop = true;

            var pos = e.GetPosition(item);
            if (pos.X > 0 && pos.X < item.ActualWidth &&
               pos.Y > 0 && pos.Y < item.ActualHeight)
            {
                var layer = AdornerLayer.GetAdornerLayer(this);
                layer.Add(mListBoxResourceItemDropAdorner);
            }
        }
        private void ListBox_ResourceItem_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var item = sender as FrameworkElement;
            var data = item.DataContext as EditorCommon.Resources.IResourceInfoDragDrop;
            if (data != null)
                data.DragLeave(e);

            if (mListBoxResourceItemDropAdorner != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(this);
                layer.Remove(mListBoxResourceItemDropAdorner);
            }
        }
        private void ListBox_ResourceItem_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var item = sender as FrameworkElement;
            var data = item.DataContext as EditorCommon.Resources.IResourceInfoDragDrop;
            if (data != null)
                data.DragOver(e);
        }
        private void ListBox_ResourceItem_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var item = sender as FrameworkElement;
            var data = item.DataContext as EditorCommon.Resources.IResourceInfoDragDrop;
            if (data != null)
                data.Drop(e);

            if (mListBoxResourceItemDropAdorner != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(this);
                layer.Remove(mListBoxResourceItemDropAdorner);
            }
        }

        #endregion
    }
}

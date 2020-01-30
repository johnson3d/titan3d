using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EditorCommon.Resources
{
    public class ResourceFolderContextMenuItem
    {
        public string Header;
        public Action ClickAction;
    }

    public interface IFolderItem
    {
        EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }
        string PathName { get; }
        void AddSubFolderItem(IFolderItem item);
    }
    /// <summary>
    /// 创建Component
    /// </summary>
    public interface IResourceInfoCreateComponent
    {
        Task<GComponent> CreateComponent(IComponentContainer componentContainer);
    }
    /// <summary>
    /// 创建Component
    /// </summary>
    public interface IResourceInfoCreateActor
    {
        Task<GActor> CreateActor();
    }
    /// <summary>
    /// 处理资源引用关系
    /// </summary>
    [Obsolete("不再使用，资源引用处理放入ResourceInfo基类中", false)]
    public interface IResourceReference
    {
        // 获取引用此资源的资源
        List<ResourceInfo> GetReferences();
        // 是否引用info资源
        bool IsDependencyWith(ResourceInfo info);
        // 当引用的info资源更改时调用
        void ChangeDependency(ResourceInfo info, object src, object tag);
    }

    /// <summary>
    /// 资源浏览器文件夹右键菜单
    /// </summary>
    public interface IResourceFolderContextMenu
    {
        System.Collections.Generic.List<ResourceFolderContextMenuItem> GetMenuItems(IFolderItem item);
    }
    /// <summary>
    /// 可以强制重新加载的资源
    /// </summary>
    public interface IResourceInfoForceReload
    {
        void ForceReload();
        // 获取需要文件监控的扩展名（带.）
        string[] GetFileSystemWatcherAttentionExtensions();
    }

    /// <summary>
    /// 可以拖动到游戏窗口的资源
    /// </summary>
    public interface IResourceInfoDragToViewport : EditorCommon.DragDrop.IDragToViewport
    {
        //System.Threading.Tasks.Task OnDragEnterViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e);
        //System.Threading.Tasks.Task OnDragLeaveViewport(ViewPort.ViewPortControl viewport, EventArgs e);
        //System.Threading.Tasks.Task OnDragOverViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e);
        //System.Threading.Tasks.Task OnDragDropViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e);
    }
    
    public interface ICustomCreateDialog
    {
        string ResourceName { get; set; }
        IResourceCreateData GetCreateData();
        bool? ShowDialog(InputWindow.Delegate_ValidateCheck onCheck);
        EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }
    }
    /// <summary>
    /// 定制创建对话框
    /// </summary>
    public interface IResourceInfoCustomCreateDialog
    {
        ICustomCreateDialog GetCustomCreateDialogWindow();
    }

    public interface IResourceCreateData
    {
        [Browsable(false)]
        string ResourceName { get; set; }
        [Browsable(false)]
        ICustomCreateDialog HostDialog { get; set; }
        [Browsable(false)]
        EngineNS.RName.enRNameType RNameType { get; set; }
        [Browsable(false)]
        string Description { get; set; }
    }

    public class ResourceCreateDataBase : IResourceCreateData
    {
        public string ResourceName { get; set; }
        public ICustomCreateDialog HostDialog { get; set; }
        public EngineNS.RName.enRNameType RNameType { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 创建空资源
    /// </summary>
    public interface IResourceInfoCreateEmpty
    {
        /// <summary>
        /// 获取资源的合法名称
        /// </summary>
        /// <returns></returns>
        string GetValidName(string absFolder);
        /// <summary>
        /// 判断资源名称合法性
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <returns></returns>
        ValidationResult ResourceNameAvailable(string absFolder, string name);
        /// <summary>
        /// 获取创建用的参数
        /// </summary>
        /// <returns></returns>
        IResourceCreateData GetResourceCreateData(string absFolder);
        /// <summary>
        /// 创建空白资源
        /// </summary>
        /// <param name="Absfolder">新资源所在的路径</param>
        /// <param name="resourceName">资源名称</param>
        /// <returns></returns>
        System.Threading.Tasks.Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData);
        /// <summary>
        /// 创建目录路径，格式为(目录名称/目录名称)，不能为空
        /// </summary>
        string CreateMenuPath { get; }
        /// <summary>
        /// 是否为基础资源，将显示在创建菜单的基础资源项里
        /// </summary>
        bool IsBaseResource { get; }
    }

    public interface IResourceInfoPreviewForEditor
    {
        [EngineNS.Rtti.MetaData]
        string SkeletonAsset
        {
            get; set;
        }
        [EngineNS.Rtti.MetaData]

        string PreViewMesh
        {
            get; set;
        }
    }
    public interface IResourceInfoEditor
    {
        string EditorTypeName { get; }
        /// <summary>
        /// 打开编辑器
        /// </summary>
        System.Threading.Tasks.Task OpenEditor();
    }
    /// <summary>
    /// 资源可以被作为拖放容器
    /// </summary>
    public interface IResourceInfoDragDrop
    {
        /// <summary>
        /// 当对象被拖入正作为放置目标元素边界时调用
        /// </summary>
        /// <param name="e">拖放参数</param>
        /// <returns>可以拖放返回true，不能拖放返回false</returns>
        bool DragEnter(System.Windows.DragEventArgs e);
        /// <summary>
        /// 当对象被拖出正作为没有放置的放置目标的元素边界时发生
        /// </summary>
        /// <param name="e">拖放参数</param>
        void DragLeave(System.Windows.DragEventArgs e);
        /// <summary>
        /// 当正作为放置目标的元素边界内的拖动对象时持续发生
        /// </summary>
        /// <param name="e">拖放参数</param>
        void DragOver(System.Windows.DragEventArgs e);
        /// <summary>
        /// 当对象被拖入正作为放置目标的元素边界时发生
        /// </summary>
        /// <param name="e">拖放参数</param>
        void Drop(System.Windows.DragEventArgs e);
    }

    // 使用插件来实现不同资源的处理
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResourceInfoAttribute : Attribute
    {
        public ResourceInfoAttribute()
        {

        }

        /// <summary>
        /// 资源类型
        /// </summary>
        public string ResourceInfoType { get; set; }
        /// <summary>
        /// 资源扩展名
        /// </summary>
        public string[] ResourceExts { get; set; }
    }

    public class ResourceInfoMetaData
    {
        public string ResourceInfoTypeStr { get; set; }
        public string[] ResourceExts { get; set; }
        public Type ResourceInfoType { get; set; }
        ResourceInfo mResInfo = null;
        public ResourceInfo ResInfo
        {
            get
            {
                if (mResInfo == null && ResourceInfoType != null)
                {
                    mResInfo = System.Activator.CreateInstance(ResourceInfoType) as ResourceInfo;
                }

                return mResInfo;
            }
        }
    }

    public interface IResourceTechInfoImportCreate
    {
        //材质实例生成返回 生成实例的GUID
        ResourceInfo CreateResourceFormImportFile(EngineNS.RName hostMaterialResName, string absFolder, string resourceName, System.Collections.Generic.Dictionary<string, string> materials);

    }
    public interface IResourceMtlInfoImportCreate
    {
        System.Threading.Tasks.Task<EditorCommon.Resources.ResourceInfo> CreateResourceFormImportFile(EngineNS.RName hostMaterialResName, string absFolder, string resourceName, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Guid>> materials);
    }

    public interface IResourceMeshInfoImportCreate
    {
        ResourceInfo CreateResourceFromImportFile(string resourceFile, EngineNS.Vector3 position);
        EngineNS.Vector3 GetPositionInSceneFromResourceInfo(string absFileName);
    }

    /// <summary>
    /// 资源提示窗口标记
    /// </summary>
    public class ResourceToolTipAttribute : Attribute
    {
        public string ToolTipColor = Colors.White.ToString();
        public ResourceToolTipAttribute()
        {

        }
        public ResourceToolTipAttribute(string color)
        {
            ToolTipColor = color;
        }
    }

    #region ResourceInfo
    /// <summary>
    /// 资源信息基类
    /// </summary>
    public abstract class ResourceInfo : DependencyObject, INotifyPropertyChanged, EditorCommon.DragDrop.IDragAbleObject, EngineNS.IO.Serializer.ISerializer
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        
        string mResourceType = "";
        [EngineNS.Rtti.MetaData]
        public virtual string ResourceType
        {
            get { return mResourceType; }
            protected set
            {
                mResourceType = value;
                OnPropertyChanged("ResourceType");
            }
        }

        [EngineNS.Rtti.MetaData]
        public List<EngineNS.RName> RNameHistory
        {
            get;
            set;
        } = new List<EngineNS.RName>();
        [EngineNS.Rtti.MetaData]
        public List<EngineNS.RName> ReferenceRNameList
        {
            get;
            set;
        } = new List<EngineNS.RName>();
        /// <summary>
        /// 资源类型名称
        /// </summary>
        [ResourceToolTipAttribute]
        [DisplayName("类型")]
        public abstract string ResourceTypeName { get; }
        /// <summary>
        /// 资源类型笔刷（用不同颜色标识不同资源）
        /// </summary>
        public abstract Brush ResourceTypeBrush { get; }
        /// <summary>
        /// 资源图标
        /// </summary>
        public abstract ImageSource ResourceIcon { get; }
        [ResourceToolTipAttribute]
        [DisplayName("路径")]
        public string Path
        {
            get
            {
                if(ResourceName != null)
                    return ResourceName.Name;
                return "";
            }
        }
        [ResourceToolTipAttribute]
        [DisplayName("路径类型")]
        public EngineNS.RName.enRNameType RNameType
        {
            get
            {
                if (ResourceName != null)
                    return ResourceName.RNameType;
                return EngineNS.RName.enRNameType.Game;
            }
        }

        /// <summary>
        /// 筛选操作
        /// </summary>
        /// <param name="filterString">筛选关键字</param>
        /// <returns></returns>
        public virtual bool DoFilter(string filterString)
        {
            if (ResourceName.PureName().Contains(filterString.ToLower()))
                return true;
            return false;
        }

        /// <summary>
        /// 获取缩略图
        /// </summary>
        /// <param name="forceCreate">是否强制创建</param>
        /// <returns></returns>
#pragma warning disable 1998
        public virtual async System.Threading.Tasks.Task<ImageSource[]> GetSnapshotImage(bool forceCreate)
        {
            return null;
        }
#pragma warning restore 1998

        public delegate void Delegate_OnDirtyChanged(bool dirty);
        public event Delegate_OnDirtyChanged OnDirtyChanged;

        public static string ExtString
        {
            get;
        } = Program.ResourceInfoExt;

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }
        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(ResourceInfo), new PropertyMetadata(false, new PropertyChangedCallback(IsDirtyChangedCallback)));
        static void IsDirtyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var info = sender as ResourceInfo;
            bool newValue = (bool)e.NewValue;
            if (newValue)
                info.UnsaveVisibility = Visibility.Visible;
            else
                info.UnsaveVisibility = Visibility.Collapsed;

            info.OnDirtyChanged?.Invoke(newValue);
        }

        public virtual void SetSelectedObjectData()
        {
            EditorCommon.PluginAssist.PropertyGridAssist.SetSelectedResourceInfo(ResourceType, this);
        }

        public virtual bool IsExists(string file)
        {
            return System.IO.File.Exists(file);
        }
        public virtual async Task DoImport(string file, string target, bool overwrite = false)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            System.IO.File.Copy(file, target, overwrite);
        }

        //bool mIsDirty = false;
        //public bool IsDirty
        //{
        //    get { return mIsDirty; }
        //    set
        //    {
        //        mIsDirty = value;

        //        if (mIsDirty)
        //            UnsaveVisibility = Visibility.Visible;
        //        else
        //            UnsaveVisibility = Visibility.Collapsed;

        //        OnDirtyChanged?.Invoke(mIsDirty);
        //        OnPropertyChanged("IsDirty");
        //    }
        //}
        ImageSource mSnapshot = null;
        public ImageSource Snapshot
        {
            get { return mSnapshot; }
            set
            {
                mSnapshot = value;
                if (mSnapshot != null)
                    StopWaitingProcess();
                OnPropertyChanged("Snapshot");
            }
        }

        Visibility mUnsaveVisibility = Visibility.Collapsed;
        public Visibility UnsaveVisibility
        {
            get { return mUnsaveVisibility; }
            set
            {
                mUnsaveVisibility = value;
                OnPropertyChanged("UnsaveVisibility");
            }
        }

        bool mNotUse = false;
        public bool NotUse
        {
            get => mNotUse;
            set
            {
                mNotUse = value;
                OnPropertyChanged("NotUse");
            }
        }

        Visibility mWaitingProcessVisibility = Visibility.Visible;
        public Visibility WaitingProcessVisibility
        {
            get { return mWaitingProcessVisibility; }
            set
            {
                mWaitingProcessVisibility = value;
                OnPropertyChanged("WaitingProcessVisibility");
            }
        }

        public EditorCommon.Controls.ResourceBrowser.IContentControlHost ParentBrowser;
        EngineNS.RName mResourceName;
        /// <summary>
        /// ResourceInfo文件全名称（包含完整路径）
        /// </summary>
        public virtual EngineNS.RName ResourceName
        {
            get => mResourceName;
            protected set
            {
                mResourceName = value;
                OnPropertyChanged("ResourceName");
            }
            //{
            //    mAbsInfoFileName = value.Replace("\\", "/");
            //    AbsResourceFileName = mAbsInfoFileName.Replace(Program.ResourceInfoExt, "");
            //    RelativeResourceFileName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(AbsResourceFileName);
            //    ResourceFileName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(AbsResourceFileName);
            //    RelativeInfoFileName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(mAbsInfoFileName);
            //    AbsPath = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(mAbsInfoFileName);
            //    RelativePath = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(AbsPath);
            //    FileExtension = EngineNS.CEngine.Instance.FileManager.GetFileExtension(AbsResourceFileName).Replace(".", "").ToLower();
            //}
        }

        string mHightlightString = "";
        [Browsable(false)]
        public string HightlightString
        {
            get => mHightlightString;
            set
            {
                mHightlightString = value;
                OnPropertyChanged("HightlightString");
            }
        }

        public string AbsPath
        {
            get
            {
                return EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(ResourceName.Address);
            }
        }
        public string AbsInfoFileName
        {
            get
            {
                return ResourceName.Address + Program.ResourceInfoExt;
            }
        }

        public async Task<bool> AssetsOption_LoadResource(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            return await AssetsOption_LoadResourceOverride(data);
        }
        public abstract Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data);
        public async Task<bool> AssetsOption_SaveResource(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            var tagAbs = data.GetTargetAbsFileName();

            var tagFile = data.GetTargetAbsFileName();
            var path = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(tagFile);
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(path))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(path);

            var srcFileName = data.GetSourceAbsFileName();
            if (await AssetsOption_SaveResourceOverride(data) == false)
                return false;

            // rinfo
            // 导包不包含RNameHistory
            var tempHistory = RNameHistory.ToArray();
            RNameHistory.Clear();
            await Save(tagAbs + EditorCommon.Program.ResourceInfoExt);
            RNameHistory = new List<EngineNS.RName>(tempHistory);
            // snapshot
            EngineNS.CEngine.Instance.FileManager.CopyFile(srcFileName + EditorCommon.Program.SnapshotExt, tagAbs + EditorCommon.Program.SnapshotExt, true);


            return true;
        }
        public abstract Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data);

        internal void StopWaitingProcess()
        {
            WaitingProcessVisibility = Visibility.Collapsed;
        }
        public async Task Save(string absInfoFileName, bool withSnapshot = false)
        {
            var saver = EngineNS.IO.XmlHolder.NewXMLHolder(this.GetType().FullName, "");
            WriteObjectXML(saver.RootNode);
            EngineNS.IO.XmlHolder.SaveXML(absInfoFileName, saver);

            if (withSnapshot)
            {
                await GetSnapshotImage(true);
            }
        }
        public virtual async Task Save(bool withSnapshot = false)
        {
            await Save(AbsInfoFileName, withSnapshot);
        }
        protected bool mIsLoading = false;
        public virtual async System.Threading.Tasks.Task<bool> AsyncLoad(string absFileName)
        {
            mIsLoading = true;
            var holder =  EngineNS.IO.XmlHolder.LoadXML(absFileName);
            if (holder == null)
            {
                mIsLoading = false;
                return false;
            }
            ReadObjectXML(holder.RootNode);
            absFileName = EngineNS.CEngine.Instance.FileManager.RemoveExtension(absFileName);
            ResourceName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(absFileName);

            // 有时需要直接读rinfo文件来做一些处理，不一定在界面中，所以这里如果ParentBorwser为空则不初始化菜单
            if(ParentBrowser != null)
                await InitializeContextMenu();
            //InitializeCustomIcons();
            //UpdateVersionControlStateShow();
            mIsLoading = false;
            return true;
        }

        public bool Load(string absFileName)
        {
            mIsLoading = true;
            var holder = EngineNS.IO.XmlHolder.LoadXML(absFileName);
            absFileName = EngineNS.CEngine.Instance.FileManager.RemoveExtension(absFileName);
            ResourceName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(absFileName);
            if (holder == null)
            {
                mIsLoading = false;
                return false;
            }
            ReadObjectXML(holder.RootNode);
            mIsLoading = false;
            return true;
        }

        /// <summary>
        /// 显示缩略图
        /// </summary>
        /// <param name="force">强制刷新</param>
        public void TryShowSnapshot(bool force)
        {
            if (Snapshot == null || force)
            {
                EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
                {
                    var imgs = await GetSnapshotImage(false);
                    if (imgs != null)
                    {
                        Snapshot = imgs[0];
                    }
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Main);
            }
        }

        /// <summary>
        /// 获取拖动的可视对象
        /// </summary>
        /// <returns></returns>
        public FrameworkElement GetDragVisual()
        {
            if (ParentBrowser != null)
            {
                return ParentBrowser.GetContainerFromItem(this);
            }

            return null;
        }
        /// <summary>
        /// 从导入资源创建资源信息文件
        /// </summary>
        /// <param name="resourceAbsFile">资源文件名</param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResource(string resourceAbsName)
        {
            var resourceName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(resourceAbsName);
            var retInfo = await CreateResourceInfoFromResourceOverride(resourceName);
            if (retInfo == null)
                return null;
            retInfo.ResourceName = resourceName;

            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //{
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Add((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{retInfo.ResourceTypeName}{retInfo.Name} {retInfo.AbsResourceFileName}使用版本控制添加失败!");
            //        }
            //    }, retInfo.AbsResourceFileName, $"AutoCommit {retInfo.ResourceTypeName}{retInfo.Name}");
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Add((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{ResourceTypeName}{Name} {retInfo.AbsInfoFileName}使用版本控制添加失败!");
            //        }
            //    }, retInfo.AbsInfoFileName, $"AutoCommit {ResourceTypeName}{Name}");
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Add((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{retInfo.ResourceTypeName}{retInfo.Name} {retInfo.AbsResourceFileName + Program.SnapshotExt}使用版本控制添加失败!");
            //        }
            //    }, retInfo.AbsResourceFileName + Program.SnapshotExt, $"AutoCommit {retInfo.ResourceTypeName}{retInfo.Name}缩略图");
            //}

            return retInfo;
        }
#pragma warning disable 1998
        protected virtual async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(EngineNS.RName resourceName) { throw new NotImplementedException(); }
#pragma warning restore 1998
        /// <summary>
        /// 删除资源
        /// </summary>
        public async System.Threading.Tasks.Task<bool> DeleteResource()
        {

            // 多选删除
            var resInfos = ParentBrowser.GetSelectedResourceInfos();
            if (resInfos == null)
            {
                return false;
            }

            // 判断是否有引用
            int refCount = 0;
            foreach(var res in resInfos)
            {
                var refResList = await EngineNS.CEngine.Instance.GameEditorInstance.GetWhoReferenceMe(res.ResourceName);
                foreach(var refRes in refResList)
                {
                    bool find = false;
                    foreach(var tempRes in resInfos)
                    {
                        if(tempRes.ResourceName == refRes)
                        {
                            // 引用对象包含在删除列表中
                            find = true;
                            break;
                        }
                    }
                    if(!find)
                        refCount += refResList.Count;
                }
            }
            if (refCount > 0)
            {
                if (EditorCommon.MessageBox.Show($"选中的资源被{refCount}个资源引用，是否继续删除？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.No)
                {
                    return false;
                }
            }

            EditorCommon.FileSystemWatcherProcess.Enable = false;

            foreach (var res in resInfos)
            {
                if (await res.DeleteResourceOverride() == false)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "资源删除", $"删除资源{res.ResourceName.Address}失败!");
                    continue;
                }

                //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
                //{
                //    EditorCommon.VersionControl.VersionControlManager.Instance.Update((EditorCommon.VersionControl.VersionControlCommandResult result) =>
                //    {
                //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
                //        {
                //            if (System.IO.File.Exists(AbsInfoFileName))
                //                System.IO.File.Delete(AbsInfoFileName);

                //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{ResourceTypeName}{Name} {AbsInfoFileName}使用版本控制删除失败!");
                //        }
                //        else
                //        {
                //            EditorCommon.VersionControl.VersionControlManager.Instance.Delete((EditorCommon.VersionControl.VersionControlCommandResult resultDelete) =>
                //            {
                //                if (resultDelete.Result != EditorCommon.VersionControl.EProcessResult.Success)
                //                {
                //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{ResourceTypeName}{Name} {AbsInfoFileName}使用版本控制删除失败!");
                //                }
                //            }, AbsInfoFileName, $"AutoCommit 删除{ResourceTypeName}{Name}");
                //        }
                //    }, AbsInfoFileName);
                //}
                //else
                {
                    var infoFile = res.ResourceName.Address + Program.ResourceInfoExt;
                    if (System.IO.File.Exists(infoFile))
                        System.IO.File.Delete(infoFile);
                    ResourceInfoManager.Instance.RemoveFromResourceInfoDic(infoFile);

                    var refReses = await EngineNS.CEngine.Instance.GameEditorInstance.GetWhoReferenceMe(res.ResourceName);
                    foreach(var refRes in refReses)
                    {
                        bool find = false;
                        foreach(var delRes in resInfos)
                        {
                            if(delRes.ResourceName == refRes)
                            {
                                find = true;
                                break;
                            }
                        }
                        // 引用的资源如果在删除列表中则不处理
                        if(!find)
                        {
                            var resInfo = await ResourceInfoManager.Instance.CreateResourceInfoFromResource(refRes.Address);
                            if (resInfo != null)
                                await resInfo.OnReferencedRNameChangedOverride(res, null, res.ResourceName);
                        }
                    }

                    await EngineNS.CEngine.Instance.GameEditorInstance.RemoveResourceInfo(res);
                }

                ParentBrowser.RemoveResourceInfo(res);
            }

            EditorCommon.FileSystemWatcherProcess.Enable = true;
            return true;
        }
        protected abstract System.Threading.Tasks.Task<bool> DeleteResourceOverride();
        /// <summary>
        /// 将资源移动到目标目录
        /// </summary>
        /// <param name="absFolder">目标目录</param>
        /// <returns></returns>
        public async Task<bool> MoveToFolder(string absFolder)
        {
            if (absFolder[absFolder.Length - 1] != '/' && absFolder[absFolder.Length - 1] != '\\')
                absFolder += "/";

            var oldRName = EngineNS.RName.GetRName(ResourceName.Name, ResourceName.RNameType);
            if (await MoveToFolderOverride(absFolder, ResourceName) == false)
                return false;

            string oldAbsInfoFileName = oldRName.Address + Program.ResourceInfoExt;
            // 移动Info文件
            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //{
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Update((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:资源{Name}移动到目录{absFolder}失败!");
            //        }
            //        else
            //        {
            //            EditorCommon.VersionControl.VersionControlManager.Instance.Move((EditorCommon.VersionControl.VersionControlCommandResult resultMove) =>
            //            {
            //                if (resultMove.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //                {
            //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:资源{Name}移动到目录{absFolder}失败!");
            //                }
            //            }, oldAbsInfoFileName, absFolder + ResourceFileName + Program.ResourceInfoExt, $"AutoCommit {ResourceTypeName}{Name}从{EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(oldAbsInfoFileName)}移动到{EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + ResourceFileName + Program.ResourceInfoExt)}");
            //        }
            //    }, oldAbsInfoFileName);
            //}
            //else
            var newAbsName = absFolder + oldRName.PureName(true);
            System.IO.File.Move(oldAbsInfoFileName, newAbsName + Program.ResourceInfoExt);
            RNameHistory.Add(oldRName);
            ResourceName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(newAbsName);
            await Save(false);

            var noUse = ProcessReferenceRNameChanged(absFolder, oldRName);

            return true;
        }
        protected abstract Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName);

        public async Task<bool> Rename()
        {
            var absFolder = ResourceName.GetDirectory();
            var oldName = ResourceName;
            var win = new InputWindow.InputWindow();
            win.Description = "输入新名称";
            win.Value = EditorCommon.Program.GetValidName(absFolder, ResourceName.PureName(), ResourceName.GetExtension());
            if (win.ShowDialog((value, cultureInfo) =>
            {
                if (value == null)
                    return new ValidationResult(false, "内容不合法");
                return ResourceNameAvailableCheck(absFolder, value.ToString());
            }) == false)
                return false;

            if (!await RenameOverride(absFolder, win.Value.ToString()))
                return false;

            var newAbsName = absFolder + win.Value.ToString() + ResourceName.GetExtension(true);
            System.IO.File.Move(oldName.Address + EditorCommon.Program.ResourceInfoExt, newAbsName + EditorCommon.Program.ResourceInfoExt);
            RNameHistory.Add(oldName);
            ResourceName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(newAbsName);
            await Save(false);

            var noUse = ProcessReferenceRNameChanged(absFolder, oldName);
            return true;
        }
        protected abstract Task<bool> RenameOverride(string absFolder, string newName);

        protected ValidationResult ResourceNameAvailableCheck(string absFolder, string name)
        {
            if(EditorCommon.Program.IsValidRName(name) == false)
                return new ValidationResult(false, "名称不合法!");
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + EngineNS.CEngineDesc.MaterialInstanceExtension, SearchOption.TopDirectoryOnly);
            if (files.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的资源文件文件!");
            }
            return new ValidationResult(true, null);
        }
        async Task ProcessReferenceRNameChanged(string absFolder, EngineNS.RName oldRName)
        {
            EditorCommon.Controls.ProcessProgressReportWin.Instance.Title = "正在处理引用关系：";
            EditorCommon.Controls.ProcessProgressReportWin.Instance.ShowReportWin(true);
            var rootFolder = oldRName.GetRootFolder();
            var resInfoFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(rootFolder, "*" + EditorCommon.Program.ResourceInfoExt, System.IO.SearchOption.AllDirectories);
            var total = resInfoFiles.Count;
            //var index = 0.0;

            /*List<string> processedFiles = new List<string>();
            // 优先处理打开的
            foreach(var ctrlData in EditorCommon.PluginAssist.Process.ControlsDic)
            {
                try
                {
                    if (ctrlData.Key == ResourceName)
                        continue;

                    index++;
                    EditorCommon.Controls.ProcessProgressReportWin.Instance.Progress = index / total;
                    EditorCommon.Controls.ProcessProgressReportWin.Instance.Info = $"正在处理{ctrlData.Key.Address}";

                    ResourceInfo resInfo;
                    if(ctrlData.Value.Context != null)
                    {
                        resInfo = ctrlData.Value.Context.ResInfo;
                    }
                    else
                    {
                        var tempFile = ctrlData.Key.Address + EditorCommon.Program.ResourceInfoExt;
                        resInfo = await ResourceInfoManager.Instance.CreateResourceInfoFromFile(tempFile, null);
                    }
                    foreach (var rName in resInfo.ReferenceRNameList)
                    {
                        if (rName == oldRName)
                        {
                            await resInfo.OnReferencedRNameChangedOverride(this, oldRName);
                            break;
                        }
                    }
                    processedFiles.Add(ctrlData.Key.Name);
                }
                catch (System.Exception e)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "MoveResource", $"文件{ctrlData.Key.Address}刷新引用的资源{this.ResourceName.Address}\r\n{e.ToString()}");
                }
            }

            //var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(resInfoFiles.Length);
            foreach (var file in resInfoFiles)
            {
                //EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
                //{
                try
                {
                    var relFile = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(file);
                    if (processedFiles.Contains(relFile))
                    {
                        continue;
                        //smp.Release();
                        //return false;
                    }

                    index++;
                    EditorCommon.Controls.ProcessProgressReportWin.Instance.Progress = index / total;
                    EditorCommon.Controls.ProcessProgressReportWin.Instance.Info = $"正在处理{file}";

                    var tempFile = file.Replace("\\", "/");
                    if (ResourceName.Address + EditorCommon.Program.ResourceInfoExt == tempFile)
                    {
                        continue;
                        //smp.Release();
                        //return false;
                    }
                    var resInfo = await ResourceInfoManager.Instance.CreateResourceInfoFromFile(tempFile, null);
                    foreach (var rName in resInfo.ReferenceRNameList)
                    {
                        if (rName == oldRName)
                            await resInfo.OnReferencedRNameChangedOverride(this, oldRName);
                    }
                }
                catch (System.Exception e)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "MoveResource", $"文件{file}刷新引用的资源{this.ResourceName.Address}\r\n{e.ToString()}");
                }
                //    smp.Release();
                //    return true;
                //}, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            }
            //await smp.Await();*/
            var refs = await EngineNS.CEngine.Instance.GameEditorInstance.GetWhoReferenceMe(oldRName);
            foreach(var res in refs)
            {
                var resInfo = await ResourceInfoManager.Instance.CreateResourceInfoFromResource(res.Address);
                if(resInfo != null)
                    await resInfo.OnReferencedRNameChangedOverride(this, ResourceName, oldRName);
            }

            ResourceInfoManager.Instance.RemoveFromResourceInfoDic(oldRName.Address + EditorCommon.Program.ResourceInfoExt);
            ResourceInfoManager.Instance.SetToResourceInfoDic(ResourceName.Address + EditorCommon.Program.ResourceInfoExt, this);
            OnPropertyChanged("Path");
            EditorCommon.Controls.ProcessProgressReportWin.Instance.ShowReportWin(false);
        }

        // 当该资源引用的资源更改目录时调用此方法
        protected abstract Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName);

#pragma warning disable 1998
        protected virtual async System.Threading.Tasks.Task CopyResource(EditorCommon.Resources.ResourceInfo copyTo)
        {
            copyTo.ResourceType = ResourceType;
            copyTo.RNameHistory = new List<EngineNS.RName>(RNameHistory);
            copyTo.ReferenceRNameList = new List<EngineNS.RName>(ReferenceRNameList);
        }
#pragma warning restore 1998

        #region VersionControl
        Visibility mSVNStateImageVisibility = Visibility.Collapsed;
        [Browsable(false)]
        public Visibility SVNStateImageVisibility
        {
            get { return mSVNStateImageVisibility; }
            set
            {
                mSVNStateImageVisibility = value;
                OnPropertyChanged("SVNLockVisibility");
            }
        }

        ImageSource mSVNStateImage = null;// /ResourcesBrowser;component/Icon/svnlock.png
        public ImageSource SVNStateImage
        {
            get { return mSVNStateImage; }
            set
            {
                mSVNStateImage = value;
                OnPropertyChanged("SVNStateImage");
            }
        }

        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }
        public static readonly DependencyProperty BorderBrushProperty =
                    DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ResourceInfo), new PropertyMetadata(null, new PropertyChangedCallback(BorderBrushCallback)));
        static void BorderBrushCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public Brush SVNStateBrush
        {
            get { return (Brush)GetValue(SVNStateBrushProperty); }
            set { SetValue(SVNStateBrushProperty, value); }
        }
        public static readonly DependencyProperty SVNStateBrushProperty =
                    DependencyProperty.Register("SVNStateBrush", typeof(Brush), typeof(ResourceInfo), new PropertyMetadata(null, new PropertyChangedCallback(SVNStateBrushCallback)));
        static void SVNStateBrushCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }


        //////static ResourceDictionary mResDic = null;
        //////protected EditorCommon.VersionControl.EStatus mVersionControlState = EditorCommon.VersionControl.EStatus.Unknow;
        //////protected virtual void UpdateVersionControlStateShow() { }
        //////protected void ShowStateView(EditorCommon.VersionControl.EStatus state)
        //////{
        //////    EditorCommon.Program.MainDispatcher.Invoke(() =>
        //////    {
        //////        if (mResDic == null)
        //////        {
        //////            mResDic = new ResourceDictionary();
        //////            mResDic.Source = new Uri("/ResourcesBrowser;component/Themes/Generic.xaml", UriKind.Relative);
        //////        }

        //////        SVNStateImageVisibility = Visibility.Collapsed;
        //////        SVNStateImage = null;
        //////        mVersionControlState = state;
        //////        switch (mVersionControlState)
        //////        {
        //////            case EditorCommon.VersionControl.EStatus.Add:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNAddBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Branch:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNBranchBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Conflict:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNConflictBrush", mResDic) as Brush;
        //////                    SVNStateImageVisibility = Visibility.Visible;
        //////                    SVNStateImage = new BitmapImage(new System.Uri("/ResourceLibrary;component/Icon/output_bang.png", UriKind.Relative));
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Delete:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNDeleteBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Ignore:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNIgnoreBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Lock:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNLockBrush", mResDic) as Brush;
        //////                    SVNStateImageVisibility = Visibility.Visible;
        //////                    SVNStateImage = new BitmapImage(new System.Uri("/ResourcesBrowser;component/Icon/svnlock.png", UriKind.Relative));
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Lost:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNLostBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Modify:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNModifyBrush", mResDic) as Brush;
        //////                    SVNStateImageVisibility = Visibility.Visible;
        //////                    SVNStateImage = new BitmapImage(new System.Uri("/ResourcesBrowser;component/Icon/svnlock.png", UriKind.Relative));
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Normal:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNNormalBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.NotControl:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNNotControlBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Replace:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNReplaceBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.TypeChanged:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNTypeChangedBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////            case EditorCommon.VersionControl.EStatus.Unauthorized:
        //////            case EditorCommon.VersionControl.EStatus.PathTooLong:
        //////            case EditorCommon.VersionControl.EStatus.Invalid:
        //////            case EditorCommon.VersionControl.EStatus.Exist:
        //////            default:
        //////                {
        //////                    SVNStateBrush = EditorCommon.Program.TryFindResource("SVNUnknowBrush", mResDic) as Brush;
        //////                }
        //////                break;
        //////        }

        //////    });
        //////}

        //////private void VersionControl_Update()
        //////{
        //////    if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
        //////    {
        //////        EditorCommon.VersionControl.VersionControlManager.Instance.Update((result) =>
        //////        {

        //////        }, AbsInfoFileName);
        //////    }
        //////    VersionControl_Update_Override();
        //////}
        //////public virtual void VersionControl_Update_Override() { }//
        //////private void VersionControl_Conflicted_UseMine()
        //////{
        //////    if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
        //////    {
        //////        EditorCommon.VersionControl.VersionControlManager.Instance.UseMine((result) =>
        //////        {

        //////        }, AbsInfoFileName);
        //////    }
        //////    VersionControl_Conflicted_UseMine_Override();
        //////}
        //////public virtual void VersionControl_Conflicted_UseMine_Override() { }//
        //////private void VersionControl_Conflicted_UseTheirs()
        //////{
        //////    if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
        //////    {
        //////        EditorCommon.VersionControl.VersionControlManager.Instance.UseTheirs((result) =>
        //////        {

        //////        }, AbsInfoFileName);
        //////    }
        //////    VersionControl_Conflicted_UseTheirs_Override();
        //////}
        //////public virtual void VersionControl_Conflicted_UseTheirs_Override() { }//
        //////private void VersionControl_Revert()
        //////{
        //////    if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
        //////    {
        //////        EditorCommon.VersionControl.VersionControlManager.Instance.Revert((result) =>
        //////        {

        //////        }, AbsInfoFileName);
        //////    }
        //////    VersionControl_Revert_Override();
        //////}
        //////public virtual void VersionControl_Revert_Override() { }//
        //////private void VersionControl_Commit()
        //////{
        //////    VersionControl_Update();
        //////    if (mVersionControlState == EditorCommon.VersionControl.EStatus.Conflict)
        //////        return;

        //////    if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
        //////    {
        //////        EditorCommon.VersionControl.VersionControlManager.Instance.Commit((result) =>
        //////        {

        //////        }, AbsInfoFileName, $"AutoCommit 提交{ResourceTypeName}{Name}");
        //////    }
        //////    VersionControl_Commit_Override();
        //////}
        //////public virtual void VersionControl_Commit_Override() { }//
        //////private void VersionControl_Add()
        //////{
        //////    if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
        //////    {
        //////        EditorCommon.VersionControl.VersionControlManager.Instance.Commit((result) =>
        //////        {

        //////        }, AbsInfoFileName, $"AutoCommit 添加{ResourceTypeName}{Name}");
        //////    }
        //////    VersionControl_Add_Override();
        //////}
        //////public virtual void VersionControl_Add_Override() { }//

        #endregion

        #region ContextMenu

        ContextMenu mContextMenu = null;
        public ContextMenu ResContextMenu
        {
            get { return mContextMenu; }
            set
            {
                mContextMenu = value;
                OnPropertyChanged("ContextMenu");
            }
        }
        bool contextMenuInitialized = false;
        public void UpdateVersionControlContextMenu()
        {
            //if (mSvnMenu == null)
            //    return;

            //////mSvnMenu.Items.Clear();
            //////switch (mVersionControlState)
            //////{
            //////    case EditorCommon.VersionControl.EStatus.Conflict:
            //////        {
            //////            var item = new MenuItem()
            //////            {
            //////                Header = "使用我的",
            //////            };
            //////            item.Click += (sender, e) =>
            //////            {
            //////                VersionControl_Conflicted_UseMine();
            //////            };
            //////            mSvnMenu.Items.Add(item);
            //////            item = new MenuItem()
            //////            {
            //////                Header = "使用他们的",
            //////            };
            //////            item.Click += (sender, e) =>
            //////            {
            //////                VersionControl_Conflicted_UseTheirs();
            //////            };
            //////            mSvnMenu.Items.Add(item);
            //////        }
            //////        break;
            //////    case EditorCommon.VersionControl.EStatus.Modify:
            //////        {
            //////            var item = new MenuItem()
            //////            {
            //////                Header = "更新",
            //////            };
            //////            item.Click += (sender, e) =>
            //////            {
            //////                VersionControl_Update();
            //////            };
            //////            mSvnMenu.Items.Add(item);
            //////            item = new MenuItem()
            //////            {
            //////                Header = "提交",
            //////            };
            //////            item.Click += (sender, e) =>
            //////            {
            //////                VersionControl_Commit();
            //////            };
            //////            mSvnMenu.Items.Add(item);
            //////            item = new MenuItem()
            //////            {
            //////                Header = "恢复",
            //////            };
            //////            item.Click += (sender, e) =>
            //////            {
            //////                VersionControl_Revert();
            //////            };
            //////            mSvnMenu.Items.Add(item);
            //////        }
            //////        break;
            //////    case EditorCommon.VersionControl.EStatus.NotControl:
            //////        {
            //////            var item = new MenuItem()
            //////            {
            //////                Header = "添加",
            //////            };
            //////            item.Click += (sender, e) =>
            //////            {
            //////                VersionControl_Add();
            //////            };
            //////            mSvnMenu.Items.Add(item);
            //////        }
            //////        break;
            //////    default:
            //////        {
            //////            var item = new MenuItem()
            //////            {
            //////                Header = "更新",
            //////            };
            //////            item.Click += (sender, e) =>
            //////            {
            //////                VersionControl_Update();
            //////            };
            //////            mSvnMenu.Items.Add(item);
            //////        }
            //////        break;
            //////}
        }

        //MenuItem mSvnMenu;
        public async System.Threading.Tasks.Task InitializeContextMenu()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            if (System.Threading.SynchronizationContext.Current==null)
                System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
            EngineNS.CEngine.Instance.EventPoster.RunOn(async ()=>
            {
                if (!contextMenuInitialized)
                {
                    var contextMenu = new ContextMenu();
                    if (Application.Current.MainWindow != null)
                        contextMenu.Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as System.Windows.Style;
                    //mSvnMenu = new MenuItem()
                    //{
                    //    Header = "版本控制",
                    //};
                    //mSvnMenu.SetBinding(MenuItem.IsEnabledProperty, new Binding("Enable") { Source = EditorCommon.VersionControl.VersionControlManager.Instance });
                    //ResContextMenu.Items.Add(mSvnMenu);

                    if (await InitializeContextMenuOverride(contextMenu))
                        ResContextMenu = contextMenu;
                    contextMenuInitialized = true;
                }
                return true;
            },EngineNS.Thread.Async.EAsyncTarget.Main);
        }
#pragma warning disable 1998
        protected virtual async System.Threading.Tasks.Task<bool> InitializeContextMenuOverride(ContextMenu contextMenu)
        {
            return false;
        }
#pragma warning restore 1998

        #endregion

        public virtual Object CustomIconsObject
        {
            get;
            set;
        } = null;
        //bool mCustomIconsInitialized = false;
        //private void InitializeCustomIcons()
        //{
        //    EditorCommon.Program.MainDispatcher.Invoke(() =>
        //    {
        //        if(!mCustomIconsInitialized)
        //        {
        //            if (this.ParentBrowser == null)
        //                return;
        //            var container = this.ParentBrowser.GetContainerFromItem(this);
        //            if (container == null)
        //                return;
        //            var panel = LogicalTreeHelper.FindLogicalNode(container, "CustomIconsPanel") as StackPanel;
        //            if(panel != null)
        //                InitializeCustomIconsOverride(panel);
        //            mCustomIconsInitialized = true;
        //        }
        //    });
        //}
        //protected virtual void InitializeCustomIconsOverride(StackPanel panel)
        //{

        //}

        #region ToolTip

        string mDescription = "";
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("说明")]
        [EngineNS.Rtti.MetaData]
        public string Description
        {
            get => mDescription;
            set
            {
                mDescription = value;
                OnPropertyChanged("Description");
            }
        }
        
        protected StackPanel mResToolTipPanel;
        public void InitializeToolTipPanel(StackPanel toolTipPanel)
        {
            mResToolTipPanel = toolTipPanel;
            UpdateToolTip();
        }
        public void UpdateToolTip()
        {
            if (mResToolTipPanel == null)
                return;

            mResToolTipPanel.Children.Clear();
            UpdateToolTipOverride();
        }
        protected virtual void UpdateToolTipOverride() { }

        #endregion

        #region ISerializer
        public void ReadObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }

        public void ReadObject(EngineNS.IO.Serializer.IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }
        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }
        public void ReadObject(EngineNS.IO.Serializer.IReader pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }
        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion
    }
    #endregion

    #region CommonResourceInfo
    public class CommonResourceInfo : ResourceInfo
    {
        public override string ResourceTypeName { get; }
        public override Brush ResourceTypeBrush { get; }
        public override ImageSource ResourceIcon { get; }

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new InvalidOperationException();
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new InvalidOperationException();
        }
        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(EngineNS.RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return null;
        }

        public override async System.Threading.Tasks.Task<bool> AsyncLoad(string absFileName)
        {
            return await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                var holder = EngineNS.IO.XmlHolder.LoadXML(absFileName);
                if (holder == null)
                    return false;
                ReadObjectXML(holder.RootNode);
                absFileName = EngineNS.CEngine.Instance.FileManager.RemoveExtension(absFileName);// absFileName.TrimEnd(Program.ResourceInfoExt.ToCharArray());
                ResourceName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(absFileName);
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
        }

#pragma warning disable 1998
        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride() { return false; }
        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName) { return false; }
#pragma warning restore 1998

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }
        //////public override void VersionControl_Add_Override() { }
        //////public override void VersionControl_Commit_Override() { }
        //////public override void VersionControl_Conflicted_UseMine_Override() { }
        //////public override void VersionControl_Conflicted_UseTheirs_Override() { }
        //////public override void VersionControl_Revert_Override() { }
        //////public override void VersionControl_Update_Override() { }
    }
    #endregion
}

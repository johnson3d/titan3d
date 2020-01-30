using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EditorCommon.Controls.ResourceBrowser
{
    public partial class BrowserControl
    {
        public class FolderView : INotifyPropertyChanged, EditorCommon.DragDrop.IDragAbleObject, EditorCommon.Resources.IFolderItem
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            #region IDragAbleObject
            public FrameworkElement GetDragVisual()
            {
                return null;
            }
            #endregion

            #region IFolderItem

            ContentControl.ShowSourcesInDirData.FolderData mFolderData;
            public ContentControl.ShowSourcesInDirData.FolderData FolderData
            {
                get => mFolderData;
                set
                {
                    mFolderData = value;
                    PathName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(mFolderData.AbsFolder);
                }
            }

            string mPathName = null;
            public string PathName
            {
                get { return mPathName; }
                set
                {
                    mPathName = value;
                    OnPropertyChanged("PathName");
                }
            }
            public void AddSubFolderItem(Resources.IFolderItem item)
            {
                ChildList.Add(item as FolderView);
            }

            #endregion

            ImageSource mImageIcon = null;
            public ImageSource ImageIcon
            {
                get { return mImageIcon; }
                set
                {
                    mImageIcon = value;
                    OnPropertyChanged("ImageIcon");
                }
            }

            Visibility mVisiblity = Visibility.Visible;
            public Visibility Visibility
            {
                get { return mVisiblity; }
                set
                {
                    mVisiblity = value;
                    OnPropertyChanged("Visibility");
                }
            }

            string mHighLightString = "";
            [Browsable(false)]
            public string HighLightString
            {
                get { return mHighLightString; }
                set
                {
                    mHighLightString = value;
                    OnPropertyChanged("HighLightString");
                }
            }

            bool mIsExpanded = false;
            public bool IsExpanded
            {
                get { return mIsExpanded; }
                set
                {
                    mIsExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
            bool mIsSelected = false;
            public bool IsSelected
            {
                get { return mIsSelected; }
                set
                {
                    mIsSelected = value;
                    UpdateImageIcon();
                    OnPropertyChanged("IsSelected");
                }
            }
            string mDescription = "";
            public string Description
            {
                get { return mDescription; }
                set
                {
                    mDescription = value;
                    OnPropertyChanged("Description");
                }
            }

            FolderView mParent = null;
            public FolderView Parent
            {
                get => mParent;
                internal set
                {
                    mParent = value;
                }
            }
            ObservableCollection<FolderView> mChildList = new ObservableCollection<FolderView>();
            public ObservableCollection<FolderView> ChildList
            {
                get { return mChildList; }
                internal set { mChildList = value; }
            }

            void UpdateImageIcon()
            {
                if (IsSelected)
                {
                    var img = mParentBrowser.TryFindResource("Folder_OpenIcon") as Image;
                    if (img != null)
                        ImageIcon = img.Source;
                }
                else
                {
                    var img = mParentBrowser.TryFindResource("Folder_CloseIcon") as Image;
                    if (img != null)
                        ImageIcon = img.Source;
                }
            }

            BrowserControl mParentBrowser = null;
            public FolderView(ContentControl.ShowSourcesInDirData.FolderData folderData, BrowserControl parentBrowser)
            {
                mParentBrowser = parentBrowser;
                FolderData = folderData;
                UpdateImageIcon();

                //if(IsExpanded)
                {
                    RefreshSubFolders();
                }
            }

            public void RefreshSubFolders()
            {
                ChildList.Clear();
                if (System.IO.Directory.Exists(FolderData.AbsFolder))
                {
                    foreach (var subFolder in EngineNS.CEngine.Instance.FileManager.GetDirectories(FolderData.AbsFolder))
                    {
                        if (!mParentBrowser.IsFolderValid(subFolder))
                            continue;

                        var data = new ContentControl.ShowSourcesInDirData.FolderData()
                        {
                            AbsFolder = subFolder,
                            RootFolder = FolderData.RootFolder,
                        };
                        var flItem = new FolderView(data, mParentBrowser);
                        flItem.Parent = this;
                        ChildList.Add(flItem);
                    }
                }
            }

            public bool SelectFolder(List<string> folderNames)
            {
                if (folderNames == null)
                    return false;
                if (folderNames.Count == 0)
                    return false;
                if(string.Equals(PathName, folderNames[0], StringComparison.OrdinalIgnoreCase))
                {
                    if (folderNames.Count == 1)
                    {
                        if (!this.IsSelected)
                        {
                            this.IsSelected = true;
                        }
                        return true;
                    }
                    IsExpanded = true;

                    if (ChildList.Count > 0)
                    {
                        folderNames.RemoveAt(0);
                        foreach(var item in ChildList)
                        {
                            if (item.SelectFolder(folderNames))
                                return true;
                        }
                    }
                }
                return false;
            }
        }

        ObservableCollection<FolderView> mAllFolders = new ObservableCollection<FolderView>();
        ObservableCollection<FolderView> mViewFolders = new ObservableCollection<FolderView>();
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
        }
        FolderView mCurrentSelectFolder;
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            mCurrentSelectFolder = TreeView_Folders.SelectedValue as FolderView;
            var data = new ContentControl.ShowSourcesInDirData();
            data.FolderDatas.Add(mCurrentSelectFolder.FolderData);
            if(mCurrentFolderData != null)
            {
                data.SearchSubFolder = mCurrentFolderData.SearchSubFolder;
                data.FileExts = mCurrentFolderData.FileExts;
                data.CompareFuction = mCurrentFolderData.CompareFuction;
            }
            var noUse = ShowSourcesInDir(data);
        }
        private void TreeViewItem_UnSelected(object sender, RoutedEventArgs e)
        {
            var item = TreeView_Folders.SelectedValue as FolderView;
            if (item == mCurrentSelectFolder)
                mCurrentSelectFolder = null;
        }
        Dictionary<FrameworkElement, EditorCommon.DragDrop.DropAdorner> mDropAdornerDic = new Dictionary<FrameworkElement, DragDrop.DropAdorner>();
        private void TreeViewItem_DragEnter(object sender, DragEventArgs e)
        {
            var grid = sender as System.Windows.Controls.Grid;
            if (grid == null)
                return;

            var folderItem = grid.DataContext as Resources.IFolderItem;
            EditorCommon.DragDrop.DropAdorner adorner;
            if(!mDropAdornerDic.TryGetValue(grid, out adorner))
            {
                adorner = new DragDrop.DropAdorner(grid);
                mDropAdornerDic[grid] = adorner;
            }
            adorner.IsAllowDrop = FolderItem_OnDragEnter(folderItem, e);

            var pos = e.GetPosition(grid);
            if(pos.X > 0 && pos.X < grid.ActualWidth &&
               pos.Y > 0 && pos.Y < grid.ActualHeight)
            {
                var layer = AdornerLayer.GetAdornerLayer(grid);
                layer.Add(adorner);
            }
        }
        private void TreeViewItem_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            var grid = sender as System.Windows.Controls.Grid;
            if (grid == null)
                return;

            EditorCommon.DragDrop.DropAdorner adorner;
            if(mDropAdornerDic.TryGetValue(grid, out adorner))
            {
                var layer = AdornerLayer.GetAdornerLayer(grid);
                layer.Remove(adorner);
            }

            var folderItem = grid.DataContext as Resources.IFolderItem;
            FolderItem_OnDragLeave(folderItem, e);
        }
        private void TreeViewItem_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var folderItem = grid.DataContext as Resources.IFolderItem;
            FolderItem_OnDragOver(folderItem, e);
        }
        private void TreeViewItem_Drop(object sender, System.Windows.DragEventArgs e)
        {
            var grid = sender as System.Windows.Controls.Grid;
            if (grid == null)
                return;

            EditorCommon.DragDrop.DropAdorner adorner;
            if (mDropAdornerDic.TryGetValue(grid, out adorner))
            {
                var layer = AdornerLayer.GetAdornerLayer(grid);
                layer.Remove(adorner);
            }

            var folderItem = grid.DataContext as Resources.IFolderItem;
            FolderItem_OnDrop(folderItem, e);
        }

        enum enDropResult
        {
            Denial_TargetPathIsEmpty,
            Denial_UnknowFormat,
            Denial_NoDragAbleObject,
            Denial_SamePath,
            Denial_SubFolder,
            Denial_MultiPath,
            Allow,
        }
        enDropResult AllowFolderItemDrop(Resources.IFolderItem item, System.Windows.DragEventArgs e)
        {
            if (CurrentFolderData == null)
                return enDropResult.Denial_TargetPathIsEmpty;
            if (CurrentFolderData.FolderDatas.Count != 1)
                return enDropResult.Denial_MultiPath;

            var formats = e.Data.GetFormats();
            if (formats == null || formats.Length == 0)
                return enDropResult.Denial_UnknowFormat;

            var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
            if (datas == null)
                return enDropResult.Denial_NoDragAbleObject;

            var folderData = CurrentFolderData.FolderDatas[0];
            foreach (var data in datas)
            {
                var dragedItem = data as FolderView;
                if (dragedItem.FolderData.IsEqual(item.FolderData))
                    return enDropResult.Denial_SamePath;

                if (item.FolderData.AbsFolder.Contains(dragedItem.FolderData.AbsFolder))
                    return enDropResult.Denial_SubFolder;
            }

            return enDropResult.Allow;
        }

        enDropResult AllowResourceItemDrop(Resources.IFolderItem item, System.Windows.DragEventArgs e)
        {
            if (CurrentFolderData == null)
                return enDropResult.Denial_TargetPathIsEmpty;
            if (CurrentFolderData.FolderDatas.Count != 1)
                return enDropResult.Denial_MultiPath;

            var formats = e.Data.GetFormats();
            if (formats == null || formats.Length == 0)
            {
                return enDropResult.Denial_UnknowFormat;
            }

            var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
            if (datas == null)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "无可移动对象";
                return enDropResult.Denial_NoDragAbleObject;
            }
            if (item != null)
            {
                var tagFolderPath = item.FolderData.AbsFolder;
                foreach (var data in datas)
                {
                    var dragedItem = data as EditorCommon.Resources.ResourceInfo;
                    var path = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(dragedItem.AbsInfoFileName).Replace("\\", "/");
                    if (path[path.Length - 1] == '/')
                        path = path.Remove(path.Length - 1);
                    if (path.Equals(tagFolderPath))
                    {
                        return enDropResult.Denial_SamePath;
                    }
                }
            }
            return enDropResult.Allow;
        }

        public delegate bool Delegate_OnItemDragEnter(Resources.IFolderItem item, System.Windows.DragEventArgs e);
        public event Delegate_OnItemDragEnter OnFolderItemDragEnter;
        public delegate bool Delegate_OnItemDragLeave(Resources.IFolderItem item, System.Windows.DragEventArgs e);
        public event Delegate_OnItemDragLeave OnFolderItemDragLeave;
        public delegate bool Delegate_OnItemDragOver(Resources.IFolderItem item, System.Windows.DragEventArgs e);
        public event Delegate_OnItemDragOver OnFolderItemDragOver;
        public delegate bool Delegate_OnItemDrop(Resources.IFolderItem item, System.Windows.DragEventArgs e);
        public event Delegate_OnItemDrop OnFolderItemDrop;
        public static string FolderDragType
        {
            get { return "BrowserFolder"; }
        }
        public bool FolderItem_OnDragEnter(Resources.IFolderItem item, System.Windows.DragEventArgs e)
        {
            if (FolderDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                // 路径拖动
                e.Handled = true;
                switch (AllowFolderItemDrop(item, e))
                {
                    case enDropResult.Allow:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "移动到" + item.PathName;
                        return true;
                    case enDropResult.Denial_NoDragAbleObject:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "无可移动对象";
                        break;
                    case enDropResult.Denial_SamePath:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "不能移动到同目录";
                        break;
                    case enDropResult.Denial_SubFolder:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "不能移动到子目录";
                        break;
                    case enDropResult.Denial_UnknowFormat:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "不合法的格式";
                        break;
                    case enDropResult.Denial_TargetPathIsEmpty:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "目标目录为空";
                        break;
                }
            }
            else if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                // 资源文件拖动
                e.Handled = true;

                switch (AllowResourceItemDrop(item, e))
                {
                    case enDropResult.Allow:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "移动到" + item.PathName;
                        return true;
                    case enDropResult.Denial_NoDragAbleObject:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "无可移动对象";
                        break;
                    case enDropResult.Denial_SamePath:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "不能移动到同目录";
                        break;
                    case enDropResult.Denial_SubFolder:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "不能移动到子目录";
                        break;
                    case enDropResult.Denial_UnknowFormat:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "不合法的格式";
                        break;
                    case enDropResult.Denial_TargetPathIsEmpty:
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "目标目录为空";
                        break;
                }
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (datas == null)
                    return false;
                if (datas.Length == 0)
                    return false;
                if (ContentCtrl.CheckFileDropAvailable(datas))
                {
                    e.Effects = DragDropEffects.Link;
                    e.Handled = true;
                    return true;
                }
            }
            else
                OnFolderItemDragEnter?.Invoke(item, e);

            return false;
        }
        public bool FolderItem_OnDragLeave(Resources.IFolderItem item, System.Windows.DragEventArgs e)
        {
            if (FolderDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                e.Handled = true;
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
            }
            else if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                e.Handled = true;
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
            }
            else
            {
                OnFolderItemDragLeave?.Invoke(item, e);
            }

            return false;
        }
        public bool FolderItem_OnDragOver(Resources.IFolderItem item, System.Windows.DragEventArgs e)
        {
            if (FolderDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                e.Handled = true;
                if (AllowFolderItemDrop(item, e) == enDropResult.Allow)
                {
                    e.Effects = DragDropEffects.Move;
                    return true;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                    return false;
                }
            }
            else if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                e.Handled = true;
                if (AllowResourceItemDrop(item, e) == enDropResult.Allow)
                {
                    e.Effects = DragDropEffects.Move;
                    return true;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                    return false;
                }
            }
            else
            {
                OnFolderItemDragOver?.Invoke(item, e);
            }

            return false;
        }
        public bool FolderItem_OnDrop(Resources.IFolderItem item, System.Windows.DragEventArgs e)
        {
            var noUse = FolderItem_OnDrop_Async(item, e);
            return true;
        }
        async Task<bool> FolderItem_OnDrop_Async(Resources.IFolderItem item, System.Windows.DragEventArgs e)
        {
            if (FolderDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                e.Handled = true;
                if (AllowFolderItemDrop(item, e) == enDropResult.Allow)
                {
                    var formats = e.Data.GetFormats();
                    if (formats == null || formats.Length == 0)
                        return false;

                    var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                    if (datas == null)
                        return false;

                    if (EditorCommon.MessageBox.Show($"是否确定将{datas.Length}个对象移动到目录{item.FolderData.AbsFolder}?", EditorCommon.MessageBox.enMessageBoxButton.OKCancel) == EditorCommon.MessageBox.enMessageBoxResult.Cancel)
                        return false;

                    foreach (var data in datas)
                    {
                        var dragedItem = data as FolderView;
                        if (dragedItem == null)
                            continue;
                        if (string.IsNullOrEmpty(dragedItem.FolderData.AbsFolder))
                            continue;
                        if (!System.IO.Directory.Exists(dragedItem.FolderData.AbsFolder))
                            continue;

                        var tagPath = item.FolderData.AbsFolder + "/" + dragedItem.PathName;
                        try
                        {
                            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
                            //{
                            //    var absSolutePath = dragedItem.AbsolutePath;
                            //    EditorCommon.VersionControl.VersionControlManager.Instance.Update((EditorCommon.VersionControl.VersionControlCommandResult result) =>
                            //    {
                            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
                            //        {
                            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"版本控制:移动失败！源(dragedItem.AbsolutePath), 目标(tagPath)");
                            //        }
                            //        else
                            //        {
                            //            EditorCommon.VersionControl.VersionControlManager.Instance.Move((EditorCommon.VersionControl.VersionControlCommandResult resultMove) =>
                            //            {
                            //                if (resultMove.Result != EditorCommon.VersionControl.EProcessResult.Success)
                            //                {
                            //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"版本控制:移动失败！源(dragedItem.AbsolutePath), 目标(tagPath)");
                            //                }
                            //            }, absSolutePath, tagPath, $"AutoCommit 从{dragedItem.AbsolutePath}移动到{tagPath}");
                            //        }
                            //    }, absSolutePath);
                            //}
                            //else
                            System.IO.Directory.Move(dragedItem.FolderData.AbsFolder, tagPath);

                            dragedItem.FolderData.AbsFolder = tagPath;
                            var parentItem = dragedItem.Parent as FolderView;
                            if (parentItem != null)
                            {
                                parentItem.ChildList.Remove(dragedItem);
                            }

                            item.AddSubFolderItem(dragedItem);
                        }
                        catch (System.IO.PathTooLongException)
                        {
                            EditorCommon.MessageBox.Show(tagPath + "路径太长");
                        }
                        catch (System.IO.IOException)
                        {
                            EditorCommon.MessageBox.Show("目录" + tagPath + "已存在");
                        }
                    }
                }
            }
            else if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                e.Handled = true;
                if (AllowResourceItemDrop(item, e) == enDropResult.Allow)
                {
                    var formats = e.Data.GetFormats();
                    if (formats == null || formats.Length == 0)
                        return false;

                    var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                    if (datas == null)
                        return false;

                    if (EditorCommon.MessageBox.Show($"是否确定将{datas.Length}个对象移动到目录{item.FolderData.AbsFolder}?", EditorCommon.MessageBox.enMessageBoxButton.OKCancel) == EditorCommon.MessageBox.enMessageBoxResult.Cancel)
                        return false;

                    foreach (var data in datas)
                    {
                        var dragedItem = data as EditorCommon.Resources.ResourceInfo;
                        if (await dragedItem.MoveToFolder(item.FolderData.AbsFolder))
                        {
                            ContentCtrl.RemoveResourceInfo(dragedItem);
                            dragedItem.ParentBrowser = null;
                        }
                        else
                        {
                            EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "ImportResource", $"资源浏览器:移动资源{dragedItem.ResourceName}失败");
                        }
                    }

                    ContentCtrl.UpdateCountString();
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (datas == null)
                    return false;
                if (datas.Length == 0)
                    return false;
                if (ContentCtrl.CheckFileDropAvailable(datas))
                {
                    var noUse = ContentCtrl.ImportResources(datas, item.FolderData);
                }
            }
            else
            {
                OnFolderItemDrop?.Invoke(item, e);
            }

            return false;
        }
        Point mMouseDownPos = new Point();
        public void FolderItem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var folderItem = sender as FolderView;
            if (folderItem == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(sender as FrameworkElement);
                if (((pos.X - mMouseDownPos.X) > 3) ||
                   ((pos.Y - mMouseDownPos.Y) > 3))
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(FolderDragType, new EditorCommon.DragDrop.IDragAbleObject[] { folderItem });
                }
            }
        }
        public void FolderItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var folderItem = sender as FrameworkElement;
            if (folderItem == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                mMouseDownPos = e.GetPosition(folderItem);
                Mouse.Capture(folderItem);
            }
        }
        private List<string> GetSubFolders(string absFolder, System.IO.SearchOption searchOption)
        {
            List<string> retList = new List<string>();
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(absFolder))
                return retList;
            var folders = System.IO.Directory.EnumerateDirectories(absFolder, "*.*", searchOption);
            foreach (var folder in folders)
            {
                var file = folder + EditorCommon.Program.ResourceInfoExt;
                // 有资源Info存在，所以此目录并非真正的文件结构目录
                if (System.IO.File.Exists(file))
                    continue;

                retList.Add(folder);
            }

            return retList;
        }
        string GetFolderName(string name)
        {
            int i = 1;
            while (true)
            {
                string folder = mCurrentSelectFolder.FolderData.AbsFolder + "/" + name + i.ToString();

                if (System.IO.Directory.Exists(folder))
                {
                    ++i;
                }
                else
                {
                    return name + i.ToString();
                }
            }
        }
        private ValidationResult CheckFolderName(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "名称不能为空");

            string valueStr = (string)value;
            if (string.IsNullOrEmpty(valueStr))
                return new ValidationResult(false, "名称不能为空");
            foreach (var keyword in ContentControl.FolderKeywords)
            {
                if (string.Equals(keyword, valueStr, System.StringComparison.OrdinalIgnoreCase))
                    return new ValidationResult(false, $"{valueStr}为引擎关键字，不能使用");
            }

            if (Regex.IsMatch(valueStr, @"[\u4e00-\u9fa5]"))
                return new ValidationResult(false, "为保证系统兼容性，文件夹名称中不能包含中文");

            foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                if (valueStr.Contains(invalidChar))
                    return new ValidationResult(false, "名称中包含不合法的字符: " + invalidChar);
            }

            var newFolder = mCurrentSelectFolder.FolderData.AbsFolder + "/" + valueStr;
            if (System.IO.Directory.Exists(newFolder))
                return new ValidationResult(false, "已存在名称为" + valueStr + "的目录！");

            return new ValidationResult(true, null);
        }
        private void MenuItem_NewFolder_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as FrameworkElement;
            var folder = elem.DataContext as FolderView;
            if (folder == null)
            {
                throw new InvalidOperationException();
            }
            if (CurrentFolderData == null || folder == null)
            {
                EditorCommon.MessageBox.Show("请先选择一个文件夹后再进行新建文件夹操作!");
                return;
            }
            if (CurrentFolderData.FolderDatas.Count != 1)
            {
                EditorCommon.MessageBox.Show("当前选择了多个文件夹，请先选择一个文件夹后再进行新建文件夹操作!");
                return;
            }

            var win = new InputWindow.InputWindow();
            win.Description = "输入目录名称";
            win.Value = GetFolderName("new");
            if (win.ShowDialog(CheckFolderName) == false)
            {
                return;
            }

            var tagFolder = folder.FolderData.AbsFolder + "/" + win.Value.ToString().ToLower();
            System.IO.Directory.CreateDirectory(tagFolder);

            var data = new ContentControl.ShowSourcesInDirData.FolderData()
            {
                AbsFolder = tagFolder,
                RootFolder = folder.FolderData.RootFolder,
            };
            var flItem = new FolderView(data, this);
            flItem.Parent = folder;
            folder.ChildList.Add(flItem);
            folder.IsExpanded = true;
        }
        private void MenuItem_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as FrameworkElement;
            var folder = elem.DataContext as FolderView;
            if (folder == null)
            {
                throw new InvalidOperationException();
            }
            System.Diagnostics.Process.Start("explorer.exe", folder.FolderData.AbsFolder.Replace("/", "\\"));
        }
        private void MenuItem_Rename_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as FrameworkElement;
            var folder = elem.DataContext as FolderView;
            if (folder == null)
            {
                throw new InvalidOperationException();
            }
            var win = new InputWindow.InputWindow();
            win.Description = "输入新目录名称";
            win.Value = folder.PathName;
            if (win.ShowDialog(CheckFolderName) == false)
            {
                return;
            }

            if (folder.PathName.ToLower() == win.Value.ToString().ToLower())
                return;

            var oldFolder = folder.FolderData.AbsFolder;
            var newFolder = folder.FolderData.AbsFolder.Replace(folder.PathName, "") + win.Value;
            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //{
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Update((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"版本控制:移动失败，源:({oldFolder})，目标:({newFolder})");
            //        }
            //        else
            //        {
            //            EditorCommon.VersionControl.VersionControlManager.Instance.Move((EditorCommon.VersionControl.VersionControlCommandResult resultMove) =>
            //            {
            //                if (resultMove.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //                {
            //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"版本控制:移动失败，源:({oldFolder})，目标:({newFolder})");
            //                }
            //            }, oldFolder, newFolder, $"AutoCommit 重命名目录，源:({oldFolder})，目标:({newFolder})");
            //        }
            //    }, oldFolder);
            //}
            //else
            {
                System.IO.Directory.Move(oldFolder, newFolder);
            }

            folder.FolderData.AbsFolder = newFolder;
            //var srcFolder = this.AbsolutePath;
            //var tagFolder = this.AbsolutePath.Remove(this.PathName) + win.Value;
            //System.IO.Directory.Move()
        }
        private void MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as FrameworkElement;
            var folder = elem.DataContext as FolderView;
            if (folder == null)
            {
                throw new InvalidOperationException();
            }
            folder.RefreshSubFolders();
            var data = new ContentControl.ShowSourcesInDirData();
            data.FolderDatas.Add(folder.FolderData);
            var noUse = this.ShowSourcesInDir(data);
        }
        private void MenuItem_RefreshSnapshot_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as FrameworkElement;
            var folder = elem.DataContext as FolderView;
            if (folder == null)
            {
                throw new InvalidOperationException();
            }
            if (System.IO.Directory.Exists(folder.FolderData.AbsFolder))
            {
                foreach (var file in System.IO.Directory.GetFiles(folder.FolderData.AbsFolder, "*" + EditorCommon.Program.SnapshotExt, System.IO.SearchOption.TopDirectoryOnly))
                {
                    System.IO.File.Delete(file);
                }
            }

            folder.RefreshSubFolders();
            var data = new ContentControl.ShowSourcesInDirData();
            data.FolderDatas.Add(folder.FolderData);
            var noUse = this.ShowSourcesInDir(data);
        }

        private void MenuItem_DeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            var noUse = MenuItem_DeleteFolder_Click_Async(sender, e);
        }
        private async Task MenuItem_DeleteFolder_Click_Async(object sender, RoutedEventArgs e)
        {
            var elem = sender as FrameworkElement;
            var folder = elem.DataContext as FolderView;
            if (folder == null)
            {
                throw new InvalidOperationException();
            }
            var parItem = folder.Parent as FolderView;
            if (parItem == null)
            {
                EditorCommon.MessageBox.Show("根目录不能删除!");
                return;
            }

            // 找到删除目录中所有RName
            List<EngineNS.RName> rNames = new List<EngineNS.RName>();
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(folder.FolderData.AbsFolder, "*" + EditorCommon.Program.ResourceInfoExt, System.IO.SearchOption.AllDirectories);
            foreach(var file in files)
            {
                var tempFile = EngineNS.CEngine.Instance.FileManager.RemoveExtension(file);
                var rName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(tempFile);
                rNames.Add(rName);
            }
            // 查找是否有目录外对象引用
            List<EngineNS.RName> extRefRNames = new List<EngineNS.RName>();
            foreach(var rName in rNames)
            {
                var refs = await EngineNS.CEngine.Instance.GameEditorInstance.GetWhoReferenceMe(rName);
                foreach(var refRName in refs)
                {
                    if(!rNames.Contains(refRName))
                    {
                        extRefRNames.Add(refRName);
                    }
                }
            }
            if(extRefRNames.Count > 0)
            {
                if (EditorCommon.MessageBox.Show($"有外部资源 {extRefRNames[0].Name} 等{extRefRNames.Count}个资源引用当年目录中的文件，是否继续删除?", MessageBox.enMessageBoxButton.YesNo) != MessageBox.enMessageBoxResult.Yes)
                    return;
            }

            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //{
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Update((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"版本控制:删除目录失败({this.AbsolutePath})");
            //        }
            //        else
            //        {
            //            EditorCommon.VersionControl.VersionControlManager.Instance.Delete((EditorCommon.VersionControl.VersionControlCommandResult resultDelete) =>
            //            {
            //                if (resultDelete.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //                {
            //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"版本控制:删除目录失败({this.AbsolutePath})");
            //                }
            //            }, this.AbsolutePath, $"AutoCommit 删除目录{EngineNS.CEngine.Instance.FileManager.Instance._GetRelativePathFromAbsPath(this.AbsolutePath)}");
            //        }
            //    }, this.AbsolutePath);
            //}
            //else
            {
                // 关闭打开的资源
                var keys = EditorCommon.PluginAssist.Process.ControlsDic.Keys.ToArray();
                foreach(var key in keys)
                {
                    if(rNames.Contains(key))
                    {
                        PluginAssist.PluginControlContainer pcc;
                        if (EditorCommon.PluginAssist.Process.ControlsDic.TryGetValue(key, out pcc))
                            pcc.TryClose(true);
                    }
                }

                System.IO.Directory.Delete(folder.FolderData.AbsFolder, true);
            }

            parItem.ChildList.Remove(folder);
        }
    }
}

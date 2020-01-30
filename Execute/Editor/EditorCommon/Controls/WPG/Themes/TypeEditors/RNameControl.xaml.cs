using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// RNameControl.xaml 的交互逻辑
    /// </summary>
    public partial class RNameControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public class ResourceNameChangedEventArgs
        {
            public EngineNS.RName Name;
        }
        public event EventHandler<ResourceNameChangedEventArgs> OnResourceNameChanged;
        public EngineNS.RName ResourceName
        {
            get { return (EngineNS.RName)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }

        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register("ResourceName", typeof(EngineNS.RName), typeof(RNameControl), new PropertyMetadata(new PropertyChangedCallback(OnResourceNameChangedCallback)));

        static void OnResourceNameChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as RNameControl;
            var newValue = e.NewValue as EngineNS.RName;
            Action action = async () =>
            {
                if (newValue == null || newValue == EngineNS.RName.EmptyName)
                {
                    ctrl.Snapshot = null;
                }
                else
                {
                    ctrl.mResourceInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(newValue.Address);
                    if (ctrl.mResourceInfo != null)
                    {
                        var snapshot = await ctrl.mResourceInfo.GetSnapshotImage(false);
                        if (snapshot == null)
                        {
                            snapshot = new ImageSource[] { ctrl.mResourceInfo.ResourceIcon };
                        }
                        ctrl.Snapshot = snapshot[0];
                    }
                }
            };
            action();
            var args = new ResourceNameChangedEventArgs();
            args.Name = newValue;
            ctrl.OnResourceNameChanged?.Invoke(ctrl, args);
        }
        //EngineNS.RName mResourceName;
        //// 依赖项属性在设置后有不生效的问题，这里换成普通属性
        //public EngineNS.RName ResourceName
        //{
        //    get => mResourceName;
        //    set
        //    {
        //        mResourceName = value;
        //        Action action = async () =>
        //        {
        //            if (mResourceName == null || mResourceName == EngineNS.RName.EmptyName)
        //            {
        //                Snapshot = null;
        //            }
        //            else
        //            {
        //                mResourceInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mResourceName.Address);
        //                if (mResourceInfo != null)
        //                {
        //                    var snapshot = await mResourceInfo.GetSnapshotImage(false);
        //                    if (snapshot == null)
        //                        snapshot = mResourceInfo.ResourceIcon;
        //                    Snapshot = snapshot;
        //                }
        //            }
        //        };
        //        action();
        //        var args = new ResourceNameChangedEventArgs();
        //        args.Name = mResourceName;
        //        OnResourceNameChanged?.Invoke(this, args);
        //        OnPropertyChanged("ResourceName");
        //    }
        //}

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RNameControl), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsReadOnlyChanged)));
        public static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as RNameControl;
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                ctrl.EditBtnVisible = Visibility.Collapsed;
                ctrl.ResetBtnVisible = Visibility.Collapsed;
            }
            else
            {
                ctrl.EditBtnVisible = Visibility.Visible;
                ctrl.ResetBtnVisible = Visibility.Visible;
            }
        }
        public Visibility EditBtnVisible
        {
            get { return (Visibility)GetValue(EditBtnVisibleProperty); }
            set { SetValue(EditBtnVisibleProperty, value); }
        }
        public static readonly DependencyProperty EditBtnVisibleProperty =
                            DependencyProperty.Register("EditBtnVisible", typeof(Visibility), typeof(RNameControl), new UIPropertyMetadata(Visibility.Visible));
        public Visibility ResetBtnVisible
        {
            get { return (Visibility)GetValue(ResetBtnVisibleProperty); }
            set { SetValue(ResetBtnVisibleProperty, value); }
        }
        public static readonly DependencyProperty ResetBtnVisibleProperty =
                            DependencyProperty.Register("ResetBtnVisible", typeof(Visibility), typeof(RNameControl), new UIPropertyMetadata(Visibility.Visible));

        public Visibility ShowActiveBtn
        {
            get { return (Visibility)GetValue(ShowActiveBtnProperty); }
            set { SetValue(ShowActiveBtnProperty, value); }
        }
        public static readonly DependencyProperty ShowActiveBtnProperty =
                            DependencyProperty.Register("ShowActiveBtn", typeof(Visibility), typeof(RNameControl), new UIPropertyMetadata(Visibility.Visible));
        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(RNameControl), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));
        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as RNameControl;
            var newValue = e.NewValue as EditorCommon.CustomPropertyDescriptor;
            var att = newValue.Attributes[typeof(EngineNS.Editor.Editor_RNameTypeAttribute)] as EngineNS.Editor.Editor_RNameTypeAttribute;
            if (att != null)
            {
                ctrl.mRNameResourceType = att.RNameType;
                ctrl.ShowActiveBtn = att.ShowActiveBtn ? Visibility.Visible : Visibility.Hidden;
                var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(ctrl.mRNameResourceType);
                if (meta != null)
                {
                    ctrl.ResourceBrush = meta.ResInfo.ResourceTypeBrush;
                    return;
                }
            }
            var macrossAtt = newValue.Attributes[typeof(EngineNS.Editor.Editor_RNameMacrossType)] as EngineNS.Editor.Editor_RNameMacrossType;
            if (macrossAtt != null)
            {
                ctrl.mRNameResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;
                ctrl.mMacrossBaseType = macrossAtt.MacrossBaseType;
                var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(ctrl.mRNameResourceType);
                if (meta != null)
                {
                    ctrl.ResourceBrush = meta.ResInfo.ResourceTypeBrush;
                    return;
                }
            }

            var excelAtt = newValue.Attributes[typeof(EngineNS.Editor.Editor_RNameMExcelType)] as EngineNS.Editor.Editor_RNameMExcelType;
            if (excelAtt != null)
            {
                ctrl.mRNameResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Excel;
                ctrl.mMacrossExcelType = excelAtt.MacrossBaseType;
                var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(ctrl.mRNameResourceType);
                if (meta != null)
                {
                    ctrl.ResourceBrush = meta.ResInfo.ResourceTypeBrush;
                }
            }
        }

        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(RNameControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public ImageSource Snapshot
        {
            get { return (ImageSource)GetValue(SnapshotProperty); }
            set { SetValue(SnapshotProperty, value); }
        }
        public static readonly DependencyProperty SnapshotProperty = DependencyProperty.Register("Snapshot", typeof(ImageSource), typeof(RNameControl), new UIPropertyMetadata(null));
        public Brush ResourceBrush
        {
            get { return (Brush)GetValue(ResourceBrushProperty); }
            set { SetValue(ResourceBrushProperty, value); }
        }
        public static readonly DependencyProperty ResourceBrushProperty = DependencyProperty.Register("ResourceBrush", typeof(Brush), typeof(RNameControl), new UIPropertyMetadata(null));

        string mRNameResourceType;
        public string RNameResourceType { get => mRNameResourceType; set => mRNameResourceType = value; }
        EditorCommon.Resources.ResourceInfo mResourceInfo;
        Type mMacrossBaseType;
        public Type MacrossBaseType
        {
            get => mMacrossBaseType;
            set => mMacrossBaseType = value;
        }
        Type mMacrossExcelType;
        public RNameControl()
        {
            InitializeComponent();

            ResBrowser.HideFolderPanel();
            ResBrowser.ContentCtrl.OnResourceItemSelected += (EditorCommon.Resources.ResourceInfo rInfo) =>
            {
                if (IsReadOnly)
                    return;
                if (rInfo == null)
                    return;

                ResourceName = rInfo.ResourceName;
                ResourceShowBtn.IsSubmenuOpen = false;
            };
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void IconTextBtn_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var data = new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData();
            data.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
            {
                AbsFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
                RootFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
            });
            if (ResBrowser.ShowEngineContent)
            {
                data.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
                {
                    AbsFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                    RootFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                });
            }
            if (ResBrowser.ShowEditorContent)
            {
                data.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
                {
                    AbsFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                    RootFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                });
            }

            var noUse = ShowSourcesInDir(data);
            ResBrowser.ContentCtrl.FocusSearchBox();
        }

        bool CheckValide(ResourceInfo info)
        {
            if (info == null)
                return false;

            if (mMacrossBaseType != null)
            {
                // 筛选Macross的类型
                var pro = info.GetType().GetProperty("BaseType");
                var baseType = pro.GetValue(info) as Type;
                if (baseType == null)
                {
                    return false;
                }

                if (mMacrossBaseType == baseType)
                {
                    return true;
                }

                //接口继承
                if (mMacrossBaseType.IsInterface && (baseType.GetInterface(mMacrossBaseType.FullName) != null))
                {
                    return true;
                }

                //继承
                if (baseType.IsSubclassOf(mMacrossBaseType))
                {
                    return true;
                }

                //判断是不是对象本身
                string classname = info.AbsInfoFileName.Replace(info.AbsPath, "").Replace(".macross.rinfo", "");
                if (classname.Equals(mMacrossBaseType.Name))
                {
                    return true;
                }
                //mMacrossBaseType不为空 并且不满足筛选条件都返回false
                return false;
            }

            if (mMacrossExcelType != null)
            {
                var pro = info.GetType().GetProperty("MacrossName");
                EngineNS.RName MacrossName = pro.GetValue(info) as EngineNS.RName;
                if (MacrossName != null)
                {
                    var getter = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<object>(MacrossName);
                    string fullname = getter.Get(false)?.GetType().FullName;
                    if (mMacrossExcelType.FullName.Equals(fullname))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        private void Button_Set_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mRNameResourceType))
                return;
            var resInfo = EditorCommon.PluginAssist.PropertyGridAssist.GetSelectedResourceInfo(mRNameResourceType);
            if (resInfo != null && CheckValide(resInfo))
                ResourceName = resInfo.ResourceName;
        }
        private void Button_Search_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var noUse = EditorCommon.Controls.ResourceBrowser.BrowserControl.ShowResource(ResourceName);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var instance = BindInstance as EngineNS.Editor.Editor_RNameTypeObjectBind;
            if (instance != null)
            {
                instance.invoke(true);
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var instance = BindInstance as EngineNS.Editor.Editor_RNameTypeObjectBind;
            if (instance != null)
            {
                instance.invoke(false);
            }

        }

        #region 拖放操作

        EditorCommon.DragDrop.DropAdorner mDropAdorner;
        enum enDropResult
        {
            Denial_UnknowFormat,
            Denial_NoDragAbleObject,
            Allow,
        }
        // 是否允许拖放
        enDropResult AllowResourceItemDrop(System.Windows.DragEventArgs e)
        {
            if (mRNameResourceType == null)
                return enDropResult.Denial_UnknowFormat;
            var formats = e.Data.GetFormats();
            if (formats == null || formats.Length == 0)
                return enDropResult.Denial_UnknowFormat;

            var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
            if (datas == null)
                return enDropResult.Denial_NoDragAbleObject;

            bool containMeshSource = false;
            foreach (var data in datas)
            {
                var resInfo = data as EditorCommon.Resources.ResourceInfo;
                if (resInfo.ResourceType == mRNameResourceType)
                {
                    containMeshSource = true;
                    break;
                }
            }

            if (!containMeshSource)
                return enDropResult.Denial_NoDragAbleObject;

            return enDropResult.Allow;
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;

                if (mDropAdorner == null)
                    mDropAdorner = new EditorCommon.DragDrop.DropAdorner(MainGrid);
                mDropAdorner.IsAllowDrop = false;

                switch (AllowResourceItemDrop(e))
                {
                    case enDropResult.Allow:
                        {
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "设置资源";

                            mDropAdorner.IsAllowDrop = true;
                            var pos = e.GetPosition(element);
                            if (pos.X > 0 && pos.X < element.ActualWidth &&
                               pos.Y > 0 && pos.Y < element.ActualHeight)
                            {
                                var layer = AdornerLayer.GetAdornerLayer(element);
                                layer.Add(mDropAdorner);
                            }
                        }
                        break;

                    case enDropResult.Denial_NoDragAbleObject:
                    case enDropResult.Denial_UnknowFormat:
                        {
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"拖动内容不是{mRNameResourceType}类型的资源";

                            mDropAdorner.IsAllowDrop = false;
                            var pos = e.GetPosition(element);
                            if (pos.X > 0 && pos.X < element.ActualWidth &&
                               pos.Y > 0 && pos.Y < element.ActualHeight)
                            {
                                var layer = AdornerLayer.GetAdornerLayer(element);
                                layer.Add(mDropAdorner);
                            }
                        }
                        break;
                }
            }
        }

        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
                var layer = AdornerLayer.GetAdornerLayer(element);
                layer.Remove(mDropAdorner);
            }
        }

        private void Rectangle_DragOver(object sender, DragEventArgs e)
        {
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                if (AllowResourceItemDrop(e) == enDropResult.Allow)
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                var layer = AdornerLayer.GetAdornerLayer(element);
                layer.Remove(mDropAdorner);

                if (AllowResourceItemDrop(e) == enDropResult.Allow)
                {
                    var formats = e.Data.GetFormats();
                    var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                    foreach (var data in datas)
                    {
                        var resInfo = data as EditorCommon.Resources.ResourceInfo;
                        if (resInfo == null)
                            continue;

                        if(CheckValide(resInfo))
                            ResourceName = resInfo.ResourceName;
                    }
                }
            }
        }

        #endregion

        private void CommandBinding_Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var resEd = mResourceInfo as EditorCommon.Resources.IResourceInfoEditor;
            if (resEd == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void CommandBinding_Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var resEd = mResourceInfo as EditorCommon.Resources.IResourceInfoEditor;
            if (resEd != null)
                resEd.OpenEditor();
        }

        private void CommandBinding_Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        class CopyPasteData : EditorCommon.ICopyPasteData
        {
            public EngineNS.RName ResourceName;
            public string RNameResourceType;
        }

        private void CommandBinding_Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var copyPasteData = new CopyPasteData()
            {
                ResourceName = this.ResourceName,
                RNameResourceType = this.mRNameResourceType,
            };
            EditorCommon.Program.SetCopyPasteData("ResourceRName", copyPasteData);
        }

        private void CommandBinding_Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var copyPasteData = EditorCommon.Program.GetCopyPasteData("ResourceRName") as CopyPasteData;
            if (copyPasteData == null)
            {
                e.CanExecute = false;
                return;
            }
            if (copyPasteData.RNameResourceType != this.mRNameResourceType)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = true;
        }

        private void CommandBinding_Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var copyPasteData = EditorCommon.Program.GetCopyPasteData("ResourceRName") as CopyPasteData;
            if (copyPasteData == null)
                return;
            var noUse = CommandBinding_Paste_ExecutedAsync(copyPasteData);
        }
        async Task CommandBinding_Paste_ExecutedAsync(CopyPasteData copyPasteData)
        {
            var info = await ResourceInfoManager.Instance.CreateResourceInfoFromResource(copyPasteData.ResourceName.Address);
            if (CheckValide(info))
                ResourceName = copyPasteData.ResourceName;
        }

        private void CommandBinding_Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (BindInstance == null || BindProperty == null)
            {
                e.CanExecute = false;
                ResetBtnVisible = Visibility.Collapsed;
                return;
            }
            var defaultVal = EditorCommon.Program.GetClassPropertyDefaultValue(BindInstance.GetType(), BindProperty.Name);
            if (object.Equals(defaultVal, ResourceName))
            {
                e.CanExecute = false;
                ResetBtnVisible = Visibility.Collapsed;
                return;
            }

            ResetBtnVisible = Visibility.Visible;
            e.CanExecute = true;
        }

        private void CommandBinding_Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (BindInstance == null || BindProperty == null)
                return;

            ResourceName = EditorCommon.Program.GetClassPropertyDefaultValue(BindInstance.GetType(), BindProperty.Name) as EngineNS.RName;
        }
        bool IsImageDoubleClick = false;
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount > 1)
                {
                    IsImageDoubleClick = true;
                }
                else
                {
                    IsImageDoubleClick = false;
                }
            }
        }
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsImageDoubleClick)
            {
                var resEd = mResourceInfo as EditorCommon.Resources.IResourceInfoEditor;
                if (resEd != null)
                    resEd.OpenEditor();
            }
        }

        //////public FrameworkElement GetContainerFromItem(ResourceInfo info)
        //////{
        //////    if (ContentCtrl.ListBox_Resources != null)
        //////        return ContentCtrl.ListBox_Resources.ItemContainerGenerator.ContainerFromItem(info) as FrameworkElement;
        //////    return null;
        //////}
        //////public void AddResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        //////{
        //////    ContentCtrl.AddResourceInfo(resInfo);
        //////}
        //////public void RemoveResourceInfo(ResourceInfo resInfo)
        //////{
        //////    ContentCtrl.RemoveResourceInfo(resInfo);
        //////}
        //////public void UpdateFilter()
        //////{
        //////    ContentCtrl.UpdateFilter();
        //////}

        //////public UInt64 ShowSourceInDirSerialId
        //////{
        //////    get;
        //////    private set;
        //////}
        public async Task ShowSourcesInDir(EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData data)
        {
            if (string.IsNullOrEmpty(mRNameResourceType))
                return;

            var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(mRNameResourceType);
            var showData = new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData()
            {
                SearchSubFolder = true,
                FileExts = meta.ResourceExts,
                CompareFuction = (info) =>
                {
                    return CheckValide(info);
                },
            };
            showData.FolderDatas = data.FolderDatas;
            //////ShowSourceInDirSerialId++;
            await ResBrowser.ShowSourcesInDir(showData);
        }
        
        //////public ResourceInfo[] GetSelectedResourceInfos()
        //////{
        //////    return ContentCtrl?.GetSelectedResourceInfos();
        //////}
        //////public void SelectResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        //////{
        //////    ContentCtrl?.SelectResourceInfos(resInfo);
        //////}
    }
}

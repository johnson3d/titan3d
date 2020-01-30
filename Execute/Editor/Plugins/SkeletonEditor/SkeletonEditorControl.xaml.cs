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
using DockControl;
using EditorCommon.Controls.Animation;
using EditorCommon.Controls.Skeleton;
using EditorCommon.ResourceInfos;
using EditorCommon.Resources;
using EngineNS;
using EngineNS.Bricks.Animation.Skeleton;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics.Mesh;
using EngineNS.IO;

namespace SkeletonEditor
{
    /// <summary>
    /// Interaction logic for SkeletonEditorControl.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "SkeletonEditor")]
    //[EditorCommon.PluginAssist.PluginMenuItem("工具(_T)/SkeletonEditor")]
    [Guid("780B12C4-7D03-4A91-87EB-4E1C2EE1D842")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SkeletonEditorControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin, EngineNS.ITickInfo, CodeGenerateSystem.Base.INodesContainerHost, EditorCommon.Controls.ResourceBrowser.IContentControlHost
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region INodesContainerHost
        public string UndoRedoKey
        {
            get
            {
                //if (mCurrentMaterial != null)
                //    return mCurrentMaterial.GetHash64().ToString();
                return "";
            }
        }
        void UpdateUndoRedoKey()
        {
            OnPropertyChanged("UndoRedoKey");
        }
        public string GetGraphFileName(string graphName)
        {
            return "";
        }
        public Guid LinkedCategoryItemID { get; set; }
        public string LinkedCategoryItemName { get; }
        public Dictionary<Guid, CodeGenerateSystem.Controls.NodesContainerControl> SubNodesContainers
        {
            get => null;
        }
        public async Task<CodeGenerateSystem.Controls.NodesContainerControl> ShowSubNodesContainer(CodeGenerateSystem.Base.SubNodesContainerData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            data.IsCreated = false;
            return null;
        }
        public async Task<CodeGenerateSystem.Controls.NodesContainerControl> GetSubNodesContainer(CodeGenerateSystem.Base.SubNodesContainerData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            data.IsCreated = false;
            return null;
        }
        public void NodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {

        }
        public void InitializeSubLinkedNodesContainer(CodeGenerateSystem.Controls.NodesContainerControl nodesControl)
        {

        }
        public async Task InitializeNodesContainer(CodeGenerateSystem.Controls.NodesContainerControl nodesControl)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        #endregion

        #region IContentControlHost


        public UInt64 ShowSourceInDirSerialId
        {
            get;
            private set;
        }
        public FrameworkElement GetContainerFromItem(ResourceInfo info)
        {
            return null;
        }

        public void AddResourceInfo(ResourceInfo resInfo)
        {
        }

        public void RemoveResourceInfo(ResourceInfo resInfo)
        {
        }

        public Task ShowSourcesInDir(EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData data)
        {
            return null;
        }

        public void UpdateFilter()
        {

        }
        public ResourceInfo[] GetSelectedResourceInfos()
        {
            return PreviewMeshCtrl?.GetSelectedResourceInfos();
        }
        public void SelectResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            PreviewMeshCtrl?.SelectResourceInfos(resInfo);
        }
        #endregion

        #region TickInfo

        public bool EnableTick { get; set; }
        public EngineNS.Profiler.TimeScope GetLogicTimeScope()
        {
            return null;
        }
        public void BeforeFrame()
        {

        }
        public void TickLogic()
        {

        }
        public void TickRender()
        {

        }
        async System.Threading.Tasks.Task DirtyProcess(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //if (mNeedDirtyProcess || force)
            //{
            //    var rc = EngineNS.CEngine.Instance.RenderContext;

            //    await DirtyProcessAsync(force, needGenerateCode);

            //    foreach (var node in NodesControl.CtrlNodeList)
            //    {
            //        var shaderVarNode = node as Controls.BaseNodeControl_ShaderVar;
            //        if (shaderVarNode == null)
            //            continue;
            //        var varInfo = shaderVarNode.GetShaderVarInfo();
            //        if (varInfo == null)
            //            continue;

            //        var param = mPreviewMaterial.GetParam(varInfo.VarName);
            //        if (param == null)
            //            continue;
            //        param.CopyFrom(varInfo);
            //        mPreviewMaterialInstance.SetParam(varInfo);
            //    }

            //    // 刷新ViewPort显示
            //    foreach (var i in mMeshComponent.mSceneMesh.MtlMeshArray)
            //    {
            //        if (i == null)
            //            continue;
            //        i.BuildTechnique(rc);
            //    }

            //    for (int i = 0; i < mMeshComponent.MaterialNumber; i++)
            //    {
            //        mMeshComponent.SetMaterial(rc, (uint)i, mPreviewMaterialInstance);
            //    }

            //    mNeedDirtyProcess = false;
            //}
        }
        async System.Threading.Tasks.Task DirtyProcessAsync(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            //{
            //    // 更新MaterialInstance参数
            //    var rc = EngineNS.CEngine.Instance.RenderContext;
            //    // 刷新预览用材质
            //    if (needGenerateCode)
            //    {
            //        System.IO.TextWriter codeFile, varFile;
            //        CodeGenerator.GenerateCode(NodesControl, mSetValueMaterialControl, out codeFile, out varFile);
            //        // Var
            //        System.IO.File.WriteAllText(mPreviewMaterial.Name.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, varFile.ToString());
            //        // Code
            //        System.IO.File.WriteAllText(mPreviewMaterial.Name.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, codeFile.ToString());

            //        mPreviewMaterial.ForceUpdateVersion();

            //        UpdateMtlMacros(mPreviewMaterial);
            //    }
            //    if (mNeedRefreshAllMaterialInstance || force)
            //    {
            //        // 预览用材质实例
            //        mPreviewMaterialInstance = EngineNS.CEngine.Instance.MaterialInstanceManager.NewMaterialInstance(rc, mPreviewMaterial);
            //        for (int i = 0; i < mMeshComponent.MaterialNumber; i++)
            //        {
            //            mMeshComponent.SetMaterial(rc, (uint)i, mPreviewMaterialInstance);
            //        }
            //    }
            //    mPreviewMaterialInstance.ForceUpdateVersion();

            //    return true;
            //});
        }
        //bool mNeedDirtyProcess = false;
        public void TickSync()
        {
            var noUse = DirtyProcess();
        }
        #endregion

        #region IEditorPlugin
        public string PluginName => "SkeletonEditor";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SkeletonEditorControl), new FrameworkPropertyMetadata(null));
        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Skeleton_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(SkeletonEditorControl), new FrameworkPropertyMetadata(null));
        public UIElement InstructionControl => null;

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public bool OnActive()
        {
            return true;
        }

        public bool OnDeactive()
        {
            return true;
        }
        EditorCommon.ResourceInfos.SkeletonResourceInfo mCurrentResourceInfo = null;
        CGfxSkeleton mSkeleton;
        SkeletonTreeViewOperation mSkeletonTreeViewOperation;
        public async System.Threading.Tasks.Task SetObjectToEdit(ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var info = context.ResInfo as EditorCommon.ResourceInfos.SkeletonResourceInfo;
            if (mCurrentResourceInfo == null || mCurrentResourceInfo.ResourceName != info.ResourceName)
            {
                mCurrentResourceInfo = info;
                SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                IconBrush = context.ResInfo.ResourceTypeBrush;
                mSkeletonTreeViewOperation = new SkeletonTreeViewOperation();
                mSkeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, mCurrentResourceInfo.ResourceName);
                mSkeletonTreeViewOperation.Skeleton = mSkeleton;
                mSkeletonTreeViewOperation.SkeletonTreeView = TreeView_Skeleton;
                CreateSkeletonTreeView();
                await CachingPreviewMeshOfSkeleton();
                await mPreviewSceneControl.Initialize(mSceneName);
                ProGrid_PreviewScene.Instance = mPreviewSceneControl;
                await PreviewMeshChanged();
                CEngine.Instance.TickManager.AddTickInfo(this);
            }
        }
        ObservableCollection<EditorBoneDetial> EditorBoneDetialTree { get; set; } = new ObservableCollection<EditorBoneDetial>();
        EditorBoneDetial GetParentFromTree(ObservableCollection<EditorBoneDetial> tree, EditorBoneDetial bone)
        {
            foreach (var sT in tree)
            {
                var p = GetParent(sT, bone);
                if (p == null)
                    continue;
                else
                    return p;
            }
            return null;
        }
        EditorBoneDetial GetParent(EditorBoneDetial tree, EditorBoneDetial bone)
        {
            if (tree.Children.Contains(bone))
                return tree;
            foreach (var child in tree.Children)
            {
                var p = GetParent(child, bone);
                if (p == null)
                    continue;
                else
                    return p;
            }
            return null;
        }
        void CreateSkeletonTreeView()
        {
            var skeletonPose = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, mCurrentResourceInfo.ResourceName);
            skeletonPose.GenerateHierarchy();
            var root = skeletonPose.Root;

            var editorBoneDetial = new EditorBoneDetial(skeletonPose, root, null);
            EditorBoneDetialTree.Add(editorBoneDetial);
            TreeView_Skeleton.ItemsSource = EditorBoneDetialTree;
        }
        public static Dictionary<string, string> SkeletonAssetMeshDic = new Dictionary<string, string>();
        async System.Threading.Tasks.Task CachingPreviewMeshOfSkeleton()
        {
            if (!string.IsNullOrEmpty(mCurrentResourceInfo.PreViewMesh) && mCurrentResourceInfo.PreViewMesh != "null")
            {
                if (!SkeletonEditorControl.SkeletonAssetMeshDic.ContainsKey(mCurrentResourceInfo.ResourceName.Name))
                    SkeletonEditorControl.SkeletonAssetMeshDic.Add(mCurrentResourceInfo.ResourceName.Name, mCurrentResourceInfo.PreViewMesh);
            }
            else
            {
                string preViewMesh = "";
                if (SkeletonEditorControl.SkeletonAssetMeshDic.TryGetValue(mCurrentResourceInfo.ResourceName.Name, out preViewMesh) == false)
                {
                    //var meshInfo = await SearchFirshMesh();
                    var meshInfo = await EditorCommon.Utility.PreviewHelper.SearchFirshResourceInfo(mPreviewMeshResourceType, (info) =>
                     {
                         var tempInfo = info as MeshResourceInfo;
                         if (tempInfo.SkeletonAsset == mCurrentResourceInfo.ResourceName.Name)
                             return true;
                         else
                             return false;
                     });
                    if (meshInfo != null)
                    {
                        mCurrentResourceInfo.PreViewMesh = meshInfo.ResourceName.Name;
                        await mCurrentResourceInfo.Save();
                        SkeletonEditorControl.SkeletonAssetMeshDic.Add(mCurrentResourceInfo.ResourceName.Name, mCurrentResourceInfo.PreViewMesh);
                    }
                }
                else
                {
                    mCurrentResourceInfo.PreViewMesh = preViewMesh;
                    await mCurrentResourceInfo.Save();
                }
            }
        }

        public void SaveElement(XmlNode node, XmlHolder holder)
        {

        }

        public IDockAbleControl LoadElement(XmlNode node)
        {
            return null;
        }

        public void StartDrag()
        {

        }

        public void EndDrag()
        {

        }

        public bool? CanClose()
        {
            return true;
        }

        public void Closed()
        {
            EditorCommon.UndoRedo.UndoRedoManager.Instance.ClearCommands(UndoRedoKey);
        }

        #endregion

        #region Command
        private void CommandBinding_Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = EditorCommon.UndoRedo.UndoRedoManager.Instance.CanUndo(UndoRedoKey);
            e.CanExecute = true;
        }
        private void CommandBinding_Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void CommandBinding_Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = EditorCommon.UndoRedo.UndoRedoManager.Instance.CanRedo(UndoRedoKey);
            e.CanExecute = true;
        }
        private void CommandBinding_Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void CommandBinding_Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //if (mCurrentResourceInfo == null)
            //{
            //    e.CanExecute = false;
            //    return;
            //}

            //e.CanExecute = true;
        }
        private void CommandBinding_Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        EditorCommon.ViewPort.PreviewSceneControl mPreviewSceneControl = null;
        public SkeletonEditorControl()
        {
            InitializeComponent();
            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;

            PreviewMeshCtrl.OnResourceItemSelected += async (EditorCommon.Resources.ResourceInfo rInfo) =>
            {
                if (rInfo == null)
                    return;
                mCurrentResourceInfo.PreViewMesh = rInfo.ResourceName.Name;
                PreviewMeshShowBtn.IsSubmenuOpen = false;
                await PreviewMeshChanged();
                await mCurrentResourceInfo.Save();
            };

        }

        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            mSkeleton.Save();
            EngineNS.CEngine.Instance.SkeletonAssetManager.Remove(mSkeleton.Name);
        }

        async System.Threading.Tasks.Task PreviewMeshChanged()
        {
            if (mPreviewSceneControl == null)
                return;
            if (mCurrentResourceInfo.PreViewMesh == "null" || string.IsNullOrEmpty(mCurrentResourceInfo.PreViewMesh))
                return;
            List<RName> rNameList = new List<RName>();
            rNameList.Add(RName.GetRName(mCurrentResourceInfo.PreViewMesh));
            await ChangePreviewMeshesActor(rNameList);
        }
        async System.Threading.Tasks.Task ChangePreviewMeshesActor(List<RName> rNameList)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (rNameList.Count == 0)
            {
                return;
            }
            mPreviewActor = await mPreviewSceneControl.CreateUniqueActor(rNameList[0]);
            mPreviewActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);

        }
        GActor mPreviewActor = null;
        EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempAnimationSequenceEditor");
        EngineNS.RName mSkeletonAssetName = EngineNS.RName.EmptyName;
        private void PreviewMesh_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var noUse = InitPreviewMesh();
        }
        string mPreviewMeshResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Mesh;
        async System.Threading.Tasks.Task InitPreviewMesh()
        {
            if (string.IsNullOrEmpty(mPreviewMeshResourceType))
                return;

            PreviewMeshCtrl.HostControl = this;

            var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(mPreviewMeshResourceType);
            var showData = new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData()
            {
                SearchSubFolder = true,
                FileExts = meta.ResourceExts,
                CompareFuction = (info) =>
                {
                    if (info != null)
                    {
                        var meshRInfo = info as EditorCommon.ResourceInfos.MeshResourceInfo;
                        if (meshRInfo.SkeletonAsset == mCurrentResourceInfo.ResourceName.Name)
                            return true;
                    }
                    return false;
                }
            };
            showData.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
            {
                AbsFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
                RootFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
            });
            if (PreviewMeshCtrl.ShowEngineContent)
            {
                showData.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
                {
                    AbsFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                    RootFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                });
            }
            if (PreviewMeshCtrl.ShowEditorContent)
            {
                showData.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
                {
                    AbsFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                    RootFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                });
            }
            ShowSourceInDirSerialId++;
            await PreviewMeshCtrl.ShowSourcesInDir(ShowSourceInDirSerialId, showData);
        }


        private void SkeletonTreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = mSkeletonTreeViewOperation.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                mSkeletonTreeViewOperation.SelectedTreeViewItem = treeViewItem;
                treeViewItem.Focus();
                e.Handled = true;
                mSkeletonTreeViewOperation.SelectBoneDetial = treeViewItem.Header as EditorBoneDetial;
                var menu = mSkeletonTreeViewOperation.CreateContextMenu(TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as System.Windows.Style);
                mSkeletonTreeViewOperation.CreateExpandCollapseMenu(menu, treeViewItem.IsExpanded);
                treeViewItem.ContextMenu = menu;
                menu.Visibility = Visibility.Visible;
                menu.PlacementTarget = treeViewItem;
                if (mSkeletonTreeViewOperation.SelectBoneDetial.Type == BoneType.Bone)
                    mSkeletonTreeViewOperation.CreateItmeToContextMenu("Add_Socket", menu, AddSocket_Click);
                else
                    mSkeletonTreeViewOperation.CreateItmeToContextMenu("Delete", menu, DeleteSocket_Click);
                menu.IsOpen = true;
            }
        }
        private void AddSocket_Click(object sender, RoutedEventArgs e)
        {
            var bone = mSkeleton.GetBone((uint)mSkeletonTreeViewOperation.SelectBoneDetial.Index);

            var desc = bone.BoneDesc.Clone();
            desc.BindMatrix = EngineNS.Matrix.Identity * bone.BoneDesc.BindMatrix;
            desc.Name = GetValidName(bone.BoneDesc.Name + "_Socket");
            desc.Parent = bone.BoneDesc.Name;
            desc.Type = BoneType.Socket;
            CGfxBone socket = mSkeleton.NewBone(desc);
            bone.AddChild(socket.IndexInTable);

            var socketBoneDetial = new EditorBoneDetial(mSkeleton, socket, bone);
            mSkeletonTreeViewOperation.SelectBoneDetial.Children.Add(socketBoneDetial);
        }
        private void DeleteSocket_Click(object sender, RoutedEventArgs e)
        {
            var bone = mSkeleton.GetBone((uint)mSkeletonTreeViewOperation.SelectBoneDetial.Index);
            var parentBone = mSkeleton.FindBone(bone.BoneDesc.ParentHash);
            mSkeleton.DeleteBone(bone);
            var parent = GetParentFromTree(EditorBoneDetialTree, mSkeletonTreeViewOperation.SelectBoneDetial);
            parent.Children.Remove(mSkeletonTreeViewOperation.SelectBoneDetial);
        }
        private string GetValidName(string name)
        {
            string validname = name;
            if (mSkeleton.FindBone(name) != null)
            {
                var lastChar = name.Substring(name.Length - 1);
                int count = 0;
                if (int.TryParse(lastChar, out count))
                {
                    var temp = name.Substring(0, name.Length - 1);
                    temp = temp + (count + 1).ToString();
                    validname = temp;
                }
                else
                {
                    validname += "1";
                }
                return GetValidName(validname);
            }
            return validname;
        }

        private void TreeView_Skeleton_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = TreeView_Skeleton.SelectedItem;
            mSkeletonTreeViewOperation.SelectBoneDetial = item as EditorBoneDetial;
            ProGrid.Instance = mSkeletonTreeViewOperation.SelectBoneDetial;
        }

        private void TreeView_Skeleton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SkeletonContexMenu.IsOpen = false;
            ProGrid.Instance = null;
        }

        private void TreeView_Skeleton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SkeletonContexMenu.IsOpen = false;
            e.Handled = true;
        }
    }
}

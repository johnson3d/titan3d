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
using EditorCommon.Resources;
using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.Animation.Skeleton;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics.Mesh;
using EngineNS.IO;

namespace AnimationAdditiveBlendSpace2DEditor
{
    public class AnimationName : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        RName mName;
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Mesh)]
        public RName Name
        {
            get => mName;
            set
            {
                mName = value;
                OnNameChanged?.Invoke();
                OnPropertyChanged("Name");
            }
        }

        public Action OnNameChanged;
    }
    /// <summary>
    /// Interaction logic for AdditiveBlendSpace2DEditorControl.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "AnimationAdditiveBlendSpace2DEditor")]
    //[EditorCommon.PluginAssist.PluginMenuItem("工具(_T)/AnimationAdditiveBlendSpace2D1DEditor")]
    [Guid("87780E3E-7431-4BA1-A657-646B5E380B13")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class AdditiveBlendSpace2DEditorControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin, EngineNS.ITickInfo, CodeGenerateSystem.Base.INodesContainerHost, EditorCommon.Controls.ResourceBrowser.IContentControlHost
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
        //bool IsBlendCtrlInit = false;
        public void BeforeFrame()
        {

        }
        public void TickLogic()
        {

            BlendControl.TickLogic();
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
        public string PluginName => "AnimationAdditiveBlendSpace2DEditor";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(AdditiveBlendSpace2DEditorControl), new FrameworkPropertyMetadata(null));
        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Material_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(AdditiveBlendSpace2DEditorControl), new FrameworkPropertyMetadata(null));
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
        EditorCommon.ResourceInfos.AnimationAdditiveBlendSpace2DResourceInfo mCurrentResourceInfo = null;
        AdditiveBlendSpace2D mBlendSpace = null;
        CGfxSkeletonPose mPose = null;
        AnimationAdditiveBlendSpaceDetial mBlendSpaceDetial = null;
        public async System.Threading.Tasks.Task SetObjectToEdit(ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var info = context.ResInfo as EditorCommon.ResourceInfos.AnimationAdditiveBlendSpace2DResourceInfo;
            if (mCurrentResourceInfo == null || mCurrentResourceInfo.ResourceName != info.ResourceName)
            {
                mCurrentResourceInfo = info;
                SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                IconBrush = context.ResInfo.ResourceTypeBrush;

                mBlendSpace = EditorCommon.Utility.EditableAnimationOperation.CreateAdditiveBlendSpace2D(mCurrentResourceInfo.ResourceName);
                mBlendSpace.Pause = true;
                var skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, mBlendSpace.SkeletonAsset);
                mPose = skeleton.CreateSkeletonPose();
                //mPose.InitDefaultPose();
                mBlendSpace.BasePose = mPose.Clone();
                mBlendSpace.Pose = mPose;
                if (mCurrentResourceInfo.ReferenceRNameList.Count == 0 && mBlendSpace.Samples.Count > 0)
                {
                    for (int i = 0; i < mBlendSpace.Samples.Count; ++i)
                    {
                        mCurrentResourceInfo.ReferenceRNameList.Add(mBlendSpace.Samples[i].AnimationName);
                    }
                    await mCurrentResourceInfo.Save();
                }

                CreateSkeletonTreeView(skeleton);
                BlendControl.AnimationBlendSpace = mBlendSpace;
                mBlendSpaceDetial = new AnimationAdditiveBlendSpaceDetial(BlendControl);
                BlendControl.BlendSpaceDetial = mBlendSpaceDetial;
                mBlendSpaceDetial.PropertyChanged += BlendSpaceDetial_PropertyChanged;
                ProGrid.Instance = mBlendSpaceDetial;
                mBlendSpaceDetial.ReferenceAnimationName = mBlendSpace.ReferenceAnimation;
                var noUse = InitContentBrowserItems();

                await mPreviewSceneControl.Initialize(mSceneName);
                ProGrid_PreviewScene.Instance = mPreviewSceneControl;

                await EditorCommon.Utility.PreviewHelper.GetPreviewMeshBySkeleton(mCurrentResourceInfo);
                await PreviewMeshChanged();
                CEngine.Instance.TickManager.AddTickInfo(this);
                mBlendSpaceDetial.PreviewAnimationName = RName.GetRName(mCurrentResourceInfo.PreviewAnimation);
            }
        }
        private void BlendSpaceDetial_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PreviewAnimationName")
            {
                var detial = sender as AnimationAdditiveBlendSpaceDetial;
                mPreviewAnimationCom.BaseAnimationName = detial.PreviewAnimationName;
                mCurrentResourceInfo.PreviewAnimation = detial.PreviewAnimationName.Name;
                Action infoSave = async () =>
                {
                    await mCurrentResourceInfo.Save();
                };
                infoSave.Invoke();
            }
        }
        void CreateSkeletonTreeView(CGfxSkeleton skeleton)
        {
            skeleton.GenerateHierarchy();
            var root = skeleton.Root;
            ObservableCollection<EditorBoneDetial> temp = new ObservableCollection<EditorBoneDetial>();
            var editorBoneDetial = new EditorBoneDetial(skeleton, root, null);
            temp.Add(editorBoneDetial);
            TreeView_Skeleton.ItemsSource = temp;
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
            CEngine.Instance.TickManager.RemoveTickInfo(this);
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

        List<CGfxMesh> mPreviewMeshes = new List<CGfxMesh>();
        ObservableCollection<AnimationName> mPreviewMeshesName = new ObservableCollection<AnimationName>();
        [EngineNS.Editor.Editor_UseCustomEditorAttribute]
        public ObservableCollection<AnimationName> PreviewMeshesName
        {
            get { return mPreviewMeshesName; }
            set { mPreviewMeshesName = value; }
        }
        public static Dictionary<string, string> SkeletonAssetMeshDic = new Dictionary<string, string>();
        EditorCommon.ViewPort.PreviewSceneControl mPreviewSceneControl = null;
        public AdditiveBlendSpace2DEditorControl()
        {
            InitializeComponent();
            BlendControl.OnAnimationAssetDrop += BlendControl_OnAnimationAssetDrop;
            BlendControl.OnAnimationAssetMove += BlendControl_OnAnimationAssetMove;
            BlendControl.OnAnimationAssetDelete += BlendControl_OnAnimationAssetDelete;

            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;

            PreviewMeshesName.CollectionChanged += PreviewMeshesName_CollectionChanged;

            PreviewMeshCtrl.HostControl = this;
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

        private void BlendControl_OnAnimationAssetDelete(object sender, AnimationAssetOperateEventArgs e)
        {
            mCurrentResourceInfo.ReferenceRNameList.Remove(e.SrcNode.Name);
        }

        private void BlendControl_OnAnimationAssetMove(object sender, EditorCommon.Controls.Animation.AnimationAssetOperateEventArgs e)
        {

        }

        private void BlendControl_OnAnimationAssetDrop(object sender, EditorCommon.Controls.Animation.AnimationAssetOperateEventArgs e)
        {
            mCurrentResourceInfo.ReferenceRNameList.Add(e.SrcNode.Name);
        }

        #region ViewPort
        GActor mPreviewActor = null;
        EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempAnimationAdditiveBlendSpace2DEditor");
        EngineNS.RName mSkeletonAssetName = EngineNS.RName.EmptyName;

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

        EngineNS.GamePlay.Component.GAdditiveBlendSpace2DComponent mPreviewAnimationCom = null;
        void SetAnimationSequence2PreviewActor()
        {
            if (mPreviewActor == null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            //string skeletonAsset = null;
            var meshComp = mPreviewActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            if (meshComp != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                //skeletonAsset = skinModifier.SkeletonAssetName;
                //animationCom = new EngineNS.GamePlay.Component.GAnimationInstance(RName.GetRName(skeletonAsset));
                mPreviewAnimationCom = new EngineNS.GamePlay.Component.GAdditiveBlendSpace2DComponent();
                mPreviewAnimationCom.Pose = mPose;
                skinModifier.AnimationPose = mPreviewAnimationCom.Pose;
            }
            var mutiMeshComp = mPreviewActor.GetComponent<EngineNS.GamePlay.Component.GMutiMeshComponent>();
            bool isFirst = true;
            if (mutiMeshComp != null)
            {
                foreach (var subMesh in mutiMeshComp.Meshes)
                {
                    var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    if (isFirst)
                    {
                        mPreviewAnimationCom = new EngineNS.GamePlay.Component.GAdditiveBlendSpace2DComponent();
                        mPreviewAnimationCom.Pose = mPose;
                        isFirst = false;
                    }
                    skinModifier.AnimationPose = mPreviewAnimationCom.Pose;
                }
            }
            mPreviewAnimationCom.Animation = mBlendSpace;
            mPreviewActor.AddComponent(mPreviewAnimationCom);
        }
        #endregion

        private void PreviewMeshesName_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        foreach (var item in e.NewItems)
                        {
                            var animationName = item as AnimationName;
                            animationName.OnNameChanged = () =>
                            {
                                var noUsed = PreviewMeshesListChanged();
                            };
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.NewItems)
                        {
                            var animationName = item as AnimationName;
                            animationName.OnNameChanged = null;
                        }
                    }
                    break;
            }
        }
        async System.Threading.Tasks.Task PreviewMeshesListChanged()
        {
            if (mPreviewSceneControl == null)
                return;
            List<RName> rNameList = new List<RName>();
            foreach (var name in mPreviewMeshesName)
            {
                if (name.Name != null)
                    rNameList.Add(name.Name);
            }
            await ChangePreviewMeshesActor(rNameList);
            SetAnimationSequence2PreviewActor();
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
            SetAnimationSequence2PreviewActor();
        }
        async System.Threading.Tasks.Task InitContentBrowserItems()
        {
            ContentBrowser.HostControl = this;
            ShowSourceInDirSerialId++;
            await EditorCommon.Utility.PreviewHelper.InitPreviewResources(ContentBrowser, EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip, ShowSourceInDirSerialId, (info) =>
            {
                var animRInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
                if (animRInfo.GetElementProperty(EngineNS.Bricks.Animation.AnimNode.ElementPropertyType.EPT_Skeleton) == mCurrentResourceInfo.SkeletonAsset)
                    return true;
                else
                    return false;
            });
        }

        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            mCurrentResourceInfo.ReferenceRNameList.Clear();
            for (int i = 0; i < mBlendSpace.Samples.Count; ++i)
            {
                if (!mCurrentResourceInfo.ReferenceRNameList.Contains(mBlendSpace.Samples[i].AnimationName))
                    mCurrentResourceInfo.ReferenceRNameList.Add(mBlendSpace.Samples[i].AnimationName);
            }
            if (!mCurrentResourceInfo.ReferenceRNameList.Contains(mBlendSpace.SkeletonAsset))
                mCurrentResourceInfo.ReferenceRNameList.Add(mBlendSpace.SkeletonAsset);
            if (!mCurrentResourceInfo.ReferenceRNameList.Contains(mBlendSpace.ReferenceAnimation))
                mCurrentResourceInfo.ReferenceRNameList.Add(mBlendSpace.ReferenceAnimation);
            Action infoSave = async () => { await mCurrentResourceInfo.Save(); };
            infoSave.Invoke();
            mBlendSpace.Save();
        }
        private void PreviewMesh_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ShowSourceInDirSerialId++;
            var noUse = EditorCommon.Utility.PreviewHelper.InitPreviewResources(PreviewMeshCtrl, EngineNS.Editor.Editor_RNameTypeAttribute.Mesh, ShowSourceInDirSerialId, (info) =>
            {
                var meshRInfo = info as EditorCommon.ResourceInfos.MeshResourceInfo;
                if (meshRInfo.SkeletonAsset == mCurrentResourceInfo.SkeletonAsset)
                    return true;
                else
                    return false;
            });
        }
        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            BlendControl.ReDrawControl();
        }

        private void BoneTreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void BoneTreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void BoneTreeViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

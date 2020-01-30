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
using EngineNS.Bricks.SimplifyGeom;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics.Mesh;
using EngineNS.IO;

namespace MeshEditor
{
    /// <summary>
    /// Interaction logic for MeshEditorControl.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "MeshEditor")]
    //[EditorCommon.PluginAssist.PluginMenuItem("工具(_T)/MeshEditor")]
    [Guid("26DC56FF-F5C5-45DD-A2F8-8EE46DEE138A")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MeshEditorControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin, EngineNS.ITickInfo, CodeGenerateSystem.Base.INodesContainerHost, EditorCommon.Controls.ResourceBrowser.IContentControlHost
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
            return PreviewAnimationCtrl?.GetSelectedResourceInfos();
        }
        public void SelectResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            PreviewAnimationCtrl?.SelectResourceInfos(resInfo);
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
            if (mPreviewClip == null)
                return;
            AnimationPlayingCtrl.TickLogic();
            var playPercent = AnimationPlayingCtrl.PlayPercent;
            var time = mPreviewClip.Duration * playPercent;
            //mPreviewAnimationSequence.CurrentTime = (uint)time;
            mPreviewClip.SeekForEditor(time);
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
        public string PluginName => "MeshEditor";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MeshEditorControl), new FrameworkPropertyMetadata(null));
        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/StaticMesh_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MeshEditorControl), new FrameworkPropertyMetadata(null));
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
        EditorCommon.ResourceInfos.MeshResourceInfo mCurrentResourceInfo = null;
        public async System.Threading.Tasks.Task SetObjectToEdit(ResourceEditorContext context)
        {
            var info = context.ResInfo as EditorCommon.ResourceInfos.MeshResourceInfo;
            if (mCurrentResourceInfo == null || mCurrentResourceInfo.ResourceName != info.ResourceName)
            {
                mCurrentResourceInfo = info;
                SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                IconBrush = context.ResInfo.ResourceTypeBrush;

                CreateSkeletonTreeView();
                mMeshName = info.ResourceName;
                var showValue = new EditorCommon.ResourceInfos.MeshResourceInfo.MeshEditProperty();
                mEditMesh = await EngineNS.CEngine.Instance.MeshManager.GetMeshOrigion(EngineNS.CEngine.Instance.RenderContext, mMeshName);
                if (mEditMesh == null)
                    return;

                showValue.SetMesh(mEditMesh);
                ProGrid.Instance = showValue;

                await mPreviewSceneControl.Initialize(mSceneName);
                ProGrid_PreviewScene.Instance = mPreviewSceneControl;
                CreatePreviewMeshesActor(mEditMesh);

                Physxpg.Instance = mSimGeom;
                mSimGeom.DPreview = DPreviewSimMesh;
                mSimGeom.Mesh = mEditMesh;
                mSimGeom.DSavePhyMesh = DSavePhyMesh;
            }
        }

        public void DPreviewSimMesh(CSimGeom simgeom, bool show)
        {
            var showValue = new EditorCommon.ResourceInfos.MeshResourceInfo.MeshEditProperty();
            if (show)
            {
                PreViewSimMesh(simgeom);
            }
            else
            {
                mPreviewSceneControl.AddUniqueActor(mPreviewActor);
            }
        }

        public void PreViewSimMesh(CSimGeom simgeom)
        {
            if (!simgeom.Preview)
                return;

            if (simgeom.Mesh == null)
                return;

            var test = PreViewSimMeshAsync(simgeom);
        }

        public async System.Threading.Tasks.Task<bool> PreViewSimMeshAsync(CSimGeom simgeom)
        {
            if (simgeom.MaxVertices == 0)
            {
                MessageBox.Show("MaxVertices is Null error");
                return false;
            }

            return await CEngine.Instance.EventPoster.Post(() =>
            {

                if (simgeom.IsChange)
                {
                    simgeom.IsChange = false;
                    //var test = BuildTriMeshAsync();
                    simgeom.BuildTriMesh(CEngine.Instance.RenderContext, simgeom.Mesh.MeshPrimitives, ref simgeom.CCDD);
                    simgeom.MeshPrimitives = simgeom.CreateMesh(CEngine.Instance.RenderContext);
                }
                if (simgeom.MeshPrimitives != null)
                {
                    GActor actor = NewMeshActor(CEngine.Instance.MeshManager.CreateMesh(CEngine.Instance.RenderContext, simgeom.MeshPrimitives));
                    actor.Placement.SetMatrix(ref mPreviewActor.Placement.mDrawTransform);
                    var test = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>().SetMaterialInstanceAsync(CEngine.Instance.RenderContext, 0, CEngine.Instance.MaterialInstanceManager.DefaultMaterialInstance, null);
                    mPreviewSceneControl.AddUniqueActor(actor);
                }
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);//, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor)
        }

        public void DSavePhyMesh(EngineNS.Graphics.Mesh.CGfxMeshPrimitives MeshPrimitives, string type)
        {
            if (MeshPrimitives == null)
                return;
 
            MeshPrimitives.SetRName(mEditMesh.Name);
            if (type.Equals("Triangle"))
            {
                MeshPrimitives.CookAndSavePhyiscsGeomAsTriMesh(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext);
            }
            else if (type.Equals("Convex"))
            {
                MeshPrimitives.CookAndSavePhyiscsGeomAsConvex(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext);
            }

            Action action = async () =>
            {
                var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(MeshPrimitives.Name.Address + (type.Equals("Triangle") ? CEngineDesc.PhyTriangleMeshGeom : CEngineDesc.PhyConvexGeom) );
                await info.Save();
            };
            action();
        }

        //private void Button_Preview(object sender, RoutedEventArgs e)
        //{
        //    mSimGeom.Preview = true;
        //}

        private void Button_SaveConvexGeom(object sender, RoutedEventArgs e)
        {
            mSimGeom.SaveConvexGeom = true;
        }
        private void Button_SaveTriMesh(object sender, RoutedEventArgs e)
        {
            mSimGeom.SaveTriMesh = true;
        }

        void CreateSkeletonTreeView()
        {
            if (string.IsNullOrEmpty(mCurrentResourceInfo.SkeletonAsset) || mCurrentResourceInfo.SkeletonAsset == "null")
                return;
            var bones = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, RName.GetRName(mCurrentResourceInfo.SkeletonAsset));
            bones.GenerateHierarchy();
            var root = bones.Root;
            if (root != null)
            {
                ObservableCollection<EditorBoneDetial> temp = new ObservableCollection<EditorBoneDetial>();
                var editorBoneDetial = new EditorBoneDetial(bones, root, null);
                temp.Add(editorBoneDetial);
                TreeView_Skeleton.ItemsSource = temp;
            }
            else
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Skeleton", $"Skeleton {RName.GetRName(mCurrentResourceInfo.SkeletonAsset)} don't have root bone");
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
        EngineNS.Bricks.SimplifyGeom.CSimGeom mSimGeom = null;
        EditorCommon.ViewPort.PreviewSceneControl mPreviewSceneControl = null;

        public MeshEditorControl()
        {
            InitializeComponent();
            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;
            PreviewAnimationCtrl.HostControl = this;
            PreviewAnimationCtrl.OnResourceItemSelected += (EditorCommon.Resources.ResourceInfo rInfo) =>
            {
                if (rInfo == null)
                    return;

                ChangePreviewAnimation(rInfo.ResourceName);
            };
            CEngine.Instance.TickManager.AddTickInfo(this);

            mSimGeom = new EngineNS.Bricks.SimplifyGeom.CSimGeom();
        }

        GActor mPreviewActor = null;
        EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempMeshEditor");
        EngineNS.RName mSkeletonAssetName = EngineNS.RName.EmptyName;
        EngineNS.RName mMeshName = EngineNS.RName.EmptyName;
        EngineNS.Graphics.Mesh.CGfxMesh mEditMesh = null;


        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            mEditMesh.SaveMesh();
            var nu = mCurrentResourceInfo.Save(true);
        }
        
        private void PreviewAnimation_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            //var noUse = InitPreviewAnimation();
            ShowSourceInDirSerialId++;
            var noUse = EditorCommon.Utility.PreviewHelper.InitPreviewResources(PreviewAnimationCtrl, EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip, ShowSourceInDirSerialId,(info)=>
            {
                var animRInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
                if (animRInfo.SkeletonAsset == mCurrentResourceInfo.SkeletonAsset)
                    return true;
                else
                    return false;
            });
        }

        async System.Threading.Tasks.Task InitPreviewAnimation()
        {
            var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip);
            var showData = new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData()
            {
                SearchSubFolder = true,
                FileExts = meta.ResourceExts,
                CompareFuction = (info) =>
                {
                    if (info != null)
                    {
                        var animRInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
                        if (animRInfo.SkeletonAsset == mCurrentResourceInfo.SkeletonAsset)
                            return true;
                    }
                    return false;
                },
            };
            showData.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
            {
                AbsFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
                RootFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
            });
            if(PreviewAnimationCtrl.ShowEngineContent)
            {
                showData.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
                {
                    AbsFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                    RootFolder = EngineNS.CEngine.Instance.FileManager.EngineContent,
                });
            }
            if (PreviewAnimationCtrl.ShowEditorContent)
            {
                showData.FolderDatas.Add(new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
                {
                    AbsFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                    RootFolder = EngineNS.CEngine.Instance.FileManager.EditorContent,
                });
            }
            ShowSourceInDirSerialId++;
            await PreviewAnimationCtrl.ShowSourcesInDir(ShowSourceInDirSerialId, showData);
        }

        #region
        void CreatePreviewMeshesActor(CGfxMesh mesh)
        {
            if (mesh == null)
                return;
            mPreviewActor = NewMeshActor(mesh);
            mPreviewActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
            mPreviewSceneControl.AddUniqueActor(mPreviewActor);

            mPreviewActor.GetComponentMesh(null).SetPassUserFlags(1);

            var nu = InitUIDrawer();
        }
        async System.Threading.Tasks.Task InitUIDrawer()
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var font = CEngine.Instance.FontManager.GetFont(EngineNS.CEngine.Instance.Desc.DefaultFont, 24, 1024, 128);
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/font.instmtl"));

            var textMeshInfo = new EngineNS.Bricks.FreeTypeFont.CFontMesh();
            await textMeshInfo.SetMaterial(rc, mtl, "txDiffuse");
            textMeshInfo.DrawText(rc, font, "模型", true);

            textMeshInfo.RenderMatrix = EngineNS.Matrix.Translate(20, 0, 0);
            //textMeshInfo.Offset = new Vector2(20, 0);
            //textMeshInfo.Scale = new Vector2(1, 1);

            var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
            rp.OnDrawUI += (cmd, view) =>
            {
                var mesh = mPreviewActor.GetComponentMesh(null);
                string outInfo = $"材质数：{mesh.MeshPrimitives.AtomNumber}:";
                for (int i = 0; i < mesh.MeshPrimitives.AtomNumber; i++)
                {
                    CDrawPrimitiveDesc desc = new CDrawPrimitiveDesc();
                    mesh.MeshPrimitives.GetAtom((UInt32)i, 0, ref desc);

                    var lodLvl = mesh.MeshPrimitives.GetLodLevel((UInt32)i, CurLod);

                    CDrawPrimitiveDesc desc1 = new CDrawPrimitiveDesc();
                    mesh.MeshPrimitives.GetAtom((UInt32)i, lodLvl, ref desc1);

                    outInfo += $"({desc1.NumPrimitives}/{desc.NumPrimitives}) ";
                }

                textMeshInfo.DrawText(rc, font, outInfo, true);
                for (int i = 0; i < textMeshInfo.PassNum; i++)
                {
                    var pass = textMeshInfo.GetPass(i);
                    if (pass == null)
                        continue;

                    pass.ViewPort = view.Viewport;
                    pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                    pass.ShadingEnv.BindResources(textMeshInfo.Mesh, pass);

                    cmd.PushPass(pass);
                }
            };
        }
        AnimationClip mPreviewClip = null;
        void ChangePreviewAnimation(RName animName)
        {
            if (mPreviewActor == null)
                return;
            mPreviewClip = AnimationClip.CreateSync(animName);
            mPreviewClip.Pause = true;
            AnimationPlayingCtrl.AnimationName = mPreviewClip.Name.PureName();
            AnimationPlayingCtrl.TotalFrame = mPreviewClip.Duration * mPreviewClip.SampleRate;
            AnimationPlayingCtrl.Duration = mPreviewClip.Duration;
            SetAnimationSequence2PreviewActor();
        }
        GActor NewMeshActor(CGfxMesh mesh)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            meshComp.SetSceneMesh(rc.ImmCommandList, mesh);
            actor.AddComponent(meshComp);
            return actor;
        }

        void SetAnimationSequence2PreviewActor()
        {
            if (mPreviewActor == null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            EngineNS.GamePlay.Component.GAnimationComponent animationCom = null;
            var meshComp = mPreviewActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            if (meshComp != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                animationCom = new EngineNS.GamePlay.Component.GAnimationComponent(RName.GetRName(skinModifier.SkeletonAsset));
            }
            animationCom.Animation = mPreviewClip;
            mPreviewActor.AddComponent(animationCom);
            mPreviewClip.Bind(animationCom.Pose);
        }

        #endregion

        private void BoneTreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void BoneTreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void BoneTreeViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        bool WireFrame = false;
        private void IconTextBtn_Click_WireFrame(object sender, RoutedEventArgs e)
        {
            WireFrame = !WireFrame;
            if (WireFrame)
            {
                var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
                rp.mForwardBasePass.SetPassBuiltCallBack(EngineNS.CCommandList.OnPassBuilt_WireFrame);
            }
            else
            {
                var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
                rp.mForwardBasePass.SetPassBuiltCallBack(null);
            }
        }
        float CurLod = 0;
        private void IconTextBtn_Click_Decrease(object sender, RoutedEventArgs e)
        {
            CurLod += 0.01f;
            if (CurLod > 1)
                CurLod = 0;
            mPreviewActor.GetComponentMesh(null).SetLodLevel(CurLod);
        }
        private void IconTextBtn_Click_Increase(object sender, RoutedEventArgs e)
        {
            CurLod -= 0.01f;
            if (CurLod < 0)
                CurLod = 0;
            mPreviewActor.GetComponentMesh(null).SetLodLevel(CurLod);
        }
    }
}

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

namespace PrefabEditor
{

    /// <summary>
    /// Interaction logic for SkeletonEditorControl.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "PrefabEditor")]
    //[EditorCommon.PluginAssist.PluginMenuItem("工具(_T)/SkeletonEditor")]
    [Guid("780B12C4-7D03-4A91-87EB-4E1C2EE1D842")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PrefabEditorControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin, EngineNS.ITickInfo, CodeGenerateSystem.Base.INodesContainerHost, EditorCommon.Controls.ResourceBrowser.IContentControlHost
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
                return "PrefabEditOperation";
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
            return null;
        }
        public void SelectResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            //PreviewMeshCtrl?.SelectResourceInfos(resInfo);
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
        public string PluginName => "PrefabEditor";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PrefabEditorControl), new FrameworkPropertyMetadata(null));
        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Skeleton_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(PrefabEditorControl), new FrameworkPropertyMetadata(null));
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
        EditorCommon.ResourceInfos.PrefabResourceInfo mCurrentResourceInfo = null;
        public async System.Threading.Tasks.Task SetObjectToEdit(ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var info = context.ResInfo as EditorCommon.ResourceInfos.PrefabResourceInfo;
            if (mCurrentResourceInfo == null || mCurrentResourceInfo.ResourceName != info.ResourceName)
            {
                mCurrentResourceInfo = info;
                SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                IconBrush = context.ResInfo.ResourceTypeBrush;
                await mPreviewSceneControl.Initialize(mSceneName);
                mPreviewSceneControl.ViewPort.CanDragIn = false;
                mPreviewSceneControl.ViewPort.CanDuplicatePrefab = false;
                Prefab = await CEngine.Instance.PrefabManager.GetPrefab(CEngine.Instance.RenderContext, mCurrentResourceInfo.ResourceName, true);
                ProGrid_PreviewScene.Instance = mPreviewSceneControl;
                //mPrefabHolder.World = mPreviewSceneControl.PreviewWorld;
                var item = PrefabCtrl.BindingPrefab(Prefab);
                item.IsExpanded = true;
                PrefabCtrl.SetViewPort(mPreviewSceneControl.ViewPort);
                //mPreviewSceneControl.PreviewActorList.Add(mPrefabHolder.Prefab);
                mPreviewSceneControl.ViewPort.FocusShow(mPreviewSceneControl.PreviewActorList);
                List<GActor> list = new List<GActor>();
                ComsCtrl.SetActor(Prefab);
                mPreviewSceneControl.AddUniqueActor(Prefab);
                CEngine.Instance.TickManager.AddTickInfo(this);
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
        public PrefabEditorControl()
        {
            InitializeComponent();
            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;
            ComsCtrl.LinkedPropertyGrid = ProGrid;
            PrefabCtrl.LinkedPropertyGrid = ProGrid;
            PrefabCtrl.OnSelectedActorsChanged += PrefabCtrl_OnSelectedActorsChanged;
            //mPreviewSceneControl.ViewPort.OnSelectAcotrs += ViewPort_OnSelectAcotrs;
            ComsCtrl.ViewPort = mPreviewSceneControl.ViewPort;
            PrefabCtrl.UndoRedoKey = UndoRedoKey;
            ComsCtrl.UndoRedoKey = UndoRedoKey;
        }

        private void PrefabCtrl_OnSelectedActorsChanged(object sender, List<GActor> actors)
        {
            //bool haveInvisible = false;
            List<GActor> showActors = new List<GActor>();
            for (int i = 0; i < actors.Count; ++i)
            {
                if (actors[i] != null)
                {
                    if (!(actors[i].Tag is EditorCommon.Controls.Outliner.InvisibleInOutliner))
                    {
                        showActors.Add(actors[i]);
                    }
                    else
                    {
                        //haveInvisible = true;
                    }
                }
            }
            mPreviewSceneControl.PreviewActorList = showActors;
            if (showActors == null || showActors.Count == 0)
                ComsCtrl.Visibility = Visibility.Collapsed;
            else
            {
                ComsCtrl.Visibility = Visibility.Visible;
                ComsCtrl.SetActors(showActors);
            }
        }

        private void ViewPort_OnSelectAcotrs(object sender, EditorCommon.ViewPort.ViewPortControl.SelectActorData[] e)
        {
            if (sender != mPreviewSceneControl.ViewPort)
                return;
            //bool haveInvisible = false;
            List<GActor> actors = new List<GActor>();
            for (int i = 0; i < e.Length; ++i)
            {
                if (e[i].Actor != null)
                {
                    if (!(e[i].Actor.Tag is EditorCommon.Controls.Outliner.InvisibleInOutliner))
                    {
                        actors.Add(e[i].Actor);
                    }
                    else
                    {
                        //haveInvisible = true;
                    }
                }
            }
            mPreviewSceneControl.PreviewActorList = actors;
            if (actors == null || actors.Count == 0)
                ComsCtrl.Visibility = Visibility.Collapsed;
            else
            {
                ComsCtrl.Visibility = Visibility.Visible;
                ComsCtrl.SetActors(actors);
            }
        }
        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            //mCurrentResourceInfo.Save();
            Action action = async () =>
             {
                 Prefab.SavePrefab(mCurrentResourceInfo.ResourceName);
                 //var rinfo = new PrefabResourceInfo();
                 var rinfo = EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfo("Prefab") as PrefabResourceInfo;
                 rinfo.Load(mCurrentResourceInfo.ResourceName.Address + ".rinfo");
                 await rinfo.RefreshReferenceRNames(Prefab);
                 await rinfo.Save(mCurrentResourceInfo.ResourceName.Address + ".rinfo");

                 var prefab = await Prefab.Clone(CEngine.Instance.RenderContext) as GPrefab;
                 if (prefab == null)
                 {
                     System.Diagnostics.Debug.Assert(false);
                 }
                 System.Diagnostics.Debug.Assert(prefab != null);
                 CEngine.Instance.PrefabManager.Prefabs[mCurrentResourceInfo.ResourceName] = prefab;
             };
            action.Invoke();
        }
        GPrefab mPrefab = null;
        public GPrefab Prefab 
        {
            get => mPrefab;
            set
            {
                mPrefab = value;
                if(value == null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
        }
        EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempPrefabEditor");

    }
}

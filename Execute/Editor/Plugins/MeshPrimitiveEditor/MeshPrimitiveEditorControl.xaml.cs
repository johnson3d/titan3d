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
using EngineNS.GamePlay;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics.Mesh;
using EngineNS.IO;

namespace MeshPrimitiveEditor
{
    /// <summary>
    /// Interaction logic for MeshPrimitiveEditorControl.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "MeshPrimitiveEditor")]
    //[EditorCommon.PluginAssist.PluginMenuItem("工具(_T)/AnimationBlendSpace1DEditor")]
    [Guid("7F3F4FFE-6CA8-4711-B84A-3483290D243B")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MeshPrimitiveEditorControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin, EngineNS.ITickInfo, CodeGenerateSystem.Base.INodesContainerHost
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
        public string PluginName => "MeshPrimitiveEditor";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MeshPrimitiveEditorControl), new FrameworkPropertyMetadata(null));
        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/StaticMesh_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MeshPrimitiveEditorControl), new FrameworkPropertyMetadata(null));
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
        EditorCommon.ResourceInfos.MeshSourceResourceInfo mCurrentResourceInfo = null;
        public async System.Threading.Tasks.Task SetObjectToEdit(ResourceEditorContext context)
        {
            var info = context.ResInfo as EditorCommon.ResourceInfos.MeshSourceResourceInfo;
            if (mCurrentResourceInfo == null || mCurrentResourceInfo.ResourceName != info.ResourceName)
            {
                mCurrentResourceInfo = info;
                SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                IconBrush = context.ResInfo.ResourceTypeBrush;

                await mPreviewSceneControl.Initialize(mSceneName);
                ProGrid_PreviewScene.Instance = mPreviewSceneControl;
                CreateSkeletonTreeView();
                mMeshPrimitiveName = info.ResourceName;
                mEditMeshPrimitive = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(CEngine.Instance.RenderContext, mMeshPrimitiveName);
                await CreatePreviewMeshesActor(mEditMeshPrimitive, true);
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
        public MeshPrimitiveEditorControl()
        {
            InitializeComponent();
            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;

            mPreviewSceneControl.ViewPort.SetDrawPanelMouseDownCallback(Viewport_MouseDown);
            mPreviewSceneControl.ViewPort.SetDrawPanelMouseUpCallback(Viewport_MouseUp);
            mPreviewSceneControl.ViewPort.SetDrawPanelMouseMoveCallback(Viewport_MouseMove);

            CEngine.Instance.TickManager.AddTickInfo(this);
        }
        EngineNS.Rectangle SelectRect = new EngineNS.Rectangle();
        EngineNS.Bricks.GraphDrawer.GraphLines mGraphLines = null;
        EngineNS.Bricks.GraphDrawer.McMulLinesGen mLineGen = new EngineNS.Bricks.GraphDrawer.McMulLinesGen();
        void Viewport_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            SelectRect.X = e.X;
            SelectRect.Y = e.Y;

            var element = sender as EditorCommon.ViewPort.ViewPortControl;
            Mouse.Capture(element);
        }
        void Viewport_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                
            }
        }
        void Viewport_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            SelectRect.Width = e.X - SelectRect.X;
            SelectRect.Height = e.Y - SelectRect.Y;
            Mouse.Capture(null);

            if (this.mPreviewActor == null)
                return;

            List<int> verts = new List<int>();

            if (e.Button == System.Windows.Forms.MouseButtons.Left && e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                if (mProgressMesh == null)
                    return;
                mProgressMesh.GetContainVertices(SelectRect, mPreviewSceneControl.ViewPort.Camera, verts);

                var un = UpdateSelected(verts);
            }
        }

        List<EngineNS.GamePlay.Actor.GActor> VertActors = new List<GActor>();
        List<int> SelectedVerts;
        private async System.Threading.Tasks.Task UpdateSelected(List<int> verts)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            foreach(var i in VertActors)
            {
                mPreviewSceneControl.RemoveActor(i);
            }
            VertActors.Clear();

            SelectedVerts = verts;

            if (verts.Count == 0)
                return;

            float bvl = this.mPreviewActor.GetComponentMesh(null).MeshPrimitives.AABB.GetMaxSide() / 20;
            var size = new EngineNS.Vector3(bvl, bvl, bvl);
            var mtl0 = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("TitanDemo/greentest.instmtl"));
            var mtl1 = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("TitanDemo/redtest.instmtl"));
            foreach (var i in verts)
            {
                var curVert = mProgressMesh.Vertices[i];
                var center = curVert.Position;

                var mesh = await EngineNS.CEngine.Instance.MeshManager.CreateMeshAsync(rc, RName.GetRName("editor/basemesh/box_center.gms"));

                var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mesh);
                actor.Placement.Location = center;
                actor.Placement.Scale = size;
                actor.GetComponentMesh(null).SetMaterialInstance(rc, 0, mtl0, null);

                VertActors.Add(actor);
                mPreviewSceneControl.AddDynamicActor(actor);

                foreach(var j in curVert.Linker.Vertices)
                {
                    curVert = mProgressMesh.Vertices[j];
                    center = curVert.Position;

                    mesh = await EngineNS.CEngine.Instance.MeshManager.CreateMeshAsync(rc, RName.GetRName("editor/basemesh/box_center.gms"));

                    actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mesh);
                    actor.Placement.Location = center;
                    actor.Placement.Scale = size;
                    actor.GetComponentMesh(null).SetMaterialInstance(rc, 0, mtl1, null);

                    VertActors.Add(actor);
                    mPreviewSceneControl.AddDynamicActor(actor);
                }
            }

            if (mGraphLines == null || mGraphLines.GraphActor==null)
            {
                mGraphLines = new EngineNS.Bricks.GraphDrawer.GraphLines();
                mGraphLines.LinesGen = mLineGen;
                mGraphLines.LinesGen.Interval = 0f;
                mGraphLines.LinesGen.Segement = 100f;
                RName lineMaterial = RName.GetRName("editor/icon/icon_3d/material/line_df.instmtl");

                var curVert = mProgressMesh.Vertices[verts[0]];
                var start = curVert.Position;
                mGraphLines.LinesGen.Start = start;

                var lnkVerts = curVert.Linker.Vertices;
                var end = mProgressMesh.Vertices[lnkVerts[0]];

                mLineGen.SetVector3Points(new Vector3[] { start, end.Position });
                var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
                    EngineNS.CEngine.Instance.RenderContext,
                    lineMaterial);
                await mGraphLines.Init(mtl, 0);

                mGraphLines.UpdateGeomMesh(CEngine.Instance.RenderContext, 0);
                mGraphLines.GraphActor.Placement.Location = Vector3.Zero;

                mPreviewSceneControl.AddDynamicActor(mGraphLines.GraphActor);
            }
            else
            {
                mGraphLines.GraphActor.Visible = verts.Count > 0;

                var curVert = mProgressMesh.Vertices[verts[0]];
                var start = curVert.Position;
                mGraphLines.LinesGen.Start = start;

                var lnkVerts = curVert.Linker.Vertices;
                List<Vector3> points = new List<Vector3>();
                foreach (var i in verts)
                {
                    curVert = mProgressMesh.Vertices[i];
                    start = curVert.Position;
                    foreach (var j in curVert.Linker.Vertices)
                    {
                        curVert = mProgressMesh.Vertices[j];
                        var end = curVert.Position;

                        points.Add(start);
                        points.Add(end);
                    }
                }

                mLineGen.SetVector3Points(points.ToArray());
                mGraphLines.UpdateGeomMesh(CEngine.Instance.RenderContext, bvl * 0.1f);
            }
        }

        void CreateSkeletonTreeView()
        {
            if (string.IsNullOrEmpty(mCurrentResourceInfo.SkeletonAsset) || mCurrentResourceInfo.SkeletonAsset == "null")
                return;
            var bones = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, RName.GetRName(mCurrentResourceInfo.SkeletonAsset));
            bones.GenerateHierarchy();
            var root = bones.Root;
            ObservableCollection<EditorBoneDetial> temp = new ObservableCollection<EditorBoneDetial>();
            var editorBoneDetial = new EditorBoneDetial(bones, root,null);
            temp.Add(editorBoneDetial);
            TreeView_Skeleton.ItemsSource = temp;
        }

        GActor mPreviewActor = null;
        EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempMeshPrimitiveEditor");
        EngineNS.RName mSkeletonAssetName = EngineNS.RName.EmptyName;
        EngineNS.RName mMeshPrimitiveName = EngineNS.RName.EmptyName;
        EngineNS.Graphics.Mesh.CGfxMeshPrimitives mEditMeshPrimitive = null;

        EngineNS.Bricks.MeshProcessor.ProgressMeshProcessor mProgressMesh = new EngineNS.Bricks.MeshProcessor.ProgressMeshProcessor();
        async System.Threading.Tasks.Task CreatePreviewMeshesActor(CGfxMeshPrimitives meshPri, bool initProgressMesh)
        {
            mPreviewActor = await NewMeshActor(meshPri);
            mPreviewActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
            //mPreviewActor.Placement.Scale *= 0.01f;
            mPreviewSceneControl.AddUniqueActor(mPreviewActor,true);

            mPreviewActor.GetComponentMesh(null).SetPassUserFlags(1);

            //mProgressMesh = new EngineNS.Bricks.MeshProcessor.ProgressMeshProcessor();
            if (initProgressMesh)
            {
                mProgressMesh.InitMesh(EngineNS.CEngine.Instance.RenderContext, meshPri);
                mProgressMesh.ClearLods();
                mProgressMesh.PushLod(0.97f);
                mProgressMesh.PushLod(0.93f);
                mProgressMesh.PushLod(0.9f);
                mProgressMesh.PushLod(0.87f);
                mProgressMesh.PushLod(0.83f);
                mProgressMesh.PushLod(0.8f);
                mProgressMesh.PushLod(0.75f);
                mProgressMesh.PushLod(0.7f);
                mProgressMesh.PushLod(0.6f);
                mProgressMesh.PushLod(0.5f);
                mProgressMesh.PushLod(0.3f);
                mProgressMesh.PushLod(0.1f);
                mProgressMesh.PushLod(0.0f);
            }

            await InitUIDrawer();
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
        async System.Threading.Tasks.Task<GActor> NewMeshActor(CGfxMeshPrimitives meshPri)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            var mesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshPri);
            var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/defaultmaterial.instmtl"));
            for (int i = 0; i < mesh.MtlMeshArray.Length; ++i)
            {
                await mesh.SetMaterialInstanceAsync(rc, (uint)i, mtl, null);
            }
            meshComp.SetSceneMesh(rc.ImmCommandList, mesh);
            actor.AddComponent(meshComp);

            return actor;
        }
        private void PreviewMesh_SubmenuOpened(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            if(mProgressMesh!=null && mCurrentResourceInfo!=null)
            {
                var rc = EngineNS.CEngine.Instance.RenderContext;
                var mesh = mProgressMesh.CookMesh(rc);
                mesh.SaveMesh(mCurrentResourceInfo.ResourceName.Address);
                EngineNS.CEngine.Instance.MeshPrimitivesManager.RefreshMeshPrimitives(rc, mCurrentResourceInfo.ResourceName);
            }
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

        bool WireFrame = false;
        private void IconTextBtn_Click_WireFrame(object sender, RoutedEventArgs e)
        {
            WireFrame = !WireFrame;
            if(WireFrame)
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

        //bool CanBuild = true;
        int NextVert = -1;
        private void IconTextBtn_Click_BuildLods(object sender, RoutedEventArgs e)
        {
            //if (CanBuild == false)
            //    return;
            //CanBuild = false;
            while (true)
            {
                int vert;
                //if (SelectedVerts == null || SelectedVerts.Count == 0)
                    vert = mProgressMesh.FindMinor(null);
                //else
                //{
                //    vert = SelectedVerts[0];
                //    mProgressMesh.DftGetVertexValue(vert, mProgressMesh.Vertices[vert].Linker);
                //}
                if (vert >= 0)
                {
                    System.Diagnostics.Debug.WriteLine(mProgressMesh.Vertices[vert].Linker.VertexValue);
                    
                    mProgressMesh.MergeVertices(vert);
                    mProgressMesh.FixDeletedFaces();
                }
                else
                {
                    break;
                    //CanBuild = true;
                    //return;
                }
            }

            mProgressMesh.BuildLodInfos();

            Action act = async () =>
            {
                await ShowCurrentProgressMesh();

                NextVert = mProgressMesh.FindMinor(null);
                if (NextVert >= 0)
                {
                    var nextCollapse = new List<int>();
                    nextCollapse.Add(NextVert);
                    await UpdateSelected(nextCollapse);
                }
                //CanBuild = true;
            };
            act();
        }
        private async System.Threading.Tasks.Task ShowCurrentProgressMesh()
        {
            if (mPreviewActor != null)
                mPreviewSceneControl.RemoveActor(mPreviewActor);
            var rc = EngineNS.CEngine.Instance.RenderContext;
            mEditMeshPrimitive = mProgressMesh.CookMesh(rc);
            await CreatePreviewMeshesActor(mEditMeshPrimitive, false);
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

        private void IconTextBtn_Click_Normal(object sender, RoutedEventArgs e)
        {
            mPreviewSceneControl.ShowNormal(mPreviewActor, RName.GetRName("TitanDemo/Character/test_girl/material/greenwireframe.instmtl"), 0.3f);
        }

        private void IconTextBtn_Click_Tangent(object sender, RoutedEventArgs e)
        {
            mPreviewSceneControl.ShowTangent(mPreviewActor, RName.GetRName("TitanDemo/Character/test_girl/material/redwireframe.instmtl"), 0.3f);
        }
    }
}

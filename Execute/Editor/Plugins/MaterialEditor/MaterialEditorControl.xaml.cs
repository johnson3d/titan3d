using System;
using System.Collections.Generic;
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
using EngineNS.IO;

namespace MaterialEditor
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "MaterialEditor")]
    //[EditorCommon.PluginAssist.PluginMenuItem("工具(_T)/MaterialEditor")]
    [Guid("8DB95031-4A06-418D-A123-10A622728206")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MaterialEditorControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin, EngineNS.ITickInfo, CodeGenerateSystem.Base.INodesContainerHost, EditorCommon.PluginAssist.IRefreshSaveFiles
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
                if (mCurrentMaterial != null)
                    return mCurrentMaterial.GetHash64().ToString();
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
        System.Threading.Tasks.Task mDirtyProcessTask = null;
        async System.Threading.Tasks.Task DirtyProcess(bool needGenerateCode = true)
        {
            if (mNeedDirtyProcess==false)
                return;
            if (mNeedDirtyProcess)
            {
                mNeedDirtyProcess = false;
                var rc = EngineNS.CEngine.Instance.RenderContext;

                if (await RefreshMaterialCode(needGenerateCode) == false)
                    return;

                foreach (var node in NodesControl.CtrlNodeList)
                {
                    var shaderVarNode = node as Controls.BaseNodeControl_ShaderVar;
                    if (shaderVarNode == null)
                        continue;
                    var varInfo = shaderVarNode.GetShaderVarInfo();
                    if (varInfo == null)
                        continue;

                    var param = mPreviewMaterial.GetParam(varInfo.VarName);
                    if (param == null)
                        continue;
                    param.CopyFrom(varInfo);

                    // Set ShaderSamplerState
                    var textureNode = shaderVarNode as Controls.ITextureSampler;
                    if (textureNode != null)
                    {
                        mPreviewMaterial.SetSamplerStateDesc(varInfo.VarName, textureNode.SamplerStateDesc);
                        //var texIdx = mPreviewMaterialInstance.FindSRVIndex(varInfo.VarName);
                        //mPreviewMaterialInstance.SetSamplerStateDesc(texIdx, textureNode.SamplerStateDesc);
                        mPreviewMaterialInstance.Material.GetSamplerStateDescs()[varInfo.VarName] = textureNode.SamplerStateDesc;
                    }

                    mPreviewMaterialInstance.SetParam(varInfo);
                }

                for (int i = 0; i < mMeshComponent.MaterialNumber; i++)
                {
                    mMeshComponent.SetMaterialInstance(rc, (uint)i, mPreviewMaterialInstance, null);
                }
            }
        }
        async System.Threading.Tasks.Task<bool> RefreshMaterialCode(bool needGenerateCode = true)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if(NodesControl.CheckError() == false)
            {
                EditorCommon.MessageBox.Show("部分节点有错误，无法生成shader");
                return false;
            }
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                // 更新MaterialInstance参数
                // 刷新预览用材质
                if(needGenerateCode)
                {
                    System.IO.TextWriter codeFile, varFile;
                    CodeGenerator.GenerateCode(NodesControl, mSetValueMaterialControl, out codeFile, out varFile);
                    // Var
                    System.IO.File.WriteAllText(mPreviewMaterial.Name.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, varFile.ToString());
                    // Code
                    System.IO.File.WriteAllText(mPreviewMaterial.Name.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, codeFile.ToString());
                    
                    mPreviewMaterial.ForceUpdateVersion();

                    UpdateMtlMacros(mPreviewMaterial);

                    mPreviewMaterialInstance.RefreshFromMaterial(mPreviewMaterial);
                }
                for (int i = 0; i < mMeshComponent.SceneMesh.MtlMeshArray.Length; i++)
                {
                    if (mMeshComponent.SceneMesh.MtlMeshArray[i] == null)
                        continue;

                    mMeshComponent.SceneMesh.MtlMeshArray[i].RefreshAllPassEffect(rc);
                }
                return true;
            });
            mRefreshPreviewMtlInst = false;
            return true;
        }
        bool mNeedDirtyProcess = false;
        public void TickSync()
        {
            if(LivePreview)
            {
                if(mDirtyProcessTask==null || mDirtyProcessTask.IsCompleted)
                    mDirtyProcessTask = DirtyProcess();
            }
        }

        #endregion

        #region IRefreshSaveFiles
        public async System.Threading.Tasks.Task RefreshSaveFiles()
        {
            var meCtrl = new MaterialEditorControl();

            foreach(var content in EngineNS.CEngine.Instance.FileManager.AllContents)
            {
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(content, "*.material.link", System.IO.SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var idx = file.LastIndexOf('.');
                    var matResFile = file.Remove(idx) + EditorCommon.Program.ResourceInfoExt;
                    var matResInfo = new ResourceInfos.MaterialResourceInfo();
                    await matResInfo.AsyncLoad(matResFile);
                    var rc = EngineNS.CEngine.Instance.RenderContext;

                    var nodesCtrl = new CodeGenerateSystem.Controls.NodesContainerControl();
                    meCtrl.mCurrentMaterial = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, matResInfo.ResourceName);
                    meCtrl.mCurrentResourceInfo = matResInfo;
                    nodesCtrl.HostControl = meCtrl;

                    var loadHolder = await EngineNS.IO.XndHolder.LoadXND(file);
                    await nodesCtrl.Load(loadHolder.Node);

                    var xndHolder = EngineNS.IO.XndHolder.NewXNDHolder();
                    nodesCtrl.Save(xndHolder.Node);
                    EngineNS.IO.XndHolder.SaveXND(file, xndHolder);


                    //var newNodesCtrl = new CodeGenerateSystem.Controls.NodesContainerControl();
                    //newNodesCtrl.HostControl = meCtrl;
                    //loadHolder = await EngineNS.IO.XndHolder.LoadXND(file);
                    //newNodesCtrl.Load(loadHolder.Node);
                }
            }
        }
        #endregion

        public UIElement InstructionControl => null;

        public string PluginName => "MaterialEditor";

        public string Version => "1.0.0";
        public bool OnActive()
        {
            return true;
        }

        public bool OnDeactive()
        {
            return true;
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MaterialEditorControl), new FrameworkPropertyMetadata(null));

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Material_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MaterialEditorControl), new FrameworkPropertyMetadata(null));

        MaterialEditor.ResourceInfos.MaterialResourceInfo mCurrentResourceInfo;
        EngineNS.Graphics.CGfxMaterial mCurrentMaterial;
        public EngineNS.Graphics.CGfxMaterial CurrentMaterial
        {
            get { return mCurrentMaterial; }
        }
        Dictionary<string, EngineNS.Graphics.CGfxMaterialParam> mCurrentMaterialParamsDic = new Dictionary<string, EngineNS.Graphics.CGfxMaterialParam>();
        EngineNS.Graphics.CGfxMaterial mPreviewMaterial;
        EngineNS.Graphics.CGfxMaterialInstance mPreviewMaterialInstance;
        EngineNS.GamePlay.Component.GMeshComponent mMeshComponent;
        public async System.Threading.Tasks.Task SetObjectToEdit(EditorCommon.Resources.ResourceEditorContext context)
        {
            IsEnabled = false;

            await mPreviewSceneControl.Initialize(EngineNS.RName.GetRName("MaterialEditorViewport"));
            mViewPortInited = false;
            ProGrid_PreviewScene.Instance = mPreviewSceneControl;
            await InitViewPort(mPreviewSceneControl.ViewPort);

            mCurrentResourceInfo = context.ResInfo as MaterialEditor.ResourceInfos.MaterialResourceInfo;
            SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
            IconBrush = context.ResInfo.ResourceTypeBrush;
            await LoadMaterialLink();
            IsDirty = false;
            IsEnabled = true;
        }
        async Task LoadMaterialLink()
        {
            if (mCurrentResourceInfo != null)
            {
                var rc = EngineNS.CEngine.Instance.RenderContext;
                mCurrentMaterial = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, mCurrentResourceInfo.ResourceName);
                UpdateUndoRedoKey();

                foreach (var param in mCurrentMaterial.ParamList)
                {
                    mCurrentMaterialParamsDic[param.VarName] = param;
                }
                // 复制材质资源到一个编辑器用临时文件夹
                var preMatRName = EngineNS.CEngine.Instance.FileManager.GetRName($"MaterialEditor/Mats/{mCurrentMaterial.GetHash64()}{EngineNS.CEngineDesc.MaterialExtension}", EngineNS.RName.enRNameType.Editor);
                var dir = preMatRName.GetDirectory();
                if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(dir))
                    EngineNS.CEngine.Instance.FileManager.CreateDirectory(dir);
                // mat
                //EngineNS.CEngine.Instance.FileManager.CopyFile(mCurrentResourceInfo.ResourceName.Address, preMatRName.Address, true);
                // var
                EngineNS.CEngine.Instance.FileManager.CopyFile(mCurrentResourceInfo.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension,
                                                               preMatRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, true);
                // code
                EngineNS.CEngine.Instance.FileManager.CopyFile(mCurrentResourceInfo.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension,
                                                               preMatRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, true);

                mMeshComponent = mMeshActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();

                // 预览用材质
                mPreviewMaterial = await EngineNS.CEngine.Instance.MaterialManager.NewMaterial(rc, preMatRName, mCurrentResourceInfo.ResourceName);
                // 预览用材质实例
                mPreviewMaterialInstance = EngineNS.CEngine.Instance.MaterialInstanceManager.NewMaterialInstance(rc, mPreviewMaterial);
                for (int i = 0; i < mMeshComponent.MaterialNumber; i++)
                {
                    //await mMeshComponent.SetMaterialInstanceAsync(rc, (uint)i, mPreviewMaterialInstance, null);
                    mMeshComponent.SetMaterialInstance(rc, (uint)i, mPreviewMaterialInstance, null);
                }

                var xndHolder = await EngineNS.IO.XndHolder.LoadXND(mCurrentResourceInfo.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension);
                if (xndHolder != null)
                {
                    await NodesControl.Load(xndHolder.Node);
                    xndHolder.Node?.TryReleaseHolder();
                }

                foreach (var node in NodesControl.OrigionNodeControls)
                {
                    if (node is Controls.MaterialControl)
                    {
                        mSetValueMaterialControl = node as Controls.MaterialControl;
                        if (mCurrentResourceInfo != null)
                            mSetValueMaterialControl.SetBinding(Controls.MaterialControl.TitleStringProperty, new Binding("ResourceName") { Source = mCurrentResourceInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                    }
                }
                mCurrentResourceInfo.IsDirty = false;
            }

            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                IsDirty = false;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }

        EditorCommon.ViewPort.PreviewSceneControl mPreviewSceneControl = null;
        public MaterialEditorControl()
        {
            InitializeComponent();

            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;

            CodeGenerateSystem.Program.RegisterNodeAssembly(Program.MaterialAssemblyKey, this.GetType().Assembly);

            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(global::MaterialEditor.ResourceInfos.MaterialResourceInfo));

            NodesControl.HostControl = this;
            NodesControl.OnDirtyChanged += NodesControl_OnDirtyChanged;
            NodesControl.OnSelectNodeControl += NodesControl_OnSelectNodeControl;
            NodesControl.OnUnSelectNodes += NodesControl_OnUnSelectNodes;
            NodesControl.OnAddedNodeControl += NodesControl_OnAddedNodeControl;
            NodesControl.OnDeletedNodeControl += NodesControl_OnDeletedNodeControl;
            NodesControl.OnInitializeNodeControl += NodesControl_OnInitializeNodeControl;
            NodesControl.OnFilterContextMenu = NodesControl_FilterContextMenu;
            NodesControl.CSType = EngineNS.ECSType.Client;
        }

        public void NodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu nodeList, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            InitializeNodesList(nodeList.GetNodesList(), filterData);
        }
        private void NodesControl_OnInitializeNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            if(node is Controls.BaseNodeControl_ShaderVar)
            {
                var sVarNode = node as Controls.BaseNodeControl_ShaderVar;
                sVarNode.OnShaderVarChanged = _OnShaderVarChanged;
                sVarNode.OnShaderVarRenamed = _OnShaderVarRenamed;
                sVarNode.OnIsGenericChanging = _OnIsGenericChanging;
            }
        }

        private void NodesControl_OnDeletedNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            if(node is Controls.BaseNodeControl_ShaderVar)
            {
                var sVarNode = node as Controls.BaseNodeControl_ShaderVar;
                sVarNode.OnShaderVarChanged = null;
                sVarNode.OnShaderVarRenamed = null;
                sVarNode.OnIsGenericChanging = null;
                var varInfo = sVarNode.GetShaderVarInfo();
                if(varInfo != null)
                {
                    switch(varInfo.VarType)
                    {
                        case EngineNS.EShaderVarType.SVT_Texture:
                        case EngineNS.EShaderVarType.SVT_Sampler:
                            mPreviewMaterial.RemoveSRV(varInfo.VarName);
                            break;
                        default:
                            mPreviewMaterial.RemoveVar(varInfo.VarName);
                            break;
                    }
                    //mNeedRefreshAllMaterialInstance = true;
                }
            }
        }

        private void NodesControl_OnAddedNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            if(node is Controls.BaseNodeControl_ShaderVar)
            {
                var sVarNode = node as Controls.BaseNodeControl_ShaderVar;
                var varInfo = sVarNode.GetShaderVarInfo();
                if (varInfo != null)
                {
                    switch(varInfo.VarType)
                    {
                        case EngineNS.EShaderVarType.SVT_Texture:
                        case EngineNS.EShaderVarType.SVT_Sampler:
                            {
                                var tc = node as Controls.ITextureSampler;
                                var shaderVarParam = mPreviewMaterial.AddSRV(varInfo.VarName, tc.SamplerStateDesc);
                                shaderVarParam.CopyFrom(varInfo);
                            }
                            break;
                        default:
                            {
                                var shaderVarParam = mPreviewMaterial.AddVar(varInfo.VarName, varInfo.VarType, 1);
                                shaderVarParam.CopyFrom(varInfo);
                            }
                            break;
                    }
                    //mNeedRefreshAllMaterialInstance = true;
                }
            }
        }
        void _OnShaderVarChanged(Controls.BaseNodeControl_ShaderVar control, string oldValue, string newValue)
        {
            //var varInfo = control.GetShaderVarInfo();
            //if (varInfo == null)
            //    return;
            //ChangeShaderVar(varInfo);
        }
        bool _OnShaderVarRenamed(Controls.BaseNodeControl_ShaderVar control, string oldName, string newName)
        {
            var varInfo = control.GetShaderVarInfo();
            if (varInfo == null)
                return true;
            var oldParam = mPreviewMaterial.GetParam(oldName);
            if(oldParam != null)
            {
                switch (varInfo.VarType)
                {
                    case EngineNS.EShaderVarType.SVT_Texture:
                    case EngineNS.EShaderVarType.SVT_Sampler:
                        {
                            mPreviewMaterial.RemoveSRV(oldName);
                            var textureNode = control as Controls.ITextureSampler;
                            var param = mPreviewMaterial.AddSRV(newName, textureNode.SamplerStateDesc);
                            param.CopyFrom(varInfo, false);
                            param.OldVarName = oldName;
                        }
                        break;
                    default:
                        {
                            mPreviewMaterial.RemoveVar(oldName);
                            var param = mPreviewMaterial.AddVar(newName, varInfo.VarType, 1);
                            param.CopyFrom(varInfo, false);
                            param.OldVarName = oldName;
                        }
                        break;
                }
            }

            IsDirty = true;
            //mNeedRefreshAllMaterialInstance = true;

            return true;
        }
        void _OnIsGenericChanging(Controls.BaseNodeControl_ShaderVar ctrl, bool oldValue, bool newValue)
        {
            var varInfo = ctrl.GetShaderVarInfo(true);
            if (varInfo == null)
                return;
            if (newValue)
            {
                // 添加
                switch(varInfo.VarType)
                {
                    case EngineNS.EShaderVarType.SVT_Texture:
                    case EngineNS.EShaderVarType.SVT_Sampler:
                        {
                            var textureNode = ctrl as Controls.ITextureSampler;
                            var param = mPreviewMaterial.AddSRV(varInfo.VarName, textureNode.SamplerStateDesc);
                            param.CopyFrom(varInfo);
                        }
                        break;
                    default:
                        {
                            var param = mPreviewMaterial.AddVar(varInfo.VarName, varInfo.VarType, 1);
                            param.CopyFrom(varInfo);
                        }
                        break;
                }
            }
            else
            {
                // 删除
                switch(varInfo.VarType)
                {
                    case EngineNS.EShaderVarType.SVT_Texture:
                    case EngineNS.EShaderVarType.SVT_Sampler:
                        mPreviewMaterial.RemoveSRV(varInfo.VarName);
                        break;
                    default:
                        mPreviewMaterial.RemoveVar(varInfo.VarName);
                        break;
                }
            }

            //mNeedRefreshAllMaterialInstance = true;
        }

        EngineNS.GamePlay.Actor.GActor mMeshActor;

        EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile RP_EditorMobile;

        bool mViewPortInited = false;
        async System.Threading.Tasks.Task InitViewPort(EditorCommon.ViewPort.ViewPortControl vpCtrl)
        {
            if (mViewPortInited)
                return;
            mViewPortInited = true;
            var rc = EngineNS.CEngine.Instance.RenderContext;

            RP_EditorMobile = new EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile();

            var width = (uint)vpCtrl.GetViewPortWidth();
            var height = (uint)vpCtrl.GetViewPortHeight();

            await RP_EditorMobile.Init(rc, width, height, /*1.0f, */vpCtrl.Camera, vpCtrl.DrawHandle);
            vpCtrl.RPolicy = RP_EditorMobile;

            RP_EditorMobile.mHitProxy.mEnabled = false;

            var meshSource = EngineNS.CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, EngineNS.CEngine.Instance.FileManager.GetRName("Meshes/sphere.vms", EngineNS.RName.enRNameType.Editor), true);
            var mesh = EngineNS.CEngine.Instance.MeshManager.CreateMesh(rc, meshSource/*, EngineNS.CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv(EngineNS.RName.GetRName("ShadingEnv/DSBase2RT.senv"))*/);
            mMeshActor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mesh);
            mMeshActor.SpecialName = "MatertialEditActor";

            //
            //mMeshActor = EngineNS.GamePlay.Actor.GActor.NewMeshActor(EngineNS.RName.GetRName("Mesh/test1.gms")/*, EngineNS.RName.GetRName("ShadingEnv/DSBase2RT.senv")*/);
            //mMeshActor = EngineNS.GamePlay.Actor.GActor.NewMeshActor(EngineNS.RName.GetRName("Mesh/c_woman_clothes_001.gms"), EngineNS.RName.GetRName("ShadingEnv/DSBase2RT.senv"));
            mMeshActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);

            vpCtrl.World.AddActor(mMeshActor);
            vpCtrl.World.DefaultScene.AddActor(mMeshActor);

            //var world = new EngineNS.GamePlay.GWorld();
            //world.Init();
            //var sceneName = EngineNS.RName.GetRName("temp");
            //var sg = await EngineNS.GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(world, typeof(EngineNS.GamePlay.SceneGraph.GSceneGraph), null);
            //world.AddScene(sceneName, sg);
            //vpCtrl.World = world;
            //vpCtrl.World.AddActor(mMeshActor);
            //vpCtrl.World.GetScene(sceneName).AddActor(mMeshActor);

            mPreviewSceneControl.IsShowFloor = false;
            vpCtrl.FocusShow(mMeshActor);

            vpCtrl.TickLogicEvent = TickMaterialEdViewport;
        }
        //float mLogicTime = 0;
        public void TickMaterialEdViewport(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            vpc.World.Tick();
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (vpc.Camera != null)
            {
                vpc.World.CheckVisible(rc.ImmCommandList, vpc.Camera);
                vpc.RPolicy.TickLogic(null, rc);
            }
        }

        private void NodesControl_OnUnSelectNodes(List<CodeGenerateSystem.Base.BaseNodeControl> nodes)
        {
            ProGrid.Instance = null;
        }

        private void NodesControl_OnSelectNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            if (node == null)
                ProGrid.Instance = null;
            else
                ProGrid.Instance = node.GetShowPropertyObject();
        }

        protected bool mIsDirty = false;
        public bool IsDirty
        {
            get { return mIsDirty; }
            set
            {
                mIsDirty = value;
                mCurrentResourceInfo.IsDirty = mIsDirty;
                //if (mIsDirty && LivePreview)
                //    mNeedDirtyProcess = true;
            }
        }

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        private void NodesControl_OnDirtyChanged(bool dirty)
        {
            if (mCurrentResourceInfo == null)
                return;
            IsDirty = dirty;

            //When the multi thread shader compiling is done,I will rewrite the logic here;
            //if(IsDirty)
            //    mNeedDirtyProcess = true;
        }

        bool mMenuInitialized = false;
        void InitMenus()
        {
            if (mMenuInitialized)
                return;

            mMenuInitialized = true;
            var menuDatas = new Dictionary<string, EditorCommon.Menu.MenuItemDataBase>();
            // Toolbar
            var toolBarMenuData = new EditorCommon.Menu.MenuItemData_ShowHideControl("Toolbar");
            toolBarMenuData.MenuNames = new string[] { "Window", "Material Editor|Toolbar" };
            toolBarMenuData.Count = 1;
            toolBarMenuData.OperationControlType = typeof(EditorCommon.ViewPort.ViewPortControl);
            toolBarMenuData.Icons = new ImageSource[] { new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_tab_Toolbars_40x.png", UriKind.Relative)) };
            menuDatas[toolBarMenuData.KeyName] = toolBarMenuData;
            // Viewport
            var viewportMenuData = new EditorCommon.Menu.MenuItemData_ShowHideControl("Viewport");
            viewportMenuData.MenuNames = new string[] { "Window", "Material Editor|Viewport" };
            viewportMenuData.Count = 1;
            viewportMenuData.OperationControlType = typeof(EditorCommon.ViewPort.ViewPortControl);
            viewportMenuData.Icons = new ImageSource[] { new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_tab_Viewports_40x.png", UriKind.Relative)) };
            menuDatas[viewportMenuData.KeyName] = viewportMenuData;
            // Graph
            var  graphMenuData = new EditorCommon.Menu.MenuItemData_ShowHideControl("Graph");
            graphMenuData.MenuNames = new string[] { "Window", "Material Editor|Graph" };
            graphMenuData.Count = 1;
            graphMenuData.OperationControlType = typeof(EditorCommon.ViewPort.ViewPortControl);
            graphMenuData.Icons = new ImageSource[] { new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Blueprint_EventGraph_24x.png", UriKind.Relative)) };
            menuDatas[graphMenuData.KeyName] = graphMenuData;

            EditorCommon.Menu.GeneralMenuManager.GenerateMenuItems(Menu_Main, menuDatas);
            //toolBarMenuData.BindOperationControl(0, MainToolbar);
            viewportMenuData.BindOperationControl(0, mPreviewSceneControl.ViewPort);

            EditorCommon.Menu.GeneralMenuManager.Instance.GenerateGeneralMenuItems(Menu_Main);
        }

        #region NodesList
        private void InitializeNodesList(CodeGenerateSystem.Controls.NodeListControl nodesList, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            nodesList.ClearNodes();
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vPosition", "float4,vPosition,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vNormal", "float3,vNormal,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vTangent", "float4,vTangent,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vBinormal", "float3,vBinormal,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vColor", "float4,vColor,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vUV", "float2,vUV,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vLightMap", "float2,vLightMap,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vWorldPos", "float3,vWorldPos,", "");
            
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vF4_1", "float4,vF4_1,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vF4_2", "float4,vF4_2,", "");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Value.PixelMaterialData), "Input/vF4_3", "float4,vF4_3,", "");

            nodesList.AddNodesFromAssembly(filterData, this.GetType().Assembly);

            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/int", "int", "整形数据");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/float1", "float1", "一维浮点数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/float2", "float2", "二维浮点数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/float3", "float3", "三维浮点数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/float4", "float4", "四维浮点数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/uint1", "uint1", "一维无符号整数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/uint2", "uint2", "二维无符号整数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/uint3", "uint3", "三维无符号整数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.CommonValueControl), "Params/uint4", "uint4", "四维无符号整数");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Operation.Arithmetic), "Math/Add", "＋", "加法运算");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Operation.Arithmetic), "Math/Subtract", "－", "减法运算");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Operation.Arithmetic), "Math/Multiply", "×", "乘法运算");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Operation.Arithmetic), "Math/Divide", "÷", "除法运算");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Operation.Arithmetic), "Math/dot", "dot", "向量点乘运算");
            nodesList.AddNodesFromType(filterData, typeof(Controls.Operation.Arithmetic), "Math/cross", "cross", "向量叉乘运算");

            InitializeAllShaderFunctions(nodesList, filterData);
            InitializeAllShaderParams(nodesList, filterData);
        }

        void InitializeAllShaderFunctions(CodeGenerateSystem.Controls.NodeListControl nodesList, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var funcFile = EngineNS.CEngine.Instance.FileManager.EngineContent + "Shaders/Common.function";
            var rd = new System.IO.StreamReader(funcFile, EngineNS.CEngine.Instance.FileManager.GetEncoding(funcFile));
            var strTemp = rd.ReadLine();
            if (strTemp != "/*function")
                return;

            strTemp = "";
            var strFuncData = "";
            while (strTemp != "*/")
            {
                strFuncData += strTemp;
                strTemp = rd.ReadLine();
            }

            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(strFuncData);

            foreach (System.Xml.XmlElement element in xmlDoc.DocumentElement.ChildNodes)
            {
                var funcName = element.GetAttribute("Name");
                var description = element.GetAttribute("Description");
                var path = element.GetAttribute("Path");
                if (string.IsNullOrEmpty(path))
                    path = "函数/" + funcName;

                var tempFile = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(funcFile, EngineNS.CEngine.Instance.FileManager.Bin);
                nodesList.AddNodesFromType(filterData, typeof(Controls.Operation.Function), path, tempFile + "|" + element.OuterXml, description);
            }
        }
        void InitializeAllShaderParams(CodeGenerateSystem.Controls.NodeListControl nodesList, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var dir = EngineNS.CEngine.Instance.FileManager.EngineContent + "Shaders/CBuffer/";
            var startKeyword = "// ShaderParamAnalyse Start";
            var endKeyword = "// ShaderParamAnalyse End";
            foreach (var file in EngineNS.CEngine.Instance.FileManager.GetFiles(dir))
            {
                var fileContent = System.IO.File.ReadAllText(file);
                var analyseStartIdx = fileContent.IndexOf(startKeyword);
                if (analyseStartIdx < 0)
                    continue;
                analyseStartIdx += startKeyword.Length;
                var analyseEndIdx = fileContent.IndexOf(endKeyword);
                var subStr = fileContent.Substring(analyseStartIdx, analyseEndIdx - analyseStartIdx);
                var subStrs = subStr.Split('\n');
                foreach(var str in subStrs)
                {
                    try
                    {
                        var tempStr = str.TrimStart('\r');
                        tempStr = tempStr.TrimEnd('\r');
                        tempStr = tempStr.TrimStart('\t');
                        tempStr = tempStr.TrimEnd('\t');
                        tempStr.TrimStart(' ');
                        if (string.IsNullOrEmpty(tempStr))
                            continue;
                        var idx = tempStr.IndexOf(' ');
                        var typeStr = tempStr.Substring(0, idx);
                        typeStr = typeStr.TrimStart(' ');
                        typeStr = typeStr.TrimEnd(' ');
                        var nameStr = tempStr.Substring(idx + 1);
                        var tempIdx = nameStr.IndexOf(';');
                        var eqIdx = nameStr.IndexOf('=');
                        if (eqIdx >= 0 && eqIdx < tempIdx)
                            tempIdx = eqIdx;
                        nameStr = nameStr.Substring(0, tempIdx);
                        nameStr = nameStr.TrimEnd(' ', ';');
                        var des = "";
                        nodesList.AddNodesFromType(filterData, typeof(Controls.ShaderAutoData), $"Params/{nameStr}", $"{typeStr},{nameStr},{des}", des);
                    }
                    catch(System.Exception e)
                    {
                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "MaterialEditor", $"InitializeAllShaderParams with {str} error, {e.ToString()}");
                    }
                }
            }
        }

        #endregion

        Controls.MaterialControl mSetValueMaterialControl;
        bool mNodesControlInitialized = false;
        void InitializeNodesControl()
        {
            if (mNodesControlInitialized)
                return;
            mNodesControlInitialized = true;

            var nodeType = typeof(Controls.MaterialControl);
            var csParam = CodeGenerateSystem.Base.BaseNodeControl.CreateConstructionParam(nodeType);
            csParam.CSType = EngineNS.ECSType.Client;
            csParam.HostNodesContainer = this.NodesControl;
            csParam.ConstructParam = "";
            csParam.DrawCanvas = this.NodesControl.GetDrawCanvas();
            Controls.MaterialControl.InitNodePinTypes(csParam);
            mSetValueMaterialControl = (Controls.MaterialControl)NodesControl.AddOrigionNode(nodeType, csParam, 800, 0);
            if(mCurrentResourceInfo != null)
                mSetValueMaterialControl.SetBinding(Controls.MaterialControl.TitleStringProperty, new Binding("ResourceName") { Source = mCurrentResourceInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });

            EngineNS.CEngine.Instance.TickManager.AddTickInfo(this);
        }

        bool mNodeListInitialized = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitMenus();
            if (!mNodeListInitialized)
            {
                mNodeListInitialized = true;
                var filterData = new CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData()
                {
                    CSType = EngineNS.ECSType.Client,
                };
                InitializeNodesList(NodesList, filterData);
            }
            InitializeNodesControl();

            EnableTick = true;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            EnableTick = false;
        }


        protected void UpdateMaterialShow(bool force)
        {
            // todo: 先进行错误检查

            System.IO.TextWriter codeFile, varFile;
            CodeGenerator.GenerateCode(NodesControl, mSetValueMaterialControl, out codeFile, out varFile);
            // todo: 版本号更新
            // todo: 存入一个临时位置或者内存中，然后刷新ViewPort窗口
        }

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
            if (mCurrentResourceInfo == null)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
        }
        private void CommandBinding_Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var noUse = Save();
        }
        // 根据材质制作情况考虑要不要刷新材质实例
        //bool mNeedRefreshAllMaterialInstance = false;
        bool mRefreshPreviewMtlInst = false;
        bool IsSaving = false;
        async Task Save()
        {
            mNeedDirtyProcess = true;
            await DirtyProcess();

            // 有错误就不存
            if (NodesControl.CheckError() == false)
                return;
            if (mCurrentResourceInfo == null)
                return;
            if (IsSaving)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "IO", $"System is busy,Material Is Saving, please try later");
                return;
            }
            IsSaving = true;
            System.IO.TextWriter codeFile, varFile;
            CodeGenerator.GenerateCode(NodesControl, mSetValueMaterialControl, out codeFile, out varFile);
            // Var
            System.IO.File.WriteAllText(mCurrentResourceInfo.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, varFile.ToString());
            // Code
            System.IO.File.WriteAllText(mCurrentResourceInfo.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, codeFile.ToString());
            // Link
            var xndHolder = EngineNS.IO.XndHolder.NewXNDHolder();
            NodesControl.Save(xndHolder.Node);
            EngineNS.IO.XndHolder.SaveXND(mCurrentResourceInfo.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension, xndHolder);

            mCurrentResourceInfo.ReferenceRNameList.Clear();
            // Material

            /////////////////////////////////////////////////
            //// 向PreviewMaterial里刷正确的Texture， 解决Texture丢失的问题
            //int textureCount = 0;
            //foreach (var node in NodesControl.CtrlNodeList)
            //{
            //    if (node is Controls.TextureControl)
            //    {
            //        textureCount++;
            //    }
            //}
            //if (mPreviewMaterial.GetSRVCount() != textureCount)
            //{
            //    foreach (var node in NodesControl.CtrlNodeList)
            //    {
            //        if (node is Controls.TextureControl)
            //        {
            //            var textureNode = node as Controls.TextureControl;
            //            var varInfo = textureNode.GetShaderVarInfo();
            //            mPreviewMaterial.AddSRV(varInfo.VarName, textureNode.SamplerStateDesc);
            //        }
            //    }
            //}
            /////////////////////////////////////////////////

            // 添加新的材质参数
            var dic = new Dictionary<string, Controls.BaseNodeControl_ShaderVar>();
            var addList = new List<EngineNS.Graphics.CGfxMaterialParam>();
            var removeList = new List<EngineNS.Graphics.CGfxMaterialParam>();
            foreach(var node in NodesControl.CtrlNodeList)
            {
                if(node is Controls.BaseNodeControl_ShaderVar)
                {
                    var svNode = node as Controls.BaseNodeControl_ShaderVar;
                    var param = svNode.GetShaderVarInfo();
                    if (param == null)
                        continue;

                    dic[param.VarName] = svNode;
                }
            }

            //foreach(var param in mPreviewMaterial.ParamList)
            foreach(var nodeData in dic)
            {
                EngineNS.Graphics.CGfxMaterialParam matParam;
                if (!mCurrentMaterialParamsDic.TryGetValue(nodeData.Key, out matParam))
                {
                    var param = nodeData.Value.GetShaderVarInfo();
                    switch(param.VarType)
                    {
                        case EngineNS.EShaderVarType.SVT_Texture:
                        case EngineNS.EShaderVarType.SVT_Sampler:
                            {
                                var tc = nodeData.Value as Controls.ITextureSampler;
                                matParam = mCurrentMaterial.AddSRV(param.VarName, tc.SamplerStateDesc);
                                matParam.CopyFrom(param);
                            }
                            break;
                        default:
                            {
                                matParam = mCurrentMaterial.AddVar(param.VarName, param.VarType, 1);
                                matParam.CopyFrom(param);
                            }
                            break;
                    }
                    addList.Add(param);
                }
            }
            // 删除旧的材质参数
            foreach(var param in mCurrentMaterialParamsDic.Values)
            {
                if(!dic.ContainsKey(param.VarName))
                {
                    switch(param.VarType)
                    {
                        case EngineNS.EShaderVarType.SVT_Texture:
                        case EngineNS.EShaderVarType.SVT_Sampler:
                            mCurrentMaterial.RemoveSRV(param.VarName);
                            break;
                        default:
                            mCurrentMaterial.RemoveVar(param.VarName);
                            break;
                    }
                    removeList.Add(param);
                }
            }
            // 重置材质参数字典
            mCurrentMaterialParamsDic.Clear();
            foreach (var param in mCurrentMaterial.ParamList)
            {
                var tempParam = dic[param.VarName].GetShaderVarInfo();
                param.CopyFrom(tempParam);
                mCurrentMaterialParamsDic[param.VarName] = param;
            }

            // 刷新SamplerStateDesc
            foreach (var param in mCurrentMaterial.ParamList)
            {
                switch (param.VarType)
                {
                    case EngineNS.EShaderVarType.SVT_Texture:
                    case EngineNS.EShaderVarType.SVT_Sampler:
                        {
                            var tc = dic[param.VarName] as Controls.ITextureSampler;
                            mCurrentMaterial.SetSamplerStateDesc(param.VarName, tc.SamplerStateDesc);
                            mCurrentResourceInfo.ReferenceRNameList.Add(param.TextureRName);
                        }
                        break;
                }
            }


            UpdateMtlMacros(mCurrentMaterial);
            // 向C++里传名字
            mCurrentMaterial.Name = mCurrentMaterial.Name;
            mCurrentMaterial.SaveMaterial();

            // MaterialInstance
            //if (mNeedRefreshAllMaterialInstance)
            {
                // 刷新所有使用该材质的材质实例
                var noUse = RefreshMaterialInstances(++mCurrentSerialId);
            }

            await mCurrentResourceInfo.Save(true);

            IsSaving = false;

        }

        UInt64 mCurrentSerialId = 0;
        async System.Threading.Tasks.Task RefreshMaterialInstances(UInt64 serialId)
        {
            // 刷新现有Effect
            //EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //{
            //    lock (EngineNS.CEngine.Instance.EffectManager.Effects)
            //    {
            //        foreach (var eft in EngineNS.CEngine.Instance.EffectManager.Effects)
            //        {
            //            if (mCurrentSerialId != serialId)
            //                return false;
            //            //if (eft.Value.Desc == null)
            //            //    continue;
            //            if (eft.Value.Desc.MtlShaderPatch.Name == mCurrentMaterial.Name)
            //            {
            //                eft.Value.BuildTechnique(EngineNS.CEngine.Instance.RenderContext);
            //                eft.Value.Save2Xnd();
            //            }
            //        }
            //    }
            //    return true;
            //}, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                EngineNS.CEngine.Instance.EffectManager.RefreshEffects(
                    EngineNS.CEngine.Instance.RenderContext,
                    mCurrentMaterial.Name);
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);

            var rc = EngineNS.CEngine.Instance.RenderContext;
            foreach(var content in EngineNS.CEngine.Instance.FileManager.AllContents)
            {
                var matInsFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(content, "*" + EngineNS.CEngineDesc.MaterialInstanceExtension, System.IO.SearchOption.AllDirectories);

                // 刷新所有打开的材质实例编辑器
                List<string> refreshedMatIns = new List<string>();
                foreach (var ctrlData in EditorCommon.PluginAssist.Process.ControlsDic)
                {
                    if (mCurrentSerialId != serialId)
                        return;

                    if (ctrlData.Key.GetExtension() == EngineNS.CEngineDesc.MaterialInstanceExtension.TrimStart('.'))
                    {
                        var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, ctrlData.Key);
                        if (matIns == null)
                            continue;
                        if (matIns.Material.Name == mCurrentMaterial.Name)
                        {
                            // 刷新材质实例
                            matIns.RefreshFromMaterial(mCurrentMaterial);
                            matIns.SaveMaterialInstance();
                            var ctrl = ctrlData.Value.Content as MaterialInstanceEditorControl;
                            if (ctrl != null)
                            {
                                await ctrl.SetObjectToEdit(ctrl.CurrentContext);
                            }
                            refreshedMatIns.Add(ctrlData.Key.Address);

                            // 刷新RInfo
                            var rInfo = ctrlData.Value.Context.ResInfo;
                            rInfo.ReferenceRNameList.Clear();
                            rInfo.ReferenceRNameList.Add(matIns.MaterialName);
                            for (UInt32 i = 0; i < matIns.SRVNumber; i++)
                            {
                                var rName = matIns.GetSRVName(i);
                                rInfo.ReferenceRNameList.Add(rName);
                            }
                            var dirty = rInfo.IsDirty;
                            await rInfo.Save();
                            rInfo.IsDirty = dirty;
                        }
                    }
                }
                var totalCount = matInsFiles.Count;
                TextBlock_RefreshMatIns.Visibility = Visibility.Visible;
                TextBlock_RefreshMatIns.Text = $"正在刷新材质实例:剩余{totalCount}";
                foreach (var file in matInsFiles)
                {
                    if (mCurrentSerialId != serialId)
                    {
                        totalCount--;
                        TextBlock_RefreshMatIns.Text = $"正在刷新材质实例:剩余{totalCount}";
                        return;
                    }

                    var rName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(file);
                    if (refreshedMatIns.Contains(rName.Address))
                    {
                        totalCount--;
                        TextBlock_RefreshMatIns.Text = $"正在刷新材质实例:剩余{totalCount}";
                        continue;
                    }

                    var rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(rName.Address) as ResourceInfos.MaterialInstanceResourceInfo;
                    await rInfo.AsyncLoad(rName.Address + EditorCommon.Program.ResourceInfoExt);
                    if (rInfo.ParentMaterialRName == mCurrentMaterial.Name)
                    {
                        var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, rName);
                        // 刷新材质实例
                        matIns.RefreshFromMaterial(mCurrentMaterial);
                        matIns.SaveMaterialInstance();

                        // 刷新RInfo
                        rInfo.ReferenceRNameList.Clear();
                        rInfo.ReferenceRNameList.Add(matIns.MaterialName);
                        for (UInt32 i = 0; i < matIns.SRVNumber; i++)
                        {
                            rInfo.ReferenceRNameList.Add(matIns.GetSRVName(i));
                        }
                        var dirty = rInfo.IsDirty;
                        await rInfo.Save();
                        rInfo.IsDirty = dirty;
                    }
                    totalCount--;
                    TextBlock_RefreshMatIns.Text = $"正在刷新材质实例:剩余{totalCount}";
                }
            }
            TextBlock_RefreshMatIns.Visibility = Visibility.Collapsed;

            //mNeedRefreshAllMaterialInstance = false;
            mRefreshPreviewMtlInst = true;
            mNeedDirtyProcess = true;
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

        public void Closed()
        {
            EditorCommon.UndoRedo.UndoRedoManager.Instance.ClearCommands(UndoRedoKey);
        }

        public bool? CanClose()
        {
            if (IsDirty)
            {
                var result = EditorCommon.MessageBox.Show("该材质模板还未保存，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                        var noUse = Save();
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.No:
                        IsDirty = false;
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        #endregion

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            var noUse = Save();
        }
        
        void UpdateMtlMacros(EngineNS.Graphics.CGfxMaterial Mtl)
        {
            Mtl.GetMtlMacroArray().Clear();

            EngineNS.Graphics.CGfxMaterial.MtlMacro MtlMacro = new EngineNS.Graphics.CGfxMaterial.MtlMacro();

            switch (mSetValueMaterialControl.mMtlMacros.MaterialID)
            {
                case MaterialEditor.Controls.MaterialControl.MtlMacros.eMtlID.Unlit:
                    MtlMacro.mMacroName = "MTL_ID_UNLIT";
                    MtlMacro.mMacroValue = "";
                    break;
                case MaterialEditor.Controls.MaterialControl.MtlMacros.eMtlID.Common:
                    MtlMacro.mMacroName = "MTL_ID_COMMON";
                    MtlMacro.mMacroValue = "";
                    break;
                case MaterialEditor.Controls.MaterialControl.MtlMacros.eMtlID.Transmit:
                    MtlMacro.mMacroName = "MTL_ID_TRANSMIT";
                    MtlMacro.mMacroValue = "";
                    break;
                case MaterialEditor.Controls.MaterialControl.MtlMacros.eMtlID.Hair:
                    MtlMacro.mMacroName = "MTL_ID_HAIR";
                    MtlMacro.mMacroValue = "";
                    break;
                case MaterialEditor.Controls.MaterialControl.MtlMacros.eMtlID.Skin:
                    MtlMacro.mMacroName = "MTL_ID_SKIN";
                    MtlMacro.mMacroValue = "";
                    break;
                case MaterialEditor.Controls.MaterialControl.MtlMacros.eMtlID.Eye:
                    MtlMacro.mMacroName = "MTL_ID_EYE";
                    MtlMacro.mMacroValue = "";
                    break;
                case MaterialEditor.Controls.MaterialControl.MtlMacros.eMtlID.NprScene:
                    MtlMacro.mMacroName = "MTL_ID_NPR_SCENE";
                    MtlMacro.mMacroValue = "";
                    break;


                default:
                    MtlMacro.mMacroName = "MTL_ID_COMMON";
                    MtlMacro.mMacroValue = "";
                    break;
            }
            Mtl.GetMtlMacroArray().Add(MtlMacro);

            if (mSetValueMaterialControl.mMtlMacros.UseAlphaTest == true)
            {
                MtlMacro.mMacroName = "ALPHA_TEST";
                MtlMacro.mMacroValue = "";
                Mtl.GetMtlMacroArray().Add(MtlMacro);
            }
        }

        public bool LivePreview
        {
            get { return (bool)GetValue(LivePreviewProperty); }
            set { SetValue(LivePreviewProperty, value); }
        }
        public static readonly DependencyProperty LivePreviewProperty = DependencyProperty.Register("LivePreview", typeof(bool), typeof(MaterialEditorControl), new FrameworkPropertyMetadata(true));
        private void IconTextBtn_Apply_Click(object sender, RoutedEventArgs e)
        {
            mNeedDirtyProcess = true;
            if (mDirtyProcessTask == null || mDirtyProcessTask.IsCompleted)
                mDirtyProcessTask = DirtyProcess(true);
        }

        private void Button_FR_Click(object sender, RoutedEventArgs e)
        {
            var noUse = FR_Process();
        }
        public async Task FR_Process()
        {
            foreach(var content in EngineNS.CEngine.Instance.FileManager.AllContents)
            {
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(content, "*.material", System.IO.SearchOption.AllDirectories);
                int i = 0;
                foreach (var file in files)
                {
                    //var file = @"E:\work\TitanEngine\Content\Map\Batmen\Material\bulb.material";
                    //var file = @"E:\work\TitanEngine\Content\Map\Batmen\Material\base_pbr_bnm.material";

                    System.Diagnostics.Debug.WriteLine($"========================{i}/{files.Count} {file}");

                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file + EditorCommon.Program.ResourceInfoExt, null) as ResourceInfos.MaterialResourceInfo;
                    var context = new EditorCommon.Resources.ResourceEditorContext("MaterialEditor", resInfo);
                    await SetObjectToEdit(context);

                    ////////var nodesControl = new CodeGenerateSystem.Controls.NodesContainerControl();
                    //////NodesControl.CSType = EngineNS.ECSType.Client;
                    //////mCurrentResourceInfo = 
                    //////var xndHolder = await EngineNS.IO.XndHolder.LoadXND(file + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension);
                    //////if (xndHolder != null)
                    //////{
                    //////    NodesControl.Load(xndHolder.Node);
                    //////    xndHolder.Node?.TryReleaseHolder();
                    //////}
                    //////foreach (var node in NodesControl.OrigionNodeControls)
                    //////{
                    //////    if (node is Controls.MaterialControl)
                    //////    {
                    //////        mSetValueMaterialControl = node as Controls.MaterialControl;
                    //////    }
                    //////}

                    bool needChange = false;
                    foreach (var node in NodesControl.CtrlNodeList)
                    {
                        if (node is Controls.Operation.Function)
                        {
                            // 刷新Function内容
                            var ctrl = node as Controls.Operation.Function;
                            var splits = ctrl.CSParam.ConstructParam.Split('|');

                            var contentStr = splits[1];
                            var firstLine = contentStr.Substring(0, contentStr.IndexOf('>'));
                            var idStr = firstLine.Substring(1, firstLine.IndexOf(' ')).TrimEnd(' ');
                            var idx1 = firstLine.IndexOf('"');
                            var idx2 = firstLine.IndexOf('"', idx1 + 1);
                            firstLine = firstLine.Substring(0, idx2);

                            var absFuncFile = content + splits[0];
                            var fileContent = System.IO.File.ReadAllText(absFuncFile, Encoding.GetEncoding("GB2312"));
                            var startIdx = fileContent.IndexOf(firstLine);
                            var endIdStr = $"</{idStr}>";
                            var endIdx = fileContent.IndexOf(endIdStr, startIdx) + endIdStr.Length;

                            var xmlDocNew = new System.Xml.XmlDocument();
                            var newContent = fileContent.Substring(startIdx, endIdx - startIdx);

                            xmlDocNew.LoadXml(newContent);
                            ctrl.UpdateParam(xmlDocNew, splits[0]);

                            needChange = true;
                        }
                    }

                    if (needChange)
                    {
                        var xndHolder = EngineNS.IO.XndHolder.NewXNDHolder();
                        NodesControl.Save(xndHolder.Node);
                        EngineNS.IO.XndHolder.SaveXND(mCurrentResourceInfo.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension, xndHolder);

                        System.IO.TextWriter codeFile, varFile;
                        CodeGenerator.GenerateCode(NodesControl, mSetValueMaterialControl, out codeFile, out varFile);
                        // Var
                        System.IO.File.WriteAllText(file + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, varFile.ToString());
                        // Code
                        System.IO.File.WriteAllText(file + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, codeFile.ToString());

                        //await mCurrentResourceInfo.Save();
                    }

                    i++;
                }
            }
        }
    }
}

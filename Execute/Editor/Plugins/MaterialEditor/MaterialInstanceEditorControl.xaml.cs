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
using EditorCommon.Resources;
using EngineNS.IO;

namespace MaterialEditor
{
    /// <summary>
    /// Interaction logic for MaterialInstanceEditorControl.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "MaterialInstanceEditor")]
    [Guid("290ECBA7-00FB-4C5E-BEA6-B200FB10F594")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MaterialInstanceEditorControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region IEditorPlugin
        public string PluginName => "MaterialInstanceEditor";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MaterialInstanceEditorControl), new FrameworkPropertyMetadata(null));

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/MaterialInstanceConstant_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MaterialInstanceEditorControl), new FrameworkPropertyMetadata(null));

        public UIElement InstructionControl => null;

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public bool? CanClose()
        {
            if (mCurrentContext == null)
                return true;
            if(mCurrentContext.ResInfo != null)
            {
                if(mCurrentContext.ResInfo.IsDirty)
                {
                    var result = EditorCommon.MessageBox.Show("该材质实例还未保存，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                    switch (result)
                    {
                        case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                            var noUse = Save();
                            return true;
                        case EditorCommon.MessageBox.enMessageBoxResult.No:
                            CurrentContext.ResInfo.IsDirty = false;
                            return true;
                        case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                            return false;
                    }
                }
            }
            return true;
        }

        public void Closed()
        {
            EditorCommon.UndoRedo.UndoRedoManager.Instance.ClearCommands(UndoRedoKey);
        }

        public void StartDrag() { }
        public void EndDrag() { }

        public IDockAbleControl LoadElement(XmlNode node)
        {
            return null;
        }

        public bool OnActive()
        {
            return true;
        }

        public bool OnDeactive()
        {
            return true;
        }

        public void SaveElement(XmlNode node, XmlHolder holder) { }

        public async System.Threading.Tasks.Task SetObjectToEdit(ResourceEditorContext context)
        {
            await mPreviewSceneControl.Initialize(EngineNS.RName.GetRName("MaterialInstanceEditorViewport"));
            mViewPortInited = false;
            ProGrid_PreviewScene.Instance = mPreviewSceneControl;
            await InitViewPort(mPreviewSceneControl.ViewPort);

            mCurrentContext = context;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            mCurrentMaterialInstance = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, mCurrentContext.ResInfo.ResourceName);

            mPreviewMaterialInstance = EngineNS.CEngine.Instance.MaterialInstanceManager.NewMaterialInstance(rc, mCurrentMaterialInstance.Material);
            mCurrentMaterialInstance.SetDataToMaterialInstance(mPreviewMaterialInstance);
            if(mMeshComponent != null)
            {
                for (int i = 0; i < mMeshComponent.MaterialNumber; i++)
                {
                    await mMeshComponent.SetMaterialInstanceAsync(rc, (uint)i, mPreviewMaterialInstance, null);
                }
            }

            var showValue = new ResourceInfos.MaterialInstanceResourceInfo.MaterialInstanceEditProperty(this);
            showValue.SetMaterialInstance(mPreviewMaterialInstance);
            var matInsResInfo = mCurrentContext.ResInfo as ResourceInfos.MaterialInstanceResourceInfo;
            if(matInsResInfo != null)
            {
                matInsResInfo.ReferenceRNameList.Remove(matInsResInfo.ParentMaterialRName);
                matInsResInfo.ParentMaterialRName = showValue.MaterialSourceName;
                matInsResInfo.ReferenceRNameList.Add(matInsResInfo.ParentMaterialRName);
            }

            SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
            IconBrush = context.ResInfo.ResourceTypeBrush;
            ProGrid.Instance = showValue;
            UpdateUndoRedoKey();

            InitBlendStateDesc(true);

            //await EngineNS.CEngine.Instance.EventPoster.YieldTo(EngineNS.Thread.EAsyncContinueType.YieldSync);
            mCurrentContext.ResInfo.IsDirty = false;
        }

        public async System.Threading.Tasks.Task OnResetPreviewMaterialParentMaterial(ResourceInfos.MaterialInstanceResourceInfo.MaterialInstanceEditProperty miep, EngineNS.RName newValue)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var mat = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, newValue);
            mPreviewMaterialInstance.NewMaterialInstance(rc, mat, mPreviewMaterialInstance.Name);
            mPreviewMaterialInstance.ResetValuesFromMaterial(mat);
            miep.SetMaterialInstance(mPreviewMaterialInstance);

            ProGrid.Instance = null;
            ProGrid.Instance = miep;
            if (mMeshComponent != null)
            {
                for (int i = 0; i < mMeshComponent.MaterialNumber; i++)
                {
                    await mMeshComponent.SetMaterialInstanceAsync(rc, (uint)i, mPreviewMaterialInstance, null);
                }
            }
        }

        #endregion

        public string UndoRedoKey
        {
            get
            {
                if (mCurrentContext != null)
                    return mCurrentContext.ResInfo.ResourceName.Name;
                return "";
            }
        }
        void UpdateUndoRedoKey()
        {
            OnPropertyChanged("UndoRedoKey");
        }

        ResourceEditorContext mCurrentContext;
        public ResourceEditorContext CurrentContext
        {
            get => mCurrentContext;
        }
        EngineNS.Graphics.CGfxMaterialInstance mPreviewMaterialInstance;
        EngineNS.Graphics.CGfxMaterialInstance mCurrentMaterialInstance;
        EngineNS.GamePlay.Component.GMeshComponent mMeshComponent;

        EditorCommon.ViewPort.PreviewSceneControl mPreviewSceneControl = null;
        public MaterialInstanceEditorControl()
        {
            InitializeComponent();

            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;

            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(global::MaterialEditor.ResourceInfos.MaterialInstanceResourceInfo));
            ProGrid.OnNotifyPropertyChanged -= ProGrid_OnNotifyPropertyChanged;
            ProGrid.OnNotifyPropertyChanged += ProGrid_OnNotifyPropertyChanged;
        }

        private void ProGrid_OnNotifyPropertyChanged(WPG.Data.Property property, object oldValue, object newValue)
        {
            mPreviewMaterialInstance?.ForceUpdateVersion();
            if(mCurrentContext != null)
                mCurrentContext.ResInfo.IsDirty = true;
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            var noUse = Save();
        }
        async Task Save()
        {
            mPreviewMaterialInstance.SetDataToMaterialInstance(mCurrentMaterialInstance);
            mCurrentMaterialInstance?.SaveMaterialInstance();
            if (mCurrentContext != null)
            {
                mCurrentContext.ResInfo.ReferenceRNameList.Clear();
                mCurrentContext.ResInfo.ReferenceRNameList.Add(mCurrentMaterialInstance.MaterialName);
                for(UInt32 i=0; i<mCurrentMaterialInstance.SRVNumber; i++)
                {
                    var rName = mCurrentMaterialInstance.GetSRVName(i);
                    mCurrentContext.ResInfo.ReferenceRNameList.Add(rName);
                }

                await mCurrentContext.ResInfo.Save(true);
                mCurrentContext.ResInfo.IsDirty = false;
            }
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
            
            await RP_EditorMobile.Init(rc, width, height, vpCtrl.Camera, vpCtrl.DrawHandle);
            vpCtrl.RPolicy = RP_EditorMobile;

            RP_EditorMobile.mHitProxy.mEnabled = false;

            var meshSource = EngineNS.CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, EngineNS.CEngine.Instance.FileManager.GetRName("Meshes/sphere.vms", EngineNS.RName.enRNameType.Editor), true);
            var mesh = EngineNS.CEngine.Instance.MeshManager.CreateMesh(rc, meshSource/*, EngineNS.CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv(EngineNS.RName.GetRName("ShadingEnv/DSBase2RT.senv"))*/);
            mMeshActor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mesh);
            mMeshActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
            mMeshComponent = mMeshActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            if(mPreviewMaterialInstance != null)
            {
                for (int i = 0; i < mMeshComponent.MaterialNumber; i++)
                {
                    await mMeshComponent.SetMaterialInstanceAsync(rc, (uint)i, mPreviewMaterialInstance, null);
                }
            }

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

            ProGrid_PreviewScene.Instance = vpCtrl.EditorViewPort;
        }
        public void TickMaterialEdViewport(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            vpc.World.Tick();
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            if (vpc.Camera != null)
            {
                ////2RTs DefferdShading 
                //EngineNS.CMRTClearColor[] clrColors = new EngineNS.CMRTClearColor[]
                //{
                //    new EngineNS.CMRTClearColor(0, 0x00FF8080),
                //    new EngineNS.CMRTClearColor(1, 0x00808080)
                //};
                ////Viewport.Camera.DisplayView.ClearMRT(clrColors, true, 1.0F, true, 0);
                //Viewport.Camera.mSceneView.ClearPasses();
                vpc.World.CheckVisible(RHICtx.ImmCommandList, vpc.Camera);
                vpc.RPolicy.TickLogic(null, RHICtx);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitMenus();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

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

            EditorCommon.Menu.GeneralMenuManager.GenerateMenuItems(Menu_Main, menuDatas);
            //toolBarMenuData.BindOperationControl(0, MainToolbar);
            viewportMenuData.BindOperationControl(0, mPreviewSceneControl.ViewPort);

            EditorCommon.Menu.GeneralMenuManager.Instance.GenerateGeneralMenuItems(Menu_Main);
        }

        class MatInsStates : INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            public MaterialInstanceEditorControl HostControl;

            [Category("简单设置")]
            EngineNS.RenderTargetBlendDesc.EBlendType mBlendType0;
            public EngineNS.RenderTargetBlendDesc.EBlendType BlendType0
            {
                get { return mBlendType0; }
                set
                {
                    mBlendType0 = value;
                    var save = BlendStateDesc;
                    save.RenderTarget0.SetBlendType(value);
                    BlendStateDesc = save;

                    HostControl.ProGrid_Desc.Instance = null;
                    HostControl.ProGrid_Desc.Instance = this;
                }
            }
            [Category("简单设置")]
            EngineNS.RenderTargetBlendDesc.EBlendType mBlendType1;
            public EngineNS.RenderTargetBlendDesc.EBlendType BlendType1
            {
                get { return mBlendType1; }
                set
                {
                    mBlendType1 = value;
                    var save = BlendStateDesc;
                    save.RenderTarget1.SetBlendType(value);
                    BlendStateDesc = save;

                    HostControl.ProGrid_Desc.Instance = null;
                    HostControl.ProGrid_Desc.Instance = this;
                }
            }
            [Category("简单设置")]
            EngineNS.RenderTargetBlendDesc.EBlendType mBlendType2;
            public EngineNS.RenderTargetBlendDesc.EBlendType BlendType2
            {
                get { return mBlendType2; }
                set
                {
                    mBlendType2 = value;
                    var save = BlendStateDesc;
                    save.RenderTarget2.SetBlendType(value);
                    BlendStateDesc = save;

                    HostControl.ProGrid_Desc.Instance = null;
                    HostControl.ProGrid_Desc.Instance = this;
                }
            }
            [Category("简单设置")]
            EngineNS.RenderTargetBlendDesc.EBlendType mBlendType3;
            public EngineNS.RenderTargetBlendDesc.EBlendType BlendType3
            {
                get { return mBlendType3; }
                set
                {
                    mBlendType3 = value;
                    var save = BlendStateDesc;
                    save.RenderTarget3.SetBlendType(value);
                    BlendStateDesc = save;

                    HostControl.ProGrid_Desc.Instance = null;
                    HostControl.ProGrid_Desc.Instance = this;
                }
            }
            [Category("RHIState")]
            public EngineNS.CBlendStateDesc BlendStateDesc
            {
                get;
                set;
            }
            [Category("RHIState")]
            public EngineNS.CDepthStencilStateDesc DepthSencilState
            {
                get;
                set;
            }
            [Category("RHIState")]
            public EngineNS.CRasterizerStateDesc RasterizerState
            {
                get;
                set;
            }
        }

        bool mBlendStateDescInitialized = false;
        MatInsStates mRHIStateDesc;
        void InitBlendStateDesc(bool force)
        {
            if (mBlendStateDescInitialized && !force)
                return;
            mBlendStateDescInitialized = true;

            mRHIStateDesc = new MatInsStates();
            mRHIStateDesc.HostControl = this;
            mRHIStateDesc.BlendStateDesc = mPreviewMaterialInstance.CustomBlendState.Desc;
            mRHIStateDesc.DepthSencilState = mPreviewMaterialInstance.CustomDepthStencilState.Desc;
            mRHIStateDesc.RasterizerState = mPreviewMaterialInstance.CustomRasterizerState.Desc;
            ProGrid_Desc.Instance = mRHIStateDesc;
            ProGrid_Desc.OnNotifyPropertyChanged -= ProGrid_Desc_OnNotifyPropertyChanged;
            ProGrid_Desc.OnNotifyPropertyChanged += ProGrid_Desc_OnNotifyPropertyChanged;
        }

        private void ProGrid_Desc_OnNotifyPropertyChanged(WPG.Data.Property property, object oldValue, object newValue)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            mPreviewMaterialInstance.CustomBlendState = EngineNS.CEngine.Instance.BlendStateManager.GetBlendState(rc, mRHIStateDesc.BlendStateDesc);
            mPreviewMaterialInstance.CustomDepthStencilState = EngineNS.CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(rc, mRHIStateDesc.DepthSencilState);
            mPreviewMaterialInstance.CustomRasterizerState = EngineNS.CEngine.Instance.RasterizerStateManager.GetRasterizerState(rc, mRHIStateDesc.RasterizerState);
            mPreviewMaterialInstance.ForceUpdateVersion();
            if (mCurrentContext != null)
                mCurrentContext.ResInfo.IsDirty = true;
            mCurrentMaterialInstance.ForceUpdateVersion();
        }
    }
}

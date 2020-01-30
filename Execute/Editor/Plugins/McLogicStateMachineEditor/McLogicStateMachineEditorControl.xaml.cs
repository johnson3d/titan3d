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

namespace McLogicStateMachineEditor
{
    /// <summary>
    /// Interaction logic for McLogicAnimationEditor.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "McLogicStateMachineEditor")]
    [Guid("8348CED6-12C4-42AA-B487-DF1DD19D0130")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class McLogicStateMachineEditorControl : Macross.MacrossControlBase, EditorCommon.PluginAssist.IEditorPlugin
    {
        void UpdateUndoRedoKey()
        {
            OnPropertyChanged("UndoRedoKey");
            Macross_Client.UpdateUndoRedoKey();
            Macross_Server.UpdateUndoRedoKey();
        }

        public string PluginName => "McLogicStateMachineEditor";

        public string Version => "1.0.0";

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/AnimBlueprint_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(McLogicStateMachineEditorControl), new FrameworkPropertyMetadata(null));

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

        public Visibility ClientVisible
        {
            get { return (Visibility)GetValue(ClientVisibleProperty); }
            set { SetValue(ClientVisibleProperty, value); }
        }
        public static readonly DependencyProperty ClientVisibleProperty = DependencyProperty.Register("ClientVisible", typeof(Visibility), typeof(McLogicStateMachineEditorControl), new UIPropertyMetadata(Visibility.Visible));
        public Visibility ServerVisible
        {
            get { return (Visibility)GetValue(ServerVisibleProperty); }
            set { SetValue(ServerVisibleProperty, value); }
        }
        public static readonly DependencyProperty ServerVisibleProperty = DependencyProperty.Register("ServerVisible", typeof(Visibility), typeof(McLogicStateMachineEditorControl), new UIPropertyMetadata(Visibility.Visible));
        public int CCSShowIdx
        {
            get { return (int)GetValue(CCSShowIdxProperty); }
            set { SetValue(CCSShowIdxProperty, value); }
        }
        public static readonly DependencyProperty CCSShowIdxProperty = DependencyProperty.Register("CCSShowIdx", typeof(int), typeof(McLogicStateMachineEditorControl), new UIPropertyMetadata(0));

        public async System.Threading.Tasks.Task SetObjectToEdit(EditorCommon.Resources.ResourceEditorContext context)
        {
            SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });

            // ResInfo 可能在不同的线程生成的，所以这里强制读取一下文件
            CurrentResourceInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(context.ResInfo.AbsInfoFileName, context.ResInfo.ParentBrowser) as Macross.ResourceInfos.MacrossResourceInfo;
            var mclaInfo = CurrentResourceInfo as ResourceInfos.McLogicStateMachineResourceInfo;
            UpdateUndoRedoKey();
            var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(CurrentResourceInfo.BaseType, typeof(EngineNS.Editor.Editor_MacrossClassAttribute).FullName, false);
            if (att != null)
            {
                var csType = (EngineNS.ECSType)EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "CSType");
                switch (csType)
                {
                    case EngineNS.ECSType.Common:
                        {
                            ClientVisible = Visibility.Visible;
                            ServerVisible = Visibility.Visible;
                            CCSShowIdx = 0;
                            Macross_Client.CurrentResourceInfo = mclaInfo;
                            Macross_Server.CurrentResourceInfo = mclaInfo;
                        }
                        break;
                    case EngineNS.ECSType.Client:
                        {
                            ClientVisible = Visibility.Visible;
                            ServerVisible = Visibility.Collapsed;
                            CCSShowIdx = 0;
                            Macross_Client.CurrentResourceInfo = mclaInfo;
                        }
                        break;
                    case EngineNS.ECSType.Server:
                        {
                            ClientVisible = Visibility.Collapsed;
                            ServerVisible = Visibility.Visible;
                            CCSShowIdx = 1;
                            Macross_Server.CurrentResourceInfo = mclaInfo;
                        }
                        break;
                }
            }

            await Load();
        }

        public void SaveElement(EngineNS.IO.XmlNode node, EngineNS.IO.XmlHolder holder) { }

        public DockControl.IDockAbleControl LoadElement(EngineNS.IO.XmlNode node)
        {
            return null;
        }

        public void StartDrag()
        {
            throw new NotImplementedException();
        }

        public void EndDrag()
        {
            throw new NotImplementedException();
        }

        public bool? CanClose()
        {
            if (IsDirty)
            {
                var result = EditorCommon.MessageBox.Show("该Macross还未保存，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                        var noUse = Save();
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.No:
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        public void Closed()
        {
            EditorCommon.UndoRedo.UndoRedoManager.Instance.ClearCommands(Macross_Client.NodesCtrlAssist.UndoRedoKey);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.ClearCommands(Macross_Server.NodesCtrlAssist.UndoRedoKey);
        }

        public McLogicStateMachineEditorControl()
        {
            CodeGenerateSystem.Program.RegisterNodeAssembly();
            CodeGenerateSystem.Program.RegisterNodeAssembly("McLogicStateMachineEditor.dll", this.GetType().Assembly);
            InitializeComponent();
            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(ResourceInfos.McLogicStateMachineResourceInfo));

            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(LFSMGraphCategoryItemInitData));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LFSMFinalTransitionResultConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LFSMTransitionNodeControlConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LogicFSMGraphNodeControlConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LogicFSMNodeControlConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LFSMTransition));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LFSMTransitionTrack));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LFSMTransitionArea));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LogicFSMNodeControlConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LogicFSMNodeControlConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LogicFSMNodeControlConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LogicFSMNodeControlConstructionParams));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(Controls.LogicFSMNodeControlConstructionParams));

            mCodeGenerator = new McLogicStateMachineEditor.CodeGenerator();


            Macross_Client.HostControl = this;
            Macross_Server.HostControl = this;

            var template = TryFindResource("TypeSetterTemplate") as DataTemplate;
            WPG.Program.RegisterDataTemplate("TypeSetterTemplate", template);
            template = TryFindResource("MethodParamSetterTemplate") as DataTemplate;
            WPG.Program.RegisterDataTemplate("MethodParamSetterTemplate", template);

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
            // Graph
            var graphMenuData = new EditorCommon.Menu.MenuItemData_ShowHideControl("Graph");
            graphMenuData.MenuNames = new string[] { "Window", "Material Editor|Graph" };
            graphMenuData.Count = 1;
            graphMenuData.OperationControlType = typeof(EditorCommon.ViewPort.ViewPortControl);
            graphMenuData.Icons = new ImageSource[] { new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Blueprint_EventGraph_24x.png", UriKind.Relative)) };
            menuDatas[graphMenuData.KeyName] = graphMenuData;

            EditorCommon.Menu.GeneralMenuManager.GenerateMenuItems(Menu_Main, menuDatas);
            EditorCommon.Menu.GeneralMenuManager.Instance.GenerateGeneralMenuItems(Menu_Main);
        }

        public ImageSource CompileStatusIcon
        {
            get { return (ImageSource)GetValue(CompileStatusIconProperty); }
            set { SetValue(CompileStatusIconProperty, value); }
        }
        public static readonly DependencyProperty CompileStatusIconProperty = DependencyProperty.Register("CompileStatusIcon", typeof(ImageSource), typeof(McLogicStateMachineEditorControl), new FrameworkPropertyMetadata(null));

        private void Btn_Compile_Click(object sender, MouseButtonEventArgs e)
        {
            var noUse = CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);
        }
        bool CheckCompileResult(System.CodeDom.Compiler.CompilerResults result, EngineNS.EPlatformType platform)
        {
            if (result.Errors.HasErrors)
            {
                CompileStatusIcon = TryFindResource("Fail") as ImageSource;
                var errorStr = "";
                foreach (var error in result.Errors)
                {
                    errorStr += error.ToString() + "\r\n";
                }
                EditorCommon.MessageBox.Show($"{platform}平台MacrossScript编译失败!\r\n" + errorStr);
                return false;
            }
            else if (result.Errors.HasWarnings)
            {
                CompileStatusIcon = TryFindResource("Warning") as ImageSource;
                return false;
            }
            else
            {
                CompileStatusIcon = TryFindResource("Good") as ImageSource;
                return true;
            }
        }
        async Task CompileCode(EngineNS.EPlatformType platform)
        {
            if (!Macross_Client.CheckError())
                return;

            CurrentResourceInfo.Version++;

            List<Macross.ResourceInfos.MacrossResourceInfo.CustomFunctionData> functions = new List<Macross.ResourceInfos.MacrossResourceInfo.CustomFunctionData>();
            Macross_Client.CollectFuncDatas(functions);
            //if (CheckCompileResult(await CompileCode(Macross_Client, platform, functions), platform) == false)
            //    return;
            //if (CheckCompileResult(await CompileMacrossCollector(Macross_Client, platform), platform) == false)
            //    return;
            if (await CompileCode(Macross_Client, EngineNS.ECSType.Client))
            {
                CompileStatusIcon = TryFindResource("Good") as ImageSource;
            }
            else
            {
                CompileStatusIcon = TryFindResource("Fail") as ImageSource;
                EditorCommon.MessageBox.Show($"{platform}平台MacrossScript编译失败!详细信息请编译游戏工程!\r\n");
                return;
            }

            if (platform == EngineNS.EPlatformType.PLATFORM_WIN)
            {
                //var scriptDllName = EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll";
                EngineNS.CEngine.Instance.MacrossDataManager.RefreshMacrossCollector();
                //var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;// EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.CIPlatform.Instance.CSType, scriptDllName, "", true, true);
                //var clsTypeFullName = Macross.Program.GetClassNamespace(CurrentResourceInfo, EngineNS.ECSType.Client) + "." + Macross.Program.GetClassName(CurrentResourceInfo, EngineNS.ECSType.Client);
                //var clsType = assembly.GetType(clsTypeFullName);
                //var clsIdPro = clsType.GetProperty("ClassId");
                //var classId = (EngineNS.Hash64)clsIdPro.GetValue(null);
                //EngineNS.CEngine.Instance.MacrossDataManager.RefreshMacrossData(ref classId, clsType);
                EngineNS.Macross.MacrossFactory.SetVersion(CurrentResourceInfo.ResourceName, CurrentResourceInfo.Version);
            }

            Macross_Client.PreviewScenePG.Instance = null;
            Macross_Server.PreviewScenePG.Instance = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CompileStatusIcon = TryFindResource("Good") as ImageSource;
            InitMenus();
        }

        async Task Save()
        {
            await CurrentResourceInfo.Save();
            if (ClientVisible == Visibility.Visible)
                Macross_Client.Save();
            if (ServerVisible == Visibility.Visible)
                Macross_Server.Save();

            CurrentResourceInfo.CustomFunctions_Client.Clear();
            Macross_Client.CollectFuncDatas(CurrentResourceInfo.CustomFunctions_Client);
            CurrentResourceInfo.CustomFunctions_Server.Clear();
            Macross_Server.CollectFuncDatas(CurrentResourceInfo.CustomFunctions_Server);
            await CurrentResourceInfo.Save();

            await CompileCode(EngineNS.EPlatformType.PLATFORM_DROID);
            await CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);
        }
        async Task Load()
        {
            if (ClientVisible == Visibility.Visible)
                await Macross_Client.Load();
            if (ServerVisible == Visibility.Visible)
                await Macross_Server.Load();
        }

        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            var noUse = Save();
        }

        private void RadioButton_Compile_Debug_Checked(object sender, RoutedEventArgs e)
        {
            mCompileType = enCompileType.Debug;
        }

        private void RadioButton_Compile_Release_Checked(object sender, RoutedEventArgs e)
        {
            mCompileType = enCompileType.Release;
        }
        public UInt64 ShowSourceInDirSerialId
        {
            get;
            private set;
        }
        private void PreviewMesh_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            
        }
        private void Btn_ClassDefaults_Click(object sender, RoutedEventArgs e)
        {
            var ctrl = LinksTabControl.SelectedContent as McLogicFSMLinkControl;
            var clsIns = ctrl.GetShowMacrossClassPropertyClassInstance();
            ctrl.PreviewScenePG.Instance = clsIns;
        }
    }
}

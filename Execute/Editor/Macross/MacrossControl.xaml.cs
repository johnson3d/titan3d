using EngineNS;
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

namespace Macross
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "MacrossEditor")]
    [Guid("89435EAF-0F4F-497C-9BA7-49D5CE5B8244")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MacrossControl : MacrossControlBase, EditorCommon.PluginAssist.IEditorPlugin
    {

        void UpdateUndoRedoKey()
        {
            OnPropertyChanged("UndoRedoKey");
            Macross_Client.UpdateUndoRedoKey();
            Macross_Server.UpdateUndoRedoKey();
        }

        public string PluginName => "MacrossEditor";

        public string Version => "1.0.0";

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Blueprint_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MacrossControl), new FrameworkPropertyMetadata(null));

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
        public static readonly DependencyProperty ClientVisibleProperty = DependencyProperty.Register("ClientVisible", typeof(Visibility), typeof(MacrossControl), new UIPropertyMetadata(Visibility.Visible));
        public Visibility ServerVisible
        {
            get { return (Visibility)GetValue(ServerVisibleProperty); }
            set { SetValue(ServerVisibleProperty, value); }
        }
        public static readonly DependencyProperty ServerVisibleProperty = DependencyProperty.Register("ServerVisible", typeof(Visibility), typeof(MacrossControl), new UIPropertyMetadata(Visibility.Visible));
        public int CCSShowIdx
        {
            get { return (int)GetValue(CCSShowIdxProperty); }
            set { SetValue(CCSShowIdxProperty, value); }
        }
        public static readonly DependencyProperty CCSShowIdxProperty = DependencyProperty.Register("CCSShowIdx", typeof(int), typeof(MacrossControl), new UIPropertyMetadata(0));

        public async System.Threading.Tasks.Task SetObjectToEdit(EditorCommon.Resources.ResourceEditorContext context)
        {
            SetBinding(TitleProperty, new Binding("TitleName") { Source = context.ResInfo });

            // ResInfo 可能在不同的线程生成的，所以这里强制读取一下文件
            CurrentResourceInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(context.ResInfo.AbsInfoFileName, context.ResInfo.ParentBrowser) as ResourceInfos.MacrossResourceInfo;
            UpdateUndoRedoKey();
            var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(CurrentResourceInfo.BaseType, typeof(EngineNS.Editor.Editor_MacrossClassAttribute).FullName, false);
            if (att != null)
            {
                var csType = (EngineNS.ECSType)EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "CSType");
                switch (csType)
                {
                    case EngineNS.ECSType.All:
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        break;
                    case EngineNS.ECSType.Common:
                        {
                            ClientVisible = Visibility.Visible;
                            ServerVisible = Visibility.Visible;
                            CCSShowIdx = 0;
                            Macross_Client.CurrentResourceInfo = CurrentResourceInfo;
                            Macross_Server.CurrentResourceInfo = CurrentResourceInfo;
                        }
                        break;
                    case EngineNS.ECSType.Client:
                        {
                            ClientVisible = Visibility.Visible;
                            ServerVisible = Visibility.Collapsed;
                            CCSShowIdx = 0;
                            Macross_Client.CurrentResourceInfo = CurrentResourceInfo;
                        }
                        break;
                    case EngineNS.ECSType.Server:
                        {
                            ClientVisible = Visibility.Collapsed;
                            ServerVisible = Visibility.Visible;
                            CCSShowIdx = 1;
                            Macross_Server.CurrentResourceInfo = CurrentResourceInfo;
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

        public MacrossControl()
        {
            InitializeComponent();
            mCodeGenerator = new CodeGenerator();
            CodeGenerateSystem.Program.RegisterNodeAssembly();
            CodeGenerateSystem.Program.RegisterNodeAssembly("Macross.dll", this.GetType().Assembly);

            Macross_Client.HostControl = this;
            Macross_Server.HostControl = this;

            var template = TryFindResource("TypeSetterTemplate") as DataTemplate;
            WPG.Program.RegisterDataTemplate("TypeSetterTemplate", template);
            template = TryFindResource("TypeSelectorTemplate") as DataTemplate;
            WPG.Program.RegisterDataTemplate("TypeSelectorTemplate", template);
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
        public static readonly DependencyProperty CompileStatusIconProperty = DependencyProperty.Register("CompileStatusIcon", typeof(ImageSource), typeof(MacrossControl), new FrameworkPropertyMetadata(null));

        private void Btn_Compile_Click(object sender, MouseButtonEventArgs e)
        {
            var noUse = CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);
        }
        //bool CheckCompileResult(System.CodeDom.Compiler.CompilerResults result, EngineNS.EPlatformType platform)
        //{
        //    if (result.Errors.HasErrors)
        //    {
        //        CompileStatusIcon = TryFindResource("Fail") as ImageSource;
        //        var errorStr = "";
        //        foreach (var error in result.Errors)
        //        {
        //            errorStr += error.ToString() + "\r\n";
        //        }
        //        EditorCommon.MessageBox.Show($"{platform}平台MacrossScript编译失败!\r\n" + errorStr);
        //        return false;
        //    }
        //    else if (result.Errors.HasWarnings)
        //    {
        //        CompileStatusIcon = TryFindResource("Warning") as ImageSource;
        //        return false;
        //    }
        //    else
        //    {
        //        CompileStatusIcon = TryFindResource("Good") as ImageSource;
        //        return true;
        //    }
        //}
        async Task CompileCode(EngineNS.EPlatformType platform)
        {
            if (CurrentResourceInfo.NotGenerateCode)
                return;

            if (!Macross_Client.CheckError())
                return;

            CurrentResourceInfo.Version++;

            List<ResourceInfos.MacrossResourceInfo.CustomFunctionData> functions = new List<ResourceInfos.MacrossResourceInfo.CustomFunctionData>();
            Macross_Client.CollectFuncDatas(functions);

            //if (CheckCompileResult(await CompileCode(Macross_Client, platform, functions), platform) == false)
            //    return;
            //if (CheckCompileResult(await CompileMacrossCollector(Macross_Client, platform), platform) == false)
            //    return;
            if (await CompileCode(Macross_Client, ECSType.Client))
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
                //EngineNS.CEngine.Instance.MacrossDataManager.RefreshMacrossCollector();
                var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;//EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.CIPlatform.Instance.CSType, scriptDllName, "", true, true);
                var clsType = assembly.GetType(Program.GetClassNamespace(CurrentResourceInfo, ECSType.Client) + "." + Program.GetClassName(CurrentResourceInfo, ECSType.Client));
                EngineNS.Macross.MacrossFactory.SetVersion(CurrentResourceInfo.ResourceName, CurrentResourceInfo.Version);
            }

            Macross_Client.PG.Instance = null;
            Macross_Server.PG.Instance = null;
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CompileStatusIcon = TryFindResource("Good") as ImageSource;
            InitMenus();
        }

        async Task Save()
        {
            CurrentResourceInfo.ReferenceRNameList.Clear();
            if (ClientVisible == Visibility.Visible)
                Macross_Client.Save();
            if (ServerVisible == Visibility.Visible)
                Macross_Server.Save();

            var className = Program.GetClassName(CurrentResourceInfo, ECSType.Client);
            var classFullName= Program.GetClassNamespace(CurrentResourceInfo, ECSType.Client)+"."+ className;
            WPG.Data.PropertyCollection.RemoveCache(EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(classFullName));

            CurrentResourceInfo.CustomFunctions_Client.Clear();
            Macross_Client.CollectFuncDatas(CurrentResourceInfo.CustomFunctions_Client);
            CurrentResourceInfo.CustomFunctions_Server.Clear();
            Macross_Server.CollectFuncDatas(CurrentResourceInfo.CustomFunctions_Server);
            //await CurrentResourceInfo.Save();

            //await CompileCode(EngineNS.EPlatformType.PLATFORM_DROID);
            await CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);

            List<RName> rnames = new List<RName>();
            if (ClientVisible == Visibility.Visible)
                await mCodeGenerator.CollectMacrossResource(Macross_Client, rnames);
            if (ServerVisible == Visibility.Visible)
                await mCodeGenerator.CollectMacrossResource(Macross_Server, rnames);

            CurrentResourceInfo.ReferenceRNameList.AddRange(rnames);
            await CurrentResourceInfo.Save();
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
        private void ReGenerateCodes_Click(object sender, RoutedEventArgs e)
        {
            var noUse = ReGenerateCodes();
        }
        // 重新生成Macross代码文件
        async Task ReGenerateCodes()
        {
            var mExt = new List<string>() { "*" + EngineNS.CEngineDesc.MacrossExtension, "*.animmacross", "*.componentmacross" };
            List<string> files = new List<string>();
            foreach (var ext in mExt)
            {
                var partFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, ext + EditorCommon.Program.ResourceInfoExt, System.IO.SearchOption.AllDirectories);
                files.AddRange(partFiles);
            }
            foreach (var file in files)
            {
                if (file.Contains(""))
                    System.Diagnostics.Debug.WriteLine($"-----{file}");
                try
                {
                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file, null) as ResourceInfos.MacrossResourceInfo;
                    var context = new EditorCommon.Resources.ResourceEditorContext(PluginName, resInfo);
                    await SetObjectToEdit(context);

                    var codeStr = await mCodeGenerator.GenerateCode(CurrentResourceInfo, Macross_Client);
                    if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(CurrentResourceInfo.ResourceName.Address))
                        EngineNS.CEngine.Instance.FileManager.CreateDirectory(CurrentResourceInfo.ResourceName.Address);
                    var codeFile = $"{CurrentResourceInfo.ResourceName.Address}/{CurrentResourceInfo.ResourceName.PureName()}_{Macross_Client.CSType.ToString()}.cs";
                    using (var fs = new System.IO.FileStream(codeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
                    {
                        fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, Encoding.Default.GetByteCount(codeStr));
                    }

                    //await CompileCode(EPlatformType.PLATFORM_WIN);
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"异常:{file} " + e.ToString());
                }
            }
            await mCodeGenerator.GenerateAndSaveMacrossCollector(Macross_Client.CSType);

            List<string> macrossfiles = mCodeGenerator.CollectionMacrossProjectFiles(Macross_Client.CSType);
            mCodeGenerator.GenerateMacrossProject(macrossfiles.ToArray(), Macross_Client.CSType);
        }

        private void Btn_ClassDefaults_Click(object sender, RoutedEventArgs e)
        {
            var ctrl = LinksTabControl.SelectedContent as MacrossLinkControl;
            var clsIns = ctrl.GetShowMacrossClassPropertyClassInstance();
            ctrl.PG.Instance = clsIns;
        }

        class TempNode : CodeGenerateSystem.Base.BaseNodeControl
        {
            public TempNode(CodeGenerateSystem.Base.ConstructionParams param)
                : base(param)
            {

            }
            public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
            {

            }
        }
        CodeGenerateSystem.Base.GeneratorClassBase mClassSettingPropertiesClassInstance = null;
        private void Btn_ClassSetting_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentResourceInfo.BaseType == null)
            {
                EditorCommon.MessageBox.Show("找不到当前Macross的基类");
                return;
            }

            if(mClassSettingPropertiesClassInstance == null)
            {
                var atts = CurrentResourceInfo.BaseType.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossClassAttribute), true);
                var att = atts[0] as EngineNS.Editor.Editor_MacrossClassAttribute;
                var csType = att.CSType;
                List<CodeGenerateSystem.Base.CustomPropertyInfo> cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();

                var baseTypeCPInfo = new CodeGenerateSystem.Base.CustomPropertyInfo()
                {
                    PropertyName = "BaseType",
                    PropertyType = typeof(Type),
                    DefaultValue = CurrentResourceInfo.BaseType,
                    CurrentValue = CurrentResourceInfo.BaseType,
                };
                // Editor.Editor_BaseTypeDefine 限定类型基类
                baseTypeCPInfo.PropertyAttributes.Add(new EngineNS.Editor.Editor_TypeFilterAttribute(EngineNS.Editor.Editor_TypeFilterAttribute.enTypeFilter.Class, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable));

                cpInfos.Add(baseTypeCPInfo);

                var initParam = new CodeGenerateSystem.Base.ConstructionParams()
                {
                    CSType = csType,
                };
                var tempNode = new TempNode(initParam);
                mClassSettingPropertiesClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, tempNode, "MCSetting_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid()), false);
                var classType = mClassSettingPropertiesClassInstance.GetType();
                //var classType = CodeGenerateSystem.Base.PropertyClassGenerator.CreateTypeFromCustomPropertys(cpInfos, "MacrossClassSettingDynamicAssembly", );
                //mClassSettingPropertiesClassInstance = System.Activator.CreateInstance(classType) as CodeGenerateSystem.Base.GeneratorClassBase;
                //mClassSettingPropertiesClassInstance.CSType = csType;
                foreach(var cpInfo in cpInfos)
                {
                    var pro = classType.GetProperty(cpInfo.PropertyName);
                    pro.SetValue(mClassSettingPropertiesClassInstance, cpInfo.CurrentValue, null);
                }

                mClassSettingPropertiesClassInstance.OnPropertyChangedAction = (propertyName, newValue, oldValue) =>
                {
                    switch(propertyName)
                    {
                        case "BaseType":
                            CurrentResourceInfo.ResetBaseType((Type)newValue);
                            break;
                    }
                };
            }

            var ctrl = LinksTabControl.SelectedContent as MacrossLinkControl;
            ctrl.PG.Instance = mClassSettingPropertiesClassInstance;
        }

        private void Btn_Copy_Click(object sender, RoutedEventArgs e)
        {
            switch(CCSShowIdx)
            {
                case 0:
                    // Client
                    Macross_Client.NodesCtrlAssist.NodesControl.Copy();
                    break;
                case 1:
                    // Server
                    Macross_Server.NodesCtrlAssist.NodesControl.Copy();
                    break;
            }
        }
        private void Btn_Paste_Click(object sender, RoutedEventArgs e)
        {
            switch(CCSShowIdx)
            {
                case 0:
                    {
                        // Client
                        var center = Macross_Client.NodesCtrlAssist.NodesControl.GetViewCenter();
                        Macross_Client.NodesCtrlAssist.NodesControl.Paste(center);
                    }
                    break;
                case 1:
                    {
                        // Server
                        var center = Macross_Client.NodesCtrlAssist.NodesControl.GetViewCenter();
                        Macross_Server.NodesCtrlAssist.NodesControl.Paste(center);
                    }
                    break;
            }
        }
        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            switch (CCSShowIdx)
            {
                case 0:
                    {
                        // Client
                        Macross_Client.NodesCtrlAssist.NodesControl.DeleteSelectedNodes();
                    }
                    break;
                case 1:
                    {
                        // Server
                        Macross_Server.NodesCtrlAssist.NodesControl.DeleteSelectedNodes();
                    }
                    break;
            }
        }
    }
}

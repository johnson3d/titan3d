using DockControl;
using EditorCommon.Resources;
using EngineNS;
using EngineNS.IO;
using Macross;
using System;
using System.Collections.Generic;
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

namespace ParticleEditor
{
    /// <summary>
    /// ParticleMacrossControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "ParticleEditor")]
    [Guid("7378116B-DB61-40AC-BD89-A101FE8E1361")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ParticleMacrossControl : MacrossControlBase, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region pluginInterface
        public string PluginName
        {
            get { return "ParticleViewEditor"; }
        }
        public string Version
        {
            get { return "1.0.0"; }
        }

        public bool OnActive()
        {
            return true;
        }
        public bool OnDeactive()
        {
            return true;
        }

        public void Tick()
        {

        }

        public object[] GetObjects(object[] param)
        {
            return null;
        }

        public bool RemoveObjects(object[] param)
        {
            return false;
        }

        public ImageSource Icon { get; }
        public Brush IconBrush { get; }

        void IDockAbleControl.StartDrag()
        {
            //throw new NotImplementedException();
        }

        void IDockAbleControl.EndDrag()
        {
            //throw new NotImplementedException();
        }

        bool? IDockAbleControl.CanClose()
        {
            return true;
            //throw new NotImplementedException();
        }

        void IDockAbleControl.Closed()
        {
            //throw new NotImplementedException();
        }

        void IDockAbleControl.SaveElement(XmlNode node, XmlHolder holder)
        {
            throw new NotImplementedException();
        }

        IDockAbleControl IDockAbleControl.LoadElement(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public ParticleResourceInfo CurrentParticleResourceInfo
        {
            get;
            protected set;
        }

        public async Task SetObjectToEdit(ResourceEditorContext context)
        {
            if (context.ResInfo == null)
                return;

            await Particle_Client.Viewport.WaitInitComplated();
            SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });

            await Particle_Client.SetObjectToEdit();
            // ResInfo 可能在不同的线程生成的，所以这里强制读取一下文件
            CurrentResourceInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(context.ResInfo.AbsInfoFileName, context.ResInfo.ParentBrowser) as Macross.ResourceInfos.MacrossResourceInfo;

            CurrentParticleResourceInfo = CurrentResourceInfo as ParticleResourceInfo;
            UpdateUndoRedoKey();

            Particle_Client.CurrentResourceInfo = CurrentResourceInfo;
            //var ddd = new EngineNS.Bricks.Particle.McParticleEffector();
            await Particle_Client.Load();

            
            string filename = CurrentResourceInfo.AbsInfoFileName.Replace(CurrentResourceInfo.AbsPath, "").Replace(".macross.rinfo", "");
            string csname = CurrentResourceInfo.ResourceName.Address + "/" + filename + "_Client.cs";
            if (!EngineNS.CEngine.Instance.FileManager.FileExists(csname))
            {
                await Save();
            }
            else
            {
                if (CurrentParticleResourceInfo.NeedRefresh)
                {
                    await CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);
                    CurrentParticleResourceInfo.NeedRefresh = false;
                    await CurrentParticleResourceInfo.Save();
                }
                //await CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);
                string pfxcs = CurrentResourceInfo.AbsInfoFileName.Replace(".rinfo", "");
                EngineNS.RName pfxname = EngineNS.RName.GetRName(pfxcs.Replace(CEngine.Instance.FileManager.ProjectContent, ""));

                await Particle_Client.AddPfxMacross(pfxname);
            }

             //await CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);
        }

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public UIElement InstructionControl => throw new NotImplementedException();
        #endregion

        public ImageSource CompileStatusIcon
        {
            get { return (ImageSource)GetValue(CompileStatusIconProperty); }
            set { SetValue(CompileStatusIconProperty, value); }
        }
        public static readonly DependencyProperty CompileStatusIconProperty = DependencyProperty.Register("CompileStatusIcon", typeof(ImageSource), typeof(ParticleMacrossControl), new FrameworkPropertyMetadata(null));
        void UpdateUndoRedoKey()
        {
            OnPropertyChanged("UndoRedoKey");
            Particle_Client.UpdateUndoRedoKey();
        }
        public ParticleMacrossControl()
        {
            try
            {
                EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(ParticleResourceInfo));
                InitializeComponent();
                mCodeGenerator = new ParticleEditor.CodeGenerator();
            }
            catch(System.Exception e)
            {

            }

            var template = TryFindResource("DataGradientSetter") as DataTemplate;
            WPG.Program.RegisterDataTemplate("DataGradientSetter", template);

            template = TryFindResource("ColorGradientSetter") as DataTemplate;
            WPG.Program.RegisterDataTemplate("ColorGradientSetter", template);

            template = TryFindResource("TransformGradientSetter") as DataTemplate;
            WPG.Program.RegisterDataTemplate("TransformGradientSetter", template);
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

        async Task CompileCode(EngineNS.EPlatformType platform, bool usecache = false)
        {
            if (!Particle_Client.CheckError())
                return;

            CurrentResourceInfo.Version++;

            List<Macross.ResourceInfos.MacrossResourceInfo.CustomFunctionData> functions = new List<Macross.ResourceInfos.MacrossResourceInfo.CustomFunctionData>();
            Particle_Client.CollectFuncDatas(functions);

            //if (CheckCompileResult(await CompileCode(Macross_Client, platform, functions), platform) == false)
            //    return;
            //if (CheckCompileResult(await CompileMacrossCollector(Macross_Client, platform), platform) == false)
            //    return;
            if (await CompileCode(Particle_Client, ECSType.Client, usecache))
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
                //var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;//EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.CIPlatform.Instance.CSType, scriptDllName, "", true, true);
                //var clsTypeFullName = Macross.Program.GetClassNamespace(CurrentResourceInfo, ECSType.Client) + "." + Macross.Program.GetClassName(CurrentResourceInfo, ECSType.Client);
                //var clsType = assembly.GetType(clsTypeFullName);
                //var clsIdPro = clsType.GetProperty("ClassId");
                //var classId = (EngineNS.Hash64)clsIdPro.GetValue(null);
                //EngineNS.CEngine.Instance.MacrossDataManager.RefreshMacrossData(ref classId, clsType);
                EngineNS.Macross.MacrossFactory.SetVersion(CurrentResourceInfo.ResourceName, CurrentResourceInfo.Version);
            }

            Particle_Client.PG.Instance = null;

            string pfxcs = CurrentResourceInfo.AbsInfoFileName.Replace(".rinfo", "");
            EngineNS.RName pfxname = EngineNS.RName.GetRName(pfxcs.Replace(CEngine.Instance.FileManager.ProjectContent, ""));
            await Particle_Client.AddPfxMacross(pfxname);
        }
        
        List<string> MacrossFiles;
        protected async Task<bool> CompileCode(MacrossLinkControlBase ctrl, EngineNS.ECSType csType, bool usecache)
        {
            //每次保存和编译需要重新设置下
            var CodeGenerator = mCodeGenerator as ParticleEditor.CodeGenerator;
            CodeGenerator.ParticleComponent = Particle_Client.ParticleComponent;
            CodeGenerator.ParticleSetter = Particle_Client.ParticleSetter;
            await mCodeGenerator.GenerateAndSaveMacrossCollector(csType);
            var codeStr = await mCodeGenerator.GenerateCode(CurrentResourceInfo, ctrl, CodeGenerator.GenerateParticleTemplateCode);
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(CurrentResourceInfo.ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(CurrentResourceInfo.ResourceName.Address);
            var codeFile = $"{CurrentResourceInfo.ResourceName.Address}/{CurrentResourceInfo.ResourceName.PureName()}_{ctrl.CSType.ToString()}.cs";
            using (var fs = new System.IO.FileStream(codeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, Encoding.Default.GetByteCount(codeStr));
            }

            if (MacrossFiles == null || usecache == false)
            {
                var codeFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, $"*_{ctrl.CSType.ToString()}.cs", System.IO.SearchOption.AllDirectories);
                var enumFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*.macross_enum.cs", System.IO.SearchOption.AllDirectories);

                MacrossFiles = new List<string>();
                MacrossFiles.AddRange(codeFiles);
                //files.Add(codeFiles[0]);
                MacrossFiles.AddRange(enumFiles);
                //mCodeGenerator.GenerateMacrossProject(MacrossFiles.ToArray(), csType);
                mCodeGenerator.GenerateMacrossProject(MacrossFiles.ToArray(), csType);
            }
            else
            {
                try
                {
                    var projFileName = EditorCommon.GameProjectConfig.Instance.MacrossGenerateProjItemsFileName;
                    var projDir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(projFileName);
                    var fileName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(codeFile, EngineNS.CEngine.Instance.FileManager.ProjectContent);
                    string pfxname = CurrentResourceInfo.AbsInfoFileName.Replace(CurrentResourceInfo.AbsPath, "").Replace(".rinfo", "");
                    var tagFile = (projDir + fileName).Replace(pfxname + "/", "");
                    var tagDir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(tagFile);
                    if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(tagDir))
                        EngineNS.CEngine.Instance.FileManager.CreateDirectory(tagDir);
                    EngineNS.CEngine.Instance.FileManager.CopyFile(codeFile, tagFile, true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
                
            }

            return await EditorCommon.Program.BuildGameDllImmediately(true);
        }

        async Task Save()
        {
            //CurrentResourceInfo.ReferenceRNameList.Clear();
            //Particle_Client.Save();

            //CurrentResourceInfo.CustomFunctions_Client.Clear();
            //Particle_Client.CollectFuncDatas(CurrentResourceInfo.CustomFunctions_Client);

            ////await CompileCode(EngineNS.EPlatformType.PLATFORM_DROID);
            //await CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);

            //List<RName> rnames = new List<RName>();
            //await mCodeGenerator.GenerateMacrossResource(Particle_Client, rnames);


            //foreach (var rname in rnames)
            //{
            //    CurrentResourceInfo.ReferenceRNameList.Add(rname);
            //}
            //await CurrentResourceInfo.Save();



            CurrentResourceInfo.ReferenceRNameList.Clear();
            Particle_Client.Save();

            var className = Program.GetClassName(CurrentResourceInfo, ECSType.Client);
            var classFullName = Program.GetClassNamespace(CurrentResourceInfo, ECSType.Client) + "." + className;
            WPG.Data.PropertyCollection.RemoveCache(EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(classFullName));

            CurrentResourceInfo.CustomFunctions_Client.Clear();
            Particle_Client.CollectFuncDatas(CurrentResourceInfo.CustomFunctions_Client);

            //await CompileCode(EngineNS.EPlatformType.PLATFORM_DROID);
            await CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);

            List<RName> rnames = new List<RName>();
            await mCodeGenerator.CollectMacrossResource(Particle_Client, rnames);
            
            foreach (var rname in rnames)
            {
                CurrentResourceInfo.ReferenceRNameList.Add(rname);
            }

            await CurrentResourceInfo.Save();

            var IsShowFloor = Particle_Client.SceneControl.IsShowFloor;
            var IsShowSkyBox = Particle_Client.SceneControl.IsShowSkyBox;
            Particle_Client.SceneControl.IsShowFloor = false;
            Particle_Client.SceneControl.IsShowSkyBox = false;

            var snapShotFile = CurrentResourceInfo.ResourceName.Address + EditorCommon.Program.SnapshotExt;
            var data = new EngineNS.Support.CBlobObject[1];
            data[0] = new EngineNS.Support.CBlobObject();
            var rc = EngineNS.CEngine.Instance.RenderContext;
            Particle_Client.SceneControl.ViewPort.RPolicy.mCopyPostprocessPass.mScreenView.FrameBuffer.GetSRV_RenderTarget(0).Save2Memory(rc, data[0], EngineNS.EIMAGE_FILE_FORMAT.PNG);
            EngineNS.CShaderResourceView.SaveSnap(snapShotFile, data);

            Particle_Client.SceneControl.IsShowFloor = IsShowFloor;
            Particle_Client.SceneControl.IsShowSkyBox = IsShowSkyBox;
        }

        //bool WireFrame = false;
        #region event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CompileStatusIcon = TryFindResource("Good") as ImageSource;
            InitMenus();
        }

        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentParticleResourceInfo.NeedRefresh = false;
            var test = Save();
        }

        private void Btn_Compile_Click(object sender, MouseButtonEventArgs e)
        {
            var test = CompileCode(EngineNS.EPlatformType.PLATFORM_WIN);
        }

        private void RadioButton_Compile_Release_Checked(object sender, RoutedEventArgs e)
        {
            mCompileType = enCompileType.Debug;
        }

        private void RadioButton_Compile_Debug_Checked(object sender, RoutedEventArgs e)
        {
            mCompileType = enCompileType.Release;
        }
        #endregion

        private void MacrossControlBase_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                ////var test = CompileCode(EngineNS.EPlatformType.PLATFORM_WIN, true);

                //string filename = CurrentResourceInfo.AbsInfoFileName.Replace(CurrentResourceInfo.AbsPath, "").Replace(".macross.rinfo", "");
                //string name = CurrentResourceInfo.ResourceName.Address + "/" + filename + "_Client.cs";
                //CurrentResourceInfo.ReferenceRNameList.Clear();
                //Particle_Client.Save();

                //CurrentResourceInfo.CustomFunctions_Client.Clear();
                //Particle_Client.CollectFuncDatas(CurrentResourceInfo.CustomFunctions_Client);
                var noinfo = CompileCode(EngineNS.EPlatformType.PLATFORM_WIN, true);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Particle_Client.ResetAix();
                e.Handled = true;

            }
            else if (e.Key == Key.OemComma)
            {
                Particle_Client.UIEffect.PlayAndPause();
                e.Handled = true;
            }
            else if (e.Key == Key.OemPeriod)
            {
                Particle_Client.UIEffect.Stop ();
                e.Handled = true;
            }
            else if (e.Key == Key.Oem2)
            {
                Particle_Client.UIEffect.Restart();
                e.Handled = true;
            }

        }

        private void Btn_Refresh_Click(object sender, MouseButtonEventArgs e)
        {
            var noinfo = CompileCode(EngineNS.EPlatformType.PLATFORM_WIN, true);
            CurrentParticleResourceInfo.NeedRefresh = true;
            var test=  CurrentResourceInfo.Save();
        }

        private void UIShowShape_Click(object sender, RoutedEventArgs e)
        {
            //Particle_Client.CanShowMesh = UIShowShape.IsChecked == null ? false : UIShowShape.IsChecked.Value;
            //Particle_Client.ShowMeshComponent(UIShowShape.IsEnabled);
        }

        private void Btn_ClassDefaults_Click(object sender, RoutedEventArgs e)
        {
            var clsIns = Particle_Client.GetShowMacrossClassPropertyClassInstance();
            Particle_Client.PG.Instance = clsIns;
        }

        //public void GenerateMacrossProject(string[] macrossCodeFiles, EngineNS.ECSType csType)
        //{
        //    var projFileName = EngineNS.CEngine.Instance.FileManager.Root + EditorCommon.GameProjectConfig.Instance.MacrossGenerateProjItemsFileName;
        //    var projDir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(projFileName);
        //    if (!EngineNS.CEngine.Instance.FileManager.FileExists(projFileName))
        //    {
        //        var srcFileName = EngineNS.CEngine.Instance.FileManager.EditorContent + "Macross/Macross.Generated.projitems";
        //        EngineNS.CEngine.Instance.FileManager.CopyFile(srcFileName, projFileName, true);
        //        var srcPJFileName = EngineNS.CEngine.Instance.FileManager.EditorContent + "Macross/Macross.Generated.shproj";
        //        var tagPJFileName = projDir + "Macross.Generated.shproj";
        //        EngineNS.CEngine.Instance.FileManager.CopyFile(srcPJFileName, tagPJFileName, true);
        //    }
        //    EngineNS.CEngine.Instance.FileManager.DeleteFilesInDirectory(projDir, "*.cs", System.IO.SearchOption.TopDirectoryOnly);
        //    foreach (var dir in EngineNS.CEngine.Instance.FileManager.GetDirectories(projDir))
        //    {
        //        EngineNS.CEngine.Instance.FileManager.DeleteDirectory(dir, true);
        //    }

        //    var xml = new XmlDocument();
        //    xml.Load(projFileName);

        //    var strWriter = new System.IO.StringWriter();
        //    var xmlTextWriter = new XmlTextWriter(strWriter);
        //    xml.WriteTo(xmlTextWriter);
        //    var oldContent = strWriter.ToString();

        //    var nsmgr = new XmlNamespaceManager(xml.NameTable);
        //    var nsUrl = "http://schemas.microsoft.com/developer/msbuild/2003";
        //    nsmgr.AddNamespace("xlns", nsUrl);
        //    var root = xml.DocumentElement;

        //    var itemGroupNode = root.SelectSingleNode("descendant::xlns:ItemGroup", nsmgr);
        //    if (itemGroupNode == null)
        //    {
        //        itemGroupNode = xml.CreateElement("ItemGroup", nsUrl);
        //        root.AppendChild(itemGroupNode);
        //    }
        //    var nodes = root.SelectNodes("descendant::xlns:ItemGroup/xlns:Compile", nsmgr);
        //    foreach (System.Xml.XmlNode node in nodes)
        //        node.ParentNode.RemoveChild(node);

        //    // 只处理在Content中的Macross，EngineContent及EditorContent中的不属于游戏逻辑
        //    foreach (var file in macrossCodeFiles)
        //    {
        //        var fileName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(file, EngineNS.CEngine.Instance.FileManager.Content);
        //        if (fileName.IndexOf("macross_enum") == -1)
        //        {
        //            var path1 = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(file);
        //            var path2 = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(path1);
        //            path1 = path1.Replace(path2, "");
        //            fileName = fileName.Replace(path1, "").Replace("/", "\\");
        //        }

        //        var node = xml.CreateElement("Compile", nsUrl);
        //        node.SetAttribute("Include", $"$(MSBuildThisFileDirectory){fileName}");
        //        itemGroupNode.AppendChild(node);

        //        var tagFile = projDir + fileName;
        //        var tagDir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(tagFile);
        //        if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(tagDir))
        //            EngineNS.CEngine.Instance.FileManager.CreateDirectory(tagDir);
        //        EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, true);
        //    }
        //    var colFile = EngineNS.CEngine.Instance.MacrossDataManager.GetCollectorCodeFileName(csType);
        //    var colFileName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(colFile);
        //    var colTagFile = projDir + colFileName;
        //    EngineNS.CEngine.Instance.FileManager.CopyFile(colFile, colTagFile, true);
        //    var colNode = xml.CreateElement("Compile", nsUrl);
        //    colNode.SetAttribute("Include", $"$(MSBuildThisFileDirectory){colFileName}");
        //    itemGroupNode.AppendChild(colNode);

        //    strWriter = new System.IO.StringWriter();
        //    xmlTextWriter = new XmlTextWriter(strWriter);
        //    xml.WriteTo(xmlTextWriter);
        //    var tagContent = strWriter.ToString();
        //    if (tagContent != oldContent)
        //        xml.Save(projFileName);
        //}

    }
}

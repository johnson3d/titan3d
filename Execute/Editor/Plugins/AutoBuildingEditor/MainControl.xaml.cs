using DockControl;
using EditorCommon.PluginAssist;
using EditorCommon.Resources;
using EngineNS.IO;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections.Generic;
using System.Reflection;
using EngineNS.Editor;
using System.Security.Cryptography;

using EnsureFile;

namespace AutoBuildingEditor
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    /// 
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "AutoBuilding")]
    [EditorCommon.PluginAssist.PluginMenuItem(EditorCommon.Menu.MenuItemDataBase.enMenuItemType.OneClick, new string[] { "Window", "General|项目编译器" })]
    [Guid("FCF8826C-B70F-4916-B5C6-387202F9CB4E")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MainControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region pluginInterface
        public string PluginName
        {
            get { return "项目编译器"; }
        }
        public string Version
        {
            get { return "1.0.0"; }
        }

        System.Windows.UIElement mInstructionControl = new System.Windows.Controls.TextBlock()
        {
            Text = "项目编译器",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        public System.Windows.UIElement InstructionControl
        {
            get { return mInstructionControl; }
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

        public void SetObjectToEdit(object[] obj)
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

        public string Title { get; }
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
        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        async System.Threading.Tasks.Task IEditorPlugin.SetObjectToEdit(ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
        }
        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";
        #endregion

        #region SceneTrees
        public class SceneData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string HighLightString
            {
                get { return (string)GetValue(HighLightStringProperty); }
                set { SetValue(HighLightStringProperty, value); }
            }

            public bool IsChecked
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string SceneName
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }
            public string SceneNameToLower
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string SceneAddress
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public EngineNS.RName ResourceName
            {
                get;
                set;
            }

            public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(SceneData), new FrameworkPropertyMetadata(null));
            //public static readonly DependencyProperty IsCheckProperty = DependencyProperty.Register("IsCheck", typeof(bool), typeof(SceneData), new FrameworkPropertyMetadata(true));

            //public EngineNS.GamePlay.Actor.GActor Actor { get; set; }

            public SceneData()
            {
                //Children = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }

        #endregion

        #region UITrees
        public class UIData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string HighLightString
            {
                get { return (string)GetValue(HighLightStringProperty); }
                set { SetValue(HighLightStringProperty, value); }
            }

            public bool IsChecked
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string UIName
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }
            public string UINameToLower
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string UIAddress
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public EngineNS.RName ResourceName
            {
                get;
                set;
            }

            public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(UIData), new FrameworkPropertyMetadata(null));
            //public static readonly DependencyProperty IsCheckProperty = DependencyProperty.Register("IsCheck", typeof(bool), typeof(SceneData), new FrameworkPropertyMetadata(true));

            //public EngineNS.GamePlay.Actor.GActor Actor { get; set; }

            public UIData()
            {
                //Children = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }

        #endregion

        #region GameTrees
        public class GameData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string HighLightString
            {
                get { return (string)GetValue(HighLightStringProperty); }
                set { SetValue(HighLightStringProperty, value); }
            }

            public bool IsChecked
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string GameName
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }
            public string GameNameToLower
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string GameAddress
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public EngineNS.RName ResourceName
            {
                get;
                set;
            }

            public ResourceInfo ResourceInfo
            {
                get;
                set;
            }

            public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(GameData), new FrameworkPropertyMetadata(null));
            //public static readonly DependencyProperty IsCheckProperty = DependencyProperty.Register("IsCheck", typeof(bool), typeof(SceneData), new FrameworkPropertyMetadata(true));

            //public EngineNS.GamePlay.Actor.GActor Actor { get; set; }

            public GameData()
            {
                //Children = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }

        #endregion

        #region ExcelTrees
        public class ExcelData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string HighLightString
            {
                get { return (string)GetValue(HighLightStringProperty); }
                set { SetValue(HighLightStringProperty, value); }
            }

            public bool IsChecked
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string ExcelName
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }
            public string ExcelNameToLower
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public string ExcelAddress
            {
                //get { return (bool)GetValue(IsCheckProperty); }
                //set { SetValue(IsCheckProperty, value); }
                get;
                set;
            }

            public ResourceInfo ResourceInfo
            {
                get;
                set;
            }

            public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(ExcelData), new FrameworkPropertyMetadata(null));
            //public static readonly DependencyProperty IsCheckProperty = DependencyProperty.Register("IsCheck", typeof(bool), typeof(SceneData), new FrameworkPropertyMetadata(true));

            //public EngineNS.GamePlay.Actor.GActor Actor { get; set; }

            public ExcelData()
            {
                //Children = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }

        #endregion

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        string mFilterString = "";
        string mLowerFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                mLowerFilterString = value.ToLower();

                mUIFilterString = value;
                mLowerUIFilterString = value.ToLower();

                OnPropertyChanged("FilterString");
                OnPropertyChanged("UIFilterString");

            }
        }

        string mUIFilterString = "";
        string mLowerUIFilterString = "";
        public string UIFilterString
        {
            get { return mUIFilterString; }
            set
            {
                mUIFilterString = value;
                mLowerUIFilterString = value.ToLower();
                OnPropertyChanged("UIFilterString");
            }
        }

        System.Diagnostics.Process proc;

        public PublisherConfig PConfig
        {
            get;
            set;
        }

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> TreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> TreeViewItemsDatas = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> UITreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> UITreeViewItemsDatas = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> GameTreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> GameTreeViewItemsDatas = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> ExcelTreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> ExcelTreeViewItemsDatas = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();

        EngineNS.IO.XmlHolder AssetInfos;

        public MainControl()
        {
            InitializeComponent();
            proc = new System.Diagnostics.Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            SceneTrees.TreeListItemsSource = TreeViewItemsNodes;

            UITrees.TreeListItemsSource = UITreeViewItemsNodes;

            GameTrees.TreeListItemsSource = GameTreeViewItemsNodes;
            ExcelTrees.TreeListItemsSource = ExcelTreeViewItemsNodes;


            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            //startInfo.FileName = Application.ExecutablePath;

            startInfo.Verb = "runas";
            //还是需要用户管理员权限的
            //proc.StartInfo = startInfo; 

            PConfig = new PublisherConfig();

            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(PConfig.GetType());
            //var test = LoadConfigFromFile();
        }

        private void SaveConfig()
        {
            PConfig.Version = Version_Ctr.Text;
            PConfig.IsAndroid = Android.IsChecked == true;
            PConfig.IsPC = PC.IsChecked == true;
            PConfig.IsCopyRInfo = CopyRInfo.IsChecked == true;
            PConfig.IsIOS = IOS.IsChecked == true;
            PConfig.IsReBuild = RebuildSelect.IsChecked == true;
            PConfig.IsBuild = BuildSelect.IsChecked == true;
            PConfig.IsDebug = Debug.IsChecked == true;
            PConfig.IsRelease = Release.IsChecked == true;

            PConfig.OutDir = OutPathText.Text;
        }

        private void SaveConfigToFile()
        {
            SaveConfig();

            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.AddAttrib("PublisherConfig");
            attr.BeginWrite();
            attr.WriteMetaObject(PConfig);

            //处理场景配置
            {
                PConfig.SelectScenes.Clear();
                foreach (var i in TreeViewItemsNodes)
                {
                    SceneData sd = i as SceneData;
                    if (sd.IsChecked == true)
                    {
                        PConfig.SelectScenes.Add(sd.SceneNameToLower);
                    }
                }

                attr.Write(PConfig.SelectScenes.Count);
                for (int i = 0; i < PConfig.SelectScenes.Count; i++)
                {
                    attr.Write(PConfig.SelectScenes[i]);
                }
            }

            //处理UI配置
            {
                PConfig.SelectUIs.Clear();
                foreach (var i in UITreeViewItemsNodes)
                {
                    UIData sd = i as UIData;
                    if (sd.IsChecked == true)
                    {
                        PConfig.SelectUIs.Add(sd.UINameToLower);
                    }
                }

                attr.Write(PConfig.SelectUIs.Count);
                for (int i = 0; i < PConfig.SelectUIs.Count; i++)
                {
                    attr.Write(PConfig.SelectUIs[i]);
                }
            }

            //处理Game配置
            {
                PConfig.SelectGames.Clear();
                foreach (var i in GameTreeViewItemsNodes)
                {
                    GameData sd = i as GameData;
                    if (sd.IsChecked == true)
                    {
                        PConfig.SelectGames.Add(sd.GameNameToLower);
                    }
                }

                attr.Write(PConfig.SelectGames.Count);
                for (int i = 0; i < PConfig.SelectGames.Count; i++)
                {
                    attr.Write(PConfig.SelectGames[i]);
                }
            }

            //处理Excel配置
            {
                PConfig.SelectExcels.Clear();
                foreach (var i in ExcelTreeViewItemsNodes)
                {
                    ExcelData sd = i as ExcelData;
                    if (sd.IsChecked == true)
                    {
                        PConfig.SelectExcels.Add(sd.ExcelNameToLower);
                    }
                }

                attr.Write(PConfig.SelectExcels.Count);
                for (int i = 0; i < PConfig.SelectExcels.Count; i++)
                {
                    attr.Write(PConfig.SelectExcels[i]);
                }
            }

            attr.EndWrite();

            EngineNS.IO.XndHolder.SaveXND("PublisherConfig.cfg", xnd);
            xnd.Node.TryReleaseHolder();

            //var test = LoadConfigFromFile();
        }

        bool IsNeedLoadConfig = true;
        private void LoadConfig()
        {
            if (!IsNeedLoadConfig)
                return;
            IsNeedLoadConfig = false;

            Version_Ctr.Text = PConfig.Version;
            Android.IsChecked = PConfig.IsAndroid;
            PC.IsChecked = PConfig.IsPC;
            CopyRInfo.IsChecked = PConfig.IsCopyRInfo;
            IOS.IsChecked = PConfig.IsIOS;
            RebuildSelect.IsChecked = PConfig.IsReBuild;
            BuildSelect.IsChecked = PConfig.IsBuild;
            Debug.IsChecked = PConfig.IsDebug;
            Release.IsChecked = PConfig.IsRelease;

            OutPathText.Text = PConfig.OutDir;
        }

        private bool IsSelectScene(string address)
        {

            for (int i = 0; i < PConfig.SelectScenes.Count; i++)
            {
                if (PConfig.SelectScenes[i].Equals(address))
                    return true;
            }
            return false;
        }

        private bool IsSelectUI(string address)
        {

            for (int i = 0; i < PConfig.SelectUIs.Count; i++)
            {
                if (PConfig.SelectUIs[i].Equals(address))
                    return true;
            }
            return false;
        }

        private bool IsSelectGame(string address)
        {

            for (int i = 0; i < PConfig.SelectGames.Count; i++)
            {
                if (PConfig.SelectGames[i].Equals(address))
                    return true;
            }
            return false;
        }

        private bool IsSelectExcel(string address)
        {

            for (int i = 0; i < PConfig.SelectExcels.Count; i++)
            {
                if (PConfig.SelectExcels[i].Equals(address))
                    return true;
            }
            return false;
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            await LoadConfigFromFile();
            LoadConfig();
            await LoadSceneDatas();
            RefreashScenes();
            RefreashUIs();
            RefreashGames();
            RefreashExcels();
        }

        private async System.Threading.Tasks.Task LoadConfigFromFile()
        {
            var xnd = await EngineNS.IO.XndHolder.LoadXND("PublisherConfig.cfg");
            if (xnd == null)
            {
                await LoadSceneDatas();
                return;
            }


            var attr = xnd.Node.FindAttrib("PublisherConfig");
            if (attr == null)
            {
                await LoadSceneDatas();
                return;
            }
            attr.BeginRead();
            PConfig = new PublisherConfig();
            attr.ReadMetaObject(PConfig);


            int count = 0;
            {
                attr.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    string name = "";
                    attr.Read(out name);
                    PConfig.SelectScenes.Add(name);
                }
            }

            {
                attr.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    string name = "";
                    attr.Read(out name);
                    PConfig.SelectUIs.Add(name);
                }
            }

            {
                attr.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    string name = "";
                    attr.Read(out name);
                    PConfig.SelectGames.Add(name);
                }
            }

            {
                attr.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    string name = "";
                    attr.Read(out name);
                    PConfig.SelectExcels.Add(name);
                }

            }
            attr.EndRead();
            xnd.Node.TryReleaseHolder();

            if (PConfig.ADBAddress != null)
            {
                UIGameRes.UIADBAdrress.Text = PConfig.ADBAddress;
                AccessGameRes.InitADBAddress(PConfig.ADBAddress);
            }

            //LoadConfig();
            await LoadSceneDatas();
        }

        bool IsLoadScenesFinished = false;
        bool IsLoadingScenes = false;
        bool IsNeedLoadScenes = true;
        List<string> MacrossInfos = new List<string>();
        List<EngineNS.RName> TempAnimas = new List<EngineNS.RName>();
        private async System.Threading.Tasks.Task LoadSceneDatas()
        {
            if (IsLoadingScenes)
                return;

            IsLoadingScenes = true;
            try
            {
                DirectoryInfo folder = new DirectoryInfo(EngineNS.CEngine.Instance.FileManager.ProjectContent);
                MacrossInfos.Clear();
                TreeViewItemsDatas.Clear();
                TreeViewItemsNodes.Clear();
                UITreeViewItemsDatas.Clear();
                UITreeViewItemsNodes.Clear();

                GameTreeViewItemsDatas.Clear();
                GameTreeViewItemsNodes.Clear();

                ExcelTreeViewItemsDatas.Clear();
                ExcelTreeViewItemsNodes.Clear();

                TempAnimas.Clear();

                var fileinfos = folder.GetFiles("*.rinfo", SearchOption.AllDirectories);
                //foreach (FileInfo file in fileinfos)
                for (int i = 0; i < fileinfos.Length; i++)
                {
                    var file = fileinfos[i];

                    if (file.FullName.IndexOf("map.rinfo") != -1 || file.FullName.IndexOf("macross.rinfo") != -1)
                    {
                        var rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file.FullName, null);
                        if (rInfo != null)
                        {
                            if (rInfo.ResourceTypeName.Equals(EngineNS.Editor.Editor_RNameTypeAttribute.Scene))
                            {
                                SceneData scenedata = new SceneData();
                                scenedata.SceneName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(rInfo.ResourceName.Address);
                                scenedata.SceneNameToLower = scenedata.SceneName.ToLower();
                                scenedata.SceneAddress = rInfo.ResourceName.Address;
                                scenedata.ResourceName = rInfo.ResourceName;
                                scenedata.IsChecked = IsSelectScene(scenedata.SceneNameToLower);
                                TreeViewItemsDatas.Add(scenedata);

                                if (scenedata.SceneNameToLower.IndexOf(mFilterString) > -1)
                                {
                                    BindingOperations.SetBinding(scenedata, SceneData.HighLightStringProperty, new Binding("FilterString") { Source = this });
                                    TreeViewItemsNodes.Add(scenedata);
                                }
                            }
                            else if (rInfo.ResourceTypeName.Equals(EngineNS.Editor.Editor_RNameTypeAttribute.UI))
                            {
                                UIData uidata = new UIData();
                                uidata.UIName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(rInfo.ResourceName.Address);
                                uidata.UINameToLower = uidata.UIName.ToLower();
                                uidata.UIAddress = rInfo.ResourceName.Address;
                                uidata.ResourceName = rInfo.ResourceName;
                                uidata.IsChecked = IsSelectUI(uidata.UINameToLower);
                                UITreeViewItemsDatas.Add(uidata);

                                if (uidata.UINameToLower.IndexOf(mUIFilterString) > -1)
                                {
                                    BindingOperations.SetBinding(uidata, UIData.HighLightStringProperty, new Binding("FilterString") { Source = this });
                                    UITreeViewItemsNodes.Add(uidata);
                                }

                            }

                            else if (rInfo.ResourceTypeName.Equals(EngineNS.Editor.Editor_RNameTypeAttribute.Macross))
                            {
                                var gameinstance = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<EngineNS.GamePlay.McGameInstance>(rInfo.ResourceName);
                                if (gameinstance != null)
                                {
                                    GameData data = new GameData();
                                    data.GameName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(rInfo.ResourceName.Address);
                                    data.GameNameToLower = data.GameName.ToLower();
                                    data.GameAddress = rInfo.ResourceName.Address;
                                    data.ResourceName = rInfo.ResourceName;
                                    data.IsChecked = IsSelectGame(data.GameNameToLower);
                                    data.ResourceInfo = rInfo;
                                    GameTreeViewItemsDatas.Add(data);

                                    if (data.GameNameToLower.IndexOf(mFilterString) > -1)
                                    {
                                        //BindingOperations.SetBinding(uidata, UIData.HighLightStringProperty, new Binding("FilterString") { Source = this });
                                        GameTreeViewItemsNodes.Add(data);
                                    }
                                }
                            }
                        }
                    }
                }

                fileinfos = folder.GetFiles("*.xls", SearchOption.AllDirectories);
                for (int i = 0; i < fileinfos.Length; i++)
                {
                    var file = fileinfos[i];

                    ExcelData data = new ExcelData();
                    data.ExcelName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(file.FullName);
                    data.ExcelNameToLower = data.ExcelName.ToLower();
                    data.ExcelAddress = file.FullName;
                    data.IsChecked = IsSelectExcel(data.ExcelNameToLower);
                    ExcelTreeViewItemsDatas.Add(data);

                    if (data.ExcelNameToLower.IndexOf(mFilterString) > -1)
                    {
                        //BindingOperations.SetBinding(uidata, UIData.HighLightStringProperty, new Binding("FilterString") { Source = this });
                        ExcelTreeViewItemsNodes.Add(data);
                    }
                }

                //fileinfos = folder.GetFiles("*.anim", SearchOption.AllDirectories);
                //for (int i = 0; i < fileinfos.Length; i++)
                //{
                //    TempAnimas.Add(EngineNS.RName.EditorOnly_GetRNameFromAbsFile(fileinfos[i].FullName));
                //}

                //fileinfos = folder.GetFiles("*.vanimbs", SearchOption.AllDirectories);
                //for (int i = 0; i < fileinfos.Length; i++)
                //{
                //    TempAnimas.Add(EngineNS.RName.EditorOnly_GetRNameFromAbsFile(fileinfos[i].FullName));
                //}

                //fileinfos = folder.GetFiles("*.vanimbs1d", SearchOption.AllDirectories);
                //for (int i = 0; i < fileinfos.Length; i++)
                //{
                //    TempAnimas.Add(EngineNS.RName.EditorOnly_GetRNameFromAbsFile(fileinfos[i].FullName));
                //}

                //fileinfos = folder.GetFiles("*.vanimabs1d", SearchOption.AllDirectories);
                //for (int i = 0; i < fileinfos.Length; i++)
                //{
                //    TempAnimas.Add(EngineNS.RName.EditorOnly_GetRNameFromAbsFile(fileinfos[i].FullName));
                //}

                //fileinfos = folder.GetFiles("*.vanimabs", SearchOption.AllDirectories);
                //for (int i = 0; i < fileinfos.Length; i++)
                //{
                //    TempAnimas.Add(EngineNS.RName.EditorOnly_GetRNameFromAbsFile(fileinfos[i].FullName));
                //}
            }
            catch (Exception e)
            {
                //int xx = 0;
            }


            IsLoadScenesFinished = true;
        }

        private void RefreashScenes()
        {
            if (!IsNeedLoadScenes)
                return;

            TreeViewItemsNodes.Clear();

            DirectoryInfo folder = new DirectoryInfo(EngineNS.CEngine.Instance.FileManager.ProjectContent);

            foreach (EditorCommon.TreeListView.ITreeModel treemodel in TreeViewItemsDatas)
            {
                SceneData sd = treemodel as SceneData;
                if (sd != null)
                {

                    if (sd.SceneNameToLower.IndexOf(mFilterString) > -1)
                    {
                        BindingOperations.SetBinding(sd, SceneData.HighLightStringProperty, new Binding("FilterString") { Source = this });
                        TreeViewItemsNodes.Add(sd);
                    }
                }
            }


            IsNeedLoadScenes = !IsLoadScenesFinished;
        }

        private void RefreashUIs()
        {
            if (!IsNeedLoadScenes)
                return;

            UITreeViewItemsNodes.Clear();

            DirectoryInfo folder = new DirectoryInfo(EngineNS.CEngine.Instance.FileManager.ProjectContent);

            foreach (EditorCommon.TreeListView.ITreeModel treemodel in UITreeViewItemsDatas)
            {
                UIData sd = treemodel as UIData;
                if (sd != null)
                {

                    if (sd.UINameToLower.IndexOf(mUIFilterString) > -1)
                    {
                        BindingOperations.SetBinding(sd, UIData.HighLightStringProperty, new Binding("UIFilterString") { Source = this });
                        UITreeViewItemsNodes.Add(sd);
                    }
                }
            }


            IsNeedLoadScenes = !IsLoadScenesFinished;
        }

        private void RefreashGames()
        {
            if (!IsNeedLoadScenes)
                return;

            GameTreeViewItemsNodes.Clear();

            DirectoryInfo folder = new DirectoryInfo(EngineNS.CEngine.Instance.FileManager.ProjectContent);

            foreach (EditorCommon.TreeListView.ITreeModel treemodel in GameTreeViewItemsDatas)
            {
                GameData sd = treemodel as GameData;
                if (sd != null)
                {

                    if (sd.GameNameToLower.IndexOf(mFilterString) > -1)
                    {
                        //BindingOperations.SetBinding(sd, UIData.HighLightStringProperty, new Binding("UIFilterString") { Source = this });
                        GameTreeViewItemsNodes.Add(sd);
                    }
                }
            }


            IsNeedLoadScenes = !IsLoadScenesFinished;
        }

        private void RefreashExcels()
        {
            if (!IsNeedLoadScenes)
                return;

            ExcelTreeViewItemsNodes.Clear();

            DirectoryInfo folder = new DirectoryInfo(EngineNS.CEngine.Instance.FileManager.ProjectContent);

            foreach (EditorCommon.TreeListView.ITreeModel treemodel in ExcelTreeViewItemsDatas)
            {
                ExcelData sd = treemodel as ExcelData;
                if (sd != null)
                {

                    if (sd.ExcelNameToLower.IndexOf(mFilterString) > -1)
                    {
                        //BindingOperations.SetBinding(sd, UIData.HighLightStringProperty, new Binding("UIFilterString") { Source = this });
                        ExcelTreeViewItemsNodes.Add(sd);
                    }
                }
            }


            IsNeedLoadScenes = !IsLoadScenesFinished;
        }
        private bool IsArray(Type type)
        {
            Type[] types = type.GetInterfaces();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].Name.Equals("IList") || types[i].Name.Equals("IDictionary"))
                    return true;
            }

            return false;
        }
        //private bool IsIDictionary(Type type)
        //{
        //    Type[] types = type.GetInterfaces();
        //    for (int i = 0; i < types.Length; i++)
        //    {
        //        if (types[i].Name.Equals("IEnumerable"))
        //            return true;
        //    }

        //    return false;
        //}

        Dictionary<string, string> FileNames = new Dictionary<string, string>();

        private string GetRInfoFileName(string name)
        {
            string rinfoname = name;
            if (name.IndexOf("scene.map") > 0)
            {
                rinfoname = name.Replace("/scene.map", "");
            }

            string ext = EngineNS.CEngine.Instance.FileManager.GetFileExtension(name);

            if (ext == "cs")
            {
                int pos = name.LastIndexOf('/');
                rinfoname = name.Substring(0, pos);
            }

            rinfoname += ".rinfo";
            if (!System.IO.File.Exists(rinfoname))
            {
                Console.WriteLine("Get rinfo file faild!");
            }

            return rinfoname;
        }

        private async System.Threading.Tasks.Task AddFileName(string name)
        {
            if (FileNames.ContainsKey(name))
                return;

            var ext = EngineNS.CEngine.Instance.FileManager.GetFileExtension(name);
            if (ext.Equals("rinfo") || ext.Equals("snap"))
                return;

            //获取文件中rinfo文件资源
            string rinfo = GetRInfoFileName(EngineNS.CEngine.Instance.FileManager.ProjectContent + name);
            if (System.IO.File.Exists(rinfo))
            {
                var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(rinfo, null);
                if (resInfo != null)
                {
                    foreach (EngineNS.RName rinforname in resInfo.ReferenceRNameList)
                    {
                        string file = rinforname.Address.Replace(EngineNS.CEngine.Instance.FileManager.ProjectContent, "");
                        if (!string.IsNullOrEmpty(file) && FileNames.ContainsKey(file) == false)
                        {
                            if (rinforname.GetExtension().Equals("xls"))
                            {
                                file += ".dataset";

                            }
                            await AddFileName(file);
                        }
                    }
                }

            }
            FileNames.Add(name, name);
        }
        private async System.Threading.Tasks.Task AddFoFileNames(string rname)
        {
            string name = rname.Replace(EngineNS.CEngine.Instance.FileManager.Root, "");
            //Console.WriteLine("Name : " + name);
            //Macross cs file

            if (name.ToLower().IndexOf(".macross") != -1)
            {
                DirectoryInfo folder = new DirectoryInfo(rname);
                foreach (FileInfo file in folder.GetFiles("*.cs", SearchOption.AllDirectories))
                {
                    name = file.FullName.Replace("\\", "/").Replace(EngineNS.CEngine.Instance.FileManager.Root, "");
                    await AddFileName(name);
                }
            }
            else if (name.Equals("") == false)
            {
                await AddFileName(name);
            }
        }

        private async System.Threading.Tasks.Task AddObjectToFileNames(Object value)
        {
            if(value==null)
            {
                progress.AddInfo("空资源! ", -1, "Red");
            }
            else if (value.GetType().Equals(typeof(EngineNS.RName)))
            {

                EngineNS.RName rname = value as EngineNS.RName;
                await AddFoFileNames(rname.Address);
            }
            else if (value.GetType().Equals(typeof(string)))
            {
                string rname = value as string;
                await AddFoFileNames(rname);
            }
            else
            {
                progress.AddInfo(value.GetType().Name + " 不是资源! ", -1, "Red");
            }
        }
        private bool DontProcessObject(Object obj, Type type)
        {
            if (type.Equals(typeof(EngineNS.CConstantBuffer)))
                return true;
            else if (type.FullName == "System.RuntimeType")
                return true;
            return false;
        }
        private bool DontProcessProperty(System.Reflection.PropertyInfo prop, Type type)
        {
            if (type.Assembly.FullName.Contains("PresentationCore"))
                return true;
            else if (type.Assembly.FullName.Contains("PresentationFramework"))
                return true;
            else if (type.Assembly.FullName.Contains("WindowsBase"))
                return true;
            else if (type.Assembly.FullName.Contains("System.Windows.Forms"))
                return true;
            else if (type.Assembly.FullName.Contains("System.Drawing"))
                return true;
            else if (type.Assembly.FullName.Contains("mscorlib"))
                return true;
            return false;

            //if (type.Assembly.FullName.Contains("CoreClient.Windows.dll"))
            //    return false;
            //if (type.Assembly.FullName.Contains("Game.Windows"))
            //    return false;
            //return true;
        }
        private async System.Threading.Tasks.Task GetPackDatasFromObject(Object obj, Type type)
        {
            if(DontProcessObject(obj, type))
            {
                return;
            }
            //
            {
                if (type.Namespace.IndexOf("System.") == 0)
                {
                    return;
                }

                if (type.Equals(typeof(EngineNS.CConstantBuffer)))
                    return;

                if (type.Equals(typeof(string)) || type.IsPrimitive)
                    return;

                Object temp;
                if (TempCache.TryGetValue(obj, out temp))
                {
                    return;
                }

                TempCache.Add(obj, obj);
            }

            //处理字段集
            FieldInfo[] ActorFields = type.GetFields();

            for (int i = 0; i < ActorFields.Length; i++)
            {
                if (progress.IsCancel)
                {
                    CloseCmd();
                    return;
                }
                
                object value;
                try
                {
                    value = ActorFields[i].GetValue(obj);
                }
                catch (Exception e)

                {
                    value = null;
                }
                if (value == null)
                    continue;

                var valuetype = value.GetType();
                if(valuetype.IsValueType)
                    continue;

                //看是否使用宏图文件
                {
                    EngineNS.RName rname = value as EngineNS.RName;
                    if (rname != null)
                    {
                        if (rname.GetExtension().Equals("macross") || rname.GetExtension().Equals("gms") || rname.GetExtension().Equals("vms"))
                        {
                            MacrossInfos.Add(rname.Address);
                        }
                        else if (rname.GetExtension().Equals("instmtl"))
                        {
                            var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, rname);
                            if (mtl == null)
                            {
                                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "GamePack", $"Material Instance {rname} is missing");
                                continue;
                            }
                            await GetPackDatasFromObject(mtl, mtl.GetType());
                        }
                    }
                    
                }
                Editor_PackDataAttribute att = ActorFields[i].GetCustomAttribute<Editor_PackDataAttribute>(true);
                if (att != null)
                {
                    Type pp = value.GetType();
                    if (IsArray(pp))
                    {
                        Type[] args = pp.GetGenericArguments();
                        //Array List
                        if (args.Length == 1 || args.Length == 0)
                        {
                            var tt = (System.Collections.IEnumerable)value;
                            if (tt != null)
                            {
                                foreach (var v in tt)
                                {
                                    if (progress.IsCancel)
                                    {
                                        CloseCmd();
                                        return;
                                    }
                                    //看是否使用宏图文件
                                    EngineNS.RName rname = v as EngineNS.RName;
                                    if (rname != null)
                                    {
                                        if (rname.GetExtension().Equals("macross") || rname.GetExtension().Equals("gms") || rname.GetExtension().Equals("vms"))
                                        {
                                            MacrossInfos.Add(rname.Address);
                                        }
                                        else if (rname.GetExtension().Equals("instmtl"))
                                        {
                                            var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, rname);
                                            if (mtl == null)
                                            {
                                                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "GamePack", $"Material Instance {rname} is missing");
                                                continue;
                                            }
                                            await GetPackDatasFromObject(mtl, mtl.GetType());
                                        }
                                    }
                                    await AddObjectToFileNames(v);
                                }

                            }
                        }
                        //Dictionary
                        else if (args.Length == 2)
                        {
                        }
                    }
                    else
                    {
                        await AddObjectToFileNames(value);
                    }
                }

                else{
                    // ActorProperties[i].PropertyType.GenericTypeArguments
                    //var ss = datas[0];
                    Type pp = value.GetType();
                    if (IsArray(pp))
                    {
                        Type[] args = pp.GetGenericArguments();
                        //Array List
                        if (args.Length == 1 || args.Length == 0)
                        {
                            var tt = (System.Collections.IEnumerable)value;
                            if (tt != null)
                            {
                                foreach (var v in tt)
                                {
                                    if (progress.IsCancel)
                                    {
                                        CloseCmd();
                                        return;
                                    }
                                    if (v != null)
                                    {
                                        await GetPackDatasFromObject(v, v.GetType());
                                    }
                                   
                                }

                            }
                        }
                        //Dictionary
                        else if (args.Length == 2)
                        {
                            var tt = (System.Collections.IDictionary)value;
                            if (tt != null)
                            {
                                foreach (var v in tt.Values)
                                {
                                    if (progress.IsCancel)
                                    {
                                        CloseCmd();
                                        return;
                                    }
                                    if (v != null)
                                    {
                                        await GetPackDatasFromObject(v, v.GetType());
                                    }
                                }

                            }
                        }

                    }

                    //else if (att2.IsList)
                    //{

                    //    var datas = ActorProperties[i].GetValue(obj);
                    //    var tt = (System.Collections.IEnumerable)datas;
                    //    if (tt != null)
                    //    {
                    //        foreach (var v in tt)
                    //        {
                    //            await GetPackDatasFromObject(v, v.GetType());
                    //        }
                    //    }

                    //}
                    else
                    {
                        if (value != null)
                        {
                            await GetPackDatasFromObject(value, value.GetType());
                        }
                    }
                }
            }

            //处理属性集
            PropertyInfo[] ActorProperties = type.GetProperties();
            for (int i = 0; i < ActorProperties.Length; i++)
            {
                if (progress.IsCancel)
                {
                    CloseCmd();
                    return;
                }
                var prop = ActorProperties[i];
                if (prop.CanRead == false)
                    continue;
                var valuetype = prop.PropertyType;
                if (valuetype.IsValueType)
                    continue;
                if (DontProcessProperty(prop, prop.PropertyType))
                    continue;
                object value;
                var indexParams = prop.GetIndexParameters();
                if (indexParams.Length != 0)
                    continue;
                try
                {
                    value = prop.GetValue(obj);
                }
                catch (Exception e)
                {
                    value = null;
                }
                if (value == null)
                    continue;

                //看是否使用宏图文件
                EngineNS.RName rname = value as EngineNS.RName;
                if (rname != null)
                {
                    if (rname.GetExtension().Equals("macross") || rname.GetExtension().Equals("gms") || rname.GetExtension().Equals("vms"))
                    {
                        MacrossInfos.Add(rname.Address);
                    }
                    else if (rname.GetExtension().Equals("instmtl"))
                    {
                        var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, rname);
                        if(mtl==null)
                        {
                            EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "GamePack", $"Material Instance {rname} is missing");
                            continue;
                        }
                        await GetPackDatasFromObject(mtl, mtl.GetType());
                    }
                }

                Editor_PackDataAttribute att = ActorProperties[i].GetCustomAttribute<Editor_PackDataAttribute>(true);
                if (att != null)
                {
                    Type pp = value.GetType();
                    if (IsArray(pp))
                    {
                        Type[] args = pp.GetGenericArguments();
                        //Array List
                        if (args.Length == 1 || args.Length == 0)
                        {
                            var tt = (System.Collections.IEnumerable)value;
                            if (tt != null)
                            {
                                foreach (var v in tt)
                                {
                                    if (progress.IsCancel)
                                    {
                                        CloseCmd();
                                        return;
                                    }
                                    //看是否使用宏图文件
                                    EngineNS.RName testrname = v as EngineNS.RName;
                                    if (testrname != null)
                                    {
                                        if (testrname.GetExtension().Equals("macross") || testrname.GetExtension().Equals("gms") || testrname.GetExtension().Equals("vms"))
                                        {
                                            MacrossInfos.Add(testrname.Address);
                                        }
                                        else if (testrname.GetExtension().Equals("instmtl"))
                                        {
                                            var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, testrname);
                                            if (mtl == null)
                                            {
                                                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "GamePack", $"Material Instance {testrname.Name} is missing");
                                                continue;
                                            }
                                            await GetPackDatasFromObject(mtl, mtl.GetType());
                                        }
                                    }
                                    await AddObjectToFileNames(v);
                                }

                            }
                        }
                        //Dictionary
                        else if (args.Length == 2)
                        {
                        }
                    }
                    else
                    {
                        await AddObjectToFileNames(value);
                    }
                }
                else
                {
                    // ActorProperties[i].PropertyType.GenericTypeArguments
                    //var ss = datas[0];
                    Type pp = value.GetType();
                    if (IsArray(pp))
                    {
                        Type[] args = pp.GetGenericArguments();
                        //Array List
                        if (args.Length == 1 || args.Length == 0)
                        {
                            var tt = (System.Collections.IEnumerable)value;
                            if (tt != null)
                            {
                                foreach (var v in tt)
                                {
                                    if (progress.IsCancel)
                                    {
                                        CloseCmd();
                                        return;
                                    }
                                    if (v != null)
                                    {
                                        await GetPackDatasFromObject(v, v.GetType());
                                    }                                 
                                }

                            }
                        }
                        //Dictionary
                        else if (args.Length == 2)
                        {
                            var tt = (System.Collections.IDictionary)value;
                            if (tt != null)
                            {
                                foreach (var v in tt.Values)
                                {
                                    if (progress.IsCancel)
                                    {
                                        CloseCmd();
                                        return;
                                    }
                                    if (v != null)
                                    {
                                        await GetPackDatasFromObject(v, v.GetType());
                                    }
                                }

                            }
                        }
                        
                    }
                    
                    //else if (att2.IsList)
                    //{

                    //    var datas = ActorProperties[i].GetValue(obj);
                    //    var tt = (System.Collections.IEnumerable)datas;
                    //    if (tt != null)
                    //    {
                    //        foreach (var v in tt)
                    //        {
                    //            await GetPackDatasFromObject(v, v.GetType());
                    //        }
                    //    }
                       
                    //}
                    else
                    {
                        if (value != null)
                        {
                            await GetPackDatasFromObject(value, value.GetType());

                        }
                        
                    }
                }
            }

        }

        private async System.Threading.Tasks.Task GetComponentResources(EngineNS.GamePlay.Component.GComponent component)
        {
            if (progress.IsCancel)
            {
                CloseCmd();
                return;
            }

            Type type = component.GetType();
            await GetPackDatasFromObject(component, type);
            EngineNS.GamePlay.Component.GComponentsContainer cc = component as EngineNS.GamePlay.Component.GComponentsContainer;
            if (cc != null)
            {
                foreach (var subcom in cc.Components)
                {
                    await GetComponentResources(subcom);
                }
            }

        }

        private async System.Threading.Tasks.Task GetActorResources(EngineNS.GamePlay.Actor.GActor actor)
        {
            await GetPackDatasFromObject(actor, actor.GetType());
            foreach (var component in actor.Components)
            {
                await GetComponentResources(component);
            }


            List<EngineNS.GamePlay.Actor.GActor> children = actor.GetChildrenUnsafe();
            foreach (var ac in children)
            {
                await GetActorResources(ac);
            }
        }

        private async System.Threading.Tasks.Task GetSceneResources(EngineNS.GamePlay.SceneGraph.GSceneGraph scene)
        {
            await GetPackDatasFromObject(scene, scene.GetType());
            using (var i = scene.Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    if (progress.IsCancel)
                    {
                        CloseCmd();
                        return;
                    }
                    EngineNS.GamePlay.Actor.GActor actor = i.Current.Value;
                    await GetActorResources(actor);
                }
            }
        }

        public async System.Threading.Tasks.Task DealScene(EngineNS.GamePlay.GWorld world, string SceneAddress)
        {
            EngineNS.GamePlay.SceneGraph.GSceneGraph scene;
            var xnd = await EngineNS.IO.XndHolder.LoadXND(SceneAddress + "/scene.map");
            if (xnd != null)
            {
                //把场景加进去
                string rname = SceneAddress + "/scene.map";
                string name = rname.Replace(EngineNS.CEngine.Instance.FileManager.Root, "");
                await AddFileName(name);

                ////把寻路文件加进去
                string navname = SceneAddress + "/navmesh.dat";
                name = navname.Replace(EngineNS.CEngine.Instance.FileManager.Root, "");
                await AddFileName(name);

                var type = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                if (type == null)
                    scene = null;
                scene = await EngineNS.GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(world, type, new EngineNS.GamePlay.SceneGraph.GSceneGraphDesc());
                if (await scene.LoadXnd(EngineNS.CEngine.Instance.RenderContext, xnd.Node))
                {
                    await GetSceneResources(scene);
                }

            }

        }

        Dictionary<Object, Object> TempCache = new Dictionary<object, object>();
        private async System.Threading.Tasks.Task GetResources()
        {
            FileNames.Clear();
            TempCache.Clear();

            {
                progress.AddInfo("解析选中游戏", AndroidStartProgress);
                foreach (var i in GameTreeViewItemsNodes)
                {
                    if (progress.IsCancel)
                    {
                        CloseCmd();
                        return;
                    }
                    GameData sd = i as GameData;
                    if (sd.IsChecked == true)
                    {
                        MacrossInfos.Add(sd.GameAddress);
                    }

                }

            }

            EngineNS.GamePlay.GWorld world = new EngineNS.GamePlay.GWorld();
            progress.AddInfo("解析选中场景场景", AndroidStartProgress);
            foreach (var i in TreeViewItemsNodes)
            {
                if (progress.IsCancel)
                {
                    CloseCmd();
                    return;
                }
                SceneData sd = i as SceneData;
                if (sd.IsChecked == true)
                {
                    await DealScene(world, sd.SceneAddress);
                    MacrossInfos.Add(sd.ResourceName.Address);
                }
               
            }

            //处理UI资源
            foreach (var i in UITreeViewItemsNodes)
            {
                if (progress.IsCancel)
                {
                    CloseCmd();
                    return;
                }
                UIData sd = i as UIData;
                if (sd.IsChecked == true)
                {
                    var element = await EngineNS.CEngine.Instance.UIManager.GetUIAsync(sd.ResourceName);
                    await GetPackDatasFromObject(element, element.GetType());
                    //UI宏图需要分析
                    MacrossInfos.Add(sd.ResourceName.Address);
                }
                
            }

            //处理Excel资源
            foreach (var i in ExcelTreeViewItemsNodes)
            {
                if (progress.IsCancel)
                {
                    CloseCmd();
                    return;
                }
                ExcelData sd = i as ExcelData;
                if (sd.IsChecked == true)
                {
                    var name = sd.ExcelAddress + ".dataset";

                    await AddFileName(name);
                 }

            }

            //处理宏图资源
            try
            {
                //foreach (var i in MacrossInfos)
                for(int i = 0; i < MacrossInfos.Count; i++)
                {
                    var macroosinfoname = MacrossInfos[i];
                    if (macroosinfoname.IndexOf(".rinfo") == -1)
                    {
                        macroosinfoname = MacrossInfos[i] + ".rinfo";
                    }
                    var rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(macroosinfoname, null);

                    if (rInfo != null)
                    {
                        var SkeletonAssetPT = rInfo.GetType().GetProperty("SkeletonAsset");
                        if (SkeletonAssetPT != null)
                        {
                            await AddObjectToFileNames(SkeletonAssetPT.GetValue(rInfo));
                        }

                        try
                        {
                            foreach (var res in rInfo.ReferenceRNameList)
                            {

                                string name = res.Address.Replace(EngineNS.CEngine.Instance.FileManager.Root, "");
                                if (!string.IsNullOrEmpty(name) && FileNames.ContainsKey(name) == false)
                                {
                                    if (res.GetExtension().Equals("xls"))
                                    {
                                        name += ".dataset";

                                    }
                                    await AddFileName(name);
                                    if (name.IndexOf("prefab") != -1)
                                    {
                                        var prefab = await EngineNS.GamePlay.Actor.GActor.NewPrefabActorAsync(res);
                                        if (prefab != null)
                                        {
                                            await GetActorResources(prefab);
                                        }
                                    }
                                    else if (name.IndexOf(".map") != -1)
                                    {
                                        await DealScene(world, res.Address);
                                    }
                                    else if (name.IndexOf("vanimabs1d") != -1)
                                    {
                                        var obj = await EngineNS.Bricks.Animation.AnimNode.AdditiveBlendSpace1D.Create(res);
                                        if (obj != null)
                                        {
                                            await GetPackDatasFromObject(obj, obj.GetType());
                                        }
                                    }
                                    else if (name.IndexOf("vanimbs1d") != -1)
                                    {
                                        var obj = await EngineNS.Bricks.Animation.AnimNode.BlendSpace1D.Create(res);
                                        if (obj != null)
                                        {
                                            await GetPackDatasFromObject(obj, obj.GetType());
                                        }
                                    }
                                    else if (name.IndexOf("vanimabs") != -1)
                                    {
                                        var obj = await EngineNS.Bricks.Animation.AnimNode.AdditiveBlendSpace2D.Create(res);
                                        if (obj != null)
                                        {
                                            await GetPackDatasFromObject(obj, obj.GetType());
                                        }
                                    }
                                    else if (name.IndexOf("vanimbs") != -1)
                                    {
                                        var obj = await EngineNS.Bricks.Animation.AnimNode.BlendSpace2D.Create(res);
                                        if (obj != null)
                                        {
                                            await GetPackDatasFromObject(obj, obj.GetType());
                                        }
                                    }
                                    else if (name.IndexOf(".anim") != -1)
                                    {
                                        var obj = await EngineNS.Bricks.Animation.AnimNode.AnimationClip.Create(res);
                                        if (obj != null)
                                        {
                                            await GetPackDatasFromObject(obj, obj.GetType());
                                        }

                                        MacrossInfos.Add(res.Address + ".rinfo");

                                        if (EngineNS.CEngine.Instance.FileManager.FileExists(res.Address + ".notify"))
                                        {
                                            await AddObjectToFileNames(res.Address + ".notify");
                                        }
                                    }
                                    else if (name.IndexOf(".gms") != -1)
                                    {
                                        var actor = new EngineNS.GamePlay.Actor.GActor();
                                        actor.ActorId = Guid.NewGuid();
                                        var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
                                        var meshCompInit = new EngineNS.GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
                                        meshCompInit.MeshName = EngineNS.RName.GetRName(name);
                                        await meshComp.SetInitializer(EngineNS.CEngine.Instance.RenderContext, actor, actor, meshCompInit);

                                        if (meshComp != null)
                                        {
                                            await GetPackDatasFromObject(meshComp, meshComp.GetType());
                                        }

                                        MacrossInfos.Add(res.Address + ".rinfo");
                                    }

                                    await AddObjectToFileNames(res);

                                }

                            }
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.ToString());
                        }
                    }
                    
                   
                }

                //Todo. 临时处理 加载全部动画
                foreach (var res in TempAnimas)
                {
                    string name = res.Address.Replace(EngineNS.CEngine.Instance.FileManager.Root, "");
                    await AddFileName(name);
                    if (name.IndexOf("vanimabs1d") != -1)
                    {
                        var obj = await EngineNS.Bricks.Animation.AnimNode.AdditiveBlendSpace1D.Create(res);
                        if (obj != null)
                        {
                            await GetPackDatasFromObject(obj, obj.GetType());
                        }
                    }
                    else if (name.IndexOf("vanimbs1d") != -1)
                    {
                        var obj = await EngineNS.Bricks.Animation.AnimNode.BlendSpace1D.Create(res);
                        if (obj != null)
                        {
                            await GetPackDatasFromObject(obj, obj.GetType());
                        }
                    }
                    else if (name.IndexOf("vanimabs") != -1)
                    {
                        var obj = await EngineNS.Bricks.Animation.AnimNode.AdditiveBlendSpace2D.Create(res);
                        if (obj != null)
                        {
                            await GetPackDatasFromObject(obj, obj.GetType());
                        }
                    }
                    else if (name.IndexOf("vanimbs") != -1)
                    {
                        var obj = await EngineNS.Bricks.Animation.AnimNode.BlendSpace2D.Create(res);
                        if (obj != null)
                        {
                            await GetPackDatasFromObject(obj, obj.GetType());
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
           

            progress.AddInfo("完成解析选中场景场景", AndroidStartProgress + (AndroidEndProgress - AndroidStartProgress) * 0.3f);
            AndroidStartProgress = AndroidStartProgress + (AndroidEndProgress - AndroidStartProgress) * 0.3f;

            //var files = Directory(EngineNS.CEngine.Instance.FileManager.content + "MetaClasses/", "*.txt");

        }
        public async System.Threading.Tasks.Task<string> BuildPC()
        {
            string projectpath = EditorCommon.GameProjectConfig.Instance.GameProjFileName;
            EngineNS.IO.XmlHolder xml = EngineNS.IO.XmlHolder.LoadXML(projectpath);
            EngineNS.IO.XmlNode xnode = xml.RootNode;
            var node = xnode.FindNode("Import");
            if (node != null)
                xnode.RemoveNode(node);

            node = xnode.FindNode("Target");
            if (node != null)
                xnode.RemoveNode(node);

            projectpath += ".temp";
            EngineNS.IO.XmlHolder.SaveXML(projectpath, xml);
            return projectpath;
        }

        //private XmlNode FindAssetNode(XmlNode node, string name)
        //{
        //    List<EngineNS.IO.XmlNode> nodes = node.GetNodes();
        //    foreach (var i in nodes)
        //    {
        //        //这里不能是空
        //        List<XmlAttrib> atts = i.GetAttribs();
        //        List<XmlNode> ssss = i.GetNodes();
        //        XmlAttrib att = i.FindAttrib("Name");

        //        if (att != null && att.Value.Equals(name))
        //        {
        //            return i;
        //        }
        //    }
        //    return null;
        //}

        private EngineNS.IO.XmlNode FindAndAddAssetNode(EngineNS.IO.XmlNode node, string name)
        {
            List<EngineNS.IO.XmlNode> nodes = node.GetNodes();
            foreach (var i in nodes)
            {
                //这里不能是空
                XmlAttrib att = i.FindAttrib("Name");
                if (att != null && att.Value.Equals(name))
                {
                    return i;
                }
            }

            EngineNS.IO.XmlNode sunnode = node.AddNode("Folder", "", AssetInfos);
            sunnode.AddAttrib("Name", name);

            return sunnode;
        }
        

        private void AddAssetInfos(string filename)
        {
            string projectpath = EngineNS.CEngine.Instance.FileManager.Root + "Execute/Games/Batman/Batman.Droid/";
            string allpath = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(filename);
            allpath = allpath.Replace(projectpath.ToLower(), "");
            allpath = allpath.Replace("\\", "/");
            string[] folders = allpath.Split('/');
            EngineNS.IO.XmlNode node = AssetInfos.RootNode;
            EngineNS.IO.XmlNode subnode;
            for (int i = 0; i < folders.Length; i++)
            {
                if (folders[i].Equals(""))
                    break;

                subnode = FindAndAddAssetNode(node, folders[i]);
                node = subnode;
            }

            EngineNS.IO.XmlNode filenode = node.FindNode("Files");
            if (filenode == null)
            {
                filenode = node.AddNode("Files", "", AssetInfos);
            }

            string name = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(filename);
            var value = filenode.AddNode("File", "", AssetInfos);
            value.AddAttrib("Name", name);

            //生成MD5码
            try
            {
                var MD5 = new MD5CryptoServiceProvider();
                FileStream fs = new FileStream(EngineNS.CEngine.Instance.FileManager.Root + allpath + name, FileMode.Open, FileAccess.Read);
                byte[] code = MD5.ComputeHash(fs);
                string str = System.Text.Encoding.ASCII.GetString(code);
                value.AddAttrib("MD5", System.Text.Encoding.ASCII.GetString(code));
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            var ofd = new System.Windows.Forms.FolderBrowserDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutPathText.Text = ofd.SelectedPath.Replace("\\", "/") + "/";
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var test = LoadData();
            progress.CancleEvent -= ProgressCancleEvent;   
            progress.CancleEvent += ProgressCancleEvent;
            UIGameRes.HostControl = this;

            //UnbundProgress -= UIGameRes.UnbundProgress1;
            //UnbundProgress += UIGameRes.UnbundProgress1;

            UnbundProgress -= UIGameRes.UnbundProgress;
            UnbundProgress += UIGameRes.UnbundProgress;
            progress.CancleEvent -= UnbundProgress1;
            progress.CancleEvent += UnbundProgress1;

            FinishCreateNewAndroidInfos -= UIGameRes.FinishCreateNewAndroidInfos;
            FinishCreateNewAndroidInfos += UIGameRes.FinishCreateNewAndroidInfos;
        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {

            var test = BuildProject();
            UnbundProgress1();
            progress_grid.Children.Add(progress);
            config_grid.Visibility = Visibility.Collapsed; 
            progress_grid.Visibility = Visibility.Visible;
        }
        private void SaveCfgBtn_Click(object sender, RoutedEventArgs e)
        {

            SaveConfigToFile();
        }

        private void UICompare_Click(object sender, RoutedEventArgs e)
        {
            //Compare compare = new Compare();
            //string projectpath = EngineNS.CEngine.Instance.FileManager.Root + "Execute/Games/Batman/Batman.Droid/";

            //string[] files = compare.CompareMD5(projectpath + "content/assetinfos.xml", projectpath + "content/oldassetinfos.xml");
            //foreach (var file in files)
            //{
            //    Console.WriteLine("aaaaaaaaaaa", file);
            //}
            //SaveConfigToFile();

        }

        double AndroidStartProgress = 0;
        double AndroidEndProgress = 0;
        private async System.Threading.Tasks.Task BuildProject()
        {
            progress.IsCancel = false;
            SaveConfig();
            EngineNS.CEngine.Instance.EventPoster.RunOn(async() =>
            {
                if (progress.IsCancel)
                {
                    CloseCmd();
                    return false;
                }

                StartCmd();
                progress.AddInfo("项目版本：" + PConfig.Version, 0, "Red");
               
                if (PConfig.IsUpdateSVN == true)
                {
                    progress.AddInfo("开始更新svn！", 1);
                    WriteCmd("svn up " + EngineNS.CEngine.Instance.FileManager.Root);
                    progress.AddInfo("svn更新完毕!", 10);
                }

                string buildinfo = "Build";
                if (PConfig.IsBuild == true)
                    buildinfo = "Build";
                else if (PConfig.IsReBuild == true)
                    buildinfo = "Rebuild";

                string releaseinfo = "Debug";
                if (PConfig.IsRelease == true)
                    releaseinfo = "Release";
                else if (PConfig.IsDebug == true)
                    releaseinfo = "Debug";

                string msbuild = EditorCommon.GameProjectConfig.Instance.MSBuildAbsFileName;

                WriteCmd("setlocal");

                if (PConfig.IsPC == true)
                {
                    if (progress.IsCancel)
                    {
                        CloseCmd();
                        return false;
                    }

                    progress.AddInfo("开始打PC包!", 20);
                    string proname = await BuildPC();
                    progress.AddInfo(WriteCmd(proname[1] + ":"), 20); 
                    string cmd = "\"" + msbuild + "\" " + proname + " / t:" + buildinfo + " /p:Configuration=" + releaseinfo + ";OutDir=" + PConfig.OutDir;
                    progress.AddInfo(WriteCmd(cmd), 30);
                    //WriteCmd("del " + proname);
                    progress.AddInfo("打包PC资源！", 30);
                }

                if (PConfig.IsAndroid == true)
                {
                    //描述android下asset文件信息
                    if (progress.IsCancel)
                    {
                        CloseCmd();
                        return false;
                    }
                    AndroidStartProgress = 30;
                    AndroidEndProgress = 60;
                    //WriteCmd("nuget restore");
                    progress.AddInfo("开始打Android包！", AndroidStartProgress);
                    string proname = await BuildAndroid();
                    //WriteCmd(proname[1] + ":");
                    progress.AddInfo(WriteCmd(proname[1] + ":"), AndroidStartProgress);
                    string cmd = "\"" + msbuild + "\" /t:SignAndroidPackage;" + buildinfo + " " + proname + " /p:Configuration=" + releaseinfo + ";OutputPath=" + PConfig.OutDir;

                    progress.AddInfo(WriteCmd(cmd), AndroidEndProgress);
                    //WriteCmd("del " + proname);

                    progress.AddInfo("打包Android资源！", AndroidEndProgress);

                    //CopyAndroidFile(proname, PConfig.OutDir + "/Adroid.porject", true);
                    //System.IO.File.Delete(proname);
                }

                string outstr = CloseCmd();
                string[] strs = outstr.Split('\n');
                for (int i = 0; i < strs.Length;  i++)
                {
                    progress.AddInfo(strs[i], 99, "Red");
                }
                //progress.AddInfo(CloseCmd(), 99, "Red");
                progress.SaveInfoToTxt(PConfig.OutDir);
                progress.AddInfo("打包完成！", 100);
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            
        }

        /// <summary>
        /// 执行CMD语句
        /// </summary>
        /// <param name="cmd">要执行的CMD命令</param>
        public void StartCmd()
        {
           
            //System.Diagnostics.Process.Start(startInfo);
            proc.Start();
        }

        public string WriteCmd(string cmd)
        {
            Console.WriteLine("the cmd ：" + cmd);
            proc.StandardInput.WriteLine(cmd);
            string outstr = proc.StandardOutput.ReadLine();
            return outstr;
        }

        public string CloseCmd()
        {
            string outStr = "";
            if (proc.StandardInput != null)
            {
                proc.StandardInput.WriteLine("exit");
                outStr = proc.StandardOutput.ReadToEnd();
            }
         
            proc.Close();
            Console.WriteLine(outStr);
            return outStr;
        }

        public void ProgressCancleEvent()
        {
            config_grid.Visibility = Visibility.Visible;
            progress_grid.Visibility = Visibility.Collapsed;
        }

        #region AndroidLogic
        public void WriteEtcAndRemoveRaw(string src, string des)
        {
            using (var xnd = EngineNS.IO.XndHolder.SyncLoadXND(src))
            {
                var rawAttr = xnd.Node.FindAttrib("PNG");
                if (rawAttr == null)
                    return;

                var newxnd = EngineNS.IO.XndHolder.NewXNDHolder();
                var etcMips = xnd.Node.FindNode("EtcMips");

                var descattr = xnd.Node.FindAttrib("Desc");
                if (etcMips == null)
                {
                    if (descattr == null)
                        return;

                    EditorCommon.TextureDesc desc = new EditorCommon.TextureDesc();
                    descattr.BeginRead();
                    switch (descattr.Version)
                    {
                        case 1:
                            {
                                string ori;
                                descattr.Read(out ori);
                                descattr.Read(out desc.PicDesc.sRGB);
                            }
                            break;
                        case 2:
                            {
                                descattr.Read(out desc.PicDesc.sRGB);
                            }
                            break;
                        case 3:
                            {
                                unsafe
                                {
                                    fixed (EngineNS.CTxPicDesc* descPin = &(desc.PicDesc))
                                    {
                                        descattr.Read((IntPtr)descPin, sizeof(EngineNS.CTxPicDesc));
                                    }
                                }
                            }
                            break;
                    }
                    descattr.EndRead();

                    byte[] pngData;
                    rawAttr.BeginRead();
                    rawAttr.Read(out pngData, (int)rawAttr.Length);
                    rawAttr.EndRead();

                    using (var etcBlob = EngineNS.Support.CBlobProxy2.CreateBlobProxy())
                    {
                        unsafe
                        {
                            fixed (byte* dataPtr = &pngData[0])
                            {
                                var texCompressor = new EngineNS.Bricks.TexCompressor.CTexCompressor();
                                var etcFormat = EngineNS.ETCFormat.RGBA8;
                                if (desc != null)
                                    etcFormat = desc.ETCFormat;
                                var mipMapLevel = 0;
                                if (desc != null)
                                    mipMapLevel = desc.MipMapLevel;
                                texCompressor.EncodePng2ETC((IntPtr)dataPtr, (uint)pngData.Length, etcFormat, mipMapLevel, etcBlob);
                                etcBlob.BeginRead();
                            }
                        }
                        if (etcBlob.DataLength >= 0)
                        {
                            etcMips = xnd.Node.AddNode("EtcMips",0,0);
                            int fmt = 0;
                            int MipLevel = 0;
                            etcBlob.Read(out fmt);
                            etcBlob.Read(out MipLevel);
                            var layer = new EngineNS.Bricks.TexCompressor.ETCLayer();
                            for (int i = 0; i < MipLevel; i++)
                            {
                                etcBlob.Read(out layer);
                                byte[] etcMipData;
                                etcBlob.Read(out etcMipData, (int)layer.Size);

                                var mipAttr = etcMips.AddAttrib($"Mip_{i}");
                                mipAttr.BeginWrite();
                                mipAttr.Write(layer);
                                mipAttr.Write(etcMipData);
                                mipAttr.EndWrite();
                            }
                        }
                    }
                }
                else
                {
                    var newEtcMips = EngineNS.IO.XndHelper.Copy2WritableNode(etcMips);
                    newxnd.Node.AddNode(newEtcMips);
                }

                var newdesc = newxnd.Node.AddAttrib("Desc");
                if (EngineNS.IO.XndHelper.CopyAttrib(newdesc, descattr) == false)
                {
                    progress.AddInfo("Copy etc desc faild! " + src, -1, "Red");
                }

                EngineNS.IO.XndHolder.SaveXND(des, newxnd);
            }
        }

        public async System.Threading.Tasks.Task<bool> GetAllAndroidRes(string projectpath, EngineNS.IO.XmlHolder xml = null, EngineNS.IO.XmlNode xnode = null)
        {
            AssetInfos = EngineNS.IO.XmlHolder.NewXMLHolder("AssetsPackage", "");

            //获取用到的所有资源
            await GetResources();
           
            await CopyDir(EngineNS.CEngine.Instance.FileManager.ProjectContent + "MetaClasses/", projectpath + "content/metaclasses/");
            await CopyDir(EngineNS.CEngine.Instance.FileManager.ProjectContent + "Macross/", projectpath + "content/macross/");
            await CopyDir(EngineNS.CEngine.Instance.FileManager.DDCDirectory + "shader/", projectpath + "deriveddatacache/shader/");
            await CopyDir(EngineNS.CEngine.Instance.FileManager.DDCDirectory + "shaderinfo/", projectpath + "deriveddatacache/shaderinfo/");
            await CopyDir(EngineNS.CEngine.Instance.FileManager.ProjectContent + "TitanDemo/", projectpath + "content/titandemo/");
            await CopyDir(EngineNS.CEngine.Instance.FileManager.ProjectContent + "Font/", projectpath + "content/font/");
            //shader需要摘取
            await CopyDir(EngineNS.CEngine.Instance.FileManager.EngineContent + "Shaders/", projectpath + "enginecontent/shaders/");
            await CopyDir(EngineNS.CEngine.Instance.FileManager.EngineContent + "ui/", projectpath + "enginecontent/ui/");

            bool needproj = xml != null && xnode != null;
            //CopyAndroidFile(EngineNS.CEngine.Instance.FileManager.content + "typeredirection.xml", projectpath + "content/typeredirection.xml", needproj);
            //FileNames.Add("content/typeredirection.xml", "content/typeredirection.xml");
            //CopyAndroidFile(EngineNS.CEngine.Instance.FileManager.content + "cenginedesc.cfg", projectpath + "content/cenginedesc.cfg", needproj);
            //FileNames.Add("content/cenginedesc.cfg", "content/cenginedesc.cfg");
            //CopyAndroidFile(EngineNS.CEngine.Instance.FileManager.content + "rpcmapping.xml", projectpath + "content/rpcmapping.xml", needproj);
            //FileNames.Add("content/rpcmapping.xml", "content/rpcmapping.xml");
         

            CopyAndroidFile(EngineNS.CEngine.Instance.FileManager.Root + "binaries/macrosscollector.android.dll",
                projectpath + "binaries/macrosscollector.android.dll", needproj);
            await AddFileName("binaries/macrosscollector.android.dll");

            CopyAndroidFile(EngineNS.CEngine.Instance.FileManager.Root + "binaries/macrossscript.android.dll",
                projectpath + "binaries/macrossscript.android.dll", needproj);
            await AddFileName("binaries/macrossscript.android.dll");

            //默认资源
            EngineNS.CEngineDesc desc = new EngineNS.CEngineDesc();
            await GetPackDatasFromObject(desc, desc.GetType());

            EngineNS.GamePlay.GGameInstanceDesc idesc = new EngineNS.GamePlay.GGameInstanceDesc();
            await GetPackDatasFromObject(idesc, idesc.GetType());

            progress.AddInfo("开始加载资源", AndroidStartProgress);

            //await CopyDir(EngineNS.CEngine.Instance.FileManager.content + "MetaClasses/", projectpath + "content/metaclasses");

            //加载引擎文件

            int index = 0;
            foreach (var i in FileNames.Values)
            {
                if (progress.IsCancel)
                {
                    CloseCmd();
                    return false;
                }

                string name = EngineNS.CEngine.Instance.FileManager.Root + i;
                string contentname = EngineNS.CEngine.Instance.FileManager.ProjectContent + i;
                //TODO..
                name = name.Replace(';', ' ');
                contentname = contentname.Replace(';', ' ');
                //判断资源文件是否存在
                if (System.IO.File.Exists(name))
                {

                    string rname = projectpath + i.ToLower();
                    rname = rname.Replace(';', ' ');
                    CopyAndroidFile(name, rname, needproj);

                    if (needproj)
                    {
                        AddXMLNode(xml, xnode, i.Replace(";", "").ToLower());
                    } 
                    progress.AddInfo("加载资源 " + i, AndroidStartProgress + (AndroidEndProgress - AndroidStartProgress) * (index / FileNames.Values.Count));
                }
                else if (System.IO.File.Exists(contentname))
                {
                    string rname = projectpath + "content/" + i.ToLower();
                    rname = rname.Replace(';', ' ');
                    CopyAndroidFile(contentname, rname, needproj);
                    if (needproj)
                    {
                        AddXMLNode(xml, xnode, ("content/" + i.Replace(";", "")).ToLower());
                    }
                    progress.AddInfo("加载资源 " + i, AndroidStartProgress + (AndroidEndProgress - AndroidStartProgress) * (index / FileNames.Values.Count));
                }
                else
                {
                    progress.AddInfo("缺少资源 " + name + "！", AndroidStartProgress + (AndroidEndProgress - AndroidStartProgress) * (index / FileNames.Values.Count), "Red");
                }

                index++;
            }

            EngineNS.IO.XmlHolder.SaveXML(projectpath + "content/assetinfos.xml", AssetInfos);
            return true;
        }

        public async System.Threading.Tasks.Task<string> BuildAndroid()
        {
            string projectpath = EngineNS.CEngine.Instance.FileManager.Root + "Execute/Games/Batman/Batman.Droid/";
            EngineNS.IO.XmlHolder xml = EngineNS.IO.XmlHolder.LoadXML(projectpath + "Batman.Droid.csproj");
            EngineNS.IO.XmlNode xnode = xml.RootNode;

            bool result = await GetAllAndroidRes(projectpath, xml, xnode);
            if (result == false)
                return "";

            //EngineNS.IO.XmlHolder.SaveXML(projectpath + "content/assetinfos.xml", AssetInfos);
            AddXMLNode(xml, xnode, "content/assetinfos.xml");
            //Use key store..
            if (Android_Config.LoadConfig.KeyStoreAddress.Equals("") == false)
            {
                progress.AddInfo("加载资源 AndroidKeyStore", AndroidEndProgress);
                XmlNode PropertyGroup = xnode.AddNode("PropertyGroup", "", xml);
                PropertyGroup.AddNode("AndroidKeyStore", "True", xml);
                PropertyGroup.AddNode("AndroidSigningKeyStore", Android_Config.LoadConfig.KeyStoreAddress, xml);
                PropertyGroup.AddNode("AndroidSigningStorePass", Android_Config.LoadConfig.KeyStorePassWord, xml);
                PropertyGroup.AddNode("AndroidSigningKeyAlias", Android_Config.LoadConfig.Alias, xml);
                PropertyGroup.AddNode("AndroidSigningKeyPass", Android_Config.LoadConfig.AliasPassWord, xml);
            }

            projectpath += "temp.csproj";
            EngineNS.IO.XmlHolder.SaveXML(projectpath, xml);
            return projectpath;
        }

        public delegate void FinishCreateNewAndroidInfosDelegate(string[] files);
        public FinishCreateNewAndroidInfosDelegate FinishCreateNewAndroidInfos;
        //给检查资源使用 需要生成新的MD5吗列表
        public async System.Threading.Tasks.Task CreateNewAndroidInfos()
        {
            progress.IsCancel = false;
            progress.AddInfo("开始检查资源！", 0);
            EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            {
                string projectpath = EngineNS.CEngine.Instance.FileManager.Root + "Execute/Games/Batman/Batman.Droid/";
                progress.AddInfo("分析资源检查资源！", 10);
                AndroidStartProgress = 30;
                AndroidEndProgress = 80;
                string contentfolder = projectpath + "content";
                if (System.IO.Directory.Exists(contentfolder) == false)
                {
                    System.IO.Directory.CreateDirectory(contentfolder);
                }

                await GetAllAndroidRes(projectpath);
                progress.AddInfo("开始检查安卓资源！", 85);

                //比较一下android和PC上的asset文件列表
                AccessGameRes.CopyAndoridAsset();

                string[] files;
                AccessGameRes.GetNeedFilesInfosByConpare(out files);
                if (files != null)
                {
                    FinishCreateNewAndroidInfos?.Invoke(files);
                    string message = "";
                    AccessGameRes.UpdateNeedFiles(files, out message);
                    if(message.Equals("") == false)
                    {
                        EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
                        {
                            EditorCommon.MessageBox.Show("文件拷贝失败！\n 请检查手机是否开启MTP 或者 ADB服务端口是否被占用！\n" + message);
                            return true;
                        }, EngineNS.Thread.Async.EAsyncTarget.Editor);
                    } 
                }
                progress.AddInfo("完成！", 100);
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncIO);
        }

        //拷贝整个文件夹中文件
        private void CopyDirFiles(string src, string des)
        {
            if (Directory.Exists(src) == false)
                return;

            if (Directory.Exists(des))
            {
                Directory.Delete(des, true);
            }

            Directory.CreateDirectory(des);

            string[] files = Directory.GetFiles(src);
            for (int i = 0; i < files.Length; i++)
            {
                //Console.WriteLine(files[i]);
                //string name = files[i].Replace(EngineNS.CEngine.Instance.FileManager.Root, "");
                //CopyAndroidFile(files[i], des + files[i].Replace(src, ""));
                System.IO.File.Copy(files[i], des + files[i].Replace(src, ""));

            }

        }

        private void CopyAndroidFile(string src, string des, bool needproj)
        {

            //test
            des = des.ToLower();

            if (System.IO.File.Exists(src) == false)
            {
                progress.AddInfo("缺少文件：" + src, -1, "Red");
                return;
            }
            //if (des.IndexOf("enginecontent") != -1)
            //{
            //    des = des.Replace("enginecontent", "EngineContent");
            //}
            //else
            //{
            //    des = des.Replace("content", "content");
            //}
            
            AddAssetInfos(des);
            if (needproj)
            {
                string createfolder = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(des);

                if (System.IO.Directory.Exists(createfolder) == false)
                {
                    createfolder = createfolder.ToLower();

                    //if (createfolder.IndexOf("EngineContent") != -1)
                    //{
                    //    createfolder = createfolder.Replace("EngineContent", "enginecontent"); 
                    //}
                    //else
                    //{
                    //    createfolder = createfolder.Replace("content", "content"); 
                    //}
                    //createfolder = createfolder.Replace("content", "content");
                    System.IO.Directory.CreateDirectory(createfolder);
                }

                //Temp目录如果已经有这个资源了 删除
                if (System.IO.File.Exists(des))
                {
                    System.IO.File.Delete(des);
                }

                //判断是否是图片 是的话 处理下
               
                System.IO.File.Copy(src, des);
                if (PConfig.IsCopyRInfo)
                {
                    try
                    {
                        //处理场景rinfo
                        if (src.IndexOf("scene.map") > 0)
                        {
                            src = src.Replace("/scene.map", "");
                            des = des.Replace("/scene.map", "");
                        }

                        string ext = EngineNS.CEngine.Instance.FileManager.GetFileExtension(des);

                        //处理场景宏图
                        if (ext == "cs")
                        {
                            int pos = src.LastIndexOf('/');
                            src = src.Substring(0, pos);

                            pos = des.LastIndexOf('/');
                            des = des.Substring(0, pos);
                            CopyDirFiles(src, des);
                        }

                        if (System.IO.File.Exists(src + ".rinfo"))
                        {
                            //处理材质
                            {
                                if (System.IO.File.Exists(src + ".code"))
                                {
                                    if (System.IO.File.Exists(des + ".code"))
                                    {
                                        System.IO.File.Delete(des + ".code");
                                    }

                                    System.IO.File.Copy(src + ".code", des + ".code");
                                }

                                if (System.IO.File.Exists(src + ".link"))
                                {
                                    if (System.IO.File.Exists(des + ".link"))
                                    {
                                        System.IO.File.Delete(des + ".link");
                                    }

                                    System.IO.File.Copy(src + ".link", des + ".link");
                                }

                                if (System.IO.File.Exists(src + ".var"))
                                {
                                    if (System.IO.File.Exists(des + ".var"))
                                    {
                                        System.IO.File.Delete(des + ".var");
                                    }

                                    System.IO.File.Copy(src + ".var", des + ".var");
                                }
                            }

                            if (System.IO.File.Exists(des + ".rinfo"))
                            {
                                System.IO.File.Delete(des + ".rinfo");
                            }
                            System.IO.File.Copy(src + ".rinfo", des + ".rinfo");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Copy rinfo file faild!");
                    }
                   
                }
            }
        }

        private async System.Threading.Tasks.Task CopyDir(string src, string des)
        {
            if (Directory.Exists(src) == false)
                return;

            string[] files = Directory.GetFiles(src);
            for (int i = 0; i < files.Length; i++)
            {
                //Console.WriteLine(files[i]);
                string name = files[i].Replace(EngineNS.CEngine.Instance.FileManager.ProjectContent, "");
                await AddFileName(name);
                //CopyAndroidFile(files[i], des + files[i].Replace(src, ""));

            }


            string[] folders = Directory.GetDirectories(src);
            for (int i = 0; i < folders.Length; i++)
            {
                //Console.WriteLine(i);
                await CopyDir(folders[i] + "/", des + "/" + folders[i].Replace(src, "") + "/");
            }

        }

        public void AddXMLNode(EngineNS.IO.XmlHolder xml, EngineNS.IO.XmlNode rootnode, string value)
        {
            EngineNS.IO.XmlNode ItemGroupNode = rootnode.AddNode("ItemGroup", "", xml);
            EngineNS.IO.XmlNode AndroidAssetNode = ItemGroupNode.AddNode("AndroidAsset", "", xml);
            //AndroidAssetNode.AddAttrib("Include", "Assets\\" + value);

            //if (value.IndexOf("enginecontent") != -1)
            //{
            //    value = value.Replace("enginecontent", "EngineContent");
            //}
            //else
            //{
            //    value = value.Replace("content", "content");
            //}

            //test
            value = value.Replace("/", "\\");
            AndroidAssetNode.AddAttrib("Include", "Assets\\" + value);
        }
        #endregion

        public delegate void UnbundProgressDetegate();
        public event UnbundProgressDetegate UnbundProgress;
        public void UnbundProgress1()
        {
            UnbundProgress?.Invoke();
            progress_grid.Children.Clear();
        }
    }
}

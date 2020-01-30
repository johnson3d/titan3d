using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EditorCommon.PluginAssist
{
    /// <summary>
    /// 当有多个同类型的插件时进行默认插件的选择
    /// </summary>
    public class PluginSelector
    {
        string mPluginType;
        public string PluginType
        {
            get { return mPluginType; }
        }

        Dictionary<Guid, PluginItem> mPlugins = new Dictionary<Guid, PluginItem>();
        public Dictionary<Guid, PluginItem> Plugins
        {
            get { return mPlugins; }
        }

        //PluginItem mDefaultPlugin;
        public PluginItem DefaultPlugin
        {
            get
            {
                foreach (var i in Plugins)
                {
                    if (i.Value.DefaultSelected)
                        return i.Value;
                }
                return null;
            }
        }

        public PluginSelector(string pluginType)
        {
            mPluginType = pluginType;
        }

        public void SetDefaultPlugin(Guid id)
        {
            PluginItem item;
            if (Plugins.TryGetValue(id, out item))
            {
                //                     mDefaultPlugin = item;
                // 
                foreach (var i in Plugins)
                {
                    i.Value.DefaultSelected = false;
                }

                item.DefaultSelected = true;
            }
        }
    }


    public class PluginManager : IPartImportsSatisfiedNotification, EngineNS.Editor.IEditorInstanceObject
    {
        public static PluginManager Instance
        {
            get
            {
                var name = typeof(PluginManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new PluginManager();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }
        private PluginManager()
        {

        }
        public void FinalCleanup()
        {

        }

        public void OnImportsSatisfied()
        {

        }
        
        [ImportMany(typeof(EditorCommon.PluginAssist.IEditorPlugin), AllowRecomposition = true)]
        public IEnumerable<Lazy<EditorCommon.PluginAssist.IEditorPlugin, EditorCommon.PluginAssist.IEditorPluginData>> Plugins
        {
            get;
            private set;
        } = null;

        Dictionary<string, PluginSelector> mPluginDicWithTypeKey = new Dictionary<string, PluginSelector>();
        public Dictionary<string, PluginSelector> PluginDicWithTypeKey
        {
            get { return mPluginDicWithTypeKey; }
        }
        Dictionary<Guid, PluginItem> mPluginsDicWithIdKey = new Dictionary<Guid, PluginItem>();
        public Dictionary<Guid, PluginItem> PluginsDicWithIdKey
        {
            get { return mPluginsDicWithIdKey; }
        }

        AggregateCatalog mCatalog;
        public AggregateCatalog Catalog
        {
            get { return mCatalog; }
        }
        bool mPluginInitialized = false;

        private CompositionContainer mCompositionContainer;
        public CompositionContainer CompositionContainer
        {
            get { return mCompositionContainer; }
        }

        // 初始化插件
        public async System.Threading.Tasks.Task InitializePlugins(double maxProgress, Action<string, double> processUpdate, bool forceInitialize = false)
        {
            if (mPluginInitialized && !forceInitialize)
                return;

            var loadPGValue = maxProgress * 0.1;
            await LoadDeleteAssembly();
            processUpdate?.Invoke("读取删除的插件信息...", loadPGValue);
            
            mCatalog = new AggregateCatalog();
            mCatalog.Changed += Catalog_Changed;

            // 遍历目录，查找插件
            // CoreEditor中的插件
            //var binCata = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom(EngineNS.CEngine.Instance.FileManager.Bin + "CoreEditor.dll"));
            //mCatalog.Catalogs.Add(binCata);
            //var binCata = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom(EngineNS.CEngine.Instance.FileManager.Bin + "WPG.dll"));
            //mCatalog.Catalogs.Add(binCata);
            var binCata = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom(EngineNS.CEngine.Instance.FileManager.Bin + "EditorCommon.dll"));
            mCatalog.Catalogs.Add(binCata);
            binCata = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom(EngineNS.CEngine.Instance.FileManager.Bin + "Macross.dll"));
            mCatalog.Catalogs.Add(binCata);
            //             binCata = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom("CodeGenerateSystem.dll"));
            //             catalog.Catalogs.Add(binCata);

            var addPluginPGValue = (maxProgress - loadPGValue) * 0.9;
            var dir = EngineNS.CEngine.Instance.FileManager.Bin + "Plugins";
            AddOutPlugins(dir, addPluginPGValue, processUpdate);
            //AddOutPlugins();

            mCompositionContainer = new CompositionContainer(mCatalog);
            try
            {
                mCompositionContainer.ComposeParts(this);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            GeneratePlugins(maxProgress - addPluginPGValue, processUpdate);

            LoadDefaultPlugin();
            SavePluginInfo();
            mPluginInitialized = true;
        }

        public void GeneratePlugins(double maxProgress, Action<string, double> processUpdate)
        {
            var pgStep = (mCatalog.Catalogs.Count > 0) ? (maxProgress / mCatalog.Catalogs.Count) : 0;
            //PluginItems.Clear();
            foreach (var plugin in Plugins)
            {
                try
                {
                    EditorCommon.PluginAssist.IEditorPlugin pluginValue = null;
                    EditorCommon.PluginAssist.IEditorPluginData pluginData = null;

                    pluginValue = plugin.Value;
                    pluginData = plugin.Metadata;

                    if (pluginValue == null || pluginData == null)
                        continue;

                    processUpdate?.Invoke($"正在创建插件{pluginValue.PluginName}", pgStep);

                    Guid pluginGuid = Guid.Empty;
                    var atts = pluginValue.GetType().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                    if (atts.Length <= 0)
                        continue;

                    pluginGuid = EngineNS.Rtti.RttiHelper.GuidTryParse(((System.Runtime.InteropServices.GuidAttribute)(atts[0])).Value);
                    if (pluginGuid == Guid.Empty)
                        continue;

                    if (mPluginsDicWithIdKey.ContainsKey(pluginGuid))
                        continue;

                    var noUse = GenericPluginItemAction(pluginGuid, pluginValue, pluginData);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }
        private async System.Threading.Tasks.Task GenericPluginItemAction(Guid pluginGuid, EditorCommon.PluginAssist.IEditorPlugin pluginValue, EditorCommon.PluginAssist.IEditorPluginData pluginData)
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(()=>
            {
                var item = new PluginItem(pluginGuid, pluginValue, pluginData);
                item.PluginName = pluginValue.PluginName;
                item.Version = pluginValue.Version;

                //if (!plugin.Value.OnActive())
                //{
                //    item.Active = false;
                //    continue;
                //}
                var pluginType = pluginValue.GetType();
                var atts = pluginType.GetCustomAttributes(typeof(EditorCommon.PluginAssist.PluginMenuItemAttribute), false);
                if (atts.Length > 0)
                {
                    var piAtt = (EditorCommon.PluginAssist.PluginMenuItemAttribute)(atts[0]);
                    var menuData = EditorCommon.Menu.MenuItemDataBase.CreateMenuData(piAtt.MenuType, pluginData.PluginType);
                    menuData.MenuNames = piAtt.MenuString;
                    menuData.Count = piAtt.Count;
                    if (piAtt.Icons != null)
                    {
                        menuData.Icons = new ImageSource[piAtt.MenuString.Length];
                        for (int i = 0; i < piAtt.Icons.Length; i++)
                        {
                            menuData.Icons[i] = new BitmapImage(new Uri(piAtt.Icons[i], UriKind.Relative));
                        }
                    }
                    if (pluginValue.Icon != null)
                    {
                        if (menuData.Icons.Length == 0)
                            menuData.Icons = new ImageSource[piAtt.MenuString.Length];
                        menuData.Icons[menuData.Icons.Length - 1] = pluginValue.Icon;
                    }
                    menuData.OperationControlType = pluginType;
                    if (menuData.OperationControls == null || menuData.OperationControls.Length != menuData.Count)
                    {
                        menuData.OperationControls = new DockControl.IDockAbleControl[menuData.Count];
                    }
                    for (int i = 0; i < menuData.Count; i++)
                    {
                        var ctrl = System.Activator.CreateInstance(menuData.OperationControlType) as DockControl.IDockAbleControl;
                        if (ctrl == null)
                        {
                            EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Plugins", $"Plugin item {pluginValue.PluginName} create menus failed");
                            continue;
                        }
                        if (menuData.Count > 1)
                            ctrl.Index = i;
                        menuData.OperationControls[i] = ctrl;
                    }
                    EditorCommon.Menu.GeneralMenuManager.Instance.RegisterMenuItem(menuData);
                }

                // 将同类型的插件放入选择器字典表中
                EditorCommon.PluginAssist.PluginSelector selector;
                if (!PluginDicWithTypeKey.TryGetValue(item.PluginData.PluginType, out selector))
                {
                    selector = new EditorCommon.PluginAssist.PluginSelector(item.PluginData.PluginType);
                    PluginDicWithTypeKey[item.PluginData.PluginType] = selector;
                }
                selector.Plugins[pluginGuid] = item;

                PluginsDicWithIdKey[pluginGuid] = item;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }

        public PluginItem GetPluginItem(string pluginType)
        {
            PluginSelector selector;
            if (!PluginDicWithTypeKey.TryGetValue(pluginType, out selector))
                return null;

            Dictionary<Guid, PluginItem> activedPlufinItems = new Dictionary<Guid, PluginItem>();

            foreach (var i in selector.Plugins)
            {
                if (i.Value.Active)
                {
                    activedPlufinItems[i.Key] = i.Value as PluginItem;
                }
            }

            if (activedPlufinItems.Count == 0)
                return null;

            if (selector.DefaultPlugin == null || !selector.DefaultPlugin.Active)
            {
                if (activedPlufinItems.Count == 1)
                {
                    foreach (var obj in activedPlufinItems)
                    {
                        return obj.Value;
                    }
                }
                else
                {
                    // 弹出窗口选择本次处理要使用的插件
                    var dsw = new PluginDefaultSelectorWindow();
                    foreach (var obj in activedPlufinItems)
                    {
                        dsw.PluginItems.Add(obj.Value);
                    }
                    dsw.ShowDialog();
                    if (dsw.NeverShow)
                    {
                        selector.SetDefaultPlugin(dsw.SelectedItem.Id);
                        return selector.DefaultPlugin as PluginItem;
                    }
                    else
                        return dsw.SelectedItem;
                }
            }
            else
                return selector.DefaultPlugin as PluginItem;

            return null;
        }
        void AddOutPlugins()
        {
            // Plugin目录
            var dir = EngineNS.CEngine.Instance.FileManager.Bin + "Plugins";
            if (System.IO.Directory.Exists(dir))
            {
                var cata = new DirectoryCatalog(dir);
                mCatalog.Catalogs.Add(cata);

                foreach (var subDir in System.IO.Directory.GetDirectories(dir))
                {
                    try
                    {
                        var tagDir = subDir.Replace("\\", "/");
                        // 优先选择bin目录下的文件
                        foreach (var testDir in System.IO.Directory.GetDirectories(tagDir))
                        {
                            var tempDir = testDir.Replace("\\", "/");
                            if (tempDir == tagDir + "/bin")
                            {
                                tagDir = tempDir;
                                break;
                            }
                        }

                        cata = new DirectoryCatalog(tagDir);
                        mCatalog.Catalogs.Add(cata);
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }
        void AddOutPlugins(string dir, double maxProgress, Action<string, double> processUpdate)
        {
            if (System.IO.Directory.Exists(dir))
            {
                var directors = System.IO.Directory.GetDirectories(dir);
                foreach (var i in directors)
                {
                    var director = i + "\\bin";
                    if (!System.IO.Directory.Exists(director))
                        continue;
                    var files = System.IO.Directory.GetFiles(director, "*.dll", System.IO.SearchOption.AllDirectories);
                    var pgStep = (files.Length > 0) ? (maxProgress / files.Length) : 0;
                    foreach (var file in files)
                    {
                        AddAssemblyCatalog(file);

                        var fileName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(file);
                        processUpdate?.Invoke($"正在加载插件{fileName}", pgStep);
                    }
                }
            }
        }

        public bool AssemblyCatalogExist(string assemblyPath)
        {
            //var assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);
            foreach (var ct in mCatalog.Catalogs)
            {
                var cta = ct as AssemblyCatalog;
                if (cta != null && !cta.Assembly.IsDynamic)
                {
                    if(string.Equals(cta.Assembly.Location, assemblyPath.Replace("/", "\\"), StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
        void AddAssemblyCatalog(string file)
        {
            var fileExt = EngineNS.CEngine.Instance.FileManager.GetFileExtension(file);
            if (fileExt == "dll")
            {
                try
                {
                    if (!AssemblyCatalogExist(file))
                    {
                        file = file.Replace("/", "\\");
                        var cata = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom(file));
                        mCatalog.Catalogs.Add(cata);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }
        void Catalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            //e.AddedDefinitions;
        }

        string mConfigFile = EngineNS.CEngine.Instance.FileManager.Bin + "Plugins/PluginConfig.xml";

        List<string> mDelAssemblys = new List<string>();

        Dictionary<string, Guid> mDefaultPlugins = new Dictionary<string, Guid>();
        public void AddDelAssembly(string fileName)
        {
            mDelAssemblys.Add(fileName);
        }
        public void DelAssembly(string fileName = "")
        {
            if(string.IsNullOrEmpty(fileName))
            {
                foreach (var i in mDelAssemblys)
                {
                    System.IO.File.Delete(i);
                }

                mDelAssemblys.Clear();
            }
            else
            {
                if (mDelAssemblys.Contains(fileName))
                {
                    mDelAssemblys.Remove(fileName);
                }
            }
        }

        async System.Threading.Tasks.Task LoadDeleteAssembly()
        {
            if (!System.IO.File.Exists(mConfigFile))
                return;

            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                var holder = EngineNS.IO.XmlHolder.LoadXML(mConfigFile);
                var node = holder.RootNode.FindNode("DeleteAssemblies");
                var childNodes = node.GetNodes();
                foreach (var i in childNodes)
                {
                    mDelAssemblys.Add(i.FindAttrib("Path").Value);
                }

                DelAssembly();
                return true;
            });
        }

        public void SavePluginInfo()
        {
            var holder = EngineNS.IO.XmlHolder.NewXMLHolder("PluginsConfig", "");
            var node = holder.RootNode.AddNode("DeleteAssemblies", "", holder);
            foreach (var i in mDelAssemblys)
            {
                var childNode = node.AddNode("Assembly", "", holder);
                childNode.AddAttrib("Path", i);
            }

            node = holder.RootNode.AddNode("DefaultPlugins", "", holder);
            foreach (var i in mDefaultPlugins)
            {
                var childNode = node.AddNode(i.Key, "", holder);
                childNode.AddAttrib("Id", i.Value.ToString());
            }

            EngineNS.IO.XmlHolder.SaveXML(mConfigFile, holder, false);
        }

        bool mDefaultPluginLoaded = false;
        void LoadDefaultPlugin()
        {
            if (!System.IO.File.Exists(mConfigFile))
                return;

            mDefaultPluginLoaded = true;

            var holder = EngineNS.IO.XmlHolder.LoadXML(mConfigFile);

            var rootNode = holder.RootNode.FindNode("DefaultPlugins");
            var childNodes = rootNode.GetNodes();
            foreach (var node in childNodes)
            {
                foreach (var i in mPluginsDicWithIdKey)
                {
                    if (i.Key.ToString() == node.FindAttrib("Id").Value)
                    {
                        i.Value.DefaultSelected = true;
                        break;
                    }
                }
            }

            mDefaultPluginLoaded = false;
        }

        public void SetDefaultPlugin(PluginItem item, bool set)
        {
            if (item == null || item.PluginData == null)
                return;

            if (set)
            {
                mDefaultPlugins[item.PluginData.PluginType] = item.Id;
            }
            else
            {
                mDefaultPlugins.Remove(item.PluginData.PluginType);
            }

            if (!mDefaultPluginLoaded)
            {
                SavePluginInfo();
            }
        }
    }
}

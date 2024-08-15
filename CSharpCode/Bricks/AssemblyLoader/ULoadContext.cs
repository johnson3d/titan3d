using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using EngineNS.Macross;
using EngineNS.Rtti;

namespace EngineNS.Bricks.AssemblyLoader
{
    public class TtLoadContext : AssemblyLoadContext
    {
        //https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
        // Resolver of the locations of the assemblies that are dependencies of the
        // main plugin assembly.
        private AssemblyDependencyResolver _resolver;

        public TtLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }
        ~TtLoadContext()
        {

        }

        public List<string> IncludeAssemblies;
        // The Load method override causes all the dependencies present in the plugin's binary directory to get loaded
        // into the HostAssemblyLoadContext together with the plugin assembly itself.
        // NOTE: The Interface assembly must not be present in the plugin's binary directory, otherwise we would
        // end up with the assembly being loaded twice. Once in the default context and once in the HostAssemblyLoadContext.
        // The types present on the host and plugin side would then not match even though they would have the same names.
        protected override Assembly Load(AssemblyName name)
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in ass)
            {
                if (assembly.GetName().Name == name.Name)
                    return assembly;
            }
            if (IncludeAssemblies != null)
            {
                foreach (var j in IncludeAssemblies)
                {
                    if (name.Name.Contains(j))
                    {
                        string assemblyPath = _resolver.ResolveAssemblyToPath(name);
                        if (assemblyPath != null)
                        {
                            Console.WriteLine($"Loading assembly {assemblyPath} into the HostAssemblyLoadContext");
                            //return LoadFromAssemblyPath(assemblyPath);
                            return LoadOnMemory(assemblyPath);
                        }
                    }
                }
            }
            else
            {
                string assemblyPath = _resolver.ResolveAssemblyToPath(name);
                if (assemblyPath != null)
                {
                    Console.WriteLine($"Loading assembly {assemblyPath} into the HostAssemblyLoadContext");
                    //return LoadFromAssemblyPath(assemblyPath);
                    return LoadOnMemory(assemblyPath);
                }
            }

            return null;
        }
        public Assembly LoadOnMemory(string assemblyPath)
        {
            string pdbPath = assemblyPath.Replace(".dll", ".pdb");
            using (FileStream sr = new FileStream(assemblyPath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                byte[] buffer = new byte[sr.Length];
                sr.Read(buffer, 0, buffer.Length);
                var mrs = new System.IO.MemoryStream(buffer);
                try
                {
                    if (pdbPath != null && IO.TtFileManager.FileExists(pdbPath))
                    {
                        using (FileStream pdbStream = new FileStream(pdbPath, FileMode.Open, FileAccess.Read))
                        {
                            var pdbBuffer = new byte[pdbStream.Length];
                            pdbStream.Read(pdbBuffer, 0, pdbBuffer.Length);
                            var pdbmrs = new System.IO.MemoryStream(pdbBuffer);
                            return this.LoadFromStream(mrs, pdbmrs);
                        }
                    }
                    else
                    {
                        return this.LoadFromStream(mrs);
                    }
                }
                catch (Exception)
                {
                    return this.LoadFromStream(mrs);
                }
            }
        }

        static void ExecuteAndUnload(string assemblyPath, out WeakReference alcWeakRef)
        {
            // Create the unloadable HostAssemblyLoadContext
            var alc = new TtLoadContext(assemblyPath);

            // Create a weak reference to the AssemblyLoadContext that will allow us to detect
            // when the unload completes.
            alcWeakRef = new WeakReference(alc);

            // Load the plugin assembly into the HostAssemblyLoadContext.
            // NOTE: the assemblyPath must be an absolute path.
            Assembly a = alc.LoadFromAssemblyPath(assemblyPath);

            //// Get the plugin interface by calling the PluginClass.GetInterface method via reflection.
            //Type pluginType = a.GetType("Plugin.PluginClass");
            //MethodInfo getInterface = pluginType.GetMethod("GetInterface", BindingFlags.Static | BindingFlags.Public);
            //Plugin.Interface plugin = (Plugin.Interface)getInterface.Invoke(null, null);

            //// Now we can call methods of the plugin using the interface
            //string result = plugin.GetMessage();
            //Plugin.Version version = plugin.GetVersion();

            //Console.WriteLine($"Response from the plugin: GetVersion(): {version}, GetMessage(): {result}");

            // This initiates the unload of the HostAssemblyLoadContext. The actual unloading doesn't happen
            // right away, GC has to kick in later to collect all the stuff.
            alc.Unload();
        }

        static void TestUnload()
        {
            WeakReference hostAlcWeakRef;
            string currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginFullPath = Path.Combine(currentAssemblyDirectory, $"..\\..\\..\\..\\Plugin\\bin\\netcoreapp3.1\\Plugin.dll");
            ExecuteAndUnload(pluginFullPath, out hostAlcWeakRef);

            // Poll and run GC until the AssemblyLoadContext is unloaded.
            // You don't need to do that unless you want to know when the context
            // got unloaded. You can just leave it to the regular GC.
            for (int i = 0; hostAlcWeakRef.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Console.WriteLine($"Unload success: {!hostAlcWeakRef.IsAlive}");
        }
    }

    public enum EPluginModuleState
    {
        Unloaded,
        Loaded,
        ReloadReady,
    }
    public interface IPlugin
    {
        void OnLoadedPlugin();
        void OnUnloadPlugin();
    }

    [Rtti.Meta]
    public class UPluginDescriptor
    {
        public string FilePath;
        [Rtti.Meta]
        public bool Enable { get; set; } = true;
        [Rtti.Meta]
        public bool LoadOnInit { get; set; } = true;
        [Rtti.Meta]
        public List<EPlatformType> Platforms { get; set; } = new List<EPlatformType>() { EPlatformType.PLTF_Windows };
        [Rtti.Meta]
        public List<string> Dependencies { get; set; } = new List<string>();
        public void SaveDescriptor()
        {
            IO.TtFileManager.SaveObjectToXml(FilePath, this);
        }
    }
    public class TtPluginModule
    {
        public UPluginModuleManager Manager;
        public string Name { get; set; }
        public UPluginDescriptor PluginDescriptor = null;
        public EPluginModuleState ModuleSate { get; set; } = EPluginModuleState.Unloaded;
        public string AssemblyPath { get; set; }
        WeakReference mLoader = null;
        public WeakReference<Assembly> ModuleAssembly { get; private set; }
        public Assembly UnsafeGetAssembly()
        {
            if (ModuleAssembly == null)
                return null;
            Assembly result;
            if (ModuleAssembly.TryGetTarget(out result))
                return result;
            return null;
        }
        public bool SureLoad()
        {
            if (PluginDescriptor.Enable == false)
            {
                if (ModuleSate != EPluginModuleState.Unloaded)
                {
                    UnloadPlugin(true);
                    ModuleSate = EPluginModuleState.Unloaded;
                }
                return false;
            }
            switch (ModuleSate)
            {
                case EPluginModuleState.Unloaded:
                    {
                        try
                        {
                            if (LoadPlugin() == false)
                                return false;
                            ModuleSate = EPluginModuleState.Loaded;
                            return true;
                        }
                        catch(Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                            ModuleSate = EPluginModuleState.Unloaded;
                            return false;
                        }
                    }
                case EPluginModuleState.Loaded:
                    {
                        return true;
                    }
                case EPluginModuleState.ReloadReady:
                    {
                        Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"PluginModule({AssemblyPath}): will be reloaded");
                        if (UnloadPlugin(false) == false)
                        {
                            ModuleSate = EPluginModuleState.Loaded;
                            return true;
                        }

                        try
                        {
                            if (LoadPlugin() == false)
                                return false;
                            ModuleSate = EPluginModuleState.Loaded;
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                            ModuleSate = EPluginModuleState.Unloaded;
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }
        private bool LoadPlugin()
        {
            foreach (var i in PluginDescriptor.Dependencies)
            {
                var dModule = Manager.GetPluginModule(i);
                if (dModule == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"PluginModule({AssemblyPath}): load failed because the {i} is not found");
                    return false;
                }
                dModule.PluginDescriptor.Enable = true;
                if (false == dModule.SureLoad())
                    return false;
            }

            var context = new TtLoadContext(AssemblyPath);// Manager.CoreBinDirectory);
            //var assembly = context.LoadFromAssemblyPath(AssemblyPath);
            var assembly = context.LoadOnMemory(AssemblyPath);
            Rtti.UAssemblyDesc.UpdateRtti(this.Name, assembly, UnsafeGetAssembly());
            ModuleAssembly = new WeakReference<Assembly>(assembly);
            if (GetPluginObjectImpl() == false)
                return false;

            mLoader = new WeakReference(context, true);
            return true;
        }
        private bool GetPluginObjectImpl()
        {
            Assembly assembly;
            if (ModuleAssembly.TryGetTarget(out assembly) == false)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"PluginModule({AssemblyPath}): ModuleAssembly is not alive");
                return false;
            }
            var type = assembly.GetType($"EngineNS.Plugins.{this.Name}.UPluginLoader");
            if (type == null)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"PluginModule({AssemblyPath}): EngineNS.Plugin.UPluginLoader is not found");
                return false;
            }
            var method = type.GetMethod("GetPluginObject", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"PluginModule({AssemblyPath}): EngineNS.Plugin.UPluginLoader.GetPluginObject is not found");
                return false;
            }
            var obj = method.Invoke(null, null);
            PluginObject = obj as IPlugin;
            if (PluginObject == null)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"PluginModule({AssemblyPath}): EngineNS.Plugin.UPluginLoader.GetPluginObject return null");
                return false;
            }
            PluginObject.OnLoadedPlugin();
            return true;
        }
        private void UnloadImpl(bool bUnregAssembly)
        {
            if (bUnregAssembly)
            {
                Rtti.UTypeDescManager.Instance.UnregAssembly(this.UnsafeGetAssembly());
            }

            try
            {
                PluginObject?.OnUnloadPlugin();

                //Rtti.UTypeDescManager.Instance.UnregAssembly();
            }
            catch(Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            PluginObject = null;

            TtLoadContext context = mLoader.Target as TtLoadContext;
            if (context != null)
            {
                context.Unload();
            }
        }
        public bool UnloadPlugin(bool bUnregAssembly)
        {
            if (ModuleSate == EPluginModuleState.Unloaded)
                return true;

            UnloadImpl(bUnregAssembly);

            for (int i = 0; mLoader.IsAlive; i++)
            {
                if (i > 10)
                {
                    GetPluginObjectImpl();
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"PluginModule({AssemblyPath}) is alive still after unload");
                    return false;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            mLoader = null;
            ModuleAssembly = null;
            ModuleSate = EPluginModuleState.Unloaded;
            return true;
        }
        private IPlugin PluginObject;//do not store this object any where
        public T GetPluginObject<T>() where T : class, IPlugin
        {
            if (SureLoad() == false)
                return default(T);
            return PluginObject as T;
        }
    }
    public class UPluginModuleManager
    {
        public string CoreBinDirectory;
        private FileSystemWatcher mWatcher;
        public Dictionary<string, TtPluginModule> PluginModules { get; } = new Dictionary<string, TtPluginModule>();
        public TtPluginModule GetPluginModule(string type)
        {
            TtPluginModule module;
            if (PluginModules.TryGetValue(type, out module))
                return module;
            return null;
        }
        void OnPluginChanged(string path)
        {
            if (path.EndsWith(PlatformSuffix))
            {
                path = path.Substring(0, path.Length - PlatformSuffix.Length);
                path += ".plugin";
                var name = IO.TtFileManager.GetPureName(path);
                var module = GetPluginModule(name);
                if (module != null && module.PluginDescriptor.Enable)
                {
                    if (module.ModuleSate == EPluginModuleState.Loaded)
                        module.ModuleSate = EPluginModuleState.ReloadReady;
                }
            }
        }
        private string PlatformSuffix;
        public void InitPlugins(TtEngine engine)
        {
            CoreBinDirectory = engine.FileManager.GetRoot(IO.TtFileManager.ERootDir.Execute);
            var path = engine.FileManager.GetRoot(IO.TtFileManager.ERootDir.Plugin);

            mWatcher = new FileSystemWatcher();
            mWatcher.Path = path;
            mWatcher.IncludeSubdirectories = true;
            mWatcher.Filter = "*.dll";
            mWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            mWatcher.Changed += (sender, e) => OnPluginChanged(e.FullPath);
            //mWatcher.Created += (sender, e) => OnPluginChanged(e.FullPath);
            //mWatcher.Deleted += (sender, e) => OnPluginChanged(e.FullPath);
            //mWatcher.Renamed += (sender, e) => { OnPluginChanged(e.FullPath); OnPluginChanged(e.OldFullPath); };
            mWatcher.EnableRaisingEvents = true;

            var files = IO.TtFileManager.GetFiles(path, "*.plugin", false);
#if PWindow
            bool bTest = false;
            if (bTest)
            {
                var template = new UPluginDescriptor();
                template.FilePath = path + "template.xml";
                template.SaveDescriptor();
            }
#elif PAndroid
#endif
            PluginModules.Clear();
            PlatformSuffix = "Window.dll";
            switch (engine.CurrentPlatform)
            {
                case EPlatformType.PLTF_Windows:
                    PlatformSuffix = ".Window.dll";
                    break;
                case EPlatformType.PLTF_Android:
                    PlatformSuffix = ".Android.dll";
                    break;
            }

            foreach (var i in files)
            {
                var descriptor = IO.TtFileManager.LoadXmlToObject<UPluginDescriptor>(i);
                if (descriptor == null)
                    continue;
                descriptor.FilePath = i;

                if (descriptor.Platforms.Contains(engine.CurrentPlatform) == false)
                    continue;

                var name = IO.TtFileManager.GetPureName(i);
                var module = new TtPluginModule();
                module.PluginDescriptor = descriptor;
                module.Manager = this;
                module.Name = name;
                var dir = IO.TtFileManager.GetBaseDirectory(i);
                module.AssemblyPath = dir + name + "/" + name + PlatformSuffix;
                PluginModules.Add(name, module);
            }

            foreach (var i in PluginModules)
            {
                if (i.Value.PluginDescriptor.LoadOnInit == false)
                    continue;
                i.Value.SureLoad();
            }

            var taskModule = this.GetPluginModule("GameTasks");
            if (taskModule != null)
            {
                //test code
                //taskModule.UnloadPlugin(true);
            }
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Bricks.AssemblyLoader.UPluginModuleManager PluginModuleManager { get; } = new Bricks.AssemblyLoader.UPluginModuleManager();
    }
}

namespace EngineNS.Macross
{
    public class UMacrosAssemblyLoader : IAssemblyLoader
    {
        Bricks.AssemblyLoader.TtLoadContext Loader = null;
        public List<string> IncludeAssemblies { get; } = new List<string>();
        public System.Reflection.Assembly LoadAssembly(string assemblyPath, string pdbPath = null)
        {
            TryUnload();

            Loader = new Bricks.AssemblyLoader.TtLoadContext(assemblyPath);
            Loader.IncludeAssemblies = IncludeAssemblies;

            using (FileStream sr = new FileStream(assemblyPath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                byte[] buffer = new byte[sr.Length];
                sr.Read(buffer, 0, buffer.Length);
                var mrs = new System.IO.MemoryStream(buffer);
                try
                {
                    if (pdbPath != null && IO.TtFileManager.FileExists(pdbPath))
                    {
                        using (FileStream pdbStream = new FileStream(pdbPath, FileMode.Open, FileAccess.Read))
                        {
                            var pdbBuffer = new byte[pdbStream.Length];
                            pdbStream.Read(pdbBuffer, 0, pdbBuffer.Length);
                            var pdbmrs = new System.IO.MemoryStream(pdbBuffer);
                            return Loader.LoadFromStream(mrs, pdbmrs);
                        }
                    }
                    else
                    {
                        return Loader.LoadFromStream(mrs);
                    }
                }
                catch (Exception)
                {
                    return Loader.LoadFromStream(mrs);
                }
            }
            //return Loader.LoadFromAssemblyPath(assemblyPath);
        }
        public void TryUnload()
        {
            if (Loader != null)
            {
                Loader.Unload();
                Loader = null;
            }
        }
        public object GetInnerObject()
        {
            return Loader;
        }
    }
    public partial class UMacrossModule
    {
        partial void CreateAssemblyLoader(ref IAssemblyLoader loader)
        {
            loader = new UMacrosAssemblyLoader();
        }
    }
}
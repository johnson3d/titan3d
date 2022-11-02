using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Reflection.Metadata.Ecma335;

namespace EngineNS.Bricks.AssemblyLoader
{
    public class ULoadContext : AssemblyLoadContext
    {
        //https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
        // Resolver of the locations of the assemblies that are dependencies of the
        // main plugin assembly.
        private AssemblyDependencyResolver _resolver;

        public ULoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }
        ~ULoadContext()
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
                    return null;
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
                    if (pdbPath != null && IO.FileManager.FileExists(pdbPath))
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
            var alc = new ULoadContext(assemblyPath);

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
    public abstract class UPlugin
    {
        public abstract void OnLoadedPlugin();
        public abstract void OnUnloadPlugin();
    }

    [Rtti.Meta]
    public class UPluginDescriptor
    {
        public string FilePath;
        [Rtti.Meta]
        public bool LoadOnInit { get; set; } = true;
        [Rtti.Meta]
        public List<EPlatformType> Platforms { get; set; } = new List<EPlatformType>() { EPlatformType.PLTF_Windows };
        public void SaveDescriptor()
        {
            IO.FileManager.SaveObjectToXml(FilePath, this);
        }
    }
    public class UPluginModule
    {
        public UPluginModuleManager Manager;
        public string Name { get; set; }
        public UPluginDescriptor PluginDescriptor = null;
        public EPluginModuleState ModuleSate { get; set; } = EPluginModuleState.Unloaded;
        public string AssemblyPath { get; set; }
        WeakReference mLoader = null;
        public void SureLoad()
        {
            switch (ModuleSate)
            {
                case EPluginModuleState.Unloaded:
                    {
                        try
                        {
                            LoadPlugin();
                            ModuleSate = EPluginModuleState.Loaded;
                        }
                        catch(Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                            ModuleSate = EPluginModuleState.Unloaded;
                        }
                    }
                    break;
                case EPluginModuleState.Loaded:
                    {
                        return;
                    }
                case EPluginModuleState.ReloadReady:
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Plugin", $"PluginModule({AssemblyPath}): will be reloaded");
                        UnloadPlugin();

                        try
                        {
                            LoadPlugin();
                            ModuleSate = EPluginModuleState.Loaded;
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                            ModuleSate = EPluginModuleState.Unloaded;
                        }
                    }
                    break;
            }
        }
        private void LoadPlugin()
        {
            var context = new ULoadContext(AssemblyPath);// Manager.CoreBinDirectory);
            //var assembly = context.LoadFromAssemblyPath(AssemblyPath);
            var assembly = context.LoadOnMemory(AssemblyPath);

            var type = assembly.GetType("EngineNS.Plugin.UPluginLoader");
            if (type == null)
            {
                return;
            }
            var method = type.GetMethod("GetPluginObject", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                return;
            }
            var obj = method.Invoke(null, null);
            PluginObject = obj as UPlugin;
            if (PluginObject == null)
                return;
            PluginObject.OnLoadedPlugin();
            mLoader = new WeakReference(context, true);
        }
        private void UnloadImpl()
        {
            try
            {
                PluginObject?.OnUnloadPlugin();
            }
            catch(Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            PluginObject = null;

            ULoadContext context = mLoader.Target as ULoadContext;
            if (context != null)
            {
                context.Unload();
            }
        }
        public void UnloadPlugin()
        {
            UnloadImpl();

            for (int i = 0; mLoader.IsAlive; i++)
            {
                if (i > 10)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Plugin", $"PluginModule({AssemblyPath}) is alive still after unload");
                    break;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            mLoader = null;
            ModuleSate = EPluginModuleState.Unloaded;
        }
        private UPlugin PluginObject;//do not store this object any where
        public T GetPluginObject<T>() where T : UPlugin
        {
            SureLoad();
            return PluginObject as T;
        }
    }
    public class UPluginModuleManager
    {
        public string CoreBinDirectory;
        private FileSystemWatcher mWatcher;
        public Dictionary<string, UPluginModule> PluginModules { get; } = new Dictionary<string, UPluginModule>();
        public UPluginModule GetPluginModule(string type)
        {
            UPluginModule module;
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
                var name = IO.FileManager.GetPureName(path);
                var module = GetPluginModule(name);
                if (module != null)
                {
                    if (module.ModuleSate == EPluginModuleState.Loaded)
                        module.ModuleSate = EPluginModuleState.ReloadReady;
                }
            }
        }
        private string PlatformSuffix;
        public void InitPlugins(UEngine engine)
        {
            CoreBinDirectory = engine.FileManager.GetRoot(IO.FileManager.ERootDir.Execute);
            var path = engine.FileManager.GetRoot(IO.FileManager.ERootDir.Plugin);

            mWatcher = new FileSystemWatcher();
            mWatcher.Path = path;
            mWatcher.IncludeSubdirectories = false;
            mWatcher.Filter = "*.dll";
            mWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            mWatcher.Changed += (sender, e) => OnPluginChanged(e.FullPath);
            //mWatcher.Created += (sender, e) => OnPluginChanged(e.FullPath);
            //mWatcher.Deleted += (sender, e) => OnPluginChanged(e.FullPath);
            //mWatcher.Renamed += (sender, e) => { OnPluginChanged(e.FullPath); OnPluginChanged(e.OldFullPath); };
            mWatcher.EnableRaisingEvents = true;

            
#if PWindow
            var files = System.IO.Directory.GetFiles(path, "*.plugin");
            if (false)
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
                var descriptor = IO.FileManager.LoadXmlToObject<UPluginDescriptor>(i);
                if (descriptor == null)
                    continue;
                descriptor.FilePath = i;

                if (descriptor.Platforms.Contains(engine.CurrentPlatform) == false)
                    continue;

                var name = IO.FileManager.GetPureName(i);
                var module = new UPluginModule();
                module.PluginDescriptor = descriptor;
                module.Manager = this;
                module.Name = name;
                var dir = IO.FileManager.GetBaseDirectory(i);
                module.AssemblyPath = dir + name + PlatformSuffix;
                PluginModules.Add(name, module);
                if (module.PluginDescriptor.LoadOnInit)
                    module.SureLoad();
            }

            //foreach (var i in PluginModules)
            //{
            //    i.Value.SureLoad();
            //    i.Value.UnloadPlugin();
            //}
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Bricks.AssemblyLoader.UPluginModuleManager PluginModuleManager { get; } = new Bricks.AssemblyLoader.UPluginModuleManager();
    }
}

namespace EngineNS.Macross
{
    public class UMacrosAssemblyLoader : IAssemblyLoader
    {
        Bricks.AssemblyLoader.ULoadContext Loader = null;
        public List<string> IncludeAssemblies { get; } = new List<string>();
        public System.Reflection.Assembly LoadAssembly(string assemblyPath, string pdbPath = null)
        {
            TryUnload();

            Loader = new Bricks.AssemblyLoader.ULoadContext(assemblyPath);
            Loader.IncludeAssemblies = IncludeAssemblies;

            using (FileStream sr = new FileStream(assemblyPath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                byte[] buffer = new byte[sr.Length];
                sr.Read(buffer, 0, buffer.Length);
                var mrs = new System.IO.MemoryStream(buffer);
                try
                {
                    if (pdbPath != null && IO.FileManager.FileExists(pdbPath))
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
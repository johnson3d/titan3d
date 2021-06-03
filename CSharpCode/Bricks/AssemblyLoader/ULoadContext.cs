using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

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
            foreach(var j in IncludeAssemblies)
            {
                if (name.Name.Contains(j))
                {
                    string assemblyPath = _resolver.ResolveAssemblyToPath(name);
                    if (assemblyPath != null)
                    {
                        Console.WriteLine($"Loading assembly {assemblyPath} into the HostAssemblyLoadContext");
                        return LoadFromAssemblyPath(assemblyPath);
                    }
                }
            }

            return null;
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
}

namespace EngineNS.Macross
{
    public class UMacrosAssemblyLoader : IAssemblyLoader
    {
        Bricks.AssemblyLoader.ULoadContext Loader = null;
        public List<string> IncludeAssemblies { get; } = new List<string>();
        public System.Reflection.Assembly LoadAssembly(string assemblyPath)
        {
            TryUnload();

            Loader = new Bricks.AssemblyLoader.ULoadContext(assemblyPath);
            Loader.IncludeAssemblies = IncludeAssemblies;

            using (FileStream sr = new FileStream(assemblyPath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                byte[] buffer = new byte[sr.Length];
                sr.Read(buffer, 0, buffer.Length);
                var mrs = new System.IO.MemoryStream(buffer);
                return Loader.LoadFromStream(mrs);
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
using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Reflection;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace EngineNS.Bricks.Plugins
{
    public interface IPlugin : IDisposable
    {
        List<Assembly> DefaultAssemblies { get; set; }
        string PluginDirectory { get; set; }
        string PluginName { get; set; }
    }
    public class TtPluginLoader
    {
        public List<Assembly> DefaultAssemblies { get; set; }
        public string PluginDirectory { get; set; }
        public string PluginName { get; set; }

        private volatile IPlugin mPluginInstance;        
        private AssemblyLoadContext mLoadContext;        
        private volatile bool mChanged;
        private FileSystemWatcher mWatcher;
        private object _reloadLock;
        public TtPluginLoader(string pluginName, string pluginDirectory)
        {
            DefaultAssemblies = AssemblyLoadContext.Default.Assemblies
                .Where(assembly => !assembly.IsDynamic)
                .ToList();
            PluginName = pluginName;
            PluginDirectory = pluginDirectory;
            _reloadLock = new object();
            ListenFileChanges();
        }
        private void ListenFileChanges()
        {
            Action<string> onFileChanged = path =>
            {
                if (Path.GetExtension(path).ToLower() == ".cs")
                    mChanged = true;
            };
            mWatcher = new FileSystemWatcher();
            mWatcher.Path = PluginDirectory;
            mWatcher.IncludeSubdirectories = true;
            mWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            mWatcher.Changed += (sender, e) => onFileChanged(e.FullPath);
            mWatcher.Created += (sender, e) => onFileChanged(e.FullPath);
            mWatcher.Deleted += (sender, e) => onFileChanged(e.FullPath);
            mWatcher.Renamed += (sender, e) => { onFileChanged(e.FullPath); onFileChanged(e.OldFullPath); };
            mWatcher.EnableRaisingEvents = true;
        }
        private void UnloadPlugin()
        {
            mPluginInstance?.Dispose();
            mPluginInstance = null;
            mLoadContext?.Unload();
            mLoadContext = null;
        }
        private Assembly CompilePlugin()
        {
            var binDirectory = Path.Combine(PluginDirectory, "bin");
            var dllPath = Path.Combine(binDirectory, $"{PluginName}.dll");
            if (!Directory.Exists(binDirectory))
                Directory.CreateDirectory(binDirectory);
            if (File.Exists(dllPath))
            {
                File.Delete($"{dllPath}.old");
                File.Move(dllPath, $"{dllPath}.old");
            }

            var sourceFiles = Directory.EnumerateFiles(
                PluginDirectory, "*.cs", SearchOption.AllDirectories);
            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug);
            var references = DefaultAssemblies
                .Select(assembly => assembly.Location)
                .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path))
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToList();
            var syntaxTrees = sourceFiles
                .Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p)))
                .ToList();
            var compilation = CSharpCompilation.Create(PluginName)
                .WithOptions(compilationOptions)
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            var emitResult = compilation.Emit(dllPath);
            if (!emitResult.Success)
            {
                throw new InvalidOperationException(string.Join("\r\n",
                    emitResult.Diagnostics.Where(d => d.WarningLevel == 0)));
            }
            //return _context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));
            using (var stream = File.OpenRead(dllPath))
            {
                var assembly = mLoadContext.LoadFromStream(stream);
                return assembly;
            }
        }
        public IPlugin GetInstance()//Get出来得接口不能保存，防止内存泄漏
        {
            var instance = mPluginInstance;
            if (instance != null && !mChanged)
                return instance;

            lock (_reloadLock)
            {
                instance = mPluginInstance;
                if (instance != null && !mChanged)
                    return instance;

                UnloadPlugin();
                mLoadContext = new AssemblyLoadContext(
                    name: $"Plugin-{PluginName}", isCollectible: true);

                var assembly = CompilePlugin();
                var pluginType = assembly.GetTypes()
                    .First(t => typeof(IPlugin).IsAssignableFrom(t));
                instance = (IPlugin)Activator.CreateInstance(pluginType);

                mPluginInstance = instance;
                mChanged = false;
            }

            return instance;
        }

        public void Dispose()
        {
            UnloadPlugin();
            mWatcher?.Dispose();
            mWatcher = null;
        }
    }
}


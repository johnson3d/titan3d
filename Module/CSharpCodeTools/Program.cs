using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EngineNS;

namespace CSharpCodeTools
{
    class Program
    {
        public static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                {
                    return i.Substring(startWith.Length);
                }
            }
            return null;
        }
        public static string[] GetArguments(string[] args, string startWith, char split = '+')
        {
            var types = FindArgument(args, startWith);
            if (types != null)
            {
                return types.Split(split);
            }
            return null;
        }
        static void Main(string[] args)
        {
            var modes = GetArguments(args, "mode=");
            if (HasMode(modes, "DataCopyer"))
            {
                RunDataCopyerPostBuild(args);
                return;
            }
        
            var file = args[0].Replace('\\', '/');
            var idx = file.LastIndexOf('/');
            var dir = file.Substring(0, idx);
            var segs = System.IO.File.ReadAllLines(file);

            bool workRpc = true;
            bool workAutoSync = true;
            bool workMacross = true;
            if (modes != null)
            {
                workRpc = false;
                workAutoSync = false;
                workMacross = false;
                foreach(var i in modes)
                {
                    switch(i)
                    {
                        case "Rpc":
                            workRpc = true;
                            break;
                        case "AutoSync":
                            workAutoSync = true;
                            break;
                        case "Macross":
                            workMacross = true;
                            break;
                    }
                }
            }

            var text = FindArgument(segs, "Include=");
            List<string> includes = new List<string>();
            {
                var inc = text.Split(',');
                Console.WriteLine("Include:");
                foreach (var i in inc)
                {
                    includes.Add(dir + "/" + i);
                    Console.WriteLine(dir + "/" + i);
                }
            }
            text = FindArgument(segs, "Exclude=");
            text = text.Replace("\r", "");            
            List<string> excludes = new List<string>();
            {
                var inc = text.Split(',');
                Console.WriteLine("Exclude:");
                foreach (var i in inc)
                {
                    excludes.Add(dir + "/" + i);
                    Console.WriteLine(dir + "/" + i);
                }
            }

            if (workRpc)
            {
                text = FindArgument(segs, "Target=");
                if (text == null)
                {
                    return;
                }

                Console.WriteLine("CSharp build event: Rpc");
                string target = dir + "/" + text;
                Console.WriteLine($"Target={target}");
                URpcCodeManager.Instance.GatherCodeFiles(includes, excludes);
                URpcCodeManager.Instance.GatherRpcClass(target);

                //Console.WriteLine("Rpc:GatherClass");
                //URpcCodeManager.Instance.GatherCodeFiles(includes, excludes);
                //URpcCodeManager.Instance.GatherClass();
                //Console.WriteLine("Rpc:WriteCode");
                //URpcCodeManager.Instance.WriteCode(target);
                URpcCodeManager.Instance.MakeSharedProjectCSharp(target + "/", "EngineRPC.projitems");
                Console.WriteLine("Rpc:Finished");
            }

            if (workAutoSync)
            {
                text = FindArgument(segs, "Property_Target=");
                if (text == null)
                {
                    return;
                }
        
                Console.WriteLine("CSharp build event: AutoSync");
                string property_target = dir + "/" + text;
        
                PropertyGen.UPropertyCodeManager.Instance.GatherCodeFiles(includes, excludes);
                PropertyGen.UPropertyCodeManager.Instance.GatherAutoSyncClass(property_target);
            }
        
            if (workMacross)
            {
                text = FindArgument(segs, "Macross_Target=");
                if (text==null)
                {
                    return;
                }
                string macross_target = dir + "/" + text;
        
                Macross.UMacrossClassManager.Instance.GatherCodeFiles(includes, excludes);
                Console.WriteLine("Macross:GatherClass");
                Macross.UMacrossClassManager.Instance.GatherMacrossClass(macross_target);
        
                Macross.UMacrossContextMenuManager.Instance.GatherCodeFiles(includes, excludes);
                Console.WriteLine("MacrossContextMenu:GatherClass");
                Macross.UMacrossContextMenuManager.Instance.GatherClass();
                Console.WriteLine("MacrossContextMenu:WriteCode");
                Macross.UMacrossContextMenuManager.Instance.WriteCode(macross_target);
                Macross.UMacrossClassManager.Instance.WritedFiles.Add($"{macross_target}/MacrossContextMenu.macross.cs".ToLower());
                Macross.UMacrossClassManager.Instance.MakeSharedProjectCSharp(macross_target + "/", "EngineMacross.projitems");
                Console.WriteLine("Macross:Finished");
            }
        }
        
        static bool HasMode(string[] modes, string mode)
        {
            if (modes == null)
                return false;

            foreach (var i in modes)
            {
                if (string.Equals(i, mode, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        static void RunDataCopyerPostBuild(string[] args)
        {
            var titanRoot = FindArgument(args, "TitanRoot=");
            if (string.IsNullOrEmpty(titanRoot))
            {
                titanRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));
            }
            titanRoot = SanitizeCommandLinePath(titanRoot);
            titanRoot = Path.GetFullPath(titanRoot);
            titanRoot = EngineNS.IO.TtFileManager.SureAsDirectory(titanRoot).Replace('\\', '/');

            var loadedModules = new List<string>();
            loadedModules.Add("EngineCore");
                        
            var forceBuild = string.Equals(FindArgument(args, "Force="), "true", StringComparison.OrdinalIgnoreCase);
                        
            Console.WriteLine("CSharp build event: DataCopyer");
            Console.WriteLine($"TitanRoot={titanRoot}");
                        
            RegisterToolAssemblyTypes();
            RegisterAllPluginAssemblyTypes(titanRoot, loadedModules);
            Console.WriteLine($"Modules={string.Join(",", loadedModules)}");
            LoadMetasForDataCopyer(titanRoot);
            
            var hash = EngineNS.Bricks.DataCopyer.TtDataCopyer.CalcVersionHash();
            var genFile = Path.Combine(titanRoot, "Plugins/DataCopyer/DataCopyer/Copyer.gen.cs");
            var oldHash = ReadDataCopyerHash(genFile);
            var changed = oldHash != hash.ToString() || HasDataCopyerModulesHeader(genFile);
            if (changed)
            {
                var code = EngineNS.Bricks.DataCopyer.TtDataCopyer.GenCode(hash);
                WriteTextIfChanged(genFile, code, out changed);
            }
            else
            {
                Console.WriteLine("DataCopyer: GenCode skipped by hash");
            }
            Console.WriteLine($"DataCopyer: VersionHash={hash}");
            Console.WriteLine(changed ? $"DataCopyer: Wrote {genFile}" : $"DataCopyer: {genFile} unchanged");

            var pluginDll = Path.Combine(titanRoot, "binaries/Plugins/DataCopyer/DataCopyer.All.dll");
            if (forceBuild || changed || File.Exists(pluginDll) == false)
            {
                var projectFile = Path.Combine(titanRoot, "Plugins/DataCopyer/DataCopyer.All/DataCopyer.All.csproj");
                if (BuildProject(projectFile, titanRoot) == false)
                    Environment.ExitCode = -1;
            }
            else
            {
                Console.WriteLine("DataCopyer: Build skipped");
            }
            Console.WriteLine("DataCopyer:Finished");
        }
        static string SanitizeCommandLinePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            return path.Trim().Replace("\"", "");
        }
        static string[] GetDataCopyerModules(string[] args, string titanRoot)
        {
            var moduleArg = FindArgument(args, "Module=");
            var result = new List<string>();
            if (string.IsNullOrEmpty(moduleArg) == false)
            {
                AddDataCopyerModules(result, moduleArg.Split(',', ';', '|', '+'));
                return result.ToArray();
            }
        
            result.Add("EngineCore");
            AddDataCopyerModules(result, ReadPluginModules(Path.Combine(titanRoot, "content/engineconfigforcook.jscfg")));
            return result.ToArray();
        }
        static void AddDataCopyerModules(List<string> modules, IEnumerable<string> names)
        {
            foreach (var i in names)
            {
                var name = i.Trim().Trim('"');
                if (string.IsNullOrEmpty(name))
                    continue;
                if (modules.Contains(name) == false)
                    modules.Add(name);
            }
        }
        static string[] ReadPluginModules(string configFile)
        {
            if (File.Exists(configFile) == false)
                return Array.Empty<string>();
        
            var plugins = new List<string>();
            var inPlugins = false;
            foreach (var line in File.ReadLines(configFile))
            {
                var text = line.Trim();
                if (inPlugins == false)
                {
                    if (text.StartsWith("\"Plugins\"", StringComparison.Ordinal))
                        inPlugins = text.Contains("[");
                    continue;
                }
        
                if (text.StartsWith("]", StringComparison.Ordinal))
                    break;
        
                text = text.TrimEnd(',').Trim().Trim('"');
                if (string.IsNullOrEmpty(text) == false)
                    plugins.Add(text);
            }
            return plugins.ToArray();
        }
        static void RegisterToolAssemblyTypes()
        {
            var assembly = typeof(EngineNS.TtEngine).Assembly;
            EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, assembly);
            foreach (var assemblyName in new[] { "System.Private.CoreLib", "System.Runtime" })
            {
                var sysAssembly = EngineNS.Rtti.TtTypeDescManager.Instance.FindAssemblyInCurrentDomain(assemblyName);
                if (sysAssembly != null)
                    EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, sysAssembly);
            }
        }
        static void RegisterAllPluginAssemblyTypes(string titanRoot, List<string> loadedModules)
        {
            var pluginRoot = Path.Combine(titanRoot, "Plugins");
            if (Directory.Exists(pluginRoot) == false)
                return;
        
            var registeredModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pluginFile in Directory.EnumerateFiles(pluginRoot, "*.plugin", SearchOption.AllDirectories))
            {
                var moduleName = Path.GetFileNameWithoutExtension(pluginFile);
                if (string.IsNullOrEmpty(moduleName))
                    continue;
                if (string.Equals(moduleName, "DataCopyer", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (registeredModules.Add(moduleName) == false)
                    continue;
        
                RegisterPluginAssemblyType(titanRoot, moduleName, loadedModules);
            }
        }
        static void RegisterPluginAssemblyType(string titanRoot, string moduleName, List<string> loadedModules)
        {
            var pluginDir = Path.Combine(titanRoot, "binaries/Plugins", moduleName);
            if (Directory.Exists(pluginDir) == false)
            {
                Console.WriteLine($"DataCopyer: Plugin binaries not found {moduleName}");
                return;
            }
        
            var dllFile = FindPluginAssemblyFile(pluginDir, moduleName);
            if (string.IsNullOrEmpty(dllFile))
            {
                Console.WriteLine($"DataCopyer: Plugin assembly not found {moduleName}");
                return;
            }
                    
            try
            {
                var assembly = System.Reflection.Assembly.LoadFrom(dllFile);
                EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, assembly);
                RegisterPluginTypeAliases(moduleName, assembly);
                loadedModules.Add(moduleName);
                Console.WriteLine($"DataCopyer: Registered plugin assembly {moduleName}={Path.GetFileName(dllFile)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DataCopyer: Register plugin assembly failed {moduleName}: {ex.Message}");
            }
        }
        static string FindPluginAssemblyFile(string pluginDir, string moduleName)
        {
            foreach (var fileName in new[] { $"{moduleName}.All.dll", $"{moduleName}.Window.dll", $"{moduleName}.dll" })
            {
                var file = Path.Combine(pluginDir, fileName);
                if (File.Exists(file))
                    return file;
            }

            foreach (var file in Directory.GetFiles(pluginDir, $"{moduleName}.*.dll", SearchOption.TopDirectoryOnly))
            {
                if (file.EndsWith(".comhost.dll", StringComparison.OrdinalIgnoreCase))
                    continue;
                return file;
            }
            return null;
        }
        static void RegisterPluginTypeAliases(string moduleName, System.Reflection.Assembly assembly)
        {
            EngineNS.Rtti.TtTypeDescManager.Instance.InterateTypes(typeDesc =>
            {
                if (typeDesc?.SystemType?.Assembly != assembly)
                    return;

                var typeString = typeDesc.TypeString;
                if (string.IsNullOrEmpty(typeString))
                    return;

                var index = typeString.LastIndexOf('@');
                if (index < 0)
                    return;

                var alias = typeString.Substring(0, index + 1) + moduleName;
                EngineNS.Rtti.TtTypeDescManager.Instance.NameAliasTypes[alias] = typeDesc;
            });
        }
        static void LoadMetasForDataCopyer(string titanRoot)
        {
            var manager = EngineNS.Rtti.TtClassMetaManager.Instance;
            manager.MetaRoot = Path.Combine(titanRoot, "enginecontent/metadata").Replace('\\', '/');
                
            var roots = new[]
            {
                Path.Combine(titanRoot, "enginecontent/metadata"),
                Path.Combine(titanRoot, "content/metadata"),
            };
                
            foreach (var root in roots)
            {
                if (Directory.Exists(root) == false)
                    continue;
                
                foreach (var typedesc in Directory.EnumerateFiles(root, "typedesc.txt", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(typedesc);
                    EngineNS.Rtti.TtClassMeta.TypeDescText(text.Replace("\r", ""), out var readModule, out var typeName);
                    if (string.IsNullOrEmpty(readModule) || string.IsNullOrEmpty(typeName))
                        continue;
                
                    var type = EngineNS.Rtti.TtTypeDesc.TypeOf(typeName, out _);
                    if (type == null)
                    {
                        Console.WriteLine($"DataCopyer: Skip unresolved type {typeName}");
                        continue;
                    }
                
                    if (manager.Metas.TryGetValue(typeName, out var meta) == false)
                    {
                        meta = new EngineNS.Rtti.TtClassMeta(type);
                        manager.Metas[typeName] = meta;
                    }
                    meta.LoadClass(Path.GetDirectoryName(typedesc).Replace('\\', '/'));
                }
            }
                
            manager.BuildMeta();
            Console.WriteLine($"DataCopyer: Loaded {manager.Metas.Count} metas");
        }
        static string ReadDataCopyerHash(string file)
        {
            const string hashPrefix = "DataCopyerHash=";
            if (File.Exists(file) == false)
                return null;
        
            using var reader = new StreamReader(file);
            var firstLine = reader.ReadLine();
            if (string.IsNullOrEmpty(firstLine))
                return null;
        
            var index = firstLine.IndexOf(hashPrefix, StringComparison.Ordinal);
            if (index < 0)
                return null;
        
            return firstLine.Substring(index + hashPrefix.Length).Trim();
        }
        static bool HasDataCopyerModulesHeader(string file)
        {
            const string modulesPrefix = "//DataCopyerModules=";
            if (File.Exists(file) == false)
                return false;

            using var reader = new StreamReader(file);
            reader.ReadLine();
            var secondLine = reader.ReadLine();
            return secondLine != null && secondLine.StartsWith(modulesPrefix, StringComparison.Ordinal);
        }
        static void WriteTextIfChanged(string file, string text, out bool changed)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            if (File.Exists(file))
            {
                var oldText = File.ReadAllText(file);
                if (oldText == text)
                {
                    changed = false;
                    return;
                }
            }
            File.WriteAllText(file, text);
            changed = true;
        }
        static bool BuildProject(string projectFile, string titanRoot)
        {
            Console.WriteLine($"DataCopyer: Build {projectFile}");
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectFile}\" /p:TitanRoot={titanRoot.Replace('/', '\\')} /p:HotReloadEnabled=false --nologo",
                WorkingDirectory = titanRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            var errors = process.StandardError.ReadToEnd();
            if (string.IsNullOrEmpty(errors) == false)
                Console.Error.WriteLine(errors);
            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }
}

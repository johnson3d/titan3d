#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharpReflectionTools
{
    // 执行段：只做 RTTI 注册与元数据生成，不调用 PreInitEngine，也不创建图形设备。
    static class DataCopyerGenerator
    {
        public static int Run(ReflectionHostOptions options, Assembly engine, IReadOnlyList<(PluginAssembly Plugin, Assembly Assembly)> plugins)
        {
            var actual = typeof(EngineNS.TtEngine).Assembly;
            if (actual != engine || !string.Equals(Path.GetFullPath(actual.Location), options.EngineAssembly, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"EngineNS 类型来自 {actual.Location}，不是父构建传入的 {options.EngineAssembly}");

            var root = options.TitanRoot.Replace('\\', '/') + "/";
            RegisterEngineAssemblies(engine);
            foreach (var (plugin, assembly) in plugins)
            {
                EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, assembly);
                RegisterPluginTypeAliases(plugin.Name, assembly);
                Console.WriteLine($"DataCopyer: Registered plugin assembly {plugin.Name}={Path.GetFileName(plugin.Path)}");
            }
            Console.WriteLine($"Modules={string.Join(",", new[] { "EngineCore" }.Concat(plugins.Select(p => p.Plugin.Name)))}");
            LoadMetas(root);

            var hash = EngineNS.Bricks.DataCopyer.TtDataCopyer.CalcVersionHash();
            var code = EngineNS.Bricks.DataCopyer.TtDataCopyer.GenCode(hash);
            var generatedChanged = WriteTextIfChanged(options.GeneratedFile, code);
            var references = string.Concat(plugins.Select(p => p.Plugin.Path + "\n"));
            var referencesChanged = WriteTextIfChanged(options.ReferencesFile, references);
            Console.WriteLine($"DataCopyer: VersionHash={hash}");
            Console.WriteLine(generatedChanged ? $"DataCopyer: Wrote {options.GeneratedFile}" : $"DataCopyer: {options.GeneratedFile} unchanged");

            var current = DataCopyerState.Create(options, plugins.Select(p => p.Plugin).ToList());
            current.Hash = hash.ToString();
            current.GeneratedSha256 = DataCopyerState.Sha256(options.GeneratedFile);
            var previous = DataCopyerState.Load(options.StateFile);
            var output = AssemblyIdentity.Read(options.DataCopyerAssembly);
            var upToDate = !options.Force && !generatedChanged && !referencesChanged && options.IsDataCopyerDescriptorCurrent()
                && previous != null && previous.SameInputs(current)
                && previous.Hash == current.Hash && previous.GeneratedSha256 == current.GeneratedSha256
                && output.Mvid != "" && output.SameAs(previous.Output);
            if (upToDate)
            {
                Console.WriteLine("DataCopyer: Build skipped");
                Console.WriteLine("DataCopyer:Finished");
                return 0;
            }

            // 构建失败时不能留下旧的成功记录。
            if (File.Exists(options.StateFile)) File.Delete(options.StateFile);
            if (BuildDataCopyer(options) != 0)
            {
                Console.Error.WriteLine("DataCopyer: 构建失败 " + options.DataCopyerProject);
                return 1;
            }
            current.Output = AssemblyIdentity.Read(options.DataCopyerAssembly);
            if (!options.IsDataCopyerDescriptorCurrent())
            {
                Console.Error.WriteLine("DataCopyer: 构建后缺少或未正确部署插件描述符 " + Path.Combine(options.PluginDirectory, "DataCopyer.plugin"));
                return 1;
            }
            if (current.Output.Mvid == "")
            {
                Console.Error.WriteLine("DataCopyer: 构建后缺少 " + options.DataCopyerAssembly);
                return 1;
            }
            current.Save(options.StateFile);
            Console.WriteLine("DataCopyer:Finished");
            return 0;
        }

        static void RegisterEngineAssemblies(Assembly engine)
        {
            EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, engine);
            foreach (var name in new[] { "System.Private.CoreLib", "System.Runtime" })
            {
                var system = EngineNS.Rtti.TtTypeDescManager.Instance.FindAssemblyInCurrentDomain(name);
                if (system != null)
                    EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, system);
            }
        }

        static void RegisterPluginTypeAliases(string moduleName, Assembly assembly)
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
                EngineNS.Rtti.TtTypeDescManager.Instance.NameAliasTypes[typeString.Substring(0, index + 1) + moduleName] = typeDesc;
            });
        }

        // 全量扫描引擎与游戏的历史 metadata，最后不按模块过滤构建。
        static void LoadMetas(string root)
        {
            var manager = EngineNS.Rtti.TtClassMetaManager.Instance;
            manager.MetaRoot = root + "enginecontent/metadata";
            foreach (var metaRoot in new[] { root + "enginecontent/metadata", root + "content/metadata" })
            {
                if (!Directory.Exists(metaRoot))
                    continue;
                foreach (var typedesc in Directory.EnumerateFiles(metaRoot, "typedesc.txt", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
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
                    if (!manager.Metas.TryGetValue(typeName, out var meta))
                    {
                        meta = new EngineNS.Rtti.TtClassMeta(type);
                        manager.Metas[typeName] = meta;
                    }
                    meta.LoadClass(Path.GetDirectoryName(typedesc)!.Replace('\\', '/'));
                }
            }
            manager.BuildMeta();
            Console.WriteLine($"DataCopyer: Loaded {manager.Metas.Count} metas");
        }

        static bool WriteTextIfChanged(string file, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            if (File.Exists(file) && File.ReadAllText(file) == text)
                return false;
            File.WriteAllText(file, text, new UTF8Encoding(false));
            return true;
        }

        // 子构建显式关闭源码生成与反射阶段，并通过普通引用消费父构建的真实引擎。
        static int BuildDataCopyer(ReflectionHostOptions options)
        {
            Console.WriteLine($"DataCopyer: Build {options.DataCopyerProject}");
            var dotnet = Environment.ProcessPath;
            if (dotnet == null || !Path.GetFileNameWithoutExtension(dotnet).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host ? host : "dotnet";
            var start = new ProcessStartInfo(dotnet)
            {
                WorkingDirectory = options.TitanRoot, UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
            };
            foreach (var argument in new[]
            {
                "build", options.DataCopyerProject, "-c", options.Configuration, "--nologo",
                "-p:TitanRoot=" + options.TitanRoot + Path.DirectorySeparatorChar,
                "-p:TitanPlatform=" + options.Platform,
                "-p:TitanEngineAssemblyPath=" + options.EngineAssembly,
                "-p:OutDir=" + Path.GetDirectoryName(options.DataCopyerAssembly) + Path.DirectorySeparatorChar,
                "-p:TtOutputBasePath=" + Path.GetDirectoryName(options.DataCopyerAssembly) + Path.DirectorySeparatorChar,
                "-p:SkipTitanCodeGen=true", "-p:SkipTitanDataCopyerGen=true", "-p:HotReloadEnabled=false"
            })
                start.ArgumentList.Add(argument);
            using var process = new Process { StartInfo = start };
            process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}

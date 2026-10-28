#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;

namespace CSharpReflectionTools
{
    static class Program
    {
#if PMac
        const string CompiledPlatform = "Mac";
#elif PAndroid
        const string CompiledPlatform = "Android";
#else
        const string CompiledPlatform = "Windows";
#endif

        // 启动段只使用 BCL：先确定参数、真实引擎和 native 加载规则，再进入引用 EngineNS 的执行段。
        static int Main(string[] args)
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            ReflectionHostOptions options;
            IReadOnlyList<PluginAssembly> plugins;
            try
            {
                options = ReflectionHostOptions.Parse(args, OperatingSystem.IsMacOS() ? "Mac" : "Windows", CompiledPlatform);
                plugins = options.SelectPlugins(Console.WriteLine);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
            Console.WriteLine($"CSharpReflectionTools: Platform={options.Platform} Configuration={options.Configuration}");
            Console.WriteLine($"CSharpReflectionTools: Engine={options.EngineAssembly}");
            Console.WriteLine($"CSharpReflectionTools: Native={options.NativeLibrary}");

            try
            {
                using var gate = options.AcquireGenerationGate(TimeSpan.FromMinutes(2));
                var engine = InstallLoadRules(options, plugins);
                var loaded = new List<(PluginAssembly Plugin, Assembly Assembly)>();
                foreach (var plugin in plugins)
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(plugin.Path);
                    RegisterNativeResolver(assembly, options);
                    loaded.Add((plugin, assembly));
                }
                return Execute(options, engine, loaded);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        // 引擎和插件共用 Default 上下文：引擎名称只解析到父构建传入的 TargetPath，依赖从引擎及所选插件目录查找。
        static Assembly InstallLoadRules(ReflectionHostOptions options, IReadOnlyList<PluginAssembly> plugins)
        {
            var engine = AssemblyLoadContext.Default.LoadFromAssemblyPath(options.EngineAssembly);
            var probes = new[] { Path.GetDirectoryName(options.EngineAssembly)! }
                .Concat(plugins.Select(p => Path.GetDirectoryName(p.Path)!))
                .Distinct(StringComparer.Ordinal).ToArray();
            // 其余两个平台的引擎名一律拒绝，避免在同一个 Default 上下文混装跨平台引擎。
            var foreignEngines = new[] { "Engine.Window", "Engine.Mac", "Engine.Android" }
                .Where(name => !string.Equals(name, options.EngineAssemblyName, StringComparison.Ordinal)).ToArray();
            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                if (string.Equals(name.Name, options.EngineAssemblyName, StringComparison.Ordinal)) return engine;
                if (foreignEngines.Contains(name.Name, StringComparer.Ordinal))
                    throw new InvalidOperationException($"Platform={options.Platform} 不能加载另一平台引擎 {name.Name}");
                foreach (var directory in probes)
                {
                    var candidate = Path.Combine(directory, name.Name + ".dll");
                    if (File.Exists(candidate)) return context.LoadFromAssemblyPath(candidate);
                }
                return null;
            };
            if (OperatingSystem.IsWindows() && !SetDllDirectoryW(options.NativeDirectory))
                throw new InvalidOperationException("SetDllDirectory 失败：" + options.NativeDirectory);
            RegisterNativeResolver(engine, options);
            return engine;
        }

        static IntPtr sCoreModule;

        // P/Invoke 模块名随平台（Core.Window.dll / Core.Mac.dylib / Core.Android），物理库始终是 options.NativeLibrary。
        static void RegisterNativeResolver(Assembly assembly, ReflectionHostOptions options)
        {
            var importName = ReflectionHostOptions.ImportNameFor(options.Platform);
            NativeLibrary.SetDllImportResolver(assembly, (name, _, _) =>
            {
                if (!string.Equals(name, importName, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(name, Path.GetFileNameWithoutExtension(importName), StringComparison.OrdinalIgnoreCase))
                    return IntPtr.Zero;
                if (sCoreModule == IntPtr.Zero)
                {
                    sCoreModule = NativeLibrary.Load(options.NativeLibrary);
                    // S1 审计需要确认 Android 引擎确实由物理 Windows 库承载导入。
                    Console.WriteLine($"CSharpReflectionTools: Native loaded {options.NativeLibrary} for {importName}");
                }
                return sCoreModule;
            });
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetDllDirectoryW(string path);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static int Execute(ReflectionHostOptions options, Assembly engine, IReadOnlyList<(PluginAssembly Plugin, Assembly Assembly)> plugins)
            => DataCopyerGenerator.Run(options, engine, plugins);
    }
}

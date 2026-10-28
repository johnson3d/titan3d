using System;

namespace CppWeaving
{
    class Program
    {
        static int Main(string[] args)
        {
            try { return Execute(args); }
            catch (Exception error)
            {
                Console.Error.WriteLine($"[CppWeaving] {error}");
                return 1;
            }
        }

        static int Execute(string[] args)
        {
            var selection = PlatformProject.Resolve(args);
            Console.WriteLine($"[CppWeaving] Platform={selection.Platform} Configuration={selection.Configuration} Project={selection.Project} Pch={selection.Pch} CppOut={selection.CppOutput} CsOut={selection.CsOutput}");
            var settings = new UProjectSettings();
            //UProjectSettings.Pch = @"F:\titan3d\Core.Window\pch.h";
            //UProjectSettings.CppPODStruct = @"F:\titan3d\codegen\NativeBinder\PODStructDefine.h";
            //UProjectSettings.ModuleNC = "EngineNS.CoreSDK.CoreModule";
            //settings.CppOutputDir = @"F:\titan3d\codegen\NativeBinder";
            //settings.CsOutputDir = @"F:\titan3d\codegen\NativeBinder";
            //string vcxproj = @"F:\titan3d\Core.Window\Core.Window.vcxproj";

            var vcxproj = selection.Project;
            UProjectSettings.Pch = selection.Pch;
            UProjectSettings.CppPODStruct = FindArgument(args, "TargetCppPOD=") ?? selection.CppOutput + "/PODStructDefine.h";
            UProjectSettings.ModuleNC = FindArgument(args, "ModuleNC=") ?? "EngineNS.CoreSDK.CoreModule";
            settings.CppOutputDir = selection.CppOutput;
            settings.CsOutputDir = selection.CsOutput;
            settings.ExtInclude = FindArgument(args, "ExtInclude=");
            // Platform=Windows|Mac|Android（缺省取宿主）；Mac/Android 档由对应平台的 GenerateCodegen.cmake 传入 SysRoot/ResourceDir/Defines
            settings.Profile.Platform = selection.Platform;
            settings.Profile.SysRoot = FindArgument(args, "SysRoot=");
            settings.Profile.ResourceDir = FindArgument(args, "ResourceDir=");
            var defines = FindArgument(args, "Defines=");
            var allowParseErrors = FindArgument(args, "AllowParseErrors=") == "1";
            Cpp2CS.USysClassManager.Init(settings.Profile.Platform);
            //settings.ExtInclude = "C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\um;C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\shared;C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\winrt;C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\cppwinrt;C:\\Program Files (x86)\\Windows Kits\\NETFXSDK\\4.8\\Include\\um;E:\\titanengine\\binaries\\";
            if (settings.ExtInclude != null)
            {
                var segs = settings.ExtInclude.Split(';');
                foreach(var i in segs)
                {
                    settings.Includes.Add(i.Replace("\\", "/"));
                }
            }

            //// 加载工程文件
            //var project = new Project(vcxproj);

            //// 获取属性值
            //var sdkIncludePath = project.GetPropertyValue("WindowsSDK_IncludePath");
            //var sdkLibPath = project.GetPropertyValue("WindowsSDK_LibraryPath");

            HppCollector.Instance.Reset();
            HppCollector.Instance.Collect(vcxproj, selection.BuildCondition, settings.Profile.Platform);

            foreach(var i in HppCollector.Instance.Headers)
            {
                settings.ParseSources.Add(i.Value);
            }
            foreach (var i in HppCollector.Instance.IncludePath)
            {
                settings.Includes.Add(i);
            }
            foreach (var i in HppCollector.Instance.MacroDefines)
            {
                settings.MacroDefines.Add(i);
            }
            if (settings.Profile.Platform == TargetPlatform.Windows)
            {
                // libclang 解析的是 MSVC STL 头；允许已验证的旧版 libclang 跳过新版 STL 的版本门禁。
                settings.MacroDefines.Add("_ALLOW_COMPILER_AND_STL_VERSION_MISMATCH");
            }
            if (!string.IsNullOrEmpty(defines))
            {
                foreach (var i in defines.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    settings.MacroDefines.Add(i);
                }
            }
            settings.MacroDefines.Add("__REF_CLANG__");

            System.IO.Directory.CreateDirectory(settings.CppOutputDir);
            System.IO.Directory.CreateDirectory(settings.CsOutputDir);
            ParseFailures.Clear();
            Cpp2CS.UTypeManager.Instance.ClangAnalyse(settings, allowParseErrors);
            return ParseFailures.Report(ParseFailures.Snapshot(), allowParseErrors, Console.Out);
        }

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
    }
}

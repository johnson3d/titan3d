using System;

namespace CppWeaving
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new UProjectSettings();
            //UProjectSettings.Pch = @"F:\titan3d\Core.Window\pch.h";
            //UProjectSettings.CppPODStruct = @"F:\titan3d\codegen\NativeBinder\PODStructDefine.h";
            //UProjectSettings.ModuleNC = "EngineNS.CoreSDK.CoreModule";
            //settings.CppOutputDir = @"F:\titan3d\codegen\NativeBinder";
            //settings.CsOutputDir = @"F:\titan3d\codegen\NativeBinder";
            //string vcxproj = @"F:\titan3d\Core.Window\Core.Window.vcxproj";

            var vcxproj = FindArgument(args, "vcxproj=");
            UProjectSettings.Pch = FindArgument(args, "Pch=");
            UProjectSettings.CppPODStruct = FindArgument(args, "TargetCppPOD=");
            UProjectSettings.ModuleNC = FindArgument(args, "ModuleNC=");
            settings.CppOutputDir = FindArgument(args, "CppOut=");
            settings.CsOutputDir = FindArgument(args, "CsOut=");
            settings.ExtInclude = FindArgument(args, "ExtInclude=");
            //settings.ExtInclude = "C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\um;C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\shared;C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\winrt;C:\\Program Files (x86)\\Windows Kits\\10\\Include\\10.0.22000.0\\cppwinrt;C:\\Program Files (x86)\\Windows Kits\\NETFXSDK\\4.8\\Include\\um;E:\\titanengine\\binaries\\";
            if (settings.ExtInclude != null)
            {
                var segs = settings.ExtInclude.Split(';');
                foreach(var i in segs)
                {
                    settings.Includes.Add(i.Replace("\\", "/"));
                }
            }

            HppCollector.Instance.Reset();
            HppCollector.Instance.Collect(vcxproj, @"'$(Configuration)|$(Platform)'=='Debug|x64'");

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
            settings.MacroDefines.Add("__REF_CLANG__");

            Cpp2CS.UTypeManager.Instance.ClangAnalyse(settings);
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

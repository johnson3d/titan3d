using System;

namespace CppWeaving
{
    class Program
    {
        static void Main(string[] args)
        {
            var mHppCollector = new HppCollector();
            mHppCollector.Collect(@"F:\titan3d\Core.Window\Core.Window.vcxproj", @"'$(Configuration)|$(Platform)'=='Debug|x64'");

            var settings = new UProjectSettings();

            foreach(var i in mHppCollector.Headers)
            {
                settings.ParseSources.Add(i.Value);
            }
            foreach (var i in mHppCollector.IncludePath)
            {
                settings.Includes.Add(i);
            }
            foreach (var i in mHppCollector.MacroDefines)
            {
                settings.MacroDefines.Add(i);
            }
            settings.MacroDefines.Add("__REF_CLANG__");

            UProjectSettings.Pch = @"F:\titan3d\Core.Window\pch.h";
            UProjectSettings.CppPODStruct = @"F:\titan3d\codegen\NativeBinder\PODStructDefine.h";
            UProjectSettings.ModuleNC = "EngineNS.CoreSDK.CoreModule";
            settings.CppOutputDir = @"F:\titan3d\codegen\NativeBinder";
            settings.CsOutputDir = @"F:\titan3d\codegen\NativeBinder";
            
            Cpp2CS.UTypeManager.Instance.ClangAnalyse(settings);
        }
    }
}

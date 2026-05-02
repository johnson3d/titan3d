// See https://aka.ms/new-console-template for more information

using EngineNS;
using Microsoft.Build.Logging;
using System.CodeDom.Compiler;
using System.Xml.Linq;
using static Org.BouncyCastle.Math.EC.ECCurve;

try
{
    var mBin = System.IO.Directory.GetCurrentDirectory();
    EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/debug");
    if (IntPtr.Zero == EngineNS.TtNativeWindow.LoadLibraryA("Core.Window.dll"))
    {
        EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/release");
        if (IntPtr.Zero == EngineNS.TtNativeWindow.LoadLibraryA("Core.Window.dll"))
        {
            EngineNS.TtNativeWindow.MessageBoxA(IntPtr.Zero, "Core.Window.dll load failed", "InitEngine", 0);
            return;
        }
    }

    var enginesln = args[0];
    var projectFile = args[1];
    var csFilesPath = args[2];
    var dotnet_ver = args[3];
    var cfgFile = args[4];
    
    var projectPath = EngineNS.IO.TtFileManager.GetBaseDirectory(projectFile, 1);
    var projName = EngineNS.IO.TtFileManager.GetPureName(projectFile);
    var assemblyFile = enginesln + $"binaries\\{dotnet_ver}\\" + projName + ".dll";

    try
    {
        EngineNS.TtEngine.InitForGameBuilder(new EngineNS.TtEngine(args), cfgFile, false);
    }
    catch (Exception ex)
    {
        System.Console.WriteLine($"try debug Core.Window.dll:{ex}");
        EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/debug");
        EngineNS.TtEngine.InitForGameBuilder(new EngineNS.TtEngine(args), cfgFile, false);
    }

    System.Console.WriteLine($"engine sln dir: {enginesln}");
    System.Console.WriteLine($"arg project file: {projectFile}");
    System.Console.WriteLine($"cs files path: {csFilesPath}");
    System.Console.WriteLine($"dotnet ver: {dotnet_ver}");
    System.Console.WriteLine($"config file: {cfgFile}");
    System.Console.WriteLine($"TitanEngine GameBuilder: {assemblyFile}");

    EngineNS.Macross.TtMacrossModule.CompileGameProject(csFilesPath, projectFile, assemblyFile, TtEngine.Instance.CurrentPlatform);
}
catch(System.Exception e)
{
    Console.WriteLine(e.ToString());
}
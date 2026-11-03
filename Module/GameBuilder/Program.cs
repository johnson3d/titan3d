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

    var compiled = EngineNS.Macross.TtMacrossModule.CompileGameProject(csFilesPath, projectFile, assemblyFile, TtEngine.Instance.CurrentPlatform, enableMirror: true);

    // 引擎 native 后台线程（VParallelTaskManager 任务池 / F2M / 内存流 / 日志）仍持有托管回调。
    // Main 正常 return 和 Environment.Exit 都会走 CLR 关闭序列，关闭期间 native 线程再反向进入托管代码会触发
    // "Attempt to execute managed code after the .NET runtime thread state has been destroyed" 断言，
    // 以 FailFast(0xC0000602) 结束进程：编译其实已成功，MSBuild 却报 MSB3073。
    // 先按 TtEngine.FinalCleanup 的尾部顺序摘掉这些 native->托管 入口，再硬退出，完全不进 CLR 关闭序列。
    try
    {
        EngineNS.VParallelTaskManager.SetFunction(null);
        EngineNS.IO.TtMemWriter.FinalNativeCallback();
        EngineNS.IO.TtMemReader.FinalNativeCallback();
        EngineNS.CoreSDK.FinalF2MManager();
    }
    catch (System.Exception cleanEx)
    {
        System.Console.WriteLine($"GameBuilder cleanup: {cleanEx.Message}");
    }

    if (!compiled)
        System.Console.WriteLine("GameBuilder: CompileGameProject failed.");

    GameBuilder.NativeExit.Now(compiled ? 0 : 1);
}
catch(System.Exception e)
{
    Console.WriteLine(e.ToString());
    GameBuilder.NativeExit.Now(1);
}
// See https://aka.ms/new-console-template for more information

using EngineNS;
using System.CodeDom.Compiler;
using System.Xml.Linq;
using static Org.BouncyCastle.Math.EC.ECCurve;

try
{
    var enginesln = args[0];
    var projectFile = args[1];
    var csFilesPath = args[2];
    var dotnet_ver = args[3];
    var cfgFile = args[4];
    
    var projectPath = EngineNS.IO.TtFileManager.GetBaseDirectory(projectFile, 1);
    var projName = EngineNS.IO.TtFileManager.GetPureName(projectFile);
    var assemblyFile = enginesln + $"binaries\\{dotnet_ver}\\" + projName + ".dll";

    EngineNS.TtEngine.OnlyInitTypes(new EngineNS.TtEngine(args), cfgFile);

    System.Console.WriteLine($"engine sln dir: {enginesln}");
    System.Console.WriteLine($"arg project file: {projectFile}");
    System.Console.WriteLine($"cs files path: {csFilesPath}");
    System.Console.WriteLine($"dotnet ver: {dotnet_ver}");
    System.Console.WriteLine($"config file: {cfgFile}");
    System.Console.WriteLine($"TitanEngine GameBuilder: {assemblyFile}");

    EngineNS.Macross.TtMacrossModule.CompileGameProject(csFilesPath, projectFile, assemblyFile);
}
catch(System.Exception e)
{
    Console.WriteLine(e.ToString());
}
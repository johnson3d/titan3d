using Microsoft.Build.Execution;
using Microsoft.Build.Logging;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace GameBuilder
{
    class Program
    {
        //static string mMSBuildAbsFileName = null;
        //public static string MSBuildAbsFileName
        //{
        //    get
        //    {
        //        if (mMSBuildAbsFileName == null)
        //        {
        //            mMSBuildAbsFileName = "";
        //            // 从注册表获取MSBuild位置
        //            var softwareKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE");
        //            if (softwareKey == null)
        //                return mMSBuildAbsFileName;
        //            var microsoftKey = softwareKey.OpenSubKey("Microsoft");
        //            if (microsoftKey == null)
        //                return mMSBuildAbsFileName;
        //            var msBuildKey = microsoftKey.OpenSubKey("MSBuild");
        //            if (msBuildKey == null)
        //                return mMSBuildAbsFileName;
        //            var toolVers = msBuildKey.OpenSubKey("ToolsVersions");
        //            if (toolVers == null)
        //                return mMSBuildAbsFileName;
        //            var keys = toolVers.GetSubKeyNames();
        //            if (keys.Length > 0)
        //            {
        //                var maxLen = 0;
        //                List<int[]> vers = new List<int[]>();
        //                for (int i = 0; i < keys.Length; i++)
        //                {
        //                    var key = keys[i];
        //                    var nums = key.Split('.');
        //                    var ary = new int[nums.Length];
        //                    for (int numIdx = 0; numIdx < nums.Length; numIdx++)
        //                    {
        //                        ary[numIdx] = System.Convert.ToInt32(nums[numIdx]);
        //                    }
        //                    vers.Add(ary);
        //                    if (ary.Length > maxLen)
        //                        maxLen = ary.Length;
        //                }

        //                for (int i = 0; i < maxLen; i++)
        //                {
        //                    int maxVer = 0;
        //                    for (int keyIdx = vers.Count - 1; keyIdx >= 0; keyIdx--)
        //                    {
        //                        var ver = vers[keyIdx];
        //                        if (ver.Length <= i)
        //                            vers.Remove(ver);
        //                        else if (ver[i] > maxVer)
        //                        {
        //                            maxVer = ver[i];
        //                        }
        //                        else
        //                            vers.Remove(ver);
        //                    }

        //                    if (vers.Count == 1)
        //                        break;
        //                }

        //                string curVer = "";
        //                foreach (var ver in vers[0])
        //                {
        //                    curVer += ver + ".";
        //                }
        //                curVer = curVer.TrimEnd('.');

        //                var verKey = toolVers.OpenSubKey(curVer);
        //                var path = verKey.GetValue("MSBuildToolsPath").ToString().Replace("/", "\\").TrimEnd('\\') + "\\";
        //                mMSBuildAbsFileName = path + "MSBuild.exe";
        //            }
        //        }

        //        return mMSBuildAbsFileName;
        //    }
        //}


        // 包含文件分析
        static void AnalizeCompileFile(string projectAbsPath, XmlNode root, StringCollection absFiles, XmlNamespaceManager nsMgr)
        {
            var compileNodes = root.SelectNodes("descendant::xlns:ItemGroup/xlns:Compile[@Include]", nsMgr);
            foreach (XmlNode node in compileNodes)
            {
                var text = node.Attributes["Include"].Value;
                string csFile;
                if (text[0] == '$')
                {
                    if (text.Contains("$(MSBuildThisFileDirectory)"))
                    {
                        csFile = projectAbsPath + text.Replace("$(MSBuildThisFileDirectory)", "");
                    }
                    else
                    {
                        throw new InvalidOperationException($"无法解析文件{text}");
                    }
                }
                else
                    csFile = projectAbsPath + text;

                absFiles.Add(csFile);
            }
        }

        // 参数说明 //////////////
        // 1. 工程文件名（全路径）
        // 2. Debug/Release
        // 3. Platform
        // 4. MSBuild filename(全路径)
        //////////////////////////
        static void Main(string[] args)
        {
            try
            {
                var projFileName = args[0];
                var configuration = args[1];
                var platform = args[2].Replace(" ", "");
                string msBuildFileName = "";
                for(int i=3; i<args.Length; i++)
                {
                    msBuildFileName += args[i] + " ";
                }

                string lastAssemblyFileName = "";
                var gamebuildDataFile = Application.StartupPath + "\\GameBuilder.data";
                if(System.IO.File.Exists(gamebuildDataFile))
                {
                    using (var r = new System.IO.StreamReader(gamebuildDataFile))
                    {
                        lastAssemblyFileName = r.ReadLine();
                    }
                }

                var fileInfo = new System.IO.FileInfo(projFileName);

                // 清除输出目录里同名dll
                var xml = new XmlDocument();
                xml.Load(projFileName);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("xlns", "http://schemas.microsoft.com/developer/msbuild/2003");
                var root = xml.DocumentElement;

                var toolsVersion = root.GetAttribute("ToolsVersion");

                var assemblyNode = root.SelectSingleNode("descendant::xlns:PropertyGroup/xlns:AssemblyName", nsmgr);
                var assemblyName = assemblyNode.InnerText;
                var propertyGroup = root.SelectSingleNode($"descendant::xlns:PropertyGroup[@Condition=\" '$(Configuration)|$(Platform)' == '{configuration}|{platform}' \"]", nsmgr);
                if (propertyGroup == null)
                {
                    Console.WriteLine($"生成失败, 工程未设置 {configuration}|{platform} 的编译参数!");
                    return;
                }
                string outputPath = null;
                var outputPathElement = propertyGroup.SelectSingleNode("xlns:OutputPath", nsmgr);
                if (outputPathElement != null)
                    outputPath = outputPathElement.InnerText;
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = "obj\\" + configuration;
                }
                outputPath = outputPath.Replace("/", "\\").TrimEnd('\\');
                var absPath = fileInfo.Directory.FullName + "\\" + outputPath + "\\";
                var absLastAssemblyFileName = absPath + lastAssemblyFileName;
                var files = System.IO.Directory.GetFiles(absPath, assemblyName + "*.dll");
                foreach(var file in files)
                {
                    try
                    {
                        // 保留上次生成的dll文件
                        if (string.Equals(file, absLastAssemblyFileName + ".dll", StringComparison.OrdinalIgnoreCase))
                            continue;
                        System.IO.File.Delete(file);
                    }
                    catch(System.Exception)
                    {

                    }
                }
                files = System.IO.Directory.GetFiles(absPath, assemblyName + "*.pdb");
                foreach(var file in files)
                {
                    try
                    {
                        // 保留上次生成的dll文件
                        if (string.Equals(file, absLastAssemblyFileName + ".pdb", StringComparison.OrdinalIgnoreCase))
                            continue;
                        System.IO.File.Delete(file);
                    }
                    catch(System.Exception)
                    {

                    }
                }
                files = System.IO.Directory.GetFiles(absPath, "dcf_*.dbg");
                foreach (var file in files)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (System.Exception)
                    {

                    }
                }

                // 删除工程文件
                files = System.IO.Directory.GetFiles(fileInfo.Directory.FullName, fileInfo.Name.Replace(fileInfo.Extension, "") + "_*" + fileInfo.Extension);
                foreach (var file in files)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (System.Exception)
                    {

                    }
                }

                // 复制工程文件并更改dll名称
                var newIdStr = Guid.NewGuid().ToString().Replace("-", "_");
                var newProjName = fileInfo.Directory.FullName + "\\" + fileInfo.Name.Replace(fileInfo.Extension, "") + "_" + newIdStr + fileInfo.Extension;
                var newDllName = assemblyName + "_" + newIdStr;
                assemblyNode.InnerText = newDllName;
                root.Attributes.RemoveNamedItem("DefaultTargets");
                root.RemoveChild(root.SelectSingleNode("descendant::xlns:Target[@Name=\"Build\"]", nsmgr));
                xml.Save(newProjName);

                lastAssemblyFileName = newDllName;

                Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
                CallMSBuild(newProjName, configuration, platform);

                //var p = new Process();
                //p.EnableRaisingEvents = true;
                //p.StartInfo.FileName = "cmd.exe";
                //p.StartInfo.UseShellExecute = false;
                //p.StartInfo.RedirectStandardInput = true;
                //p.StartInfo.RedirectStandardOutput = true;
                //p.StartInfo.RedirectStandardError = true;
                //p.StartInfo.CreateNoWindow = true;
                //p.Start();
                //p.StandardInput.WriteLine($"\"{msBuildFileName}\" \"{newProjName}\" /p:Configuration={configuration} /p:Platform={platform}" + " &exit");
                //p.StandardInput.AutoFlush = true;
                //Console.WriteLine(p.StandardOutput.ReadToEnd());
                //Console.WriteLine(p.StandardError.ReadToEnd());
                //p.WaitForExit();
                //p.Close();

                //System.IO.File.Delete(newProjName);

                var newDllFullName = absPath + newDllName + ".dll";
                if(System.IO.File.Exists(newDllFullName))
                {
                    using(var w = new System.IO.StreamWriter(gamebuildDataFile))
                    {
                        w.WriteLine(lastAssemblyFileName);
                    }
                }
                System.IO.File.Copy(newDllFullName, absPath + assemblyName + ".dll", true);
                System.IO.File.Copy(absPath + newDllName + ".pdb", absPath + assemblyName + ".pdb", true);

                System.IO.File.WriteAllText(absPath + $"dcf_{newIdStr}.dbg", $"{newIdStr}");

                /*StringCollection absScripFiles = new StringCollection();
                StringCollection refAssemblys = new StringCollection();


                // 引用分析
                var refNodes = root.SelectNodes("descendant::xlns:ItemGroup/xlns:Reference[@Include]", nsmgr);
                foreach (XmlNode node in refNodes)
                {
                    string dllFile;
                    var pathNode = node.SelectSingleNode("descendant::xlns:HintPath", nsmgr);
                    if (pathNode != null)
                        dllFile = fileInfo.Directory.FullName + "\\" + pathNode.InnerText;
                    else
                    {
                        dllFile = node.Attributes["Include"].Value;
                        var idx = dllFile.LastIndexOf('.');
                        if (idx < 0)
                            dllFile += ".dll";
                        else
                        {
                            var ext = dllFile.Substring(idx);
                            if (ext != ".dll")
                                dllFile += ".dll";
                        }
                    }
                    refAssemblys.Add(dllFile);
                }
                // 工程引用
                var projRefNodes = root.SelectNodes("descendant::xlns:ItemGroup/xlns:ProjectReference[@Include]", nsmgr);
                if(projRefNodes.Count > 0)
                {
                    throw new InvalidOperationException("不支持使用工程引用，请直接引用编译好的dll文件");
                }
                //foreach(XmlNode node in projRefNodes)
                //{
                //    var projRefFile = fileInfo.Directory.FullName + "\\" + node.Attributes["Include"].Value;
                //}
                // 包含文件分析
                AnalizeCompileFile(fileInfo.Directory.FullName + "\\", root, absScripFiles, nsmgr);
                // 共享工程分析
                var importNodes = root.SelectNodes("descendant::xlns:Import[@Label=\"Shared\"]", nsmgr);
                foreach(XmlNode node in importNodes)
                {
                    var spProjFile = fileInfo.Directory.FullName + "\\" + node.Attributes["Project"].Value;
                    var spFileInfo = new System.IO.FileInfo(spProjFile);
                    var spXml = new XmlDocument();
                    spXml.Load(spProjFile);
                    var spNSMgr = new XmlNamespaceManager(spXml.NameTable);
                    spNSMgr.AddNamespace("xlns", "http://schemas.microsoft.com/developer/msbuild/2003");
                    AnalizeCompileFile(spFileInfo.Directory.FullName + "\\", spXml.DocumentElement, absScripFiles, spNSMgr);
                }

                // 编译选项
                var parameters = new CompilerParameters()
                {
                    IncludeDebugInformation = true,
                    GenerateInMemory = false,
                };

                var refArray = new string[refAssemblys.Count];
                refAssemblys.CopyTo(refArray, 0);
                parameters.ReferencedAssemblies.AddRange(refArray);

                // 分析工程编译设置
                var assemblyNode = root.SelectSingleNode("descendant::xlns:PropertyGroup/xlns:AssemblyName", nsmgr);
                var assemblyName = assemblyNode.InnerText;
                var propertyGroup = root.SelectSingleNode($"descendant::xlns:PropertyGroup[@Condition=\" '$(Configuration)|$(Platform)' == '{configuration}|{platform}' \"]", nsmgr);
                if (propertyGroup == null)
                {
                    Console.WriteLine($"生成失败, 工程未设置 {configuration}|{platform} 的编译参数!");
                    return;
                }
                string outputPath = null;
                var outputPathElement = propertyGroup.SelectSingleNode("xlns:OutputPath", nsmgr);
                if(outputPathElement != null)
                    outputPath = outputPathElement.InnerText;
                if(string.IsNullOrEmpty(outputPath))
                {
                    parameters.OutputAssembly = fileInfo.Directory.FullName + "\\obj\\" + configuration + "\\" + assemblyName + "_" + Guid.NewGuid().ToString().Replace("-", "_") + ".dll";
                }
                else
                {
                    parameters.OutputAssembly = fileInfo.Directory.FullName + "\\" + outputPath.Replace("/", "\\").TrimEnd('\\') + "\\" + assemblyName + "_" + Guid.NewGuid().ToString().Replace("-", "_") + ".dll";
                }
                var warningLevelElement = propertyGroup.SelectSingleNode("xlns:WarningLevel", nsmgr);
                if (warningLevelElement != null)
                {
                    parameters.WarningLevel = System.Convert.ToInt32(warningLevelElement.InnerText);
                }
                var unsafeOptElement = propertyGroup.SelectSingleNode("xlns:AllowUnsafeBlocks", nsmgr);
                if(unsafeOptElement != null)
                {
                    var unsafeOpt = System.Convert.ToBoolean(unsafeOptElement.InnerText);
                    if(unsafeOpt)
                        parameters.CompilerOptions += "/unsafe";
                }
                var compilerOptionsElement = propertyGroup.SelectSingleNode("xlns:DefineConstants", nsmgr);
                if(compilerOptionsElement != null)
                {
                    var compilerOptions = compilerOptionsElement.InnerText;
                    var opts = compilerOptions.Split(';');
                    foreach (var opt in opts)
                    {
                        parameters.CompilerOptions += " /define:" + opt;
                    }
                }
                var optimizeOptElement = propertyGroup.SelectSingleNode("xlns:Optimize", nsmgr);
                if(optimizeOptElement != null)
                {
                    var optimizeOpt = System.Convert.ToBoolean(optimizeOptElement.InnerText);
                    if (optimizeOpt)
                        parameters.CompilerOptions += " /optimize";
                }
                var treatWarningsAsErrorsOptElement = propertyGroup.SelectSingleNode("xlns:TreatWarningsAsErrors", nsmgr);
                if(treatWarningsAsErrorsOptElement != null)
                {
                    var treatWarningsAsErrorsOpt = System.Convert.ToBoolean(treatWarningsAsErrorsOptElement.InnerText);
                    parameters.TreatWarningsAsErrors = treatWarningsAsErrorsOpt;
                }
                var warningsAsErrorsOptElement = propertyGroup.SelectSingleNode("xlns:WarningsAsErrors", nsmgr);
                if(warningsAsErrorsOptElement != null)
                {
                    parameters.CompilerOptions += " /warnaserror:" + warningsAsErrorsOptElement.InnerText;
                }
                var debugTypeOptElement = propertyGroup.SelectSingleNode("xlns:DebugType", nsmgr);
                if(debugTypeOptElement != null)
                {
                    parameters.CompilerOptions += " /debug:" + debugTypeOptElement.InnerText;
                }
                var noWarnOptElement = propertyGroup.SelectSingleNode("xlns:NoWarn", nsmgr);
                if(noWarnOptElement != null)
                {
                    parameters.CompilerOptions += " /nowarn:" + noWarnOptElement.InnerText;

                }
                var errorReportOptElement = propertyGroup.SelectSingleNode("xlns:ErrorReport", nsmgr);
                if(errorReportOptElement != null)
                {
                    parameters.CompilerOptions += " /errorreport:" + errorReportOptElement.InnerText;
                }
                parameters.CompilerOptions += " /platform:" + platform;

                // 编译
                var compiler = new CodeGenerateSystem.CSharpCodeProvider();
                var filesArray = new string[absScripFiles.Count];
                absScripFiles.CopyTo(filesArray, 0);
                var result = compiler.CompileAssemblyFromFile(parameters, filesArray);
                foreach(var str in result.Output)
                {
                    Console.WriteLine(str.ToString());
                }
                foreach(var error in result.Errors)
                {
                    Console.WriteLine(error);
                }*/
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void CallMSBuild(string newProjName, string configuration, string platform)
        {
            var pc = new Microsoft.Build.Evaluation.ProjectCollection();
            var param = new BuildParameters(pc);
            param.Loggers = new[]
            {
                    new ConsoleLogger()
                    {
                        ShowSummary = true,
                        Verbosity = Microsoft.Build.Framework.LoggerVerbosity.Minimal,
                        SkipProjectStartedText = true,
                    }
                };
            var pros = new Dictionary<string, string>();
            pros["Platform"] = platform;
            pros["Configuration"] = configuration;
            var request = new BuildRequestData(newProjName, pros, null, new string[] { "Build" }, null);
            var result = BuildManager.DefaultBuildManager.Build(param, request);
        }
    }
}

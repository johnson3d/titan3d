using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving
{
    public class HppCollector
    {
        public static HppCollector Instance = new HppCollector();
        public void Reset()
        {
            Headers.Clear();
            IncludePath.Clear();
            MacroDefines.Clear();
        }
        public class HppUnit
        {
            public string File;
            public string Module;
        }
        public Dictionary<string, HppUnit> Headers = new Dictionary<string, HppUnit>();
        public HppUnit FindHpp(string file)
        {
            bool error;
            file = UTypeManagerBase.NormalizePath(file, out error);
            if (string.IsNullOrEmpty(file))
                return null;
            if (OperatingSystem.IsWindows())
            {
                // Windows 上 clang 会把 #include 里的反斜杠与 .. 原样拼进文件名，先规范成绝对路径再查表。
                file = Path.GetFullPath(file).Replace('\\', '/');
            }
            file = file.ToLower();
            lock (this)
            {
                HppUnit result;
                if (Headers.TryGetValue(file, out result))
                {
                    return result;
                }
                return null;
            }
        }

        public List<string> IncludePath = new List<string>();
        public List<string> MacroDefines = new List<string>();
        public void AddExtraIncludePath(string cfg)
        {
            var text = System.IO.File.ReadAllText(cfg);
            var segs = text.Split('\n');
            foreach(var i in segs)
            {
                if (string.IsNullOrEmpty(i))
                    continue;
                IncludePath.Add(i);
            }
        }
        public void AddExtraMacroDefine(string cfg)
        {
            var text = System.IO.File.ReadAllText(cfg);
            var segs = text.Split('\n');
            foreach (var i in segs)
            {
                if (string.IsNullOrEmpty(i))
                    continue;
                MacroDefines.Add(i);
            }
        }
        public void Collect(string proj, string buildConfig = @"'$(Configuration)|$(Platform)'=='Debug|x64'", TargetPlatform platform = TargetPlatform.Windows)
        {
            proj = proj.Replace("\\", "/");
            var path = proj.Substring(0, proj.LastIndexOf("/") + 1);

            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(proj);
            var root = myXmlDoc.DocumentElement;
            var portable = platform != TargetPlatform.Windows;
            var platformMacro = platform == TargetPlatform.Android ? "PLATFORM_DROID" : "PLATFORM_MAC";
            var platformProject = portable && root.GetElementsByTagName("PreprocessorDefinitions")
                .Cast<System.Xml.XmlNode>().Any(n => n.InnerText.Split(';').Any(d => d.Trim() == platformMacro));

            System.Xml.XmlNode sharedNode = null;
            bool foundConfiguration = false;
            foreach (System.Xml.XmlNode node in root.ChildNodes)
            {
                if (node.Name == "ImportGroup" && node.Attributes["Label"] != null && node.Attributes["Label"].Value == "Shared")
                {
                    sharedNode = node;
                }
                else if (node.Name == "ItemDefinitionGroup")
                {
                    var attr = node.Attributes["Condition"];
                    if (attr == null && !portable)
                        continue;

                    if (portable ? !MatchesPortableCondition(attr?.InnerText, buildConfig) : attr.InnerText != buildConfig)
                    {
                        continue;
                    }

                    if (attr != null) foundConfiguration = true;
                    var clCompile = FindChild(node.ChildNodes, "ClCompile");
                    if (clCompile != null)
                    {
                        if (portable)
                        {
                            CollectPortableCompileMetadata(clCompile, proj, buildConfig, platformProject, platform);
                            continue;
                        }
                        var cur = FindChild(clCompile.ChildNodes, "AdditionalIncludeDirectories");
                        if (cur != null)
                        {
                            var segs = cur.InnerText.Split(';');
                            foreach (var i in segs)
                            {
                                if (i.StartsWith("%(AdditionalIncludeDirectories)"))
                                    continue;
                                if (ClangArgs.IsHostOnlyInclude(platform, i))
                                    continue;
                                if (i=="$(WindowsSDK_IncludePath)" && OperatingSystem.IsWindows())
                                {
                                    var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Kits\Installed Roots");
                                    var kitsRoot10 = key?.GetValue("KitsRoot10") as string;

                                    if (!string.IsNullOrEmpty(kitsRoot10))
                                    {
                                        var latestVersion = Directory.GetDirectories(Path.Combine(kitsRoot10, "Include"))
                                                                     .OrderByDescending(v => v)  // 选最新版本
                                                                     .First();

                                        var ucrt = Path.Combine(latestVersion, "ucrt");
                                        var shared = Path.Combine(latestVersion, "shared");
                                        var um = Path.Combine(latestVersion, "um");

                                        IncludePath.Add(ucrt);
                                        IncludePath.Add(shared);
                                        IncludePath.Add(um);
                                    }
                                }
                                if (System.IO.Path.IsPathRooted(i))
                                    IncludePath.Add(i);
                                else
                                    IncludePath.Add(path + i);
                            }
                        }
                        cur = FindChild(clCompile.ChildNodes, "PreprocessorDefinitions");
                        if (cur != null)
                        {
                            var segs = cur.InnerText.Split(';');
                            foreach (var i in segs)
                            {
                                if (string.IsNullOrWhiteSpace(i) || i.StartsWith("%(PreprocessorDefinitions)"))
                                    continue;
                                MacroDefines.Add(i);
                            }
                        }
                    }
                }
            }
            if (!foundConfiguration)
                throw new InvalidDataException($"工程 {proj} 不包含配置 {buildConfig}");
            if (sharedNode == null)
                throw new InvalidDataException($"工程 {proj} 没有 Shared ImportGroup");

            Headers.Clear();
            foreach (System.Xml.XmlNode node in sharedNode.ChildNodes)
            {
                if (node.Name == "Import")
                {
                    var attr = node.Attributes["Project"];
                    if (attr == null)
                        continue;

                    var sharedProjFile = attr.Value.Replace('\\', '/');
                    // 生成胶水不是声明输入，首次生成前允许不存在，并避免重复解析生成输出。
                    if (Path.GetFileName(sharedProjFile).Equals("CodeGen.vcxitems", StringComparison.OrdinalIgnoreCase))
                        continue;
                    sharedProjFile = Path.GetFullPath(Path.Combine(path, sharedProjFile)).Replace('\\', '/');
                    if (!File.Exists(sharedProjFile))
                        throw new FileNotFoundException($"工程 {proj} 的 Shared Import 不存在", sharedProjFile);

                    bool error;
                    sharedProjFile = NormalizePath(sharedProjFile, out error);
                    if (error)
                    {
                        Console.WriteLine($"header file {sharedProjFile} is error path");
                    }

                    System.Xml.XmlDocument sharedProjXmlDoc = new System.Xml.XmlDocument();
                    sharedProjXmlDoc.Load(sharedProjFile);
                    //var spjRoot = sharedProjXmlDoc.SelectSingleNode("Project");
                    var spjRoot = sharedProjXmlDoc.DocumentElement;
                    if (portable)
                        CollectSharedMetadata(spjRoot, sharedProjFile, buildConfig, platform);
                    foreach (System.Xml.XmlNode sn in spjRoot.ChildNodes)
                    {
                        if (sn.Name == "ItemGroup")
                        {
                            CollectInclude(sn, sharedProjFile, Headers);
                        }
                    }
                }
            }
        }
        void CollectSharedMetadata(System.Xml.XmlNode root, string file, string buildConfig, TargetPlatform platform)
        {
            foreach (System.Xml.XmlNode group in root.ChildNodes)
            {
                if (group.Name != "ItemDefinitionGroup") continue;
                var condition = group.Attributes?["Condition"]?.Value;
                if (!MatchesPortableCondition(condition, buildConfig)) continue;
                var compile = FindChild(group.ChildNodes, "ClCompile");
                if (compile == null) continue;
                CollectPortableCompileMetadata(compile, file, buildConfig, true, platform);
            }
        }

        void CollectPortableCompileMetadata(System.Xml.XmlNode compile, string file, string buildConfig, bool definesEnabled, TargetPlatform platform)
        {
            var baseDir = Path.GetDirectoryName(Path.GetFullPath(file)).Replace('\\', '/') + "/";
            if (!MatchesPortableCondition(compile.Attributes?["Condition"]?.Value, buildConfig)) return;
            foreach (System.Xml.XmlNode element in compile.ChildNodes)
            {
                var isPath = element.Name == "AdditionalIncludeDirectories";
                if (!isPath && element.Name != "PreprocessorDefinitions") continue;
                if (!MatchesPortableCondition(element.Attributes?["Condition"]?.Value, buildConfig)) continue;
                // 显式读取 Windows 工程的旧诊断调用仍过滤其工程宏。
                if (!isPath && !definesEnabled) continue;
                foreach (var raw in element.InnerText.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var value = raw.Trim();
                    if (value.Length == 0 || value.StartsWith("%(")) continue;
                    value = value.Replace("$(MSBuildThisFileDirectory)", baseDir);
                    if (isPath && ClangArgs.IsHostOnlyInclude(platform, value)) continue;
                    if (value.Contains("$(")) throw new InvalidDataException($"工程 {file} 的未解析属性: {value}");
                    if (isPath)
                    {
                        value = Path.GetFullPath(Path.Combine(baseDir, value.Replace('\\', '/'))).Replace('\\', '/');
                        if (!IncludePath.Contains(value)) IncludePath.Add(value);
                    }
                    else if (!MacroDefines.Contains(value)) MacroDefines.Add(value);
                }
            }
        }

        static bool MatchesPortableCondition(string condition, string buildConfig)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;
            condition = System.Text.RegularExpressions.Regex.Replace(condition, @"\s", "");
            var selected = System.Text.RegularExpressions.Regex.Match(buildConfig, @"=='(Debug|Release)\|[^']+'$");
            if (!selected.Success) throw new InvalidDataException($"不支持配置: {buildConfig}");
            var config = selected.Groups[1].Value;
            if (condition == $"'$(Configuration)'=='{config}'" || condition == System.Text.RegularExpressions.Regex.Replace(buildConfig, @"\s", "")) return true;
            if (System.Text.RegularExpressions.Regex.IsMatch(condition, @"^'\$\(Configuration\)'=='(Debug|Release)'$") ||
                System.Text.RegularExpressions.Regex.IsMatch(condition, @"^'\$\(Configuration\)\|\$\(Platform\)'=='(Debug|Release)\|(ARM64|x64|Win32)'$")) return false;
            throw new InvalidDataException($"不支持条件: {condition}");
        }

        static System.Xml.XmlNode FindChild(System.Xml.XmlNodeList lst, string name)
        {
            foreach (System.Xml.XmlNode node in lst)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }
            return null;
        }
        static bool CollectInclude(System.Xml.XmlNode sn, string vcxFile, Dictionary<string, HppUnit> headers)
        {
            var spjPath = UTypeManagerBase.GetFileDirectory(vcxFile);
            bool bFinded = false;
            foreach (System.Xml.XmlNode i in sn.ChildNodes)
            {
                if (i.Name == "ClInclude")
                {
                    var attr = i.Attributes["Include"];
                    if (attr != null)
                    {
                        if (attr.Value.EndsWith("TypeUtility.h"))
                            continue;
                        if (attr.Value.EndsWith(".inl.h"))
                            continue;
                        var hf = attr.Value.Replace("$(MSBuildThisFileDirectory)", spjPath);
                        bool error;
                        hf = Path.GetFullPath(Path.Combine(spjPath, hf.Replace('\\', '/'))).Replace('\\', '/');
                        hf = NormalizePath(hf, out error);
                        if (!File.Exists(hf)) throw new FileNotFoundException($"共享工程 {vcxFile} 的头文件不存在", hf);
                        if (error)
                        {
                            Console.WriteLine($"header file {hf} is error path");
                        }
                        var tmp = new HppUnit();
                        tmp.File = hf;
                        tmp.Module = UTypeManagerBase.GetPureFileName(vcxFile);
                        tmp.Module = tmp.Module.Replace('.', '_');

                        headers[hf.ToLower()] = tmp;
                        bFinded = true;
                    }
                }
            }
            return bFinded;
        }
        static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                    return i;
            }
            return null;
        }
        static string NormalizePath(string path, out bool error)
        {
            error = false;

            path = path.Replace("\\", "/");

            path = path.Replace("../", "$/");

            path = path.Replace("./", "");

            //path = path.ToLower();

            int UpDirLength = "$/".Length;
            int startPos = path.LastIndexOf("$/");
            while (startPos >= 0)
            {
                int rmvNum = 1;
                var head = path.Substring(0, startPos);
                var tail = path.Substring(startPos + UpDirLength);
                while (head.Length > UpDirLength && head.EndsWith("$/"))
                {
                    rmvNum++;
                    head = head.Substring(0, head.Length - "$/".Length);
                }
                if (head.EndsWith('/'))
                    head = head.Substring(0, head.Length - 1);
                int discardPos = -1;
                for (int i = 0; i < rmvNum; i++)
                {
                    discardPos = head.LastIndexOf("/");
                    if (discardPos < 0)
                    {
                        error = true;
                        return null;
                    }
                    else
                    {
                        head = head.Substring(0, discardPos);
                    }
                }
                path = head + '/' + tail;

                startPos = path.LastIndexOf("$/");
            }

            return path;
        }
    }
}

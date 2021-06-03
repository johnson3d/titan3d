using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangHeadTools
{
    public class HppCollector
    {
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

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
        public void Collect(string proj, string genDir, string buildConfig = @"'$(Configuration)|$(Platform)'=='Debug|x64'")
        {
            var path = proj.Substring(0, proj.LastIndexOf("/") + 1);

            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(proj);
            var root = myXmlDoc.LastChild;

            System.Xml.XmlNode sharedNode = null;
            foreach (System.Xml.XmlNode node in root.ChildNodes)
            {
                if (node.Name == "ImportGroup" && node.Attributes["Label"] != null && node.Attributes["Label"].Value == "Shared")
                {
                    sharedNode = node;
                }
                else if (node.Name == "ItemDefinitionGroup")
                {
                    var attr = node.Attributes["Condition"];
                    if (attr == null)
                        continue;

                    if (attr.InnerText != buildConfig)
                    {
                        continue;
                    }

                    var clCompile = FindChild(node.ChildNodes, "ClCompile");
                    if (clCompile != null)
                    {
                        var cur = FindChild(clCompile.ChildNodes, "AdditionalIncludeDirectories");
                        if (cur != null)
                        {
                            var segs = cur.InnerText.Split(';');
                            foreach (var i in segs)
                            {
                                if (i.StartsWith("%(AdditionalIncludeDirectories)"))
                                    continue;
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
                                if (i.StartsWith("%(PreprocessorDefinitions)"))
                                    continue;
                                MacroDefines.Add(i);
                            }
                        }
                    }
                }
            }
            if (sharedNode == null)
            {
                Console.WriteLine($"no shared projects in {proj}");
                return;
            }

            Headers.Clear();
            foreach (System.Xml.XmlNode node in sharedNode.ChildNodes)
            {
                if (node.Name == "Import")
                {
                    var attr = node.Attributes["Project"];
                    if (attr == null)
                        continue;

                    var sharedProjFile = attr.Value.Replace('\\', '/');
                    sharedProjFile = path + sharedProjFile;

                    bool error;
                    sharedProjFile = NormalizePath(sharedProjFile, out error);
                    if (error)
                    {
                        Console.WriteLine($"header file {sharedProjFile} is error path");
                    }

                    System.Xml.XmlDocument sharedProjXmlDoc = new System.Xml.XmlDocument();
                    sharedProjXmlDoc.Load(sharedProjFile);
                    var spjPath = sharedProjFile.Substring(0, sharedProjFile.LastIndexOf("/") + 1);
                    //var spjRoot = sharedProjXmlDoc.SelectSingleNode("Project");
                    var spjRoot = sharedProjXmlDoc.LastChild;
                    foreach (System.Xml.XmlNode sn in spjRoot.ChildNodes)
                    {
                        if (sn.Name == "ItemGroup")
                        {
                            if (CollectInclude(sn, spjPath, Headers))
                            {
                                break;
                            }
                        }
                    }
                }
            }
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
        static bool CollectInclude(System.Xml.XmlNode sn, string spjPath, Dictionary<string, string> headers)
        {
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
                        hf = NormalizePath(hf, out error);
                        if (error)
                        {
                            Console.WriteLine($"header file {hf} is error path");
                        }
                        headers[hf] = hf;
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

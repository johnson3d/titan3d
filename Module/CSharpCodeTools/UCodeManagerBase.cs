using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCodeTools
{
    class UCodeManagerBase
    {
        public List<string> SourceCodes = new List<string>();
        public List<string> WritedFiles = new List<string>();
        public List<string> WritedCppFiles = new List<string>();

        public void GatherCodeFiles(List<string> includes, List<string> excludes)
        {
            List<string> stdExcludes = new List<string>();
            foreach (var i in excludes)
            {
                bool error;
                var s = NormalizePath(i, out error);
                stdExcludes.Add(s);
            }
            foreach (var i in includes)
            {
                var files = System.IO.Directory.GetFiles(i, "*.cs", System.IO.SearchOption.AllDirectories);
                foreach (var j in files)
                {
                    bool error;
                    var s = NormalizePath(j, out error);
                    if (IsExclude(s, stdExcludes))
                    {
                        continue;
                    }
                    SourceCodes.Add(s);
                }
            }
        }
        bool IsExclude(string file, List<string> stdExcludes)
        {
            foreach (var i in stdExcludes)
            {
                if (file.StartsWith(i))
                {
                    return true;
                }
            }
            var code = System.IO.File.ReadAllText(file);
            return CheckSourceCode(code) == false;
        }
        protected virtual bool CheckSourceCode(string code)
        {
            return false;
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
        static string GetRelativePath(string path, string parent)
        {
            bool error;
            path = NormalizePath(path, out error);
            parent = NormalizePath(parent, out error);
            if (path.StartsWith(parent))
            {
                return path.Substring(parent.Length);
            }
            else
            {
                return path;
            }
        }

        public void MakeSharedProjectCSharp(string genDir, string fileName)
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(genDir + "Empty_CodeGenCSharp.projitems");
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(genDir, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                if (!WritedFiles.Contains(i.Replace("\\", "/").ToLower()))
                {
                    System.IO.File.Delete(i);
                }
            }
            allFiles = System.IO.Directory.GetFiles(genDir, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                var cs = myXmlDoc.CreateElement("Compile", root.NamespaceURI);
                var file = myXmlDoc.CreateAttribute("Include");
                file.Value = "$(MSBuildThisFileDirectory)" + GetRelativePath(i, genDir);
                cs.Attributes.Append(file);
                compile.AppendChild(cs);
            }

            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, Encoding.UTF8);
            writer.Formatting = System.Xml.Formatting.Indented;
            myXmlDoc.Save(writer);
            var reader = new System.IO.StreamReader(streamXml, Encoding.UTF8);
            streamXml.Position = 0;
            var content = reader.ReadToEnd();
            reader.Close();
            streamXml.Close();

            var projFile = genDir + fileName;
            if (System.IO.File.Exists(projFile))
            {
                string old_code = System.IO.File.ReadAllText(projFile);
                if (content == old_code)
                    return;
            }
            System.IO.File.WriteAllText(projFile, content);
        }

        public void MakeSharedProjectCpp(string genDir, string fileName)
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(genDir + "Empty_CodeGen.vcxitems");
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(genDir, "*.cpp", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                if (!WritedCppFiles.Contains(i.Replace("\\", "/").ToLower()))
                {
                    System.IO.File.Delete(i);
                }
            }
            allFiles = System.IO.Directory.GetFiles(genDir, "*.cpp", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                var cpp = myXmlDoc.CreateElement("ClCompile", root.NamespaceURI);
                var file = myXmlDoc.CreateAttribute("Include");
                file.Value = "$(MSBuildThisFileDirectory)" + GetRelativePath(i, genDir);
                cpp.Attributes.Append(file);
                compile.AppendChild(cpp);
            }

            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, Encoding.UTF8);
            writer.Formatting = System.Xml.Formatting.Indented;
            myXmlDoc.Save(writer);
            var reader = new System.IO.StreamReader(streamXml, Encoding.UTF8);
            streamXml.Position = 0;
            var content = reader.ReadToEnd();
            reader.Close();
            streamXml.Close();

            var projFile = genDir + fileName;
            if (System.IO.File.Exists(projFile))
            {
                string old_code = System.IO.File.ReadAllText(projFile);
                if (content == old_code)
                    return;
            }
            System.IO.File.WriteAllText(projFile, content);
        }
    }
}

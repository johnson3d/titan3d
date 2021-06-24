using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving
{
	class UTypeManagerBase
	{
		public Dictionary<string, string> WroteFiles = new Dictionary<string, string>();

        public static string GetRegularPath(string path)
        {
            path = path.Replace('\\', '/');

            var cur = path.IndexOf("/..");
            while (cur >= 0)
            {
                cur--;
                var start = path.LastIndexOf('/', cur);
                if (start < 0)
                    return null;
                path = path.Remove(start, cur + 1 - start + 3);
                cur = path.IndexOf("/..");
            }
            return path;
        }
        public static string GetPureFileName(string path)
        {
            bool error;
            path = NormalizePath(path, out error);
            var pos1 = path.LastIndexOf('/');
            var pos2 = path.LastIndexOf('.');
            return path.Substring(pos1 + 1, pos2 - pos1 - 1);
        }
        public static string GetFileDirectory(string file)
        {
            return file.Substring(0, file.LastIndexOf("/") + 1);
        }
        public static string NormalizePath(string path, out bool error)
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
                if (!WroteFiles.ContainsKey(i.Replace("\\", "/").ToLower()))
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
                if (!WroteFiles.ContainsKey(i.Replace("\\", "/").ToLower()))
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

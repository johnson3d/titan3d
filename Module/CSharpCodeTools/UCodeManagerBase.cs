using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        #region Iterator
        public Dictionary<string, UClassCodeBase> ClassDefines = new Dictionary<string, UClassCodeBase>();
        public UClassCodeBase FindOrCreate(string fullname)
        {
            UClassCodeBase result;
            if (ClassDefines.TryGetValue(fullname, out result))
            {
                return result;
            }

            result = CreateClassDefine(fullname);

            ClassDefines.Add(fullname, result);

            return result;
        }
        protected virtual UClassCodeBase CreateClassDefine(string fullname)
        {
            var result = new UClassCodeBase();
            result.FullName = fullname;
            return result;
        }
        protected virtual void OnVisitMethod(UClassCodeBase kls, MethodDeclarationSyntax method)
        {

        }
        public void GatherClass()
        {
            const string Start_String = "#if TitanEngine_AutoGen";
            const string End_String = "#endif//TitanEngine_AutoGen";
            foreach (var i in SourceCodes)
            {
                string beforeStr = null;
                string afterStr = null;
                var code = System.IO.File.ReadAllText(i);
                var istart = code.IndexOf(Start_String);
                if (istart >= 0)
                {
                    beforeStr = code.Substring(0, istart);
                    var iend = code.IndexOf(End_String);
                    if (iend >= 0)
                    {
                        afterStr = code.Substring(iend + End_String.Length);
                    }
                }
                if (beforeStr != null)
                {
                    code = beforeStr + afterStr;
                }
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                foreach (var j in root.Members)
                {
                    IterateClass(root, j);
                }
            }

            foreach (var i in ClassDefines)
            {
                var klsDefine = i.Value;
                klsDefine.Build();
            }
        }
        public virtual void IterateClass(CompilationUnitSyntax root, MemberDeclarationSyntax decl)
        {
            switch (decl.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    {
                        var kls = decl as ClassDeclarationSyntax;
                        var fullname = ClassDeclarationSyntaxExtensions.GetFullName(kls);
                        if (GetClassAttribute(kls))
                        {
                            var klsDeffine = FindOrCreate(fullname);
                            foreach (var i in root.Usings)
                            {
                                klsDeffine.AddUsing(i.ToString());
                            }
                        }
                        {
                            var klsDeffine = FindOrCreate(fullname);
                            foreach (var i in kls.Members)
                            {
                                if (i.Kind() == SyntaxKind.MethodDeclaration)
                                {
                                    var method = i as MethodDeclarationSyntax;
                                    OnVisitMethod(klsDeffine, method);
                                }
                            }
                        }
                    }
                    break;
                case SyntaxKind.NamespaceDeclaration:
                    {
                        var ns = decl as NamespaceDeclarationSyntax;
                        foreach (var i in ns.Members)
                        {
                            IterateClass(root, i);
                        }
                    }
                    break;
            }
        }

        protected virtual bool GetClassAttribute(ClassDeclarationSyntax decl)
        {
            foreach (var i in decl.AttributeLists)
            {
                foreach (var j in i.Attributes)
                {
                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                    if (attributeName.EndsWith("UCs2CppAttribute") || attributeName.EndsWith("UCs2Cpp"))
                    {
                        if (j.ArgumentList != null)
                        {
                            foreach (var m in j.ArgumentList.Arguments)
                            {
                                var argName = m.NormalizeWhitespace().ToFullString();
                                if (m.NameEquals != null && m.Expression != null)
                                {
                                    var name = m.NameEquals.Name.Identifier.ValueText;
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void WriteCode(string dir)
        {
            foreach (var i in ClassDefines)
            {
                i.Value.GenCode(dir);
            }
        }
        #endregion
    }
}

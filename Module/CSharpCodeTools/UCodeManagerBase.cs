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
        protected ProjectSourceInputs ProjectInputs;
        private string CurrentSourceFile;
        private readonly Dictionary<string, string> ClassSources = new(StringComparer.Ordinal);
        protected readonly Dictionary<string, List<ClassDeclarationSyntax>> ProjectDeclarations = new(StringComparer.Ordinal);

        public void SetProjectInputs(ProjectSourceInputs inputs)
        {
            ProjectInputs = inputs;
            SourceCodes.Clear();
            SourceCodes.AddRange(inputs.SourceFiles);
            WritedFiles.Clear();
            ClassDefines.Clear();
            ClassSources.Clear();
        }

        protected static InvalidDataException SourceError(SyntaxNode node, string message)
        {
            var location = node.GetLocation().GetLineSpan();
            return new InvalidDataException($"{location.Path}({location.StartLinePosition.Line + 1}): {message}");
        }

        protected void WriteGeneratedCode(UClassCodeBase definition, string directory, string suffix, string role = "", bool sourceUsings = true)
        {
            if (definition.ClassCode.Length == 0) return;
            var relative = System.IO.Path.GetRelativePath(System.IO.Path.GetDirectoryName(ProjectInputs.ProjectFile), ClassSources[definition.FullName]);
            var identity = relative.Replace('\\', '/') + "|" + definition.FullName + role;
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
            var file = System.IO.Path.Combine(directory, hash + suffix);
            var code = (sourceUsings ? string.Join("\n", definition.Usings.Distinct(StringComparer.Ordinal)) + "\n" : "") + definition.ClassCode;
            System.IO.Directory.CreateDirectory(directory);
            if (!System.IO.File.Exists(file) || System.IO.File.ReadAllText(file) != code)
                System.IO.File.WriteAllText(file, code, new UTF8Encoding(false));
            WritedFiles.Add(System.IO.Path.GetFullPath(file));
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
            Array.Sort(allFiles, StringComparer.OrdinalIgnoreCase);
            foreach (var i in allFiles)
            {
                if (!WritedFiles.Contains(i.Replace("\\", "/").ToLower()))
                {
                    System.IO.File.Delete(i);
                }
            }
            allFiles = System.IO.Directory.GetFiles(genDir, "*.cs", System.IO.SearchOption.AllDirectories);
            Array.Sort(allFiles, StringComparer.OrdinalIgnoreCase);
            foreach (var i in allFiles)
            {
                var cs = myXmlDoc.CreateElement("Compile", root.NamespaceURI);
                var file = myXmlDoc.CreateAttribute("Include");
                file.Value = "$(MSBuildThisFileDirectory)" + GetRelativePath(i, genDir);
                cs.Attributes.Append(file);
                compile.AppendChild(cs);
            }

            var encoding = new System.Text.UTF8Encoding(false);
            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, encoding);
            writer.Formatting = System.Xml.Formatting.Indented;
            myXmlDoc.Save(writer);
            writer.Flush();
            var newBytes = streamXml.ToArray();
            streamXml.Close();

            var projFile = genDir + fileName;
            if (System.IO.File.Exists(projFile))
            {
                var oldBytes = System.IO.File.ReadAllBytes(projFile);
                if (oldBytes.AsSpan().SequenceEqual(newBytes))
                    return;
            }
            System.IO.File.WriteAllBytes(projFile, newBytes);
        }

        public void MakeSharedProjectCpp(string genDir, string fileName)
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(genDir + "Empty_CodeGen.vcxitems");
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(genDir, "*.cpp", System.IO.SearchOption.AllDirectories);
            Array.Sort(allFiles, StringComparer.OrdinalIgnoreCase);
            foreach (var i in allFiles)
            {
                if (!WritedCppFiles.Contains(i.Replace("\\", "/").ToLower()))
                {
                    System.IO.File.Delete(i);
                }
            }
            allFiles = System.IO.Directory.GetFiles(genDir, "*.cpp", System.IO.SearchOption.AllDirectories);
            Array.Sort(allFiles, StringComparer.OrdinalIgnoreCase);
            foreach (var i in allFiles)
            {
                var cpp = myXmlDoc.CreateElement("ClCompile", root.NamespaceURI);
                var file = myXmlDoc.CreateAttribute("Include");
                file.Value = "$(MSBuildThisFileDirectory)" + GetRelativePath(i, genDir);
                cpp.Attributes.Append(file);
                compile.AppendChild(cpp);
            }

            var encoding = new System.Text.UTF8Encoding(false);
            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, encoding);
            writer.Formatting = System.Xml.Formatting.Indented;
            myXmlDoc.Save(writer);
            writer.Flush();
            var newBytes = streamXml.ToArray();
            streamXml.Close();

            var projFile = genDir + fileName;
            if (System.IO.File.Exists(projFile))
            {
                var oldBytes = System.IO.File.ReadAllBytes(projFile);
                if (oldBytes.AsSpan().SequenceEqual(newBytes))
                    return;
            }
            System.IO.File.WriteAllBytes(projFile, newBytes);
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
            result.Declaration = ProjectDeclarations[fullname][0];
            foreach (var declaration in ProjectDeclarations[fullname])
            {
                foreach (var directive in declaration.SyntaxTree.GetCompilationUnitRoot().Usings)
                    result.AddUsing(directive.ToString());
            }

            ClassDefines.Add(fullname, result);
            ClassSources.Add(fullname, CurrentSourceFile);

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
            if (ProjectInputs == null) throw new InvalidOperationException("必须先提供目标工程源码上下文");
            ClassDefines.Clear();
            ClassSources.Clear();
            ProjectDeclarations.Clear();
            var roots = new List<(string File, CompilationUnitSyntax Root)>();
            foreach (var file in SourceCodes.OrderBy(p => p, StringComparer.Ordinal))
            {
                CurrentSourceFile = file;
                var code = LegacyGeneratedBlocks.Strip(System.IO.File.ReadAllText(file), file);
                var tree = CSharpSyntaxTree.ParseText(code, ProjectInputs.ParseOptions, file);
                var errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
                if (errors.Length != 0) throw new InvalidDataException(string.Join("\n", errors.Select(d => d.ToString())));
                var root = tree.GetCompilationUnitRoot();
                roots.Add((file, root));
                foreach (var declaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    var fullname = declaration.GetFullName();
                    if (!ProjectDeclarations.TryGetValue(fullname, out var partials))
                        ProjectDeclarations.Add(fullname, partials = new List<ClassDeclarationSyntax>());
                    partials.Add(declaration);
                }
            }
            foreach (var (file, root) in roots)
            {
                CurrentSourceFile = file;
                foreach (var declaration in root.Members) IterateClass(root, declaration);
            }

            foreach (var i in ClassDefines)
            {
                var klsDefine = i.Value;
                klsDefine.Build();
            }
        }
        public virtual void IterateClass(CompilationUnitSyntax root, MemberDeclarationSyntax decl)
        {
            if (decl is BaseNamespaceDeclarationSyntax scope)
            {
                foreach (var member in scope.Members) IterateClass(root, member);
                return;
            }
            if (decl is TypeDeclarationSyntax enclosing)
                foreach (var nested in enclosing.Members.OfType<TypeDeclarationSyntax>()) IterateClass(root, nested);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangHeadTools
{
    //[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 4, Size = 8)]
    //public struct AAA
    //{
    //    [System.Runtime.InteropServices.FieldOffset(100)]
    //    public int a;
    //}
    public class CppTypeManager
    {
        public static CppTypeManager Instance = new CppTypeManager();

        HppCollector mHppCollector = new HppCollector();

        List<TUCreator> mTransUnits = new List<TUCreator>();

        public class TUTask : IBuilderTask
        {
            public string CppFile;
            public override void Execute(TaskThread thread)
            {
                var tmp = new TUCreator();
                if (tmp.CreateTU(CppFile, CppTypeManager.Instance.mHppCollector.IncludePath, CppTypeManager.Instance.mHppCollector.MacroDefines))
                {
                    lock (CppTypeManager.Instance.mTransUnits)
                    {
                        CppTypeManager.Instance.mTransUnits.Add(tmp);
                    }
                }
                if (thread.Tasks.Count == 0)
                {
                    thread.Stop();
                }
            }
        }

        public void CollectTypes(string proj, string genDir, string extIncPath, string extMacro)
        {
            mHppCollector.Collect(proj, genDir, @"'$(Configuration)|$(Platform)'=='Debug|x64'");
            mHppCollector.AddExtraIncludePath(extIncPath);
            mHppCollector.AddExtraMacroDefine(extMacro);

            Console.WriteLine("Begin Clang Scanner");

            //foreach (var i in mHppCollector.Headers)
            //{
            //    var tmp = new TUCreator();
            //    if (tmp.CreateTU(i.Value, mHppCollector.IncludePath, mHppCollector.MacroDefines))
            //        mTransUnits.Add(tmp);
            //}
            //return;

            TaskThreadManager.Instance.InitThreads(60);
            foreach (var i in mHppCollector.Headers)
            {
                var tmp = new TUTask();
                tmp.CppFile = i.Value;
                TaskThreadManager.Instance.DispatchTask(tmp);
            }
            TaskThreadManager.Instance.StartThreads();
            TaskThreadManager.Instance.WaitAllThreadFinished();

            System.Threading.Thread.Sleep(2000);
            Console.WriteLine("End Clang Scanner");

            Console.WriteLine("Begin Build Writer");
            foreach (var i in mTransUnits)
            {
                var nsDecls = i.mTransUnit.TranslationUnitDecl.Decls.OfType<ClangSharp.NamespaceDecl>();
                var classDecls = i.mTransUnit.TranslationUnitDecl.Decls.OfType<ClangSharp.CXXRecordDecl>();
                var enumsDecls = i.mTransUnit.TranslationUnitDecl.Decls.OfType<ClangSharp.EnumDecl>();
                var typeDefs = i.mTransUnit.TranslationUnitDecl.Decls.OfType<ClangSharp.TypedefDecl>();

                foreach (var j in classDecls)
                {
                    if (GetExportDesc(j.Attrs) == null)
                    {
                        continue;
                    }
                    var tmp = new ClassWriter();
                    tmp.Decl = j;

                    var fname = tmp.FullName;
                    if (CodeWriterManager.Instance.Classes.ContainsKey(fname) == false)
                        CodeWriterManager.Instance.Classes.Add(fname, tmp);
                }
                foreach (var j in enumsDecls)
                {
                    if (GetExportDesc(j.Attrs) == null)
                    {
                        continue;
                    }
                    var tmp = new EnumWriter();
                    tmp.Decl = j;
                    var fname = tmp.FullName;
                    if (CodeWriterManager.Instance.Enums.ContainsKey(fname) == false)
                        CodeWriterManager.Instance.Enums.Add(fname, tmp);
                }
                foreach (var j in typeDefs)
                {
                    if (GetExportDesc(j.Attrs) == null)
                    {
                        continue;
                    }

                    var t = j.UnderlyingType.PointeeType as ClangSharp.FunctionProtoType;
                    if (t != null)
                    {
                        var tmp = new DelegateWriter();
                        tmp.Decl = j;
                        var fname = t.ToString();
                        fname = fname.Replace("::", ".");
                        if (CodeWriterManager.Instance.Delegates.ContainsKey(fname) == false)
                            CodeWriterManager.Instance.Delegates.Add(fname, tmp);
                    }
                }
                foreach (var j in nsDecls)
                {
                    CollectNS(j);
                }
            }
            Console.WriteLine("End Build Writer");

            Console.WriteLine("Begin CSBinder Code");

            foreach(var i in CodeWriterManager.Instance.Classes)
            {
                var cppWriter = new CSharp.CppBinderWriter();
                cppWriter.Parent = i.Value;
                cppWriter.Decl = i.Value.Decl;
                cppWriter.GenCode();
                cppWriter.WriteSourceFile(genDir, ".gen.cpp");

                var csWriter = new CSharp.CSClassBinderWriter();
                csWriter.Parent = i.Value;
                csWriter.Decl = i.Value.Decl;
                csWriter.GenCode();
                csWriter.WriteSourceFile(genDir, ".gen.cs");
            }

            foreach (var i in CodeWriterManager.Instance.Enums)
            {
                var cppWriter = new CSharp.CppEnumBinderWriter();
                cppWriter.Parent = i.Value;
                cppWriter.Decl = i.Value.Decl;
                cppWriter.GenCode();
                cppWriter.WriteSourceFile(genDir, ".gen.cpp");

                var csWriter = new CSharp.CSEnumBinderWriter();
                csWriter.Parent = i.Value;
                csWriter.Decl = i.Value.Decl;
                csWriter.GenCode();
                csWriter.WriteSourceFile(genDir, ".gen.cs");
            }

            var dlgtWriter = new CSharp.CSDelegateBinderWriter();
            dlgtWriter.Namespace = "";
            dlgtWriter.AddLine("using System;");
            dlgtWriter.AddLine("using System.Collections.Generic;");
            foreach (var i in CodeWriterManager.Instance.Delegates)
            {
                dlgtWriter.GenCode(i.Value);
            }

            dlgtWriter.WriteSourceFile(genDir, ".gen.cs");

            Console.WriteLine("End CSBinder Code");

            MakeSharedProjectCpp(genDir);
            MakeSharedProjectCSharp(genDir);

            Console.WriteLine("C++/C# Bind finished");
        }

        private string GetExportDesc(IReadOnlyList<ClangSharp.Attr> attrs)
        {
            foreach (var i in attrs)
            {
                if (i.Kind == ClangSharp.Interop.CX_AttrKind.CX_AttrKind_Annotate)
                {
                    return i.Spelling;
                }
            }
            return null;
        }
        private void CollectNS(ClangSharp.NamespaceDecl decl)
        {
            var nsDecls = decl.Decls.OfType<ClangSharp.NamespaceDecl>();
            var classDecls = decl.Decls.OfType<ClangSharp.CXXRecordDecl>();
            var enumsDecls = decl.Decls.OfType<ClangSharp.EnumDecl>();
            var typeDefs = decl.Decls.OfType<ClangSharp.TypedefDecl>();

            foreach (var j in classDecls)
            {
                if (GetExportDesc(j.Attrs) == null)
                {
                    continue;
                }
                var tmp = new ClassWriter();
                tmp.Decl = j;

                var fname = tmp.FullName;
                if (CodeWriterManager.Instance.Classes.ContainsKey(fname) == false)
                    CodeWriterManager.Instance.Classes.Add(fname, tmp);
            }
            foreach (var j in enumsDecls)
            {
                if (GetExportDesc(j.Attrs) == null)
                {
                    continue;
                }
                var tmp = new EnumWriter();
                tmp.Decl = j;
                var fname = tmp.FullName;
                if (CodeWriterManager.Instance.Enums.ContainsKey(fname) == false)
                    CodeWriterManager.Instance.Enums.Add(fname, tmp);
            }
            foreach (var j in typeDefs)
            {
                if (GetExportDesc(j.Attrs) == null)
                {
                    continue;
                }

                var t = j.UnderlyingType.PointeeType as ClangSharp.FunctionProtoType;
                if (t != null)
                {
                    var tmp = new DelegateWriter();
                    tmp.Decl = j;
                    var fname = t.ToString();
                    fname = fname.Replace("::", ".");
                    if (CodeWriterManager.Instance.Delegates.ContainsKey(fname) == false)
                        CodeWriterManager.Instance.Delegates.Add(fname, tmp);
                }
            }
            foreach (var j in nsDecls)
            {
                CollectNS(j);
            }
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
        public void MakeSharedProjectCpp(string genDir)
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(genDir + "Empty_CodeGen.vcxitems");
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(genDir, "*.cpp", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                if (!CodeWriterManager.Instance.WritedFiles.Contains(i.Replace("\\", "/").ToLower()))
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

            var projFile = genDir + "CodeGen.vcxitems";
            if (System.IO.File.Exists(projFile))
            {
                string old_code = System.IO.File.ReadAllText(projFile);
                if (content == old_code)
                    return;
            }
            System.IO.File.WriteAllText(projFile, content);
        }

        public void MakeSharedProjectCSharp(string genDir)
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(genDir + "Empty_CodeGenCSharp.projitems");
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(genDir, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                if (!CodeWriterManager.Instance.WritedFiles.Contains(i.Replace("\\", "/").ToLower()))
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

            var projFile = genDir + "CodeGenCSharp.projitems";
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

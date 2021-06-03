using System;
using ClangSharp;
using ClangSharp.Interop;
using System.Text;
using System.Linq;

namespace ClangHeadTools
{
    class Program
    {
        static int Main1(string[] args)
        {
            CppTypeManager.Instance.CollectTypes("F:/TProject/Core.Window/Core.Window.vcxproj",
                "F:/TProject/codegen/NativeBinder/",
                "F:/TProject/Module/IncludePath.txt",
                "F:/TProject/Module/MacroDefine.txt");

            //Console.WriteLine($"Begin execute");
            //System.Threading.Thread.Sleep(1000 * 60);
            //Console.WriteLine($"End execute");
            return 0;
        }
        static int Main(string[] args)
        {
            if (true)
            {
                var proj = FindArgument(args, "vcxproj=");
                if (proj == null)
                {
                    Console.WriteLine("must input vcxproj");
                    return -1;
                }
                proj = proj.Substring("vcxproj=".Length);
                proj = proj.Replace('\\', '/');
                Console.WriteLine($"vcxproj = {proj}");

                var path = proj.Substring(0, proj.LastIndexOf("/") + 1);

                var genDir = FindArgument(args, "gen_dir=");
                genDir = genDir.Substring("gen_dir=".Length);
                Console.WriteLine($"gen_dir = {genDir}");
                
                var pch = FindArgument(args, "pch=");
                pch = pch.Substring("pch=".Length);
                Console.WriteLine($"gen_dir = {pch}");
                CodeWriter.PchFile = pch;

                var incFile = FindArgument(args, "include=");
                incFile = incFile.Substring("include=".Length);
                Console.WriteLine($"include = {incFile}");

                var defFile = FindArgument(args, "preprocessor=");
                defFile = defFile.Substring("preprocessor=".Length);
                Console.WriteLine($"preprocessor = {defFile}");

                CppTypeManager.Instance.CollectTypes(proj,//"F:/TProject/Core.Window/Core.Window.vcxproj",
                    genDir, //"F:/TProject/codegen/",
                    incFile, //"F:/TProject/Module/IncludePath.txt",
                    defFile); //"F:/TProject/Module/MacroDefine.txt");

                for(int i=0; i<5; i++)
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
                return 0;
            }
            else
            {
                return Main1(args);
            }

            var inputContents = $@"template<class T>
class MyClass
{{
    T value;
}};
";

            /*
#include "what.h"

class AAA
{
public:
	__attribute__((annotate("test field")))
	int a;
	float b;
	bool c;
	[[deprecated("Will remove in next release")]] 
	__attribute__((annotate("test function"))) 
		int Test(int t0, float t1)
	{
		int f0 = 3;
		f0 = f0+4;
		a = f0;
		return a + 3;
	}
} __attribute__((annotate("test class")));

enum EBType
{
	EBT_1,
	EBT_2 = EBT_1 + 3,
};
             */

            using var translationUnit = CreateTranslationUnit(inputContents);

            var nsDecls = translationUnit.TranslationUnitDecl.Decls.OfType<ClangSharp.NamespaceDecl>();

            ClangSharp.NamespaceDecl testNs = null;
            foreach (var i in nsDecls)
            {
                if (i.Name == "TestLibCLang")
                {
                    testNs = i;
                    break;
                }
            }

            {
                var typeDefDecls = testNs.Decls.OfType<ClangSharp.TypedefDecl>();
                foreach (var i in typeDefDecls)
                {
                    if (i.Attrs.Count > 0)
                    {
                        var t = i.UnderlyingType.PointeeType as ClangSharp.FunctionProtoType;
                        if (t != null)
                        {
                            int xxx = 0;
                        }
                    }
                }
            }

            //var classTemplateDecl = translationUnit.TranslationUnitDecl.Decls.OfType<ClangSharp.ClassTemplateDecl>().Single();
            {
                var fields = testNs.Decls.OfType<ClangSharp.EnumDecl>();
                foreach (var i in fields)
                {
                    var csWriter = new CSharp.CSEnumBinderWriter();
                    csWriter.Decl = i;
                    csWriter.GenCode();
                    csWriter.WriteSourceFile("F:/TProject/codegen/", ".gen.cs");

                    Console.WriteLine(i.Name);
                    foreach(var j in i.Enumerators)
                    {
                        Console.WriteLine(j.Name + " = " + j.InitVal); 
                    }
                }
            }

            var typeDecls = testNs.Decls.OfType<ClangSharp.CXXRecordDecl>();
            foreach(var i in typeDecls)
            {
                var desc = i as ClangSharp.CXXRecordDecl;
                if (desc != null && desc.Name == "CCC")
                {
                    foreach (var j in desc.Fields)
                    {
                        if(j.IsAnonymousField)
                        {
                            var tagDecl = j.Type.AsTagDecl as ClangSharp.CXXRecordDecl;
                            if (tagDecl != null)
                            {
                                foreach(var k in tagDecl.Fields)
                                {
                                    var offset = desc.TypeForDecl.Handle.GetOffsetOf(k.Name);
                                    if(offset>0)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
                else if (desc != null && desc.Name == "AAA")
                {
                    var cppWriter = new CSharp.CppBinderWriter();
                    cppWriter.Decl = desc;
                    cppWriter.GenCode();
                    cppWriter.WriteSourceFile("F:/TProject/codegen/", ".gen.cpp");
                    var csWriter = new CSharp.CSClassBinderWriter();
                    csWriter.Decl = desc;
                    csWriter.GenCode();
                    csWriter.WriteSourceFile("F:/TProject/codegen/", ".gen.cs");
                    var fields = desc.Decls.OfType<ClangSharp.FieldDecl>();
                    var parent = desc.Bases[0].Referenced as ClangSharp.CXXRecordDecl;
                    var dn = parent.Name;
                    foreach (var j in fields)
                    {
                        Console.WriteLine(j.Type.ToString());
                        Console.WriteLine(j.Name); 
                        if (j.Attrs.Count > 0)
                        {
                            if (j.Attrs[0].Kind == CX_AttrKind.CX_AttrKind_Annotate)
                            {
                                Console.WriteLine(j.Attrs[0].Spelling);
                            }
                        }
                    }
                    var funcList = desc.Decls.OfType<ClangSharp.FunctionDecl>();
                    ClangSharp.FunctionDecl func = null;
                    foreach(var j in funcList)
                    {
                        if (j.Name == "Test")
                        {
                            func = j;
                            break;
                        }
                    }
                    if (func != null)
                    {
                        //Console.WriteLine(func.Type);
                        Console.WriteLine(func.ReturnType);
                        Console.WriteLine(func.Name);
                        //Console.WriteLine(func.Body.ToString());
                        for (int j = 0; j < func.NumParams; j++)
                        {
                            Console.WriteLine(func.Parameters[j].Type);
                            Console.WriteLine(func.Parameters[j].Name);
                        }
                        foreach (var j in func.Attrs)
                        {
                            var at = j as ClangSharp.InheritableAttr;
                            if (at != null)
                            {
                                if (at.Kind == CX_AttrKind.CX_AttrKind_Deprecated)
                                {

                                }
                                else if (at.Kind == CX_AttrKind.CX_AttrKind_Annotate)
                                {
                                    Console.WriteLine(at.Spelling);
                                }
                            }
                        }
                    }
                }
            }
            //Assert.Equal("MyClass", classTemplateDecl.Name);

            //var templateParameter = classTemplateDecl.TemplateParameters.Single();
            //Assert.Equal("T", templateParameter.Name);

            return 0;
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

        protected const string DefaultInputFileName = "ClangUnsavedFile.h";
        protected const CXTranslationUnit_Flags DefaultTranslationUnitFlags = CXTranslationUnit_Flags.CXTranslationUnit_IncludeAttributedTypes      // Include attributed types in CXType
                                                                            | CXTranslationUnit_Flags.CXTranslationUnit_VisitImplicitAttributes;    // Implicit attributes should be visited
        protected static readonly string[] DefaultClangCommandLineArgs = new string[]
        {
            "-std=c++17",                           // The input files should be compiled for C++ 17
            "-xc++",                                // The input files are C++
            "-Wno-pragma-once-outside-header",       // We are processing files which may be header files
            "-IF:/cherubim/CRBProject/Source/CRBProject/World"
        };
        protected static TranslationUnit CreateTranslationUnit(string inputContents)
        {
            using var unsavedFile = CXUnsavedFile.Create(DefaultInputFileName, inputContents);
            var unsavedFiles = new CXUnsavedFile[] { unsavedFile};

            var index = CXIndex.Create();
            //var translationUnit = CXTranslationUnit.Parse(index, DefaultInputFileName, DefaultClangCommandLineArgs, unsavedFiles, DefaultTranslationUnitFlags);
            var translationUnit = CXTranslationUnit.Parse(index, "F:/cherubim/CRBProject/Source/CRBProject/aa.h", DefaultClangCommandLineArgs, Array.Empty<CXUnsavedFile>(), DefaultTranslationUnitFlags);

            if (translationUnit.NumDiagnostics != 0)
            {
                var errorDiagnostics = new StringBuilder();
                _ = errorDiagnostics.AppendLine($"The provided {nameof(CXTranslationUnit)} has the following diagnostics which prevent its use:");
                
                for (uint i = 0; i < translationUnit.NumDiagnostics; ++i)
                {
                    using var diagnostic = translationUnit.GetDiagnostic(i);

                    if (diagnostic.Severity is CXDiagnosticSeverity.CXDiagnostic_Error or CXDiagnosticSeverity.CXDiagnostic_Fatal)
                    {
                        _ = errorDiagnostics.Append(' ', 4);
                        _ = errorDiagnostics.AppendLine(diagnostic.Format(CXDiagnosticDisplayOptions.CXDiagnostic_DisplayOption).ToString());
                    }
                }
            }

            return TranslationUnit.GetOrCreate(translationUnit);
        }
    }
}

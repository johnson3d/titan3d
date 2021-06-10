using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving
{
    public class TUCreator
    {
        static TUCreator()
        {
            ExpFlags.Add("TR_CLASS");
            ExpFlags.Add("TR_ENUM");
            ExpFlags.Add("TR_CALLBACK");
        }
        public ClangSharp.TranslationUnit mTransUnit;
		public List<string> mUsings;
        public static List<string> ExpFlags = new List<string>();
        public bool CreateTU(string file, List<string> includePath, List<string> macros)
        {
            var allCode = System.IO.File.ReadAllText(file);
            bool findExp = false;
            foreach(var i in ExpFlags)
            {
                if (allCode.IndexOf(i) >= 0)
                {
                    findExp = true;
                    break;
                }
            }
            if (findExp == false)
                return false;

            var index = ClangSharp.Interop.CXIndex.Create();
            const ClangSharp.Interop.CXTranslationUnit_Flags DefaultTranslationUnitFlags = ClangSharp.Interop.CXTranslationUnit_Flags.CXTranslationUnit_IncludeAttributedTypes      // Include attributed types in CXType
                                                                            | ClangSharp.Interop.CXTranslationUnit_Flags.CXTranslationUnit_VisitImplicitAttributes    // Implicit attributes should be visited
																			| ClangSharp.Interop.CXTranslationUnit_Flags.CXTranslationUnit_ForSerialization
																			| ClangSharp.Interop.CXTranslationUnit_Flags.CXTranslationUnit_SkipFunctionBodies;

            var args = new List<string>();
            args.Add("-std=c++17");
            args.Add("-xc++");
            args.Add("-Wno-pragma-once-outside-header");
            args.Add("-Wno-address-of-temporary");
            foreach (var i in includePath)
            {
                args.Add($"-I{i}");
            }
            foreach (var i in macros)
            {
                args.Add($"-D{i}");
            }
            string[] DefaultClangCommandLineArgs = args.ToArray();
            var translationUnit = ClangSharp.Interop.CXTranslationUnit.Parse(index, file, DefaultClangCommandLineArgs, Array.Empty<ClangSharp.Interop.CXUnsavedFile>(), DefaultTranslationUnitFlags);

            if (translationUnit.NumDiagnostics != 0)
            {
                Console.WriteLine($"The provided {nameof(ClangSharp.Interop.CXTranslationUnit)} has the following diagnostics which prevent its use:");

                for (uint i = 0; i < translationUnit.NumDiagnostics; ++i)
                {
                    using var diagnostic = translationUnit.GetDiagnostic(i);

                    if (diagnostic.Severity is ClangSharp.Interop.CXDiagnosticSeverity.CXDiagnostic_Error or ClangSharp.Interop.CXDiagnosticSeverity.CXDiagnostic_Fatal)
                    {
                        Console.WriteLine(file);
                        ClangSharp.Interop.CXFile tfile;
                        uint line, col, offset;
                        diagnostic.Location.GetFileLocation(out tfile, out line, out col, out offset);
                        Console.WriteLine($"Translation Unit Error: {tfile}:({line}:{col})");
                        Console.WriteLine(diagnostic.Format(ClangSharp.Interop.CXDiagnosticDisplayOptions.CXDiagnostic_DisplayOption).ToString());

                        index.Dispose();
                        translationUnit.Dispose();
                        return false;
                    }
                }
            }

            mTransUnit = ClangSharp.TranslationUnit.GetOrCreate(translationUnit);

			//translationUnit.Save();
			//mTransUnit.Handle.Save();
			//ClangSharp.Interop.CXTranslationUnit.Create()

			return true;
        }
    }
}

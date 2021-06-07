using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangHeadTools
{
    public class TUCreator
    {
        public ClangSharp.TranslationUnit mTransUnit;
        public bool CreateTU(string file, List<string> includePath, List<string> macros)
        {
            var allCode = System.IO.File.ReadAllText(file);
            if (allCode.IndexOf("TR_CLASS") < 0 && allCode.IndexOf("TR_ENUM") < 0 && allCode.IndexOf("TR_CALLBACK") < 0)
                return false;

            var index = ClangSharp.Interop.CXIndex.Create();
            const ClangSharp.Interop.CXTranslationUnit_Flags DefaultTranslationUnitFlags = ClangSharp.Interop.CXTranslationUnit_Flags.CXTranslationUnit_IncludeAttributedTypes      // Include attributed types in CXType
                                                                            | ClangSharp.Interop.CXTranslationUnit_Flags.CXTranslationUnit_VisitImplicitAttributes    // Implicit attributes should be visited
                                                                            //| ClangSharp.Interop.CXTranslationUnit_Flags.CXTranslationUnit_ForSerialization
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
            //translationUnit.Save();
            //ClangSharp.Interop.CXTranslationUnit.Create()
            mTransUnit = ClangSharp.TranslationUnit.GetOrCreate(translationUnit);
            return true;
        }
    }
}

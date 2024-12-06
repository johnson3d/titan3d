using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeTools.Macross
{
    class UMacrossClassManager : UCodeManagerBase
    {
        public static UMacrossClassManager Instance = new UMacrossClassManager();
        protected override bool CheckSourceCode(string code)
        {
            if (code.Contains("Meta"))
                return true;
            return false;
        }
        protected override UClassCodeBase CreateClassDefine(string fullname)
        {
            var result = new UMacrossClassDefine();
            result.FullName = fullname;
            return result;
        }
        protected override bool GetClassAttribute(ClassDeclarationSyntax decl)
        {
            return true;
            //foreach (var i in decl.AttributeLists)
            //{
            //    foreach (var j in i.Attributes)
            //    {
            //        var attributeName = j.Name.NormalizeWhitespace().ToFullString();
            //        if (attributeName.EndsWith("MetaAttribute") || attributeName.EndsWith("Meta"))
            //        {
            //            if (j.ArgumentList != null)
            //            {
            //                foreach (var m in j.ArgumentList.Arguments)
            //                {
            //                    var argName = m.NormalizeWhitespace().ToFullString();
            //                    if (m.NameEquals != null && m.Expression != null)
            //                    {
            //                        var name = m.NameEquals.Name.Identifier.ValueText;
            //                    }
            //                }
            //            }
            //            return true;
            //        }
            //    }
            //}
            //return false;
        }
        public static AttributeSyntax HasAttribute(MethodDeclarationSyntax method, string[] attrNames)
        {
            foreach (var j in method.AttributeLists)
            {
                foreach (var k in j.Attributes)
                {
                    var attributeName = k.Name.NormalizeWhitespace().ToFullString();
                    foreach(var n in attrNames)
                    {
                        if (attributeName.EndsWith(n))
                        {
                            return k;
                        }
                    }
                }
            }
            return null;
        }
        protected override void OnVisitMethod(UClassCodeBase kls, MethodDeclarationSyntax method)
        {
            var attr = HasAttribute(method, new string[] { "MetaAttribute", "Meta" });
            if (attr != null)
            {
                if (attr.ArgumentList != null)
                {
                    foreach (var i in attr.ArgumentList.Arguments)
                    {
                        var argStr = i.ToFullString();
                        if (argStr.Contains("ManualMarshal"))
                        {
                            return;
                        }
                    }
                }
                var tmp = new UMacrossFunction();
                tmp.MethodSyntax = method;
                var klsDeffine = kls as UMacrossClassDefine;
                klsDeffine.Functions.Add(tmp);
            }
            //foreach (var j in method.AttributeLists)
            //{
            //    foreach (var k in j.Attributes)
            //    {
            //        var attributeName = k.Name.NormalizeWhitespace().ToFullString();
            //        if (attributeName.EndsWith("MetaAttribute") || attributeName.EndsWith("Meta"))
            //        {
            //            var tmp = new UMacrossFunction();
            //            tmp.MethodSyntax = method;
            //            var klsDeffine = kls as UMacrossClassDefine;
            //            klsDeffine.Functions.Add(tmp);
            //            break;
            //        }
            //    }
            //}
        }

        public void GatherMacrossClass(string dir)
        {
            const string Start_String = "#if TitanEngine_AutoGen_Macross";
            const string End_String = "#endif//TitanEngine_AutoGen_Macross";
            foreach (var i in SourceCodes)
            {
                string beforeStr = null;
                string afterStr = null;
                var code = System.IO.File.ReadAllText(i);
                var saved_code = code;
                var istart = code.IndexOf(Start_String);
                if (istart >= 0)
                {
                    beforeStr = code.Substring(0, istart);
                    var iend = code.IndexOf(End_String, istart);
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

                ClassDefines.Clear();

                foreach (var j in root.Members)
                {
                    IterateClass(root, j);
                }

                foreach (var j in ClassDefines)
                {
                    var klsDefine = j.Value;
                    klsDefine.Build();
                }

                string genCode = "";
                foreach (var j in ClassDefines)
                {
                    //j.Value.Usings.Clear();
                    //j.Value.GenCode(null);
                    (j.Value as UMacrossClassDefine).GenCode(dir);
                    if (j.Value.ClassCode.Length > 0)
                    {
                        genCode += j.Value.ClassCode;
                        //genCode += "\r\n";
                    }
                }

                if (genCode.Length > 0)
                {
                    if (code.EndsWith("\r\n") == false)
                        code += "\r\n";

                    code += Start_String;
                    code += "\r\n";

                    code += "#region TitanEngine_AutoGen_Macross\r\n";

                    code += genCode;

                    code += "#endregion//TitanEngine_AutoGen_Macross\r\n";

                    code += End_String;

                    if (saved_code != code)
                        System.IO.File.WriteAllText(i, code);
                }
            }
        }
    }
}

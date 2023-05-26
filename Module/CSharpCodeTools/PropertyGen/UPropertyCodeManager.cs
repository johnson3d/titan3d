using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeTools.PropertyGen
{
    class UPropertyCodeManager : UCodeManagerBase
    {
        public static UPropertyCodeManager Instance = new UPropertyCodeManager();
        protected override UClassCodeBase CreateClassDefine(string fullname)
        {
            var result = new UPropertyClassDefine();
            result.FullName = fullname;
            return result;
        }
        protected override bool CheckSourceCode(string code)
        {
            if (code.Contains("UAutoSync"))
                return true;
            return false;
        }
        public override void IterateClass(CompilationUnitSyntax root, MemberDeclarationSyntax decl)
        {
            switch (decl.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    {
                        var kls = decl as ClassDeclarationSyntax;
                        var fullname = ClassDeclarationSyntaxExtensions.GetFullName(kls);
                        
                        if (GetClassAttribute(kls))
                        {
                            var klsDeffine = FindOrCreate(fullname) as UPropertyClassDefine;
                            foreach (var i in root.Usings)
                            {
                                klsDeffine.Usings.Add(i.ToString());
                            }
                        }
                        else
                        {
                            return;
                        }

                        foreach (var i in kls.Members)
                        {
                            if (i.Kind() == SyntaxKind.PropertyDeclaration)
                            {
                                var field = i as PropertyDeclarationSyntax;
                                foreach (var j in field.AttributeLists)
                                {
                                    foreach (var k in j.Attributes)
                                    {
                                        var attributeName = k.Name.NormalizeWhitespace().ToFullString();
                                        if (attributeName.EndsWith("UAutoSync") || attributeName.EndsWith("UAutoSyncAttribute"))
                                        {
                                            var klsDeffine = FindOrCreate(fullname) as UPropertyClassDefine;
                                            var prop = new UPropertyField();
                                            prop.Type = field.Type.ToString();
                                            prop.Name = field.Identifier.Text;
                                            klsDeffine.Properties.Add(prop);
                                            if (k.ArgumentList != null)
                                            {
                                                foreach (var m in k.ArgumentList.Arguments)
                                                {
                                                    var argName = m.NormalizeWhitespace().ToFullString();
                                                    if (m.NameEquals != null && m.Expression != null)
                                                    {
                                                        var name = m.NameEquals.Name.Identifier.ValueText;
                                                        if (name == "Index")
                                                        {
                                                            var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                            prop.Index = value;
                                                        }
                                                        //else if (name == "DirtyFlags")
                                                        //{
                                                        //    var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                        //    prop.DirtyFlags = System.Convert.ToBoolean(value);
                                                        //}
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
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

        protected override bool GetClassAttribute(ClassDeclarationSyntax decl)
        {
            foreach (var i in decl.AttributeLists)
            {
                foreach (var j in i.Attributes)
                {
                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                    if (attributeName.EndsWith("UAutoSyncAttribute") || attributeName.EndsWith("UAutoSync"))
                    {
                        //foreach (var m in j.ArgumentList.Arguments)
                        //{
                        //    var argName = m.NormalizeWhitespace().ToFullString();
                        //    if (m.NameEquals != null && m.Expression != null)
                        //    {
                        //        var name = m.NameEquals.Name.Identifier.ValueText;
                        //        if (name == "IsOverrideBitset")
                        //        {
                        //            IsOverrideBitset = System.Convert.ToBoolean(m.Expression.NormalizeWhitespace().ToFullString());
                        //        }
                        //    }
                        //}
                        return true;
                    }
                }
            }
            return false;
        }
        public void GatherAutoSyncClass(string dir)
        {
            const string Start_String = "#if TitanEngine_AutoGen_AutoSync";
            const string End_String = "#endif//TitanEngine_AutoGen_AutoSync";
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
                    (j.Value as UPropertyClassDefine).GenCode(dir, i);
                    if (j.Value.ClassCode.Length > 0)
                    {
                        genCode += j.Value.ClassCode;
                        //genCode += "\r\n";
                    }
                }

                if (genCode.Length > 0)
                {
                    code += Start_String;
                    code += "\r\n";

                    code += "#region TitanEngine_AutoGen_AutoSync\r\n";

                    code += genCode;

                    code += "#endregion//TitanEngine_AutoGen_AutoSync\r\n";

                    code += End_String;

                    if (saved_code != code)
                        System.IO.File.WriteAllText(i, code);
                }
            }
        }
    }
}

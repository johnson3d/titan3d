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
            return code.Contains("UAutoSync");
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
                            var klsDefine = FindOrCreate(fullname) as UPropertyClassDefine;
                            foreach (var i in root.Usings)
                            {
                                klsDefine.Usings.Add(i.ToString());
                            }
                        }
                        else
                        {
                            return;
                        }

                        foreach (var i in kls.Members)
                        {
                            if (i.Kind() != SyntaxKind.PropertyDeclaration)
                                continue;

                            var field = i as PropertyDeclarationSyntax;
                            foreach (var j in field.AttributeLists)
                            {
                                foreach (var k in j.Attributes)
                                {
                                    var attributeName = k.Name.NormalizeWhitespace().ToFullString();
                                    if (!attributeName.EndsWith("UAutoSync") && !attributeName.EndsWith("UAutoSyncAttribute"))
                                        continue;

                                    var klsDefine = FindOrCreate(fullname) as UPropertyClassDefine;
                                    var prop = new UPropertyField
                                    {
                                        Type = field.Type.ToString(),
                                        Name = field.Identifier.Text
                                    };
                                    klsDefine.Properties.Add(prop);
                                    if (k.ArgumentList != null)
                                    {
                                        foreach (var m in k.ArgumentList.Arguments)
                                        {
                                            if (m.NameEquals == null || m.Expression == null)
                                                continue;

                                            var name = m.NameEquals.Name.Identifier.ValueText;
                                            if (name == "Index")
                                            {
                                                prop.Index = m.Expression.NormalizeWhitespace().ToFullString();
                                            }
                                        }
                                    }
                                    break;
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
                        return true;
                }
            }
            return false;
        }

        public void GatherAutoSyncClass(string dir)
        {
            const string StartString = "#if TitanEngine_AutoGen_AutoSync";
            const string EndString = "#endif//TitanEngine_AutoGen_AutoSync";
            foreach (var i in SourceCodes)
            {
                string beforeStr = null;
                string afterStr = null;
                var code = System.IO.File.ReadAllText(i);
                var savedCode = code;
                var iStart = code.IndexOf(StartString);
                if (iStart >= 0)
                {
                    beforeStr = code.Substring(0, iStart);
                    var iEnd = code.IndexOf(EndString, iStart);
                    if (iEnd >= 0)
                    {
                        afterStr = code.Substring(iEnd + EndString.Length);
                    }
                }
                if (beforeStr != null)
                {
                    code = beforeStr + afterStr;
                }

                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetCompilationUnitRoot();

                ClassDefines.Clear();

                foreach (var j in root.Members)
                {
                    IterateClass(root, j);
                }

                foreach (var j in ClassDefines)
                {
                    j.Value.Build();
                }

                var genCode = "";
                foreach (var j in ClassDefines)
                {
                    (j.Value as UPropertyClassDefine).GenCode(dir, i);
                    if (j.Value.ClassCode.Length > 0)
                    {
                        genCode += j.Value.ClassCode;
                    }
                }

                if (genCode.Length > 0)
                {
                    code += StartString;
                    code += "\r\n";
                    code += "#region TitanEngine_AutoGen_AutoSync\r\n";
                    code += genCode;
                    code += "#endregion//TitanEngine_AutoGen_AutoSync\r\n";
                    code += EndString;
                }

                if (savedCode != code)
                    System.IO.File.WriteAllText(i, code);
            }
        }
    }
}

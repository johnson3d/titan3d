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

                        if (ProjectDeclarations[fullname].Any(GetClassAttribute))
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
                                    if (string.IsNullOrWhiteSpace(prop.Index))
                                        throw SourceError(field, fullname + "." + prop.Name + " 必须显式声明 AutoSync Index");
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
            GatherClass();
            foreach (var definition in ClassDefines.Values.OrderBy(d => d.FullName, StringComparer.Ordinal))
            {
                definition.ResetWriter();
                ((UPropertyClassDefine)definition).GenCode(null, "");
                WriteGeneratedCode(definition, dir, ".prop.cs");
            }
        }
    }
}

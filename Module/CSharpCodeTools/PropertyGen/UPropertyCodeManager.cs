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
            if (code.Contains("GenMeta"))
                return true;
            return false;
        }
        public void WriteCode(string dir)
        {
            foreach (var i in ClassDefines)
            {
                i.Value.GenCode(dir);
            }
        }
        public override void IterateClass(CompilationUnitSyntax root, MemberDeclarationSyntax decl)
        {
            switch (decl.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    {   
                        var kls = decl as ClassDeclarationSyntax;
                        var fullname = ClassDeclarationSyntaxExtensions.GetFullName(kls);
                        {
                            var klsDeffine = FindOrCreate(fullname);
                            foreach (var i in root.Usings)
                            {
                                klsDeffine.Usings.Add(i.ToString());
                            }
                        }
                        bool IsOverrideBitset;
                        if (GetClassAttribute(kls, out IsOverrideBitset))
                        {
                            var klsDeffine = FindOrCreate(fullname) as UPropertyClassDefine;
                            klsDeffine.IsOverrideBitset = IsOverrideBitset;
                        }
                        foreach (var i in kls.Members)
                        {
                            if (i.Kind() == SyntaxKind.FieldDeclaration)
                            {
                                var field = i as FieldDeclarationSyntax;
                                foreach (var j in field.AttributeLists)
                                {
                                    foreach (var k in j.Attributes)
                                    {
                                        var attributeName = k.Name.NormalizeWhitespace().ToFullString();
                                        if (attributeName.EndsWith("GenMeta") || attributeName.EndsWith("GenMetaAttribute"))
                                        {
                                            var klsDeffine = FindOrCreate(fullname) as UPropertyClassDefine;
                                            var prop = new UPropertyField();
                                            prop.Type = field.Declaration.Type.ToString();
                                            prop.Name = field.Declaration.Variables.First().Identifier.Text;
                                            klsDeffine.Properties.Add(prop);
                                            if (k.ArgumentList != null)
                                            {
                                                foreach (var m in k.ArgumentList.Arguments)
                                                {
                                                    var argName = m.NormalizeWhitespace().ToFullString();
                                                    if (m.NameEquals != null && m.Expression != null)
                                                    {
                                                        var name = m.NameEquals.Name.Identifier.ValueText;
                                                        if (name == "Flags")
                                                        {
                                                            var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                            prop.Flags = value;
                                                        }
                                                        else if (name == "DirtyFlags")
                                                        {
                                                            var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                            prop.DirtyFlags = System.Convert.ToBoolean(value);
                                                        }
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

        protected bool GetClassAttribute(ClassDeclarationSyntax decl, out bool IsOverrideBitset)
        {
            IsOverrideBitset = true;
            foreach (var i in decl.AttributeLists)
            {
                foreach (var j in i.Attributes)
                {
                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                    if (attributeName.EndsWith("GenMetaClassAttribute") || attributeName.EndsWith("GenMetaClass"))
                    {
                        foreach (var m in j.ArgumentList.Arguments)
                        {
                            var argName = m.NormalizeWhitespace().ToFullString();
                            if (m.NameEquals != null && m.Expression != null)
                            {
                                var name = m.NameEquals.Name.Identifier.ValueText;
                                if (name == "IsOverrideBitset")
                                {
                                    IsOverrideBitset = System.Convert.ToBoolean(m.Expression.NormalizeWhitespace().ToFullString());
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

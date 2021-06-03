using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeTools.Cs2Cpp
{
    class UCs2CppManager : UCodeManagerBase
    {
        public string Pch = "";
        public static UCs2CppManager Instance = new UCs2CppManager();
        public Dictionary<string, UCs2CppClassDefine> ClassDefines = new Dictionary<string, UCs2CppClassDefine>();
        
        public UCs2CppClassDefine FindOrCreate(string fullname)
        {
            UCs2CppClassDefine result;
            if (ClassDefines.TryGetValue(fullname, out result))
            {
                return result;
            }
            result = new UCs2CppClassDefine();
            result.FullName = fullname;
            ClassDefines.Add(fullname, result);

            return result;
        }
        protected override bool CheckSourceCode(string code)
        {
            if (code.Contains("UCs2Cpp"))
                return true;
            return false;
        }

        public void GatherClass()
        {
            foreach (var i in SourceCodes)
            {
                var code = System.IO.File.ReadAllText(i);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                foreach (var j in root.Members)
                {
                    IterateClass(root, j);
                }
            }

            foreach(var i in ClassDefines)
            {
                i.Value.Build();
            }
        }
        public void WriteCode(string dir)
        {
            foreach (var i in ClassDefines)
            {
                i.Value.GenCode(dir);
            }
        }
        private void IterateClass(CompilationUnitSyntax root, MemberDeclarationSyntax decl)
        {
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
                                klsDeffine.Usings.Add(i.ToString());
                            }
                        }
                        foreach (var i in kls.Members)
                        {
                            if (i.Kind() == SyntaxKind.MethodDeclaration)
                            {
                                var method = i as MethodDeclarationSyntax;
                                foreach (var j in method.AttributeLists)
                                {
                                    foreach (var k in j.Attributes)
                                    {
                                        var attributeName = k.Name.NormalizeWhitespace().ToFullString();
                                        if (attributeName.EndsWith("UCs2CppAttribute") || attributeName.EndsWith("UCs2Cpp"))
                                        {
                                            var klsDeffine = FindOrCreate(fullname);
                                            var tmp = new UCppCallback();
                                            tmp.MethodSyntax = method;
                                            tmp.Name = method.Identifier.Text;
                                            klsDeffine.Callbacks.Add(tmp);
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

        private bool GetClassAttribute(ClassDeclarationSyntax decl)
        {
            foreach (var i in decl.AttributeLists)
            {
                foreach (var j in i.Attributes)
                {
                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                    if (attributeName.EndsWith("UCs2CppAttribute") || attributeName.EndsWith("UCs2Cpp"))
                    {
                        if (j.ArgumentList != null)
                        {
                            foreach (var m in j.ArgumentList.Arguments)
                            {
                                var argName = m.NormalizeWhitespace().ToFullString();
                                if (m.NameEquals != null && m.Expression != null)
                                {
                                    var name = m.NameEquals.Name.Identifier.ValueText;
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TypeNameMappper(TypeSyntax typeSyntax, UProperty prop)
        {
            if (typeSyntax.Kind() == SyntaxKind.RefType)
                return false;

            prop.NumOfPointer = 0;
            var typeName = typeSyntax.ToString();
            while (typeSyntax!=null && typeSyntax.Kind() == SyntaxKind.PointerType)
            {
                prop.NumOfPointer++;
                var ptrType = typeSyntax as PointerTypeSyntax;
                typeSyntax = ptrType.ElementType;
            }


            var pureTypeName = typeSyntax.ToString();
            if (typeSyntax.IsKind(SyntaxKind.PredefinedType))
            {
                prop.Type = pureTypeName;
                return true;
            }
            else if (pureTypeName == "IntPtr" || pureTypeName == "System.IntPtr")
            {
                prop.Type = "System.IntPtr";
                return true;
            }
            else if (pureTypeName == "Vector2" || pureTypeName == "EngineNS.Vector2")
            {
                prop.Type = "EngineNS.Vector2";
                return true;
            }
            else if (pureTypeName == "Vector3" || pureTypeName == "EngineNS.Vector3")
            {
                prop.Type = "EngineNS.Vector3";
                return true;
            }
            else if (pureTypeName == "Quaternion" || pureTypeName == "EngineNS.Quaternion")
            {
                prop.Type = "EngineNS.Quaternion";
                return true;
            }
            else if (pureTypeName == "Matrix" || pureTypeName == "EngineNS.Matrix")
            {
                prop.Type = "EngineNS.Matrix";
                return true;
            }
            else if (pureTypeName == "UAnyValue" || pureTypeName == "Support.UAnyValue" || pureTypeName == "EngineNS.Support.UAnyValue")
            {
                prop.Type = "EngineNS.Support.UAnyValue";
                return true;
            }
            else
            {
                if (UCs2CppManager.Instance.ClassDefines.ContainsKey(pureTypeName))
                {
                    prop.Kind = UProperty.ETypeKind.CSBinder;
                    prop.Type = typeName;
                    return true;
                }
                else
                {
                    prop.Kind = UProperty.ETypeKind.Object;
                    prop.Type = typeName;
                    return true;
                    //return false;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeTools
{
    class URpcCodeManager : UCodeManagerBase
    {
        public static URpcCodeManager Instance = new URpcCodeManager();

        protected override bool CheckSourceCode(string code)
        {
            if (code.Contains("URpcMethod"))
                return true;
            return false;
        }
        protected override UClassCodeBase CreateClassDefine(string fullname)
        {
            var result = new URpcClassDefine();
            result.FullName = fullname;
            return result;
        }
        protected override void OnVisitMethod(UClassCodeBase kls, MethodDeclarationSyntax method)
        {

        }
        public void WriteCode(string dir)
        {
            foreach(var i in ClassDefines)
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
                        string target, executer;
                        if (GetRpcClassAttribute(kls, out target, out executer))
                        {
                            var klsDeffine = FindOrCreate(fullname) as URpcClassDefine;
                            klsDeffine.RunTarget = target;
                            klsDeffine.Executer = executer;
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
                                        if (attributeName.EndsWith("URpcMethodAttribute") || attributeName.EndsWith("URpcMethod"))
                                        {
                                            var klsDeffine = FindOrCreate(fullname) as URpcClassDefine;
                                            URpcMethod rpcMethod = new URpcMethod();
                                            rpcMethod.Name = method.Identifier.Text;
                                            if (method.ToString().Contains("async "))
                                            {
                                                if (method.ToString().IndexOf("async ") < method.ToString().IndexOf(rpcMethod.Name))
                                                    rpcMethod.IsAsync = true;
                                            }
                                            klsDeffine.Methods.Add(rpcMethod);
                                            rpcMethod.ArgDataType = URpcMethod.EDataType.Unmanaged;
                                            bool bReturnISerializer = false;
                                            foreach (var m in k.ArgumentList.Arguments)
                                            {
                                                var argName = m.NormalizeWhitespace().ToFullString();
                                                if (m.NameEquals != null && m.Expression != null)
                                                {
                                                    var name = m.NameEquals.Name.Identifier.ValueText;
                                                    if (name == "Index")
                                                    {
                                                        var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                        rpcMethod.Index = value;
                                                    }
                                                    else if (name == "ReturnISerializer")
                                                    {
                                                        var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                        bReturnISerializer = System.Convert.ToBoolean(value);
                                                    }
                                                    else if (name == "ArgISerializer")
                                                    {
                                                        var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                        rpcMethod.ArgDataType = URpcMethod.EDataType.ISerializer;
                                                    }
                                                }
                                            }

                                            var a = method.ParameterList.Parameters[0];
                                            rpcMethod.ArgType = a.Type.ToString();
                                            rpcMethod.ArgName = a.Identifier.ValueText;
                                            
                                            if (method.ReturnType.ToString() == "System.Void" || method.ReturnType.ToString() == "void")
                                            {
                                                rpcMethod.ReturnType = null;
                                                rpcMethod.RetType = URpcMethod.EDataType.Void;
                                            }
                                            else if (method.ReturnType.ToString() == "System.String" || method.ReturnType.ToString() == "string")
                                            {
                                                rpcMethod.ReturnType = "string";
                                                rpcMethod.RetType = URpcMethod.EDataType.String;
                                            }
                                            else if (bReturnISerializer)
                                            {
                                                rpcMethod.RetType = URpcMethod.EDataType.ISerializer;
                                                rpcMethod.ReturnType = method.ReturnType.ToString();
                                            }
                                            else
                                            {
                                                rpcMethod.RetType = URpcMethod.EDataType.Unmanaged;
                                                rpcMethod.ReturnType = method.ReturnType.ToString();
                                                //if (method.ReturnType.Kind() == SyntaxKind.PredefinedType || method.ReturnType.IsUnmanaged)
                                                //    rpcMethod.RetType = URpcMethod.ERetType.Unmanaged; 
                                                //else
                                                //    rpcMethod.RetType = URpcMethod.ERetType.ISerializer;
                                                //rpcMethod.ReturnType = method.ReturnType.ToString();
                                            }
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
                        foreach(var i in ns.Members)
                        {
                            IterateClass(root, i);
                        }
                    }
                    break;
            }
        }
        
        private bool GetRpcClassAttribute(ClassDeclarationSyntax decl, out string target, out string executer)
        {
            target = null;
            executer = null;
            foreach (var i in decl.AttributeLists)
            {
                foreach (var j in i.Attributes)
                {
                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                    if (attributeName.EndsWith("URpcClassAttribute") || attributeName.EndsWith("URpcClass"))
                    {
                        target = "EngineNS.Bricks.Network.RPC.ERunTarget.None";
                        executer = "EngineNS.Bricks.Network.RPC.EExecuter.Root";
                        foreach (var m in j.ArgumentList.Arguments)
                        {
                            var argName = m.NormalizeWhitespace().ToFullString();
                            if (m.NameEquals != null && m.Expression != null)
                            {
                                var name = m.NameEquals.Name.Identifier.ValueText;
                                if (name == "RunTarget")
                                {
                                    target = m.Expression.NormalizeWhitespace().ToFullString();
                                }
                                else if (name == "Executer")
                                {
                                    executer = m.Expression.NormalizeWhitespace().ToFullString();
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

    public static class ClassDeclarationSyntaxExtensions
    {
        public const string NESTED_CLASS_DELIMITER = "+";
        public const string NAMESPACE_CLASS_DELIMITER = ".";

        public static string GetFullName(this ClassDeclarationSyntax source)
        {
            Contract.Requires(null != source);

            var items = new List<string>();
            var parent = source.Parent;
            while (parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                var parentClass = parent as ClassDeclarationSyntax;
                Contract.Assert(null != parentClass);
                items.Add(parentClass.Identifier.Text);

                parent = parent.Parent;
            }

            var nameSpace = parent as NamespaceDeclarationSyntax;
            Contract.Assert(null != nameSpace);
            var sb = new StringBuilder().Append(nameSpace.Name).Append(NAMESPACE_CLASS_DELIMITER);
            items.Reverse();
            items.ForEach(i => { sb.Append(i).Append(NESTED_CLASS_DELIMITER); });
            sb.Append(source.Identifier.Text);

            var result = sb.ToString();
            return result;
        }
    }
}

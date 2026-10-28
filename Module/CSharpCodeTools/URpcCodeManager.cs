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
        const string RpcAttributeName = "TtRpcMethod";
        const string RpcClassAttributeName = "TtRpcClass";
        public static URpcCodeManager Instance = new URpcCodeManager();

        protected override bool CheckSourceCode(string code)
        {
            if (code.Contains(RpcAttributeName))
                return true;
            return false;
        }
        protected override UClassCodeBase CreateClassDefine(string fullname)
        {
            var result = new URpcClassDefine();
            result.FullName = fullname;
            return result;
        }
        public void GatherRpcClass(string dir, string callerDir)
        {
            GatherClass();
            foreach (var definition in ClassDefines.Values.OrderBy(d => d.FullName, StringComparer.Ordinal))
            {
                var rpc = (URpcClassDefine)definition;
                // 共享调用方在另一程序集编译，嵌套类型无法在该程序集重建外层类型。
                if (!rpc.CallerInClass && rpc.Methods.Count > 0 && rpc.Declaration.Parent is TypeDeclarationSyntax)
                    throw SourceError(rpc.Declaration, rpc.FullName + " 是嵌套类型，必须声明 TtRpcClass(CallerInClass=true)");
                // 共享调用方与基线一致，不携带源码 using，避免依赖拥有工程私有命名空间。
                rpc.ResetWriter();
                rpc.GenCallerCode();
                WriteGeneratedCode(rpc, callerDir, ".rpc.cs", "|caller", false);
                rpc.ResetWriter();
                rpc.GenCode();
                WriteGeneratedCode(rpc, dir, ".rpc.cs");
            }
        }
        protected override void OnVisitMethod(UClassCodeBase kls, MethodDeclarationSyntax method)
        {

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
                        {
                            var klsDeffine = FindOrCreate(fullname);
                            foreach (var i in root.Usings)
                            {
                                klsDeffine.Usings.Add(i.ToString());
                            }
                        }
                        foreach (var partial in ProjectDeclarations[fullname])
                        {
                            if (!GetRpcClassAttribute(partial, out var target, out var executer, out var callerInClass)) continue;
                            var klsDeffine = FindOrCreate(fullname) as URpcClassDefine;
                            klsDeffine.RunTarget = target;
                            klsDeffine.Executer = executer;
                            klsDeffine.CallerInClass = callerInClass;
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
                                        if (attributeName.EndsWith(RpcAttributeName+"Attribute") || attributeName.EndsWith(RpcAttributeName))
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
                                            if (klsDeffine.RunTarget == null)
                                                throw SourceError(method, fullname + "." + rpcMethod.Name + " 缺少 TtRpcClass");
                                            if (k.ArgumentList == null)
                                                throw SourceError(method, fullname + "." + rpcMethod.Name + " 必须显式声明 RPC Index");
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
                                                    else if (name == "PkgFlags")
                                                    {
                                                        var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                        rpcMethod.Flags = value;
                                                    }
                                                    else if (name == "IsBroadCaster")
                                                    {
                                                        var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                        rpcMethod.IsBroadCaster = value=="true" ? true : false;
                                                    }
                                                }
                                            }

                                            if (string.IsNullOrWhiteSpace(rpcMethod.Index))
                                                throw SourceError(method, fullname + "." + rpcMethod.Name + " 必须显式声明 RPC Index");
                                            rpcMethod.ArgTypes.Clear();
                                            var lstParam = method.ParameterList.Parameters.LastOrDefault();
                                            if (lstParam?.Type?.ToString().Split('.').Last() != "TtCallContext")
                                                throw SourceError(method, fullname + "." + rpcMethod.Name + " 的末参必须是 TtCallContext");
                                            for (int n = 0; n < method.ParameterList.Parameters.Count - 1; n++)
                                            {
                                                var a = method.ParameterList.Parameters[n];
                                                var ta = new KeyValuePair<string, string>(a.Type.ToString(), a.Identifier.ValueText);
                                                rpcMethod.ArgTypes.Add(ta);
                                            }
                                            
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
                                            else if (method.ReturnType.ToString() == "EngineNS.IO.TtMemWriter" || method.ReturnType.ToString() == "IO.TtMemWriter"|| method.ReturnType.ToString() == "TtMemWriter")
                                            {
                                                rpcMethod.ReturnType = "EngineNS.IO.TtMemWriter";
                                                rpcMethod.RetType = URpcMethod.EDataType.MemWriter;
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
        
        private bool GetRpcClassAttribute(ClassDeclarationSyntax decl, out string target, out string executer, out bool callerInClass)
        {
            target = null;
            executer = null;
            callerInClass = false;
            foreach (var i in decl.AttributeLists)
            {
                foreach (var j in i.Attributes)
                {
                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                    if (attributeName.EndsWith(RpcClassAttributeName+"Attribute") || attributeName.EndsWith(RpcClassAttributeName))
                    {
                        target = "EngineNS.Bricks.Network.RPC.ERunTarget.None";
                        executer = "EngineNS.Bricks.Network.RPC.EExecuter.Root";
                        foreach (var m in j.ArgumentList?.Arguments ?? default)
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
                                else if (name == "CallerInClass")
                                {
                                    if (m.Expression.NormalizeWhitespace().ToFullString() == "true")
                                        callerInClass = true;
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
            var items = source.Ancestors().Reverse().Select(node => node switch
            {
                BaseNamespaceDeclarationSyntax scope => scope.Name.ToString(),
                TypeDeclarationSyntax type => type.Identifier.Text + type.TypeParameterList,
                _ => null
            }).Where(name => name != null).ToList();
            items.Add(source.Identifier.Text + source.TypeParameterList);
            return string.Join(".", items);
        }
    }
}

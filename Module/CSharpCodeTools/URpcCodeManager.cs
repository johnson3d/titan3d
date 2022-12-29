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
        public void GatherRpcClass(string dir)
        {
            const string Start_String = "#if TitanEngine_AutoGen";
            const string End_String = "#endif//TitanEngine_AutoGen";
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
                    (j.Value as URpcClassDefine).GenCode(dir, i);
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

                    code += "#region TitanEngine_AutoGen\r\n";

                    code += genCode;

                    code += "#endregion//TitanEngine_AutoGen\r\n";

                    code += End_String;

                    if (saved_code != code)
                        System.IO.File.WriteAllText(i, code);
                }
            }
        }
        protected override void OnVisitMethod(UClassCodeBase kls, MethodDeclarationSyntax method)
        {

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
                        bool callerInClass = false;
                        if (GetRpcClassAttribute(kls, out target, out executer, out callerInClass))
                        {
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
                                                    else if (name == "PkgFlags")
                                                    {
                                                        var value = m.Expression.NormalizeWhitespace().ToFullString();
                                                        rpcMethod.Flags = value;
                                                    }
                                                }
                                            }

                                            rpcMethod.ArgTypes.Clear();
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

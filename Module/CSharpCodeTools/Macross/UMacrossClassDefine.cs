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
    class UMacrossFunction
    {
        public MethodDeclarationSyntax MethodSyntax;
        public int ParamenterCount
        {
            get
            {
                return MethodSyntax.ParameterList.Parameters.Count;
            }
        }
        public bool IsStatic
        {
            get
            {
                foreach(var i in MethodSyntax.Modifiers)
                {
                    if (i.ValueText == "static")
                        return true;
                }
                return false;
            }
        }
        public bool IsGeneric
        {
            get
            {
                return MethodSyntax.ConstraintClauses.Count > 0;
                //foreach (var i in MethodSyntax.ConstraintClauses)
                //{
                //    //if (i.Constraints. == "where")
                //        return true;
                //}
                //return false;
            }
        }
        public bool IsAsync
        {
            get
            {
                foreach (var i in MethodSyntax.Modifiers)
                {
                    if (i.ValueText == "async")
                        return true;
                }
                return false;
            }
        }
        public string GetDefineString(UMacrossClassDefine kls)
        {
            string result = kls.FullName + "->";
            var staticPrefix = IsStatic ? "static " : "";
            result += $"{staticPrefix}{MethodSyntax.ReturnType.ToString()} {MethodSyntax.Identifier.ValueText}({GetParameterDefine()})";
            return result;
        }
        public static int GetStableHashCode(string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
        public uint GetParameterHashCode()
        {
            return (uint)GetStableHashCode(GetParameterDefine() + IsStatic.ToString());
        }
        public string GetParameterDefine()
        {
            string result = "";
            foreach(var i in MethodSyntax.ParameterList.Parameters)
            {
                string mode = "";
                if (i.ToString().StartsWith("out "))
                {
                    mode = "out ";
                }
                else if (i.ToString().StartsWith("in "))
                {
                    mode = "in ";
                }
                else if (i.ToString().StartsWith("ref "))
                {
                    mode = "ref ";
                }
                if (result=="")
                {
                    result += $"{mode}{i.Type.ToString()} {i.Identifier.ValueText}";
                }
                else
                {
                    result += $", {mode}{i.Type.ToString()} {i.Identifier.ValueText}";
                }
            }
            return result;
        }
        public string GetParameterCallee()
        {
            string result = "";
            foreach (var i in MethodSyntax.ParameterList.Parameters)
            {
                string mode = "";
                if (i.ToString().StartsWith("out "))
                {
                    mode = "out ";
                }
                else if (i.ToString().StartsWith("in "))
                {
                    mode = "in ";
                }
                else if (i.ToString().StartsWith("ref "))
                {
                    mode = "ref ";
                }
                if (result == "")
                {
                    result += $"{mode}{i.Identifier.ValueText}";
                }
                else
                {
                    result += $", {mode}{i.Identifier.ValueText}";
                }
            }
            return result;
        }
    }
    class UMacrossClassDefine : UClassCodeBase
    {
        public List<UMacrossFunction> Functions { get; } = new List<UMacrossFunction>();
        public override void GenCode(string dir)
        {
            if (Functions.Count == 0)
                return;
            foreach (var i in Usings)
            {
                AddLine(i);
            }
            NewLine();

            AddLine($"namespace {this.Namespace}");
            PushBrackets();
            {
                AddLine($"partial class {this.Name}");
                PushBrackets();
                {
                    foreach (var i in Functions)
                    {
                        string isStatic = "";
                        foreach (var j in i.MethodSyntax.Modifiers)
                        {
                            if (j.ValueText == "static")
                            {
                                isStatic = "static ";
                            }
                        }

                        string asyncStr = "";
                        var isAsync = i.IsAsync;
                        if (isAsync)
                            asyncStr = "async ";
                        else
                            asyncStr = "unsafe ";
                        var funName = i.GetDefineString(this);
                        string constraint = "";
                        string genericDef = "";
                        if (i.IsGeneric)
                        {
                            genericDef = "<";
                            foreach (var j in i.MethodSyntax.ConstraintClauses)
                            {
                                if (genericDef == "<")
                                {
                                    genericDef += j.Name.ToString();
                                }
                                else
                                {
                                    genericDef += "," + j.Name.ToString();
                                }
                                constraint += j.ToString() + " ";
                            }
                            genericDef += ">";
                        }
                        
                        AddLine($"private static EngineNS.Macross.UMacrossBreak macross_break_{i.MethodSyntax.Identifier.Text}_{i.GetParameterHashCode()} = new EngineNS.Macross.UMacrossBreak(\"{funName}\");");
                        if (i.ParamenterCount > 0)
                            AddLine($"public {isStatic}{asyncStr}{i.MethodSyntax.ReturnType.ToString()} macross_{i.MethodSyntax.Identifier.Text} {genericDef}(string nodeName, {i.GetParameterDefine()}) {constraint}");
                        else
                            AddLine($"public {isStatic}{asyncStr}{i.MethodSyntax.ReturnType.ToString()} macross_{i.MethodSyntax.Identifier.Text} {genericDef}(string nodeName) {constraint}");
                        PushBrackets();
                        {
                            bool hasOut = false;
                            AddLine($"using(var stackframe = EngineNS.Macross.UMacrossStackTracer.CurrentFrame)");
                            PushBrackets();
                            {
                                AddLine($"if(stackframe != null)");
                                PushBrackets();
                                {
                                    foreach (var j in i.MethodSyntax.ParameterList.Parameters)
                                    {
                                        if (j.ToString().StartsWith("out "))
                                        {
                                            hasOut = true;
                                            continue;
                                        }
                                        AddLine($"stackframe.SetWatchVariable(nodeName + \":{j.Identifier.ValueText}\", {j.Identifier.ValueText});");
                                    }
                                }
                                PopBrackets();
                            }
                            PopBrackets();

                            bool needReturen = false;
                            if (i.MethodSyntax.ReturnType.ToString() == "void" || i.MethodSyntax.ReturnType.ToString() == "System.Void" || 
                                i.MethodSyntax.ReturnType.ToString() == "System.Threading.Tasks.Task" ||
                                i.MethodSyntax.ReturnType.ToString() == "Task" ||
                                i.MethodSyntax.ReturnType.ToString() == "EngineNS.Thread.Async.TtTask" ||
                                i.MethodSyntax.ReturnType.ToString() == "Thread.Async.TtTask" ||
                                i.MethodSyntax.ReturnType.ToString() == "TtTask")
                            {
                                if (isAsync)
                                    AddLine($"await {i.MethodSyntax.Identifier.Text}({i.GetParameterCallee()});");
                                else
                                    AddLine($"{i.MethodSyntax.Identifier.Text}({i.GetParameterCallee()});");
                            }
                            else
                            {
                                needReturen = true;
                                if (isAsync)
                                    AddLine($"var _return_value = await {i.MethodSyntax.Identifier.Text}{genericDef}({i.GetParameterCallee()});");
                                else
                                    AddLine($"var _return_value = {i.MethodSyntax.Identifier.Text}{genericDef}({i.GetParameterCallee()});");
                            }

                            if (hasOut)
                            {
                                AddLine($"using(var stackframe = EngineNS.Macross.UMacrossStackTracer.CurrentFrame)");
                                PushBrackets();
                                {
                                    AddLine($"if(stackframe != null)");
                                    PushBrackets();
                                    {
                                        foreach (var j in i.MethodSyntax.ParameterList.Parameters)
                                        {
                                            if (j.ToString().StartsWith("out "))
                                            {
                                                AddLine($"stackframe.SetWatchVariable(nodeName + \":{j.Identifier.ValueText}\", {j.Identifier.ValueText});");
                                            }
                                        }
                                    }
                                    PopBrackets();
                                }
                                PopBrackets();
                            }

                            AddLine($"macross_break_{i.MethodSyntax.Identifier.Text}_{i.GetParameterHashCode()}.TryBreak();");

                            if (needReturen)
                            {
                                AddLine($"return _return_value;");
                            }
                        }
                        PopBrackets();
                    }
                }
                PopBrackets();
            }
            PopBrackets();

            var file = dir + "/" + FullName + ".macross.cs";
            if (!UMacrossClassManager.Instance.WritedFiles.Contains(file.Replace("\\", "/").ToLower()))
            {
                UMacrossClassManager.Instance.WritedFiles.Add(file.Replace("\\", "/").ToLower());
            }

            if (System.IO.File.Exists(file))
            {
                var oldCode = System.IO.File.ReadAllText(file);
                if (oldCode == ClassCode)
                    return;
            }

            System.IO.File.WriteAllText(file, ClassCode);
        }
    }
}

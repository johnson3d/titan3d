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
        public string GetParameterDefine()
        {
            if(MethodSyntax.Identifier.Text == "SampleLevel2D")
            {
                int xx = 0;
            }
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
        public List<UMacrossFunction> Functions = new List<UMacrossFunction>();
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
                        if (i.ParamenterCount > 0)
                            AddLine($"public {i.MethodSyntax.ReturnType.ToString()} macross_{i.MethodSyntax.Identifier.Text}(string nodeName, {i.GetParameterDefine()})");
                        else
                            AddLine($"public {i.MethodSyntax.ReturnType.ToString()} macross_{i.MethodSyntax.Identifier.Text}(string nodeName)");
                        PushBrackets();
                        {
                            bool hasOut = false;
                            AddLine($"using(var stackframe = EngineNS.Macross.UMacrossStackTracer.CurrentFrame)");
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

                            bool needReturen = false;
                            if (i.MethodSyntax.ReturnType.ToString() == "void" || i.MethodSyntax.ReturnType.ToString() == "System.Void")
                            {
                                AddLine($"{i.MethodSyntax.Identifier.Text}({i.GetParameterCallee()});");
                            }
                            else
                            {
                                needReturen = true;
                                if(hasOut)
                                    AddLine($"var _return_value = {i.MethodSyntax.Identifier.Text}({i.GetParameterCallee()});");
                                else
                                    AddLine($"return {i.MethodSyntax.Identifier.Text}({i.GetParameterCallee()});");
                            }

                            if (hasOut)
                            {
                                AddLine($"using(var stackframe = EngineNS.Macross.UMacrossStackTracer.CurrentFrame)");
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

                                if (needReturen)
                                {
                                    AddLine($"return _return_value;");
                                }
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

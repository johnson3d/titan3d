using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangHeadTools.CSharp
{
    public class CppBinderWriter : ClassWriter
    {   
        public override void GenCode()
        {
            ClangSharp.Interop.CXFile tfile;
            uint line, col, offset;
            mDecl.Location.GetFileLocation(out tfile, out line, out col, out offset);

            bool bLayoutStruct = Parent.HasMeta(SV_LayoutStruct);

            AddLine($"#include \"{PchFile}\"");
            AddLine($"#include \"{GetRegularPath(tfile.ToString())}\"");

            NewLine();
            AddLine($"#define new VNEW");
            AddLine($"using namespace EngineNS;");
            //AddLine($"#pragma warning(disable:4190)"); 
            NewLine();

            if (IsGenCppReflection)
            {
                AddLine($"namespace EngineNS");
                PushBrackets();
                {
                    AddLine($"StructBegin({mDecl.Name},{this.Namespace.Replace(".", "::")})");
                    PushTab();
                    {
                        var fields = mDecl.Fields;//.Decls.OfType<ClangSharp.FieldDecl>();
                        foreach (var i in fields)
                        {
                            if (IsIgnoreField(i))
                                continue;

                            AddLine($"StructMember({i.Name})");
                        }
                        var methods = mDecl.Decls.OfType<ClangSharp.FunctionDecl>();
                        foreach (var i in methods)
                        {
                            if (IsIgnoreFunction(i))
                                continue;

                            if (CheckTypes(i, false) == false)
                                continue;

                            if (i.Name == mDecl.Name)
                            {//构造器后面处理
                                continue;
                            }

                            AppendCode($"StructMethodEx{i.Parameters.Count}(", true, false);
                            AppendCode($"{i.Name}, {GetFullNameCpp(i.ReturnType)}", false, false);
                            foreach (var j in i.Parameters)
                            {
                                AppendCode($", {GetFullNameCpp(j.Type)}, {j.Name}", false, false);
                            }
                            AppendCode(");", false, true);
                        }
                    }
                    PopTab();
                    ClassWriter parentWriter = null;
                    if (mDecl.Bases.Count > 0)
                    {
                        var parent = mDecl.Bases[0].Referenced as ClangSharp.CXXRecordDecl;
                        parentWriter = CodeWriterManager.Instance.FindClass(ClassWriter.GetFullName(parent.TypeForDecl));
                    }
                    if (parentWriter != null)
                    {
                        var ns = parentWriter.Namespace.Replace(".", "::");
                        if (string.IsNullOrEmpty(ns) == false)
                        {
                            ns += "::";
                        }
                        AddLine($"StructEnd({ns}{parentWriter.Name})");
                    }
                    else
                    {
                        AddLine($"StructEnd(void)");
                    }
                    AddLine($"StructImpl({this.FullNameCpp});");
                }
                PopBrackets();
            }

            NewLine();
            NewLine();

            if (IsGenGlue)
                GenPInvoke(bLayoutStruct);
        }
        public static string GetFullNameCpp(ClangSharp.Type decl)
        {
            var result = GetFullName(decl).Replace(".", "::");
            int pos = result.IndexOf('[');
            if (pos>0)
            {
                result = result.Substring(0, pos);
                return result + "*";
            }
            return result;
        }
        public string FullNameCpp
        {
            get
            {
                return FullName.Replace(".", "::");
            }
        }
        private void GenPInvoke(bool bStructLayout)
        {
            var methods = mDecl.Methods;//.Decls.OfType<ClangSharp.FunctionDecl>();
            var fields = mDecl.Fields;// .Decls.OfType<ClangSharp.FieldDecl>();
            AddLine($"namespace {GlueNamespace}");
            PushBrackets();
            {
                AddLine($"struct {mDecl.Name}_Visitor");
                PushBrackets();
                {
                    foreach (var i in methods)
                    {
                        if (IsIgnoreFunction(i))
                            continue;

                        if (CheckTypes(i, false) == false)
                            continue;

                        if (i.Name == mDecl.Name)
                        {//构造器后面处理
                            continue;
                        }

                        string selfArg = $"{FullNameCpp}* self";
                        if (i.IsStatic)
                        {
                            selfArg = "";
                        }
                        else
                        {
                            if (i.Parameters.Count > 0)
                            {
                                selfArg += ",";
                            }
                        }

                        if (i.Parameters.Count > 0)
                            AddLine($"static {GetFullNameCpp(i.ReturnType)} {i.Name}({selfArg} {GetParameterDefine(i)})");
                        else
                            AddLine($"static {GetFullNameCpp(i.ReturnType)} {i.Name}({selfArg})");
                        PushBrackets();
                        {
                            if (i.IsStatic == false)
                            {
                                AddLine($"if(self==nullptr)");
                                PushBrackets();
                                {
                                    var retTypeStr = GetFullNameCpp(i.ReturnType);
                                    if (retTypeStr.EndsWith("&") == false)
                                        AddLine($"return VGetTypeDefault<{GetFullNameCpp(i.ReturnType)}>();");
                                    else
                                    {
                                        retTypeStr = retTypeStr.Substring(0, retTypeStr.Length - 1);
                                        AddLine($"{retTypeStr}* tmp = nullptr;");
                                        AddLine($"return *tmp;");
                                    }
                                }
                                PopBrackets();
                            }
                            if (i.IsStatic)
                            {
                                AddLine($"return {FullNameCpp}::{i.Name}({GetParameterCallee(i)});");
                            }
                            else
                            {
                                AddLine($"return self->{i.Name}({GetParameterCallee(i)});");
                            }
                        }
                        PopBrackets();
                    }
                    AddLine($"///Fields Set&Get");
                    foreach (var i in fields)
                    {
                        bool isDelegate = false;
                        var argType = GetFullNameCpp(i.Type);
                        if (argType.IndexOf("(*)") >= 0)
                        {
                            isDelegate = true;
                        }
                        else
                        {
                            if (IsIgnoreField(i))
                                continue;
                        }

                        if (isDelegate)
                        {
                            argType = argType.Replace("(*)", $"(*value)");
                            AddLine($"static void __FieldSet__{i.Name}({FullNameCpp}* self, {argType})");
                        }
                        else
                        {
                            AddLine($"static void __FieldSet__{i.Name}({FullNameCpp}* self, {GetFullNameCpp(i.Type)} value)");
                        }
                        PushBrackets();
                        {
                            AddLine($"if(self==nullptr)");
                            PushBrackets();
                            {
                                AddLine($"return;");
                            }
                            PopBrackets();

                            if (i.Type.Handle.NumElements > 0)
                            {
                                AddLine($"for (int i = 0; i < {i.Type.Handle.NumElements}; i++)");
                                PushBrackets();
                                {
                                    AddLine($"self->{i.Name}[i] = value[i];");
                                }
                                PopBrackets();
                            }
                            else
                            {
                                AddLine($"self->{i.Name} = value;");
                            }
                        }
                        PopBrackets();

                        if (GetFullNameCpp(i.Type).Contains("(*)"))
                            continue;

                        AddLine($"static {GetFullNameCpp(i.Type)} __FieldGet__{i.Name}({FullNameCpp}* self)");
                        PushBrackets();
                        {
                            AddLine($"if(self==nullptr)");
                            PushBrackets();
                            {
                                AddLine($"return VGetTypeDefault<{GetFullNameCpp(i.Type)}>();");
                            }
                            PopBrackets();
                            AddLine($"return self->{i.Name};");
                        }
                        PopBrackets();
                    }
                }
                PopBrackets(true);
            }
            PopBrackets();

            AddLine($"///Methods");
            if (bStructLayout)
            {
                var ns = FullName.Replace(".", "_");
                AddLine($"extern \"C\" {GlueExporter} void {GluePrefix}_{ns}__Call_Destruct_OnMemory({FullNameCpp}* self)");
                PushBrackets();
                {
                    AddLine($"self->~{Name}();");
                }
                PopBrackets();
            }
            foreach (var i in methods)
            {
                if (IsIgnoreFunction(i))
                    continue;

                if (CheckTypes(i, false) == false)
                    continue;

                if (i.Name == mDecl.Name)
                {//构造器后面处理
                    GenConstructor(i);
                    continue;
                }

                var fn = this.FullName.Replace(".", "_");
                var selfStr = $"{FullNameCpp}* self";
                var selfVarStr = $"self";
                if (i.IsStatic)
                {
                    selfStr = "";
                    selfVarStr = "";
                }
                else
                {
                    if (i.Parameters.Count > 0)
                    {
                        selfStr += ",";
                        selfVarStr += ",";
                    }
                }

                bool bRetConvert = false;
                var retTypeStr = GetFullNameCpp(i.ReturnType);
                if (GetCReturnMapper(retTypeStr) != null)
                {
                    retTypeStr = GetCReturnMapper(retTypeStr);
                    bRetConvert = true;
                }

                if (i.Parameters.Count > 0)
                    AddLine($"extern \"C\" {GlueExporter} {retTypeStr} {GluePrefix}_{fn}_{i.Name}__{GetMethodHash(i)}({selfStr} {GetParameterDefine(i)})");
                else
                    AddLine($"extern \"C\" {GlueExporter} {retTypeStr} {GluePrefix}_{fn}_{i.Name}__{GetMethodHash(i)}({selfStr})");
                PushBrackets();
                {
                    if (bRetConvert)
                    {
                        if (i.Parameters.Count > 0)
                            AddLine($"auto tmp_result = {GlueNamespace}::{mDecl.Name}_Visitor::{i.Name}({selfVarStr} {GetParameterCallee(i)});");
                        else
                            AddLine($"auto tmp_result = {GlueNamespace}::{mDecl.Name}_Visitor::{i.Name}({selfVarStr});");
                        AddLine($"return VReturnValueMarshal<{GetFullNameCpp(i.ReturnType)},{retTypeStr}>(tmp_result);");
                    }
                    else
                    {
                        if (i.Parameters.Count > 0)
                            AddLine($"return {GlueNamespace}::{mDecl.Name}_Visitor::{i.Name}({selfVarStr} {GetParameterCallee(i)});");
                        else
                            AddLine($"return {GlueNamespace}::{mDecl.Name}_Visitor::{i.Name}({selfVarStr});");
                    }
                }
                PopBrackets();
            }

            AddLine($"///Fields Set&Get");
            foreach (var i in fields)
            {
                bool isDelegate = false;
                var fn = Namespace.Replace(".", "_");
                var argType = GetFullNameCpp(i.Type);
                if (argType.IndexOf("(*)") >= 0)
                {
                    isDelegate = true;
                }
                else
                {
                    if (IsIgnoreField(i))
                        continue;
                }

                
                if (isDelegate)
                {
                    argType = argType.Replace("(*)", $"(*value)");
                    AddLine($"extern \"C\" {GlueExporter} void {GluePrefix}_{fn}_{mDecl.Name}__FieldSet__{i.Name}({FullNameCpp}* self, {argType})");
                }
                else
                {
                    AddLine($"extern \"C\" {GlueExporter} void {GluePrefix}_{fn}_{mDecl.Name}__FieldSet__{i.Name}({FullNameCpp}* self, {GetFullNameCpp(i.Type)} value)");
                }
                PushBrackets();
                {
                    AddLine($"{GlueNamespace}::{mDecl.Name}_Visitor::__FieldSet__{i.Name}(self, value);");
                }
                PopBrackets();

                if (GetFullNameCpp(i.Type).Contains("(*)"))
                    continue;

                bool bRetConvert = false;
                var retTypeStr = GetFullNameCpp(i.Type);
                if (GetCReturnMapper(retTypeStr) != null)
                {
                    retTypeStr = GetCReturnMapper(retTypeStr);
                    bRetConvert = true;
                }

                AddLine($"extern \"C\" {GlueExporter} {retTypeStr} {GluePrefix}_{fn}_{mDecl.Name}__FieldGet__{i.Name}({FullNameCpp}* self)");
                PushBrackets();
                {
                    if (bRetConvert)
                    {
                        AddLine($"auto tmp_result = {GlueNamespace}::{mDecl.Name}_Visitor::__FieldGet__{i.Name}(self);");
                        AddLine($"return VReturnValueMarshal<{GetFullNameCpp(i.Type)},{retTypeStr}>(tmp_result);");
                    }
                    else
                    {
                        AddLine($"return {GlueNamespace}::{mDecl.Name}_Visitor::__FieldGet__{i.Name}(self);");
                    }
                }
                PopBrackets();
            }

            AddLine($"///Cast");
            GenCast();
        }
        private void GenConstructor(ClangSharp.FunctionDecl func1)
        {
            var func = func1 as ClangSharp.CXXConstructorDecl;
            if (mDecl.IsAbstract)
                return;

            if (func.IsCopyConstructor)
                return;

            var ns = FullName.Replace(".", "_");
            AddLine($"extern \"C\" {GlueExporter} {FullNameCpp}* {GluePrefix}_{ns}__CreateInstance_{GetMethodHash(func)}({GetParameterDefine(func)})");
            PushBrackets();
            {
                AddLine($"return new {FullNameCpp}({GetParameterCallee(func)});");
            }
            PopBrackets();

            if (func1.Parameters.Count > 0)
                AddLine($"extern \"C\" {GlueExporter} void {GluePrefix}_{ns}__Call_Construct_OnMemory__{GetMethodHash(func)}(void* selfPtr, {GetParameterDefine(func)})");
            else
                AddLine($"extern \"C\" {GlueExporter} void {GluePrefix}_{ns}__Call_Construct_OnMemory__{GetMethodHash(func)}(void* selfPtr)");
            PushBrackets();
            {
                if (func1.Parameters.Count > 0)
                {
                    AddLine($"#undef new");
                    AddLine($"new (selfPtr){FullNameCpp}({GetParameterCallee(func)});");
                    AddLine($"#define new VNEW");
                }
                else
                {
                    AddLine($"#undef new");
                    AddLine($"new (selfPtr){FullNameCpp}();");
                    AddLine($"#define new VNEW");
                }
            }
            PopBrackets();
        }
        private void GenCast()
        {
            var fn = FullName.Replace(".", "_");
            foreach (var i in mDecl.Bases)
            {
                var baseDecl = i.Type.AsCXXRecordDecl;
                if (baseDecl == null)
                    continue;

                var base_fn = GetFullName(i.Type).Replace("::", "_");
                AddLine($"extern \"C\" {GlueExporter} {GetFullNameCpp(i.Type)}* {GluePrefix}_{fn}__CastSuperClass__{base_fn}({FullNameCpp}* self)");
                PushBrackets();
                {
                    AddLine($"return static_cast<{GetFullName(i.Type)}*>(self);");
                }
                PopBrackets();
            }

            if (HasMeta(SV_Dispose))
            {
                var dispose = GetMeta(SV_Dispose);
                AddLine($"extern \"C\" {GlueExporter} void {GluePrefix}_{fn}___Dispose({FullNameCpp}* self)");
                PushBrackets();
                {
                    AddLine($"{dispose};");
                }
                PopBrackets();
            }
        }
        private string GetParameterDefine(ClangSharp.FunctionDecl decl)
        {
            string result = "";
            foreach (var j in decl.Parameters)
            {
                if (!string.IsNullOrEmpty(result))
                    result += ",";
                var argType = GetFullNameCpp(j.Type);
                if (argType.IndexOf("(*)") >= 0)
                {
                    argType = argType.Replace("(*)", $"(*{j.Name})");
                    result += $"{argType}";
                }
                else
                {
                    var mashal = GetParameterMarshal(GetFullName(j.Type));
                    if (mashal != null)
                    {
                        result += $"{mashal} {j.Name}";
                    }
                    else
                    {
                        result += $"{GetFullNameCpp(j.Type)} {j.Name}";
                    }
                }
            }
            return result;
        }
        private string GetParameterCallee(ClangSharp.FunctionDecl decl)
        {
            string result = "";
            foreach (var j in decl.Parameters)
            {
                if (string.IsNullOrEmpty(result) == false)
                    result += $", ";
                var mashal = GetParameterMarshal(GetFullName(j.Type));
                if (mashal != null)
                {
                    result += $"VParameterMarshal<{GetFullNameCpp(j.Type)},{mashal}>({j.Name})";
                }
                else
                {
                    result += $"{j.Name}";
                }
            }
            return result;
        }
    }

    public class CppEnumBinderWriter : EnumWriter
    {
        public string FullNameCpp
        {
            get
            {
                return FullName.Replace(".", "::");
            }
        }
        public override void GenCode()
        {
            ClangSharp.Interop.CXFile tfile;
            uint line, col, offset;
            mDecl.Location.GetFileLocation(out tfile, out line, out col, out offset);

            AddLine($"//This cs is generated by THT.exe");
            NewLine();

            AddLine($"#include \"{PchFile}\"");
            AddLine($"#include \"{GetRegularPath(tfile.ToString())}\"");

            NewLine();

            AddLine($"namespace EngineNS");
            PushBrackets();
            {
                AddLine($"EnumBegin({Namespace.Replace(".", "::")}::{Name})");
                PushTab();
                {
                    foreach (var i in mDecl.Enumerators)
                    {
                        AddLine($"EnumMember({Namespace.Replace(".", "::")}::{i.Name})");
                    }
                }
                PopTab();
                AddLine($"EnumEnd({Name}, {Namespace.Replace(".", "::")})");
                AddLine($"EnumImpl({FullNameCpp})");
            }
            PopBrackets();
        }
    }
}

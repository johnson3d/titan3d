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
    class UProperty
    {
        public enum ETypeKind
        {
            Predefine,
            CSBinder,
            Object,
        }
        public ETypeKind Kind = ETypeKind.Predefine;
        public int NumOfPointer; 
        public string Type;
        public string Name;
        public string ToCSTypeName()
        {
            var result = Type;
            if (Kind != ETypeKind.Predefine)
            {
                result = "System.IntPtr";
            }
            for (int i = 0; i < NumOfPointer; i++)
            {
                result += "*";
            }
            return result;
        }
        public string ToCppTypeName()
        {
            var result = Cs2CppConveter(Type);
            for (int i = 0; i < NumOfPointer; i++)
            {
                result += "*";
            }
            return result;
        }
        public string Cs2CppConveter(string csType)
        {
            switch (csType)
            {
                case "System.IntPtr":
                    return "void*";
                case "string":
                    return "char*";
                case "sbyte":
                    return "char";
                case "Vector2":
                case "EngineNS.Vector2":
                    return "v3dxVector2";
                case "Vector3":
                case "EngineNS.Vector3":
                    return "v3dxVector3";
                case "Quaternion":
                case "EngineNS.Quaternion":
                    return "v3dxQuaternion";
                case "Matrix":
                case "EngineNS.Matrix":
                    return "v3dxMatrix4";
                case "EngineNS.Support.TtAnyValue":
                    return "EngineNS::TtAnyValue";
                default:
                    {
                        if (Kind != ETypeKind.Predefine)
                        {
                            return $"EngineNS::FDNObjectHandle/*{csType}*/";
                        }
                        return csType;
                    }
            }
        }
    }
    class UCppCallback
    {
        public MethodDeclarationSyntax MethodSyntax;
        public UProperty RetType = new UProperty();
        public string Name;
        public List<UProperty> Parameters = new List<UProperty>();
        public bool Build()
        {
            if (UCs2CppManager.TypeNameMappper(MethodSyntax.ReturnType, RetType) == false)
                return false;
            RetType.Name = "_ReturnValue";
            Parameters.Clear();
            foreach (var m in MethodSyntax.ParameterList.Parameters)
            {
                var a = new UProperty();
                if (UCs2CppManager.TypeNameMappper(m.Type, a) == false)
                    return false;
                a.Name = m.Identifier.ValueText;
                Parameters.Add(a);
            }
            return true;
        }
        public string GetArgumentDefine()
        {
            string result = "";
            foreach(var i in Parameters)
            {
                if (result != "")
                {
                    result += ", ";
                }
                result += $"{i.ToCSTypeName()} {i.Name}";
            }
            return result;
        }
        public string GetArgumentDefineCpp()
        {
            string result = "";
            foreach (var i in Parameters)
            {
                if (result != "")
                {
                    result += ", ";
                }
                result += $"{i.ToCppTypeName()} {i.Name}";
            }
            return result;
        }
        public string GetArgumentCall()
        {
            string result = "";
            foreach (var i in Parameters)
            {
                if (result != "")
                {
                    result += ", ";
                }
                result += $"{i.Name}";
            }
            return result;
        }
        public string GetArgumentCallCS()
        {
            string result = "";
            foreach (var i in Parameters)
            {
                if (result != "")
                {
                    result += ", ";
                }
                if (i.Kind != UProperty.ETypeKind.Predefine)
                {
                    result += $"EngineNS.Rtti.TtNativeCoreProvider.ObjectFromGCHandle<{i.Type}>({i.Name})";
                }
                else
                {
                    result += $"{i.Name}";
                }
            }
            return result;
        }
    }
    class UCs2CppClassDefine : UClassCodeBase
    {
        public List<UCppCallback> Callbacks = new List<UCppCallback>();
        public override void Build()
        {
            for (int i = 0; i < Callbacks.Count; i++)
            {
                if (Callbacks[i].Build() == false)
                {
                    Callbacks.RemoveAt(i);
                    i--;
                }
            }
        }
        public override void GenCode(string dir)
        {
            if (Callbacks.Count == 0)
                return;
            GenCodeCS(dir + "/cs");

            var csCode = ClassCode;
            ClassCode = "";
            GenCodeCpp(dir + "/cpp");
        }
        public void GenCodeCS(string dir)
        {
            AddLine("#pragma warning disable 105");
            AddLine("using System;");
            AddLine("using System.Runtime.InteropServices;");
            
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
                    foreach (var i in Callbacks)
                    {
                        AddLine($"[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                        AddLine($"private unsafe delegate {i.RetType.ToCSTypeName()} FDelegate_{i.Name}(IntPtr self, {i.GetArgumentDefine()});");
                        AddLine($"private unsafe static FDelegate_{i.Name} fn_{i.Name} = {i.Name}_Impl;");
                        AddLine($"private unsafe static {i.RetType.ToCSTypeName()} {i.Name}_Impl(IntPtr self, {i.GetArgumentDefine()})");
                        PushBrackets();
                        {
                            string retStr = "return ";
                            if (i.RetType.ToCSTypeName() == "void" || i.RetType.ToCSTypeName() == "System.Void")
                            {
                                retStr = "";
                            }  
                            AddLine($"var _gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);");
                            AddLine($"var _This = _gcHandle.Target as {this.FullName};");
                            AddLine($"if( _This == null)");
                            PushBrackets();
                            {
                                if (retStr == "")
                                {
                                    AddLine($"return;");
                                }
                                else
                                {
                                    if (i.RetType.NumOfPointer != 0)
                                    {
                                        AddLine($"return ({i.RetType.ToCSTypeName()})(0);");
                                    }
                                    else
                                    {
                                        if (i.RetType.ToCSTypeName() == "string")
                                            AddLine($"return null;");
                                        else if (i.RetType.ToCSTypeName() == "IntPtr" || i.RetType.ToCSTypeName() == "System.IntPtr")
                                            AddLine($"return IntPtr.Zero;");
                                        else if (i.RetType.Kind != UProperty.ETypeKind.Predefine)
                                            AddLine($"return null;");
                                        else
                                            AddLine($"return new {i.RetType.ToCSTypeName()}();");
                                    }
                                }
                            }
                            PopBrackets();

                            AddLine($"{retStr}_This.{i.Name}({i.GetArgumentCallCS()});");
                            //AddLine($"try");
                            //PushBrackets();
                            //{
                                
                            //}
                            //PopBrackets();
                            //AddLine($"finally");
                            //PushBrackets();
                            //{

                            //}
                            //PopBrackets();
                            
                        }
                        PopBrackets();
                    }

                    AddLine($"#region SDK");
                    AddLine($"const string ModuleNC = EngineNS.CoreSDK.CoreModule;");
                    foreach (var i in Callbacks)
                    {
                        AddLine($"[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                        AddLine($"extern unsafe static void {this.Namespace.Replace(".", "_")}_FCsMethods_Set{i.Name}(FDelegate_{i.Name} fn);");
                    }
                    AddLine($"#endregion");

                    AddLine($"public static void InitCallbacks()");
                    PushBrackets();
                    {
                        AddLine($"unsafe");
                        PushBrackets();
                        {
                            foreach (var i in Callbacks)
                            {
                                AddLine($"{this.Namespace.Replace(".", "_")}_FCsMethods_Set{i.Name}(fn_{i.Name});");
                            }
                        }
                        PopBrackets();
                    }
                    PopBrackets();
                }
                PopBrackets();
            }
            PopBrackets();

            var file = dir + "/" + FullName + ".cs2cpp.cs";
            if (!UCs2CppManager.Instance.WritedFiles.Contains(file.Replace("\\", "/").ToLower()))
            {
                UCs2CppManager.Instance.WritedFiles.Add(file.Replace("\\", "/").ToLower());
            }

            if (System.IO.File.Exists(file))
            {
                var oldCode = System.IO.File.ReadAllText(file);
                if (oldCode == ClassCode)
                    return;
            }

            System.IO.File.WriteAllText(file, ClassCode);
        }
        public void GenCodeCpp(string dir)
        {
            AddLine($"#pragma once");
            AddLine($"#include \"{UCs2CppManager.Instance.Pch}\"");

            NewLine();

            AddLine($"namespace {Namespace.Replace(".", "::")}");
            PushBrackets();
            {
                AddLine($"struct {this.Name} : public EngineNS::UCs2CppBase");
                PushBrackets();
                {
                    AddLine($"struct FCsMethods");
                    PushBrackets();
                    {
                        AddLine($"FCsMethods()");
                        PushBrackets();
                        {
                            AddLine($"memset(this, 0 ,sizeof(FCsMethods));");
                        }
                        PopBrackets();

                        foreach (var i in Callbacks)
                        {
                            AddLine($"typedef {i.RetType.ToCppTypeName()} (*{i.Name})(void* self, {i.GetArgumentDefineCpp()});");
                            AddLine($"{i.Name} fn_{i.Name};");
                        }
                    }
                    PopBrackets(true);
                    AddLine($"static FCsMethods CsMethods;");
                    NewLine();
                    foreach (var i in Callbacks)
                    {
                        AddLine($"{i.RetType.ToCppTypeName()} {i.Name}({i.GetArgumentDefineCpp()})");
                        PushBrackets();
                        {
                            AddLine($"return CsMethods.fn_{i.Name}(mCSharpHandle, {i.GetArgumentCall()});");
                        }
                        PopBrackets();
                    }
                }
                PopBrackets(true);
            }
            PopBrackets();

            var file = dir + "/" + FullName + ".cs2cpp.h";
            if (System.IO.File.Exists(file))
            {
                var oldCode = System.IO.File.ReadAllText(file);
                if (oldCode != ClassCode)
                {
                    System.IO.File.WriteAllText(file, ClassCode);
                }
            }
            else
            {
                System.IO.File.WriteAllText(file, ClassCode);
            }

            ClassCode = "";
            GenCodeCpp_cpp(dir);
        }
        private void GenCodeCpp_cpp(string dir)
        {
            AddLine($"#include \"{FullName}.cs2cpp.h\"");
            NewLine();

            AddLine($"namespace {Namespace.Replace(".", "::")}");
            PushBrackets();
            {
                AddLine($"{this.Name}::FCsMethods {this.Name}::CsMethods;");
            }
            PopBrackets();

            NewLine();

            foreach (var i in Callbacks)
            {
                AddLine($"extern \"C\" VFX_API void {this.Namespace.Replace(".", "_")}_FCsMethods_Set{i.Name}({FullName.Replace(".", "::")}::FCsMethods::{i.Name} fn)");
                PushBrackets();
                {
                    AddLine($"{FullName.Replace(".", "::")}::CsMethods.fn_{i.Name} = fn;");
                }
                PopBrackets();
            }

            var file = dir + "/" + FullName + ".cs2cpp.cpp";
            if (!UCs2CppManager.Instance.WritedCppFiles.Contains(file.Replace("\\", "/").ToLower()))
            {
                UCs2CppManager.Instance.WritedCppFiles.Add(file.Replace("\\", "/").ToLower());
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

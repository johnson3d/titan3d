using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public enum EStructType
    {
        Struct,
        Class,
    }
    public enum EVisitMode
    {
        Public,
        Protected,
        Private,
    }
    public enum EFunctionSuffix
    {
        None,
        Pure,
        Override,
        Default,
        Delete,
    }
    [Flags]
    public enum EDeclareType : uint
    {
        DT_Volatile = 1,
        DT_Const = (1 << 1),
        DT_Static = (1 << 2),
        DT_Virtual = (1 << 3),
        DT_Inline = (1 << 4),
        DT_Friend = (1 << 5),
        DT_API = (1 << 6),
    }
    public class CppMetaBase
    {
        public bool HasMetaFlag
        {
            get;
            set;
        } = false;
        public EVisitMode VisitMode
        {
            get;
            set;
        } = EVisitMode.Public;
        public Dictionary<string, string> MetaInfos
        {
            get;
        } = new Dictionary<string, string>();
        public void AnalyzeMetaString(string klsMeta)
        {
            MetaInfos.Clear();
            if (string.IsNullOrEmpty(klsMeta))
                return;
            int validCharNum = 0;
            foreach(var i in klsMeta)
            {
                if(CppHeaderScanner.IsBlankChar(i)!=true)
                {
                    validCharNum++;
                }
            }
            if (validCharNum == 0)
                return;
            var segs = klsMeta.Split(',');
            foreach (var i in segs)
            {
                var pair = i.Split('=');
                if (pair.Length == 2)
                {
                    MetaInfos.Add(pair[0].Trim(), pair[1].Trim());
                }
                else if (pair.Length == 1)
                {
                    MetaInfos.Add(pair[0].Trim(), "");
                }
            }
        }
        public string GetMetaValue(string name)
        {
            string result;
            if (MetaInfos.TryGetValue(name, out result))
                return result;
            return null;
        }
        public class Symbol
        {
            public const string SV_NameSpace = "SV_NameSpace";
            public const string SV_ReturnConverter = "SV_ReturenConverter";
            public const string SV_UsingNS = "SV_UsingNS";
            public const string SV_ReflectAll = "SV_ReflectAll";
            public const string SV_LayoutStruct = "SV_LayoutStruct";
        }
        
        public string GetReturnConverter()
        {
            return this.GetMetaValue(Symbol.SV_ReturnConverter);
        }
    }

    public class CppClass : CppMetaBase
    {
        public override string ToString()
        {
            return $"{GetNameSpace()}.{Name} : {ParentName}";
        }
        public string HeaderSource
        {
            get;
            set;
        }
        public EStructType StructType
        {
            get;
            set;
        } = EStructType.Class;
        public bool IsAPI
        {
            get;
            set;
        } = false;
        public string Name
        {
            get;
            set;
        }
        public EVisitMode InheritMode
        {
            get;
            set;
        } = EVisitMode.Public;
        public string ParentName
        {
            get;
            set;
        }
        public List<CppFunction> Methods
        {
            get;
        } = new List<CppFunction>();
        public List<CppMember> Members
        {
            get;
        } = new List<CppMember>();
        public List<CppConstructor> Constructors
        {
            get;
        } = new List<CppConstructor>();
        public string GetFullName(bool asCpp)
        {
            var ns = GetNameSpace();
            string fullName;
            if (ns == null)
                return Name;
            else
                fullName = ns + "." + this.Name;
            if (asCpp)
            {
                return fullName.Replace(".", "::");
            }
            else
            {
                return fullName;
            }
        }
        public string GetNameSpace()
        {
            return this.GetMetaValue(Symbol.SV_NameSpace);
        }
        public string GetUsingNS()
        {
            return this.GetMetaValue(Symbol.SV_UsingNS);
        }
        public string GetGenFileName()
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return Name + ".gen.cpp";
            else
                return ns + "." + Name + ".gen.cpp";
        }
        public string GetGenFileNameCSharp()
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return Name + ".gen.cs";
            else
                return ns + "." + Name + ".gen.cs";
        }
        public static bool IsSystemType(string name)
        {
            return CodeGenerator.Instance.IsSystemType(name);
        }
        public static string RemovePtrAndRef(string name)
        {
            int i = name.Length - 1;
            for (; i >= 0; i--)
            {
                if (name[i] != '*' && name[i] != '&')
                {
                    break;
                }
            }
            if (i == name.Length - 1)
                return name;
            else
                return name.Substring(0, i+1);
        }
        public static string SplitPureName(string name, out string suffix)
        {
            name = name.Replace("::", ".");
            name = name.Replace(" ", "");
            int i = name.Length - 1;
            for (; i >= 0; i--)
            {
                if (name[i] != '*' && name[i] != '&')
                {
                    break;
                }
            }
            suffix = name.Substring(i + 1);
            return name.Substring(0, i + 1); ;
        }
        public void CheckValid(CodeGenerator manager)
        {
            var usingNS = this.GetUsingNS();
            string[] segs = null;
            if (usingNS!=null)
                segs = usingNS.Split('&');

            foreach (var i in Members)
            {
                var realType = RemovePtrAndRef(i.Type);
                var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                if (klass != null)
                {
                    i.Type = klass.GetFullName(false);
                }
                if (IsSystemType(realType))
                    continue;
                else if (manager.FindClass(realType)!=null)
                    continue;
                else
                {
                    Console.WriteLine($"{realType} used by RTTI member({i.Name}) in {this.ToString()}, Please Reflect this class");
                }
            }

            foreach (var i in Methods)
            {
                var realType = RemovePtrAndRef(i.ReturnType);
                if (!IsSystemType(realType) && manager.FindClass(realType) == null)
                {
                    Console.WriteLine($"{realType} used by RTTI Method({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                }
                foreach(var j in i.Arguments)
                {
                    realType = RemovePtrAndRef(j.Key);
                    var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                    if (klass != null)
                    {
                        j.Key = klass.GetFullName(false);
                    }
                    if (!IsSystemType(realType) && manager.FindClass(realType) == null)
                    {
                        Console.WriteLine($"{realType} used by RTTI Method({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                    }
                }
            }

            foreach (var i in Constructors)
            {
                foreach (var j in i.Arguments)
                {
                    var realType = RemovePtrAndRef(j.Key);
                    var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                    if (klass != null)
                    {
                        j.Key = klass.GetFullName(false);
                    }
                    if (!IsSystemType(realType) && manager.FindClass(realType) == null)
                    {
                        Console.WriteLine($"{realType} used by RTTI Constructor({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                    }
                }
            }
        }
    }
    public class CppMember : CppMetaBase
    {
        public override string ToString()
        {
            return $"{VisitMode.ToString()}: {Type} {Name}";
        }
        public EDeclareType DeclareType
        {
            get;
            set;
        } = 0;
        string mType;
        public string Type
        {
            get { return mType; }
            set
            {
                string suffix;
                var pureType = CppClass.SplitPureName(value, out suffix);
                pureType = CodeGenerator.Instance.NormalizePureType(pureType);
                mType = pureType + suffix;
            }
        }
        public string PureType
        {
            get
            {
                return CppClass.RemovePtrAndRef(mType);
            }
        }
        public string Name
        {
            get;
            set;
        }
        public string DefaultValue
        {
            get;
            set;
        } = "";
        public bool IsNativePtr()
        {
            bool isNativePtr;
            CodeGenerator.Instance.CppTypeToCSType(Type, true, out isNativePtr);
            return isNativePtr;
        }
        private string GetConverterType()
        {
            if (IsNativePtr())
                return CppClass.RemovePtrAndRef(Type) + ".PtrType";
            else
                return Type;
        }
        public string GenPInvokeBinding(CppClass klass)
        {
            string code = $"extern \"C\" VFX_API {Type} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}({klass.GetFullName(true)}* self)\n";
            code += "{\n";
            code += $"\treturn self->{Name};\n";
            code += "}\n";
            code += $"extern \"C\" VFX_API void {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{Name}({klass.GetFullName(true)}* self, {Type} InValue)\n";
            code += "{\n";
            code += $"\tself->{Name} = InValue;\n";
            code += "}\n";
            return code;
        }
        public string GenPInvokeBindingCSharp_Getter(CppClass klass)
        {
            return $"private extern static {GetConverterType()} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}(PtrType self);";
        }
        public string GenPInvokeBindingCSharp_Setter(CppClass klass)
        {
            return $"private extern static void {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{Name}(PtrType self, {GetConverterType()} InValue);";
        }
        public string GenCallBindingCSharp(ref int nTable, CppClass klass)
        {
            string code;
            code = CodeGenerator.GenLine(nTable, $"public {GetConverterType()} {Name}");

            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            
            code += CodeGenerator.GenLine(nTable, $"get");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"return {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}(mPtr);");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");

            code += CodeGenerator.GenLine(nTable, $"set");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{Name}(mPtr, value);");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");

            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
    }
    public class CppCallParameters : CppMetaBase
    {
        public class ArgKeyValuePair
        {
            public ArgKeyValuePair(string k,string v, EDeclareType dt)
            {
                Key = k;
                Value = v;
                DeclType = dt;
            }
            public EDeclareType DeclType
            {
                get;
                set;
            } = 0;
            string mKey;
            public string Key
            {
                get { return mKey; }
                set
                {
                    string suffix;
                    var pureType = CppClass.SplitPureName(value, out suffix);
                    pureType = CodeGenerator.Instance.NormalizePureType(pureType);
                    mKey = pureType + suffix;
                }
            }
            public string Value;
        }
        public List<ArgKeyValuePair> Arguments
        {
            get;
        } = new List<ArgKeyValuePair>();
        public bool IsNativePtr(int i)
        {
            bool isNativePtr;
            CodeGenerator.Instance.CppTypeToCSType(Arguments[i].Key, true, out isNativePtr);
            return isNativePtr;
        }
        public string GetParameterString(string split = " ")
        {
            string result = "";
            for(int i = 0; i < Arguments.Count; i++)
            {
                var cppName = Arguments[i].Key.Replace(".", "::");
                if (i==0)
                    result += $"{cppName}{split}{Arguments[i].Value}";
                else
                    result += $", {cppName}{split}{Arguments[i].Value}";
            }
            return result;
        }
        public string GetParameterStringCSharp(bool bPtrType)
        {//bPtrType如果true，那么就要把XXX_Wrapper转换成XXX_Wrapper.PtrType
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                bool isNativePtr;
                var type = CodeGenerator.Instance.CppTypeToCSType(Arguments[i].Key, bPtrType, out isNativePtr);
                if (i == 0)
                    result += $"{type} {Arguments[i].Value}";
                else
                    result += $", {type} {Arguments[i].Value}";
            }
            return result;
        }
        public string GetParameterCallString(bool bNativePtr)
        {
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i].Value;
                if (bNativePtr && IsNativePtr(i))
                {
                    arg = arg + ".Ptr";
                }
                if (i == 0)
                    result += $"{arg}";
                else
                    result += $", {arg}";
            }
            return result;
        }
    }

    public class CppFunction : CppCallParameters
    {
        public override string ToString()
        {
            string result = "";
            if(IsVirtual)
            {
                result += "virtual ";
            }
            if (IsStatic)
            {
                result += "static ";
            }
            if (IsInline)
            {
                result += "inline ";
            }

            string suffix = "";
            if (IsConst)
                suffix += "const";

            switch (Suffix)
            {
                case EFunctionSuffix.Delete:
                    suffix += " = delete";
                    break;
                case EFunctionSuffix.Default:
                    suffix += " = default";
                    break;
                case EFunctionSuffix.Override:
                    suffix += " override";
                    break;
                case EFunctionSuffix.Pure:
                    suffix += " = 0";
                    break;
            }   

            if(IsReturnConstType)
                result += $"const {ReturnType} {Name}({GetParameterString()})";
            else
                result += $"{ReturnType} {Name}({GetParameterString()})";

            return result + suffix;
        }
        public bool IsConst
        {
            get;
            set;
        } = false;
        public bool IsVirtual
        {
            get;
            set;
        } = false;
        public bool IsStatic
        {
            get;
            set;
        } = false;
        public bool IsInline
        {
            get;
            set;
        } = false;
        public bool IsFriend
        {
            get;
            set;
        } = false;
        public bool IsAPI
        {//VFX_API
            get;
            set;
        } = false;
        public EFunctionSuffix Suffix
        {
            get;
            set;
        } = EFunctionSuffix.None;
        public string Name
        {
            get;
            set;
        }
        public bool IsReturnConstType
        {
            get;
            set;
        } = false;
        string mReturnType;
        public string ReturnType
        {
            get { return mReturnType; }
            set
            {
                string suffix;
                var pureType = CppClass.SplitPureName(value, out suffix);
                pureType = CodeGenerator.Instance.NormalizePureType(pureType);
                mReturnType = pureType + suffix;
            }
        }
        public string CSReturnType
        {
            get
            {
                bool isNativePtr;
                var type = CodeGenerator.Instance.CppTypeToCSType(mReturnType, true, out isNativePtr);
                return type;
            }
        }
        public string GenPInvokeBinding(CppClass klass)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = $"extern \"C\" VFX_API {ReturnType} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}({klass.GetFullName(true)}* self{afterSelf}{this.GetParameterString()})\n";
            code += "{\n";
            code += "\tif(self==nullptr) {\n";
            code += $"\t\treturn TypeDefault({ReturnType});\n";
            code += "\t}\n";
            code += $"\treturn self->{Name}({this.GetParameterCallString(false)});\n";
            code += "}\n";
            return code;
        }
        public string GenPInvokeBindingCSharp(CppClass klass, string selfType = "PtrType", bool isUnsafe = false)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            var unsafeDef = isUnsafe ? "unsafe " : "";
            return $"private extern static {unsafeDef}{ReturnType} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}({selfType} self{afterSelf}{this.GetParameterStringCSharp(true)});";
        }
        public string GenCallBindingCSharp(ref int nTable, CppClass klass)
        {
            string code = CodeGenerator.GenLine(nTable, $"public {CSReturnType} {Name}({this.GetParameterStringCSharp(false)})");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"return {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}(mPtr, {this.GetParameterCallString(true)});");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
    }
    public class CppConstructor : CppCallParameters
    {
        public override string ToString()
        {
            return $"Constructor({GetParameterString()})";
        }
        public string ApiName
        {
            get;
            set;
        } = null;

        public string GenPInvokeBinding(CppClass klass)
        {
            string code = $"extern \"C\" VFX_API {klass.GetFullName(true)}* {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstruct({this.GetParameterString()})\n";
            code += "{\n";
            code += $"\treturn new {klass.GetFullName(true)}({this.GetParameterCallString(false)});\n";
            code += "}\n";
            return code;
        }
        public string GenPInvokeBindingCSharp(CppClass klass)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            return $"private extern static PtrType {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstructor({this.GetParameterStringCSharp(true)});";
        }
        public string GenCallBindingCSharp(ref int nTable, CppClass klass)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = CodeGenerator.GenLine(nTable, $"public {klass.Name}{CodeGenerator.Symbol.NativeSuffix}({this.GetParameterStringCSharp(false)}{afterSelf}bool _dont_care_just_for_compile)");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"mPtr = {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstructor({this.GetParameterCallString(true)});");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
    }
}

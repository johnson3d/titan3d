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
        DT_Unsigned = (1 << 6),
        DT_API = (1 << 7),
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
            public const string SV_ContainClass = "SV_ContainClass";//用于标志类种类
            public const string SV_ReturnConverter = "SV_ReturnConverter";//For method member
            public const string SV_ConstValue = "SV_ConstValue";//For method member
            public const string SV_UsingNS = "SV_UsingNS";//For class enum
            public const string SV_ReflectAll = "SV_ReflectAll";//For class 
            public const string SV_LayoutStruct = "SV_LayoutStruct";//For class 
            public const string SV_RemoveStrings = "SV_RemoveStrings";//For class 
            public const string SV_CharSet = "SV_CharSet";//For method: CharSet.Ansi , CharSet.Unicode
            public const string SV_CallConvention = "SV_CallConvention";//For method: System.Runtime.InteropServices.CallingConvention.Cdecl , System.Runtime.InteropServices.CallingConvention.StdCall
            public const string SV_GenStaticFunction = "SV_GenStaticFunction";//For method:对外暴露PInvoke函数，使得可输入self指针
        }
        
        public string GetReturnConverter()
        {
            return this.GetMetaValue(Symbol.SV_ReturnConverter);
        }
    }

    public class CppContainer : CppMetaBase
    {
        public string GetNameSpace(bool asCpp = false)
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return ns;
            if (asCpp)
                return ns.Replace(".", "::");
            return ns;
        }
        public string GetContainClass()
        {
            return this.GetMetaValue(Symbol.SV_ContainClass);
        }
        public string GetUsingNS()
        {
            return this.GetMetaValue(Symbol.SV_UsingNS);
        }
        public string[] GetRemoveStrings()
        {
            var str = this.GetMetaValue(Symbol.SV_RemoveStrings);
            if (str == null)
                return null;
            return str.Split('+');
        }
        public string GetPureNameSpace(bool asCpp = false)
        {
            var container = GetContainClass();
            if (container == null)
                return GetNameSpace(asCpp);
            var ns = GetNameSpace();
            if(ns.EndsWith(container)==false)
            {
                throw new Exception(CppHeaderScanner.TraceMessage($"{ns} no {container}"));
            }
            if(asCpp)
                return ns.Substring(0, ns.Length - container.Length - 1).Replace(".", "::");
            else
                return ns.Substring(0, ns.Length - container.Length - 1);
        }
    }

    public class CppClass : CppContainer
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
        public bool IsLayout
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
        public string GetCppName(int starNum)
        {
            var result = "";
            result = GetNameSpace() + "." + Name;
            for (int i = 0; i < starNum; i++)
            {
                result += '*';
            }
            return result.Replace(".", "::");
        }
        public string GetCSName(int starNum)
        {
            var result = "";
            if (IsLayout)
            {//CSharp的内存布局值类型
                result = GetNameSpace() + "." + CodeGenerator.Symbol.LayoutPrefix + Name;
                for (int i = 0; i < starNum; i++)
                {
                    result += '*';
                }
                return result;
            }
            else
            {
                result = GetNameSpace() + "." + Name + CodeGenerator.Symbol.NativeSuffix + ".PtrType";
                for (int i = 1; i < starNum; i++)
                {
                    result += '*';
                }
                return result;
            }
        }
        public static string GetCSTypeImpl(CppClass TypeClass, CppEnum TypeEnum, CppCallback TypeCallback, int TypeStarNum, EDeclareType DeclType, string PureType, bool tryMashralString)
        {
            if (TypeClass != null)
                return TypeClass.GetCSName(TypeStarNum);
            if (TypeEnum != null)
                return TypeEnum.GetCSName(TypeStarNum);
            if (TypeCallback != null)
                return TypeCallback.GetCSName(TypeStarNum);
            string csType = "";
            if ((DeclType & EDeclareType.DT_Unsigned) == EDeclareType.DT_Unsigned)
            {
                csType = CodeGenerator.Instance.NormalizePureType("unsigned " + PureType);
            }
            else
            {
                csType = CodeGenerator.Instance.NormalizePureType(PureType);
            }
            for (int i = 0; i < TypeStarNum; i++)
            {
                csType += "*";
            }
            if (tryMashralString)
            {
                if (csType == "SByte*")
                    return "string";
                else if (csType == "Wchar16*")
                    return "string";
                else if (csType == "Wchar32*")
                    return "string";
            }
            return csType;
        }
        public static string GetCppTypeImpl(CppClass TypeClass, int TypeStarNum, EDeclareType DeclType, string Type)
        {
            string result = "";
            if (TypeClass != null)
                result = TypeClass.GetCppName(TypeStarNum);
            else
            {
                result = Type;
                for (int i = 0; i < TypeStarNum; i++)
                {
                    result += '*';
                }
            }
            if ((DeclType & EDeclareType.DT_Unsigned) == EDeclareType.DT_Unsigned)
            {
                result = "unsigned " + result;
            }
            if ((DeclType & EDeclareType.DT_Const) == EDeclareType.DT_Const)
            {
                result = "const " + result;
            }
            return result.Replace(".", "::");
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
        public static bool IsSystemType(string name)
        {
            return CodeGenerator.Instance.IsSystemType(name);
        }
        public static string SplitPureName(string name, out string suffix)
        {
            name = name.Replace("::", ".");
            name = name.Replace(" ", "");
            int i = name.Length - 1;
            suffix = "";
            for (; i >= 0; i--)
            {
                if (name[i] != '*' && name[i] != '&')
                {
                    break;
                }
                suffix += '*';
            }
            return name.Substring(0, i + 1); ;
        }
        public void CheckValid(CodeGenerator manager)
        {
            var usingNS = this.GetUsingNS();
            string[] segs = null;
            if (usingNS != null)
                segs = usingNS.Split('&');

            foreach (var i in Members)
            {
                string suffix;
                var realType = SplitPureName(i.Type, out suffix);
                var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                var enumType = CodeGenerator.Instance.MatchEnum(realType, segs);
                var cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
                if (klass != null)
                {
                    i.TypeClass = klass;
                    continue;
                }
                else if (enumType != null)
                {
                    i.TypeEnum = enumType;
                }
                else if (cbType != null)
                {
                    i.TypeCallback = cbType;
                }
                else if (IsSystemType(realType))
                {
                    continue;
                }
                else
                {
                    Console.WriteLine($"{realType} used by RTTI member({i.Name}) in {this.ToString()}, Please Reflect this class");
                }
            }

            foreach (var i in Methods)
            {
                string suffix;
                var realType = SplitPureName(i.ReturnType, out suffix);
                var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                var enumType = CodeGenerator.Instance.MatchEnum(realType, segs);
                var cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
                if (klass != null)
                {
                    i.ReturnTypeClass = klass;
                }
                else if (enumType != null)
                {
                    i.ReturnTypeEnum = enumType;
                }
                else if (cbType != null)
                {
                    i.ReturnTypeCallback = cbType;
                }
                else if(!IsSystemType(realType))
                {
                    Console.WriteLine($"{realType} used by RTTI Method({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                }
                foreach(var j in i.Arguments)
                {
                    realType = SplitPureName(j.Type, out suffix);
                    klass = CodeGenerator.Instance.MatchClass(realType, segs);
                    enumType = CodeGenerator.Instance.MatchEnum(realType, segs);
                    cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
                    if (klass != null)
                    {
                        j.TypeClass = klass;
                    }
                    else if (enumType != null)
                    {
                        j.TypeEnum = enumType;
                    }
                    else if (cbType != null)
                    {
                        j.TypeCallback = cbType;
                    }
                    else if (!IsSystemType(realType))
                    {
                        Console.WriteLine($"{realType} used by RTTI Method({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                    }
                }
            }

            foreach (var i in Constructors)
            {
                string suffix;
                foreach (var j in i.Arguments)
                {
                    var realType = SplitPureName(j.Type, out suffix);
                    var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                    var cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
                    if (klass != null)
                    {
                        j.TypeClass = klass;
                    }
                    else if(cbType!=null)
                    {
                        j.TypeCallback = cbType;
                    }
                    else if (!IsSystemType(realType))
                    {
                        Console.WriteLine($"{realType} used by RTTI Constructor({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                    }
                }
            }
        }
    }
}

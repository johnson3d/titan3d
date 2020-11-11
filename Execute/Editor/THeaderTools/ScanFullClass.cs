using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public partial class CppHeaderScanner
    {
        public static void AnalyzeClassFullInfo(string code, string klassName, List<CppMember> members, List<CppFunction> methods, List<CppConstructor> constructors)
        {
            var curMetType = "";
            var metaString = "";
            bool hasMetaFlag = false;
            int index = code.IndexOf('{');
            index++;
            SkipBlank(ref index, code);
            EVisitMode mode = EVisitMode.Private;
            if (code.StartsWith("struct"))
            {
                mode = EVisitMode.Public;
            }
            while (index < code.Length)
            {
                var token = GetTokenStringConbineStarAndRef(ref index, code, null);
                while (token == ";")
                {
                    token = GetTokenStringConbineStarAndRef(ref index, code, null);
                }
                EDeclareType dtStyles = 0;
                while (IsSkipKeyToken(token, ref dtStyles))
                {
                    token = GetTokenStringConbineStarAndRef(ref index, code, null);
                }
                if (index >= code.Length)
                {
                    if (token != "}")
                    {
                        throw new Exception(TraceMessage($"class {klassName} miss end character"));
                    }
                    break;
                }
                if ((dtStyles & EDeclareType.DT_Friend) == EDeclareType.DT_Friend)
                {//处理友元类和函数
                    int savedIndex = index;
                    SkipChar(ref index, code, ";(", 1);
                    if (code[index] == ';')
                    {
                        continue;
                    }
                    else if (code[index] == '(')
                    {
                        index--;
                        int rangeStart;
                        int rangeEnd;
                        SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                        var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                        index = savedIndex;
                    }
                }
                switch (token)
                {
                    case "class":
                    case "struct":
                        {
                            int rangeStart;
                            int rangeEnd;
                            SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                            token = GetTokenString(ref index, code, null);
                            if (token != ";")
                                throw new Exception(TraceMessage($"class {klassName}: public miss ':'"));
                        }
                        break;
                    case "public":
                        mode = EVisitMode.Public;
                        token = GetTokenString(ref index, code, null);
                        if (token != ":")
                            throw new Exception(TraceMessage($"class {klassName}: public miss ':'"));
                        break;
                    case "protected":
                        mode = EVisitMode.Protected;
                        token = GetTokenString(ref index, code, null);
                        if (token != ":")
                            throw new Exception(TraceMessage($"class {klassName}: protected miss ':'"));
                        break;
                    case "private":
                        mode = EVisitMode.Private;
                        token = GetTokenString(ref index, code, null);
                        if (token != ":")
                            throw new Exception(TraceMessage($"class {klassName}: private miss ':'"));
                        break;
                    case Symbol.MetaMember:
                    case Symbol.MetaFunction:
                    case Symbol.MetaConstructor:
                        {
                            int rangeStart;
                            int rangeEnd;
                            SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                            metaString = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                            SkipBlank(ref index, code);

                            hasMetaFlag = true;
                            curMetType = token;
                        }
                        break;
                    default:
                        {
                            if(token == "typedef")
                            {
                                SkipChar(ref index, code, ";", 1);
                                index++;
                                continue;
                            }
                            int rangeStart;
                            int rangeEnd;
                            var token2 = GetTokenStringConbineStarAndRef(ref index, code, null);
                            if (token == klassName && token2 == "(")
                            {//Constructor(...)构造器
                                var tmp = new CppConstructor();
                                index--;
                                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                                var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                                AnalyzeClassFuntionArguments(args, tmp);

                                tmp.VisitMode = mode;
                                tmp.HasMetaFlag = hasMetaFlag;
                                tmp.AnalyzeMetaString(metaString);
                                constructors.Add(tmp);
                                if (curMetType != "" && curMetType != Symbol.MetaConstructor)
                                {
                                    throw new Exception(TraceMessage($"{klassName} Constructor MetaType invalid:{curMetType}->{tmp.ToString()}"));
                                }
                                curMetType = "";
                                metaString = "";
                                hasMetaFlag = false;
                                token = GetTokenString(ref index, code, null);
                                if (token == ";")
                                {
                                    continue;
                                }
                                else if (token == "{")
                                {
                                    index--;
                                    SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                                    var bodyCode = code.Substring(rangeStart, rangeEnd - rangeStart);
                                    continue;
                                }
                                else
                                {
                                    throw new Exception(TraceMessage($"{klassName} Constructor miss ending"));
                                }
                            }
                            else if(token == "~" && token2 == klassName)
                            {
                                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                                var args = code.Substring(rangeStart, rangeEnd - rangeStart);
                                token = GetTokenString(ref index, code, null);
                                if (token == ";")
                                    continue;
                                else if (token == "{")
                                {
                                    index--;
                                    SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                                    continue;
                                }
                            }
                            else if (token == $"~{klassName}" && token2 == "(")
                            {
                                index--;
                                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                                var args = code.Substring(rangeStart, rangeEnd - rangeStart);
                                token = GetTokenString(ref index, code, null);
                                if (token == ";")
                                    continue;
                                else if (token == "{")
                                {
                                    index--;
                                    SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                                    continue;
                                }
                            }
                            else if (token2 == "(")
                            {//是函数指针
                                ProcFunctionPtr(ref index, code, klassName, members, mode, token, dtStyles, ref hasMetaFlag, ref metaString, ref curMetType);
                                continue;
                            }
                            else if (token2 == "operator")
                            {//操作符重载，这个无法分析反射信息，直接跳过
                                //var op = GetTokenStringConbineStarAndRef(ref index, code, null);
                                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                                var tk0 = GetTokenStringConbineStarAndRef(ref index, code, null);
                                if (tk0 == ";")
                                {
                                    continue;
                                }
                                else if (tk0 == "{")
                                {
                                    index--;
                                    SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                                    var bodyCode = code.Substring(rangeStart, rangeEnd - rangeStart);
                                    continue;
                                }
                                else
                                {
                                    throw new Exception(TraceMessage($"{klassName} operator error"));
                                }
                            }
                            var token3 = GetTokenStringConbineStarAndRef(ref index, code, null);
                            if(token3 == "[")
                            {
                                ProcArrayMember(ref index, code, klassName, members, mode, token, token2, dtStyles, ref hasMetaFlag, ref metaString, ref curMetType);
                            }
                            else if (token3 == ";")
                            {//是成员变量
                                var tmp = new CppMember();
                                tmp.VisitMode = mode;
                                tmp.Name = token2;
                                tmp.Type = token;
                                tmp.DeclareType = dtStyles;
                                tmp.HasMetaFlag = hasMetaFlag;
                                tmp.AnalyzeMetaString(metaString);
                                members.Add(tmp);
                                if (curMetType != "" && curMetType != Symbol.MetaMember)
                                {
                                    throw new Exception(TraceMessage($"Member MetaType invalid:{curMetType}->{tmp.ToString()}"));
                                }
                                curMetType = "";
                                metaString = "";
                                hasMetaFlag = false;
                            }
                            else if (token3 == "=")
                            {//是const成员变量或者c++11后支持的初始化
                                var tmp = new CppMember();
                                tmp.VisitMode = mode;
                                tmp.Name = token2;
                                tmp.Type = token;
                                tmp.DeclareType = dtStyles;
                                tmp.HasMetaFlag = hasMetaFlag;
                                tmp.AnalyzeMetaString(metaString);
                                members.Add(tmp);
                                if (curMetType != "" && curMetType != Symbol.MetaMember)
                                {
                                    throw new Exception(TraceMessage($"Member MetaType invalid:{curMetType}->{tmp.ToString()}"));
                                }
                                curMetType = "";
                                metaString = "";
                                hasMetaFlag = false;

                                tmp.DefaultValue = GetTokenString(ref index, code, null);//初始化值
                                token3 = GetTokenString(ref index, code, null);
                                if (token3 != ";")
                                    throw new Exception(TraceMessage($"member {tmp.ToString()} error"));
                            }
                            else if (token3 == "(")
                            {//是函数
                                var tmp = new CppFunction();
                                //这里还要加上dtStyles标志出来的virtual static const inline一类的前缀处理，其中const可以用来修饰返回值
                                if ((dtStyles & EDeclareType.DT_Virtual) == EDeclareType.DT_Virtual)
                                {
                                    tmp.IsVirtual = true;
                                }
                                if ((dtStyles & EDeclareType.DT_Static) == EDeclareType.DT_Static)
                                {
                                    tmp.IsStatic = true;
                                }
                                if ((dtStyles & EDeclareType.DT_Inline) == EDeclareType.DT_Inline)
                                {
                                    tmp.IsInline = true;
                                }
                                if ((dtStyles & EDeclareType.DT_Friend) == EDeclareType.DT_Friend)
                                {
                                    tmp.IsFriend = true;
                                }

                                tmp.ReturnType = token;
                                if ((dtStyles & EDeclareType.DT_Const) == EDeclareType.DT_Const)
                                {
                                    tmp.IsReturnConstType = true;
                                }
                                if ((dtStyles & EDeclareType.DT_Unsigned) == EDeclareType.DT_Unsigned)
                                {
                                    tmp.IsReturnUnsignedType = true;
                                }
                                tmp.Name = token2;
                                index--;
                                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                                var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                                token = GetTokenString(ref index, code, null);
                                if (token == "const")
                                {//这是一个const函数
                                    token = GetTokenString(ref index, code, null);
                                    tmp.IsConst = true;
                                }

                                AnalyzeClassFuntionArguments(args, tmp);

                                if (token == "=")
                                {//void fun(xxx) = 0;void fun(xxx) = default;void fun(xxx) = delete;
                                    token = GetTokenString(ref index, code, null);
                                    switch (token)
                                    {
                                        case "0":
                                            //msvc可以支持 virtual TR func(...) abstract这样表示纯虚，但是clang不允许，我们就不做支持了
                                            tmp.Suffix = EFunctionSuffix.Pure;
                                            break;
                                        case "default":
                                            tmp.Suffix = EFunctionSuffix.Default;
                                            break;
                                        case "delete":
                                            tmp.Suffix = EFunctionSuffix.Delete;
                                            break;
                                        default:
                                            throw new Exception(TraceMessage($"function {tmp.Name} invalid suffix"));
                                    }
                                    token = GetTokenString(ref index, code, null);
                                    if (token != ";")
                                    {//其实virtual TR  Func(...) = 0 {...}也是c++允许的的，这种奇怪的情况懒得分析了
                                        if (tmp.Suffix == EFunctionSuffix.Pure)
                                        {
                                            if (token == "{")
                                            {
                                                index--;
                                                SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                                                var bodyCode = code.Substring(rangeStart, rangeEnd - rangeStart);
                                            }
                                            else
                                            {
                                                throw new Exception(TraceMessage($"pure function {tmp.Name} miss ';'"));
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception(TraceMessage($"function {tmp.Name} miss ';'"));
                                        }
                                    }
                                }
                                else if (token == "override")
                                {
                                    tmp.Suffix = EFunctionSuffix.Override;
                                    token = GetTokenString(ref index, code, null);
                                    if (token == ";")
                                    {

                                    }
                                    else if (token == "{")
                                    {//void fun(...) const override
                                        index--;
                                        SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                                        var bodyCode = code.Substring(rangeStart, rangeEnd - rangeStart);
                                    }
                                    else
                                    {
                                        throw new Exception(TraceMessage($"function {tmp.Name} error"));
                                    }
                                }
                                else if (token == ";")
                                {//一个函数申明结束

                                }
                                else if (token == "{")
                                {//void fun(...) const
                                    index--;
                                    SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                                    var bodyCode = code.Substring(rangeStart, rangeEnd - rangeStart);
                                }

                                tmp.HasMetaFlag = hasMetaFlag;
                                tmp.AnalyzeMetaString(metaString);
                                methods.Add(tmp);
                                if (curMetType != "" && curMetType != Symbol.MetaFunction)
                                {
                                    throw new Exception(TraceMessage($"Function MetaType invalid:{curMetType}->{tmp.ToString()}"));
                                }
                                curMetType = "";
                                metaString = "";
                                hasMetaFlag = false;
                            }
                        }
                        break;
                }
            }
        }

        public static bool IsAllowFixedArrayType(string type)
        {
            switch(type)
            {
                case "bool":
                case "byte":
                case "short":
                case "int":
                case "long":
                case "char":
                case "sbyte":
                case "ushort":
                case "uint":
                case "ulong":
                case "float":
                case "double":
                case "SByte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Byte":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return true;
                default:
                    return false;
            }
        }
        private static void ProcArrayMember(ref int index, string code, string klassName, List<CppMember> members,
            EVisitMode mode, string type, string name, EDeclareType dtStyles, 
            ref bool hasMetaFlag, ref string metaString, ref string curMetType)
        {
            int rangeStart;
            int rangeEnd;

            var tmp = new CppMember();
            tmp.AnalyzeMetaString(metaString);
            var constValue = tmp.GetMetaValue(CppMetaBase.Symbol.SV_ConstValue);
            Dictionary<string, string> ConstMapper = new Dictionary<string, string>();
            if(constValue!=null)
            {
                var segs = constValue.Split('+');
                foreach(var i in segs)
                {
                    var pair = i.Split(':');
                    if (pair.Length != 2)
                        continue;

                    ConstMapper[pair[0]] = pair[1];
                }
            }
            tmp.IsArray = true;
            do
            {
                index--;
                SkipPair(ref index, code, '[', ']', out rangeStart, out rangeEnd);
                var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                args = args.Replace(" ", "");
                int sz = 0;
                try
                {
                    string tarNum;
                    if(ConstMapper.TryGetValue(args, out tarNum)==false)
                    {
                        tarNum = args;
                    }
                    sz = System.Convert.ToInt32(tarNum);
                }
                catch
                {
                    Console.WriteLine($"{klassName}::{name} array size error");
                }
                tmp.ArraySize.Add(sz);
                var token = GetTokenString(ref index, code, null);
                if (token == ";")
                {
                    tmp.VisitMode = mode;
                    tmp.Name = name;
                    tmp.Type = type;
                    tmp.DeclareType = dtStyles;
                    tmp.HasMetaFlag = hasMetaFlag;
                    members.Add(tmp);
                    if (curMetType != "" && curMetType != Symbol.MetaMember)
                    {
                        throw new Exception(TraceMessage($"Member MetaType invalid:{curMetType}->{tmp.ToString()}"));
                    }
                    curMetType = "";
                    metaString = "";
                    hasMetaFlag = false;

                    return;
                }
                else if(token == "[")
                {
                    continue;
                }
                else
                {
                    throw new Exception(TraceMessage($"{klassName}::{name} is invalid"));
                }
            }
            while (index<code.Length);
        }

        private static void ProcFunctionPtr(ref int index, string code, string klassName, List<CppMember> members,
            EVisitMode mode, string returnType, EDeclareType dtStyles,
            ref bool hasMetaFlag, ref string metaString, ref string curMetType)
        {
            int rangeStart;
            int rangeEnd;

            var tmp = new CppMember();
            index--;
            SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
            var funName = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
            funName = funName.Replace(" ", "");
            tmp.Name = funName;
            tmp.FunctionPtr = new CppFunction();

            SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
            var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
            AnalyzeClassFuntionArguments(args, tmp.FunctionPtr);

            if ((dtStyles & EDeclareType.DT_Const) == EDeclareType.DT_Const)
            {
                tmp.FunctionPtr.IsReturnConstType = true;
            }
            tmp.FunctionPtr.ReturnType = returnType;

            var token = GetTokenString(ref index, code, null);
            if (token == "const")
            {
                tmp.FunctionPtr.IsConst = true;
                token = GetTokenString(ref index, code, null);
            }
            if (token != ";")
            {
                throw new Exception(TraceMessage($"{klassName}::{funName} is a invalid function pointer"));
            }

            tmp.VisitMode = mode;
            tmp.Type = "void*";
            tmp.DeclareType = 0;
            tmp.HasMetaFlag = hasMetaFlag;
            tmp.AnalyzeMetaString(metaString);
            members.Add(tmp);
            if (curMetType != "" && curMetType != Symbol.MetaMember)
            {
                throw new Exception(TraceMessage($"Member MetaType invalid:{curMetType}->{tmp.ToString()}"));
            }
            curMetType = "";
            metaString = "";
            hasMetaFlag = false;
        }
    }
}

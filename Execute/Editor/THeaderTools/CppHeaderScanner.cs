using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppHeaderScanner
    {
        public class Symbol
        {
            public const string MetaClass = "TR_CLASS";
            public const string MetaFunction = "TR_FUNCTION";
            public const string MetaMember = "TR_MEMBER";
            public const string MetaEnum = "TR_ENUM";
            public const string MetaConstructor = "TR_CONSTRUCTOR";
            public const char BeginParentheses = '(';
            public const char EndParentheses = ')';
            public const char BeginBrace = '{';
            public const char EndBrace = '}';
            public const char Semicolon = ';';
            public const char StringFlag = '\"';
            public const char CharFlag = '\'';
            public const char NewLine = '\n';
        }
        public static string TraceMessage(string message = "error code",
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            return $"{sourceFilePath}:{sourceLineNumber}->{memberName}->{message}";
        }
        public static string RemoveAllComment(string src)
        {
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == Symbol.StringFlag)
                {
                    SkipString(ref i, src);
                }
                else if (src[i] == '/')
                {
                    if (i == src.Length - 1)
                    {
                        throw new Exception("error code: end with /'");
                    }
                    else if (src[i + 1] == '/')
                    {
                        int j = i + 2;
                        while(j < src.Length)
                        {
                            if(src[j] == Symbol.NewLine)
                            {//保留换行，否则代码很难看
                                break;
                            }
                            if (j == src.Length - 1)
                            {
                                j++;
                                break;
                            }
                            j++;
                        }
                        src = src.Remove(i, j - i);
                    }
                    else if (src[i + 1] == '*')
                    {
                        int j = i + 2;
                        while (j < src.Length)
                        {
                            if (j == src.Length - 1)
                                throw new Exception("error code");
                            if (src[j] == '*' && src[j + 1] == '/')
                            {
                                break;
                            }
                            j++;
                        }
                        src = src.Remove(i, j - i + 2);
                    }
                }
            }
            return src;
        }

        public void ScanHeader(string file, List<CppClass> klsCollector)
        {
            string code = System.IO.File.ReadAllText(file);
            code = RemoveAllComment(code);
            
            ScanClassCode(file, code, klsCollector);
        }
        public static void ScanClassCode(string sourceHeader, string code, List<CppClass> klsCollector)
        {
            string classDef = "";
            string klsMeta;
            var klsBegin = FindMetaFlags(0, code, Symbol.MetaClass, out klsMeta, null);
            while(klsBegin >= 0)
            {
                int i = klsBegin;
                SkipBlank(ref i, code);
                klsBegin = i;
                var mustIsClass = GetTokenString(ref i, code, null);
                if(mustIsClass != "class" && mustIsClass != "struct")
                {
                    throw new Exception(TraceMessage($"{Symbol.MetaClass} must use for class"));
                }
                int BraceDeep = 0;
                int klsEnd = -1;
                while(i < code.Length)
                {
                    if(code[i] == Symbol.StringFlag)
                    {
                        SkipString(ref i, code);
                    }
                    else if (code[i] == Symbol.BeginBrace)
                    {
                        BraceDeep++;
                    }
                    else if (code[i] == Symbol.EndBrace)
                    {
                        BraceDeep--;
                        if (BraceDeep == 0)
                        {
                            klsEnd = i;
                            break;
                        }
                    }
                    i++;
                }
                if(klsEnd < 0)
                {
                    throw new Exception($"{Symbol.MetaClass} must use for class define");
                }
                classDef = code.Substring(klsBegin, klsEnd - klsBegin + 1);

                var klass = AnalyzeClassDef(classDef, mustIsClass, klsMeta, sourceHeader, klsCollector);
                klass.HeaderSource = sourceHeader;

                foreach(var k in  klsCollector)
                {
                    if(k.GetGenFileName() == klass.GetGenFileName())
                    {
                        throw new Exception($"class name & namspace is same{klass.GetGenFileName()}");
                    }
                }
                klsCollector.Add(klass);

                klsBegin = FindMetaFlags(klsEnd, code, Symbol.MetaClass, out klsMeta, null);
            }
        }
        public static int FindMetaWithStartup(int start, string code, string type, out string meta, FTokenEndChar cb)
        {
            int idx = -1;
            meta = "";
            do
            {
                idx = code.IndexOf(type, start);
                if (idx < 0)
                    return -1;
                idx += type.Length;

                if (IsTokenEndChar(idx, code, cb) == false)
                {
                    start = idx;
                    continue;
                }
                else
                {
                    return idx -= type.Length;
                }
            }
            while (true);
        }
        public static int FindMetaFlags(int start, string code, string type, out string meta, FTokenEndChar cb)
        {
            int idx = -1;
            meta = "";
            do
            {
                idx = code.IndexOf(type, start);
                if (idx < 0)
                    return -1;
                idx += type.Length;

                if (IsTokenEndChar(idx, code, cb) == false)
                {
                    start = idx;
                    continue;
                }
                else
                {
                    break;
                }
            }
            while (true);

            SkipBlank(ref idx, code);

            if(code[idx++] != Symbol.BeginParentheses)
            {
                return -1;
            }
            else
            {
                var beginIdx = idx;
                int parenthesesNum = 1;
                while(idx<code.Length)
                {
                    switch (code[idx])
                    {
                        case Symbol.BeginParentheses:
                            parenthesesNum++;
                            break;
                        case Symbol.EndParentheses:
                            parenthesesNum--;
                            if(parenthesesNum==0)
                            {
                                meta = code.Substring(beginIdx, idx - beginIdx);
                                return idx + 1;
                            }
                            break;
                    }
                    idx++;
                }
            }

            return -1;
        }
        public static string RemoveClassInClasss(string code, List<string> classInClass)
        {
            string klsMeta;
            var klsBegin = FindMetaFlags(0, code, Symbol.MetaClass, out klsMeta, null);
            while (klsBegin >= 0)
            {
                var removeBegin = FindMetaWithStartup(0, code, Symbol.MetaClass, out klsMeta, null);

                int i = klsBegin;
                SkipBlank(ref i, code);
                klsBegin = i;

                var mustIsClass = GetTokenString(ref i, code, null);
                if (mustIsClass != "class" && mustIsClass != "struct")
                {
                    throw new Exception(TraceMessage($"{Symbol.MetaClass} must use for class"));
                }
                int BraceDeep = 0;
                int klsEnd = -1;
                while (i < code.Length)
                {
                    if (code[i] == Symbol.StringFlag)
                    {
                        SkipString(ref i, code);
                    }
                    else if (code[i] == Symbol.BeginBrace)
                    {
                        BraceDeep++;
                    }
                    else if (code[i] == Symbol.EndBrace)
                    {
                        BraceDeep--;
                        if (BraceDeep == 0)
                        {
                            klsEnd = i;
                            break;
                        }
                    }
                    i++;
                }
                if (klsEnd < 0)
                {
                    throw new Exception($"{Symbol.MetaClass} must use for class define");
                }
                klsEnd++;//这里把大括号移除
                SkipBlank(ref klsEnd, code);//移除大括号后面的空白字符
                var classInCode = code.Substring(klsBegin, klsEnd - klsBegin + 1);

                RemoveClassInClasss(classInCode, classInClass);

                classInCode = code.Substring(removeBegin, klsEnd - removeBegin + 1);
                classInClass.Add(classInCode);
                
                code = code.Remove(removeBegin, klsEnd - removeBegin + 1);//+1是要把class {};的分号移除
                klsBegin = FindMetaFlags(removeBegin, code, Symbol.MetaClass, out klsMeta, null);
            }
            return code;
        }
        public static CppClass AnalyzeClassDef(string code, string mustIsClass, string klsMeta, string sourceHeader, List<CppClass> klsCollector)
        {
            //这里先要丢掉类中类的所有字符
            List<string> classInClass = new List<string>();
            code = RemoveClassInClasss(code, classInClass);
            foreach(var i in classInClass)
            {
                //假设类中类不允许用struct
                ScanClassCode(sourceHeader, i, klsCollector);
            }

            if (code.StartsWith(mustIsClass)==false)
                throw new Exception(TraceMessage("not a class or struct"));

            var index = mustIsClass.Length;
            if(IsBlankChar(code[index])==false)
                throw new Exception(TraceMessage("not a class or struct"));

            var result = new CppClass();

            switch (mustIsClass)
            {
                case "class":
                    result.StructType = EStructType.Class;
                    break;
                case "struct":
                    result.StructType = EStructType.Struct;
                    break;
                default:
                    throw new Exception(TraceMessage("not a class or struct"));
            }

            SkipBlank(ref index, code);

            var firstToken = GetTokenString(ref index, code, null);

            SkipBlank(ref index, code);

            if (code[index] == ':')
            {
                index++;
                result.Name = firstToken;
                
                SkipBlank(ref index, code);
                string keyPublic = GetTokenString(ref index, code, null);
                switch (keyPublic)
                {
                    case "public":
                        result.InheritMode = EVisitMode.Public;
                        break;
                    case "protected":
                        result.InheritMode = EVisitMode.Protected;
                        break;
                    case "private":
                        break;
                    default://没有写继承模式，缺省为private
                        keyPublic = "private";
                        result.InheritMode = EVisitMode.Private;
                        index -= result.Name.Length;
                        break;
                }

                SkipBlank(ref index, code);
                result.ParentName = GetTokenString(ref index, code, null);
            }
            else if (code[index] == '{')
            {
                index++;
                result.ParentName = null;
                result.Name = firstToken;
            }
            else
            {
                if (firstToken == CodeGenerator.Instance.API_Name)
                {
                    result.IsAPI = true;
                }
                result.Name = GetTokenString(ref index, code, null);

                SkipBlank(ref index, code);

                if (code[index] == ':')
                {
                    index++;
                    SkipBlank(ref index, code);
                    string keyPublic = GetTokenString(ref index, code, null);
                    switch (keyPublic)
                    {
                        case "public":
                            result.InheritMode = EVisitMode.Public;
                            break;
                        case "protected":
                            result.InheritMode = EVisitMode.Protected;
                            break;
                        case "private":
                            break;
                        default://没有写继承模式，缺省为private
                            keyPublic = "private";
                            result.InheritMode = EVisitMode.Private;
                            index -= result.Name.Length;
                            break;
                    }

                    SkipBlank(ref index, code);
                    result.ParentName = GetTokenString(ref index, code, null);
                }
                else if (code[index] == '{')
                {
                    index++;
                    result.ParentName = null;
                }
            }

            result.AnalyzeMetaString(klsMeta);

            if (result.GetMetaValue(CppMetaBase.Symbol.SV_ReflectAll) != null || result.GetMetaValue(CppMetaBase.Symbol.SV_LayoutStruct) != null)
            {   
                AnalyzeClassFullInfo(code, result.Name, result.Members, result.Methods, result.Constructors);
                if (result.GetMetaValue(CppMetaBase.Symbol.SV_LayoutStruct) != null)
                    result.IsLayout = true;
                else
                    result.IsLayout = false;
            }
            else
            {
                AnalyzeClassMember(code, result);
                AnalyzeClassFuntion(code, result);
                AnalyzeClassConstructor(code, result);
            }

            return result;
        }
        private static bool IsSkipKeyToken(string token, ref EDeclareType dtStyles)
        {
            if (token == CodeGenerator.Instance.API_Name)
            {
                dtStyles |= EDeclareType.DT_API;
                return true;
            }
            switch (token)
            {
                case "volatile":
                    dtStyles |= EDeclareType.DT_Volatile;
                    return true;
                case "const":
                    dtStyles |= EDeclareType.DT_Const;
                    return true;
                case "static":
                    dtStyles |= EDeclareType.DT_Static;
                    return true;
                case "virtual":
                    dtStyles |= EDeclareType.DT_Virtual;
                    return true;
                case "inline":
                    dtStyles |= EDeclareType.DT_Inline;
                    return true;
                case "friend":
                    dtStyles |= EDeclareType.DT_Friend;
                    return true;
                default:
                    return false;
            }
        }        
        public static void AnalyzeClassFullInfo(string code, string klassName, List<CppMember> members, List<CppFunction> methods, List<CppConstructor> constructors)
        {
            var curMetType = "";
            var metaString = "";
            bool hasMetaFlag = false;
            int index = code.IndexOf('{');
            index++;
            SkipBlank(ref index, code);
            EVisitMode mode = EVisitMode.Private;
            if(code.StartsWith("struct"))
            {
                mode = EVisitMode.Public;
            }
            while(index<code.Length)
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
                    case "public":
                        mode = EVisitMode.Public;
                        token = GetTokenString(ref index, code, null);
                        if(token!=":")
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
                            int rangeStart;
                            int rangeEnd;
                            var token2 = GetTokenStringConbineStarAndRef(ref index, code, null);
                            if(token == klassName && token2 =="(")
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
                                if(token == ";")
                                {
                                    continue;
                                }
                                else if(token == "{")
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
                            else if(token2=="operator")
                            {//操作符重载，这个无法分析反射信息，直接跳过
                                //var op = GetTokenStringConbineStarAndRef(ref index, code, null);
                                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                                var tk0 =  GetTokenStringConbineStarAndRef(ref index, code, null);
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
                            if (token3 == ";")
                            {//是成员变量
                                var tmp = new CppMember();
                                tmp.VisitMode = mode;
                                tmp.Name = token2;
                                tmp.Type = token;
                                tmp.DeclareType = dtStyles;
                                tmp.HasMetaFlag = hasMetaFlag;
                                tmp.AnalyzeMetaString(metaString);
                                members.Add(tmp);
                                if (curMetType!="" && curMetType != Symbol.MetaMember)
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
                                if(token3!=";")
                                    throw new Exception(TraceMessage($"member {tmp.ToString()} error"));
                            }
                            else if (token3 == "(")
                            {//是函数
                                var tmp = new CppFunction();
                                //这里还要加上dtStyles标志出来的virtual static const inline一类的前缀处理，其中const可以用来修饰返回值
                                if((dtStyles & EDeclareType.DT_Virtual)== EDeclareType.DT_Virtual)
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
                                tmp.Name = token2;
                                index--;
                                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                                var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                                token = GetTokenString(ref index, code, null);
                                if (token == "const")
                                {
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
        public static void AnalyzeClassMember(string code, CppClass klass)
        {
            string meta;
            var index = FindMetaFlags(0, code, Symbol.MetaMember, out meta, null);
            while (index >= 0)
            {
                var memberInfo = new CppMember();
                SkipBlank(ref index, code);
                var nameEnd = code.IndexOf(Symbol.Semicolon, index);
                var nameStrs = code.Substring(index, nameEnd - index + 1);//带上了分号
                index += nameEnd - index;
                var tokens = GetTokens(0, nameStrs.Length - 1, nameStrs, IsEndToken_TypeDef);

                if(tokens.Count<2)
                    throw new Exception(TraceMessage("error code"));

                //volatile int xxx;
                memberInfo.Name = tokens[tokens.Count - 1];
                memberInfo.Type = tokens[tokens.Count - 2];
                memberInfo.AnalyzeMetaString(meta);

                klass.Members.Add(memberInfo);
                index = FindMetaFlags(index, code, Symbol.MetaMember, out meta, null);
            }
        }
        public static void AnalyzeClassConstructor(string code, CppClass klass)
        {
            string meta;
            var index = FindMetaFlags(0, code, Symbol.MetaConstructor, out meta, null);
            while (index >= 0)
            {
                var funInfo = new CppConstructor();
                SkipBlank(ref index, code);

                var nameEnd = code.IndexOf(Symbol.BeginParentheses, index);
                var nameStrs = code.Substring(index, nameEnd - index + 1);//带上了左大括号
                index += nameEnd - index;

                nameStrs = nameStrs.Replace("::", ".");
                var tokens = GetTokens(0, nameStrs.Length - 1, nameStrs, IsEndToken_TypeDef);

                {
                    if (tokens.Count == 2)
                    {//VFX_API ClassName(...)
                        funInfo.ApiName = tokens[0];
                    }
                    else if (tokens.Count == 1)
                    {//ClassName(...)
                        funInfo.ApiName = null;
                    }
                    else
                    {
                        throw new Exception(TraceMessage("error code"));
                    }
                }

                int deeps = 0;
                int argBegin = -1;
                while (index < code.Length)
                {
                    if (code[index] == Symbol.BeginParentheses)
                    {
                        if (deeps == 0)
                            argBegin = index + 1;
                        deeps++;
                    }
                    else if (code[index] == Symbol.EndParentheses)
                    {
                        deeps--;
                        if (deeps == 0)
                            break;
                    }
                    index++;
                }
                var args = code.Substring(argBegin, index - argBegin);

                AnalyzeClassFuntionArguments(args, funInfo);

                funInfo.AnalyzeMetaString(meta);
                klass.Constructors.Add(funInfo);

                index++;
                index = FindMetaFlags(index, code, Symbol.MetaConstructor, out meta, null);
            }
        }
        public static void AnalyzeClassFuntion(string code, CppClass klass)
        {
            string meta;
            var index = FindMetaFlags(0, code, Symbol.MetaFunction, out meta, null);
            while (index>=0)
            {
                var funInfo = new CppFunction();
                SkipBlank(ref index, code);

                var nameEnd = code.IndexOf(Symbol.BeginParentheses, index);
                var nameStrs = code.Substring(index, nameEnd - index + 1);//带上了左大括号
                index += nameEnd - index;

                nameStrs = nameStrs.Replace("::", ".");
                var tokens = GetTokens(0, nameStrs.Length - 1, nameStrs, IsEndToken_TypeDef);
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i] == "virtual")
                    {
                        funInfo.IsVirtual = true;
                        tokens.RemoveAt(i);
                        i--;
                    }
                    else if (tokens[i] == "const")
                    {
                        funInfo.IsReturnConstType = true;
                        tokens.RemoveAt(i);
                        i--;
                    }
                    else if (tokens[i] == CodeGenerator.Instance.API_Name)
                    {
                        funInfo.IsAPI = true;
                        tokens.RemoveAt(i);
                        i--;
                    }
                }

                if (tokens.Count == 2)
                {
                    funInfo.ReturnType = tokens[0];
                    funInfo.Name = tokens[1];
                }
                else
                {
                    throw new Exception(TraceMessage($"{klass.Name} function is invalid"));
                }

                int deeps = 0;
                int argBegin = -1;
                while(index<code.Length)
                {
                    if (code[index] == Symbol.BeginParentheses)
                    {
                        if (deeps == 0)
                            argBegin = index + 1;
                        deeps++;
                    }
                    else if (code[index] == Symbol.EndParentheses)
                    {
                        deeps--;
                        if (deeps == 0)
                            break;
                    }
                    index++;
                }
                var args = code.Substring(argBegin, index - argBegin);

                AnalyzeClassFuntionArguments(args, funInfo);

                funInfo.AnalyzeMetaString(meta);
                klass.Methods.Add(funInfo);

                index++;
                index = FindMetaFlags(index, code, Symbol.MetaFunction, out meta, null);
            }
        }
        public static void AnalyzeClassFuntionArguments(string code, CppCallParameters function)
        {
            var args = code.Split(',');
            function.Arguments.Clear();

            foreach (var i in args)
            {
                bool isBlankStr = true;
                foreach(var j in i)
                {
                    if(IsBlankChar(j)==false)
                    {
                        isBlankStr = false;
                        break;
                    }
                }
                if (isBlankStr)
                    continue;
                string type;
                string name;
                EDeclareType dtStyles;
                NormalizeArgument(i, out type, out name, out dtStyles);
                function.Arguments.Add(new CppCallParameters.CppParameter(type, name, dtStyles));
            }
        }
        private static void NormalizeArgument(string code, out string type, out string name, out EDeclareType dtStyles)
        {
            dtStyles = 0;
            var tokens = GetTokens(0, code.Length-1, code, IsEndToken_TypeDef);
            for(int i=0; i<tokens.Count; i++)
            {
                //去除无用得in out宏标志 
                if(tokens[i]=="IN" || tokens[i] == "OUT" || tokens[i] == "INOUT")
                {
                    tokens.RemoveAt(i);
                    i--;
                }
                else if(tokens[i] == "const")
                {
                    dtStyles = EDeclareType.DT_Const;
                    tokens.RemoveAt(i);
                    i--;
                }
            }
            if (tokens.Count==1)
            {
                type = tokens[0].Replace("::", ".");
                name = null;
            }
            else if (tokens.Count == 2)
            {
                type = tokens[0].Replace("::", ".");
                name = tokens[1];
            }
            else
            {
                throw new Exception(TraceMessage("function arguments invalid"));
            }
        }
        public static void SkipBlank(ref int i, string code)
        {
            if (i >= code.Length)
                return;

            while (IsBlankChar(code[i]))
            {
                i++;
                if (i >= code.Length)
                    throw new Exception(TraceMessage("error code"));
            }
        }
        public static void SkipString(ref int i, string src)
        {
            int j = i + 1;
            while (j < src.Length)
            {
                if (src[j] == Symbol.StringFlag && src[j - 1] != '\\')
                {
                    break;
                }
                if (j == src.Length - 1)
                {
                    j++;
                    break;
                    //throw new Exception(TraceMessage("error code"));
                }
                j++;
            }
            //var str = src.Substring(i, j - i);
            i = j;
        }
        public static void SkipChar(ref int i, string code, string CmpChars, int count)
        {
            int saveStart = i;
            int deep = 0;
            while (i < code.Length)
            {
                if (code[i] == '\\')
                {
                    i++;
                }
                foreach(var j in CmpChars)
                {
                    if(code[i] == j)
                    {
                        deep++;
                        if (deep == count)
                            return;
                    }
                }
                i++;
            }
            throw new Exception(TraceMessage($"no any character[{CmpChars}] in {code} from {saveStart}"));
        }
        public static void SkipPair(ref int i, string code, char startChar, char endChar, out int rangeStart, out int rangeEnd)
        {//得到的结果跳过endChar了
            rangeStart = i;
            int deep = 0;
            while(i<code.Length)
            {
                if(code[i] == '\"')
                {
                    SkipString(ref i, code);
                }
                else if (code[i] == startChar)
                {
                    deep++;
                    if(deep==1)
                    {
                        rangeStart = i;
                    }
                }
                else if (code[i] == endChar)
                {
                    deep--;
                    if(deep==0)
                    {
                        i++;
                        rangeEnd = i;
                        return;
                    }
                }
                i++;
            }
            throw new Exception(TraceMessage($"Miss Pair with {startChar} : {endChar}"));
        }
        public delegate bool FTokenEndChar(int index, string str);
        private static bool IsTokenEndChar(int index, string str, FTokenEndChar cb)
        {
            if (index >= str.Length)
                return true;
            char c = str[index];
            if (c == ' ')
                return true;
            else if (c == '\t')
                return true;
            else if (c == '\n')
                return true;
            else if (c == '\r')
                return true;
            else if (c == ';')
                return true;
            else if (c == ',')
                return true;
            else if (c == ':')
            {
                if(index+1<str.Length && str[index+1]!=':')
                    return true;
            }
            else if (c == '{')
                return true;
            else if (c == '}')
                return true;
            else if (c == '(')
                return true;
            else if (c == ')')
                return true;
            else if (c == '[')
                return true;
            else if (c == ']')
                return true;
            else if (c == '=')
                return true;
            else if (c == '+')
                return true;
            else if (c == '-')
                return true;
            else if (c == '*')
                return true;
            else if (c == '/')
                return true;
            else if (c == '%')
                return true;
            else if (c == '&')
                return true;
            else if (c == '!')
                return true;

            //else if (Char.IsDigit(c))
            //    return false;
            //else if (Char.IsLetter(c))
            //    return false;
            //else if (c == '_')
            //    return false;
            if (cb != null)
                return cb(index, str);
            return false;
        }
        private static bool IsEndToken_TypeDef(int index, string str)
        {
            return false;
        }
        public static bool IsBlankChar(char c)
        {
            if (c == ' ')
                return true;
            else if (c == '\t')
                return true;
            else if (c == '\r')
                return true;
            else if (c == '\n')
                return true;
            return false;
        }
        public static string GetTokenStringConbineStarAndRef(ref int start, string code, FTokenEndChar cb)
        {
            int tokenStart;
            int tokenEnd;

            GetNextTokenRange(start, code, out tokenStart, out tokenEnd, cb);
            var result = code.Substring(tokenStart, tokenEnd - tokenStart);
            start = tokenEnd;
            do
            {
                GetNextTokenRange(start, code, out tokenStart, out tokenEnd, cb);
                var tmp1 = code.Substring(tokenStart, tokenEnd - tokenStart);
                if(IsAllPtrOrRef(tmp1))
                {
                    start = tokenEnd;
                    result += tmp1; 
                }
                else
                {
                    break;
                }
            }
            while (tokenEnd<code.Length);

            result = result.Replace(" ", "");
            result = result.Replace("\t", "");
            result = result.Replace("\n", "");
            return result;
        }
        public static string GetTokenString(ref int start, string code, FTokenEndChar cb)
        {
            int tokenStart;
            int tokenEnd;
            GetNextTokenRange(start, code, out tokenStart, out tokenEnd, cb);
            if (tokenEnd == tokenStart)
                return "";
            var result = code.Substring(tokenStart, tokenEnd - tokenStart);
            result = result.Replace(" ", "");
            result = result.Replace("\t", "");
            result = result.Replace("\n", "");
            start = tokenEnd;
            return result;
        }
        public static void GetNextTokenRange(int start, string code, out int tokenStart, out int tokenEnd, FTokenEndChar cb)
        {
            SkipBlank(ref start, code);
            tokenStart = start;
            tokenEnd = start;
            while (IsTokenEndChar(tokenEnd, code, cb)==false)
            {
                var c1 = code[tokenEnd];
                tokenEnd++;
                if (tokenEnd >= code.Length)
                {
                    //throw new Exception(TraceMessage("error code"));
                    break;
                }
                var c2 = code[tokenEnd];
                if (c1 == ':' && c2 == ':')
                {//c++里面出现 :: 操作符，后面必须接一个字符串才能组成完整的token信息，但是中间是可以有空白字符的
                    tokenEnd++;
                    while (IsBlankChar(code[tokenEnd]))
                    {
                        tokenEnd++;
                    }
                }
            }
            if (tokenEnd == code.Length)
                return;
            if(tokenEnd == tokenStart)
            {//不是空字符，那么就是类似;,(一类的操作符了，他也是一个token
                tokenEnd = tokenStart + 1;
            }
        }
        public static List<string> GetTokens(int start, int end, string code, FTokenEndChar cb)
        {
            List<string> result = new List<string>();
            int i = start;
            while (i <= end)
            {
                SkipBlank(ref i, code);
                var str = GetTokenString(ref i, code, cb);
                if (i >= end)
                {
                    if(str.Length>0)
                        result.Add(str);
                    break;
                }

                result.Add(str);
            }
            if(cb == IsEndToken_TypeDef)
            {//这里解决数据定义 abc* ** *& 这种变态空格习惯
                int lastestStr = -1;
                for(int j=0; j<result.Count; j++)
                {
                    if(IsAllPtrOrRef(result[j]))
                    {
                        if(lastestStr==-1)
                        {
                            throw new Exception(TraceMessage("error code"));
                        }
                        result[lastestStr] = result[lastestStr] + result[j];
                        result.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        lastestStr = j;
                    }
                }
            }
            return result;
        }
        private static bool IsAllPtrOrRef(string str)
        {
            if (str.Length == 0)
                return false;
            foreach(var i in str)
            {
                if (i != '*' && i != '&')
                {
                    return false;
                }
            }
            return true;
        }
    }
}

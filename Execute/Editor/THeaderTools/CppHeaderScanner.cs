using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public partial class CppHeaderScanner
    {
        public class Symbol
        {
            public const string MetaClass = "TR_CLASS";
            public const string MetaFunction = "TR_FUNCTION";
            public const string MetaMember = "TR_MEMBER";
            public const string MetaEnum = "TR_ENUM";
            public const string MetaEnumMember = "TR_ENUM_MEMBER";
            public const string MetaConstructor = "TR_CONSTRUCTOR";
            public const string MetaCallback = "TR_CALLBACK";
            public const string MetaDiscard = "TR_DISCARD";
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
        
        public void ScanHeader(string file, List<CppClass> klsCollector, List<CppEnum> EnumCollector, List<CppCallback> cbCollector)
        {
            string code = System.IO.File.ReadAllText(file);

            CppPreprocess.Preprocess(ref code);

            ScanClassCode(file, code, klsCollector);
            ScanEnumCode(file, code, EnumCollector);
            CppCallbackAnalizer.ScanCode(file, code, cbCollector);
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
            int metaStart;
            return FindMetaFlags2(start, code, type, out metaStart, out meta, cb);
        }
        public static int FindMetaFlags2(int start, string code, string type, out int metaStart, out string meta, FTokenEndChar cb)
        {
            int idx = -1;
            metaStart = -1;
            meta = "";
            do
            {
                idx = code.IndexOf(type, start);
                if (idx < 0)
                    return -1;
                metaStart = idx;
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

            var result = new CppClass();
            result.AnalyzeMetaString(klsMeta);

            CppPreprocess.RemoveNextMember(ref code);

            var rmvStrs = result.GetRemoveStrings();
            if (rmvStrs != null)
            {
                foreach (var r in rmvStrs)
                {
                    int pos = code.IndexOf(r, 0);
                    while(pos >= 0)
                    {
                        bool before = false;
                        bool after = false;
                        if ( pos>0)
                        {
                            if(IsValidVarChar(code[pos - 1])==false)
                            {
                                before = true;
                            }
                        }
                        else
                        {
                            before = true;
                        }
                        if (pos + r.Length < code.Length - 1)
                        {
                            if(IsValidVarChar(code[pos + r.Length]) == false)
                            {
                                after = true;
                            }
                        }
                        else
                        {
                            after = true;
                        }
                        if(before && after)
                        {
                            code = code.Remove(pos, r.Length);
                        }
                        pos = code.IndexOf(r, pos);
                    }
                }
            }

            if (code.StartsWith(mustIsClass)==false)
                throw new Exception(TraceMessage("not a class or struct"));

            var index = mustIsClass.Length;
            if(IsBlankChar(code[index])==false)
                throw new Exception(TraceMessage("not a class or struct"));

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
        public static bool IsSkipKeyToken(string token, ref EDeclareType dtStyles)
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
                case "unsigned":
                    dtStyles |= EDeclareType.DT_Unsigned;
                    return true;
                default:
                    return false;
            }
        }        
        
        public static void AnalyzeClassMember(string code, CppClass klass)
        {
            string meta;
            var index = FindMetaFlags(0, code, Symbol.MetaMember, out meta, null);
            while (index >= 0)
            {
                var memberInfo = new CppMember();
                var token = "";
                EDeclareType dtStyles = 0;
                token = GetTokenStringConbineStarAndRef(ref index, code, null);
                while (IsSkipKeyToken(token, ref dtStyles))
                {
                    token = GetTokenStringConbineStarAndRef(ref index, code, null);
                }

                memberInfo.DeclareType = dtStyles;

                memberInfo.Type = token;
                memberInfo.Name = GetTokenString(ref index, code, null);

                token = GetTokenString(ref index, code, null);
                if(token!=";")
                {
                    throw new Exception(TraceMessage($"{klass}.{memberInfo.Name} error"));
                }

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
                var token = "";
                EDeclareType dtStyles = 0;
                token = GetTokenStringConbineStarAndRef(ref index, code, null);
                while (IsSkipKeyToken(token, ref dtStyles))
                {
                    token = GetTokenStringConbineStarAndRef(ref index, code, null);
                }
                if (token != klass.Name)
                {
                    throw new Exception(TraceMessage($"{klass} constructor error"));
                }

                int rangeStart;
                int rangeEnd;
                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);

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
                var token = "";
                EDeclareType dtStyles = 0;
                token = GetTokenStringConbineStarAndRef(ref index, code, null);
                while (IsSkipKeyToken(token, ref dtStyles))
                {
                    token = GetTokenStringConbineStarAndRef(ref index, code, null);
                }
                if ((dtStyles & EDeclareType.DT_Virtual) == EDeclareType.DT_Virtual)
                {
                    funInfo.IsVirtual = true;
                }
                if ((dtStyles & EDeclareType.DT_Static) == EDeclareType.DT_Static)
                {
                    funInfo.IsStatic = true;
                }
                if ((dtStyles & EDeclareType.DT_Inline) == EDeclareType.DT_Inline)
                {
                    funInfo.IsInline = true;
                }
                if ((dtStyles & EDeclareType.DT_Friend) == EDeclareType.DT_Friend)
                {
                    funInfo.IsFriend = true;
                }

                funInfo.ReturnType = token;
                if ((dtStyles & EDeclareType.DT_Const) == EDeclareType.DT_Const)
                {
                    funInfo.IsReturnConstType = true;
                }
                if ((dtStyles & EDeclareType.DT_Unsigned) == EDeclareType.DT_Unsigned)
                {
                    funInfo.IsReturnUnsignedType = true;
                }

                funInfo.Name = GetTokenString(ref index, code, null);

                int rangeStart;
                int rangeEnd;
                SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);

                AnalyzeClassFuntionArguments(args, funInfo);

                token = GetTokenString(ref index, code, null);
                if (token == "const")
                {//这是一个const函数
                    funInfo.IsConst = true;
                }

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
                function.Arguments.Add(new CppCallParameters.CppParameter(function, type, name, dtStyles));
            }
        }
        private static void NormalizeArgument(string code, out string type, out string name, out EDeclareType dtStyles)
        {
            var defaultValue = code.IndexOf('=');
            if (defaultValue>0)
            {
                code = code.Substring(0, defaultValue);
            }
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
                    dtStyles |= EDeclareType.DT_Const;
                    tokens.RemoveAt(i);
                    i--;
                }
                else if (tokens[i] == "unsigned")
                {
                    dtStyles |= EDeclareType.DT_Unsigned;
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
        public static bool IsValidVarChar(char c)
        {
            if (Char.IsDigit(c))
                return true;
            else if (Char.IsLetter(c))
                return true;
            else if (c == '_')
                return true;
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
                if(IsAllPtrOrRef(tmp1) && IsValidVarChar(result[result.Length - 1]))
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

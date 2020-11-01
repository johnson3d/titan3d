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

                if (IsTokenEndChar(code[idx], cb) == false)
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

                if (IsTokenEndChar(code[idx], cb) == false)
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
                result.ApiName = null;

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
                result.ApiName = null;
                result.Name = firstToken;
            }
            else
            {
                result.ApiName = firstToken;
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

            AnalyzeClassMember(code, result);
            AnalyzeClassFuntion(code, result);
            AnalyzeClassConstructor(code, result);

            return result;
        }
        private static bool IsEndToken_PtrOrRef(char c)
        {
            if (c == '*')
                return false;
            else if (c == '&')
                return false;
            return true;
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
                var tokens = GetTokens(0, nameStrs.Length - 1, nameStrs, IsEndToken_PtrOrRef);

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

                var tokens = GetTokens(0, nameStrs.Length - 1, nameStrs, IsEndToken_PtrOrRef);

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

                var tokens = GetTokens(0, nameStrs.Length - 1, nameStrs, IsEndToken_PtrOrRef);

                if (tokens[0] == "virtual")
                {
                    funInfo.IsVirtual = true;
                    if(tokens.Count==4)
                    {//virtual VFX_API void FunName
                        funInfo.ApiName = tokens[1];
                        funInfo.ReturnType = tokens[2];
                        funInfo.Name = tokens[3];
                    }
                    else if (tokens.Count == 3)
                    {//virtual void FunName
                        funInfo.ApiName = null;
                        funInfo.ReturnType = tokens[1];
                        funInfo.Name = tokens[2];
                    }
                    else
                    {
                        throw new Exception(TraceMessage("error code"));
                    }
                }
                else
                {
                    funInfo.IsVirtual = false;
                    if (tokens.Count == 3)
                    {//VFX_API void FunName
                        funInfo.ApiName = tokens[0];
                        funInfo.ReturnType = tokens[1];
                        funInfo.Name = tokens[2];
                    }
                    else if (tokens.Count == 2)
                    {//void FunName
                        funInfo.ApiName = null;
                        funInfo.ReturnType = tokens[0];
                        funInfo.Name = tokens[1];
                    }
                    else
                    {
                        throw new Exception(TraceMessage("error code"));
                    }
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
                NormalizeArgument(i, out type, out name);
                function.Arguments.Add(new KeyValuePair<string, string>(type, name));
            }
        }
        private static void NormalizeArgument(string code, out string type, out string name)
        {
            var tokens = GetTokens(0, code.Length-1, code, IsEndToken_PtrOrRef);
            for(int i=0; i<tokens.Count; i++)
            {
                //去除无用得in out宏标志 
                if(tokens[i]=="IN" || tokens[i] == "OUT" || tokens[i] == "INOUT")
                {
                    tokens.RemoveAt(i);
                    i--;
                }
            }
            if (tokens.Count==1)
            {
                type = tokens[0];
                name = null;
            }
            else if (tokens.Count == 2)
            {
                type = tokens[0];
                name = tokens[1];
            }
            else
            {
                throw new Exception(TraceMessage("function arguments invalid"));
            }
        }
        public static void SkipBlank(ref int i, string code)
        {
            while(IsBlankChar(code[i]))
            {
                i++;
                if (i >= code.Length)
                    throw new Exception(TraceMessage("error code"));
            }
        }
        public delegate bool FTokenEndChar(char c);
        private static bool IsTokenEndChar(char c, FTokenEndChar cb)
        {
            if (Char.IsDigit(c))
                return false;
            else if (Char.IsLetter(c))
                return false;
            else if (c == '_')
                return false;
            if (cb != null)
                return cb(c);
            return true;
        }
        private static bool IsBlankChar(char c)
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
        public static string GetTokenString(ref int start, string code, FTokenEndChar cb)
        {
            var len = GetTokenLength(start, code, cb);
            var result = code.Substring(start, len);
            start += len;
            return result;
        }
        public static int GetTokenLength(int start, string code, FTokenEndChar cb)
        {
            int count = 0;
            while(IsTokenEndChar(code[start], cb)==false)
            {
                count++;
                start++;
                if (start >= code.Length)
                {
                    //throw new Exception(TraceMessage("error code"));
                    break;
                }
            }
            return count;
        }
        public static int GetFullNameLength(int start, string code, FTokenEndChar cb)
        {
            int result = 0;
            do
            {
                var len = GetTokenLength(start, code, cb);
                result += len;
                start = start + len;
                if (start + 2 <= code.Length && code[start] == ':' && code[start + 1] == ':')
                {
                    result += 2;
                    start += 2;
                    continue;
                }
                else
                {
                    break;
                }
            }
            while (true);
            return result;
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
            if(cb == IsEndToken_PtrOrRef)
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

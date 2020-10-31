using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppHeaderScanner
    {
        public class Symbol
        {
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
                    throw new Exception(TraceMessage("error code"));
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
                            {
                                break;
                            }
                            if (j == src.Length - 1)
                                throw new Exception(TraceMessage("error code"));
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

        public void ScanHeader(string file)
        {
            string code = System.IO.File.ReadAllText(file);
            code = RemoveAllComment(code);
            string classDef = "";
            string klsMeta;
            var klsBegin = FindMetaFlags(0, code, "TR_CLASS", out klsMeta);
            while(klsBegin >= 0)
            {
                int i = klsBegin;
                SkipBlank(ref i, code);
                klsBegin = i;
                var mustIsClass = GetTokenString(ref i, code);
                if(mustIsClass != "class" && mustIsClass != "struct")
                {
                    throw new Exception(TraceMessage("TR_CLASS must use for class"));
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
                    throw new Exception("TR_CLASS must use for class define");
                }
                classDef = code.Substring(klsBegin, klsEnd - klsBegin + 1);

                var klass = AnalyzeClassDef(classDef, mustIsClass, klsMeta);

                klsBegin = FindMetaFlags(klsEnd, code, "TR_CLASS", out klsMeta);
            }
        }
        public static int FindMetaFlags(int start, string code, string type, out string meta)
        {
            int idx = -1;
            meta = "";
            do
            {
                idx = code.IndexOf(type, start);
                if (idx < 0)
                    return -1;
                idx += type.Length;

                if (IsTokenEndChar(code[idx]) == false)
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
        public static CppClass AnalyzeClassDef(string code, string mustIsClass, string klsMeta)
        {
            if(code.StartsWith(mustIsClass)==false)
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

            var firstToken = GetTokenString(ref index, code);

            SkipBlank(ref index, code);

            if (code[index] == ':')
            {
                index++;
                result.Name = firstToken;
                result.ApiName = null;

                SkipBlank(ref index, code);
                string keyPublic = GetTokenString(ref index, code);
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
                result.ParentName = GetTokenString(ref index, code);
            }
            else if (code[index] == '{')
            {
                index++;
                result.ParentName = null;
                result.ApiName = null;
            }
            else
            {
                result.ApiName = firstToken;
                result.Name = GetTokenString(ref index, code);

                SkipBlank(ref index, code);

                if (code[index] == ':')
                {
                    index++;
                    SkipBlank(ref index, code);
                    string keyPublic = GetTokenString(ref index, code);
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
                    result.ParentName = GetTokenString(ref index, code);
                }
                else if (code[index] == '{')
                {
                    index++;
                    result.ParentName = null;
                }
            }

            result.AnalyzeMetaString(klsMeta);
            return result;
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
        private static bool IsTokenEndChar(char c)
        {
            if (Char.IsDigit(c))
                return false;
            else if (Char.IsLetter(c))
                return false;
            else if (c == '_')
                return false;
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
        public static string GetTokenString(ref int start, string code)
        {
            var len = GetTokenLength(start, code);
            var result = code.Substring(start, len);
            start += len;
            return result;
        }
        public static int GetTokenLength(int start, string code)
        {
            int count = 0;
            while(IsTokenEndChar(code[start])==false)
            {
                count++;
                start++;
                if(start>=code.Length)
                    throw new Exception(TraceMessage("error code"));
            }
            return count;
        }
        public static int GetFullNameLength(int start, string code)
        {
            int result = 0;
            do
            {
                var len = GetTokenLength(start, code);
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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppHeaderScanner
    {
        public class Symbol
        {
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

        public void SkipString(ref int i, string src)
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

        public string RemoveAllComment(string src)
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
            var klsIndex = code.IndexOf("TR_CLASS()");
            while(klsIndex>=0)
            {
                var klsBegin = code.IndexOf("class", klsIndex);
                if(klsBegin<0)
                {
                    throw new Exception("TR_CLASS must use for class");
                }
                int BraceDeep = 0;
                int klsEnd = -1;
                int i = klsBegin;
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

                AnalyzeClassDef(classDef);

                klsIndex = code.IndexOf("TR_CLASS()", klsEnd);
            }
        }

        public void AnalyzeClassDef(string code)
        {
            var index = code.IndexOf("class ");
            if(index<0)
                throw new Exception(TraceMessage("error code"));

            index += "class ".Length;
            SkipBlank(ref index, code);
            int len = GetTokenLength(index, code);
            string klsName = code.Substring(index, len);
            index += len;

            SkipBlank(ref index, code);

            if (code[index++] == ':')
            {
                SkipBlank(ref index, code);
                len = GetTokenLength(index, code);
                string keyPublic = code.Substring(index, len);
                index += len;
                switch (keyPublic)
                {
                    case "public":
                        break;
                    case "protected":
                        break;
                    case "private":
                        break;
                    default://没有写继承模式，缺省为private
                        keyPublic = "private";
                        index -= len;
                        break;
                }

                SkipBlank(ref index, code);
                len = GetFullNameLength(index, code);
                string parentName = code.Substring(index, len);
                index += len;
            }
        }
        public void SkipBlank(ref int i, string code)
        {
            while(IsBlankChar(code[i]))
            {
                i++;
                if (i >= code.Length)
                    throw new Exception(TraceMessage("error code"));
            }
        }
        private bool IsTokenEndChar(char c)
        {
            if (Char.IsDigit(c))
                return false;
            else if (Char.IsLetter(c))
                return false;
            else if (c == '_')
                return false;
            return true;
        }
        private bool IsBlankChar(char c)
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
        public int GetTokenLength(int start, string code)
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
        public int GetFullNameLength(int start, string code)
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

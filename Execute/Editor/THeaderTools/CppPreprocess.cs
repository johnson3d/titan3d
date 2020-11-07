using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppPreprocess
    {
        private static string RemoveAllComment(string src)
        {
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == CppHeaderScanner.Symbol.StringFlag)
                {
                    CppHeaderScanner.SkipString(ref i, src);
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
                        while (j < src.Length)
                        {
                            if (src[j] == CppHeaderScanner.Symbol.NewLine)
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
        public static void Preprocess(ref string code)
        {
            code = RemoveAllComment(code);
            var pos = code.IndexOf(CodeGenerator.Symbol.PreprocessDiscardBegin, 0);
            while (pos >= 0)
            {
                var endPos = code.IndexOf(CodeGenerator.Symbol.PreprocessDiscardEnd, pos);
                code = code.Remove(pos, endPos + CodeGenerator.Symbol.PreprocessDiscardEnd.Length - pos);
                pos = code.IndexOf(CodeGenerator.Symbol.PreprocessDiscardBegin, pos);
            }

            foreach(var i in CodeGenerator.Symbol.PreprocessDiscardMacros)
            {
                RemoveMacro(ref code, i);
            }
        }
        public static void RemoveMacro(ref string code, string key)
        {//移除XXX(..)这种宏
            int rangeStart;
            int rangeEnd;
            int index = code.IndexOf(key, 0);
            while (index >= 0)
            {
                if(index>0)
                {
                    if (CppHeaderScanner.IsValidVarChar(code[index - 1]))
                    {
                        index += key.Length;
                        index = code.IndexOf(key, index);
                        continue;
                    }
                }
                int cutStart = index;
                index += key.Length;
                var token = CppHeaderScanner.GetTokenString(ref index, code, null);
                if (token == "(")
                {
                    index--;
                    CppHeaderScanner.SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                    code = code.Remove(cutStart, rangeEnd - cutStart);
                }
                index = code.IndexOf(key, index);
            }
        }
        public static void RemoveNextMember(ref string code)
        {
            int rangeStart;
            int rangeEnd;
            string meta;
            int metaStart;
            var index = CppHeaderScanner.FindMetaFlags2(0, code, CppHeaderScanner.Symbol.MetaDiscard, out metaStart, out meta, null);
            while (index >= 0)
            {
                CppHeaderScanner.SkipChar(ref index, code, ";(", 1);
                if (code[index] == ';')
                {//丢掉的是一个成员变量
                    code = code.Remove(metaStart, index - metaStart + 1);
                    index = metaStart;
                }
                else if (code[index] == '(')
                {//丢掉的是一个函数或者构造器
                    index--;
                    //跳过参数
                    CppHeaderScanner.SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);

                    CppHeaderScanner.SkipChar(ref index, code, ";{", 1);
                    if (code[index] == ';')
                    {//只是定义
                        code = code.Remove(metaStart, index - metaStart + 1);
                        index = metaStart;
                    }
                    else if (code[index] == '{')
                    {
                        index--;
                        //跳过函数体
                        CppHeaderScanner.SkipPair(ref index, code, '{', '}', out rangeStart, out rangeEnd);
                        code = code.Remove(metaStart, index - metaStart + 1);
                        index = metaStart;
                    }
                }
                index = CppHeaderScanner.FindMetaFlags2(index, code, CppHeaderScanner.Symbol.MetaDiscard, out metaStart, out meta, null);
            }
        }
    }
}

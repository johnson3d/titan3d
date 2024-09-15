using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class TypeConverterAttributeBase : Attribute
    {
        public virtual bool ConvertFromString(ref object obj, string text)
        {
            return true;
        }
        public static string GetMatchPair(string text, ref int cur, char startChar, char endChar)
        {
            return UTextUtility.GetMatchPair(text, ref cur, startChar, endChar);
        }
    }

    public class UTextUtility
    {
        public class Symbol
        {
            public const char StringFlag = '\"';
            public const char NewLine = '\n';
        }
        public static bool IsTokenEndChar(int index, string str)
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
                if (index + 1 < str.Length && str[index + 1] != ':')
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
        public static void SkipBlank(ref int i, string code)
        {
            if (i >= code.Length)
                return;

            while (IsBlankChar(code[i]))
            {
                i++;
                if (i >= code.Length)
                    throw new IO.IOException("error code");
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
                }
                j++;
            }
            i = j;
        }
        public static string RemoveCStyleComments(string src)
        {
            if (src == null)
                return "";
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == '/')
                {
                    if (i == src.Length - 1)
                    {
                        throw new IO.IOException("error code: end with /'");
                    }
                    else if (src[i + 1] == '/')
                    {
                        int j = i + 2;
                        while (j < src.Length)
                        {
                            if (src[j] == Symbol.NewLine)
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
                                throw new IO.IOException("error code");
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
        public static string GetMatchPair(string text, ref int cur, char startChar, char endChar)
        {
            int deep = 0;
            int start = -1;
            while (cur < text.Length - 1)
            {
                if (text[cur] == startChar)
                {
                    deep++;
                    start = cur + 1;
                }
                else if (text[cur] == endChar)
                {
                    deep--;
                    if (deep == 0)
                    {
                        var result = text.Substring(start, cur - start);
                        cur++;
                        return result;
                    }
                }
                cur++;
            }
            return null;
        }

        public static string GetTokenString(ref int start, string code)
        {
            int tokenStart;
            int tokenEnd;
            GetNextTokenRange(start, code, out tokenStart, out tokenEnd);
            if (tokenEnd == tokenStart)
                return "";
            var result = code.Substring(tokenStart, tokenEnd - tokenStart);
            result = result.Replace(" ", "");
            result = result.Replace("\t", "");
            result = result.Replace("\n", "");
            start = tokenEnd;
            return result;
        }
        private static void GetNextTokenRange(int start, string code, out int tokenStart, out int tokenEnd)
        {
            SkipBlank(ref start, code);
            tokenStart = start;
            tokenEnd = start;
            while (IsTokenEndChar(tokenEnd, code) == false)
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
            if (tokenEnd == tokenStart)
            {//不是空字符，那么就是类似;,(一类的操作符了，他也是一个token
                tokenEnd = tokenStart + 1;
            }
        }
    }


    public class TConvert
    {
        public static object ToEnumValue(System.Type realType, string name)
        {
            try
            {
                return System.Enum.Parse(realType, name);
            }
            catch
            {
                return 0;
            }
        }
        public static object ToObject(Type type, object obj)
        {
            if (obj == null)
                return null;
            if (obj.GetType() == type || obj.GetType().IsSubclassOf(type))
            {
                return obj;
            }
            else if(type == typeof(Vector4))
            {
                return Vector4.FromObject(obj);
            }
            else if (type == typeof(Vector3))
            {
                return Vector3.FromObject(obj);
            }
            else if (type == typeof(Color4f))
            {
                return Color4f.FromObject(obj);
            }
            else if (type == typeof(Color3f))
            {
                return Color3f.FromObject(obj);
            }
            else if (type == typeof(Color4b))
            {
                return Color4b.FromObject(obj);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
                return obj;
            }
        }
        public static object ToObject(Rtti.TtTypeDesc type, string text)
        {
            return ToObject(type.SystemType, text);
        }
        public static object ToObject(System.Type type, string text)
        {
            try
            {
                if (type == typeof(bool))
                    return System.Convert.ToBoolean(text);
                else if (type == typeof(sbyte))
                    return System.Convert.ToSByte(text);
                else if (type == typeof(Int16))
                    return System.Convert.ToInt16(text);
                else if (type == typeof(Int32))
                    return System.Convert.ToInt32(text);
                else if (type == typeof(Int64))
                    return System.Convert.ToInt64(text);
                else if (type == typeof(byte))
                    return System.Convert.ToByte(text);
                else if (type == typeof(UInt16))
                    return System.Convert.ToUInt16(text);
                else if (type == typeof(UInt32))
                    return System.Convert.ToUInt32(text);
                else if (type == typeof(UInt64))
                    return System.Convert.ToUInt64(text);
                else if (type == typeof(float))
                    return System.Convert.ToSingle(text);
                else if (type == typeof(double))
                    return System.Convert.ToDouble(text);
                else if (type == typeof(string))
                    return text;
                else if (type == typeof(RName))
                {
                    var segs = text.Split(':');
                    if (segs.Length != 2)
                    {
                        return null;
                    }
                    var eType = (RName.ERNameType)TConvert.ToEnumValue(typeof(RName.ERNameType), segs[1]);
                    return RName.GetRName(segs[0], eType);
                }
                else if (type == typeof(Guid))
                {
                    return Guid.Parse(text);
                }
                else if (type == typeof(FTransform))
                {
                    return FTransform.Parse(text);
                }
                else if (type == typeof(Color3f))
                {
                    return Color3f.FromString(text);
                }
                else if (type == typeof(Color4f))
                {
                    return Color4f.FromString(text);
                }
                else if (type == typeof(Color4b))
                {
                    return Color4b.FromString(text);
                }
                else if (type.IsEnum)
                {
                    return ToEnumValue(type, text);
                }
                else if (type.FullName == "System.Type")
                {
                    var desc = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(text);
                    if (desc != null)
                        return desc.SystemType;
                    return null;
                }
                else if (type == typeof(Rtti.TtTypeDesc))
                {
                    var desc = Rtti.TtTypeDesc.TypeOf(text);
                    if (desc != null)
                        return desc;
                    return null;
                }
                else
                {
                    var result = Rtti.TtTypeDescManager.CreateInstance(type);
                    var attrs = type.GetCustomAttributes(typeof(TypeConverterAttributeBase), true);
                    if (attrs.Length != 0)
                    {
                        var cvt = attrs[0] as TypeConverterAttributeBase;
                        cvt.ConvertFromString(ref result, text);
                    }
                    return result;
                }
            }
            catch
            {
                return Rtti.TtTypeDescManager.CreateInstance(type);
            }
        }
    }
}

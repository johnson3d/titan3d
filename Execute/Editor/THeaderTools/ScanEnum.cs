using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public partial class CppHeaderScanner
    {
        public static void ScanEnumCode(string sourceHeader, string code, List<CppEnum> EnumCollector)
        {
            int rangeStart;
            int rangeEnd;
            string klsMeta;
            var klsBegin = FindMetaFlags(0, code, Symbol.MetaEnum, out klsMeta, null);
            while (klsBegin >= 0)
            {
                int i = klsBegin;
                SkipBlank(ref i, code);
                klsBegin = i;
                var mustIsEnum = GetTokenString(ref i, code, null);
                if (mustIsEnum != "enum")
                {
                    throw new Exception(TraceMessage($"{Symbol.MetaEnum} must use for enum"));
                }
                SkipPair(ref i, code, '{', '}', out rangeStart, out rangeEnd);
                var enumDef = code.Substring(klsBegin, rangeEnd - klsBegin + 1);

                var klass = AnalyzeEnumDef(enumDef, klsMeta, sourceHeader);
                klass.HeaderSource = sourceHeader;
                EnumCollector.Add(klass);

                klsBegin = FindMetaFlags(rangeEnd, code, Symbol.MetaEnum, out klsMeta, null);
            }
        }
        public static CppEnum AnalyzeEnumDef(string code, string klsMeta, string sourceHeader)
        {
            CppEnum kls = new CppEnum();
            kls.HeaderSource = sourceHeader;
            kls.AnalyzeMetaString(klsMeta);

            string enumName = "";
            int index = 0;
            var token = GetTokenString(ref index, code, null);
            if (token != "enum")
            {
                throw new Exception(TraceMessage($"{sourceHeader} Enum invalid"));
            }
            token = GetTokenString(ref index, code, null);
            if (token != "{")
            {
                enumName = token;
                token = GetTokenString(ref index, code, null);
                if (token != "{")
                    throw new Exception(TraceMessage($"{sourceHeader} Enum invalid"));
            }

            kls.Name = enumName;

            var metaString = "";
            bool hasMetaFlag = false;
            while (index < code.Length)
            {
                token = GetTokenString(ref index, code, null);
                switch(token)
                {
                    case "}":
                    case ";":
                        break;
                    case Symbol.MetaEnumMember:
                        {
                            int rangeStart;
                            int rangeEnd;
                            SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                            metaString = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                            SkipBlank(ref index, code);

                            hasMetaFlag = true;
                        }
                        break;
                    default:
                        {
                            CppEnum.EnumMember tmp = new CppEnum.EnumMember();
                            tmp.Name = token;
                            token = GetTokenString(ref index, code, null);
                            if (token == "," || token == "}")
                            {
                                tmp.HasMetaFlag = hasMetaFlag;
                                tmp.AnalyzeMetaString(metaString);                                
                                metaString = "";
                                hasMetaFlag = false;
                                kls.Members.Add(tmp);
                                if (token == "}")
                                    break;
                            }
                            else if(token == "=")
                            {
                                SkipBlank(ref index, code);
                                int vStart = index;
                                SkipChar(ref index, code, ",}", 1);
                                tmp.Value = code.Substring(vStart, index - vStart);
                                tmp.HasMetaFlag = hasMetaFlag;
                                tmp.AnalyzeMetaString(metaString);
                                metaString = "";
                                hasMetaFlag = false;
                                kls.Members.Add(tmp);

                                if (code[index] == '}')
                                {
                                    break;
                                }
                                else if (code[index] == ',')
                                {
                                    index++;
                                    continue;
                                }
                                else
                                {
                                    throw new Exception(TraceMessage($"{sourceHeader} Enum {enumName}.{tmp.Name} invalid"));
                                }
                            }
                        }
                        break;
                }
            }

            return kls;
        }
    }
    public class EnumCodeHelper
    {
        public static string GenCppReflection(CppEnum klass)
        {
            string code = "";

            int nTable = 0;
            code += CodeGenerator.GenLine(nTable, $"EnumBegin({klass.GetFullName(true)})");
            nTable++;
            foreach (var i in klass.Members)
            {
                code += CodeGenerator.GenLine(nTable, $"EnumMember({klass.GetNameSpace(true)}::{i.Name})");
            }
            nTable--;
            code += CodeGenerator.GenLine(nTable, $"EnumEnd({klass.Name}, {klass.GetNameSpace(true)})");
            code += CodeGenerator.GenLine(nTable, $"EnumImpl({klass.GetFullName(true)})"); 
            return code;
        }
        public static string GenCSharpEnumDefine(CppEnum klass)
        {
            string code = "";

            int nTable = 1;
            code += CodeGenerator.GenLine(nTable, $"public enum {klass.Name}");
            code += CodeGenerator.GenLine(nTable++, "{"); 
            foreach (var i in klass.Members)
            {
                if (i.Value == null)
                {
                    code += CodeGenerator.GenLine(nTable, $"{i.Name},");
                }
                else
                {
                    code += CodeGenerator.GenLine(nTable, $"{i.Name} = {i.Value},");
                }
            }
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppCallback : CppCallParameters
    {
        public override string ToString()
        {
            string result = "delegate: ";
            if ((DeclType& EDeclareType.DT_Virtual)!=0)
            {
                result += "virtual ";
            }
            if ((DeclType & EDeclareType.DT_Static) != 0)
            {
                result += "static ";
            }
            if ((DeclType & EDeclareType.DT_Inline) != 0)
            {
                result += "inline ";
            }

            if ((DeclType & EDeclareType.DT_Const) != 0)
                result += $"const {ReturnType} {Name}({GetParameterString()})";
            else
                result += $"{ReturnType} {Name}({GetParameterString()})";

            return result;
        }
        public EDeclareType DeclType
        {
            get;
            set;
        }
        public CppTypeDesc ReturnType
        {
            get;
        } = new CppTypeDesc();
        public string CppReturnPureType
        {
            get
            {
                return ReturnType.GetCppType(DeclType);
            }
        }
        public string GetNameSpace(bool asCpp = false)
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return ns;
            if (asCpp)
                return ns.Replace(".", "::");
            return ns;
        }
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
        public string GetUsingNS()
        {
            return this.GetMetaValue(Symbol.SV_UsingNS);
        }
        public string GetCSName(int starNum)
        {
            var result = "";
            result = GetNameSpace() + "." + Name;
            for (int i = 0; i < starNum; i++)
            {
                result += '*';
            }
            return result;
        }
        public void CheckValid(CodeGenerator manager)
        {
            var usingNS = this.GetUsingNS();
            string[] segs = null;
            if (usingNS != null)
                segs = usingNS.Split('&');

            //string suffix;
            var realType = ReturnType.CppPureType;// CppClass.SplitPureName(this.ReturnType, out suffix);
            var klass = CodeGenerator.Instance.MatchClass(realType, segs);
            var enumType = CodeGenerator.Instance.MatchEnum(realType, segs);
            var cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
            if (klass != null)
            {
                ReturnType.TypeClass = klass;
            }
            else if (enumType != null)
            {
                ReturnType.TypeEnum = enumType;
            }
            else if (cbType != null)
            {
                ReturnType.TypeCallback = cbType;
            }
            else if (!CppClass.IsSystemType(realType))
            {
                Console.WriteLine($"{realType} in {this.ToString()}, Please Reflect this class");
            }
            foreach (var j in this.Arguments)
            {
                realType = j.Type.CppPureType;// CppClass.SplitPureName(j.Type.Type, out suffix);
                klass = CodeGenerator.Instance.MatchClass(realType, segs);
                enumType = CodeGenerator.Instance.MatchEnum(realType, segs);
                cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
                if (klass != null)
                {
                    j.Type.TypeClass = klass;
                }
                else if (enumType != null)
                {
                    j.TypeEnum = enumType;
                }
                else if (cbType != null)
                {
                    j.TypeCallback = cbType;
                }
                else if (!CppClass.IsSystemType(realType))
                {
                    Console.WriteLine($"{realType} in {this.ToString()}, Please Reflect this class");
                }
            }
        }
    }
    public class CppCallbackAnalizer
    {
        public static void ScanCode(string sourceHeader, string code, List<CppCallback> cbCollector)
        {
            string klsMeta;
            var index = CppHeaderScanner.FindMetaFlags(0, code, CppHeaderScanner.Symbol.MetaCallback, out klsMeta, null);
            while (index >= 0)
            {
                var mustIsTypedef = CppHeaderScanner.GetTokenString(ref index, code, null);
                if(mustIsTypedef!="typedef")
                {
                    throw new Exception(CppHeaderScanner.TraceMessage($"{sourceHeader}: TR_CALLBACK is invalid"));
                }
                var token = CppHeaderScanner.GetTokenStringConbineStarAndRef(ref index, code, null);
                EDeclareType dtStyles = 0;
                while (CppHeaderScanner.IsSkipKeyToken(token, ref dtStyles))
                {
                    token = CppHeaderScanner.GetTokenStringConbineStarAndRef(ref index, code, null);
                }
                CppCallback tmp = new CppCallback();
                tmp.AnalyzeMetaString(klsMeta);
                tmp.ReturnType.Type = token;

                token = CppHeaderScanner.GetTokenString(ref index, code, null);
                if(token!="(")
                {
                    throw new Exception(CppHeaderScanner.TraceMessage($"{sourceHeader}: TR_CALLBACK missing '('"));
                }
                token = CppHeaderScanner.GetTokenString(ref index, code, null);
                if (token != "*")
                {
                    throw new Exception(CppHeaderScanner.TraceMessage($"{sourceHeader}: TR_CALLBACK missing '(*'"));
                }
                token = CppHeaderScanner.GetTokenString(ref index, code, null);
                tmp.Name = token;
                token = CppHeaderScanner.GetTokenString(ref index, code, null);
                if (token != ")")
                {
                    throw new Exception(CppHeaderScanner.TraceMessage($"{sourceHeader}: TR_CALLBACK missing ')'"));
                }
                int rangeStart;
                int rangeEnd;
                CppHeaderScanner.SkipPair(ref index, code, '(', ')', out rangeStart, out rangeEnd);
                var args = code.Substring(rangeStart + 1, rangeEnd - rangeStart - 2);
                CppHeaderScanner.AnalyzeClassFuntionArguments(args, tmp);
                token = CppHeaderScanner.GetTokenString(ref index, code, null);
                if (token != ";")
                {
                    throw new Exception(CppHeaderScanner.TraceMessage($"{sourceHeader}: TR_CALLBACK {tmp.ToString()}missing ';'"));
                }

                cbCollector.Add(tmp);

                index = CppHeaderScanner.FindMetaFlags(index, code, CppHeaderScanner.Symbol.MetaCallback, out klsMeta, null);
            }
        }
    }
}

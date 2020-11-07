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
        public string Name
        {
            get;
            set;
        }
        public EDeclareType DeclType
        {
            get;
            set;
        }
        string mReturnType;
        public string ReturnType
        {
            get { return mReturnType; }
            set
            {
                string suffix;
                CppReturnPureType = CppClass.SplitPureName(value, out suffix);
                ReturnPureType = CodeGenerator.Instance.NormalizePureType(CppReturnPureType);
                mReturnType = ReturnPureType + suffix;
                ReturnTypeStarNum = suffix.Length;
            }
        }
        public string ReturnPureType
        {
            get;
            private set;
        }
        public int ReturnTypeStarNum
        {
            get;
            protected set;
        } = 0;
        public string CppReturnPureType
        {
            get;
            private set;
        }
        public CppClass ReturnTypeClass
        {
            get;
            set;
        }
        public CppEnum ReturnTypeEnum
        {
            get;
            set;
        }
        public CppCallback ReturnTypeCallback
        {
            get;
            set;
        }
        public string CSReturnType
        {
            get
            {
                string result = CppClass.GetCSTypeImpl(ReturnTypeClass, ReturnTypeEnum, ReturnTypeCallback, ReturnTypeStarNum, DeclType, ReturnPureType, false);
                if (result[0] == '.')
                {
                    return result.Substring(1);
                }
                else
                {
                    return result;
                }
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

            string suffix;
            var realType = CppClass.SplitPureName(this.ReturnType, out suffix);
            var klass = CodeGenerator.Instance.MatchClass(realType, segs);
            var enumType = CodeGenerator.Instance.MatchEnum(realType, segs);
            var cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
            if (klass != null)
            {
                ReturnTypeClass = klass;
            }
            else if (enumType != null)
            {
                ReturnTypeEnum = enumType;
            }
            else if (cbType != null)
            {
                ReturnTypeCallback = cbType;
            }
            else if (!CppClass.IsSystemType(realType))
            {
                Console.WriteLine($"{realType} in {this.ToString()}, Please Reflect this class");
            }
            foreach (var j in this.Arguments)
            {
                realType = CppClass.SplitPureName(j.Type, out suffix);
                klass = CodeGenerator.Instance.MatchClass(realType, segs);
                enumType = CodeGenerator.Instance.MatchEnum(realType, segs);
                cbType = CodeGenerator.Instance.MatchCallback(realType, segs);
                if (klass != null)
                {
                    j.TypeClass = klass;
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
                tmp.ReturnType = token;

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

        public static void GenCode(string targetDir, List<CppCallback> cbCollector)
        {
            string genCode = "//This cs is generated by THT.exe\n";
            genCode += CodeGenerator.GenLine(0, "using System;");
            genCode += CodeGenerator.GenLine(0, "using System.Runtime.InteropServices;");

            genCode += "\n\n";

            foreach (var i in cbCollector)
            {
                var ns = i.GetNameSpace();

                int nTable = 0;
                if (ns != null)
                {
                    genCode += CodeGenerator.GenLine(nTable, $"namespace {ns}");                    
                    genCode += CodeGenerator.GenLine(nTable++, "{");
                }

                genCode += CodeGenerator.GenLine(nTable++, $"public unsafe delegate {i.CSReturnType} {i.Name}({i.GetParameterStringCSharp()});");

                if (ns != null)
                {
                    genCode += CodeGenerator.GenLine(--nTable, "}");
                }
            }

            var file = targetDir + "CppDelegateDefine.gen.cs";
            CodeGenerator.Instance.NewCsFiles.Add(file);
            if (System.IO.File.Exists(file))
            {
                string old_code = System.IO.File.ReadAllText(file);
                if (genCode == old_code)
                    return;
            }
            System.IO.File.WriteAllText(file, genCode); ;
        }
    }
}

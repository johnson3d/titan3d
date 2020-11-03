using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public partial class CodeGenerator
    {
        private void InitType2Type()
        {
            Cpp2CSTypes["bool"] = "bool";
            Cpp2CSTypes["void"] = "void";
            Cpp2CSTypes["char"] = "char";
            Cpp2CSTypes["unsigned char"] = "byte";
            Cpp2CSTypes["short"] = "short";
            Cpp2CSTypes["unsigned short"] = "ushort";
            Cpp2CSTypes["int"] = "int";
            Cpp2CSTypes["unsigned int"] = "uint";
            Cpp2CSTypes["long"] = "int";
            Cpp2CSTypes["unsigned long"] = "uint";
            Cpp2CSTypes["long long"] = "long";
            Cpp2CSTypes["unsigned long long"] = "ulong";
            Cpp2CSTypes["float"] = "float";
            Cpp2CSTypes["double"] = "double";
            //Cpp2CSTypes["std::string"] = "string";
            Cpp2CSTypes["BYTE"] = "byte";
            Cpp2CSTypes["WORD"] = "ushort";
            Cpp2CSTypes["DWORD"] = "uint";
            Cpp2CSTypes["QWORD"] = "uint64";
            Cpp2CSTypes["SHORT"] = "short";
            Cpp2CSTypes["USHORT"] = "ushort";
            Cpp2CSTypes["INT"] = "int";
            Cpp2CSTypes["UINT"] = "uint";
            Cpp2CSTypes["INT64"] = "int64";
            Cpp2CSTypes["UINT64"] = "uint64";
            Cpp2CSTypes["vBOOL"] = "vBOOL";
        }
        public bool IsSystemType(string type)
        {
            return Cpp2CSTypes.ContainsKey(type);
        }
        public string NormalizePureType(string pureType)
        {
            string result;
            if (Cpp2CSTypes.TryGetValue(pureType, out result))
                return result;
            return pureType;
        }
    }
}

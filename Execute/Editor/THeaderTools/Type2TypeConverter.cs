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
            Cpp2CSTypes["unsigned char"] = "UInt8";
            Cpp2CSTypes["short"] = "short";
            Cpp2CSTypes["unsigned short"] = "UInt16";
            Cpp2CSTypes["int"] = "int";
            Cpp2CSTypes["unsigned int"] = "UInt32";
            Cpp2CSTypes["long"] = "int";
            Cpp2CSTypes["unsigned long"] = "UInt32";
            Cpp2CSTypes["long long"] = "long";
            Cpp2CSTypes["unsigned long long"] = "UInt64";
            Cpp2CSTypes["float"] = "float";
            Cpp2CSTypes["double"] = "double";
            //Cpp2CSTypes["std::string"] = "string";
            Cpp2CSTypes["BYTE"] = "byte";
            Cpp2CSTypes["WORD"] = "UInt16";
            Cpp2CSTypes["DWORD"] = "UInt32";
            Cpp2CSTypes["QWORD"] = "UInt64";
            Cpp2CSTypes["SHORT"] = "short";
            Cpp2CSTypes["USHORT"] = "UInt16";
            Cpp2CSTypes["INT"] = "int";
            Cpp2CSTypes["UINT"] = "UInt32";
            Cpp2CSTypes["INT64"] = "Int64";
            Cpp2CSTypes["UINT64"] = "UInt64";
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

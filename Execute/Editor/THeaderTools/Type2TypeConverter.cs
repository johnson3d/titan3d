using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public partial class CodeGenerator
    {
        private void InitType2Type()
        {
            Cpp2CSTypes["unsigned SByte"] = "byte";
            Cpp2CSTypes["unsigned char"] = "byte";
            Cpp2CSTypes["unsigned int"] = "UInt32";
            Cpp2CSTypes["unsigned long"] = "UInt32";
            Cpp2CSTypes["unsigned short"] = "UInt16";
            
            Cpp2CSTypes["Int8"] = "SByte";
            Cpp2CSTypes["Int16"] = "Int16";
            Cpp2CSTypes["Int32"] = "Int32";
            Cpp2CSTypes["Int64"] = "Int64";
            Cpp2CSTypes["UInt8"] = "byte";
            Cpp2CSTypes["UInt16"] = "UInt16";
            Cpp2CSTypes["UInt32"] = "UInt32";
            Cpp2CSTypes["UInt64"] = "UInt64";
            
            Cpp2CSTypes["bool"] = "bool";
            Cpp2CSTypes["void"] = "void";
            Cpp2CSTypes["char"] = "SByte";
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
            if (Cpp2CSTypes.ContainsKey(type) == true)
                return true;

            return Cpp2CSTypes.ContainsValue(type);
        }
        public string NormalizePureType(string pureType)
        {
            string result;
            if (Cpp2CSTypes.TryGetValue(pureType, out result))
                return result;
            return pureType;
        }
        public void LoadType2TypeMapper(string file)
        {
            string text = System.IO.File.ReadAllText(file);
            var lines = text.Split('\n');
            foreach(var i in lines)
            {
                var pair = i.Split(',');
                if (pair.Length != 2)
                    continue;

                string tar = pair[1];
                if (pair[1].EndsWith('\r'))
                    tar = tar.Substring(0, tar.Length - 1);
                if (pair[1].EndsWith('\n'))
                    tar = tar.Substring(0, tar.Length - 1);
                Cpp2CSTypes[pair[0]] = tar;
            }
        }
    }
}

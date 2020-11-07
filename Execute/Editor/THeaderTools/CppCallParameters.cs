using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppCallParameters : CppMetaBase
    {
        public string GetCharSet()
        {
            var meta = this.GetMetaValue(CppClass.Symbol.SV_CharSet);
            if (meta == null)
                return "CharSet.Ansi";
            return meta;
        }        
        public class CppParameter
        {
            public CppCallParameters Caller
            {
                get;
                private set;
            }
            public CppParameter(CppCallParameters func, string type, string v, EDeclareType dt)
            {
                Type = type;
                Value = v;
                DeclType = dt;
                TypeClass = null;
                Caller = func;
            }
            public EDeclareType DeclType
            {
                get;
                set;
            } = 0;
            string mType;
            public string Type
            {
                get { return mType; }
                set
                {
                    string suffix;
                    CppPureType = CppClass.SplitPureName(value, out suffix);
                    PureType = CodeGenerator.Instance.NormalizePureType(CppPureType);
                    mType = PureType + suffix;
                    TypeStarNum = suffix.Length;
                }
            }
            public string CppPureType
            {
                get;
                private set;
            }
            public string PureType
            {
                get;
                private set;
            }
            public string Value
            {
                get;
                set;
            }
            public int TypeStarNum
            {
                get;
                protected set;
            } = 0;
            public CppClass TypeClass
            {
                get;
                set;
            }
            public string CSType
            {
                get
                {
                    bool tryMashralString = true;
                    if (Caller.GetCharSet() == "CharSet.None")
                        tryMashralString = false;
                    string result = CppClass.GetCSTypeImpl(TypeClass, TypeEnum, TypeCallback, TypeStarNum, DeclType, PureType, tryMashralString);
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
            public string CppType
            {
                get
                {
                    return CppClass.GetCppTypeImpl(TypeClass, TypeStarNum, DeclType, CppPureType);
                }
            }
            public CppEnum TypeEnum
            {
                get;
                set;
            }
            public CppCallback TypeCallback
            {
                get;
                set;
            }
        }
        public List<CppParameter> Arguments
        {
            get;
        } = new List<CppParameter>();
        public string GetParameterString(string split = " ", bool needConst = false)
        {
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                var cppName = Arguments[i].CppType;

                if (i == 0)
                    result += $"{cppName}{split}{Arguments[i].Value}";
                else
                    result += $", {cppName}{split}{Arguments[i].Value}";
            }
            return result;
        }
        public string GetParameterStringCSharp()
        {//bPtrType如果false，那么就要把XXX_Wrapper.PtrType转换成XXX_Wrapper
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                var csType = Arguments[i].CSType;
                if (i == 0)
                    result += $"{csType} {Arguments[i].Value}";
                else
                    result += $", {csType} {Arguments[i].Value}";
            }
            return result;
        }
        public string GetParameterCallString()
        {
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i].Value;
                if (i == 0)
                    result += $"{arg}";
                else
                    result += $", {arg}";
            }
            return result;
        }
    }
}

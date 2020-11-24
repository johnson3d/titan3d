using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppCallParameters : CppMetaBase
    {
        string mName;
        public override string Name
        {
            get => mName;
            set => mName = value;
        }
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
                Type.Type = type;
                Value = v;
                DeclType = dt;
                Caller = func;
            }
            public EDeclareType DeclType
            {
                get;
                set;
            } = 0;
            public CppTypeDesc Type
            {
                get;
            } = new CppTypeDesc();
            public string Value
            {
                get;
                set;
            }
            public string CppType
            {
                get
                {
                    return Type.GetCppType(DeclType);
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
            public string CppDefaultValue
            {
                get;
                set;
            }
        }
        public List<CppParameter> Arguments
        {
            get;
        } = new List<CppParameter>();
        public string GetParameterString(string split = " ", bool needConst = false, bool tryParseRefer = false)
        {
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                var cppName = Arguments[i].CppType;
                if(tryParseRefer)
                {
                    if(Arguments[i].Type.IsRefer)
                    {
                        cppName = cppName.Substring(0, cppName.Length - 1) + "&";
                    }
                }

                if (i == 0)
                    result += $"{cppName}{split}{Arguments[i].Value}";
                else
                    result += $", {cppName}{split}{Arguments[i].Value}";
            }
            return result;
        }
        public string GetParameterCallString(bool tryParseRefer = false)
        {
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i].Value;
                if(tryParseRefer && Arguments[i].Type.IsRefer)
                {
                    arg = "*" + arg;
                }
                if (i == 0)
                    result += $"{arg}";
                else
                    result += $", {arg}";
            }
            return result;
        }
        public bool IsNoStar2RefArgument(int index)
        {
            var noStar2Ref = GetMetaValue(Symbol.SV_NoStarToRef);
            if (noStar2Ref == null)
                return false;
            string[] noTransTabs = null;
            if (noStar2Ref != null)
            {
                noTransTabs = noStar2Ref.Split('+');
            }
            var arg = Arguments[index].Value;
            foreach (var j in noTransTabs)
            {
                if (j == arg)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppEnum : CppContainer
    {
        public class EnumMember : CppMetaBase
        {
            public string Name
            {
                get;
                set;
            }
            public string Value
            {
                get;
                set;
            }
        }
        public string HeaderSource
        {
            get;
            set;
        }
        public string GetGenFileName()
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return Name + ".gen.cpp";
            else
                return ns + "." + Name + ".gen.cpp";
        }
        public string GetGenFileNameCSharp()
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return Name + ".gen.cs";
            else
                return ns + "." + Name + ".gen.cs";
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
        public string Type
        {
            get
            {
                var type = GetMetaValue(Symbol.SV_EnumType);
                if (type == null)
                    return "int";
                return type;
            }
        }
        public string Name
        {
            get;
            set;
        }
        public List<EnumMember> Members
        {
            get;
        } = new List<EnumMember>();
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
            foreach(var i in Members)
            {
                if (string.IsNullOrEmpty(i.Value))
                    continue;
                int index = 0;
                CppHeaderScanner.SkipBlank(ref index, i.Value);
                var value = i.Value.Substring(index);
                for (int j = value.Length - 1; j >= 0; j--)
                {
                    if (CppHeaderScanner.IsBlankChar(value[j]) != true)
                    {
                        value = value.Substring(0, j + 1);
                        i.Value = value;
                        break;
                    }
                }
            }
        }
    }
}

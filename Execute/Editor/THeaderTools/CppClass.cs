using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public enum EStructType
    {
        Struct,
        Class,
    }
    public enum EVisitMode
    {
        Public,
        Protected,
        Private,
    }
    public class CppMetaBase
    {
        public Dictionary<string, string> MetaInfos
        {
            get;
        } = new Dictionary<string, string>();
        public void AnalyzeMetaString(string klsMeta)
        {
            MetaInfos.Clear();
            var segs = klsMeta.Split(',');
            foreach (var i in segs)
            {
                var pair = i.Split('=');
                if (pair.Length == 2)
                {
                    MetaInfos.Add(pair[0].Trim(), pair[1].Trim());
                }
            }
        }
        public string GetMetaValue(string name)
        {
            string result;
            if (MetaInfos.TryGetValue(name, out result))
                return result;
            return null;
        }
    }

    public class CppClass : CppMetaBase
    {
        public override string ToString()
        {
            return $"{Name} : {ParentName}";
        }
        public string HeaderSource
        {
            get;
            set;
        }
        public EStructType StructType
        {
            get;
            set;
        } = EStructType.Class;
        public string ApiName
        {
            get;
            set;
        } = null;
        public string Name
        {
            get;
            set;
        }
        public EVisitMode InheritMode
        {
            get;
            set;
        } = EVisitMode.Public;
        public string ParentName
        {
            get;
            set;
        }
        public List<CppFunction> Methods
        {
            get;
        } = new List<CppFunction>();
        public List<CppMember> Members
        {
            get;
        } = new List<CppMember>();
        public string GetGenFileName()
        {
            var ns = this.GetMetaValue("NameSpace");
            if (ns == null)
                return Name + ".gen.cpp";
            else
                return ns + "." + Name + ".gen.cpp";
        }
        public string GetNameSpace()
        {
            return this.GetMetaValue("NameSpace");
        }
    }
    public class CppMember : CppMetaBase
    {
        public string Type
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }
    public class CppFunction : CppMetaBase
    {
        public bool IsVirtual
        {
            get;
            set;
        }
        public string ApiName
        {
            get;
            set;
        } = null;
        public string Name
        {
            get;
            set;
        }
        public string ReturnType
        {
            get;
            set;
        }
        public List<KeyValuePair<string, string>> Arguments
        {
            get;
        } = new List<KeyValuePair<string, string>>();
    }
}

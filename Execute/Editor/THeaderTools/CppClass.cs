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
    }

    public class CppClass : CppMetaBase
    {
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

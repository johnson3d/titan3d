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
    public class CppClass
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
        public Dictionary<string, string> MetaInfos
        {
            get;
        } = new Dictionary<string, string>();
        public void AnalyzeMetaString(string klsMeta)
        {
            MetaInfos.Clear();
            var segs = klsMeta.Split(',');
            foreach(var i in segs)
            {
                var pair = i.Split('=');
                if(pair.Length==2)
                {
                    MetaInfos.Add(pair[0].Trim(), pair[1].Trim());
                }
            }
        }
    }
}

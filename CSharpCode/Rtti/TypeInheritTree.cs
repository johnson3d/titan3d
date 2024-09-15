using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class TtNameSpace
    {
        public string Name;
        public List<TtNameSpace> ChildrenNS = new List<TtNameSpace>();
        public List<TtClassMeta> Types = new List<TtClassMeta>();
        public TtNameSpace GetNS(string n)
        {
            foreach(var i in ChildrenNS)
            {
                if (i.Name == n)
                    return i;
            }
            var ns = new TtNameSpace();
            ns.Name = n;
            ChildrenNS.Add(ns);
            return ns;
        }
        public bool IsContain(string filter)
        {
            foreach (var i in Types)
            {
                if (i.ClassType.Name.Contains(filter))
                    return true;
                foreach(var j in i.Methods)
                {
                    if (j.MethodName.Contains(filter))
                        return true;
                }
                foreach (var j in i.Fields)
                {
                    if (j.FieldName.Contains(filter))
                        return true;
                }
                foreach (var j in i.Properties)
                {
                    if (j.PropertyName.Contains(filter))
                        return true;
                }
            }
            foreach (var i in ChildrenNS)
            {
                if (i.Name.Contains(filter))
                    return true;
                if (i.IsContain(filter))
                    return true;
            }
            return false;
        }
    }
    public class TtTypeTreeManager
    {
        public TtTypeTreeManager()
        {
            RootNS.Name = "global";
        }
        public TtNameSpace RootNS = new TtNameSpace();
        public void RegType(TtClassMeta kls)
        {
            var fname = kls.ClassType.FullName.Replace('+', '.');
            var segs = fname.Split('.');

            var nsp = SureNS(RootNS, segs, 0);
            nsp.Types.Add(kls);
        }
        private static TtNameSpace SureNS(TtNameSpace cur, string[] ns, int index)
        {
            if (index == ns.Length - 1)
            {
                return cur;
            }
            var n = ns[index];
            var nsp = cur.GetNS(n);
            return SureNS(nsp, ns, index + 1);
        }
    }
}

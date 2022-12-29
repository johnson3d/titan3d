using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class UNameSpace
    {
        public string Name;
        public List<UNameSpace> ChildrenNS = new List<UNameSpace>();
        public List<UClassMeta> Types = new List<UClassMeta>();
        public UNameSpace GetNS(string n)
        {
            foreach(var i in ChildrenNS)
            {
                if (i.Name == n)
                    return i;
            }
            var ns = new UNameSpace();
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
                    if (j.Field.Name.Contains(filter))
                        return true;
                }
                foreach (var j in i.CurrentVersion.Propertys)
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
    public class UTypeTreeManager
    {
        public UTypeTreeManager()
        {
            RootNS.Name = "global";
        }
        public UNameSpace RootNS = new UNameSpace();
        public void RegType(UClassMeta kls)
        {
            var fname = kls.ClassType.FullName.Replace('+', '.');
            var segs = fname.Split('.');

            var nsp = SureNS(RootNS, segs, 0);
            nsp.Types.Add(kls);
        }
        private static UNameSpace SureNS(UNameSpace cur, string[] ns, int index)
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

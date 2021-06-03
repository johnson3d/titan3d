using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class NameSpace
    {
        public string Name;
        public List<NameSpace> ChildrenNS = new List<NameSpace>();
        public List<UClassMeta> Types = new List<UClassMeta>();
        public NameSpace GetNS(string n)
        {
            foreach(var i in ChildrenNS)
            {
                if (i.Name == n)
                    return i;
            }
            var ns = new NameSpace();
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
                    if (j.Method.Name.Contains(filter))
                        return true;
                }
                foreach (var j in i.Fields)
                {
                    if (j.Field.Name.Contains(filter))
                        return true;
                }
                foreach (var j in i.CurrentVersion.Fields)
                {
                    if (j.FieldName.Contains(filter))
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
    public class TypeTreeManager
    {
        public TypeTreeManager()
        {
            RootNS.Name = "global";
        }
        public NameSpace RootNS = new NameSpace();
        public void RegType(UClassMeta kls)
        {
            var fname = kls.ClassType.FullName.Replace('+', '.');
            var segs = fname.Split('.');

            var nsp = SureNS(RootNS, segs, 0);
            nsp.Types.Add(kls);
        }
        private static NameSpace SureNS(NameSpace cur, string[] ns, int index)
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

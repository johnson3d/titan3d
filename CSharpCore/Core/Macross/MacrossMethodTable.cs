using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Macross
{
    public partial class MacrossMethodTable
    {
        public static MacrossMethodTable Instance
        {
            get;
        } = new MacrossMethodTable();
        public void RebuildTable()
        {
            Methods.Clear();
            Properties.Clear();
            var assm = EngineNS.Rtti.RttiHelper.GetAnalyseAssembly(ECSType.Client, "EngineCore");
            BuildAssembly(assm.Assembly);
            assm = EngineNS.Rtti.RttiHelper.GetAnalyseAssembly(ECSType.Client, "Game");
            BuildAssembly(assm.Assembly);
        }
        protected List<System.Reflection.MethodInfo> Methods = new List<System.Reflection.MethodInfo>();
        protected List<System.Reflection.PropertyInfo> Properties = new List<System.Reflection.PropertyInfo>();
        private void BuildAssembly(System.Reflection.Assembly asm)
        {
            var types = asm.GetTypes();
            foreach(var i in types)
            {
                var mtds = i.GetMethods();
                foreach(var j in mtds)
                {
                    var attrs = j.GetCustomAttributes(typeof(Editor.MacrossMemberAttribute), false);
                    if (attrs == null || attrs.Length == 0)
                        continue;

                    Methods.Add(j);
                }
                var props = i.GetProperties();
                foreach (var j in props)
                {
                    var attrs = j.GetCustomAttributes(typeof(Editor.MacrossMemberAttribute), false);
                    if (attrs == null || attrs.Length == 0)
                        continue;

                    Properties.Add(j);
                }
            }
        }
        public List<System.Reflection.MethodInfo> FindMethod(string name)
        {
            var result = new List<System.Reflection.MethodInfo>();
            foreach (var i in Methods)
            {
                var lh = i.Name.ToLower();
                if (lh.Contains(name.ToLower()))
                {
                    result.Add(i);
                }
            }
            return result;
        }
        public List<System.Reflection.PropertyInfo> FindProperty(string name)
        {
            var result = new List<System.Reflection.PropertyInfo>();
            foreach (var i in Properties)
            {
                var lh = i.Name.ToLower();
                if (lh.Contains(name.ToLower()))
                {
                    result.Add(i);
                }
            }
            return result;
        }

        public List<System.Reflection.MethodInfo> GetMethodMatches(System.Type t)
        {
            var result = new List<System.Reflection.MethodInfo>();
            foreach (var i in Methods)
            {
                if (i.ReturnType == t)
                {
                    result.Add(i);
                    continue;
                }
                var pmts = i.GetParameters();
                foreach(var j in pmts)
                {
                    if(j.IsOut && j.ParameterType==t)
                    {
                        result.Add(i);
                        break;
                    }
                }
            }
            return result;
        }

        public List<System.Reflection.PropertyInfo> GetPropertyMatches(System.Type t)
        {
            var result = new List<System.Reflection.PropertyInfo>();
            foreach (var i in Properties)
            {
                if (i.CanRead == false)
                    continue;
                if (i.PropertyType == t)
                {
                    result.Add(i);
                    continue;
                }
            }
            return result;
        }
    }
}

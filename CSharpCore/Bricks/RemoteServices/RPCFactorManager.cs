using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public class FactoryObjectAttribute : System.Attribute
    {

    }
    public class FactoryObjectManager
    {
        public static FactoryObjectManager Instance = new FactoryObjectManager();
        public Dictionary<System.Type, System.Type> Factorys
        {
            get;
        } = new Dictionary<Type, Type>();

        private static Type GetRootAttributeType(Type t, Type root)
        {
            var attrs = t.GetCustomAttributes(typeof(FactoryObjectAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                root = t;
            }

            if (t.BaseType == null)
                return root;
            else
                return GetRootAttributeType(t.BaseType, root);
        }
        public void BuildFactory(System.Reflection.Assembly[] assems)
        {
            if(assems==null)
            {
                assems = AppDomain.CurrentDomain.GetAssemblies();
            }
            for (var i = 0; i < assems.Length; i++)
            {
                try
                {
                    var assm = assems[i];
                    var types = assm.GetTypes();
                    foreach (var t in types)
                    {
                        var attrs = t.GetCustomAttributes(typeof(FactoryObjectAttribute), true);
                        if (attrs == null || attrs.Length == 0)
                            continue;
                        var root = GetRootAttributeType(t, t);
                        var att = attrs[0] as FactoryObjectAttribute;
                        System.Type curType = null;
                        if (Factorys.TryGetValue(root, out curType) == false)
                        {
                            Factorys.Add(root, t);
                        }
                        else
                        {
                            if (t.IsSubclassOf(curType))
                                Factorys[root] = t;
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
        }

        public T CreateObject<T>() where T : class
        {
            return CreateObject(typeof(T)) as T;
        }
        public object CreateObject(Type baseType)
        {
            Type t;
            if (Factorys.TryGetValue(baseType, out t))
            {
                return System.Activator.CreateInstance(t);
            }
            System.Diagnostics.Debug.WriteLine($"Class {baseType.FullName} isn't a Factory Type");
            return null;
        }
    }
}
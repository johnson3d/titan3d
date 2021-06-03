using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Network.RPC
{
    public class URpcClass
    {
        public ERunTarget RunTarget;
        public EExecuter Executer;
        FCallMethod[] Methods = new FCallMethod[UInt16.MaxValue];
        public URpcClass(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(URpcClassAttribute), false);
            if (attrs.Length == 0)
                throw new UException("");

            var kls = attrs[0] as URpcClassAttribute;
            RunTarget = kls.RunTarget;
            Executer = kls.Executer;

            var methods = type.GetMethods();
            foreach (var i in methods)
            {
                attrs = i.GetCustomAttributes(typeof(URpcMethodAttribute), false);
                if (attrs.Length == 0)
                    continue;

                if (CheckDefine(i) == false)
                {
                    throw new UException("");
                }

                var dlgt = GetFieldInherit(type, $"rpc_{i.Name}", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (dlgt == null)
                    throw new UException("");

                var fun = dlgt.GetValue(null) as FCallMethod;
                if (fun == null)
                    throw new UException("");

                var mtd = attrs[0] as URpcMethodAttribute;
                if (Methods[mtd.Index] != null)
                    throw new UException("");
                Methods[mtd.Index] = fun;
            }
        }
        static System.Reflection.FieldInfo GetFieldInherit(System.Type type, string name, System.Reflection.BindingFlags flags)
        {
            var dlgt = type.GetField(name, flags);
            if (dlgt != null)
            {
                return dlgt;
            }
            if (type.BaseType == null)
                return null;
            return GetFieldInherit(type.BaseType, name, flags);
        }
        private bool CheckDefine(System.Reflection.MethodInfo mtd)
        {
            System.Type realRetType = mtd.ReturnType;
            if (mtd.ReturnType.IsGenericType && mtd.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task"))
            {
                realRetType = mtd.ReturnType.GetGenericArguments()[0];
            }
            if (realRetType != typeof(void) &&
                realRetType != typeof(string) &&
                realRetType.IsValueType == false &&
                realRetType.Name != "ISerializer" &&
                realRetType.GetInterface("ISerializer") == null)
                return false;
            var parms = mtd.GetParameters();
            if (parms.Length != 2)
                return false;
            if (parms[0].ParameterType != typeof(string) && 
                parms[0].ParameterType.IsValueType == false &&
                parms[0].ParameterType.Name != "ISerializer" &&
                parms[0].ParameterType.GetInterface("ISerializer") == null)
                return false;
            if (parms[1].ParameterType != typeof(UCallContext))
                return false;
            return true;
        }
        public FCallMethod GetCallee(UInt16 index)
        {
            return Methods[index];
        }
    }
}

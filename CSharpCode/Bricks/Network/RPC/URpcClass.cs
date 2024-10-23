﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Network.RPC
{
    public class URpcClass
    {
        public ERunTarget RunTarget;
        public EExecuter Executer;
        public struct FRpcInfo
        {
            public string Name;
            public FCallMethod Method;
            public URpcMethodAttribute Attribute;
        }

        FRpcInfo[] Methods = new FRpcInfo[UInt16.MaxValue];
        public URpcClass(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(URpcClassAttribute), true);
            if (attrs.Length == 0)
                throw new TtException("");

            var kls = attrs[0] as URpcClassAttribute;
            RunTarget = kls.RunTarget;
            Executer = kls.Executer;

            var methods = type.GetMethods();
            foreach (var i in methods)
            {
                attrs = i.GetCustomAttributes(typeof(URpcMethodAttribute), true);
                if (attrs.Length == 0)
                    continue;

                if (CheckDefine(i) == false)
                {
                    throw new TtException("");
                }

                var dlgt = GetFieldInherit(type, $"rpc_{i.Name}", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (dlgt == null)
                    throw new TtException("");

                var fun = dlgt.GetValue(null) as FCallMethod;
                if (fun == null)
                    throw new TtException("");

                var mtd = attrs[0] as URpcMethodAttribute;
                if (Methods[mtd.Index].Method != null)
                    throw new TtException("");
                Methods[mtd.Index].Name = i.Name;
                Methods[mtd.Index].Method = fun;
                Methods[mtd.Index].Attribute = mtd;
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
                realRetType != typeof(RName) &&
                realRetType.IsValueType == false &&
                realRetType.Name != "ISerializer" &&
                realRetType.GetInterface("ISerializer") == null)
            {
                Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Error, $"{mtd.Name} return type is invalid");
                return false;
            }
                
            var parms = mtd.GetParameters();
            //if (parms.Length != 2)
            //    return false;
            
            for (int i = 0; i < parms.Length - 1; i++)
            {
                if (parms[i].ParameterType != typeof(string) &&
                    parms[i].ParameterType != typeof(RName) &&
                    parms[i].ParameterType.IsValueType == false &&
                    parms[i].ParameterType.Name != "ISerializer" &&
                    parms[i].ParameterType.GetInterface("ISerializer") == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Error, $"{mtd.Name} parameter{i} type is invalid");
                    return false;
                }
                    
            }
            if (parms[parms.Length - 1].ParameterType != typeof(TtCallContext))
                return false;
            return true;
        }
        public FRpcInfo GetCallee(UInt16 index)
        {
            return Methods[index];
        }
    }
}

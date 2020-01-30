using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public enum RPCExecuteLimitLevel
    {
        Unknown = 0,
        Player = 100,
        Lord = 200,
        GM = 300,
        Developer = 400,
        God = 500,
        TheOne = 600,
    }
    public class RPCCallAttribute : System.Attribute
    {
        public RPCExecuteLimitLevel LimitLevel
        {
            get;
            set;
        } = RPCExecuteLimitLevel.Developer;
        public bool IsWeakPkg { get; set; }
    }

    public class RPCExec
    {
        public System.Reflection.MethodInfo Method;
        
        public UInt16 Index = RPCExecManager.MaxRPC;
        public UInt32 MethordHash;
        public string HashString;
        public FWritePackage OnWritePackage;
        public string Macro = null;
        public RPCCallAttribute CallAttr;

        private System.Type mParamType;
        public System.Type ParamType
        {
            get { return mParamType; }
        }

        public void SetMethod(System.Reflection.MethodInfo mtd, UInt16 index, System.Type paramType)
        {
            Index = index;
            Method = mtd;
            HashString = RPCParameter.GetRPCCode(mtd);
            MethordHash = UniHash.APHash(HashString);

            mParamType = paramType;
            if (paramType.BaseType != null && paramType.BaseType.IsGenericType)
            {
                var typeArgs = paramType.BaseType.GenericTypeArguments;
                if (typeArgs.Length > 1)
                {
                    if (typeArgs[0].FullName != paramType.FullName)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }
            }
        }

        public virtual RPCParameter CreateParameter(PkgReader pkg)
        {
            //反射读取包
            //如果生成代码，这里就直接new ，然后按顺序read包数据

            var param = System.Activator.CreateInstance(ParamType) as RPCParameter;
            if (param == null)
                return null;

            param.ReadObject(pkg);

            return param;
        }
        public virtual void DestroyParameter(RPCParameter obj)
        {

        }
        public virtual object Execute(object host, RPCParameter parameter)
        {
            //var method = parameter.GetRPCMethod();
            var args = new object[] { parameter };
            return Method.Invoke(host, args);
        }

        #region CodeGen
        public UInt32 GetMethodHash()
        {
            var code = RPCParameter.GetRPCCode(Method);
            return (UInt32)UniHash.APHash(code);
        }
        
        public string GetExecuteCode()
        {
            var codeRead = "";
            //codeRead += RPCParameter.GetPkgReadCode(ParamType);
            //codeRead += RPCParameter.GetPkgWriteCode(ParamType);

            var code = $"public class RPCExec_{GetMethodHash()} : {typeof(EngineNS.Bricks.RemoteServices.RPCExec).FullName}\r\n";
            code += "{\r\n";

            if (string.IsNullOrEmpty(Macro) == false)
                code += $"#if {Macro}\r\n";

            var code1 = $"   public override {typeof(EngineNS.Bricks.RemoteServices.RPCParameter).FullName} CreateParameter({typeof(EngineNS.Bricks.RemoteServices.PkgReader).FullName} pkg)\r\n";
            code1 += "  {\r\n";
            var typeName = ParamType.FullName.Replace('+', '.');
            code1 += "      var parameter = new " + typeName + "();\r\n";
            code1 += "      parameter.ReadObject(pkg);\r\n";
            code1 += "      return parameter;\r\n";
            code1 += "  }\r\n";

            code += code1;

            var code2 = $"   public override object Execute(object host, {typeof(EngineNS.Bricks.RemoteServices.RPCParameter).FullName} parameter)\r\n";
            code2 += "  {\r\n";
            code2 += $"     var obj = ({Method.DeclaringType.FullName})host;\r\n";
            if (Method.ReturnType != typeof(void))
            {
                code2 += $"     var task = obj.{Method.Name}(({typeName})parameter);\r\n";
            }
            else
            {
                code2 += $"     obj.{Method.Name}(({typeName})parameter);\r\n";
            }
            code2 += "      return null;\r\n";
            code2 += "  }\r\n";

            code += code2;

            code += codeRead;

            if (string.IsNullOrEmpty(Macro) == false)
                code += $"#endif\r\n";

            code += "}\r\n";
            return code;
        }
        #endregion
    }

    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public partial class RPCExecManager
    {
        public const UInt16 MaxRPC = 0x7FFF;
        public const UInt16 WaitFlag = 0x8000;

        public ERouteTarget AppTarget
        {
            get;
            set;
        }

        public static RPCExecManager Insance = new RPCExecManager();

        #region IndexToExe
        public Dictionary<UInt32, RPCExec> HashExec
        {
            get;
        } = new Dictionary<uint, RPCExec>();
        private RPCExec[] mExec = new RPCExec[MaxRPC];
        private UInt16 FindValidIndex()
        {
            for (UInt16 i = 0; i < MaxRPC; i++)
            {
                if (mExec[i] == null)
                    return i;
            }
            return UInt16.MaxValue;
        }
        
        public RPCExec GetExecByIndex(UInt16 index)
        {
            if (index >= MaxRPC)
                return null;
            return mExec[index];
        }
        public RPCExec GetExecByHash(UInt32 hash)
        {
            RPCExec result;
            if (HashExec.TryGetValue(hash, out result) == false)
                return null;
            return result;
        }
        public void SystemRegExe(UInt16 index, System.Type argType, RPCExec binder)
        {
            var arg = System.Activator.CreateInstance(argType) as RPCParameter;
            var old = arg.GetMethodBinder();
            arg.SystemSetMethodBinder(binder);

            System.Diagnostics.Debug.Assert(old.Method != null);

            binder.SetMethod(old.Method, index, old.ParamType);

            binder.CallAttr = old.CallAttr;
            binder.OnWritePackage = old.OnWritePackage;
            binder.Macro = old.Macro;
            

            mExec[old.Index] = null;
            mExec[index] = binder;
            HashExec[binder.MethordHash] = binder;
        }
        #endregion

        #region RegRPC
        private class RPCTypeInfo
        {
            public System.Type Type;
            public string Macro;
        }
        private string GetAssmemblyRPCMacro(System.Reflection.Assembly assm)
        {
            var klass = assm.GetType("EngineNS.Bricks.RemoteServices." + "AssemblyInfo");
            if (klass == null)
                return "";
            var fun = klass.GetMethod("GetMacro");
            if (fun == null)
                return "";
            return fun.Invoke(null, null) as string;
        }
        private bool IsRPCClass(System.Type type)
        {
            foreach(var i in type.GetMethods())
            {
                var rpcAttr = Rtti.AttributeHelper.GetCustomAttribute(i, typeof(RPCCallAttribute).FullName, true);
                if (rpcAttr != null)
                    return true;
            }
            return false;
        }
        private List<RPCTypeInfo> GetFinalKlass(List<RPCTypeInfo> types)
        {
            var result = new List<RPCTypeInfo>();
            for(int i=0;i<types.Count;i++)
            {
                var t = types[i];
                bool isSubclass = false;
                for (int j = 0; j < types.Count; j++)
                {
                    if (types[i] == types[j])
                        continue;
                    if (types[j].Type.IsSubclassOf(t.Type))
                    {
                        isSubclass = true;
                        break;
                    }
                }
                if(isSubclass==false)
                {
                    result.Add(t);
                }
            }
            return result;
        }
        
        public void BuildAssemblyRPC(System.Reflection.Assembly[] assems, string[] assemMacro, EngineNS.ECSType csType)
        {
            if (this.HashExec.Count > 0)
                return;
            if(assems==null)
            {
                assems = AppDomain.CurrentDomain.GetAssemblies();
            }
            List<RPCTypeInfo> AllClass = new List<RPCTypeInfo>();
            var mapRpcFuncs = new List<System.Reflection.MethodInfo>();
            for (var i = 0; i < assems.Length; i++)
            {
                try
                {
                    var assm = assems[i];

                    var mapKlass = assm.GetType("EngineNS.Bricks.RemoteServices" + ".SystemMapRPC");
                    if (mapKlass != null)
                    {
                        var mapFun = mapKlass.GetMethod("DoRPCMap");
                        if (mapFun != null)
                        {
                            var args = mapFun.GetParameters();
                            if (args.Length != 1)
                                continue;
                            if (args[0].ParameterType != this.GetType())
                                continue;
                            if (mapFun.IsStatic == false)
                                continue;

                            mapRpcFuncs.Add(mapFun);
                            //mapFun.Invoke(null, new object[] { this });
                        }
                    }

                    if (assemMacro != null && assemMacro.Length == assems.Length && string.IsNullOrEmpty(assemMacro[i]))
                    {
                        assemMacro[i] = GetAssmemblyRPCMacro(assm);
                    }
                    foreach (var j in assm.GetTypes())
                    {
                        if (IsRPCClass(j))
                        {
                            var tmp = new RPCTypeInfo();
                            tmp.Type = j;
                            if (assemMacro != null && assemMacro.Length == assems.Length)
                            {
                                tmp.Macro = assemMacro[i];
                            }
                            AllClass.Add(tmp);
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
            //只有不会再被派生的类才能做RPC分析，否则就会出现一个RPCParameter被多个函数使用的情况
            var finalKlass = GetFinalKlass(AllClass);
            foreach(var i in finalKlass)
            {
                RegClass(i.Type, i.Macro, csType);
            }

            RefreshRPCMap(mapRpcFuncs);
        }
        public void RefreshRPCMap(List<System.Reflection.MethodInfo> funcs)
        {
            foreach(var i in funcs)
            {
                i.Invoke(null, new object[] { this });
            }
        }
        public void RegRPC(RPCParameter arg, System.Reflection.MethodInfo method, string macro)
        {
            var desc = arg.GetMethodBinder();
            if(desc.Method!=null)
            {
                System.Diagnostics.Debug.Assert(desc.Method == method);
                //同一个RPCParameter被多个RPCMethod作为参数使用，这是不符合系统需求的
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            var index = desc.Index;
            if (index == MaxRPC)
                index = FindValidIndex();

            desc.SetMethod(method, index, arg.GetType());
            desc.Macro = macro;

            var rpcAttr = Rtti.AttributeHelper.GetCustomAttribute(method, typeof(RPCCallAttribute).FullName, true);
            if (rpcAttr != null)
            {
                var cattr = rpcAttr as RPCCallAttribute;
                if (cattr != null)
                    desc.CallAttr = cattr;
            }
            else
            {
                desc.CallAttr = null;
            }

            HashExec[desc.MethordHash] = desc;
            mExec[index] = desc;
        }
        public void RegClass(System.Type klass, string macro, ECSType csType)
        {
            var methods = klass.GetMethods();
            foreach (var i in methods)
            {
                var rpcAttr = Rtti.AttributeHelper.GetCustomAttribute(i, typeof(RPCCallAttribute).FullName, true);
                if (rpcAttr == null)
                    continue;

                var args = i.GetParameters();
                if (args.Length != 1)
                    continue;

                if (Rtti.RttiHelper.IsSubclassOf(args[0].ParameterType, typeof(RPCParameter).FullName) == false)
                    continue;
                var type = Rtti.RttiHelper.GetTypeFromTypeFullName(args[0].ParameterType.FullName, csType);
                var parameter = System.Activator.CreateInstance(type) as RPCParameter;
                
                RegRPC(parameter, i, macro);
            }
        }
        #endregion

        #region CodeGen
        public void SaveCode()
        {
            string file = CEngineDesc.RPCAutoCodeFile;
            if (System.IO.File.Exists(file))
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(file);
                if (sr != null)
                {
                    var oldCode = sr.ReadToEnd();
                    sr.Close();
                    var code = GetSaveCode();
                    if (oldCode != code)
                    {
                        System.IO.StreamWriter sw = new System.IO.StreamWriter(file);
                        if (sw != null)
                        {
                            sw.Write(code);
                            sw.Close();
                        }
                    }
                }
            }
            //else
            //{
            //    var code = GetSaveCode();
            //    System.IO.StreamWriter sw = new System.IO.StreamWriter(file);
            //    if (sw != null)
            //    {
            //        sw.Write(code);
            //        sw.Close();
            //    }
            //}
        }
        private string GetSaveCode()
        {
            string codeClass = "";
            string codeRead = "";
            string code = $"namespace EngineNS.Bricks.RemoteServices\r\n";
            code += "{\r\n";
            code += "   public class SystemMapRPC\r\n";
            code += "   {\r\n";
            code += $"       public static void DoRPCMap({typeof(EngineNS.Bricks.RemoteServices.RPCExecManager).FullName} manager)\r\n";
            code += "       {\r\n";
            for (var i=0;i<mExec.Length;i++)
            {
                var exec = mExec[i];
                if (exec == null)
                    continue;//exec.Desc == null说明这个函数已经不存在了

                codeClass += exec.GetExecuteCode();

                UInt32 hashCode = exec.GetMethodHash();
                var fixedFullName = exec.ParamType.FullName.Replace('+', '.');
                code += $"          manager.SystemRegExe({i}, typeof({fixedFullName}), new RPCExec_{hashCode}());\r\n";
            }
            code += "       }\r\n";

            code += codeRead;

            code += "   }\r\n";
            code += "}\r\n";
            return codeClass + code;
        }
        #endregion

        #region WaitRPC
        static UInt16 CurSerialId = 0;
        public delegate void FRPCReturnCallBack(PkgReader data, bool isTimeOut);
        public class RPCWait
        {
            public UInt16 SerialId;
            public FRPCReturnCallBack RetCallBack;
            public long CallTime;
            public long Timout = long.MaxValue;
        }
        public PooledObject<RPCWait> RPCWaitAllocator
        {
            get;
        } = new PooledObject<RPCWait>();
        public Dictionary<UInt16, RPCWait> WaitCallBacks
        {
            get;
        } = new Dictionary<ushort, RPCWait>();
        public RPCExecManager.RPCWait AddCalBack(RPCExecManager.FRPCReturnCallBack cb)
        {
            lock (WaitCallBacks)
            {
                var serialId = CurSerialId++;
                RPCExecManager.RPCWait wait;
                if (WaitCallBacks.TryGetValue(serialId, out wait))
                {
                    if (wait.RetCallBack != null && wait.RetCallBack.Method != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"RPCExecManager A CallBack has Timeout:{wait.RetCallBack.Method.DeclaringType.FullName}.{wait.RetCallBack.Method.Name}");
                    }
                    wait.RetCallBack(PkgReader.NullReader, true);
                    wait.RetCallBack = cb;
                }
                else
                {
                    var tempTask = RPCWaitAllocator.QueryObject();
                    tempTask.Wait();
                    wait = tempTask.Result;
                    wait.SerialId = serialId;
                    wait.RetCallBack = cb;
                    wait.CallTime = Support.Time.GetTickCount();
                    WaitCallBacks.Add(wait.SerialId, wait);
                }

                return wait;
            }
        }

        public void TryDoTimeout()
        {
            lock (WaitCallBacks)
            {
                var rmvList = new List<UInt16>();
                var rmvObjList = new List<RPCWait>();
                var now = Support.Time.GetTickCount();
                foreach (var i in WaitCallBacks)
                {
                    if(now - i.Value.CallTime > i.Value.Timout)
                    {
                        rmvList.Add(i.Key);
                        rmvObjList.Add(i.Value);
                    }
                }

                foreach(var i in rmvList)
                {
                    WaitCallBacks.Remove(i);
                }
                rmvList.Clear();
                foreach(var i in rmvObjList)
                {
                    RPCWaitAllocator.ReleaseObject(i);
                }
                rmvObjList.Clear();
            }
        }
        public void DoCallBack(UInt16 serialId, PkgReader data)
        {
            lock (WaitCallBacks)
            {
                RPCWait wait;
                if (WaitCallBacks.TryGetValue(serialId, out wait))
                {
                    WaitCallBacks.Remove(serialId);
#if PWindow
                    var saved = System.Threading.SynchronizationContext.Current;
                    System.Threading.SynchronizationContext.SetSynchronizationContext(null);
                    wait.RetCallBack(data, false);
                    System.Threading.SynchronizationContext.SetSynchronizationContext(saved);
#else
                    wait.RetCallBack(data, false);
#endif
                    RPCWaitAllocator.ReleaseObject(wait);
                }
            }
        }
        #endregion
    }
}

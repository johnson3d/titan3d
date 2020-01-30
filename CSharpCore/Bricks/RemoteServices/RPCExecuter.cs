using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public class RPCCallAttribute : System.Attribute
    {
        public NetCore.RPCExecuteLimitLevel LimitLevel
        {
            get;
            set;
        } = NetCore.RPCExecuteLimitLevel.Developer;
        public bool IsWeakPkg { get; set; }
    }
    public class RPCExecuter
    {
        public readonly static RPCExecuter Instance = new RPCExecuter();
        public const UInt16 MaxRPC = 0x7FFF;
        public const UInt16 WaitFlag = 0x8000;
        public object Execute(ref NetCore.PkgReader pkg, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            UInt16 rpcIndex = 0;
            UInt32 rpcHash = 0;
            bool hasReturn = false;
            bool isHashIndex = pkg.IsHashIndex();
            if (isHashIndex)
            {
                hasReturn = pkg.IsHasReturn();
                pkg.Read(out rpcHash);
            }
            else
            {
                pkg.Read(out rpcIndex);
                hasReturn = (rpcIndex & WaitFlag) != 0 ? true : false;
            }
            if (hasReturn != pkg.IsHasReturn())
            {
                System.Diagnostics.Debug.Assert(false);
            }
            UInt16 serialId = 0;
            if (hasReturn)
            {
                pkg.Read(out serialId);
                rpcIndex = (UInt16)(rpcIndex & MaxRPC);
            }
            RPCProcessor exec = null;
            if (isHashIndex)
            {
                exec = GetExecByHash(rpcHash);
            }
            else
            {
                exec = GetExecByIndex(rpcIndex);
            }
            if (exec == null)
            {
                System.Diagnostics.Debug.WriteLine($"RPC GetExecByIndex is null:{rpcIndex}");
                return null;
            }
            object host = exec.GetHostObject(ref routeInfo);
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"RPC HostObject is null:{exec.GetType().FullName}");
                return null;
            }
            var authority = exec.GetAuthority(host);
            if (authority < exec.CallAttr.LimitLevel)
            {//超越权限
                //System.Diagnostics.Debug.WriteLine($"Over Authority[{authority}<{exec.CallAttr.LimitLevel.ToString()}]:{parameter.GetHostObjectName()}=>{parameter.GetRPCMethod().Name}");
                return null;
            }

            try
            {
                //这里如果反射调用就有GC，生成代码可以去掉GC
                return exec.Execute(host, ref pkg, serialId, connect, ref routeInfo);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
            finally
            {
                
            }
        }
        public void SaveCode()
        {

        }
        #region Connect
        public NetCore.NetConnection C2SConnect
        {
            get;
            set;
        }
        public NetCore.NetConnection GetServerConnect(NetCore.ERouteTarget target)
        {
            return PkgRcver.GetServerConnect(target);
        }
        private NetCore.NetConnection RouterTargetConnect(NetCore.ERouteTarget target, NetCore.RPCRouter router)
        {
            switch (target)
            {
                case NetCore.ERouteTarget.Reg:
                case NetCore.ERouteTarget.Log:
                case NetCore.ERouteTarget.Path:
                case NetCore.ERouteTarget.Data:
                case NetCore.ERouteTarget.Keep:
                    return PkgRcver.GetServerConnect(target);
                case NetCore.ERouteTarget.Hall:
                    return router.ToHConnect;
                case NetCore.ERouteTarget.Gate:
                    return router.ToGConnect;
                default:
                    break;
            }
            return null;
        }
        #endregion
        #region NetEvent
        public IPackageReceiver PkgRcver
        {
            get;
            set;
        }
        public void ReceiveData(NetCore.NetConnection sender, byte[] pData, int nLength, Int64 recvTime)
        {
            unsafe
            {
                if (nLength < sizeof(NetCore.PkgHeader))
                {
                    return;
                }
            }

            var router = sender.Router as NetCore.RPCRouter;
            if (router == null)
            {
                System.Diagnostics.Debug.WriteLine($"connect bind a invalid data");
                return;
            }
            var pkg = new NetCore.PkgReader(pData, nLength, recvTime);
            try
            {
                if (PkgRcver != null)
                {
                    PkgRcver.OnReceivePackage(sender, pkg);
                }

                byte route = 0;
                pkg.Read(out route);
                var isReturn = (route & (byte)NetCore.ERouteTarget.ReturnFlag) != 0 ? true : false;
                route = (byte)(route & (byte)NetCore.ERouteTarget.ReturnMask);
                var target = (NetCore.ERouteTarget)route;
                switch (target)
                {
                    case NetCore.ERouteTarget.Self:
                        {
                            if (isReturn)
                            {
                                UInt16 serialId = 0;
                                pkg.Read(out serialId);
                                DoCallBack(serialId, ref pkg);
                            }
                            else
                            {
                                Execute(ref pkg, sender, ref router.RouteInfo);
                            }
                        }
                        break;
                    case NetCore.ERouteTarget.Routed:
                        {
                            var routeInfo = new NetCore.RPCRouter.RouteData();
                            routeInfo.Load(ref pkg);
                            Execute(ref pkg, sender, ref routeInfo);
                        }
                        break;
                    case NetCore.ERouteTarget.Client:
                        {//只有GateServer才有转发到客户端的需求
#if Server
                            var routePkg = new PkgWriter();
                            var routeInfo = new RPCRouter.RouteData();
                            try
                            {
                                routeInfo.Load(pkg);
                                if (Titan3D.Server.IGateServer.Instance != null)
                                {
                                    if (isReturn)
                                    {
                                        route = (byte)((byte)ERouteTarget.Self | (byte)ERouteTarget.ReturnFlag);
                                        routePkg.Write(route);
                                        UInt16 seriaId;
                                        pkg.Read(out seriaId);
                                        routePkg.Write(seriaId);
                                    }
                                    else
                                    {
                                        route = (byte)ERouteTarget.Self;
                                        routePkg.Write(route);
                                    }
                            
                                    routePkg.AppendPkg(pkg, pkg.GetPosition());
                                    routePkg.SendBuffer(Titan3D.Server.IGateServer.Instance.GetClientConnect(routeInfo.RouteSlot));
                                }
                            }
                            finally
                            {
                                routePkg.Dispose();
                            }
#endif
                        }
                        break;
                    case NetCore.ERouteTarget.Hall:
                        {
                            var routePkg = new NetCore.PkgWriter(nLength + 64);
                            try
                            {
                                routePkg.Write((byte)NetCore.ERouteTarget.Routed);
                                router.RouteInfo.Save(ref routePkg, NetCore.ERouteTarget.Hall);
                                routePkg.SetFlags(pkg.GetFlags());
                                routePkg.AppendPkg(pkg, pkg.GetPosition());
                                routePkg.SendBuffer(RouterTargetConnect(target, router));
                            }
                            finally
                            {
                                routePkg.Dispose();
                            }
                        }
                        break;
                    case NetCore.ERouteTarget.Data:
                    case NetCore.ERouteTarget.Keep:
                    case NetCore.ERouteTarget.Reg:
                    case NetCore.ERouteTarget.Path:
                    case NetCore.ERouteTarget.Log:
                        {
                            var routePkg = new NetCore.PkgWriter(nLength + 64);
                            try
                            {
                                routePkg.Write((byte)NetCore.ERouteTarget.Routed);
                                router.RouteInfo.Save(ref routePkg, target);
                                routePkg.SetFlags(pkg.GetFlags());
                                routePkg.AppendPkg(pkg, pkg.GetPosition());
                                routePkg.SendBuffer(GetServerConnect(target));
                            }
                            finally
                            {
                                routePkg.Dispose();
                            }
                        }
                        break;
                }
            }
            finally
            {
                pkg.Dispose();
            }
        }
        #endregion
        #region WaitRPC
        static UInt16 CurSerialId = 0;
        public delegate void FRPCReturnCallBack(ref NetCore.PkgReader data, bool isTimeOut);
        public class RPCWait
        {
            public UInt16 SerialId;
            public FRPCReturnCallBack RetCallBack;
            public long CallTime;
            public long Timout = long.MaxValue;
            public RPCProcessor Processor;
        }
        public PooledObject<RPCWait> RPCWaitAllocator
        {
            get;
        } = new PooledObject<RPCWait>();
        public Dictionary<UInt16, RPCWait> WaitCallBacks
        {
            get;
        } = new Dictionary<ushort, RPCWait>();
        public RPCWait AddCalBack(FRPCReturnCallBack cb)
        {
            lock (WaitCallBacks)
            {
                var serialId = CurSerialId++;
                RPCWait wait;
                if (WaitCallBacks.TryGetValue(serialId, out wait))
                {
                    if (wait.RetCallBack != null && wait.RetCallBack.Method != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"RPCExecManager A CallBack has Timeout:{wait.RetCallBack.Method.DeclaringType.FullName}.{wait.RetCallBack.Method.Name}");
                    }
                    wait.RetCallBack(ref NetCore.PkgReader.NullReader, true);
                    wait.RetCallBack = cb;
                }
                else
                {
                    var tempTask = RPCWaitAllocator.QueryObject(null);
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
                    if (now - i.Value.CallTime > i.Value.Timout)
                    {
                        rmvList.Add(i.Key);
                        rmvObjList.Add(i.Value);
                    }
                }

                foreach (var i in rmvList)
                {
                    WaitCallBacks.Remove(i);
                }
                rmvList.Clear();
                foreach (var i in rmvObjList)
                {
                    RPCWaitAllocator.ReleaseObject(i);
                }
                rmvObjList.Clear();
            }
        }
        public void DoCallBack(UInt16 serialId, ref NetCore.PkgReader data)
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
                    wait.RetCallBack(ref data, false);
                    System.Threading.SynchronizationContext.SetSynchronizationContext(saved);
#else
                    wait.RetCallBack(ref data, false);
#endif
                    RPCWaitAllocator.ReleaseObject(wait);
                }
            }
        }
        #endregion
        public RPCProcessor GetExecByHash(UInt32 hash)
        {
            RPCProcessor desc;
            if (DescTable.TryGetValue(hash, out desc))
            {
                return desc;
            }
            return null;
        }
        public RPCProcessor GetExecByIndex(UInt16 index)
        {
            return DescArray[index];
        }
        protected RPCProcessor[] DescArray = new RPCProcessor[MaxRPC];
        protected Dictionary<UInt32, RPCProcessor> DescTable = new Dictionary<UInt32, RPCProcessor>();

        #region RegRPC
        private UInt16 FindValidIndex()
        {
            for (UInt16 i = 0; i < MaxRPC; i++)
            {
                if (DescArray[i] == null)
                    return i;
            }
            return UInt16.MaxValue;
        }
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
            foreach (var i in type.GetMethods())
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
            for (int i = 0; i < types.Count; i++)
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
                if (isSubclass == false)
                {
                    result.Add(t);
                }
            }
            return result;
        }

        public void BuildAssemblyRPC(System.Reflection.Assembly[] assems, string[] assemMacro, EngineNS.ECSType csType)
        {
            if (assems == null)
            {
                assems = AppDomain.CurrentDomain.GetAssemblies();
            }
            List<RPCTypeInfo> AllClass = new List<RPCTypeInfo>();
            for (var i = 0; i < assems.Length; i++)
            {
                try
                {
                    var assm = assems[i];

                    if (assemMacro != null && assemMacro.Length == assems.Length && string.IsNullOrEmpty(assemMacro[i]))
                    {
                        assemMacro[i] = GetAssmemblyRPCMacro(assm);
                    }
                    foreach (var j in assm.GetTypes())
                    {
                        if(j.IsSubclassOf(typeof(RPCProcessor)))
                        {
                            if (j.IsGenericType)
                                continue;
                            if (RPCProcessor.GetProcessor(j)==null)
                            {
                                RPCProcessor.InitProcessor(j);
                            }
                        }

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
                catch (System.Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
            //只有不会再被派生的类才能做RPC分析，否则就会出现一个RPCParameter被多个函数使用的情况
            var finalKlass = GetFinalKlass(AllClass);
            foreach (var i in finalKlass)
            {
                RegClass(i.Type, i.Macro, csType);
            }

            RefreshRPCMap(null);
        }
        private class RPCProcessorMapping : IO.Serializer.Serializer
        {
            public class Desc : IO.Serializer.Serializer
            {
                [Rtti.MetaData]
                public string ProcessorName
                {
                    get;
                    set;
                }
                [Rtti.MetaData]
                public UInt16 Index
                {
                    get;
                    set;
                }
            }
            [Rtti.MetaData]
            public List<Desc> Descs
            {
                get;
                set;
            } = new List<Desc>();
        }
        public void RefreshRPCMap(List<System.Reflection.MethodInfo> funcs)
        {
            var rn = RName.GetRName("RPCMapping.xml");
            var mappings = IO.XmlHolder.CreateObjectFromXML(rn) as RPCProcessorMapping;
            if(mappings==null)
            {
#if PWindow
                mappings = new RPCProcessorMapping();
                for (UInt16 i = 0; i < DescArray.Length; i++)
                {
                    if (DescArray[i] == null)
                        continue;
                    
                    var desc = new RPCProcessorMapping.Desc();
                    desc.ProcessorName = Rtti.RttiHelper.GetTypeSaveString(DescArray[i].GetType());
                    desc.Index = i;
                    mappings.Descs.Add(desc);
                }
                IO.XmlHolder.SaveObjectToXML(mappings, rn);
#endif
            }
            else
            {
                bool isdirty = false;
                RPCProcessor[] SavedDescArray = DescArray;
                DescArray = new RPCProcessor[MaxRPC];
                for (UInt16 i = 0; i < (UInt16)mappings.Descs.Count; i++)
                {
                    var type = Rtti.RttiHelper.GetTypeFromSaveString(mappings.Descs[i].ProcessorName);
                    if(type==null)
                    {
                        isdirty = true;
                        continue;
                    }
                    var proc = RPCProcessor.GetProcessor(type);
                    if (proc == null)
                    {
                        isdirty = true;
                        continue;
                    }
                    DescArray[mappings.Descs[i].Index] = proc;
                    proc.RPCIndex = mappings.Descs[i].Index;
                }
                for (UInt16 i = 0; i < SavedDescArray.Length; i++)
                {
                    if (SavedDescArray[i] == null)
                        continue;

                    bool finded = false;
                    for (UInt16 j = 0; j < DescArray.Length; j++)
                    {
                        if(DescArray[j] == SavedDescArray[i])
                        {
                            finded = true;
                            break;
                        }
                    }

                    if (finded == false)
                    {
                        var idx = this.FindValidIndex();
                        DescArray[idx] = SavedDescArray[i];
                        DescArray[idx].RPCIndex = idx;
                        isdirty = true;
                    }
                }

                if(isdirty)
                {
#if PWindow
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "RPC", $"RPC mapping changed!");
                    mappings = new RPCProcessorMapping();
                    for (UInt16 i = 0; i < DescArray.Length; i++)
                    {
                        if (DescArray[i] == null)
                            continue;

                        var desc = new RPCProcessorMapping.Desc();
                        desc.ProcessorName = Rtti.RttiHelper.GetTypeSaveString(DescArray[i].GetType());
                        desc.Index = i;
                        mappings.Descs.Add(desc);
                    }
                    IO.XmlHolder.SaveObjectToXML(mappings, rn);
#endif
                }
            }
        }
        public void RegRPC(RPCProcessor arg, System.Reflection.MethodInfo method, System.Type argType, string macro)
        {
            if (arg.Method != null)
            {
                System.Diagnostics.Debug.Assert(arg.Method == method);
                //同一个RPCParameter被多个RPCMethod作为参数使用，这是不符合系统需求的
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            var index = arg.RPCIndex;
            if (index == MaxRPC)
                index = FindValidIndex();

            arg.SetMethod(method, index, argType);
            
            var rpcAttr = Rtti.AttributeHelper.GetCustomAttribute(method, typeof(RPCCallAttribute).FullName, true);
            if (rpcAttr != null)
            {
                var cattr = rpcAttr as RPCCallAttribute;
                if (cattr != null)
                    arg.CallAttr = cattr;
            }
            else
            {
                arg.CallAttr = null;
            }

            DescTable[arg.MethodHash] = arg;
            DescArray[index] = arg;
        }
        private System.Type GetRPCMethodProcessor(System.Reflection.ParameterInfo[] args)
        {
            if (args.Length != 5)
                return null;
            if (args[0].ParameterType != typeof(byte))
                return null;
            if (args[1].ParameterType.IsByRef == false || args[4].ParameterType.IsByRef == false)
            {
                return null;
            }
            if (args[1].ParameterType.GetElementType().GetInterface(typeof(IArgument).FullName) == null)
                return null;
            if (args[2].ParameterType != typeof(UInt16))
                return null;
            if (args[3].ParameterType != typeof(NetCore.NetConnection))
                return null;
            if (args[4].ParameterType.GetElementType() != typeof(NetCore.RPCRouter.RouteData))
                return null;
            return args[1].ParameterType.GetElementType();
        }
        public void RegClass(System.Type klass, string macro, ECSType csType)
        {
            if (klass.IsGenericType)
                return;
            var methods = klass.GetMethods();
            foreach (var i in methods)
            {
                var rpcAttr = Rtti.AttributeHelper.GetCustomAttribute(i, typeof(RPCCallAttribute).FullName, true);
                if (rpcAttr == null)
                    continue;

                var args = i.GetParameters();

                var proc_type = GetRPCMethodProcessor(args);
                if (proc_type == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"Method: {i.DeclaringType.FullName}.{i.Name} arguments is not valid");
                    continue;
                }

                var type = Rtti.RttiHelper.GetTypeFromTypeFullName(proc_type.FullName, csType);
                //var atts = type.GetCustomAttributes(typeof(RPCProcessorAttribute), false);
                //if (atts == null || atts.Length == 0)
                //    continue;
                //var senderDesc = atts[0] as RPCProcessorAttribute;
                //var sender = RPCProcessor.InitProcessor(senderDesc.ProcessorType, type, senderDesc.ReturnType);
                var sender = RPCProcessor.GetProcessorByArgument(type);
                RegRPC(sender, i, type, macro);
            }
        }
#endregion
    }
}

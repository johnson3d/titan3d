using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCodeTools
{
    class URpcMethod
    {
        public string Name;
        public string Index;
        public List<KeyValuePair<string, string>> ArgTypes = new List<KeyValuePair<string, string>>();
        public string ReturnType;
        public string Flags;
        public bool IsBroadCaster = false;
        public bool IsAsync;
        public enum EDataType
        {
            Void,
            Unmanaged,
            String,
            ISerializer,
            MemWriter,
        }
        public EDataType RetType;
        public EDataType ArgDataType;
        public string GetNakedReturnType()
        {
            if (IsAsync == false)
                return ReturnType;

            int start, end;
            if (UClassCodeBase.MatchPair(ReturnType, '<', '>', out start, out end))
            {
                return ReturnType.Substring(start + 1, end - start - 1);
            }
            return ReturnType;
        }
    }
    class URpcClassDefine : UClassCodeBase
    {
        public string RunTarget;
        public string Executer;
        public bool CallerInClass = false;
        public List<URpcMethod> Methods = new List<URpcMethod>();
        
        // CallerInClass=false 时调用方由共享调用方宿主单独编译，被调方仍归拥有源码的工程。
        public void GenCallerCode()
        {
            if (Methods.Count == 0 || CallerInClass)
                return;
            SaveRpcCaller();
        }
        public void GenCode()
        {
            if (Methods.Count == 0)
                return;

            if (CallerInClass)
                SaveRpcCaller();
            SaveRpcCallee();
        }
        public void SaveRpcCallee()
        {
            NewLine();

            BeginNamespaceAndOuter();
            {
                AddLine(PartialDeclaration());
                PushBrackets();
                {
                    foreach (var i in Methods)
                    {
                        BuildCallee(i);
                        BuildCaller(i);
                    }
                }
                PopBrackets();
            }
            EndNamespaceAndOuter();
        }
        private void BuildCallee(URpcMethod i)
        {
            if (i.IsAsync)
            {
                AddLine($"public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_{i.Name} = async (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.TtCallContext context) =>");
            }
            else
            {
                AddLine($"public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_{i.Name} = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>");
            }
            PushBrackets();
            {
                string argCallStr = "";
                {
                    foreach (var j in i.ArgTypes)
                    {
                        AddLine($"{j.Key} {j.Value};");
                        AddLine($"reader.Read(out {j.Value});");

                        if (argCallStr != "")
                            argCallStr += ", ";
                        argCallStr += j.Value;
                    }
                    if (argCallStr != "")
                        argCallStr += ", ";
                }
                if (i.ReturnType != null)
                {
                    AddLine($"EngineNS.Bricks.Network.RPC.FReturnContext retContext;");
                    AddLine($"reader.Read(out retContext, false);");

                    if (i.IsAsync)
                    {
                        AddLine($"var ret = await (({this.FullName})host).{i.Name}({argCallStr}context);");
                    }
                    else
                    {
                        AddLine($"var ret = (({this.FullName})host).{i.Name}({argCallStr}context);");
                    }

                    AddLine($"using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())");
                    PushBrackets();
                    {
                        AddLine($"var pkg = new IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);");
                        AddLine($"var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();");
                        AddLine($"pkgHeader.SetHasReturn(true);");
                        AddLine($"pkg.Write(pkgHeader);");
                        AddLine($"pkg.Write(retContext, false);");
                        AddLine($"pkg.Write(ret);");
                        AddLine($"pkg.CoreWriter.SurePkgHeader();");
                        AddLine($"context.NetConnect?.Send(in pkg);");
                    }
                    PopBrackets();
                }
                else
                {
                    AddLine($"(({this.FullName})host).{i.Name}({argCallStr}context);");
                }

                foreach (var j in i.ArgTypes)
                {
                    if (j.Key == "EngineNS.IO.TtMemWriter" || j.Key == "IO.TtMemWriter" || j.Key == "TtMemWriter")
                    {
                        AddLine($"{j.Value}.Dispose();");
                    }
                }
                if (i.ReturnType == "EngineNS.IO.TtMemWriter" || i.ReturnType == "IO.TtMemWriter" || i.ReturnType == "TtMemWriter")
                {
                    AddLine($"ret.Dispose();");
                }
                BuildBroadCaster(i);
            }
            PopBrackets(true);
        }
        private void BuildBroadCaster(URpcMethod i)
        {
            if (i.RetType != URpcMethod.EDataType.Void)
                return;
            if (i.IsBroadCaster)
            {
                AddLine($"if (context.NoBroadCast)");
                PushBrackets();
                {
                    AddLine($"return;");
                }
                PopBrackets();
                AddLine($"var broadCaster = (host as IRpcHost)?.GetRpcBroadCaster({i.Index});");
                AddLine($"if (broadCaster != null)");
                PushBrackets();
                {
                    AddLine($"var t_iter = broadCaster.GetEnumerator(host as IRpcHost, typeof({this.FullName}));");
                    AddLine($"if (t_iter!=null)");
                    PushBrackets();
                    {
                        AddLine($"while (t_iter.MoveNext())");
                        PushBrackets();
                        {
                            AddLine($"var t_sendTarget = t_iter.Current as {this.FullName};");
                            AddLine($"if (t_sendTarget!=null)");
                            PushBrackets();
                            {
                                string argCallStr = "";
                                {
                                    foreach (var j in i.ArgTypes)
                                    {
                                        if (argCallStr != "")
                                            argCallStr += ", ";
                                        argCallStr += j.Value;
                                    }
                                    if (argCallStr != "")
                                        argCallStr += ", ";
                                }
                                AddLine($"t_sendTarget.RPC_{i.Name}({argCallStr}null);");
                            }
                            PopBrackets();
                        }
                        PopBrackets();
                    }
                    PopBrackets();
                }
                PopBrackets();
            }
        }
        private void BuildCaller(URpcMethod i)
        {
            string argDeclStr = "";
            string argCallStr = "";
            foreach (var j in i.ArgTypes)
            {
                if (argDeclStr != "")
                    argDeclStr += ", ";
                argDeclStr += j.Key + " " + j.Value;

                if (argCallStr != "")
                    argCallStr += ", ";
                argCallStr += j.Value;
            }
            if (argDeclStr != "")
                argDeclStr += ", ";
            if (argCallStr != "")
                argCallStr += ", ";
            if (i.RetType != URpcMethod.EDataType.Void)
                AddLine($"public async Thread.Async.TtTask<{i.GetNakedReturnType()}> RPC_{i.Name}({argDeclStr}EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)");
            else
                AddLine($"public void RPC_{i.Name}({argDeclStr}EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)");
            PushBrackets();
            {
                AddLine($"var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);");
                AddLine($"rpcArg.ExeIndex = RpcExecuteIndex;");
                AddLine($"rpcArg.NetConnect = GetRpcConnect({i.Index});");

                if (i.RetType != URpcMethod.EDataType.Void)
                {
                    AddLine($"return await {this.CallerName}.{i.Name}({argCallStr}rpcArg);");
                }
                else
                {
                    AddLine($"{this.CallerName}.{i.Name}({argCallStr}rpcArg);");
                }
            }
            PopBrackets();
        }
        public void SaveRpcCaller()
        {
            AddLine("#pragma warning disable 105");
            
            {
                //foreach (var i in Usings)
                //{
                //    AddLine(i);
                //}
                AddLine("using System;");
                AddLine("using System.Collections.Generic;");
                AddLine("using EngineNS.Bricks.Network.RPC;");
                AddLine("using EngineNS;");
            }
            
            NewLine();

            BeginNamespaceAndOuter();
            {
                AddLine(PartialDeclaration(caller: true));
                PushBrackets();
                {
                    foreach (var i in Methods)
                    {
                        string argDeclStr = "";
                        foreach (var j in i.ArgTypes)
                        {
                            if (argDeclStr != "")
                                argDeclStr += ", ";
                            argDeclStr += j.Key + " " + j.Value;
                        }
                        if (argDeclStr != "")
                            argDeclStr += ", ";
                        if (i.RetType != URpcMethod.EDataType.Void)
                            AddLine($"public static async Thread.Async.TtTask<{i.GetNakedReturnType()}> {i.Name}({argDeclStr}EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)");
                        else
                            AddLine($"public static void {i.Name}({argDeclStr}in EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)");
                        PushBrackets();
                        {
                            AddLine($"var ExeIndex = rpcArg.ExeIndex;");
                            AddLine($"var NetConnect = rpcArg.NetConnect;");
                            AddLine($"if (ExeIndex == UInt16.MaxValue)");
                            PushBrackets();
                            {
                                AddLine($"ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;");
                            }
                            PopBrackets();
                            AddLine($"if (NetConnect == null)");
                            PushBrackets();
                            {
                                AddLine($"NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;");
                            }
                            PopBrackets();

                            if (i.RetType != URpcMethod.EDataType.Void)
                            {
                                AddLine($"var retContext = TtReturnAwaiter<{i.GetNakedReturnType()}>.CreateInstance(rpcArg.Timeout, rpcArg.ReturnContext);");
                                AddLine($"if (NetConnect != null)");
                                PushBrackets();
                                {
                                    AddLine($"retContext.Context.Index = ExeIndex;");
                                }
                                PopBrackets();
                            }
                            AddLine($"using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())");
                            PushBrackets();
                            {
                                AddLine($"var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);");
                                AddLine($"var router = new EngineNS.Bricks.Network.RPC.FRouter();");
                                AddLine($"router.RunTarget = {this.RunTarget};");
                                AddLine($"router.Executer = {this.Executer};");
                                AddLine($"router.Index = ExeIndex;");
                                AddLine($"router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;");
                                AddLine($"var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();");
                                if (i.Flags != null)
                                {
                                    AddLine($"pkgHeader.PKGFlags = (byte){i.Flags};");
                                }
                                AddLine($"pkg.Write(pkgHeader);");
                                AddLine($"pkg.Write(router, false);");
                                AddLine($"UInt16 methodIndex = {i.Index};");
                                AddLine($"pkg.Write(methodIndex);");
                                foreach (var j in i.ArgTypes)
                                {
                                    AddLine($"pkg.Write({j.Value});");
                                }

                                if (i.RetType != URpcMethod.EDataType.Void)
                                {
                                    AddLine($"pkg.Write(retContext.Context, false);");
                                }

                                AddLine($"pkg.CoreWriter.SurePkgHeader();");
                                AddLine($"NetConnect?.Send(in pkg);");
                            }
                            PopBrackets();
                            if (i.ReturnType != null)
                            {
                                switch (i.RetType)
                                {
                                    case URpcMethod.EDataType.Unmanaged:
                                        AddLine($"return await TtRpcAwaiter.AwaitReturn<{i.GetNakedReturnType()}>(retContext);");
                                        break;
                                    case URpcMethod.EDataType.ISerializer:
                                        AddLine($"return await TtRpcAwaiter.AwaitReturn_ISerializer<{i.GetNakedReturnType()}>(retContext);");
                                        break;
                                    case URpcMethod.EDataType.String:
                                        AddLine($"return await TtRpcAwaiter.AwaitReturn_String(retContext);");
                                        break;
                                    case URpcMethod.EDataType.MemWriter:
                                        AddLine($"return await TtRpcAwaiter.AwaitReturn_MemWriter(retContext);");
                                        break;
                                }
                            }
                        }
                        PopBrackets();
                    }
                }
                PopBrackets();
            }
            EndNamespaceAndOuter();

        }
    }
}

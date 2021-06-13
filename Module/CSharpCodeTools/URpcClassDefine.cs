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
        public string ArgType;
        public string ArgName;
        public string ReturnType;
        public bool IsAsync;
        public enum EDataType
        {
            Void,
            Unmanaged,
            String,
            ISerializer,
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
        public List<URpcMethod> Methods = new List<URpcMethod>();
        
        public override void  GenCode(string dir)
        {
            if (Methods.Count == 0)
                return;

            AddLine("#pragma warning disable 105");
            foreach (var i in Usings)
            {
                AddLine(i);
            }
            AddLine("using EngineNS.Bricks.Network.RPC;");
            NewLine();

            AddLine($"namespace {this.Namespace}");
            PushBrackets();
            {
                AddLine($"partial class {this.Name}");
                PushBrackets();
                {
                    foreach(var i in Methods)
                    {
                        if (i.RetType != URpcMethod.EDataType.Void)
                            AddLine($"public static async System.Threading.Tasks.Task<{i.GetNakedReturnType()}> {i.Name}({i.ArgType} {i.ArgName}, UInt16 ExeIndex = 0, EngineNS.Bricks.Network.INetConnect NetConnect = null)");
                        else
                            AddLine($"public static void {i.Name}({i.ArgType} {i.ArgName}, UInt16 ExeIndex = 0, EngineNS.Bricks.Network.INetConnect NetConnect = null)");
                        PushBrackets();
                        {
                            AddLine($"if (NetConnect == null)");
                            PushBrackets();
                            {
                                AddLine($"NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;");
                            }
                            PopBrackets();

                            AddLine($"using (var writer = UMemWriter.CreateInstance())");
                            PushBrackets();
                            {
                                AddLine($"var pkg = new IO.AuxWriter<UMemWriter>(writer);");
                                AddLine($"URouter router = new URouter();");
                                AddLine($"router.RunTarget = {this.RunTarget};");
                                AddLine($"router.Executer = {this.Executer};");
                                AddLine($"router.Index = ExeIndex;");
                                AddLine($"EPkgTypes pkgTypes = 0;");
                                AddLine($"pkg.Write(pkgTypes);");
                                AddLine($"pkg.Write(router);");
                                AddLine($"UInt16 methodIndex = {i.Index};");
                                AddLine($"pkg.Write(methodIndex);");
                                AddLine($"pkg.Write({i.ArgName});");

                                if (i.RetType != URpcMethod.EDataType.Void)
                                {
                                    AddLine($"var retContext = UReturnAwaiter.CreateInstance();");
                                    AddLine($"if (NetConnect != null)");
                                    PushBrackets();
                                    {
                                        AddLine($"retContext.Context.ConnectId = NetConnect.GetConnectId();");
                                    }
                                    PopBrackets();
                                    AddLine($"pkg.Write(retContext.Context);");
                                }

                                AddLine($"NetConnect?.Send(ref pkg);");

                                if (i.ReturnType != null)
                                {
                                    switch(i.RetType)
                                    {
                                        case URpcMethod.EDataType.Unmanaged:
                                            AddLine($"return await URpcAwaiter.AwaitReturn<{i.GetNakedReturnType()}>(retContext);");
                                            break;
                                        case URpcMethod.EDataType.ISerializer:
                                            AddLine($"return await URpcAwaiter.AwaitReturn_ISerializer<{i.GetNakedReturnType()}>(retContext);");
                                            break;
                                        case URpcMethod.EDataType.String:
                                            AddLine($"return await URpcAwaiter.AwaitReturn_String(retContext);");
                                            break;
                                    }
                                }
                            }
                            PopBrackets();
                        }
                        PopBrackets();

                        if (i.IsAsync)
                        {
                            AddLine($"public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_{i.Name} = async (IO.AuxReader<UMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.UCallContext context) =>");
                        }
                        else
                        {
                            AddLine($"public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_{i.Name} = (IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>");
                        }
                        PushBrackets();
                        {
                            if (i.ArgDataType == URpcMethod.EDataType.ISerializer)
                            {
                                AddLine($"EngineNS.IO.ISerializer _tmp;");
                                AddLine($"reader.Read(out _tmp);");
                                AddLine($"var arg = _tmp as {i.ArgType};");
                                AddLine($"if (arg == null)");
                                PushBrackets();
                                {
                                    AddLine($"return;");
                                }
                                PopBrackets();
                            }
                            else
                            {
                                AddLine($"{i.ArgType} arg;");
                                AddLine($"reader.Read(out arg);");
                            }
                            if (i.ReturnType != null)
                            {
                                AddLine($"UReturnContext retContext;");
                                AddLine($"reader.Read(out retContext);");

                                if (i.IsAsync)
                                {
                                    AddLine($"var ret = await (({this.FullName})host).{i.Name}(arg, context);");
                                }
                                else
                                {
                                    AddLine($"var ret = (({this.FullName})host).{i.Name}(arg, context);");
                                }

                                AddLine($"using (var writer = UMemWriter.CreateInstance())");
                                PushBrackets();
                                {
                                    AddLine($"var pkg = new IO.AuxWriter<UMemWriter>(writer);");
                                    AddLine($"EPkgTypes pkgTypes = EPkgTypes.IsReturn;");
                                    AddLine($"pkg.Write(pkgTypes);");
                                    AddLine($"pkg.Write(retContext);");
                                    AddLine($"pkg.Write(ret);");
                                    AddLine($"context.NetConnect?.Send(ref pkg);");
                                }
                                PopBrackets();
                            }
                            else
                            {
                                AddLine($"(({this.FullName})host).{i.Name}(arg, context);");
                            }
                        }
                        PopBrackets(true);
                    }
                }
                PopBrackets();
            }
            PopBrackets();

            var file = dir + "/" + FullName + ".rpc.cs";
            if (!URpcCodeManager.Instance.WritedFiles.Contains(file.Replace("\\", "/").ToLower()))
            {
                URpcCodeManager.Instance.WritedFiles.Add(file.Replace("\\", "/").ToLower());
            }
            
            if (System.IO.File.Exists(file))
            {
                var oldCode = System.IO.File.ReadAllText(file);
                if (oldCode == ClassCode)
                    return;
            }

            System.IO.File.WriteAllText(file, ClassCode);
        }
    }
}

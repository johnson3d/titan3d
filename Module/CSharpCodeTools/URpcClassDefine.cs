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
        public bool CallerInClass = false;
        public List<URpcMethod> Methods = new List<URpcMethod>();
        
        public void GenCode(string dir, string source)
        {
            if (Methods.Count == 0)
                return;

            source = source.Replace('\\', '/');
            var idx = source.LastIndexOf('/');
            if (idx >= 0)
            {
                source = source.Substring(idx + 1);
            }

            if (CallerInClass == false)
            {
                SaveRpcCaller(dir, APHash(source).ToString());
                this.ResetWriter();
            }
            else
            {
                SaveRpcCaller(null, APHash(source).ToString());
            }
            SaveRpcCallee();
        }
        public void SaveRpcCallee()
        {
            NewLine();

            AddLine($"namespace {this.Namespace}");
            PushBrackets();
            {
                AddLine($"partial class {this.Name}");
                PushBrackets();
                {
                    foreach (var i in Methods)
                    {
                        if (i.IsAsync)
                        {
                            AddLine($"public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_{i.Name} = async (EngineNS.IO.AuxReader<UMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.UCallContext context) =>");
                        }
                        else
                        {
                            AddLine($"public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_{i.Name} = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>");
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
                                AddLine($"UReturnContext retContext;");
                                AddLine($"reader.Read(out retContext);");

                                if (i.IsAsync)
                                {
                                    AddLine($"var ret = await (({this.FullName})host).{i.Name}({argCallStr}context);");
                                }
                                else
                                {
                                    AddLine($"var ret = (({this.FullName})host).{i.Name}({argCallStr}context);");
                                }

                                AddLine($"using (var writer = UMemWriter.CreateInstance())");
                                PushBrackets();
                                {
                                    AddLine($"var pkg = new IO.AuxWriter<UMemWriter>(writer);");
                                    AddLine($"var pkgHeader = new FPkgHeader();");
                                    AddLine($"pkgHeader.SetHasReturn(true);");
                                    AddLine($"pkg.Write(pkgHeader);");
                                    AddLine($"pkg.Write(retContext);");
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
                        }
                        PopBrackets(true);
                    }
                }
                PopBrackets();
            }
            PopBrackets();
        }
        public void SaveRpcCaller(string dir, string source)
        {
            AddLine("#pragma warning disable 105");
            
            if (dir != null)
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

            AddLine($"namespace {this.Namespace}");
            PushBrackets();
            {
                AddLine($"public partial class {this.Name}_RpcCaller");
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
                            AddLine($"public static async System.Threading.Tasks.Task<{i.GetNakedReturnType()}> {i.Name}({argDeclStr}uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)");
                        else
                            AddLine($"public static void {i.Name}({argDeclStr}UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)");
                        PushBrackets();
                        {
                            AddLine($"if (ExeIndex == UInt16.MaxValue)");
                            PushBrackets();
                            {
                                AddLine($"ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;");
                            }
                            PopBrackets();
                            AddLine($"if (NetConnect == null)");
                            PushBrackets();
                            {
                                AddLine($"NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;");
                            }
                            PopBrackets();

                            if (i.RetType != URpcMethod.EDataType.Void)
                            {
                                AddLine($"var retContext = UReturnAwaiter.CreateInstance(Timeout);");
                                AddLine($"if (NetConnect != null)");
                                PushBrackets();
                                {
                                    AddLine($"retContext.Context.Index = ExeIndex;");
                                }
                                PopBrackets();
                            }
                            AddLine($"using (var writer = UMemWriter.CreateInstance())");
                            PushBrackets();
                            {
                                AddLine($"var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);");
                                AddLine($"URouter router = new URouter();");
                                AddLine($"router.RunTarget = {this.RunTarget};");
                                AddLine($"router.Executer = {this.Executer};");
                                AddLine($"router.Index = ExeIndex;");
                                AddLine($"router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;");
                                AddLine($"var pkgHeader = new FPkgHeader();");
                                if (i.Flags != null)
                                {
                                    AddLine($"pkgHeader.PKGFlags = (byte){i.Flags};");
                                }
                                AddLine($"pkg.Write(pkgHeader);");
                                AddLine($"pkg.Write(router);");
                                AddLine($"UInt16 methodIndex = {i.Index};");
                                AddLine($"pkg.Write(methodIndex);");
                                foreach (var j in i.ArgTypes)
                                {
                                    AddLine($"pkg.Write({j.Value});");
                                }

                                if (i.RetType != URpcMethod.EDataType.Void)
                                {
                                    AddLine($"pkg.Write(retContext.Context);");
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
                }
                PopBrackets();
            }
            PopBrackets();

            if (dir == null)
                return;
            var file = dir + "/" + FullName + $"_{source}.rpc.cs";
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

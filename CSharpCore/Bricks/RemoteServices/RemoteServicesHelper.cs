using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.RemoteServices.Net;

namespace EngineNS.Bricks.RemoteServices
{
    public partial class RemoteServicesHelper
    {
        partial void Cleanup_Server();
        partial void Tick_Server();
        public void Cleanup()
        {
            Cleanup_Server();
            StopClient();
        }
        public async System.Threading.Tasks.Task InitServices()
        {
            await Thread.AsyncDummyClass.DummyFunc();

            RPCExecuter.Instance.BuildAssemblyRPC(null, null, ECSType.Server);
            FactoryObjectManager.Instance.BuildFactory(null);
            DefaultRoute.HostObject = this;

            //await InitServer();
            //await InitServer("127.0.0.1", 2020);
        }
        public async System.Threading.Tasks.Task InitClient(string ip, UInt16 port)
        {
            if (mClient!=null)
                return;

            mClient = new TcpClient.CTcpClient();
            mClient.Router = new NetCore.RPCRouter();
            mClient.Router.ToGConnect = mClient;
            RPCExecuter.Instance.C2SConnect = mClient;
            mClient.OnDisconnected = (con) =>
            {
                if(mClient!=null)
                    mClient.Router = null;
            };
            mClient.OnReceiveData = (pClient, pData, nLength, recvTime) =>
            {
                RPCExecuter.Instance.ReceiveData(pClient, pData, nLength, recvTime);
            };
            if (false == await mClient.Connect(ip, port, 3000))
            {
                mClient = null;
            }

            RPCProcessor.DefaultConnection = mClient;
            mProfiler.ReporterName = CEngine.Instance.Desc.ProfilerReporterName;
        }
        public void StopClient()
        {
            if (mClient == null)
                return;
            mClient.Disconnect();
            mClient = null;
            RPCProcessor.DefaultConnection = null;
        }
        C2S_ReportProfiler.ArgumentData mProfiler = new C2S_ReportProfiler.ArgumentData();
        long PrevReportTime = Support.Time.GetTickCount();
        bool DoPerfReport = true;
        public void Tick()
        {
            Tick_Server();
            if (mClient!=null)
            {
                mClient.Update();
                if (DoPerfReport == false)
                    return;
                //var nu = CallRPC();
                long nowTime = Support.Time.GetTickCount();
                if (nowTime - PrevReportTime < 1000)
                    return;
                PrevReportTime = nowTime;

                var pver = CEngine.Instance.Stat.PViewer;
                if (pver.IsReporting)
                {
                    if (mProfiler.Scopes == null || mProfiler.Scopes.Count != pver.Scopes.Count)
                    {
                        mProfiler.Scopes = new List<Profiler.PerfViewer.PfValue_PerfCounter>(pver.Scopes.Count);
                        for (int i = 0; i < pver.Scopes.Count; i++)
                        {
                            var e = new Profiler.PerfViewer.PfValue_PerfCounter();
                            e.Name = pver.Scopes[i];
                            mProfiler.Scopes.Add(e);
                        }
                    }
                    if (mProfiler.Datas ==null || mProfiler.Datas.Count != pver.Datas.Count)
                    {
                        mProfiler.Datas = new List<Profiler.PerfViewer.PfValue_Data>(pver.Datas.Count);
                        for (int i = 0; i < pver.Datas.Count; i++)
                        {
                            mProfiler.Datas.Add(new Profiler.PerfViewer.PfValue_Data());
                        }
                    }
                    for (int i = 0; i < pver.Scopes.Count; i++)
                    {
                        var scope = Profiler.TimeScopeManager.Instance.FindTimeScope(pver.Scopes[i]);
                        if (scope == null)
                            continue;
                        mProfiler.Scopes[i].AvgHit = scope.AvgHit;
                        mProfiler.Scopes[i].AvgTime = (int)scope.AvgTime;
                    }
                    for (int i = 0; i < pver.Datas.Count; i++)
                    {
                        var scope = pver.Datas[i];
                        if (scope == null)
                            continue;

                        mProfiler.Datas[i].Name = scope.Name;
                        //mProfiler.Datas[i].ValueNames = new List<string>(pver.Datas[i].GetValueNameAction());
                        mProfiler.Datas[i].ValueDatas = new List<string>(pver.Datas[i].GetValueAction());
                    }
                    //mProfiler.C2S_Call<NullReturn>(1, ERouteTarget.Self);

                    C2S_ReportProfiler.Instance.DoHashCall(
                        ref mProfiler,
                        NetCore.ERouteTarget.Self);
                }
            }
        }
        TcpClient.CTcpClient mClient = null;
        public TcpClient.CTcpClient Client
        {
            get { return mClient; }
        }

        public string ProfilerTarget = "TitanEngine";
        public NetCore.NetConnection ProfilerConnect;

#region ProfilerReporter
        public delegate void FOnReciveReportPerfCounter(string reporterId, List<Profiler.PerfViewer.PfValue_PerfCounter> scopes, List<Profiler.PerfViewer.PfValue_Data> datas);
        public FOnReciveReportPerfCounter OnReciveReportPerfCounter;
        public class C2S_ReportProfiler : AuxProcessorEx<C2S_ReportProfiler.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 1024;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public string ReporterName
                {
                    get;
                    set;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public List<Profiler.PerfViewer.PfValue_PerfCounter> Scopes
                {
                    get;
                    set;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public List<Profiler.PerfViewer.PfValue_Data> Datas
                {
                    get;
                    set;
                }
            }
            protected override void OnWriteArugment(ref ArgumentData obj, ref NetCore.PkgWriter pkg)
            {
                //pkg.Write(obj.ReporterName);

                //pkg.Write(obj.Scopes.Count);
                //for (int i=0; i< obj.Scopes.Count; i++)
                //{
                //    var cur = obj.Scopes[i];
                //    pkg.Write(cur.Name);
                //    pkg.Write(cur.NameHash);
                //    pkg.Write(cur.AvgTime);
                //    pkg.Write(cur.AvgHit);
                //}
                base.OnWriteArugment(ref obj, ref pkg);
            }
            protected override void OnReadArugment(ref ArgumentData obj, ref NetCore.PkgReader pkg)
            {
                //string tmp;
                //pkg.Read(out tmp);
                //obj.ReporterName = tmp;

                //int count = 0;
                //pkg.Read(out count);
                //obj.Scopes = new List<Profiler.PerfViewer.PfValue_PerfCounter>();
                //for (int i = 0; i < count; i++)
                //{
                //    var cur = new Profiler.PerfViewer.PfValue_PerfCounter();
                //    pkg.Read(out tmp);
                //    cur.Name = tmp;
                //    UInt32 hash = 0;
                //    pkg.Read(out hash);
                //    cur.NameHash = hash;
                //    int tmTime = 0;
                //    pkg.Read(out tmTime);
                //    cur.AvgTime = tmTime;
                //    pkg.Read(out tmTime);
                //    cur.AvgHit = tmTime;

                //    obj.Scopes.Add(cur);
                //}
                base.OnReadArugment(ref obj, ref pkg);
            }
            public override object GetHostObject(ref NetCore.RPCRouter.RouteData routeInfo)
            {
                return CEngine.Instance.RemoteServices;
                //return base.GetHostObject(ref routeInfo);
            }
            protected override object OnCallMethod(object host, byte userFlags, ref ArgumentData arg, ushort serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
            {
                ((RemoteServicesHelper)host).OnC2S_ReportProfiler(userFlags, ref arg, serialId, connect, ref routeInfo);
                return null;
                //return base.OnCallMethod(host, ref arg, serialId, connect, ref routeInfo);
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnC2S_ReportProfiler(byte userFlags, ref C2S_ReportProfiler.ArgumentData profiler, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            if (OnReciveReportPerfCounter != null)
            {
                if (ProfilerTarget == profiler.ReporterName)
                {
                    OnReciveReportPerfCounter(profiler.ReporterName, profiler.Scopes, profiler.Datas);
                    ProfilerConnect = connect;
                }
            }
        }
#endregion

#region Turn Instance 
        public class S2C_InstanceRender : AuxUnmanagedProcessorEx<S2C_InstanceRender.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    unsafe
                    {
                        return sizeof(ArgumentData);
                    }
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Instancing
                {
                    get;
                    set;
                }
            }
            protected override object OnCallMethod(object host, byte userFlags, ref ArgumentData arg, ushort serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
            {
                ((RemoteServicesHelper)host).OnS2C_InstanceRender(userFlags, ref arg, serialId, connect, ref routeInfo);
                return null;
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_InstanceRender(byte userFlags, ref S2C_InstanceRender.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.UseInstancing = cmd.Instancing;
        }
        public void AllClientsInstancing(bool useInstancing)
        {
            var pkg = new S2C_InstanceRender.ArgumentData();
            pkg.Instancing = useInstancing;
            if(ProfilerConnect!=null)
                S2C_InstanceRender.Instance.DoCall(ref pkg, NetCore.ERouteTarget.Self, ProfilerConnect, null);
        }
#endregion

#region Turn Shadow
        public class S2C_Shadow : AuxUnmanagedProcessorEx<S2C_Shadow.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Shadow
                {
                    get;
                    set;
                }
            }

            protected override object OnCallMethod(object host, byte userFlags, ref ArgumentData arg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
            {
                var rsh = (RemoteServicesHelper)host;
                rsh.OnS2C_Shadow(userFlags, ref arg, serialId, connect, ref routeInfo);
                return null;
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_Shadow(byte userFlags, ref S2C_Shadow.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.EnableShadow = cmd.Shadow;
        }
        public void AllClientsShadow(bool shadow)
        {
            var pkg = new S2C_Shadow.ArgumentData();
            pkg.Shadow = shadow;
            if (ProfilerConnect != null)
            {
                S2C_Shadow.Instance.DoCall(ref pkg, NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }

            {
                
            }

            Action act = async () =>
            {
                var pkg1 = new S2C_TestAwait.ArgumentData();
                pkg1.Enable = true;
                S2C_TestAwait.ReturnData ret = await S2C_TestAwait.Instance.DoAwaitHashCall(pkg1, -1, NetCore.ERouteTarget.Self, ProfilerConnect, null);
                if (ret.A1 == 0)
                {
                    return;
                }
            };
            act();
        }
#endregion

#region Turn Postprocess Bloom
        public class S2C_Postprocess_Bloom : AuxUnmanagedProcessorEx<S2C_Postprocess_Bloom.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_Postprocess_Bloom(byte userFlags, ref S2C_Postprocess_Bloom.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.EnableBloom = cmd.Enable;
        }
        public void AllClientsBloom(bool enable)
        {
            var pkg = new S2C_Postprocess_Bloom.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_Postprocess_Bloom.Instance.DoCall(
                    ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn PVS
        public class S2C_PVS : AuxUnmanagedProcessorEx<S2C_PVS.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_PVS(byte userFlags, ref S2C_PVS.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.UsePVS = cmd.Enable;
        }
        public void AllClientsPVS(bool enable)
        {
            var pkg = new S2C_PVS.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_PVS.Instance.DoCall(ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn FrozenCulling
        public class S2C_FrozenCulling : AuxUnmanagedProcessorEx<S2C_FrozenCulling.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_FrozenCulling(byte userFlags, ref S2C_FrozenCulling.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            if(CEngine.Instance.GameInstance!=null)
            {
                CEngine.Instance.GameInstance.GameCamera.LockCulling = cmd.Enable;
            }
        }
        public void AllClientsFrozenCulling(bool enable)
        {
            var pkg = new S2C_FrozenCulling.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_FrozenCulling.Instance.DoCall(ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn MTForeach
        public class S2C_MTForeach : AuxUnmanagedProcessorEx<S2C_MTForeach.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_MTForeach(byte userFlags, ref S2C_MTForeach.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.Instance.EventPoster.EnableMTForeach = cmd.Enable;
        }
        public void AllClientsMTForeach(bool enable)
        {
            var pkg = new S2C_MTForeach.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_MTForeach.Instance.DoCall(ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn ShowPhysXDebugMesh
        public class S2C_ShowPhysXDebugMesh : AuxUnmanagedProcessorEx<S2C_ShowPhysXDebugMesh.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_ShowPhysXDebugMesh(byte userFlags, ref S2C_ShowPhysXDebugMesh.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.PhysicsDebug = cmd.Enable;
        }
        public void AllClientsShowPhysXDebugMesh(bool enable)
        {
            var pkg = new S2C_ShowPhysXDebugMesh.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_ShowPhysXDebugMesh.Instance.DoCall(
                    ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn ShowNavMesh
        public class S2C_ShowNavMesh : AuxUnmanagedProcessorEx<S2C_ShowNavMesh.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_ShowNavMesh(byte userFlags, ref S2C_ShowNavMesh.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.PhysicsDebug = cmd.Enable;
        }
        public void AllClientsShowNavMesh(bool enable)
        {
            var pkg = new S2C_ShowNavMesh.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_ShowNavMesh.Instance.DoCall(
                    ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn NoPixelShader
        public class S2C_NoPixelShader : AuxUnmanagedProcessorEx<S2C_NoPixelShader.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_NoPixelShader(byte userFlags, ref S2C_NoPixelShader.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            bool enable = cmd.Enable;
            CEngine.Instance.EventPoster.RunOn(() =>
            {
                if (CEngine.Instance.GameInstance != null)
                {
                    CEngine.Instance.GameInstance.NoPixelShader = enable;
                    var policy = CEngine.Instance.GameInstance.RenderPolicy as Graphics.RenderPolicy.CGfxRP_GameMobile;
                    if (policy != null)
                    {
                        var profiler = CEngine.Instance.GameInstance.GetGraphicProfiler();
                        policy.SetGraphicsProfiler(profiler);
                        Profiler.Log.WriteLine(Profiler.ELogTag.Info, "GraphicsDebugger", $"No Pixel Shader:{enable}");
                    }
                }
                return null;
            }, Thread.Async.EAsyncTarget.Main);
        }
        public void AllClientsNoPixelShader(bool enable)
        {
            var pkg = new S2C_NoPixelShader.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_NoPixelShader.Instance.DoCall(
                    ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn NoPixelWrite
        public class S2C_NoPixelWrite : AuxUnmanagedProcessorEx<S2C_NoPixelWrite.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_NoPixelWrite(byte userFlags, ref S2C_NoPixelWrite.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            var enable = cmd.Enable;
            CEngine.Instance.EventPoster.RunOn(() =>
            {
                if (CEngine.Instance.GameInstance != null)
                {
                    CEngine.Instance.GameInstance.NoPixelWrite = enable;
                    var policy = CEngine.Instance.GameInstance.RenderPolicy as Graphics.RenderPolicy.CGfxRP_GameMobile;
                    if (policy != null)
                    {
                        var profiler = CEngine.Instance.GameInstance.GetGraphicProfiler();
                        policy.SetGraphicsProfiler(profiler);
                        Profiler.Log.WriteLine(Profiler.ELogTag.Info, "GraphicsDebugger", $"No Pixel Write:{enable}");
                    }
                }
                return null;
            }, Thread.Async.EAsyncTarget.Main);
        }
        public void AllClientsNoPixelWrite(bool enable)
        {
            var pkg = new S2C_NoPixelWrite.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_NoPixelWrite.Instance.DoCall(
                    ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
#endregion

#region Turn IgnoreGLCall
        public class S2C_IgnoreGLCall : AuxUnmanagedProcessorEx<S2C_IgnoreGLCall.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_IgnoreGLCall(byte userFlags, ref S2C_IgnoreGLCall.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            CEngine.Instance.IgnoreGLCall = cmd.Enable;
        }
        public void AllClientsIgnoreGLCall(bool enable)
        {
            var pkg = new S2C_IgnoreGLCall.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_IgnoreGLCall.Instance.DoCall(
                    ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
        #endregion

        #region Turn FullGC
        public class S2C_FullGC : AuxUnmanagedProcessorEx<S2C_FullGC.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_S2C_FullGC(byte userFlags, ref S2C_FullGC.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            System.GC.Collect();
        }
        public void AllClientsFullGC(bool enable)
        {
            System.GC.Collect();
            var pkg = new S2C_FullGC.ArgumentData();
            pkg.Enable = enable;
            if (ProfilerConnect != null)
            {
                S2C_FullGC.Instance.DoCall(
                    ref pkg,
                    NetCore.ERouteTarget.Self, ProfilerConnect, null);
            }
        }
        #endregion

        #region Test RPCAwait
        public class S2C_Test : AuxUnmanagedProcessorEx<S2C_Test.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 8;
                }
                public int A
                {
                    get;
                    set;
                }
                public float B
                {
                    get;
                    set;
                }
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnC2S_Test(byte userFlags, ref S2C_Test.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {

        }
        public class S2C_TestAwait : AuxUnmangedReturnProcessorEx<S2C_TestAwait.ReturnData, S2C_TestAwait.ArgumentData>
        {
            public struct ArgumentData : IArgument
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public bool Enable
                {
                    get;
                    set;
                }
            }
            public struct ReturnData : IReturnValue
            {
                public int GetPkgSize()
                {
                    return 4;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public RPCError ResultState
                {
                    get;
                    set;
                }
                [Rtti.MetaData(IO.Serializer.EIOType.Network)]
                public int A1
                {
                    get;
                    set;
                }
            }
            //protected override bool OnWriteArgument(IO.Serializer.IWriter pkg, ref S2C_TestAwait.ArgumentData data)
            //{
            //    pkg.Write(data.Enable);
            //    return true;
            //}
            //protected override bool OnReadArgument(IO.Serializer.IReader pkg, ref S2C_TestAwait.ArgumentData data)
            //{
            //    bool a1 = false;
            //    pkg.Read(out a1);
            //    data.Enable = a1;
            //    return true;
            //}

            public struct TestStructRLE : IO.IConstructorStruct
            {
                public int A;
                public float B;
                public Vector3 C;
                public Matrix D;
                public UInt64 E;
                public void ToDefault()
                {

                }
            }

            protected override void OnWriteArgument(ref NetCore.PkgWriter pkg, ref ArgumentData data)
            {
                pkg.SetUserFlags(2);
                base.OnWriteArgument(ref pkg, ref data);
            }

            protected override void OnWriteReturn(ref NetCore.PkgWriter pkg, ref S2C_TestAwait.ReturnData data)
            {
                pkg.Write(data.A1);

                var obj = new TestStructRLE();
                obj.A = 1;
                obj.D = Matrix.Translate(1, 2, 3);

                IO.StructCompress<TestStructRLE, NetCore.PkgWriter, NetCore.PkgReader>.Instance.Write(
                    ref pkg, ref obj);
            }
            protected override void OnReadReturn(ref NetCore.PkgReader pkg, ref S2C_TestAwait.ReturnData data)
            {
                int a1 = 0;
                pkg.Read(out a1);
                data.A1 = a1;

                var obj = new TestStructRLE();
                IO.StructCompress<TestStructRLE, NetCore.PkgWriter, NetCore.PkgReader>.Instance.Read(
                    ref pkg, ref obj);
            }

            protected override object OnCallMethod(object host, byte userFlags, ref ArgumentData arg, ushort serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
            {
                return base.OnCallMethod(host, userFlags, ref arg, serialId, connect, ref routeInfo);
            }
        }
        [RPCCall(LimitLevel = NetCore.RPCExecuteLimitLevel.Player)]
        public void OnS2C_TestAwait(byte userFlags, ref S2C_TestAwait.ArgumentData cmd, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            var ret = new S2C_TestAwait.ReturnData();
            ret.ResultState = RPCError.OK;
            ret.A1 = 100;
            S2C_TestAwait.Instance.DoReturn(ref ret, serialId, connect);
        }
#endregion
    }

    public class RemoteServicesHelperProcessor : CEngineAutoMemberProcessor
    {
        public override async System.Threading.Tasks.Task<object> CreateObject()
        {
            var Services = new RemoteServicesHelper();
            await Services.InitServices();

            return Services;
        }
        public override void Tick(object obj)
        {
            var Services = obj as RemoteServicesHelper;
            Services.Tick();
        }
        public override void Cleanup(object obj)
        {
            var Services = obj as RemoteServicesHelper;
            Services.Cleanup();
        }

        public override async System.Threading.Tasks.Task<bool> OnGameStart(object obj)
        {
            var Services = obj as RemoteServicesHelper;
            if (CEngine.Instance.Desc.ProfilerServerPort != 0)
            {
                CEngine.Instance.IsReportStatistic = true;
                await Services.InitClient(CEngine.Instance.Desc.ProfilerServerIp, CEngine.Instance.Desc.ProfilerServerPort);
            }

            return true;
        }
        public override void OnGameStop(object obj)
        {
            var Services = obj as RemoteServicesHelper;
            Services.StopClient();
        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(Bricks.RemoteServices.RemoteServicesHelperProcessor))]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Bricks.RemoteServices.RemoteServicesHelper RemoteServices
        {
            get;
            set;
        }
    }

    public partial class CEngineDesc
    {
        [Rtti.MetaData]
        public string ProfilerServerIp
        {
            get;
            set;
        } = "172.16.2.196";
        [Rtti.MetaData]
        public UInt16 ProfilerServerPort
        {
            get;
            set;
        } = 2020;
    }
}


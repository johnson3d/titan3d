using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using EngineNS.IO;
using EngineNS.Rtti;
using EngineNS.Bricks.Network.RPC;

namespace EngineNS.Profiler
{
    public struct TimeScopeHelper : IDisposable//Waiting for C#8 ,ref struct -> Dispose
    {
        public TimeScope mTime;
        public TimeScopeHelper(TimeScope t)
        {
            mTime = t;
            mTime?.Begin(true);
        }
        public void Dispose()
        {
            mTime?.End();
        }
    }
    
    public class TimeScope : AuxPtrType<SampResult>
    {
        public override void Dispose()
        {
            if (NeedDispose)
                base.Dispose();
        }
        [Flags]
        public enum EProfileFlag : byte
        {
            Windows = 1,
            Android = (1<<1),
            IOS = (1 << 2),

            FlagsAll = 0xFF,
        }
        private bool NeedDispose = true;
        public TimeScope(Type t, string method, TimeScope.EProfileFlag flags = TimeScope.EProfileFlag.FlagsAll, bool createWhenNotFound = true)
        {
            Flags = flags;
            ShowName = t.FullName + "." + method;
            NeedDispose = false;
        }
        public TimeScope(SampResult self, EProfileFlag flag)
        {
            mCoreObject = self;
            Flags = flag;
            mEnable = mCoreObject.mEnable;

            this.Core_AddRef();
            NeedDispose = true;
        }
        public string GetFriendName()
        {
            if (string.IsNullOrEmpty(ShowName))
                return mCoreObject.GetName();
            return ShowName;
        }
        public string ShowName
        {
            get;
            set;
        }
        EProfileFlag Flags;
        bool mEnable;
        public bool Enable
        {
            get { return mEnable; }
            set
            {
                mEnable = value;
                mCoreObject.mEnable = value;
            }
        }
        Int64 mBeginTime;
        public void Begin(bool bPushParent = true)
        {
            if (TimeScopeManager.IsFinalCleanup)
                return;
            if (mCoreObject.IsValidPointer == false)
            {
                mCoreObject = TimeScopeManager.GetTimeScope(ShowName, Flags);
                //Core_AddRef();
                NeedDispose = false;
                mEnable = mCoreObject.mEnable;
            }
            if (mEnable == false)
                return;

            mBeginTime = mCoreObject.Begin(TimeScopeManager.Instance.mCoreObject, bPushParent);
        }
        public void End()
        {
            if (TimeScopeManager.IsFinalCleanup)
                return;

            if (mEnable == false)
                return;

            mCoreObject.End(TimeScopeManager.Instance.mCoreObject, mBeginTime);
        }
    }
    public class TimeScopeManager
    {
        public static bool IsFinalCleanup { get; private set; } = false;
        #region ThreadInstance
        public static List<TimeScopeManager> AllThreadInstance { get; } = new List<TimeScopeManager>();
        public unsafe static void UpdateAllInstance()
        {
            lock(AllThreadInstance)
            {
                foreach (var i in AllThreadInstance)
                {
                    var num = i.mCoreObject.GetSampNum();
                    if (num != i.Scopes.Count)
                    {
                        EngineNS.SampResult** pOuts = (EngineNS.SampResult**)CoreSDK.Alloc((uint)sizeof(EngineNS.SampResult*) * num, null, 0);
                        i.mCoreObject.GetAllSamps(pOuts, num);
                        for (int j = 0; j < (uint)num; j++)
                        {
                            var pCur = new EngineNS.SampResult(pOuts[j]);
                            var name = pCur.GetName();
                            if (i.Scopes.ContainsKey(name))
                                continue;
                            var result = new TimeScope(pCur, TimeScope.EProfileFlag.FlagsAll);
                            i.Scopes.Add(name, result);
                        }
                        CoreSDK.Free(pOuts);
                    }
                }
                EngineNS.v3dSampMgr.UpdateAllThreadInstance();
            }
        }
        public static void FinalCleanup()
        {
            IsFinalCleanup = true;
            foreach (var i in AllThreadInstance)
            {
                i.Cleanup();
            }
            AllThreadInstance.Clear();
            EngineNS.v3dSampMgr.FinalCleanup();
        }
        public static TimeScopeManager FindManager(string name)
        {
            lock (AllThreadInstance)
            {
                foreach (var i in AllThreadInstance)
                {
                    if (i.ThreadName == name)
                        return i;
                }
                return null;
            }
        }
        #endregion

        [ThreadStatic]
        static TimeScopeManager mInstance = null;
        public static TimeScopeManager Instance
        {
            get 
            { 
                if(mInstance == null)
                {
                    mInstance = new TimeScopeManager();
                }
                return mInstance; 
            }
        }
        public string ThreadName { get; set; }
        public v3dSampMgr mCoreObject;
        public Dictionary<string, TimeScope> Scopes { get; } = new Dictionary<string, TimeScope>();
        private TimeScopeManager()
        {
            unsafe
            {
                mCoreObject = new v3dSampMgr(v3dSampMgr.GetThreadInstance());
                mCoreObject.NativeSuper.AddRef();
            }
            ThreadName = System.Threading.Thread.CurrentThread.Name;
            lock (AllThreadInstance)
            {
                AllThreadInstance.Add(this);
            }
        }
        public void Cleanup()
        {
            if (mCoreObject.NativePointer == IntPtr.Zero)
                return;
            foreach(var i in Scopes)
            {
                i.Value.Dispose();
            }
            Scopes.Clear();
            mCoreObject.ClearSamps();
            mCoreObject.Cleanup();
            unsafe
            {
                mCoreObject.NativeSuper.Release();
                mCoreObject.NativePointer = IntPtr.Zero;
            }
            mInstance = null;
        }
        public static SampResult GetTimeScope(string name, TimeScope.EProfileFlag flags = TimeScope.EProfileFlag.FlagsAll, bool createWhenNotFound = true)
        {
            TimeScope result;
            if (Instance.Scopes.TryGetValue(name, out result))
            {
                return result.mCoreObject;
            }

            EngineNS.SampResult samp;
            if (createWhenNotFound)
            {
                samp = Instance.mCoreObject.FindSamp(name);
            }
            else
            {
                samp = Instance.mCoreObject.PureFindSamp(name);
            }

            if (samp.NativePointer == IntPtr.Zero)
                return new SampResult();
            result = new TimeScope(samp, flags);
            Instance.Scopes.Add(name, result);
            return result.mCoreObject;
        }
        public string GetCurrentTimeScopeName()
        {
            var samp = mCoreObject.GetCurrentSamp();
            if (samp.IsValidPointer)
                return samp.GetName();
            else
                return "";
        }
    }

    [URpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.Profiler, CallerInClass = true)]
    public partial class TtRpcProfiler : IRpcHost
    {
        static URpcClass smRpcClass = null;
        public URpcClass GetRpcClass()
        {
            if (smRpcClass == null)
                smRpcClass = new URpcClass(this.GetType());
            return smRpcClass;
        }

        #region RPC
        public class RpcProfilerThreads : IO.BaseSerializer
        {
            public override void OnWriteMember(IWriter ar, ISerializer obj, TtMetaVersion metaVersion)
            {
                ar.Write((int)Profiler.TimeScopeManager.AllThreadInstance.Count);
                foreach (var i in Profiler.TimeScopeManager.AllThreadInstance)
                {
                    ar.Write(i.ThreadName);
                }
            }
            public List<string> ThreadNames = new List<string>();
            public override void OnReadMember(IReader ar, ISerializer obj, TtMetaVersion metaVersion)
            {
                int count = 0;
                ar.Read(out count);
                ThreadNames = new List<string>(count);
                for (int i = 0; i < count; i++)
                {
                    string tmp;
                    ar.Read(out tmp);
                    ThreadNames.Add(tmp);
                }
            }
        }
        RpcProfilerThreads mRpcProfilerThreads = new RpcProfilerThreads();
        [URpcMethod(Index = 0)]
        public EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads GetProfilerThreads(sbyte arg, UCallContext context)
        {
            mRpcProfilerThreads.ThreadNames.Clear();
            return mRpcProfilerThreads;
        }

        public class RpcProfilerData : IO.BaseSerializer
        {
            public Profiler.TimeScopeManager Manager;
            public override void OnWriteMember(IWriter ar, ISerializer obj, TtMetaVersion metaVersion)
            {
                if (Manager == null)
                {
                    ar.Write((int)0);
                    return;
                }
                ar.Write((int)Manager.Scopes.Count);
                foreach (var i in Manager.Scopes)
                {
                    ar.Write(i.Value.GetFriendName());
                    ar.Write(i.Value.mCoreObject.mAvgTime);
                    ar.Write(i.Value.mCoreObject.mAvgHit);
                    ar.Write(i.Value.mCoreObject.mMaxTimeInLife);
                    if (i.Value.mCoreObject.IsValidPointer)
                        ar.Write(i.Value.mCoreObject.mParent.GetName());
                    else
                        ar.Write("null");
                }
            }
            public struct ScopeInfo
            {
                public string ShowName;
                public long AvgTime;
                public int AvgHit;
                public long MaxTime;
                public string Parent;
            }
            public List<ScopeInfo> Scopes = new List<ScopeInfo>();
            public override void OnReadMember(IReader ar, ISerializer obj, TtMetaVersion metaVersion)
            {
                int count = 0;
                ar.Read(out count);
                Scopes = new List<ScopeInfo>(count);
                for (int i = 0; i < count; i++)
                {
                    ScopeInfo tmp;
                    ar.Read(out tmp.ShowName);
                    ar.Read(out tmp.AvgTime);
                    ar.Read(out tmp.AvgHit);
                    ar.Read(out tmp.MaxTime);
                    ar.Read(out tmp.Parent);

                    Scopes.Add(tmp);
                }
            }
        }        
        RpcProfilerData mRpcProfilerData = new RpcProfilerData();
        [URpcMethod(Index = 1)]
        public EngineNS.Profiler.TtRpcProfiler.RpcProfilerData GetProfilerData(string name, UCallContext context)
        {
            foreach (var i in Profiler.TimeScopeManager.AllThreadInstance)
            {
                if (i.ThreadName == name)
                {
                    mRpcProfilerData.Scopes.Clear();
                    mRpcProfilerData.Manager = i;
                    return mRpcProfilerData;
                }
            }
            return null;
        }
        public class ResetMaxTimeArg : IO.BaseSerializer
        { 
            [Rtti.Meta]
            public string ThreadName { get; set; }
            [Rtti.Meta]
            public string ScopeName { get; set; }
        }
        [URpcMethod(Index = 2)]
        public void ResetMaxTime(EngineNS.Profiler.TtRpcProfiler.ResetMaxTimeArg arg, UCallContext context)
        {
            foreach (var i in Profiler.TimeScopeManager.AllThreadInstance)
            {
                if (i.ThreadName == arg.ThreadName)
                {
                    foreach (var j in i.Scopes)
                    {
                        if(j.Value.GetFriendName() == arg.ScopeName)
                        {
                            j.Value.mCoreObject.mMaxTimeInLife = 0;
                        }
                    }
                    return;
                }
            }
        }
        #endregion
    }

    public class TtProfilerModule : TtModule<TtEngine>
    {
        public NxRHI.TtGpuTimeScopeManager GpuTimeScopeManager { get; } = new NxRHI.TtGpuTimeScopeManager();
        public override unsafe void TickModule(TtEngine host)
        {
            GpuTimeScopeManager.UpdateSync();
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Profiler.TtProfilerModule ProfilerModule { get; } = new Profiler.TtProfilerModule();
    }
}

#if TitanEngine_AutoGen
#region TitanEngine_AutoGen
#pragma warning disable 105


namespace EngineNS.Profiler
{
	public partial class TtRpcProfiler_RpcCaller
	{
		public static async System.Threading.Tasks.Task<EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads> GetProfilerThreads(sbyte arg, EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = TtReturnAwaiter<EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads>.CreateInstance(rpcArg.Timeout, rpcArg.ReturnContext);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Profiler;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 0;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await TtRpcAwaiter.AwaitReturn<EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads>(retContext);
		}
		public static async System.Threading.Tasks.Task<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData> GetProfilerData(string name, EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = TtReturnAwaiter<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData>.CreateInstance(rpcArg.Timeout, rpcArg.ReturnContext);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Profiler;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 1;
				pkg.Write(methodIndex);
				pkg.Write(name);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await TtRpcAwaiter.AwaitReturn<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData>(retContext);
		}
		public static void ResetMaxTime(EngineNS.Profiler.TtRpcProfiler.ResetMaxTimeArg arg, in EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Profiler;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 2;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
		}
	}
}


namespace EngineNS.Profiler
{
	partial class TtRpcProfiler
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetProfilerThreads = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			sbyte arg;
			reader.Read(out arg);
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Profiler.TtRpcProfiler)host).GetProfilerThreads(arg, context);
			using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetProfilerData = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string name;
			reader.Read(out name);
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Profiler.TtRpcProfiler)host).GetProfilerData(name, context);
			using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_ResetMaxTime = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			EngineNS.Profiler.TtRpcProfiler.ResetMaxTimeArg arg;
			reader.Read(out arg);
			((EngineNS.Profiler.TtRpcProfiler)host).ResetMaxTime(arg, context);
		};
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen
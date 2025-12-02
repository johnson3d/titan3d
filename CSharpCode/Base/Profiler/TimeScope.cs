using EngineNS.Bricks.Network.RPC;
using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Profiler
{
    public struct TimeScopeHelper : IDisposable//Waiting for C#8 ,ref struct -> Dispose
    {
        public TimeScope mTime;
        public TimeScopeHelper(TimeScope t, [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            mTime = t;
            if (t==null)
                return;
            if (TtEngine.Instance.Config.IsScopeWithSource)
            {
                var SourceFilePath = sourceFilePath;
                var SourceLineNumber = sourceLineNumber;
                mTime?.Begin(SourceFilePath, SourceLineNumber);
            }
            else
            {
                mTime?.Begin(null, 0);
            }
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
        public string GetName()
        {
            return mCoreObject.GetName();
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
        public void Begin(string file, int line)
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

            mBeginTime = mCoreObject.Begin(TimeScopeManager.Instance.mCoreObject, file, line);
        }
        public void End()
        {
            if (TimeScopeManager.IsFinalCleanup)
                return;

            if (mEnable == false)
                return;

            mCoreObject.End(TimeScopeManager.Instance.mCoreObject, mBeginTime);
        }
        public string ParentName
        {
            get
            {
                if (mCoreObject.mParent.IsValidPointer)
                    return mCoreObject.mParent.GetName();
                else
                    return "null";
            }
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
                mCoreObject.NativeSuper.NativeSuper.AddRef();
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
                mCoreObject.NativeSuper.NativeSuper.Release();
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

    public class TtTypeScope<T, F>
    {
        [ThreadStatic]
        private static Profiler.TimeScope mScope;
        public static Profiler.TimeScope Scope
        {
            get
            {
                if (mScope == null)
                    mScope = new Profiler.TimeScope(typeof(T), typeof(F).Name);
                return mScope;
            }
        }
    }

    [TtRpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.Profiler, CallerInClass = true)]
    public partial class TtRpcProfiler : AuxRpcHost<TtRpcProfiler>
    {
        #region Interface
        public override Bricks.Network.INetConnect GetRpcConnect(UInt16 methodIndex)
        {
            return TtEngine.Instance.RpcModule.FakeConnect;
        }
        #endregion

        #region RPC
        [RpcProfilerThreads.TtCreator]
        public class RpcProfilerThreads : IO.BaseSerializer, IPooledObject
        {
            public bool IsAlloc { get; set; }
            public class TtPooled : TtObjectPool<RpcProfilerThreads>
            {
                protected override bool OnObjectRelease(RpcProfilerThreads obj)
                {
                    obj.ThreadNames.Clear();
                    return base.OnObjectRelease(obj);
                }
            }
            public void RecycleThis()
            {
                TtCreatorAttribute.Pooled.ReleaseObject(this);
            }
            public class TtCreatorAttribute : Rtti.TtObjectCreatorAttribute
            {
                public static TtPooled Pooled = new TtPooled();
                public override object CreateInstance(object[] args)
                {
                    //return new RpcProfilerThreads();
                    return Pooled.QueryObjectSync();
                }
                public override void DisposeInstance(object obj)
                {
                    var v = obj as RpcProfilerThreads;
                    if (v == null)
                        return;
                    Pooled.ReleaseObject(v);
                }
            }
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
                //ThreadNames = new List<string>(count);
                ThreadNames.Clear();
                for (int i = 0; i < count; i++)
                {
                    string tmp;
                    ar.Read(out tmp);
                    ThreadNames.Add(tmp);
                }
            }
        }
        RpcProfilerThreads mRpcProfilerThreads = new RpcProfilerThreads();
        [TtRpcMethod(Index = 0)]
        public EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads GetProfilerThreads(sbyte arg, TtCallContext context)
        {
            //mRpcProfilerThreads.ThreadNames.Clear();
            return mRpcProfilerThreads;
        }
        
        [RpcProfilerData.TtCreator]
        public class RpcProfilerData : IO.BaseSerializer, IPooledObject
        {
            public Profiler.TimeScopeManager Manager;
            public bool IsAlloc { get; set; }
            public class TtPooled : TtObjectPool<RpcProfilerData>
            {
                protected override bool OnObjectRelease(RpcProfilerData obj)
                {
                    obj.Scopes.Clear();
                    return base.OnObjectRelease(obj);
                }
            }
            public void RecycleThis()
            {
                TtCreatorAttribute.Pooled.ReleaseObject(this);
            }
            public class TtCreatorAttribute : Rtti.TtObjectCreatorAttribute
            {
                public static TtPooled Pooled = new TtPooled();
                public override object CreateInstance(object[] args)
                {
                    //return new RpcProfilerData();
                    return Pooled.QueryObjectSync();
                }
                public override void DisposeInstance(object obj)
                {
                    var v = obj as RpcProfilerData;
                    if (v == null)
                        return;
                    Pooled.ReleaseObject(v);
                }
            }
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
                    ar.Write(i.Value.GetName());
                    ar.Write(i.Value.GetFriendName());
                    ar.Write(i.Value.mCoreObject.mAvgTime);
                    ar.Write(i.Value.mCoreObject.mAvgHit);
                    ar.Write(i.Value.mCoreObject.mMaxTimeInLife);
                    ar.Write(i.Value.mCoreObject.GetDebugSourceLine());
                    ar.Write(i.Value.mCoreObject.GetDebugSourceFile());
                    var num = i.Value.mCoreObject.GetNumOfCaller();
                    if(i.Value.mCoreObject.mParent.IsValidPointer == false)
                    {
                        ar.Write((int)0);
                    }
                    else
                    {
                        ar.Write(num);
                        for (int j = 0; j < num; j++)
                        {
                            ar.Write(i.Value.mCoreObject.GetCaller(j).GetName());
                            ar.Write(i.Value.mCoreObject.GetCallerRatio(j));
                        }
                    }   
                }
            }
            public struct ScopeInfo
            {
                public string Name;
                public string ShowName;
                public long AvgTime;
                public int AvgHit;
                public long MaxTime;
                public string SourceFile;
                public int SourceLine;
                public KeyValuePair<string, float>[] Callers;
            }
            public List<ScopeInfo> Scopes = new List<ScopeInfo>();
            public override void OnReadMember(IReader ar, ISerializer obj, TtMetaVersion metaVersion)
            {
                int count = 0;
                ar.Read(out count);
                Scopes.Clear();
                for (int i = 0; i < count; i++)
                {
                    ScopeInfo tmp;
                    ar.Read(out tmp.Name);
                    ar.Read(out tmp.ShowName);
                    ar.Read(out tmp.AvgTime);
                    ar.Read(out tmp.AvgHit);
                    ar.Read(out tmp.MaxTime);
                    ar.Read(out tmp.SourceLine);
                    ar.Read(out tmp.SourceFile); 
                    int num = 0;
                    ar.Read(out num);
                    if (num>0)
                    {
                        tmp.Callers = new KeyValuePair<string, float>[num];
                        for (int j = 0; j < num; j++)
                        {
                            string n;
                            ar.Read(out n);
                            float r;
                            ar.Read(out r);
                            tmp.Callers[j] = new KeyValuePair<string, float>(n, r);
                        }
                    }
                    else
                    {
                        tmp.Callers = null;
                    }
                    Scopes.Add(tmp);
                }
            }
        }        
        RpcProfilerData mRpcProfilerData = new RpcProfilerData();
        [TtRpcMethod(Index = 1)]
        public EngineNS.Profiler.TtRpcProfiler.RpcProfilerData GetProfilerData(string name, TtCallContext context)
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
            [Rtti.Meta("")]
            public string ThreadName { get; set; }
            [Rtti.Meta("")]
            public string ScopeName { get; set; }
        }
        [TtRpcMethod(Index = 2)]
        public void ResetMaxTime(EngineNS.Profiler.TtRpcProfiler.ResetMaxTimeArg arg, TtCallContext context)
        {
            if (context==null)
                return;
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
        public override void Cleanup(TtEngine host)
        {
            GpuTimeScopeManager.Dispose();
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

#if TitanEngine_AutoGen_RPC
#region TitanEngine_AutoGen_RPC
#pragma warning disable 105


namespace EngineNS.Profiler
{
	public partial class TtRpcProfiler_RpcCaller
	{
		public static async Thread.Async.TtTask<EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads> GetProfilerThreads(sbyte arg, EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
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
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var router = new EngineNS.Bricks.Network.RPC.FRouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Profiler;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router, false);
				UInt16 methodIndex = 0;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context, false);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await TtRpcAwaiter.AwaitReturn<EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads>(retContext);
		}
		public static async Thread.Async.TtTask<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData> GetProfilerData(string name, EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
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
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var router = new EngineNS.Bricks.Network.RPC.FRouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Profiler;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router, false);
				UInt16 methodIndex = 1;
				pkg.Write(methodIndex);
				pkg.Write(name);
				pkg.Write(retContext.Context, false);
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
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var router = new EngineNS.Bricks.Network.RPC.FRouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Profiler;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router, false);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetProfilerThreads = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			sbyte arg;
			reader.Read(out arg);
			EngineNS.Bricks.Network.RPC.FReturnContext retContext;
			reader.Read(out retContext, false);
			var ret = ((EngineNS.Profiler.TtRpcProfiler)host).GetProfilerThreads(arg, context);
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext, false);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public async Thread.Async.TtTask<EngineNS.Profiler.TtRpcProfiler.RpcProfilerThreads> RPC_GetProfilerThreads(sbyte arg, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(0);
			return await TtRpcProfiler_RpcCaller.GetProfilerThreads(arg, rpcArg);
		}
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetProfilerData = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			string name;
			reader.Read(out name);
			EngineNS.Bricks.Network.RPC.FReturnContext retContext;
			reader.Read(out retContext, false);
			var ret = ((EngineNS.Profiler.TtRpcProfiler)host).GetProfilerData(name, context);
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext, false);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public async Thread.Async.TtTask<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData> RPC_GetProfilerData(string name, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(1);
			return await TtRpcProfiler_RpcCaller.GetProfilerData(name, rpcArg);
		}
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_ResetMaxTime = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			EngineNS.Profiler.TtRpcProfiler.ResetMaxTimeArg arg;
			reader.Read(out arg);
			((EngineNS.Profiler.TtRpcProfiler)host).ResetMaxTime(arg, context);
		};
		public void RPC_ResetMaxTime(EngineNS.Profiler.TtRpcProfiler.ResetMaxTimeArg arg, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(2);
			TtRpcProfiler_RpcCaller.ResetMaxTime(arg, rpcArg);
			ResetMaxTime(arg, null);
		}
	}
}
#endregion//TitanEngine_AutoGen_RPC
#endif//TitanEngine_AutoGen_RPC
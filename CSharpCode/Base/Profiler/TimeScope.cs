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
            mTime.Begin(true);
        }
        public void Dispose()
        {
            mTime.End();
        }
    }
    public struct TimeScopeWindows : IDisposable
    {
#if PWindow
        public TimeScope mTime;
#endif
        public TimeScopeWindows(TimeScope t)
        {
#if PWindow
            mTime = t;
            mTime.Begin();
#endif
        }
        public void Dispose()
        {
#if PWindow
            mTime.End();
#endif
        }
    }
    public struct TimeClipHelper : IDisposable
    {
#if PWindow
        public TimeClip mClip;
        long mStartTime;
#endif
        public TimeClipHelper(TimeClip clip)
        {
#if PWindow
            mClip = clip;
            mStartTime = Support.Time.HighPrecision_GetTickCount();
#endif
        }
        public void Dispose()
        {
#if PWindow
            var now = Support.Time.HighPrecision_GetTickCount();
            if (now - mStartTime> mClip.MaxElapse)
            {
                mClip.OnTimeOut(now - mStartTime);
            }
#endif
        }
    }
    public class TimeClip
    {
        public TimeClip(string n, long time, Action<TimeClip, long> onTimeout)
        {
            Name = n;
            MaxElapse = time;
            TimeOutAction = onTimeout;
        }
        public TimeClip(string n, long time)
        {
            Name = n;
            MaxElapse = time;
        }
        public TimeClip(long time, Action<TimeClip, long> onTimeout)
        {
            MaxElapse = time;
            TimeOutAction = onTimeout;
        }
        public string Name;
        public long MaxElapse;
        public Action<TimeClip, long> TimeOutAction = (clip, time)=>{System.Diagnostics.Debug.WriteLine($"TimeClip {clip.Name} time out: {time}/{clip.MaxElapse}");};
        public virtual void OnTimeOut(long time)
        {
            TimeOutAction(this, time);
        }
    }
    public class TimeScope : AuxPtrType<SampResult>
    {
        [Flags]
        public enum EProfileFlag : byte
        {
            Windows = 1,
            Android = (1<<1),
            IOS = (1 << 2),

            FlagsAll = 0xFF,
        }
        public TimeScope(SampResult self, EProfileFlag flag)
        {
            mCoreObject = self;
            Flags = flag;
            mEnable = mCoreObject.mEnable;

            this.Core_AddRef();
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
            if (mEnable == false)
                return;
            unsafe
            {
                mBeginTime = mCoreObject.Begin(TimeScopeManager.Instance.mCoreObject, bPushParent);
            }
        }
        public void End()
        {
            if (mEnable == false)
                return;
            unsafe
            {
                mCoreObject.End(TimeScopeManager.Instance.mCoreObject, mBeginTime);
            }
        }
    }
    public class TimeScopeManager
    {
        #region ThreadInstance
        public static List<TimeScopeManager> AllThreadInstance { get; } = new List<TimeScopeManager>();
        public static void UpdateAllInstance()
        {
            EngineNS.v3dSampMgr.UpdateAllThreadInstance();
        }
        public static void FinalCleanup()
        {
            foreach(var i in AllThreadInstance)
            {
                i.Cleanup();
            }
            AllThreadInstance.Clear();
        }
        public static TimeScopeManager FindManager(string name)
        {
            foreach (var i in AllThreadInstance)
            {
                if (i.ThreadName == name)
                    return i;
            }
            return null;
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
            AllThreadInstance.Add(this);
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
        static bool EnableScopeAfterGet = true;
        public static TimeScope GetTimeScope(Type t, string method, string showName = null, TimeScope.EProfileFlag flags = TimeScope.EProfileFlag.FlagsAll, bool createWhenNotFound = true)
        {
            var result = GetTimeScope(t.FullName + "." + method, flags, createWhenNotFound);
            result.Enable = EnableScopeAfterGet;
            result.ShowName = showName;
            return result;
        }
        public static TimeScope GetTimeScope(string name, TimeScope.EProfileFlag flags = TimeScope.EProfileFlag.FlagsAll, bool createWhenNotFound = true)
        {
            TimeScope result;
            if (Instance.Scopes.TryGetValue(name, out result))
                return result;

            unsafe
            {
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
                    return null;
                result = new TimeScope(samp, flags);
                Instance.Scopes.Add(name, result);
                return result;
            }
        }
    }

    [Rtti.GenMetaClass(IsOverrideBitset = false)]
    [URpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.Profiler)]
    public partial class URpcProfiler : IRpcHost
    {
        static URpcClass smRpcClass = null;
        public URpcClass GetRpcClass()
        {
            if (smRpcClass == null)
                smRpcClass = new URpcClass(this.GetType());
            return smRpcClass;
        }

        const int RpcIndexStart = 0;

        #region RPC
        public class RpcProfilerThreads : IO.BaseSerializer
        {
            public override void OnWriteMember(IWriter ar, ISerializer obj, UMetaVersion metaVersion)
            {
                ar.Write((int)Profiler.TimeScopeManager.AllThreadInstance.Count);
                foreach (var i in Profiler.TimeScopeManager.AllThreadInstance)
                {
                    ar.Write(i.ThreadName);
                }
            }
            public List<string> ThreadNames = new List<string>();
            public override void OnReadMember(IReader ar, ISerializer obj, UMetaVersion metaVersion)
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
        [URpcMethod(Index = RpcIndexStart + 0, ReturnISerializer = true)]
        public RpcProfilerThreads GetProfilerThreads(sbyte arg, UCallContext context)
        {
            mRpcProfilerThreads.ThreadNames.Clear();
            return mRpcProfilerThreads;
        }

        public class RpcProfilerData : IO.BaseSerializer
        {
            public Profiler.TimeScopeManager Manager;
            public override void OnWriteMember(IWriter ar, ISerializer obj, UMetaVersion metaVersion)
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
            public override void OnReadMember(IReader ar, ISerializer obj, UMetaVersion metaVersion)
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
        [URpcMethod(Index = RpcIndexStart + 1, ReturnISerializer = true)]
        public RpcProfilerData GetProfilerData(string name, UCallContext context)
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
        [URpcMethod(Index = RpcIndexStart + 2, ArgISerializer = true)]
        public void ResetMaxTime(ResetMaxTimeArg arg, UCallContext context)
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
}

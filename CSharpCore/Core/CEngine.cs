using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS
{
    public partial class CEngine
    {
        public const string NativeNS = "Titan3D";
        CEngineDesc mDesc;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CEngineDesc Desc
        {
            get { return mDesc; }
        }
        public CEngine()
        {
            if (Instance == null)
            {
                Instance = this;
                Profiler.Log.InitLogger();
            }
        }
        ~CEngine()
        {
            Profiler.Log.FinalLogger();
        }
        public override void Cleanup()
        {
            if (mGameInstance != null)
            {
                mGameInstance.Cleanup();
                mGameInstance = null;
            }

            foreach (var i in AutoMembers)
            {
                i.Key.Cleanup(i.Value);
            }
            StopSystemThreads();

            GfxCleanup();
            base.Cleanup();
            SDK_VDefferedDeleteManager_Cleanup();
            Profiler.NativeMemory.EndProfile();
        }
        public void PreInitEngine()
        {
            if (mNativeRttiManager != null)
                return;

            FileManager.Initialize();
            SDK_CoreRttiManager_BuildRtti();
            mNativeRttiManager = new Rtti.NativeRttiManager();
            mNativeEnumManager = new Rtti.NativeEnumManager();
            mNativeStructManager = new Rtti.NativeStructManager();

            //var rtti = mNativeStructManager.FindRtti($"{CEngine.NativeNS}::IDepthStencilStateDesc");
            //for (UInt32 i = 0; i < rtti.MemberNumber; i++)
            //{
            //    var name = rtti.GetMemberName(i);
            //    var type = rtti.GetMemberType(i);
            //    if(type.IsEnum)
            //    {
            //        var rtti_enum = mNativeEnumManager.FindRtti(type.FullName);
            //        for (UInt32 j = 0; j < rtti_enum.MemberNumber; j++)
            //        {
            //            var mname =  rtti_enum.GetMemberName(j);
            //        }
            //    }
            //}
        }
        public bool InitEngine(string pkgName, CEngineDesc desc, bool isServer = false)
        {   
            if(mNativeRttiManager==null)
            {
                PreInitEngine();
            }
            InitVAssembly(isServer);
            IO.Serializer.TypeDescGenerator.Instance.BuildTypes(System.AppDomain.CurrentDomain.GetAssemblies(), (isServer ? ECSType.Server : ECSType.Client));
            
            var typeRedirectionFile = FileManager.ProjectContent + "typeredirection.xml";
            EngineNS.Rtti.TypeRedirectionHelper.Load(typeRedirectionFile);

            if (desc == null)
            {
                var rn = RName.GetRName($"{nameof(CEngineDesc)}.cfg");
                desc = IO.XmlHolder.CreateObjectFromXML(rn) as CEngineDesc;
                if (desc == null)
                {
                    desc = new CEngineDesc();
                    IO.XmlHolder.SaveObjectToXML(desc, rn);
                }
            }

            mDesc = desc;

            StartSystemThreads();

            MetaClassManager.MetaDirectory = RName.GetRName("MetaClasses");
            MetaClassManager.LoadMetaClasses();
            return true;
        }
        public virtual async System.Threading.Tasks.Task<bool> InitSystem(IntPtr drawHandle, UInt32 width, UInt32 height,
            EngineNS.ERHIType rhiType = EngineNS.ERHIType.RHT_OGL, bool debugGraphicsLayer = false)
        {
            var ok = EngineNS.CEngine.Instance.GfxInitEngine(rhiType, 0, drawHandle, debugGraphicsLayer);
            if (ok == false)
            {
                return false;
            }

            var gameDesc = new EngineNS.GamePlay.GGameInstanceDesc();

            await InitDefaultResource(gameDesc);
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);

            CEngine.UseInstancing = true;
            return true;
        }
        private void InitVAssembly(bool isServer)
        {
            string PlatformStr = "Windows";
            switch (CIPlatform.Instance.PlatformType)
            {
                case EPlatformType.PLATFORM_WIN:
                    PlatformStr = "Windows";
                    break;
                case EPlatformType.PLATFORM_DROID:
                    PlatformStr = "Android";
                    break;
                case EPlatformType.PLATFORM_IOS:
                    PlatformStr = "IOS";
                    break;
            }
            string scType = "Client";
            if (isServer)
            {
                scType = "Server";
            }

            var clientWindowsAssembly = Rtti.RttiHelper.GetAssemblyFromDllFileName((isServer ? ECSType.Server : ECSType.Client), $"Core{scType}.{PlatformStr}.dll");
            Rtti.RttiHelper.RegisterAnalyseAssembly("EngineCore", clientWindowsAssembly);
        }

        Rtti.NativeRttiManager mNativeRttiManager;
        public Rtti.NativeRttiManager NativeRttiManager
        {
            get { return mNativeRttiManager; }
        }
        Rtti.NativeEnumManager mNativeEnumManager;
        public Rtti.NativeEnumManager NativeEnumManager
        {
            get { return mNativeEnumManager; }
        }
        Rtti.NativeStructManager mNativeStructManager;
        public Rtti.NativeStructManager NativeStructManager
        {
            get { return mNativeStructManager; }
        }
        public IO.FileManager FileManager
        {
            get;
        } = new IO.FileManager();
        public Rtti.MetaClassManager MetaClassManager
        {
            get;
        } = new Rtti.MetaClassManager();
        public Thread.Async.ContextThreadManager EventPoster
        {
            get
            {
                return Thread.Async.ContextThreadManager.Instance;
            }
        }
        public Macross.MacrossDataManager MacrossDataManager
        {
            get;
        } = new Macross.MacrossDataManager();
        bool mIsSync = true;
        [Browsable(false)]
        public bool IsSync
        {
            get { return mIsSync; }
        }

        float mEngineElapseTimeSecond;
        [Browsable(false)]
        public float EngineElapseTimeSecond
        {
            get
            {
                return mEngineElapseTimeSecond;
            }
        }
        Int64 mEngineElapseTime;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Int64 EngineElapseTime
        {
            get { return mEngineElapseTime; }
        }
        Int64 mEngineTime;
        Int64 mRealEngineTime;
        // 单位（毫秒）
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Int64 EngineTime
        {
            get 
            { 
                return mEngineTime; 
            }
        }
        float mEngineTimeScale = 1.0f;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("全局参数")]
        public float EngineTimeScale
        {
            get => mEngineTimeScale;
            set => mEngineTimeScale = value;
        }
        public void _ResetTime(long time, long elapse)
        {
            mEngineTime = time;
            mEngineElapseTime = elapse;
            mEngineElapseTimeSecond = (float)mEngineElapseTime / 1000.0f;
            mRealEngineTime = Support.Time.GetTickCount();
        }
        public void _UpdateEngineTime(Int64 now)
        {
            if (mRealEngineTime == 0)
            {
                mRealEngineTime = now;
            }
            mEngineElapseTime = (long)((float)(now - mRealEngineTime) * mEngineTimeScale);
            mEngineElapseTimeSecond = (float)mEngineElapseTime / 1000.0f;
            mRealEngineTime = now;
            mEngineTime += mEngineElapseTime;
            SDK_GfxEngine_SetEngineTime(CoreObject, mEngineTime);
        }
        [Browsable(false)]
        public float EngineTimeSecond
        {
            get
            {
                return (float)EngineTime / 1000.0f;
            }
        }
        //[ReadOnly(true)]
        [Category("全局参数")]
        public int Interval
        {
            get;
            set;
        } = 0;
        [Browsable(false)]
        public TickInfoManager TickManager
        {
            get;
        } = new TickInfoManager();
        private long mFrameCount = 0;
        [Browsable(false)]
        public long FrameCount
        {
            get { return mFrameCount; }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("全局参数")]
        public bool IsReportStatistic
        {
            get
            {
                return Stat.PViewer.IsReporting;
            }
            set
            {
                Stat.PViewer.IsReporting = true;
            }
        }
        public EngineNS.GamePlay.Statistic Stat = new EngineNS.GamePlay.Statistic(new Profiler.PerfViewer());
        public void MainTick()
        {
            Stat.TickSync();
            var time1 = Support.Time.GetTickCount();
            if (Desc.RenderMT && this.ThreadLogic.IsTicking)//PauseGameTick)
            {
                mEngineTime = time1;
                return;
            }
            mFrameCount++;
            try
            {
                ScopeMainTick.Begin();
                MainTickImpl();
                ScopeMainTick.End();
            }
            catch(Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            finally
            {
                EngineNS.Thread.ContextThread.CurrentContext.ExitWhenFrameFinished();
            }
            var time2 = Support.Time.GetTickCount();
            if (time2 - time1 < Interval)
                System.Threading.Thread.Sleep(Interval - (int)(time2 - time1));
        }
        private bool mPauseGameTick = false;
        private System.Threading.AutoResetEvent mDebugWaitEvent = new System.Threading.AutoResetEvent(false);
        [Browsable(false)]
        public bool PauseGameTick
        {
            get
            {
                return mPauseGameTick;
            }
            set
            {
                //true不能在ThreadMain或者ThreadRender中调用，false可以在任何线程调用
                if(value && (Thread.ContextThread.CurrentContext==this.ThreadRHI ||
                    Thread.ContextThread.CurrentContext == this.ThreadMain || Desc.RenderMT == false))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "Thread", $"PauseGameTick by Not MultThread Rendering");
                    System.Diagnostics.Debugger.Break();
                }
                if (value)
                {
                    if (mPauseGameTick == true)
                        return;
                    mPauseGameTick = true;
                    this.ThreadLogic.mLogicEnd.Set();
                    this.ThreadMain.WaitResumeOk = false;
                    mDebugWaitEvent.WaitOne();
                    mDebugWaitEvent.Reset();
                    this.ThreadMain.WaitResumeOk = false;
                    //this.ThreadLogic.mLogicBegin.Reset();
                    //this.ThreadLogic.mLogicEnd.Reset();
                    DelayInvalidResourceTime = 10 * 1000;
                    //this.ThreadRender.mRenderEnd.Set();
                    //this.ThreadLogic.mLogicEnd.Set();
                    mPauseGameTick = false;
                }
                else
                {
                    if(mPauseGameTick)
                        mDebugWaitEvent.Set();
                }
            }
        }

        //long oneSecond = 0;

        private void MainTickImpl()
        {
            var now = Support.Time.GetTickCount();
            _UpdateEngineTime(now);

            this.ThreadMain.Tick();
        }
        public static Profiler.TimeScope ScopeMainTick = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), nameof(MainTick));
        public static Profiler.TimeScope ScopeTickRender = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), nameof(TickRender));
        public void TryTickRender()
        {
            ScopeTickRender.Begin();
            try
            {
                TickRender();
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }            
            finally
            {
                EngineNS.Thread.ContextThread.CurrentContext.ExitWhenFrameFinished();
            }
            ScopeTickRender.End();
        }
        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), nameof(TickLogic));
        internal void TryTickLogic()
        {
            ScopeTickLogic.Begin();
            try
            {
                TickLogic();
            }
            catch(Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            finally
            {
                EngineNS.Thread.ContextThread.CurrentContext.ExitWhenFrameFinished();
            }
            ScopeTickLogic.End();
        }
        protected virtual void TickRender()
        {
            TickManager.TickRender();
        }
        public static Profiler.TimeScope ScopeTickAutoMember = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), "TickAutoMember");
        protected virtual void TickLogic()
        {
            this.SetPerFrameCBuffer();

            TickManager.TickLogic();

            ScopeTickAutoMember.Begin();
            foreach (var i in AutoMembers)
            {
                i.Key.Tick(i.Value);
            }
            ScopeTickAutoMember.End();

            //if(GameInstance==null)
            //    CEngine.Instance.MacrossDataManager.CleanDebugContextGCRefrence();
        }
        public virtual void BeforeFrame()
        {
            TickManager.BeforeFrame();
        }
        public static Profiler.TimeScope ScopeTickSync = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), nameof(TickSync));
        public static Profiler.TimeScope ScopeTickSwap = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), "TickSwap");
        public static Profiler.TimeScope ScopeTickNativeDelayExec = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), "TickNativeDelayExec");
        public static Profiler.TimeScope ScopeTickDelayDelete = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), "TickDelayDelete");
        public virtual void TickSync()
        {
            ScopeTickSync.Begin();
            mIsSync = true;
            TickManager.TickSync();

            ScopeTickSwap.Begin();
            
            if (this.RenderContext != null)
            {
                ScopeTickNativeDelayExec.Begin();
                //SDK_GLDelayExecuter_Tick();
                RenderContext.FlushImmContext();
                ScopeTickNativeDelayExec.End();
            }

            ScopeTickDelayDelete.Begin();
            SDK_VDefferedDeleteManager_Tick(10);
            ScopeTickDelayDelete.End();

            ScopeTickSwap.End();

            GfxTickSync();
            mIsSync = false;
            ScopeTickSync.End();
        }

        public virtual async System.Threading.Tasks.Task<bool> OnEngineInited()
        {
            await BuildAutoMembers();
            this.EngineMacross = this.Desc.EngineMacross;

            var rn = EngineNS.RName.GetRName("GameTable/perfview.cfg");
            this.Stat.PViewer.LoadReportLists(rn);
            return true;
        }

        public virtual void OnPause()
        {
            PauseSystemThreads();
            GameInstance?.OnPause();
        }

        public virtual void OnResume(IntPtr window)
        {
            GameInstance?.OnResume(window);
            ResumeSystemThreads();
        }

        private bool mIgnoreGLCall = false;
        [ReadOnly(true)]
        public bool IgnoreGLCall
        {
            get
            {
                return mIgnoreGLCall;
            }
            set
            {
                mIgnoreGLCall = value;
#if !PlatformIOS
                SDK_GLSdk_IgnoreGLCall(vBOOL.FromBoolean(value));
#endif
            }
        }

        #region Macross Static
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void RunOn<T>(Thread.Async.FAsyncPostEvent<T> evt, Thread.Async.EAsyncTarget target = Thread.Async.EAsyncTarget.AsyncIO)
        {
            CEngine.Instance.EventPoster.RunOn(evt, target);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task DelayTime(int time)
        {
            await CEngine.Instance.EventPoster.DelayTime(time);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task<T> Post<T>(Thread.Async.FPostEventReturn<T> evt, Thread.Async.EAsyncTarget target = Thread.Async.EAsyncTarget.AsyncIO)
        {
            return await CEngine.Instance.EventPoster.Post<T>(evt, target);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task AwaitSemaphore(Thread.ASyncSemaphore smp)
        {
            await CEngine.Instance.EventPoster.AwaitSemaphore(smp);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task AwaitAsyncIOEmpty()
        {
            await CEngine.Instance.EventPoster.AwaitAsyncIOEmpty();
        }
        #endregion

        #region AutoMember
        protected Dictionary<CEngineAutoMemberProcessor, object> AutoMembers = new Dictionary<CEngineAutoMemberProcessor, object>();
        public async System.Threading.Tasks.Task BuildAutoMembers()
        {
            var props = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            foreach(var i in props)
            {
                var attrs = i.GetCustomAttributes(typeof(CEngineAutoMemberAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;
                var am = attrs[0] as CEngineAutoMemberAttribute;
                var processor = System.Activator.CreateInstance(am.Processor) as CEngineAutoMemberProcessor;
                if (processor == null)
                    continue;

                var obj = await processor.CreateObject();
                i.SetValue(this, obj);
                
                AutoMembers[processor] = obj;
            }
        }
        #endregion

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CoreRttiManager_BuildRtti();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_VDefferedDeleteManager_Tick(int limitTimes);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_VDefferedDeleteManager_Cleanup();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxEngine_SetEngineTime(NativePointer self, Int64 time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RResourceSwapChain_TickSwap(CRenderContext.NativePointer rc);

#if !PlatformIOS
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GLSdk_IgnoreGLCall(vBOOL value);
#endif
#endregion
    }

    public class CEngineAutoMemberProcessor
    {
        public virtual async System.Threading.Tasks.Task<object> CreateObject()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        public virtual void Tick(object obj)
        {

        }
        public virtual void Cleanup(object obj)
        {

        }
        public virtual async System.Threading.Tasks.Task<bool> OnGameStart(object obj)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public virtual void OnGameStop(object obj)
        {

        }
        public virtual async System.Threading.Tasks.Task OnGameInit(object obj, UInt32 width, UInt32 height)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        public virtual void ClearCache(object obj)
        {

        }
    }
    public sealed class CEngineAutoMemberAttribute : Attribute
    {
        public System.Type Processor;
        public CEngineAutoMemberAttribute(System.Type proc)
        {
            Processor = proc;
        }
    }
}

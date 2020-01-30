using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace EngineNS.Editor.Runner
{
    public interface IEventInfo
    {
        string Name { get; set; }
        string GetPerfCounterKeyName(object param);
    }
    public class EventContext
    {
        public bool CanBreak = false;
        public System.Threading.Thread CurrentThread;
        public bool EventRunnerFinished = false;

        public object Callee = null;

        public EventContext()
        {
            CurrentThread = System.Threading.Thread.CurrentThread;
        }
    }
    public class RunnerManager : EngineNS.Editor.IEditorInstanceObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public static RunnerManager Instance
        {
            get
            {
                var name = typeof(RunnerManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new RunnerManager();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        private RunnerManager()
        {
            EngineNS.CEngine.Instance.MacrossDataManager.OnRefreshedMacrossCollector += ActiveAllEnabledBreaks;
        }

        public bool EnableDebug = true;

        public bool DebugMode = false;

        [EngineNS.Rtti.MetaClass]
        public class MacrossBreakContext : EngineNS.Bricks.RemoteServices.IArgument
        {
            public UInt16 SerialId
            {
                get;
                set;
            }
            public int GetPkgSize()
            {
                return 512;
            }
            [EngineNS.Rtti.MetaData]
            public Guid DebuggerID
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public Guid BreakID
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public bool Enable
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public string ClassName
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public string Namespace
            {
                get;
                set;
            } = "Macross.Generated";
            public string FullName
            {
                get => Namespace + "." + ClassName;
            }
            public override bool Equals(object obj)
            {
                var context = obj as MacrossBreakContext;
                if (context == null)
                    return false;
                return ((context.DebuggerID == DebuggerID) &&
                        (context.BreakID == BreakID) &&
                        (context.ClassName == ClassName));
            }
            public override int GetHashCode()
            {
                return (DebuggerID.ToString() + BreakID.ToString() + ClassName).GetHashCode();
            }
        }

        List<MacrossBreakContext> mBreakedContexts = new List<MacrossBreakContext>();
        public void ActiveAllEnabledBreaks()
        {
            foreach(var bk in mBreakedContexts)
            {
                bk.Enable = true;
                SetBreakEnable(bk, false);
            }
        }
        public void DeactiveAllEnabledBreaks()
        {
            for(int i=mBreakedContexts.Count-1; i>=0; i--)
            {
                var bk = mBreakedContexts[i];
                bk.Enable = false;
                SetBreakEnable(bk, false);
            }
        }

        public void SetBreakEnable(MacrossBreakContext context, bool modifyBreakContexts = true)
        {
            if (context == null)
               return;

            if(modifyBreakContexts)
            {
                if (context.Enable)
                {
                    if (!mBreakedContexts.Contains(context))
                        mBreakedContexts.Add(context);
                }
                else
                    mBreakedContexts.Remove(context);
            }
            var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;// EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(CIPlatform.Instance.CSType, EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll", "", true);
            //var type = assembly.GetType("Macross.Generated." + context.ClassName);
            var type = assembly.GetType(context.FullName);
            if (type == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", $"SetBreakEnable:{context.FullName} failed");
                return;
            }
            var fieldInfo = type.GetField("BreakEnable_" + EngineNS.Editor.Assist.GetValuedGUIDString(context.BreakID));
            // 有些节点并没有连线，所以这里可能找不到
            fieldInfo?.SetValue(null, context.Enable);
        }

        public struct BreakContext
        {
            public Guid DebuggerId;
            public Guid BreakId;
            [EngineNS.Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public string ClassName;
            public object ValueContext;
            [EngineNS.Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public object ThisObject;
        }

        bool mIsBreaked = false;
        public bool IsBreaked
        {
            get => mIsBreaked;
            set
            {
                mIsBreaked = value;
                OnPropertyChanged("IsBreaked");
            }
        }

        BreakContext mCurrentBreakContext;
        public delegate void Delegate_OnBreakOperation(BreakContext context);
        public event Delegate_OnBreakOperation OnBreak;
        public event Delegate_OnBreakOperation OnResume;
        public string BreakTargetName = null;
        public delegate bool FMacrossBreakCondition(BreakContext context);
        private FMacrossBreakCondition OnMacrossBreakCondition;
        [EngineNS.Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public static void ClearMacrossBreakCondition()
        {
            Instance.OnMacrossBreakCondition = null;
            Instance.BreakTargetName = null;
        }
        [EngineNS.Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public static void SetMacrossBreakCondition(FMacrossBreakCondition condition)
        {
            Instance.OnMacrossBreakCondition = condition;
        }
        [EngineNS.Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public static void SetMacrossBreakTarget(string targetName)
        {
            Instance.BreakTargetName = targetName;
        }
        public void Break(BreakContext context)
        {
            if(OnMacrossBreakCondition!=null)
            {
                if (OnMacrossBreakCondition(context) == false)
                    return;
            }
            else  if(BreakTargetName != null && context.ThisObject!=null)
            {
                if(context.ThisObject.GetType().IsSubclassOf(typeof(EngineNS.GamePlay.Actor.McActor)))
                {
                    var mcActor = context.ThisObject as EngineNS.GamePlay.Actor.McActor;
                    if(mcActor.HostActor!=null)
                    {
                        if (mcActor.HostActor.SpecialName != BreakTargetName)
                            return;
                    }
                }
                else if (context.ThisObject.GetType().IsSubclassOf(typeof(EngineNS.GamePlay.Component.McComponent)))
                {
                    var mcComp = context.ThisObject as EngineNS.GamePlay.Component.McComponent;
                    if (mcComp.HostActor != null)
                    {
                        if (mcComp.HostActor.SpecialName != BreakTargetName)
                            return;
                    }
                }
            }
            //if (mCurrentBreakContext == context)
            //    return;
            OnBreak?.Invoke(context);
            mCurrentBreakContext = context;
            IsBreaked = true;
            EngineNS.CEngine.Instance.PauseGameTick = true;
        }
        public void Resume()
        {
            //if(mCurrentBreakContext != null)
                //OnResume?.Invoke(mCurrentBreakContext);
            OnResume?.Invoke(mCurrentBreakContext);
            EngineNS.CEngine.Instance.PauseGameTick = false;
            IsBreaked = false;
        }

        public void FinalCleanup()
        {

        }

        public void SetDataValue(string name, object value)
        {
            try
            {
                var valueContext = mCurrentBreakContext.ValueContext;
                if (valueContext != null)
                {
                    var field = valueContext.GetType().GetField(name);
                    if (field != null)
                        field.SetValue(valueContext, value);
                }
            }
            catch(System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteException(ex);
            }
        }
        public object GetDataValue(string name)
        {
            bool bFind = false;
            return GetDataValue(name, out bFind);
        }
        public object GetDataValue(string name, out bool bFind)
        {
            bFind = false;
            //if (mCurrentBreakContext == null)
            //    return null;
            var valueContext = mCurrentBreakContext.ValueContext;
            if (valueContext == null)
                return null;
            var field = valueContext.GetType().GetField(name);
            if (field == null)
                return null;
            bFind = true;
            return field.GetValue(valueContext);
        }
    }
}

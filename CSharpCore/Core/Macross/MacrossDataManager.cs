using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Macross
{
    public class MacrossHandle
    {
        public System.WeakReference<EngineNS.Macross.IMacrossType> Host;
        public byte[] ByteDatas;
        public string[] StringDatas;
    }
    public class MacrossDesc
    {
        public int Version;
    }
    public class MacrossDataManager
    {
        public class MacrossDebugContextHolder
        {
            public MacrossDebugContextHolder(object ctx)
            {
                mContext = ctx;
                CEngine.Instance.MacrossDataManager.AddDebugContext(this);
            }
            System.Type ContextType;
            object mContext;
            public object Context
            {
                get
                {
                    if (mContext == null && ContextType != null)
                    {
                        mContext = System.Activator.CreateInstance(ContextType);
                    }
                    return mContext;
                }
                set
                {
                    mContext = value;
                    if (value != null)
                        ContextType = value.GetType();
                }
            }
        }
        List<MacrossDebugContextHolder> DebugContexts = new List<MacrossDebugContextHolder>();
        // 返回getter，而不再是MacrossType,防止实例中咬住Macross导致不能刷新
        public MacrossGetter<T> NewObjectGetter<T>(RName name) where T : class, new()
        {
            if (name == null)
                return null;
            if (MacrossScripAssembly == null)
                return null;
            try
            {
                var obj = Macross.MacrossFactory.Instance.CreateMacrossObject(name, MacrossScripAssembly) as T;
                if (obj == null)
                {
                    Type tp;
                    if (Macross.MacrossFactory.RegTypes.TryGetValue(name, out tp) == false)
                    {
                        return null;
                    }
                    else
                    {
                        if (typeof(T).IsSubclassOf(tp) == false)
                            return null;
                        obj = Rtti.RttiHelper.CreateInstance(tp) as T;
                    }
                }
                var macrossGetter = new MacrossGetter<T>(name, obj);
                return macrossGetter;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
        }
        public void AddDebugContext(MacrossDebugContextHolder ctx)
        {
            lock (DebugContexts)
            {
                DebugContexts.Add(ctx);
            }
        }
        public void ClearDebugContext()
        {
            lock (DebugContexts)
            {
                foreach (var i in DebugContexts)
                {
                    i.Context = null;
                }
                DebugContexts.Clear();
            }
            System.GC.Collect();
        }
        public void CleanDebugContextGCRefrence()
        {
            ClearDebugContext();
            //lock (DebugContexts)
            //{
            //    bool bNeedGC = false;
            //    foreach (var i in DebugContexts)
            //    {
            //        if (i.Context != null)
            //            bNeedGC = true;
            //        i.Context = null;
            //    }
            //    if (bNeedGC)
            //        System.GC.Collect();
            //}
        }
        public LinkedListNode<MacrossHandle> AllocMacrossData(EngineNS.Hash64 id, Type classType, MacrossHandle value)
        {
            return null;
        }
        public bool FreeMacrossData(EngineNS.Hash64 id, LinkedListNode<MacrossHandle> node)
        {
            return true;
        }
        public bool IsValid = true;
        
        public delegate void Delegate_OnRefreshedMacrossCollector();
        public event Delegate_OnRefreshedMacrossCollector OnRefreshedMacrossCollector;
        //public static readonly string MacrossCollectorDllName_Android = "MacrossCollector.Android.dll";
        //public static readonly string MacrossCollectorDllName = "MacrossCollector.dll";

        //public static readonly string MacrossScriptDllName_Android = "MacrossScript.Android.dll";
        //public static readonly string MacrossScriptDllName = "MacrossScript.dll";

        public string GetCollectorCodeFileName(ECSType csType)
        {
            var dir = EngineNS.CEngine.Instance.FileManager.EditorContent + "Macross";
            return $"{dir}/MacrossCollector_{csType.ToString()}.cs".Replace("/", "\\");
        }

        //EngineNS.Rtti.VAssembly mMacrossScripAssembly;
        public EngineNS.Rtti.VAssembly MacrossScripAssembly
        {
            get
            {
                //if(mMacrossScripAssembly == null)
                //{
                //    string scriptDllName = "";
                //    switch (EngineNS.CIPlatform.Instance.PlatformType)
                //    {
                //        case EPlatformType.PLATFORM_WIN:
                //            scriptDllName = EngineNS.CEngine.Instance.FileManager.Bin + MacrossScriptDllName;
                //            break;
                //        case EPlatformType.PLATFORM_DROID:
                //            scriptDllName = EngineNS.CEngine.Instance.FileManager.Bin + MacrossScriptDllName_Android;
                //            break;
                //        default:
                //            throw new InvalidOperationException();
                //    }
                //    mMacrossScripAssembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(CIPlatform.Instance.CSType, scriptDllName, "", true, true);
                //}
                //return mMacrossScripAssembly;
                return EngineNS.Rtti.RttiHelper.GetAnalyseAssembly(CIPlatform.Instance.CSType, "Game");
            }
        }
        public UInt32 CollectorVersion = 0;
        public void RefreshMacrossCollector()
        {
            this.ClearDebugContext();
            string collectorDllName = "";
            //mMacrossScripAssembly = null;

            //switch (EngineNS.CIPlatform.Instance.PlatformType)
            //{
            //    case EPlatformType.PLATFORM_WIN:
            //        collectorDllName = EngineNS.CEngine.Instance.FileManager.Bin + MacrossCollectorDllName;
            //        break;
            //    case EPlatformType.PLATFORM_DROID:
            //        collectorDllName = EngineNS.CEngine.Instance.FileManager.Bin + MacrossCollectorDllName_Android;
            //        break;
            //    default:
            //        throw new InvalidOperationException();
            //}

            // MacrossCollector.dll
            var assembly = MacrossScripAssembly;// EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(CIPlatform.Instance.CSType, collectorDllName, "", true, false);
            if (assembly != null)
            {
                var type = assembly.GetType("Macross.MacrossCollector.MacrossFactory");
                System.Activator.CreateInstance(type);
            }
            else
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", $"{collectorDllName} is not found");
            }
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                OnRefreshedMacrossCollector?.Invoke();
                return true;
            }, Thread.Async.EAsyncTarget.Main);

#if PWindow
            // 清理所有缓存，防止咬住旧的dll文件
            EngineNS.CEngine.Instance.ClearCache();
#endif
            CollectorVersion++;
        }
    }

    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    [Rtti.MetaClass]
    [Editor.Editor_MacrossClassIconAttribute("icon/mctemplate_64.txpic", RName.enRNameType.Editor)]
    public class NullObject : IO.Serializer.Serializer
    {

    }
}

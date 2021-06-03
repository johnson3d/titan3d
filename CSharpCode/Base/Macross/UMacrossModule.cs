using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Macross
{
    public class UMacrossGetterBase
    {
        public RName Name { get; set; }
        public uint Version { get; protected set; }
        public virtual object InnerObject { get; set; }
        public virtual void Reset(UMacrossModule module)
        {
            Version = 0;
            InnerObject = null;
        }
    }
    public class UMacrossGetter<T> : UMacrossGetterBase where T : class
    {
        private UMacrossGetter()
        {
        }
        public static UMacrossGetter<T> NewInstance()
        {
            var result = new UMacrossGetter<T>();
            UEngine.Instance.MacrossModule.AddGetter(result);
            return result;
        }

        public override object InnerObject
        {
            get { return mInnerObject; }
            set { mInnerObject = value as T; }
        }
        private T mInnerObject;
        public T Get()
        {
            if (UEngine.Instance.MacrossModule.Version != Version)
            {
                InnerObject = UEngine.Instance.MacrossModule.NewInnerObject<T>(Name);
                Version = UEngine.Instance.MacrossModule.Version;
            }
            return mInnerObject;
        }
        public override void Reset(UMacrossModule module)
        {
            Version = 0;
            var newObj = module.NewInnerObject<T>(Name);
            Rtti.UClassMetaManager.Instance.GetMeta(typeof(T).FullName).CopyObjectMetaField(newObj, mInnerObject);
            InnerObject = newObj;
        }
    }
    public partial class UMacrossModule : UModule<UEngine>
    {
        private IAssemblyLoader mAssemblyLoader;
        public WeakReference mAssembly;
        private Rtti.AssemblyDesc mAssemblyDesc;
        public uint Version = 1;
        public T NewInnerObject<T>(RName name) where T : class
        {//不要保存返回值!!
            if (mAssemblyDesc == null)
                return null;

            return mAssemblyDesc.CreateInstance(name) as T;
        }
        public List<WeakReference<UMacrossGetterBase>> mGetters = new List<WeakReference<UMacrossGetterBase>>();
        partial void CreateAssemblyLoader(ref IAssemblyLoader loader);
        public void ReloadAssembly(string assemblyPath)
        {
            var hostAlcWeakRef = UEngine.Instance.MacrossModule.mAssembly;

            UEngine.Instance.MacrossModule.ReloadAssembly_Impl(assemblyPath);

            if (hostAlcWeakRef != null)
            {
                for (int i = 0; hostAlcWeakRef.IsAlive && (i < 10); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                if (hostAlcWeakRef.IsAlive)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Core", "MacrossModule Assembly unload failed, Check assembly reference please");
                }
            }
        }
        private void ReloadAssembly_Impl(string assemblyPath)
        {
            IAssemblyLoader loader = null;
            CreateAssemblyLoader(ref loader);
            if (loader == null)
                return;
            var newAssembly = loader.LoadAssembly(assemblyPath);

            Rtti.UTypeDescManager.ServiceManager manager;
            Rtti.AssemblyDesc desc;
            if (Rtti.UTypeDescManager.Instance.RegAssembly(newAssembly, out manager, out desc))
            {
                List<Type> removed = new List<Type>();
                List<Type> changed = new List<Type>();
                List<Type> added = new List<Type>();
                var oldAssembly = mAssembly.Target as System.Reflection.Assembly;

                if (oldAssembly != null)
                {
                    GetChangedLists(removed, changed, added, newAssembly, oldAssembly);
                }

                UpdateTypeManager(manager, desc, removed, changed, added);
                desc.Assembly = newAssembly;

                for (int i = 0; i < 10; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                StartUpdateIndex = 0;
                UpdateRefercences(int.MaxValue, true);
            }
            else
            {
                manager.AddAssemblyDesc(desc);
            }

            Rtti.UTypeDescManager.Instance.OnTypeChangedInvoke();

            mAssembly = new WeakReference(newAssembly);
            mAssemblyLoader?.TryUnload();
            mAssemblyLoader = loader;
            mAssemblyDesc = desc;
            System.GC.Collect();
        }
        private void GetChangedLists(List<Type> removed, List<Type> changed, List<Type> added, System.Reflection.Assembly newAssembly, System.Reflection.Assembly oldAssembly)
        {
            var newTypes = newAssembly.GetTypes();
            if (oldAssembly == null)
            {
                foreach (var i in newTypes)
                {
                    added.Add(i);
                }
                return;
            }
            var oldTypes = oldAssembly.GetTypes();

            foreach (var i in newTypes)
            {
                Type c = null;
                foreach(var j in oldTypes)
                {
                    if(i.FullName == j.FullName)
                    {
                        c = i;
                        break;
                    }
                }
                if (c != null)
                    changed.Add(c);
                else
                    added.Add(i);
            }
            foreach (var i in oldTypes)
            {
                Type c = null;
                foreach (var j in newTypes)
                {
                    if (i.FullName == j.FullName)
                    {
                        c = i;
                        break;
                    }
                }
                if (c == null)
                {
                    removed.Add(i);
                }
            }
        }
        private void UpdateTypeManager(Rtti.UTypeDescManager.ServiceManager manager, Rtti.AssemblyDesc desc, List<Type> removed, List<Type> changed, List<Type> added)
        {
            List<string> removedNames = new List<string>();
            foreach (var j in manager.Types)
            {
                if (removed.Contains(j.Value.SystemType))
                {
                    j.Value.IsRemoved = true;
                    j.Value.SystemType = null;
                    removedNames.Add(j.Key);
                }
                foreach (var k in changed)
                {
                    if (k.FullName == j.Value.SystemType.FullName)
                    {
                        j.Value.SystemType = k;
                        j.Value.Assembly = desc;
                        changed.Remove(k);
                        break;
                    }
                }
            }
            foreach (var j in removedNames)
            {
                manager.Types.Remove(j);

                var meta = Rtti.UClassMetaManager.Instance.GetMeta(j, false);
                if (meta != null)
                {
                    meta.CheckMetaField();
                }
            }
            foreach(var j in added)
            {
                var tmp = new Rtti.UTypeDesc();
                tmp.SystemType = j;
                tmp.Assembly = desc;
                manager.Types[Rtti.UTypeDesc.TypeStr(j)] = tmp;
            }
        }

        private void UpdateMetaManager(List<Type> removed, List<Type> changed)
        {
            //Rtti.ClassMetaManager.Instance.Metas
        }
        internal void AddGetter(UMacrossGetterBase getter)
        {
            lock (mGetters)
            {
                mGetters.Add(new WeakReference<UMacrossGetterBase>(getter));
            }
        }
        int StartUpdateIndex = 0;
        public void UpdateRefercences(int limitTime, bool bReset = false)
        {
            var t1 = Support.Time.HighPrecision_GetTickCount();
            lock (mGetters)
            {
                UMacrossGetterBase tmp;
                for (int i = StartUpdateIndex; i < mGetters.Count; i++)
                {
                    var v = mGetters[i];
                    if (v.TryGetTarget(out tmp) == false)
                    {
                        mGetters.RemoveAt(i);
                        i--;
                    }
                    else if (bReset)
                    {
                        tmp.Reset(this);
                    }
                    var t2 = Support.Time.HighPrecision_GetTickCount();
                    if ((int)(t2 - t1) > limitTime)
                        return;
                }
                StartUpdateIndex = 0;
            }
        }
        public override void Tick(UEngine host)
        {
            UpdateRefercences(1000, false);
        }
        public override void EndFrame(UEngine host)
        {

        }
        public override void Cleanup(UEngine host)
        {

        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Macross.UMacrossModule MacrossModule { get; } = new Macross.UMacrossModule();
    }
}
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace EngineNS.Macross
{
    public class UMacrossGetterBase
    {
        public RName Name { get; set; }
        public uint Version { get; protected set; }
        public virtual object InnerObject { get; set; }
        public virtual void Clear(UMacrossModule module)
        {
            Version = 0;
            InnerObject = null;
        }
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
        public static UMacrossGetter<T> UnsafeNewInstance(uint ver, object innerObj, bool addGetter = false)
        {
            var result = new UMacrossGetter<T>();
            if (addGetter)
                UEngine.Instance.MacrossModule.AddGetter(result);
            result.Version = ver;
            result.InnerObject = innerObj;
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
            if (mInnerObject != null)
            {
                var meta = Rtti.UClassMetaManager.Instance.GetMeta(Rtti.UTypeDescGetter<T>.TypeDesc);
                meta?.CopyObjectMetaField(newObj, mInnerObject);
            }
            InnerObject = newObj;
        }
    }
    public partial class UMacrossModule : UModule<UEngine>
    {
        private IAssemblyLoader mAssemblyLoader;
        public WeakReference mAssembly;
        private Rtti.UAssemblyDesc mAssemblyDesc;
        public uint Version = 1;
        public T NewInnerObject<T>(RName name) where T : class
        {//不要保存返回值!!
            if (mAssemblyDesc == null)
                return null;

            return mAssemblyDesc.CreateInstance(name) as T;
        }
        public List<WeakReference<UMacrossGetterBase>> mGetters = new List<WeakReference<UMacrossGetterBase>>();
        partial void CreateAssemblyLoader(ref IAssemblyLoader loader);
        partial void TryCompileCode(string assemblyFile, ref bool success);
        public void ReloadAssembly(string assemblyPath)
        {
            try
            {
                if (!IO.FileManager.FileExists(assemblyPath))
                {
                    bool success = false;
                    TryCompileCode(assemblyPath, ref success);
                    if(!success)
                        return;
                }
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
                else
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "MacrossModule Assembly unload successed");
                }
            }
            catch (Exception)
            {

            }
        }
        private void ReloadAssembly_Impl(string assemblyPath)
        {
            IAssemblyLoader loader = null;
            CreateAssemblyLoader(ref loader);
            if (loader == null)
                return;
            var pdbPath = IO.FileManager.RemoveExtName(assemblyPath);
            pdbPath += ".tpdb";
            var newAssembly = loader.LoadAssembly(assemblyPath, pdbPath);

            Rtti.UTypeDescManager.ServiceManager manager;
            Rtti.UAssemblyDesc desc;
            if (Rtti.UTypeDescManager.Instance.RegAssembly(newAssembly, out manager, out desc))
            {
                List<Type> removed = new List<Type>();
                List<Type> changed = new List<Type>();
                List<Type> added = new List<Type>();
                var oldAssembly = mAssembly.Target as System.Reflection.Assembly;

                if (oldAssembly != null)
                {
                    Rtti.UAssemblyDesc.GetChangedLists(removed, changed, added, newAssembly, oldAssembly);
                }

                Rtti.UAssemblyDesc.UpdateTypeManager(manager, desc, removed, changed, added);
                desc.ModuleAssembly = new WeakReference<System.Reflection.Assembly>(newAssembly);

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
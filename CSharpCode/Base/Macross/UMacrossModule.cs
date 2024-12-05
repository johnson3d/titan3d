using EngineNS.Bricks.CodeBuilder;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;
using System.Xml.Linq;

namespace EngineNS.Macross
{
    public struct PropertyData
    {
        public string Name;
        public Rtti.TtTypeDesc Type;
        public ulong NameHash;
        public ulong GetNameHash()
        {
            return Standart.Hash.xxHash.xxHash64.ComputeHash(Name);
        }
    }

    public interface ISceneNodeMacrossInterface
    {
        public RName MacrossName { get; set; }
        public List<PropertyData> CollectionMacrossProperties();
        public TtExpressionBase GetPropertyExpression(in PropertyData propData);
        public object GetMacrossObject();
    }

    public class TtMacrossGetterBase : IDisposable
    {
        public virtual RName Name { get; set; }
        public uint Version { get; protected set; }
        protected RName InnerObjectName = null;
        public virtual object InnerObject { get; set; }
        public virtual void Dispose()
        {
            Version = 0;
            InnerObject = null;
        }
        public virtual void Reset(TtMacrossModule module)
        {
            Version = 0;
            InnerObject = null;
        }
    }
    public class TtMacrossGetter<T> : TtMacrossGetterBase where T : class
    {
        private TtMacrossGetter()
        {
        }
        public override void Dispose()
        {
            Name = null;
            mInnerObject = null;
        }
        public static TtMacrossGetter<T> NewInstance()
        {
            var result = new TtMacrossGetter<T>();
            TtEngine.Instance.MacrossModule.AddGetter(result);
            return result;
        }
        //public static TtMacrossGetter<T> UnsafeNewInstance(uint ver, object innerObj, bool addGetter = false)
        //{
        //    var result = new TtMacrossGetter<T>();
        //    if (addGetter)
        //        TtEngine.Instance.MacrossModule.AddGetter(result);
        //    result.Version = ver;
        //    result.InnerObject = innerObj;
        //    return result;
        //}

        public override RName Name 
        { 
            get => base.Name; 
            set
            {
                base.Name = value;
                if (value == null)
                {
                    InnerObject = null;
                    return;
                }
                //Reset(TtEngine.Instance.MacrossModule);
            }
        }

        public override object InnerObject
        {
            get { return mInnerObject; }
            set { mInnerObject = value as T; }
        }
        private T mInnerObject;
        public T Get()
        {
            if (TtEngine.Instance.MacrossModule.Version != Version || InnerObjectName != Name)
            {
                var newObj = TtEngine.Instance.MacrossModule.NewInnerObject<T>(Name);
                if (mInnerObject != null)
                {
                    var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDescGetter<T>.TypeDesc);
                    meta?.CopyObjectMetaField(newObj, mInnerObject);
                }
                mInnerObject = newObj;
                Version = TtEngine.Instance.MacrossModule.Version;
                InnerObjectName = Name;
            }
            return mInnerObject;
        }
        public override void Reset(TtMacrossModule module)
        {
            Version = 0;
            var newObj = module.NewInnerObject<T>(Name);
            if (mInnerObject != null)
            {
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDescGetter<T>.TypeDesc);
                meta?.CopyObjectMetaField(newObj, mInnerObject);
            }
            InnerObject = newObj;
        }
    }
    public partial class TtMacrossModule : TtModule<TtEngine>
    {
        WeakReference mAssembly;
        private IAssemblyLoader mAssemblyLoader;
        private Rtti.TtAssemblyDesc mAssemblyDesc;
        public uint Version = 1;
        public T NewInnerObject<T>(RName name) where T : class
        {//不要保存返回值!!
            if (mAssemblyDesc == null)
                return null;
            if (name == null)
                return null;
            return mAssemblyDesc.CreateInstance(name) as T;
        }
        public List<WeakReference<TtMacrossGetterBase>> mGetters = new List<WeakReference<TtMacrossGetterBase>>();
        partial void CreateAssemblyLoader(ref IAssemblyLoader loader);
        partial void TryCompileCode(string assemblyFile, ref bool success);
        public void ReloadAssembly(string assemblyPath)
        {
            try
            {
                if (!IO.TtFileManager.FileExists(assemblyPath))
                {
                    bool success = false;
                    TryCompileCode(assemblyPath, ref success);
                    if(!success)
                        return;
                }
                
                Rtti.TtClassMetaManager.Instance.ResetSystemRef();
                //TtEngine.Instance.MacrossModule.ResetGetterReferences();
                WeakReference oldWeakRef = TtEngine.Instance.MacrossModule.ReloadAssemblyImpl(assemblyPath);

                if (oldWeakRef != null)
                {
                    for (int i = 0; oldWeakRef.IsAlive && (i < 10); i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                    if (oldWeakRef.IsAlive)
                    {
                        Profiler.Log.WriteLine<Profiler.TtMacrossCategory>(Profiler.ELogTag.Warning, "MacrossModule Assembly unload failed, Check assembly reference please");
                    }
                    else
                    {
                        Profiler.Log.WriteLine<Profiler.TtMacrossCategory>(Profiler.ELogTag.Info, "MacrossModule Assembly unload successed");
                    }
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtMacrossCategory>(Profiler.ELogTag.Info, "MacrossModule Assembly unload successed");
                }
            }
            catch (Exception)
            {

            }
        }
        private WeakReference ReloadAssemblyImpl(string assemblyPath)
        {
            IAssemblyLoader loader = null;
            CreateAssemblyLoader(ref loader);
            if (loader == null)
                return null;
            WeakReference oldWeakRef = null;
            var pdbPath = IO.TtFileManager.RemoveExtName(assemblyPath);
            pdbPath += ".tpdb";
            var newAssembly = loader.LoadAssembly(assemblyPath, pdbPath);

            Rtti.TtTypeDescManager.ServiceManager manager;
            Rtti.TtAssemblyDesc desc;
            if (Rtti.TtTypeDescManager.Instance.RegAssembly(newAssembly, out manager, out desc))
            {
                List<Type> removed = new List<Type>();
                List<Type> changed = new List<Type>();
                List<Type> added = new List<Type>();
                var oldAssembly = mAssembly.Target as System.Reflection.Assembly;
                if (oldAssembly != null)
                {
                    oldWeakRef = new WeakReference(oldAssembly);
                    Rtti.TtAssemblyDesc.GetChangedLists(removed, changed, added, newAssembly, oldAssembly);
                }

                Rtti.TtAssemblyDesc.UpdateTypeManager(manager, desc, removed, changed, added);
                desc.ModuleAssembly = new WeakReference<System.Reflection.Assembly>(newAssembly);

                for (int i = 0; i < 10; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                StartUpdateIndex = 0;
                mAssembly = new WeakReference(newAssembly);
                mAssemblyDesc = desc;
                UpdateRefercences(int.MaxValue, true);
            }
            else
            {
                manager.AddAssemblyDesc(desc);
                mAssembly = new WeakReference(newAssembly);
                mAssemblyDesc = desc;
            }
            Rtti.TtTypeDescManager.Instance.OnTypeChangedInvoke();

            mAssemblyLoader?.TryUnload();
            mAssemblyLoader = loader;
            
            System.GC.Collect();

            return oldWeakRef;
        }
        private void UpdateMetaManager(List<Type> removed, List<Type> changed)
        {
            //Rtti.ClassMetaManager.Instance.Metas
        }
        internal void AddGetter(TtMacrossGetterBase getter)
        {
            lock (mGetters)
            {
                mGetters.Add(new WeakReference<TtMacrossGetterBase>(getter));
            }
        }
        int StartUpdateIndex = 0;
        public void UpdateRefercences(int limitTime, bool bReset = false)
        {
            var t1 = Support.TtTime.HighPrecision_GetTickCount();
            lock (mGetters)
            {
                TtMacrossGetterBase tmp;
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
                    var t2 = Support.TtTime.HighPrecision_GetTickCount();
                    if ((int)(t2 - t1) > limitTime)
                        return;
                }
                StartUpdateIndex = 0;
            }
        }
        public override void TickModule(TtEngine host)
        {
            UpdateRefercences(1000, false);
        }
        public override void EndFrame(TtEngine host)
        {

        }
        public override void Cleanup(TtEngine host)
        {

        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Macross.TtMacrossModule MacrossModule { get; } = new Macross.TtMacrossModule();
    }
}
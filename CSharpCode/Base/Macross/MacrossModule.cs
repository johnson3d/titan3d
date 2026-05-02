using EngineNS.Bricks.CodeBuilder;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Reflection;
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
    public interface IMacrossObject : IDisposable
    {
        void ResetDebugger();
        TtMacrossGetterBase MacrossGetter { get; set; }
    }
    public class AuxMacrossObject : IMacrossObject
    {
        ~AuxMacrossObject()
        {
            Dispose();
        }
        public virtual void Dispose()
        {
            ResetDebugger();
        }
        public TtMacrossGetterBase MacrossGetter { get; set; }
        public void ResetDebugger()
        {
            var type = GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var f in fields)
            {
                if (f.FieldType == typeof(TtMacrossStackFrame))
                {
                    var frame = f.GetValue(this) as TtMacrossStackFrame;
                    if (frame != null)
                    {
                        frame.ClearDebugInfo();
                    }
                }
                else if (f.FieldType == typeof(TtMacrossStackTracer))
                {
                    var stack = f.GetValue(this) as TtMacrossStackTracer;
                    if (stack != null)
                    {
                        foreach (var i in stack.mFrames)
                        {
                            i.ClearDebugInfo();
                        }
                        stack.mFrames.Clear();
                    }
                }
            }
        }
    }
    public class TtMacrossGetter<T> : TtMacrossGetterBase where T : class, IMacrossObject
    {
        static int mNumOfMacrossGetter = 0;
        public static int NumOfMacrossGetter { get => mNumOfMacrossGetter; }
        private TtMacrossGetter()
        {
            System.Threading.Interlocked.Increment(ref mNumOfMacrossGetter);
        }
        ~TtMacrossGetter()
        {
            System.Threading.Interlocked.Decrement(ref mNumOfMacrossGetter);
        }
        public override void Dispose()
        {
            Name = null;
            mInnerObject = null;
        }
        public static TtMacrossGetter<T> NewInstance(RName rn = null)
        {
            var result = new TtMacrossGetter<T>();
            result.Name = rn;
            //TtEngine.Instance.MacrossModule.AddGetter(result);
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
                mInnerObject.MacrossGetter = this;
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
        public System.Reflection.Assembly TryGetAssembly()
        {
            if (mAssembly == null)
                return null;
            return mAssembly.Target as System.Reflection.Assembly;
        }
        private TtMacrosAssemblyLoader mAssemblyLoader;
        private Rtti.TtAssemblyDesc mAssemblyDesc;
        public Rtti.TtAssemblyDesc AssemblyDesc
        {
            get
            {
                if (mAssemblyDesc == null)
                {
                    var assemblyFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameAssembly;
                    this.ReloadAssembly(assemblyFile);
                }
                return mAssemblyDesc;
            }
        }
        public uint Version
        {
            get
            {
                return (uint)AssemblyDesc?.Version;
            }
        }
        public T NewInnerObject<T>(RName name) where T : class
        {//不要保存返回值!!
            if (mAssemblyDesc == null)
                return null;
            if (name == null)
                return null;
            return mAssemblyDesc.CreateInstance(name) as T;
        }
        public List<WeakReference<TtMacrossGetterBase>> mGetters = new List<WeakReference<TtMacrossGetterBase>>();
        partial void TryCompileCode(string assemblyFile, ref bool success, EPlatformType platformType);
        public void ReloadAssembly(string assemblyPath, bool bUnloadDLL = true)
        {
            try
            {
                if (!IO.TtFileManager.FileExists(assemblyPath))
                {
                    bool success = false;
                    TryCompileCode(assemblyPath, ref success, TtEngine.Instance.CurrentPlatform);
                    if(!success)
                        return;
                }
                
                Rtti.TtClassMetaManager.Instance.ResetSystemRef();
                WeakReference oldWeakRef = this.ReloadAssemblyImpl(assemblyPath, bUnloadDLL);

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
        private bool CheckAssembly(System.Reflection.Assembly assembly)
        {
            int NumOfStatic = 0;
            var types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null)
                    continue;
                var fields = t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                foreach (var f in fields)
                {
                    NumOfStatic++;
                    if (f.Name != "AssmblyDesc")
                    {
                        return false;
                    }
                }
            }
            return NumOfStatic == 1;
        }
        int CurrentVersion = 0;
        private WeakReference ReloadAssemblyImpl(string assemblyPath, bool bUnloadDLL = true)
        {
            TtMacrosAssemblyLoader loader = null;
            CreateAssemblyLoader(ref loader);
            if (loader == null)
                return null;
            WeakReference oldWeakRef = null;
            var pdbPath = IO.TtFileManager.RemoveExtName(assemblyPath);
            pdbPath += ".tpdb";
            var newAssembly = loader.LoadAssembly(assemblyPath, pdbPath);
            if (CheckAssembly(newAssembly) == false)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"{assemblyPath} contain static fields");
            }

            var oldAssembly = mAssembly?.Target as System.Reflection.Assembly;
            var desc = EngineNS.Rtti.TtAssemblyDesc.UpdateRtti("GameProject", newAssembly, oldAssembly);
            mAssembly = new WeakReference(newAssembly);
            mAssemblyDesc = desc;
            UpdateRefercences(int.MaxValue, true);
            mAssemblyDesc.Version = CurrentVersion++;

            //Rtti.TtTypeDescManager.ServiceManager manager;
            //Rtti.TtAssemblyDesc desc;
            //var isReplace = Rtti.TtTypeDescManager.Instance.RegAssembly(newAssembly, out manager, out desc);
            //desc.Version = CurrentVersion++;
            //if (isReplace)
            //{
            //    List<Type> removed = new List<Type>();
            //    List<Type> changed = new List<Type>();
            //    List<Type> added = new List<Type>();

            //    if (oldAssembly != null)
            //    {
            //        oldWeakRef = new WeakReference(oldAssembly);
            //        Rtti.TtAssemblyDesc.GetChangedLists(removed, changed, added, newAssembly, oldAssembly);
            //    }

            //    Rtti.TtAssemblyDesc.UpdateTypeManager(manager, desc, removed, changed, added);
            //    desc.ModuleAssembly = new WeakReference<System.Reflection.Assembly>(newAssembly);

            //    for (int i = 0; i < 10; i++)
            //    {
            //        GC.Collect();
            //        GC.WaitForPendingFinalizers();
            //    }
            //    manager.RegAssemblyTypes(desc);

            //    mAssembly = new WeakReference(newAssembly);
            //    mAssemblyDesc = desc;
            //    UpdateRefercences(int.MaxValue, true);
            //}
            //else
            //{
            //    manager.RegAssemblyTypes(desc);
            //    mAssembly = new WeakReference(newAssembly);
            //    mAssemblyDesc = desc;
            //}
            Rtti.TtTypeDescManager.Instance.OnTypeChangedInvoke();

            if (bUnloadDLL)
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
        public void UpdateRefercences(int limitTime, bool bReset = false)
        {
            var t1 = Support.TtTime.HighPrecision_GetTickCount();
            lock (mGetters)
            {
                TtMacrossGetterBase tmp;
                for (int i = 0; i < mGetters.Count; i++)
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